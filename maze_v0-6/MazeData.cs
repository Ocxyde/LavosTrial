// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details

using System;

namespace LavosTrial.Core.Maze
{
    // ─────────────────────────────────────────────────────────────
    //  Cell wall flags — packed into 1 byte for binary storage
    //  Bit layout:
    //    0 = NORTH wall present
    //    1 = SOUTH wall present
    //    2 = EAST  wall present
    //    3 = WEST  wall present
    //    4 = Spawn room cell
    //    5 = Has chest
    //    6 = Has enemy
    //    7 = Has torch
    // ─────────────────────────────────────────────────────────────
    [Flags]
    public enum CellFlags : byte
    {
        None      = 0,
        WallN     = 1 << 0,   // 0x01
        WallS     = 1 << 1,   // 0x02
        WallE     = 1 << 2,   // 0x04
        WallW     = 1 << 3,   // 0x08
        SpawnRoom = 1 << 4,   // 0x10
        HasChest  = 1 << 5,   // 0x20
        HasEnemy  = 1 << 6,   // 0x40
        HasTorch  = 1 << 7,   // 0x80

        AllWalls  = WallN | WallS | WallE | WallW
    }

    // ─────────────────────────────────────────────────────────────
    //  Direction helpers
    // ─────────────────────────────────────────────────────────────
    public enum Direction { North = 0, South = 1, East = 2, West = 3 }

    public static class DirectionHelper
    {
        public static (int dx, int dz) ToOffset(Direction d) => d switch
        {
            Direction.North => ( 0,  1),
            Direction.South => ( 0, -1),
            Direction.East  => ( 1,  0),
            Direction.West  => (-1,  0),
            _ => (0, 0)
        };

        public static Direction Opposite(Direction d) => d switch
        {
            Direction.North => Direction.South,
            Direction.South => Direction.North,
            Direction.East  => Direction.West,
            Direction.West  => Direction.East,
            _ => d
        };

        public static CellFlags ToWallFlag(Direction d) => d switch
        {
            Direction.North => CellFlags.WallN,
            Direction.South => CellFlags.WallS,
            Direction.East  => CellFlags.WallE,
            Direction.West  => CellFlags.WallW,
            _ => CellFlags.None
        };
    }

    // ─────────────────────────────────────────────────────────────
    //  MazeData — immutable snapshot of a generated maze
    // ─────────────────────────────────────────────────────────────
    public sealed class MazeData
    {
        // ── Metadata ──────────────────────────────────────────────
        public int    Width     { get; }
        public int    Height    { get; }
        public int    Seed      { get; }
        public int    Level     { get; }
        public long   Timestamp { get; }          // Unix seconds (UTC)

        // ── Entry / Exit ──────────────────────────────────────────
        public (int x, int z) SpawnCell  { get; private set; }
        public (int x, int z) ExitCell   { get; private set; }

        // ── Grid ──────────────────────────────────────────────────
        // cells[x, z] — x = column, z = row
        private readonly CellFlags[,] _cells;

        public MazeData(int width, int height, int seed, int level)
        {
            Width     = width;
            Height    = height;
            Seed      = seed;
            Level     = level;
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _cells    = new CellFlags[width, height];
        }

        // ── Cell accessors ────────────────────────────────────────
        public CellFlags GetCell(int x, int z)              => _cells[x, z];
        public void      SetCell(int x, int z, CellFlags f) => _cells[x, z] = f;
        public void      AddFlag(int x, int z, CellFlags f) => _cells[x, z] |= f;
        public void      ClearFlag(int x, int z, CellFlags f)=> _cells[x, z] &= ~f;

        public bool HasWall(int x, int z, Direction d)
            => (_cells[x, z] & DirectionHelper.ToWallFlag(d)) != 0;

        public bool InBounds(int x, int z)
            => x >= 0 && x < Width && z >= 0 && z < Height;

        // ── Spawn / Exit setters (called by generator) ────────────
        public void SetSpawn(int x, int z)
        {
            SpawnCell = (x, z);
            AddFlag(x, z, CellFlags.SpawnRoom);
        }

        public void SetExit(int x, int z) => ExitCell = (x, z);

        // ── Binary serialisation header constants ─────────────────
        // File layout:
        //   [0..4]  Magic   "LAVOS"  (5 bytes)
        //   [5]     Version  byte    (1 = current)
        //   [6..7]  Width    int16
        //   [8..9]  Height   int16
        //   [10..13] Seed    int32
        //   [14..17] Level   int32
        //   [18..25] Timestamp int64
        //   [26..27] SpawnX  int16
        //   [28..29] SpawnZ  int16
        //   [30..31] ExitX   int16
        //   [32..33] ExitZ   int16
        //   [34 .. 34 + W*H - 1]  cells (1 byte each, row-major)
        //   [last 4] Checksum  uint32  (simple XOR fold)
        public static readonly byte[] MAGIC   = System.Text.Encoding.ASCII.GetBytes("LAVOS");
        public const byte            VERSION  = 1;
        public const int             HEADER_SIZE = 34;    // bytes before cell data

        // ── Flat cell index helper ────────────────────────────────
        public int CellIndex(int x, int z) => z * Width + x;
    }
}
