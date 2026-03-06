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
// MazeLightingConfig.cs
// Dynamic lighting configuration based on maze difficulty level
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// LIGHTING PROGRESSION:
// - Maze 01 (Easy): 100% torches, 100% range (very bright)
// - Maze 05 (Medium): 75% torches, 85% range (moderate)
// - Maze 10 (Hard): 50% torches, 70% range (dark)
// - Maze 15 (Expert): 25% torches, 50% range (very dark)
// - Maze 20+ (Master): 15% torches, 40% range (player needs own light)
//
// Location: Assets/Scripts/Core/08_Environment/

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// MazeLightingConfig - Dynamic lighting based on maze difficulty.
    /// Provides lighting ratios for torch count, range, and intensity.
    /// </summary>
    [System.Serializable]
    public class MazeLightingConfig
    {
        #region Inspector Settings
        
        [Header("Base Settings")]
        [Tooltip("Maximum torch count at level 1")]
        [SerializeField] private int maxTorchCount = 60;
        
        [Tooltip("Maximum light range at level 1")]
        [SerializeField] private float maxLightRange = 12f;
        
        [Tooltip("Maximum light intensity at level 1")]
        [SerializeField] private float maxLightIntensity = 1.8f;
        
        [Header("Difficulty Curve")]
        [Tooltip("How fast lighting decreases (1 = linear, 2 = quadratic)")]
        [Range(0.5f, 3f)]
        [SerializeField] private float difficultyExponent = 1.5f;
        
        [Tooltip("Minimum lighting ratio (even at max level)")]
        [Range(0.05f, 0.5f)]
        [SerializeField] private float minimumLightingRatio = 0.15f;
        
        [Header("Level Range")]
        [Tooltip("Maximum level for lighting calculation")]
        [SerializeField] private int maxLevel = 20;
        
        #endregion
        
        #region Public Properties
        
        public int MaxTorchCount => maxTorchCount;
        public float MaxLightRange => maxLightRange;
        public float MaxLightIntensity => maxLightIntensity;
        public float DifficultyExponent => difficultyExponent;
        public float MinimumLightingRatio => minimumLightingRatio;
        public int MaxLevel => maxLevel;
        
        #endregion
        
        #region Lighting Calculation
        
        /// <summary>
        /// Calculate lighting ratio based on maze level.
        /// Level 1 = 100% lighting, Level 20+ = minimum lighting
        /// </summary>
        /// <param name="mazeLevel">Current maze level (1-20+)</param>
        /// <returns>Lighting ratio (0.15 to 1.0)</returns>
        public float CalculateLightingRatio(int mazeLevel)
        {
            // Clamp level to valid range
            int clampedLevel = Mathf.Clamp(mazeLevel, 1, maxLevel * 2);
            
            // Calculate normalized difficulty (0 to 1)
            float normalizedDifficulty = (float)(clampedLevel - 1) / (maxLevel - 1);
            
            // Apply difficulty curve (exponential decay)
            float lightingRatio = Mathf.Pow(1f - normalizedDifficulty, difficultyExponent);
            
            // Ensure minimum lighting
            lightingRatio = Mathf.Max(lightingRatio, minimumLightingRatio);
            
            return lightingRatio;
        }
        
        /// <summary>
        /// Get torch count for current maze level.
        /// </summary>
        public int GetTorchCountForLevel(int mazeLevel)
        {
            float ratio = CalculateLightingRatio(mazeLevel);
            return Mathf.Max(5, Mathf.RoundToInt(maxTorchCount * ratio));
        }
        
        /// <summary>
        /// Get light range for current maze level.
        /// </summary>
        public float GetLightRangeForLevel(int mazeLevel)
        {
            float ratio = CalculateLightingRatio(mazeLevel);
            return Mathf.Max(3f, maxLightRange * ratio);
        }
        
        /// <summary>
        /// Get light intensity for current maze level.
        /// </summary>
        public float GetLightIntensityForLevel(int mazeLevel)
        {
            float ratio = CalculateLightingRatio(mazeLevel);
            return Mathf.Max(0.5f, maxLightIntensity * ratio);
        }
        
        #endregion
        
        #region Examples
        
        /// <summary>
        /// Print lighting config for debugging.
        /// </summary>
        public void PrintLightingProgression()
        {
            Debug.Log("═══════════════════════════════════════════════");
            Debug.Log("  MAZE LIGHTING PROGRESSION");
            Debug.Log("═══════════════════════════════════════════════");
            Debug.Log($"Base Torch Count: {maxTorchCount}");
            Debug.Log($"Base Light Range: {maxLightRange}m");
            Debug.Log($"Base Intensity: {maxLightIntensity}");
            Debug.Log($"Difficulty Exponent: {difficultyExponent}");
            Debug.Log($"Minimum Ratio: {minimumLightingRatio:P0}");
            Debug.Log("───────────────────────────────────────────────");
            Debug.Log("Level | Ratio | Torches | Range  | Intensity");
            Debug.Log("───────────────────────────────────────────────");
            
            int[] exampleLevels = { 1, 3, 5, 10, 15, 20, 25, 30 };
            foreach (int level in exampleLevels)
            {
                float ratio = CalculateLightingRatio(level);
                int torches = GetTorchCountForLevel(level);
                float range = GetLightRangeForLevel(level);
                float intensity = GetLightIntensityForLevel(level);
                
                Debug.Log($"L{level:D2}  | {ratio:P0}   | {torches:D3}     | {range:F1}m   | {intensity:F2}");
            }
            
            Debug.Log("═══════════════════════════════════════════════");
        }
        
        #endregion
    }
}
