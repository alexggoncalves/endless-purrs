using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

public class Place : MonoBehaviour
{
    [SerializeField] private string placeName;
    [SerializeField] private int[,] grid;
    [SerializeField, Range(1,40)] int width = 2, height = 2;
    [SerializeField] string tileName;

    private float cellScale;
    private TileBitmask placeTileBitmask;

    private RectangularArea extents;
   
    public void Initialize(Vector2 position, TileLoader tileLoader, float cellScale)
    {
        this.cellScale = cellScale;
        transform.position = new Vector3(position.x, 0, position.y);

        Tile resolved = tileLoader.GetTileByName(tileName);
        if (resolved == null)
        {
            Debug.LogError($"Place '{placeName}': no tile named '{tileName}' found in TileLoader.");
            return;
        }

        int tileID = resolved.GetID();
        placeTileBitmask.Clear();
        placeTileBitmask.Set(tileID);

        FillWith(tileID);
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

    public Vector2 GetPlaceGridDimensions() { return new Vector2(width, height); }

    public Vector2 GetPlaceWorldDimensions() { return new Vector2(width * cellScale, height * cellScale); }

    public Vector2 GetPosition() { return transform.position; }

    public int[,] GetGrid() { return grid; }

    public RectangularArea GetExtents() { return extents; }

    public TileBitmask GetTileBitmask() { return placeTileBitmask; } 
}