using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class DialogueManager : Singleton<DialogueManager>
{
    [SerializeField] private InputAction nextInput;
    [SerializeField] private InputAction optionAInput;
    [SerializeField] private InputAction optionBInput;

    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset bubbleContentTemplate;
    [SerializeField] private StyleSheet bubbleStyleSheet;

    [SerializeField] private AudioClip bubblePopClip;

    [SerializeField] private Vector3 worldOffset = new(0f, 2.2f, 0f);

    DialogueBubble bubble;
    private DialogueData currentDialogue;
    private int lineIndex;
    private Action<string> onResolved;

    private Camera mainCamera;
    private Transform anchor;

    private bool isActive;
    private bool waitingOnChoice;
    private string pendingResultId;


    private void OnEnable()
    {
        mainCamera = Camera.main;

        bubble = new DialogueBubble(bubbleContentTemplate);
        bubble.styleSheets.Add(bubbleStyleSheet);
        bubble.style.display = DisplayStyle.None;
        uiDocument.rootVisualElement.Add(bubble);

        nextInput.Enable();
        optionAInput.Enable();
        optionBInput.Enable();

        nextInput.performed += OnNext;
        optionAInput.performed += OnSelectOptionA;
        optionBInput.performed += OnSelectOptionB;
    }

    private void OnDisable()
    {
        nextInput.Disable();
        optionAInput.Disable();
        optionBInput.Disable();

        nextInput.performed -= OnNext;
        optionAInput.performed -= OnSelectOptionA;
        optionBInput.performed -= OnSelectOptionB;
    }

    private void OnNext(InputAction.CallbackContext ctx)
    {
        if (!isActive || waitingOnChoice) return;

        Advance();
    }

    private void OnSelectOptionA(InputAction.CallbackContext ctx)
    {
        if (!isActive || !waitingOnChoice) return;
        MakeChoice(0);
        Debug.Log("Option A selected");
    }

    private void OnSelectOptionB(InputAction.CallbackContext ctx)
    {
        if (!isActive || !waitingOnChoice) return;
        MakeChoice(1);
        Debug.Log("Option B selected");
    }

    // ------------------------------ public API ------------------------

    public void StartDialogue(DialogueData data, Transform worldAnchor, Action<string> onComplete = null)
    {
        currentDialogue = data;
        anchor = worldAnchor;
        lineIndex = 0;
        isActive = true;
        waitingOnChoice = false;
        onResolved = onComplete;

        bubble.PlayIn();
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        var line = currentDialogue.lines[lineIndex];
        bool isLastLine = lineIndex >= currentDialogue.lines.Length - 1;
        bool isDecision = currentDialogue is DecisionDialogueData;
        bubble.SetLine("GARY", line.text); // ADD SPEAKER NAME TO SCRIPTABLE OBJ
        bubble.SetContinuePromptVisible(!(isLastLine && isDecision));

        if (bubblePopClip != null)
            SoundFXManager.Instance.PlaySoundFXClip(bubblePopClip,anchor.position,0.5f);

        if (currentDialogue is DecisionDialogueData decision)
            DisplayChoices(decision);
        else bubble.HideChoices();

    }

    private void Advance()
    {
        bool isLastLine = lineIndex >= currentDialogue.lines.Length - 1;
        if (!isLastLine) { lineIndex++; ShowCurrentLine(); return; }

        // Display decision if there is a following one
        if (currentDialogue is NormalDialogueData dialogue)
        {
            if (dialogue.followingDecision != null)
            {
                currentDialogue = dialogue.followingDecision;
                lineIndex = 0;
                ShowCurrentLine();
                return;
            }
        }

        EndDialogue(pendingResultId);
    }

    private void DisplayChoices(DecisionDialogueData decision)
    {
        waitingOnChoice = true;
        bubble.SetContinuePromptVisible(false);
        bubble.SetChoices(decision.choiceA.choiceText, decision.choiceB.choiceText);
    }

    private void MakeChoice(int choiceId)
    {
        if (currentDialogue is not DecisionDialogueData decision) return;
        var choice = choiceId == 0 ? decision.choiceA : decision.choiceB;

        if (choice.nextDialogue != null)
        {
            StartDialogue(choice.nextDialogue, anchor, onResolved);
            // Set the result after StartDialogue so it survives the reset
            if (!string.IsNullOrEmpty(choice.resultId))
                pendingResultId = choice.resultId;
        }
        else
        {
            EndDialogue(choice.resultId);
        }
    }

    private void EndDialogue(string resultId)
    {
        isActive = false;
        waitingOnChoice = false;
        bubble.HideChoices();
        bubble.PlayOut();

        currentDialogue = null;
        pendingResultId = null;
        var callback = onResolved;
        onResolved = null;
        callback?.Invoke(resultId);
    }

    private void LateUpdate()
    {
        if (!isActive || anchor == null || bubble.panel == null) return;

        Vector3 screenPoint = mainCamera.WorldToScreenPoint(anchor.position + worldOffset);
        if (screenPoint.z < 0f) { bubble.style.display = DisplayStyle.None; return; }

        screenPoint.y = mainCamera.pixelHeight - screenPoint.y;

        bubble.style.display = DisplayStyle.Flex;

        float halfW = bubble.resolvedStyle.width * 0.5f;
        float halfH = bubble.resolvedStyle.height * 0.5f;

        Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(bubble.panel, screenPoint);
        bubble.style.translate = new Translate(panelPos.x - halfW, panelPos.y - halfH, 0);
    }
}
