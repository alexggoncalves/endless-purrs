using System;
using UnityEngine;
using UnityEngine.UIElements;

//[UxmlElement]
public class DialogueBubble : VisualElement
{
    private const string ussClassName = "dialogue-bubble";

    private const float cornerOffsetRange = 8f;
    private const float cornerRadius = 10f;
    private const float triangleYOffset = 20f;
    private const float triangleXOffset = 40f;

    private readonly VisualElement bubbleBackground;
    private readonly VisualElement bubbleContent;

    private readonly Label speakerLabel;
    private readonly Label textLabel;

    private readonly VisualElement continuePrompt;

    private readonly VisualElement choicesContainer;
    private readonly VisualElement optionA;
    private readonly VisualElement optionB;
    private readonly Label optionALabel;
    private readonly Label optionBLabel;

    private readonly System.Random rand = new();
    private readonly Vector2[] cornerOffsets = new Vector2[4];

    public DialogueBubble(VisualTreeAsset contentTemplate)
    {
        AddToClassList(ussClassName);
        style.position = Position.Absolute;
        style.left = 0;
        style.top = 0;
        style.scale = new Scale(Vector3.zero);
        style.opacity = 0;

        // Create background and add to this element
        bubbleBackground = new VisualElement { name = "background" };
        bubbleBackground.style.position = Position.Absolute;
        bubbleBackground.style.left = 0;
        bubbleBackground.style.right = 0;
        bubbleBackground.style.top = 0;
        bubbleBackground.style.bottom = 0;
        bubbleBackground.generateVisualContent += OnGenerateVisualContent;

        Add(bubbleBackground);

        // Clone content template and add to this element
        bubbleContent = contentTemplate.CloneTree();
        Add(bubbleContent);

        speakerLabel = bubbleContent.Q<Label>("SpeakerLabel");
        textLabel = bubbleContent.Q<Label>("TextLabel");
        continuePrompt = bubbleContent.Q<VisualElement>("ContinuePrompt");

        choicesContainer = bubbleContent.Q<VisualElement>("ChoicesContainer");
        optionA = choicesContainer.Q<VisualElement>("OptionA");
        optionB = choicesContainer.Q<VisualElement>("OptionB");
        optionALabel = optionA.Q<Label>("OptionLabel");
        optionBLabel = optionB.Q<Label>("OptionLabel");

        // Update background when content size resolves
        bubbleContent.RegisterCallback<GeometryChangedEvent>(_ =>
        {
            // Generate new random corner offset
            GenerateCornerOffset();
            // Then rebuild the bubble geometry (OnGenerateVisualContent())
            bubbleBackground.MarkDirtyRepaint();
        });
    }

    public void PlayIn()
    {
        style.display = DisplayStyle.Flex;
        style.scale = new Scale(Vector3.zero);
        style.opacity = 1;

        schedule.Execute(() => style.scale = new Scale(Vector3.one));
    }

    public void PlayOut(Action onHidden = null)
    {
        if (style.display == DisplayStyle.None) { onHidden?.Invoke(); return; }

        void OnTransitionEnd(TransitionEndEvent evt)
        {
            UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
            style.display = DisplayStyle.None;
            onHidden?.Invoke();
        }
        RegisterCallback<TransitionEndEvent>(OnTransitionEnd);

        style.opacity = 0;
        style.scale = new Scale(Vector3.zero);
    }

    private void OnGenerateVisualContent(MeshGenerationContext ctx)
    {
        var rect = bubbleContent.contentRect;

        if (rect.width <= 0f || rect.height <= 0f) return;

        // Create box
        Vector2 topLeft = new Vector2(0, 0) + cornerOffsets[0];
        Vector2 topRight = new Vector2(rect.width, 0) + cornerOffsets[1];
        Vector2 bottomRight = new Vector2(rect.width, rect.height) + cornerOffsets[2];
        Vector2 bottomLeft = new Vector2(0, rect.height) + cornerOffsets[3];

        Vector2 triangleBottom = new(rect.width / 2, rect.height + triangleYOffset);

        Vector2[] corners = { topLeft, topRight, bottomRight, bottomLeft };

        var painter = ctx.painter2D;
        painter.fillColor = new Color(1, 1, 1, 1);

        painter.BeginPath();

        // Start on the midpoint of the left edge
        painter.MoveTo(Vector2.Lerp(corners[3], corners[0], 0.5f));
        for (int i = 0; i < corners.Length; i++)
        {
            Vector2 corner = corners[i];
            Vector2 next = corners[(i + 1) % corners.Length];
            painter.ArcTo(corner, next, cornerRadius);

            // Create triangle tip on bottom edge
            if(i == 2)
            {
                painter.LineTo(Vector2.Lerp(corners[i], corners[i + 1], 0.45f));
                painter.LineTo(triangleBottom);
                painter.LineTo(Vector2.Lerp(corners[i], corners[i + 1], 0.5f));
            }
        }

        painter.ClosePath();
        painter.Fill();
    }

    public void SetLine(string speakerName, string text)
    {
        speakerLabel.text = speakerName;
        textLabel.text = text;
    }

    private void GenerateCornerOffset()
    {
        // Generate a random offset for each corner
        for (int i = 0; i < 4; i++)
        {
            cornerOffsets[i] = new Vector2(
                (float)(rand.NextDouble() * 2 - 1) * cornerOffsetRange,
                (float)(rand.NextDouble() * 2 - 1) * cornerOffsetRange);
        }
    }
    public void SetContinuePromptVisible(bool visible) =>
    continuePrompt.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    public void SetChoices(string optionAText, string optionBText)
    {
        optionALabel.text = optionAText;
        optionBLabel.text = optionBText;
        choicesContainer.style.display = DisplayStyle.Flex;
    }

    public void HideChoices() => choicesContainer.style.display = DisplayStyle.None;


}
