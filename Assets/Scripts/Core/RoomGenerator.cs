// RoomGenerator.cs
// Procedural room generation for maze-based dungeons
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Generates rectangular rooms with entrances/exits that integrate with maze walls

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Generates procedural rooms that integrate with maze generator.
    /// Rooms have guaranteed entrances and exits for player passage.
    /// </summary>
    public class RoomGenerator : MonoBehaviour
    {
        [Header("Room Settings")]
        [SerializeField] private int minRoomWidth = 3;
        [SerializeField] private int maxRoomWidth = 8;
        [SerializeField] private int minRoomHeight = 3;
        [SerializeField] private int maxRoomHeight = 8;
        
        [Header("Room Density")]
        [SerializeField] [Range(0f, 1f)] private float roomDensity = 0.3f;  // Reserved for density-based placement
        [SerializeField] private int minRooms = 1;
        [SerializeField] private int maxRooms = 5;
        
        [Header("Room Types")]
        [SerializeField] private bool allowSpecialRooms = true;
        [SerializeField] private float specialRoomChance = 0.2f;
        
        [Header("References")]
        [SerializeField] private MazeGenerator mazeGenerator;
        
        private List<RoomData> _generatedRooms = new();
        private RoomData[,] _roomGrid;
        
        public List<RoomData> GeneratedRooms => _generatedRooms;
        public int RoomCount => _generatedRooms.Count;
        
        private void Awake()
        {
            if (mazeGenerator == null)
            {
                mazeGenerator = GetComponent<MazeGenerator>();
            }
        }
        
        /// <summary>
        /// Generate rooms integrated with the maze.
        /// Call after maze generation.
        /// </summary>
        public void GenerateRooms()
        {
            if (mazeGenerator == null || mazeGenerator.Grid == null)
            {
                Debug.LogError("[RoomGenerator] MazeGenerator not initialized!");
                return;
            }
            
            _generatedRooms.Clear();
            
            int targetRooms = Random.Range(minRooms, maxRooms + 1);
            int attempts = 0;
            int maxAttempts = targetRooms * 10;
            
            while (_generatedRooms.Count < targetRooms && attempts < maxAttempts)
            {
                attempts++;
                
                // Try to place a room
                RoomData room = TryGenerateRoom();
                if (room != null && IsValidRoomPosition(room))
                {
                    _generatedRooms.Add(room);
                    CarveRoomIntoMaze(room);
                }
            }
            
            // Ensure at least one room exists
            if (_generatedRooms.Count == 0 && minRooms > 0)
            {
                RoomData fallback = CreateFallbackRoom();
                if (fallback != null)
                {
                    _generatedRooms.Add(fallback);
                    CarveRoomIntoMaze(fallback);
                }
            }
            
            Debug.Log($"[RoomGenerator] Generated {_generatedRooms.Count} rooms");
        }
        
        /// <summary>
        /// Get room at maze cell position.
        /// </summary>
        public RoomData GetRoomAtCell(int x, int y)
        {
            foreach (var room in _generatedRooms)
            {
                if (room.ContainsCell(x, y))
                {
                    return room;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Check if cell is inside any room.
        /// </summary>
        public bool IsCellInRoom(int x, int y)
        {
            return GetRoomAtCell(x, y) != null;
        }
        
        #region Room Generation
        
        private RoomData TryGenerateRoom()
        {
            // Random room dimensions
            int width = Random.Range(minRoomWidth, maxRoomWidth + 1);
            int height = Random.Range(minRoomHeight, maxRoomHeight + 1);
            
            // Random position (keep within maze bounds with padding)
            int startX = Random.Range(1, mazeGenerator.Width - width - 1);
            int startY = Random.Range(1, mazeGenerator.Height - height - 1);
            
            // Determine room type
            RoomType type = RoomType.Normal;
            if (allowSpecialRooms && Random.value < specialRoomChance)
            {
                type = (RoomType)Random.Range(1, System.Enum.GetNames(typeof(RoomType)).Length);
            }
            
            return new RoomData
            {
                Position = new Vector2Int(startX, startY),
                Width = width,
                Height = height,
                Type = type,
                Seed = Random.Range(0, 10000)
            };
        }
        
        private RoomData CreateFallbackRoom()
        {
            // Create a simple room near the start position
            return new RoomData
            {
                Position = new Vector2Int(2, 2),
                Width = 4,
                Height = 4,
                Type = RoomType.Normal,
                Seed = 12345
            };
        }
        
        private bool IsValidRoomPosition(RoomData room)
        {
            // Check room doesn't overlap with existing rooms
            foreach (var existing in _generatedRooms)
            {
                // Add padding between rooms
                if (room.Position.x < existing.Position.x + existing.Width + 2 &&
                    room.Position.x + room.Width + 2 > existing.Position.x &&
                    room.Position.y < existing.Position.y + existing.Height + 2 &&
                    room.Position.y + room.Height + 2 > existing.Position.y)
                {
                    return false;
                }
            }
            
            // Check room is within maze bounds
            if (room.Position.x < 1 || room.Position.y < 1 ||
                room.Position.x + room.Width >= mazeGenerator.Width - 1 ||
                room.Position.y + room.Height >= mazeGenerator.Height - 1)
            {
                return false;
            }
            
            return true;
        }
        
        #endregion
        
        #region Maze Integration
        
        private void CarveRoomIntoMaze(RoomData room)
        {
            // Clear walls inside room
            for (int x = room.Position.x; x < room.Position.x + room.Width; x++)
            {
                for (int y = room.Position.y; y < room.Position.y + room.Height; y++)
                {
                    mazeGenerator.Grid[x, y] = MazeGenerator.Wall.None;
                }
            }
            
            // Create entrances/exits (at least 2 per room)
            CreateRoomEntrances(room);
        }
        
        private void CreateRoomEntrances(RoomData room)
        {
            List<Vector2Int> possibleEntrances = new();
            
            // North wall
            for (int x = room.Position.x + 1; x < room.Position.x + room.Width - 1; x++)
            {
                possibleEntrances.Add(new Vector2Int(x, room.Position.y));
            }
            
            // South wall
            for (int x = room.Position.x + 1; x < room.Position.x + room.Width - 1; x++)
            {
                possibleEntrances.Add(new Vector2Int(x, room.Position.y + room.Height - 1));
            }
            
            // West wall
            for (int y = room.Position.y + 1; y < room.Position.y + room.Height - 1; y++)
            {
                possibleEntrances.Add(new Vector2Int(room.Position.x, y));
            }
            
            // East wall
            for (int y = room.Position.y + 1; y < room.Position.y + room.Height - 1; y++)
            {
                possibleEntrances.Add(new Vector2Int(room.Position.x + room.Width - 1, y));
            }
            
            // Select at least 2 entrances (more for larger rooms)
            int entranceCount = Mathf.Max(2, (room.Width + room.Height) / 3);
            entranceCount = Mathf.Min(entranceCount, possibleEntrances.Count);
            
            // Shuffle and select
            for (int i = 0; i < entranceCount; i++)
            {
                int index = Random.Range(i, possibleEntrances.Count);
                Vector2Int entrance = possibleEntrances[index];
                
                // Swap to prevent duplicates
                possibleEntrances[index] = possibleEntrances[i];
                
                // Remove wall at entrance
                mazeGenerator.Grid[entrance.x, entrance.y] = MazeGenerator.Wall.None;
                
                // Store entrance
                room.Entrances.Add(entrance);
            }
        }
        
        #endregion
        
        #region Utilities
        
        /// <summary>
        /// Clear all generated rooms.
        /// </summary>
        public void ClearRooms()
        {
            _generatedRooms.Clear();
            
            // Regenerate maze if needed
            if (mazeGenerator != null && mazeGenerator.Grid != null)
            {
                mazeGenerator.Generate();
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Room data structure for storage and retrieval.
    /// </summary>
    [System.Serializable]
    public class RoomData
    {
        public Vector2Int Position;
        public int Width;
        public int Height;
        public RoomType Type;
        public int Seed;
        public List<Vector2Int> Entrances = new();
        
        public Vector2Int Center => new(
            Position.x + Width / 2,
            Position.y + Height / 2
        );
        
        public bool ContainsCell(int x, int y)
        {
            return x >= Position.x && x < Position.x + Width &&
                   y >= Position.y && y < Position.y + Height;
        }
        
        public bool IsEntrance(int x, int y)
        {
            return Entrances.Contains(new Vector2Int(x, y));
        }
    }
    
    /// <summary>
    /// Types of rooms for special behaviors.
    /// </summary>
    public enum RoomType
    {
        Normal,     // Standard room
        Treasure,   // Contains chests/loot
        Combat,     // Enemy spawning room
        Trap,       // Trap-filled room
        Safe,       // No enemies/traps (rest area)
        Boss,       // Boss battle room
        Secret,     // Hidden room
        Puzzle      // Puzzle room
    }
}
