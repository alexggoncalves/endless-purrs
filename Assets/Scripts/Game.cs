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
    Speech speech;

    private void Start()
    {
        speech = player.transform.GetChild(1).gameObject.GetComponent<Speech>(); // Get Speech
        player.transform.position = GameObject.FindGameObjectWithTag("SpawnPoint").transform.position;
    }

    private void Update()
    {
        
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
