using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;


public enum TileType
{
    grass,
    grass_L1,
    grass_L2,
    cliff,
    cliff_L1,
    sand,
    water
}

[System.Serializable]
public class TileOption
{
    public GameObject tilePrefab;
    public float weight;
}

[System.Serializable]
public class TileConstraints
{
    [SerializeField, Range(0, 3)]
    public int rotation;
    public string pX, nX, pY, nY;
}

[System.Serializable]
public class TileInfo
{
    public string name;
    public Boolean enabled = true;
    public TileType type;
    public TileOption[] tileOptions;
    public TileConstraints[] constraints;
    public float weight;
}

public class TileLoader : MonoBehaviour
{
    public List<Tile> tiles = new List<Tile>();
    [SerializeField]
    public TileInfo[] tileInfo;

    private void Start()
    {
        // Create all the tiles with the information given on the editor
        foreach (TileInfo tile in tileInfo)
        {
            if(tile.enabled)
            {
                foreach (TileConstraints constraints in tile.constraints)
                {
                    string name = tile.name.ToString();
                    GameObject newTile = new GameObject(name);
                    Tile tileComponent = newTile.AddComponent<Tile>();
                    tileComponent.Initialize(tile.tileOptions, name, constraints.pX, constraints.nX, constraints.pY, constraints.nY, tile.weight, constraints.rotation);
                    tiles.Add(tileComponent);
                    newTile.transform.SetParent(transform);
                }
            }
            
        }

        /*// Order tiles by weight
        tiles.Sort((Tile tileA, Tile tileB) =>
        {
            return tileA.weight.CompareTo(tileB.weight);
        });*/

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
    }

    public List<Tile> GetTiles()
    {
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

