using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : Singleton<DialogueManager>
{
    [SerializeField] private InputAction nextInput;
    [SerializeField] private InputAction optionAInput;
    [SerializeField] private InputAction optionBInput;

    //[SerializeField] private DialogueBubble bubblePrefab;

    private DialogueData currentDialogue;
    private int lineIndex;
    private Action<string> onResolved;

    private Camera mainCamera;
    private Transform anchor;

    private bool isActive;

    private void OnEnable()
    {
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
        if (!isActive) return;

        Advance();
    }

    private void OnSelectOptionA(InputAction.CallbackContext ctx)
    {
        if (!isActive) return;
        MakeChoice(0);
        Debug.Log("Option A selected");
    }

    private void OnSelectOptionB(InputAction.CallbackContext ctx)
    {
        if (!isActive) return;
        MakeChoice(1);
        Debug.Log("Option B selected");
    }

    // ------------------------------ public API ------------------------

    public void StartDialogue(DialogueData data, Action<string> onComplete = null)
    {
        currentDialogue = data;
        lineIndex = 0;
        isActive = true;
        onResolved = onComplete;

        //if (activeBubble == null)
        //    activeBubble = Instantiate(bubblePrefab);

        //activeBubble.gameObject.SetActive(true);
        //activeBubble.transform.position = anchor.position + worldOffset;

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        var line = currentDialogue.lines[lineIndex];
        bool isLastLine = lineIndex >= currentDialogue.lines.Length - 1;
        bool isDecision = currentDialogue is DecisionDialogueData;

        //activeBubble.SetLine(line.text);
        //activeBubble.SetContinueIndicator(!(isLastLine && isDecision));
        //activeBubble.ClearChoices();
    }

    private void Advance()
    {
        bool isLastLine = lineIndex >= currentDialogue.lines.Length - 1;
        if (!isLastLine) { lineIndex++; ShowCurrentLine(); return; }



        //if (currentDialogue is DecisionDialogueData decision) ShowChoices(decision);
        else EndDialogue(null);
    }

    private void DisplayChoices(DecisionDialogueData decision)
    {

    }

    private void MakeChoice(int choiceId)
    {

    }

    private void EndDialogue(string resultId)
    {
        isActive = false;
        //activeBubble.gameObject.SetActive(false);
        currentDialogue = null;

        var callback = onResolved;
        onResolved = null;
        currentDialogue = null;
        callback?.Invoke(resultId);
    }

    private void LateUpdate()
    {
        if (!isActive || anchor == null) return;

        // Update bubble position and point to camera

        //activeBubble.transform.position = anchor.position;
        //activeBubble.FaceCamera(mainCamera);
    }
}
