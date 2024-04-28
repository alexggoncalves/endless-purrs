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

    [SerializeField, Range(2,40)]
    int width = 2, height = 2;
    [SerializeField, Range(2, 40)]
    int cellScale = 2;

    [SerializeField]
    string tile;

    public Vector2 position;

    public RectangularArea extents;

    public bool onWait = false;
    public bool toDelete = false;
    

    public void Initialize(Vector2 position, Tile tile)
    {
        this.position = position;
        transform.position = new Vector3(position.x, 0, position.y);

        FillWith(tile);

        SetExtents();
    }

    public void SetExtents()
    {
        extents = this.AddComponent<RectangularArea>();
        extents.Initialize(width * cellScale, height * cellScale, Vector2.zero, UnityEngine.Color.yellow);
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

}
