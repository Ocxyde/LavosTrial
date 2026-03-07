// MazeBuilderEditor.cs
// Editor tools for CompleteMazeBuilder
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   1. Tools → Generate Maze
//   2. Tools → Validate Maze Paths
//   3. Tools → Clear Maze Objects
//
// Location: Assets/Scripts/Editor/

using UnityEngine;
using UnityEditor;
using Code.Lavos.Core;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// MazeBuilderEditor - Editor menu items for maze generation tools.
    /// Plug-in-out compliant: Uses EventHandler for communication.
    /// </summary>
    public class MazeBuilderEditor : EditorWindow
    {
        [MenuItem("Tools/Maze/Generate Maze %&G")]
        public static void GenerateMaze()
        {
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  MAZE GENERATOR - Complete Maze Generation");
            Debug.Log("═══════════════════════════════════════════");

            // Find or create maze builder
            var mazeBuilder = FindFirstObjectByType<CompleteMazeBuilder>();
            
            if (mazeBuilder == null)
            {
                // Create new GameObject with CompleteMazeBuilder
                GameObject mazeGO = new GameObject("MazeBuilder");
                mazeBuilder = mazeGO.AddComponent<CompleteMazeBuilder>();
                Debug.Log("✅ Created MazeBuilder GameObject");

                // Manually add required components (since Awake won't run in editor)
                var mazeGenerator = mazeGO.AddComponent<Code.Lavos.Core.MazeGenerator>();
                mazeGO.AddComponent<Code.Lavos.Core.MazeRenderer>();
                mazeGO.AddComponent<Code.Lavos.Core.SpatialPlacer>();
                mazeGO.AddComponent<Code.Lavos.Core.LightPlacementEngine>();
                Debug.Log("✅ Added required components");

                // Manually initialize references (mimic Awake() behavior)
                var torchPool = mazeGO.AddComponent<Code.Lavos.Core.TorchPool>();
                
                // Use reflection to set private fields in CompleteMazeBuilder
                var builderType = typeof(CompleteMazeBuilder);
                var mazeGenField = builderType.GetField("mazeGenerator", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var spatialPlacerField = builderType.GetField("spatialPlacer",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var torchPoolField = builderType.GetField("torchPool",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var lightPlacementEngineField = builderType.GetField("lightPlacementEngine",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                mazeGenField?.SetValue(mazeBuilder, mazeGenerator);
                spatialPlacerField?.SetValue(mazeBuilder, mazeGO.GetComponent<Code.Lavos.Core.SpatialPlacer>());
                torchPoolField?.SetValue(mazeBuilder, torchPool);
                lightPlacementEngineField?.SetValue(mazeBuilder, mazeGO.GetComponent<Code.Lavos.Core.LightPlacementEngine>());
                
                Debug.Log("🔌 Manually initialized component references");
            }
            else
            {
                Debug.Log("✓ Found existing CompleteMazeBuilder");
                
                // Ensure mazeGenerator is assigned (in case it was cleared)
                var mazeGenField = typeof(CompleteMazeBuilder).GetField("mazeGenerator",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (mazeGenField?.GetValue(mazeBuilder) == null)
                {
                    var mazeGenerator = mazeBuilder.GetComponent<Code.Lavos.Core.MazeGenerator>();
                    if (mazeGenerator != null)
                    {
                        mazeGenField?.SetValue(mazeBuilder, mazeGenerator);
                        Debug.Log("🔌 Re-assigned mazeGenerator reference");
                    }
                }
            }

            // Generate maze geometry ONLY (NO player in editor!)
            // Player will spawn automatically in Start() when entering Play mode
            mazeBuilder.GenerateMazeGeometryOnly();

            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  ✅ MAZE GENERATED!");
            Debug.Log("  💡 Press Play to spawn player inside maze");
            Debug.Log("═══════════════════════════════════════════");
        }

        [MenuItem("Tools/Maze/Validate Paths")]
        public static void ValidatePaths()
        {
            var mazeBuilder = FindFirstObjectByType<CompleteMazeBuilder>();
            
            if (mazeBuilder != null)
            {
                mazeBuilder.ValidatePaths();
            }
            else
            {
                Debug.LogWarning("[MazeBuilderEditor] ⚠️ No CompleteMazeBuilder found in scene!");
                Debug.LogWarning("[MazeBuilderEditor] 💡 Run: Tools → Maze → Generate Maze");
            }
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

            // Clear stored spawn position
            var mazeBuilder = FindFirstObjectByType<CompleteMazeBuilder>();
            if (mazeBuilder != null)
            {
                mazeBuilder.ClearSpawnPosition();
            }

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
