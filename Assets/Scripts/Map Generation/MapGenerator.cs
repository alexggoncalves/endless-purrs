using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

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
    Walking player;
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


    void Start()
    {
        Time.timeScale = 1f;
        TileLoader tileLoader = this.AddComponent<TileLoader>();
        tileLoader.Initialize(tileInfoJSON, tiles);
        possibleTiles = tileLoader.Load();

        player = GameObject.Find("Player").GetComponent<Walking>();
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
        int maxAttempts = 100;
        int attempts = 0;
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        while (placeInstances.Count < placeDensity && attempts < maxAttempts)
        {
            bool valid = false;
            while (!valid && attempts < maxAttempts)
            {
                //Choose random place
                Place place = unorderedPlaces[0];

                Vector3 center = player.transform.position;

                int x = UnityEngine.Random.Range((int)(center.x - placesExtents.x/2), (int)(center.x + placesExtents.x/2));
                int y = UnityEngine.Random.Range((int)(center.z - placesExtents.y/2), (int)(center.z + placesExtents.y/2));

                Vector2 placement = new Vector2(x,y);
                
                // Check if chosen coordinates are inside player area or if they collide with any other placed area
                if (!player.GetInnerPlayerArea().Contains(new Vector2(x, y)))
                {
                    valid = true;
                    
                    for (int i = 0; i < placeInstances.Count; i++)
                    {
                        Vector2 placePosition = new Vector2(placeInstances[i].transform.position.x, placeInstances[i].transform.position.y);

                        if (Vector2.Distance(placement, placePosition) < place.GetDimensions().x + placeInstances[i].GetDimensions().x + 10)
                        {
                            valid = false;
                            break;
                        }
                    }
                }

                // If the placement is valid create an instance of the place
                // and send it to the wave function collapse to affect the terrain generation
                if (valid)
                {
                    Place newPlace = Instantiate(orderedPlaces[0], Vector3.zero, Quaternion.identity);
                    newPlace.Initialize(new Vector3(x, y), possibleTiles[0]);

                    placeInstances.Add(newPlace);
                    newPlace.onWait = true;
                    wfc.AddPlaceForPlacement(newPlace);
                } else
                {
                    attempts++;
                }
            }
        }
    }

    // Removes any place that's outside the determined area
    void CheckPlaces()
    {
        foreach (Place place in placeInstances)
        {
            if (Vector3.Distance(player.transform.position,place.transform.position) > placesExtents.x)
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
}
