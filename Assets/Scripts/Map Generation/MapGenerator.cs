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
    [SerializeField]
    public List<Place> places;
    public GameObject startingPlace;

    public List<Place> placeInstances;

    void Start()
    {
        TileLoader tileLoader = this.AddComponent<TileLoader>();
        tileLoader.Initialize(tileInfoJSON, tiles);
        possibleTiles = tileLoader.Load();
        /*InstantiatePlace(places[0]);*/

        player = GameObject.Find("Player").GetComponent<Walking>();
        player.SetMapDetails(gridWidth, gridHeight, cellScale, worldOffset, edgeSize);        

        wfc = this.AddComponent<WaveFunctionCollapse>();
        wfc.Initialize(possibleTiles, gridWidth, gridHeight, cellScale, cellObj, worldOffset, player, startingPlace) ;

        /*lastPlayerCoordinates = player.GetPlayerGridCoordinates();*/
        lastPlayerCoordinates = wfc.CalculateWorldCoordinates(player.transform.position.x, player.transform.position.z);
        InstantiatePlace(places[0]);
    }

    public void Update()
    {
        HandleGridMove();
        HandlePlacePlacement();
    }

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

    private void HandlePlacePlacement()
    {
        for (int i = 0; i < placeInstances.Count; i++)
        {
            // Add the place to the map generation if the place's area collides with the players area, and the place is not yet on wait for placement
            if (placeInstances[i].GetExtents().CollidesWith(player.GetOutterPlayerArea()) && !placeInstances[i].isPlaced && !placeInstances[i].onWait)
            {
                placeInstances[i].onWait = true;
                wfc.AddPlaceForPlacement(placeInstances[i]);
            }
        }
    }

    private void InstantiatePlace(Place place)
    {
        Place newPlace = Instantiate(place,Vector3.zero, Quaternion.identity);
        newPlace.Initialize(new Vector3(-56,40), possibleTiles[0]);

        placeInstances.Add(newPlace);
    }
}
