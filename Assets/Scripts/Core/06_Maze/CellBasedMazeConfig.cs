// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8 (no BOM) | Line Endings: Unix LF
// Cell-Based Maze Configuration - Plug-in-Out Compliant

using UnityEngine;

namespace Code.Lavos.Core.Maze
{
    /// <summary>
    /// CellBasedMazeConfig - Configuration for cell-based maze generation.
    /// 
    /// Plug-in-Out Compliant:
    /// - Loads from Resources (never creates)
    /// - All values from JSON config
    /// - No hardcoded values
    /// 
    /// Usage:
    /// var config = CellBasedMazeConfig.Load();
    /// float cellSize = config.CellSize;
    /// </summary>
    [CreateAssetMenu(fileName = "CellBasedMazeConfig", menuName = "Lavos/Cell-Based Maze Config")]
    public class CellBasedMazeConfig : ScriptableObject
    {
        // Singleton instance
        private static CellBasedMazeConfig _instance;
        public static CellBasedMazeConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<CellBasedMazeConfig>("Config/CellBasedMazeConfig");
                    if (_instance == null)
                    {
                        Debug.LogWarning("[CellBasedMazeConfig] Config not found, using defaults");
                        _instance = CreateDefaultConfig();
                    }
                }
                return _instance;
            }
        }

        // Maze Settings
        [Header("Maze Settings")]
        public int defaultWidth = 21;
        public int defaultHeight = 21;
        public int minSize = 12;
        public int maxSize = 51;
        public float cellSize = 6.0f;
        public float wallHeight = 4.0f;
        public float wallThickness = 0.3f;
        public bool wallPivotIsAtMeshCenter = true;

        // Difficulty Scaling
        [Header("Difficulty Scaling")]
        public int minLevel = 0;
        public int maxLevel = 39;
        public float decoyDensityMin = 0.20f;
        public float decoyDensityMax = 0.75f;
        public float chestDensityMin = 0.03f;
        public float chestDensityMax = 0.08f;
        public float enemyDensityMin = 0.02f;
        public float enemyDensityMax = 0.06f;
        public float trapDensityMin = 0.01f;
        public float trapDensityMax = 0.04f;

        // Room Settings
        [Header("Room Settings")]
        public int roomSize = 3;
        public int minRooms = 1;
        public int maxRooms = 5;
        public int doorOpeningWidth = 3;

        // Decoy Settings
        [Header("Decoy Settings")]
        [Range(0f, 1f)] public float lShapePercentage = 0.50f;
        [Range(0f, 1f)] public float spiralPercentage = 0.20f;
        [Range(0f, 1f)] public float forkPercentage = 0.20f;
        [Range(0f, 1f)] public float chamberPercentage = 0.10f;
        public int minSegmentLength = 2;
        public int maxSegmentLength = 5;
        [Range(0f, 1f)] public float chestRewardChance = 0.50f;

        // Agreement Settings
        [Header("Agreement Settings")]
        [Range(0f, 1f)] public float fillCorridorChance = 0.30f;
        [Range(0f, 1f)] public float fillRoomChance = 0.10f;
        [Range(0f, 1f)] public float fillDeadEndChance = 0.20f;

        // Player Settings
        [Header("Player Settings")]
        public float spawnOffsetX = 0.5f;
        public float spawnOffsetZ = 0.5f;
        public float playerMass = 70.0f;
        public float playerDrag = 0.1f;
        public float playerAngularDrag = 0.5f;
        public float characterControllerHeight = 2.0f;
        public float characterControllerRadius = 0.5f;
        public float characterControllerStepOffset = 0.3f;

        // Verification Settings
        [Header("Verification Settings")]
        public bool autoVerify = true;
        public bool autoFix = true;

        /// <summary>
        /// Load config from Resources.
        /// </summary>
        public static CellBasedMazeConfig Load()
        {
            return Instance;
        }

        /// <summary>
        /// Create default config (fallback).
        /// </summary>
        public static CellBasedMazeConfig CreateDefaultConfig()
        {
            var config = CreateInstance<CellBasedMazeConfig>();
            config.name = "CellBasedMazeConfig (Default)";
            return config;
        }

        /// <summary>
        /// Get decoy density for level.
        /// </summary>
        public float GetDecoyDensity(int level)
        {
            float t = Mathf.InverseLerp(minLevel, maxLevel, level);
            return Mathf.Lerp(decoyDensityMin, decoyDensityMax, t);
        }

        /// <summary>
        /// Get chest density for level.
        /// </summary>
        public float GetChestDensity(int level)
        {
            float t = Mathf.InverseLerp(minLevel, maxLevel, level);
            return Mathf.Lerp(chestDensityMin, chestDensityMax, t);
        }

        /// <summary>
        /// Get enemy density for level.
        /// </summary>
        public float GetEnemyDensity(int level)
        {
            float t = Mathf.InverseLerp(minLevel, maxLevel, level);
            return Mathf.Lerp(enemyDensityMin, enemyDensityMax, t);
        }

        /// <summary>
        /// Get trap density for level.
        /// </summary>
        public float GetTrapDensity(int level)
        {
            float t = Mathf.InverseLerp(minLevel, maxLevel, level);
            return Mathf.Lerp(trapDensityMin, trapDensityMax, t);
        }

        /// <summary>
        /// Get room count for level.
        /// </summary>
        public int GetRoomCount(int level)
        {
            float t = Mathf.InverseLerp(minLevel, maxLevel, level);
            return Mathf.RoundToInt(Mathf.Lerp(minRooms, maxRooms, t));
        }
    }
}
