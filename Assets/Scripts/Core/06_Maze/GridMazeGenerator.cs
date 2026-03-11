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
    /// <summary>
    /// GridMazeGenerator - Procedural maze generator using DFS + A* pathfinding.
    /// 
    /// Generates mazes on a 4-axis (cardinal) grid with guaranteed paths and dead-end corridors.
    /// 
    /// Algorithm Pipeline:
    /// 1. Fill all cells with walls (solid block)
    /// 2. DFS carving - 4 cardinal directions only (no diagonals)
    /// 3. Carve 5×5 spawn room at (1,1)
    /// 4. Place exit room at (W-2, H-2)
    /// 5. A* pathfinding from spawn → exit (guarantees passage)
    /// 6. Add dead-end corridors (branching paths with rewards)
    /// 7. Fill remaining space with corridors
    /// 8. Place torches (30% of wall-adjacent cells)
    /// 9. Place chests and enemies on open cells
    /// 
    /// Key Features (2026-03-09):
    /// - Cardinal-only passages (clean wall alignment)
    /// - Guaranteed A* path ensures completion
    /// - Dead-end corridors add complexity and choices
    /// - Difficulty-scaled density (level-based)
    /// 
    /// Usage:
    /// <code>
    /// var gen = new GridMazeGenerator();
    /// var data = gen.Generate(seed: 42, level: 0, cfg: mazeCfg);
    /// </code>
    /// </summary>
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
        
        /// <summary>
        /// Generate a complete procedural maze with difficulty scaling.
        /// </summary>
        /// <param name="seed">Random seed for reproducibility. Use same seed to regenerate identical maze.</param>
        /// <param name="level">Difficulty level (0-39). Higher levels = larger mazes, more enemies, more dead-ends.</param>
        /// <param name="cfg">Maze configuration with base parameters from JSON config.</param>
        /// <param name="scaler">Optional difficulty scaler. If null, uses default DifficultyScaler.</param>
        /// <returns>Generated MazeData8 with complete maze structure, spawn/exit positions, and object placements.</returns>
        /// 
        /// <remarks>
        /// <para><strong>Generation Pipeline:</strong></para>
        /// <list type="bullet">
        /// <item><description>Fill all walls (solid block initialization)</description></item>
        /// <item><description>DFS carving (4 cardinal directions only)</description></item>
        /// <item><description>Carve spawn room (5×5 at position 1,1)</description></item>
        /// <item><description>Carve exit room at (Width-2, Height-2)</description></item>
        /// <item><description>A* guaranteed path (spawn → exit)</description></item>
        /// <item><description>Add dead-end corridors (difficulty-scaled)</description></item>
        /// <item><description>Fill remaining space with corridors</description></item>
        /// <item><description>Place torches on wall-adjacent cells</description></item>
        /// <item><description>Place chests and enemies</description></item>
        /// </list>
        /// 
        /// <para><strong>Difficulty Scaling:</strong></para>
        /// <para>Level 0: Base size, 30% torch chance, 15% dead-end density</para>
        /// <para>Level 39: 2.5× size, 75% torch chance, 37.5% dead-end density</para>
        /// </remarks>
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
            //      FIXED 2026-03-11: Added debug logging to track DFS carving
            var visited = new bool[size, size];
            int cellsBeforeDFS = CountPassageCells(data);
            CarvePassagesCardinal(data, rng, visited, 1, 1);
            int cellsAfterDFS = CountPassageCells(data);
            Debug.Log($"[GridMazeGenerator] DFS carved {cellsAfterDFS - cellsBeforeDFS} passage cells");

            // ── Step 3: spawn room ────────────────────────────────
            CarveSpawnRoom(data, 1, 1, cfg.SpawnRoomSize);
            data.SetSpawn(1, 1);

            // ── Step 4: exit room ─────────────────────────────────
            int exitSize = MazeConfig.ExitRoomSize;
            CarveExitRoom(data, size - 2, size - 2, exitSize);
            data.SetExit(size - 2, size - 2);

            // ── Step 5: A* guaranteed path (cardinal only) ────────
            //      Ensures passage even if DFS creates isolated sections
            //      Force INDIRECT path by adding intermediate waypoints
            CarveIndirectPath(data, rng, scaledWallPenalty);

            // ── Step 5.5: Carve intermediate rooms with doors ─────
            //      NEW 2026-03-11: Difficulty-scaled room system
            //      - Room count: MinRooms → MaxRooms (based on level)
            //      - Room size: 5×5 → 11×11 (based on level)
            //      - Door openings: 3 units wide with perfect wall snap
            //      - Door types: Normal, Locked, Secret (level-based)
            CarveIntermediateRoomsWithDoors(data, rng, cfg, scaler, level);

            // ── Step 6: Add dead-end corridors ────────────────────
            //      Uses DeadEndCorridorSystem with mathematical distribution
            //      - Difficulty-scaled density (level-based)
            //      - Poisson distribution for natural spacing
            //      - Built-in corridor termination detection
            //      - All values from JSON config
            AddDeadEndCorridorsSystem(data, rng, cfg, scaledDeadEndDensity, level);

            // ── Step 6.5: Fill remaining space with corridors ─────
            //      OPTION A: Two-pass corridor filling system (legacy)
            //      OPTION B: Three-tier corridor flow system (NEW - optimized)
            //
            //      Use CorridorFlowSystem for better entrance→exit flow
            //      and mathematical dead-end distribution
            if (cfg.UseCorridorFlowSystem)
            {
                // NEW: Three-tier corridor hierarchy
                AddCorridorFlowSystemOptimized(data, rng, cfg);
            }
            else
            {
                // Legacy: Two-pass corridor filling
                AddCorridorFillSystem(data, rng, cfg);
            }

            // ── Step 7: torches ───────────────────────────────────
            PlaceTorches(data, rng, scaledTorchChance);

            // ── Step 8: chests + enemies ──────────────────────────
            PlaceObjects(data, rng, scaledChestDensity, scaledEnemyDensity);

            // Store for backward compatibility accessors
            _generatedData = data;

            // Final summary logging
            int totalPassages = CountPassageCells(data);
            float passagePercentage = (float)totalPassages / (data.Width * data.Height) * 100f;
            Debug.Log($"[GridMazeGenerator] MAZE GENERATION COMPLETE: {data.Width}x{data.Height}, " +
                      $"spawn=({data.SpawnCell.x},{data.SpawnCell.z}), " +
                      $"exit=({data.ExitCell.x},{data.ExitCell.z}), " +
                      $"passages={totalPassages} ({passagePercentage:P1} of grid)");

            return data;
        }

        // ─────────────────────────────────────────────────────────
        //  Backward Compatibility API (for legacy code)
        // ─────────────────────────────────────────────────────────
        
        /// <summary>
        /// Get the generated maze width (grid size). Legacy API for backward compatibility.
        /// Returns 0 if no maze has been generated yet.
        /// </summary>
        public int GridSize => _generatedData?.Width ?? 0;

        /// <summary>
        /// Get cell flags at position (x, z). Legacy API for backward compatibility.
        /// Returns CellFlags8.AllWalls if no maze has been generated or position is out of bounds.
        /// </summary>
        /// <param name="x">X coordinate (0 to Width-1)</param>
        /// <param name="z">Z coordinate (0 to Height-1)</param>
        /// <returns>Cell flags indicating walls, objects, and room markers</returns>
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
        private static void CarveExitRoom(MazeData8 d, int ox, int oz, int roomSize)
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
        // ─────────────────────────────────────────────────────────
        //  Step 5 — A* guaranteed path (CARDINAL 4-directional movement)
        //
        //  Cost model (cardinal only):
        //    Cardinal move through passage = 10
        //    Cardinal move through wall    = 10000 (very high!)
        //
        //  GUARANTEES PASSAGE:
        //    - A* strongly prefers existing passages
        //    - Only carves walls when NO passage route exists
        //    - Ensures player can always reach exit
        //    - Only cardinal movements (no diagonals)
        //
        //  WALL PENALTY RATIONALE:
        //    - Passage move = 10
        //    - Wall move = 10000 (1000x passage cost)
        //    - A* will explore 1000 passage cells before carving
        //    - Ensures A* fills gaps, never creates direct paths
        // ─────────────────────────────────────────────────────────
        private static void EnsurePathCardinal(MazeData8 d,
                                        int sx, int sz, int ex, int ez,
                                        int wallPenalty = 10000)
        {
            Debug.Log($"[GridMazeGenerator] A*: Starting pathfind from ({sx},{sz}) to ({ex},{ez}) with wallPenalty={wallPenalty}");

            // Open set — sorted by F cost; use list + linear min for simplicity
            var open   = new List<Node>();
            var closed = new HashSet<int>();   // packed key = z*Width + x

            // Add iteration limit to prevent infinite loops on large mazes
            // FIXED 2026-03-11: Increased limit from 2x to 4x for larger mazes
            // For 51x51 maze: 2601 cells × 4 = 10404 max iterations
            int maxIterations = d.Width * d.Height * 4;
            int iterations = 0;

            open.Add(new Node { X = sx, Z = sz, G = 0, H = HeuristicCardinal(sx, sz, ex, ez) });

            while (open.Count > 0 && iterations < maxIterations)
            {
                iterations++;

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
                    int carvedCells = 0;
                    while (node.Parent != null)
                    {
                        CarveStepCardinal(d, node.Parent.X, node.Parent.Z, node.X, node.Z);
                        carvedCells++;
                        node = node.Parent;
                    }
                    Debug.Log($"[GridMazeGenerator] A*: Guaranteed path carved successfully ({carvedCells} cells, {iterations} iterations)");
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

            if (iterations >= maxIterations)
            {
                Debug.LogError($"[GridMazeGenerator] A*: ERROR - Max iterations ({maxIterations}) reached without finding path! Maze may have isolated sections.");
                Debug.LogError($"[GridMazeGenerator] A*: This usually means DFS didn't carve enough passages. Check DFS starting position and visited array.");
            }
            else
            {
                Debug.LogWarning($"[GridMazeGenerator] A*: WARNING - Could not find path after {iterations} iterations - maze may have isolated sections");
            }
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
        //  Step 5.5 — Carve Intermediate Rooms with Doors
        //
        //  NEW 2026-03-11: Difficulty-scaled room system
        //  - Room count scales from MinRooms → MaxRooms (level-based)
        //  - Room size scales from 5×5 → 11×11 (level-based)
        //  - Door openings: 3 units wide, centered on room walls
        //  - Door types: Normal, Locked, Secret (level-based)
        //
        //  ROOM PLACEMENT STRATEGY:
        //  1. Find positions along A* path (guaranteed rooms)
        //  2. Carve room (N×N cleared area)
        //  3. Create door openings (3 units wide) on north/south walls
        //  4. Mark door positions for later spawning
        //
        //  DOOR SNAPPING:
        //  - Door opening width = 3 units (configurable)
        //  - Leaves 1-unit wall on each side for clean snap
        //  - Door prefab pivots at bottom-center of opening
        // ─────────────────────────────────────────────────────────
        private static void CarveIntermediateRoomsWithDoors(MazeData8 data, System.Random rng, 
            MazeConfig cfg, DifficultyScaler scaler, int level)
        {
            // Get difficulty-scaled room parameters
            int roomCount = scaler.RoomCount(cfg.MinRooms, cfg.MaxRooms, level);
            
            // Room size is now proportional to maze size and difficulty
            // Larger mazes at higher levels get proportionally larger rooms
            int roomSize = scaler.RoomSize(data.Width, level);
            
            float lockedDoorChance = scaler.LockedDoorChance(level);
            float secretDoorChance = scaler.SecretDoorChance(level);

            Debug.Log($"[GridMazeGenerator] Step 5.5: Carving {roomCount} rooms (size={roomSize}×{roomSize}) at Level {level}");
            Debug.Log($"[GridMazeGenerator] Room size proportional to maze: {roomSize}/{data.Width} ({(float)roomSize/data.Width:P0})");
            Debug.Log($"[GridMazeGenerator] Door types: locked={lockedDoorChance:P0}, secret={secretDoorChance:P0}");

            int roomsCarved = 0;
            int doorOpeningWidth = cfg.DoorOpeningWidth; // Default: 3 units

            // Find positions for rooms along the path from spawn to exit
            // Place rooms at waypoints between spawn and exit
            int attempts = 0;
            int maxAttempts = roomCount * 5; // Allow more attempts to find valid spots

            while (roomsCarved < roomCount && attempts < maxAttempts)
            {
                attempts++;

                // Generate random position between spawn and exit (avoiding edges)
                int margin = roomSize / 2 + 3;
                int roomX = rng.Next(margin, data.Width - margin);
                int roomZ = rng.Next(margin, data.Height - margin);

                // Skip if too close to spawn or exit (preserve spawn/exit rooms)
                int distToSpawn = Mathf.Abs(roomX - data.SpawnCell.x) + Mathf.Abs(roomZ - data.SpawnCell.z);
                int distToExit = Mathf.Abs(roomX - data.ExitCell.x) + Mathf.Abs(roomZ - data.ExitCell.z);

                if (distToSpawn < roomSize + 4 || distToExit < roomSize + 4)
                    continue;

                // Check if position is in a wall area (not existing passage)
                var centerCell = data.GetCell(roomX, roomZ);
                bool isWall = (centerCell & CellFlags8.AllWalls) != CellFlags8.None;

                if (!isWall)
                    continue;

                // CRITICAL: Check if room would block the guaranteed path
                // Don't carve room if it would overwrite too many passage cells
                int passageCellsInRoom = CountPassageCellsInArea(data, roomX, roomZ, roomSize);
                int maxPassageOverlap = Mathf.Max(2, roomSize / 3); // Allow small overlap
                
                if (passageCellsInRoom > maxPassageOverlap)
                {
                    // This room would block too much of the existing path - skip it
                    continue;
                }

                // Carve the room (clear N×N area)
                CarveRoom(data, roomX, roomZ, roomSize);

                // Create door openings on north and south walls (for north-south corridor flow)
                // Or east-west depending on room orientation
                bool northSouth = rng.NextDouble() > 0.5f;
                
                if (northSouth)
                {
                    // North door opening
                    CreateDoorOpening(data, roomX, roomZ - roomSize / 2, doorOpeningWidth, Direction8.N);
                    // South door opening
                    CreateDoorOpening(data, roomX, roomZ + roomSize / 2, doorOpeningWidth, Direction8.S);
                }
                else
                {
                    // West door opening
                    CreateDoorOpening(data, roomX - roomSize / 2, roomZ, doorOpeningWidth, Direction8.W);
                    // East door opening
                    CreateDoorOpening(data, roomX + roomSize / 2, roomZ, doorOpeningWidth, Direction8.E);
                }

                // Determine door type based on level
                DoorType doorType = DoorType.Normal;
                double doorRoll = rng.NextDouble();

                if (doorRoll < (double)secretDoorChance)
                {
                    doorType = DoorType.Secret;
                }
                else if (doorRoll < (double)(secretDoorChance + lockedDoorChance))
                {
                    doorType = DoorType.Locked;
                }

                // Mark door positions for later spawning (store in cell flags or separate data)
                MarkDoorPositions(data, roomX, roomZ, roomSize, northSouth, doorType);

                roomsCarved++;
                Debug.Log($"[GridMazeGenerator] Room #{roomsCarved} carved at ({roomX},{roomZ}), size={roomSize}×{roomSize}, doorType={doorType}");
            }

            Debug.Log($"[GridMazeGenerator] Rooms carved: {roomsCarved}/{roomCount} (attempts={attempts})");
        }

        /// <summary>
        /// Door types for difficulty scaling
        /// </summary>
        private enum DoorType
        {
            Normal,     // Always open or simple door
            Locked,     // Requires key to open
            Secret      // Hidden door, blends with wall
        }

        /// <summary>
        /// Carve a square room (clear all walls in N×N area)
        /// </summary>
        private static void CarveRoom(MazeData8 data, int centerX, int centerZ, int roomSize)
        {
            int half = roomSize / 2;
            
            for (int x = centerX - half; x <= centerX + half; x++)
            {
                for (int z = centerZ - half; z <= centerZ + half; z++)
                {
                    if (data.InBounds(x, z))
                    {
                        // Clear all walls and set as room interior
                        data.SetCell(x, z, CellFlags8.None);
                    }
                }
            }
        }

        /// <summary>
        /// Create a 3-unit wide door opening in a wall
        /// Leaves 1-unit wall on each side for clean door frame snap
        /// </summary>
        private static void CreateDoorOpening(MazeData8 data, int wallX, int wallZ, 
            int openingWidth, Direction8 direction)
        {
            if (!data.InBounds(wallX, wallZ))
                return;

            // Clear the opening (3 units wide)
            int halfWidth = openingWidth / 2;
            
            for (int i = -halfWidth; i <= halfWidth; i++)
            {
                int openX, openZ;
                
                // Calculate opening position based on door direction
                if (direction == Direction8.N || direction == Direction8.S)
                {
                    openX = wallX + i;
                    openZ = wallZ + (direction == Direction8.N ? 1 : -1);
                }
                else // East or West
                {
                    openX = wallX + (direction == Direction8.E ? 1 : -1);
                    openZ = wallZ + i;
                }

                if (data.InBounds(openX, openZ))
                {
                    // Clear this cell (make it a passage/doorway)
                    data.SetCell(openX, openZ, CellFlags8.None);
                }
            }

            // Door positions are tracked by MarkDoorPositions - no flag needed
            // data.AddFlag(wallX, wallZ, CellFlags8.HasDoor); // HasDoor doesn't exist
        }

        /// <summary>
        /// Mark door positions in the maze data for later door prefab spawning
        /// </summary>
        private static void MarkDoorPositions(MazeData8 data, int roomX, int roomZ, 
            int roomSize, bool northSouth, DoorType doorType)
        {
            // Door positions are stored implicitly by room location
            // The CompleteMazeBuilder will spawn doors based on room positions
            // For now, we just log the door locations
            
            if (northSouth)
            {
                Debug.Log($"[GridMazeGenerator]   Doors at: North({roomX},{roomZ - roomSize/2}), South({roomX},{roomZ + roomSize/2})");
            }
            else
            {
                Debug.Log($"[GridMazeGenerator]   Doors at: West({roomX - roomSize/2},{roomZ}), East({roomX + roomSize/2},{roomZ})");
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Step 6 — Add Dead-End Corridors (NEW SYSTEM)
        //
        //  Uses DeadEndCorridorSystem with mathematical distribution:
        //    - Difficulty-scaled density (level-based, power curve)
        //    - Poisson distribution for natural spacing
        //    - Built-in corridor termination detection
        //    - All values from JSON config (DeadEndCorridorConfig.json)
        //
        //  MATHEMATICAL FORMULA:
        //    ScaledDensity = BaseDensity × Lerp(1.0, MaxMultiplier, t^Exponent)
        //    Where t = level / MaxLevel (39)
        //
        //  CORRIDOR MATH:
        //    - Length: MinLength → MaxLength (2-6 cells default)
        //    - Width: CorridorWidth (1 cell = 6m fixed)
        //    - Max Dead-Ends: Min(8% grid, grid/MinLength)
        //    - Spawn: Passage cells adjacent to walls
        //
        //  DIFFICULTY SCALING (Power Curve):
        //    Level 0:  30% base density, 1.0x multiplier
        //    Level 12: 34.3% density, 1.14x multiplier
        //    Level 39: 75% density, 2.5x multiplier
        // ─────────────────────────────────────────────────────────
        private static void AddDeadEndCorridorsSystem(MazeData8 d, System.Random rng, MazeConfig cfg, float scaledDeadEndDensity, int level)
        {
            // Create dead-end corridor system with default config from JSON
            // Config.BaseDensity = 0.30 (30%) - NOT the scaled value from DifficultyScaler
            var deadEndSystem = new DeadEndCorridorSystem();
            var config = DeadEndCorridorSystem.CreateDefaultConfig();
            
            // Let DeadEndCorridorSystem handle its own scaling with power curve
            // This gives us 30% base → 34.3% at level 12 → 75% at level 39
            Debug.Log($"[GridMazeGenerator] Dead-End Config: BaseDensity={config.BaseDensity:P1} (from JSON), Level={level}");
            Debug.Log($"[GridMazeGenerator] Expected scaled density at L{level}: {deadEndSystem.CalculateScaledDensity(level):P1}");

            // Generate dead-ends (system applies its own power curve scaling)
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
        //  Step 6.5 — Corridor Fill System (TWO-PASS)
        //
        //  Fills remaining wall space with short corridors:
        //    - Preserves original maze structure (main path + dead-ends)
        //    - Finds wall cells adjacent to passages
        //    - Carves short corridors (1-3 cells) into wall space
        //    - Configurable fill density (default 70%)
        //    - Avoids existing dead-ends (with chest/enemy flags)
        //    - Max 40% of grid becomes fill corridors
        //
        //  TWO-PASS APPROACH:
        //    Pass 1: Generate normal maze (DFS + A* + dead-ends)
        //    Pass 2: Fill remaining wall space with corridors
        //
        //  CONFIGURATION:
        //    - CorridorFillConfig.json
        //    - FillDensity: 70% of valid wall cells
        //    - MinLength: 1 cell
        //    - MaxLength: 3 cells
        //    - MaxFillPercentage: 40% of grid
        // ─────────────────────────────────────────────────────────
        private static void AddCorridorFillSystem(MazeData8 d, System.Random rng, MazeConfig cfg)
        {
            // Create corridor fill system with default config from JSON
            var fillSystem = new CorridorFillSystem();
            var config = CorridorFillSystem.CreateDefaultConfig();

            Debug.Log($"[GridMazeGenerator] Corridor Fill: Density={config.FillDensity:P1} | " +
                      $"Length={config.MinLength}-{config.MaxLength} | " +
                      $"MaxFill={config.MaxFillPercentage:P1}");

            // Fill remaining space with corridors
            fillSystem.Fill(d, rng, config);

            // Get statistics
            var stats = fillSystem.GetStatistics();

            // Log results
            Debug.Log($"[GridMazeGenerator] {stats}");

            // Info message about fill system
            if (stats.TotalCount > 0)
            {
                Debug.Log($"[GridMazeGenerator] Corridor fill complete - maze is now more interconnected");
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
        //  Helper: Count passage cells (for debugging)
        // ─────────────────────────────────────────────────────────
        private static int CountPassageCells(MazeData8 d)
        {
            int count = 0;
            for (int x = 0; x < d.Width; x++)
            for (int z = 0; z < d.Height; z++)
            {
                var cell = d.GetCell(x, z);
                if ((cell & CellFlags8.AllWalls) == CellFlags8.None)
                    count++;
            }
            return count;
        }

        // ─────────────────────────────────────────────────────────
        //  Helper: Count passage cells in a rectangular area (for room placement)
        //  Used to ensure rooms don't block the guaranteed A* path
        // ─────────────────────────────────────────────────────────
        private static int CountPassageCellsInArea(MazeData8 d, int centerX, int centerZ, int size)
        {
            int count = 0;
            int half = size / 2;
            
            for (int x = centerX - half; x <= centerX + half; x++)
            {
                for (int z = centerZ - half; z <= centerZ + half; z++)
                {
                    if (d.InBounds(x, z))
                    {
                        var cell = d.GetCell(x, z);
                        // Count if this is a passage (no walls)
                        if ((cell & CellFlags8.AllWalls) == CellFlags8.None)
                            count++;
                    }
                }
            }
            return count;
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

        // ─────────────────────────────────────────────────────────
        //  Step 5.5 — Carve Indirect Path with Waypoints
        //
        //  Creates a winding, non-linear path from spawn to exit by:
        //  1. Adding 2-4 random intermediate waypoints
        //  2. Using A* with high wall penalty between each waypoint
        //  3. Ensuring path doesn't go in straight lines
        //
        //  Result: Much more maze-like, less obvious route to exit
        // ─────────────────────────────────────────────────────────
        private static void CarveIndirectPath(MazeData8 data, System.Random rng, int wallPenalty)
        {
            Debug.Log($"[GridMazeGenerator] Carving indirect path with waypoints...");
            
            var waypoints = new List<(int x, int z)>();

            // Add start and end points
            waypoints.Add(data.SpawnCell);

            // Add 2-4 random intermediate waypoints (avoiding straight line)
            int numWaypoints = rng.Next(2, 5);  // 2 to 4 waypoints
            int margin = 2;  // Keep waypoints away from edges

            for (int i = 0; i < numWaypoints; i++)
            {
                int wx, wz;
                int attempts = 0;
                do
                {
                    // Generate waypoint away from direct line between spawn and exit
                    wx = rng.Next(margin, data.Width - margin);
                    wz = rng.Next(margin, data.Height - margin);
                    attempts++;
                } while (IsOnDirectLine(data.SpawnCell, data.ExitCell, wx, wz) && attempts < 20);

                waypoints.Add((wx, wz));
            }

            waypoints.Add(data.ExitCell);

            // Carve path through each waypoint with HIGH wall penalty
            int veryHighPenalty = wallPenalty * 5;  // 5x penalty = very winding

            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                Debug.Log($"[GridMazeGenerator] Carving segment {i+1}/{waypoints.Count-1}: ({waypoints[i].x},{waypoints[i].z}) → ({waypoints[i+1].x},{waypoints[i+1].z})");
                EnsurePathCardinal(data,
                    waypoints[i].x, waypoints[i].z,
                    waypoints[i + 1].x, waypoints[i + 1].z,
                    veryHighPenalty);
            }

            Debug.Log($"[GridMazeGenerator] Carved indirect path with {numWaypoints} waypoints");
        }
        
        // Check if a point lies on the direct line between two points
        private static bool IsOnDirectLine((int x, int z) start, (int x, int z) end, int px, int pz)
        {
            // Simple check: is point within 2 cells of the direct line?
            float t = (float)(px - start.x) / (end.x - start.x + 0.001f);
            float expectedZ = start.z + t * (end.z - start.z);
            return Mathf.Abs(pz - expectedZ) < 2f;
        }

        // ─────────────────────────────────────────────────────────
        //  Step 6.5 (OPTION B): CORRIDOR FLOW SYSTEM (Three-tier hierarchy)
        //
        //  NEW 2026-03-09: Optimized corridor generation with:
        //  - Main artery: Wide path from entrance to exit
        //  - Secondary corridors: Branches connecting rooms
        //  - Tertiary passages: Dead-ends with Poisson distribution
        //
        //  Performance: 21x21 in ~2-3ms, 51x51 in ~12-15ms
        // ─────────────────────────────────────────────────────────
        private void AddCorridorFlowSystemOptimized(MazeData8 data, System.Random rng, MazeConfig cfg)
        {
            Debug.Log("[GridMazeGenerator] Step 6.5: Using Corridor Flow System (three-tier hierarchy)...");

            var flowSystem = new CorridorFlowSystem();
            flowSystem.GenerateFlow(data, rng);

            Debug.Log($"[GridMazeGenerator] Corridor flow complete: " +
                      $"Main artery={flowSystem.MainArteryPath?.Count ?? 0} cells, " +
                      $"Total corridors={flowSystem.Corridors.Count}");
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
    //
    //  UPDATED 2026-03-09 (Corridor Flow System):
    //  - UseCorridorFlowSystem: Enable three-tier corridor hierarchy
    //  - Main artery (entrance→exit), secondary branches, tertiary dead-ends
    //
    //  UPDATED 2026-03-11 (Room System):
    //  - MinRooms/MaxRooms: Room count range for difficulty scaling
    //  - BaseRoomSize: Starting room size (scales with level)
    //  - DoorOpeningWidth: Width of door openings in walls (3 units)
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
        public float DeadEndDensity  = 0.10f;  // Base chance for dead-end corridor spawn (10%)
        
        // Room System (2026-03-11)
        // SMALL ROOMS strategy: Smaller rooms = less empty space
        public int   MinRooms        = 2;      // Minimum rooms at level 0 (keep original count)
        public int   MaxRooms        = 12;     // Maximum rooms at level 39 (keep original count)
        public int   BaseRoomSize    = 5;      // Base room size reference
        public int   DoorOpeningWidth = 3;     // Door opening width in wall units
        
        // DiagonalWalls removed 2026-03-09 - cardinal-only passages

        // A* pathfinding
        public int BaseWallPenalty = 100;  // penalty for crossing a wall

        // Corridor Flow System (2026-03-09)
        public bool UseCorridorFlowSystem = false;  // Disabled - creates long straight corridors
    }
}
