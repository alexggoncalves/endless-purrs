using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tile
{
    public string name;
    public  int id;
    public string pX, nX, pY, nY;
    public int rotation;

    public float weight;

    public List<int> upNeighbours;
    public List<int> downNeighbours;
    public List<int> leftNeighbours;
    public List<int> rightNeighbours;

    public TileOption[] options;
    public TileType tileType;

    public Tile Initialize (TileOption[] options, string name, string pX, string nX, string pY, string nY, float weight,int rotation, TileType type)
    {
        this.name = name;
        this.pX = pX;
        this.pY = pY;
        this.nX = nX;
        this.nY = nY;
        this.rotation = rotation;
        this.weight = weight;
        this.options = options;
        this.tileType = type;


        upNeighbours = new List<int>();
        downNeighbours = new List<int>();
        leftNeighbours = new List<int>();
        rightNeighbours = new List<int>();

        return this;
    }

    public void SetID(int id)
    {
        this.id = id;
    }

    public int GetID()
    {
        return this.id;
    }

    public GameObject SelectOption()
    {
        // Choose from weighted list !!
        GameObject prefab = options[0].tilePrefab;
        return prefab;
    }

    public GameObject Instantiate(Vector3 position)
    {
        // Choose from weighted list !!
        GameObject prefab = options[0].tilePrefab;

        return GameObject.Instantiate(prefab, position, Quaternion.Euler(0, prefab.transform.rotation.y + 90 * rotation, 0));
    }
}