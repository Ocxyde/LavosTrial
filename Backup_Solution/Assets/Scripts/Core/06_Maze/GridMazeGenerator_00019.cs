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
// Custom grid-based maze generation system - DFS Recursive Backtracking
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-OUT: Independent maze generator
// - No dependencies on MazeGenerator class
// - Pure grid-based DFS algorithm with recursive backtracking
// - SPAWN ROOM PLACED FIRST (guaranteed with entrance/exit)
// - Exit corridor carved FROM spawn room to connect with main maze
// - Values loaded from GameConfig (no hardcoding)
//
// GENERATION ORDER:
// 1. Fill entire grid with walls
// 2. Generate maze using DFS (carve passages on odd cells)
// 3. Place spawn room at fixed position (carved into the maze)
// 4. Carve exit corridor from spawn room to connect with main maze
// 5. Add outer walls (ensure perimeter is solid)
//
// Location: Assets/Scripts/Core/06_Maze/

using UnityEngine;
using System.Collections.Generic;

namespace Code.Lavos.Core
{
    /// <summary>
    /// GridMazeGenerator - Custom grid-based maze generation using DFS.
    /// Creates maze with spawn room first, then carves exit corridor to connect with main maze.
    /// Values loaded from GameConfig (no hardcoding).
    /// </summary>
    public class GridMazeGenerator
    {
        #region Grid Settings (From JSON Config)

        // Grid settings - loaded from GameConfig
        public int gridSize;
        public int roomSize;
        public int corridorWidth;

        // Difficulty scaling from seed
        private float seedFactor;

        #endregion

        #region Grid Data

        // The grid
        private GridMazeCell[,] grid;

        // Spawn room specific data
        private Vector2Int spawnRoomCenter;
        private Vector2Int spawnEntranceDirection;
        private Vector2Int spawnExitDirection;

        #endregion

        #region Public Accessors

        // Public access to grid
        public GridMazeCell[,] Grid => grid;
        public int GridSize => gridSize;
        public Vector2Int SpawnRoomCenter => spawnRoomCenter;
        public Vector2Int SpawnEntranceDirection => spawnEntranceDirection;
        public Vector2Int SpawnExitDirection => spawnExitDirection;

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize grid settings from GameConfig.
        /// Call this before Generate().
        /// </summary>
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
        /// Generate complete maze grid using DFS (Depth-First Search) with recursive backtracking.
        /// </summary>
        /// <param name="seed">Random seed for reproducible generation</param>
        /// <param name="difficultyFactor">0.0-1.0 difficulty scaling (unused for DFS)</param>
        public void Generate(uint seed, float difficultyFactor)
        {
            // Store difficulty factor for future use
            seedFactor = Mathf.Clamp01(difficultyFactor);

            // Initialize random state for reproducible generation
            Random.InitState((int)seed);
            Debug.Log($"[GridMazeGenerator] Random seed initialized: {seed} (difficulty: {seedFactor:F2})");

            // Initialize from config if not already set
            if (gridSize == 0) InitializeFromConfig();

            Debug.Log($"[GridMazeGenerator] Creating {gridSize}x{gridSize} grid...");

            // Step 1: Fill grid with walls
            FillGridWithWalls();

            // Step 2: Generate maze using DFS (carve passages on odd cells)
            GenerateDFS();

            // Step 3: Place spawn room at fixed position (carved into the maze)
            PlaceSpawnRoom();

            // Step 4: Carve exit corridor from spawn room to connect with main maze
            CarveExitCorridor();

            // Step 5: Add outer walls (ensure perimeter is solid)
            AddOuterWalls();

            Debug.Log($"[GridMazeGenerator] Maze generated with DFS algorithm");
            Debug.Log($"[GridMazeGenerator] Spawn room at: {spawnRoomCenter}");
            Debug.Log($"[GridMazeGenerator] Entrance direction: {spawnEntranceDirection}");
        }

        #endregion

        #region Step 1: Fill Grid With Walls

        /// <summary>
        /// Step 1: Fill entire grid with walls (DFS will carve passages).
        /// </summary>
        private void FillGridWithWalls()
        {
            grid = new GridMazeCell[gridSize, gridSize];

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    grid[x, y] = GridMazeCell.Wall;
                }
            }

            Debug.Log($"[GridMazeGenerator] Grid filled with walls (DFS will carve passages)");
        }

        #endregion

        #region Step 2: Generate Maze Using DFS

        /// <summary>
        /// Step 2: Generate maze using DFS with recursive backtracking.
        /// Works on odd-numbered cells for proper wall/passage structure.
        /// Uses iterative approach with explicit stack to avoid stack overflow on large grids.
        /// </summary>
        private void GenerateDFS()
        {
            Debug.Log($"[GridMazeGenerator] STEP 2: Generating maze with DFS (recursive backtracking)...");

            // Use iterative DFS with explicit stack (avoids stack overflow on large grids)
            var stack = new Stack<Vector2Int>();

            // Start from center of grid (odd coordinates for proper maze structure)
            int startX = (gridSize / 2) | 1;  // Force odd
            int startY = (gridSize / 2) | 1;  // Force odd

            // Clamp to valid range (stay within bounds, leave room for outer walls)
            startX = Mathf.Clamp(startX, 1, gridSize - 2);
            startY = Mathf.Clamp(startY, 1, gridSize - 2);

            // Mark starting cell as passage
            grid[startX, startY] = GridMazeCell.Floor;
            stack.Push(new Vector2Int(startX, startY));

            int cellsCarved = 1;

            // Directions: up, right, down, left (2 cells at a time for maze structure)
            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up * 2,
                Vector2Int.right * 2,
                Vector2Int.down * 2,
                Vector2Int.left * 2
            };

            while (stack.Count > 0)
            {
                Vector2Int current = stack.Peek();

                // Get unvisited neighbors
                List<Vector2Int> neighbors = new List<Vector2Int>();

                foreach (var dir in directions)
                {
                    int nx = current.x + dir.x;
                    int ny = current.y + dir.y;

                    // Check bounds (stay within grid, leave outer wall)
                    if (nx > 0 && nx < gridSize - 1 && ny > 0 && ny < gridSize - 1)
                    {
                        // Check if neighbor is unvisited (still a wall)
                        if (grid[nx, ny] == GridMazeCell.Wall)
                        {
                            neighbors.Add(new Vector2Int(nx, ny));
                        }
                    }
                }

                if (neighbors.Count > 0)
                {
                    // Choose random unvisited neighbor
                    Vector2Int next = neighbors[Random.Range(0, neighbors.Count)];

                    // Carve wall between current and next (the cell in between)
                    int wallX = current.x + (next.x - current.x) / 2;
                    int wallY = current.y + (next.y - current.y) / 2;
                    grid[wallX, wallY] = GridMazeCell.Floor;

                    // Mark next cell as passage
                    grid[next.x, next.y] = GridMazeCell.Floor;
                    cellsCarved += 2;

                    // Push next cell to stack
                    stack.Push(next);
                }
                else
                {
                    // Backtrack
                    stack.Pop();
                }
            }

            Debug.Log($"[GridMazeGenerator] DFS complete: {cellsCarved} cells carved into passages");
        }

        #endregion

        #region Step 3: Place Spawn Room

        /// <summary>
        /// Step 3: Place SPAWN room at a fixed position (carved into the maze).
        /// Spawn room is a walkable room with the spawn point in the center.
        /// The open wall direction determines where the exit corridor will be carved.
        /// </summary>
        private void PlaceSpawnRoom()
        {
            var cfg = GameConfig.Instance;
            int spawnRoomSize = cfg.spawnRoomSize;           // Default 5 (from JSON)
            int margin = cfg.spawnRoomMargin;                // Default 2 (from JSON)
            int spawnPointX = cfg.spawnPointInRoomX;         // Default 2 (center of 5x5)
            int spawnPointY = cfg.spawnPointInRoomY;         // Default 2 (center of 5x5)
            int openWall = cfg.spawnRoomOpenWall;            // Default 0 (west: 0=left, 1=right, 2=top, 3=bottom)

            // Compute spawn room position: fixed margin from left, centered vertically
            int spawnRoomX = margin + 1;
            int spawnRoomY = (gridSize / 2) - (spawnRoomSize / 2);

            Debug.Log($"[GridMazeGenerator] ==================================");
            Debug.Log($"[GridMazeGenerator] STEP 3: Placing SPAWN ROOM ({spawnRoomSize}x{spawnRoomSize})...");
            Debug.Log($"[GridMazeGenerator] Spawn room position: ({spawnRoomX}, {spawnRoomY}) to ({spawnRoomX + spawnRoomSize - 1}, {spawnRoomY + spawnRoomSize - 1})");
            Debug.Log($"[GridMazeGenerator] Open wall: {(openWall == 0 ? "WEST" : openWall == 1 ? "EAST" : openWall == 2 ? "NORTH" : "SOUTH")}");

            // Carve spawn room area (clear walls to create walkable space)
            int cellsMarked = 0;
            for (int dx = 0; dx < spawnRoomSize; dx++)
            {
                for (int dy = 0; dy < spawnRoomSize; dy++)
                {
                    int gridX = spawnRoomX + dx;
                    int gridY = spawnRoomY + dy;

                    if (gridX >= 0 && gridX < gridSize && gridY >= 0 && gridY < gridSize)
                    {
                        if (dx == spawnPointX && dy == spawnPointY)
                        {
                            grid[gridX, gridY] = GridMazeCell.SpawnPoint;
                            spawnRoomCenter = new Vector2Int(gridX, gridY);
                            cellsMarked++;
                        }
                        else
                        {
                            grid[gridX, gridY] = GridMazeCell.Room;
                            cellsMarked++;
                        }
                    }
                }
            }

            Debug.Log($"[GridMazeGenerator] Spawn room placed: {cellsMarked} cells marked");
            Debug.Log($"[GridMazeGenerator] ==================================");

            // Set entrance/exit directions based on open wall
            spawnEntranceDirection = openWall switch
            {
                0 => Vector2Int.left,
                1 => Vector2Int.right,
                2 => Vector2Int.up,
                3 => Vector2Int.down,
                _ => Vector2Int.left
            };
            spawnExitDirection = -spawnEntranceDirection;
        }

        #endregion

        #region Step 4: Carve Exit Corridor

        /// <summary>
        /// Step 4: Carve a corridor from spawn room to connect with the main DFS maze.
        /// The DFS maze is in the center, spawn room is on the left.
        /// Carve EAST from spawn room to connect with the maze.
        /// </summary>
        private void CarveExitCorridor()
        {
            Debug.Log($"[GridMazeGenerator] STEP 4: Carving exit corridor from spawn room...");

            // Carve corridor from spawn room toward the CENTER of the grid (where DFS maze is)
            int halfWidth = corridorWidth / 2;

            // Start from spawn center, carve EAST toward maze center
            int startX = spawnRoomCenter.x;
            int startY = spawnRoomCenter.y;

            // Direction: EAST (toward maze center)
            Vector2Int dir = Vector2Int.right;

            // Carve until we hit a Floor cell (DFS passage)
            int maxSteps = gridSize;
            int steps = 0;

            while (steps < maxSteps)
            {
                startX += dir.x;
                startY += dir.y;
                steps++;

                // Check bounds
                if (startX <= 0 || startX >= gridSize - 1 || startY <= 0 || startY >= gridSize - 1)
                    break;

                // Carve corridor (with width)
                for (int w = -halfWidth; w <= halfWidth; w++)
                {
                    int cx = startX;
                    int cy = startY + w;

                    if (cx >= 0 && cx < gridSize && cy >= 0 && cy < gridSize)
                    {
                        // Don't overwrite spawn room or existing passages
                        if (grid[cx, cy] != GridMazeCell.Room && grid[cx, cy] != GridMazeCell.SpawnPoint)
                        {
                            grid[cx, cy] = GridMazeCell.Corridor;
                        }
                    }
                }

                // Stop when we hit a DFS-carved passage
                if (grid[startX, startY] == GridMazeCell.Floor)
                {
                    grid[startX, startY] = GridMazeCell.Corridor;  // Connect to maze
                    break;
                }
            }

            Debug.Log($"[GridMazeGenerator] Exit corridor carved {steps} cells to connect with maze");
        }

        #endregion

        #region Step 5: Add Outer Walls

        /// <summary>
        /// Step 5: Add outer walls (maze perimeter).
        /// Preserves SpawnPoint and Room cells.
        /// </summary>
        private void AddOuterWalls()
        {
            Debug.Log($"[GridMazeGenerator] STEP 5: Adding outer walls...");

            // Mark outer edges as Wall (but NOT SpawnPoint or Room!)
            for (int x = 0; x < gridSize; x++)
            {
                if (grid[x, 0] != GridMazeCell.SpawnPoint && grid[x, 0] != GridMazeCell.Room)
                    grid[x, 0] = GridMazeCell.Wall;
                if (grid[x, gridSize - 1] != GridMazeCell.SpawnPoint && grid[x, gridSize - 1] != GridMazeCell.Room)
                    grid[x, gridSize - 1] = GridMazeCell.Wall;
            }

            for (int y = 0; y < gridSize; y++)
            {
                if (grid[0, y] != GridMazeCell.SpawnPoint && grid[0, y] != GridMazeCell.Room)
                    grid[0, y] = GridMazeCell.Wall;
                if (grid[gridSize - 1, y] != GridMazeCell.SpawnPoint && grid[gridSize - 1, y] != GridMazeCell.Room)
                    grid[gridSize - 1, y] = GridMazeCell.Wall;
            }

            Debug.Log($"[GridMazeGenerator] Outer walls added");
        }

        #endregion

        #region Grid Cell Access

        /// <summary>
        /// Get cell type at position.
        /// </summary>
        public GridMazeCell GetCell(int x, int y)
        {
            if (x >= 0 && x < gridSize && y >= 0 && y < gridSize)
            {
                return grid[x, y];
            }
            return GridMazeCell.Floor;
        }

        /// <summary>
        /// Find the spawn point cell in the grid.
        /// </summary>
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

            // Fallback to spawn room center
            Debug.LogWarning("[GridMazeGenerator] No SpawnPoint found - using spawn room center");
            return spawnRoomCenter;
        }

        #endregion

        #region Serialization (Binary Storage)

        /// <summary>
        /// Serialize entire grid to compact byte array.
        /// </summary>
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

            Debug.Log($"[GridMazeGenerator] Serialized grid: {totalBytes} bytes");
            return data;
        }

        /// <summary>
        /// Deserialize grid from byte array.
        /// </summary>
        public void DeserializeFromBytes(byte[] data)
        {
            if (data == null || data.Length < 2)
            {
                Debug.LogError("[GridMazeGenerator] Invalid data for deserialization");
                return;
            }

            gridSize = data[0];
            int expectedSize = 2 + (gridSize * gridSize);

            if (data.Length != expectedSize)
            {
                Debug.LogError($"[GridMazeGenerator] Data size mismatch: expected {expectedSize}, got {data.Length}");
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

            Debug.Log($"[GridMazeGenerator] Deserialized grid: {data.Length} bytes");
        }

        #endregion
    }
}
