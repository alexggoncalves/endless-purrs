using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private bool triggerActive = false;

    public CharacterController player;
    public GameObject playerCollidion;
    public Vector3 target = new Vector3(0, 0, 0);

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            triggerActive = true;

            /*player.transform.position = target;
            print("Hallo");

            DestroyPortal();*/
        }
    }

    private void Update()
    {
        if (triggerActive)
        {
            player.transform.position = target;
            playerCollidion.transform.position = target;
            print("Hallo");

            triggerActive = false;
            //Destroy(this.gameObject);
        }
    }
}