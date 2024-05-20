using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UIElements;

public class Game : MonoBehaviour
{
    private Boolean hasBegun = false;

    public GameObject player;
    private Movement playerMovement;

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
        playerMovement = player.GetComponent<Movement>();
    }

    private void Update()
    {
        if(catCounter.Success() && playerMovement.IsInsideHouse() && endSequence == 0)
        {
            endSequence = 1;
            speech.SetText("I did it!\nBut maybe they want to be free...");
            speech.Show();
        }

        if(endSequence == 1)
        {
            if (!speech.IsActive())
            {
                endSequence = 2;
                
            }
        }

        if(endSequence == 2)
        {
            decision.Show();
            if(decision.GetDecision() != '0')
            {
                endSequence = 3;
            }
        }

        if(endSequence == 3)
        {
            decision.Hide();
            if(decision.GetDecision() == 'y')
            {
                catCounter.SetMessage("You let the cats be free. Thanks for play-testing our game!");

                foreach (GameObject cat in atHome)
                {
                    Destroy(cat);
                }
            } 
            else if(decision.GetDecision() == 'n')
            {
                catCounter.SetMessage("You are keeping the cats locked in. Thanks for play-testing our game!");
            }
           endSequence = 4;

        }
    }

    public void Begin()
    {
        hasBegun = true;
        player.GetComponent<Movement>().LockMovement();
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
            catCounter.AddCat();
        }
    }

    public void RemoveFromFollowers(GameObject cat)
    {
        if (followers.Contains(cat)) {
            followers.Remove(cat);
            catCounter.RemoveCat();
        }
    }

    public void AddToHome(GameObject cat)
    {
        if (!atHome.Contains(cat))
        {
            atHome.Add(cat);
        }
    }

    public void RemoveFromHome(GameObject cat)
    {
        if (atHome.Contains(cat))
        {
            atHome.Remove(cat);
        }
    }
}
