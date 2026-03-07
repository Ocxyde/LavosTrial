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
// Simple room-and-corridor maze generation

using UnityEngine;
using System.Collections.Generic;

namespace Code.Lavos.Core
{
    /// <summary>
    /// GridMazeGenerator - Simple maze with rooms connected by corridors.
    /// 1. Fill grid with Floor (walkable)
    /// 2. Place walls in a grid pattern to create "cells"
    /// 3. Carve passages between cells using DFS
    /// 4. Place spawn room on west edge
    /// 5. Connect spawn to main maze
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

            Debug.Log($"[GridMazeGenerator] Config: {gridSize}x{gridSize} grid");
        }

        #endregion

        #region Main Generation

        public void Generate(uint seed, float difficultyFactor)
        {
            seedFactor = Mathf.Clamp01(difficultyFactor);
            Random.InitState((int)seed);
            
            Debug.Log($"[GridMazeGenerator] Seed: {seed} (difficulty: {seedFactor:F2})");

            if (gridSize == 0) InitializeFromConfig();

            Debug.Log($"[GridMazeGenerator] Generating {gridSize}x{gridSize} maze...");

            // Step 1: Fill with Floor (all walkable initially)
            FillGrid();

            // Step 2: Add wall grid pattern (create maze cells)
            CreateWallGrid();

            // Step 3: Carve DFS passages through wall grid
            CarveDFSPassages();

            // Step 4: Place spawn room on west edge
            PlaceSpawnRoom();

            // Step 5: Connect spawn to maze
            ConnectSpawnToMaze();

            // Step 6: Mark outer boundary walls
            MarkOuterWalls();

            Debug.Log($"[GridMazeGenerator] Maze complete - Spawn: {spawnRoomCenter}");
        }

        #endregion

        #region Step 1: Fill Grid

        private void FillGrid()
        {
            grid = new GridMazeCell[gridSize, gridSize];

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    grid[x, y] = GridMazeCell.Floor;
                }
            }

            Debug.Log($"[GridMazeGenerator] Grid filled with Floor");
        }

        #endregion

        #region Step 2: Create Wall Grid Pattern

        /// <summary>
        /// Create a grid of walls that divides the maze into cells.
        /// Walls are placed every 3 cells (creates 2-cell wide passages).
        /// </summary>
        private void CreateWallGrid()
        {
            Debug.Log($"[GridMazeGenerator] Creating wall grid pattern...");

            int cellSpacing = 3; // Wall every 3 cells

            // Vertical walls
            for (int x = cellSpacing; x < gridSize - 1; x += cellSpacing)
            {
                for (int y = 1; y < gridSize - 1; y++)
                {
                    grid[x, y] = GridMazeCell.Wall;
                }
            }

            // Horizontal walls
            for (int y = cellSpacing; y < gridSize - 1; y += cellSpacing)
            {
                for (int x = 1; x < gridSize - 1; x++)
                {
                    grid[x, y] = GridMazeCell.Wall;
                }
            }

            Debug.Log($"[GridMazeGenerator] Wall grid created (spacing: {cellSpacing})");
        }

        #endregion

        #region Step 3: Carve DFS Passages

        /// <summary>
        /// Use DFS to carve passages through the wall grid.
        /// This creates a solvable maze by removing some walls.
        /// </summary>
        private void CarveDFSPassages()
        {
            Debug.Log($"[GridMazeGenerator] Carving DFS passages...");

            var stack = new Stack<Vector2Int>();
            bool[,] visited = new bool[gridSize, gridSize];

            // Start from center
            int startX = gridSize / 2;
            int startY = gridSize / 2;

            stack.Push(new Vector2Int(startX, startY));
            visited[startX, startY] = true;
            int carved = 0;

            // 4 directions
            Vector2Int[] dirs = new Vector2Int[]
            {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };

            while (stack.Count > 0)
            {
                Vector2Int current = stack.Peek();
                List<Vector2Int> walls = new List<Vector2Int>();

                // Find wall neighbors (2 cells away)
                foreach (var dir in dirs)
                {
                    int nx = current.x + dir.x * 2;
                    int ny = current.y + dir.y * 2;

                    if (nx > 0 && nx < gridSize - 1 && ny > 0 && ny < gridSize - 1)
                    {
                        if (!visited[nx, ny] && grid[nx, ny] == GridMazeCell.Wall)
                        {
                            walls.Add(new Vector2Int(nx, ny));
                        }
                    }
                }

                if (walls.Count > 0)
                {
                    // Pick random wall
                    Vector2Int wall = walls[Random.Range(0, walls.Count)];
                    
                    // Carve wall between current and target
                    int midX = current.x + (wall.x - current.x) / 2;
                    int midY = current.y + (wall.y - current.y) / 2;
                    
                    grid[midX, midY] = GridMazeCell.Floor;
                    grid[wall.x, wall.y] = GridMazeCell.Floor;
                    visited[wall.x, wall.y] = true;
                    carved += 2;

                    stack.Push(wall);
                }
                else
                {
                    stack.Pop();
                }
            }

            Debug.Log($"[GridMazeGenerator] Carved {carved} passages");
        }

        #endregion

        #region Step 4: Place Spawn Room

        private void PlaceSpawnRoom()
        {
            var cfg = GameConfig.Instance;
            int spawnRoomSize = cfg.spawnRoomSize;
            int margin = cfg.spawnRoomMargin;

            // Position on west edge, centered vertically
            int spawnX = margin;
            int spawnY = (gridSize / 2) - (spawnRoomSize / 2);
            spawnY = Mathf.Clamp(spawnY, margin, gridSize - spawnRoomSize - margin);

            Debug.Log($"[GridMazeGenerator] Spawn room at ({spawnX}, {spawnY}) size {spawnRoomSize}x{spawnRoomSize}");

            // Carve spawn room (clear all walls)
            for (int dx = 0; dx < spawnRoomSize; dx++)
            {
                for (int dy = 0; dy < spawnRoomSize; dy++)
                {
                    int gx = spawnX + dx;
                    int gy = spawnY + dy;

                    if (gx >= 0 && gx < gridSize && gy >= 0 && gy < gridSize)
                    {
                        if (dx == spawnRoomSize / 2 && dy == spawnRoomSize / 2)
                        {
                            grid[gx, gy] = GridMazeCell.SpawnPoint;
                            spawnRoomCenter = new Vector2Int(gx, gy);
                        }
                        else
                        {
                            grid[gx, gy] = GridMazeCell.Room;
                        }
                    }
                }
            }

            spawnEntranceDirection = Vector2Int.right; // Opens east toward maze
            spawnExitDirection = Vector2Int.left;

            Debug.Log($"[GridMazeGenerator] Spawn placed at {spawnRoomCenter}");
        }

        #endregion

        #region Step 5: Connect Spawn to Maze

        private void ConnectSpawnToMaze()
        {
            Debug.Log($"[GridMazeGenerator] Connecting spawn to maze...");

            int halfWidth = corridorWidth / 2;
            int startX = spawnRoomCenter.x + 1;
            int startY = spawnRoomCenter.y;

            // Carve east until we hit Floor
            for (int x = startX; x < gridSize - 1; x++)
            {
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

                if (grid[x, startY] == GridMazeCell.Floor)
                {
                    grid[x, startY] = GridMazeCell.Corridor;
                    Debug.Log($"[GridMazeGenerator] Connected after {x - startX} cells");
                    break;
                }
            }
        }

        #endregion

        #region Step 6: Mark Outer Walls

        private void MarkOuterWalls()
        {
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

            Debug.Log($"[GridMazeGenerator] Outer walls marked");
        }

        #endregion

        #region Grid Access

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
                        return new Vector2Int(x, y);
                    }
                }
            }
            return spawnRoomCenter;
        }

        #endregion

        #region Serialization

        public byte[] SerializeToBytes()
        {
            int total = 2 + gridSize * gridSize;
            byte[] data = new byte[total];
            data[0] = (byte)gridSize;
            data[1] = (byte)gridSize;

            int i = 2;
            for (int y = 0; y < gridSize; y++)
                for (int x = 0; x < gridSize; x++)
                    data[i++] = (byte)grid[x, y];

            return data;
        }

        public void DeserializeFromBytes(byte[] data)
        {
            if (data.Length < 2) return;
            
            gridSize = data[0];
            grid = new GridMazeCell[gridSize, gridSize];

            int i = 2;
            for (int y = 0; y < gridSize; y++)
                for (int x = 0; x < gridSize; x++)
                    grid[x, y] = (GridMazeCell)data[i++];
        }

        #endregion
    }
}
