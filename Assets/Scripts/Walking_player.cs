using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed = 3.0f;
    public GameObject character;

    void Update()
    {

        if ((Input.GetKey(KeyCode.RightArrow)) || (Input.GetKey(KeyCode.D)))
        {
            transform.position += Vector3.right * speed * Time.deltaTime;
        }
        if ((Input.GetKey(KeyCode.LeftArrow)) || (Input.GetKey(KeyCode.A)))
        {
            transform.position += Vector3.left * speed * Time.deltaTime;
        }
        if ((Input.GetKey(KeyCode.UpArrow)) || (Input.GetKey(KeyCode.W)))
        {
            transform.position += Vector3.forward * speed * Time.deltaTime;
        }
        if ((Input.GetKey(KeyCode.DownArrow)) || (Input.GetKey(KeyCode.S)))
        {
            transform.position += Vector3.back * speed * Time.deltaTime;
        }
    }

}