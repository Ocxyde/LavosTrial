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
// GridMazeGenerator.cs
// 8-axis maze generation with DFS + A* pathfinding
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    // ─────────────────────────────────────────────────────────────
    //  GridMazeGenerator
    //
    //  Generates a procedural maze on an 8-axis grid.
    //
    //  Algorithm:
    //    1. Fill all cells — all 8 walls set (ushort = 0x00FF)
    //    2. Recursive Backtracker (DFS) — shuffled over 8 axes
    //       Diagonal step: dx=±2, dz=±2  (intermediate cell cleared)
    //       Cardinal step: dx=±2 OR dz=±2 (standard wall removal)
    //    3. Carve guaranteed 5×5 spawn room at (1,1)
    //    4. Place exit at (W-2, H-2)
    //    5. A* from spawn → exit  (diagonal cost = 14, cardinal = 10)
    //    6. Torch placement  (30% of wall-adjacent, non-spawn cells)
    //    7. Chest + enemy placement on open interior cells
    //
    //  Diagonal passages share the same DFS logic — the intermediate
    //  cell at (cx+dx/2, cz+dz/2) is cleared of all walls so the
    //  visual corridor is unambiguous.
    //
    //  Usage:
    //      var gen  = new GridMazeGenerator();
    //      var data = gen.Generate(seed: 42, level: 0, cfg: mazeCfg);
    //      
    //  Backward compatibility:
    //      - GridSize returns data.Width
    //      - GetCell(x,z) returns data.GetCell(x,z)
    // ─────────────────────────────────────────────────────────────
    public sealed class GridMazeGenerator
    {
        // ─────────────────────────────────────────────────────────
        //  A* node
        // ─────────────────────────────────────────────────────────
        private sealed class Node
        {
            public int   X, Z;
            public int   G;           // cost from start
            public int   H;           // heuristic to goal
            public int   F => G + H;
            public Node  Parent;
        }

        // ─────────────────────────────────────────────────────────
        //  Generated maze data
        // ─────────────────────────────────────────────────────────
        private MazeData8 _generatedData;

        // ─────────────────────────────────────────────────────────
        //  PUBLIC — Generate with difficulty scaling
        // ─────────────────────────────────────────────────────────
        public MazeData8 Generate(int seed, int level, MazeConfig cfg, DifficultyScaler scaler = null)
        {
            // Use provided scaler or create default
            if (scaler == null) scaler = new DifficultyScaler();

            // Compute difficulty factor for this level
            float difficultyFactor = scaler.Factor(level);

            // Scale values using DifficultyScaler
            int size = scaler.MazeSize(level, cfg.BaseSize, cfg.MinSize, cfg.MaxSize);
            float scaledTorchChance = scaler.TorchChance(cfg.TorchChance, level);
            float scaledChestDensity = scaler.ChestDensity(cfg.ChestDensity, level);
            float scaledEnemyDensity = scaler.EnemyDensity(cfg.EnemyDensity, level);
            int scaledWallPenalty = scaler.WallCrossPenalty(cfg.BaseWallPenalty, level);

            Debug.Log($"[GridMazeGenerator] LEVEL {level} | factor={difficultyFactor:F3} | " +
                      $"size={size}×{size} | torch={scaledTorchChance:P1} | " +
                      $"chest={scaledChestDensity:P1} | enemy={scaledEnemyDensity:P1} | " +
                      $"wallPenalty={scaledWallPenalty}");

            var rng  = new System.Random(seed);
            var data = new MazeData8(size, size, seed, level);

            // Store difficulty factor in data for binary save
            data.DifficultyFactor = difficultyFactor;

            // ── Step 1: fill all walls ────────────────────────────
            FillAllWalls(data);

            // ── Step 2: DFS over 8 axes ───────────────────────────
            var visited = new bool[size, size];
            CarvePassages8(data, rng, visited, 1, 1);

            // ── Step 3: spawn room ────────────────────────────────
            CarveSpawnRoom(data, 1, 1, cfg.SpawnRoomSize);
            data.SetSpawn(1, 1);

            // ── Step 4: exit ──────────────────────────────────────
            data.SetExit(size - 2, size - 2);

            // ── Step 5: A* guaranteed path ────────────────────────
            EnsurePath(data,
                       data.SpawnCell.x, data.SpawnCell.z,
                       data.ExitCell.x,  data.ExitCell.z,
                       scaledWallPenalty);  // Use scaled wall penalty

            // ── Step 6: torches ───────────────────────────────────
            PlaceTorches(data, rng, scaledTorchChance);

            // ── Step 7: chests + enemies ──────────────────────────
            PlaceObjects(data, rng, scaledChestDensity, scaledEnemyDensity);

            // Store for backward compatibility accessors
            _generatedData = data;

            return data;
        }

        // ─────────────────────────────────────────────────────────
        //  Backward Compatibility API (for legacy code)
        // ─────────────────────────────────────────────────────────
        public int GridSize => _generatedData?.Width ?? 0;
        
        public CellFlags8 GetCell(int x, int z)
        {
            return _generatedData?.GetCell(x, z) ?? CellFlags8.AllWalls;
        }

        // ─────────────────────────────────────────────────────────
        //  Step 1 — Fill every cell with all 8 walls
        // ─────────────────────────────────────────────────────────
        private static void FillAllWalls(MazeData8 d)
        {
            for (int x = 0; x < d.Width;  x++)
            for (int z = 0; z < d.Height; z++)
                d.SetCell(x, z, CellFlags8.AllWalls);
        }

        // ─────────────────────────────────────────────────────────
        //  Step 2 — Recursive Backtracker (DFS), 8 directions
        //
        //  For cardinal directions:
        //    step = 2 cells along one axis
        //    intermediate wall cell = (cx+dx, cz+dz)
        //
        //  For diagonal directions:
        //    step = 2 cells along both axes  (cx+2, cz+2) etc.
        //    intermediate cell = (cx+dx, cz+dz) — the shared corner
        //    We clear all walls on the intermediate cell so no
        //    invisible collision blocks diagonal traversal.
        // ─────────────────────────────────────────────────────────
        private static void CarvePassages8(MazeData8 d, System.Random rng,
                                            bool[,] visited, int cx, int cz)
        {
            visited[cx, cz] = true;

            // Shuffle all 8 directions
            var dirs = (Direction8[])Direction8Helper.All.Clone();
            Shuffle(dirs, rng);

            foreach (var dir in dirs)
            {
                var (dx, dz) = Direction8Helper.ToOffset(dir);

                // Destination cell — 2 steps away
                int nx = cx + dx * 2;
                int nz = cz + dz * 2;

                if (!d.InBounds(nx, nz) || visited[nx, nz]) continue;

                // ── Remove walls on both ends ─────────────────────
                d.ClearFlag(cx, cz, Direction8Helper.ToWallFlag(dir));
                d.ClearFlag(nx, nz, Direction8Helper.ToWallFlag(Direction8Helper.Opposite(dir)));

                // ── Clear the intermediate cell ───────────────────
                int wx = cx + dx, wz = cz + dz;
                d.SetCell(wx, wz, CellFlags8.None);

                CarvePassages8(d, rng, visited, nx, nz);
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Step 3 — Carve spawn room (square, centered on ox, oz)
        // ─────────────────────────────────────────────────────────
        private static void CarveSpawnRoom(MazeData8 d, int ox, int oz, int roomSize)
        {
            int half = roomSize / 2;
            for (int x = ox - half; x <= ox + half; x++)
            for (int z = oz - half; z <= oz + half; z++)
            {
                if (d.InBounds(x, z))
                    d.SetCell(x, z, CellFlags8.None);   // wipe all wall flags
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Step 5 — A* guaranteed path (8-directional movement)
        //
        //  Cost model (standard diagonal A*):
        //    Cardinal move   = 10
        //    Diagonal move   = 14  (approx √2 × 10)
        //    Crossing a wall = +wallPenalty (from DifficultyScaler)
        // ─────────────────────────────────────────────────────────
        private static void EnsurePath(MazeData8 d,
                                        int sx, int sz, int ex, int ez,
                                        int wallPenalty = 100)
        {
            // Open set — sorted by F cost; use list + linear min for simplicity
            var open   = new List<Node>();
            var closed = new HashSet<int>();   // packed key = z*Width + x

            open.Add(new Node { X = sx, Z = sz, G = 0, H = Heuristic8(sx, sz, ex, ez) });

            while (open.Count > 0)
            {
                // Find minimum F
                int  best  = 0;
                for (int i = 1; i < open.Count; i++)
                    if (open[i].F < open[best].F) best = i;

                var current = open[best];
                open.RemoveAt(best);

                if (current.X == ex && current.Z == ez)
                {
                    // Trace path back and carve any walls
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

                // Expand all 8 neighbours
                foreach (var dir in Direction8Helper.All)
                {
                    var (dx, dz) = Direction8Helper.ToOffset(dir);
                    int nx = current.X + dx;
                    int nz = current.Z + dz;

                    if (!d.InBounds(nx, nz)) continue;
                    if (closed.Contains(nz * d.Width + nx)) continue;

                    // Movement cost
                    int moveCost = Direction8Helper.IsDiagonal(dir) ? 14 : 10;
                    // Wall crossing penalty (scaled by difficulty)
                    int penalty = d.HasWall(current.X, current.Z, dir) ? wallPenalty : 0;
                    int g = current.G + moveCost + penalty;

                    open.Add(new Node
                    {
                        X      = nx,
                        Z      = nz,
                        G      = g,
                        H      = Heuristic8(nx, nz, ex, ez),
                        Parent = current,
                    });
                }
            }
        }

        // Chebyshev heuristic — correct for 8-directional movement
        private static int Heuristic8(int ax, int az, int bx, int bz)
        {
            int dx = Math.Abs(ax - bx);
            int dz = Math.Abs(az - bz);
            // Chebyshev × 10 (matches movement cost scale)
            return 10 * Math.Max(dx, dz);
        }

        private static void CarveStep(MazeData8 d, int fx, int fz, int tx, int tz)
        {
            int ddx = tx - fx;
            int ddz = tz - fz;

            // Map delta → Direction8
            Direction8 dir = (Math.Sign(ddx), Math.Sign(ddz)) switch
            {
                ( 0,  1) => Direction8.N,
                ( 0, -1) => Direction8.S,
                ( 1,  0) => Direction8.E,
                (-1,  0) => Direction8.W,
                ( 1,  1) => Direction8.NE,
                (-1,  1) => Direction8.NW,
                ( 1, -1) => Direction8.SE,
                (-1, -1) => Direction8.SW,
                _ => Direction8.N
            };

            d.ClearFlag(fx, fz, Direction8Helper.ToWallFlag(dir));
            d.ClearFlag(tx, tz, Direction8Helper.ToWallFlag(Direction8Helper.Opposite(dir)));
        }

        // ─────────────────────────────────────────────────────────
        //  Step 6 — Torch placement
        // ─────────────────────────────────────────────────────────
        private static void PlaceTorches(MazeData8 d, System.Random rng, float chance)
        {
            for (int x = 0; x < d.Width;  x++)
            for (int z = 0; z < d.Height; z++)
            {
                var cell = d.GetCell(x, z);
                bool hasAnyWall  = (cell & CellFlags8.AllWalls) != CellFlags8.None;
                bool isSpawnRoom = (cell & CellFlags8.SpawnRoom) != CellFlags8.None;

                if (hasAnyWall && !isSpawnRoom && rng.NextDouble() < chance)
                    d.AddFlag(x, z, CellFlags8.HasTorch);
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Step 7 — Chest + enemy placement
        // ─────────────────────────────────────────────────────────
        private static void PlaceObjects(MazeData8 d, System.Random rng,
                                          float chestDensity, float enemyDensity)
        {
            for (int x = 0; x < d.Width;  x++)
            for (int z = 0; z < d.Height; z++)
            {
                var cell = d.GetCell(x, z);
                if ((cell & CellFlags8.AllWalls)  == CellFlags8.AllWalls)  continue;
                if ((cell & CellFlags8.SpawnRoom) != CellFlags8.None)      continue;
                if ((cell & CellFlags8.IsExit)    != CellFlags8.None)      continue;

                if (rng.NextDouble() < chestDensity)
                    d.AddFlag(x, z, CellFlags8.HasChest);
                else if (rng.NextDouble() < enemyDensity)
                    d.AddFlag(x, z, CellFlags8.HasEnemy);
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Fisher-Yates in-place shuffle
        // ─────────────────────────────────────────────────────────
        private static void Shuffle<T>(T[] arr, System.Random rng)
        {
            for (int i = arr.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  MazeConfig  —  all values loaded from JSON, no hardcodes
    // ─────────────────────────────────────────────────────────────
    [Serializable]
    public sealed class MazeConfig
    {
        public int   BaseSize       = 12;
        public int   MinSize        = 12;
        public int   MaxSize        = 51;
        public int   SpawnRoomSize  = 5;
        public float TorchChance    = 0.30f;
        public float ChestDensity   = 0.03f;
        public float EnemyDensity   = 0.05f;
        public bool  DiagonalWalls  = true;   // toggle diagonal wall rendering
        
        // A* pathfinding
        public int BaseWallPenalty = 100;  // penalty for crossing a wall
    }
}
