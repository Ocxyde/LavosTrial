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
// TestDeadEndConfig.cs
// Temporary debug script to verify config values
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// TestDeadEndConfig.cs
// Temporary debug script to verify config values
// DELETE THIS AFTER TESTING
// NOTE: This is an Editor script - must be in Editor folder or have UNITY_EDITOR define

using UnityEngine;
using Code.Lavos.Core;

#if UNITY_EDITOR
using UnityEditor;

namespace Code.Lavos.Core
{
    public class TestDeadEndConfig : MonoBehaviour
    {
        [MenuItem("Tools/DEBUG: Check Dead-End Config")]
        public static void CheckConfig()
        {
            Debug.Log("=== DEAD-END CONFIG CHECK ===");

            // Create default config
            var config = DeadEndCorridorSystem.CreateDefaultConfig();

            Debug.Log($"BaseDensity: {config.BaseDensity} (should be 0.30)");
            Debug.Log($"MaxGridPercentage: {config.MaxGridPercentage} (should be 0.35)");
            Debug.Log($"MinLength: {config.MinLength} (should be 3)");
            Debug.Log($"MaxLength: {config.MaxLength} (should be 8)");
            Debug.Log($"AllowBranching: {config.AllowBranching} (should be true)");

            if (config.BaseDensity < 0.5f)
            {
                Debug.LogWarning($"BaseDensity is {config.BaseDensity:P1} (expected 30% base, scales to 75% at level 39)");
                Debug.Log("Check: Config/DeadEndCorridorConfig.json");
            }
            else
            {
                Debug.Log(" CONFIG CORRECT!");
            }

            Debug.Log("=== END CHECK ===");
        }
    }
}
#endif
