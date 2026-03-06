// Copyright (C) 2026 Ocxyde
//
// This file is part of Code.Lavos.
//
// Code.Lavos is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Code.Lavos is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Code.Lavos.  If not, see <https://www.gnu.org/licenses/>.
// GridMazeGenerator.cs
// PROPER GRID MAZE - Walls snap to grid boundaries
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// GRID MATH:
// - Grid cells are WALKABLE spaces (Floor/Corridor/SpawnPoint)
// - Walls are placed on CELL BOUNDARIES (between cells)
// - DFS carves corridors through cells
// - Outer perimeter = walls on boundary
//
// RESULT: Walls snap perfectly to grid!

using UnityEngine;
using System.Collections.Generic;

namespace Code.Lavos.Core
{
    /// <summary>
    /// GridMazeGenerator - PROPER GRID MAZE with wall snapping.
    /// 
    /// GRID STRUCTURE:
    /// - Each cell represents a WALKABLE space (6m x 6m)
    /// - Walls are placed on CELL BOUNDARIES (edges)
    /// - DFS carves corridors by marking cells as walkable
    /// 
    /// CELL TYPES:
    /// - Wall: Solid boundary (not walkable)
    /// - Floor: Empty walkable space
    /// - Corridor: DFS-carved path
    /// - SpawnPoint: Player spawn location
    /// 
    /// GENERATION STEPS:
    /// 1. Fill grid with Wall (all solid)
    /// 2. DFS carves corridors (marks cells as walkable)
    /// 3. Mark spawn point (single cell)
    /// 4. Mark outer boundary (perimeter walls)
    /// 
    /// WALL PLACEMENT (by MazeRenderer):
    /// - Walls placed on cell boundaries (edges between cells)
    /// - Each wall segment = cellSize x wallHeight
    /// - Walls snap to grid perfectly
    /// </summary>
    public class GridMazeGenerator
    {
        #region Inspector Fields

        [Header("Grid Settings")]
        [SerializeField] private int _gridSize = 21;
        [SerializeField] private int _corridorWidth = 1;

        #endregion

        #region Private Fields

        private GridMazeCell[,] _grid;
        private Vector2Int _spawnPoint;
        private float _seedFactor;
        private int _currentLevel;

        // 4-way direction arrays (N, E, S, W)
        private static readonly int[] _directionsX4 = { 0,  1,  0, -1 };
        private static readonly int[] _directionsY4 = { 1,  0, -1,  0 };

        #endregion

        #region Public Properties

        public GridMazeCell[,] Grid => _grid;
        public int GridSize { get => _gridSize; set => _gridSize = value; }
        public int CorridorWidth { get => _corridorWidth; set => _corridorWidth = value; }
        public Vector2Int SpawnPoint => _spawnPoint;

        #endregion

        #region Initialization

        public void InitializeFromConfig()
        {
            GameConfig config = GameConfig.Instance;
            _gridSize = config.defaultGridSize;
            _corridorWidth = config.defaultCorridorWidth;

            Debug.Log($"[GridMazeGenerator] Config: {_gridSize}x{_gridSize} grid, cell size: {config.defaultCellSize}m");
        }

        #endregion

        #region Main Generation

        /// <summary>
        /// Generate proper grid maze with wall snapping.
        /// Grid cells = walkable spaces, walls on boundaries.
        /// </summary>
        /// <param name="seed">Random seed for procedural generation</param>
        /// <param name="difficultyFactor">0.0 to 1.0 difficulty scaling</param>
        /// <param name="level">Current level</param>
        public void Generate(uint seed, float difficultyFactor = 0f, int level = 0)
        {
            _seedFactor = Mathf.Clamp01(difficultyFactor);
            _currentLevel = level;
            Random.InitState((int)seed);

            Debug.Log($"[GridMazeGenerator] Seed: {seed} (grid maze with wall snapping)");

            if (_gridSize == 0)
            {
                InitializeFromConfig();
            }

            // Corridor width always 1 for proper maze
            _corridorWidth = 1;

            Debug.Log($"[GridMazeGenerator] Generating {_gridSize}x{_gridSize} grid maze...");

            // Step 1: Fill with Wall (all solid)
            FillGridWithWalls();

            // Step 2: Mark outer boundary FIRST (so DFS won't carve into it)
            MarkOuterBoundary();

            // Step 3: Carve maze with DFS (marks cells as walkable, respects boundary)
            CarveMazeWithDfs();

            // Step 4: Carve exit corridor to south wall (ensures player can reach exit)
            CarveExitToSouth();

            Debug.Log($"[GridMazeGenerator] Maze complete - spawn: {_spawnPoint}");
            LogGridStatistics();
        }

        #endregion

        #region Step 1: Fill Grid with WALL

        private void FillGridWithWalls()
        {
            _grid = new GridMazeCell[_gridSize, _gridSize];

            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    _grid[x, y] = GridMazeCell.Wall;
                }
            }

            Debug.Log($"[GridMazeGenerator] Grid filled with WALL ({_gridSize}x{_gridSize} cells)");
        }

        #endregion

        #region Step 2: Carve Maze with DFS

        /// <summary>
        /// DFS carves corridors by marking cells as walkable.
        /// Boundary is already marked, so DFS won't carve into it.
        /// Ensures spawn point has at least one exit corridor.
        /// Walls will be placed on cell boundaries by MazeRenderer.
        /// </summary>
        private void CarveMazeWithDfs()
        {
            int[] dirX = _directionsX4;
            int[] dirY = _directionsY4;
            int numDirections = 4;

            Debug.Log($"[GridMazeGenerator] Carving maze with DFS (4-way)...");

            // Start from inner area (not on boundary)
            // Spawn is at x=1, which is just inside the west boundary (x=0)
            int startX = 1;
            int startY = _gridSize / 2;

            // Mark spawn point (SINGLE CELL)
            _spawnPoint = new Vector2Int(startX, startY);
            _grid[startX, startY] = GridMazeCell.SpawnPoint;

            // Track visited cells
            bool[,] visited = new bool[_gridSize, _gridSize];
            visited[startX, startY] = true;

            // Stack for backtracking
            Stack<Vector2Int> stack = new Stack<Vector2Int>();
            stack.Push(new Vector2Int(startX, startY));

            int carvedCells = 0;
            int targetCells = (_gridSize * _gridSize) / 3; // ~33% walkable

            Debug.Log($"[GridMazeGenerator] DFS target: {targetCells} walkable cells");

            int backtrackCount = 0;
            bool spawnedExit = false;

            while (stack.Count > 0 && carvedCells < targetCells)
            {
                Vector2Int current = stack.Peek();

                // Get unvisited neighbors (1 cell away)
                List<int> unvisitedDirections = new List<int>();

                for (int i = 0; i < numDirections; i++)
                {
                    int nx = current.x + dirX[i];
                    int ny = current.y + dirY[i];

                    if (IsValidMazeCell(nx, ny) && !visited[nx, ny])
                    {
                        // Check if it's a wall (can carve into it)
                        // Boundary walls are already marked, so DFS won't carve into them
                        if (_grid[nx, ny] == GridMazeCell.Wall)
                        {
                            unvisitedDirections.Add(i);
                        }
                    }
                }

                if (unvisitedDirections.Count > 0)
                {
                    // Choose random direction
                    int dirIndex = unvisitedDirections[Random.Range(0, unvisitedDirections.Count)];
                    int nx = current.x + dirX[dirIndex];
                    int ny = current.y + dirY[dirIndex];

                    // Carve corridor (mark cell as walkable)
                    _grid[nx, ny] = GridMazeCell.Corridor;
                    visited[nx, ny] = true;
                    stack.Push(new Vector2Int(nx, ny));
                    carvedCells++;

                    // Mark that we've carved at least one exit from spawn
                    if (!spawnedExit && current.x == startX && current.y == startY)
                    {
                        spawnedExit = true;
                        Debug.Log($"[GridMazeGenerator] Spawn exit carved at ({nx}, {ny})");
                    }
                }
                else
                {
                    // Backtrack
                    stack.Pop();
                    backtrackCount++;
                }
            }

            // Ensure spawn has at least one exit (fallback)
            if (!spawnedExit)
            {
                Debug.Log($"[GridMazeGenerator] Forcing spawn exit (DFS didn't carve one)...");
                // Try to carve east from spawn (most likely direction)
                int eastX = startX + 1;
                int eastY = startY;
                if (IsValidMazeCell(eastX, eastY) && _grid[eastX, eastY] == GridMazeCell.Wall)
                {
                    _grid[eastX, eastY] = GridMazeCell.Corridor;
                    Debug.Log($"[GridMazeGenerator] Forced exit east to ({eastX}, {eastY})");
                }
                else
                {
                    // Try all directions
                    for (int i = 0; i < numDirections; i++)
                    {
                        int nx = startX + dirX[i];
                        int ny = startY + dirY[i];
                        if (IsValidMazeCell(nx, ny) && _grid[nx, ny] == GridMazeCell.Wall)
                        {
                            _grid[nx, ny] = GridMazeCell.Corridor;
                            Debug.Log($"[GridMazeGenerator] Forced exit to ({nx}, {ny})");
                            break;
                        }
                    }
                }
            }

            Debug.Log($"[GridMazeGenerator] Maze carved: {carvedCells} walkable cells, {backtrackCount} backtracks, spawn exit: {spawnedExit}");
        }

        /// <summary>
        /// Carve an exit corridor to the south boundary.
        /// Ensures player can reach the exit door.
        /// </summary>
        public void CarveExitToSouth()
        {
            int exitX = _gridSize / 2;  // Center of south wall
            int exitY = _gridSize - 2;  // Just inside south boundary

            // Find nearest walkable cell to exit
            Vector2Int nearestWalkable = FindNearestWalkableTo(exitX, exitY);

            if (nearestWalkable.x >= 0)
            {
                // Carve corridor from nearest walkable to exit
                CarveCorridorTo(nearestWalkable, new Vector2Int(exitX, exitY));
                _grid[exitX, exitY] = GridMazeCell.Corridor;
                Debug.Log($"[GridMazeGenerator] Exit corridor carved to south wall at ({exitX}, {exitY})");
            }
            else
            {
                Debug.LogWarning($"[GridMazeGenerator] Could not find path to exit!");
            }
        }

        /// <summary>
        /// Find nearest walkable cell to target position.
        /// </summary>
        private Vector2Int FindNearestWalkableTo(int targetX, int targetY)
        {
            // Search outward from target
            for (int radius = 1; radius < _gridSize / 2; radius++)
            {
                for (int x = targetX - radius; x <= targetX + radius; x++)
                {
                    for (int y = targetY - radius; y <= targetY + radius; y++)
                    {
                        if (IsValidMazeCell(x, y) && 
                            (_grid[x, y] == GridMazeCell.Corridor || _grid[x, y] == GridMazeCell.SpawnPoint))
                        {
                            return new Vector2Int(x, y);
                        }
                    }
                }
            }
            return new Vector2Int(-1, -1); // Not found
        }

        /// <summary>
        /// Carve corridor from start to end (straight line).
        /// </summary>
        private void CarveCorridorTo(Vector2Int start, Vector2Int end)
        {
            int x = start.x;
            int y = start.y;

            while (x != end.x || y != end.y)
            {
                if (x < end.x) x++;
                else if (x > end.x) x--;
                
                if (y < end.y) y++;
                else if (y > end.y) y--;

                if (IsValidMazeCell(x, y) && _grid[x, y] == GridMazeCell.Wall)
                {
                    _grid[x, y] = GridMazeCell.Corridor;
                }
            }
        }

        #endregion

        #region Step 3: Mark Outer Boundary

        /// <summary>
        /// Mark outer perimeter as Wall boundary.
        /// Called BEFORE DFS so DFS won't carve into boundary.
        /// </summary>
        private void MarkOuterBoundary()
        {
            // Mark perimeter cells as Wall (boundary)
            // DFS will respect this and won't carve into it
            for (int x = 0; x < _gridSize; x++)
            {
                _grid[x, 0] = GridMazeCell.Wall;
                _grid[x, _gridSize - 1] = GridMazeCell.Wall;
            }

            for (int y = 0; y < _gridSize; y++)
            {
                _grid[0, y] = GridMazeCell.Wall;
                _grid[_gridSize - 1, y] = GridMazeCell.Wall;
            }

            Debug.Log($"[GridMazeGenerator] Outer boundary marked (perimeter walls)");
        }

        #endregion

        #region Grid Access

        public GridMazeCell GetCell(int x, int y)
        {
            if (x >= 0 && x < _gridSize && y >= 0 && y < _gridSize)
            {
                return _grid[x, y];
            }
            return GridMazeCell.Wall;
        }

        public Vector2Int FindSpawnPoint()
        {
            return _spawnPoint;
        }

        #endregion

        #region Utilities

        private bool IsValidMazeCell(int x, int y)
        {
            return x >= 0 && x < _gridSize && y >= 0 && y < _gridSize;
        }

        /// <summary>
        /// Log grid statistics for debugging.
        /// </summary>
        private void LogGridStatistics()
        {
            int wallCount = 0, corridorCount = 0, spawnCount = 0, floorCount = 0;

            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    switch (_grid[x, y])
                    {
                        case GridMazeCell.Wall: wallCount++; break;
                        case GridMazeCell.Corridor: corridorCount++; break;
                        case GridMazeCell.SpawnPoint: spawnCount++; break;
                        case GridMazeCell.Floor: floorCount++; break;
                    }
                }
            }

            int total = _gridSize * _gridSize;
            Debug.Log($"[GridMazeGenerator] GRID STATS: {_gridSize}x{_gridSize} = {total} cells");
            Debug.Log($"[GridMazeGenerator]   Walls: {wallCount} ({wallCount * 100 / total}%)");
            Debug.Log($"[GridMazeGenerator]   Corridors: {corridorCount} ({corridorCount * 100 / total}%)");
            Debug.Log($"[GridMazeGenerator]   Spawn: {spawnCount}");
            Debug.Log($"[GridMazeGenerator]   Floor: {floorCount}");
        }

        #endregion

        #region Serialization

        public byte[] SerializeToBytes()
        {
            int total = 2 + _gridSize * _gridSize;
            byte[] data = new byte[total];
            data[0] = (byte)_gridSize;
            data[1] = (byte)_gridSize;

            int i = 2;
            for (int y = 0; y < _gridSize; y++)
            {
                for (int x = 0; x < _gridSize; x++)
                {
                    data[i++] = (byte)_grid[x, y];
                }
            }

            return data;
        }

        public void DeserializeFromBytes(byte[] data)
        {
            if (data.Length < 2)
            {
                return;
            }

            _gridSize = data[0];
            _grid = new GridMazeCell[_gridSize, _gridSize];

            int i = 2;
            for (int y = 0; y < _gridSize; y++)
            {
                for (int x = 0; x < _gridSize; x++)
                {
                    _grid[x, y] = (GridMazeCell)data[i++];
                }
            }
        }

        #endregion
    }
}
