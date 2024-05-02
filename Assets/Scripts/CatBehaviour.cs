using System;
using System.Collections.Generic;
using UnityEngine;

public class CatBehaviour : MonoBehaviour
{
    
    [SerializeField] private bool triggerActive = false;
    private int objectId;

    private RectTransform uiButtonPrefab;

    private AudioSource[] Meow;

    private RectTransform currentUIButton;

    private CatCounter catCounter;

    private List<Slot> slots;
    GameObject slotsObject;

    Boolean caught = false;

    private void Start()
    {
        slots = new List<Slot>();
        
        Meow = gameObject.GetComponents<AudioSource>();

        uiButtonPrefab = Resources.Load<RectTransform>("CatCatcher");
        HideUIButton();

        catCounter = GameObject.Find("Cat Counter").GetComponent<CatCounter>();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            triggerActive = true;
            ShowUIButton();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            triggerActive = false;
            HideUIButton();
        }
    }

    private void Update()
    {
        if (triggerActive && Input.GetKeyDown(KeyCode.E))
        {
            PlaySound(Meow[1].clip);
            CatchCat();
        }
    }

    private void PlaySound(AudioClip clip)
    {
        GameObject soundObject = new GameObject("TemporarySoundObject");
        AudioSource audioSource = soundObject.AddComponent<AudioSource>();

        audioSource.clip = clip;
        audioSource.Play();

        Destroy(soundObject, clip.length);

    }

    public void CatchCat()
    {
        if (!caught)
        {
            // Save the ID of the object
            objectId = gameObject.GetInstanceID();

            slotsObject = GameObject.Find("Slots");

            // Send cat to a empty slot inside the house
            foreach (Transform child in slotsObject.transform)
            {
                slots.Add(child.GetComponent<Slot>());
            }
            foreach (Slot slot in slots)
            {
                if (!slot.IsOccupied())
                {
                    Debug.Log(slot.gameObject.transform.position);
                    gameObject.transform.position = slot.gameObject.transform.position;
                    gameObject.transform.localRotation = slot.gameObject.transform.localRotation;
                    slot.SetInstance(gameObject);

                    slot.SetOccupied(true);
                    break;
                }
            }
            /*gameObject.SetActive(false);*/
            HideUIButton();

            catCounter.AddCat();
            caught = true;
        }
        
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

    public Boolean HasBeenCaught()
    {
        return caught;
    }
}