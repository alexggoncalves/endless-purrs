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

    GameObject cellContainer;
    GameObject tileInstanceContainer;

    private Stack<Cell> updatedCells;

    public void Initialize(List<Tile> possibleTiles, int width, int height, float cellScale, Cell cellObj, GameObject backupTile)
    {
        tiles = possibleTiles.ToArray();

        this.cellScale = cellScale;
        this.width = width;
        this.height = height;
        this.cellObj = cellObj;

        GameObject errorTile = new GameObject("error");
        this.backupTile = errorTile.AddComponent<Tile>();
        this.backupTile.Initialize(backupTile, "error", "-1", "-1", "-1", "-1", 0, 0);

        cellContainer = new GameObject("Grid Container");
        tileInstanceContainer = new GameObject("Tile Instance Container");

        updatedCells = new Stack<Cell>();

        InitializeGrid();
    }

    public void InitializeGrid() 
    {
        grid = new List<Cell>();
        // Add the Cell component for every cell of the grid
        for (int y = 0; y < height; y++) { 
            for(int x = 0; x < width; x++) {
                Cell newCell = Instantiate(cellObj, new Vector3(-(cellScale*width)/2 + x*cellScale, 0, -(cellScale * height) / 2 + y *cellScale), Quaternion.identity);
                newCell.transform.SetParent(cellContainer.transform);
                // Every cell is given all the possible tiles and it's collapsed state is set to false 
                newCell.CreateCell(false, tiles,x,y);
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
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        int randomIndex = UnityEngine.Random.Range(0, tempGrid.Count);
        Cell cellToCollapse = tempGrid[randomIndex];
        updatedCells.Push(cellToCollapse);
        cellToCollapse.collapsed = true;

        Tile selectedTile = SelectRandomTile(cellToCollapse);
        if (selectedTile != null)
        {
            cellToCollapse.tileOptions = new Tile[] { selectedTile };
        } else
        {
            cellToCollapse.tileOptions = new Tile[] { backupTile };
            
        }

        Tile foundTile = cellToCollapse.tileOptions[0];

        GameObject tileInstance = foundTile.Instantiate(cellToCollapse.transform.position);
        tileInstance.transform.SetParent(tileInstanceContainer.transform);

        

        if (selectedTile == null) { ResetGrid(); }
         else UpdateGeneration();
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

    // Destroy all placed tile instances, reset all cell states and restart algorithm
    private void ResetGrid()
    {
        foreach (Transform child in tileInstanceContainer.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Cell cell in grid)
        {
            cell.collapsed = false;
            cell.tileOptions = tiles;
        }

        iteration = 0;
        
        StartCoroutine(CheckEntropy());
    }

    //  Looks at the 4 surrounding cells and updates the list of possible tiles.
    void UpdateCellsEntropy(Cell cell)
    {
        int x = cell.GetX();
        int y = cell.GetY();

        // Start with considering all possibilities and then remove the tiles that are not valid by checking the 4 surrounding neighbours
        List<Tile> options = new List<Tile>(tiles);

        if (y > 0) // UP
        {
            Cell up = grid[x + (y - 1) * width];
            List<Tile> validOptions = new List<Tile>();

            // Loop through the up cell tileOptions and get all their compatible down neighbours
            foreach (Tile possibleTile in up.tileOptions)
            {
                validOptions = validOptions.Concat(possibleTile.downNeighbours).ToList();
            }

            CheckValidity(options, validOptions);
        }

        if (x < width - 1) // LEFT
        {
            Cell left = grid[x + 1 + y * width];
            List<Tile> validOptions = new List<Tile>();

            foreach (Tile possibleTile in left.tileOptions)
            {
                validOptions = validOptions.Concat(possibleTile.rightNeighbours).ToList();
            }

            CheckValidity(options, validOptions);
        }

        if (y < height - 1) // DOWN
        {
            Cell down = grid[x + (y + 1) * width];
            List<Tile> validOptions = new List<Tile>();

            foreach (Tile possibleTile in down.tileOptions)
            {
                validOptions = validOptions.Concat(possibleTile.upNeighbours).ToList();
            }

            CheckValidity(options, validOptions);
        }

        if (x > 0) // RIGHT
        {
            Cell right = grid[x - 1 + y * width];
            List<Tile> validOptions = new List<Tile>();

            foreach (Tile possibleTile in right.tileOptions)
            {
                validOptions = validOptions.Concat(possibleTile.leftNeighbours).ToList();
            }

            CheckValidity(options, validOptions);
        }

        if (cell.tileOptions.Length != options.Count)
        {
            updatedCells.Push(cell);
        }

        // Update cell's tile options
        cell.RecreateCell(options.ToArray());
    }



    void UpdateGeneration()
    {
        while(updatedCells.Count > 0)
        {
            Debug.Log(grid.Count);
            Cell cell = updatedCells.Pop();
            int x = cell.GetX();
            int y = cell.GetY();

            if (y > 0)
            {
                Cell up = grid[x + (y - 1) * width];
                UpdateCellsEntropy(up);
            }
            if (x < width - 1)
            {
                Cell left = grid[x + 1 + y * width];
                UpdateCellsEntropy(left);
            }
            if (y < height - 1)
            {
                Cell down = grid[x + (y + 1) * width];
                UpdateCellsEntropy(down);
            }
            if (x > 0)
            {
                Cell right = grid[x - 1 + y * width];
                UpdateCellsEntropy(right);
            }
        }

        // After collapsing one cell and updating the tiles that need to be updated:
        // start another iteration of the algorithm if there are still cells remaining to be collapsed
        iteration++;
        if (iteration < width * height)
        {
            StartCoroutine(CheckEntropy());
        }
    }
    
    // Go through the options array for the cell being updated and remove any tile that isn't present in the valid options array
    // The valid options array will bring the tiles that each of the directions allow the focused tile to be.
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

    private void OnDestroy()
    {
        Destroy(gameObject);
    }
}