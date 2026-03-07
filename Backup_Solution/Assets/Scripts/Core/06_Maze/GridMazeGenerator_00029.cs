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
        #region Inspector Fields

        [Header("Grid Settings")]
        [SerializeField] private int _gridSize = 21;
        [SerializeField] private int _roomSize = 5;
        [SerializeField] private int _corridorWidth = 1;

        [Header("Difficulty")]
        [SerializeField] private int _baseRooms = 5;

        #endregion

        #region Private Fields

        private GridMazeCell[,] _grid;
        private Vector2Int _spawnRoomCenter;
        private Vector2Int _spawnEntranceDirection;
        private List<Vector2Int> _roomCenters = new List<Vector2Int>();
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
        public int RoomSize { get => _roomSize; set => _roomSize = value; }
        public int CorridorWidth { get => _corridorWidth; set => _corridorWidth = value; }
        public Vector2Int SpawnRoomCenter => _spawnRoomCenter;
        public Vector2Int SpawnEntranceDirection => _spawnEntranceDirection;
        public Vector2Int SpawnExitDirection => _spawnEntranceDirection * -1;

        #endregion

        #region Initialization

        public void InitializeFromConfig()
        {
            GameConfig config = GameConfig.Instance;
            _gridSize = config.defaultGridSize;
            _roomSize = config.defaultRoomSize;
            _corridorWidth = config.defaultCorridorWidth;

            Debug.Log($"[GridMazeGenerator] Config: {_gridSize}x{_gridSize} grid, {_roomSize}x{_roomSize} rooms");
        }

        #endregion

        #region Main Generation

        /// <summary>
        /// Generate maze with specified seed and difficulty.
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

            Debug.Log($"[GridMazeGenerator] Generating {_gridSize}x{_gridSize} maze...");

            // Step 1: Fill with Floor
            FillGrid();

            // Step 2: Place rooms
            PlaceRooms();

            // Step 3: Connect rooms with corridors (4-way or 8-way)
            ConnectRooms();

            // Step 4: Mark outer walls
            MarkOuterWalls();

            Debug.Log($"[GridMazeGenerator] Maze complete - {_roomCenters.Count} rooms, spawn: {_spawnRoomCenter}");
        }

        #endregion

        #region Step 1: Fill Grid

        private void FillGrid()
        {
            _grid = new GridMazeCell[_gridSize, _gridSize];

            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    _grid[x, y] = GridMazeCell.Floor;
                }
            }

            Debug.Log($"[GridMazeGenerator] Grid filled with Floor");
        }

        #endregion

        #region Step 2: Place Rooms

        private void PlaceRooms()
        {
            GameConfig config = GameConfig.Instance;
            int spawnRoomSize = config.spawnRoomSize;
            int margin = config.spawnRoomMargin;

            // Place spawn room on west edge
            int spawnX = margin;
            int spawnY = (_gridSize / 2) - (spawnRoomSize / 2);
            spawnY = Mathf.Clamp(spawnY, margin, _gridSize - spawnRoomSize - margin);

            PlaceRoom(spawnX, spawnY, spawnRoomSize, true);
            _spawnRoomCenter = new Vector2Int(
                spawnX + spawnRoomSize / 2,
                spawnY + spawnRoomSize / 2
            );
            _spawnEntranceDirection = Vector2Int.right; // Opens east

            // Calculate room count based on maze size and difficulty
            int sizeBonus = Mathf.FloorToInt(_gridSize / 4f);
            int seedVariation = Mathf.FloorToInt(_seedFactor * 4f);
            int roomCount = _baseRooms + sizeBonus + seedVariation;
            roomCount = Mathf.Clamp(roomCount, 8, Mathf.FloorToInt(_gridSize / 1.5f));

            int placed = 1; // spawn room already placed

            Debug.Log($"[GridMazeGenerator] Target rooms: {roomCount} (base:{_baseRooms} + size:{sizeBonus} + seed:{seedVariation})");

            // Divide grid into zones for better spatial distribution
            int zoneSize = _gridSize / 3;
            int roomsPerZone = Mathf.CeilToInt((float)(roomCount - 1) / 6f);

            Debug.Log($"[GridMazeGenerator] Placing {roomCount - 1} rooms across 6 zones ({roomsPerZone} per zone)");

            // Place rooms in 6 zones (3x2 grid) for better coverage
            for (int zx = 0; zx < 3; zx++)
            {
                for (int zy = 0; zy < 2; zy++)
                {
                    // Skip spawn zone (west center)
                    if (zx == 0 && zy == 0)
                    {
                        continue;
                    }

                    int zoneRoomsPlaced = 0;
                    int zoneAttempts = 0;
                    int maxZoneAttempts = 30;

                    // Calculate zone bounds
                    int zxStart = zx * zoneSize + margin;
                    int zyStart = zy * zoneSize + margin;
                    int zxEnd = (zx + 1) * zoneSize - margin - _roomSize;
                    int zyEnd = (zy + 1) * zoneSize - margin - _roomSize;

                    // Clamp to valid range
                    zxStart = Mathf.Max(zxStart, margin);
                    zyStart = Mathf.Max(zyStart, margin);
                    zxEnd = Mathf.Min(zxEnd, _gridSize - _roomSize - margin);
                    zyEnd = Mathf.Min(zyEnd, _gridSize - _roomSize - margin);

                    // Place rooms in this zone
                    while (zoneRoomsPlaced < roomsPerZone && zoneAttempts < maxZoneAttempts && placed < roomCount)
                    {
                        zoneAttempts++;

                        // Random position within zone
                        int rx = Random.Range(zxStart, zxEnd + 1);
                        int ry = Random.Range(zyStart, zyEnd + 1);

                        // Check if overlaps with existing rooms
                        if (!OverlapsExistingRooms(rx, ry, _roomSize))
                        {
                            PlaceRoom(rx, ry, _roomSize, false);
                            placed++;
                            zoneRoomsPlaced++;
                        }
                    }

                    if (zoneRoomsPlaced > 0)
                    {
                        Debug.Log($"[GridMazeGenerator] Zone ({zx},{zy}): {zoneRoomsPlaced} rooms placed");
                    }
                }
            }

            // Fill remaining room count with random placement (fallback)
            int fallbackAttempts = 0;
            while (placed < roomCount && fallbackAttempts < 50)
            {
                fallbackAttempts++;

                // Random position anywhere valid
                int rx = Random.Range(margin + spawnRoomSize + 2, _gridSize - _roomSize - margin);
                int ry = Random.Range(margin, _gridSize - _roomSize - margin);

                // Check if overlaps with existing rooms
                if (!OverlapsExistingRooms(rx, ry, _roomSize))
                {
                    PlaceRoom(rx, ry, _roomSize, false);
                    placed++;
                }
            }

            Debug.Log($"[GridMazeGenerator] Placed {placed} rooms total ({fallbackAttempts} attempts)");
        }

        private void PlaceRoom(int x, int y, int size, bool isSpawn)
        {
            for (int dx = 0; dx < size; dx++)
            {
                for (int dy = 0; dy < size; dy++)
                {
                    int gx = x + dx;
                    int gy = y + dy;

                    if (gx >= 0 && gx < _gridSize && gy >= 0 && gy < _gridSize)
                    {
                        if (isSpawn && dx == size / 2 && dy == size / 2)
                        {
                            _grid[gx, gy] = GridMazeCell.SpawnPoint;
                        }
                        else
                        {
                            _grid[gx, gy] = GridMazeCell.Room;
                        }
                    }
                }
            }

            _roomCenters.Add(new Vector2Int(x + size / 2, y + size / 2));
        }

        private bool OverlapsExistingRooms(int x, int y, int size)
        {
            int margin = 1; // Keep rooms separated by 1 cell (corridors will connect)

            foreach (Vector2Int center in _roomCenters)
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

        #region Step 3: Connect Rooms (8-Way Maze Generation)

        private void ConnectRooms()
        {
            if (_roomCenters.Count < 2)
            {
                return;
            }

            Debug.Log($"[GridMazeGenerator] Connecting {_roomCenters.Count} rooms...");

            // Determine if using 8-way or 4-way based on level
            bool use8Way = _currentLevel >= 3;
            Debug.Log($"[GridMazeGenerator] Using {(use8Way ? "8-way" : "4-way")} maze (level {_currentLevel})");

            // Generate proper maze structure (not just paths!)
            GenerateMazeStructure(use8Way);

            // Connect rooms to maze
            ConnectRoomsToMaze();

            Debug.Log($"[GridMazeGenerator] Rooms connected with {(use8Way ? "8-way" : "4-way")} maze structure");
        }

        /// <summary>
        /// Generate proper maze structure with walls between corridors.
        /// Uses recursive backtracker on a grid where each cell is a potential corridor.
        /// </summary>
        private void GenerateMazeStructure(bool use8Way)
        {
            int[] dx = use8Way ? _directionsX8 : _directionsX4;
            int[] dy = use8Way ? _directionsY8 : _directionsY4;
            int numDirections = use8Way ? 8 : 4;
            int stepSize = 1; // Move 1 cell at a time for dense maze

            // Start from spawn point
            int startX = _spawnRoomCenter.x;
            int startY = _spawnRoomCenter.y;

            // Track visited cells
            bool[,] visited = new bool[_gridSize, _gridSize];

            // Stack for backtracking
            Stack<Vector2Int> stack = new Stack<Vector2Int>();
            
            // Start from spawn room edge (not center)
            Vector2Int startPos = FindSpawnRoomEdge();
            stack.Push(startPos);
            visited[startPos.x, startPos.y] = true;
            _grid[startPos.x, startPos.y] = GridMazeCell.Corridor;

            int carvedCells = 0;
            int targetCells = (_gridSize * _gridSize) / 4; // Carve ~25% of grid (more walls!)

            while (stack.Count > 0 && carvedCells < targetCells)
            {
                Vector2Int current = stack.Peek();

                // Get unvisited neighbors
                List<int> unvisitedDirections = new List<int>();

                for (int i = 0; i < numDirections; i++)
                {
                    int nx = current.x + dx[i] * stepSize;
                    int ny = current.y + dy[i] * stepSize;

                    if (IsValidMazeCell(nx, ny) && !visited[nx, ny])
                    {
                        // Check if neighbor is not inside a room
                        if (_grid[nx, ny] != GridMazeCell.Room && _grid[nx, ny] != GridMazeCell.SpawnPoint)
                        {
                            unvisitedDirections.Add(i);
                        }
                    }
                }

                if (unvisitedDirections.Count > 0)
                {
                    // Choose random direction
                    int dirIndex = unvisitedDirections[Random.Range(0, unvisitedDirections.Count)];
                    int nx = current.x + dx[dirIndex] * stepSize;
                    int ny = current.y + dy[dirIndex] * stepSize;

                    // Carve corridor (only if not in room)
                    if (_grid[nx, ny] != GridMazeCell.Room && _grid[nx, ny] != GridMazeCell.SpawnPoint)
                    {
                        _grid[nx, ny] = GridMazeCell.Corridor;
                        visited[nx, ny] = true;
                        stack.Push(new Vector2Int(nx, ny));
                        carvedCells++;
                    }
                }
                else
                {
                    // Backtrack
                    stack.Pop();
                }
            }

            Debug.Log($"[GridMazeGenerator] Maze structure carved: {carvedCells} corridor cells");
        }

        /// <summary>
        /// Find edge of spawn room to start maze generation.
        /// </summary>
        private Vector2Int FindSpawnRoomEdge()
        {
            // Start from east edge of spawn room (where entrance should be)
            return new Vector2Int(_spawnRoomCenter.x + 3, _spawnRoomCenter.y);
        }

        /// <summary>
        /// Connect room chambers to the generated maze corridors.
        /// Carves entrance from each room to nearest corridor.
        /// </summary>
        private void ConnectRoomsToMaze()
        {
            foreach (Vector2Int roomCenter in _roomCenters)
            {
                // Find nearest corridor cell
                Vector2Int? nearestCorridor = FindNearestCorridor(roomCenter);

                if (nearestCorridor.HasValue)
                {
                    // Carve path from room edge to corridor
                    CarveRoomConnection(roomCenter, nearestCorridor.Value);
                }
            }
        }

        /// <summary>
        /// Find nearest corridor cell to room center.
        /// </summary>
        private Vector2Int? FindNearestCorridor(Vector2Int from)
        {
            int searchRadius = _gridSize / 4;

            for (int radius = 1; radius < searchRadius; radius++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        if (Mathf.Abs(dx) == radius || Mathf.Abs(dy) == radius)
                        {
                            int x = from.x + dx;
                            int y = from.y + dy;

                            if (IsValidMazeCell(x, y) && _grid[x, y] == GridMazeCell.Corridor)
                            {
                                return new Vector2Int(x, y);
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Carve connection from room to corridor.
        /// </summary>
        private void CarveRoomConnection(Vector2Int roomCenter, Vector2Int corridorPos)
        {
            // Simple line carving from room edge to corridor
            int x = roomCenter.x;
            int y = roomCenter.y;

            int dx = (int)Mathf.Sign(corridorPos.x - x);
            int dy = (int)Mathf.Sign(corridorPos.y - y);

            // Move towards corridor
            while (x != corridorPos.x || y != corridorPos.y)
            {
                if (IsValidMazeCell(x, y) && _grid[x, y] != GridMazeCell.Room)
                {
                    _grid[x, y] = GridMazeCell.Corridor;
                }

                // Move in primary direction
                if (x != corridorPos.x)
                {
                    x += dx;
                }
                else if (y != corridorPos.y)
                {
                    y += dy;
                }
            }
        }

        /// <summary>
        /// Check if cell is valid for maze carving.
        /// </summary>
        private bool IsValidMazeCell(int x, int y)
        {
            return x >= 0 && x < _gridSize && y >= 0 && y < _gridSize;
        }

        #endregion

        #region Step 4: Mark Outer Walls

        private void MarkOuterWalls()
        {
            for (int x = 0; x < _gridSize; x++)
            {
                if (_grid[x, 0] != GridMazeCell.SpawnPoint && _grid[x, 0] != GridMazeCell.Room)
                {
                    _grid[x, 0] = GridMazeCell.Wall;
                }
                if (_grid[x, _gridSize - 1] != GridMazeCell.SpawnPoint && _grid[x, _gridSize - 1] != GridMazeCell.Room)
                {
                    _grid[x, _gridSize - 1] = GridMazeCell.Wall;
                }
            }

            for (int y = 0; y < _gridSize; y++)
            {
                if (_grid[0, y] != GridMazeCell.SpawnPoint && _grid[0, y] != GridMazeCell.Room)
                {
                    _grid[0, y] = GridMazeCell.Wall;
                }
                if (_grid[_gridSize - 1, y] != GridMazeCell.SpawnPoint && _grid[_gridSize - 1, y] != GridMazeCell.Room)
                {
                    _grid[_gridSize - 1, y] = GridMazeCell.Wall;
                }
            }

            Debug.Log($"[GridMazeGenerator] Outer walls marked");
        }

        #endregion

        #region Grid Access

        public GridMazeCell GetCell(int x, int y)
        {
            if (x >= 0 && x < _gridSize && y >= 0 && y < _gridSize)
            {
                return _grid[x, y];
            }
            return GridMazeCell.Floor;
        }

        public Vector2Int FindSpawnPoint()
        {
            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    if (_grid[x, y] == GridMazeCell.SpawnPoint)
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }
            return _spawnRoomCenter;
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
