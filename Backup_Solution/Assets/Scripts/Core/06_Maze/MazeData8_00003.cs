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
// MazeData8.cs
// 8-axis maze data structure with packed cell flags
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using System;

namespace Code.Lavos.Core
{
    // ─────────────────────────────────────────────────────────────
    //  CellFlags8  —  packed into 1 ushort (2 bytes) per cell
    //
    //  Low byte  (bits 0-7)  : wall presence per axis
    //  High byte (bits 8-12) : object / room metadata
    //
    //  Bit map:
    //     0  WallN       North wall
    //     1  WallS       South wall
    //     2  WallE       East wall
    //     3  WallW       West wall
    //     4  WallNE      North-East diagonal wall
    //     5  WallNW      North-West diagonal wall
    //     6  WallSE      South-East diagonal wall
    //     7  WallSW      South-West diagonal wall
    //     8  SpawnRoom   Part of guaranteed spawn room
    //     9  HasChest    Chest placed here
    //    10  HasEnemy    Enemy placed here
    //    11  HasTorch    Torch on this cell
    //    12  IsExit      Exit cell marker
    //    13-15           (reserved)
    // ─────────────────────────────────────────────────────────────
    [Flags]
    public enum CellFlags8 : ushort
    {
        None      = 0,

        // ── Cardinal ──────────────────────────────────────────────
        WallN     = 1 << 0,   // 0x0001
        WallS     = 1 << 1,   // 0x0002
        WallE     = 1 << 2,   // 0x0004
        WallW     = 1 << 3,   // 0x0008

        // ── Diagonal ──────────────────────────────────────────────
        WallNE    = 1 << 4,   // 0x0010
        WallNW    = 1 << 5,   // 0x0020
        WallSE    = 1 << 6,   // 0x0040
        WallSW    = 1 << 7,   // 0x0080

        // ── Object / room ─────────────────────────────────────────
        SpawnRoom = 1 << 8,   // 0x0100
        HasChest  = 1 << 9,   // 0x0200
        HasEnemy  = 1 << 10,  // 0x0400
        HasTorch  = 1 << 11,  // 0x0800
        IsExit    = 1 << 12,  // 0x1000

        // ── Composite masks ───────────────────────────────────────
        AllCardinal = WallN | WallS | WallE | WallW,
        AllDiagonal = WallNE | WallNW | WallSE | WallSW,
        AllWalls    = AllCardinal | AllDiagonal,
        AllObjects  = SpawnRoom | HasChest | HasEnemy | HasTorch | IsExit,
    }

    // ─────────────────────────────────────────────────────────────
    //  Direction8  —  8-axis enum
    // ─────────────────────────────────────────────────────────────
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

    // ─────────────────────────────────────────────────────────────
    //  Direction8Helper
    // ─────────────────────────────────────────────────────────────
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

        // Static array — avoid repeated allocation in hot loops
        public static readonly Direction8[] All =
        {
            Direction8.N, Direction8.S, Direction8.E, Direction8.W,
            Direction8.NE, Direction8.NW, Direction8.SE, Direction8.SW,
        };
    }

    // ─────────────────────────────────────────────────────────────
    //  MazeData8  —  immutable snapshot of a generated maze
    // ─────────────────────────────────────────────────────────────
    public sealed class MazeData8
    {
        // ── Metadata ──────────────────────────────────────────────
        public int   Width          { get; }
        public int   Height         { get; }
        public int   Seed           { get; }
        public int   Level          { get; }
        public long  Timestamp      { get; }
        public float DifficultyFactor { get; internal set; }   // set by generator

        // ── Entry / Exit ──────────────────────────────────────────
        public (int x, int z) SpawnCell { get; private set; }
        public (int x, int z) ExitCell  { get; private set; }

        // ── Grid — ushort[x, z] ───────────────────────────────────
        private readonly CellFlags8[,] _cells;

        public MazeData8(int width, int height, int seed, int level)
        {
            Width     = width;
            Height    = height;
            Seed      = seed;
            Level     = level;
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _cells    = new CellFlags8[width, height];
        }

        // ── Cell accessors ────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────────
        //  Binary file constants  — LAV8S v2
        //
        //  File layout (all values little-endian):
        //  ┌─────────┬────────┬────────────────────────────────────┐
        //  │ Offset  │ Bytes  │ Field                              │
        //  ├─────────┼────────┼────────────────────────────────────┤
        //  │  0      │  5     │ Magic  "LAV8S"                     │
        //  │  5      │  1     │ Version (2)                        │
        //  │  6      │  2     │ Width   (int16)                    │
        //  │  8      │  2     │ Height  (int16)                    │
        //  │ 10      │  4     │ Seed    (int32)                    │
        //  │ 14      │  4     │ Level   (int32)                    │
        //  │ 18      │  8     │ Timestamp (int64, UTC unix secs)   │
        //  │ 26      │  2     │ SpawnX  (int16)                    │
        //  │ 28      │  2     │ SpawnZ  (int16)                    │
        //  │ 30      │  2     │ ExitX   (int16)                    │
        //  │ 32      │  2     │ ExitZ   (int16)                    │
        //  │ 34      │  4     │ DifficultyFactor (float32)         │
        //  │ 38      │ W×H×2  │ Cell data — ushort per cell (LE)   │
        //  │ 38+W*H*2│  4     │ Checksum XOR-fold (uint32)         │
        //  └─────────┴────────┴────────────────────────────────────┘
        //  Total: 42 + (W × H × 2) bytes
        //    Level  0 (12×12) →   330 bytes
        //    Level 39 (51×51) → 5,244 bytes
        // ─────────────────────────────────────────────────────────
        public static readonly byte[] MAGIC    = System.Text.Encoding.ASCII.GetBytes("LAV8S");
        public const  byte            VERSION  = 2;
        public const  int             HEADER_SIZE = 38;   // bytes before cell data
    }
}
