using UnityEngine;
using UnityEngine.UIElements;

public class InteractionPromptManager : Singleton<InteractionPromptManager>
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Vector3 worldOffset = Vector3.zero;
    [SerializeField] private VisualTreeAsset promptTemplate;

    private VisualElement prompt;
    private Label promptLabel;
    private Camera mainCamera;
    private Interactable currentTarget;

    private void OnEnable()
    {
        mainCamera = Camera.main;

        prompt = promptTemplate.CloneTree();
        promptLabel = prompt.Q<Label>("PromptLabel");

        prompt.style.position = Position.Absolute;
        prompt.style.left = 0;
        prompt.style.top = 0;
        prompt.style.opacity = 0;
        prompt.style.opacity = 0;

        uiDocument.rootVisualElement.Add(prompt);
    }

    public void Show(Interactable target)
    {
        currentTarget = target;
        promptLabel.text = target.PromptText;
        prompt.style.opacity = 1;
    }

    public void Hide()
    {
        currentTarget = null;
        prompt.style.opacity = 0;
    }

    private void LateUpdate()
    {
        if (currentTarget == null || prompt.panel == null) return;

        Vector3 screenPoint = mainCamera.WorldToScreenPoint(currentTarget.PromptWorldPosition + worldOffset);
        if (screenPoint.z < 0f) { prompt.style.opacity = 0; return; }
        prompt.style.opacity = 1;

        screenPoint.y = mainCamera.pixelHeight - screenPoint.y;

        float halfW = prompt.resolvedStyle.width * 0.5f;
        float halfH = prompt.resolvedStyle.height * 0.5f;

        Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(prompt.panel, screenPoint);
        prompt.style.translate = new Translate(panelPos.x - halfW, panelPos.y - halfH, 0);
    }
}
