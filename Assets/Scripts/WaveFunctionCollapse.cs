using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using DG.Tweening;

public class WaveFunctionCollapse : MonoBehaviour
{
    // List of all the possible tiles
    List<Tile> tiles;

    // Dimensions
    float cellScale;
    int gridWidth, gridHeight;

    // Cell prefab
    private Cell cellObj;

    /*public List<Cell> grid;*/
    public Cell[,] grid;
    private int iteration = 0;

    GameObject cellContainer;
    GameObject tileInstanceContainer;

    private Stack<Cell> updatedCells;
    private Stack<GameObject> instancesToDestroy;

    public bool update;
    private bool updating;

    //Player 
    GameObject player;
    RectangularArea innerPlayerArea;
    RectangularArea outterPlayerArea;
    Vector2 lastPlayerCoordinates; // According to the grid
    Vector2 playerCoordinates;
    Vector2 moveOffset = new Vector2(0,0);
    Vector2 worldOffset = new Vector2(0,0);

    


    public void Update()
    {
        /*if (update)
        {
            if (!updating)
            {
                updating = true;
                ResetGridSection(0, 0, gridWidth / 4, gridHeight / 4);
                *//*ShiftGrid();*//*
            }
            update = false;
        }*/
        

        playerCoordinates = new Vector2(Mathf.RoundToInt((player.transform.position.x/2 + (gridWidth / 2))),
                                        Mathf.RoundToInt((player.transform.position.z/2 + (gridHeight / 2))));

        if (playerCoordinates != lastPlayerCoordinates) {
            moveOffset += (playerCoordinates - lastPlayerCoordinates);

        }
        if((moveOffset.x != 0 || moveOffset.y !=0 ) && !updating)
        {
            updating = true;
            ShiftGrid();
        }

        /*Debug.Log(playerCoordinates);*/
        lastPlayerCoordinates = playerCoordinates;
    }

    public void Initialize(List<Tile> possibleTiles, int width, int height, float cellScale, Cell cellObj, float mapScale)
    {
        this.updating = true;
        tiles = new List<Tile>(possibleTiles);

        this.cellScale = cellScale;
        
        lastPlayerCoordinates.x = 0;
        lastPlayerCoordinates.y = 0;

        this.gridWidth = width;
        this.gridHeight = height;
        this.cellObj = cellObj;
        updatedCells = new Stack<Cell>();
        instancesToDestroy = new Stack<GameObject>();

        player = GameObject.Find("Player");
        innerPlayerArea = player.GetComponent<RectangularArea>();
        outterPlayerArea = player.AddComponent<RectangularArea>();
        outterPlayerArea.Initialize(width*cellScale, height * cellScale,innerPlayerArea.GetOffset());
        lastPlayerCoordinates = new Vector2(Mathf.RoundToInt((player.transform.position.x / 2 + (gridWidth / 2))),
                                        Mathf.RoundToInt((player.transform.position.z / 2 + (gridHeight / 2))));
        worldOffset = innerPlayerArea.GetOffset();

        cellContainer = new GameObject("Grid Container");
        tileInstanceContainer = new GameObject("Tile Instance Container");
        tileInstanceContainer.transform.localScale = transform.localScale * mapScale;


        InitializeGrid();
    }

    public void InitializeGrid()
    {
        grid = new Cell[gridWidth, gridHeight];
        // Add the Cell component for every cell of the grid
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Cell newCell = 
                    Instantiate(cellObj, 
                    new Vector3(worldOffset.x + x * cellScale - ((gridWidth - 1) * cellScale) / 2f,
                    0,
                    worldOffset.y + y * cellScale - ((gridHeight - 1) * cellScale) / 2f),
                    Quaternion.identity);

                newCell.transform.SetParent(cellContainer.transform);

                // Every cell is given all the possible tiles and it's collapsed state is set to false 
                newCell.CreateCell(false, tiles, x, y);
                grid[x,y] = newCell;
            }
        }

        StartCoroutine(CheckEntropy());
    }

    
    // Find the cell(s) with the least tile possibilies and collapse it into one of the superpositions(tiles)
    IEnumerator CheckEntropy()
    {
        List<Cell> tempGrid = new List<Cell>();
        foreach (Cell c in grid)
        {
            if(!c.collapsed && innerPlayerArea.Contains(new Vector2(c.transform.position.x, c.transform.position.z))) 
            tempGrid.Add(c);
        }
        tempGrid.Sort((a, b) => a.GetTileOptions().Count - b.GetTileOptions().Count);
        tempGrid.RemoveAll(a => a.GetTileOptions().Count != tempGrid[0].GetTileOptions().Count);
        

        yield return new WaitForSeconds(0.005f);
        /*yield return null;*/

        CollapseCell(tempGrid);
        
        
    }


    //  Collapses one of the cells with the least number of tile possibilities(superpositions)
    void CollapseCell(List<Cell> tempGrid)
    {
         
        if(tempGrid.Count > 0)
        {
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
            int randomIndex = UnityEngine.Random.Range(0, tempGrid.Count);
            Cell cellToCollapse = tempGrid[randomIndex];

            updatedCells.Push(cellToCollapse);

            cellToCollapse.collapsed = true;

            Tile selectedTile = SelectRandomTile(cellToCollapse);

            if (selectedTile != null)
            {
                cellToCollapse.RecreateCell(new List<Tile> { selectedTile });
            }
            else
            {
                cellToCollapse.RecreateCell(new List<Tile> { tiles[0] });

            }

            cellToCollapse.InstantiateTile().transform.SetParent(tileInstanceContainer.transform);
        }
            
        
            
        
        UpdateGeneration();
    }

    // Selects a random Tile from the possible options based on their weights
    Tile SelectRandomTile(Cell cellToColapse)
    {
        Tile[] options = new Tile[cellToColapse.GetTileOptions().Count];
        for (int i = 0; i < cellToColapse.GetTileOptions().Count; i++)
        {
            options[i] = cellToColapse.GetTileOptions()[i];
        }

        options.OrderBy(tile => tile.weight);

        float totalWeight = 0f;
        foreach (Tile t in options) totalWeight += t.weight;

        float diceRoll = UnityEngine.Random.Range(0, totalWeight);

        float cumulative = 0f;
        for (int i = 0; i < options.Length; i++)
        {
            cumulative += options[i].weight;
            if (diceRoll < cumulative)
            {
                return options[i]; ;
            }
        }
        return null;
    }

    // Destroy all placed tile instances, reset all cell states and restart algorithm
   /* private void ResetGrid()
    {
        foreach (Transform child in tileInstanceContainer.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Cell cell in grid)
        {
            cell.collapsed = false;
            cell.tileOptions = new List<Tile>(tiles);
        }

        iteration = 0;

        StartCoroutine(CheckEntropy());
    }*/

    //  Looks at the 4 surrounding cells and updates the list of possible tiles.
    void UpdateCellsEntropy(Cell cell)
    {
        int x = cell.GetX();
        int y = cell.GetY();

        Cell down = null, up = null, left = null, right = null;

        // Start with considering all possibilities and then remove the tiles that are not valid by checking the 4 surrounding neighbours
        List<Tile> options = new List<Tile>(tiles);

        if (y > 0) // DOWN
        {
            /*Cell down = grid[x + (y - 1) * gridWidth];*/
            down = grid[x,y-1];
            List<Tile> validOptions = new List<Tile>();

            // Loop through the up cell tileOptions and get all their compatible down neighbours
            foreach (Tile possibleTile in down.GetTileOptions())
            {
                validOptions = validOptions.Concat(possibleTile.upNeighbours).ToList();
            }

            CheckValidity(options, validOptions);
        }

        if (x < gridWidth - 1) // RIGHT
        {
            /*Cell right = grid[x + 1 + y * gridWidth];*/
            right = grid[x + 1,y];
            List<Tile> validOptions = new List<Tile>();

            foreach (Tile possibleTile in right.GetTileOptions())
            {
                validOptions = validOptions.Concat(possibleTile.leftNeighbours).ToList();
            }

            CheckValidity(options, validOptions);
        }

        if (y < gridHeight - 1) // UP
        {
            /*Cell up = grid[x + (y + 1) * gridWidth];*/
            up = grid[x, y + 1];
            List<Tile> validOptions = new List<Tile>();

            foreach (Tile possibleTile in up.GetTileOptions())
            {
                validOptions = validOptions.Concat(possibleTile.downNeighbours).ToList();
            }

            CheckValidity(options, validOptions);
        }

        if (x > 0) // LEFT
        {
            /*Cell left = grid[x - 1 + y * gridWidth];*/
            left = grid[x - 1, y];
            List<Tile> validOptions = new List<Tile>();

            foreach (Tile possibleTile in left.GetTileOptions())
            {
                validOptions = validOptions.Concat(possibleTile.rightNeighbours).ToList();
            }

            CheckValidity(options, validOptions);
        }

        if (cell.GetTileOptions().Count != options.Count)
        {
            updatedCells.Push(cell);
        }

        // Update cell's tile options
        cell.RecreateCell(options);
    }


    void UpdateGeneration()
    {
       
        while (updatedCells.Count > 0)
        {
            Cell cell = updatedCells.Pop();
            int x = cell.GetX();
            int y = cell.GetY();

            if (y > 0)
            {
                /*Cell up = grid[x + (y - 1) * gridWidth];*/
                Cell down = grid[x, y - 1];
                UpdateCellsEntropy(down);
            }
            if (x < gridWidth - 1)
            {
                /*Cell left = grid[x + 1 + y * gridWidth];*/
                Cell right = grid[x + 1, y];
                UpdateCellsEntropy(right);
            }
            if (y < gridHeight - 1)
            {
                /*Cell down = grid[x + (y + 1) * gridWidth];*/
                Cell up = grid[x, y+1];
                UpdateCellsEntropy(up);
            }
            if (x > 0)
            {
                /*Cell right = grid[x - 1 + y * gridWidth];*/
                Cell left = grid[x - 1, y];
                UpdateCellsEntropy(left);
            }
        }

        // After collapsing one cell and updating the tiles that need to be updated:
        // start another iteration of the algorithm if there are still cells remaining to be collapsed
        //  else the algorithm enters a rest state until there's a cell that needs to be collapsed

        iteration++;

        /*StartCoroutine(CheckEntropy());*/
        /*//**Debug.Log(iteration);*/
        if (iteration < innerPlayerArea.GetArea() / 2 - gridWidth - gridHeight - 1)
        {
            StartCoroutine(CheckEntropy());
        } else updating = false;
        
    }

    // Go through the options array for the cell being updated and remove any tile that isn't present in the valid options array.
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

    /*void ResetGridSection(int x, int y, int width, int height)
    {
        List<Cell> cellsToReset = new List<Cell>();
        for (int i = x; i < x + width; i++)
        {
            
            for (int j = y; j < y + height; j++)
            {
                Cell cellToReset = grid[j * gridWidth + i];
                cellsToReset.Add(cellToReset);
                *//*cellToReset.ResetCell(tiles);*//*

                if (j == height - 1) updatedCells.Push(cellToReset);

            }
        }

        foreach (Cell cellToReset in cellsToReset)
        {
            cellToReset.ResetCell(tiles);
            *//*updatedCells.Push(cellToReset);*//*
        }

        iteration -= (width * height + 1);
        
        *//*yield return null;*//*

        UpdateGeneration();
    }*/


    void ShiftGrid()
    {
        Vector2 direction = new Vector2();
        if(moveOffset.x == 0) direction.x = 0;
            else direction.x = moveOffset.x/ Mathf.Abs(moveOffset.x);

        if(moveOffset.y == 0) direction.y = 0;
        else direction.y = moveOffset.y / Mathf.Abs(moveOffset.y);
        Debug.Log(direction);
        
        if(direction.x == 1 )
        {
            for(int x = 0; x < gridWidth; x++)
            {
                for(int y = 0; y < gridHeight; y++)
                {
                    if(x < gridWidth - 1)
                    {
                        if (x == 0) Destroy(grid[x, y].tileInstance);
                        grid[x, y].tileInstance = grid[x + 1, y].tileInstance;
                        grid[x, y].tileOptions = grid[x + 1, y].tileOptions;
                        grid[x, y].collapsed = grid[x + 1, y].collapsed;
                    }
                    else
                    {
                        grid[x, y].ResetCell(tiles);
                        updatedCells.Push(grid[x, y]);
                    }
                    
                }
            }
            cellContainer.transform.position += new Vector3(direction.x * cellScale, 0, 0);
            moveOffset.x -= 1;
            iteration -= gridHeight + 1;

        }
        else if(direction.x == -1 )
        {
            for (int x = gridWidth-1; x >= 0; x--)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (x > 0)
                    {
                        if (x == gridWidth - 1) Destroy(grid[x, y].tileInstance);
                        grid[x, y].tileInstance = grid[x - 1, y].tileInstance;
                        grid[x, y].tileOptions = grid[x - 1, y].tileOptions;
                        grid[x, y].collapsed = grid[x - 1, y].collapsed;
                    }
                    else
                    {
                        grid[x, y].ResetCell(tiles);
                        updatedCells.Push(grid[x, y]);
                    }
                    
                }
            }
            cellContainer.transform.position += new Vector3(direction.x * cellScale, 0, 0);
            moveOffset.x += 1;
            iteration -= gridHeight + 1;
        }
        else if (direction.y == 1)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (y <gridHeight - 1)
                    {
                        if (y == 0) Destroy(grid[x, y].tileInstance);
                        grid[x, y].tileInstance = grid[x, y + 1].tileInstance;
                        grid[x, y].tileOptions = grid[x, y + 1].tileOptions;
                        grid[x, y].collapsed = grid[x, y + 1].collapsed;
                    }
                    else
                    {
                        grid[x, y].ResetCell(tiles);

                        updatedCells.Push(grid[x, y]);
                    }
                    
                }
            }
            cellContainer.transform.position += new Vector3(0, 0, direction.y * cellScale);
            moveOffset.y -= 1;
            iteration -= gridWidth + 1;
        }
        else if (direction.y == -1)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = gridHeight - 1; y >= 0; y--)
                {
                    if (y > 0)
                    {
                        if (y == gridHeight - 1) Destroy(grid[x, y].tileInstance);
                        grid[x, y].tileInstance = grid[x, y - 1].tileInstance;
                        grid[x, y].tileOptions = grid[x, y - 1].tileOptions;
                        grid[x, y].collapsed = grid[x, y - 1].collapsed;
                    }
                    else
                    {
                        grid[x, y].ResetCell(tiles);
                        updatedCells.Push(grid[x, y]);
                    }
                    
                }
            }
            cellContainer.transform.position += new Vector3(0, 0, direction.y * cellScale);
            moveOffset.y += 1;
            iteration -= gridWidth + 1;
        }
        UpdateGeneration();
        
    }

    private void OnDestroy()
    {
        Destroy(gameObject);
    }
}