using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] Transform player;
    [SerializeField] float threshold;

    // Update is called once per frame
    void Update()
    {
        Vector3 mouse = Input.mousePosition;
        //Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        //Vector3 mousePos = Input.mousePosition;
        Vector3 mousePos = cam.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, 10));
        Vector3 targetPos = new Vector3((player.position.x + mousePos.x) / 2f, 0f, (player.position.z + mousePos.y));

        targetPos.x = Mathf.Clamp(targetPos.x, -threshold/2 + player.position.x, threshold/2 + player.position.x);
        targetPos.z = Mathf.Clamp(targetPos.z, -threshold/2 + player.position.z, threshold/2 + player.position.z);

        this.transform.position = targetPos;
    }
}
