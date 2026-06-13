using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenuController : MonoBehaviour
{
    public VisualElement root;

    public Button playButton;
    public Button quitButton;

    private VisualElement mainMenu;
    private VisualElement loadingScreen;
    private VisualElement progressFill;

    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private PlayerController playerController;

    private bool isLoadingMap = false;


    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        mainMenu = root.Q<VisualElement>("MainMenu");
        loadingScreen = root.Q<VisualElement>("LoadingScreen");
        progressFill = root.Q<VisualElement>("LoadingBarProgress");
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (mapGenerator == null || !isLoadingMap) return;

        if (!mapGenerator.IsInitialAreaGenerated())
        {
            float progress = mapGenerator.GetInitialAreaLoadingProgress();
            if (progressFill != null)
            {
                progressFill.style.width = Length.Percent(progress * 100f);
            }
        }
        else if (loadingScreen != null && loadingScreen.style.display != DisplayStyle.None)
        {
            loadingScreen.style.display = DisplayStyle.None;
            playerController.SetState(PlayerState.Free);
        }
    }

    private void OnEnable()
    {
        if (root == null) return;

        playButton = root.Q<Button>("StartButton");
        if (playButton != null)
        {
            playButton.clicked += OnPlayButtonClicked;
        }

        quitButton = root.Q<Button>("QuitButton");
        if (quitButton != null)
        {
            quitButton.clicked += OnQuitButtonClicked;
        }
    }

    private void OnDisable()
    {
        if (playButton != null)
        {
            playButton.clicked -= OnPlayButtonClicked;
        }

        if (quitButton != null)
        {
            quitButton.clicked -= OnQuitButtonClicked;
        }
    }

    private void OnPlayButtonClicked()
    {
        if (loadingScreen != null)
        {
            loadingScreen.style.display = DisplayStyle.Flex;
        }

        // FADE OUT !! TODO

        if (mainMenu != null)
        {
            mainMenu.style.display = DisplayStyle.None;
        }

        // Begin generating map
        if (mapGenerator != null)
        {
            mapGenerator.BeginGeneration();
            isLoadingMap = true;
        }
    }

    private void OnQuitButtonClicked()
    {
        Application.Quit();
        EditorApplication.isPlaying = false;
    }
}
