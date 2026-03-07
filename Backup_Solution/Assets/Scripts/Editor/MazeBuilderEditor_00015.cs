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
// Editor tool for maze generation - AUTO-SETUP SCENE
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   Tools → Generate Maze (Ctrl+Alt+G)
//   Auto-creates ALL required components if missing
//   Press Play to test
//
// NOTE: This is an EDITOR TOOL - it creates objects for initial setup.
// Runtime code (CompleteMazeBuilder, etc.) follows strict plug-in-out.

using UnityEngine;
using UnityEditor;
using Code.Lavos.Core;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// MazeBuilderEditor - Editor tool for maze generation.
    /// FINDS existing components first, creates only if missing.
    /// </summary>
    public class MazeBuilderEditor : EditorWindow
    {
        #region Menu Items

        [MenuItem("Tools/Maze/Generate Maze %&G")]
        public static void GenerateMaze()
        {
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  MAZE GENERATOR - Auto-Setup & Generation");
            Debug.Log("═══════════════════════════════════════════");

            // Setup scene if needed (finds first, creates if missing)
            SetupScene();

            // Load config
            var config = GameConfig.Instance;
            Debug.Log($"[MazeBuilderEditor]  Config loaded:");
            Debug.Log($"    Maze Size: {config.defaultGridSize}x{config.defaultGridSize}");
            Debug.Log($"    Cell Size: {config.defaultCellSize}m");
            Debug.Log($"    Wall Height: {config.defaultWallHeight}m");

            // Find maze builder (plug-in-out: find only!)
            var mazeBuilder = FindFirstObjectByType<CompleteMazeBuilder>();

            if (mazeBuilder == null)
            {
                Debug.LogError("[MazeBuilderEditor]  CompleteMazeBuilder not found after setup!");
                return;
            }

            // Generate maze
            mazeBuilder.GenerateMaze();

            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("   MAZE GENERATED!");
            Debug.Log($"   Size: {mazeBuilder.MazeSize}x{mazeBuilder.MazeSize}");
            Debug.Log($"   Level: {mazeBuilder.CurrentLevel}");
            Debug.Log("   Press Play to test");
            Debug.Log("═══════════════════════════════════════════");
        }

        [MenuItem("Tools/Maze/Setup Scene")]
        public static void SetupSceneMenuItem()
        {
            SetupScene();
        }

        [MenuItem("Tools/Maze/Next Level (Harder)")]
        public static void NextLevel()
        {
            var mazeBuilder = FindFirstObjectByType<CompleteMazeBuilder>();
            if (mazeBuilder != null)
            {
                mazeBuilder.NextLevel();
                Debug.Log($"[MazeBuilderEditor]  Level {mazeBuilder.CurrentLevel} - Maze {mazeBuilder.MazeSize}x{mazeBuilder.MazeSize}");
            }
            else
            {
                Debug.LogWarning("[MazeBuilderEditor]  No CompleteMazeBuilder! Run: Tools → Generate Maze");
            }
        }

        [MenuItem("Tools/Maze/Clear Maze Objects")]
        public static void ClearMazeObjects()
        {
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  CLEARING MAZE OBJECTS...");
            Debug.Log("═══════════════════════════════════════════");

            CleanUpObject("MazeWalls");
            CleanUpObject("GroundFloor");
            CleanUpObject("Ceiling");
            CleanUpObject("Lights");
            CleanUpObject("Torches");
            CleanUpObject("Doors");
            CleanUpObject("Rooms");

            PlayerPrefs.DeleteKey("MazeSpawnX");
            PlayerPrefs.DeleteKey("MazeSpawnY");
            PlayerPrefs.Save();

            Debug.Log("  Maze objects cleared");
            Debug.Log("═══════════════════════════════════════════");
        }

        #endregion

        #region Scene Setup

        /// <summary>
        /// Auto-setup scene: FINDS existing components first, creates only if missing.
        /// EDITOR TOOL ONLY - runtime code uses strict plug-in-out.
        /// </summary>
        private static void SetupScene()
        {
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  SETTING UP SCENE (Find First, Create If Missing)...");
            Debug.Log("═══════════════════════════════════════════");

            EnsureMazeBuilder();
            EnsureEventHandler();
            EnsurePlayer();

            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("   SCENE SETUP COMPLETE!");
            Debug.Log("═══════════════════════════════════════════");
        }

        private static void EnsureMazeBuilder()
        {
            // FIND first (plug-in-out!)
            var mazeBuilder = FindFirstObjectByType<CompleteMazeBuilder>();

            if (mazeBuilder != null)
            {
                Debug.Log("   CompleteMazeBuilder: Found existing");

                // Ensure required components exist (find first!)
                EnsureComponent<SpatialPlacer>(mazeBuilder.gameObject);
                EnsureComponent<LightPlacementEngine>(mazeBuilder.gameObject);
                EnsureComponent<TorchPool>(mazeBuilder.gameObject);

                // Auto-assign references
                AutoAssignReferences(mazeBuilder);
                return;
            }

            // CREATE only if missing (editor tool convenience)
            Debug.Log("   CompleteMazeBuilder: Not found - creating...");

            GameObject mazeGO = new GameObject("MazeBuilder");
            mazeBuilder = mazeGO.AddComponent<CompleteMazeBuilder>();

            mazeGO.AddComponent<SpatialPlacer>();
            mazeGO.AddComponent<LightPlacementEngine>();
            mazeGO.AddComponent<TorchPool>();

            AutoAssignReferences(mazeBuilder);

            Debug.Log("   Created CompleteMazeBuilder with required components");
        }

        private static void EnsureEventHandler()
        {
            // FIND first (plug-in-out!)
            var eventHandler = FindFirstObjectByType<EventHandler>();

            if (eventHandler != null)
            {
                Debug.Log("   EventHandler: Found existing");
                return;
            }

            // CREATE only if missing (editor tool convenience)
            Debug.Log("   EventHandler: Not found - creating...");

            GameObject eventGO = new GameObject("EventHandler");
            eventGO.AddComponent<EventHandler>();

            Debug.Log("   Created EventHandler");
        }

        private static void EnsurePlayer()
        {
            // FIND first (plug-in-out!)
            var playerSetup = FindFirstObjectByType<PlayerSetup>();

            if (playerSetup != null)
            {
                Debug.Log("   Player: Found existing");
                return;
            }

            // CREATE only if missing (editor tool convenience)
            Debug.Log("   Player: Not found - creating...");

            GameObject playerGO = new GameObject("Player");
            playerGO.transform.position = new Vector3(0, 0, 0);

            playerGO.AddComponent<PlayerSetup>();
            playerGO.AddComponent<PlayerController>();
            playerGO.AddComponent<PlayerStats>();

            // Create Camera child
            GameObject cameraGO = new GameObject("Main Camera");
            cameraGO.transform.SetParent(playerGO.transform);
            cameraGO.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            cameraGO.transform.localRotation = Quaternion.identity;

            var camera = cameraGO.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.fieldOfView = 60f;

            cameraGO.AddComponent<CameraFollow>();

            Debug.Log("   Created Player with Camera (eye height: 1.7m)");
        }

        /// <summary>
        /// Ensure component exists - finds first, adds only if missing.
        /// </summary>
        private static T EnsureComponent<T>(GameObject go) where T : Component
        {
            var existing = go.GetComponent<T>();
            if (existing != null)
            {
                Debug.Log($"     {typeof(T).Name}: Found existing");
                return existing;
            }

            Debug.Log($"     {typeof(T).Name}: Not found - adding...");
            return go.AddComponent<T>();
        }

        /// <summary>
        /// Auto-assign component references via SerializedObject.
        /// </summary>
        private static void AutoAssignReferences(CompleteMazeBuilder mazeBuilder)
        {
            var serializedObject = new SerializedObject(mazeBuilder);

            var spatialPlacerProp = serializedObject.FindProperty("spatialPlacer");
            var lightEngineProp = serializedObject.FindProperty("lightPlacementEngine");
            var torchPoolProp = serializedObject.FindProperty("torchPool");

            var spatialPlacer = mazeBuilder.GetComponent<SpatialPlacer>();
            var lightEngine = mazeBuilder.GetComponent<LightPlacementEngine>();
            var torchPool = mazeBuilder.GetComponent<TorchPool>();

            if (spatialPlacerProp != null && spatialPlacer != null)
            {
                spatialPlacerProp.objectReferenceValue = spatialPlacer;
                Debug.Log("   Auto-assigned: spatialPlacer");
            }

            if (lightEngineProp != null && lightEngine != null)
            {
                lightEngineProp.objectReferenceValue = lightEngine;
                Debug.Log("   Auto-assigned: lightPlacementEngine");
            }

            if (torchPoolProp != null && torchPool != null)
            {
                torchPoolProp.objectReferenceValue = torchPool;
                Debug.Log("   Auto-assigned: torchPool");
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Utilities

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
            window.minSize = new Vector2(350, 400);
        }

        private void OnGUI()
        {
            GUILayout.Label("Maze Builder Editor", EditorStyles.boldLabel);

            GUILayout.Space(15);

            if (GUILayout.Button("Generate Maze", GUILayout.Height(35)))
            {
                GenerateMaze();
            }

            if (GUILayout.Button("Setup Scene (Find First)"))
            {
                SetupSceneMenuItem();
            }

            if (GUILayout.Button("Next Level (Harder)"))
            {
                NextLevel();
            }

            if (GUILayout.Button("Clear Maze Objects"))
            {
                ClearMazeObjects();
            }

            GUILayout.Space(20);

            EditorGUILayout.HelpBox(
                "Click 'Generate Maze' to:\n" +
                "1. Find existing components (plug-in-out)\n" +
                "2. Create missing components (editor tool)\n" +
                "3. Generate maze instantly\n" +
                "4. Press Play to test\n\n" +
                "Shortcut: Ctrl+Alt+G\n\n" +
                "NOTE: Runtime code follows strict plug-in-out.\n" +
                "This editor tool creates only for convenience.",
                MessageType.Info);
        }

        #endregion
    }
}
