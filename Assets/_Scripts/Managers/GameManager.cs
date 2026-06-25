using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("Ojective")]
    [SerializeField] private int totalCats = 3;

    public event Action<CatController> OnCatAddedToHome;
    public event Action OnAllCatsHome;

    public List<CatController> CatsAtHome { get; private set; } = new();
    public GameState State { get; private set; }
    public bool HasBegun { get; private set; } = false;
    public PlayerController Player { get; private set; }
    public HUDController HUD { get; private set; }
    public LoadingScreenController LoadingScreen { get; private set; }

    // Start by setting loading game state and executing loading
    private void Start() => ChangeState(GameState.InitialLoading);

    public void ChangeState(GameState state)
    {
        State = state;
        switch (state)
        {
            case GameState.InitialLoading:
                HandleLoading();
                break;
            case GameState.Starting: 
                HandleStarting(); 
                break;
            case GameState.InitialDialogue:

                break;
            case GameState.Playing:

                break;
            case GameState.FinalDecision:
                
                break;
            case GameState.GoodEnding: 

                break;
            case GameState.BadEnding:

                break;
            case GameState.Closing:

                break;
            default:
                break;
        }
    }

    private void Update()
    {
        if (!HasBegun && LoadingScreen.HasFinishedLoading)
        {
            StartCoroutine(Begin());
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

    private void HandleStarting()
    {
        // Teleport player to spawnPoint and lock movement
        if (Player != null)
        {
            StartCoroutine(Player.TeleportToSpawnPoint());
            Player.SetState(PlayerState.Locked);
        }
        else Debug.LogWarning("GameManager.Start: No player registered.");
    }

    public IEnumerator Begin()
    {
        HasBegun = true;

        yield return new WaitForSeconds(1f);

        // Unlock player
        Player.SetState(PlayerState.Free);

        // Fade in hud
        StartCoroutine(HUD.Fade(0f, 1f, 2f));

    }

    public void AddToHome(CatController cat)
    {
        if (cat == null || CatsAtHome.Contains(cat)) return;

        CatsAtHome.Add(cat);
        OnCatAddedToHome?.Invoke(cat);

        if (CatsAtHome.Count >= totalCats)
            OnAllCatsHome?.Invoke();
    }



    //private void PlayEndSequence()
    //{
    //    if (playerController == null || speech == null || decision == null) return;

    //    if (playerController.IsInsideHouse() && endSequence == 0)
    //    {
    //        endSequence = 1;
    //        speech.SetText("I did it!\nBut maybe they want to be free...");
    //        speech.Show();
    //    }

    //    if (endSequence == 1)
    //    {
    //        if (!speech.IsActive())
    //        {
    //            endSequence = 2;
    //        }
    //    }

    //    if (endSequence == 2)
    //    {
    //        decision.Show();
    //        if (decision.GetDecision() != '0')
    //        {
    //            endSequence = 3;
    //        }
    //    }

    //    if (endSequence == 3)
    //    {
    //        decision.Hide();
    //        if (decision.GetDecision() == 'y')
    //        {
    //            catCounter.SetMessage("You let the cats be free. Thanks for play-testing our game!");

    //            foreach (GameObject cat in atHome)
    //            {
    //                Destroy(cat);
    //            }
    //        }
    //        else if (decision.GetDecision() == 'n')
    //        {
    //            catCounter.SetMessage("You are keeping the cats locked in. Thanks for play-testing our game!");
    //        }
    //        endSequence = 4;

    //    }
    //}
    public void RegisterPlayer(PlayerController player) => Player = player;
    public void RegisterHUD(HUDController hud) => HUD = hud;
    public void RegisterLoadingScreen(LoadingScreenController loadingScreen) => LoadingScreen = loadingScreen;
    public bool Success() => CatsAtHome.Count >= totalCats;
    public int TotalCats() { return totalCats; }
}

public enum GameState
{
    InitialLoading,
    Starting,
    InitialDialogue,
    Playing,
    TeleportLoading,
    FinalDecision,
    GoodEnding,
    BadEnding,
    Closing
}