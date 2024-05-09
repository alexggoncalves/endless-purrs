using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime;
using static UnityEditor.Rendering.FilterWindow;
using Unity.VisualScripting;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class WaveFunctionCollapse : MonoBehaviour
{
    TileLoader tileLoader;
    // Dimensions
    float cellScale;
    int gridWidth, gridHeight;
    Vector2 worldOffset = new(0, 0);
    Vector2 edgeSize;

    // Cells
    GameObject cellContainer;
    private Cell cellObj;
    public Stack<Cell> updatedCells;

    // Grid
    public Cell[,] grid;
    GameObject tileInstanceContainer;
    Tile[,] initialAreaGrid;

    // Player
    Movement player;
    Vector2 moveOffset = new(0, 0);
    Vector2 totalMoveOffset = new(0, 0);

    // Places
    private List<Place> placesOnWait;
    private Stack<Place> placesToDestroy;
    public Stack<GameObject> instancesToDelete;
    

    // Other
    private int iteration = 0;
    bool initialLoading = true;
    private bool paused;
    NatureElementPlacer natureElements;

    private void Update()
    {
        if(instancesToDelete.Count > 0)
        {
            GameObject instance = instancesToDelete.Pop();
            Destroy(instance);
        }

        if (placesToDestroy.Count > 0)
        {
            Place place = placesToDestroy.Pop();
            placesOnWait.Remove(place);
            Destroy(place.gameObject);
        }
    }

    public void Initialize(TileLoader tileLoader, int width, int height, float cellScale, Cell cellObj, Vector2 worldOffset, Movement player, GameObject startingPlace, Vector2 edgeSize)
    { 
        this.player = player;
        this.tileLoader = tileLoader;
        this.cellScale = cellScale;
        this.gridWidth = width;
        this.gridHeight = height;
        this.worldOffset = worldOffset;

        this.cellObj = cellObj;
        updatedCells = new Stack<Cell>();
        cellContainer = new GameObject("Grid Container");
        tileInstanceContainer = new GameObject("Tile Instance Container");

        instancesToDelete = new Stack<GameObject>();
        placesToDestroy = new Stack<Place>();

        this.placesOnWait = new List<Place>();
        
        this.edgeSize = edgeSize;
        this.paused = false;

        natureElements = GameObject.Find("Nature Elements").GetComponent<NatureElementPlacer>();

        InitializeGrid();
        PlaceStartingArea(startingPlace, 0, 6);
        StartCoroutine(CheckEntropy());
    }

    public void InitializeGrid()
    {
        grid = new Cell[gridWidth, gridHeight];
        initialAreaGrid = new Tile[gridWidth, gridHeight];

        // Add the Cell component for every cell of the grid
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                float cellX = worldOffset.x + (x * cellScale) - (gridWidth * cellScale) / 2 + cellScale / 2;
                float cellY = worldOffset.y + (y * cellScale) - (gridHeight * cellScale) / 2 + cellScale / 2;

                Cell newCell = Instantiate(cellObj, new Vector3(cellX, 0, cellY), Quaternion.identity);

                newCell.transform.SetParent(cellContainer.transform);

                // Every cell is given all the possible tiles and it's collapsed state is set to false 
                newCell.CreateCell(false, x, y,tileLoader);
                grid[x, y] = newCell;
            }
        }
    }

    

    void PlaceStartingArea(GameObject startingPlace, int x, int y)
    {
        // Create initial grass area
        GameObject startingAreaInstance = Instantiate(startingPlace, new Vector3(x, 0, y), Quaternion.identity);
        Place place = startingAreaInstance.GetComponent<Place>();
        place.Initialize(new Vector2(x, y), tileLoader.grassID);
        placesOnWait.Add(place);

        Vector2 dimensions = place.GetDimensions();
        Vector2 position = CalculateGridCoordinates(place.GetPosition().x, place.GetPosition().y);

        SetGridSection(place.GetGrid(), position.x - dimensions.x / 2 + 1, position.y - dimensions.y / 2 + cellScale + 1, dimensions.x, dimensions.y);
    }

    /*void SaveStartingArea()
    {
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                initialAreaGrid[i, j] = grid[i, j].tileOptions[0];
            }
        }
    }*/

    /*public void MoveToOrigin()
    {
        cellContainer.transform.position = new Vector3(0, 0, 0);
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                grid[i, j].collapsed = false;
                
                grid[i, j].RecreateCell(new List<Tile> { initialAreaGrid[i, j] });
            }
        }

        iteration = 0;

        *//*RefreshInstances();*//*
        moveOffset = new Vector2(0, 0);
        totalMoveOffset = new Vector2(0, 0);
    }*/


    // Find the cell(s) with the least tile possibilies and collapse it into one of the superpositions(tiles)
    IEnumerator CheckEntropy()
    {
        List<Cell> tempGrid = new();
        foreach (Cell c in grid)
        {
            if (!c.collapsed && player.GetInnerPlayerArea().Contains(new Vector2(c.transform.position.x, c.transform.position.z)))
                tempGrid.Add(c);
        }
        tempGrid.Sort((a, b) => a.GetTileOptions().Count - b.GetTileOptions().Count);
        tempGrid.RemoveAll(a => a.GetTileOptions().Count != tempGrid[0].GetTileOptions().Count);


        yield return new WaitForSeconds(0.01f);

        CollapseCell(tempGrid);
    }

    //  Collapses one of the cells with the least number of tile possibilities(superpositions)
    void CollapseCell(List<Cell> tempGrid)
    {
        if (tempGrid.Count > 0)
        {
            // Select 1 (or 1 of the cells) with the least possible tiles
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
            int randomIndex = UnityEngine.Random.Range(0, tempGrid.Count);
            Cell cellToCollapse = tempGrid[randomIndex];

            // As the cell is being altered: add the cell to the updated cells stack to later update surrounding cells
            updatedCells.Push(cellToCollapse);

            // Set the cell as collapsed
            cellToCollapse.collapsed = true;

            // Choose onde of the possibilities
            int selectedTile = SelectRandomTile(cellToCollapse);

            // If there is a compatible tile, place the first element of the tiles list (generally the grass tile)
            if (selectedTile != -1)
            {
                cellToCollapse.RecreateCell(new List<int> { selectedTile });
                if (!CellIsInsidePlace(cellToCollapse))
                {
                    if (tileLoader.GetNameById(selectedTile) == "grass")
                    {
                        GameObject natureElement = natureElements.PlaceElement(cellToCollapse.transform.position, NatureElementPlacer.BiomeType.Forest);
                        cellToCollapse.SetNatureElementInstance(natureElement);
                    }
                    else if (tileLoader.GetNameById(selectedTile) == "grass_L1")
                    {
                        GameObject natureElement = natureElements.PlaceElement(cellToCollapse.transform.position + Vector3.up, NatureElementPlacer.BiomeType.Forest);
                        cellToCollapse.SetNatureElementInstance(natureElement);
                    }
                    else if (tileLoader.GetNameById(selectedTile) == "grass_L2")
                    {
                        GameObject natureElement = natureElements.PlaceElement(cellToCollapse.transform.position + Vector3.up, NatureElementPlacer.BiomeType.Forest);
                        cellToCollapse.SetNatureElementInstance(natureElement);
                    }
                }
            }
            else
            {
                cellToCollapse.RecreateCell(new List<int> { tileLoader.grassID });
            }

            // Instantiate the chosen tile and set it as a child of the instance container
            GameObject instance = cellToCollapse.InstantiateTile();
            instance.layer = LayerMask.NameToLayer("Tiles");

            instance.transform.SetParent(tileInstanceContainer.transform);
        }

        // Move on to propagate changes and restart temporarily pause cycle
        StartCoroutine(UpdateGeneration());
    }

    Boolean CellIsInsidePlace(Cell cell)
    {
        foreach (Place place in placesOnWait)
        {
            if (place.extents.Contains(new Vector2(cell.transform.position.x, cell.transform.position.z)))
            {
                return true;
            }
        }
        return false;
    }

    int SelectRandomTile(Cell cellToCollapse)
    {
        List<int> options = cellToCollapse.GetTileOptions();
        
        // Calculate total weight
        float totalWeight = 0f;
        foreach (int id in options) totalWeight += tileLoader.GetTileByID(id).weight;

        float diceRoll = UnityEngine.Random.Range(0, totalWeight);
        

        float cumulative = 0f;
        for (int i = 0; i < options.Count; i++)
        {
            cumulative += tileLoader.GetTileByID(options[i]).weight;
            if (diceRoll < cumulative)
            {
                return options[i];
            }
        }
        return -1;
    }

    //  Looks at the 4 surrounding cells and updates the list of possible tiles.
    void UpdateCellsEntropy(Cell cell, bool propagate)
    {
        int x = cell.GetX();
        int y = cell.GetY();

        // Start with considering all possibilities and then remove the tiles that are not valid by checking the 4 surrounding neighbours
        List<int> options = tileLoader.GetPossibleTileIDs().ToList();

        if (!cell.collapsed)
        {
            if (y > 0) // DOWN
            {
                List<int> down = grid[x, y - 1].GetTileOptions();
                List<int> validOptions = new();
                // Loop through the up cell tileOptions and get all their compatible down neighbours
                for (int i = 0; i < down.Count; i++)
                {
                    Tile tile = tileLoader.GetTileByID(down[i]);
                    validOptions = validOptions.Concat(tile.upNeighbours).ToList();
                    
                }
                CheckValidity(options, validOptions);
            }

            if (x < gridWidth - 1) // RIGHT
            {
                List<int> right = grid[x + 1, y].GetTileOptions();
                List<int> validOptions = new();

                for (int i = 0; i < right.Count; i++)
                {
                    Tile tile = tileLoader.GetTileByID(right[i]);
                    validOptions = validOptions.Concat(tile.leftNeighbours).ToList();
                }

                CheckValidity(options, validOptions);
            }

            if (y < gridHeight - 1) // UP
            {
                List<int> up = grid[x, y + 1].GetTileOptions();
                List<int> validOptions = new();

                for (int i = 0; i < up.Count; i++)
                {
                    Tile tile = tileLoader.GetTileByID(up[i]);
                    validOptions = validOptions.Concat(tile.downNeighbours).ToList();
                }

                CheckValidity(options, validOptions);
            }

            if (x > 0) // LEFT
            {
                List<int> left = grid[x - 1, y].GetTileOptions();
                List<int> validOptions = new();

                for (int i = 0; i < left.Count; i++)
                {
                    Tile tile = tileLoader.GetTileByID(left[i]);
                    validOptions = validOptions.Concat(tile.rightNeighbours).ToList();
                }

                CheckValidity(options, validOptions);
            }

            if (cell.GetTileOptions().Count != options.Count)
            {
                // Update cell's tile options
                cell.RecreateCell(options);
                if (propagate) { updatedCells.Push(cell); }
            }
        }
    }


    
    void SetGridSection(int[,] newTiles, float x, float y, float width, float height)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(x + i < width && y + j < height)
                {
                    Cell cell = grid[(int)x + i, (int)y + j];
                    cell.RecreateCell(new List<int> { newTiles[i, j] });
                    updatedCells.Push(cell);

                }
            }
        }

    }

    IEnumerator UpdateGeneration()
    {   // After collapsing one cell and updating the tiles that need to be updated:
        // start another iteration of the algorithm if there are still cells remaining to be collapsed
        // else the algorithm enters a rest state until there's a cell that needs to be collapsed
        while (updatedCells.Count > 0)
        {
            Cell cell = updatedCells.Pop();
            int x = cell.GetX();
            int y = cell.GetY();

            foreach (Place place in placesOnWait)
            {
                if (place.extents.Contains(new Vector2(cell.transform.position.x, cell.transform.position.z)))
                {
                    cell.RecreateCell(new List<int> { tileLoader.grassID });
                }
            }

            if (y > 0)
            {
                Cell down = grid[x, y - 1];
                UpdateCellsEntropy(down, true);

            }
            if (x < gridWidth - 1)
            {
                Cell right = grid[x + 1, y];
                UpdateCellsEntropy(right, true);

            }
            if (y < gridHeight - 1)
            {
                Cell up = grid[x, y + 1];
                UpdateCellsEntropy(up, true);

            }
            if (x > 0)
            {
                Cell left = grid[x - 1, y];
                UpdateCellsEntropy(left, true);

            }
            if(updatedCells.Count % 6 == 0)
            {
                yield return new WaitForSeconds(0.01f);
            }
            
        }

        
        


        iteration++;

        if (iteration < player.GetInnerPlayerArea().GetCellArea(cellScale))
        {
            paused = false;
            StartCoroutine(CheckEntropy());
        }
        else
        {
            if (initialLoading)
            {
                initialLoading = false;
            }
            paused = true;
        }
    }

    // Go through the options array for the cell being updated and remove any tile that isn't present in the valid options array.
    // The valid options array will bring the tiles that each of the directions allow the focused tile to be.
    void CheckValidity(List<int> options, List<int> validOption)
    {
        for (int i = options.Count - 1; i >= 0; i--)
        {
            int element = options[i];

            if (!validOption.Contains(element))
            {
                options.RemoveAt(i);
            }
        }
    }

    public void ShiftGrid()
    {
        Vector2 direction = new();
        if (moveOffset.x == 0) direction.x = 0;
        else direction.x = moveOffset.x / Mathf.Abs(moveOffset.x);

        if (moveOffset.y == 0) direction.y = 0;
        else direction.y = moveOffset.y / Mathf.Abs(moveOffset.y);

        MoveAndOffsetGeneration(direction);

        paused = false;
        StartCoroutine(UpdateGeneration());
        GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    void SwapCellState(Cell copyTo, Cell copyFrom)
    {
        copyTo.natureElement = copyFrom.natureElement;
        copyTo.tileInstance = copyFrom.tileInstance;
        copyTo.tileOptions = copyFrom.tileOptions;
        copyTo.collapsed = copyFrom.collapsed;
    }


    void MoveAndOffsetGeneration(Vector2 direction)
    {
        if (direction.x == 1)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (x < gridWidth - 1)
                    {
                        if (x == 0)
                        {
                            if (grid[x, y].natureElement != null) instancesToDelete.Push(grid[x, y].natureElement);
                            instancesToDelete.Push(grid[x, y].tileInstance);
                        }
                        SwapCellState(grid[x, y], grid[x + 1, y]);
                    }
                    else
                    {
                        grid[x, y].ResetCell();
                        updatedCells.Push(grid[x, y]);
                    }
                }
            }
            cellContainer.transform.position += new Vector3(direction.x * cellScale, 0, 0);
            moveOffset.x -= 1;
            iteration -= (gridHeight - (int)edgeSize.y * 2 + 1);
            totalMoveOffset.x += 1;
        }
        else if (direction.x == -1)
        {
            for (int x = gridWidth - 1; x >= 0; x--)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (x > 0)
                    {
                        if (x == gridWidth - 1)
                        {
                            if (grid[x, y].natureElement != null) instancesToDelete.Push(grid[x, y].natureElement);
                            instancesToDelete.Push(grid[x, y].tileInstance);
                        }
                        SwapCellState(grid[x, y], grid[x - 1, y]);
                    }
                    else
                    {
                        grid[x, y].ResetCell();
                        updatedCells.Push(grid[x, y]);
                    }
                }
            }
            cellContainer.transform.position += new Vector3(direction.x * cellScale, 0, 0);
            moveOffset.x += 1;
            iteration -= (gridHeight - (int)edgeSize.y * 2 + 1);
            totalMoveOffset.x -= 1;
        }
        else if (direction.y == 1)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (y < gridHeight - 1)
                    {
                        if (y == 0)
                        {
                            if (grid[x, y].natureElement != null) instancesToDelete.Push(grid[x, y].natureElement);
                            instancesToDelete.Push(grid[x, y].tileInstance);
                        }

                        SwapCellState(grid[x, y], grid[x, y + 1]);
                        grid[x, y].collapsed = grid[x, y + 1].collapsed;
                    }
                    else
                    {
                        grid[x, y].ResetCell();
                        updatedCells.Push(grid[x, y]);
                    }
                }
            }
            cellContainer.transform.position += new Vector3(0, 0, direction.y * cellScale);
            moveOffset.y -= 1;
            iteration -= (gridWidth - (int)edgeSize.x * 2 + 1);
            totalMoveOffset.y += 1;

        }
        else if (direction.y == -1)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = gridHeight - 1; y >= 0; y--)
                {
                    if (y > 0)
                    {
                        if (y == gridHeight - 1)
                        {
                            if (grid[x, y].natureElement != null) instancesToDelete.Push(grid[x, y].natureElement);
                            instancesToDelete.Push(grid[x, y].tileInstance);
                        }
                        SwapCellState(grid[x, y], grid[x, y - 1]);
                    }
                    else
                    {
                        grid[x, y].ResetCell();
                        updatedCells.Push(grid[x, y]);
                    }
                }
            }

            cellContainer.transform.position += new Vector3(0, 0, direction.y * cellScale);
            moveOffset.y += 1;
            iteration -= (gridWidth - (int)edgeSize.x * 2 + 1); ;
            totalMoveOffset.y -= 1;
        }
    }



    public void AddPlaceForPlacement(Place place)
    {
        placesOnWait.Add(place);
    }

    public void AddPlaceToDestroy(Place place)
    {
        placesToDestroy.Push(place);
    }

    public Vector2 CalculateGridCoordinates(float x, float y)
    {
        x -= cellScale / 2;
        y -= cellScale / 2;

        // Calculate grid coordinates
        int gridX = Mathf.RoundToInt((x + (gridWidth * cellScale) / 2) / cellScale);
        int gridY = Mathf.RoundToInt((y + (gridHeight * cellScale) / 2) / cellScale);

        // Adjust for world offset
        gridX -= Mathf.RoundToInt(worldOffset.x / cellScale);
        gridY -= Mathf.RoundToInt(worldOffset.y / cellScale);

        // Adjust for move offset
        gridX -= Mathf.RoundToInt(totalMoveOffset.x);
        gridY -= Mathf.RoundToInt(totalMoveOffset.y);


        return new Vector2(gridX, gridY);
    }

    public Vector2 CalculateWorldCoordinates(float x, float y)
    {
        if (gridWidth % 2 == 0) { x -= cellScale / 2; }

        if (gridHeight % 2 == 0) { y -= cellScale / 2; }

        int gridX = Mathf.RoundToInt((x) / cellScale);
        int gridY = Mathf.RoundToInt((y) / cellScale);
        return new Vector2(gridX, gridY);
    }

    public bool IsPaused() { return paused; }

    public void AddToMoveOffset(Vector2 offset)
    {
        moveOffset += offset;
    }

    public Vector2 GetMoveOffset() { return moveOffset; }

    public Boolean HasLoadedInitialZone()
    {
        return !initialLoading;
    }

    public int GetIteration()
    {
        return iteration;
    }

    private void OnDestroy()
    {
        Destroy(gameObject);
    }
}