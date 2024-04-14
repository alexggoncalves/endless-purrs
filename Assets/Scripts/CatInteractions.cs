using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatInteractions : MonoBehaviour
{
    //https://www.youtube.com/watch?v=dHzeHh-3bp4

    [SerializeField] Camera cam;
    private Vector3 targetPosition;
    //private RectTransform pointer;
    [SerializeField] Transform pointer;

    private void Awake()
    {
        targetPosition = GameObject.FindWithTag("ArrowTarget").transform.position; //only for one cat in scenario
        //pointer = transform.Find("Pointer").GetComponent<RectTransform>();
    }

    private void Update()
    {
        Vector3 toPosition = targetPosition;
        Vector3 fromPosition = new Vector3 (cam.transform.position.x, 0f, cam.transform.position.z);
        Vector3 dir = (toPosition - fromPosition).normalized;
        print(fromPosition);

        float angle = ((Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg) % 360)-90;
        pointer.localEulerAngles = new Vector3(0, 0, angle);
    }
}
