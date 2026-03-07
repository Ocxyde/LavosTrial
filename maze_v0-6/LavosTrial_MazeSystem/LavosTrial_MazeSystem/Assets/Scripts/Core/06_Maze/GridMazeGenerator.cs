// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details

using System;
using System.Collections.Generic;
using UnityEngine;

namespace LavosTrial.Core.Maze
{
    // ─────────────────────────────────────────────────────────────
    //  GridMazeGenerator
    //
    //  Algorithm:  Recursive Backtracker (DFS) for perfect maze,
    //              then A* to carve guaranteed entrance→exit path.
    //
    //  Usage:
    //      var gen  = new GridMazeGenerator();
    //      var data = gen.Generate(seed: 42, level: 0, config: cfg);
    // ─────────────────────────────────────────────────────────────
    public sealed class GridMazeGenerator
    {
        // ── A* node ───────────────────────────────────────────────
        private sealed class AStarNode : IComparable<AStarNode>
        {
            public int X, Z, G, H;
            public AStarNode Parent;
            public int F => G + H;
            public int CompareTo(AStarNode other) => F.CompareTo(other.F);
        }

        // ─────────────────────────────────────────────────────────
        public MazeData Generate(int seed, int level, MazeConfig cfg)
        {
            int size = Mathf.Clamp(cfg.BaseSize + level, cfg.MinSize, cfg.MaxSize);
            // Ensure odd dimensions — required for wall-based grid
            if (size % 2 == 0) size++;

            var rng  = new System.Random(seed);
            var data = new MazeData(size, size, seed, level);

            // 1 ─ Fill everything with walls
            FillAllWalls(data);

            // 2 ─ Carve passages via recursive backtracker (DFS)
            bool[,] visited = new bool[size, size];
            CarvePassages(data, rng, visited, 1, 1);

            // 3 ─ Guarantee 5×5 spawn room at top-left interior
            int spawnX = 1, spawnZ = 1;
            CarveSpawnRoom(data, spawnX, spawnZ, cfg.SpawnRoomSize);
            data.SetSpawn(spawnX, spawnZ);

            // 4 ─ Place exit at opposite corner
            int exitX = size - 2, exitZ = size - 2;
            data.SetExit(exitX, exitZ);
            // Carve a door gap on the east border wall
            data.ClearFlag(exitX, exitZ, CellFlags.WallE);
            data.ClearFlag(exitX, exitZ, CellFlags.WallS);

            // 5 ─ A* guaranteed path from spawn to exit
            //      (forces corridor even if DFS missed it)
            EnsurePathAStar(data, spawnX, spawnZ, exitX, exitZ, rng);

            // 6 ─ Torch placement (30 % of wall-adjacent cells)
            PlaceTorches(data, rng, cfg.TorchChance);

            // 7 ─ Chest & enemy placement
            PlaceObjects(data, rng, cfg.ChestDensity, cfg.EnemyDensity);

            return data;
        }

        // ─────────────────────────────────────────────────────────
        //  Step 1 — Fill
        // ─────────────────────────────────────────────────────────
        private static void FillAllWalls(MazeData d)
        {
            for (int x = 0; x < d.Width;  x++)
            for (int z = 0; z < d.Height; z++)
                d.SetCell(x, z, CellFlags.AllWalls);
        }

        // ─────────────────────────────────────────────────────────
        //  Step 2 — Recursive Backtracker (DFS)
        // ─────────────────────────────────────────────────────────
        private static void CarvePassages(MazeData d, System.Random rng,
                                          bool[,] visited, int cx, int cz)
        {
            visited[cx, cz] = true;

            // Shuffle directions
            Direction[] dirs = { Direction.North, Direction.South, Direction.East, Direction.West };
            Shuffle(dirs, rng);

            foreach (var dir in dirs)
            {
                var (dx, dz) = DirectionHelper.ToOffset(dir);
                int nx = cx + dx * 2;   // step 2 cells (wall between)
                int nz = cz + dz * 2;

                if (!d.InBounds(nx, nz) || visited[nx, nz]) continue;

                // Remove wall between cx,cz and nx,nz
                RemoveWall(d, cx, cz, dir);
                RemoveWall(d, nx, nz, DirectionHelper.Opposite(dir));

                // Also clear the intermediate wall cell
                int wx = cx + dx, wz = cz + dz;
                d.SetCell(wx, wz, CellFlags.None);

                CarvePassages(d, rng, visited, nx, nz);
            }
        }

        private static void RemoveWall(MazeData d, int x, int z, Direction dir)
            => d.ClearFlag(x, z, DirectionHelper.ToWallFlag(dir));

        // ─────────────────────────────────────────────────────────
        //  Step 3 — Spawn Room (5×5 clear area)
        // ─────────────────────────────────────────────────────────
        private static void CarveSpawnRoom(MazeData d, int ox, int oz, int roomSize)
        {
            int half = roomSize / 2;
            for (int x = ox - half; x <= ox + half; x++)
            for (int z = oz - half; z <= oz + half; z++)
            {
                if (!d.InBounds(x, z)) continue;
                d.SetCell(x, z, CellFlags.None);   // clear walls
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Step 5 — A* guaranteed corridor
        // ─────────────────────────────────────────────────────────
        private static void EnsurePathAStar(MazeData d,
                                             int sx, int sz, int ex, int ez,
                                             System.Random rng)
        {
            // Simple A* on the cell grid; carve any walls we cross
            var open   = new SortedSet<AStarNode>(Comparer<AStarNode>.Create(
                             (a, b) => a.F != b.F ? a.F.CompareTo(b.F) : a.GetHashCode().CompareTo(b.GetHashCode())));
            var closed = new HashSet<(int, int)>();

            var start = new AStarNode { X = sx, Z = sz, G = 0, H = Heuristic(sx, sz, ex, ez) };
            open.Add(start);

            while (open.Count > 0)
            {
                var current = open.Min;
                open.Remove(current);

                if (current.X == ex && current.Z == ez)
                {
                    // Trace back and carve
                    var node = current;
                    while (node.Parent != null)
                    {
                        CarveStep(d, node.Parent.X, node.Parent.Z, node.X, node.Z);
                        node = node.Parent;
                    }
                    return;
                }

                closed.Add((current.X, current.Z));

                foreach (var dir in new[] { Direction.North, Direction.South, Direction.East, Direction.West })
                {
                    var (dx, dz) = DirectionHelper.ToOffset(dir);
                    int nx = current.X + dx, nz = current.Z + dz;
                    if (!d.InBounds(nx, nz) || closed.Contains((nx, nz))) continue;

                    // Cost: crossing a wall is expensive (we prefer open passages)
                    int extra = d.HasWall(current.X, current.Z, dir) ? 10 : 1;
                    int g = current.G + extra;

                    var next = new AStarNode
                    {
                        X = nx, Z = nz,
                        G = g,
                        H = Heuristic(nx, nz, ex, ez),
                        Parent = current
                    };
                    open.Add(next);
                }
            }
        }

        private static int Heuristic(int ax, int az, int bx, int bz)
            => Math.Abs(ax - bx) + Math.Abs(az - bz);

        private static void CarveStep(MazeData d, int fx, int fz, int tx, int tz)
        {
            // Determine direction from (fx,fz) → (tx,tz)
            int dx = tx - fx, dz = tz - fz;
            Direction dir = (dx, dz) switch
            {
                ( 0,  1) => Direction.North,
                ( 0, -1) => Direction.South,
                ( 1,  0) => Direction.East,
                (-1,  0) => Direction.West,
                _ => Direction.North
            };
            d.ClearFlag(fx, fz, DirectionHelper.ToWallFlag(dir));
            d.ClearFlag(tx, tz, DirectionHelper.ToWallFlag(DirectionHelper.Opposite(dir)));
        }

        // ─────────────────────────────────────────────────────────
        //  Step 6 — Torches
        // ─────────────────────────────────────────────────────────
        private static void PlaceTorches(MazeData d, System.Random rng, float chance)
        {
            for (int x = 0; x < d.Width;  x++)
            for (int z = 0; z < d.Height; z++)
            {
                var cell = d.GetCell(x, z);
                // Place on cells that have at least one wall (torch mounts on wall)
                bool hasAnyWall = (cell & CellFlags.AllWalls) != 0;
                if (hasAnyWall && (cell & CellFlags.SpawnRoom) == 0)
                    if (rng.NextDouble() < chance)
                        d.AddFlag(x, z, CellFlags.HasTorch);
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Step 7 — Chests & Enemies
        // ─────────────────────────────────────────────────────────
        private static void PlaceObjects(MazeData d, System.Random rng,
                                          float chestDensity, float enemyDensity)
        {
            for (int x = 0; x < d.Width;  x++)
            for (int z = 0; z < d.Height; z++)
            {
                var cell = d.GetCell(x, z);
                // Only open (non-wall-blocked) interior cells, skip spawn
                if ((cell & CellFlags.AllWalls) == CellFlags.AllWalls) continue;
                if ((cell & CellFlags.SpawnRoom) != 0) continue;

                if (rng.NextDouble() < chestDensity)
                    d.AddFlag(x, z, CellFlags.HasChest);
                else if (rng.NextDouble() < enemyDensity)
                    d.AddFlag(x, z, CellFlags.HasEnemy);
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Fisher-Yates shuffle
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
    //  MazeConfig — filled from GameConfig JSON at runtime
    // ─────────────────────────────────────────────────────────────
    [Serializable]
    public sealed class MazeConfig
    {
        public int   BaseSize      = 12;
        public int   MinSize       = 12;
        public int   MaxSize       = 51;
        public int   SpawnRoomSize = 5;
        public float TorchChance   = 0.30f;
        public float ChestDensity  = 0.03f;
        public float EnemyDensity  = 0.05f;
    }
}
