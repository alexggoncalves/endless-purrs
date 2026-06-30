using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : Singleton<GameManager>
{
    [Header("Ojective")]
    [SerializeField] private int totalCats = 3;

    [Header("Dialogues")]
    [SerializeField] private DialogueData initialDialogue;
    [SerializeField] private DialogueData finalDialogue;

    public event Action<CatController> OnCatAddedToHome;
    public event Action OnAllCatsHome;

    private VisualElement collectObjective, decisionObjective;

    public List<CatController> CatsAtHome { get; private set; } = new();
    public GameState State { get; private set; }
    public bool HasBegun { get; private set; } = false;
    public bool IsOver { get; private set; } = false;

    public PlayerController Player { get; private set; }
    public HUDController HUD { get; private set; }
    public LoadingScreenController LoadingScreen { get; private set; }


    private void Start()
    {
        // Start by setting loading game state and executing loading
        ChangeState(GameState.InitialLoading);
        LoadingScreen.OnLoadingComplete += HandleLoadingComplete;
        LoadingScreen.OnLoadingScreenFadeOutComplete += HandleLoadingFadeComplete;
    }

    private void OnDisable()
    {
        LoadingScreen.OnLoadingComplete -= HandleLoadingComplete;
        LoadingScreen.OnLoadingScreenFadeOutComplete -= HandleLoadingFadeComplete;
    }

    private void HandleLoadingComplete()
    {
        if (State == GameState.InitialLoading)
            ChangeState(GameState.InitialSetup);
    }

    private void HandleLoadingFadeComplete()
    {
        if (State == GameState.InitialSetup)
            ChangeState(GameState.InitialDialogue);
    }

    public void ChangeState(GameState state)
    {
        State = state;
        switch (state)
        {
            case GameState.InitialLoading:
                HandleLoading();
                break;
            case GameState.InitialSetup:
                HandleInitialSetup();
                break;
            case GameState.InitialDialogue:
                HandleInitialDialogue();
                break;
            case GameState.Playing:
                HandlePlaying();
                break;
            case GameState.Teleporting:
                //Nothing
                break;
            case GameState.FinalDecision:
                HandleFinalDecision();
                break;
            case GameState.GoodEnding:
                HandleGoodEnding();
                break;
            case GameState.BadEnding:
                HandleBadEnding();
                break;
            case GameState.Closing:

                break;
            default:
                break;
        }
    }

    private void HandleLoading()
    {
        // Start loading map
        if (LoadingScreen != null)
        {
            StartCoroutine(LoadingScreen.StartLoading());
        }
    }

    private void HandleInitialSetup()
    {
        // Teleport player to spawnPoint and lock movement
        if (Player != null)
        {
            StartCoroutine(Player.TeleportToSpawnPoint());
            Player.SetState(PlayerState.Locked);
        }
        else Debug.LogWarning("GameManager.Start: No player registered.");
    }

    private void HandleInitialDialogue()
    {
        void OnInitialDialogueEnd(string _) => ChangeState(GameState.Playing);

        // MAKE PLAYER LOOK AT THE CAMERA

        DialogueManager.Instance.StartDialogue(initialDialogue, Player.transform, OnInitialDialogueEnd);
    }

    private void HandlePlaying()
    {
        if (!HasBegun)
        {
            Player.SetState(PlayerState.Free);
            StartCoroutine(HUD.Fade(0f, 1f, 2f));
        }

        HasBegun = true;

        if (collectObjective != null || IsOver) return;

        ObjectiveHUD.Instance.ShowObjectivePanel();
        collectObjective = ObjectiveHUD.Instance.AddObjective("Bring 3 cats home");
    }

    private void HandleFinalDecision()
    {
        void OnFinalDecisionMade(string decisionID)
        {
            if (decisionID == "0") ChangeState(GameState.GoodEnding);
            else ChangeState(GameState.BadEnding);
        }

        //StartCoroutine(HUD.Fade(1f, 0f, 2f));

        Player.SetState(PlayerState.Locked);

        // MAKE PLAYER LOOK AT THE CAMERA
        IsOver = true;
        DialogueManager.Instance.StartDialogue(finalDialogue, Player.transform, OnFinalDecisionMade);

    }

    private void HandleGoodEnding()
    {
        ObjectiveHUD.Instance.CompleteObjective(decisionObjective);

        StartCoroutine(EndGame());
    }

    private IEnumerator EndGame()
    {
        yield return new WaitForSeconds(1);
        // CLOSE IRIS AND SHOW END SCREEN
    }

    private void HandleBadEnding()
    {
        ObjectiveHUD.Instance.CompleteObjective(decisionObjective);
        Player.SetState(PlayerState.Free);
    }

    public void AddToHome(CatController cat)
    {
        if (cat == null || CatsAtHome.Contains(cat)) return;

        CatsAtHome.Add(cat);
        OnCatAddedToHome?.Invoke(cat);

        if (CatsAtHome.Count >= totalCats && !IsOver)
        {
            OnAllCatsHome?.Invoke();

            StartCoroutine(StartFinalDecision());
        }

    }

    private IEnumerator StartFinalDecision()
    {
        while(State == GameState.Teleporting)
        {
            yield return null;
        }

        ChangeState(GameState.FinalDecision);
        ObjectiveHUD.Instance.CompleteObjective(collectObjective);
        decisionObjective = ObjectiveHUD.Instance.AddObjective("Make a decision");
    }

    public void RegisterPlayer(PlayerController player) => Player = player;
    public void RegisterHUD(HUDController hud) => HUD = hud;
    public void RegisterLoadingScreen(LoadingScreenController loadingScreen) => LoadingScreen = loadingScreen;
    public bool Success() => CatsAtHome.Count >= totalCats;
    public int TotalCats() { return totalCats; }
}

public enum GameState
{
    InitialLoading,
    InitialSetup,
    InitialDialogue,
    Playing,
    Teleporting,
    FinalDecision,
    GoodEnding,
    BadEnding,
    Closing
}