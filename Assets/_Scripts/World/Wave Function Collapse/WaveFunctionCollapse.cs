using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(TileLoader))]
public class WaveFunctionCollapse : MonoBehaviour
{
    private TileLoader tileLoader;
    private DecorationPlacer decorationPlacer;

    // Dimensions
    float cellScale;
    int gridWidth, gridHeight;
    Vector2 worldOffset = new(0, 0);
    int edgeSize;

    // Cells
    GameObject cellContainer;
    private Cell cellObj;
    public Stack<Cell> updatedCells;

    // Grid
    public Cell[,] grid;
    GameObject tileInstanceContainer;
    RectangularArea innerArea;
    RectangularArea outerArea;
    Vector2 moveOffset = new(0, 0);
    Vector2 totalMoveOffset = new(0, 0);

    // Places
    private List<Place> placesOnWait;
    public Stack<GameObject> instancesToDelete;
    public GameObject homeInstance;


    // Other
    public int iteration = 0;
    public bool initialLoading = true;
    private bool paused;
    private bool updatingCells = false;
    private bool initialized = false;

    private Coroutine generationCoroutine;


    // Optimization variables
    private TileBitmask[] upNeighborsMask;
    private TileBitmask[] downNeighborsMask;
    private TileBitmask[] leftNeighborsMask;
    private TileBitmask[] rightNeighborsMask;
    private TileBitmask defaultPossibleTilesMask;
    private TileBitmask singleTileScratchMask;

    private int totalTileCount;

    private WaitForSeconds generationDelay;
    private bool[,] isCellInInnerArea;
    private Vector3[,] cachedCellPositions;
    private readonly Queue<Cell> pendingInstantiations = new();
    private const int InstantiationsPerFrame = 3;

    private void Update()
    {
        if (!initialized) return;

        int budget = InstantiationsPerFrame;
        while (pendingInstantiations.Count > 0 && budget-- > 0)
        {
            Cell cell = pendingInstantiations.Dequeue();
            if (cell == null) continue;
            GameObject instance = cell.InstantiateTile();
            instance.layer = LayerMask.NameToLayer("Tiles");
            instance.transform.SetParent(tileInstanceContainer.transform);
        }

        while (instancesToDelete.Count > 0)
        {
            GameObject instance = instancesToDelete.Pop();
            if (instance != null)
            {
                Destroy(instance);
            }
        }
    }

    public void Initialize(TileLoader tileLoader, int width, int height, float cellScale, Cell cellObj, Vector2 worldOffset, PlayerController player, GameObject startingPlace, int edgeSize)
    {
        this.tileLoader = GetComponent<TileLoader>();

        #pragma warning disable UNT0039
        // Try get optional decoration Placer;
        decorationPlacer = GetComponent<DecorationPlacer>();
        if (decorationPlacer != null) decorationPlacer.CellScale = cellScale;
        #pragma warning restore UNT0039

        this.cellScale = cellScale;
        this.gridWidth = width;
        this.gridHeight = height;
        this.worldOffset = worldOffset;
        this.edgeSize = edgeSize;
        this.cellObj = cellObj;

        updatedCells = new Stack<Cell>();
        cellContainer = new GameObject("Grid Container");
        tileInstanceContainer = new GameObject("Tile Instance Container");
        instancesToDelete = new Stack<GameObject>();
        generationDelay = new WaitForSeconds(0.01f);


        this.placesOnWait = new List<Place>();
        this.paused = false;

        Vector2 gridDimensions = new(gridWidth, gridHeight);
        innerArea = cellContainer.AddComponent<RectangularArea>();
        innerArea.Initialize((gridDimensions.x - edgeSize * 2f) * cellScale, (gridDimensions.y - edgeSize * 2f) * cellScale, worldOffset, UnityEngine.Color.green);
        outerArea = cellContainer.AddComponent<RectangularArea>();
        outerArea.Initialize(gridDimensions.x * cellScale, gridDimensions.y * cellScale, worldOffset, UnityEngine.Color.magenta);

        defaultPossibleTilesMask = tileLoader.GetPossibleTilesMask();
        InitializeLookupTables();

        InitializeGrid();
        PlaceStartingArea(startingPlace, 0, 6);
        TriggerUpdate();

        initialized = true;
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

            upNeighborsMask[tileId] = tile.upNeighboursMask;
            downNeighborsMask[tileId] = tile.downNeighboursMask;
            leftNeighborsMask[tileId] = tile.leftNeighboursMask;
            rightNeighborsMask[tileId] = tile.rightNeighboursMask;
        }
    }

    public void InitializeGrid()
    {
        grid = new Cell[gridWidth, gridHeight];
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
                newCell.CreateCell(false, x, y, tileLoader);
                grid[x, y] = newCell;

                Vector3 pos = newCell.transform.position;
                cachedCellPositions[x, y] = pos;
                isCellInInnerArea[x, y] = (x >= edgeSize && x < gridWidth - edgeSize && y >= edgeSize && y < gridHeight - edgeSize);
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
                        if (p != null && !cell.collapsed && p.GetExtents().Contains(cellPos.x, cellPos.z))
                        {
                            cell.RecreateCell(p.GetTileBitmask());
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
        }
    }

    private Cell FindNextCellToCollapse()
    {
        // 1. First collapse at center
        // Collapse center cell first to avoid invalid colapses around initial place
        if (iteration == 0)
        {
            int midX = gridWidth / 2;
            int midY = gridHeight / 2;

            Cell c = grid[midX, midY];

            if (!c.collapsed && isCellInInnerArea[midX, midY])
                return c;
        }

        // 2. Go over the grid and find best candidate for collapse
        // Cells inside the extent of a Place take priority

        Cell placeBest = null; // Best candidate found so far that's inside a Place
        int placeMin = int.MaxValue;
        int placeScore = 0;

        Cell best = null; // Best candidate found so far, ignoring Places
        int minCount = int.MaxValue;
        int reservoirCount = 0;

        bool hasPlaces = placesOnWait.Count > 0;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (!isCellInInnerArea[x, y]) continue; // Skip cells on the margin. These cells only exist for propagation

                Cell c = grid[x, y];
                if (c.collapsed) continue;

                int count = c.tileOptionsMask.Count;
                if (count <= 0) continue;

                // Find cell candidate with least tile options (outside places first)
                // If there is a tie, randomly decide either to swap it or not
                if (count < minCount)
                {
                    minCount = count;
                    best = c;
                    reservoirCount = 1;
                }
                else if (count == minCount)
                {
                    reservoirCount++;
                    if (UnityEngine.Random.Range(0, reservoirCount) == 0)
                        best = c;
                }

                if (!hasPlaces) continue; // Skip place related search if there are no Places on the wait list 
                if (count > placeMin) continue; // If this cell can't beat the current best place candidate skip place check

                // Check if this cell is inside any of the places
                Vector3 pos = cachedCellPositions[x, y];
                bool insidePlace = false;
                for (int i = 0; i < placesOnWait.Count; i++)
                {
                    Place p = placesOnWait[i];
                    if (p != null && p.GetExtents() != null && p.GetExtents().Contains(pos.x, pos.z))
                    {
                        insidePlace = true;
                        break;
                    }
                }
                if (!insidePlace) continue;

                // Find cell candidate with least tile options again (now inside places extents)
                if (count < placeMin)
                {
                    placeMin = count;
                    placeBest = c;
                    placeScore = 1;
                }
                else if (count == placeMin)
                {
                    placeScore++;
                    if (UnityEngine.Random.Range(0, placeScore) == 0)
                        placeBest = c;
                }
            }
        }

        // Return best candidate
        // Cells inside a place always take priority
        return placeBest != null ? placeBest : best;
    }


    //  Collapses one of the cells with the least number of tile possibilities(superpositions)
    void CollapseCell(Cell cellToCollapse)
    {
        if (cellToCollapse == null) return;

        int selectedTile = SelectRandomTile(cellToCollapse);

        if (selectedTile == -1)
        {
            cellToCollapse.RecreateCell(defaultPossibleTilesMask);

            updatedCells.Push(cellToCollapse);
            TriggerUpdate();
            return;
        }

        // NOW it's safe to collapse
        cellToCollapse.collapsed = true;
        iteration++;

        updatedCells.Push(cellToCollapse);

        singleTileScratchMask.Clear();
        singleTileScratchMask.Set(selectedTile);
        cellToCollapse.RecreateCell(singleTileScratchMask);

        // Queue cell instantiation
        pendingInstantiations.Enqueue(cellToCollapse);

        // Place decorations
        if (decorationPlacer != null && !CellIsInsidePlace(cellToCollapse))
        {
            Tile tile = tileLoader.GetTileByID(selectedTile);
            decorationPlacer.TryDecorate(cellToCollapse, tile);
        }

        TriggerUpdate();
    }

    int SelectRandomTile(Cell cellToCollapse)
    {
        TileBitmask mask = cellToCollapse.tileOptionsMask;

        // Calculate total weight
        float totalWeight = 0f;
        ulong temp = mask.m0;
        while (temp != 0)
        {
            int id = TileBitmask.TrailingZeroCount(temp);
            totalWeight += tileLoader.GetTileByID(id).weight;
            temp &= temp - 1;
        }
        temp = mask.m1;
        while (temp != 0)
        {
            int id = 64 + TileBitmask.TrailingZeroCount(temp);
            totalWeight += tileLoader.GetTileByID(id).weight;
            temp &= temp - 1;
        }
        temp = mask.m2;
        while (temp != 0)
        {
            int id = 128 + TileBitmask.TrailingZeroCount(temp);
            totalWeight += tileLoader.GetTileByID(id).weight;
            temp &= temp - 1;
        }
        temp = mask.m3;
        while (temp != 0)
        {
            int id = 192 + TileBitmask.TrailingZeroCount(temp);
            totalWeight += tileLoader.GetTileByID(id).weight;
            temp &= temp - 1;
        }

        if (totalWeight <= 0f)
        {
            return -1;
        }

        float diceRoll = Random.value * totalWeight;

        float cumulative = 0f;
        temp = mask.m0;
        while (temp != 0)
        {
            int id = TileBitmask.TrailingZeroCount(temp);
            cumulative += tileLoader.GetTileByID(id).weight;
            if (diceRoll < cumulative) return id;
            temp &= temp - 1;
        }
        temp = mask.m1;
        while (temp != 0)
        {
            int id = 64 + TileBitmask.TrailingZeroCount(temp);
            cumulative += tileLoader.GetTileByID(id).weight;
            if (diceRoll < cumulative) return id;
            temp &= temp - 1;
        }
        temp = mask.m2;
        while (temp != 0)
        {
            int id = 128 + TileBitmask.TrailingZeroCount(temp);
            cumulative += tileLoader.GetTileByID(id).weight;
            if (diceRoll < cumulative) return id;
            temp &= temp - 1;
        }
        temp = mask.m3;
        while (temp != 0)
        {
            int id = 192 + TileBitmask.TrailingZeroCount(temp);
            cumulative += tileLoader.GetTileByID(id).weight;
            if (diceRoll < cumulative) return id;
            temp &= temp - 1;
        }

        return mask.GetFirstSetBit();
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
        TileBitmask placeConstraint = defaultPossibleTilesMask; // tracked separately for recovery
        bool inPlace = false;
        for (int i = 0; i < placesOnWait.Count; i++)
        {
            Place p = placesOnWait[i];
            if (p != null && p.GetExtents() != null && p.GetExtents().Contains(cellPos.x, cellPos.z))
            {
                placeConstraint = p.GetTileBitmask();
                options = TileBitmask.And(options, placeConstraint);
                inPlace = true;
                break;
            }
        }

        // Check DOWN
        if (y > 0)
        {
            TileBitmask allowed = new();
            TileBitmask downMask = grid[x, y - 1].tileOptionsMask;
            ulong temp = downMask.m0;
            while (temp != 0) { int t = TileBitmask.TrailingZeroCount(temp); allowed = TileBitmask.Or(allowed, upNeighborsMask[t]); temp &= temp - 1; }
            temp = downMask.m1;
            while (temp != 0) { int t = 64 + TileBitmask.TrailingZeroCount(temp); allowed = TileBitmask.Or(allowed, upNeighborsMask[t]); temp &= temp - 1; }
            temp = downMask.m2;
            while (temp != 0) { int t = 128 + TileBitmask.TrailingZeroCount(temp); allowed = TileBitmask.Or(allowed, upNeighborsMask[t]); temp &= temp - 1; }
            temp = downMask.m3;
            while (temp != 0) { int t = 192 + TileBitmask.TrailingZeroCount(temp); allowed = TileBitmask.Or(allowed, upNeighborsMask[t]); temp &= temp - 1; }

            options = TileBitmask.And(options, allowed);
        }

        // Check RIGHT
        if (x < gridWidth - 1)
        {
            TileBitmask allowed = new TileBitmask();
            TileBitmask rightMask = grid[x + 1, y].tileOptionsMask;
            ulong temp = rightMask.m0;
            while (temp != 0) { int t = TileBitmask.TrailingZeroCount(temp); allowed = TileBitmask.Or(allowed, leftNeighborsMask[t]); temp &= temp - 1; }
            temp = rightMask.m1;
            while (temp != 0) { int t = 64 + TileBitmask.TrailingZeroCount(temp); allowed = TileBitmask.Or(allowed, leftNeighborsMask[t]); temp &= temp - 1; }
            temp = rightMask.m2;
            while (temp != 0) { int t = 128 + TileBitmask.TrailingZeroCount(temp); allowed = TileBitmask.Or(allowed, leftNeighborsMask[t]); temp &= temp - 1; }
            temp = rightMask.m3;
            while (temp != 0) { int t = 192 + TileBitmask.TrailingZeroCount(temp); allowed = TileBitmask.Or(allowed, leftNeighborsMask[t]); temp &= temp - 1; }

            options = TileBitmask.And(options, allowed);
        }

        // Check UP
        if (y < gridHeight - 1)
        {
            TileBitmask allowed = new TileBitmask();
            TileBitmask upMask = grid[x, y + 1].tileOptionsMask;
            ulong temp = upMask.m0;
            while (temp != 0) { int t = TileBitmask.TrailingZeroCount(temp); allowed = TileBitmask.Or(allowed, downNeighborsMask[t]); temp &= temp - 1; }
            temp = upMask.m1;
            while (temp != 0) { int t = 64 + TileBitmask.TrailingZeroCount(temp); allowed = TileBitmask.Or(allowed, downNeighborsMask[t]); temp &= temp - 1; }
            temp = upMask.m2;
            while (temp != 0) { int t = 128 + TileBitmask.TrailingZeroCount(temp); allowed = TileBitmask.Or(allowed, downNeighborsMask[t]); temp &= temp - 1; }
            temp = upMask.m3;
            while (temp != 0) { int t = 192 + TileBitmask.TrailingZeroCount(temp); allowed = TileBitmask.Or(allowed, downNeighborsMask[t]); temp &= temp - 1; }

            options = TileBitmask.And(options, allowed);
        }

        // Check LEFT
        if (x > 0)
        {
            TileBitmask allowed = new TileBitmask();
            TileBitmask leftMask = grid[x - 1, y].tileOptionsMask;
            ulong temp = leftMask.m0;
            while (temp != 0) { int t = TileBitmask.TrailingZeroCount(temp); allowed = TileBitmask.Or(allowed, rightNeighborsMask[t]); temp &= temp - 1; }
            temp = leftMask.m1;
            while (temp != 0) { int t = 64 + TileBitmask.TrailingZeroCount(temp); allowed = TileBitmask.Or(allowed, rightNeighborsMask[t]); temp &= temp - 1; }
            temp = leftMask.m2;
            while (temp != 0) { int t = 128 + TileBitmask.TrailingZeroCount(temp); allowed = TileBitmask.Or(allowed, rightNeighborsMask[t]); temp &= temp - 1; }
            temp = leftMask.m3;
            while (temp != 0) { int t = 192 + TileBitmask.TrailingZeroCount(temp); allowed = TileBitmask.Or(allowed, rightNeighborsMask[t]); temp &= temp - 1; }

            options = TileBitmask.And(options, allowed);
        }

        // Contradiction recovery: only after ALL four neighbors have been
        // applied. Falls back to the place constraint if inside one, so
        // place forcing isn't defeated by recovery.
        if (options.IsEmpty)
        {
            options = inPlace ? placeConstraint : defaultPossibleTilesMask;
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
                            if (grid[x, y].decoration != null) instancesToDelete.Push(grid[x, y].decoration);
                            instancesToDelete.Push(grid[x, y].tileInstance);
                        }
                        CopyCellState(grid[x, y], grid[x + 1, y]);
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
                            if (grid[x, y].decoration != null) instancesToDelete.Push(grid[x, y].decoration);
                            instancesToDelete.Push(grid[x, y].tileInstance);
                        }
                        CopyCellState(grid[x, y], grid[x - 1, y]);
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
                            if (grid[x, y].decoration != null) instancesToDelete.Push(grid[x, y].decoration);
                            instancesToDelete.Push(grid[x, y].tileInstance);
                        }

                        CopyCellState(grid[x, y], grid[x, y + 1]);
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
                            if (grid[x, y].decoration != null) instancesToDelete.Push(grid[x, y].decoration);
                            instancesToDelete.Push(grid[x, y].tileInstance);
                        }
                        CopyCellState(grid[x, y], grid[x, y - 1]);
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
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(homeInstance, gameObject.scene);

        Place place = homeInstance.GetComponent<Place>();
        place.Initialize(new Vector2(x, y), tileLoader, cellScale);
        placesOnWait.Add(place);

        Vector2 dimensions = place.GetPlaceGridDimensions();
        Vector2 position = CalculateGridCoordinates(place.GetPosition().x, place.GetPosition().y);

        SetGridSection(place.GetGrid(), position.x - dimensions.x / 2 + 1, position.y - dimensions.y / 2 + cellScale + 1, dimensions.x, dimensions.y);
    }

    public void ClearPendingShift()
    {
        moveOffset = Vector2.zero;
    }

    public IEnumerator MoveToOriginRoutine()
    {
        paused = true;
        updatingCells = false;

        updatedCells.Clear();
        instancesToDelete.Clear();
        pendingInstantiations.Clear();

        cellContainer.transform.position = new Vector3(0, 0, 0);

        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                instancesToDelete.Push(grid[i, j].tileInstance);

                if (grid[i, j].decoration != null)
                    instancesToDelete.Push(grid[i, j].decoration);

                grid[i, j].ResetCell();

                float cellX = worldOffset.x + (i * cellScale) - (gridWidth * cellScale) / 2 + cellScale / 2;
                float cellY = worldOffset.y + (j * cellScale) - (gridHeight * cellScale) / 2 + cellScale / 2;
                cachedCellPositions[i, j] = new Vector3(cellX, 0, cellY);
            }
        }

        iteration = 0;

        yield return null;
    }

    public void Resume()
    {
        paused = false;
        TriggerUpdate();
    }

    // Copy cell state to new positioning on grid (Used on grid shift)
    void CopyCellState(Cell copyTo, Cell copyFrom)
    {
        copyTo.decoration = copyFrom.decoration;
        copyTo.tileInstance = copyFrom.tileInstance;
        copyTo.tileOptionsMask = copyFrom.tileOptionsMask;
        copyTo.collapsed = copyFrom.collapsed;

        copyFrom.decoration = null;
        copyFrom.tileInstance = null;
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

                    TileBitmask m = new();
                    m.Set(newTiles[i, j]);

                    cell.RecreateCell(m);
                    updatedCells.Push(cell);
                }
            }
        }
    }

    bool CellIsInsidePlace(Cell cell)
    {
        Vector3 pos = cell.transform.position;
        for (int i = 0; i < placesOnWait.Count; i++)
        {
            Place place = placesOnWait[i];
            if (place != null && place.GetExtents() != null && place.GetExtents().Contains(pos.x, pos.z))
            {
                return true;
            }
        }
        return false;
    }

    public void ResetMoveOffset()
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

    public void RemovePlace(Place place)
    {
        placesOnWait.Remove(place);
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
        if (homeInstance != null)
            Destroy(homeInstance);
    }
}