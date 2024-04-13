using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public TextAsset tileInfoJSON;

    public GameObject[] tiles;

    public Cell cellObj;

    [SerializeField, Range(0, 4)]
    public float cellScale = 2;

    [SerializeField, Range(1, 100)]
    public int width, height;

    [SerializeField, Range(0.5f, 10)]
    public float mapScale;


    void Start()
    {
        TileLoader tileLoader = this.AddComponent<TileLoader>();
        tileLoader.Initialize(tileInfoJSON, tiles);
        List<Tile> possibleTiles = tileLoader.Load();

        WaveFunctionCollapse wfc = this.AddComponent<WaveFunctionCollapse>();
        wfc.Initialize(possibleTiles, width, height, cellScale, cellObj, mapScale);
    }
}
