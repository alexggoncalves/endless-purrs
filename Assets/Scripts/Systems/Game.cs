using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class Game : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private Transform spawnPoint;

    private Boolean hasBegun = false;

    private PlayerController playerController;

    Speech speech;
    Decision decision;
    //public CatCounter catCounter;

    public List<CatController> followers = new();
    public List<CatController> atHome = new();



    int endSequence = 0;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("Game: Player reference not assigned!");
            return;
        }

        Transform speechTransform = player.transform.childCount > 1 ? player.transform.GetChild(1) : null;
        if (speechTransform != null) speech = speechTransform.GetComponent<Speech>();

        Transform decisionTransform = player.transform.childCount > 2 ? player.transform.GetChild(2) : null;
        if (decisionTransform != null) decision = decisionTransform.GetComponent<Decision>();

        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");
        if (spawnPoint != null)
        {
            player.transform.position = spawnPoint.transform.position;
        }



        playerController = player.GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (playerController == null || speech == null || decision == null) return;

        if (Success())
        {
            PlayEndSequence();
        }
    }

    private void PlayEndSequence()
    {
        if (playerController == null || speech == null || decision == null) return;

        if (playerController.IsInsideHouse() && endSequence == 0)
        {
            endSequence = 1;
            speech.SetText("I did it!\nBut maybe they want to be free...");
            speech.Show();
        }

        if (endSequence == 1)
        {
            if (!speech.IsActive())
            {
                endSequence = 2;
            }
        }

        if (endSequence == 2)
        {
            decision.Show();
            if (decision.GetDecision() != '0')
            {
                endSequence = 3;
            }
        }

        //if (endSequence == 3)
        //{
        //    decision.Hide();
        //    if (decision.GetDecision() == 'y')
        //    {
        //        catCounter.SetMessage("You let the cats be free. Thanks for play-testing our game!");

        //        foreach (GameObject cat in atHome)
        //        {
        //            Destroy(cat);
        //        }
        //    }
        //    else if (decision.GetDecision() == 'n')
        //    {
        //        catCounter.SetMessage("You are keeping the cats locked in. Thanks for play-testing our game!");
        //    }
        //    endSequence = 4;

        //}
    }

    public void Begin()
    {
        hasBegun = true;
        if (playerController != null) playerController.SetState(PlayerState.Locked);
        if (speech != null)
        {
            speech.SetText("Where are my babies?\nI must find them!");
            speech.Show();
        }
    }

    public bool HasBegun()
    {
        return hasBegun;
    }

    public void AddToFollowers(CatController cat)
    {
        if (cat == null) return;
        if (!followers.Contains(cat)){
            followers.Add(cat);
        }
    }

    public void RemoveFromFollowers(CatController cat)
    {
        if (cat == null) return;
        if (followers.Contains(cat)) {
            followers.Remove(cat);
        }
    }

    public void AddToHome(CatController cat)
    {
        if (cat == null) return;
        if (!atHome.Contains(cat))
        {
            atHome.Add(cat);
            //catCounter.AddCat();
        }
    }

    public void RemoveFromHome(CatController cat)
    {
        if (cat == null) return;
        if (atHome.Contains(cat))
        {
            atHome.Remove(cat);
        }
    }

    public void MoveFollowersHome()
    {
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");
        if (spawnPoint == null) return;
        
        Vector3 spawnPos = spawnPoint.transform.position;

        // Followers being teleported home
        foreach (var controller in followers.ToArray())
        {
            if (controller == null) continue;

            NavMeshAgent agent = controller.GetComponent<NavMeshAgent>();

            // Ensure agent is enabled before warping
            if (agent != null && !agent.enabled)
                agent.enabled = true;

            controller.transform.position = spawnPos;
            if (agent != null && agent.enabled) agent.Warp(spawnPos);

            controller.SetWandering();
            controller.SetState(CatState.AtHome);
            
            RemoveFromFollowers(controller);
            AddToHome(controller);
        }
    }

    public bool Success()
    {
        //return atHome.Count >= catCounter.totalCatAmount;
        return true;
    }
}
