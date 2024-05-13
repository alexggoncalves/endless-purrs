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
public struct TileOption
{
    public GameObject tilePrefab;
    public float weight;
}

[System.Serializable]
public struct TileConstraints
{
    [SerializeField, Range(0, 3)]
    public int rotation;
    public string pX, nX, pY, nY;
}

[System.Serializable]
public class TileInfo
{ 
    public string name;
    public bool enabled = true;
    public TileType type;
    public TileOption[] tileOptions;
    public TileConstraints[] constraints;
    public float weight;
}

public class TileLoader : MonoBehaviour
{
    
    public TileInfo[] tileInfo;
    public List<Tile> tiles = new List<Tile>();
    public int grassID;
    private List<int> possibleTileIDs = new();

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
                    /*GameObject newTile = new(name);*/
                    Tile tileComponent = new Tile(); /*newTile.AddComponent<Tile>();*/
                    tileComponent.Initialize(tile.tileOptions, name, constraints.pX, constraints.nX, constraints.pY, constraints.nY, tile.weight, constraints.rotation);
                    tiles.Add(tileComponent);
                    /*newTile.transform.SetParent(transform);*/
                }
            }
        }

        // Order tiles by weight
        tiles.Sort((tileA, tileB) => tileB.weight.CompareTo(tileA.weight));

        int counter = 0;
        // Compare every profile (on the X and Y [Z in unity] axis) of the pieces of the tileset to each other and set up neighbours.
        foreach (Tile tileA in tiles)
        {
            tileA.SetID(counter);

            foreach (Tile tileB in tiles)
            { 
                if (IsSymmetrical(tileA.pX, tileB.nX) ^ IsAsymmetrical(tileA.pX, tileB.nX))
                {
                    if (!tileB.leftNeighbours.Contains(tileA.GetID())) tileB.leftNeighbours.Add(tileA.GetID());
                }
                if (IsSymmetrical(tileA.nX, tileB.pX) ^ IsAsymmetrical(tileA.nX, tileB.pX))
                {
                    if (!tileB.rightNeighbours.Contains(tileA.GetID())) tileB.rightNeighbours.Add(tileA.GetID());
                }
                if (IsSymmetrical(tileA.pY, tileB.nY) ^ IsAsymmetrical(tileA.pY, tileB.nY))
                {
                    if (!tileB.downNeighbours.Contains(tileA.GetID())) tileB.downNeighbours.Add(tileA.GetID());
                }
                if (IsSymmetrical(tileA.nY, tileB.pY) ^ IsAsymmetrical(tileA.nY, tileB.pY))
                {
                    if (!tileB.upNeighbours.Contains(tileA.GetID())) tileB.upNeighbours.Add(tileA.GetID());
                }

            }

            if(tileA.name == "grass") grassID = tileA.GetID();
            possibleTileIDs.Add(tileA.GetID());
            counter++;
        }
    }

    bool IsSymmetrical(string socketA, string socketB)
    {
        if ((socketA.Contains("s") && socketA == socketB) || (socketA == "-1" || socketB == "-1"))
            return true;
        else
            return false;
    }

    bool IsAsymmetrical(string socketA, string socketB)
    {
        return socketA == (socketB + "f") || socketB == (socketA + "f");
    }

    public Tile GetTileByID(int id)
    {
        return tiles[id];
    }

    public List<Tile> GetAllTiles()
    {
        return tiles;
    }

    public Tile GetTileByName(string name)
    {
        Tile tile = tiles.Where(obj => obj.name == name).SingleOrDefault();
        return tile;
    }

    public string GetNameById(int id)
    {
        return tiles[id].name;
    }

    public List<int> GetPossibleTileIDs()
    { 
       return possibleTileIDs;
    }

  
} 

