// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8 | Locale: en_US
// Cell-Based Maze Generation System - Phase 1: Core Cell Structure

using UnityEngine;

namespace Code.Lavos.Core.Maze
{
    /// <summary>
    /// CellType - Classification of cell purpose in maze.
    /// </summary>
    public enum CellType
    {
        Empty,          // Walkable corridor/path
        Wall,           // Blocked (not walkable)
        Room,           // 3x3 room cell
        DeadEnd,        // Intentional dead-end corridor
        Treasure,       // Contains chest/loot
        EnemyGuard,     // Enemy placement
        Trap,           // Trap cell
        Safe            // Rest area (no enemies)
    }

    /// <summary>
    /// CellAgreement - What agreement/purpose this cell fulfills.
    /// </summary>
    public enum CellAgreement
    {
        None,           // No specific agreement
        Corridor,       // Main pathway cell
        RoomEntry,      // Room entrance cell
        RoomExit,       // Room exit cell
        RoomInterior,   // Inside room
        DeadEndTerm,    // Dead-end termination
        ChestLocation,  // Chest spawn point
        EnemyLocation,  // Enemy spawn point
        TrapLocation,   // Trap spawn point
        Doorway         // Transition cell (has door)
    }

    /// <summary>
    /// MazeCell - Core cell structure for cell-based maze generation.
    /// </summary>
    [System.Serializable]
    public struct MazeCell
    {
        // Wall Flags (Edge-Based)
        [Tooltip("Wall on north edge")] public bool hasNorthWall;
        [Tooltip("Wall on south edge")] public bool hasSouthWall;
        [Tooltip("Wall on east edge")] public bool hasEastWall;
        [Tooltip("Wall on west edge")] public bool hasWestWall;
        
        public bool HaveWallOnEdge => hasNorthWall || hasSouthWall || hasEastWall || hasWestWall;
        public bool IsFullySurrounded => hasNorthWall && hasSouthWall && hasEastWall && hasWestWall;
        
        // Path Classification
        [Tooltip("Is this cell on the PRIMARY path?")] public bool isOnPrimaryPath;
        [Tooltip("Is this cell on a DECOY path?")] public bool isOnDecoyPath;
        [Tooltip("Is this cell a dead-end?")] public bool isDeadEnd;
        [Tooltip("Path index from spawn")] public int pathIndex;
        
        // Cell Type & Agreement
        [Tooltip("Cell type")] public CellType cellType;
        [Tooltip("Cell agreement")] public CellAgreement agreement;
        
        // Constructors
        public static MazeCell CreateWall() => new MazeCell
        {
            hasNorthWall = true, hasSouthWall = true, hasEastWall = true, hasWestWall = true,
            cellType = CellType.Wall
        };
        
        public static MazeCell CreateEmpty() => new MazeCell
        {
            hasNorthWall = false, hasSouthWall = false, hasEastWall = false, hasWestWall = false,
            cellType = CellType.Empty
        };
        
        public bool IsWalkable() => cellType != CellType.Wall && !IsFullySurrounded;
        
        public void SetWall(Direction8 direction, bool hasWall = true)
        {
            switch (direction)
            {
                case Direction8.N: hasNorthWall = hasWall; break;
                case Direction8.S: hasSouthWall = hasWall; break;
                case Direction8.E: hasEastWall = hasWall; break;
                case Direction8.W: hasWestWall = hasWall; break;
            }
        }
        
        public void RemoveWall(Direction8 direction) => SetWall(direction, false);
        
        public bool HasWall(Direction8 direction) => direction switch
        {
            Direction8.N => hasNorthWall,
            Direction8.S => hasSouthWall,
            Direction8.E => hasEastWall,
            Direction8.W => hasWestWall,
            _ => false
        };
        
        public void MarkAsPrimaryPath(int index)
        {
            isOnPrimaryPath = true; isOnDecoyPath = false; isDeadEnd = false;
            cellType = CellType.Empty; agreement = CellAgreement.Corridor; pathIndex = index;
        }
        
        public void MarkAsDecoyPath()
        {
            isOnPrimaryPath = false; isOnDecoyPath = true; isDeadEnd = false;
            cellType = CellType.Empty; agreement = CellAgreement.Corridor;
        }
        
        public void MarkAsDeadEnd()
        {
            isOnPrimaryPath = false; isOnDecoyPath = true; isDeadEnd = true;
            cellType = CellType.DeadEnd; agreement = CellAgreement.DeadEndTerm;
        }
        
        public override string ToString() => $"MazeCell({cellType}, {(isOnPrimaryPath ? "PRIMARY" : "DECOY")})";
    }
}
