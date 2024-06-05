using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
    public float locaterRange = 80f;

    private AudioSource call;
    private AudioSource[] Miau;

    public PlayerController player;
    private void Start()
    {
        pointer.gameObject.SetActive(false);
        button.gameObject.SetActive(true);
        call = GetComponent<AudioSource>();
    }

    public List<GameObject> FindCatsInRange()
    {
        List<GameObject> catsInRange = new List<GameObject> ();
        GameObject[] allCats;
        allCats = GameObject.FindGameObjectsWithTag("Cat");

        if (allCats != null)
        {
            Vector3 playerPosition = player.transform.position;

            foreach (GameObject cat in allCats)
            {
                Vector3 diff = cat.transform.position - playerPosition;
                float curDistance = diff.sqrMagnitude;

                if (curDistance < locaterRange
                    && !cat.GetComponent<CatController>().IsAtHome
                    && !cat.GetComponent<CatIdentity>().behaviour.Equals(BehaviourType.Scaredy)
                    && !cat.GetComponent<CatController>().GetCatState().Equals(CatState.Following)
                    )
                {
                    catsInRange.Add(cat);
                }
            }
        }
        if (catsInRange.Count > 0) { return catsInRange; }
        else return null;
    }

    

    private void Update()
    {
        Boolean success = GameObject.Find("Game").GetComponent<Game>().Success();
        if (success)
        {
            float fixXPos = 0;
            float fixZPos = 0;
            targetPosition = Vector3.zero;
            pointer.gameObject.SetActive(true);
            button.gameObject.SetActive(false);
            Vector3 toPosition = targetPosition;
            Vector3 fromPosition = new Vector3(player.transform.position.x - fixXPos, 0f, player.transform.position.z + fixZPos);
            Vector3 dir = (toPosition - fromPosition).normalized;

            float angle = ((Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg) % 360) - 90;
            pointer.localEulerAngles = new Vector3(0, 0, angle);

        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Q) && !isPointerActive && !isCooldown)
            {
                button.gameObject.SetActive(false);

                // Get all cats in range
                List<GameObject> catsInRange = FindCatsInRange();

                if(catsInRange != null)
                {
                    foreach (GameObject cat in catsInRange)
                    {
                        // Create the elements at the correct position!!!
                    }
                }
                

                /// TEMP////////////////////////
                if (catsInRange != null)
                {
                    isPointerActive = true;
                    pointer.gameObject.SetActive(true);

                    targetPosition = catsInRange[0].transform.position;

                    StartCoroutine(StopPointerAfterDelay());
                }
                else
                {
                    call.Play();
                    isCooldown = true;
                    lastDeactivationTime = Time.time;
                }
                ////////////////////
            }

            if (isPointerActive) // Change this as well
            {
                float fixXPos = 0;
                float fixZPos = 0;
                Vector3 toPosition = targetPosition;
                Vector3 fromPosition = new Vector3(player.transform.position.x - fixXPos, 0f, player.transform.position.z + fixZPos);
                Vector3 dir = (toPosition - fromPosition).normalized;

                float angle = ((Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg) % 360) - 90;
                pointer.localEulerAngles = new Vector3(0, 0, angle);
            }

            if (isCooldown && Time.time - lastDeactivationTime >= cooldownDuration)
            {
                isCooldown = false;
                button.gameObject.SetActive(true);
            }
        }
    }

    private IEnumerator StopPointerAfterDelay()
    {
        call.Play();
        yield return new WaitForSeconds((call.clip.length)+0.2f);
        Miau = FindCatsInRange()[0].GetComponents<AudioSource>(); //plays first audioSource of the cat
        Miau[1].Play();
        yield return new WaitForSeconds(pointerTime);
        isPointerActive = false;
        pointer.gameObject.SetActive(false);
        isCooldown = true;
        lastDeactivationTime = Time.time;
    }
}
