using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class CatLocater : MonoBehaviour
{
    //https://www.youtube.com/watch?v=dHzeHh-3bp4

    private Vector3 targetPosition;
    public RectTransform pointer;
    public RectTransform button;

    private bool isPointerActive = false;
    private float pointerTime = 3.5f;
    private bool isCooldown = false;
    private float cooldownDuration = 2f;
    private float lastDeactivationTime = 0f;

    [SerializeField]
    public float locaterRange = 100f;

    private AudioSource call;
    private AudioSource[] Miau;

    List<GameObject> catsInRange = new List<GameObject>();
    List<GameObject> activePointers = new List<GameObject>();
    public GameObject pointerObj;

    public PlayerController player;
    public GameObject pointerBase;

    private void Start()
    {
        
        
        button.gameObject.SetActive(true);
        call = GetComponent<AudioSource>();
    }

    public List<GameObject> FindCatsInRange()
    {
        GameObject[] allCats;
        
        allCats = GameObject.FindGameObjectsWithTag("Cat");
        List<GameObject> catsLocated = new List<GameObject>();

        if (allCats != null)
        {
            Vector3 playerPosition = player.transform.position;

            foreach (GameObject cat in allCats)
            {
                Vector3 diff = cat.transform.position - playerPosition;
                float curDistance = diff.magnitude;

                Debug.Log(curDistance);
                if (curDistance < locaterRange
                    && !cat.GetComponent<CatController>().IsAtHome
                    && !cat.GetComponent<CatIdentity>().behaviour.Equals(BehaviourType.Scaredy)
                    && !cat.GetComponent<CatController>().GetCatState().Equals(CatState.Following)
                    )
                {
                    Debug.Log("added");
                    catsLocated.Add(cat);
                }
            }
        }
        if (catsLocated.Count > 0) { return catsLocated; }
        else return null;
    }

    

    private void Update()
    {
       
         if (Input.GetKeyDown(KeyCode.Q) && !isPointerActive && !isCooldown)
         {
             button.gameObject.SetActive(false);

            // Get all cats in range
            catsInRange = FindCatsInRange();

             if(catsInRange != null)
             {
                 foreach (GameObject cat in catsInRange)
                 {
                     Debug.Log("aa");
                     GameObject pointerInstance = Instantiate(pointerObj);
                     pointerInstance.transform.SetParent(pointerBase.transform,false);
                     
                     activePointers.Add(pointerInstance);
                 }
             }

            if (catsInRange != null)
            {
                isPointerActive = true;             
                StartCoroutine(StopPointerAfterDelay());
            }
            else
            {
                call.Play();
                isCooldown = true;
                lastDeactivationTime = Time.time;
            }
        }

         if (isPointerActive && !player.IsTeleporting()) // Change this as well
         {
            for(int i = 0; i < activePointers.Count; i++)
            {
                GameObject pointerInstance = activePointers[i];

                if (catsInRange[i] != null)
                {
                    Vector3 toPosition = catsInRange[i].transform.position;
                    Vector3 dir = (toPosition - player.transform.position).normalized;
                    float angle = ((Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg) % 360);

                    pointerInstance.transform.localEulerAngles = new Vector3(0, 0, angle);
                }
            }
         }

         if (isCooldown && Time.time - lastDeactivationTime >= cooldownDuration)
         {
             isCooldown = false;
             button.gameObject.SetActive(true);
         } 
         
    }

    private IEnumerator StopPointerAfterDelay()
    {
        call.Play();
        yield return new WaitForSeconds((call.clip.length) + 0.2f);

        foreach (GameObject cat in catsInRange)
        {
            Miau = cat.GetComponents<AudioSource>(); //plays first audioSource of the cat
            Miau[1].Play(); 
            new WaitForSeconds(UnityEngine.Random.Range(0.2f, 1.2f));
        }
        yield return new WaitForSeconds(pointerTime);

        isPointerActive = false;
        //Destroy all pointers
        catsInRange.Clear();
        foreach(GameObject pointer in activePointers)
        {
            Destroy(pointer);
        }
        activePointers.Clear();


        isCooldown = true;
        lastDeactivationTime = Time.time;
    }
}
