// Copyright (C) 2026 Ocxyde
//
// This file is part of PeuImporte.
//
// PeuImporte is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PeuImporte is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PeuImporte.  If not, see <https://www.gnu.org/licenses/>.
// MazeSetupHelper.cs
// ️ DEPRECATED - LEGACY SETUP HELPER ️
// Editor helper to setup LEGACY maze generation system
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// ️ WARNING: This helper uses DEPRECATED components!
// For NEW development, use CompleteMazeBuilder + specialized placers instead.
//
// Usage: Create empty GameObject, add this script, run "Setup Complete Maze System"
// NOTE: Only use for legacy system support. New projects should use CompleteMazeBuilder.

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// Suppress obsolete warnings - this file intentionally helps with legacy system setup
#pragma warning disable CS0618

namespace Code.Lavos.Core
{
    /// <summary>
    /// ️ DEPRECATED - MazeSetupHelper - Legacy setup helper.
    /// For new development, use CompleteMazeBuilder + specialized placers.
    /// </summary>
    [ExecuteInEditMode]
    public class MazeSetupHelper : MonoBehaviour
    {
        [ContextMenu("Setup Complete Maze System (LEGACY)")]
        public void SetupCompleteMaze()
        {
            Debug.Log("=== Setting Up LEGACY Maze System ===");
            Debug.Log("️  WARNING: This uses DEPRECATED components!");
            Debug.Log(" For new development, use CompleteMazeBuilder instead");
            Debug.Log("");

            // Add all required LEGACY components
            AddOrGetComponent<MazeGenerator>();
            AddOrGetComponent<RoomGenerator>();
            AddOrGetComponent<DoorHolePlacer>();
            AddOrGetComponent<RoomDoorPlacer>();
            // MazeRenderer - DEPRECATED (geometry handled by CompleteMazeBuilder)
            AddOrGetComponent<TorchPool>();
            AddOrGetComponent<SeedManager>();
            AddOrGetComponent<DrawingPool>();

            // Add integration component last
            var integration = AddOrGetComponent<MazeIntegration>();

            Debug.Log("=== All LEGACY Components Added ===");
            Debug.Log("️ This is for LEGACY system support only");
            Debug.Log("️ For new projects, use CompleteMazeBuilder");

            LogSetupSummary();
        }

        [ContextMenu("Configure Default Settings (LEGACY)")]
        public void ConfigureDefaultSettings()
        {
            Debug.Log("=== Configuring LEGACY Default Settings ===");

            // Configure MazeGenerator
            var mazeGen = GetComponent<MazeGenerator>();
            if (mazeGen != null)
            {
                mazeGen.width = 21;
                mazeGen.height = 21;
                Debug.Log(" MazeGenerator: 21x21");
            }

            // Configure RoomGenerator
            var roomGen = GetComponent<RoomGenerator>();
            if (roomGen != null)
            {
                Debug.Log(" RoomGenerator: Default settings");
            }

            // Configure DoorHolePlacer (legacy component)
            var holePlacer = GetComponent<DoorHolePlacer>();
            if (holePlacer != null)
            {
                // Legacy properties - component is deprecated
                Debug.Log(" DoorHolePlacer configured (legacy)");
            }

            // Configure RoomDoorPlacer (legacy component)
            var doorPlacer = GetComponent<RoomDoorPlacer>();
            if (doorPlacer != null)
            {
                // Legacy properties - component is deprecated
                Debug.Log(" RoomDoorPlacer configured (legacy)");
            }

            // Configure MazeIntegration (legacy orchestrator)
            var integration = GetComponent<MazeIntegration>();
            if (integration != null)
            {
                integration.AutoGenerateOnStart = true;
                integration.UseRandomSeed = true;
                integration.GenerateRooms = true;
                integration.GenerateDoors = true;
                Debug.Log(" MazeIntegration: Auto-generate enabled (legacy)");
            }

            Debug.Log("=== LEGACY Components Configured ===");
            Debug.Log("️  Consider migrating to CompleteMazeBuilder for new projects");
        }

        [ContextMenu("Add Wall Texture Sets (DEPRECATED)")]
        public void AddWallTextureSets()
        {
            //  DEPRECATED - WallTextureSet is no longer used
            Debug.LogWarning("[MazeSetupHelper] ️ AddWallTextureSets is deprecated");
            Debug.Log(" Tip: Textures are now loaded from GameConfig-default.json");
        }

        [ContextMenu("Verify Complete Setup (LEGACY)")]
        public void VerifyCompleteSetup()
        {
            Debug.Log("=== Verifying LEGACY Maze Setup ===");

            bool allGood = true;

            // Check required LEGACY components
            string[] requiredComponents = {
                "MazeGenerator",
                "RoomGenerator",
                "DoorHolePlacer",
                "RoomDoorPlacer",
                "MazeIntegration",
                "TorchPool",
                "SeedManager",
                "DrawingPool"
            };

            foreach (var componentName in requiredComponents)
            {
                if (GetComponent(componentName) == null)
                {
                    Debug.LogError($" {componentName} MISSING!");
                    allGood = false;
                }
                else
                {
                    Debug.Log($" {componentName} found");
                }
            }

            // MazeRenderer is deprecated
            Debug.Log("️  MazeRenderer check skipped (deprecated)");

            Debug.Log("=== Verification Complete ===");

            if (allGood)
            {
                Debug.Log(" All LEGACY components present");
                Debug.Log("️ Press Play to generate maze");
            }
            else
            {
                Debug.LogError(" Some components missing! Run 'Setup Complete Maze System'");
            }
        }

        private T AddOrGetComponent<T>() where T : Component
        {
            T component = GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
                Debug.Log($" Added {typeof(T).Name}");
            }
            else
            {
                Debug.Log($" {typeof(T).Name} already present");
            }
            return component;
        }

        private void LogSetupSummary()
        {
            Debug.Log("\n=== Setup Summary (LEGACY) ===");
            Debug.Log("️  This helper uses DEPRECATED components!");
            Debug.Log(" For NEW development, use:");
            Debug.Log("   - CompleteMazeBuilder (maze generation)");
            Debug.Log("   - ChestPlacer, EnemyPlacer, ItemPlacer, TorchPlacer (objects)");
            Debug.Log("   - All values from GameConfig-default.json");
            Debug.Log("======================\n");
        }
    }
}
#endif
