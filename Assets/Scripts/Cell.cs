using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Cell : MonoBehaviour
{
    public bool collapsed;
    private int x, y;
    public Tile[] tileOptions;

    public void CreateCell(bool collapseState, Tile[] tiles, int x, int y)
    {
        collapsed = collapseState;
        tileOptions = tiles;
        this.x = x;
        this.y = y; 
    }

    public void RecreateCell(Tile[] tiles)
    {
        tileOptions = tiles;
    }

    public void ResetCell(Tile[] tiles)
    {
        collapsed = false;
        tileOptions = tiles;
    }

    public int GetX()
    {
        return x;
    }

    public int GetY()
    {
        return y;
    }
}


