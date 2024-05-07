using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class Tile : MonoBehaviour
{
    public string pX, nX, pY, nY;
    public int rotation;

    public float weight;

    public List<Tile> upNeighbours;
    public List<Tile> downNeighbours;
    public List<Tile> leftNeighbours;
    public List<Tile> rightNeighbours;

    public TileOption[] options;

    public Tile Initialize (TileOption[] options, string name, string pX, string nX, string pY, string nY, float weight,int rotation)
    {
        this.name = name;
        this.pX = pX;
        this.pY = pY;
        this.nX = nX;
        this.nY = nY;
        this.rotation = rotation;
        this.weight = weight;
        this.options = options;


        upNeighbours = new List<Tile>();
        downNeighbours = new List<Tile>();
        leftNeighbours = new List<Tile>();
        rightNeighbours = new List<Tile>();

        return this;
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

        return Instantiate(prefab, position, Quaternion.Euler(0, prefab.transform.rotation.y + 90 * rotation, 0));
    }
}