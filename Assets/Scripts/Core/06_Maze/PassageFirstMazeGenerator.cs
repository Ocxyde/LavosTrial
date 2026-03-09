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
using Code.Lavos.Core;

namespace Code.Lavos.Core.Advanced
{
    /// <summary>
    /// Passage-first maze generator with advanced corridor system.
    ///
    /// Algorithm:
    /// 1. Fill all cells with walls
    /// 2. Carve clear main passage from Entrance (A) to Exit (B) with variations
    /// 3. Carve branch passages from main passage
    /// 4. Expand chambers at dead-ends
    /// 5. Add corridor decorations (pillars, arches, niches)
    /// 6. Place objects (torches, enemies, chests)
    ///
    /// Guarantees:
    /// - Always walkable from spawn to exit
    /// - No walls blocking the main path
    /// - Varied corridor widths (1-3 cells)
    /// - Landmark features (junctions, plazas, gates)
    /// - Decorative elements along corridors
    /// </summary>
    public sealed class PassageFirstMazeGenerator
    {
        private DungeonMazeData _mazeData;
        private System.Random _rng;
        private List<(int x, int z)> _passageCells;
        private List<(int x, int z)> _branchCells;
        private List<(int x, int z)> _deadEnds;
        private List<(int x, int z)> _landmarkCells;
        private List<(int x, int z)> _decorationCells;

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
            _landmarkCells = new List<(int, int)>();
            _decorationCells = new List<(int, int)>();

            Debug.Log($"[PassageFirst] Level {level} | Size {size}x{size} | Seed {seed}");

            // === PHASE 1: Fill all walls ===
            FillAllWalls();
            Debug.Log($"[PassageFirst] Phase 1: All walls filled ({size * size} cells)");

            // === PHASE 2: Carve main passage A->B with variations ===
            int passageWidth = Mathf.Max(1, cfg.SpawnRoomSize - 1);
            CarveMainPassageWithVariations(passageWidth);
            Debug.Log($"[PassageFirst] Phase 2: Main passage carved ({_passageCells.Count} cells, with variations)");

            // === PHASE 3: Carve branch passages ===
            CarveBranchPassages(cfg.CorridorWindingFactor);
            Debug.Log($"[PassageFirst] Phase 3: Branch passages carved ({_branchCells.Count} cells)");

            // === PHASE 4: Expand chambers ===
            ExpandChambers(cfg.ChamberExpansionRadius);
            Debug.Log($"[PassageFirst] Phase 4: {_deadEnds.Count} chambers expanded");

            // === PHASE 5: Add corridor decorations ===
            AddCorridorDecorations();
            Debug.Log($"[PassageFirst] Phase 5: {_decorationCells.Count} decorations added");

            // === PHASE 6: Place objects ===
            PlaceTorches(cfg.TorchChance);
            PlaceEnemies(cfg.EnemyDensity);
            PlaceChests(cfg.ChestDensity);
            Debug.Log($"[PassageFirst] Phase 6: Objects placed");

            // === PHASE 7: Set spawn/exit ===
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
        // PHASE 2: Carve Main Passage (Entrance A to Exit B) with Variations
        // ─────────────────────────────────────────────────────────────
        private void CarveMainPassageWithVariations(int baseWidth)
        {
            int startX = 1;
            int startZ = 1;
            int endX = _mazeData.Width - 2;
            int endZ = _mazeData.Height - 2;

            int cx = startX;
            int cz = startZ;
            int stepCount = 0;
            int landmarkInterval = Mathf.Max(5, _mazeData.Width / 8);

            // Clear start cell
            ClearCellWithNeighbors(cx, cz);
            _passageCells.Add((cx, cz));

            // Greedy path to exit with variations
            while (cx != endX || cz != endZ)
            {
                stepCount++;

                // Add landmark features at intervals
                if (stepCount % landmarkInterval == 0)
                {
                    AddLandmarkFeature(cx, cz);
                }

                // Vary corridor width along the path
                int currentWidth = GetCorridorWidthAtStep(stepCount, baseWidth);

                // Randomly choose horizontal or vertical movement
                bool moved = false;
                if (_rng.NextDouble() < 0.5f && cx != endX)
                {
                    // Move horizontally
                    if (cx < endX) cx++;
                    else cx--;
                    moved = true;
                }
                
                if (!moved && cz != endZ)
                {
                    // Move vertically
                    if (cz < endZ) cz++;
                    else cz--;
                }
                else if (!moved && cx != endX)
                {
                    // Must move horizontally
                    if (cx < endX) cx++;
                    else cx--;
                }

                // Clear cell with current width
                ClearCellWithNeighbors(cx, cz);
                _passageCells.Add((cx, cz));

                // Widen corridor based on currentWidth
                if (currentWidth > 1)
                {
                    WidenCorridor(cx, cz, currentWidth);
                }

                // Add curves occasionally (diagonal bulge)
                if (_rng.NextDouble() < 0.15f)
                {
                    AddCorridorCurve(cx, cz);
                }
            }

            // Ensure exit area is clear
            ClearRoomAround(endX, endZ, 2);
            _landmarkCells.Add((endX, endZ));

            Debug.Log($"[PassageFirst] Main passage with variations: ({startX},{startZ}) -> ({endX},{endZ})");
            Debug.Log($"[PassageFirst] Landmarks added: {_landmarkCells.Count}");
        }

        /// <summary>
        /// Get corridor width at a given step (varies along the path).
        /// </summary>
        private int GetCorridorWidthAtStep(int step, int baseWidth)
        {
            // Create natural width variations
            int variation = (int)(_rng.NextDouble() * 3);
            
            // Wider at junctions and landmarks
            if (step % 10 == 0)
                return baseWidth + 2;
            
            // Slightly wider in middle sections
            if (variation == 2)
                return baseWidth + 1;
            
            return baseWidth;
        }

        /// <summary>
        /// Widen corridor at position.
        /// </summary>
        private void WidenCorridor(int cx, int cz, int width)
        {
            // Widen in both directions
            for (int w = 1; w < width; w++)
            {
                // Horizontal widening
                if (_rng.NextDouble() < 0.5f && _mazeData.InBounds(cx + 1, cz))
                {
                    ClearCellWithNeighbors(cx + 1, cz);
                    _passageCells.Add((cx + 1, cz));
                }
                
                // Vertical widening
                if (_rng.NextDouble() < 0.5f && _mazeData.InBounds(cx, cz + 1))
                {
                    ClearCellWithNeighbors(cx, cz + 1);
                    _passageCells.Add((cx, cz + 1));
                }
            }
        }

        /// <summary>
        /// Add a curve/bulge to the corridor for visual interest.
        /// </summary>
        private void AddCorridorCurve(int cx, int cz)
        {
            Direction8 dir = (Direction8)_rng.Next(4);
            var (dx, dz) = Direction8Helper.ToOffset(dir);
            
            int bulgeX = cx + dx;
            int bulgeZ = cz + dz;
            
            if (_mazeData.InBounds(bulgeX, bulgeZ))
            {
                ClearCellWithNeighbors(bulgeX, bulgeZ);
                _decorationCells.Add((bulgeX, bulgeZ));
            }
        }

        /// <summary>
        /// Add landmark feature (junction, plaza, or gate).
        /// </summary>
        private void AddLandmarkFeature(int cx, int cz)
        {
            int landmarkType = _rng.Next(3);
            
            switch (landmarkType)
            {
                case 0: // Small plaza (3x3 open area)
                    ClearRoomAround(cx, cz, 1);
                    _landmarkCells.Add((cx, cz));
                    Debug.Log($"[PassageFirst] Plaza added at ({cx},{cz})");
                    break;
                    
                case 1: // Junction (cross-shaped)
                    ClearCellWithNeighbors(cx, cz);
                    if (_mazeData.InBounds(cx + 1, cz)) ClearCellWithNeighbors(cx + 1, cz);
                    if (_mazeData.InBounds(cx - 1, cz)) ClearCellWithNeighbors(cx - 1, cz);
                    if (_mazeData.InBounds(cx, cz + 1)) ClearCellWithNeighbors(cx, cz + 1);
                    if (_mazeData.InBounds(cx, cz - 1)) ClearCellWithNeighbors(cx, cz - 1);
                    _landmarkCells.Add((cx, cz));
                    Debug.Log($"[PassageFirst] Junction added at ({cx},{cz})");
                    break;
                    
                case 2: // Gate (narrowed passage with pillars)
                    ClearCellWithNeighbors(cx, cz);
                    _landmarkCells.Add((cx, cz));
                    _decorationCells.Add((cx, cz)); // Mark for pillar decoration
                    Debug.Log($"[PassageFirst] Gate added at ({cx},{cz})");
                    break;
            }
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
        // PHASE 5: Add Corridor Decorations
        // ─────────────────────────────────────────────────────────────
        private void AddCorridorDecorations()
        {
            // Add pillars at landmark cells (gates, junctions)
            AddPillars();

            // Add wall niches along corridors
            AddWallNiches();

            // Add arches at passage transitions
            AddArches();
        }

        /// <summary>
        /// Add pillar decorations at landmark cells.
        /// Pillars are marked for later instantiation (not walls, but decorative).
        /// </summary>
        private void AddPillars()
        {
            foreach (var (x, z) in _landmarkCells)
            {
                // Mark adjacent wall cells as pillar positions
                // Pillars are placed at corners of landmarks
                TryAddPillar(x - 1, z - 1);
                TryAddPillar(x + 1, z - 1);
                TryAddPillar(x - 1, z + 1);
                TryAddPillar(x + 1, z + 1);
            }

            Debug.Log($"[PassageFirst] Pillars added at {_decorationCells.Count} positions");
        }

        /// <summary>
        /// Try to add a pillar at position (must be a wall cell).
        /// </summary>
        private void TryAddPillar(int x, int z)
        {
            if (!_mazeData.InBounds(x, z))
                return;

            var cell = _mazeData.GetCell(x, z);
            // Only add pillar if it's a wall cell
            if ((cell & CellFlags8.Wall_All) != 0)
            {
                cell |= CellFlags8.HasPillar;
                _mazeData.SetCell(x, z, cell);
                _decorationCells.Add((x, z));
            }
        }

        /// <summary>
        /// Add wall niches (alcoves) along corridor walls.
        /// Niches are small recesses that can hold statues, torches, or treasures.
        /// </summary>
        private void AddWallNiches()
        {
            int nicheChance = 15; // 1 in 15 chance per corridor cell
            int nicheCount = 0;

            foreach (var (x, z) in _passageCells)
            {
                if (_rng.Next(nicheChance) == 0)
                {
                    // Try to create a niche in one of the adjacent wall cells
                    Direction8 dir = (Direction8)_rng.Next(4);
                    var (dx, dz) = Direction8Helper.ToOffset(dir);

                    int nicheX = x + dx;
                    int nicheZ = z + dz;

                    if (_mazeData.InBounds(nicheX, nicheZ))
                    {
                        var cell = _mazeData.GetCell(nicheX, nicheZ);
                        // Only create niche in wall cells
                        if ((cell & CellFlags8.Wall_All) != 0)
                        {
                            // Clear the wall to create a niche (1-cell deep alcove)
                            _mazeData.SetCell(nicheX, nicheZ, 0);
                            cell |= CellFlags8.HasNiche;
                            _mazeData.SetCell(nicheX, nicheZ, cell);
                            _decorationCells.Add((nicheX, nicheZ));
                            nicheCount++;
                        }
                    }
                }
            }

            Debug.Log($"[PassageFirst] Wall niches added: {nicheCount}");
        }

        /// <summary>
        /// Add arches at corridor transitions and landmark entrances.
        /// Arches are marked using HasTorch flag on adjacent wall cells for later instantiation.
        /// (Using HasTorch as placeholder since we're at ushort limit for CellFlags8)
        /// </summary>
        private void AddArches()
        {
            int archInterval = Mathf.Max(10, _mazeData.Width / 6);
            int archCount = 0;

            // Add arches at regular intervals along the main passage
            for (int i = 0; i < _passageCells.Count; i += archInterval)
            {
                var (x, z) = _passageCells[i];

                // Find the direction of travel (to adjacent passage cell)
                Direction8? travelDir = null;
                if (i > 0)
                {
                    var (prevX, prevZ) = _passageCells[i - 1];
                    travelDir = GetDirectionFromOffset(x - prevX, z - prevZ);
                }

                if (travelDir.HasValue)
                {
                    // Place arch marker on the walls perpendicular to travel direction
                    var (dx, dz) = Direction8Helper.ToOffset(travelDir.Value);
                    // Perpendicular directions (rotate 90 degrees)
                    int perpDx1 = -dz;
                    int perpDz1 = dx;
                    int perpDx2 = dz;
                    int perpDz2 = -dx;

                    // Try to place arch marker on one side
                    int archX1 = x + perpDx1;
                    int archZ1 = z + perpDz1;
                    if (_mazeData.InBounds(archX1, archZ1))
                    {
                        var cell = _mazeData.GetCell(archX1, archZ1);
                        if ((cell & CellFlags8.Wall_All) != 0)
                        {
                            // Use HasTorch as arch marker (will be interpreted by MazePlacementEngine)
                            cell |= CellFlags8.HasTorch;
                            _mazeData.SetCell(archX1, archZ1, cell);
                            _decorationCells.Add((archX1, archZ1));
                            archCount++;
                        }
                    }

                    // Try to place arch marker on the other side
                    int archX2 = x + perpDx2;
                    int archZ2 = z + perpDz2;
                    if (_mazeData.InBounds(archX2, archZ2))
                    {
                        var cell = _mazeData.GetCell(archX2, archZ2);
                        if ((cell & CellFlags8.Wall_All) != 0)
                        {
                            // Use HasTorch as arch marker
                            cell |= CellFlags8.HasTorch;
                            _mazeData.SetCell(archX2, archZ2, cell);
                            _decorationCells.Add((archX2, archZ2));
                            archCount++;
                        }
                    }
                }
            }

            Debug.Log($"[PassageFirst] Arch markers added: {archCount}");
        }

        /// <summary>
        /// Get direction from offset values.
        /// </summary>
        private Direction8? GetDirectionFromOffset(int dx, int dz)
        {
            if (dx > 0) return Direction8.E;
            if (dx < 0) return Direction8.W;
            if (dz > 0) return Direction8.S;
            if (dz < 0) return Direction8.N;
            return null;
        }

        // ─────────────────────────────────────────────────────────────
        // PHASE 6: Object Placement
        // ─────────────────────────────────────────────────────────────
        private void PlaceTorches(float chance)
        {
            int torchCount = 0;
            foreach (var (x, z) in _passageCells)
            {
                // Skip cells already marked as decorations (arches)
                if (_decorationCells.Contains((x, z)))
                    continue;

                if (_rng.NextDouble() < chance)
                {
                    var cell = _mazeData.GetCell(x, z);
                    cell |= CellFlags8.HasTorch;
                    _mazeData.SetCell(x, z, cell);
                    torchCount++;
                }
            }
            Debug.Log($"[PassageFirst] Torches placed: {torchCount}");
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
