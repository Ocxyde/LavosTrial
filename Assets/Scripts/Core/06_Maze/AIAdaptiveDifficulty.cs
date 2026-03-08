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
// AIAdaptiveDifficulty.cs
// AI system for computing and adapting maze difficulty dynamically
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core.Advanced
{
    /// <summary>
    /// AI-based adaptive difficulty system.
    /// 
    /// Analyzes generated maze characteristics and computes a difficulty
    /// multiplier that reflects:
    /// - Path length (longer = harder)
    /// - Trap density (more traps = harder)
    /// - Treasure presence (guarded treasures = harder)
    /// - Maze complexity (dead-ends, crossroads = harder)
    /// - Boss room presence (bosses = significantly harder)
    /// 
    /// Can learn from player performance over multiple runs.
    /// </summary>
    public sealed class AIAdaptiveDifficulty
    {
        private int _level;
        private AIAdaptiveSettings _settings;
        private List<float> _performanceHistory;
        private float _computedDifficultyMultiplier = 1.0f;

        public float ComputedDifficultyMultiplier => _computedDifficultyMultiplier;

        public AIAdaptiveDifficulty(int level, AIAdaptiveSettings settings)
        {
            _level = level;
            _settings = settings ?? new AIAdaptiveSettings();
            _performanceHistory = new List<float>();
        }

        /// <summary>
        /// Analyze maze characteristics and compute difficulty multiplier.
        /// Called after maze generation to reflect actual maze complexity.
        /// </summary>
        public void AnalyzeMaze(DungeonMazeData mazeData, int trapRoomCount, int treasureRoomCount)
        {
            if (!_settings.EnableAdaptivity)
            {
                _computedDifficultyMultiplier = 1.0f;
                return;
            }

            // Compute individual metrics
            float pathLengthFactor = ComputePathLengthFactor(mazeData);
            float trapDensityFactor = ComputeTrapDensityFactor(mazeData, trapRoomCount);
            float treasureFactor = ComputeTreasureFactor(mazeData, treasureRoomCount);
            float complexityFactor = ComputeMazeComplexityFactor(mazeData);

            // Weighted combination
            float baseDifficulty =
                (pathLengthFactor * _settings.PathLengthWeight) +
                (trapDensityFactor * _settings.TrapDensityWeight) +
                (treasureFactor * _settings.TreasureDensityWeight) +
                (complexityFactor * _settings.MazeComplexityWeight);

            // Normalize and clamp
            _computedDifficultyMultiplier = Mathf.Clamp(
                baseDifficulty,
                _settings.MinAdaptiveFactor,
                _settings.MaxAdaptiveFactor
            );

            Debug.Log($"[AI Difficulty] Level {_level} computed factor: {_computedDifficultyMultiplier:F2}");
        }

        /// <summary>
        /// Compute factor based on path length from spawn to exit.
        /// Longer paths = higher difficulty.
        /// </summary>
        private float ComputePathLengthFactor(DungeonMazeData mazeData)
        {
            int pathLength = EstimatePathLength(mazeData);
            int mazeSize = mazeData.Width + mazeData.Height;

            // Normalize: typical path is ~2x maze size
            float normalizedPath = (float)pathLength / (mazeSize * 1.5f);
            return Mathf.Clamp01(normalizedPath);
        }

        private int EstimatePathLength(DungeonMazeData mazeData)
        {
            var spawn = mazeData.SpawnCell;
            var exit = mazeData.ExitCell;

            // Manhattan distance as estimate
            return Mathf.Abs(exit.x - spawn.x) + Mathf.Abs(exit.z - spawn.z);
        }

        /// <summary>
        /// Compute factor based on trap density.
        /// More traps = higher difficulty.
        /// </summary>
        private float ComputeTrapDensityFactor(DungeonMazeData mazeData, int trapCount)
        {
            int totalCells = mazeData.Width * mazeData.Height;
            float trapDensity = trapCount > 0 ? (float)trapCount / totalCells : 0.0f;

            // Expected density ~0.25
            float normalizedDensity = trapDensity / 0.25f;
            return Mathf.Clamp01(normalizedDensity);
        }

        /// <summary>
        /// Compute factor based on treasure presence and guarding difficulty.
        /// More treasures guarded by strong enemies = higher difficulty.
        /// </summary>
        private float ComputeTreasureFactor(DungeonMazeData mazeData, int treasureCount)
        {
            int totalCells = mazeData.Width * mazeData.Height;
            float treasureDensity = treasureCount > 0 ? (float)treasureCount / totalCells : 0.0f;

            // Expected density ~0.15, guarded treasures increase challenge
            float normalizedTreasure = treasureDensity / 0.15f;
            return Mathf.Clamp01(normalizedTreasure * 1.3f); // 30% boost for guard difficulty
        }

        /// <summary>
        /// Compute factor based on maze complexity.
        /// More dead-ends and crossroads = higher difficulty.
        /// </summary>
        private float ComputeMazeComplexityFactor(DungeonMazeData mazeData)
        {
            int deadEndCount = 0;
            int crossroadCount = 0;

            for (int x = 0; x < mazeData.Width; x++)
            {
                for (int z = 0; z < mazeData.Height; z++)
                {
                    int openCount = CountOpenDirections(mazeData, x, z);

                    if (openCount == 1)
                        deadEndCount++;
                    else if (openCount >= 3)
                        crossroadCount++;
                }
            }

            int totalCells = mazeData.Width * mazeData.Height;
            float complexityScore = ((float)deadEndCount * 0.5f + (float)crossroadCount * 1.5f) / totalCells;

            return Mathf.Clamp01(complexityScore * 2.0f);
        }

        private int CountOpenDirections(DungeonMazeData mazeData, int x, int z)
        {
            int count = 0;

            Direction8[] cardinals = { Direction8.N, Direction8.S, Direction8.E, Direction8.W };
            foreach (var dir in cardinals)
            {
                if (!mazeData.HasWall(x, z, dir))
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Record player performance for this level.
        /// Used for learning adjustments in future runs.
        /// </summary>
        public void RecordPerformance(float performanceScore)
        {
            if (!_settings.LearnFromPlayerPerformance)
                return;

            _performanceHistory.Add(performanceScore);

            // Keep only recent history
            while (_performanceHistory.Count > _settings.PerformanceHistoryLength)
            {
                _performanceHistory.RemoveAt(0);
            }

            Debug.Log($"[AI Learning] Recorded performance: {performanceScore:F2}");
        }

        /// <summary>
        /// Get adjustment factor based on player performance history.
        /// Can be used for next maze generation.
        /// </summary>
        public float GetAdaptiveAdjustment()
        {
            if (_performanceHistory.Count == 0)
                return 1.0f;

            float averagePerformance = 0;
            foreach (var score in _performanceHistory)
            {
                averagePerformance += score;
            }
            averagePerformance /= _performanceHistory.Count;

            // If player is struggling (low scores), reduce difficulty
            // If player is dominating (high scores), increase difficulty
            if (averagePerformance < 0.4f)
                return 0.85f; // Reduce by 15%
            else if (averagePerformance > 0.8f)
                return 1.15f; // Increase by 15%
            else
                return 1.0f; // Maintain current difficulty
        }

        /// <summary>
        /// Describe the computed difficulty in human-readable form.
        /// </summary>
        public string Describe()
        {
            string baseDifficulty = _computedDifficultyMultiplier switch
            {
                < 0.8f => "Very Easy",
                < 1.0f => "Easy",
                < 1.2f => "Normal",
                < 1.5f => "Hard",
                < 1.8f => "Very Hard",
                _ => "Insane",
            };

            return $"[AI] Difficulty: {baseDifficulty} (Factor: {_computedDifficultyMultiplier:F2})";
        }
    }

    /// <summary>
    /// Player performance tracker for AI learning.
    /// </summary>
    [System.Serializable]
    public sealed class PlayerPerformanceMetrics
    {
        public int Level { get; set; }
        public float CompletionTime { get; set; }
        public int TrapsDodged { get; set; }
        public int EnemiesDefeated { get; set; }
        public int TreasuresCollected { get; set; }
        public bool ReachedExit { get; set; }
        public float DamageTaken { get; set; }

        /// <summary>
        /// Compute a normalized score (0-1) representing player performance.
        /// </summary>
        public float ComputePerformanceScore()
        {
            float score = 0.0f;

            if (ReachedExit)
                score += 0.4f;

            // Time efficiency (faster is better)
            float timeEfficiency = Mathf.Clamp01(1.0f - (CompletionTime / 600.0f)); // 600s = max
            score += timeEfficiency * 0.2f;

            // Combat effectiveness
            float combatScore = Mathf.Clamp01((float)EnemiesDefeated / 10.0f);
            score += combatScore * 0.2f;

            // Survivability (less damage is better)
            float survivalScore = Mathf.Clamp01(1.0f - (DamageTaken / 100.0f));
            score += survivalScore * 0.2f;

            return score;
        }
    }
}
