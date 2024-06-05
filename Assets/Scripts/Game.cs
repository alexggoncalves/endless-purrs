using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class Game : MonoBehaviour
{
    private Boolean hasBegun = false;

    public GameObject player;
    private PlayerController playerMovement;

    Speech speech;
    Decision decision;
    public CatCounter catCounter;

    public List<GameObject> followers = new List<GameObject>();
    public List<GameObject> atHome = new List<GameObject>();

    int endSequence = 0;

    private void Start()
    {
        speech = player.transform.GetChild(1).gameObject.GetComponent<Speech>(); // Get Speech
        decision = player.transform.GetChild(2).gameObject.GetComponent<Decision>();
        player.transform.position = GameObject.FindGameObjectWithTag("SpawnPoint").transform.position;
        playerMovement = player.GetComponent<PlayerController>();
    }

    private void Update()
    {
       if(Success())
        {
            PlayEndSequence();
        }
    }

    private void PlayEndSequence()
    {
        if (playerMovement.IsInsideHouse() && endSequence == 0)
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

        if (endSequence == 3)
        {
            decision.Hide();
            if (decision.GetDecision() == 'y')
            {
                catCounter.SetMessage("You let the cats be free. Thanks for play-testing our game!");

                foreach (GameObject cat in atHome)
                {
                    Destroy(cat);
                }
            }
            else if (decision.GetDecision() == 'n')
            {
                catCounter.SetMessage("You are keeping the cats locked in. Thanks for play-testing our game!");
            }
            endSequence = 4;

        }
    }

    public void Begin()
    {
        hasBegun = true;
        player.GetComponent<PlayerController>().LockMovement();
        speech.SetText("Where are my babies?\nI must find them!");
        speech.Show();

    }

    public bool HasBegun()
    {
        return hasBegun;
    }

    public void AddToFollowers(GameObject cat)
    {
        if (!followers.Contains(cat)){
            followers.Add(cat);
        }
    }

    public void RemoveFromFollowers(GameObject cat)
    {
        if (followers.Contains(cat)) {
            followers.Remove(cat);
        }
    }

    public void AddToHome(GameObject cat)
    {
        if (!atHome.Contains(cat))
        {
            atHome.Add(cat);
            catCounter.AddCat();
        }
    }

    public void RemoveFromHome(GameObject cat)
    {
        if (atHome.Contains(cat))
        {
            atHome.Remove(cat);
        }
    }

    public void MoveFollowersHome()
    {
        foreach (GameObject cat in atHome)
        {
            NavMeshAgent agent = cat.GetComponent<NavMeshAgent>();
            cat.transform.position = GameObject.FindGameObjectWithTag("SpawnPoint").transform.position;

            agent.Warp(cat.transform.position);
            cat.GetComponent<CatController>().SetWandering();
            cat.GetComponent<CatController>().target = null;
            cat.GetComponent<CatController>().SetIsAtHome(true);
            
        }


        foreach (GameObject cat in followers)
        {
            NavMeshAgent agent = cat.GetComponent<NavMeshAgent>();
            cat.transform.position = GameObject.FindGameObjectWithTag("SpawnPoint").transform.position;
            
            agent.Warp(cat.transform.position);
            cat.GetComponent<CatController>().SetWandering();
            cat.GetComponent<CatController>().target = null;
            cat.GetComponent<CatController>().SetIsAtHome(true);
            AddToHome(cat);
            
        }

        foreach (GameObject cat in atHome)
        {
            RemoveFromFollowers(cat);
        }
        
    }

    public bool Success()
    {
        return atHome.Count >= catCounter.totalCatAmount;
    }
}
