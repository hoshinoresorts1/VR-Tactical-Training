using UnityEngine;
using System.Collections.Generic;

public class MineGrid : MonoBehaviour
{
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private int mineCount = 10;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Material revealedMaterial;
    [SerializeField] private Material unrevealedMaterial;
    [SerializeField] private Transform gridParent;
    [SerializeField] private Transform gridOrigin;
    [SerializeField] private Vector2Int startCell = new Vector2Int(0, 0);
    [SerializeField] private Vector2Int goalCell = new Vector2Int(9, 9);
    [SerializeField] private Transform playerSpawn;
    [SerializeField] private GameObject minePrefab;
    [SerializeField] private GameObject decorationPrefab;
    [SerializeField] private int decorationCount = 5;
    [SerializeField] private GameObject startMarkerPrefab;
    [SerializeField] private GameObject goalMarkerPrefab;

    private MineCell[,] grid;
    private bool gameActive = true;
    private GameObject startMarkerObject;
    private GameObject goalMarkerObject;
    private readonly List<GameObject> decorationObjects = new List<GameObject>();

    private void Start()
    {
        InitializeGrid();
    }

    public void InitializeGrid()
    {
        if (grid != null)
        {
            foreach (var cell in grid)
            {
                if (cell != null && cell.cellVisualObject != null)
                {
                    Destroy(cell.cellVisualObject);
                }
            }
        }

        grid = new MineCell[gridWidth, gridHeight];
        gameActive = true;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y] = new MineCell(x, y);
            }
        }

        PlaceMinesRandomly();
        CalculateAdjacentMines();
        CreateGridVisuals();
        PlaceStartGoalMarkers();
        SpawnPlayerAtStart();
        PlaceDecorationObjects();
    }

    private void PlaceStartGoalMarkers()
    {
        if (!IsValidPosition(startCell.x, startCell.y) || !IsValidPosition(goalCell.x, goalCell.y))
            return;

        if (startMarkerObject != null)
            Destroy(startMarkerObject);
        if (goalMarkerObject != null)
            Destroy(goalMarkerObject);

        Vector3 origin = gridOrigin != null ? gridOrigin.position : transform.position;
        Vector3 startPos = origin + new Vector3(startCell.x * cellSize, 0.15f, startCell.y * cellSize);
        Vector3 goalPos = origin + new Vector3(goalCell.x * cellSize, 0.15f, goalCell.y * cellSize);

        startMarkerObject = CreateMarker(startPos, "StartMarker", startMarkerPrefab, Color.green, false);
        goalMarkerObject = CreateMarker(goalPos, "GoalMarker", goalMarkerPrefab, Color.yellow, true);

        MineGoal goalComponent = goalMarkerObject.GetComponent<MineGoal>();
        if (goalComponent == null)
        {
            goalComponent = goalMarkerObject.AddComponent<MineGoal>();
        }
        goalComponent.mineGrid = this;
    }

    private GameObject CreateMarker(Vector3 position, string name, GameObject prefab, Color color, bool isTrigger)
    {
        GameObject marker;
        if (prefab != null)
        {
            marker = Instantiate(prefab, position, Quaternion.identity, gridParent != null ? gridParent : transform);
            marker.name = name;
        }
        else
        {
            marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = name;
            marker.transform.position = position;
            marker.transform.localScale = new Vector3(cellSize * 0.8f, 0.2f, cellSize * 0.8f);
            marker.transform.SetParent(gridParent != null ? gridParent : transform);
            Renderer renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }
        }

        Collider collider = marker.GetComponent<Collider>();
        if (collider == null)
        {
            collider = marker.AddComponent<BoxCollider>();
        }
        collider.isTrigger = isTrigger;
        return marker;
    }

    private void SpawnPlayerAtStart()
    {
        if (playerSpawn == null)
            return;

        if (!IsValidPosition(startCell.x, startCell.y))
            return;

        Vector3 origin = gridOrigin != null ? gridOrigin.position : transform.position;
        Vector3 spawnPosition = origin + new Vector3(startCell.x * cellSize, 0.5f, startCell.y * cellSize);
        playerSpawn.position = spawnPosition;
    }

    private void PlaceDecorationObjects()
    {
        foreach (var obj in decorationObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        decorationObjects.Clear();

        if (decorationPrefab == null || decorationCount <= 0)
            return;

        Vector3 origin = gridOrigin != null ? gridOrigin.position : transform.position;
        int tries = 0;

        while (decorationObjects.Count < decorationCount && tries < decorationCount * 4)
        {
            int x = Random.Range(0, gridWidth);
            int y = Random.Range(0, gridHeight);
            tries++;

            if (!IsValidPosition(x, y))
                continue;

            if (new Vector2Int(x, y) == startCell || new Vector2Int(x, y) == goalCell)
                continue;

            if (grid[x, y].hasMine)
                continue;

            Vector3 position = origin + new Vector3(x * cellSize, 0.05f, y * cellSize);
            GameObject deco = Instantiate(decorationPrefab, position, Quaternion.identity, gridParent != null ? gridParent : transform);
            deco.name = $"Decoration_{x}_{y}";
            decorationObjects.Add(deco);
        }
    }

    private void PlaceMinesRandomly()
    {
        int minesPlaced = 0;
        while (minesPlaced < mineCount)
        {
            int randomX = Random.Range(0, gridWidth);
            int randomY = Random.Range(0, gridHeight);

            if (!grid[randomX, randomY].hasMine)
            {
                grid[randomX, randomY].hasMine = true;
                minesPlaced++;
            }
        }
    }

    private void CalculateAdjacentMines()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (!grid[x, y].hasMine)
                {
                    int count = 0;
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int newX = x + dx;
                            int newY = y + dy;

                            if (IsValidPosition(newX, newY) && grid[newX, newY].hasMine)
                            {
                                count++;
                            }
                        }
                    }
                    grid[x, y].adjacentMineCount = count;
                }
            }
        }
    }

    private void CreateGridVisuals()
    {
        Vector3 origin = gridOrigin != null ? gridOrigin.position : transform.position;
        if (gridParent == null)
        {
            gridParent = transform;
        }

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 position = origin + new Vector3(x * cellSize, 0.05f, y * cellSize);
                GameObject cell = new GameObject($"Cell_{x}_{y}");
                cell.transform.position = position;
                cell.transform.parent = gridParent;

                GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visual.transform.parent = cell.transform;
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localScale = new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f);

                Renderer renderer = visual.GetComponent<Renderer>();
                if (renderer != null && unrevealedMaterial != null)
                {
                    renderer.material = unrevealedMaterial;
                }

                Collider collider = visual.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.isTrigger = true;
                }

                        MineCell cellData = grid[x, y];
                cellData.cellVisualObject = cell;
                cellData.revealedMaterial = revealedMaterial;
                cellData.unrevealedMaterial = unrevealedMaterial;

                if (cellData.hasMine && minePrefab != null)
                {
                    GameObject mineObject = Instantiate(minePrefab, cell.transform);
                    mineObject.transform.localPosition = Vector3.up * 0.05f;
                    mineObject.transform.localRotation = Quaternion.identity;
                    mineObject.transform.localScale = Vector3.one * 0.5f;
                    mineObject.SetActive(false);
                    cellData.mineObject = mineObject;
                }

                MineCellVisual visualComponent = cell.AddComponent<MineCellVisual>();
                visualComponent.Initialize(cellData);

                MineCellInteraction interaction = visual.AddComponent<MineCellInteraction>();
                interaction.Initialize(this, cellData);
            }
        }
    }

    public void OnCellClicked(int x, int y)
    {
        if (!gameActive || !IsValidPosition(x, y))
            return;

        MineCell clickedCell = grid[x, y];
        if (clickedCell.isRevealed)
            return;

        if (clickedCell.hasMine)
        {
            OnPlayerDied();
            return;
        }

        FloodFillReveal(x, y);
        CheckGameCompletion();
    }

    public void OnPlayerSteppedMine()
    {
        if (!gameActive)
            return;

        OnPlayerDied();
    }

    private void OnPlayerDied()
    {
        gameActive = false;
        Debug.Log("지뢰를 밟았습니다! 게임 오버");
        RevealAllMines();
    }

    public void OnGoalReached()
    {
        if (!gameActive)
            return;

        gameActive = false;
        Debug.Log("목적지 도달! 훈련 성공");
    }

    private void FloodFillReveal(int x, int y)
    {
        Queue<(int, int)> queue = new Queue<(int, int)>();
        HashSet<(int, int)> visited = new HashSet<(int, int)>();

        queue.Enqueue((x, y));
        visited.Add((x, y));

        while (queue.Count > 0)
        {
            var (cx, cy) = queue.Dequeue();

            if (!IsValidPosition(cx, cy))
                continue;

            MineCell cell = grid[cx, cy];
            if (cell.isRevealed)
                continue;

            cell.Reveal();

            if (cell.adjacentMineCount == 0 && !cell.hasMine)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int newX = cx + dx;
                        int newY = cy + dy;
                        if (IsValidPosition(newX, newY) && !visited.Contains((newX, newY)))
                        {
                            visited.Add((newX, newY));
                            queue.Enqueue((newX, newY));
                        }
                    }
                }
            }
        }
    }

    private void RevealAllMines()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                MineCell cell = grid[x, y];
                if (cell.hasMine)
                {
                    cell.Reveal();
                }
            }
        }
    }

    private void CheckGameCompletion()
    {
        int revealedCount = 0;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y].isRevealed)
                    revealedCount++;
            }
        }

        int totalSafeCells = (gridWidth * gridHeight) - mineCount;
        if (revealedCount == totalSafeCells)
        {
            gameActive = false;
            Debug.Log("게임 완료! 모든 안전 영역을 공개했습니다.");
        }
    }

    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }

    public MineCell GetCell(int x, int y)
    {
        return IsValidPosition(x, y) ? grid[x, y] : null;
    }

    public void ResetGame()
    {
        InitializeGrid();
    }

    public int GetGridWidth() => gridWidth;
    public int GetGridHeight() => gridHeight;
    public bool IsGameActive() => gameActive;
}
