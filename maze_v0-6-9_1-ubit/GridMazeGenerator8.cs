// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8  |  Locale: en_US

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    // -------------------------------------------------------------------------
    // GridMazeGenerator8
    //
    // 8-axis procedural maze generator.
    // All difficulty parameters are computed via DifficultyScaler.
    //
    // Algorithm:
    //   1. Fill all cells with all 8 walls  (ushort = 0x00FF)
    //   2. Recursive Backtracker (DFS) over 8 axes
    //      Cardinal : step 2 along one axis
    //      Diagonal : step 2 along both axes, intermediate cell cleared
    //   3. Carve guaranteed 5x5 spawn room at (1,1)
    //   3.5 Carve rooms at dead-ends and crossroads (terminus + 3+ openings)
    //   4. Place exit at (W-2, H-2)
    //   5. A* spawn to exit  (Chebyshev heuristic, wall penalty x factor)
    //   6. Torch placement  (scaled chance)
    //   7. Chest + enemy placement (densities scaled by DifficultyScaler)
    //
    // Usage:
    //   var gen  = new GridMazeGenerator8();
    //   var data = gen.Generate(seed, level, config);
    // -------------------------------------------------------------------------
    public sealed class GridMazeGenerator8
    {
        // A* node
        private sealed class Node
        {
            public int  X, Z, G, H;
            public Node Parent;
            public int  F => G + H;
        }

        // -------------------------------------------------------------------------
        // PUBLIC - Generate
        // -------------------------------------------------------------------------
        public MazeData8 Generate(int seed, int level, MazeConfig8 cfg)
        {
            var scaler = cfg.Difficulty;
            int size   = scaler.MazeSize(level, cfg.BaseSize, cfg.MinSize, cfg.MaxSize);

            var rng  = new System.Random(seed);
            var data = new MazeData8(size, size, seed, level)
            {
                DifficultyFactor = scaler.Factor(level),
            };

            Debug.Log(
                $"[MazeGen8] L{level}  size={size}x{size}  " +
                $"factor={data.DifficultyFactor:F3}  seed={seed}");
            Debug.Log($"[MazeGen8] {scaler.Describe(level, cfg)}");

            // 1 - Fill
            FillAllWalls(data);

            // 2 - DFS (8 axes)
            var visited = new bool[size, size];
            CarvePassages8(data, rng, visited, 1, 1);

            // 3 - Spawn room
            CarveSpawnRoom(data, 1, 1, cfg.SpawnRoomSize);
            data.SetSpawn(1, 1);

            // 3.5 - Carve rooms at dead-ends and crossroads
            CarveRooms(data, cfg.RoomRadius);

            // 4 - Exit
            data.SetExit(size - 2, size - 2);

            // 5 - A* guaranteed path
            int wallPenalty = scaler.WallCrossPenalty(cfg.BaseWallPenalty, level);
            EnsurePath(
                data,
                data.SpawnCell.x, data.SpawnCell.z,
                data.ExitCell.x,  data.ExitCell.z,
                wallPenalty);

            // 6 - Torches
            float torchChance = scaler.TorchChance(cfg.TorchChance, level);
            PlaceTorches(data, rng, torchChance);

            // 7 - Objects
            float enemyDensity = scaler.EnemyDensity(cfg.EnemyDensity, level);
            float chestDensity = scaler.ChestDensity(cfg.ChestDensity, level);
            PlaceObjects(data, rng, chestDensity, enemyDensity);

            return data;
        }

        // -------------------------------------------------------------------------
        // 1 - Fill all walls
        // -------------------------------------------------------------------------
        private static void FillAllWalls(MazeData8 d)
        {
            for (int x = 0; x < d.Width;  x++)
            for (int z = 0; z < d.Height; z++)
                d.SetCell(x, z, CellFlags8.AllWalls);
        }

        // -------------------------------------------------------------------------
        // 2 - Recursive Backtracker (DFS) over 8 axes
        //
        // Cardinal step : 2 cells along one axis
        // Diagonal step : 2 cells along both axes  (dx=+/-2, dz=+/-2)
        // Intermediate  : cell at (cx+dx, cz+dz) fully cleared
        // -------------------------------------------------------------------------
        private static void CarvePassages8(
            MazeData8      d,
            System.Random  rng,
            bool[,]        visited,
            int            cx,
            int            cz)
        {
            visited[cx, cz] = true;

            var dirs = (Direction8[])Direction8Helper.All.Clone();
            Shuffle(dirs, rng);

            foreach (var dir in dirs)
            {
                var (dx, dz) = Direction8Helper.ToOffset(dir);
                int nx = cx + dx * 2;
                int nz = cz + dz * 2;

                if (!d.InBounds(nx, nz) || visited[nx, nz]) continue;

                d.ClearFlag(cx, cz, Direction8Helper.ToWallFlag(dir));
                d.ClearFlag(nx, nz, Direction8Helper.ToWallFlag(Direction8Helper.Opposite(dir)));

                // Clear the intermediate cell fully so no invisible wall blocks the passage
                d.SetCell(cx + dx, cz + dz, CellFlags8.None);

                CarvePassages8(d, rng, visited, nx, nz);
            }
        }

        // -------------------------------------------------------------------------
        // 3 - Carve spawn room
        // -------------------------------------------------------------------------
        private static void CarveSpawnRoom(MazeData8 d, int ox, int oz, int roomSize)
        {
            int half = roomSize / 2;
            for (int x = ox - half; x <= ox + half; x++)
            for (int z = oz - half; z <= oz + half; z++)
                if (d.InBounds(x, z))
                    d.SetCell(x, z, CellFlags8.None);
        }

        // -------------------------------------------------------------------------
        // 3.5 - CarveRooms
        //
        // Scans every open cell and classifies it:
        //   terminus   : exactly 1 open cardinal neighbour  (dead end)
        //   crossroads : 3 or more open cardinal neighbours (junction)
        //
        // At each such cell, carves a square room of radius roomRadius,
        // clearing all wall flags so the room is visually open.
        //
        // The HasRoomFlag is set on every cell of the carved room so that
        // CompleteMazeBuilder8 can later place doors on the boundary walls.
        // -------------------------------------------------------------------------
        private static void CarveRooms(MazeData8 d, int roomRadius)
        {
            // Collect candidates first to avoid modifying the grid mid-scan
            var candidates = new List<(int x, int z)>();

            for (int x = 1; x < d.Width  - 1; x++)
            for (int z = 1; z < d.Height - 1; z++)
            {
                var cell = d.GetCell(x, z);

                // Skip fully-walled cells (solid wall block, not a passage cell)
                if ((cell & CellFlags8.AllWalls) == CellFlags8.AllWalls) continue;

                // Count open cardinal neighbours only (diagonals do not form walkable rooms)
                int openCount = 0;
                foreach (var dir in Direction8Helper.All)
                {
                    if (Direction8Helper.IsDiagonal(dir)) continue;
                    if (!d.HasWall(x, z, dir))
                        openCount++;
                }

                bool isTerminus   = openCount == 1;
                bool isCrossroads = openCount >= 3;

                if (isTerminus || isCrossroads)
                    candidates.Add((x, z));
            }

            foreach (var (cx, cz) in candidates)
            {
                for (int dx = -roomRadius; dx <= roomRadius; dx++)
                for (int dz = -roomRadius; dz <= roomRadius; dz++)
                {
                    int nx = cx + dx;
                    int nz = cz + dz;
                    if (!d.InBounds(nx, nz)) continue;

                    // Clear all wall flags, keep object flags intact
                    var existing = d.GetCell(nx, nz);
                    d.SetCell(nx, nz, existing & CellFlags8.AllObjects);
                    d.AddFlag(nx, nz, CellFlags8.IsRoom);
                }
            }
        }

        // -------------------------------------------------------------------------
        // 5 - A* guaranteed path (8-directional, Chebyshev heuristic)
        //
        // Cost model:
        //   Cardinal move   = 10
        //   Diagonal move   = 14  (approx sqrt(2) x 10)
        //   Wall penalty    = BaseWallPenalty x DifficultyFactor
        //                     Higher level -> walls more expensive
        //                     -> A* picks longer but more open paths
        // -------------------------------------------------------------------------
        private static void EnsurePath(
            MazeData8 d,
            int       sx, int sz,
            int       ex, int ez,
            int       wallPenalty)
        {
            var open   = new List<Node>();
            var closed = new HashSet<int>();

            open.Add(new Node { X = sx, Z = sz, G = 0, H = Heuristic(sx, sz, ex, ez) });

            while (open.Count > 0)
            {
                int best = 0;
                for (int i = 1; i < open.Count; i++)
                    if (open[i].F < open[best].F) best = i;

                var current = open[best];
                open.RemoveAt(best);

                if (current.X == ex && current.Z == ez)
                {
                    var node = current;
                    while (node.Parent != null)
                    {
                        CarveStep(d, node.Parent.X, node.Parent.Z, node.X, node.Z);
                        node = node.Parent;
                    }
                    return;
                }

                int key = current.Z * d.Width + current.X;
                if (closed.Contains(key)) continue;
                closed.Add(key);

                foreach (var dir in Direction8Helper.All)
                {
                    var (dx, dz) = Direction8Helper.ToOffset(dir);
                    int nx = current.X + dx;
                    int nz = current.Z + dz;

                    if (!d.InBounds(nx, nz)) continue;
                    if (closed.Contains(nz * d.Width + nx)) continue;

                    int moveCost = Direction8Helper.IsDiagonal(dir) ? 14 : 10;
                    int penalty  = d.HasWall(current.X, current.Z, dir) ? wallPenalty : 0;
                    int g        = current.G + moveCost + penalty;

                    open.Add(new Node
                    {
                        X      = nx,
                        Z      = nz,
                        G      = g,
                        H      = Heuristic(nx, nz, ex, ez),
                        Parent = current,
                    });
                }
            }
        }

        // Chebyshev heuristic - exact for 8-directional movement
        private static int Heuristic(int ax, int az, int bx, int bz)
            => 10 * Math.Max(Math.Abs(ax - bx), Math.Abs(az - bz));

        private static void CarveStep(MazeData8 d, int fx, int fz, int tx, int tz)
        {
            Direction8 dir = (Math.Sign(tx - fx), Math.Sign(tz - fz)) switch
            {
                ( 0,  1) => Direction8.N,
                ( 0, -1) => Direction8.S,
                ( 1,  0) => Direction8.E,
                (-1,  0) => Direction8.W,
                ( 1,  1) => Direction8.NE,
                (-1,  1) => Direction8.NW,
                ( 1, -1) => Direction8.SE,
                (-1, -1) => Direction8.SW,
                _        => Direction8.N,
            };
            d.ClearFlag(fx, fz, Direction8Helper.ToWallFlag(dir));
            d.ClearFlag(tx, tz, Direction8Helper.ToWallFlag(Direction8Helper.Opposite(dir)));
        }

        // -------------------------------------------------------------------------
        // 6 - Torches
        // -------------------------------------------------------------------------
        private static void PlaceTorches(MazeData8 d, System.Random rng, float chance)
        {
            for (int x = 0; x < d.Width;  x++)
            for (int z = 0; z < d.Height; z++)
            {
                var cell     = d.GetCell(x, z);
                bool hasWall = (cell & CellFlags8.AllWalls)  != CellFlags8.None;
                bool isSpawn = (cell & CellFlags8.SpawnRoom) != CellFlags8.None;
                if (hasWall && !isSpawn && rng.NextDouble() < chance)
                    d.AddFlag(x, z, CellFlags8.HasTorch);
            }
        }

        // -------------------------------------------------------------------------
        // 7 - Chests + Enemies
        // -------------------------------------------------------------------------
        private static void PlaceObjects(
            MazeData8     d,
            System.Random rng,
            float         chestDensity,
            float         enemyDensity)
        {
            for (int x = 0; x < d.Width;  x++)
            for (int z = 0; z < d.Height; z++)
            {
                var cell = d.GetCell(x, z);
                if ((cell & CellFlags8.AllWalls)  == CellFlags8.AllWalls)  continue;
                if ((cell & CellFlags8.SpawnRoom) != CellFlags8.None)      continue;
                if ((cell & CellFlags8.IsExit)    != CellFlags8.None)      continue;

                if      (rng.NextDouble() < chestDensity) d.AddFlag(x, z, CellFlags8.HasChest);
                else if (rng.NextDouble() < enemyDensity) d.AddFlag(x, z, CellFlags8.HasEnemy);
            }
        }

        // -------------------------------------------------------------------------
        // Fisher-Yates in-place shuffle
        // -------------------------------------------------------------------------
        private static void Shuffle<T>(T[] arr, System.Random rng)
        {
            for (int i = arr.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
        }
    }

    // -------------------------------------------------------------------------
    // MazeConfig8 - all values from JSON, no hardcodes
    // -------------------------------------------------------------------------
    [Serializable]
    public sealed class MazeConfig8
    {
        // Maze size bounds
        public int   BaseSize        = 12;
        public int   MinSize         = 12;
        public int   MaxSize         = 51;
        public int   SpawnRoomSize   = 5;

        // Room carving radius at dead-ends and crossroads (step 3.5)
        // 1 = 3x3 room,  2 = 5x5 room
        public int   RoomRadius      = 1;

        // Base densities (before difficulty scaling)
        public float TorchChance     = 0.30f;
        public float ChestDensity    = 0.05f;
        public float EnemyDensity    = 0.03f;
        public int   BaseWallPenalty = 100;

        // Rendering
        public bool  DiagonalWalls   = true;

        // Difficulty curve
        public DifficultyScaler Difficulty = new DifficultyScaler();
    }
}
