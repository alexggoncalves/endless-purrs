using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileLoader : MonoBehaviour
{
    
    private GameObject[] tilePrefabs;

    public List<Tile> tiles;

    private TextAsset jsonFile;

    public void Initialize(TextAsset jsonFile, GameObject[] tilePrefabs)
    {
        this.tilePrefabs = tilePrefabs;
        this.jsonFile = jsonFile;
        this.tiles = new List<Tile>(); 
    }

    public List<Tile> Load()
    {
        TileData tileData = JsonUtility.FromJson<TileData>(jsonFile.text);

        // Create tiles based in the imported data.
        foreach (TileInfo tileInfo in tileData.tiles) { 
            foreach(GameObject tile in tilePrefabs) {
                if(tile.name == tileInfo.prefab)
                {
                    GameObject newTile = new GameObject(tileInfo.name);
                    Tile tileComponent = newTile.AddComponent<Tile>();
                    tileComponent.Initialize(tile, tileInfo.name, tileInfo.pX, tileInfo.nX, tileInfo.pY, tileInfo.nY, tileInfo.weight, tileInfo.rotation);
                    tiles.Add(tileComponent);
                }
            }
        }

        // Compare every profile (on the X and Y [Z in unity] axis) of the pieces of the tileset to each other and set up neighbours.
        foreach (Tile tile1 in tiles)
        {
            foreach(Tile tile2 in tiles)
            {
                if (tile1.pX == tile2.nX)
                {
                    if (!tile2.rightNeighbours.Contains(tile1)) tile2.rightNeighbours.Add(tile1);
                }
                if (tile1.nX == tile2.pX)
                {
                    if (!tile2.leftNeighbours.Contains(tile1)) tile2.leftNeighbours.Add(tile1);
                }
                if (tile1.pY == tile2.nY) {
                    if (!tile2.upNeighbours.Contains(tile1)) tile2.upNeighbours.Add(tile1);
                }
                if (tile1.nY == tile2.pY)
                {
                    if (!tile2.downNeighbours.Contains(tile1)) tile2.downNeighbours.Add(tile1);
                }
               
            }
        }
        
        return tiles;
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
    public string prefab;
    public int rotation;
    public string pX, nX,pY,nY;
    public float weight;
}
