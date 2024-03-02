using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class Game : MonoBehaviour
{
    public TextAsset tileInfoJSON;
    public GameObject[] tiles;
    public Cell cellObj;
    public GameObject backup;

    [SerializeField, Min(0.1f)]
    public float cellScale = 2;
    [SerializeField, Range(2, 20)]
    public int width, height;


    void Start()
    {
        TileLoader tileLoader = this.AddComponent<TileLoader>();
        tileLoader.Initialize(tileInfoJSON, tiles);

        List<Tile> possibleTiles = tileLoader.Load();

        WaveFunctionCollapse wfc = this.AddComponent<WaveFunctionCollapse>();
        wfc.Initialize(possibleTiles, width, height, cellScale, cellObj,backup);
    }

}
