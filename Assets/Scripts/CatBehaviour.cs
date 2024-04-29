using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatBehaviour : MonoBehaviour
{
    private Camera cam;
    [SerializeField] private bool triggerActive = false;
    private int objectId;

    private RectTransform uiButtonPrefab;

    private AudioSource[] Meow;

    private RectTransform currentUIButton;

    public CatCounter catCounter;

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>() as Camera;
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
        // Save the ID of the object
        objectId = gameObject.GetInstanceID();

        print(objectId);
        gameObject.SetActive(false);
        HideUIButton();

        catCounter.AddCat();
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
}