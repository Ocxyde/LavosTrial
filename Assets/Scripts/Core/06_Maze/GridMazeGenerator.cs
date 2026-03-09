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
// UPDATED 2026-03-09: Cardinal-only passages, guaranteed paths, dead-end corridors
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    // ─────────────────────────────────────────────────────────────
    //  GridMazeGenerator
    //
    //  Generates a procedural maze on a 4-axis (cardinal) grid.
    //
    //  Algorithm:
    //    1. Fill all cells — all 4 walls set (ushort = 0x000F)
    //    2. Recursive Backtracker (DFS) — 4 cardinal directions only
    //       Cardinal step: dx=±2 OR dz=±2 (standard wall removal)
    //       NO diagonal passages - ensures clear wall alignment
    //    3. Carve guaranteed 5×5 spawn room at (1,1)
    //    4. Place exit at (W-2, H-2)
    //    5. A* from spawn → exit (cardinal only, cost = 10)
    //       Guarantees passage even if DFS fails
    //    6. Add dead-end corridors (branching from main path)
    //    7. Torch placement  (30% of wall-adjacent, non-spawn cells)
    //    8. Chest + enemy placement on open interior cells
    //
    //  Key Changes (2026-03-09):
    //  - Removed diagonal wall carving from DFS
    //  - Guaranteed A* path ensures passage
    //  - Dead-end corridors add complexity
    //  - Corridor choices at intersections
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
            float scaledDeadEndDensity = scaler.DeadEndDensity(cfg.DeadEndDensity, level);

            Debug.Log($"[GridMazeGenerator] LEVEL {level} | factor={difficultyFactor:F3} | " +
                      $"size={size}×{size} | torch={scaledTorchChance:P1} | " +
                      $"chest={scaledChestDensity:P1} | enemy={scaledEnemyDensity:P1} | " +
                      $"deadEnd={scaledDeadEndDensity:P1} | wallPenalty={scaledWallPenalty}");

            var rng  = new System.Random(seed);
            var data = new MazeData8(size, size, seed, level);

            // Store difficulty factor in data for binary save
            data.DifficultyFactor = difficultyFactor;

            // ── Step 1: fill all walls ────────────────────────────
            FillAllWalls(data);

            // ── Step 2: DFS over 4 cardinal axes ONLY ─────────────
            //      No diagonal passages - ensures clean wall alignment
            var visited = new bool[size, size];
            CarvePassagesCardinal(data, rng, visited, 1, 1);

            // ── Step 3: spawn room ────────────────────────────────
            CarveSpawnRoom(data, 1, 1, cfg.SpawnRoomSize);
            data.SetSpawn(1, 1);

            // ── Step 4: exit ──────────────────────────────────────
            data.SetExit(size - 2, size - 2);

            // ── Step 5: A* guaranteed path (cardinal only) ────────
            //      Ensures passage even if DFS creates isolated sections
            EnsurePathCardinal(data,
                       data.SpawnCell.x, data.SpawnCell.z,
                       data.ExitCell.x,  data.ExitCell.z,
                       scaledWallPenalty);

            // ── Step 6: Add dead-end corridors ────────────────────
            //      Uses DeadEndCorridorSystem with mathematical distribution
            //      - Difficulty-scaled density (level-based)
            //      - Poisson distribution for natural spacing
            //      - Built-in corridor termination detection
            //      - All values from JSON config
            AddDeadEndCorridorsSystem(data, rng, cfg, scaledDeadEndDensity, level);

            // ── Step 7: torches ───────────────────────────────────
            PlaceTorches(data, rng, scaledTorchChance);

            // ── Step 8: chests + enemies ──────────────────────────
            PlaceObjects(data, rng, scaledChestDensity, scaledEnemyDensity);

            // Store for backward compatibility accessors
            _generatedData = data;

            Debug.Log($"[GridMazeGenerator] Maze generated: {size}x{size}, " +
                      $"spawn=({data.SpawnCell.x},{data.SpawnCell.z}), " +
                      $"exit=({data.ExitCell.x},{data.ExitCell.z})");

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
            if (d == null) throw new ArgumentNullException(nameof(d));
    
            uint wallMask = (uint)CellFlags8.AllWalls;
    
            for (int x = 0; x < d.Width; x++)
            {
                for (int z = 0; z < d.Height; z++)
                {
                    var existing = d.GetCell(x, z);
                    CellFlags8 newValue = existing | (CellFlags8)wallMask;
            
                    if (newValue != (CellFlags8)existing)
                    {
                        d.SetCell(x, z, newValue);
                    }
                }
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Step 2 — Recursive Backtracker (DFS), 4 CARDINAL directions ONLY
        //
        //  Cardinal directions only (N, S, E, W):
        //    step = 2 cells along one axis
        //    wall cell between = (cx+dx, cz) or (cx, cz+dz)
        //    Wall is cleared between current and next cell
        //
        //  NO DIAGONAL PASSAGES:
        //    - Diagonals removed to ensure clean wall alignment
        //    - All corridors are straight (N-S or E-W)
        //    - Walls snap perfectly to grid boundaries
        // ─────────────────────────────────────────────────────────
        private static void CarvePassagesCardinal(MazeData8 d, System.Random rng,
                                            bool[,] visited, int cx, int cz)
        {
            visited[cx, cz] = true;

            // Shuffle 4 cardinal directions only (N, S, E, W)
            var dirs = new Direction8[] { Direction8.N, Direction8.S, Direction8.E, Direction8.W };
            Shuffle(dirs, rng);

            foreach (var dir in dirs)
            {
                var (dx, dz) = Direction8Helper.ToOffset(dir);

                // Destination cell — 2 steps away (cardinal only)
                int nx = cx + dx * 2;
                int nz = cz + dz * 2;

                if (!d.InBounds(nx, nz) || visited[nx, nz]) continue;

                // ── Remove wall between current and next ──────────
                // Clear wall flag in current cell (facing direction)
                d.ClearFlag(cx, cz, Direction8Helper.ToWallFlag(dir));
                // Clear opposite wall flag in next cell
                d.ClearFlag(nx, nz, Direction8Helper.ToWallFlag(Direction8Helper.Opposite(dir)));

                // ── Clear the wall cell between ───────────────────
                // The cell between current and next becomes a passage
                int wallX = cx + dx, wallZ = cz + dz;
                if (d.InBounds(wallX, wallZ))
                {
                    d.SetCell(wallX, wallZ, CellFlags8.None);
                }

                // Recurse into next cell
                CarvePassagesCardinal(d, rng, visited, nx, nz);
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
        //  Step 5 — A* guaranteed path (CARDINAL 4-directional movement)
        //
        //  Cost model (cardinal A* only):
        //    Cardinal move   = 10
        //    Crossing a wall = +wallPenalty (from DifficultyScaler)
        //
        //  GUARANTEES PASSAGE:
        //    - If DFS creates isolated sections, A* carves a path
        //    - Ensures player can always reach exit
        //    - Only cardinal movements (no diagonals)
        // ─────────────────────────────────────────────────────────
        private static void EnsurePathCardinal(MazeData8 d,
                                        int sx, int sz, int ex, int ez,
                                        int wallPenalty = 100)
        {
            // Open set — sorted by F cost; use list + linear min for simplicity
            var open   = new List<Node>();
            var closed = new HashSet<int>();   // packed key = z*Width + x

            open.Add(new Node { X = sx, Z = sz, G = 0, H = HeuristicCardinal(sx, sz, ex, ez) });

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
                        CarveStepCardinal(d, node.Parent.X, node.Parent.Z, node.X, node.Z);
                        node = node.Parent;
                    }
                    Debug.Log("[GridMazeGenerator] A*: Guaranteed path carved successfully");
                    return;
                }

                int key = current.Z * d.Width + current.X;
                if (closed.Contains(key)) continue;
                closed.Add(key);

                // Expand 4 cardinal neighbours only (N, S, E, W)
                var cardinalDirs = new Direction8[] { Direction8.N, Direction8.S, Direction8.E, Direction8.W };
                foreach (var dir in cardinalDirs)
                {
                    var (dx, dz) = Direction8Helper.ToOffset(dir);
                    int nx = current.X + dx;
                    int nz = current.Z + dz;

                    if (!d.InBounds(nx, nz)) continue;
                    if (closed.Contains(nz * d.Width + nx)) continue;

                    // Movement cost (cardinal only = 10)
                    int moveCost = 10;
                    // Wall crossing penalty (scaled by difficulty)
                    int penalty = d.HasWall(current.X, current.Z, dir) ? wallPenalty : 0;
                    int g = current.G + moveCost + penalty;

                    open.Add(new Node
                    {
                        X      = nx,
                        Z      = nz,
                        G      = g,
                        H      = HeuristicCardinal(nx, nz, ex, ez),
                        Parent = current,
                    });
                }
            }

            Debug.LogWarning("[GridMazeGenerator] A*: Could not find path - maze may have isolated sections");
        }

        // Manhattan heuristic — correct for 4-directional (cardinal) movement
        private static int HeuristicCardinal(int ax, int az, int bx, int bz)
        {
            int dx = Math.Abs(ax - bx);
            int dz = Math.Abs(az - bz);
            // Manhattan × 10 (matches movement cost scale)
            return 10 * (dx + dz);
        }

        private static void CarveStepCardinal(MazeData8 d, int fx, int fz, int tx, int tz)
        {
            int ddx = tx - fx;
            int ddz = tz - fz;

            // Map delta → Direction8 (cardinal only)
            Direction8 dir = (Math.Sign(ddx), Math.Sign(ddz)) switch
            {
                ( 0,  1) => Direction8.N,
                ( 0, -1) => Direction8.S,
                ( 1,  0) => Direction8.E,
                (-1,  0) => Direction8.W,
                _ => Direction8.N  // Default (should not happen for cardinal)
            };

            // Clear wall between from and to cells
            d.ClearFlag(fx, fz, Direction8Helper.ToWallFlag(dir));
            d.ClearFlag(tx, tz, Direction8Helper.ToWallFlag(Direction8Helper.Opposite(dir)));
        }

        // ─────────────────────────────────────────────────────────
        //  Step 6 — Add Dead-End Corridors
        //
        //  Creates branching paths from existing corridors:
        //    1. Find all passage cells (no walls, not spawn, not exit)
        //    2. For each passage cell, try to extend in random direction
        //    3. Create dead-end corridor of random length (2-5 cells)
        //    4. Chance to add chest/enemy at dead-end
        //
        //  CORRIDOR CHOICES:
        //    - Intersections have 2-4 possible directions
        //    - Player must choose correct path
        //    - Dead-ends contain rewards or enemies
        //
        //  DEAD-END DENSITY SCALING:
        //    - Base density from MazeConfig.DeadEndDensity
        //    - Scaled by DifficultyScaler.DeadEndDensity()
        //    - Level 0: ~15% base chance (increased from 30% spawn rate)
        //    - Level 39: ~37.5% (2.5× multiplier at max level)
        //    - More dead-ends = more complex maze, more choices
        // ─────────────────────────────────────────────────────────
        private static void AddDeadEndCorridors(MazeData8 d, System.Random rng, MazeConfig cfg, float scaledDeadEndDensity)
        {
            int deadEndCount = 0;
            int maxDeadEnds = d.Width * d.Height / 20;  // Max 5% of grid

            // Find all passage cells (walkable, not spawn, not exit)
            var passageCells = new List<(int x, int z)>();
            for (int x = 0; x < d.Width; x++)
            {
                for (int z = 0; z < d.Height; z++)
                {
                    var cell = d.GetCell(x, z);
                    bool isPassage = (cell & CellFlags8.AllWalls) == CellFlags8.None;
                    bool isSpawn = (cell & CellFlags8.SpawnRoom) != CellFlags8.None;
                    bool isExit = (cell & CellFlags8.IsExit) != CellFlags8.None;

                    if (isPassage && !isSpawn && !isExit)
                    {
                        passageCells.Add((x, z));
                    }
                }
            }

            // Shuffle passage cells for random distribution
            Shuffle(passageCells.ToArray(), rng);

            // Try to create dead-end corridors using scaled density
            // Higher levels = higher density = more attempts
            foreach (var (px, pz) in passageCells)
            {
                if (deadEndCount >= maxDeadEnds) break;

                // Use scaled dead-end density for spawn chance
                // Level 0: ~15% chance, Level 39: ~37.5% chance
                if (rng.NextDouble() > scaledDeadEndDensity) continue;

                // Try 4 cardinal directions
                var dirs = new Direction8[] { Direction8.N, Direction8.S, Direction8.E, Direction8.W };
                Shuffle(dirs, rng);

                foreach (var dir in dirs)
                {
                    var (dx, dz) = Direction8Helper.ToOffset(dir);

                    // Check if we can extend in this direction (wall exists)
                    int nextX = px + dx;
                    int nextZ = pz + dz;

                    if (!d.InBounds(nextX, nextZ)) continue;

                    var nextCell = d.GetCell(nextX, nextZ);
                    bool isWall = (nextCell & CellFlags8.AllWalls) == CellFlags8.AllWalls;

                    if (!isWall) continue;

                    // Carve dead-end corridor (2-5 cells long)
                    int corridorLength = rng.Next(2, 6);
                    int carvedLength = 0;

                    int currX = px, currZ = pz;
                    for (int i = 0; i < corridorLength; i++)
                    {
                        int carveX = currX + dx * (i + 1);
                        int carveZ = currZ + dz * (i + 1);

                        if (!d.InBounds(carveX, carveZ)) break;

                        var carveCell = d.GetCell(carveX, carveZ);
                        if ((carveCell & CellFlags8.AllWalls) == CellFlags8.None)
                        {
                            // Hit existing passage, stop
                            break;
                        }

                        // Clear this cell (make it a passage)
                        d.SetCell(carveX, carveZ, CellFlags8.None);
                        carvedLength++;
                        currX = carveX;
                        currZ = carveZ;
                    }

                    if (carvedLength >= 2)
                    {
                        // Successfully created dead-end corridor
                        deadEndCount++;

                        // 50% chance to add chest at end
                        if (rng.NextDouble() < 0.5f)
                        {
                            d.AddFlag(currX, currZ, CellFlags8.HasChest);
                        }
                        // 30% chance to add enemy at end (if no chest)
                        else if (rng.NextDouble() < 0.3f)
                        {
                            d.AddFlag(currX, currZ, CellFlags8.HasEnemy);
                        }

                        Debug.Log($"[GridMazeGenerator] Dead-end corridor #{deadEndCount} carved at ({currX},{currZ}), length={carvedLength}");
                    }
                }
            }

            Debug.Log($"[GridMazeGenerator] Total dead-end corridors added: {deadEndCount}");
        }

        // ─────────────────────────────────────────────────────────
        //  Step 6 — Add Dead-End Corridors (NEW SYSTEM)
        //
        //  Uses DeadEndCorridorSystem with mathematical distribution:
        //    - Difficulty-scaled density (level-based)
        //    - Poisson distribution for natural spacing
        //    - Built-in corridor termination detection
        //    - All values from JSON config (DeadEndCorridorConfig.json)
        //
        //  MATHEMATICAL FORMULA:
        //    ScaledDensity = BaseDensity × Lerp(1.0, MaxMultiplier, t^Exponent)
        //    Where t = level / MaxLevel (39)
        //
        //  CORRIDOR MATH:
        //    - Length: MinLength → MaxLength (2-5 cells default)
        //    - Width: CorridorWidth (1 cell = 6m fixed)
        //    - Max Dead-Ends: Min(5% grid, grid/MinLength)
        //    - Spawn: Passage cells adjacent to walls
        //
        //  DIFFICULTY SCALING:
        //    Level 0:  15% density, 1.0x factor
        //    Level 10: 21% density, 1.75x factor
        //    Level 39: 37.5% density, 3.0x factor
        // ─────────────────────────────────────────────────────────
        private static void AddDeadEndCorridorsSystem(MazeData8 d, System.Random rng, MazeConfig cfg, float scaledDeadEndDensity, int level)
        {
            // Create dead-end corridor system with config
            var deadEndSystem = new DeadEndCorridorSystem();
            
            // Create config that uses the scaled density from MazeConfig
            var config = DeadEndCorridorSystem.CreateDefaultConfig();
            config.BaseDensity = scaledDeadEndDensity;  // Use already-scaled density
            
            Debug.Log($"[GridMazeGenerator] Dead-End Config: BaseDensity={scaledDeadEndDensity:P1}, Level={level}");

            // Generate dead-ends with config
            var corridors = deadEndSystem.Generate(d, level, rng, config);

            // Get statistics
            var stats = deadEndSystem.GetStatistics();

            // Log results
            Debug.Log($"[GridMazeGenerator] {stats}");
            
            // Warn if no dead-ends generated
            if (stats.TotalCount == 0)
            {
                Debug.LogWarning($"[GridMazeGenerator] No dead-ends generated at Level {level}! Check spawn points and density.");
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Step 7 — Torch placement
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
        //  Step 8 — Chest + enemy placement
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
    //
    //  UPDATED 2026-03-09:
    //  - DiagonalWalls removed (cardinal-only passages)
    //  - Dead-end corridors auto-generated
    //
    //  UPDATED 2026-03-09 (Dead-End Scaling):
    //  - DeadEndDensity added (base chance for dead-end corridors)
    //  - Scales with level difficulty (more dead-ends at higher levels)
    // ─────────────────────────────────────────────────────────────
    [Serializable]
    public sealed class MazeConfig
    {
        public const int ExitRoomSize = 1;
        public int   BaseSize        = 12;
        public int   MinSize         = 12;
        public int   MaxSize         = 51;
        public int   SpawnRoomSize   = 5;
        public float TorchChance     = 0.30f;
        public float ChestDensity    = 0.03f;
        public float EnemyDensity    = 0.05f;
        public float DeadEndDensity  = 0.15f;  // Base chance for dead-end corridor spawn (15%)
        // DiagonalWalls removed 2026-03-09 - cardinal-only passages

        // A* pathfinding
        public int BaseWallPenalty = 100;  // penalty for crossing a wall
    }
}
