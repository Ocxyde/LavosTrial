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
// PURE MAZE generation - NO rooms, just corridors and walls
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// GENERATION APPROACH:
// 1. Fill grid with WALL (all solid)
// 2. DFS carves 1-cell corridors (proper maze)
// 3. Mark single SpawnPoint cell (not a room!)
// 4. Mark outer walls (perimeter solid)
//
// RESULT: Pure maze with walls between corridors, single spawn cell!

using UnityEngine;
using System.Collections.Generic;

namespace Code.Lavos.Core
{
    /// <summary>
    /// GridMazeGenerator - PURE MAZE generation.
    /// Creates proper maze structure with walls between corridors.
    /// NO rooms - just corridors and a single SpawnPoint cell.
    ///
    /// GENERATION STEPS:
    /// 1. Fill grid with WALL (not Floor!)
    /// 2. DFS from spawn point, carving 1-cell corridors
    /// 3. Mark single SpawnPoint cell (west edge)
    /// 4. Mark outer walls (perimeter solid)
    ///
    /// DIFFICULTY PROGRESSION:
    /// - Maze size increases with level (from CompleteMazeBuilder)
    /// - Seed affects randomness in DFS carving
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

            Debug.Log($"[GridMazeGenerator] Config: {_gridSize}x{_gridSize} grid");
        }

        #endregion

        #region Main Generation

        /// <summary>
        /// Generate pure maze with specified seed and difficulty.
        /// NO rooms - just corridors and a single SpawnPoint cell.
        /// </summary>
        /// <param name="seed">Random seed for procedural generation</param>
        /// <param name="difficultyFactor">0.0 to 1.0 difficulty scaling (unused, kept for API)</param>
        /// <param name="level">Current level (unused, kept for API)</param>
        public void Generate(uint seed, float difficultyFactor = 0f, int level = 0)
        {
            _seedFactor = Mathf.Clamp01(difficultyFactor);
            _currentLevel = level;
            Random.InitState((int)seed);

            Debug.Log($"[GridMazeGenerator] Seed: {seed} (pure maze - no rooms)");

            if (_gridSize == 0)
            {
                InitializeFromConfig();
            }

            // Corridor width always 1 for proper maze
            _corridorWidth = 1;

            Debug.Log($"[GridMazeGenerator] Generating {_gridSize}x{_gridSize} pure maze...");

            // Step 1: Fill with WALL (not Floor!)
            FillGridWithWalls();

            // Step 2: Carve maze with DFS
            CarveMazeWithDfs();

            // Step 3: Mark outer walls (keep perimeter solid)
            MarkOuterWalls();

            Debug.Log($"[GridMazeGenerator] Maze complete - spawn: {_spawnPoint}");
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

            Debug.Log($"[GridMazeGenerator] Grid filled with WALL (ready for carving)");
        }

        #endregion

        #region Step 2: Carve Maze with DFS

        private void CarveMazeWithDfs()
        {
            int[] dirX = _directionsX4;
            int[] dirY = _directionsY4;
            int numDirections = 4;

            Debug.Log($"[GridMazeGenerator] Carving maze with 4-way DFS...");

            // Start from west edge (spawn area)
            int startX = 1;
            int startY = _gridSize / 2;

            // Mark spawn point (SINGLE CELL, not a room!)
            _spawnPoint = new Vector2Int(startX, startY);
            _grid[startX, startY] = GridMazeCell.SpawnPoint;

            // Track visited cells
            bool[,] visited = new bool[_gridSize, _gridSize];
            visited[startX, startY] = true;

            // Stack for backtracking
            Stack<Vector2Int> stack = new Stack<Vector2Int>();
            stack.Push(new Vector2Int(startX, startY));

            int carvedCells = 0;
            int targetCells = (_gridSize * _gridSize) / 3; // ~33% of grid

            Debug.Log($"[GridMazeGenerator] DFS target: {targetCells} cells");

            int backtrackCount = 0;
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

                    // Carve corridor
                    _grid[nx, ny] = GridMazeCell.Corridor;
                    visited[nx, ny] = true;
                    stack.Push(new Vector2Int(nx, ny));
                    carvedCells++;
                }
                else
                {
                    // Backtrack
                    stack.Pop();
                    backtrackCount++;
                }
            }

            Debug.Log($"[GridMazeGenerator] Maze carved: {carvedCells} corridor cells, {backtrackCount} backtracks");
        }

        #endregion

        #region Step 3: Mark Outer Walls

        private void MarkOuterWalls()
        {
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

            Debug.Log($"[GridMazeGenerator] Outer walls marked (perimeter solid)");
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
