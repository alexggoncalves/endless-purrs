using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField, Range(0, 4)]
    public float cellScale = 2;
    [SerializeField, Range(1, 100)]
    public int gridWidth, gridHeight;
    [SerializeField, Range(0.5f, 10)]
    Vector2 worldOffset = new Vector2(0, 0);

    // Tiles
    public TextAsset tileInfoJSON;
    public GameObject[] tiles;

    public Cell cellObj;

    // Terrain generation algorythm
    private WaveFunctionCollapse wfc;

    //Player 
    /*GameObject player;*/
    Walking player;
    Vector2 lastPlayerCoordinates; // According to the grid


    void Start()
    {
        TileLoader tileLoader = this.AddComponent<TileLoader>();
        tileLoader.Initialize(tileInfoJSON, tiles);
        List<Tile> possibleTiles = tileLoader.Load();

        player = GameObject.Find("Player").GetComponent<Walking>();
        player.SetMapDetails(gridWidth, gridHeight, cellScale);
        worldOffset = player.GetInnerPlayerArea().GetOffset();
        
        wfc = this.AddComponent<WaveFunctionCollapse>();
        wfc.Initialize(possibleTiles, gridWidth, gridHeight, cellScale, cellObj, worldOffset,player) ;

        lastPlayerCoordinates = player.GetPlayerGridCoordinates();
    }

    public void Update()
    {
        Vector2 playerCoordinates = player.GetPlayerGridCoordinates();

        if (playerCoordinates != lastPlayerCoordinates)
        {
            wfc.AddToMoveOffset(playerCoordinates - lastPlayerCoordinates);
        }

        if ((wfc.GetMoveOffset().x != 0 || wfc.GetMoveOffset().y != 0) && !wfc.IsUpdating())
        {
            wfc.ShiftGrid();
        }

        lastPlayerCoordinates = playerCoordinates;

    }
}
