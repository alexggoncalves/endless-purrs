using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaveor : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 15, -13);
    public float smoothTime = 0.45f;

    Vector3 currentVelocity;

    private void LateUpdate()
    {
        transform.position = Vector3.SmoothDamp(
            transform.position,
            target.position + offset,
            ref currentVelocity,
            smoothTime
            );
    }
}
