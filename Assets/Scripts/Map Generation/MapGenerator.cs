using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    [SerializeField, Range(0, 4)]
    public float cellScale = 2;
    [SerializeField, Range(1, 100)]
    public int gridWidth, gridHeight;
    [SerializeField]
    Vector2 worldOffset;
    [SerializeField]
    Vector2 edgeSize = new Vector2(4, 4);

    // Tiles
    public TextAsset tileInfoJSON;
    public GameObject[] tiles;
    List<Tile> possibleTiles;

    public Cell cellObj;

    // Terrain generation algorythm
    private WaveFunctionCollapse wfc;

    // Player 
    Movement player;
    Vector2 lastPlayerCoordinates; // (According to the grid)

    // Places
    [SerializeField, Min(1)]
    int placeDensity = 3;
    [SerializeField]
    Vector2 placesExtents = new Vector2(100, 100);

    public GameObject startingPlace;
    public List<Place> orderedPlaces; // Places that are related to the story and 
    public List<Place> unorderedPlaces;
    
    public List<Place> placeInstances;
    private Stack<Place> placesToDestroy;

    public Game game;

    // loading screen
    public GameObject loadingScreen;
    public Slider loadingSlider;


    void Start()
    {
        loadingScreen.SetActive(true);

        Time.timeScale = 1f;
        TileLoader tileLoader = this.AddComponent<TileLoader>();
        tileLoader.Initialize(tileInfoJSON, tiles);
        possibleTiles = tileLoader.Load();

        player = GameObject.Find("Player").GetComponent<Movement>();
        player.SetMapDetails(gridWidth, gridHeight, cellScale, worldOffset, edgeSize);

        placesToDestroy = new Stack<Place>();

        wfc = this.AddComponent<WaveFunctionCollapse>();
        wfc.Initialize(possibleTiles, gridWidth, gridHeight, cellScale, cellObj, worldOffset, player, startingPlace);

        lastPlayerCoordinates = wfc.CalculateWorldCoordinates(player.transform.position.x, player.transform.position.z);
    }

    public void Update()
    {
        HandleGridMove();

        if (orderedPlaces.Count > 0 || unorderedPlaces.Count > 0)
        {
            SpawnPlaces();
        }

        CheckPlaces();

        if (wfc.HasLoadedInitialZone() && !game.HasBegun()) game.Begin();

        //Update Loading screen
        if (!wfc.HasLoadedInitialZone())
        {
            float progress = map(wfc.GetIteration(), 0, player.GetInnerPlayerArea().GetCellArea(2), 0, 1);
            loadingSlider.value = progress;
        }
        else if (loadingScreen.activeSelf)
        {
            loadingScreen.SetActive(false);
            player.UnlockMovement();
        }
        
    }

    // Detects every time the player moves one cell size and shifts the grid in that direction
    public void HandleGridMove()
    {
        Vector2 playerCoordinates = wfc.CalculateWorldCoordinates(player.transform.position.x, player.transform.position.z);

        if (playerCoordinates != lastPlayerCoordinates)
        {
            Vector2 moveAmout = playerCoordinates - lastPlayerCoordinates;
            wfc.AddToMoveOffset(moveAmout);
        }

        if ((wfc.GetMoveOffset().x != 0 || wfc.GetMoveOffset().y != 0) && !wfc.IsUpdating())
        {
            wfc.ShiftGrid();
        }

        lastPlayerCoordinates = playerCoordinates;

    }

    // Spawns the places randomly inside the determined extents
    // Mantains the chosen density
    void SpawnPlaces()
    {
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        float attempts = 0;
        float maxAttempts = 50;

        while (placeInstances.Count < placeDensity && attempts < maxAttempts)
        {
            bool valid = false;

            while (!valid && attempts < maxAttempts)
            {
                Place place = unorderedPlaces[UnityEngine.Random.Range(0, unorderedPlaces.Count)];

                Vector3 center = player.transform.position;

                // Calculate random coordinates within extents
                float x = UnityEngine.Random.Range(center.x - placesExtents.x / 2, center.x + placesExtents.x / 2);
                float y = UnityEngine.Random.Range(center.z - placesExtents.y / 2, center.z + placesExtents.y / 2);

                // Check if chosen coordinates are inside player area
                Vector2 placement = new Vector2(x, y);
                if(Vector2.Distance(placement, new Vector2(0,0)) > 20)
                {
                    if (Vector2.Distance(placement, new Vector2(center.x, center.z)) > (gridWidth / 2) * cellScale + 15 + place.GetDimensions().x / 2)
                    {
                        valid = true;

                        // Check for collisions with other placed areas
                        foreach (Place placed in placeInstances)
                        {
                            // Consider the dimensions of the places for overlap check
                            float distanceThreshold = place.GetDimensions().x * cellScale + placed.GetDimensions().x * cellScale;
                            if (Vector2.Distance(placement, new Vector2(placed.transform.position.x, placed.transform.position.z)) < distanceThreshold)
                            {
                                valid = false;
                                break;
                            }
                        }
                    }
                }

                // If the placement is valid, create an instance of the place
                // and send it to the wave function collapse to affect the terrain generation
                if (valid)
                {
                    Place newPlace = Instantiate(place, new Vector3(x, 0, y), Quaternion.identity);
                    newPlace.Initialize(new Vector3(x, y), possibleTiles[0]);

                    placeInstances.Add(newPlace);
                    newPlace.onWait = true;
                    wfc.AddPlaceForPlacement(newPlace);
                }

                attempts++;
            }
        }
    }

    // Removes any place that's outside the determined area
    void CheckPlaces()
    {
        foreach (Place place in placeInstances)
        {
            if (Vector3.Distance(player.transform.position,place.transform.position) > placesExtents.x/2 + 20)
            {
                placesToDestroy.Push(place);
            }
        }

        while (placesToDestroy.Count > 0)
        {
            Place place = placesToDestroy.Pop();
            wfc.AddPlaceToDestroy(place);
            place.toDelete = true;
            placeInstances.Remove(place);
        }
    }

    float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }
}
