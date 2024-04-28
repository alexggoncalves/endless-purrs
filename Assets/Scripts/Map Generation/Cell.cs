using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UIElements;
using Unity.AI.Navigation;

public class Cell : MonoBehaviour
{
    public bool collapsed;
    public int x, y;
    public List<Tile> tileOptions;
    public GameObject tileInstance;
    public bool preset = false;

    public void CreateCell(bool collapseState, List<Tile> tiles, int x, int y)
    {
        collapsed = collapseState;
        tileOptions = tiles;
        this.x = x;
        this.y = y;
        tileInstance = null;
    }

    public void CollapseCell(Tile tile)
    {
        collapsed = true;
        tileOptions = new List<Tile> { tile };
        preset = true;
        /*tileInstance = Instantiate(tile.prefab, transform.position, Quaternion.Euler(0, tile.prefab.transform.rotation.y + 90 * tile.rotation, 0));*/ 

    }

    public void RecreateCell(List<Tile> tiles)
    {
        tileOptions = tiles;
    }

    public GameObject InstantiateTile()
    {
        tileInstance = Instantiate(tileOptions[0].prefab, transform.position, Quaternion.Euler(0, tileOptions[0].prefab.transform.rotation.y + 90 * tileOptions[0].rotation, 0));
        return tileInstance;
    }

    public void ResetCell(List<Tile> tiles)
    {
        collapsed = false;
        tileOptions = tiles;
        /*Destroy(tileInstance);*/
        tileInstance = null;
    }

    public void UpdateCell()
    {
        Destroy(tileInstance);
        tileInstance = Instantiate(tileOptions[0].prefab, transform.position, Quaternion.Euler(0, tileOptions[0].prefab.transform.rotation.y + 90 * tileOptions[0].rotation, 0));
    }

    public int GetX()
    {
        return x;
    }

    public int GetY()
    {
        return y;
    }

    public List<Tile> GetTileOptions()
    {
        return tileOptions;
    }
}


