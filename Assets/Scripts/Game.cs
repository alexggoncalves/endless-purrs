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

    /*[SerializeField, Min(0.1f)]     //for 3d
    public Vector3 cellScale = new Vector3(2, 1, 2);*/

    [SerializeField, Range(0,4)]
    public float cellScale = 2;

    [SerializeField, Range(1, 100)]
    public int width, height;


    void Start()
    {
        TileLoader tileLoader = this.AddComponent<TileLoader>();
        tileLoader.Initialize(tileInfoJSON, tiles);
        List<Tile> possibleTiles = tileLoader.Load();

        WaveFunctionCollapse wfc = this.AddComponent<WaveFunctionCollapse>();
        wfc.Initialize(possibleTiles, width, height, cellScale, cellObj);
    }

}
