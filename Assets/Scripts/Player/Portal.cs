using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private bool triggerActive = false;

    public GameObject player;
    public Vector3 target = new Vector3(0, 0, 0);
    public MapGenerator mapGenerator;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            triggerActive = true;
        }
    }

    private void FixedUpdate()
    {
        if (triggerActive)
        {
            player.transform.position = target;

            triggerActive = false;
            /*mapGenerator.GetWFC().MoveToOrigin();*/

            Destroy(this.gameObject, 1);
        }
    }
}
