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
// DeadEndCorridorSystem.cs
// Mathematical dead-end corridor generation system
// UPDATED 2026-03-09: Built-in dead-end system with corridor math
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Dead-End Corridor Configuration
    /// All values loaded from JSON config - no hardcoding
    /// Updated 2026-03-10: Corrected defaults per design specification
    /// </summary>
    [Serializable]
    public class DeadEndCorridorConfig
    {
        [Header("Base Density")]
        [Range(0f, 1f)]
        public float BaseDensity = 0.30f;  // 30% base chance at level 0 (was 0.15f)

        [Header("Difficulty Scaling")]
        [Range(1f, 5f)]
        public float MaxMultiplier = 2.5f;  // 2.5× at max level

        [Range(1, 100)]
        public int MaxLevel = 39;

        [Range(0.5f, 3f)]
        public float Exponent = 2.0f;  // Power curve shaping

        [Header("Corridor Dimensions")]
        [Range(1, 10)]
        public int MinLength = 3;  // Minimum dead-end length (cells) - was 2

        [Range(2, 15)]
        public int MaxLength = 8;  // Maximum dead-end length (cells) - was 5

        [Range(1, 5)]
        public int CorridorWidth = 1;  // 1 cell = 6m (fixed)

        [Header("Object Placement")]
        [Range(0f, 1f)]
        public float ChestChanceAtEnd = 0.40f;  // 40% chest at dead-end (was 0.5f)

        [Range(0f, 1f)]
        public float EnemyChanceAtEnd = 0.40f;  // 40% enemy at dead-end (was 0.3f)

        [Range(0f, 1f)]
        public float TrapChanceAtEnd = 0.05f;  // 5% trap at dead-end (was 0.1f)

        [Header("Limits")]
        [Range(0.01f, 0.5f)]
        public float MaxGridPercentage = 0.35f;  // Max 35% of grid becomes dead-ends (was 0.05f)

        [Header("Advanced")]
        public bool AllowBranching = true;  // Dead-ends can branch
        public bool PreferOuterWalls = false;  // Prefer dead-ends near outer walls
        public bool UseMathematicalDistribution = true;  // Use Poisson distribution for spacing
    }

    /// <summary>
    /// Dead-End Corridor Data Structure
    /// </summary>
    [Serializable]
    public class DeadEndCorridor
    {
        public int StartX;          // Starting X (from main corridor)
        public int StartZ;          // Starting Z (from main corridor)
        public Direction8 Direction;  // Cardinal direction (N, S, E, W)
        public int Length;          // Length in cells
        public int Width;           // Width in cells (usually 1)
        public int EndX;            // Terminal X position
        public int EndZ;            // Terminal Z position
        public DeadEndType Type;    // Type of dead-end
        public bool HasChest;       // Contains chest
        public bool HasEnemy;       // Contains enemy
        public bool HasTrap;        // Contains trap
    }

    /// <summary>
    /// Dead-End Type Classification
    /// </summary>
    public enum DeadEndType
    {
        Simple,         // Basic dead-end (2-3 cells)
        Long,           // Extended dead-end (4-6 cells)
        Branching,      // Dead-end with branches
        Chamber,        // Dead-end ending in small chamber
        Treasure,       // Guaranteed treasure at end
        Combat,         // Guaranteed enemy at end
        Trap            // Guaranteed trap at end
    }

    /// <summary>
    /// Mathematical Dead-End Corridor System
    /// 
    /// Features:
    /// - Difficulty-scaled density (level-based)
    /// - Mathematical distribution (Poisson for spacing)
    /// - Built-in corridor termination detection
    /// - Config-driven parameters (JSON)
    /// - Cardinal-only passages
    /// 
    /// Usage:
    ///   var system = new DeadEndCorridorSystem();
    ///   var corridors = system.Generate(mazeData, config, level, rng);
    /// </summary>
    public sealed class DeadEndCorridorSystem
    {
        private DeadEndCorridorConfig _config;
        private List<DeadEndCorridor> _generatedCorridors;
        private MazeData8 _mazeData;
        private System.Random _rng;
        private int _level;

        /// <summary>
        /// Generated dead-end corridors
        /// </summary>
        public IReadOnlyList<DeadEndCorridor> GeneratedCorridors => _generatedCorridors?.AsReadOnly() ?? new List<DeadEndCorridor>().AsReadOnly();

        /// <summary>
        /// Total dead-end count
        /// </summary>
        public int TotalCount => _generatedCorridors?.Count ?? 0;

        /// <summary>
        /// Total dead-end cells (including width)
        /// </summary>
        public int TotalCells { get; private set; }

        /// <summary>
        /// Initialize dead-end corridor system
        /// </summary>
        public DeadEndCorridorSystem(DeadEndCorridorConfig config = null)
        {
            _config = config ?? CreateDefaultConfig();
            _generatedCorridors = new List<DeadEndCorridor>();
        }

        /// <summary>
        /// Create default configuration
        /// </summary>
        public static DeadEndCorridorConfig CreateDefaultConfig()
        {
            return new DeadEndCorridorConfig
            {
                BaseDensity = 0.30f,      // 30% base at level 0
                MaxMultiplier = 2.5f,     // 2.5× at max level
                MaxLevel = 39,            // Max level for scaling
                Exponent = 2.0f,          // Power curve (quadratic)
                MinLength = 3,            // Minimum 3 cells
                MaxLength = 8,            // Maximum 8 cells
                CorridorWidth = 1,        // 1 cell = 6m
                ChestChanceAtEnd = 0.4f,  // 40% chest
                EnemyChanceAtEnd = 0.4f,  // 40% enemy
                TrapChanceAtEnd = 0.05f,  // 5% trap
                MaxGridPercentage = 0.35f,// Max 35% of grid
                AllowBranching = true,    // Allow branching
                PreferOuterWalls = false, // No outer wall preference
                UseMathematicalDistribution = false // Use uniform distribution
            };
        }

        /// <summary>
        /// Generate dead-end corridors for maze
        /// </summary>
        public List<DeadEndCorridor> Generate(MazeData8 mazeData, int level, System.Random rng, DeadEndCorridorConfig overrideConfig = null)
        {
            _mazeData = mazeData ?? throw new ArgumentNullException(nameof(mazeData));
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
            _level = level;

            if (overrideConfig != null)
            {
                _config = overrideConfig;
            }

            _generatedCorridors = new List<DeadEndCorridor>();
            TotalCells = 0;

            // Calculate scaled density using power curve formula
            // Formula: BaseDensity × Lerp(1.0, MaxMultiplier, t^Exponent)
            // Where t = level / MaxLevel (39)
            float spawnDensity = CalculateScaledDensity(level);
            int maxDeadEnds = CalculateMaxDeadEnds(mazeData);

            Debug.Log($"[DeadEndSystem] LEVEL {level} | Base Density: {_config.BaseDensity:P1} | Scaled Density: {spawnDensity:P1} | Max Dead-Ends: {maxDeadEnds}");

            // Find all valid spawn points (passage cells adjacent to walls)
            var spawnPoints = FindValidSpawnPoints();
            Debug.Log($"[DeadEndSystem] Found {spawnPoints.Count} valid spawn points");

            // Shuffle for random distribution
            Shuffle(spawnPoints);

            // Generate dead-ends using uniform distribution (more reliable)
            GenerateWithUniformDistribution(spawnPoints, spawnDensity, maxDeadEnds);

            Debug.Log($"[DeadEndSystem] Generated {TotalCount} dead-end corridors, {TotalCells} total cells");

            return _generatedCorridors;
        }

        /// <summary>
        /// Calculate scaled density based on level difficulty
        /// Formula: BaseDensity × Lerp(1.0, MaxMultiplier, t^Exponent)
        /// Where t = level / MaxLevel
        /// </summary>
        public float CalculateScaledDensity(int level)
        {
            if (level <= 0) return _config.BaseDensity;

            float t = Mathf.Clamp01((float)level / _config.MaxLevel);
            float curved = Mathf.Pow(t, _config.Exponent);
            float multiplier = Mathf.Lerp(1.0f, _config.MaxMultiplier, curved);

            return Mathf.Clamp01(_config.BaseDensity * multiplier);
        }

        /// <summary>
        /// Calculate maximum dead-ends based on grid size
        /// </summary>
        private int CalculateMaxDeadEnds(MazeData8 mazeData)
        {
            int totalCells = mazeData.Width * mazeData.Height;
            int maxByPercentage = Mathf.RoundToInt(totalCells * _config.MaxGridPercentage);
            int maxByLength = Mathf.RoundToInt(totalCells / (_config.MinLength * _config.CorridorWidth));

            return Mathf.Min(maxByPercentage, maxByLength);
        }

        /// <summary>
        /// Find all valid spawn points for dead-end corridors
        /// A valid spawn point is a passage cell adjacent to at least one wall cell
        /// </summary>
        private List<(int x, int z)> FindValidSpawnPoints()
        {
            var spawnPoints = new List<(int x, int z)>();

            for (int x = 0; x < _mazeData.Width; x++)
            {
                for (int z = 0; z < _mazeData.Height; z++)
                {
                    var cell = _mazeData.GetCell(x, z);

                    // Must be a passage (no walls)
                    bool isPassage = (cell & CellFlags8.AllWalls) == CellFlags8.None;
                    if (!isPassage) continue;

                    // Cannot be spawn or exit
                    bool isSpawn = (cell & CellFlags8.SpawnRoom) != CellFlags8.None;
                    bool isExit = (cell & CellFlags8.IsExit) != CellFlags8.None;
                    if (isSpawn || isExit) continue;

                    // Check if adjacent to at least one wall (valid for dead-end extension)
                    if (HasAdjacentWall(x, z))
                    {
                        spawnPoints.Add((x, z));
                    }
                }
            }

            return spawnPoints;
        }

        /// <summary>
        /// Check if cell has adjacent wall in any cardinal direction
        /// </summary>
        private bool HasAdjacentWall(int x, int z)
        {
            var cardinalDirs = new Direction8[] { Direction8.N, Direction8.S, Direction8.E, Direction8.W };

            foreach (var dir in cardinalDirs)
            {
                var (dx, dz) = Direction8Helper.ToOffset(dir);
                int nx = x + dx;
                int nz = z + dz;

                if (!_mazeData.InBounds(nx, nz)) continue;

                var neighborCell = _mazeData.GetCell(nx, nz);
                bool isWall = (neighborCell & CellFlags8.AllWalls) == CellFlags8.AllWalls;

                if (isWall) return true;
            }

            return false;
        }

        /// <summary>
        /// Generate dead-ends using Poisson distribution for natural spacing
        /// </summary>
        private void GenerateWithPoissonDistribution(List<(int x, int z)> spawnPoints, float density, int maxCount)
        {
            int attempts = 0;
            int maxAttempts = spawnPoints.Count * 3;  // Allow multiple passes

            while (_generatedCorridors.Count < maxCount && attempts < maxAttempts)
            {
                attempts++;

                // Pick random spawn point
                int spawnIndex = _rng.Next(spawnPoints.Count);
                var (spawnX, spawnZ) = spawnPoints[spawnIndex];

                // Try to create dead-end from this point
                if (TryCreateDeadEnd(spawnX, spawnZ, out DeadEndCorridor corridor))
                {
                    _generatedCorridors.Add(corridor);
                    TotalCells += corridor.Length * corridor.Width;

                    // Mark cells as dead-end (for future reference)
                    MarkDeadEndCells(corridor);
                }
            }
        }

        /// <summary>
        /// Generate dead-ends using uniform distribution
        /// Tries ALL directions from each spawn point to fill maze with corridors
        /// </summary>
        private void GenerateWithUniformDistribution(List<(int x, int z)> spawnPoints, float density, int maxCount)
        {
            int totalAttempts = 0;
            int maxAttempts = spawnPoints.Count * 4;  // 4 directions per spawn point

            foreach (var (spawnX, spawnZ) in spawnPoints)
            {
                if (_generatedCorridors.Count >= maxCount) break;
                if (totalAttempts >= maxAttempts) break;

                // Use density as spawn chance for this spawn point
                if (_rng.NextDouble() > density) continue;

                // Try to create dead-ends in ALL 4 directions from this spawn point
                TryCreateDeadEndsAllDirections(spawnX, spawnZ);
                
                totalAttempts++;
            }
        }

        /// <summary>
        /// Try to create dead-end corridors in ALL 4 directions from spawn point
        /// This fills the maze with many branching corridors
        /// </summary>
        private void TryCreateDeadEndsAllDirections(int startX, int startZ)
        {
            // Try all 4 cardinal directions
            var dirs = new Direction8[] { Direction8.N, Direction8.S, Direction8.E, Direction8.W };

            foreach (var dir in dirs)
            {
                // Check if we've hit max count
                if (_generatedCorridors.Count >= _config.MaxGridPercentage * _mazeData.Width * _mazeData.Height / _config.MinLength)
                {
                    return;
                }

                // Try to carve in this direction
                if (TryCarveDeadEnd(startX, startZ, dir, out DeadEndCorridor corridor))
                {
                    _generatedCorridors.Add(corridor);
                    TotalCells += corridor.Length * corridor.Width;
                    MarkDeadEndCells(corridor);
                }
            }
        }

        /// <summary>
        /// Try to create a dead-end corridor from spawn point
        /// </summary>
        private bool TryCreateDeadEnd(int startX, int startZ, out DeadEndCorridor corridor)
        {
            corridor = null;

            // Get random cardinal direction
            var dirs = new Direction8[] { Direction8.N, Direction8.S, Direction8.E, Direction8.W };
            Shuffle(dirs);

            foreach (var dir in dirs)
            {
                if (TryCarveDeadEnd(startX, startZ, dir, out corridor))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Try to carve dead-end corridor in specific direction
        /// </summary>
        private bool TryCarveDeadEnd(int startX, int startZ, Direction8 direction, out DeadEndCorridor corridor)
        {
            corridor = null;

            var (dx, dz) = Direction8Helper.ToOffset(direction);

            // Determine corridor length (mathematical distribution)
            int targetLength = _rng.Next(_config.MinLength, _config.MaxLength + 1);
            int actualLength = 0;

            // Carve corridor
            int currX = startX;
            int currZ = startZ;

            for (int i = 0; i < targetLength; i++)
            {
                int nextX = currX + dx;
                int nextZ = currZ + dz;

                // Check bounds
                if (!_mazeData.InBounds(nextX, nextZ)) break;

                // Check if cell is wall (can be carved)
                var cell = _mazeData.GetCell(nextX, nextZ);
                bool isWall = (cell & CellFlags8.AllWalls) == CellFlags8.AllWalls;

                if (!isWall)
                {
                    // Hit existing passage, stop
                    break;
                }

                // Carve this cell
                _mazeData.SetCell(nextX, nextZ, CellFlags8.None);
                actualLength++;
                currX = nextX;
                currZ = nextZ;
            }

            // Validate minimum length
            if (actualLength < _config.MinLength)
            {
                return false;
            }

            // Create dead-end corridor data
            corridor = new DeadEndCorridor
            {
                StartX = startX,
                StartZ = startZ,
                Direction = direction,
                Length = actualLength,
                Width = _config.CorridorWidth,
                EndX = currX,
                EndZ = currZ,
                Type = ClassifyDeadEndType(actualLength),
                HasChest = false,
                HasEnemy = false,
                HasTrap = false
            };

            // Place objects at dead-end
            PlaceObjectsAtDeadEnd(corridor);

            return true;
        }

        /// <summary>
        /// Classify dead-end type based on properties
        /// </summary>
        private DeadEndType ClassifyDeadEndType(int length)
        {
            if (length >= 6) return DeadEndType.Long;
            if (length <= 2) return DeadEndType.Simple;
            return DeadEndType.Simple;
        }

        /// <summary>
        /// Place objects (chest, enemy, trap) at dead-end terminus
        /// </summary>
        private void PlaceObjectsAtDeadEnd(DeadEndCorridor corridor)
        {
            float roll = (float)_rng.NextDouble();

            if (roll < _config.ChestChanceAtEnd)
            {
                corridor.HasChest = true;
                _mazeData.AddFlag(corridor.EndX, corridor.EndZ, CellFlags8.HasChest);
                corridor.Type = DeadEndType.Treasure;
            }
            else if (roll < _config.ChestChanceAtEnd + _config.EnemyChanceAtEnd)
            {
                corridor.HasEnemy = true;
                _mazeData.AddFlag(corridor.EndX, corridor.EndZ, CellFlags8.HasEnemy);
                corridor.Type = DeadEndType.Combat;
            }
            else if (roll < _config.ChestChanceAtEnd + _config.EnemyChanceAtEnd + _config.TrapChanceAtEnd)
            {
                corridor.HasTrap = true;
                // Note: Trap flag would need to be added to CellFlags8 if not exists
                corridor.Type = DeadEndType.Trap;
            }
        }

        /// <summary>
        /// Mark cells as dead-end for future reference
        /// </summary>
        private void MarkDeadEndCells(DeadEndCorridor corridor)
        {
            var (dx, dz) = Direction8Helper.ToOffset(corridor.Direction);

            for (int i = 0; i < corridor.Length; i++)
            {
                int x = corridor.StartX + dx * (i + 1);
                int z = corridor.StartZ + dz * (i + 1);

                if (!_mazeData.InBounds(x, z)) break;

                // Could add a DeadEnd flag to CellFlags8 if needed
                // For now, just track in the corridor list
            }
        }

        /// <summary>
        /// Fisher-Yates shuffle
        /// </summary>
        private void Shuffle<T>(T[] arr)
        {
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
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// <summary>
        /// Get statistics about generated dead-ends
        /// </summary>
        public DeadEndStatistics GetStatistics()
        {
            return new DeadEndStatistics
            {
                TotalCount = TotalCount,
                TotalCells = TotalCells,
                AvgLength = _generatedCorridors.Count > 0 ?
                    _generatedCorridors.Average(c => c.Length) : 0,
                TreasureCount = _generatedCorridors.Count(c => c.HasChest),
                CombatCount = _generatedCorridors.Count(c => c.HasEnemy),
                TrapCount = _generatedCorridors.Count(c => c.HasTrap),
                SimpleCount = _generatedCorridors.Count(c => c.Type == DeadEndType.Simple),
                LongCount = _generatedCorridors.Count(c => c.Type == DeadEndType.Long)
            };
        }
    }

    /// <summary>
    /// Dead-end generation statistics
    /// </summary>
    [Serializable]
    public class DeadEndStatistics
    {
        public int TotalCount;
        public int TotalCells;
        public double AvgLength;
        public int TreasureCount;
        public int CombatCount;
        public int TrapCount;
        public int SimpleCount;
        public int LongCount;

        public override string ToString()
        {
            return $"Dead-Ends: {TotalCount} | Cells: {TotalCells} | Avg Len: {AvgLength:F1} | " +
                   $"Treasure: {TreasureCount} | Combat: {CombatCount} | Traps: {TrapCount}";
        }
    }
}
