using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImportTileInfo : MonoBehaviour
{
    /*Tile[] allTiles;*/

    Tile[] tiles;

    public TextAsset jsonFile;

    private void Start()
    {
        TileData tileData = JsonUtility.FromJson<TileData>(jsonFile.text);

        foreach (TileInfo tileInfo in tileData.tiles) { 
            Tile newTile = new Tile();

        }
    }

}

[System.Serializable]
public class TileData
{
    public TileInfo[] tiles;
}

[System.Serializable]
public class TileInfo
{
    public string name;
    public int rotation;
    public string posX, posZ,negX,negZ;
    public float weight;
}
