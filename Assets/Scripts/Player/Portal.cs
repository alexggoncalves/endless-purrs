using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private bool triggerActive = false;

    public GameObject player;
    public Vector3 target = new Vector3(-8, 0, 0);
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
        if (triggerActive && mapGenerator.GetWFC().IsPaused())
        {
            player.GetComponent<Movement>().SetTeleporting(true);
            triggerActive = false;

            mapGenerator.GetWFC().MoveToOrigin();
            

            player.GetComponent<Movement>().SetTeleporting(false);
            Destroy(this.gameObject, 1);
        }
    }
}
