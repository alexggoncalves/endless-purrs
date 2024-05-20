using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
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
    List<Tile> possibleTiles;

    public Cell cellObj;

    // Terrain generation algorythm
    private WaveFunctionCollapse wfc;

    // Player 
    Movement player;
    Vector2 lastPlayerCoordinates; // (According to the grid)

    // Places
    [SerializeField, Min(1)]
    int placeDensity = 6;
    [SerializeField]
    Vector2 placesExtents = new Vector2(200, 200);

    public GameObject startingPlace;
    public List<Place> orderedPlaces; // Places that are related to the story and 
    public List<Place> unorderedPlaces;
    
    private List<Place> placeInstances;
    private Stack<Place> placesToDestroy;

    public Game game;

    // loading screen
    public GameObject loadingScreen;
    public Slider loadingSlider;

    public TileLoader tileLoader;

    public NavMeshSurface navMesh;

    void Start()
    {
        
        loadingScreen.SetActive(true);

        placeInstances = new List<Place>();

        player = GameObject.Find("Player").GetComponent<Movement>();

        placesToDestroy = new Stack<Place>();

        wfc = this.AddComponent<WaveFunctionCollapse>();
        wfc.Initialize(tileLoader, gridWidth, gridHeight, cellScale, cellObj, worldOffset, player, startingPlace, edgeSize);

        lastPlayerCoordinates = wfc.CalculateWorldCoordinates(player.transform.position.x, player.transform.position.z);
    }

    public void LateUpdate()
    {
        HandleGridMove();
        CheckPlaces();

        if (orderedPlaces.Count > 0 || unorderedPlaces.Count > 0)
        {
            SpawnPlaces();
        }

        if (wfc.HasLoadedInitialZone() && !game.HasBegun())
        {
            game.Begin();
            
        };        

        //Update Loading screen
        if (!wfc.HasLoadedInitialZone())
        {
            float progress = map(wfc.GetIteration(), 0, wfc.GetInnerArea().GetCellArea(cellScale), 0, 1);
            loadingSlider.value = progress;
        }
        else if (loadingScreen.activeSelf)
        {
            loadingScreen.SetActive(false);
            player.UnlockMovement();
        }

        if (placesToDestroy.Count > 0)
        {
            Place place = placesToDestroy.Pop();
            wfc.AddPlaceToDestroy(place);
            place.toDelete = true;
            placeInstances.Remove(place);
        }

    }

    // Detects every time the player moves one cell size and shifts the grid in that direction
    public void HandleGridMove()
    {
        if (!player.IsTeleporting())
        {
            Vector2 playerCoordinates = wfc.CalculateWorldCoordinates(player.transform.position.x, player.transform.position.z);

            if (playerCoordinates != lastPlayerCoordinates)
            {
                Vector2 moveAmount = (playerCoordinates - lastPlayerCoordinates);
                wfc.AddToMoveOffset(moveAmount);
            }

            if ((wfc.GetMoveOffset().x != 0 || wfc.GetMoveOffset().y != 0) && wfc.IsPaused() && !wfc.IsUpdatingCells())
            {
                wfc.ShiftGrid();

            }

            lastPlayerCoordinates = playerCoordinates;
        }
    }

    void SpawnPlaces()
    {
        bool valid = false;
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);

        if (placeInstances.Count < placeDensity)
        {
            Place place = unorderedPlaces[UnityEngine.Random.Range(0, unorderedPlaces.Count)];
            Vector3 center = player.transform.position;

            int gridX = UnityEngine.Random.Range((int)(center.x - placesExtents.x), (int)(center.x + placesExtents.x));
            int gridY = UnityEngine.Random.Range((int)(center.z - placesExtents.y), (int)(center.z + placesExtents.y));

            float x = gridX * cellScale;
            float y = gridY * cellScale;
            
            // Check if chosen coordinates are inside player area
            bool collidesWithHomeInstance = wfc.GetHomeInstance().GetComponent<Place>().GetExtents().CollidesWith(x, y, place.GetDimensions().x * cellScale, place.GetDimensions().y * cellScale, cellScale * 4);
            bool collidesWithOuterPlayerArea = wfc.GetOuterArea().CollidesWith(x, y, place.GetDimensions().x * cellScale, place.GetDimensions().y * cellScale, cellScale * 6);

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

    // Removes any place that's outside the determined area
    void CheckPlaces()
    {
        foreach (Place place in placeInstances)
        {
            if (Vector3.Distance(player.transform.position,place.transform.position) > placesExtents.x * cellScale + 20)
            {
                placesToDestroy.Push(place);
            }
        }
    }

    public WaveFunctionCollapse GetWFC()
    {
        return wfc;
    }

    float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }


}
