using System;
using System.Collections;
using UnityEngine;

public class CatLocater : MonoBehaviour
{
    //https://www.youtube.com/watch?v=dHzeHh-3bp4

    private Vector3 targetPosition;
    public RectTransform pointer;
    public RectTransform button;

    private bool isPointerActive = false;
    public float distance = 50f;
    private float pointerTime = 3.5f;
    private bool isCooldown = false;
    private float cooldownDuration = 2f;
    private float lastDeactivationTime = 0f;

    private AudioSource call;
    private AudioSource[] Miau;

    public CatCounter Success;

    public Movement player;
    private void Start()
    {
        pointer.gameObject.SetActive(false);
        button.gameObject.SetActive(true);
        call = GetComponent<AudioSource>();
    }

    public GameObject FindClosestCat()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Cat");

        if (gos != null)
        {
            GameObject closest = null;
            float distance = Mathf.Infinity;
            Vector3 position = player.transform.position;
            foreach (GameObject go in gos)
            {
                Debug.Log(go.GetComponent<CatController>().IsAtHome);
                Vector3 diff = go.transform.position - position;
                float curDistance = diff.sqrMagnitude;
                if (curDistance < distance 
                    && !go.GetComponent<CatController>().IsAtHome
                    && !go.GetComponent<CatIdentity>().behaviour.Equals(BehaviourType.Scaredy)
                    && !go.GetComponent<CatController>().GetCatState().Equals(CatState.Following)
                    )
                {
                    closest = go;
                    distance = curDistance;
                }
            }
            return closest;
        } else
        {
            return null;
        }
    }

    

    private void Update()
    {
        Boolean success = GameObject.Find("Cat Counter").GetComponent<CatCounter>().Success();
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
                float maxDistance = distance;
                float distanceToTarget = Vector3.Distance(player.transform.position, targetPosition);

                if (distanceToTarget <= maxDistance && FindClosestCat() != null)
                //if (distanceToTarget <= maxDistance)
                {
                    isPointerActive = true;
                    pointer.gameObject.SetActive(true);
                    targetPosition = FindClosestCat().transform.position;

                    StartCoroutine(StopPointerAfterDelay());
                }
                else
                {
                    call.Play();
                    isCooldown = true;
                    lastDeactivationTime = Time.time;
                }
            }

            if (isPointerActive)
            {
                float fixXPos = 0;
                float fixZPos = 0;
                Vector3 toPosition = targetPosition;
                Vector3 fromPosition = new Vector3(player.transform.position.x - fixXPos, 0f, player.transform.position.z + fixZPos);
                Vector3 dir = (toPosition - fromPosition).normalized;

                float angle = ((Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg) % 360) - 90;
                pointer.localEulerAngles = new Vector3(0, 0, angle);

                //code for dynamic pointer
                /*float borderSize = 500f;
                Vector3 targetScreenPos = cam.WorldToScreenPoint(targetPosition);
                bool isOffScreen = targetScreenPos.x <= borderSize || targetScreenPos.x >= Screen.width - borderSize || targetScreenPos.y <= borderSize || targetScreenPos.y >= Screen.height - borderSize;

                if (isOffScreen)
                {
                    Vector3 cappedTargetScreenPos = targetScreenPos;
                    if (cappedTargetScreenPos.x <= borderSize) cappedTargetScreenPos.x = borderSize;
                    if (cappedTargetScreenPos.x >= Screen.width - borderSize) cappedTargetScreenPos.x = Screen.width - borderSize;
                    if (cappedTargetScreenPos.y <= borderSize) cappedTargetScreenPos.y = borderSize;
                    if (cappedTargetScreenPos.y >= Screen.height - borderSize) cappedTargetScreenPos.y = Screen.height - borderSize;

                    Vector3 pointerPos = cam.ScreenToWorldPoint(cappedTargetScreenPos);
                    //pointer.position = new Vector3(pointerPos.x, 1.5f, pointerPos.y/2f);

                    // Calculate y and z positions for pointer with a 45-degree angle
                    float canvasDistance = Mathf.Abs(cam.transform.position.y - pointerPos.y);
                    float pointerY = Mathf.Sin(Mathf.Deg2Rad * 45) * canvasDistance;
                    float pointerZ = Mathf.Cos(Mathf.Deg2Rad * 45) * canvasDistance;

                    Vector3 adjustedPointerPos = new Vector3(pointerPos.x, pointerY, -pointerZ);
                    pointer.position = adjustedPointerPos;
                } else
                {
                    Vector3 pointerPos = cam.ScreenToWorldPoint(targetScreenPos);

                    float canvasDistance = Mathf.Abs(cam.transform.position.y - pointerPos.y);
                    float pointerY = Mathf.Sin(Mathf.Deg2Rad * 45) * canvasDistance;
                    float pointerZ = Mathf.Cos(Mathf.Deg2Rad * 45) * canvasDistance;

                    Vector3 adjustedPointerPos = new Vector3(pointerPos.x, pointerY, -pointerZ);
                    pointer.position = adjustedPointerPos;
                }*/
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
        Miau = FindClosestCat().GetComponents<AudioSource>(); //plays first audioSource of the cat
        Miau[1].Play();
        yield return new WaitForSeconds(pointerTime);
        isPointerActive = false;
        pointer.gameObject.SetActive(false);
        isCooldown = true;
        lastDeactivationTime = Time.time;
    }
}
