using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;

public class WaveFunctionCollapse : MonoBehaviour
{
    // List of all the possible tiles
    Tile[] tiles;
    Tile backupTile;

    // Dimensions
    float cellScale;
    int width, height;

    // Cell prefab
    private Cell cellObj;

    public List<Cell> grid;
    private int iteration = 0;

    public void Initialize(List<Tile> possibleTiles, int width, int height, float cellScale, Cell cellObj)
    {
        tiles = possibleTiles.ToArray();

        this.cellScale = cellScale;
        this.width = width;
        this.height = height;
        this.cellObj = cellObj;
        this.backupTile = tiles[0];

        grid = new List<Cell>();

        InitializeGrid();
    }

    public void InitializeGrid() 
    { 
       
        // Add the Cell component for every cell of the grid
        for (int y = 0; y < height; y++) { 
            for(int x = 0; x < width; x++) {
                Cell newCell = Instantiate(cellObj, new Vector3(x*cellScale, 0, y*cellScale), Quaternion.identity);
                // Every cell is given all the possible tiles and it's collapsed state is set to false 
                newCell.CreateCell(false, tiles);
                grid.Add(newCell);
            }
        }

        StartCoroutine(CheckEntropy());
    }


    // Find the cell(s) with the least tile possibilies and collapse it into one of the superpositions(tiles)
    IEnumerator CheckEntropy()
    {
        List<Cell> tempGrid = new List<Cell>(grid);
        tempGrid.RemoveAll((c) => c.collapsed);
        tempGrid.Sort((a,b) => a.tileOptions.Length - b.tileOptions.Length);
        tempGrid.RemoveAll(a => a.tileOptions.Length != tempGrid[0].tileOptions.Length);

        yield return new WaitForSeconds(0);

        CollapseCell(tempGrid);
    }


    //  Collapses one of the cells with the least number of tile possibilities(superpositions)
    void CollapseCell(List<Cell> tempGrid)
    {
        int randomIndex = UnityEngine.Random.Range(0, tempGrid.Count);

        Cell cellToCollapse = tempGrid[randomIndex];
        cellToCollapse.collapsed = true;

        Tile selectedTile = SelectRandomTile(cellToCollapse);

        cellToCollapse.tileOptions = new Tile[] { selectedTile };

        Tile foundTile = cellToCollapse.tileOptions[0];
        if (foundTile == null) { cellToCollapse.tileOptions[0] = backupTile; }

        foundTile.Instantiate(cellToCollapse.transform.position);

        UpdateGeneration();
    }

    // Selects a random Tile from the possible options based on their weights
    Tile SelectRandomTile (Cell cellToColapse)
    {
        Tile[] options = new Tile[cellToColapse.tileOptions.Length] ;
        for (int i = 0;i< cellToColapse.tileOptions.Length; i++)
        {
            options[i] = cellToColapse.tileOptions[i]; 
        }

        options.OrderBy(tile => tile.weight);

        float totalWeight = 0f;
        foreach(Tile t in options) totalWeight += t.weight;
        
        float diceRoll = UnityEngine.Random.Range(0, totalWeight);
        
       
        float cumulative = 0f;
        for(int i = 0; i< options.Length; i++)
        {
            cumulative += options[i].weight;
            if(diceRoll < cumulative)
            {
                return options[i]; ;
            }
        }
        return null;
    }
    

    //  Iterate through every cell and update it's entropy.
    //  Looks at the 4 surrounding cells and updates the list of possible tiles.
    void UpdateGeneration()
    {
        List<Cell> newGenerationGrid = new List<Cell>(grid);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;

                if (grid[index].collapsed)
                {
                    newGenerationGrid[index] = grid[index];
                }
                else
                {
                    List<Tile> options = new List<Tile>();
                    foreach (Tile t in tiles)
                    {
                        options.Add(t);
                    }

                    if (y > 0)
                    {
                        Cell up = grid[x + (y - 1) * width];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in up.tileOptions)
                        {
                            var validOption = Array.FindIndex(tiles, obj => obj == possibleOptions);
                            var valid = tiles[validOption].downNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    if (x < width - 1)
                    {
                        Cell left = grid[x + 1 + y * width];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in left.tileOptions)
                        {
                            var validOption = Array.FindIndex(tiles, obj => obj == possibleOptions);
                            var valid = tiles[validOption].rightNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    if (y < height - 1)
                    {
                        Cell down = grid[x + (y + 1) * width];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in down.tileOptions)
                        {
                            var validOption = Array.FindIndex(tiles, obj => obj == possibleOptions);
                            var valid = tiles[validOption].upNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    if (x > 0)
                    {
                        Cell right = grid[x - 1 + y * width];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in right.tileOptions)
                        {
                            var validOption = Array.FindIndex(tiles, obj => obj == possibleOptions);
                            var valid = tiles[validOption].leftNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    Tile[] newTileList = new Tile[options.Count];

                    for (int i = 0; i < options.Count; i++)
                    {
                        newTileList[i] = options[i];
                    }

                    newGenerationGrid[index].RecreateCell(newTileList);

                }
            }
        }

        grid = newGenerationGrid;
        iteration++;

        if (iteration < width * height)
        {
            StartCoroutine(CheckEntropy());
        }

    }

    void CheckValidity(List<Tile> options, List<Tile> validOption)
    {
        for (int i = options.Count - 1; i >= 0; i--)
        {
            var element = options[i];
            if (!validOption.Contains(element))
            {
                options.RemoveAt(i);
            }
        }
    }
}