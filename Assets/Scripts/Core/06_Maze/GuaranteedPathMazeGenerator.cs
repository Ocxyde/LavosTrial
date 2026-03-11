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
//
// GuaranteedPathMazeGenerator.cs
// "Maze of Minotaur" - Classic labyrinth with guaranteed path
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// ALGORITHM:
//   Phase 1: Carve guaranteed path AB (30% of maze)
//   Phase 2: Add primary branches (40% of maze)
//   Phase 3: Add dead-end corridors (20% of maze)
//   Phase 4: Add secondary connections (10% of maze)
//   Phase 5: Place walls on both sides of all corridors
//
// RESULT: Classic maze with structure, complexity, and guaranteed solution
//
// Usage:
//   var gen = new GuaranteedPathMazeGenerator();
//   var data = gen.Generate(seed, level, config);

using System;
using System.Collections.Generic;
using Code.Lavos.Core.Advanced;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Guaranteed Path Maze Generator
    /// "Maze of Minotaur" - Classic labyrinth structure
    /// Returns DungeonMazeData for compatibility with CompleteMazeBuilder
    /// </summary>
    public sealed class GuaranteedPathMazeGenerator
    {
        private DungeonMazeData _mazeData;
        private System.Random _rng;
        private List<(int x, int z)> _mainPath;
        private HashSet<(int x, int z)> _branchPointSet;
        private List<(int x, int z)> _branchPoints;
        private int _level;

        // Cardinal walls constant (excludes diagonals)
        private const uint CardinalWalls = (uint)(CellFlags8.WallN | CellFlags8.WallS |
                                                  CellFlags8.WallE | CellFlags8.WallW);

        /// <summary>
        /// Generate maze with guaranteed path from spawn to exit
        /// Returns DungeonMazeData for compatibility
        /// </summary>
        public DungeonMazeData Generate(int seed, int level, DungeonMazeConfig cfg)
        {
            if (cfg == null) throw new ArgumentNullException(nameof(cfg));

            _rng = new System.Random(seed);
            _level = level;
            _mainPath = new List<(int, int)>();
            _branchPoints = new List<(int, int)>();
            _branchPointSet = new HashSet<(int, int)>();

            // Calculate maze size (scaled with level)
            int size = CalculateMazeSize(level, cfg);

            Debug.Log($"[MinotaurMaze] Level {level} | Size {size}x{size} | Seed {seed}");

            // Create maze data (DungeonMazeData for compatibility)
            _mazeData = new DungeonMazeData(size, size, seed, level)
            {
                DifficultyFactor = cfg.Difficulty.Factor(level),
                Config = cfg,
            };

            // === PHASE 1: Fill all walls ===
            FillAllWalls();
            Debug.Log($"[MinotaurMaze] Phase 1: All walls filled");

            // === PHASE 2: Carve guaranteed path AB ===
            CarveGuaranteedPath();
            Debug.Log($"[MinotaurMaze] Phase 2: Main path carved ({_mainPath.Count} cells)");

            // === PHASE 3: Add primary branches ===
            AddPrimaryBranches();
            Debug.Log($"[MinotaurMaze] Phase 3: Primary branches added ({_branchPoints.Count} branches)");

            // === PHASE 4: Add dead-end corridors ===
            AddDeadEndCorridors(cfg);
            Debug.Log($"[MinotaurMaze] Phase 4: Dead-end corridors added");

            // === PHASE 5: Add secondary connections ===
            AddSecondaryConnections();
            Debug.Log($"[MinotaurMaze] Phase 5: Secondary connections added");

            // === PHASE 6: Place walls on both sides of corridors ===
            PlaceCorridorWalls();
            Debug.Log($"[MinotaurMaze] Phase 6: Corridor walls placed");

            // === PHASE 7: Place objects ===
            PlaceObjects(cfg);
            Debug.Log($"[MinotaurMaze] Phase 7: Objects placed");

            Debug.Log($"[MinotaurMaze] COMPLETE - Maze generated successfully");

            return _mazeData;
        }

        /// <summary>
        /// Calculate maze size based on level
        /// </summary>
        private int CalculateMazeSize(int level, DungeonMazeConfig cfg)
        {
            // Scale from minSize to maxSize over 39 levels
            float t = Mathf.Clamp01(level / 39f);
            int size = Mathf.RoundToInt(Mathf.Lerp(cfg.MinSize, cfg.MaxSize, t));

            // Ensure odd size for proper wall alignment
            if (size % 2 == 0) size++;

            return Mathf.Max(cfg.MinSize, Mathf.Min(cfg.MaxSize, size));
        }

        /// <summary>
        /// PHASE 1: Fill all cells with walls
        /// </summary>
        private void FillAllWalls()
        {
            for (int x = 0; x < _mazeData.Width; x++)
            {
                for (int z = 0; z < _mazeData.Height; z++)
                {
                    _mazeData.SetCell(x, z, (uint)CardinalWalls);
                }
            }
        }

        /// <summary>
        /// PHASE 2: Carve guaranteed path from spawn (1,1) to exit (W-2, H-2)
        /// Uses biased random walk with gentle curves
        /// </summary>
        private void CarveGuaranteedPath()
        {
            int startX = 1;
            int startZ = 1;
            int endX = _mazeData.Width - 2;
            int endZ = _mazeData.Height - 2;

            // Set spawn and exit
            _mazeData.SetSpawn(startX, startZ);
            _mazeData.SetExit(endX, endZ);

            // Carve spawn room (5x5)
            CarveRoom(startX, startZ, 5);

            // Carve exit room (5x5)
            CarveRoom(endX, endZ, 5);

            // Carve main path with gentle curves
            int currX = startX + 2;
            int currZ = startZ + 2;

            _mainPath.Add((currX, currZ));

            // Bias towards exit - cardinal movement only (no diagonals)
            while (currX < endX - 2 || currZ < endZ - 2)
            {
                // Random choice with bias towards exit
                float roll = (float)_rng.NextDouble();

                if (currX < endX && currZ < endZ)
                {
                    // Move cardinal towards exit (no diagonal jumps)
                    if (roll < 0.5f)
                        currX += 2;  // East
                    else
                        currZ += 2;  // South
                }
                else if (currX < endX)
                {
                    currX += 2;
                }
                else if (currZ < endZ)
                {
                    currZ += 2;
                }

                // Clamp to bounds
                currX = Mathf.Clamp(currX, 1, endX - 1);
                currZ = Mathf.Clamp(currZ, 1, endZ - 1);

                // Carve this cell
                CarveCell(currX, currZ);
                _mainPath.Add((currX, currZ));

                // Mark as branch point occasionally (for Phase 3) - prevent duplicates
                if (_rng.NextDouble() < 0.15f && _mainPath.Count > 5)
                {
                    var point = (currX, currZ);
                    if (_branchPointSet.Add(point))  // Returns false if already present
                    {
                        _branchPoints.Add(point);
                    }
                }
            }

            // Ensure path is walkable (remove walls between path cells)
            ConnectPathCells();
        }

        /// <summary>
        /// PHASE 3: Add primary branches from main path
        /// </summary>
        private void AddPrimaryBranches()
        {
            foreach (var (startX, startZ) in _branchPoints)
            {
                // Get available directions (not back along main path)
                var dirs = GetAvailableDirections(startX, startZ);

                if (dirs.Count == 0) continue;

                // Pick random direction
                var dir = dirs[_rng.Next(dirs.Count)];
                var (dx, dz) = Direction8Helper.ToOffset(dir);

                // Carve branch (3-8 cells long)
                int length = _rng.Next(3, 9);
                int currX = startX + dx;
                int currZ = startZ + dz;

                // Validate starting position before entering loop
                if (!_mazeData.InBounds(currX, currZ)) continue;

                // Track carved cells to prevent duplicate carving within this branch
                var carvedThisBranch = new HashSet<(int, int)>();

                for (int i = 0; i < length; i++)
                {
                    if (!_mazeData.InBounds(currX, currZ)) break;

                    // Skip if already carved in this branch (overlapping direction)
                    if (carvedThisBranch.Contains((currX, currZ)))
                    {
                        currX += dx;
                        currZ += dz;
                        continue;
                    }

                    var cell = _mazeData.GetCell(currX, currZ);
                    bool isWall = (cell & (uint)CardinalWalls) == (uint)CardinalWalls;

                    if (!isWall) break;  // Hit existing passage

                    CarveCell(currX, currZ);
                    carvedThisBranch.Add((currX, currZ));
                    currX += dx;
                    currZ += dz;
                }
            }
        }

        /// <summary>
        /// PHASE 4: Add dead-end corridors
        /// </summary>
        private void AddDeadEndCorridors(DungeonMazeConfig cfg)
        {
            // Calculate dead-end count based on level
            int deadEndCount = CalculateDeadEndCount(_level);

            for (int i = 0; i < deadEndCount; i++)
            {
                // Pick random point on main path
                if (_mainPath.Count == 0) continue;

                var (startX, startZ) = _mainPath[_rng.Next(_mainPath.Count)];

                // Get available directions
                var dirs = GetAvailableDirections(startX, startZ);

                if (dirs.Count == 0) continue;

                var dir = dirs[_rng.Next(dirs.Count)];
                var (dx, dz) = Direction8Helper.ToOffset(dir);

                // Carve dead-end (2-5 cells long)
                int length = _rng.Next(2, 6);
                int currX = startX + dx;
                int currZ = startZ + dz;

                int actualLength = 0;
                for (int j = 0; j < length; j++)
                {
                    if (!_mazeData.InBounds(currX, currZ)) break;

                    var cell = _mazeData.GetCell(currX, currZ);
                    bool isWall = (cell & (uint)CardinalWalls) == (uint)CardinalWalls;

                    if (!isWall) break;

                    CarveCell(currX, currZ);
                    currX += dx;
                    currZ += dz;
                    actualLength++;
                }

                // Place treasure/enemy at end if corridor is long enough
                if (actualLength >= 2)
                {
                    PlaceDeadEndObject(currX - dx, currZ - dz);
                }
            }
        }

        /// <summary>
        /// PHASE 5: Add secondary connections (loops)
        /// </summary>
        private void AddSecondaryConnections()
        {
            // Connect some dead-ends back to main path (create loops)
            int loopCount = _branchPoints.Count / 3;

            for (int i = 0; i < loopCount; i++)
            {
                // Pick two random points on main path
                if (_mainPath.Count < 10) continue;

                int idx1 = _rng.Next(_mainPath.Count);
                int idx2 = _rng.Next(_mainPath.Count);

                if (Mathf.Abs(idx1 - idx2) < 3) continue;  // Too close

                var (x1, z1) = _mainPath[idx1];
                var (x2, z2) = _mainPath[idx2];

                // Carve simple L-shaped connection
                CarveLConnection(x1, z1, x2, z2);
            }
        }

        /// <summary>
        /// PHASE 6: Place walls on both sides of all corridors
        /// </summary>
        private void PlaceCorridorWalls()
        {
            // For each carved cell, ensure adjacent cells have wall flags
            for (int x = 0; x < _mazeData.Width; x++)
            {
                for (int z = 0; z < _mazeData.Height; z++)
                {
                    var cell = _mazeData.GetCell(x, z);
                    bool isPassage = (cell & (uint)CardinalWalls) == 0;

                    if (!isPassage) continue;

                    // This is a passage cell - add wall flags to neighbors
                    AddWallFlagToNeighbor(x, z + 1, (uint)CellFlags8.WallN);  // North neighbor
                    AddWallFlagToNeighbor(x, z - 1, (uint)CellFlags8.WallS);  // South neighbor
                    AddWallFlagToNeighbor(x + 1, z, (uint)CellFlags8.WallW);  // East neighbor
                    AddWallFlagToNeighbor(x - 1, z, (uint)CellFlags8.WallE);  // West neighbor
                }
            }
        }

        /// <summary>
        /// PHASE 7: Place objects (torches, chests, enemies)
        /// </summary>
        private void PlaceObjects(DungeonMazeConfig cfg)
        {
            // Place torches along main path
            for (int i = 0; i < _mainPath.Count; i += 2)
            {
                var (x, z) = _mainPath[i];
                if (_rng.NextDouble() < cfg.TorchChance)
                {
                    _mazeData.AddFlag(x, z, (uint)CellFlags8.HasTorch);
                }
            }
        }

        #region Helper Methods

        /// <summary>
        /// Carve a single cell (remove all walls)
        /// Uses DungeonMazeData uint cell format
        /// </summary>
        private void CarveCell(int x, int z)
        {
            uint cell = _mazeData.GetCell(x, z);
            cell &= ~(uint)CellFlags8.WallN;
            cell &= ~(uint)CellFlags8.WallS;
            cell &= ~(uint)CellFlags8.WallE;
            cell &= ~(uint)CellFlags8.WallW;
            _mazeData.SetCell(x, z, cell);
        }

        /// <summary>
        /// Carve a room (square area)
        /// </summary>
        private void CarveRoom(int centerX, int centerZ, int size)
        {
            int half = size / 2;
            for (int x = centerX - half; x <= centerX + half; x++)
            {
                for (int z = centerZ - half; z <= centerZ + half; z++)
                {
                    if (_mazeData.InBounds(x, z))
                    {
                        CarveCell(x, z);
                    }
                }
            }
        }

        /// <summary>
        /// Connect path cells (ensure walkability)
        /// </summary>
        private void ConnectPathCells()
        {
            for (int i = 0; i < _mainPath.Count - 1; i++)
            {
                var (x1, z1) = _mainPath[i];
                var (x2, z2) = _mainPath[i + 1];

                // Clear walls between cells
                ClearWallBetween(x1, z1, x2, z2);
            }
        }

        /// <summary>
        /// Clear wall between two adjacent cells
        /// </summary>
        private void ClearWallBetween(int x1, int z1, int x2, int z2)
        {
            var cell1 = _mazeData.GetCell(x1, z1);
            var cell2 = _mazeData.GetCell(x2, z2);

            if (x2 > x1)  // East
            {
                cell1 &= ~(uint)CellFlags8.WallE;
                cell2 &= ~(uint)CellFlags8.WallW;
            }
            else if (x2 < x1)  // West
            {
                cell1 &= ~(uint)CellFlags8.WallW;
                cell2 &= ~(uint)CellFlags8.WallE;
            }
            else if (z2 > z1)  // South
            {
                cell1 &= ~(uint)CellFlags8.WallS;
                cell2 &= ~(uint)CellFlags8.WallN;
            }
            else if (z2 < z1)  // North
            {
                cell1 &= ~(uint)CellFlags8.WallN;
                cell2 &= ~(uint)CellFlags8.WallS;
            }

            _mazeData.SetCell(x1, z1, cell1);
            _mazeData.SetCell(x2, z2, cell2);
        }

        /// <summary>
        /// Get available directions for branching
        /// </summary>
        private List<Direction8> GetAvailableDirections(int x, int z)
        {
            var dirs = new List<Direction8>();
            var cardinalDirs = new Direction8[] { Direction8.N, Direction8.S, Direction8.E, Direction8.W };

            foreach (var dir in cardinalDirs)
            {
                var (dx, dz) = Direction8Helper.ToOffset(dir);
                int nx = x + dx * 2;
                int nz = z + dz * 2;

                if (!_mazeData.InBounds(nx, nz)) continue;

                var cell = _mazeData.GetCell(nx, nz);
                bool isWall = (cell & CardinalWalls) == CardinalWalls;

                if (isWall)
                {
                    dirs.Add(dir);
                }
            }

            return dirs;
        }

        /// <summary>
        /// Calculate dead-end count based on level
        /// </summary>
        private int CalculateDeadEndCount(int level)
        {
            // Scale from 5 to 20 dead-ends over 39 levels
            float t = Mathf.Clamp01(level / 39f);
            return Mathf.RoundToInt(Mathf.Lerp(5, 20, t));
        }

        /// <summary>
        /// Place treasure or enemy at dead-end
        /// </summary>
        private void PlaceDeadEndObject(int x, int z)
        {
            float roll = (float)_rng.NextDouble();

            if (roll < 0.5f)
            {
                _mazeData.AddFlag(x, z, (uint)CellFlags8.HasChest);
            }
            else if (roll < 0.8f)
            {
                _mazeData.AddFlag(x, z, (uint)CellFlags8.HasEnemy);
            }
            // else: empty dead-end
        }

        /// <summary>
        /// Add wall flag to neighbor cell
        /// </summary>
        private void AddWallFlagToNeighbor(int x, int z, uint wallFlag)
        {
            if (!_mazeData.InBounds(x, z)) return;

            var cell = _mazeData.GetCell(x, z);
            bool isSpawn = (cell & (uint)CellFlags8.SpawnRoom) != 0;
            bool isExit = (cell & (uint)CellFlags8.IsExit) != 0;

            if (!isSpawn && !isExit)
            {
                cell |= wallFlag;
                _mazeData.SetCell(x, z, cell);
            }
        }

        /// <summary>
        /// Carve L-shaped connection between two points
        /// Only carves cells that are still walls (doesn't overwrite existing passages)
        /// </summary>
        private void CarveLConnection(int x1, int z1, int x2, int z2)
        {
            // Go horizontal first, then vertical
            int dx = (int)Mathf.Sign(x2 - x1);
            int dz = (int)Mathf.Sign(z2 - z1);

            int currX = x1;
            int currZ = z1;

            // Horizontal segment - only carve walls
            while (currX != x2)
            {
                currX += dx;
                if (_mazeData.InBounds(currX, currZ))
                {
                    var cell = _mazeData.GetCell(currX, currZ);
                    if ((cell & CardinalWalls) == CardinalWalls)  // Only carve if wall
                        CarveCell(currX, currZ);
                }
            }

            // Vertical segment - only carve walls
            while (currZ != z2)
            {
                currZ += dz;
                if (_mazeData.InBounds(currX, currZ))
                {
                    var cell = _mazeData.GetCell(currX, currZ);
                    if ((cell & CardinalWalls) == CardinalWalls)  // Only carve if wall
                        CarveCell(currX, currZ);
                }
            }
        }

        #endregion
    }
}
