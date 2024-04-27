using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseController : MonoBehaviour
{
    bool isPlayerInside = false;

    [SerializeField]
    Transform door;

    [SerializeField]
    GameObject houseTop;

    [SerializeField] 
    float closedDoorAngle = 0f;
    [SerializeField]
    float openDoorAngle = -160f;
    [SerializeField]
    float doorSpeed = 20f;

    private Quaternion closedDoorRotation;
    private Quaternion openDoorRotation;

    private void Start()
    {
        closedDoorRotation = door.rotation;
        openDoorRotation = Quaternion.Euler(door.rotation.eulerAngles + Vector3.up * openDoorAngle);
    }

    private void FixedUpdate()
    {
        if(isPlayerInside)
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
            isPlayerInside = true;
            
            HideHouseTop();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            ShowHouseTop();
        }
    }



    void HideHouseTop()
    {
        Material[] materials = houseTop.GetComponent<MeshRenderer>().materials;
        foreach(Material mat in materials)
        {
            Color color = mat.color;
            color.a = 0;
            mat.color = color;
        }
    }

    void ShowHouseTop()
    {
        Material[] materials = houseTop.GetComponent<MeshRenderer>().materials;
        foreach (Material mat in materials)
        {
            Color color = mat.color;
            color.a = 1f;
            mat.color = color;
        }
    }
}
