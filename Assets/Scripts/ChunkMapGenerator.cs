using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class ChunkMapGenerator : MonoBehaviour
{
    public TextAsset tileInfoJSON;

    public GameObject[] tiles;

    public Cell cellObj;
    public Chunk chunkObj;

    [SerializeField, Range(0, 4)]
    public float cellScale = 2;

    [SerializeField, Range(1, 100)]
    public int width, height;

    void Start()
    {
        TileLoader tileLoader = this.AddComponent<TileLoader>();
        tileLoader.Initialize(tileInfoJSON, tiles);
        List<Tile> possibleTiles = tileLoader.Load();



        CreateChunk(possibleTiles);

        

        /*WaveFunctionCollapse wfc = this.AddComponent<WaveFunctionCollapse>();
        wfc.Initialize(possibleTiles, width, height, cellScale, cellObj);*/
    }

    void CreateChunk(List<Tile> possibleTiles)
    {
        GameObject chunkContainer = new GameObject("Chunk1");
        List<Tile> topRow = new List<Tile>() { possibleTiles[0], possibleTiles[2], possibleTiles[0] };
        List<Tile> rightRow = new List<Tile>() { possibleTiles[0], possibleTiles[0], possibleTiles[0] }; 
        List<Tile> bottomRow = new List<Tile>() { possibleTiles[0], possibleTiles[2], possibleTiles[0] }; 
        List<Tile> LeftRow = new List<Tile>() { possibleTiles[0], possibleTiles[0], possibleTiles[0] }; 
        chunkContainer.AddComponent<Chunk>().Initialize(topRow, rightRow, bottomRow, LeftRow, possibleTiles, cellObj, cellScale);

    }
}
