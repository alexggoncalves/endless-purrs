using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class MapGenerator : MonoBehaviour
{

    [Range(0, 4)] public float cellScale = 2;
    [Range(1, 100)] public int gridWidth, gridHeight;
    [SerializeField] Vector2 gridOffset;
    [SerializeField] Vector2 edgeSize = new(4, 4);
    [SerializeField] Vector2 placesExtents = new(200, 200);
    [SerializeField, Min(1)] int placeDensity = 6;
    [SerializeField, Range(0, 1)] float natureElementRate = 0.5f;

    public Cell cellObj;
    public TileLoader tileLoader;

    // Player 
    PlayerController playerController;
    private Vector2 lastPlayerCoordinates; // (According to the grid)
    private Vector2 playerCoordinates; // (According to the grid)

    // Places
    public GameObject startingPlace; // House
    public List<Place> orderedPlaces; // Places that are related to the story (UNUSED FOR NOW) 
    public List<Place> unorderedPlaces; // Random places to place around the map

    private readonly List<Place> placeInstances = new();
    private readonly List<Place> placesToDestroy = new();
    private readonly WaitForSeconds checkPlacesInterval = new(1.5f);
    private float placesSpawnCooldown = 0f;

    // Terrain generation algorithm
    private WaveFunctionCollapse wfc;

    private bool hasStarted = false;

    public void BeginGeneration()
    {
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();

        wfc = this.AddComponent<WaveFunctionCollapse>();
        wfc.Initialize(tileLoader, gridWidth, gridHeight, cellScale, cellObj, gridOffset, playerController, startingPlace, edgeSize, natureElementRate);

        lastPlayerCoordinates = wfc.CalculateWorldCoordinates(playerController.transform.position.x, playerController.transform.position.z);

        StartCoroutine(CheckPlacesRoutine());

        hasStarted = true;
    }

    public void Update()
    {
        if (!hasStarted) return;

        // Remove places assigned to be destroyed
        foreach (Place place in placesToDestroy)
        {
            wfc.RemovePlace(place);
            placeInstances.Remove(place);
            Destroy(place.gameObject);
        }
        placesToDestroy.Clear();

        HandleGridMove();
        SpawnPlaces();

    }

    /// <summary>
    /// Detects every time the player moves one cell size and shifts the grid in that direction
    /// </summary>
    public void HandleGridMove()
    {
        if (playerController.IsTeleporting) return;

        playerCoordinates = wfc.CalculateWorldCoordinates(playerController.transform.position.x, playerController.transform.position.z);

        if (playerCoordinates != lastPlayerCoordinates)
        {
            wfc.AddToMoveOffset(playerCoordinates - lastPlayerCoordinates);
        }

        if ((wfc.GetMoveOffset().x != 0 || wfc.GetMoveOffset().y != 0) && wfc.IsPaused() && !wfc.IsUpdatingCells())
        {
            wfc.ShiftGrid();
        }

        lastPlayerCoordinates = playerCoordinates;
    }

    public void ForceResyncAfterTeleport()
    {
        playerCoordinates = wfc.CalculateWorldCoordinates(
            playerController.transform.position.x,
            playerController.transform.position.z
        );

        lastPlayerCoordinates = playerCoordinates;
    }

    private void SpawnPlaces()
    {
        // Cooldown timer for place check
        placesSpawnCooldown -= Time.deltaTime;
        if (placesSpawnCooldown > 0f) return;
        placesSpawnCooldown = 0.5f;

        if (placeInstances.Count >= placeDensity) return;

        Place place = unorderedPlaces[UnityEngine.Random.Range(0, unorderedPlaces.Count)];
        Vector3 center = playerController.transform.position + new Vector3(gridOffset.x, 0, gridOffset.y);

        float spawnX = Random.Range(center.x - placesExtents.x, center.x + placesExtents.x);
        float spawnZ = Random.Range(center.z - placesExtents.y, center.z + placesExtents.y);


        // Check if chosen coordinates overlap the active WFC grid area (world space AABB, avoids transform drift issues)
        Vector2 outerSize = wfc.GetOuterAreaSize();
        float outerMargin = cellScale * 3;

        bool collidesWithOuterPlayerArea =
            Mathf.Abs(spawnX - center.x) < (outerSize.x / 2 + outerMargin) &&
            Mathf.Abs(spawnZ - center.z) < (outerSize.y / 2 + outerMargin);

        // Check if chosen coordinates overlap the home/starting area
        bool collidesWithHomeInstance = wfc.GetHomeInstance().GetComponent<Place>().GetExtents()
            .CollidesWith(spawnX, spawnZ, place.GetDimensions().x * cellScale, place.GetDimensions().y * cellScale, cellScale * 4);

        if (collidesWithHomeInstance || collidesWithOuterPlayerArea) return;

        // Check for collisions with other already placed areas
        foreach (Place placed in placeInstances)
        {
            if (placed.GetExtents().CollidesWith(spawnX, spawnZ, place.GetDimensions().x * cellScale, place.GetDimensions().y * cellScale, cellScale * 2))
                return;
        }

        // Placement is valid — instantiate and register with WFC
        Place newPlace = Instantiate(place, new Vector3(spawnX, 0, spawnZ), Quaternion.identity);
        newPlace.Initialize(new Vector2(spawnX, spawnZ), tileLoader.grassID, cellScale);
        placeInstances.Add(newPlace);
        newPlace.onWait = true;
        wfc.AddPlaceForPlacement(newPlace);
    }

    /// <summary>
    /// Removes any place that's outside the determined area
    /// </summary>
    void CheckPlaces()
    {
        Vector3 center = playerController.transform.position + new Vector3(gridOffset.x, 0, gridOffset.y);

        foreach (Place place in placeInstances)
        {
            Vector3 pos = place.transform.position;
            float dx = Mathf.Abs(pos.x - center.x);
            float dz = Mathf.Abs(pos.z - center.z);


            if (dx > placesExtents.x + cellScale * 5 || dz > placesExtents.y + cellScale * 5)
                if (!placesToDestroy.Contains(place))
                    placesToDestroy.Add(place);
        }
    }

    /// <summary>
    /// Periodically checks for places that have moved out of range and marks them for removal.
    /// </summary>
    IEnumerator CheckPlacesRoutine()
    {
        while (true)
        {
            yield return checkPlacesInterval;
            CheckPlaces();
        }
    }

    /// <summary>
    /// Calculates the progress of the initial area generation
    /// </summary>
    /// <returns>A value between 0 and 1</returns>
    public float GetInitialAreaLoadingProgress()
    {
        if (wfc == null || wfc.GetInnerArea() == null) return 0f;
        float total = wfc.GetInnerArea().GetCellArea(cellScale);
        if (total == 0f) return 0f;
        return Mathf.Clamp01(Map(wfc.GetIteration(), 0, total, 0, 1));
    }


    public void SyncPlayerCoordinates()
    {
        lastPlayerCoordinates = playerCoordinates;
    }

    public bool IsInitialAreaGenerated()
    {
        return wfc != null && wfc.HasLoadedInitialZone();
    }

    public bool HasStarted()
    {
        return hasStarted;
    }

    public WaveFunctionCollapse GetWFC()
    {
        return wfc;
    }

    float Map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }
}
