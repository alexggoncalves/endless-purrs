using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class CatCounterHUD : MonoBehaviour
{
    [SerializeField] private float countDelay = 1.0f;

    private VisualElement root;
    private VisualElement counterContainer;
    private Label currentCatCount;
    private Label totalCatCount;
    private VisualElement catIcon;

    private GameManager game;

    private int displayedCount = 0;
    private int targetCount = 0;

    private Coroutine countRoutine;
    private WaitForSeconds staggerDelay;

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        counterContainer = root.Q<VisualElement>("CatCounterContainer");
        currentCatCount = root.Q<Label>("CurrentCatCount");
        totalCatCount = root.Q<Label>("TotalCatCount");
        catIcon = root.Q<VisualElement>("CatIcon");

        GameObject gameObj = GameObject.Find("GameManager");
        if (gameObj != null) game = gameObj.GetComponent<GameManager>();

        staggerDelay = new(countDelay);

        if (game == null) return;
        game.OnCatAddedToHome += AddCatToCounter;

        if (totalCatCount != null)
            totalCatCount.text = game.TotalCats().ToString();
    }

    private void OnDisable()
    {
        if (game == null) return;
        game.OnCatAddedToHome -= AddCatToCounter;
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
}
