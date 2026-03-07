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
        /// Also fills prefabs and materials from GameConfig defaults.
        /// </summary>
        private static void AutoAssignReferences(CompleteMazeBuilder mazeBuilder)
        {
            var serializedObject = new SerializedObject(mazeBuilder);

            // Component references
            var spatialPlacerProp = serializedObject.FindProperty("spatialPlacer");
            var lightEngineProp = serializedObject.FindProperty("lightPlacementEngine");
            var torchPoolProp = serializedObject.FindProperty("torchPool");
            var mazeRendererProp = serializedObject.FindProperty("mazeRenderer");

            var spatialPlacer = mazeBuilder.GetComponent<SpatialPlacer>();
            var lightEngine = mazeBuilder.GetComponent<LightPlacementEngine>();
            var torchPool = mazeBuilder.GetComponent<TorchPool>();
            var mazeRenderer = mazeBuilder.GetComponent<MazeRenderer>();

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

            if (mazeRendererProp != null && mazeRenderer != null)
            {
                mazeRendererProp.objectReferenceValue = mazeRenderer;
                Debug.Log("   Auto-assigned: mazeRenderer");
            }

            // Prefab and material references from GameConfig
            AutoAssignPrefabsFromConfig(serializedObject);

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Auto-assign prefabs and materials from GameConfig defaults.
        /// Prevents pink missing textures and ensures proper generation.
        /// </summary>
        private static void AutoAssignPrefabsFromConfig(SerializedObject serializedObject)
        {
            var config = GameConfig.Instance;

            // Wall Prefab
            var wallPrefabProp = serializedObject.FindProperty("wallPrefab");
            if (wallPrefabProp != null && wallPrefabProp.objectReferenceValue == null)
            {
                wallPrefabProp.objectReferenceValue = LoadPrefabFromConfig(config.wallPrefab, "Wall");
                if (wallPrefabProp.objectReferenceValue != null)
                    Debug.Log($"   Auto-assigned: wallPrefab from config ({config.wallPrefab})");
            }

            // Door Prefab
            var doorPrefabProp = serializedObject.FindProperty("doorPrefab");
            if (doorPrefabProp != null && doorPrefabProp.objectReferenceValue == null)
            {
                doorPrefabProp.objectReferenceValue = LoadPrefabFromConfig(config.doorPrefab, "Door");
                if (doorPrefabProp.objectReferenceValue != null)
                    Debug.Log($"   Auto-assigned: doorPrefab from config ({config.doorPrefab})");
            }

            // Wall Material
            var wallMaterialProp = serializedObject.FindProperty("wallMaterial");
            if (wallMaterialProp != null && wallMaterialProp.objectReferenceValue == null)
            {
                wallMaterialProp.objectReferenceValue = LoadMaterialFromConfig(config.wallMaterial, "Wall");
                if (wallMaterialProp.objectReferenceValue != null)
                    Debug.Log($"   Auto-assigned: wallMaterial from config ({config.wallMaterial})");
            }

            // Floor Material
            var floorMaterialProp = serializedObject.FindProperty("floorMaterial");
            if (floorMaterialProp != null && floorMaterialProp.objectReferenceValue == null)
            {
                floorMaterialProp.objectReferenceValue = LoadMaterialFromConfig(config.floorMaterial, "Floor");
                if (floorMaterialProp.objectReferenceValue != null)
                    Debug.Log($"   Auto-assigned: floorMaterial from config ({config.floorMaterial})");
            }

            // Ground Texture
            var groundTextureProp = serializedObject.FindProperty("groundTexture");
            if (groundTextureProp != null && groundTextureProp.objectReferenceValue == null)
            {
                groundTextureProp.objectReferenceValue = LoadTextureFromConfig(config.groundTexture, "Floor");
                if (groundTextureProp.objectReferenceValue != null)
                    Debug.Log($"   Auto-assigned: groundTexture from config ({config.groundTexture})");
            }
        }

        /// <summary>
        /// Load prefab from config path with fallback search.
        /// </summary>
        private static UnityEngine.Object LoadPrefabFromConfig(string configPath, string searchName)
        {
            // Try config path first
            if (!string.IsNullOrEmpty(configPath))
            {
                string resourcePath = configPath.Replace("Assets/Resources/", "").Replace(".prefab", "");
                UnityEngine.Object prefab = Resources.Load(resourcePath);
                if (prefab != null)
                {
                    return prefab;
                }
            }

            // Fallback: search in Resources subfolders
            string[] folders = { "Prefabs", "Prefabs/Walls", "Prefabs/Doors", "" };
            foreach (string folder in folders)
            {
                string searchPath = string.IsNullOrEmpty(folder) ? searchName : $"{folder}/{searchName}";
                UnityEngine.Object prefab = Resources.Load(searchPath);
                if (prefab != null)
                {
                    return prefab;
                }
            }

            // Fallback: search all Resources by name
            UnityEngine.Object[] allPrefabs = Resources.LoadAll("");
            foreach (UnityEngine.Object p in allPrefabs)
            {
                if (p != null && p.name.Contains(searchName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return p;
                }
            }

            Debug.LogWarning($"[MazeBuilderEditor] Prefab '{searchName}' not found in Resources!");
            return null;
        }

        /// <summary>
        /// Load material from config path with fallback search.
        /// </summary>
        private static UnityEngine.Object LoadMaterialFromConfig(string configPath, string searchName)
        {
            // Try config path first
            if (!string.IsNullOrEmpty(configPath))
            {
                string resourcePath = configPath.Replace("Assets/Resources/", "").Replace(".mat", "");
                UnityEngine.Object mat = Resources.Load(resourcePath);
                if (mat != null)
                {
                    return mat;
                }
            }

            // Fallback: search in Resources subfolders
            string[] folders = { "Materials", "Materials/Walls", "Materials/Floors", "" };
            foreach (string folder in folders)
            {
                string searchPath = string.IsNullOrEmpty(folder) ? searchName : $"{folder}/{searchName}";
                UnityEngine.Object mat = Resources.Load(searchPath);
                if (mat != null)
                {
                    return mat;
                }
            }

            Debug.LogWarning($"[MazeBuilderEditor] Material '{searchName}' not found in Resources!");
            return null;
        }

        /// <summary>
        /// Load texture from config path with fallback search.
        /// </summary>
        private static UnityEngine.Object LoadTextureFromConfig(string configPath, string searchName)
        {
            // Try config path first
            if (!string.IsNullOrEmpty(configPath))
            {
                string resourcePath = configPath.Replace("Assets/Resources/", "").Replace(".png", "").Replace(".jpg", "");
                UnityEngine.Object tex = Resources.Load(resourcePath);
                if (tex != null)
                {
                    return tex;
                }
            }

            // Fallback: search in Resources subfolders
            string[] folders = { "Textures", "Textures/Floors", "Textures/Walls", "" };
            foreach (string folder in folders)
            {
                string searchPath = string.IsNullOrEmpty(folder) ? searchName : $"{folder}/{searchName}";
                UnityEngine.Object tex = Resources.Load(searchPath);
                if (tex != null)
                {
                    return tex;
                }
            }

            Debug.LogWarning($"[MazeBuilderEditor] Texture '{searchName}' not found in Resources!");
            return null;
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
