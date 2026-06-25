using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CatController))]
public class CatHouseSystem : MonoBehaviour
{
    private GameManager game;
    private CatController cat;

    private bool isAtHome = false;

    private BoxCollider houseTrigger;
    private Transform homeRoot;

    private float homeCheckTimer = 0f;
    private const float POLL_INTERVAL = 0.2f;

    private void Start()
    {
        // Find Game Manager
        var gameObj = GameObject.Find("GameManager");
        if (gameObj != null) game = gameObj.GetComponent<GameManager>();

        cat = GetComponent<CatController>();
    }

    public void Tick()
    {
        TryFindHouse();

        if (houseTrigger == null) return;

        UpdateHomeCheck();
        UpdateAtHomeDeactivation();
    }
    private void TryFindHouse()
    {
        if (houseTrigger == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("HouseInterior");
            if (obj != null)
                houseTrigger = obj.GetComponent<BoxCollider>();
        }

        if (homeRoot == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Home");
            if (obj != null)
                homeRoot = obj.transform;
        }
    }

    private void UpdateHomeCheck()
    {
        if (isAtHome || houseTrigger == null) return;

        homeCheckTimer -= Time.deltaTime;
        if (homeCheckTimer > 0) return;
        homeCheckTimer = POLL_INTERVAL;

        if (houseTrigger.bounds.Contains(transform.position))
        {
            isAtHome = true;
            game.AddToHome(cat);

            if (homeRoot != null)
                transform.SetParent(homeRoot, true);
        }
    }

    private void UpdateAtHomeDeactivation()
    {
        //if (Vector3.Distance(transform.position, player.position) <= 40f) return;

        //if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        //    agent.ResetPath();

        //agent.enabled = false;
        //wander.enabled = false;

        //animator.SetBool(IsMovingHash, false);
        //animator.SetFloat(SpeedMultiplierHash, 0f);
    }

    //public static void MoveFollowersHome()
    //{
    //    GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");
    //    if (spawnPoint == null) return;

    //    Vector3 spawnPos = spawnPoint.transform.position;

    //    // Followers being teleported home
    //    foreach (var controller in followers.ToArray())
    //    {
    //        if (controller == null) continue;
    //        NavMeshAgent agent = controller.GetComponent<NavMeshAgent>();
    //        controller.transform.position = spawnPos;

    //        if (agent != null)
    //        {
    //            agent.enabled = true;
    //            agent.ResetPath();
    //            agent.velocity = Vector3.zero;
    //            agent.Warp(spawnPos);
    //        }

    //        controller.SetState(CatState.Wandering);
    //        //controller.SetState(CatState.AtHome);

    //        RemoveFromFollowers(controller);
    //        AddToHome(controller);
    //    }
    //}

    public Vector2 GetHouseDimensions()
    {
        Vector3 size = houseTrigger.bounds.size;
        return new Vector2(size.x, size.z);
    }

    public Vector3 GetHouseCenter()
    {
        Vector3 c = houseTrigger.bounds.center;
        return new Vector3(c.x, 0, c.z);
    }

    public bool IsAtHome() => isAtHome;
}
