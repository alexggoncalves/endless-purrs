using System;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    bool doorOpened = false;

    [SerializeField]
    public Transform door;

    [SerializeField]
    float openDoorAngle = -160f;
    [SerializeField]
    float doorSpeed = 2f;

    private Quaternion closedDoorRotation;
    private Quaternion openDoorRotation;

    private AudioSource[] DoorSound;

    private void Start()
    {
        closedDoorRotation = door.rotation;
        openDoorRotation = Quaternion.Euler(door.rotation.eulerAngles + Vector3.up * openDoorAngle);
        DoorSound = GetComponents<AudioSource>();
    }

    private void FixedUpdate()
    {
        if(doorOpened)
        {
            door.rotation = Quaternion.Lerp(door.rotation, openDoorRotation, Time.fixedDeltaTime * doorSpeed);
        }
        else
        {
            door.rotation = Quaternion.Lerp(door.rotation, closedDoorRotation, Time.fixedDeltaTime * doorSpeed);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            doorOpened = true;
            DoorSound[0].Play();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            doorOpened = false;
            DoorSound[1].Play();
        }
    }

    

    public Boolean IsDoorOpen()
    {
        return doorOpened;
    }
}
