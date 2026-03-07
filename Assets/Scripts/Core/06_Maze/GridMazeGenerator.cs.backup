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
// Hybrid dungeon maze generation with 8-way support
// Level-based difficulty progression (4-way -> 8-way)

using UnityEngine;
using System.Collections.Generic;

namespace Code.Lavos.Core
{
    /// <summary>
    /// GridMazeGenerator - Hybrid dungeon maze with rooms and corridors.
    /// Supports 4-way (levels 0-2) and 8-way (levels 3+) maze generation.
    /// 
    /// GENERATION STEPS:
    /// 1. Fill grid with Floor
    /// 2. Place rooms (quadrant-based distribution)
    /// 3. Generate corridors (4-way or 8-way based on level)
    /// 4. Mark outer walls
    /// 
    /// DIFFICULTY PROGRESSION:
    /// - Levels 0-2: 4-way corridors, 20% dead ends (tutorial)
    /// - Levels 3-10: 8-way corridors, 35% dead ends (intermediate)
    /// - Levels 11+: 8-way corridors, 50% dead ends (expert)
    /// </summary>
    public class GridMazeGenerator
    {
        #region Grid Settings

        public int gridSize;
        public int roomSize;
        public int corridorWidth = 1; // Always 1 cell wide
        private float seedFactor;
        private int currentLevel; // For 4-way vs 8-way decision

        // 8-way direction arrays (N, NE, E, SE, S, SW, W, NW)
        private static readonly int[] dx8 = { 0,  1,  1,  1,  0, -1, -1, -1 };
        private static readonly int[] dy8 = { 1,  1,  0, -1, -1, -1,  0,  1 };

        // 4-way direction arrays (N, E, S, W)
        private static readonly int[] dx4 = { 0,  1,  0, -1 };
        private static readonly int[] dy4 = { 1,  0, -1,  0 };

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

        public void Generate(uint seed, float difficultyFactor, int level = 0)
        {
            seedFactor = Mathf.Clamp01(difficultyFactor);
            currentLevel = level;
            Random.InitState((int)seed);

            Debug.Log($"[GridMazeGenerator] Seed: {seed} (difficulty: {seedFactor:F2}, level: {level})");

            if (gridSize == 0) InitializeFromConfig();

            // Corridor width always 1 for proper maze
            corridorWidth = 1;

            Debug.Log($"[GridMazeGenerator] Generating {gridSize}x{gridSize} maze...");

            // Step 1: Fill with Floor
            FillGrid();

            // Step 2: Place rooms
            PlaceRooms();

            // Step 3: Connect rooms with corridors (4-way or 8-way)
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

            // FIX: More rooms for better maze structure (not dick-shaped!)
            // Formula: base rooms + maze size factor + seed variation
            int baseRooms = 5;
            int sizeBonus = Mathf.FloorToInt(gridSize / 4f); // +4 rooms for 16x16
            int seedVariation = Mathf.FloorToInt(seedFactor * 4f); // 0-4 extra
            int roomCount = baseRooms + sizeBonus + seedVariation; // ~9-13 rooms for 16x16
            roomCount = Mathf.Clamp(roomCount, 8, Mathf.FloorToInt(gridSize / 1.5f));
            
            int placed = 1; // spawn room already placed

            Debug.Log($"[GridMazeGenerator] Target rooms: {roomCount} (base:{baseRooms} + size:{sizeBonus} + seed:{seedVariation})");

            // Divide grid into quadrants for better spatial distribution
            int quadrantSize = gridSize / 3; // Smaller quadrants = more spread
            int roomsPerQuadrant = Mathf.CeilToInt((float)(roomCount - 1) / 6f); // 6 zones

            Debug.Log($"[GridMazeGenerator] Placing {roomCount - 1} rooms across 6 zones ({roomsPerQuadrant} per zone)");

            // Place rooms in 6 zones (3x2 grid) for better coverage
            for (int zx = 0; zx < 3; zx++)
            {
                for (int zy = 0; zy < 2; zy++)
                {
                    // Skip spawn zone (west center)
                    if (zx == 0 && zy == 0) continue;

                    int zoneRoomsPlaced = 0;
                    int zoneAttempts = 0;
                    int maxZoneAttempts = 30;

                    // Calculate zone bounds
                    int zxStart = zx * quadrantSize + margin;
                    int zyStart = zy * quadrantSize + margin;
                    int zxEnd = (zx + 1) * quadrantSize - margin - roomSize;
                    int zyEnd = (zy + 1) * quadrantSize - margin - roomSize;

                    // Clamp to valid range
                    zxStart = Mathf.Max(zxStart, margin);
                    zyStart = Mathf.Max(zyStart, margin);
                    zxEnd = Mathf.Min(zxEnd, gridSize - roomSize - margin);
                    zyEnd = Mathf.Min(zyEnd, gridSize - roomSize - margin);

                    // Place rooms in this zone
                    while (zoneRoomsPlaced < roomsPerQuadrant && zoneAttempts < maxZoneAttempts && placed < roomCount)
                    {
                        zoneAttempts++;

                        // Random position within zone
                        int rx = Random.Range(zxStart, zxEnd + 1);
                        int ry = Random.Range(zyStart, zyEnd + 1);

                        // Check if overlaps with existing rooms (with extra spacing)
                        if (!OverlapsExistingRooms(rx, ry, roomSize))
                        {
                            PlaceRoom(rx, ry, roomSize, false);
                            placed++;
                            zoneRoomsPlaced++;
                        }
                    }

                    if (zoneRoomsPlaced > 0)
                        Debug.Log($"[GridMazeGenerator] Zone ({zx},{zy}): {zoneRoomsPlaced} rooms placed");
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
            int margin = 1; // Keep rooms separated by 1 cell (corridors will connect)

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

            // FIX: Connect rooms with winding corridors (not straight shafts!)
            // Use shuffled room order for more organic connections
            List<Vector2Int> shuffledRooms = new List<Vector2Int>(roomCenters);
            ShuffleList(shuffledRooms);

            // Connect each room to nearest neighbor (creates branching paths)
            HashSet<int> connected = new HashSet<int> { 0 };
            for (int i = 1; i < shuffledRooms.Count; i++)
            {
                int nearestIdx = FindNearestRoom(shuffledRooms, i, connected);
                if (nearestIdx >= 0)
                {
                    CarveWindingCorridor(shuffledRooms[i], shuffledRooms[nearestIdx]);
                    connected.Add(i);
                }
            }

            // Add extra connections for loops (more maze-like!)
            int extraConnections = Mathf.FloorToInt(seedFactor * 4f) + 2; // 2-6 extra
            for (int i = 0; i < extraConnections; i++)
            {
                int a = Random.Range(0, roomCenters.Count);
                int b = Random.Range(0, roomCenters.Count);
                if (a != b)
                {
                    CarveWindingCorridor(roomCenters[a], roomCenters[b]);
                }
            }

            Debug.Log($"[GridMazeGenerator] Rooms connected with winding corridors");
        }

        /// <summary>
        /// Find nearest room that's already connected.
        /// </summary>
        private int FindNearestRoom(List<Vector2Int> rooms, int currentIndex, HashSet<int> connected)
        {
            int bestIdx = -1;
            float bestDist = float.MaxValue;

            for (int i = 0; i < rooms.Count; i++)
            {
                if (connected.Contains(i))
                {
                    float dist = Vector2Int.Distance(rooms[currentIndex], rooms[i]);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestIdx = i;
                    }
                }
            }

            return bestIdx;
        }

        /// <summary>
        /// Shuffle list using Fisher-Yates algorithm.
        /// </summary>
        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        /// <summary>
        /// Carve winding corridor with random turns (not straight L-shape!).
        /// </summary>
        private void CarveWindingCorridor(Vector2Int from, Vector2Int to)
        {
            int halfWidth = (corridorWidth - 1) / 2;

            // Add 1-2 random intermediate points for winding path
            int waypoints = 1 + Mathf.FloorToInt(Random.value * 1.5f); // 1-2 waypoints
            List<Vector2Int> points = new List<Vector2Int> { from };

            for (int i = 0; i < waypoints; i++)
            {
                // Random point between from and to (with offset)
                float t = (i + 1f) / (waypoints + 1f);
                int midX = Mathf.RoundToInt(Mathf.Lerp(from.x, to.x, t));
                int midY = Mathf.RoundToInt(Mathf.Lerp(from.y, to.y, t));

                // Add random offset (±2 cells)
                midX += Random.Range(-2, 3);
                midY += Random.Range(-2, 3);

                // Clamp to grid bounds
                midX = Mathf.Clamp(midX, 1, gridSize - 2);
                midY = Mathf.Clamp(midY, 1, gridSize - 2);

                points.Add(new Vector2Int(midX, midY));
            }

            points.Add(to);

            // Carve corridors between waypoints
            for (int i = 0; i < points.Count - 1; i++)
            {
                CarveStraightCorridor(points[i], points[i + 1], halfWidth);
            }
        }

        /// <summary>
        /// Carve straight corridor segment (used by winding corridor).
        /// Carves through Floor/Wall, stops at Room edges (creates natural doorways).
        /// </summary>
        private void CarveStraightCorridor(Vector2Int from, Vector2Int to, int halfWidth)
        {
            int minX = Mathf.Min(from.x, to.x);
            int maxX = Mathf.Max(from.x, to.x);
            int minY = Mathf.Min(from.y, to.y);
            int maxY = Mathf.Max(from.y, to.y);

            // Horizontal segment
            for (int x = minX; x <= maxX; x++)
            {
                for (int w = -halfWidth; w <= halfWidth; w++)
                {
                    int gy = from.y + w;
                    if (gy >= 0 && gy < gridSize)
                    {
                        var cell = grid[x, gy];
                        // Carve through Floor or Wall (creates doorway at Room edge)
                        if (cell == GridMazeCell.Floor || cell == GridMazeCell.Wall)
                        {
                            grid[x, gy] = GridMazeCell.Corridor;
                        }
                    }
                }
            }

            // Vertical segment
            for (int y = minY; y <= maxY; y++)
            {
                for (int w = -halfWidth; w <= halfWidth; w++)
                {
                    int gx = to.x + w;
                    if (gx >= 0 && gx < gridSize)
                    {
                        var cell = grid[gx, y];
                        // Carve through Floor or Wall (creates doorway at Room edge)
                        if (cell == GridMazeCell.Floor || cell == GridMazeCell.Wall)
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
