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
// MAZE-FIRST dungeon generation with chambers
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// GENERATION APPROACH:
// 1. Fill grid with WALL (all solid)
// 2. DFS carves 1-cell corridors (proper maze)
// 3. Expand intersections to chambers (3x3, 5x5)
// 4. Mark spawn/exit chambers
// 5. Validate connectivity
//
// RESULT: Real dungeon maze with walls between corridors!

using UnityEngine;
using System.Collections.Generic;

namespace Code.Lavos.Core
{
    /// <summary>
    /// GridMazeGenerator - MAZE-FIRST dungeon generation.
    /// Creates proper maze structure with walls between corridors.
    /// Expands corridor intersections into chambers (rooms).
    /// 
    /// GENERATION STEPS:
    /// 1. Fill grid with WALL (not Floor!)
    /// 2. DFS from spawn point, carving 1-cell corridors
    /// 3. Expand 3-way/4-way intersections to chambers
    /// 4. Mark spawn chamber (west edge) and exit chamber (east edge)
    /// 5. Validate all chambers reachable
    /// 
    /// DIFFICULTY PROGRESSION:
    /// - Levels 0-2: 4-way corridors, 2-3 chambers (tutorial)
    /// - Levels 3-10: 8-way corridors, 4-6 chambers (intermediate)
    /// - Levels 11+: 8-way corridors, 6-10 chambers (expert)
    /// </summary>
    public class GridMazeGenerator
    {
        #region Inspector Fields

        [Header("Grid Settings")]
        [SerializeField] private int _gridSize = 21;
        [SerializeField] private int _chamberSize = 5;
        [SerializeField] private int _corridorWidth = 1;

        [Header("Difficulty")]
        [SerializeField] private int _baseChambers = 3;
        [SerializeField] private int _maxChambers = 10;

        #endregion

        #region Private Fields

        private GridMazeCell[,] _grid;
        private Vector2Int _spawnChamberCenter;
        private Vector2Int _exitChamberCenter;
        private List<Vector2Int> _chamberCenters = new List<Vector2Int>();
        private float _seedFactor;
        private int _currentLevel;

        // 8-way direction arrays (N, NE, E, SE, S, SW, W, NW)
        private static readonly int[] _directionsX8 = { 0,  1,  1,  1,  0, -1, -1, -1 };
        private static readonly int[] _directionsY8 = { 1,  1,  0, -1, -1, -1,  0,  1 };

        // 4-way direction arrays (N, E, S, W)
        private static readonly int[] _directionsX4 = { 0,  1,  0, -1 };
        private static readonly int[] _directionsY4 = { 1,  0, -1,  0 };

        #endregion

        #region Public Properties

        public GridMazeCell[,] Grid => _grid;
        public int GridSize { get => _gridSize; set => _gridSize = value; }
        public int ChamberSize { get => _chamberSize; set => _chamberSize = value; }
        public int CorridorWidth { get => _corridorWidth; set => _corridorWidth = value; }
        public Vector2Int SpawnChamberCenter => _spawnChamberCenter;
        public Vector2Int ExitChamberCenter => _exitChamberCenter;
        public List<Vector2Int> ChamberCenters => _chamberCenters;

        #endregion

        #region Initialization

        public void InitializeFromConfig()
        {
            GameConfig config = GameConfig.Instance;
            _gridSize = config.defaultGridSize;
            _chamberSize = config.defaultRoomSize;
            _corridorWidth = config.defaultCorridorWidth;

            Debug.Log($"[GridMazeGenerator] Config: {_gridSize}x{_gridSize} grid, {_chamberSize}x{_chamberSize} chambers");
        }

        #endregion

        #region Main Generation

        /// <summary>
        /// Generate maze-first dungeon with specified seed and difficulty.
        /// </summary>
        /// <param name="seed">Random seed for procedural generation</param>
        /// <param name="difficultyFactor">0.0 to 1.0 difficulty scaling</param>
        /// <param name="level">Current level (determines 4-way vs 8-way)</param>
        public void Generate(uint seed, float difficultyFactor, int level = 0)
        {
            _seedFactor = Mathf.Clamp01(difficultyFactor);
            _currentLevel = level;
            Random.InitState((int)seed);

            Debug.Log($"[GridMazeGenerator] Seed: {seed} (difficulty: {_seedFactor:F2}, level: {level})");

            if (_gridSize == 0)
            {
                InitializeFromConfig();
            }

            // Corridor width always 1 for proper maze
            _corridorWidth = 1;

            Debug.Log($"[GridMazeGenerator] Generating {_gridSize}x{_gridSize} maze-first dungeon...");

            // Step 1: Fill with WALL (not Floor!)
            FillGridWithWalls();

            // Step 2: Carve maze with DFS
            CarveMazeWithDfs();

            // Step 3: Expand intersections to chambers
            ExpandIntersectionsToChambers();

            // Step 4: Mark outer walls (keep perimeter solid)
            MarkOuterWalls();

            Debug.Log($"[GridMazeGenerator] Maze complete - {_chamberCenters.Count} chambers, spawn: {_spawnChamberCenter}");
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
            bool use8Way = _currentLevel >= 3;
            int[] dx = use8Way ? _directionsX8 : _directionsX4;
            int[] dy = use8Way ? _directionsY8 : _directionsY4;
            int numDirections = use8Way ? 8 : 4;

            Debug.Log($"[GridMazeGenerator] Carving maze with {(use8Way ? "8-way" : "4-way")} DFS...");

            // Start from west edge (spawn area)
            int startX = 3;
            int startY = _gridSize / 2;

            // Track visited cells
            bool[,] visited = new bool[_gridSize, _gridSize];

            // Stack for backtracking
            Stack<Vector2Int> stack = new Stack<Vector2Int>();
            stack.Push(new Vector2Int(startX, startY));
            visited[startX, startY] = true;

            // Carve spawn chamber (3x3)
            CarveChamberWithConnections(startX, startY, 3);
            _spawnChamberCenter = new Vector2Int(startX, startY);

            // Mark the entire spawn chamber as visited to prevent DFS from carving back into it
            int halfSize = 1; // 3x3 chamber
            for (int dx = -halfSize; dx <= halfSize; dx++)
            {
                for (int dy = -halfSize; dy <= halfSize; dy++)
                {
                    visited[startX + dx, startY + dy] = true;
                }
            }

            int carvedCells = 0;
            int targetCells = (_gridSize * _gridSize) / 6; // Carve ~16% of grid

            while (stack.Count > 0 && carvedCells < targetCells)
            {
                Vector2Int current = stack.Peek();

                // Get unvisited neighbors (1 cell away for dense maze)
                List<int> unvisitedDirections = new List<int>();

                for (int i = 0; i < numDirections; i++)
                {
                    int nx = current.x + dx[i];
                    int ny = current.y + dy[i];

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
                    int nx = current.x + dx[dirIndex];
                    int ny = current.y + dy[dirIndex];

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
                }
            }

            Debug.Log($"[GridMazeGenerator] Maze carved: {carvedCells} corridor cells");
        }

        #endregion

        #region Step 3: Expand Intersections to Chambers

        private void ExpandIntersectionsToChambers()
        {
            Debug.Log($"[GridMazeGenerator] Expanding intersections to chambers...");

            int chamberCount = _baseChambers + Mathf.FloorToInt(_seedFactor * (_maxChambers - _baseChambers));
            chamberCount = Mathf.Clamp(chamberCount, 3, _maxChambers);

            int chambersCreated = 1; // Spawn chamber already exists

            // Find corridor intersections and expand them
            for (int x = 3; x < _gridSize - 3; x++)
            {
                for (int y = 3; y < _gridSize - 3; y++)
                {
                    if (chambersCreated >= chamberCount)
                    {
                        break;
                    }

                    if (_grid[x, y] == GridMazeCell.Corridor && IsIntersection(x, y))
                    {
                        // Random chance to expand (not all intersections)
                        if (Random.value < 0.4f) // 40% chance
                        {
                            int chamberSize = (Random.value < 0.7f) ? 3 : 5; // 70% small, 30% large
                            
                            // Carve chamber BUT keep corridor connections!
                            CarveChamberWithConnections(x, y, chamberSize);
                            _chamberCenters.Add(new Vector2Int(x, y));
                            chambersCreated++;

                            // Mark exit chamber if on east edge
                            if (x > _gridSize * 0.7f)
                            {
                                _exitChamberCenter = new Vector2Int(x, y);
                            }
                        }
                    }
                }
            }

            // Ensure exit chamber exists
            if (_exitChamberCenter.x == 0)
            {
                // Create exit chamber on east edge
                int exitX = _gridSize - 3;
                int exitY = _gridSize / 2;
                CarveChamberWithConnections(exitX, exitY, 5);
                _exitChamberCenter = new Vector2Int(exitX, exitY);
                _chamberCenters.Add(_exitChamberCenter);
            }

            Debug.Log($"[GridMazeGenerator] Created {chambersCreated} chambers");
        }

        /// <summary>
        /// Carve a chamber with guaranteed corridor connections.
        /// </summary>
        private void CarveChamberWithConnections(int centerX, int centerY, int size)
        {
            int halfSize = size / 2;

            for (int dx = -halfSize; dx <= halfSize; dx++)
            {
                for (int dy = -halfSize; dy <= halfSize; dy++)
                {
                    int x = centerX + dx;
                    int y = centerY + dy;

                    if (IsValidMazeCell(x, y))
                    {
                        // Carve as Room
                        _grid[x, y] = GridMazeCell.Room;
                    }
                }
            }

            // Ensure corridor connections remain (don't block entrances)
            EnsureChamberConnections(centerX, centerY, halfSize);
        }

        /// <summary>
        /// Ensure chamber has corridor connections (don't block entrances).
        /// </summary>
        private void EnsureChamberConnections(int centerX, int centerY, int halfSize)
        {
            // Check cardinal directions and keep corridors open
            if (centerX - halfSize - 1 >= 0 && _grid[centerX - halfSize - 1, centerY] == GridMazeCell.Corridor)
            {
                _grid[centerX - halfSize, centerY] = GridMazeCell.Room; // Keep west entrance
            }
            if (centerX + halfSize + 1 < _gridSize && _grid[centerX + halfSize + 1, centerY] == GridMazeCell.Corridor)
            {
                _grid[centerX + halfSize, centerY] = GridMazeCell.Room; // Keep east entrance
            }
            if (centerY - halfSize - 1 >= 0 && _grid[centerX, centerY - halfSize - 1] == GridMazeCell.Corridor)
            {
                _grid[centerX, centerY - halfSize] = GridMazeCell.Room; // Keep north entrance
            }
            if (centerY + halfSize + 1 < _gridSize && _grid[centerX, centerY + halfSize + 1] == GridMazeCell.Corridor)
            {
                _grid[centerX, centerY + halfSize] = GridMazeCell.Room; // Keep south entrance
            }
        }

        #endregion

        #region Step 4: Mark Outer Walls

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
            return _spawnChamberCenter;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Check if cell is a corridor intersection (3+ ways).
        /// </summary>
        private bool IsIntersection(int x, int y)
        {
            int neighbors = 0;

            // Check 4 cardinal directions
            if (x > 0 && _grid[x - 1, y] == GridMazeCell.Corridor) neighbors++;
            if (x < _gridSize - 1 && _grid[x + 1, y] == GridMazeCell.Corridor) neighbors++;
            if (y > 0 && _grid[x, y - 1] == GridMazeCell.Corridor) neighbors++;
            if (y < _gridSize - 1 && _grid[x, y + 1] == GridMazeCell.Corridor) neighbors++;

            return neighbors >= 3; // 3+ ways = intersection
        }

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
