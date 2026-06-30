using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class LoadingScreenController : MonoBehaviour
{
    [SerializeField] private IrisWipeController irisWipe;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private int pawCount = 50;
    [SerializeField] private VisualTreeAsset pawTemplate;

    private WorldGenerator worldGenerator;

    private VisualElement root;
    private VisualElement panel;
    private VisualElement elementContainer;
    private VisualElement pawTrail;

    public event Action OnLoadingComplete;
    public event Action OnLoadingScreenFadeOutComplete;

    private readonly List<VisualElement> paws = new();
    private int filledPaws = 0;

    private bool isLoadingMap;
    public bool HasFinishedLoading { get; set; }

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        panel = root.Q<VisualElement>("LoadingScreenPanel");
        elementContainer = root.Q<VisualElement>("LoadingScreenContainer");
        pawTrail = root.Q<VisualElement>("PawTrail");

        GameManager.Instance.RegisterLoadingScreen(this);
    }

    private void Start()
    {
        // Hide HUD
        GameManager.Instance.HUD.Hide();

        // Set Iris
        if (irisWipe != null)
            irisWipe.SetClosed();
    }

    public IEnumerator StartLoading(bool manageIris = true)
    {
        ResetState();
        panel.style.display = DisplayStyle.Flex;
        panel.style.opacity = 1f;
        elementContainer.style.display = DisplayStyle.Flex;
        elementContainer.style.opacity = 0f;

        yield return StartCoroutine(BuildPawTrail());
        yield return StartCoroutine(Fade(elementContainer, 0f, 1f, fadeInDuration));

        // Start loading map
        worldGenerator = WorldGenerator.Instance;
        if (worldGenerator != null)
        {
            worldGenerator.BeginGeneration();

            // Drive the trail until the initial area is ready
            while (!worldGenerator.IsInitialAreaGenerated())
            {
                UpdatePawTrail();
                yield return null;
            }
            UpdatePawTrail();   // snap to 100%
        }

        HasFinishedLoading = true;
        OnLoadingComplete?.Invoke();

        yield return StartCoroutine(FadeOutLoadingScreen(manageIris));
    }

    private void ResetState()
    {
        HasFinishedLoading = false;
        filledPaws = 0;
    }

    private IEnumerator FadeOutLoadingScreen(bool manageIris)
    {
        yield return StartCoroutine(Fade(panel, 1f, 0f, fadeOutDuration));
        if (manageIris && irisWipe != null)
            yield return StartCoroutine(irisWipe.OpenIris(1f));
        OnLoadingScreenFadeOutComplete.Invoke();
    }

    private IEnumerator BuildPawTrail()
    {
        pawTrail.Clear();
        paws.Clear();

        while (!(pawTrail.resolvedStyle.width > 0f))
        {
            yield return null;
        }

        float trailLength = pawTrail.resolvedStyle.width;
        float pawWidth = trailLength / pawCount;

        for (int i = 0; i < pawCount; i++)
        {
            var paw = pawTemplate.Instantiate().ElementAt(0);
            paw.style.width = new StyleLength(pawWidth);

            // Mirror every other paw
            if (i % 2 == 0)
                paw.AddToClassList("paw--mirrored");

            // Add paw to trail
            pawTrail.Add(paw);
            paws.Add(paw);
        }
    }

    private void UpdatePawTrail()
    {
        float progress = worldGenerator.GetInitialAreaLoadingProgress();
        progress = Mathf.Clamp01(progress);

        int filledCount = Mathf.Clamp(Mathf.RoundToInt(progress * pawCount), 0, pawCount);

        while (filledPaws < filledCount)
        {
            paws[filledPaws].AddToClassList("paw--filled");
            filledPaws++;
        }
    }

    private IEnumerator Fade(VisualElement element, float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            element.style.opacity = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        element.style.opacity = to;

        if (to == 0f)
        {
            element.style.display = DisplayStyle.None;
        }
    }
}