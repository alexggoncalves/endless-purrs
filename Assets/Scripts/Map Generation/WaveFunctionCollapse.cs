using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.AI.Navigation;

public class WaveFunctionCollapse : MonoBehaviour
{
    public TileLoader tileLoader;
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
    int[,] initialAreaGrid;
    RectangularArea innerArea;
    RectangularArea outerArea;

    // Player
    PlayerController player;
    Vector2 moveOffset = new(0, 0);
    Vector2 totalMoveOffset = new(0, 0);

    // Places
    private List<Place> placesOnWait;
    private Stack<Place> placesToDestroy;
    public Stack<GameObject> instancesToDelete;
    public GameObject homeInstance;


    // Other
    public int iteration = 0;
    public bool initialLoading = true;
    private bool paused;
    private bool updatingCells = false;
    private Coroutine generationCoroutine;
    NatureElementPlacer natureElements;
    NavMeshSurface meshSurface;

    // Optimization variables
    private TileBitmask[] upNeighborsMask;
    private TileBitmask[] downNeighborsMask;
    private TileBitmask[] leftNeighborsMask;
    private TileBitmask[] rightNeighborsMask;
    private TileBitmask defaultPossibleTilesMask;
    private TileBitmask grassSingleOptionMask;
    private int totalTileCount;
    private List<int> scratchList = new List<int>(64);
    private List<int> grassSingleOptionList;
    private WaitForSeconds generationDelay;
    private bool[,] isCellInInnerArea;
    private Vector3[,] cachedCellPositions;

    private void Update()
    {
        while (instancesToDelete.Count > 0)
        {
            GameObject instance = instancesToDelete.Pop();
            if(instance != null )
            {
                Destroy(instance);
            }
        }

        while (placesToDestroy.Count > 0)
        {
            Place place = placesToDestroy.Pop();
            if( place != null)
            {
                placesOnWait.Remove(place);
                Destroy(place.gameObject);
            }
        }
    }

    public void Initialize(TileLoader tileLoader, int width, int height, float cellScale, Cell cellObj, Vector2 worldOffset, PlayerController player, GameObject startingPlace, Vector2 edgeSize)
    {

        this.player = player;
        this.tileLoader = tileLoader;
        this.cellScale = cellScale;
        this.gridWidth = width;
        this.gridHeight = height;
        this.worldOffset = worldOffset;

        meshSurface = GetComponent<NavMeshSurface>();

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

        Vector2 gridDimensions = new Vector2(gridWidth, gridHeight);
        innerArea = cellContainer.AddComponent<RectangularArea>();
        innerArea.Initialize((gridDimensions.x - edgeSize.x * 2f) * cellScale, (gridDimensions.y - edgeSize.y * 2f) * cellScale, worldOffset, UnityEngine.Color.green);
        outerArea = cellContainer.AddComponent<RectangularArea>();
        outerArea.Initialize(gridDimensions.x * cellScale, gridDimensions.y * cellScale, worldOffset, UnityEngine.Color.magenta);

        // Pre-initialize optimization lookups and cached objects
        grassSingleOptionList = new List<int> { tileLoader.grassID };
        grassSingleOptionMask.Clear();
        grassSingleOptionMask.Set(tileLoader.grassID);

        defaultPossibleTilesMask.Clear();
        List<int> possible = tileLoader.GetPossibleTileIDs();
        int possibleCount = possible.Count;
        for (int i = 0; i < possibleCount; i++)
        {
            defaultPossibleTilesMask.Set(possible[i]);
        }

        generationDelay = new WaitForSeconds(0.01f);
        InitializeLookupTables();

        InitializeGrid();
        PlaceStartingArea(startingPlace, 0, 6);
        TriggerUpdate();
    }

    private void InitializeLookupTables()
    {
        List<Tile> tiles = tileLoader.GetAllTiles();
        totalTileCount = tiles.Count;

        upNeighborsMask = new TileBitmask[totalTileCount];
        downNeighborsMask = new TileBitmask[totalTileCount];
        leftNeighborsMask = new TileBitmask[totalTileCount];
        rightNeighborsMask = new TileBitmask[totalTileCount];

        for (int i = 0; i < totalTileCount; i++)
        {
            Tile tile = tiles[i];
            int tileId = tile.GetID();

            upNeighborsMask[tileId].Clear();
            foreach (int neighborId in tile.upNeighbours)
            {
                upNeighborsMask[tileId].Set(neighborId);
            }

            downNeighborsMask[tileId].Clear();
            foreach (int neighborId in tile.downNeighbours)
            {
                downNeighborsMask[tileId].Set(neighborId);
            }

            leftNeighborsMask[tileId].Clear();
            foreach (int neighborId in tile.leftNeighbours)
            {
                leftNeighborsMask[tileId].Set(neighborId);
            }

            rightNeighborsMask[tileId].Clear();
            foreach (int neighborId in tile.rightNeighbours)
            {
                rightNeighborsMask[tileId].Set(neighborId);
            }
        }
    }

    public void InitializeGrid()
    {
        grid = new Cell[gridWidth, gridHeight];
        initialAreaGrid = new int[gridWidth, gridHeight];
        isCellInInnerArea = new bool[gridWidth, gridHeight];
        cachedCellPositions = new Vector3[gridWidth, gridHeight];

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

                Vector3 pos = newCell.transform.position;
                cachedCellPositions[x, y] = pos;
                isCellInInnerArea[x, y] = (x >= edgeSize.x && x < gridWidth - edgeSize.x && y >= edgeSize.y && y < gridHeight - edgeSize.y);
            }
        }
    }

    IEnumerator RunGenerationLoop()
    {
        paused = false;
        updatingCells = true;
        
        while (true)
        {
            // Phase 1: Propagate changes
            if (updatedCells.Count > 0)
            {
                int iterationCounter = 0;
                while (updatedCells.Count > 0)
                {
                    Cell cell = updatedCells.Pop();
                    int cx = cell.GetX();
                    int cy = cell.GetY();
                    Vector3 cellPos = cachedCellPositions[cx, cy];

                    // Forced constraints from places
                    for (int i = 0; i < placesOnWait.Count; i++)
                    {
                        Place p = placesOnWait[i];
                        if (p != null && p.extents != null && p.extents.Contains(cellPos.x, cellPos.z))
                        {
                            cell.RecreateCell(grassSingleOptionMask);
                            break;
                        }
                    }

                    if (cy > 0) UpdateCellsEntropy(grid[cx, cy - 1], true);
                    if (cx < gridWidth - 1) UpdateCellsEntropy(grid[cx + 1, cy], true);
                    if (cy < gridHeight - 1) UpdateCellsEntropy(grid[cx, cy + 1], true);
                    if (cx > 0) UpdateCellsEntropy(grid[cx - 1, cy], true);

                    iterationCounter++;
                    if (iterationCounter % 10 == 0) yield return null;
                }
            }

            // Phase 2: Check if all cells in inner area are collapsed
            Cell cellToCollapse = FindNextCellToCollapse();

            if (cellToCollapse == null)
            {
                // Everything in inner area is collapsed. We can rest.
                break;
            }

            // Phase 3: Collapse a cell
            CollapseCell(cellToCollapse);
            
            yield return generationDelay; // Pacing yield
        }

        updatingCells = false;
        paused = true;
        generationCoroutine = null;

        if (initialLoading)
        {
            initialLoading = false;
            SaveInitialArea();
        }
    }

    private Cell FindNextCellToCollapse()
    {
        // First collapse: Center
        if (iteration == 0)
        {
            int midX = gridWidth / 2;
            int midY = gridHeight / 2;
            Cell c = grid[midX, midY];
            if (!c.collapsed && isCellInInnerArea[midX, midY]) return c;
        }

        // Reservoir sampling for minimum entropy
        Cell best = null;
        int minCount = int.MaxValue;
        int reservoirCount = 0;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (isCellInInnerArea[x, y] && !grid[x, y].collapsed)
                {
                    int count = grid[x, y].tileOptionsMask.Count;
                    if (count < minCount)
                    {
                        minCount = count;
                        best = grid[x, y];
                        reservoirCount = 1;
                    }
                    else if (count == minCount)
                    {
                        reservoirCount++;
                        if (UnityEngine.Random.Range(0, reservoirCount) == 0) best = grid[x, y];
                    }
                }
            }
        }
        return best;
    }


    //  Collapses one of the cells with the least number of tile possibilities(superpositions)
    void CollapseCell(Cell cellToCollapse)
    {
        if (cellToCollapse != null)
        {
            // As the cell is being altered: add the cell to the updated cells stack to later update surrounding cells
            updatedCells.Push(cellToCollapse);

            // Set the cell as collapsed
            cellToCollapse.collapsed = true;

            // Choose onde of the possibilities
            int selectedTile = SelectRandomTile(cellToCollapse);

            // If there is a compatible tile, place the first element of the tiles list (generally the grass tile)
            if (selectedTile != -1)
            {
                if (selectedTile == tileLoader.grassID)
                {
                    cellToCollapse.RecreateCell(grassSingleOptionList);
                }
                else
                {
                    cellToCollapse.RecreateCell(new List<int> { selectedTile });
                }

                if (!CellIsInsidePlace(cellToCollapse))
                {
                    string tileName = tileLoader.GetNameById(selectedTile);
                    if (tileName == "grass")
                    {
                        GameObject natureElement = natureElements.PlaceElement(cellToCollapse.transform.position, cellScale, NatureElementPlacer.BiomeType.Forest);
                        cellToCollapse.SetNatureElementInstance(natureElement);
                    }
                    else if (tileName == "grass_L1")
                    {
                        GameObject natureElement = natureElements.PlaceElement(cellToCollapse.transform.position + Vector3.up * 1.6f, cellScale, NatureElementPlacer.BiomeType.Forest);
                        cellToCollapse.SetNatureElementInstance(natureElement);
                    }
                    else if (tileName == "grass_L2")
                    {
                        GameObject natureElement = natureElements.PlaceElement(cellToCollapse.transform.position + Vector3.up * 2.8f, cellScale, NatureElementPlacer.BiomeType.Forest);
                        cellToCollapse.SetNatureElementInstance(natureElement);
                    }
                }
            }
            else
            {
                cellToCollapse.RecreateCell(grassSingleOptionList);
            }

            // Instantiate the chosen tile and set it as a child of the instance container
            GameObject instance = cellToCollapse.InstantiateTile();
            instance.layer = LayerMask.NameToLayer("Tiles");

            instance.transform.SetParent(tileInstanceContainer.transform);

        }
        // Move on to propagate changes and restart temporarily pause cycle
        TriggerUpdate();
    }


    int SelectRandomTile(Cell cellToCollapse)
    {
        TileBitmask mask = cellToCollapse.tileOptionsMask;
        
        // Calculate total weight
        float totalWeight = 0f;
        ulong temp = mask.m0;
        while (temp != 0) {
            int id = TrailingZeroCount(temp);
            totalWeight += tileLoader.GetTileByID(id).weight;
            temp &= temp - 1;
        }
        temp = mask.m1;
        while (temp != 0) {
            int id = 64 + TrailingZeroCount(temp);
            totalWeight += tileLoader.GetTileByID(id).weight;
            temp &= temp - 1;
        }
        temp = mask.m2;
        while (temp != 0) {
            int id = 128 + TrailingZeroCount(temp);
            totalWeight += tileLoader.GetTileByID(id).weight;
            temp &= temp - 1;
        }
        temp = mask.m3;
        while (temp != 0) {
            int id = 192 + TrailingZeroCount(temp);
            totalWeight += tileLoader.GetTileByID(id).weight;
            temp &= temp - 1;
        }

        float diceRoll = UnityEngine.Random.Range(0, totalWeight);
        
        float cumulative = 0f;
        temp = mask.m0;
        while (temp != 0) {
            int id = TrailingZeroCount(temp);
            cumulative += tileLoader.GetTileByID(id).weight;
            if (diceRoll < cumulative) return id;
            temp &= temp - 1;
        }
        temp = mask.m1;
        while (temp != 0) {
            int id = 64 + TrailingZeroCount(temp);
            cumulative += tileLoader.GetTileByID(id).weight;
            if (diceRoll < cumulative) return id;
            temp &= temp - 1;
        }
        temp = mask.m2;
        while (temp != 0) {
            int id = 128 + TrailingZeroCount(temp);
            cumulative += tileLoader.GetTileByID(id).weight;
            if (diceRoll < cumulative) return id;
            temp &= temp - 1;
        }
        temp = mask.m3;
        while (temp != 0) {
            int id = 192 + TrailingZeroCount(temp);
            cumulative += tileLoader.GetTileByID(id).weight;
            if (diceRoll < cumulative) return id;
            temp &= temp - 1;
        }
        return -1;
    }

    //  Looks at the 4 surrounding cells and updates the list of possible tiles.
    void UpdateCellsEntropy(Cell cell, bool propagate)
    {
        int x = cell.GetX();
        int y = cell.GetY();

        if (cell.collapsed) return;

        // Re-evaluate from scratch to allow recovery from contradictions and handle place changes
        TileBitmask options = defaultPossibleTilesMask;

        // Apply Place constraints first
        Vector3 cellPos = cachedCellPositions[x, y];
        for (int i = 0; i < placesOnWait.Count; i++)
        {
            Place p = placesOnWait[i];
            if (p != null && p.extents != null && p.extents.Contains(cellPos.x, cellPos.z))
            {
                options = TileBitmask.And(options, grassSingleOptionMask);
                break;
            }
        }

        // Check DOWN
        if (y > 0)
        {
            TileBitmask allowed = new TileBitmask();
            TileBitmask downMask = grid[x, y - 1].tileOptionsMask;
            
            ulong temp = downMask.m0;
            while (temp != 0) {
                int t = TrailingZeroCount(temp);
                allowed = TileBitmask.Or(allowed, upNeighborsMask[t]);
                temp &= temp - 1;
            }
            temp = downMask.m1;
            while (temp != 0) {
                int t = 64 + TrailingZeroCount(temp);
                allowed = TileBitmask.Or(allowed, upNeighborsMask[t]);
                temp &= temp - 1;
            }
            temp = downMask.m2;
            while (temp != 0) {
                int t = 128 + TrailingZeroCount(temp);
                allowed = TileBitmask.Or(allowed, upNeighborsMask[t]);
                temp &= temp - 1;
            }
            temp = downMask.m3;
            while (temp != 0) {
                int t = 192 + TrailingZeroCount(temp);
                allowed = TileBitmask.Or(allowed, upNeighborsMask[t]);
                temp &= temp - 1;
            }

            options = TileBitmask.And(options, allowed);
        }

        // Check RIGHT
        if (x < gridWidth - 1)
        {
            TileBitmask allowed = new TileBitmask();
            TileBitmask rightMask = grid[x + 1, y].tileOptionsMask;

            ulong temp = rightMask.m0;
            while (temp != 0) {
                int t = TrailingZeroCount(temp);
                allowed = TileBitmask.Or(allowed, leftNeighborsMask[t]);
                temp &= temp - 1;
            }
            temp = rightMask.m1;
            while (temp != 0) {
                int t = 64 + TrailingZeroCount(temp);
                allowed = TileBitmask.Or(allowed, leftNeighborsMask[t]);
                temp &= temp - 1;
            }
            temp = rightMask.m2;
            while (temp != 0) {
                int t = 128 + TrailingZeroCount(temp);
                allowed = TileBitmask.Or(allowed, leftNeighborsMask[t]);
                temp &= temp - 1;
            }
            temp = rightMask.m3;
            while (temp != 0) {
                int t = 192 + TrailingZeroCount(temp);
                allowed = TileBitmask.Or(allowed, leftNeighborsMask[t]);
                temp &= temp - 1;
            }

            options = TileBitmask.And(options, allowed);
        }

        // Check UP
        if (y < gridHeight - 1)
        {
            TileBitmask allowed = new TileBitmask();
            TileBitmask upMask = grid[x, y + 1].tileOptionsMask;

            ulong temp = upMask.m0;
            while (temp != 0) {
                int t = TrailingZeroCount(temp);
                allowed = TileBitmask.Or(allowed, downNeighborsMask[t]);
                temp &= temp - 1;
            }
            temp = upMask.m1;
            while (temp != 0) {
                int t = 64 + TrailingZeroCount(temp);
                allowed = TileBitmask.Or(allowed, downNeighborsMask[t]);
                temp &= temp - 1;
            }
            temp = upMask.m2;
            while (temp != 0) {
                int t = 128 + TrailingZeroCount(temp);
                allowed = TileBitmask.Or(allowed, downNeighborsMask[t]);
                temp &= temp - 1;
            }
            temp = upMask.m3;
            while (temp != 0) {
                int t = 192 + TrailingZeroCount(temp);
                allowed = TileBitmask.Or(allowed, downNeighborsMask[t]);
                temp &= temp - 1;
            }

            options = TileBitmask.And(options, allowed);
        }

        // Check LEFT
        if (x > 0)
        {
            TileBitmask allowed = new TileBitmask();
            TileBitmask leftMask = grid[x - 1, y].tileOptionsMask;

            ulong temp = leftMask.m0;
            while (temp != 0) {
                int t = TrailingZeroCount(temp);
                allowed = TileBitmask.Or(allowed, rightNeighborsMask[t]);
                temp &= temp - 1;
            }
            temp = leftMask.m1;
            while (temp != 0) {
                int t = 64 + TrailingZeroCount(temp);
                allowed = TileBitmask.Or(allowed, rightNeighborsMask[t]);
                temp &= temp - 1;
            }
            temp = leftMask.m2;
            while (temp != 0) {
                int t = 128 + TrailingZeroCount(temp);
                allowed = TileBitmask.Or(allowed, rightNeighborsMask[t]);
                temp &= temp - 1;
            }
            temp = leftMask.m3;
            while (temp != 0) {
                int t = 192 + TrailingZeroCount(temp);
                allowed = TileBitmask.Or(allowed, rightNeighborsMask[t]);
                temp &= temp - 1;
            }

            options = TileBitmask.And(options, allowed);
        }

        if (!cell.tileOptionsMask.Equals(options))
        {
            cell.RecreateCell(options);
            if (propagate)
            {
                updatedCells.Push(cell);
            }
        }
    }

    private static int TrailingZeroCount(ulong v)
    {
        if (v == 0) return 64;
        int n = 0;
        if ((v & 0xFFFFFFFFUL) == 0) { n += 32; v >>= 32; }
        if ((v & 0xFFFFUL) == 0) { n += 16; v >>= 16; }
        if ((v & 0xFFUL) == 0) { n += 8; v >>= 8; }
        if ((v & 0xFUL) == 0) { n += 4; v >>= 4; }
        if ((v & 0x3UL) == 0) { n += 2; v >>= 2; }
        if ((v & 0x1UL) == 0) { n += 1; }
        return n;
    }



    public void InitialUpdate()
    {
        Place homePlace = homeInstance.GetComponent<Place>();
        while (updatedCells.Count > 0)
        {
            Cell cell = updatedCells.Pop();
            int cx = cell.GetX();
            int cy = cell.GetY();
            Vector3 cellPos = cachedCellPositions[cx, cy];

            if (homePlace.extents.Contains(cellPos.x, cellPos.z))
            {
                cell.RecreateCell(grassSingleOptionList);
            }

            if (cy > 0)
            {
                // Down Cell;
                UpdateCellsEntropy(grid[cx, cy - 1], true);

            }
            if (cx < gridWidth - 1)
            {
                // Right Cell
                UpdateCellsEntropy(grid[cx + 1, cy], true);

            }
            if (cy < gridHeight - 1)
            {
                // Up Cell
                UpdateCellsEntropy(grid[cx, cy + 1], true);

            }
            if (cx > 0)
            {
                // Left Cell
                UpdateCellsEntropy(grid[cx - 1, cy], true);

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
        TriggerUpdate();
    }

    private void TriggerUpdate()
    {
        if (generationCoroutine == null)
        {
            generationCoroutine = StartCoroutine(RunGenerationLoop());
        }
    }

    void MoveAndOffsetGeneration(Vector2 direction)
    {
        // Handle X Shift
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
            totalMoveOffset.x -= 1;
        }

        // Handle Y Shift
        if (direction.y == 1)
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
            totalMoveOffset.y -= 1;
        }

        Vector3 shiftVector = new Vector3(direction.x * cellScale, 0, direction.y * cellScale);
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                cachedCellPositions[x, y] += shiftVector;
            }
        }
    }

    void PlaceStartingArea(GameObject startingPlace, int x, int y)
    {
        // Create initial grass area
        homeInstance = Instantiate(startingPlace, new Vector3(x, 0, y), Quaternion.identity);
        Place place = homeInstance.GetComponent<Place>();
        place.Initialize(new Vector2(x, y), tileLoader.grassID, cellScale);
        placesOnWait.Add(place);

        Vector2 dimensions = place.GetDimensions();
        Vector2 position = CalculateGridCoordinates(place.GetPosition().x, place.GetPosition().y);

        SetGridSection(place.GetGrid(), position.x - dimensions.x / 2 + 1, position.y - dimensions.y / 2 + cellScale + 1, dimensions.x, dimensions.y);
        
    }

    void SaveInitialArea()
    {
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                initialAreaGrid[i, j] = grid[i, j].tileOptions[0];
            }
        }
    }

    public void MoveToOrigin()
    {
        updatedCells.Clear();
        instancesToDelete.Clear();

        cellContainer.transform.position = new Vector3(0,0,0);

        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                instancesToDelete.Push(grid[i, j].tileInstance);
                instancesToDelete.Push(grid[i, j].natureElement);
          /*      grid[i, j].tileInstance = null;
                grid[i, j].natureElement = null;
                grid[i, j].collapsed = false;*/
                grid[i, j].ResetCell();
                /*grid[i, j].RecreateCell(new List<int> { initialAreaGrid[i, j] });*/
                /*updatedCells.Push(grid[i, j]);*/

                float cellX = worldOffset.x + (i * cellScale) - (gridWidth * cellScale) / 2 + cellScale / 2;
                float cellY = worldOffset.y + (j * cellScale) - (gridHeight * cellScale) / 2 + cellScale / 2;
                cachedCellPositions[i, j] = new Vector3(cellX, 0, cellY);
            }
        }

        paused = false;

        iteration =  -1;
        TriggerUpdate();
    }


    void SwapCellState(Cell copyTo, Cell copyFrom)
    {
        copyTo.natureElement = copyFrom.natureElement;
        copyTo.tileInstance = copyFrom.tileInstance;
        copyTo.tileOptions = copyFrom.tileOptions;
        copyTo.tileOptionsMask = copyFrom.tileOptionsMask;
        copyTo.collapsed = copyFrom.collapsed;
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

    void SetGridSection(int[,] newTiles, float x, float y, float width, float height)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                int gx = (int)x + i;
                int gy = (int)y + j;
                if (gx >= 0 && gx < gridWidth && gy >= 0 && gy < gridHeight)
                {
                    Cell cell = grid[gx, gy];
                    cell.RecreateCell(new List<int> { newTiles[i, j] });
                    updatedCells.Push(cell);
                }
            }
        }
    }

    Boolean CellIsInsidePlace(Cell cell)
    {
        Vector3 pos = cell.transform.position;
        for (int i = 0; i < placesOnWait.Count; i++)
        {
            Place place = placesOnWait[i];
            if (place != null && place.extents != null && place.extents.Contains(pos.x, pos.z))
            {
                return true;
            }
        }
        return false;
    }

    public void ClearMoveOffset()
    {
        moveOffset = new Vector2(0, 0);
        totalMoveOffset = new Vector2(0, 0);
    }

    public void AddToMoveOffset(Vector2 offset)
    {
        moveOffset += offset;
    }


    public GameObject GetHomeInstance()
    {
        return homeInstance;
    }

    public void AddPlaceForPlacement(Place place)
    {
        placesOnWait.Add(place);
    }

    public void AddPlaceToDestroy(Place place)
    {
        placesToDestroy.Push(place);
    }

    public bool IsUpdatingCells()
    {
        return updatingCells;
    }
    public bool IsPaused() { return paused; }

    public Vector2 GetMoveOffset() { return moveOffset; }

    public Boolean HasLoadedInitialZone()
    {
        return !initialLoading;
    }

    public int GetIteration()
    {
        return iteration;
    }

    public RectangularArea GetInnerArea()
    {
        return innerArea;
    }

    public RectangularArea GetOuterArea()
    {
        return outerArea;
    }

    private void OnDestroy()
    {
        Destroy(gameObject);
    }
}