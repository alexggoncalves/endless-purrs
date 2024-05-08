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
    public List<int> tileOptions;
    public GameObject tileInstance;
    public bool preset = false;

    TileLoader tileLoader;

    public GameObject natureElement;

    public void CreateCell(bool collapseState, int x, int y, TileLoader tileLoader)
    {
        this.x = x;
        this.y = y;

        collapsed = collapseState;

        this.tileLoader = tileLoader;
        tileOptions = tileLoader.GetPossibleTileIDs();
        
        tileInstance = null;
        natureElement = null;
    }

    public void CollapseCell(int tileID)
    {
        collapsed = true;
        tileOptions = new List<int> { tileID };
        preset = true;
        /*tileInstance = Instantiate(tile.prefab, transform.position, Quaternion.Euler(0, tile.prefab.transform.rotation.y + 90 * tile.rotation, 0));*/ 

    }

    public void RecreateCell(List<int> tileIDs)
    {
        tileOptions =  tileIDs;
    }

    public GameObject InstantiateTile()
    {
        Tile tile = tileLoader.GetTileByID(tileOptions[0]);
        GameObject selectedOption = tile.SelectOption();
        tileInstance = Instantiate(selectedOption, transform.position, Quaternion.Euler(0, selectedOption.transform.rotation.y + 90 * tile.rotation, 0));
        return tileInstance;
    }

    public void ResetCell()
    {
        collapsed = false;
        tileOptions = tileLoader.GetPossibleTileIDs();
        tileInstance = null;
    }

    public int GetX()
    {
        return x;
    }

    public int GetY()
    {
        return y;
    }

    public List<int> GetTileOptions()
    {
        return tileOptions;
    }

    public void SetNatureElementInstance(GameObject natureElement)
    {
        this.natureElement = natureElement;
    }
}


