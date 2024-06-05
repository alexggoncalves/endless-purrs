using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private bool triggerActive = false;

    public GameObject player;
    public Vector3 target = new Vector3(-8, 0, 0);
    public MapGenerator mapGenerator;

    private void Start()
    {
        player = GameObject.Find("Player");
        mapGenerator = GameObject.Find("Map Generator").GetComponent<MapGenerator>();
        target = GameObject.FindGameObjectWithTag("SpawnPoint").transform.position;
    }

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
            player.GetComponent<PlayerController>().SetTeleporting(true);
            triggerActive = false;

            
               
            mapGenerator.GetWFC().MoveToOrigin();

            player.transform.position = target;
            mapGenerator.lastPlayerCoordinates = mapGenerator.playerCoordinates;
            player.GetComponent<PlayerController>().SetTeleporting(false);
            Destroy(this.gameObject, 1);
        }
    }
}
