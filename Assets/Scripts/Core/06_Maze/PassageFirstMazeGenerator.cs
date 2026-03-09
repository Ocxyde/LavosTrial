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
// PassageFirstMazeGenerator.cs
// Passage-first maze generation - creates clear path from entrance to exit first
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core.Advanced
{
    /// <summary>
    /// Passage-first maze generator.
    /// 
    /// Algorithm:
    /// 1. Fill all cells with walls
    /// 2. Carve clear main passage from Entrance (A) to Exit (B)
    /// 3. Carve branch passages from main passage
    /// 4. Expand chambers at dead-ends
    /// 5. Place objects (torches, enemies, chests)
    /// 
    /// Guarantees:
    /// - Always walkable from spawn to exit
    /// - No walls blocking the main path
    /// - Clear 1-2 cell wide corridor
    /// </summary>
    public sealed class PassageFirstMazeGenerator
    {
        private DungeonMazeData _mazeData;
        private System.Random _rng;
        private List<(int x, int z)> _passageCells;
        private List<(int x, int z)> _branchCells;
        private List<(int x, int z)> _deadEnds;

        /// <summary>
        /// Generate a maze using passage-first algorithm.
        /// Result is stored in RAM (DungeonMazeData), not database.
        /// </summary>
        public DungeonMazeData Generate(int seed, int level, DungeonMazeConfig cfg)
        {
            ValidateConfig(cfg);

            var scaler = cfg.Difficulty;
            int size = scaler.MazeSize(level, cfg.BaseSize, cfg.MinSize, cfg.MaxSize);

            _rng = new System.Random(seed);
            _mazeData = new DungeonMazeData(size, size, seed, level)
            {
                DifficultyFactor = scaler.Factor(level),
                Config = cfg,
            };

            _passageCells = new List<(int, int)>();
            _branchCells = new List<(int, int)>();
            _deadEnds = new List<(int, int)>();

            Debug.Log($"[PassageFirst] Level {level} | Size {size}x{size} | Seed {seed}");

            // === PHASE 1: Fill all walls ===
            FillAllWalls();
            Debug.Log($"[PassageFirst] Phase 1: All walls filled ({size * size} cells)");

            // === PHASE 2: Carve main passage A->B ===
            int passageWidth = Mathf.Max(1, cfg.SpawnRoomSize - 1);
            CarveMainPassage(passageWidth);
            Debug.Log($"[PassageFirst] Phase 2: Main passage carved ({_passageCells.Count} cells, width={passageWidth})");

            // === PHASE 3: Carve branch passages ===
            CarveBranchPassages(cfg.CorridorWindingFactor);
            Debug.Log($"[PassageFirst] Phase 3: Branch passages carved ({_branchCells.Count} cells)");

            // === PHASE 4: Expand chambers ===
            ExpandChambers(cfg.ChamberExpansionRadius);
            Debug.Log($"[PassageFirst] Phase 4: {_deadEnds.Count} chambers expanded");

            // === PHASE 5: Place objects ===
            PlaceTorches(cfg.TorchChance);
            PlaceEnemies(cfg.EnemyDensity);
            PlaceChests(cfg.ChestDensity);
            Debug.Log($"[PassageFirst] Phase 5: Objects placed");

            // === PHASE 6: Set spawn/exit ===
            _mazeData.SetSpawn(1, 1);
            _mazeData.SetExit(size - 2, size - 2);

            Debug.Log($"[PassageFirst] COMPLETE - Maze generated and stored in RAM");
            return _mazeData;
        }

        // ─────────────────────────────────────────────────────────────
        // PHASE 1: Initialize Walls
        // ─────────────────────────────────────────────────────────────
        private void FillAllWalls()
        {
            int w = _mazeData.Width;
            int h = _mazeData.Height;

            for (int x = 0; x < w; x++)
            {
                for (int z = 0; z < h; z++)
                {
                    var cell = _mazeData.GetCell(x, z);
                    cell |= CellFlags8.Wall_All;
                    _mazeData.SetCell(x, z, cell);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        // PHASE 2: Carve Main Passage (Entrance A to Exit B)
        // ─────────────────────────────────────────────────────────────
        private void CarveMainPassage(int width)
        {
            int startX = 1;
            int startZ = 1;
            int endX = _mazeData.Width - 2;
            int endZ = _mazeData.Height - 2;

            int cx = startX;
            int cz = startZ;

            // Clear start cell
            ClearCellWithNeighbors(cx, cz);
            _passageCells.Add((cx, cz));

            // Greedy path to exit
            while (cx != endX || cz != endZ)
            {
                // Randomly choose horizontal or vertical movement
                if (_rng.NextDouble() < 0.5f && cx != endX)
                {
                    // Move horizontally
                    if (cx < endX) cx++;
                    else cx--;
                }
                else if (cz != endZ)
                {
                    // Move vertically
                    if (cz < endZ) cz++;
                    else cz--;
                }
                else if (cx != endX)
                {
                    // Must move horizontally
                    if (cx < endX) cx++;
                    else cx--;
                }

                ClearCellWithNeighbors(cx, cz);
                _passageCells.Add((cx, cz));

                // Add width to passage (make it wider)
                if (width > 1)
                {
                    // Clear adjacent cells for wider passage
                    if (_rng.NextDouble() < 0.5f && _mazeData.InBounds(cx, cz + 1))
                    {
                        ClearCellWithNeighbors(cx, cz + 1);
                        _passageCells.Add((cx, cz + 1));
                    }
                    if (_rng.NextDouble() < 0.5f && _mazeData.InBounds(cx + 1, cz))
                    {
                        ClearCellWithNeighbors(cx + 1, cz);
                        _passageCells.Add((cx + 1, cz));
                    }
                }
            }

            Debug.Log($"[PassageFirst] Main passage: ({startX},{startZ}) -> ({endX},{endZ})");
        }

        // ─────────────────────────────────────────────────────────────
        // PHASE 3: Carve Branch Passages
        // ─────────────────────────────────────────────────────────────
        private void CarveBranchPassages(float windingFactor)
        {
            // Pick random points along main passage to branch from
            int branchCount = Mathf.RoundToInt(_passageCells.Count * windingFactor);
            branchCount = Mathf.Clamp(branchCount, 5, 30);

            for (int i = 0; i < branchCount; i++)
            {
                int idx = _rng.Next(_passageCells.Count);
                var (startX, startZ) = _passageCells[idx];

                // Pick random direction
                Direction8 dir = (Direction8)_rng.Next(4); // Only cardinal
                var (dx, dz) = Direction8Helper.ToOffset(dir);

                // Carve branch
                int branchLength = _rng.Next(3, 8);
                int cx = startX;
                int cz = startZ;

                for (int j = 0; j < branchLength; j++)
                {
                    cx += dx;
                    cz += dz;

                    if (!_mazeData.InBounds(cx, cz))
                        break;

                    // Stop if we hit another passage
                    var cell = _mazeData.GetCell(cx, cz);
                    if ((cell & CellFlags8.Wall_All) != CellFlags8.Wall_All)
                        break;

                    ClearCellWithNeighbors(cx, cz);
                    _branchCells.Add((cx, cz));
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        // PHASE 4: Expand Chambers
        // ─────────────────────────────────────────────────────────────
        private void ExpandChambers(int radius)
        {
            // Find dead-ends (branch endpoints)
            foreach (var (x, z) in _branchCells)
            {
                int openNeighbors = CountOpenNeighbors(x, z);
                if (openNeighbors == 1)
                {
                    _deadEnds.Add((x, z));
                }
            }

            // Expand dead-ends into chambers
            foreach (var (x, z) in _deadEnds)
            {
                ClearRoomAround(x, z, radius);
                var cell = _mazeData.GetCell(x, z);
                cell |= CellFlags8.IsRoom;
                _mazeData.SetCell(x, z, cell);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // PHASE 5: Object Placement
        // ─────────────────────────────────────────────────────────────
        private void PlaceTorches(float chance)
        {
            foreach (var (x, z) in _passageCells)
            {
                if (_rng.NextDouble() < chance)
                {
                    var cell = _mazeData.GetCell(x, z);
                    cell |= CellFlags8.HasTorch;
                    _mazeData.SetCell(x, z, cell);
                }
            }
        }

        private void PlaceEnemies(float density)
        {
            int count = Mathf.RoundToInt(_deadEnds.Count * density * 5);
            count = Mathf.Min(count, _deadEnds.Count);

            for (int i = 0; i < count; i++)
            {
                int idx = _rng.Next(_deadEnds.Count);
                var (x, z) = _deadEnds[idx];

                var cell = _mazeData.GetCell(x, z);
                cell |= CellFlags8.HasEnemy;
                _mazeData.SetCell(x, z, cell);
            }
        }

        private void PlaceChests(float density)
        {
            int count = Mathf.RoundToInt(_deadEnds.Count * density * 3);
            count = Mathf.Min(count, _deadEnds.Count);

            for (int i = 0; i < count; i++)
            {
                int idx = _rng.Next(_deadEnds.Count);
                var (x, z) = _deadEnds[idx];

                var cell = _mazeData.GetCell(x, z);
                cell |= CellFlags8.HasChest;
                _mazeData.SetCell(x, z, cell);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────
        private void ClearCellWithNeighbors(int x, int z)
        {
            // Clear this cell completely
            _mazeData.SetCell(x, z, 0);

            // Clear cardinal walls on neighbors
            ClearNeighborWall(x, z + 1, CellFlags8.Wall_S);
            ClearNeighborWall(x, z - 1, CellFlags8.Wall_N);
            ClearNeighborWall(x + 1, z, CellFlags8.Wall_W);
            ClearNeighborWall(x - 1, z, CellFlags8.Wall_E);
        }

        private void ClearNeighborWall(int x, int z, uint wallFlag)
        {
            if (_mazeData.InBounds(x, z))
            {
                var cell = _mazeData.GetCell(x, z);
                cell &= ~wallFlag;
                _mazeData.SetCell(x, z, cell);
            }
        }

        private void ClearRoomAround(int centerX, int centerZ, int radius)
        {
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                for (int z = centerZ - radius; z <= centerZ + radius; z++)
                {
                    if (_mazeData.InBounds(x, z))
                    {
                        _mazeData.SetCell(x, z, 0);
                    }
                }
            }
        }

        private int CountOpenNeighbors(int x, int z)
        {
            int count = 0;
            if (_mazeData.InBounds(x, z + 1) && IsCellClear(x, z + 1)) count++;
            if (_mazeData.InBounds(x, z - 1) && IsCellClear(x, z - 1)) count++;
            if (_mazeData.InBounds(x + 1, z) && IsCellClear(x + 1, z)) count++;
            if (_mazeData.InBounds(x - 1, z) && IsCellClear(x - 1, z)) count++;
            return count;
        }

        private bool IsCellClear(int x, int z)
        {
            var cell = _mazeData.GetCell(x, z);
            return (cell & CellFlags8.Wall_All) == 0;
        }

        // ─────────────────────────────────────────────────────────────
        // Validation
        // ─────────────────────────────────────────────────────────────
        private void ValidateConfig(DungeonMazeConfig cfg)
        {
            if (cfg == null)
                throw new ArgumentNullException(nameof(cfg));
        }
    }
}
