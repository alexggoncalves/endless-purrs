using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Place : MonoBehaviour
{
    [SerializeField]
    string placeName;

    [SerializeField]
    public int[,] grid;

    [SerializeField, Range(1,40)]
    int width = 2, height = 2;
    float cellScale;

    [SerializeField]
    string tile;

    public RectangularArea extents;

    public bool onWait = false;
    public bool toDelete = false;
    

    public void Initialize(Vector2 position, int tile, float cellScale)
    {
        this.cellScale = cellScale;
        transform.position = new Vector3(position.x, 0, position.y);

        FillWith(tile);
        extents = this.AddComponent<RectangularArea>();
        extents.Initialize(width * cellScale, height * cellScale, Vector2.zero, UnityEngine.Color.yellow);
    }

    public void FillWith(int tile)
    {
        grid = new int[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                grid[i, j] = tile;
            }
        }
    }

    public Vector2 GetDimensions() { return new Vector2(width, height); }

    public Vector2 GetPosition() { return transform.position; }

    public int[,] GetGrid() { return grid; }

    public RectangularArea GetExtents() { return extents; }

}