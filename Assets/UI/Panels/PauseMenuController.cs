using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip openClip;

    private SoundMixerManager soundMixerManager;

    private VisualElement root;
    private VisualElement pauseMenu;
    private Button resumeButton;
    private Button mainMenuButton;
    private Button quitButton;
    private Slider masterVolumeSlider;
    private Slider soundFXVolumeSlider;
    private Slider musicVolumeSlider;

    private IVisualElementScheduledItem fadeSchedule;

    private InputAction pauseAction;
    public bool IsPaused { get; private set; }

    private void Awake()
    {
        soundMixerManager = SoundMixerManager.Instance;
    }

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        pauseMenu = root.Q<VisualElement>("PauseMenuPanel");

        // Hide pause menu
        pauseMenu.style.display = DisplayStyle.None;
        pauseMenu.style.opacity = 0f;

        // BUTTONS
        resumeButton = root.Q<Button>("ResumeButton");
        mainMenuButton = root.Q<Button>("MainMenuButton");
        quitButton = root.Q<Button>("QuitButton");

        if (clickClip != null && hoverClip != null)
        {
            resumeButton.WithClickSound(clickClip).WithHoverSound(hoverClip);
            mainMenuButton.WithClickSound(clickClip).WithHoverSound(hoverClip);
            quitButton.WithClickSound(clickClip).WithHoverSound(hoverClip);
        }


        if (resumeButton != null)
            resumeButton.clicked += Resume;

        if (mainMenuButton != null)
            mainMenuButton.clicked += ReturnToMainMenu;

        if (quitButton != null)
            quitButton.clicked += Quit;

        // VOLUME SLIDERS
        masterVolumeSlider = root.Q<Slider>("Master");
        soundFXVolumeSlider = root.Q<Slider>("SoundFX");
        musicVolumeSlider = root.Q<Slider>("Music");

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = soundMixerManager.GetMasterVolume();
            masterVolumeSlider.RegisterCallback<ChangeEvent<float>>(SetMasterVolume);
        }

        if (soundFXVolumeSlider != null)
        {
            soundFXVolumeSlider.value = soundMixerManager.GetSoundFXVolume();
            soundFXVolumeSlider.RegisterCallback<ChangeEvent<float>>(SetSoundFXVolume);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = soundMixerManager.GetMusicVolume();
            musicVolumeSlider.RegisterCallback<ChangeEvent<float>>(SetMusicVolume);
        }

        pauseAction = InputSystem.actions.FindAction("Pause");

        if (pauseAction != null)
        {
            pauseAction.performed += OnPausePerformed;
        }
    }

    private void OnDisable()
    {
        fadeSchedule?.Pause();
        Time.timeScale = 1f;

        if (resumeButton != null)
            resumeButton.clicked -= Resume;

        if (mainMenuButton != null)
            mainMenuButton.clicked -= ReturnToMainMenu;

        if (quitButton != null)
            quitButton.clicked -= Quit;

        masterVolumeSlider?.UnregisterCallback<ChangeEvent<float>>(SetMasterVolume);
        soundFXVolumeSlider?.UnregisterCallback<ChangeEvent<float>>(SetSoundFXVolume);
        musicVolumeSlider?.UnregisterCallback<ChangeEvent<float>>(SetMusicVolume);

        if (pauseAction != null)
            pauseAction.performed -= OnPausePerformed;
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        if (IsPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        if (openClip != null)
            SoundFXManager.Instance.Play2DSoundFXClip(openClip, 0.05f);

        IsPaused = true;
        Time.timeScale = 0f;
        FadeIn();
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        FadeOut();
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void FadeIn()
    {
        fadeSchedule?.Pause();

        pauseMenu.style.display = DisplayStyle.Flex;
        pauseMenu.style.opacity = 0f;

        // Schedule the fade in to the next frame
        // On the UIDocument root so it runs on the UI time (since timeScale is 0)
        fadeSchedule = root.schedule.Execute(() => pauseMenu.style.opacity = 1f).StartingIn(1);
    }

    private void FadeOut()
    {
        fadeSchedule?.Pause();
        pauseMenu.style.opacity = 0f;

        // Schedule the fade in to the end of the fade out
        // On the UIDocument root so it runs on the UI time
        fadeSchedule = root.schedule.Execute(() => pauseMenu.style.display = DisplayStyle.None).StartingIn((long)(fadeDuration * 1000));
    }

    private void SetMasterVolume(ChangeEvent<float> evt) { soundMixerManager.SetMasterVolume(evt.newValue); }
    private void SetSoundFXVolume(ChangeEvent<float> evt) { soundMixerManager.SetSoundFXVolume(evt.newValue); }
    private void SetMusicVolume(ChangeEvent<float> evt) { soundMixerManager.SetMusicVolume(evt.newValue); }
}
