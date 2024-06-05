using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private bool triggerActive = false;

    private GameObject player;
    private Vector3 target;
    private MapGenerator mapGenerator;
    private Game game;


    private void Start()
    {
        player = GameObject.Find("Player");
        mapGenerator = GameObject.Find("Map Generator").GetComponent<MapGenerator>();
        target = GameObject.FindGameObjectWithTag("SpawnPoint").transform.position;
        game = GameObject.Find("Game").GetComponent<Game>();
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
            game.MoveFollowersHome();

            player.GetComponent<PlayerController>().SetTeleporting(false);
            Destroy(this.gameObject, 1);
        }
    }
}
