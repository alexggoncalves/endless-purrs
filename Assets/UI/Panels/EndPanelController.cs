using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class EndPanelController : Singleton<EndPanelController>
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private float fadeInDuration = 2f;
    [SerializeField] private float fadeOutDuration = 2f;
    [SerializeField] private IrisWipeController irisWipe;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip hoverClip;

    private VisualElement root;
    private VisualElement endPanel;
    private Button returnButton;
    private bool transitioning = false;

    private void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        endPanel = root.Q<VisualElement>("EndPanelContainer");
        returnButton = root.Q<Button>("ReturnButton");

        if (clickClip != null && hoverClip != null)
        {
            returnButton.WithClickSound(clickClip).WithHoverSound(hoverClip);
        }

        if (returnButton != null) returnButton.clicked += OnReturnToMainMenu;
        endPanel.style.opacity = 0;
        endPanel.pickingMode = PickingMode.Ignore;
    }

    private void OnDisable()
    {
        if (returnButton != null) returnButton.clicked -= OnReturnToMainMenu;
    }

    public IEnumerator ShowEndPanel()
    {
        yield return irisWipe.CloseIris(fadeInDuration);
        StartCoroutine(FadePanel(0f, 1f, fadeInDuration));
        endPanel.pickingMode = PickingMode.Position;
    }

    private void OnReturnToMainMenu()
    {
        if (transitioning) return;
        transitioning = true;
        StartCoroutine(GoToMainMenu());
    }

    private IEnumerator GoToMainMenu()
    {
        yield return StartCoroutine(FadePanel(1f, 0f, fadeOutDuration));

        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
    }

    private IEnumerator FadePanel(float from, float to, float duration)
    {

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            endPanel.style.opacity = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        endPanel.style.opacity = to;
    }
}
