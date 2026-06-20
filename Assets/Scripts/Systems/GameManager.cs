using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private Transform spawnPoint;

    [Header("Ojective")]
    [SerializeField] private int totalCats = 3;

    public event Action<CatController> OnCatAddedToHome;
    public event Action OnAllCatsHome;

    public List<CatController> atHome = new();
    private Boolean hasBegun = false;
    //Speech speech;
    //Decision decision;
    //int endSequence = 0;

    private void Start()
    {
        // Find spawnPoint
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");
        if (spawnPoint != null)
            this.spawnPoint = spawnPoint.transform;

        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player != null)
            this.player = player.GetComponent<PlayerController>();

        //Transform speechTransform = player.transform.childCount > 1 ? player.transform.GetChild(1) : null;
        //if (speechTransform != null) speech = speechTransform.GetComponent<Speech>();

        //Transform decisionTransform = player.transform.childCount > 2 ? player.transform.GetChild(2) : null;
        //if (decisionTransform != null) decision = decisionTransform.GetComponent<Decision>();

    }

    //private void Update()
    //{
        //if (playerController == null || speech == null || decision == null) return;

        //if (Success())
        //{
        //    PlayEndSequence();
        //}
    //}

    public void Begin()
    {
        hasBegun = true;

        //if (playerController != null) playerController.SetState(PlayerState.Locked);
        //if (speech != null)
        //{
        //    speech.SetText("Where are my babies?\nI must find them!");
        //    speech.Show();
        //}
    }

    public void AddToHome(CatController cat)
    {
        if (cat == null || atHome.Contains(cat)) return;
        
        atHome.Add(cat);
        OnCatAddedToHome?.Invoke(cat);

        if (atHome.Count >= totalCats)
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

    public bool Success() => atHome.Count >= totalCats;

    public bool HasBegun() { return hasBegun; }

    public int TotalCats() { return totalCats; }
}
