using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    public Transform target;
    //public GameObject pointAhead;
    public Vector3 offset = new Vector3(0, 15, -13);
    public float smoothTime = 0.65f;

    Vector3 currentVelocity;

    private void LateUpdate()
    {
        if (target)
        {
            //Vector3 aheadPoint = target.position + new Vector3(pointAhead.velocity.x, 0, 0);
            //Vector3 aheadPoint = target.position + new Vector3(target.GetComponent<Rigidbody2D>().velocity.x, 0, 0);

            transform.position = Vector3.SmoothDamp(
                 transform.position,
                 target.position + offset,
                 ref currentVelocity,
                 smoothTime
                 );
        }
    }
}
