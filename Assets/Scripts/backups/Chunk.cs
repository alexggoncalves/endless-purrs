using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Chunk : MonoBehaviour 
{
    int width, height;

    List<Tile> tiles;

    Cell cellObj;
    float cellScale;

    public List<Cell> grid;

    public void Initialize(List<Tile> topRow, List<Tile> rightRow, List<Tile> bottomRow, List<Tile> leftRow, List<Tile> tiles, Cell cellObj, float cellScale)
    {
        this.width = topRow.Count + 2;
        this.height = leftRow.Count + 2;
        this.cellObj = cellObj;
        this.cellScale = cellScale;
        this.tiles = tiles;
        grid = new List<Cell>((width+2) * (height+2));

        InitializeChunk(topRow, rightRow, bottomRow, leftRow, tiles);
        GenerateChunk();
    }

    void InitializeChunk(List<Tile> topRow, List<Tile> rightRow, List<Tile> bottomRow, List<Tile> leftRow, List<Tile> tiles)
    {
        for(int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell newCell = Instantiate(cellObj, new Vector3(-(cellScale * width) / 2 + x * cellScale, 0, -(cellScale * height) / 2 + y * cellScale), Quaternion.identity);
                newCell.CreateCell(false, tiles, x, y);
                newCell.transform.SetParent(transform);
                grid.Add(newCell);
            }
        }

        //Set top row
        for (int x = 1; x < width - 1; x++)
        {
            grid[x].CollapseCell(topRow[x - 1]);
        }

        //Set right column
        for (int y = 1; y < height - 1; y++)
        {
            int index = y * width + (width - 1);
            grid[index].CollapseCell(rightRow[y - 1]);
        }

        //Set left column
        for (int y = 1; y < height - 1; y++)
        {
            int index = width * y;
            grid[index].CollapseCell(leftRow[y - 1]);
        }

        //Set bottomRow
        for (int x = 1; x < width - 1;x++)
        {
            int index = width * (height - 1) + x;
            grid[index].CollapseCell(bottomRow[x - 1]);
        }


    }
    
    void GenerateChunk()
    {
        transform.AddComponent<ChunkWaveFunctionCollapse>().Initialize(tiles, width,height,cellScale,grid);
    }


}
