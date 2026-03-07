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
//
// MazeBuilderEditor.cs
// Editor tool for maze generation testing
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-OUT COMPLIANT:
// - Does NOT create components (only finds existing)
// - All values from JSON config (no hardcoding)
// - Provides setup instructions if components missing
//
// USAGE:
//   1. Add CompleteMazeBuilder to scene manually
//   2. Add required components (see Scene Setup Requirements below)
//   3. Tools → Generate Maze (or Ctrl+Alt+G)
//   4. Press Play to test with player
//
// SCENE SETUP REQUIREMENTS (add manually once):
//   - CompleteMazeBuilder
//   - SpatialPlacer
//   - LightPlacementEngine
//   - TorchPool
//   - EventHandler
//   - PlayerSetup + PlayerController + PlayerStats (on Player GameObject)
//
// NOTE: This is an EDITOR TOOL for convenience.
// Runtime code follows plug-in-out architecture strictly.

using UnityEngine;
using UnityEditor;
using Code.Lavos.Core;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// MazeBuilderEditor - Editor tool for testing maze generation.
    /// PLUG-IN-OUT COMPLIANT: Only finds existing components, never creates.
    /// Provides clear instructions if setup is incomplete.
    /// </summary>
    public class MazeBuilderEditor : EditorWindow
    {
        #region Menu Items

        [MenuItem("Tools/Maze/Generate Maze %&G")]
        public static void GenerateMaze()
        {
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  MAZE GENERATOR - Complete Maze Generation");
            Debug.Log("═══════════════════════════════════════════");

            // Validate scene setup first
            if (!ValidateSceneSetup())
            {
                Debug.LogError("═══════════════════════════════════════════");
                Debug.LogError("  MAZE GENERATION ABORTED - Missing Components!");
                Debug.LogError("═══════════════════════════════════════════");
                Debug.LogError("  Add these components to your scene:");
                Debug.LogError("    1. CompleteMazeBuilder");
                Debug.LogError("    2. SpatialPlacer");
                Debug.LogError("    3. LightPlacementEngine");
                Debug.LogError("    4. TorchPool");
                Debug.LogError("    5. EventHandler");
                Debug.LogError("    6. PlayerSetup (on Player GameObject)");
                Debug.LogError("═══════════════════════════════════════════");
                Debug.LogError("  Run: Tools → Quick Setup Prefabs (for testing)");
                Debug.LogError("  Then add components manually to scene.");
                Debug.LogError("═══════════════════════════════════════════");
                return;
            }

            // Load config values from JSON (source of truth!)
            var config = GameConfig.Instance;
            Debug.Log($"[MazeBuilderEditor]  Config loaded from JSON:");
            Debug.Log($"    Maze Size: {config.defaultGridSize}x{config.defaultGridSize}");
            Debug.Log($"    Cell Size: {config.defaultCellSize}m");
            Debug.Log($"    Room Size: {config.defaultRoomSize}x{config.defaultRoomSize}");
            Debug.Log($"    Corridor Width: {config.defaultCorridorWidth} cells");
            Debug.Log($"    Wall Height: {config.defaultWallHeight}m");

            // Find existing maze builder (plug-in-out: never create!)
            var mazeBuilder = FindFirstObjectByType<CompleteMazeBuilder>();

            if (mazeBuilder == null)
            {
                Debug.LogError("[MazeBuilderEditor]  CompleteMazeBuilder not found in scene!");
                Debug.LogError("  Add CompleteMazeBuilder component to a GameObject manually.");
                return;
            }

            Debug.Log("  Found CompleteMazeBuilder");

            // Validate required components on maze builder
            if (!ValidateMazeBuilderComponents(mazeBuilder))
            {
                Debug.LogWarning("[MazeBuilderEditor]  Some components missing on CompleteMazeBuilder.");
                Debug.LogWarning("  Maze generation may fail. Check Inspector and assign:");
                Debug.LogWarning("    - SpatialPlacer");
                Debug.LogWarning("    - LightPlacementEngine");
                Debug.LogWarning("    - TorchPool");
            }

            // Generate maze
            mazeBuilder.GenerateMaze();

            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("   MAZE GENERATED!");
            Debug.Log($"   Maze Size: {mazeBuilder.MazeSize}x{mazeBuilder.MazeSize}");
            Debug.Log($"   Level: {mazeBuilder.CurrentLevel}");
            Debug.Log("   Press Play to test");
            Debug.Log("═══════════════════════════════════════════");
        }

        [MenuItem("Tools/Maze/Next Level (Harder)")]
        public static void NextLevel()
        {
            var mazeBuilder = FindFirstObjectByType<CompleteMazeBuilder>();

            if (mazeBuilder != null)
            {
                mazeBuilder.NextLevel();
                Debug.Log($"[MazeBuilderEditor]  Advanced to Level {mazeBuilder.CurrentLevel} - Maze {mazeBuilder.MazeSize}x{mazeBuilder.MazeSize}");
            }
            else
            {
                Debug.LogWarning("[MazeBuilderEditor]  No CompleteMazeBuilder found in scene!");
                Debug.LogWarning("  Add CompleteMazeBuilder component manually first.");
            }
        }

        [MenuItem("Tools/Maze/Validate Scene Setup")]
        public static void ValidateSceneSetupMenuItem()
        {
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  VALIDATING SCENE SETUP...");
            Debug.Log("═══════════════════════════════════════════");

            bool isValid = ValidateSceneSetup();

            if (isValid)
            {
                Debug.Log("✅ SCENE SETUP VALID!");
                Debug.Log("  All required components found.");
                Debug.Log("  Ready to generate maze.");
            }
            else
            {
                Debug.LogError("❌ SCENE SETUP INCOMPLETE!");
                Debug.LogError("  Missing components. See errors above.");
            }

            Debug.Log("═══════════════════════════════════════════");
        }

        [MenuItem("Tools/Maze/Validate Paths")]
        public static void ValidatePaths()
        {
            Debug.Log("[MazeBuilderEditor]  ℹ️ Path validation is automatic - prefabs loaded from JSON config");
            Debug.Log("[MazeBuilderEditor]  If maze generates, paths are valid!");
        }

        [MenuItem("Tools/Maze/Clear Maze Objects")]
        public static void ClearMazeObjects()
        {
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  CLEARING MAZE OBJECTS...");
            Debug.Log("═══════════════════════════════════════════");

            // Clean up generated objects (plug-in-out: only clean what we created)
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
            Debug.Log("  Spawn position cleared");

            // Don't delete MazeBuilder or Player (user-managed)
            Debug.Log("  Maze objects cleared");
            Debug.Log("═══════════════════════════════════════════");
        }

        [MenuItem("Tools/Maze/Show Documentation")]
        public static void ShowDocumentation()
        {
            string docPath = "Assets/Docs/TODO.md";

            if (System.IO.File.Exists(docPath))
            {
                UnityEditor.EditorUtility.RevealInFinder(docPath);
                Debug.Log("[MazeBuilderEditor]  Opened TODO.md");
            }
            else
            {
                Debug.LogWarning("[MazeBuilderEditor]  Documentation not found at: " + docPath);
            }
        }

        #endregion

        #region Private Methods - Validation

        /// <summary>
        /// Validate all required components are in scene (plug-in-out: find only).
        /// </summary>
        private static bool ValidateSceneSetup()
        {
            bool isValid = true;

            // Check CompleteMazeBuilder
            var mazeBuilder = FindFirstObjectByType<CompleteMazeBuilder>();
            if (mazeBuilder == null)
            {
                Debug.LogError("  ❌ CompleteMazeBuilder: NOT FOUND");
                isValid = false;
            }
            else
            {
                Debug.Log("  ✅ CompleteMazeBuilder: Found");
            }

            // Check SpatialPlacer
            var spatialPlacer = FindFirstObjectByType<SpatialPlacer>();
            if (spatialPlacer == null)
            {
                Debug.LogError("  ❌ SpatialPlacer: NOT FOUND");
                isValid = false;
            }
            else
            {
                Debug.Log("  ✅ SpatialPlacer: Found");
            }

            // Check LightPlacementEngine
            var lightEngine = FindFirstObjectByType<LightPlacementEngine>();
            if (lightEngine == null)
            {
                Debug.LogError("  ❌ LightPlacementEngine: NOT FOUND");
                isValid = false;
            }
            else
            {
                Debug.Log("  ✅ LightPlacementEngine: Found");
            }

            // Check TorchPool
            var torchPool = FindFirstObjectByType<TorchPool>();
            if (torchPool == null)
            {
                Debug.LogError("  ❌ TorchPool: NOT FOUND");
                isValid = false;
            }
            else
            {
                Debug.Log("  ✅ TorchPool: Found");
            }

            // Check EventHandler
            var eventHandler = FindFirstObjectByType<EventHandler>();
            if (eventHandler == null)
            {
                Debug.LogError("  ❌ EventHandler: NOT FOUND");
                isValid = false;
            }
            else
            {
                Debug.Log("  ✅ EventHandler: Found");
            }

            // Check PlayerSetup
            var playerSetup = FindFirstObjectByType<PlayerSetup>();
            if (playerSetup == null)
            {
                Debug.LogError("  ❌ PlayerSetup: NOT FOUND (on Player GameObject)");
                isValid = false;
            }
            else
            {
                Debug.Log("  ✅ PlayerSetup: Found");
            }

            return isValid;
        }

        /// <summary>
        /// Validate CompleteMazeBuilder has required component references assigned.
        /// </summary>
        private static bool ValidateMazeBuilderComponents(CompleteMazeBuilder mazeBuilder)
        {
            bool isValid = true;

            // Use serialized properties to check assigned references
            var serializedObject = new SerializedObject(mazeBuilder);
            var spatialPlacerProp = serializedObject.FindProperty("spatialPlacer");
            var lightEngineProp = serializedObject.FindProperty("lightPlacementEngine");
            var torchPoolProp = serializedObject.FindProperty("torchPool");

            if (spatialPlacerProp == null || spatialPlacerProp.objectReferenceValue == null)
            {
                Debug.LogWarning("  ⚠️ CompleteMazeBuilder.spatialPlacer: Not assigned");
                isValid = false;
            }

            if (lightEngineProp == null || lightEngineProp.objectReferenceValue == null)
            {
                Debug.LogWarning("  ⚠️ CompleteMazeBuilder.lightPlacementEngine: Not assigned");
                isValid = false;
            }

            if (torchPoolProp == null || torchPoolProp.objectReferenceValue == null)
            {
                Debug.LogWarning("  ⚠️ CompleteMazeBuilder.torchPool: Not assigned");
                isValid = false;
            }

            return isValid;
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

        #endregion

        #region Editor Window

        [MenuItem("Tools/Maze/Maze Builder Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<MazeBuilderEditor>("Maze Builder");
            window.minSize = new Vector2(350, 450);
        }

        private void OnGUI()
        {
            GUILayout.Label("Maze Builder Editor", EditorStyles.boldLabel);

            GUILayout.Space(10);

            // Scene setup status
            GUILayout.Label("Scene Setup Status", EditorStyles.boldLabel);
            bool isValid = ValidateSceneSetup();

            GUIStyle statusStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = isValid ? Color.green : Color.red }
            };
            GUILayout.Label(isValid ? "✅ READY" : "❌ INCOMPLETE", statusStyle);

            GUILayout.Space(10);

            // Action buttons
            GUILayout.Label("Actions", EditorStyles.boldLabel);

            GUI.enabled = isValid;
            if (GUILayout.Button("Generate Maze", GUILayout.Height(30)))
            {
                GenerateMaze();
            }
            GUI.enabled = true;

            if (GUILayout.Button("Next Level (Harder)"))
            {
                NextLevel();
            }

            if (GUILayout.Button("Clear Maze Objects"))
            {
                ClearMazeObjects();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Validate Scene Setup"))
            {
                ValidateSceneSetupMenuItem();
            }

            GUILayout.Space(20);

            // Documentation
            GUILayout.Label("Documentation", EditorStyles.boldLabel);

            if (GUILayout.Button("Show TODO.md"))
            {
                ShowDocumentation();
            }

            GUILayout.Space(10);

            // Help box
            EditorGUILayout.HelpBox(
                "SCENE SETUP REQUIRED:\n" +
                "1. Add CompleteMazeBuilder to a GameObject\n" +
                "2. Add SpatialPlacer, LightPlacementEngine, TorchPool\n" +
                "3. Add EventHandler\n" +
                "4. Create Player with PlayerSetup + PlayerController + PlayerStats\n" +
                "\nRun: Tools → Quick Setup Prefabs (for testing)",
                MessageType.Info);
        }

        #endregion
    }
}
