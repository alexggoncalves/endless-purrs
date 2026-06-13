using System;
using System.Collections.Generic;
using UnityEngine;

public class CatBehaviour : MonoBehaviour
{
    
    private bool inPlayersRange = false;
    private int objectId;

    private RectTransform uiButtonPrefab;

    private AudioSource[] Meow;

    private RectTransform currentUIButton;


    private void Start()
    {
        Meow = gameObject.GetComponents<AudioSource>();


    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inPlayersRange = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inPlayersRange = false;
        }
    }    

    public void PlayMeow()
    {
        PlaySound(Meow[1].clip);

        // Set a random meow for everycat!!!!!!!!!
    }

    private void PlaySound(AudioClip clip)
    {
        GameObject soundObject = new GameObject("TemporarySoundObject");
        AudioSource audioSource = soundObject.AddComponent<AudioSource>();

        audioSource.clip = clip;
        audioSource.Play();

        Destroy(soundObject, clip.length);

    }


    public bool InPlayersRange() { return inPlayersRange; }
}