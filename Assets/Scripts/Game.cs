using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;


public class Game : MonoBehaviour
{
    private Boolean hasBegun = false;

    public GameObject player;
    private Movement playerMovement;

    Speech speech;
    public CatCounter catCounter;

    int endSequence = 0;

    private void Start()
    {
        speech = player.transform.GetChild(1).gameObject.GetComponent<Speech>(); // Get Speech
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
                speech.SetText("aaa");
                speech.Show();
            }
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
}
