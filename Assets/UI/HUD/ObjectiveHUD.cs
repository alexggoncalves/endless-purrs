using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

[RequireComponent(typeof(UIDocument))]
public class ObjectiveHUD : Singleton<ObjectiveHUD>
{
    // COUNTER
    [SerializeField] private float countDelay = 1.0f;
    [SerializeField] private AudioClip countUpSound;

    private VisualElement root;
    private VisualElement counterContainer;
    private Label currentCatCount;
    private Label totalCatCount;
    private VisualElement catIcon;

    private int displayedCount = 0;
    private int targetCount = 0;

    private Coroutine countRoutine;
    private WaitForSeconds staggerDelay;

    // OBJECTIVES
    [SerializeField] private AudioClip newObjectiveClip;
    [SerializeField] private AudioClip pencilScratchClip;
    [SerializeField] private VisualTreeAsset objectiveElementTemplate;
    [SerializeField] private float scratchDuration = 1;

    private VisualElement objectiveContainer;
    private VisualElement objectiveList;


    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        counterContainer = root.Q<VisualElement>("CatCounterContainer");
        currentCatCount = root.Q<Label>("CurrentCatCount");
        totalCatCount = root.Q<Label>("TotalCatCount");
        catIcon = root.Q<VisualElement>("CatIcon");
        objectiveContainer = root.Q<VisualElement>("ObjectiveLContainer");
        objectiveList = root.Q<VisualElement>("ObjectiveList");

        if (objectiveContainer != null)
        {
            objectiveContainer.style.opacity = 0f;
            objectiveContainer.style.display = DisplayStyle.None;
        }

        staggerDelay = new(countDelay);

        if (GameManager.Instance == null) return;
        GameManager.Instance.OnCatAddedToHome += AddCatToCounter;

        if (totalCatCount != null)
            totalCatCount.text = GameManager.Instance.TotalCats().ToString();
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnCatAddedToHome -= AddCatToCounter;
    }

    private void AddCatToCounter(CatController cat)
    {
        targetCount += 1;

        countRoutine ??= StartCoroutine(StaggerCount());
    }

    private IEnumerator StaggerCount()
    {
        while (displayedCount < targetCount)
        {
            displayedCount += 1;
            RefreshCount();
            yield return staggerDelay;
        }

        countRoutine = null;
    }


    private void RefreshCount()
    {
        if (currentCatCount != null)
            currentCatCount.text = displayedCount.ToString();

        if (countUpSound != null)
        {
            SoundFXManager.Instance.Play2DSoundFXClip(countUpSound, 0.02f);
        }

        if (catIcon != null)
            Pulse(catIcon);
    }

    private void Pulse(VisualElement element)
    {
        element.experimental.animation.Scale(1.05f, 120).OnCompleted(() =>
        {
            element.experimental.animation.Scale(1f, 200);
        });
    }

    public void ShowObjectivePanel()
    {
        if (objectiveContainer == null) return;

        objectiveContainer.style.opacity = 0f;
        objectiveContainer.style.display = DisplayStyle.Flex; // in case it starts hidden

        objectiveContainer.experimental.animation
            .Start(0f, 1f, 400, (e, val) => e.style.opacity = val)
            .Ease(Easing.OutQuad);
    }

    public VisualElement AddObjective(string text)
    {
        if (objectiveElementTemplate == null || objectiveList == null)
            return null;

        VisualElement objectiveElement = objectiveElementTemplate.Instantiate();

        Label label = objectiveElement.Q<Label>("ObjectiveText");

        if (label != null)
            label.text = text;

        objectiveElement.style.opacity = 0f;
        objectiveList.Add(objectiveElement);

        objectiveElement.experimental.animation
            .Start(0f, 1f, 300, (e, val) => e.style.opacity = val)
            .Ease(Easing.OutQuad);

        if (newObjectiveClip != null)
            SoundFXManager.Instance.Play2DSoundFXClip(newObjectiveClip, 0.02f);

        return objectiveElement;
    }

    public void CompleteObjective(VisualElement objectiveElement)
    {
        if(objectiveElement == null) return;
 
        if (objectiveElement.ClassListContains("objective--complete")) return;
        objectiveElement.AddToClassList("objective--complete");

        VisualElement strike = objectiveElement.Q<VisualElement>("ObjectiveStrike");

        if (strike != null)
        {
            strike.style.width = Length.Percent(0);
            strike.experimental.animation
                .Start(0f, 1f, (int)scratchDuration*1000, (e, val) => e.style.width = Length.Percent(val * 100f))
                .Ease(Easing.OutQuad);
        }

        if (pencilScratchClip != null)
            SoundFXManager.Instance.Play2DSoundFXClip(pencilScratchClip, 0.01f);
    }
}
