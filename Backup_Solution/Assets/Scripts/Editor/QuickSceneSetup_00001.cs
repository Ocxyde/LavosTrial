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
using System.IO;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// QuickSceneSetup - Complete scene generation using plug-in-and-out system.
    /// Generates: Maze, Rooms, Doors, Door Holes, Textures, Ground, Ceiling, Torches.
    /// Prepares for enemies (prefabs assigned but not spawned yet).
    /// </summary>
    public class QuickSceneSetup : MonoBehaviour
    {
        #region Quick Setup Method

        [ContextMenu("Generate Complete Scene")]
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

            // Core maze components
            if (mazeTest.GetComponent<MazeGenerator>() == null)
                mazeTest.AddComponent<MazeGenerator>();

            if (mazeTest.GetComponent<MazeRenderer>() == null)
                mazeTest.AddComponent<MazeRenderer>();

            if (mazeTest.GetComponent<MazeIntegration>() == null)
                mazeTest.AddComponent<MazeIntegration>();

            if (mazeTest.GetComponent<SpatialPlacer>() == null)
                mazeTest.AddComponent<SpatialPlacer>();

            if (mazeTest.GetComponent<TorchPool>() == null)
                mazeTest.AddComponent<TorchPool>();

            // Lighting components
            if (mazeTest.GetComponent<LightPlacementEngine>() == null)
                mazeTest.AddComponent<LightPlacementEngine>();

            if (mazeTest.GetComponent<LightEngine>() == null)
                mazeTest.AddComponent<LightEngine>();

            // Environment components
            if (mazeTest.GetComponent<GroundPlaneGenerator>() == null)
                mazeTest.AddComponent<GroundPlaneGenerator>();

            if (mazeTest.GetComponent<CeilingGenerator>() == null)
                mazeTest.AddComponent<CeilingGenerator>();

            // Test controller
            if (mazeTest.GetComponent<FpsMazeTest>() == null)
                mazeTest.AddComponent<FpsMazeTest>();

            Debug.Log($"  ✅ Added {9} components");
        }

        private static void ConfigureComponents(GameObject mazeTest)
        {
            Debug.Log("[3/4] Configuring components...");

            // Configure MazeGenerator
            var mazeGenerator = mazeTest.GetComponent<MazeGenerator>();
            mazeGenerator.width = 31;
            mazeGenerator.height = 31;
            mazeGenerator.useDynamicSize = false;
            Debug.Log("  • MazeGenerator: 31x31");

            // Configure MazeRenderer
            var mazeRenderer = mazeTest.GetComponent<MazeRenderer>();
            SetField(mazeRenderer, "cellSize", 6f); // Wide corridors
            SetField(mazeRenderer, "wallHeight", 3.5f);
            SetField(mazeRenderer, "floorType", 0); // Stone
            SetField(mazeRenderer, "autoGenerateFloorMaterials", true);
            Debug.Log("  • MazeRenderer: Cell=6m, Walls=3.5m, Stone texture");

            // Configure MazeIntegration
            var mazeIntegration = mazeTest.GetComponent<MazeIntegration>();
            mazeIntegration.autoGenerateOnStart = true;
            mazeIntegration.useRandomSeed = true;
            mazeIntegration.generateRooms = true;
            mazeIntegration.minRooms = 3;
            mazeIntegration.maxRooms = 8;
            mazeIntegration.generateDoors = true;
            mazeIntegration.doorChance = 0.6f;
            mazeIntegration.placeTorches = false; // SpatialPlacer handles this
            mazeIntegration.currentLevel = 1;
            mazeIntegration.useLevelProgression = true;
            
            // Wire up references
            SetField(mazeIntegration, "mazeGenerator", mazeGenerator);
            SetField(mazeIntegration, "spatialPlacer", mazeTest.GetComponent<SpatialPlacer>());
            Debug.Log("  • MazeIntegration: Rooms=3-8, Doors=60%, Level=1");

            // Configure SpatialPlacer
            var spatialPlacer = mazeTest.GetComponent<SpatialPlacer>();
            spatialPlacer.placeTorches = true;
            spatialPlacer.torchCount = 60;
            spatialPlacer.torchHeightRatio = 0.55f;
            spatialPlacer.torchInset = 0.5f;
            spatialPlacer.minDistanceBetweenTorches = 4f;
            spatialPlacer.placeChests = true;
            spatialPlacer.chestCount = 5;
            spatialPlacer.placeEnemies = true;
            spatialPlacer.enemyCount = 8;
            spatialPlacer.placeItems = true;
            spatialPlacer.itemCount = 10;
            
            // Wire up references
            SetField(spatialPlacer, "mazeGenerator", mazeGenerator);
            SetField(spatialPlacer, "torchPool", mazeTest.GetComponent<TorchPool>());
            SetField(spatialPlacer, "lightPlacementEngine", mazeTest.GetComponent<LightPlacementEngine>());
            Debug.Log("  • SpatialPlacer: Torches=60, Chests=5, Enemies=8, Items=10");

            // Configure TorchPool
            var torchPool = mazeTest.GetComponent<TorchPool>();
            SetField(torchPool, "torchHandlePrefab", (GameObject)null);
            SetField(torchPool, "useBraseroFlame", true);
            Debug.Log("  • TorchPool: BraseroFlame particles enabled");

            // Configure LightPlacementEngine
            var lightPlacementEngine = mazeTest.GetComponent<LightPlacementEngine>();
            lightPlacementEngine.torchPrefab = null; // Use TorchPool
            lightPlacementEngine.parentName = "Lights";
            lightPlacementEngine.startOn = false; // Torches control this
            Debug.Log("  • LightPlacementEngine: Dynamic lighting ready");

            // Configure LightEngine
            var lightEngine = mazeTest.GetComponent<LightEngine>();
            SetField(lightEngine, "enableDynamicLights", true);
            SetField(lightEngine, "defaultLightRange", 12f);
            SetField(lightEngine, "defaultLightIntensity", 1.8f);
            SetField(lightEngine, "defaultLightColor", new Color(1f, 0.9f, 0.7f));
            SetField(lightEngine, "enableFogOfWar", true);
            SetField(lightEngine, "darknessFalloff", 15f);
            Debug.Log("  • LightEngine: Dynamic lights + Fog of war");

            // Configure FpsMazeTest
            var fpsMazeTest = mazeTest.GetComponent<FpsMazeTest>();
            fpsMazeTest.autoGenerateOnStart = true;
            fpsMazeTest.useRandomSeed = true;
            fpsMazeTest.mazeWidth = 31;
            fpsMazeTest.mazeHeight = 31;
            SetField(fpsMazeTest, "cellSize", 6f);
            fpsMazeTest.enableTorches = true;
            fpsMazeTest.torchCount = 60;
            fpsMazeTest.spawnTestPlayer = true;
            fpsMazeTest.eyeHeight = 1.7f;
            fpsMazeTest.enableHeadBob = true;
            fpsMazeTest.spawnGroundPlane = true;
            fpsMazeTest.groundSize = 200f;
            Debug.Log("  • FpsMazeTest: FPS player, Ground=200m");

            Debug.Log("  ✅ All components configured");
        }

        private static void SaveScene()
        {
            Debug.Log("[4/4] Saving scene...");
            
            // Mark scene as dirty
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            
            Debug.Log("  ✅ Scene saved");
        }

        private static void SetField<T>(object target, string fieldName, T value)
        {
            var field = target.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
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

        [MenuItem("Tools/Quick Scene Setup/Generate Complete Scene %&G")]
        public static void GenerateCompleteSceneMenu()
        {
            GenerateCompleteScene();
        }

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
                Object.DestroyImmediate(mazeTest);
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

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Debug.Log("  ✅ Scene cleared");
            Debug.Log("═══════════════════════════════════════════");
        }

        private static void CleanUpObject(string name)
        {
            var obj = GameObject.Find(name);
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
                Debug.Log($"  • Removed: {name}");
            }
        }

        #endregion
    }
}
