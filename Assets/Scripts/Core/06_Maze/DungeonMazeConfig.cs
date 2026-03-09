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
// DungeonMazeConfig.cs
// Configuration for advanced dungeon maze generation
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using UnityEngine;

namespace Code.Lavos.Core.Advanced
{
    /// <summary>
    /// Configuration for advanced dungeon maze generation.
    /// Can be loaded from JSON or set programmatically.
    /// Uses Unity's JsonUtility for serialization (no external dependencies).
    /// </summary>
    [System.Serializable]
    public class DungeonMazeConfig
    {
        // ─────────────────────────────────────────────────────────────
        // Core Maze Parameters
        // ─────────────────────────────────────────────────────────────
        public int BaseSize = 21;

        public int MinSize = 15;

        public int MaxSize = 51;

        public float CellSize = 6.0f;

        public float WallHeight = 3.0f;

        // ─────────────────────────────────────────────────────────────
        // Room Configuration
        // ─────────────────────────────────────────────────────────────
        public int SpawnRoomSize = 2;

        public int ExitRoomSize = 2;

        public int ChamberExpansionRadius = 1;

        // ─────────────────────────────────────────────────────────────
        // Danger & Treasure Parameters
        // ─────────────────────────────────────────────────────────────
        public float TrapDensity = 0.25f;

        public string[] TrapTypes = { "spikes", "lava", "darkness", "poison" };

        public float TreasureDensity = 0.15f;

        public Vector2Int TreasureValueRange = new Vector2Int(100, 500);

        // ─────────────────────────────────────────────────────────────
        // Corridor & Complexity
        // ─────────────────────────────────────────────────────────────
        public float CorridorWindingFactor = 0.3f;

        public float DeadEndExpansionChance = 0.6f;

        public float CrossroadExpansionChance = 0.8f;

        // ─────────────────────────────────────────────────────────────
        // Object Placement
        // ─────────────────────────────────────────────────────────────
        public float TorchChance = 0.25f;

        public float EnemyDensity = 0.03f;

        public float ChestDensity = 0.05f;

        public int BossRoomCount = 1;

        // ─────────────────────────────────────────────────────────────
        // Difficulty Scaling
        // ─────────────────────────────────────────────────────────────
        public DifficultyScalerConfig Difficulty = new DifficultyScalerConfig();

        // ─────────────────────────────────────────────────────────────
        // AI Settings
        // ─────────────────────────────────────────────────────────────
        public AIAdaptiveSettings AISettings = new AIAdaptiveSettings();

        // ─────────────────────────────────────────────────────────────
        // Advanced Features
        // ─────────────────────────────────────────────────────────────
        public bool MultiEntranceEnabled = true;

        public bool MultiExitEnabled = false;

        public bool AllowDiagonalWalls = true;

        public bool GuaranteedPathRequired = true;

        // Passage-First Generation (alternative to DFS)
        // If true, creates clear passage from entrance to exit first,
        // then adds walls around it. Guarantees walkable path.
        public bool UsePassageFirst = false;

        public int BaseWallPenalty { get; }

        // ─────────────────────────────────────────────────────────────
        // Validation & Utilities
        // ─────────────────────────────────────────────────────────────
        public bool IsValid()
        {
            if (BaseSize < MinSize || BaseSize > MaxSize)
                return false;
            if (TrapDensity < 0 || TrapDensity > 1)
                return false;
            if (TreasureDensity < 0 || TreasureDensity > 1)
                return false;
            if (CorridorWindingFactor < 0 || CorridorWindingFactor > 1)
                return false;
            return true;
        }

        public static DungeonMazeConfig CreateDefault()
        {
            return new DungeonMazeConfig();
        }

        public static DungeonMazeConfig LoadFromJson(string jsonText)
        {
            try
            {
                var config = JsonUtility.FromJson<DungeonMazeConfig>(jsonText);
                if (config != null && config.IsValid())
                {
                    return config;
                }
                else
                {
                    Debug.LogWarning("[DungeonConfig] Loaded config is invalid, using defaults");
                    return CreateDefault();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DungeonConfig] Failed to load JSON: {ex.Message}");
                return CreateDefault();
            }
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this, prettyPrint: true);
        }
    }

    /// <summary>
    /// Difficulty scaling configuration.
    /// </summary>
    [System.Serializable]
    public class DifficultyScalerConfig
    {
        public float BaseFactor = 1.0f;

        public float FactorPerLevel = 0.15f;

        public float MaxFactor = 5.0f;

        public int SizeGrowthPerLevel = 2;

        public float Factor(int level)
        {
            return Mathf.Min(BaseFactor + (level * FactorPerLevel), MaxFactor);
        }

        public int MazeSize(int level, int baseSize, int minSize, int maxSize)
        {
            int size = baseSize + (level * SizeGrowthPerLevel);
            return Mathf.Clamp(size, minSize, maxSize);
        }

        public string Describe(int level, DungeonMazeConfig cfg)
        {
            return $"Level {level} | Factor {Factor(level):F2} | " +
                   $"Size {MazeSize(level, cfg.BaseSize, cfg.MinSize, cfg.MaxSize)}x{MazeSize(level, cfg.BaseSize, cfg.MinSize, cfg.MaxSize)}";
        }
    }

    /// <summary>
    /// AI-based adaptive difficulty settings.
    /// </summary>
    [System.Serializable]
    public class AIAdaptiveSettings
    {
        public bool EnableAdaptivity = true;

        public float MinAdaptiveFactor = 0.7f;

        public float MaxAdaptiveFactor = 1.8f;

        public float PathLengthWeight = 0.3f;

        public float TrapDensityWeight = 0.25f;

        public float TreasureDensityWeight = 0.2f;

        public float MazeComplexityWeight = 0.25f;

        public bool LearnFromPlayerPerformance = true;

        public int PerformanceHistoryLength = 5;
    }
}
