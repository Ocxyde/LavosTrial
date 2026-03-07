// Copyright (C) 2026 Ocxyde
//
// This file is part of PeuImporte.
//
// PeuImporte is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PeuImporte is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PeuImporte.  If not, see <https://www.gnu.org/licenses/>.
// GridMazeGenerator.cs
// Custom grid-based maze generation system - IMPROVED ALGORITHM
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-OUT: Independent maze generator
// - No dependencies on MazeGenerator class
// - Pure grid-based algorithm
// - SPAWN ROOM PLACED FIRST (guaranteed with entrance/exit)
// - Corridors carved TO spawn room
// - Values loaded from GameConfig (no hardcoding)
//
// GENERATION ORDER:
// 1. Create empty grid (all Floor)
// 2. Place SPAWN ROOM first (guaranteed with entrance/exit)
// 3. Place other rooms (random, don't block spawn corridors)
// 4. Carve corridors TO spawn room (guaranteed connection)
// 5. Mark remaining cells as walls
// 6. Add outer walls
//
// Location: Assets/Scripts/Core/06_Maze/

using UnityEngine;
using System.Collections.Generic;

namespace Code.Lavos.Core
{
    /// <summary>
    /// GridMazeGenerator - Custom grid-based maze generation.
    /// Creates maze with SPAWN ROOM FIRST, then other rooms, then corridors.
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

        // Room positions
        private List<Vector2Int> roomCenters = new List<Vector2Int>();

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

            Debug.Log($"[GridMazeGenerator]  Config loaded: {gridSize}x{gridSize} grid, {roomSize}x{roomSize} rooms, {corridorWidth}-cell corridors");
        }

        #endregion

        #region Main Generation

        /// <summary>
        /// Generate complete maze grid with SPAWN ROOM FIRST.
        /// </summary>
        /// <param name="seed">Random seed for reproducible generation</param>
        /// <param name="difficultyFactor">0.0-1.0 difficulty scaling (higher = more rooms, complex)</param>
        public void Generate(uint seed, float difficultyFactor)
        {
            // Store difficulty factor for room/complexity scaling
            seedFactor = Mathf.Clamp01(difficultyFactor);
            
            // Initialize random state for reproducible generation
            Random.InitState((int)seed);
            Debug.Log($"[GridMazeGenerator]  Random seed initialized: {seed} (difficulty: {seedFactor:F2})");

            // Initialize from config if not already set
            if (gridSize == 0) InitializeFromConfig();

            Debug.Log($"[GridMazeGenerator]  Creating {gridSize}x{gridSize} grid...");

            // Step 1: Create empty grid (all Floor)
            CreateEmptyGrid();

            // Step 2: Place SPAWN ROOM first (guaranteed with entrance/exit)
            PlaceSpawnRoom();

            // Step 3: Place other rooms (scaled by difficulty)
            PlaceOtherRooms();

            // Step 4: Carve corridors TO spawn room (guaranteed connection)
            CarveCorridorsToSpawn();

            // Step 5: Add outer walls
            AddOuterWalls();

            Debug.Log($"[GridMazeGenerator]  Maze generated: {roomCenters.Count} rooms, {corridorWidth}-cell corridors");
            Debug.Log($"[GridMazeGenerator]  Spawn room at: {spawnRoomCenter}");
            Debug.Log($"[GridMazeGenerator]  Entrance direction: {spawnEntranceDirection}");
            Debug.Log($"[GridMazeGenerator]  Exit direction: {spawnExitDirection}");
        }

        #endregion

        #region Step 1: Create Empty Grid

        /// <summary>
        /// Step 1: Create empty grid (all Floor).
        /// </summary>
        private void CreateEmptyGrid()
        {
            grid = new GridMazeCell[gridSize, gridSize];

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    grid[x, y] = GridMazeCell.Floor;
                }
            }

            Debug.Log($"[GridMazeGenerator]  Empty grid created (all Floor)");
        }

        #endregion

        #region Step 2: Place Spawn Room (PRIORITY #1)

        /// <summary>
        /// Step 2: Place SPAWN ROOM first (guaranteed with entrance/exit).
        /// Spawn room is 3x3 WALKABLE room with walls on PERIMETER only.
        /// ONE WALL is OPEN (west side) for entrance/exit with door.
        /// Player spawns in CENTER of walkable room.
        /// </summary>
        private void PlaceSpawnRoom()
        {
            Debug.Log($"[GridMazeGenerator]  STEP 2: Placing SPAWN ROOM (3x3, priority #1)...");

            // Load spawn room settings from GameConfig
            var cfg = GameConfig.Instance;
            int spawnRoomSize = cfg.spawnRoomSize; // Default 3
            int margin = cfg.spawnRoomMargin; // Default 2
            int spawnPointX = cfg.spawnPointInRoomX; // Default 1 (center)
            int spawnPointY = cfg.spawnPointInRoomY; // Default 1 (center)

            // Place spawn room with margin from edges
            int spawnRoomX = Mathf.Max(margin, gridSize / 4);
            int spawnRoomY = Mathf.Max(margin, gridSize / 2 - spawnRoomSize / 2);

            // Mark spawn room area as WALKABLE ROOM (walls will be placed on perimeter)
            // Only WEST side (dx=0) is OPEN for door/corridor
            for (int dx = 0; dx < spawnRoomSize; dx++)
            {
                for (int dy = 0; dy < spawnRoomSize; dy++)
                {
                    int gridX = spawnRoomX + dx;
                    int gridY = spawnRoomY + dy;

                    if (gridX >= 0 && gridX < gridSize && gridY >= 0 && gridY < gridSize)
                    {
                        // CENTER cell is the SpawnPoint
                        if (dx == spawnPointX && dy == spawnPointY)
                        {
                            grid[gridX, gridY] = GridMazeCell.SpawnPoint;
                            spawnRoomCenter = new Vector2Int(gridX, gridY);
                            Debug.Log($"[GridMazeGenerator]  SpawnPoint (CENTER) at ({gridX}, {gridY})");
                        }
                        // WEST WALL (dx=0) is OPEN for door/entrance (Floor cell)
                        else if (dx == 0)
                        {
                            grid[gridX, gridY] = GridMazeCell.Floor;
                        }
                        else
                        {
                            // Rest of spawn room is WALKABLE (Room cell)
                            // Walls will be spawned on perimeter by PlaceInteriorWalls()
                            grid[gridX, gridY] = GridMazeCell.Room;
                        }
                    }
                }
            }

            // Set entrance/exit direction (west side only)
            spawnEntranceDirection = Vector2Int.left;   // Left wall open
            spawnExitDirection = Vector2Int.right;       // Right wall blocked

            roomCenters.Add(new Vector2Int(spawnRoomX + spawnPointX, spawnRoomY + spawnPointY));

            Debug.Log($"[GridMazeGenerator]  Spawn room placed at ({spawnRoomX}, {spawnRoomY}) - {spawnRoomSize}x{spawnRoomSize}");
            Debug.Log($"[GridMazeGenerator]  ONE OPENING: WEST side only (for door)");
            Debug.Log($"[GridMazeGenerator]  Player protected: Walls on NORTH+SOUTH+EAST perimeter");
        }

        #endregion

        #region Step 3: Place Other Rooms

        /// <summary>
        /// Step 3: Place other rooms (random, don't block spawn corridors).
        /// </summary>
        private void PlaceOtherRooms()
        {
            Debug.Log($"[GridMazeGenerator]  STEP 3: Placing other rooms...");

            // Scale room count by difficulty factor (from GameConfig)
            var cfg = GameConfig.Instance;
            int baseRooms = Random.Range(cfg.baseRoomMin, cfg.baseRoomMax);
            int bonusRooms = Mathf.FloorToInt(seedFactor * cfg.maxDifficultyRoomBonus);
            int additionalRooms = baseRooms + bonusRooms;

            Debug.Log($"[GridMazeGenerator]  Placing {additionalRooms} rooms (base: {baseRooms}, bonus: {bonusRooms})");

            for (int i = 0; i < additionalRooms; i++)
            {
                Vector2Int roomPos = FindValidRoomPosition();
                if (roomPos.x >= 0)
                {
                    PlaceRoom(roomPos.x, roomPos.y, $"Room{i+1}");
                }
                else
                {
                    Debug.LogWarning($"[GridMazeGenerator] ️ Could not place room {i+1} (no valid position)");
                }
            }

            Debug.Log($"[GridMazeGenerator] ️ Total rooms: {roomCenters.Count}");
        }

        /// <summary>
        /// Find valid position for a room (not blocking spawn corridors).
        /// </summary>
        private Vector2Int FindValidRoomPosition()
        {
            int attempts = 30;

            for (int i = 0; i < attempts; i++)
            {
                int x = Random.Range(1, gridSize - roomSize - 1);
                int y = Random.Range(1, gridSize - roomSize - 1);

                // Skip spawn room area and its corridors
                if (IsInSpawnCorridorArea(x, y))
                    continue;

                // Check if position overlaps existing rooms
                bool overlaps = false;
                for (int dx = -1; dx <= roomSize; dx++)
                {
                    for (int dy = -1; dy <= roomSize; dy++)
                    {
                        int checkX = x + dx;
                        int checkY = y + dy;

                        if (checkX >= 0 && checkX < gridSize && checkY >= 0 && checkY < gridSize)
                        {
                            if (grid[checkX, checkY] == GridMazeCell.Room || 
                                grid[checkX, checkY] == GridMazeCell.SpawnPoint)
                            {
                                overlaps = true;
                                break;
                            }
                        }
                    }
                    if (overlaps) break;
                }

                if (!overlaps)
                {
                    return new Vector2Int(x, y);
                }
            }

            return new Vector2Int(-1, -1);  // No valid position found
        }

        /// <summary>
        /// Check if position is in spawn room corridor area.
        /// </summary>
        private bool IsInSpawnCorridorArea(int x, int y)
        {
            // Avoid horizontal corridor line from spawn room
            int spawnY = spawnRoomCenter.y;
            int corridorRange = corridorWidth + 1;
            
            if (Mathf.Abs(y - spawnY) <= corridorRange)
            {
                return true;  // Too close to spawn corridors
            }

            return false;
        }

        /// <summary>
        /// Place a single room at position.
        /// </summary>
        private void PlaceRoom(int x, int y, string roomType)
        {
            // Mark room area as Room (clear space)
            for (int dx = 0; dx < roomSize; dx++)
            {
                for (int dy = 0; dy < roomSize; dy++)
                {
                    int gridX = x + dx;
                    int gridY = y + dy;

                    if (gridX >= 0 && gridX < gridSize && gridY >= 0 && gridY < gridSize)
                    {
                        grid[gridX, gridY] = GridMazeCell.Room;
                    }
                }
            }

            roomCenters.Add(new Vector2Int(x + roomSize/2, y + roomSize/2));
            Debug.Log($"[GridMazeGenerator] ️ {roomType} placed at ({x}, {y})");
        }

        #endregion

        #region Step 4: Carve Corridors TO Spawn Room

        /// <summary>
        /// Step 4: Carve corridors TO spawn room (guaranteed connection).
        /// </summary>
        private void CarveCorridorsToSpawn()
        {
            Debug.Log($"[GridMazeGenerator]  STEP 4: Carving corridors TO spawn room...");

            // Carve entrance corridor TO spawn room (from left)
            CarveCorridorToSpawn(spawnEntranceDirection);

            // Carve exit corridor FROM spawn room (to right)
            CarveCorridorFromSpawn(spawnExitDirection);

            // Connect other rooms with L-shaped corridors
            ConnectOtherRooms();

            Debug.Log($"[GridMazeGenerator]  Corridors carved ({corridorWidth} cells wide)");
        }

        /// <summary>
        /// Carve corridor TO spawn room from specified direction.
        /// </summary>
        private void CarveCorridorToSpawn(Vector2Int direction)
        {
            int startX = direction == Vector2Int.left ? 0 : spawnRoomCenter.x;
            int endX = direction == Vector2Int.left ? spawnRoomCenter.x : gridSize - 1;
            int y = spawnRoomCenter.y;

            CarveHorizontalCorridor(startX, endX, y);
        }

        /// <summary>
        /// Carve corridor FROM spawn room to specified direction.
        /// </summary>
        private void CarveCorridorFromSpawn(Vector2Int direction)
        {
            int startX = spawnRoomCenter.x;
            int endX = direction == Vector2Int.right ? gridSize - 1 : 0;
            int y = spawnRoomCenter.y;

            CarveHorizontalCorridor(startX, endX, y);
        }

        /// <summary>
        /// Carve horizontal corridor.
        /// </summary>
        private void CarveHorizontalCorridor(int startX, int endX, int y)
        {
            int halfWidth = corridorWidth / 2;
            int minX = Mathf.Min(startX, endX);
            int maxX = Mathf.Max(startX, endX);

            for (int x = minX; x <= maxX; x++)
            {
                for (int w = -halfWidth; w <= halfWidth; w++)
                {
                    int checkY = y + w;
                    if (x >= 0 && x < gridSize && checkY >= 0 && checkY < gridSize)
                    {
                        // Don't overwrite rooms or spawn point
                        if (grid[x, checkY] != GridMazeCell.Room && 
                            grid[x, checkY] != GridMazeCell.SpawnPoint)
                        {
                            grid[x, checkY] = GridMazeCell.Corridor;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Connect other rooms with L-shaped corridors.
        /// </summary>
        private void ConnectOtherRooms()
        {
            if (roomCenters.Count < 2) return;

            // Connect each room to the next
            for (int i = 1; i < roomCenters.Count; i++)
            {
                Vector2Int start = roomCenters[i - 1];
                Vector2Int end = roomCenters[i];

                CarveLShapedCorridor(start, end);
            }
        }

        /// <summary>
        /// Carve L-shaped corridor between two points.
        /// Preserves SpawnPoint and Room cells.
        /// </summary>
        private void CarveLShapedCorridor(Vector2Int start, Vector2Int end)
        {
            int halfWidth = corridorWidth / 2;

            // Horizontal segment
            int minX = Mathf.Min(start.x, end.x);
            int maxX = Mathf.Max(start.x, end.x);

            for (int x = minX - halfWidth; x <= maxX + halfWidth; x++)
            {
                for (int w = -halfWidth; w <= halfWidth; w++)
                {
                    int y = start.y + w;
                    if (x >= 0 && x < gridSize && y >= 0 && y < gridSize)
                    {
                        // NEVER overwrite Room or SpawnPoint!
                        if (grid[x, y] != GridMazeCell.Room && grid[x, y] != GridMazeCell.SpawnPoint)
                        {
                            grid[x, y] = GridMazeCell.Corridor;
                        }
                    }
                }
            }

            // Vertical segment
            int minY = Mathf.Min(start.y, end.y);
            int maxY = Mathf.Max(start.y, end.y);

            for (int y = minY - halfWidth; y <= maxY + halfWidth; y++)
            {
                for (int w = -halfWidth; w <= halfWidth; w++)
                {
                    int x = end.x + w;
                    if (x >= 0 && x < gridSize && y >= 0 && y < gridSize)
                    {
                        // NEVER overwrite Room or SpawnPoint!
                        if (grid[x, y] != GridMazeCell.Room && grid[x, y] != GridMazeCell.SpawnPoint)
                        {
                            grid[x, y] = GridMazeCell.Corridor;
                        }
                    }
                }
            }
        }

        #endregion

        #region Step 5: Add Outer Walls

        /// <summary>
        /// Step 5: Add outer walls (maze perimeter).
        /// Preserves SpawnPoint and Room cells.
        /// </summary>
        private void AddOuterWalls()
        {
            Debug.Log($"[GridMazeGenerator]  STEP 5: Adding outer walls...");

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

            Debug.Log($"[GridMazeGenerator]  Outer walls added");
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
                        Debug.Log($"[GridMazeGenerator]  SpawnPoint found at ({x}, {y})");
                        return new Vector2Int(x, y);
                    }
                }
            }

            // Fallback to spawn room center
            Debug.LogWarning("[GridMazeGenerator] ️ No SpawnPoint found - using spawn room center");
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

            Debug.Log($"[GridMazeGenerator]  Serialized grid: {totalBytes} bytes");
            return data;
        }

        /// <summary>
        /// Deserialize grid from byte array.
        /// </summary>
        public void DeserializeFromBytes(byte[] data)
        {
            if (data == null || data.Length < 2)
            {
                Debug.LogError("[GridMazeGenerator]  Invalid data for deserialization");
                return;
            }

            gridSize = data[0];
            int expectedSize = 2 + (gridSize * gridSize);

            if (data.Length != expectedSize)
            {
                Debug.LogError($"[GridMazeGenerator]  Data size mismatch: expected {expectedSize}, got {data.Length}");
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

            Debug.Log($"[GridMazeGenerator]  Deserialized grid: {data.Length} bytes");
        }

        #endregion
    }
}
