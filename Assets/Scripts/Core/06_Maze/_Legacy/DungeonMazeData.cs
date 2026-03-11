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
// DungeonMazeData.cs
// Advanced maze data with trap, treasure, and difficulty metrics
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// DEPRECATED: Use MazeData8 instead. This class is kept for legacy compatibility.
// -------------------------------------------------------------------------
// [OBSOLETE 2026-03-10] Use MazeData8 for all maze operations.
// DungeonMazeData is no longer maintained and will be removed in a future version.
// -------------------------------------------------------------------------

using UnityEngine;
using Code.Lavos.Core;

namespace Code.Lavos.Core.Advanced
{
    /// <summary>
    /// DEPRECATED: Use MazeData8 instead.
    /// 
    /// Advanced maze data structure extending MazeData8 with:
    /// - Trap room tracking
    /// - Treasure room tracking
    /// - Boss room locations
    /// - Danger zones
    /// - AI-computed difficulty metrics
    /// - Path guarantee markers
    /// </summary>
    [System.Obsolete("Use MazeData8 instead. DungeonMazeData is deprecated and will be removed.")]
    public class DungeonMazeData
    {
        private uint[,] _cells;
        private int _width;
        private int _height;
        private int _seed;
        private int _level;
        private float _generationTimeMs;
        private (int x, int z) _spawnCell;
        private (int x, int z) _exitCell;
        private int _spawnRoomRadius;
        private int _exitRoomRadius;

        // Advanced metrics
        public float DifficultyFactor { get; set; }
        public float AIAdaptiveFactor { get; set; }
        public DungeonMazeConfig Config { get; set; }

        public int Width => _width;
        public int Height => _height;
        public int Seed => _seed;
        public int Level => _level;
        public float GenerationTimeMs { get => _generationTimeMs; set => _generationTimeMs = value; }
        public (int x, int z) SpawnCell => _spawnCell;
        public (int x, int z) ExitCell => _exitCell;

        public DungeonMazeData(int width, int height, int seed, int level)
        {
            _width = width;
            _height = height;
            _seed = seed;
            _level = level;
            _cells = new uint[width, height];
            _generationTimeMs = 0;
            AIAdaptiveFactor = 1.0f;
        }

        public uint GetCell(int x, int z)
        {
            if (!InBounds(x, z))
                return 0;
            return _cells[x, z];
        }

        public void SetCell(int x, int z, uint value)
        {
            if (InBounds(x, z))
                _cells[x, z] = value;
        }

        public void AddFlag(int x, int z, uint flag)
        {
            if (InBounds(x, z))
                _cells[x, z] |= flag;
        }

        public bool HasWall(int x, int z, Core.Direction8 dir)
        {
            var cell = GetCell(x, z);
            var flag = Direction8Helper.ToWallFlag(dir);
            return (cell & flag) != 0;
        }

        public void SetSpawn(int x, int z)
        {
            _spawnCell = (x, z);
        }

        public void SetExit(int x, int z)
        {
            _exitCell = (x, z);
        }

        public void SetSpawnRoom(int x, int z, int radius)
        {
            _spawnCell = (x, z);
            _spawnRoomRadius = radius;
        }

        public void SetExitRoom(int x, int z, int radius)
        {
            _exitCell = (x, z);
            _exitRoomRadius = radius;
        }

        public bool IsSpawnRoom(int x, int z)
        {
            int dx = x - _spawnCell.x;
            int dz = z - _spawnCell.z;
            return dx * dx + dz * dz <= _spawnRoomRadius * _spawnRoomRadius;
        }

        public bool IsExitRoom(int x, int z)
        {
            int dx = x - _exitCell.x;
            int dz = z - _exitCell.z;
            return dx * dx + dz * dz <= _exitRoomRadius * _exitRoomRadius;
        }

        public void MarkAsMainPath(int x, int z)
        {
            var cell = GetCell(x, z);
            cell |= CellFlags8.IsMainPath;
            SetCell(x, z, cell);
        }

        public void MarkAsSpawnRoom(int x, int z)
        {
            if (!InBounds(x, z)) return;
            var cell = GetCell(x, z);
            cell |= CellFlags8.SpawnRoom;
            SetCell(x, z, cell);
        }

        public bool IsSpawnRoomCell(int x, int z)
        {
            if (!InBounds(x, z)) return false;
            return (GetCell(x, z) & CellFlags8.SpawnRoom) != 0;
        }

        public void MarkAsExitRoom(int x, int z)
        {
            if (!InBounds(x, z)) return;
            var cell = GetCell(x, z);
            cell |= CellFlags8.IsExit;
            SetCell(x, z, cell);
        }

        public bool IsExitRoomCell(int x, int z)
        {
            if (!InBounds(x, z)) return false;
            return (GetCell(x, z) & CellFlags8.IsExit) != 0;
        }

        public bool InBounds(int x, int z)
        {
            return x >= 0 && x < _width && z >= 0 && z < _height;
        }
    }

    /// <summary>
    /// Cell flag bits for advanced maze state.
    /// Extends standard 8-wall flags with room type and danger indicators.
    /// Uses uint (32-bit) to support extended flags.
    /// </summary>
    public static class CellFlags8
    {
        // Walls (bits 0-7) - Cardinal and Diagonal
        public const uint Wall_N = 0x0001;
        public const uint Wall_S = 0x0002;
        public const uint Wall_E = 0x0004;
        public const uint Wall_W = 0x0008;
        public const uint Wall_NE = 0x0010;
        public const uint Wall_NW = 0x0020;
        public const uint Wall_SE = 0x0040;
        public const uint Wall_SW = 0x0080;
        public const uint Wall_All = 0x00FF;

        // Room Types (bits 8-13)
        public const uint IsRoom = 0x0100;
        public const uint IsHall = 0x0200;
        public const uint IsTrapRoom = 0x0400;
        public const uint IsTreasureRoom = 0x0800;
        public const uint IsBossRoom = 0x1000;
        public const uint SpawnRoom = 0x02000;  // Part of spawn room area (bit 13)
        public const uint IsExit = 0x04000;     // Exit room marker (bit 14)

        // Objects (bits 15-17)
        public const uint HasTorch = 0x08000;   // Bit 15
        public const uint HasEnemy = 0x10000;   // Bit 16
        public const uint HasChest = 0x20000;   // Bit 17

        // Decoration (bits 20-21)
        public const uint HasPillar = 0x0010_0000;  // Bit 20
        public const uint HasNiche = 0x0020_0000;   // Bit 21

        // Advanced markers (bits 18-19)
        public const uint IsMainPath = 0x0004_0000;  // Bit 18
        public const uint IsDanger = 0x0008_0000;    // Bit 19
    }

    /// <summary>
    /// Direction8 helper utilities.
    /// </summary>
    public static class Direction8Helper
    {
        public static (int dx, int dz) ToOffset(Core.Direction8 dir)
        {
            return dir switch
            {
                Core.Direction8.N => (0, 1),
                Core.Direction8.S => (0, -1),
                Core.Direction8.E => (1, 0),
                Core.Direction8.W => (-1, 0),
                Core.Direction8.NE => (1, 1),
                Core.Direction8.NW => (-1, 1),
                Core.Direction8.SE => (1, -1),
                Core.Direction8.SW => (-1, -1),
                _ => (0, 0),
            };
        }

        public static Core.Direction8 Opposite(Core.Direction8 dir)
        {
            return dir switch
            {
                Core.Direction8.N => Core.Direction8.S,
                Core.Direction8.S => Core.Direction8.N,
                Core.Direction8.E => Core.Direction8.W,
                Core.Direction8.W => Core.Direction8.E,
                Core.Direction8.NE => Core.Direction8.SW,
                Core.Direction8.NW => Core.Direction8.SE,
                Core.Direction8.SE => Core.Direction8.NW,
                Core.Direction8.SW => Core.Direction8.NE,
                _ => dir,
            };
        }

        public static bool IsDiagonal(Core.Direction8 dir)
        {
            return dir >= Core.Direction8.NE;
        }

        public static uint ToWallFlag(Core.Direction8 dir)
        {
            return dir switch
            {
                Core.Direction8.N => CellFlags8.Wall_N,
                Core.Direction8.S => CellFlags8.Wall_S,
                Core.Direction8.E => CellFlags8.Wall_E,
                Core.Direction8.W => CellFlags8.Wall_W,
                Core.Direction8.NE => CellFlags8.Wall_NE,
                Core.Direction8.NW => CellFlags8.Wall_NW,
                Core.Direction8.SE => CellFlags8.Wall_SE,
                Core.Direction8.SW => CellFlags8.Wall_SW,
                _ => 0,
            };
        }
    }
}
