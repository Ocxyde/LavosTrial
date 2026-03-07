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
// Simple room-based maze generation

using UnityEngine;
using System.Collections.Generic;

namespace Code.Lavos.Core
{
    /// <summary>
    /// GridMazeGenerator - Simple room and corridor maze.
    /// 1. Fill grid with Floor
    /// 2. Place rectangular rooms (Room cells)
    /// 3. Connect rooms with corridors (Corridor cells)
    /// 4. Mark remaining as Wall
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
        private List<Vector2Int> roomCenters = new List<Vector2Int>();

        #endregion

        #region Public Accessors

        public GridMazeCell[,] Grid => grid;
        public int GridSize { get => gridSize; set => gridSize = value; }
        public int RoomSize { get => roomSize; set => roomSize = value; }
        public int CorridorWidth { get => corridorWidth; set => corridorWidth = value; }
        public Vector2Int SpawnRoomCenter => spawnRoomCenter;
        public Vector2Int SpawnEntranceDirection => spawnEntranceDirection;
        public Vector2Int SpawnExitDirection => spawnEntranceDirection * -1;

        #endregion

        #region Initialization

        public void InitializeFromConfig()
        {
            var config = GameConfig.Instance;
            gridSize = config.defaultGridSize;
            roomSize = config.defaultRoomSize;
            corridorWidth = config.defaultCorridorWidth;

            Debug.Log($"[GridMazeGenerator] Config: {gridSize}x{gridSize} grid, {roomSize}x{roomSize} rooms");
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

            // Step 1: Fill with Floor
            FillGrid();

            // Step 2: Place rooms
            PlaceRooms();

            // Step 3: Connect rooms with corridors
            ConnectRooms();

            // Step 4: Mark outer walls
            MarkOuterWalls();

            Debug.Log($"[GridMazeGenerator] Maze complete - {roomCenters.Count} rooms, spawn: {spawnRoomCenter}");
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

        #region Step 2: Place Rooms

        private void PlaceRooms()
        {
            var cfg = GameConfig.Instance;
            int spawnRoomSize = cfg.spawnRoomSize;
            int margin = cfg.spawnRoomMargin;

            // Place spawn room on west edge
            int spawnX = margin;
            int spawnY = (gridSize / 2) - (spawnRoomSize / 2);
            spawnY = Mathf.Clamp(spawnY, margin, gridSize - spawnRoomSize - margin);

            PlaceRoom(spawnX, spawnY, spawnRoomSize, true);
            spawnRoomCenter = new Vector2Int(
                spawnX + spawnRoomSize / 2,
                spawnY + spawnRoomSize / 2
            );
            spawnEntranceDirection = Vector2Int.right; // Opens east

            // IMPROVED: Place additional rooms using quadrant-based distribution
            // This ensures rooms are spread across the entire maze, not clustered in center
            int roomCount = 3 + Mathf.FloorToInt(seedFactor * 5f); // 3-8 rooms
            int placed = 1; // spawn room already placed
            
            // Divide grid into quadrants for better spatial distribution
            int quadrantSize = gridSize / 2;
            int roomsPerQuadrant = Mathf.CeilToInt((float)(roomCount - 1) / 4f);
            
            Debug.Log($"[GridMazeGenerator] Placing {roomCount - 1} rooms across 4 quadrants ({roomsPerQuadrant} per quadrant)");

            // Place rooms in each quadrant (NE, NW, SE, SW)
            for (int qx = 0; qx < 2; qx++)
            {
                for (int qy = 0; qy < 2; qy++)
                {
                    // Skip spawn quadrant (west center)
                    if (qx == 0 && qy == 0) continue;

                    int quadrantRoomsPlaced = 0;
                    int quadrantAttempts = 0;
                    int maxQuadrantAttempts = 25;

                    // Calculate quadrant bounds
                    int qxStart = qx * quadrantSize + margin;
                    int qyStart = qy * quadrantSize + margin;
                    int qxEnd = (qx + 1) * quadrantSize - margin - roomSize;
                    int qyEnd = (qy + 1) * quadrantSize - margin - roomSize;

                    // Clamp to valid range
                    qxStart = Mathf.Max(qxStart, margin);
                    qyStart = Mathf.Max(qyStart, margin);
                    qxEnd = Mathf.Min(qxEnd, gridSize - roomSize - margin);
                    qyEnd = Mathf.Min(qyEnd, gridSize - roomSize - margin);

                    // Place rooms in this quadrant
                    while (quadrantRoomsPlaced < roomsPerQuadrant && quadrantAttempts < maxQuadrantAttempts && placed < roomCount)
                    {
                        quadrantAttempts++;

                        // Random position within quadrant
                        int rx = Random.Range(qxStart, qxEnd + 1);
                        int ry = Random.Range(qyStart, qyEnd + 1);

                        // Check if overlaps with existing rooms (with extra spacing)
                        if (!OverlapsExistingRooms(rx, ry, roomSize))
                        {
                            PlaceRoom(rx, ry, roomSize, false);
                            placed++;
                            quadrantRoomsPlaced++;
                        }
                    }

                    Debug.Log($"[GridMazeGenerator] Quadrant ({qx},{qy}): {quadrantRoomsPlaced} rooms placed");
                }
            }

            // Fill remaining room count with random placement (fallback)
            int attempts = 0;
            while (placed < roomCount && attempts < 50)
            {
                attempts++;

                // Random position anywhere valid
                int rx = Random.Range(margin + spawnRoomSize + 2, gridSize - roomSize - margin);
                int ry = Random.Range(margin, gridSize - roomSize - margin);

                // Check if overlaps with existing rooms
                if (!OverlapsExistingRooms(rx, ry, roomSize))
                {
                    PlaceRoom(rx, ry, roomSize, false);
                    placed++;
                }
            }

            Debug.Log($"[GridMazeGenerator] Placed {placed} rooms total ({attempts} attempts)");
        }

        private void PlaceRoom(int x, int y, int size, bool isSpawn)
        {
            for (int dx = 0; dx < size; dx++)
            {
                for (int dy = 0; dy < size; dy++)
                {
                    int gx = x + dx;
                    int gy = y + dy;

                    if (gx >= 0 && gx < gridSize && gy >= 0 && gy < gridSize)
                    {
                        if (isSpawn && dx == size / 2 && dy == size / 2)
                        {
                            grid[gx, gy] = GridMazeCell.SpawnPoint;
                        }
                        else
                        {
                            grid[gx, gy] = GridMazeCell.Room;
                        }
                    }
                }
            }

            roomCenters.Add(new Vector2Int(x + size / 2, y + size / 2));
        }

        private bool OverlapsExistingRooms(int x, int y, int size)
        {
            int margin = 1; // Keep rooms separated

            foreach (var center in roomCenters)
            {
                int dx = Mathf.Abs(center.x - (x + size / 2));
                int dy = Mathf.Abs(center.y - (y + size / 2));

                if (dx < size + margin && dy < size + margin)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Step 3: Connect Rooms

        private void ConnectRooms()
        {
            if (roomCenters.Count < 2) return;

            Debug.Log($"[GridMazeGenerator] Connecting {roomCenters.Count} rooms...");

            // Connect each room to the next (creates a path through all rooms)
            for (int i = 0; i < roomCenters.Count - 1; i++)
            {
                Vector2Int from = roomCenters[i];
                Vector2Int to = roomCenters[i + 1];

                CarveCorridor(from, to);
            }

            // Add some random extra connections for loops
            int extraConnections = Mathf.FloorToInt(seedFactor * 3f);
            for (int i = 0; i < extraConnections; i++)
            {
                int a = Random.Range(0, roomCenters.Count);
                int b = Random.Range(0, roomCenters.Count);
                if (a != b)
                {
                    CarveCorridor(roomCenters[a], roomCenters[b]);
                }
            }
        }

        private void CarveCorridor(Vector2Int from, Vector2Int to)
        {
            // FIX: Correct corridor width calculation
            // For width=2, halfWidth=0 → carves exactly 2 cells (not 3)
            int halfWidth = (corridorWidth - 1) / 2;

            // L-shaped corridor: horizontal then vertical
            int minX = Mathf.Min(from.x, to.x);
            int maxX = Mathf.Max(from.x, to.x);

            // Horizontal segment
            for (int x = minX; x <= maxX; x++)
            {
                for (int w = -halfWidth; w <= halfWidth; w++)
                {
                    int gy = from.y + w;
                    if (gy >= 0 && gy < gridSize)
                    {
                        if (grid[x, gy] != GridMazeCell.Room && grid[x, gy] != GridMazeCell.SpawnPoint)
                        {
                            grid[x, gy] = GridMazeCell.Corridor;
                        }
                    }
                }
            }

            // Vertical segment
            int minY = Mathf.Min(from.y, to.y);
            int maxY = Mathf.Max(from.y, to.y);

            for (int y = minY; y <= maxY; y++)
            {
                for (int w = -halfWidth; w <= halfWidth; w++)
                {
                    int gx = to.x + w;
                    if (gx >= 0 && gx < gridSize)
                    {
                        if (grid[gx, y] != GridMazeCell.Room && grid[gx, y] != GridMazeCell.SpawnPoint)
                        {
                            grid[gx, y] = GridMazeCell.Corridor;
                        }
                    }
                }
            }
        }

        #endregion

        #region Step 4: Mark Outer Walls

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
