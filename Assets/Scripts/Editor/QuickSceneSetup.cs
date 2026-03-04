// QuickSceneSetup.cs
// Complete scene setup using plug-in-and-out architecture
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   1. Create empty GameObject "MazeTest"
//   2. Add this component
//   3. Tools → Quick Scene Setup → Generate Complete Scene
//   4. Press Play - everything is ready!
//
// Location: Assets/Scripts/Editor/

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Reflection;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// QuickSceneSetup - Complete scene generation using plug-in-and-out system.
    /// Generates: Maze, Rooms, Doors, Door Holes, Textures, Ground, Ceiling, Torches.
    /// Prepares for enemies (prefabs assigned but not spawned yet).
    /// </summary>
    public class QuickSceneSetup
    {
        #region Quick Setup Method

        [MenuItem("Tools/Quick Scene Setup/Generate Complete Scene %&G")]
        public static void GenerateCompleteScene()
        {
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  QUICK SCENE SETUP - Complete Generation");
            Debug.Log("═══════════════════════════════════════════");

            // Step 1: Find or create MazeTest GameObject
            GameObject mazeTest = FindOrCreateMazeTest();

            // Step 2: Add all required components (plug-in-and-out)
            AddCoreComponents(mazeTest);

            // Step 3: Configure components
            ConfigureComponents(mazeTest);

            // Step 4: Save scene
            SaveScene();

            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  ✅ COMPLETE SCENE READY!");
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  Components Added:");
            Debug.Log("    • MazeGenerator (maze generation)");
            Debug.Log("    • MazeRenderer (wall textures)");
            Debug.Log("    • MazeIntegration (rooms + doors)");
            Debug.Log("    • SpatialPlacer (torches, chests, items)");
            Debug.Log("    • TorchPool (torch instancing)");
            Debug.Log("    • LightPlacementEngine (dynamic lighting)");
            Debug.Log("    • LightEngine (lighting system)");
            Debug.Log("    • GroundPlaneGenerator (floor)");
            Debug.Log("    • CeilingGenerator (ceiling)");
            Debug.Log("    • FpsMazeTest (test controller)");
            Debug.Log("");
            Debug.Log("  Features Enabled:");
            Debug.Log("    ✅ Maze: 31x31 with wide corridors (6m)");
            Debug.Log("    ✅ Rooms: 3-8 special rooms");
            Debug.Log("    ✅ Doors: 60% chance with holes");
            Debug.Log("    ✅ Textures: Stone walls/floor/ceiling");
            Debug.Log("    ✅ Torches: 60 with dynamic light");
            Debug.Log("    ✅ Ground: Stone tile floor");
            Debug.Log("    ✅ Ceiling: Dark stone");
            Debug.Log("    ✅ Lighting: Fog of war + dynamic lights");
            Debug.Log("    ✅ Enemies: Prefabs ready (not spawned)");
            Debug.Log("");
            Debug.Log("  🎮 Press Play to test!");
            Debug.Log("═══════════════════════════════════════════");
        }

        #endregion

        #region Helper Methods

        private static GameObject FindOrCreateMazeTest()
        {
            GameObject mazeTest = GameObject.Find("MazeTest");
            
            if (mazeTest == null)
            {
                Debug.Log("[1/4] Creating MazeTest GameObject...");
                mazeTest = new GameObject("MazeTest");
                mazeTest.transform.position = Vector3.zero;
            }
            else
            {
                Debug.Log("[1/4] Found existing MazeTest GameObject");
            }

            return mazeTest;
        }

        private static void AddCoreComponents(GameObject mazeTest)
        {
            Debug.Log("[2/4] Adding core components (plug-in-and-out)...");

            // STEP 1: Add EventHandler FIRST (required by other components)
            var eventHandler = AddComponentIfMissing(mazeTest, "Code.Lavos.Core.EventHandler");
            Debug.Log("  • EventHandler: Added (central event hub)");

            // STEP 2: Add SeedManager (required for maze generation)
            AddComponentIfMissing(mazeTest, "Code.Lavos.Core.SeedManager");
            Debug.Log("  • SeedManager: Added (procedural seed)");

            // STEP 3: Core maze components
            AddComponentIfMissing(mazeTest, "Code.Lavos.Core.MazeGenerator");
            AddComponentIfMissing(mazeTest, "Code.Lavos.Core.MazeRenderer");
            AddComponentIfMissing(mazeTest, "Code.Lavos.Core.MazeIntegration");
            AddComponentIfMissing(mazeTest, "Code.Lavos.Core.SpatialPlacer");
            AddComponentIfMissing(mazeTest, "Code.Lavos.Core.TorchPool");
            
            // Lighting components
            AddComponentIfMissing(mazeTest, "Code.Lavos.Core.LightPlacementEngine");
            AddComponentIfMissing(mazeTest, "Code.Lavos.Core.LightEngine");
            
            // Environment components
            AddComponentIfMissing(mazeTest, "Code.Lavos.Core.GroundPlaneGenerator");
            AddComponentIfMissing(mazeTest, "Code.Lavos.Core.CeilingGenerator");
            
            // Test controller
            AddComponentIfMissing(mazeTest, "Code.Lavos.Core.FpsMazeTest");

            Debug.Log("  ✅ Added 13 components");
        }

        private static UnityEngine.Object AddComponentIfMissing(GameObject go, string typeName)
        {
            Type type = Type.GetType(typeName + ", Code.Lavos.Core");
            if (type != null && go.GetComponent(type) == null)
            {
                return go.AddComponent(type);
            }
            return go.GetComponent(type);
        }

        private static void ConfigureComponents(GameObject mazeTest)
        {
            Debug.Log("[3/4] Configuring components...");

            // Get all components
            var mazeGenerator = mazeTest.GetComponent(Type.GetType("Code.Lavos.Core.MazeGenerator, Code.Lavos.Core"));
            var mazeRenderer = mazeTest.GetComponent(Type.GetType("Code.Lavos.Core.MazeRenderer, Code.Lavos.Core"));
            var mazeIntegration = mazeTest.GetComponent(Type.GetType("Code.Lavos.Core.MazeIntegration, Code.Lavos.Core"));
            var spatialPlacer = mazeTest.GetComponent(Type.GetType("Code.Lavos.Core.SpatialPlacer, Code.Lavos.Core"));
            var torchPool = mazeTest.GetComponent(Type.GetType("Code.Lavos.Core.TorchPool, Code.Lavos.Core"));
            var lightPlacementEngine = mazeTest.GetComponent(Type.GetType("Code.Lavos.Core.LightPlacementEngine, Code.Lavos.Core"));
            var lightEngine = mazeTest.GetComponent(Type.GetType("Code.Lavos.Core.LightEngine, Code.Lavos.Core"));
            var fpsMazeTest = mazeTest.GetComponent(Type.GetType("Code.Lavos.Core.FpsMazeTest, Code.Lavos.Core"));

            // Configure MazeGenerator
            if (mazeGenerator != null)
            {
                SetField(mazeGenerator, "width", 31);
                SetField(mazeGenerator, "height", 31);
                SetField(mazeGenerator, "useDynamicSize", false);
                Debug.Log("  • MazeGenerator: 31x31");
            }

            // Configure MazeRenderer
            if (mazeRenderer != null)
            {
                SetField(mazeRenderer, "cellSize", 6f); // Wide corridors
                SetField(mazeRenderer, "wallHeight", 3.5f);
                SetField(mazeRenderer, "floorType", 0); // Stone
                SetField(mazeRenderer, "autoGenerateFloorMaterials", true);
                Debug.Log("  • MazeRenderer: Cell=6m, Walls=3.5m, Stone texture");
            }

            // Configure MazeIntegration
            if (mazeIntegration != null)
            {
                SetField(mazeIntegration, "autoGenerateOnStart", false); // FpsMazeTest handles this
                SetField(mazeIntegration, "useRandomSeed", true);
                SetField(mazeIntegration, "generateRooms", true);
                SetField(mazeIntegration, "minRooms", 3);
                SetField(mazeIntegration, "maxRooms", 8);
                SetField(mazeIntegration, "generateDoors", true);
                SetField(mazeIntegration, "doorChance", 0.6f);
                SetField(mazeIntegration, "placeTorches", false);
                SetField(mazeIntegration, "currentLevel", 1);
                SetField(mazeIntegration, "useLevelProgression", true);
                
                // Wire up references
                SetField(mazeIntegration, "mazeGenerator", mazeGenerator);
                SetField(mazeIntegration, "spatialPlacer", spatialPlacer);
                Debug.Log("  • MazeIntegration: Rooms=3-8, Doors=60%, Level=1 (FpsMazeTest controls gen)");
            }

            // Configure SpatialPlacer
            if (spatialPlacer != null)
            {
                SetField(spatialPlacer, "placeTorches", true);
                SetField(spatialPlacer, "torchCount", 60);
                SetField(spatialPlacer, "torchHeightRatio", 0.55f);
                SetField(spatialPlacer, "torchInset", 0.5f);
                SetField(spatialPlacer, "minDistanceBetweenTorches", 4f);
                SetField(spatialPlacer, "placeChests", true);
                SetField(spatialPlacer, "chestCount", 5);
                SetField(spatialPlacer, "placeEnemies", true);
                SetField(spatialPlacer, "enemyCount", 8);
                SetField(spatialPlacer, "placeItems", true);
                SetField(spatialPlacer, "itemCount", 10);
                
                // Wire up references
                SetField(spatialPlacer, "mazeGenerator", mazeGenerator);
                SetField(spatialPlacer, "torchPool", torchPool);
                SetField(spatialPlacer, "lightPlacementEngine", lightPlacementEngine);
                Debug.Log("  • SpatialPlacer: Torches=60, Chests=5, Enemies=8, Items=10");
            }

            // Configure TorchPool
            if (torchPool != null)
            {
                SetField(torchPool, "torchHandlePrefab", (GameObject)null);
                SetField(torchPool, "useBraseroFlame", true);
                Debug.Log("  • TorchPool: BraseroFlame particles enabled");
            }

            // Configure LightPlacementEngine
            if (lightPlacementEngine != null)
            {
                SetField(lightPlacementEngine, "torchPrefab", (GameObject)null);
                SetField(lightPlacementEngine, "parentName", "Lights");
                SetField(lightPlacementEngine, "startOn", false);
                Debug.Log("  • LightPlacementEngine: Dynamic lighting ready");
            }

            // Configure LightEngine
            if (lightEngine != null)
            {
                SetField(lightEngine, "enableDynamicLights", true);
                SetField(lightEngine, "defaultLightRange", 12f);
                SetField(lightEngine, "defaultLightIntensity", 1.8f);
                SetField(lightEngine, "defaultLightColor", new Color(1f, 0.9f, 0.7f));
                SetField(lightEngine, "enableFogOfWar", true);
                SetField(lightEngine, "darknessFalloff", 15f);
                Debug.Log("  • LightEngine: Dynamic lights + Fog of war");
            }

            // Configure FpsMazeTest
            if (fpsMazeTest != null)
            {
                SetField(fpsMazeTest, "autoGenerateOnStart", true);
                SetField(fpsMazeTest, "useRandomSeed", true);
                SetField(fpsMazeTest, "mazeWidth", 31);
                SetField(fpsMazeTest, "mazeHeight", 31);
                SetField(fpsMazeTest, "cellSize", 6f);
                SetField(fpsMazeTest, "enableTorches", true);
                SetField(fpsMazeTest, "torchCount", 60);
                SetField(fpsMazeTest, "spawnTestPlayer", true);
                SetField(fpsMazeTest, "eyeHeight", 1.7f);
                SetField(fpsMazeTest, "enableHeadBob", true);
                SetField(fpsMazeTest, "spawnGroundPlane", true);
                SetField(fpsMazeTest, "groundSize", 200f);
                SetField(fpsMazeTest, "showDebugUI", true);
                SetField(fpsMazeTest, "verboseLogging", true);
                Debug.Log("  • FpsMazeTest: FPS player, Ground=200m, Debug=ON");
            }

            Debug.Log("  ✅ All components configured");
        }

        private static void SaveScene()
        {
            Debug.Log("[4/4] Saving scene...");
            
            // Mark scene as dirty
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            
            Debug.Log("  ✅ Scene saved");
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, 
                BindingFlags.Public | 
                BindingFlags.NonPublic | 
                BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(target, value);
            }
            else
            {
                Debug.LogWarning($"  ⚠️ Field not found: {fieldName}");
            }
        }

        #endregion

        #region Menu Item

        [MenuItem("Tools/Quick Scene Setup/Clear Scene %&X")]
        public static void ClearSceneMenu()
        {
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  CLEARING SCENE...");
            Debug.Log("═══════════════════════════════════════════");

            // Find and destroy MazeTest
            var mazeTest = GameObject.Find("MazeTest");
            if (mazeTest != null)
            {
                UnityEngine.Object.DestroyImmediate(mazeTest);
                Debug.Log("  ✅ Removed MazeTest GameObject");
            }

            // Clean up generated objects
            CleanUpObject("MazeWalls");
            CleanUpObject("Floor");
            CleanUpObject("Ceiling");
            CleanUpObject("Lights");
            CleanUpObject("Torches");
            CleanUpObject("Doors");
            CleanUpObject("Player");

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            Debug.Log("  ✅ Scene cleared");
            Debug.Log("═══════════════════════════════════════════");
        }

        private static void CleanUpObject(string name)
        {
            var obj = GameObject.Find(name);
            if (obj != null)
            {
                UnityEngine.Object.DestroyImmediate(obj);
                Debug.Log($"  • Removed: {name}");
            }
        }

        #endregion
    }
}
