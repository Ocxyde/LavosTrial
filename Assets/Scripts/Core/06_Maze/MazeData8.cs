// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8  |  Locale: en_US

using System;
using System.Collections.Generic;
using UnityEngine;
using Code.Lavos.Core.Advanced;

namespace Code.Lavos.Core
{
    // -------------------------------------------------------------------------
    // CellFlags8 - packed into 1 ushort (2 bytes) per cell
    //
    // Low byte  (bits 0-7)  : wall presence per axis
    // High byte (bits 8-15) : object / room / decoration metadata
    // -------------------------------------------------------------------------
    [Flags]
    public enum CellFlags8 : ushort
    {
        None      = 0,

        // Cardinal walls
        WallN     = 1 << 0,   // 0x0001
        WallS     = 1 << 1,   // 0x0002
        WallE     = 1 << 2,   // 0x0004
        WallW     = 1 << 3,   // 0x0008

        // Diagonal walls
        WallNE    = 1 << 4,   // 0x0010
        WallNW    = 1 << 5,   // 0x0020
        WallSE    = 1 << 6,   // 0x0040
        WallSW    = 1 << 7,   // 0x0080

        // Object / room flags
        SpawnRoom = 1 << 8,   // 0x0100
        HasChest  = 1 << 9,   // 0x0200
        HasEnemy  = 1 << 10,  // 0x0400
        HasTorch  = 1 << 11,  // 0x0800
        IsExit    = 1 << 12,  // 0x1000
        IsRoom    = 1 << 13,  // 0x2000
        HasPillar = 1 << 14,  // 0x4000  - pillar decoration
        HasNiche  = 1 << 15,  // 0x8000  - wall niche/alcove

        // Extended flags (requires ushort expansion - stored in metadata)
        // HasArch is stored in WallMetadata instead (bit 0 of flags)

        // Composite masks
        AllCardinal = WallN | WallS | WallE | WallW,
        AllDiagonal = WallNE | WallNW | WallSE | WallSW,
        AllWalls    = AllCardinal | AllDiagonal,
        AllObjects  = SpawnRoom | HasChest | HasEnemy | HasTorch | IsExit | IsRoom | HasPillar | HasNiche,
    }

    // -------------------------------------------------------------------------
    // Direction8 - 8-axis enum
    // -------------------------------------------------------------------------
    public enum Direction8
    {
        N  = 0,
        S  = 1,
        E  = 2,
        W  = 3,
        NE = 4,
        NW = 5,
        SE = 6,
        SW = 7,
    }

    // -------------------------------------------------------------------------
    // Direction8Helper
    // -------------------------------------------------------------------------
    public static class Direction8Helper
    {
        public static (int dx, int dz) ToOffset(Direction8 d) => d switch
        {
            Direction8.N  => ( 0,  1),
            Direction8.S  => ( 0, -1),
            Direction8.E  => ( 1,  0),
            Direction8.W  => (-1,  0),
            Direction8.NE => ( 1,  1),
            Direction8.NW => (-1,  1),
            Direction8.SE => ( 1, -1),
            Direction8.SW => (-1, -1),
            _             => ( 0,  0),
        };

        public static Direction8 Opposite(Direction8 d) => d switch
        {
            Direction8.N  => Direction8.S,
            Direction8.S  => Direction8.N,
            Direction8.E  => Direction8.W,
            Direction8.W  => Direction8.E,
            Direction8.NE => Direction8.SW,
            Direction8.SW => Direction8.NE,
            Direction8.NW => Direction8.SE,
            Direction8.SE => Direction8.NW,
            _             => d,
        };

        public static CellFlags8 ToWallFlag(Direction8 d) => d switch
        {
            Direction8.N  => CellFlags8.WallN,
            Direction8.S  => CellFlags8.WallS,
            Direction8.E  => CellFlags8.WallE,
            Direction8.W  => CellFlags8.WallW,
            Direction8.NE => CellFlags8.WallNE,
            Direction8.NW => CellFlags8.WallNW,
            Direction8.SE => CellFlags8.WallSE,
            Direction8.SW => CellFlags8.WallSW,
            _             => CellFlags8.None,
        };

        public static bool IsDiagonal(Direction8 d)
            => d == Direction8.NE || d == Direction8.NW
            || d == Direction8.SE || d == Direction8.SW;

        // Static array - avoid repeated allocation in hot loops
        public static readonly Direction8[] All =
        {
            Direction8.N, Direction8.S, Direction8.E, Direction8.W,
            Direction8.NE, Direction8.NW, Direction8.SE, Direction8.SW,
        };
    }

    // -------------------------------------------------------------------------
    // DoorOpening - represents a carved door opening in a wall
    // -------------------------------------------------------------------------
    public struct DoorOpening
    {
        public int X;                  // Grid X position
        public int Z;                  // Grid Z position
        public Direction8 Direction;   // Which direction the door faces
        public MazeDoorType Type;      // Normal, Locked, Secret, Exit
        public bool IsCarved;          // true = opening carved in wall

        public DoorOpening(int x, int z, Direction8 dir, MazeDoorType type = MazeDoorType.Normal)
        {
            X = x;
            Z = z;
            Direction = dir;
            Type = type;
            IsCarved = false;
        }

        public override string ToString()
            => $"DoorOpening({X},{Z} {Direction} {Type})";
    }

    // -------------------------------------------------------------------------
    // DoorType - types of doors for maze generation
    // -------------------------------------------------------------------------
    public enum MazeDoorType
    {
        Normal = 0,   // Standard door
        Locked = 1,   // Requires key
        Secret = 2,   // Hidden door
        Exit   = 3,   // Maze exit door
    }

    // -------------------------------------------------------------------------
    // MazeData8 - mutable data container for generated maze
    // -------------------------------------------------------------------------
    public sealed class MazeData8
    {
        // Metadata
        public int                 Width            { get; }
        public int                 Height           { get; }
        public int                 Seed             { get; }
        public int                 Level            { get; }
        public long                Timestamp        { get; }
        public float               DifficultyFactor { get; internal set; }
        public DungeonMazeConfig   Config           { get; internal set; }
        public float               AIAdaptiveFactor { get; internal set; }

        // Entry / Exit
        public (int x, int z) SpawnCell { get; private set; }
        public (int x, int z) ExitCell  { get; private set; }

        // Door openings (carved during generation)
        private readonly List<DoorOpening> _doorOpenings;
        public IReadOnlyList<DoorOpening> DoorOpenings => _doorOpenings.AsReadOnly();

        // Grid - ushort[x, z]
        private readonly CellFlags8[,] _cells;

        public MazeData8(int width, int height, int seed, int level)
        {
            Width     = width;
            Height    = height;
            Seed      = seed;
            Level     = level;
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _cells    = new CellFlags8[width, height];
            _doorOpenings = new List<DoorOpening>();
        }

        // Cell accessors
        public CellFlags8 GetCell(int x, int z)                => _cells[x, z];
        public void       SetCell(int x, int z, CellFlags8 f)  => _cells[x, z]  = f;
        public void       AddFlag(int x, int z, CellFlags8 f)  => _cells[x, z] |= f;
        public void       ClearFlag(int x, int z, CellFlags8 f)=> _cells[x, z] &= ~f;

        public bool HasWall(int x, int z, Direction8 d)
            => (_cells[x, z] & Direction8Helper.ToWallFlag(d)) != 0;

        public bool InBounds(int x, int z)
            => x >= 0 && x < Width && z >= 0 && z < Height;

        public void SetSpawn(int x, int z)
        {
            SpawnCell = (x, z);
            AddFlag(x, z, CellFlags8.SpawnRoom);
        }

        public void SetExit(int x, int z)
        {
            ExitCell = (x, z);
            AddFlag(x, z, CellFlags8.IsExit);
        }

        public void SetSpawnRoom(int x, int z, int radius)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dz = -radius; dz <= radius; dz++)
                {
                    int nx = x + dx;
                    int nz = z + dz;
                    if (InBounds(nx, nz))
                    {
                        AddFlag(nx, nz, CellFlags8.SpawnRoom);
                    }
                }
            }
        }

        public void SetExitRoom(int x, int z, int radius)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dz = -radius; dz <= radius; dz++)
                {
                    int nx = x + dx;
                    int nz = z + dz;
                    if (InBounds(nx, nz))
                    {
                        AddFlag(nx, nz, CellFlags8.IsExit);
                    }
                }
            }
        }

        public bool IsSpawnRoom(int x, int z)
            => (_cells[x, z] & CellFlags8.SpawnRoom) != 0;

        public bool IsExitRoom(int x, int z)
            => (_cells[x, z] & CellFlags8.IsExit) != 0;

        public void MarkAsMainPath(int x, int z)
        {
            // Marker for main path - using IsRoom flag as proxy
            AddFlag(x, z, CellFlags8.IsRoom);
        }

        // -------------------------------------------------------------------------
        // Door Opening Management
        // -------------------------------------------------------------------------

        /// <summary>
        /// Add a door opening at the specified position.
        /// </summary>
        public void AddDoorOpening(int x, int z, Direction8 direction, MazeDoorType type = MazeDoorType.Normal)
        {
            var door = new DoorOpening(x, z, direction, type);
            _doorOpenings.Add(door);
            Debug.Log($"[MazeData8] Door opening added: {door}");
        }

        /// <summary>
        /// Mark a door opening as carved (geometry removed).
        /// </summary>
        public void MarkDoorCarved(int index)
        {
            if (index >= 0 && index < _doorOpenings.Count)
            {
                var door = _doorOpenings[index];
                door.IsCarved = true;
                _doorOpenings[index] = door;
            }
        }

        /// <summary>
        /// Clear all door openings (for regeneration).
        /// </summary>
        public void ClearDoorOpenings()
        {
            _doorOpenings.Clear();
        }

        /// <summary>
        /// Get the number of door openings.
        /// </summary>
        public int DoorOpeningCount => _doorOpenings.Count;

        // -------------------------------------------------------------------------
        // Binary file constants - LAV8S v3
        //
        // File layout (all fields little-endian):
        // +---------+--------+------------------------------------+
        // | Offset  | Bytes  | Field                              |
        // +---------+--------+------------------------------------+
        // |  0      |  5     | Magic  "LAV8S"                     |
        // |  5      |  1     | Version (3)                        |
        // |  6      |  2     | Width   (int16)                    |
        // |  8      |  2     | Height  (int16)                    |
        // | 10      |  4     | Seed    (int32)                    |
        // | 14      |  4     | Level   (int32)                    |
        // | 18      |  8     | Timestamp (int64, UTC unix secs)   |
        // | 26      |  2     | SpawnX  (int16)                    |
        // | 28      |  2     | SpawnZ  (int16)                    |
        // | 30      |  2     | ExitX   (int16)                    |
        // | 32      |  2     | ExitZ   (int16)                    |
        // | 34      |  4     | DifficultyFactor (float32)         |
        // | 38      | W*H*2  | Cell data - ushort per cell (LE)   |
        // | 38+W*H*2|  4     | Checksum XOR-fold (uint32)         |
        // +---------+--------+------------------------------------+
        // Total: 42 + (W * H * 2) bytes
        //   Level  0 (12x12) ->  330 bytes
        //   Level 39 (51x51) -> 5,244 bytes
        // -------------------------------------------------------------------------
        public static readonly byte[] MAGIC   = System.Text.Encoding.ASCII.GetBytes("LAV8S");
        public const  byte            VERSION = 3;
        public const  int             HEADER_SIZE = 38;
    }
}
