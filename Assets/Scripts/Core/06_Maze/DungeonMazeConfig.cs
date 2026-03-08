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
using Newtonsoft.Json;

namespace Code.Lavos.Core.Advanced
{
    /// <summary>
    /// Configuration for advanced dungeon maze generation.
    /// Can be loaded from JSON or set programmatically.
    /// </summary>
    [System.Serializable]
    public class DungeonMazeConfig
    {
        // ─────────────────────────────────────────────────────────────
        // Core Maze Parameters
        // ─────────────────────────────────────────────────────────────
        [JsonProperty("baseSize")]
        public int BaseSize = 21;

        [JsonProperty("minSize")]
        public int MinSize = 15;

        [JsonProperty("maxSize")]
        public int MaxSize = 51;

        [JsonProperty("cellSize")]
        public float CellSize = 6.0f;

        [JsonProperty("wallHeight")]
        public float WallHeight = 3.0f;

        // ─────────────────────────────────────────────────────────────
        // Room Configuration
        // ─────────────────────────────────────────────────────────────
        [JsonProperty("spawnRoomSize")]
        public int SpawnRoomSize = 2;

        [JsonProperty("exitRoomSize")]
        public int ExitRoomSize = 2;

        [JsonProperty("chamberExpansionRadius")]
        public int ChamberExpansionRadius = 1;

        // ─────────────────────────────────────────────────────────────
        // Danger & Treasure Parameters
        // ─────────────────────────────────────────────────────────────
        [JsonProperty("trapDensity")]
        public float TrapDensity = 0.25f;

        [JsonProperty("trapTypes")]
        public string[] TrapTypes = { "spikes", "lava", "darkness", "poison" };

        [JsonProperty("treasureDensity")]
        public float TreasureDensity = 0.15f;

        [JsonProperty("treasureValueRange")]
        public Vector2Int TreasureValueRange = new Vector2Int(100, 500);

        // ─────────────────────────────────────────────────────────────
        // Corridor & Complexity
        // ─────────────────────────────────────────────────────────────
        [JsonProperty("corridorWindingFactor")]
        public float CorridorWindingFactor = 0.3f;

        [JsonProperty("deadEndExpansionChance")]
        public float DeadEndExpansionChance = 0.6f;

        [JsonProperty("crossroadExpansionChance")]
        public float CrossroadExpansionChance = 0.8f;

        // ─────────────────────────────────────────────────────────────
        // Object Placement
        // ─────────────────────────────────────────────────────────────
        [JsonProperty("torchChance")]
        public float TorchChance = 0.25f;

        [JsonProperty("enemyDensity")]
        public float EnemyDensity = 0.03f;

        [JsonProperty("chestDensity")]
        public float ChestDensity = 0.05f;

        [JsonProperty("bossRoomCount")]
        public int BossRoomCount = 1;

        // ─────────────────────────────────────────────────────────────
        // Difficulty Scaling
        // ─────────────────────────────────────────────────────────────
        [JsonProperty("difficulty")]
        public DifficultyScalerConfig Difficulty = new DifficultyScalerConfig();

        // ─────────────────────────────────────────────────────────────
        // AI Settings
        // ─────────────────────────────────────────────────────────────
        [JsonProperty("aiSettings")]
        public AIAdaptiveSettings AISettings = new AIAdaptiveSettings();

        // ─────────────────────────────────────────────────────────────
        // Advanced Features
        // ─────────────────────────────────────────────────────────────
        [JsonProperty("multiEntranceEnabled")]
        public bool MultiEntranceEnabled = true;

        [JsonProperty("multiExitEnabled")]
        public bool MultiExitEnabled = false;

        [JsonProperty("allowDiagonalWalls")]
        public bool AllowDiagonalWalls = true;

        [JsonProperty("guaranteedPathRequired")]
        public bool GuaranteedPathRequired = true;

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
                var config = JsonConvert.DeserializeObject<DungeonMazeConfig>(jsonText);
                if (config.IsValid())
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
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    /// <summary>
    /// Difficulty scaling configuration.
    /// </summary>
    [System.Serializable]
    public class DifficultyScalerConfig
    {
        [JsonProperty("baseFactor")]
        public float BaseFactor = 1.0f;

        [JsonProperty("factorPerLevel")]
        public float FactorPerLevel = 0.15f;

        [JsonProperty("maxFactor")]
        public float MaxFactor = 5.0f;

        [JsonProperty("sizeGrowthPerLevel")]
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
        [JsonProperty("enableAdaptivity")]
        public bool EnableAdaptivity = true;

        [JsonProperty("minAdaptiveFactor")]
        public float MinAdaptiveFactor = 0.7f;

        [JsonProperty("maxAdaptiveFactor")]
        public float MaxAdaptiveFactor = 1.8f;

        [JsonProperty("pathLengthWeight")]
        public float PathLengthWeight = 0.3f;

        [JsonProperty("trapDensityWeight")]
        public float TrapDensityWeight = 0.25f;

        [JsonProperty("treasureDensityWeight")]
        public float TreasureDensityWeight = 0.2f;

        [JsonProperty("mazeComplexityWeight")]
        public float MazeComplexityWeight = 0.25f;

        [JsonProperty("learnFromPlayerPerformance")]
        public bool LearnFromPlayerPerformance = true;

        [JsonProperty("performanceHistoryLength")]
        public int PerformanceHistoryLength = 5;
    }
}
