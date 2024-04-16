using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
        GameObject tilesContainer = new GameObject("Imported Tiles");
        // Create tiles based in the imported data.
        foreach (TileInfo tileInfo in tileData.tiles)
        {
            foreach (GameObject tile in tilePrefabs)
            {
                if (tile.name == tileInfo.prefab)
                {
                    GameObject newTile = new GameObject(tileInfo.name);
                    Tile tileComponent = newTile.AddComponent<Tile>();
                    tileComponent.Initialize(tile, tileInfo.name, tileInfo.pX, tileInfo.nX, tileInfo.pY, tileInfo.nY, tileInfo.weight, tileInfo.rotation);
                    tiles.Add(tileComponent);
                    newTile.transform.SetParent(tilesContainer.transform);
                }
            }
        }

        // Compare every profile (on the X and Y [Z in unity] axis) of the pieces of the tileset to each other and set up neighbours.
        foreach (Tile tileA in tiles)
        {
            foreach (Tile tileB in tiles)
            {

                if (isSymmetrical(tileA.pX, tileB.nX) ^ isAsymmetrical(tileA.pX, tileB.nX))
                {
                    if (!tileB.leftNeighbours.Contains(tileA)) tileB.leftNeighbours.Add(tileA);
                }
                if (isSymmetrical(tileA.nX, tileB.pX) ^ isAsymmetrical(tileA.nX, tileB.pX))
                {
                    if (!tileB.rightNeighbours.Contains(tileA)) tileB.rightNeighbours.Add(tileA);
                }
                if (isSymmetrical(tileA.pY, tileB.nY) ^ isAsymmetrical(tileA.pY, tileB.nY))
                {
                    if (!tileB.downNeighbours.Contains(tileA)) tileB.downNeighbours.Add(tileA);
                }
                if (isSymmetrical(tileA.nY, tileB.pY) ^ isAsymmetrical(tileA.nY, tileB.pY))
                {
                    if (!tileB.upNeighbours.Contains(tileA)) tileB.upNeighbours.Add(tileA);
                }

            }
        }

        return tiles;
    }

    bool isSymmetrical(string socketA, string socketB)
    {
        if (socketA.Contains("s") && socketA == socketB)
            return true;
        else
            return false;
    }

    bool isAsymmetrical(string socketA, string socketB)
    {
        return socketA == (socketB + "f") || socketB == (socketA + "f");
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
    public string pX, nX, pY, nY;
    public float weight;
}