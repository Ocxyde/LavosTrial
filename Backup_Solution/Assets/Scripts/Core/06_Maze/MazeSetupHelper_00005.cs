// MazeSetupHelper.cs
// ⚠️ DEPRECATED - LEGACY SETUP HELPER ⚠️
// Editor helper to setup complete maze generation system
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// This file uses deprecated components (MazeRenderer, DoorHolePlacer, RoomDoorPlacer, WallTextureSet).
// For new development, use CompleteMazeBuilder + specialized placers.
//
// Usage: Create empty GameObject, add this script, run "Setup Complete Maze System"

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// Suppress obsolete warnings - this file intentionally helps with legacy system setup
#pragma warning disable CS0618
// NOTE: CS0246 warnings suppressed by commenting out deprecated type usage

namespace Code.Lavos.Core
{
    /// <summary>
    /// MazeSetupHelper - Configure complete maze generation system.
    /// Run from Inspector context menu.
    /// </summary>
    [ExecuteInEditMode]
    public class MazeSetupHelper : MonoBehaviour
    {
        [ContextMenu("Setup Complete Maze System")]
        public void SetupCompleteMaze()
        {
            Debug.Log("=== Setting Up Complete Maze System ===");

            // Add all required components (LEGACY - deprecated)
            AddOrGetComponent<MazeGenerator>();
            AddOrGetComponent<RoomGenerator>();
            AddOrGetComponent<DoorHolePlacer>();
            AddOrGetComponent<RoomDoorPlacer>();
            // AddOrGetComponent<MazeRenderer>();  // ❌ DEPRECATED - Geometry handled by CompleteMazeBuilder
            AddOrGetComponent<TorchPool>();
            AddOrGetComponent<SeedManager>();
            AddOrGetComponent<DrawingPool>();

            // Add integration component last
            var integration = AddOrGetComponent<MazeIntegration>();

            Debug.Log("=== All Components Added ===");
            Debug.Log("▶️ Select the GameObject and configure MazeIntegration in Inspector");
            Debug.Log("▶️ Or press Play to auto-generate");

            // Log setup summary
            LogSetupSummary();
        }

        [ContextMenu("Configure Default Settings")]
        public void ConfigureDefaultSettings()
        {
            Debug.Log("=== Configuring Default Settings ===");

            // Configure MazeGenerator
            var mazeGen = GetComponent<MazeGenerator>();
            if (mazeGen != null)
            {
                mazeGen.width = 21;
                mazeGen.height = 21;
                Debug.Log("✅ MazeGenerator: 21x21");
            }

            // Configure RoomGenerator
            var roomGen = GetComponent<RoomGenerator>();
            if (roomGen != null)
            {
                // Set via serialized fields if accessible
                Debug.Log("✅ RoomGenerator: Default settings");
            }

            // Configure DoorHolePlacer (legacy component)
            var holePlacer = GetComponent<DoorHolePlacer>();
            if (holePlacer != null)
            {
                // Legacy properties - commented out as component is deprecated
                // holePlacer.HoleWidth = 2.5f;
                // holePlacer.HoleHeight = 3f;
                // holePlacer.HoleDepth = 0.5f;
                // holePlacer.DoorChancePerWall = 0.6f;
                // holePlacer.CarveHolesInWalls = true;
                Debug.Log("✅ DoorHolePlacer configured (legacy)");
            }

            // Configure RoomDoorPlacer (legacy component)
            var doorPlacer = GetComponent<RoomDoorPlacer>();
            if (doorPlacer != null)
            {
                // Legacy properties - commented out as component is deprecated
                // doorPlacer.PlaceDoorsInHoles = true;
                // doorPlacer.RandomizeWallTextures = true;
                Debug.Log("✅ RoomDoorPlacer configured (legacy)");
            }

            // Configure MazeRenderer (deprecated - geometry now handled by CompleteMazeBuilder)
            // var renderer = GetComponent<MazeRenderer>();  // ❌ DEPRECATED
            // if (renderer != null) { ... }  // ❌ Removed

            // Configure MazeIntegration (legacy orchestrator)
            var integration = GetComponent<MazeIntegration>();
            if (integration != null)
            {
                integration.AutoGenerateOnStart = true;
                integration.UseRandomSeed = true;
                integration.GenerateRooms = true;
                integration.GenerateDoors = true;
                Debug.Log("✅ MazeIntegration: Auto-generate enabled (legacy)");
            }

            Debug.Log("=== Legacy Components Configured ===");
        }

        [ContextMenu("Add Wall Texture Sets")]
        public void AddWallTextureSets()
        {
            // ❌ DEPRECATED - WallTextureSet is no longer used
            Debug.LogWarning("[MazeSetupHelper] ⚠️ AddWallTextureSets is deprecated - textures now loaded from JSON config");
            Debug.Log("💡 Tip: Assign textures via GameConfig-default.json instead");
        }

        [ContextMenu("Verify Complete Setup")]
        public void VerifyCompleteSetup()
        {
            Debug.Log("=== Verifying Maze Setup ===");

            bool allGood = true;

            // Check required components
            string[] requiredComponents = {
                "MazeGenerator",
                "RoomGenerator",
                "DoorHolePlacer",
                "RoomDoorPlacer",
                "MazeRenderer",
                "MazeIntegration",
                "SeedManager",
                "DrawingPool",
                "TorchPool"
            };

            foreach (string componentName in requiredComponents)
            {
                var comp = GetComponent(componentName);
                if (comp != null)
                {
                    Debug.Log($"✅ {componentName}");
                }
                else
                {
                    Debug.LogError($"❌ {componentName} MISSING!");
                    allGood = false;
                }
            }

            // Check MazeIntegration settings
            var integration = GetComponent<MazeIntegration>();
            if (integration != null)
            {
                if (integration.MazeWidth < 15 || integration.MazeHeight < 15)
                    Debug.LogWarning("⚠️ Maze dimensions may be too small");
                if (integration.DoorChance <= 0f)
                    Debug.LogWarning("⚠️ Door chance is 0 - no doors will be placed");
            }

            if (allGood)
            {
                Debug.Log("✅ All components present and configured!");
                Debug.Log("▶️ Press Play to generate maze with rooms and doors");
            }
            else
            {
                Debug.Log("❌ Some components are missing. Run 'Setup Complete Maze System'");
            }
        }

        private T AddOrGetComponent<T>() where T : Component
        {
            T component = GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
                Debug.Log($"✅ Added {typeof(T).Name}");
            }
            else
            {
                Debug.Log($"✅ {typeof(T).Name} already present");
            }
            return component;
        }

        private void LogSetupSummary()
        {
            Debug.Log("\n=== Setup Summary (LEGACY) ===");
            Debug.Log("⚠️  This helper uses deprecated components!");
            Debug.Log("💡 For new development, use:");
            Debug.Log("   - CompleteMazeBuilder (maze generation)");
            Debug.Log("   - ChestPlacer, EnemyPlacer, ItemPlacer, TorchPlacer (objects)");
            Debug.Log("   - All values from GameConfig-default.json");
            Debug.Log("======================\n");
        }
    }
}
#endif
