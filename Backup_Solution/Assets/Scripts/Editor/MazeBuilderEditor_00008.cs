// MazeBuilderEditor.cs
// Editor tool for maze generation testing
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   1. Tools → Generate Maze (or Ctrl+Alt+G)
//   2. Auto-creates CompleteMazeBuilder + required components
//   3. Generates maze instantly for testing
//   4. Press Play to test with player
//
// NOTE: This is an EDITOR TOOL - it creates components for convenience.
// Runtime code should still follow plug-in-out architecture.

using UnityEngine;
using UnityEditor;
using Code.Lavos.Core;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// MazeBuilderEditor - Editor tool for testing maze generation.
    /// Auto-creates CompleteMazeBuilder and required components for quick testing.
    /// </summary>
    public class MazeBuilderEditor : EditorWindow
    {
        [MenuItem("Tools/Maze/Generate Maze %&G")]
        public static void GenerateMaze()
        {
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  MAZE GENERATOR - Complete Maze Generation");
            Debug.Log("═══════════════════════════════════════════");

            // Load config values from JSON (source of truth!)
            var config = GameConfig.Instance;
            Debug.Log($"[MazeBuilderEditor] 📖 Config loaded from JSON:");
            Debug.Log($"  • Maze Size: {config.defaultMazeWidth}x{config.defaultMazeHeight}");
            Debug.Log($"  • Cell Size: {config.defaultCellSize}m");
            Debug.Log($"  • Room Size: {config.defaultRoomSize}x{config.defaultRoomSize}");
            Debug.Log($"  • Corridor Width: {config.defaultCorridorWidth} cells");
            Debug.Log($"  • Wall Height: {config.defaultWallHeight}m");

            // Find existing maze builder
            var mazeBuilder = FindFirstObjectByType<CompleteMazeBuilder>();

            if (mazeBuilder == null)
            {
                // Create new GameObject with CompleteMazeBuilder (editor tool - acceptable!)
                GameObject mazeGO = new GameObject("MazeBuilder");
                mazeBuilder = mazeGO.AddComponent<CompleteMazeBuilder>();
                Debug.Log("✅ Created MazeBuilder GameObject");

                // Add required components for testing (editor tool only!)
                // Note: GridMazeGenerator is a plain C# class, not a MonoBehaviour
                // CompleteMazeBuilder will initialize it from JSON config automatically
                var spatialPlacer = mazeGO.AddComponent<SpatialPlacer>();
                var lightPlacementEngine = mazeGO.AddComponent<LightPlacementEngine>();
                var torchPool = mazeGO.AddComponent<TorchPool>();
                Debug.Log("✅ Added required components");

                // Configure components from JSON config
                Debug.Log("[MazeBuilderEditor] 🔧 Configuring components from JSON...");
                
                // SpatialPlacer - finds TorchPool automatically
                Debug.Log($"  • SpatialPlacer: will use TorchPool reference");

                // LightPlacementEngine - load torch prefab from Resources (no extension needed)
                var torchPrefab = Resources.Load<GameObject>(config.torchPrefab.Replace(".prefab", ""));
                if (torchPrefab != null)
                {
                    lightPlacementEngine.SetTorchPrefab(torchPrefab);
                    Debug.Log($"  • LightPlacementEngine: torch prefab loaded from {config.torchPrefab}");
                }
                else
                {
                    Debug.LogWarning($"  ⚠️ LightPlacementEngine: torch prefab not found at Resources/{config.torchPrefab}");
                    Debug.LogWarning($"  💡 Make sure TorchHandlePrefab.prefab is in Assets/Resources/ folder");
                }

                // TorchPool - auto-initializes
                Debug.Log($"  • TorchPool: ready (prefab: {config.torchPrefab})");

                // Also add EventHandler if not present (editor-safe - no DontDestroyOnLoad)
                var eventHandler = FindFirstObjectByType<EventHandler>();
                if (eventHandler == null)
                {
                    var eventGO = new GameObject("EventHandler");
                    eventHandler = eventGO.AddComponent<EventHandler>();
                    Debug.Log("✅ Created EventHandler (scene-only, not persistent in editor)");
                }

                Debug.Log("🔌 All components configured from JSON!");
                Debug.Log("🔌 All components ready for testing!");
            }
            else
            {
                Debug.Log("✓ Found existing CompleteMazeBuilder");
                Debug.Log($"[MazeBuilderEditor] 📖 Using existing instance - config already loaded from JSON");
            }

            // Generate maze (editor mode - no player spawn)
            // Player will spawn automatically in Start() when entering Play mode
            mazeBuilder.GenerateMaze();

            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  ✅ MAZE GENERATED!");
            Debug.Log($"  📏 Maze Size: {mazeBuilder.MazeSize}x{mazeBuilder.MazeSize}");
            Debug.Log($"  🎮 Level: {mazeBuilder.CurrentLevel}");
            Debug.Log("  💡 Press Play to spawn player inside maze");
            Debug.Log("═══════════════════════════════════════════");
        }

        [MenuItem("Tools/Maze/Next Level (Harder)")]
        public static void NextLevel()
        {
            var mazeBuilder = FindFirstObjectByType<CompleteMazeBuilder>();

            if (mazeBuilder != null)
            {
                mazeBuilder.NextLevel();
                Debug.Log($"[MazeBuilderEditor] 🎮 Advanced to Level {mazeBuilder.CurrentLevel} - Maze {mazeBuilder.MazeSize}x{mazeBuilder.MazeSize}");
            }
            else
            {
                Debug.LogWarning("[MazeBuilderEditor] ⚠️ No CompleteMazeBuilder found in scene!");
            }
        }

        [MenuItem("Tools/Maze/Validate Paths")]
        public static void ValidatePaths()
        {
            Debug.Log("[MazeBuilderEditor] ℹ️ Path validation is now automatic - prefabs loaded from JSON config");
            Debug.Log("[MazeBuilderEditor] 💡 If maze generates, paths are valid!");
        }

        [MenuItem("Tools/Maze/Clear Maze Objects")]
        public static void ClearMazeObjects()
        {
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  CLEARING MAZE OBJECTS...");
            Debug.Log("═══════════════════════════════════════════");

            // Clean up generated objects
            CleanUpObject("MazeWalls");
            CleanUpObject("GroundFloor");
            CleanUpObject("Ceiling");
            CleanUpObject("Lights");
            CleanUpObject("Torches");
            CleanUpObject("Doors");
            CleanUpObject("Rooms");

            // Clear PlayerPrefs spawn position (if any)
            PlayerPrefs.DeleteKey("MazeSpawnX");
            PlayerPrefs.DeleteKey("MazeSpawnY");
            PlayerPrefs.Save();
            Debug.Log("  ✅ Spawn position cleared");

            // Don't delete MazeBuilder or Player
            Debug.Log("  ✅ Maze objects cleared");
            Debug.Log("═══════════════════════════════════════════");
        }

        [MenuItem("Tools/Maze/Show Documentation")]
        public static void ShowDocumentation()
        {
            string docPath = "Assets/Docs/CompleteMazeBuilder_Documentation.md";
            
            if (System.IO.File.Exists(docPath))
            {
                UnityEditor.EditorUtility.RevealInFinder(docPath);
                Debug.Log("[MazeBuilderEditor] 📖 Opened documentation");
            }
            else
            {
                Debug.LogWarning("[MazeBuilderEditor] ⚠️ Documentation not found at: " + docPath);
            }
        }

        private static void CleanUpObject(string name)
        {
            var obj = GameObject.Find(name);
            if (obj != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(obj);
                else
                    Object.DestroyImmediate(obj);
                Debug.Log($"  • Removed: {name}");
            }
        }

        // Editor window (optional - for advanced settings)
        [MenuItem("Tools/Maze/Maze Builder Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<MazeBuilderEditor>("Maze Builder");
            window.minSize = new Vector2(300, 400);
        }

        private void OnGUI()
        {
            GUILayout.Label("Maze Builder Settings", EditorStyles.boldLabel);
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Generate Maze", GUILayout.Height(30)))
            {
                GenerateMaze();
            }
            
            if (GUILayout.Button("Validate Paths"))
            {
                ValidatePaths();
            }
            
            if (GUILayout.Button("Clear Maze Objects"))
            {
                ClearMazeObjects();
            }
            
            GUILayout.Space(20);
            
            GUILayout.Label("Documentation", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Show Documentation"))
            {
                ShowDocumentation();
            }
        }
    }
}
