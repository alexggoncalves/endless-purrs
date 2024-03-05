using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Tile : MonoBehaviour
{
    public string pX, nX, pY, nY;
    public int rotation;

    public float weight;

    public List<Tile> upNeighbours;
    public List<Tile> downNeighbours;
    public List<Tile> leftNeighbours;
    public List<Tile> rightNeighbours;

    public GameObject prefab;

    public Tile Initialize (GameObject prefab, string name, string pX, string nX, string pY, string nY, float weight,int rotation)
    {
        this.name = name;
        this.pX = pX;
        this.pY = pY;
        this.nX = nX;
        this.nY = nY;
        this.rotation = rotation;
        this.weight = weight;
        this.prefab = prefab;
        

        upNeighbours = new List<Tile>();
        downNeighbours = new List<Tile>();
        leftNeighbours = new List<Tile>();
        rightNeighbours = new List<Tile>();

        /*Instantiate(prefab, Vector3.zero, Quaternion.Euler(0, prefab.transform.rotation.y + 90 * rotation, 0));*/
        return this;
    }

    public GameObject Instantiate(Vector3 position)
    {
        return Instantiate(prefab, position, Quaternion.Euler(0, prefab.transform.rotation.y + 90 * rotation, 0));
    }

    public void SetAllNeighbours(Tile tile) {
        upNeighbours.Add(tile);
        downNeighbours.Add(tile);
        leftNeighbours.Add(tile);
        rightNeighbours.Add(tile);
    }
}
