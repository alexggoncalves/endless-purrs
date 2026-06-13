using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    [Range(0, 4)] public float cellScale = 2;
    [Range(1, 100)] public int gridWidth, gridHeight;
    [SerializeField] Vector2 worldOffset;
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
        wfc.Initialize(tileLoader, gridWidth, gridHeight, cellScale, cellObj, worldOffset, playerController, startingPlace, edgeSize, natureElementRate);

        lastPlayerCoordinates = wfc.CalculateWorldCoordinates(playerController.transform.position.x, playerController.transform.position.z);

        StartCoroutine(CheckPlacesRoutine());

        hasStarted = true;
    }

    public void Update()
    {
        if (!hasStarted) return;

        HandleGridMove();

        if (orderedPlaces.Count > 0 || unorderedPlaces.Count > 0)
        {
            SpawnPlaces();
        }

        // Remove places assigned to be destroyed
        foreach (Place place in placesToDestroy)
        {
            wfc.AddPlaceToDestroy(place);
            place.toDelete = true;
            placeInstances.Remove(place);
        }
        placesToDestroy.Clear();

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

    void SpawnPlaces()
    {
        // Cooldown timer for place check
        placesSpawnCooldown -= Time.deltaTime;
        if (placesSpawnCooldown > 0f) return;
        placesSpawnCooldown = 0.5f;

        bool valid = false;
        if (placeInstances.Count < placeDensity)
        {
            Place place = unorderedPlaces[UnityEngine.Random.Range(0, unorderedPlaces.Count)];
            Vector3 center = playerController.transform.position;

            float x = UnityEngine.Random.Range((center.x - placesExtents.x), (center.x + placesExtents.x));
            float y = UnityEngine.Random.Range((center.z - placesExtents.y), (center.z + placesExtents.y));

            // Check if chosen coordinates are inside player area
            bool collidesWithHomeInstance = wfc.GetHomeInstance().GetComponent<Place>().GetExtents().CollidesWith(x, y, place.GetDimensions().x * cellScale, place.GetDimensions().y * cellScale, cellScale * 4);
            bool collidesWithOuterPlayerArea = wfc.GetOuterArea().CollidesWith(x, y, place.GetDimensions().x * cellScale, place.GetDimensions().y * cellScale, cellScale * 10);

            if (!collidesWithHomeInstance && !collidesWithOuterPlayerArea)
            {
                valid = true;

                // Check for collisions with other placed areas
                foreach (Place placed in placeInstances)
                {
                    if (placed.GetExtents().CollidesWith(x, y, place.GetDimensions().x * cellScale, place.GetDimensions().y * cellScale, cellScale * 2))
                    {
                        valid = false;
                        break;
                    }
                }
            }

            // If the placement is valid, create an instance of the place
            // and send it to the wave function collapse to affect the terrain generation
            if (valid)
            {
                Place newPlace = Instantiate(place, new Vector3(x, 0, y), Quaternion.identity);
                newPlace.Initialize(new Vector2(x, y), tileLoader.grassID, cellScale);

                placeInstances.Add(newPlace);
                newPlace.onWait = true;
                wfc.AddPlaceForPlacement(newPlace);
            }
        }
    }

    /// <summary>
    /// Removes any place that's outside the determined area
    /// </summary>
    void CheckPlaces()
    {
        List<Place> toRemove = new();

        foreach (Place place in placeInstances)
            if (Vector3.Distance(playerController.transform.position, place.transform.position) > placesExtents.x + 20)
                toRemove.Add(place);

        placesToDestroy.AddRange(toRemove);
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
