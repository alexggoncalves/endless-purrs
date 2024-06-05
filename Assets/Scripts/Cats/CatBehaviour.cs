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

        uiButtonPrefab = Resources.Load<RectTransform>("CatCatcher");
        HideUIButton();

    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inPlayersRange = true;
            /*ShowUIButton();*/
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inPlayersRange = false;
            /*HideUIButton();*/
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

    private void ShowUIButton()
    {
        if (currentUIButton == null && uiButtonPrefab != null)
        {
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();

            currentUIButton = Instantiate(uiButtonPrefab, Vector3.zero, Quaternion.identity) as RectTransform;
            currentUIButton.SetParent(canvas.transform, false);
        }
    }

    private void HideUIButton()
    {
        if (currentUIButton != null)
        {
            Destroy(currentUIButton.gameObject);
            currentUIButton = null;
        }
    }

    public bool InPlayersRange() { return inPlayersRange; }
}