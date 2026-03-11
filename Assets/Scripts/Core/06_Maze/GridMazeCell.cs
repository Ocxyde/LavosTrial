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
// GridMazeCell.cs
// Cell types for custom grid-based maze system
// Unity 6 compatible - UTF-8 encoding - Unix line endings

namespace Code.Lavos.Core
{
    /// <summary>
    /// GridMazeCell - Cell types for grid-based maze generation.
    /// Each cell type fits in 4 bits (0-15) for compact byte storage.
    /// </summary>
    public enum GridMazeCell : byte
    {
        Floor = 0,      // 0000 - Empty space
        Room = 1,       // 0001 - Room interior
        Corridor = 2,   // 0010 - Corridor path
        Wall = 3,       // 0011 - Wall cell
        Door = 4,       // 0100 - Doorway
        SpawnPoint = 5, // 0101 - Spawn/respawn point
        // 6-15 reserved for future types
    }
    
    /// <summary>
    /// GridMazeFlags - Additional cell flags (optional metadata).
    /// Stored in upper 4 bits of each cell byte.
    /// </summary>
    [System.Flags]
    public enum GridMazeFlags : byte
    {
        None = 0,
        HasTorch = 1 << 4,    // 0001 0000
        HasChest = 2 << 4,    // 0010 0000
        HasEnemy = 3 << 4,    // 0011 0000
        IsLit = 4 << 4,       // 0100 0000
        IsLocked = 5 << 4,    // 0101 0000
    }

    // 
    //  Compatibility helpers for 8-axis system (CellFlags8)
    // 
    /// <summary>
    /// Extension methods for CellFlags8 to GridMazeCell compatibility.
    /// </summary>
    public static class CellTypeCompatibility
    {
        /// <summary>
        /// Check if CellFlags8 represents a wall (all 8 walls set).
        /// Compatible with old GridMazeCell.Wall check.
        /// </summary>
        public static bool IsWall(this CellFlags8 cell)
        {
            return (cell & CellFlags8.AllWalls) == CellFlags8.AllWalls;
        }

        /// <summary>
        /// Check if CellFlags8 represents a walkable cell (not all walls).
        /// Compatible with old GridMazeCell.Corridor/Room/Floor checks.
        /// </summary>
        public static bool IsWalkable(this CellFlags8 cell)
        {
            return (cell & CellFlags8.AllWalls) != CellFlags8.AllWalls;
        }

        /// <summary>
        /// Check if CellFlags8 has spawn room flag.
        /// Compatible with old GridMazeCell.SpawnPoint check.
        /// </summary>
        public static bool IsSpawnPoint(this CellFlags8 cell)
        {
            return (cell & CellFlags8.SpawnRoom) != CellFlags8.None;
        }

        /// <summary>
        /// Check if CellFlags8 has torch flag.
        /// Compatible with old GridMazeFlags.HasTorch.
        /// </summary>
        public static bool HasTorch(this CellFlags8 cell)
        {
            return (cell & CellFlags8.HasTorch) != CellFlags8.None;
        }

        /// <summary>
        /// Check if CellFlags8 has chest flag.
        /// Compatible with old GridMazeFlags.HasChest.
        /// </summary>
        public static bool HasChest(this CellFlags8 cell)
        {
            return (cell & CellFlags8.HasChest) != CellFlags8.None;
        }

        /// <summary>
        /// Check if CellFlags8 has enemy flag.
        /// Compatible with old GridMazeFlags.HasEnemy.
        /// </summary>
        public static bool HasEnemy(this CellFlags8 cell)
        {
            return (cell & CellFlags8.HasEnemy) != CellFlags8.None;
        }
    }
}
