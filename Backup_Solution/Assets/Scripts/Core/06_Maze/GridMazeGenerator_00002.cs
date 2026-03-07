// GridMazeGenerator.cs
// Custom grid-based maze generation system
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-OUT: Independent maze generator
// - No dependencies on MazeGenerator class
// - Pure grid-based algorithm
// - Rooms placed first, corridors carved after
//
// Location: Assets/Scripts/Core/06_Maze/

using UnityEngine;
using System.Collections.Generic;

namespace Code.Lavos.Core
{
    /// <summary>
    /// GridMazeGenerator - Custom grid-based maze generation.
    /// Creates maze with rooms and 2-cell wide corridors.
    /// Values loaded from GameConfig (no hardcoding).
    /// </summary>
    public class GridMazeGenerator
    {
        // Grid settings - loaded from GameConfig
        public int gridSize;
        public int roomSize;
        public int corridorWidth;

        // The grid
        private GridMazeCell[,] grid;

        // Room positions
        private List<Vector2Int> roomCenters = new List<Vector2Int>();

        // Public access to grid
        public GridMazeCell[,] Grid => grid;
        public int GridSize => gridSize;

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
            
            Debug.Log($"[GridMazeGenerator] 📋 Config loaded: {gridSize}x{gridSize} grid, {roomSize}x{roomSize} rooms, {corridorWidth}-cell corridors");
        }

        /// <summary>
        /// Generate complete maze grid.
        /// </summary>
        public void Generate()
        {
            // Initialize from config if not already set
            if (gridSize == 0) InitializeFromConfig();
            
            Debug.Log($"[GridMazeGenerator] 🔲 Creating {gridSize}x{gridSize} grid...");
            
            // Step 1: Create empty grid (all Floor)
            CreateEmptyGrid();
            
            // Step 2: Place rooms (5x5 clear areas)
            PlaceRooms();
            
            // Step 3: Carve 2-cell wide corridors
            CarveCorridors();
            
            // Step 4: Add outer walls
            AddOuterWalls();
            
            Debug.Log($"[GridMazeGenerator] ✅ Maze generated: {roomCenters.Count} rooms, {corridorWidth}-cell corridors");
        }
        
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
            
            Debug.Log($"[GridMazeGenerator] 📄 Empty grid created (all Floor)");
        }
        
        /// <summary>
        /// Step 2: Place rooms (5x5 clear areas).
        /// Marks center cell of entrance room as SpawnPoint.
        /// </summary>
        private void PlaceRooms()
        {
            Debug.Log($"[GridMazeGenerator] 🏛️ Placing rooms (size: {roomSize}x{roomSize})...");

            // Place entrance room (top-left quadrant)
            int entranceX = 2;
            int entranceY = 2;
            PlaceRoom(entranceX, entranceY, "Entrance");
            Vector2Int entranceCenter = new Vector2Int(entranceX + roomSize/2, entranceY + roomSize/2);
            roomCenters.Add(entranceCenter);
            Debug.Log($"[GridMazeGenerator] 🎯 Entrance room center (SpawnPoint): {entranceCenter}");

            // Place exit room (bottom-right quadrant)
            int exitX = gridSize - roomSize - 2;
            int exitY = gridSize - roomSize - 2;
            PlaceRoom(exitX, exitY, "Exit");
            roomCenters.Add(new Vector2Int(exitX + roomSize/2, exitY + roomSize/2));

            // Place 1-2 normal rooms (random positions)
            int normalRooms = Random.Range(1, 3);
            for (int i = 0; i < normalRooms; i++)
            {
                Vector2Int roomPos = FindValidRoomPosition();
                if (roomPos.x >= 0)
                {
                    PlaceRoom(roomPos.x, roomPos.y, $"Normal{i+1}");
                    roomCenters.Add(new Vector2Int(roomPos.x + roomSize/2, roomPos.y + roomSize/2));
                }
                else
                {
                    Debug.LogWarning($"[GridMazeGenerator] ⚠️ Could not place normal room {i+1} (no valid position)");
                }
            }

            Debug.Log($"[GridMazeGenerator] 🏛️ {roomCenters.Count} rooms placed ({roomSize}x{roomSize} each)");
            
            // Verify SpawnPoint exists
            bool hasSpawnPoint = false;
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    if (grid[x, y] == GridMazeCell.SpawnPoint)
                    {
                        hasSpawnPoint = true;
                        Debug.Log($"[GridMazeGenerator] ✅ SpawnPoint verified at ({x}, {y})");
                    }
                }
            }
            
            if (!hasSpawnPoint)
            {
                Debug.LogError($"[GridMazeGenerator] ❌ NO SpawnPoint found after room placement!");
            }
        }
        
        /// <summary>
        /// Place a single room at position.
        /// Marks center cell as SpawnPoint for player spawn/respawn.
        /// </summary>
        private void PlaceRoom(int x, int y, string roomType)
        {
            // Mark 5x5 area as Room (clear space)
            for (int dx = 0; dx < roomSize; dx++)
            {
                for (int dy = 0; dy < roomSize; dy++)
                {
                    int gridX = x + dx;
                    int gridY = y + dy;
                    
                    if (gridX >= 0 && gridX < gridSize && gridY >= 0 && gridY < gridSize)
                    {
                        // Center cell of 5x5 room is the SpawnPoint!
                        if (dx == roomSize / 2 && dy == roomSize / 2)
                        {
                            grid[gridX, gridY] = GridMazeCell.SpawnPoint;
                            Debug.Log($"[GridMazeGenerator] 🎯 SpawnPoint marked at ({gridX}, {gridY}) for {roomType} room");
                        }
                        else
                        {
                            grid[gridX, gridY] = GridMazeCell.Room;
                        }
                    }
                }
            }
            
            Debug.Log($"[GridMazeGenerator] 🏛️ {roomType} room placed at ({x}, {y}) - SpawnPoint at center");
        }
        
        /// <summary>
        /// Find valid position for a room (not overlapping existing rooms).
        /// </summary>
        private Vector2Int FindValidRoomPosition()
        {
            int attempts = 20;
            
            for (int i = 0; i < attempts; i++)
            {
                int x = Random.Range(1, gridSize - roomSize - 1);
                int y = Random.Range(1, gridSize - roomSize - 1);
                
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
                            if (grid[checkX, checkY] == GridMazeCell.Room)
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
        /// Step 3: Carve 2-cell wide corridors between rooms.
        /// Uses simple L-shaped paths (horizontal + vertical).
        /// </summary>
        private void CarveCorridors()
        {
            if (roomCenters.Count < 2) return;
            
            // Connect rooms in sequence
            for (int i = 0; i < roomCenters.Count - 1; i++)
            {
                Vector2Int start = roomCenters[i];
                Vector2Int end = roomCenters[i + 1];
                
                CarveLShapedCorridor(start, end);
            }
            
            Debug.Log($"[GridMazeGenerator] 🔨 Corridors carved ({corridorWidth} cells wide)");
        }
        
        /// <summary>
        /// Carve L-shaped corridor between two points.
        /// Preserves SpawnPoint cells (never overwrites them).
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
        
        /// <summary>
        /// Step 4: Add outer walls (maze perimeter).
        /// Preserves SpawnPoint cells (never overwrites them).
        /// </summary>
        private void AddOuterWalls()
        {
            // Mark outer edges as Wall (but NOT SpawnPoint!)
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

            Debug.Log($"[GridMazeGenerator] 🧱 Outer walls added");
        }
        
        /// <summary>
        /// Check if cell has wall in specific direction.
        /// Used by wall spawning system.
        /// </summary>
        public bool HasWall(int x, int y, int direction)
        {
            if (x < 0 || x >= gridSize || y < 0 || y >= gridSize) return false;
            
            // Rooms and Corridors don't have internal walls
            if (grid[x, y] == GridMazeCell.Room || grid[x, y] == GridMazeCell.Corridor)
            {
                return false;
            }
            
            // Floor cells have walls on all sides
            if (grid[x, y] == GridMazeCell.Floor)
            {
                return true;
            }
            
            return false;
        }
        
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
        /// Returns the center cell of the entrance room.
        /// </summary>
        public Vector2Int FindSpawnPoint()
        {
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    if (grid[x, y] == GridMazeCell.SpawnPoint)
                    {
                        Debug.Log($"[GridMazeGenerator] 🎯 SpawnPoint found at ({x}, {y})");
                        return new Vector2Int(x, y);
                    }
                }
            }
            
            // Fallback to center of grid
            Debug.LogWarning("[GridMazeGenerator] ⚠️ No SpawnPoint found - using grid center");
            return new Vector2Int(gridSize / 2, gridSize / 2);
        }
        
        /// <summary>
        /// Serialize entire grid to compact byte array.
        /// 1 byte per cell = gridSize × gridSize bytes total.
        /// Format: [width][height][cell0][cell1]...[cellN]
        /// </summary>
        public byte[] SerializeToBytes()
        {
            int totalBytes = 2 + (gridSize * gridSize);  // width + height + cells
            byte[] data = new byte[totalBytes];
            
            // Header: grid dimensions
            data[0] = (byte)gridSize;
            data[1] = (byte)gridSize;
            
            // Cell data: 1 byte per cell
            int index = 2;
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    data[index++] = (byte)grid[x, y];
                }
            }
            
            Debug.Log($"[GridMazeGenerator] 💾 Serialized grid: {totalBytes} bytes ({gridSize}x{gridSize})");
            return data;
        }
        
        /// <summary>
        /// Deserialize grid from byte array.
        /// Reconstructs grid from compact binary format.
        /// </summary>
        public void DeserializeFromBytes(byte[] data)
        {
            if (data == null || data.Length < 2)
            {
                Debug.LogError("[GridMazeGenerator] ❌ Invalid data for deserialization");
                return;
            }
            
            // Read header
            gridSize = data[0];
            int expectedSize = 2 + (gridSize * gridSize);
            
            if (data.Length != expectedSize)
            {
                Debug.LogError($"[GridMazeGenerator] ❌ Data size mismatch: expected {expectedSize}, got {data.Length}");
                return;
            }
            
            // Create grid
            grid = new GridMazeCell[gridSize, gridSize];
            
            // Read cell data
            int index = 2;
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    grid[x, y] = (GridMazeCell)data[index++];
                }
            }
            
            Debug.Log($"[GridMazeGenerator] 📂 Deserialized grid: {data.Length} bytes ({gridSize}x{gridSize})");
        }
        
        /// <summary>
        /// Get grid data as base64 string (for JSON storage).
        /// </summary>
        public string SerializeToBase64()
        {
            byte[] data = SerializeToBytes();
            return System.Convert.ToBase64String(data);
        }
        
        /// <summary>
        /// Load grid from base64 string.
        /// </summary>
        public void DeserializeFromBase64(string base64)
        {
            if (string.IsNullOrEmpty(base64))
            {
                Debug.LogError("[GridMazeGenerator] ❌ Empty base64 data");
                return;
            }
            
            byte[] data = System.Convert.FromBase64String(base64);
            DeserializeFromBytes(data);
        }
    }
}
