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
// DFS Maze Generation with Recursive Backtracking
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using UnityEngine;
using System.Collections.Generic;

namespace Code.Lavos.Core
{
    /// <summary>
    /// GridMazeGenerator - DFS maze generation with recursive backtracking.
    /// Generates a perfect maze (no loops, all cells reachable) using DFS.
    /// Spawn room is placed at grid edge and connected to main maze.
    /// </summary>
    public class GridMazeGenerator
    {
        #region Grid Settings

        public int gridSize;
        public int roomSize;
        public int corridorWidth;
        private float seedFactor;

        #endregion

        #region Grid Data

        private GridMazeCell[,] grid;
        private Vector2Int spawnRoomCenter;
        private Vector2Int spawnEntranceDirection;
        private Vector2Int spawnExitDirection;

        #endregion

        #region Public Accessors

        public GridMazeCell[,] Grid => grid;
        public int GridSize => gridSize;
        public Vector2Int SpawnRoomCenter => spawnRoomCenter;
        public Vector2Int SpawnEntranceDirection => spawnEntranceDirection;
        public Vector2Int SpawnExitDirection => spawnExitDirection;

        #endregion

        #region Initialization

        public void InitializeFromConfig()
        {
            var config = GameConfig.Instance;
            gridSize = config.defaultGridSize;
            roomSize = config.defaultRoomSize;
            corridorWidth = config.defaultCorridorWidth;

            Debug.Log($"[GridMazeGenerator] Config loaded: {gridSize}x{gridSize} grid, {roomSize}x{roomSize} rooms, {corridorWidth}-cell corridors");
        }

        #endregion

        #region Main Generation

        /// <summary>
        /// Generate maze using DFS recursive backtracking.
        /// Grid must be odd-sized for proper maze structure (e.g., 19x19, 21x21).
        /// </summary>
        public void Generate(uint seed, float difficultyFactor)
        {
            seedFactor = Mathf.Clamp01(difficultyFactor);
            Random.InitState((int)seed);
            
            Debug.Log($"[GridMazeGenerator] Random seed: {seed} (difficulty: {seedFactor:F2})");

            if (gridSize == 0) InitializeFromConfig();

            // Ensure odd grid size for proper maze structure
            if (gridSize % 2 == 0)
            {
                gridSize++;
                Debug.Log($"[GridMazeGenerator] Grid size adjusted to {gridSize} (must be odd for DFS)");
            }

            Debug.Log($"[GridMazeGenerator] Creating {gridSize}x{gridSize} DFS maze...");

            // Step 1: Initialize grid with cells (not walls yet)
            InitializeGrid();

            // Step 2: Generate DFS maze (carve passages)
            GenerateDFS();

            // Step 3: Place spawn room on grid edge
            PlaceSpawnRoom();

            // Step 4: Connect spawn room to main maze
            ConnectSpawnToMaze();

            // Step 5: Mark outer boundary as walls
            MarkOuterWalls();

            Debug.Log($"[GridMazeGenerator] DFS maze complete - Spawn: {spawnRoomCenter}");
        }

        #endregion

        #region Step 1: Initialize Grid

        /// <summary>
        /// Initialize grid - all cells start as potential passages.
        /// DFS will carve the actual maze paths.
        /// </summary>
        private void InitializeGrid()
        {
            grid = new GridMazeCell[gridSize, gridSize];

            // Initialize all as Floor (DFS will create walls by not visiting some cells)
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    grid[x, y] = GridMazeCell.Floor;
                }
            }

            Debug.Log($"[GridMazeGenerator] Grid initialized");
        }

        #endregion

        #region Step 2: Generate DFS Maze

        /// <summary>
        /// Generate maze using iterative DFS with recursive backtracking.
        /// Works on a cell grid where odd coordinates are passages, even are walls.
        /// </summary>
        private void GenerateDFS()
        {
            Debug.Log($"[GridMazeGenerator] Generating DFS maze (recursive backtracking)...");

            var stack = new Stack<Vector2Int>();
            
            // Start from center (odd coordinates)
            int startX = (gridSize / 2) | 1;
            int startY = (gridSize / 2) | 1;
            startX = Mathf.Clamp(startX, 1, gridSize - 2);
            startY = Mathf.Clamp(startY, 1, gridSize - 2);

            // Mark all cells as unvisited (we'll use a separate tracking)
            bool[,] visited = new bool[gridSize, gridSize];

            // Start DFS
            grid[startX, startY] = GridMazeCell.Floor;
            visited[startX, startY] = true;
            stack.Push(new Vector2Int(startX, startY));

            int cellsVisited = 1;

            // 4 directions (move by 2 to skip wall cells)
            Vector2Int[] dirs = new Vector2Int[]
            {
                Vector2Int.up * 2,
                Vector2Int.right * 2,
                Vector2Int.down * 2,
                Vector2Int.left * 2
            };

            while (stack.Count > 0)
            {
                Vector2Int current = stack.Peek();
                List<Vector2Int> unvisited = new List<Vector2Int>();

                // Check all 4 directions
                foreach (var dir in dirs)
                {
                    int nx = current.x + dir.x;
                    int ny = current.y + dir.y;

                    if (nx > 0 && nx < gridSize - 1 && ny > 0 && ny < gridSize - 1)
                    {
                        if (!visited[nx, ny])
                        {
                            unvisited.Add(new Vector2Int(nx, ny));
                        }
                    }
                }

                if (unvisited.Count > 0)
                {
                    // Pick random unvisited neighbor
                    Vector2Int next = unvisited[Random.Range(0, unvisited.Count)];
                    
                    // Carve passage (the cell between current and next)
                    int wallX = current.x + (next.x - current.x) / 2;
                    int wallY = current.y + (next.y - current.y) / 2;
                    
                    grid[wallX, wallY] = GridMazeCell.Floor;
                    visited[wallX, wallY] = true;
                    
                    // Mark next cell
                    grid[next.x, next.y] = GridMazeCell.Floor;
                    visited[next.x, next.y] = true;
                    cellsVisited += 2;

                    stack.Push(next);
                }
                else
                {
                    // Backtrack
                    stack.Pop();
                }
            }

            // Now mark non-passage cells as walls
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    if (!visited[x, y])
                    {
                        grid[x, y] = GridMazeCell.Wall;
                    }
                }
            }

            Debug.Log($"[GridMazeGenerator] DFS complete: {cellsVisited} passage cells, rest are walls");
        }

        #endregion

        #region Step 3: Place Spawn Room

        /// <summary>
        /// Place spawn room on the WEST edge of the grid.
        /// Spawn room is a clear area where player spawns.
        /// </summary>
        private void PlaceSpawnRoom()
        {
            var cfg = GameConfig.Instance;
            int spawnRoomSize = cfg.spawnRoomSize;
            int margin = cfg.spawnRoomMargin;
            int spawnPointX = cfg.spawnPointInRoomX;
            int spawnPointY = cfg.spawnPointInRoomY;

            // Position spawn room on WEST edge, centered vertically
            int spawnRoomX = margin;
            int spawnRoomY = (gridSize / 2) - (spawnRoomSize / 2);
            spawnRoomY = Mathf.Clamp(spawnRoomY, margin, gridSize - spawnRoomSize - margin);

            Debug.Log($"[GridMazeGenerator] Placing spawn room ({spawnRoomSize}x{spawnRoomSize}) at ({spawnRoomX}, {spawnRoomY})");

            // Carve out spawn room (clear walls)
            int cellsMarked = 0;
            for (int dx = 0; dx < spawnRoomSize; dx++)
            {
                for (int dy = 0; dy < spawnRoomSize; dy++)
                {
                    int gx = spawnRoomX + dx;
                    int gy = spawnRoomY + dy;

                    if (gx >= 0 && gx < gridSize && gy >= 0 && gy < gridSize)
                    {
                        if (dx == spawnPointX && dy == spawnPointY)
                        {
                            grid[gx, gy] = GridMazeCell.SpawnPoint;
                            spawnRoomCenter = new Vector2Int(gx, gy);
                        }
                        else
                        {
                            grid[gx, gy] = GridMazeCell.Room;
                        }
                        cellsMarked++;
                    }
                }
            }

            // Set directions (spawn opens to EAST toward maze)
            spawnEntranceDirection = Vector2Int.right;
            spawnExitDirection = Vector2Int.left;

            Debug.Log($"[GridMazeGenerator] Spawn room placed: {cellsMarked} cells, center: {spawnRoomCenter}");
        }

        #endregion

        #region Step 4: Connect Spawn to Maze

        /// <summary>
        /// Carve a corridor from spawn room EAST to connect with main DFS maze.
        /// </summary>
        private void ConnectSpawnToMaze()
        {
            Debug.Log($"[GridMazeGenerator] Connecting spawn room to maze...");

            int halfWidth = corridorWidth / 2;
            int startX = spawnRoomCenter.x + 1;  // Start just right of spawn room
            int startY = spawnRoomCenter.y;

            // Carve EAST until we hit a Floor cell (DFS passage)
            int maxSteps = gridSize;
            int steps = 0;

            for (int x = startX; x < gridSize - 1 && steps < maxSteps; x++, steps++)
            {
                // Carve corridor with width
                for (int w = -halfWidth; w <= halfWidth; w++)
                {
                    int gy = startY + w;
                    if (gy >= 0 && gy < gridSize)
                    {
                        if (grid[x, gy] != GridMazeCell.Room && grid[x, gy] != GridMazeCell.SpawnPoint)
                        {
                            grid[x, gy] = GridMazeCell.Corridor;
                        }
                    }
                }

                // Check if we hit DFS passage
                if (grid[x, startY] == GridMazeCell.Floor)
                {
                    grid[x, startY] = GridMazeCell.Corridor;
                    Debug.Log($"[GridMazeGenerator] Connected to maze after {steps} cells");
                    break;
                }
            }
        }

        #endregion

        #region Step 5: Mark Outer Walls

        /// <summary>
        /// Mark grid perimeter as walls (except spawn room area).
        /// </summary>
        private void MarkOuterWalls()
        {
            Debug.Log($"[GridMazeGenerator] Marking outer walls...");

            for (int x = 0; x < gridSize; x++)
            {
                // Top and bottom edges
                if (grid[x, 0] != GridMazeCell.SpawnPoint && grid[x, 0] != GridMazeCell.Room)
                    grid[x, 0] = GridMazeCell.Wall;
                if (grid[x, gridSize - 1] != GridMazeCell.SpawnPoint && grid[x, gridSize - 1] != GridMazeCell.Room)
                    grid[x, gridSize - 1] = GridMazeCell.Wall;
            }

            for (int y = 0; y < gridSize; y++)
            {
                // Left and right edges (except spawn room)
                if (grid[0, y] != GridMazeCell.SpawnPoint && grid[0, y] != GridMazeCell.Room)
                    grid[0, y] = GridMazeCell.Wall;
                if (grid[gridSize - 1, y] != GridMazeCell.SpawnPoint && grid[gridSize - 1, y] != GridMazeCell.Room)
                    grid[gridSize - 1, y] = GridMazeCell.Wall;
            }

            Debug.Log($"[GridMazeGenerator] Outer walls marked");
        }

        #endregion

        #region Grid Cell Access

        public GridMazeCell GetCell(int x, int y)
        {
            if (x >= 0 && x < gridSize && y >= 0 && y < gridSize)
                return grid[x, y];
            return GridMazeCell.Floor;
        }

        public Vector2Int FindSpawnPoint()
        {
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    if (grid[x, y] == GridMazeCell.SpawnPoint)
                    {
                        Debug.Log($"[GridMazeGenerator] SpawnPoint found at ({x}, {y})");
                        return new Vector2Int(x, y);
                    }
                }
            }

            Debug.LogWarning("[GridMazeGenerator] No SpawnPoint found - using spawn room center");
            return spawnRoomCenter;
        }

        #endregion

        #region Serialization

        public byte[] SerializeToBytes()
        {
            int totalBytes = 2 + (gridSize * gridSize);
            byte[] data = new byte[totalBytes];

            data[0] = (byte)gridSize;
            data[1] = (byte)gridSize;

            int index = 2;
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    data[index++] = (byte)grid[x, y];
                }
            }

            Debug.Log($"[GridMazeGenerator] Serialized: {totalBytes} bytes");
            return data;
        }

        public void DeserializeFromBytes(byte[] data)
        {
            if (data == null || data.Length < 2)
            {
                Debug.LogError("[GridMazeGenerator] Invalid data");
                return;
            }

            gridSize = data[0];
            int expectedSize = 2 + (gridSize * gridSize);

            if (data.Length != expectedSize)
            {
                Debug.LogError($"[GridMazeGenerator] Size mismatch: expected {expectedSize}, got {data.Length}");
                return;
            }

            grid = new GridMazeCell[gridSize, gridSize];

            int index = 2;
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    grid[x, y] = (GridMazeCell)data[index++];
                }
            }

            Debug.Log($"[GridMazeGenerator] Deserialized: {data.Length} bytes");
        }

        #endregion
    }
}
