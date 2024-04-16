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
    Vector2 worldOffset = new Vector2(0, 0);

    // Cells
    GameObject cellContainer;
    private Cell cellObj;

    // Grid
    public Cell[,] grid;
    GameObject tileInstanceContainer;

    public Stack<Cell> updatedCells;
    private bool updating;
    private int iteration = 0;

    // Player
    Walking player;
    Vector2 moveOffset = new Vector2(0, 0);
    Vector2 totalMoveOffset = new Vector2(0, 0);

    // Places
    List<Place> placesOnWait;
    
    public void Initialize(List<Tile> possibleTiles, int width, int height, float cellScale, Cell cellObj, Vector2 worldOffset, Walking player, GameObject startingPlace)
    {
        tiles = new List<Tile>(possibleTiles);
        this.player = player;

        this.cellScale = cellScale;
        this.gridWidth = width;
        this.gridHeight = height;
        this.worldOffset = worldOffset;

        this.cellObj = cellObj;
        updatedCells = new Stack<Cell>();
        cellContainer = new GameObject("Grid Container");
        tileInstanceContainer = new GameObject("Tile Instance Container");

        this.placesOnWait = new List<Place>();
        
        InitializeGrid();
        SetStartingArea(startingPlace);

        this.updating = true;
        StartCoroutine(CheckEntropy());
    }

    public void InitializeGrid()
    {
        grid = new Cell[gridWidth, gridHeight];
        // Add the Cell component for every cell of the grid
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                float cellX = worldOffset.x + (x * cellScale) - (gridWidth*cellScale)/2 + cellScale/2;
                float cellY = worldOffset.y + (y * cellScale) - (gridHeight*cellScale)/2 + cellScale/2;

                Cell newCell = Instantiate(cellObj, new Vector3(cellX,0,cellY), Quaternion.identity);

                newCell.transform.SetParent(cellContainer.transform);

                // Every cell is given all the possible tiles and it's collapsed state is set to false 
                newCell.CreateCell(false, tiles, x, y);
                grid[x,y] = newCell;
            }
        }
    }

    void SetStartingArea(GameObject startingPlace)
    {
        // Create initial grass area
        GameObject startingAreaInstance = GameObject.Instantiate(startingPlace, new Vector3(0, 0, 0), Quaternion.identity);
        Place place = startingAreaInstance.GetComponent<Place>();
        
        place.FillWith(tiles[0]);

        Vector2 dimensions = place.GetDimensions();
        Vector2 position = CalculateGridCoordinates(place.GetPosition().x, place.GetPosition().y);

        SetGridSection(place.GetGrid(),position.x - dimensions.x / 2 + 1, position.y - dimensions.y / 2 + cellScale + 1, dimensions.x, dimensions.y);
    }


    // Find the cell(s) with the least tile possibilies and collapse it into one of the superpositions(tiles)
    IEnumerator CheckEntropy()
    {
        List<Cell> tempGrid = new List<Cell>();
        foreach (Cell c in grid)
        {
            if(!c.collapsed && player.GetInnerPlayerArea().Contains(new Vector2(c.transform.position.x, c.transform.position.z))) 
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
            Tile selectedTile = SelectRandomTile(cellToCollapse);

            // If there is a compatible tile, place the first element of the tiles list (generally the grass tile)
            if (selectedTile != null) {
                cellToCollapse.RecreateCell(new List<Tile> { selectedTile }); 
            } else {
                cellToCollapse.RecreateCell(new List<Tile> { tiles[0] });
            }

            // Instantiate the chosen tile and set it as a child of the instance container
            GameObject instance = cellToCollapse.InstantiateTile();
            instance.transform.SetParent(tileInstanceContainer.transform);
        }

        // Move on to propagate changes and restart temporarily pause cycle
        UpdateGeneration();
    }

    // Selects a random Tile from the possible options based on their weights
    Tile SelectRandomTile(Cell cellToColapse)
    {
        Tile[] options = cellToColapse.GetTileOptions().ToArray();
        Array.Sort(options, (a,b)=> a.weight.CompareTo(b.weight));

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

    //  Looks at the 4 surrounding cells and updates the list of possible tiles.
    void UpdateCellsEntropy(Cell cell, bool propagate)
    {
        int x = cell.GetX();
        int y = cell.GetY();

        // Start with considering all possibilities and then remove the tiles that are not valid by checking the 4 surrounding neighbours
        List<Tile> options = new List<Tile>(tiles);

        if (!cell.collapsed)
        {
            if (y > 0) // DOWN
            {
                Cell down = grid[x, y - 1];
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
                Cell right = grid[x + 1, y];
                List<Tile> validOptions = new List<Tile>();

                foreach (Tile possibleTile in right.GetTileOptions())
                {
                    validOptions = validOptions.Concat(possibleTile.leftNeighbours).ToList();
                }

                CheckValidity(options, validOptions);
            }

            if (y < gridHeight - 1) // UP
            {
                Cell up = grid[x, y + 1];
                List<Tile> validOptions = new List<Tile>();

                foreach (Tile possibleTile in up.GetTileOptions())
                {
                    validOptions = validOptions.Concat(possibleTile.downNeighbours).ToList();
                }

                CheckValidity(options, validOptions);
            }

            if (x > 0) // LEFT
            {
                Cell left = grid[x - 1, y];
                List<Tile> validOptions = new List<Tile>();

                foreach (Tile possibleTile in left.GetTileOptions())
                {
                    validOptions = validOptions.Concat(possibleTile.rightNeighbours).ToList();
                }

                CheckValidity(options, validOptions);
            }

            

            if (cell.GetTileOptions().Count != options.Count)
            {
                
                // Update cell's tile options
                cell.RecreateCell(options);
                if(propagate) { updatedCells.Push(cell); }
            }
        }
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
                Cell up = grid[x, y+1];
                UpdateCellsEntropy(up, true);
            }
            if (x > 0)
            {
                Cell left = grid[x - 1, y];
                UpdateCellsEntropy(left, true);
            }
        }

        // After collapsing one cell and updating the tiles that need to be updated:
        // start another iteration of the algorithm if there are still cells remaining to be collapsed
        // else the algorithm enters a rest state until there's a cell that needs to be collapsed

        iteration++;

        if (iteration < player.GetInnerPlayerArea().GetCellArea(cellScale))
        {
            StartCoroutine(CheckEntropy());
        }
        else
        {
            updating = false;
        }
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

    public void ShiftGrid()
    {
        updating = true;
        Vector2 direction = new Vector2();
        if(moveOffset.x == 0) direction.x = 0;
            else direction.x = moveOffset.x/ Mathf.Abs(moveOffset.x);

        if(moveOffset.y == 0) direction.y = 0;
        else direction.y = moveOffset.y / Mathf.Abs(moveOffset.y);

        PlacePlacesOnWait();
        MoveAndOffsetGeneration(direction);
        /*Shift(direction);*/


        UpdateGeneration();
        /*StartCoroutine(CheckEntropy());*/
    }

    void PlacePlacesOnWait()
    {
        List<Place> finishedPlaces = new List<Place>();
        float halfCellScale = cellScale / 2;
        foreach (Place place in placesOnWait)
        {
            Vector2 dimensions = place.GetDimensions();
            Vector3 placePosition = place.position;
            for (int i = 0; i < dimensions.x; i++)
            {
                for (int j = 0; j < dimensions.y; j++)
                {
                    if (!place.cellPlacements[i, j])
                    {
                        float cellX = placePosition.x - (dimensions.x * cellScale) / 2 + (cellScale * i) + halfCellScale;
                        float cellY = placePosition.y - (dimensions.y * cellScale) / 2 + (cellScale * j) + halfCellScale;
                        Vector2 gridCoordinates = CalculateGridCoordinates(cellX, cellY);
                        int gridX = (int)gridCoordinates.x;
                        int gridY = (int)gridCoordinates.y;

                        if (gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight)
                        {
                            Cell cellToUpdate = grid[gridX, gridY];
                            cellToUpdate.RecreateCell(new List<Tile>() { place.GetGrid()[i, j] });
                            place.SetCellPlacement(i, j, true);
                            updatedCells.Push(cellToUpdate);
                        }
                    }
                }
            }

            if (place.isPlaced)
            {
                finishedPlaces.Add(place);
            }
        }

        foreach (Place place in finishedPlaces)
        {
            placesOnWait.Remove(place);
        }

    }

    void Shift(Vector2 direction)
    {
        if (direction.x != 0) {
            int startX = (direction.x > 0) ? 0 : gridWidth - 1;
            int endX = (direction.x > 0) ? gridWidth - 1 : 0;
            int increment = (direction.x > 0) ? 1 : -1;

            for(int x = startX; x < endX; x += increment)
            {
                for(int y = 0; y < gridHeight - 1; y++)
                {
                    if ((direction.x > 0 && x < endX) || (direction.x < 0 && x > endX)) {
                        if (x == startX)
                        {
                            Destroy(grid[x, y].tileInstance);
                            
                        }
                        grid[x, y].tileInstance = grid[x + increment, y].tileInstance;
                        grid[x, y].tileOptions = grid[x + increment, y].tileOptions;
                        grid[x, y].collapsed = grid[x + increment, y].collapsed;
                    } else
                    {
                        grid[x, y].ResetCell(tiles);
                        updatedCells.Push(grid[x, y]);
                    }
                }
            }
            cellContainer.transform.position += new Vector3(direction.x * cellScale, 0, 0);
            moveOffset.x -= increment;
            iteration -= gridHeight;
            totalMoveOffset.x += increment;
        }
        moveOffset.y = 0;
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
            totalMoveOffset.y -= 1;
        }
    }

    void SetGridSection(Tile[,] newTiles,float x, float y, float width, float height)
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                Cell cellToCollapse = grid[(int)x + i,(int)y + j];
                cellToCollapse.RecreateCell(new List<Tile> { newTiles[i,j] });
            }
        }
    }

    public void AddPlaceForPlacement(Place place)
    {
        placesOnWait.Add(place);
    }

    public Vector2 CalculateGridCoordinates(float x, float y)
    {
        x -= cellScale / 2;
        y -= cellScale / 2;

        // Calculating grid coordinates
        int gridX = Mathf.RoundToInt((x + (gridWidth * cellScale) / 2) / cellScale);
        int gridY = Mathf.RoundToInt((y + (gridHeight * cellScale) / 2) / cellScale);

        // Adjusting for world offset
        gridX -= Mathf.RoundToInt(worldOffset.x / cellScale);
        gridY -= Mathf.RoundToInt(worldOffset.y / cellScale);

        gridX -= Mathf.RoundToInt(totalMoveOffset.x);
        gridY -= Mathf.RoundToInt(totalMoveOffset.y);

        // Adding world offset
        return new Vector2(gridX, gridY);
    }

    public Vector2 CalculateWorldCoordinates(float x, float y)
    {
        if (gridWidth % 2 == 0) { x -= cellScale / 2; }

        if (gridHeight % 2 == 0) { y -= cellScale / 2; }

        // Calculating grid coordinates
        int gridX = Mathf.RoundToInt((x) / cellScale);
        int gridY = Mathf.RoundToInt((y) / cellScale);

        // Adding world offset
        return new Vector2(gridX, gridY);
    }

    public bool IsUpdating() { return updating; }

    public void AddToMoveOffset(Vector2 offset)
    {
        moveOffset += offset;
    }

    public Vector2 GetMoveOffset() { return moveOffset; }

    private void OnDestroy()
    {
        Destroy(gameObject);
    }
}