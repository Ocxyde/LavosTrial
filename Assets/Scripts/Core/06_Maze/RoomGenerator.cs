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
// RoomGenerator.cs
// Procedural room generation for maze-based dungeons
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Generates rectangular rooms with entrances/exits that integrate with maze walls
//
// NOTE: This class exposes many [SerializeField] fields for Inspector configuration.
// Some fields are reserved for future features and may not be used in current implementation.

using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0414 // Disable warnings for unused private fields (reserved for future features)
#pragma warning disable CS0649 // Disable warnings for fields never assigned to (serialized in Inspector)

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
        [SerializeField] [Range(0f, 1f)] private float roomDensity = 0.3f;  // Reserved for density-based room placement
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
        /// Seed complexity determines room count and size.
        /// </summary>
        public void GenerateRooms()
        {
            if (mazeGenerator == null || mazeGenerator.Grid == null)
            {
                Debug.LogError("[RoomGenerator] MazeGenerator not initialized!");
                return;
            }
            
            _generatedRooms.Clear();
            
            // Calculate room count based on maze complexity (seed-derived)
            int mazeComplexity = (mazeGenerator.Width * mazeGenerator.Height) / 100;
            int targetRooms = Mathf.Clamp(mazeComplexity, minRooms, maxRooms * 2);
            
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
            
            // Ensure entrance and exit are clear
            EnsureClearEntrance();
            EnsureClearExit();
            
            Debug.Log($"[RoomGenerator] Generated {_generatedRooms.Count} rooms (complexity: {mazeComplexity})");
        }
        
        /// <summary>
        /// Ensure spawn point (0,0) is clear for player entrance.
        /// Only clears the spawn cell, not a large area.
        /// </summary>
        private void EnsureClearEntrance()
        {
            // Clear only the spawn cell (1x1)
            if (0 < mazeGenerator.Width && 0 < mazeGenerator.Height)
            {
                mazeGenerator.Grid[0, 0] = MazeGenerator.Wall.None;
            }

            Debug.Log("[RoomGenerator] Entrance cleared at (0,0)");
        }

        /// <summary>
        /// Ensure exit point is clear for player progression.
        /// Only clears the exit cell, not a large area.
        /// </summary>
        private void EnsureClearExit()
        {
            int exitX = mazeGenerator.Width - 1;
            int exitY = mazeGenerator.Height - 1;

            // Clear only the exit cell (1x1)
            if (exitX >= 0 && exitX < mazeGenerator.Width && exitY >= 0 && exitY < mazeGenerator.Height)
            {
                mazeGenerator.Grid[exitX, exitY] = MazeGenerator.Wall.None;
            }

            Debug.Log($"[RoomGenerator] Exit cleared at ({exitX},{exitY})");
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
            // Room dimensions based on maze complexity
            int complexity = (mazeGenerator.Width * mazeGenerator.Height) / 100;
            
            // More complex maze = bigger rooms
            int currentMinWidth = Mathf.Max(3, minRoomWidth + complexity / 5);
            int currentMaxWidth = Mathf.Max(currentMinWidth, maxRoomWidth + complexity / 3);
            int currentMinHeight = Mathf.Max(3, minRoomHeight + complexity / 5);
            int currentMaxHeight = Mathf.Max(currentMinHeight, maxRoomHeight + complexity / 3);
            
            // Random room dimensions
            int width = Random.Range(currentMinWidth, currentMaxWidth + 1);
            int height = Random.Range(currentMinHeight, currentMaxHeight + 1);
            
            // Random position (keep within maze bounds with padding)
            int startX = Random.Range(2, mazeGenerator.Width - width - 2);
            int startY = Random.Range(2, mazeGenerator.Height - height - 2);
            
            // Determine room type based on position and randomness
            RoomType type = DetermineRoomType(startX, startY, width, height);
            
            return new RoomData
            {
                Position = new Vector2Int(startX, startY),
                Width = width,
                Height = height,
                Type = type,
                Seed = Random.Range(0, 10000)
            };
        }
        
        /// <summary>
        /// Determine room type based on position and maze layout.
        /// </summary>
        private RoomType DetermineRoomType(int x, int y, int width, int height)
        {
            // Exit area - Boss or Combat room
            if (x > mazeGenerator.Width * 0.7f && y > mazeGenerator.Height * 0.7f)
            {
                if (Random.value < 0.3f)
                    return RoomType.Boss;
                return RoomType.Combat;
            }
            
            // Start area - Safe room
            if (x < mazeGenerator.Width * 0.3f && y < mazeGenerator.Height * 0.3f)
            {
                return RoomType.Safe;
            }
            
            // Random special rooms
            if (allowSpecialRooms && Random.value < specialRoomChance)
            {
                int roll = Random.Range(0, 100);
                if (roll < 15) return RoomType.Treasure;
                if (roll < 30) return RoomType.Combat;
                if (roll < 45) return RoomType.Trap;
                if (roll < 55) return RoomType.Secret;
                if (roll < 65) return RoomType.Puzzle;
                return RoomType.Safe;
            }
            
            return RoomType.Normal;
        }
        
        private RoomData CreateFallbackRoom()
        {
            // Create a simple room near the start position (entrance)
            return new RoomData
            {
                Position = new Vector2Int(2, 2),
                Width = 4,
                Height = 4,
                Type = RoomType.Safe,
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
            // Clear walls inside room only (keep perimeter walls intact)
            for (int x = room.Position.x + 1; x < room.Position.x + room.Width - 1; x++)
            {
                for (int y = room.Position.y + 1; y < room.Position.y + room.Height - 1; y++)
                {
                    mazeGenerator.Grid[x, y] = MazeGenerator.Wall.None;
                }
            }

            // Create exactly 1 entrance and 1 exit
            CreateRoomEntrances(room);
        }

        private void CreateRoomEntrances(RoomData room)
        {
            // Select exactly 2 positions: 1 entrance, 1 exit
            // Place them on opposite sides of the room for better flow
            
            List<Vector2Int> northPositions = new();
            List<Vector2Int> southPositions = new();
            List<Vector2Int> westPositions = new();
            List<Vector2Int> eastPositions = new();

            // North wall (center position only)
            int northX = room.Position.x + room.Width / 2;
            if (northX > room.Position.x && northX < room.Position.x + room.Width - 1)
            {
                northPositions.Add(new Vector2Int(northX, room.Position.y));
            }

            // South wall (center position only)
            int southX = room.Position.x + room.Width / 2;
            if (southX > room.Position.x && southX < room.Position.x + room.Width - 1)
            {
                southPositions.Add(new Vector2Int(southX, room.Position.y + room.Height - 1));
            }

            // West wall (center position only)
            int westY = room.Position.y + room.Height / 2;
            if (westY > room.Position.y && westY < room.Position.y + room.Height - 1)
            {
                westPositions.Add(new Vector2Int(room.Position.x, westY));
            }

            // East wall (center position only)
            int eastY = room.Position.y + room.Height / 2;
            if (eastY > room.Position.y && eastY < room.Position.y + room.Height - 1)
            {
                eastPositions.Add(new Vector2Int(room.Position.x + room.Width - 1, eastY));
            }

            // Choose 2 opposite sides for entrance and exit
            List<List<Vector2Int>> allSides = new() { northPositions, southPositions, westPositions, eastPositions };
            
            // Shuffle sides
            for (int i = allSides.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                var temp = allSides[i];
                allSides[i] = allSides[j];
                allSides[j] = temp;
            }

            // Pick first 2 sides that have valid positions
            int entrancesCreated = 0;
            foreach (var side in allSides)
            {
                if (side.Count > 0 && entrancesCreated < 2)
                {
                    Vector2Int entrance = side[0];
                    
                    // Remove wall at entrance position
                    mazeGenerator.Grid[entrance.x, entrance.y] = MazeGenerator.Wall.None;
                    
                    // Store entrance
                    room.Entrances.Add(entrance);
                    entrancesCreated++;
                }
            }

            // Ensure we have at least 2 entrances
            if (room.Entrances.Count < 2)
            {
                Debug.LogWarning($"[RoomGenerator] Room at ({room.Position.x},{room.Position.y}) has only {room.Entrances.Count} entrance(s)");
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
