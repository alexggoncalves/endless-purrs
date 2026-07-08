using System;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private Transform door;
    [SerializeField] float openDoorAngle = -160f;
    [SerializeField] float doorSpeed = 2f;

    [SerializeField] AudioClip doorOpenSound;
    [SerializeField] AudioClip doorCloseSound;
    private SoundFXManager SoundFXManager;

    private Quaternion closedDoorRotation;
    private Quaternion openDoorRotation;

    int insideCount = 0;
    bool doorOpened = false;

    private void Start()
    {
        closedDoorRotation = door.rotation;
        openDoorRotation = Quaternion.Euler(door.rotation.eulerAngles + Vector3.up * openDoorAngle);

        SoundFXManager = GameObject.FindGameObjectWithTag("SoundFXManager").GetComponent<SoundFXManager>();
    }

    private void FixedUpdate()
    {
        if (doorOpened)
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
        if (IsValidEntity(other))
        {
            insideCount++;

            if (insideCount == 1)
                Open();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsValidEntity(other))
        {
            insideCount = Mathf.Max(0, insideCount - 1);

            if (insideCount == 0)
                Close();
        }
    }


    private bool IsValidEntity(Collider other)
    {
        return other.CompareTag("Player");
    }

    public void Open()
    {
        doorOpened = true;

        if (SoundFXManager != null)
            SoundFXManager.PlaySoundFXClip(doorOpenSound, transform.position, 0.5f);
    }

    public void Close()
    {
        doorOpened = false;

        if (SoundFXManager != null)
            SoundFXManager.PlaySoundFXClip(doorCloseSound, transform.position, 0.5f);
    }

    public Boolean IsOpen()
    {
        return doorOpened;
    }

    public bool IsFullyOpen()
    {
        return doorOpened &&
               Quaternion.Angle(door.rotation, openDoorRotation) < 1f;
    }
}
