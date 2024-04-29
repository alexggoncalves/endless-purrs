/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatBehaviour : MonoBehaviour
{
    private Camera cam;
    [SerializeField] private bool triggerActive = false;
    private int objectId;

    public RectTransform uiButton;
    //public Vector3 offset;

    private AudioSource[] Meow;

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>() as Camera;
        Meow = gameObject.GetComponents<AudioSource>();
        uiButton.gameObject.SetActive(false);

        //GameObject newObject = Instantiate (yourPrefabRef);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            triggerActive = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            triggerActive = false;
        }
    }

    private void Update()
    {
        if (triggerActive)
        {
            uiButton.gameObject.SetActive(true);

            /*Vector3 targetPosition = gameObject.transform.position;
            Vector3 targetScreenPos = cam.WorldToScreenPoint(targetPosition);

            Vector3 buttonPos = cam.ScreenToWorldPoint(targetScreenPos);

            float canvasDistance = Mathf.Abs(cam.transform.position.y - buttonPos.y);
            float canvasDistanceZ = Mathf.Abs(cam.transform.position.z - buttonPos.z);
            float buttonY = Mathf.Sin(Mathf.Deg2Rad * 45) * canvasDistance;
            float buttonZ = Mathf.Cos(Mathf.Deg2Rad * 45) * canvasDistanceZ;

            Vector3 adjustedPointerPos = new Vector3(buttonPos.x, buttonY, buttonZ);

            uiButton.transform.position = adjustedPointerPos + offset;*/

/*  } else
  {
      uiButton.gameObject.SetActive(false);
  }


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
  uiButton.gameObject.SetActive(false);
}
}*/

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

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>() as Camera;
        Meow = gameObject.GetComponents<AudioSource>();

        uiButtonPrefab = Resources.Load<RectTransform>("CatCatcher");
        HideUIButton();
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