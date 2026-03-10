﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿// Copyright (C) 2026 Ocxyde
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
// CorridorFillSystem.cs
// Two-pass corridor filling system - fills remaining space with corridors
// UPDATED 2026-03-09: Preserves maze structure, fills empty wall space
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   var fillSystem = new CorridorFillSystem();
//   fillSystem.Fill(mazeData, rng, config);
//
// Location: Assets/Scripts/Core/06_Maze/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Corridor Fill Configuration
    /// Controls how remaining wall space is filled with corridors
    /// </summary>
    [Serializable]
    public class CorridorFillConfig
    {
        [Header("Fill Density")]
        [Range(0f, 1f)]
        public float FillDensity = 0.15f;

        [Header("Corridor Dimensions")]
        [Range(1, 5)]
        public int MinLength = 1;

        [Range(2, 6)]
        public int MaxLength = 3;

        [Range(1, 3)]
        public int CorridorWidth = 1;

        [Header("Limits")]
        [Range(0.1f, 0.6f)]
        public float MaxFillPercentage = 0.40f;

        [Header("Behavior")]
        public bool AvoidDeadEnds = true;
        public bool PreferCardinalDirections = true;
        public bool AllowShortCorridors = true;
    }

    /// <summary>
    /// Corridor Fill Data Structure
    /// </summary>
    [Serializable]
    public class FillCorridor
    {
        public int StartX;
        public int StartZ;
        public Direction8 Direction;
        public int Length;
        public int Width;
        public int EndX;
        public int EndZ;
    }

    /// <summary>
    /// Corridor Fill System - Two-pass approach
    ///
    /// Phase 1: Generate normal maze (DFS + A* + dead-ends)
    /// Phase 2: Fill remaining wall space with short corridors
    ///
    /// Features:
    /// - Preserves original maze structure
    /// - Fills empty space naturally
    /// - Configurable fill density
    /// - Short connecting corridors (1-3 cells)
    /// - Cardinal directions only (N,S,E,W)
    ///
    /// Usage:
    ///   var fillSystem = new CorridorFillSystem();
    ///   fillSystem.Fill(mazeData, rng, config);
    /// </summary>
    public sealed class CorridorFillSystem
    {
        private CorridorFillConfig _config;
        private MazeData8 _mazeData;
        private System.Random _rng;
        private List<FillCorridor> _filledCorridors;
        private int _totalCellsCarved;
        private HashSet<(int x, int z)> _carvedCells;

        /// <summary>
        /// Ensure Fill() has been called and initialized state
        /// </summary>
        private void EnsureInitialized()
        {
            if (_mazeData == null) throw new InvalidOperationException("Fill() must be called first");
            if (_rng == null) throw new InvalidOperationException("Fill() must be called first");
        }

        /// <summary>
        /// Generated fill corridors
        /// </summary>
        public IReadOnlyList<FillCorridor> FilledCorridors => _filledCorridors?.AsReadOnly() ?? new List<FillCorridor>().AsReadOnly();

        /// <summary>
        /// Total fill corridors created
        /// </summary>
        public int TotalCount => _filledCorridors?.Count ?? 0;

        /// <summary>
        /// Total cells carved by fill system
        /// </summary>
        public int TotalCellsCarved => _totalCellsCarved;

        /// <summary>
        /// Initialize corridor fill system
        /// </summary>
        public CorridorFillSystem(CorridorFillConfig config = null)
        {
            _config = config ?? CreateDefaultConfig();
            _filledCorridors = new List<FillCorridor>();
        }

        /// <summary>
        /// Create default configuration
        /// </summary>
        public static CorridorFillConfig CreateDefaultConfig()
        {
            return new CorridorFillConfig
            {
                FillDensity = 0.15f,
                MinLength = 1,
                MaxLength = 3,
                CorridorWidth = 1,
                MaxFillPercentage = 0.10f,
                AvoidDeadEnds = true,
                PreferCardinalDirections = true,
                AllowShortCorridors = true
            };
        }

        /// <summary>
        /// Fill remaining wall space with corridors
        /// </summary>
        public void Fill(MazeData8 mazeData, System.Random rng, CorridorFillConfig overrideConfig = null)
        {
            _mazeData = mazeData ?? throw new ArgumentNullException(nameof(mazeData));
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));

            if (overrideConfig != null)
            {
                _config = overrideConfig;
            }

            if (_config.MinLength > _config.MaxLength)
            {
                throw new InvalidOperationException($"CorridorFillConfig: MinLength ({_config.MinLength}) cannot be greater than MaxLength ({_config.MaxLength})");
            }

            _filledCorridors = new List<FillCorridor>();
            _totalCellsCarved = 0;
            _carvedCells = new HashSet<(int x, int z)>();

            int maxFillCells = CalculateMaxFillCells(mazeData);

#if UNITY_EDITOR
            Debug.Log($"[CorridorFill] Starting fill | Density: {_config.FillDensity:P1} | Max Fill: {maxFillCells} cells");
#endif

            // Find all wall cells adjacent to passages (valid spawn points)
            var validWalls = FindWallCellsAdjacentToPassages(mazeData);
#if UNITY_EDITOR
            Debug.Log($"[CorridorFill] Found {validWalls.Count} valid wall cells");
#endif

            if (validWalls.Count == 0)
            {
                Debug.LogWarning("[CorridorFill] No valid wall cells found - maze may be incomplete");
                return;
            }

            // Shuffle for random distribution
            Shuffle(validWalls);

            // Fill walls with corridors
            int carvedCells = 0;
            foreach (var (x, z) in validWalls)
            {
                // Check if we've hit the limit
                if (carvedCells >= maxFillCells)
                {
#if UNITY_EDITOR
                    Debug.Log($"[CorridorFill] Reached max fill limit ({maxFillCells} cells)");
#endif
                    break;
                }

                // Use FillDensity as spawn chance
                if (_rng.NextDouble() > _config.FillDensity)
                {
                    continue;
                }

                // Try to carve corridor from this wall cell
                if (TryCarveFillCorridor(x, z, out FillCorridor corridor))
                {
                    _filledCorridors.Add(corridor);
                    _totalCellsCarved += corridor.Length * corridor.Width;
                    carvedCells += corridor.Length * corridor.Width;
                }
            }

#if UNITY_EDITOR
            Debug.Log($"[CorridorFill] Fill complete | Corridors: {TotalCount} | Cells carved: {TotalCellsCarved}");
#endif
        }

        /// <summary>
        /// Calculate maximum fill cells based on grid size
        /// </summary>
        private int CalculateMaxFillCells(MazeData8 mazeData)
        {
            int totalCells = mazeData.Width * mazeData.Height;
            return Mathf.RoundToInt(totalCells * _config.MaxFillPercentage);
        }

        /// <summary>
        /// Find all wall cells adjacent to passage cells
        /// These are valid spawn points for fill corridors
        /// </summary>
        private List<(int x, int z)> FindWallCellsAdjacentToPassages(MazeData8 mazeData)
        {
            var validWalls = new List<(int x, int z)>();

            for (int x = 0; x < mazeData.Width; x++)
            {
                for (int z = 0; z < mazeData.Height; z++)
                {
                    var cell = mazeData.GetCell(x, z);

                    // Must be a wall cell (all walls set)
                    bool isWall = (cell & CellFlags8.AllWalls) == CellFlags8.AllWalls;
                    if (!isWall) continue;

                    // Cannot be spawn or exit room
                    bool isSpawn = (cell & CellFlags8.SpawnRoom) != CellFlags8.None;
                    bool isExit = (cell & CellFlags8.IsExit) != CellFlags8.None;
                    if (isSpawn || isExit) continue;

                    // Check if adjacent to at least one passage cell
                    if (HasAdjacentPassage(mazeData, x, z))
                    {
                        validWalls.Add((x, z));
                    }
                }
            }

            return validWalls;
        }

        /// <summary>
        /// Check if cell has adjacent passage in any cardinal direction
        /// </summary>
        private bool HasAdjacentPassage(MazeData8 mazeData, int x, int z)
        {
            var cardinalDirs = new Direction8[] { Direction8.N, Direction8.S, Direction8.E, Direction8.W };

            foreach (var dir in cardinalDirs)
            {
                var (dx, dz) = Direction8Helper.ToOffset(dir);
                int nx = x + dx;
                int nz = z + dz;

                if (!mazeData.InBounds(nx, nz)) continue;

                var neighborCell = mazeData.GetCell(nx, nz);
                bool isPassage = (neighborCell & CellFlags8.AllWalls) == CellFlags8.None;

                if (isPassage) return true;
            }

            return false;
        }

        /// <summary>
        /// Try to carve a fill corridor from wall cell
        /// </summary>
        private bool TryCarveFillCorridor(int startX, int startZ, out FillCorridor corridor)
        {
            corridor = null;

            if (_carvedCells.Contains((startX, startZ)))
            {
                return false;
            }

            // Get random cardinal directions
            var dirs = GetRandomCardinalDirections();

            foreach (var dir in dirs)
            {
                // Check if we can carve in this direction
                if (CanCarveCorridor(startX, startZ, dir, out int length))
                {
                    // Carve the corridor
                    CarveCorridor(startX, startZ, dir, length);

                    // Create corridor data
                    var (dx, dz) = Direction8Helper.ToOffset(dir);
                    corridor = new FillCorridor
                    {
                        StartX = startX,
                        StartZ = startZ,
                        Direction = dir,
                        Length = length,
                        Width = _config.CorridorWidth,
                        EndX = startX + dx * length,
                        EndZ = startZ + dz * length
                    };

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if corridor can be carved in specified direction
        /// </summary>
        private bool CanCarveCorridor(int startX, int startZ, Direction8 dir, out int length)
        {
            EnsureInitialized();
            length = 0;

            var (dx, dz) = Direction8Helper.ToOffset(dir);

            // First cell must be wall (we're carving into it)
            if (!IsCarveableWall(startX, startZ))
            {
                return false;
            }

            int actualLength = 0;
            int currX = startX;
            int currZ = startZ;

            // Try to carve up to MaxLength cells
            for (int i = 0; i < _config.MaxLength; i++)
            {
                int nextX = currX + dx;
                int nextZ = currZ + dz;

                // Check bounds
                if (!_mazeData.InBounds(nextX, nextZ))
                {
                    break;
                }

                // Check if cell is carveable
                if (!IsCarveableWall(nextX, nextZ))
                {
                    // Hit existing passage or room - stop here
                    break;
                }

                // Check if cell was already carved by another corridor
                if (_carvedCells.Contains((nextX, nextZ)))
                {
                    break;
                }

                // Check if we'd be carving into a dead-end (if AvoidDeadEnds is enabled)
                if (_config.AvoidDeadEnds && IsNearDeadEnd(nextX, nextZ))
                {
                    // Allow carving up to this point, but stop
                    break;
                }

                actualLength++;
                currX = nextX;
                currZ = nextZ;
            }

            // Validate minimum length
            if (actualLength < _config.MinLength)
            {
                return false;
            }

            length = actualLength;
            return true;
        }

        /// <summary>
        /// Check if cell is a wall that can be carved
        /// </summary>
        private bool IsCarveableWall(int x, int z)
        {
            EnsureInitialized();
            var cell = _mazeData.GetCell(x, z);

            // Must have all walls set
            bool isWall = (cell & CellFlags8.AllWalls) == CellFlags8.AllWalls;
            if (!isWall) return false;

            // Cannot be spawn or exit
            bool isSpawn = (cell & CellFlags8.SpawnRoom) != CellFlags8.None;
            bool isExit = (cell & CellFlags8.IsExit) != CellFlags8.None;
            if (isSpawn || isExit) return false;

            return true;
        }

        /// <summary>
        /// Check if cell is near a dead-end (to avoid conflicts)
        /// </summary>
        private bool IsNearDeadEnd(int x, int z)
        {
            EnsureInitialized();
            var cardinalDirs = new Direction8[] { Direction8.N, Direction8.S, Direction8.E, Direction8.W };
            int passageCount = 0;

            foreach (var dir in cardinalDirs)
            {
                var (dx, dz) = Direction8Helper.ToOffset(dir);
                int nx = x + dx;
                int nz = z + dz;

                if (!_mazeData.InBounds(nx, nz)) continue;

                var cell = _mazeData.GetCell(nx, nz);

                bool hasChest = (cell & CellFlags8.HasChest) != CellFlags8.None;
                bool hasEnemy = (cell & CellFlags8.HasEnemy) != CellFlags8.None;
                if (hasChest || hasEnemy) return true;

                bool isPassage = (cell & CellFlags8.AllWalls) == CellFlags8.None;
                if (isPassage) passageCount++;
            }

            if (passageCount == 1) return true;

            return false;
        }

        /// <summary>
        /// Carve corridor in specified direction WITH WALLS ON BOTH SIDES + CORNERS
        /// Uses MazeData8 wall flag system (N, S, E, W individual walls)
        /// </summary>
        private void CarveCorridor(int startX, int startZ, Direction8 dir, int length)
        {
            var (dx, dz) = Direction8Helper.ToOffset(dir);

            // Get perpendicular directions for wall placement
            Direction8[] perpendiculars = GetPerpendicularDirections(dir);

            // Carve main path and ensure walls on both sides
            int currX = startX;
            int currZ = startZ;

            for (int i = 0; i < length; i++)
            {
                // STEP 1: Carve this cell (remove all wall flags - it's the floor)
                var cell = _mazeData.GetCell(currX, currZ);
                cell &= ~CellFlags8.WallN;
                cell &= ~CellFlags8.WallS;
                cell &= ~CellFlags8.WallE;
                cell &= ~CellFlags8.WallW;
                _mazeData.SetCell(currX, currZ, cell);
                _carvedCells.Add((currX, currZ));

                // STEP 2: Ensure walls on both sides of corridor
                foreach (var perpDir in perpendiculars)
                {
                    var (pdx, pdz) = Direction8Helper.ToOffset(perpDir);
                    int wallX = currX + pdx;
                    int wallZ = currZ + pdz;

                    if (!_mazeData.InBounds(wallX, wallZ)) continue;
                    if (!IsCarveableWall(wallX, wallZ)) continue;

                    var wallCell = _mazeData.GetCell(wallX, wallZ);
                    wallCell |= GetWallFlagForDirection(GetOppositeDirection(perpDir));
                    _mazeData.SetCell(wallX, wallZ, wallCell);
                }

                // Move to next cell
                currX += dx;
                currZ += dz;
            }

            // STEP 3: Add corner pieces at start and end of corridor
            AddCornerPieces(startX, startZ, dir, true);  // Start corner
            AddCornerPieces(currX - dx, currZ - dz, dir, false);  // End corner
        }

        /// <summary>
        /// Get wall flag for direction
        /// </summary>
        private CellFlags8 GetWallFlagForDirection(Direction8 dir)
        {
            switch (dir)
            {
                case Direction8.N: return CellFlags8.WallN;
                case Direction8.S: return CellFlags8.WallS;
                case Direction8.E: return CellFlags8.WallE;
                case Direction8.W: return CellFlags8.WallW;
                default: throw new ArgumentException($"Direction {dir} is not supported for wall flags", nameof(dir));
            }
        }

        /// <summary>
        /// Add corner wall pieces at corridor ends
        /// Creates L-shaped corners (perpendicular wall + end wall)
        /// </summary>
        private void AddCornerPieces(int x, int z, Direction8 dir, bool isStart)
        {
            // Get perpendicular directions
            Direction8[] perpendiculars = GetPerpendicularDirections(dir);

            // Get the direction we came from (for corner placement)
            Direction8 fromDir = isStart ? GetOppositeDirection(dir) : dir;

            // Place corner walls in perpendicular + from directions
            foreach (var perpDir in perpendiculars)
            {
                var (pdx, pdz) = Direction8Helper.ToOffset(perpDir);
                var (fdx, fdz) = Direction8Helper.ToOffset(fromDir);

                // Corner cell (perpendicular + from direction)
                int cornerX = x + pdx + fdx;
                int cornerZ = z + pdz + fdz;

                if (!_mazeData.InBounds(cornerX, cornerZ)) continue;
                if (!IsCarveableWall(cornerX, cornerZ)) continue;

                var cornerCell = _mazeData.GetCell(cornerX, cornerZ);
                cornerCell |= GetWallFlagForDirection(GetOppositeDirection(perpDir));
                cornerCell |= GetWallFlagForDirection(GetOppositeDirection(fromDir));
                _mazeData.SetCell(cornerX, cornerZ, cornerCell);
            }
        }

        /// <summary>
        /// Get opposite direction
        /// </summary>
        private Direction8 GetOppositeDirection(Direction8 dir)
        {
            switch (dir)
            {
                case Direction8.N: return Direction8.S;
                case Direction8.S: return Direction8.N;
                case Direction8.E: return Direction8.W;
                case Direction8.W: return Direction8.E;
                default: throw new ArgumentException($"Direction {dir} does not have a supported opposite", nameof(dir));
            }
        }

        /// <summary>
        /// Get perpendicular directions for wall placement
        /// </summary>
        private Direction8[] GetPerpendicularDirections(Direction8 dir)
        {
            switch (dir)
            {
                case Direction8.N:
                case Direction8.S:
                    return new Direction8[] { Direction8.E, Direction8.W };
                case Direction8.E:
                case Direction8.W:
                    return new Direction8[] { Direction8.N, Direction8.S };
                default:
                    throw new ArgumentException($"Direction {dir} does not have supported perpendiculars", nameof(dir));
            }
        }

        private static readonly Direction8[] _cardinalDirections = { Direction8.N, Direction8.S, Direction8.E, Direction8.W };

        /// <summary>
        /// Get random cardinal directions (N,S,E,W)
        /// </summary>
        private Direction8[] GetRandomCardinalDirections()
        {
            var dirs = new Direction8[4];
            Array.Copy(_cardinalDirections, dirs, 4);
            Shuffle(dirs);
            return dirs;
        }

        /// <summary>
        /// Fisher-Yates shuffle
        /// </summary>
        private void Shuffle<T>(T[] arr)
        {
            if (_rng == null) throw new InvalidOperationException("Fill() must be called first");
            for (int i = arr.Length - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
        }

        /// <summary>
        /// Fisher-Yates shuffle for lists
        /// </summary>
        private void Shuffle<T>(List<T> list)
        {
            if (_rng == null) throw new InvalidOperationException("Fill() must be called first");
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// <summary>
        /// Get statistics about filled corridors
        /// </summary>
        public CorridorFillStatistics GetStatistics()
        {
            double avgLength = 0;
            if (_filledCorridors.Count > 0)
            {
                int totalLength = 0;
                for (int i = 0; i < _filledCorridors.Count; i++)
                {
                    totalLength += _filledCorridors[i].Length;
                }
                avgLength = (double)totalLength / _filledCorridors.Count;
            }

            return new CorridorFillStatistics
            {
                TotalCount = TotalCount,
                TotalCellsCarved = TotalCellsCarved,
                AvgLength = avgLength
            };
        }
    }

    /// <summary>
    /// Corridor fill statistics
    /// </summary>
    [Serializable]
    public class CorridorFillStatistics
    {
        public int TotalCount;
        public int TotalCellsCarved;
        public double AvgLength;

        public override string ToString()
        {
            return $"Fill Corridors: {TotalCount} | Cells: {TotalCellsCarved} | Avg Len: {AvgLength:F1}";
        }
    }
}
