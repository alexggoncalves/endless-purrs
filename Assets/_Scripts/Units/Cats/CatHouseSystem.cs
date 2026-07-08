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
        if (houseTrigger == null) return;

        homeCheckTimer -= Time.deltaTime;
        if (homeCheckTimer > 0) return;
        homeCheckTimer = POLL_INTERVAL;

        if (houseTrigger.bounds.Contains(transform.position) && !isAtHome)
        {
            isAtHome = true;
            game.AddToHome(cat);

            if (homeRoot != null)
                transform.SetParent(homeRoot, true);
        }
        else if (!houseTrigger.bounds.Contains(transform.position) && isAtHome)
        {
            isAtHome = false;
            game.RemoveFromHome(cat);

        }
    }

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
