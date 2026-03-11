// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8 (no BOM) | Line Endings: Unix LF
// Cell-Based Maze Generation System - Phase 3: Room System

using System;
using UnityEngine;

namespace Code.Lavos.Core.Maze
{
    /// <summary>
    /// RoomType - Classification of room purpose.
    /// </summary>
    public enum RoomType
    {
        Normal,         // Standard through-room
        Treasure,       // Contains chest(s)
        Boss,           // Boss encounter
        Safe,           // Rest area (no enemies)
        Trap,           // Trapped room
        Shop,           // Merchant room
        Puzzle          // Puzzle room
    }

    /// <summary>
    /// Room - 3x3 cell room with 2 doors (entry + exit).
    /// 
    /// Specifications:
    /// - Size: 3x3 cells (configurable)
    /// - Walls: Surround all 4 sides
    /// - Doors: Exactly 2 (entry + exit)
    /// - Door Connection: Both must lead to walkable cells
    /// - Flow: Players ENTER and EXIT (not dead-end)
    /// 
    /// Plug-in-Out Compliant:
    /// - Pure data structure
    /// - Used by RoomSystem
    /// </summary>
    [Serializable]
    public struct Room
    {
        // Room Identity
        [Tooltip("Unique room ID")] public int id;
        [Tooltip("Room type")] public RoomType type;
        
        // Position
        [Tooltip("Center cell position")] public Vector2Int center;
        [Tooltip("Room width (default: 3)")] public int width;
        [Tooltip("Room height (default: 3)")] public int height;
        
        // Doors
        [Tooltip("Entry door position")] public Vector2Int entryDoor;
        [Tooltip("Exit door position")] public Vector2Int exitDoor;
        [Tooltip("Entry door direction")] public Direction8 entryDirection;
        [Tooltip("Exit door direction")] public Direction8 exitDirection;
        
        // State
        [Tooltip("Is room cleared (all interior cells empty)?")] public bool isCleared;
        [Tooltip("Are walls surrounding room?")] public bool isSurrounded;
        [Tooltip("Are both doors connected to walkable cells?")] public bool doorsConnected;
        
        // Properties
        public int HalfWidth => width / 2;
        public int HalfHeight => height / 2;
        
        /// <summary>
        /// Create a new room at center position.
        /// </summary>
        public static Room Create(Vector2Int center, int size = 3)
        {
            return new Room
            {
                id = UnityEngine.Random.Range(0, 99999),
                type = RoomType.Normal,
                center = center,
                width = size,
                height = size,
                isCleared = false,
                isSurrounded = false,
                doorsConnected = false
            };
        }
        
        /// <summary>
        /// Get all cells in room area.
        /// </summary>
        public Vector2Int[] GetRoomCells()
        {
            var cells = new Vector2Int[width * height];
            int index = 0;
            
            for (int x = -HalfWidth; x <= HalfWidth; x++)
            {
                for (int z = -HalfHeight; z <= HalfHeight; z++)
                {
                    cells[index++] = center + new Vector2Int(x, z);
                }
            }
            
            return cells;
        }
        
        /// <summary>
        /// Get room boundary cells (where walls are).
        /// </summary>
        public Vector2Int[] GetBoundaryCells()
        {
            var boundary = new System.Collections.Generic.List<Vector2Int>();
            
            // North boundary
            for (int x = -HalfWidth; x <= HalfWidth; x++)
                boundary.Add(center + new Vector2Int(x, HalfHeight + 1));
            
            // South boundary
            for (int x = -HalfWidth; x <= HalfWidth; x++)
                boundary.Add(center + new Vector2Int(x, -HalfHeight - 1));
            
            // East boundary
            for (int z = -HalfHeight; z <= HalfHeight; z++)
                boundary.Add(center + new Vector2Int(HalfWidth + 1, z));
            
            // West boundary
            for (int z = -HalfHeight; z <= HalfHeight; z++)
                boundary.Add(center + new Vector2Int(-HalfWidth - 1, z));
            
            return boundary.ToArray();
        }
        
        /// <summary>
        /// Check if position is inside room.
        /// </summary>
        public bool IsInside(Vector2Int pos)
        {
            int dx = Mathf.Abs(pos.x - center.x);
            int dz = Mathf.Abs(pos.y - center.y);
            
            return dx <= HalfWidth && dz <= HalfHeight;
        }
        
        /// <summary>
        /// Check if position is on room boundary.
        /// </summary>
        public bool IsOnBoundary(Vector2Int pos)
        {
            int dx = Mathf.Abs(pos.x - center.x);
            int dz = Mathf.Abs(pos.y - center.y);
            
            return dx == HalfWidth + 1 || dz == HalfHeight + 1;
        }
        
        /// <summary>
        /// Validate room (ensure it meets requirements).
        /// </summary>
        public bool Validate()
        {
            if (width < 3 || height < 3) return false;
            if (entryDoor == exitDoor) return false;
            if (!doorsConnected) return false;
            return true;
        }
        
        /// <summary>
        /// Log room info for debugging.
        /// </summary>
        public void Log()
        {
            Debug.Log($"[Room] ID:{id} Type:{type} Center:{center} Size:{width}x{height} " +
                     $"Entry:{entryDoor}({entryDirection}) Exit:{exitDoor}({exitDirection}) " +
                     $"Cleared:{isCleared} Surrounded:{isSurrounded} Connected:{doorsConnected}");
        }
        
        public override string ToString()
        {
            return $"Room(ID:{id}, {type}, {center}, {width}x{height})";
        }
    }
}
