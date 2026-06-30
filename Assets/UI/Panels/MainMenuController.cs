using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private float menuFadeDuration = 1.0f;
    [SerializeField] private float fadeOutDuration = 2.0f;
    [SerializeField] private IrisWipeController irisWipe;
    [SerializeField] private ScreenFadeController screenFade;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip hoverClip;

    private VisualElement root;
    private VisualElement mainMenu;
    private Button playButton;
    private Button quitButton;
    private bool transitioning;

    private void Awake() {
        root = GetComponent<UIDocument>().rootVisualElement;
        mainMenu = root.Q<VisualElement>("MainMenu");
        playButton = root.Q<Button>("StartButton");
        quitButton = root.Q<Button>("QuitButton");

        if (clickClip != null && hoverClip != null)
        {
            playButton.WithClickSound(clickClip).WithHoverSound(hoverClip);
            quitButton.WithClickSound(clickClip).WithHoverSound(hoverClip);
        }
    }

    private void OnEnable()
    {
        if (playButton != null) playButton.clicked += OnPlay;
        if (quitButton != null) quitButton.clicked += OnQuit;
    }

    private void OnDisable()
    {
        if (playButton != null) playButton.clicked -= OnPlay;
        if (quitButton != null) quitButton.clicked -= OnQuit;
    }

    private void Start()
    {
        mainMenu.style.opacity = 0;
        StartCoroutine(InitialFadeIn());
    }

    private IEnumerator InitialFadeIn()
    {
        yield return StartCoroutine(screenFade.FadeIn(fadeInDuration));

        StartCoroutine(FadeMenu(0f, 1f, menuFadeDuration));
    }

    private void OnPlay()
    {
        if (transitioning) return;
        transitioning = true;
        StartCoroutine(LoadGame());
    }

    private IEnumerator LoadGame()
    {
        yield return StartCoroutine(FadeMenu(1f, 0f, menuFadeDuration));
        yield return irisWipe.CloseIris(fadeOutDuration);

        SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }

    private IEnumerator FadeMenu(float from, float to, float duration)
    {
        mainMenu.pickingMode = PickingMode.Ignore;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            mainMenu.style.opacity = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        mainMenu.style.opacity = to;
        mainMenu.pickingMode = to > 0f ? PickingMode.Position : PickingMode.Ignore;
    }

    private void OnQuit()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}