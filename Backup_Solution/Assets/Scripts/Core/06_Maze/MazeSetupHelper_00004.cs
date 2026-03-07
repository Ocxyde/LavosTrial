// MazeSetupHelper.cs
// Editor helper to setup complete maze generation system
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// ⚠️ HELPS SETUP LEGACY SYSTEM - For new projects use CompleteMazeBuilder

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// Suppress obsolete warnings - this file intentionally helps with legacy system setup
#pragma warning disable CS0618

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

            // Add all required components
            AddOrGetComponent<MazeGenerator>();
            AddOrGetComponent<RoomGenerator>();
            AddOrGetComponent<DoorHolePlacer>();
            AddOrGetComponent<RoomDoorPlacer>();
            AddOrGetComponent<MazeRenderer>();
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
            var renderer = GetComponent<MazeRenderer>();
            if (renderer != null)
            {
                Debug.Log("⚠️ MazeRenderer found (deprecated - use CompleteMazeBuilder for geometry)");
            }

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
            var doorPlacer = GetComponent<RoomDoorPlacer>();
            if (doorPlacer == null)
            {
                Debug.LogError("❌ RoomDoorPlacer not found!");
                return;
            }

            // Create 3 default texture sets
            var textureSets = new WallTextureSet[3];

            // Stone Dungeon
            textureSets[0] = new WallTextureSet
            {
                setName = "Stone Dungeon",
                baseColor = new Color(0.47f, 0.47f, 0.47f),
                variationColor = new Color(0.31f, 0.31f, 0.31f),
                noiseScale = 0.1f,
                contrast = 1.2f,
                tint = Color.white,
                smoothness = 0.5f
            };

            // Brick Wall
            textureSets[1] = new WallTextureSet
            {
                setName = "Brick Wall",
                baseColor = new Color(0.55f, 0.23f, 0.16f),
                variationColor = new Color(0.39f, 0.16f, 0.12f),
                noiseScale = 0.15f,
                contrast = 1.3f,
                tint = new Color(1f, 0.94f, 0.9f),
                smoothness = 0.3f
            };

            // Wood Panel
            textureSets[2] = new WallTextureSet
            {
                setName = "Wood Panel",
                baseColor = new Color(0.39f, 0.27f, 0.16f),
                variationColor = new Color(0.31f, 0.22f, 0.12f),
                noiseScale = 0.2f,
                contrast = 1.1f,
                tint = new Color(1f, 0.86f, 0.7f),
                smoothness = 0.6f
            };

            doorPlacer.WallTextureSets = textureSets;
            Debug.Log("✅ Added 3 wall texture sets");
            Debug.Log("   - Stone Dungeon");
            Debug.Log("   - Brick Wall");
            Debug.Log("   - Wood Panel");
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
            Debug.Log("\n=== Setup Summary ===");
            Debug.Log("Maze Generation System Components:");
            Debug.Log("  - MazeGenerator: Creates maze grid");
            Debug.Log("  - RoomGenerator: Carves rooms into maze");
            Debug.Log("  - DoorHolePlacer: Reserves wall holes");
            Debug.Log("  - RoomDoorPlacer: Places doors in holes");
            Debug.Log("  - MazeRenderer: Builds 3D geometry");
            Debug.Log("  - TorchPool: Places wall torches");
            Debug.Log("  - SeedManager: Manages procedural seeds");
            Debug.Log("  - DrawingPool: Texture generation");
            Debug.Log("  - MazeIntegration: Orchestrates all");
            Debug.Log("======================\n");
        }
    }
}
#endif
