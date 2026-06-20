using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Random = UnityEngine.Random;

[RequireComponent(typeof(TileLoader))]
public class WorldGenerator : MonoBehaviour
{
    [Header("Wave Function Collapse")]
    [SerializeField] private float cellScale = 2;
    [SerializeField] private Vector2 gridSize;
    [SerializeField] private Vector2 gridOffset;
    [SerializeField] private int edgeSize = 2;
    [SerializeField] private Cell cellObj;

    [Header("Places")]
    [SerializeField] private int placesMargin = 40;
    [SerializeField, Min(1)] private int placeDensity = 6;
    [SerializeField] private int placesDeletionMargin = 4;
    [SerializeField] private GameObject startingPlace; // House
    [SerializeField] private List<Place> places; // Random places to place around the map
    //[SerializeField] private List<Place> orderedPlaces; // Places that are related to the story (UNUSED FOR NOW) 

    // Player 
    PlayerController playerController;
    private Vector2 lastPlayerCoordinates; // (According to the grid)
    private Vector2 playerCoordinates; // (According to the grid)

    // Places
    private readonly List<Place> placeInstances = new();
    private readonly List<Place> placesToDestroy = new();
    private readonly WaitForSeconds checkPlacesInterval = new(1.5f);
    private float placesSpawnCooldown = 0f;

    // Components
    private WaveFunctionCollapse wfc;
    private TileLoader tileLoader;

    private bool hasStarted = false;

    public void Start()
    {
        tileLoader = GetComponent<TileLoader>();
        wfc = this.AddComponent<WaveFunctionCollapse>();
    }

    public void BeginGeneration()
    {
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();

        wfc.Initialize(tileLoader, (int)gridSize.x, (int)gridSize.y, cellScale, cellObj, gridOffset, playerController, startingPlace, edgeSize);

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

        // Get center of grid
        Vector3 gridCenter = playerController.transform.position + new Vector3(gridOffset.x, 0, gridOffset.y);

        // Size of the grid + edge margin to allow wfc to compute entropy
        Vector2 outerGridSize = new(cellScale * (gridSize.x + edgeSize * 2), cellScale * (gridSize.y + edgeSize * 2));
        float halfW = outerGridSize.x / 2f;
        float halfH = outerGridSize.y / 2f;

        // Find random position to spawn place outside the outer area and inside the places margin
        float spawnX, spawnZ;
        int attempts = 0;
        do
        {
            spawnX = Random.Range(gridCenter.x - halfW - placesMargin, gridCenter.x + halfW + placesMargin);
            spawnZ = Random.Range(gridCenter.z - halfH - placesMargin, gridCenter.z + halfH + placesMargin);
            attempts++;
            if (attempts > 10) return;
        }
        while (Mathf.Abs(spawnX - gridCenter.x) < halfW &&
               Mathf.Abs(spawnZ - gridCenter.z) < halfH);

        // Pick random place after a valid location has been found
        Place place = places[Random.Range(0, places.Count)];

        // Check if chosen coordinates overlap with the starting area
        bool collidesWithHomeInstance = wfc.GetHomeInstance().GetComponent<Place>().GetExtents()
            .CollidesWith(spawnX, spawnZ, place.GetPlaceWorldDimensions().x, place.GetPlaceWorldDimensions().y, cellScale * 4);
        if (collidesWithHomeInstance) return;

        // Check for collisions with other already placed areas
        foreach (Place placed in placeInstances)
        {
            if (placed.GetExtents().CollidesWith(spawnX, spawnZ, place.GetPlaceWorldDimensions().x, place.GetPlaceWorldDimensions().y, cellScale * 2))
                return;
        }

        // Placement is valid —> instantiate and register with WFC
        Place newPlace = Instantiate(place, new Vector3(spawnX, 0, spawnZ), Quaternion.identity);
        newPlace.Initialize(new Vector2(spawnX, spawnZ), tileLoader, cellScale);
        placeInstances.Add(newPlace);
        wfc.AddPlaceForPlacement(newPlace);
    }

    /// <summary>
    /// Removes any place that's outside the determined area
    /// </summary>
    void CheckPlaces()
    {
        Vector3 center = playerController.transform.position + new Vector3(gridOffset.x, 0, gridOffset.y);
        Vector2 outerGridSize = new(cellScale * (gridSize.x + (edgeSize + 2) * 2), cellScale * (gridSize.y + (edgeSize + 2) * 2));

        float cullW = outerGridSize.x / 2f + placesMargin + cellScale * placesDeletionMargin;
        float cullH = outerGridSize.y / 2f + placesMargin + cellScale * placesDeletionMargin;

        foreach (Place place in placeInstances)
        {
            Vector3 pos = place.transform.position;

            if (Mathf.Abs(pos.x - center.x) > cullW || Mathf.Abs(pos.z - center.z) > cullH)
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
