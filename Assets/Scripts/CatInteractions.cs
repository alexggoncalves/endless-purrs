using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatInteractions : MonoBehaviour
{
    //https://www.youtube.com/watch?v=dHzeHh-3bp4

    [SerializeField] Camera cam;
    private Vector3 targetPosition;
    public RectTransform pointer;
    private bool isPointerActive = false;
    public float distance = 20f;
    private bool isCooldown = false;
    private float cooldownDuration = 3f;
    private float lastDeactivationTime = 0f;

    private void Start()
    {
        pointer.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && !isPointerActive && !isCooldown)
        {
            float maxDistance = distance;
            float distanceToTarget = Vector3.Distance(cam.transform.position, targetPosition);

            if (distanceToTarget <= maxDistance)
            {
                isPointerActive = true;
                pointer.gameObject.SetActive(true);
                targetPosition = GameObject.FindWithTag("ArrowTarget").transform.position;
                StartCoroutine(StopPointerAfterDelay());
            }
        }

        if (isPointerActive)
        {
            float fixPos = 9f; //camera dependent magic number
            Vector3 toPosition = targetPosition;
            Vector3 fromPosition = new Vector3(cam.transform.position.x, 0f, cam.transform.position.z + fixPos);
            Vector3 dir = (toPosition - fromPosition).normalized;

            float angle = ((Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg) % 360) - 90;
            pointer.localEulerAngles = new Vector3(0, 0, angle);

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
        }
    }

    private IEnumerator StopPointerAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        isPointerActive = false;
        pointer.gameObject.SetActive(false);
        isCooldown = true;
        lastDeactivationTime = Time.time;
    }
}
