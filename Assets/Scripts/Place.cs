using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Place : MonoBehaviour
{
    [SerializeField]
    string placeName;

    [SerializeField]
    public Tile[,] grid;

    public bool[,] cellPlacements;

    [SerializeField, Range(2,10)]
    int width = 2, height = 2;
    [SerializeField, Range(2, 10)]
    int cellScale = 2;
    public Vector2 position;

    private RectangularArea extents;

    public GameObject instance;

    public bool isPlaced = false;
    public bool onWait = false;

    public void Initialize(Vector2 position, Tile tile)
    {
        this.position = position;
        transform.position = new Vector3(position.x, 0, position.y);
        extents = this.AddComponent<RectangularArea>();
        extents.Initialize(width * cellScale, height * cellScale, Vector2.zero, UnityEngine.Color.yellow);
        FillWith(tile);

        cellPlacements = new bool[width, height];
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                cellPlacements[i, j] = false;
            }
        }
    }

    public void FillWith(Tile tile)
    {
        grid = new Tile[width, height];
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++)
            {
                grid[i, j] = tile;
            }
        }
    }

    public Vector2 GetDimensions() { return new Vector2(width, height); }

    public Vector2 GetPosition() { return position; }

    public Tile[,] GetGrid() { return grid; }

    public RectangularArea GetExtents() { return extents; }

    public void FillWithCustom(Tile[,] tiles, Tile backup)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (tiles[i, j] != null)
                {
                    grid[i, j] = tiles[i, j];
                } else
                {
                    grid[i, j] = backup;
                }
                
            }
        }
    }

    public Place InstantiatePlace()
    {
        return Instantiate(this, position, Quaternion.identity);
    }
    
    /*public void SetCellPlacement(int x, int y, bool placed)
    {
        cellPlacements[x, y] = placed;

        int placedAmount = 0;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
               if(cellPlacements[i, j]) { placedAmount++; }
            }
        }
        if(placedAmount >= width * height) isPlaced = true;
    }*/
}
