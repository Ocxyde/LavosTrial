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
// MazeBuilderEditor.cs
// Editor tool for maze generation testing
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   1. Tools → Generate Maze (or Ctrl+Alt+G)
//   2. Auto-creates CompleteMazeBuilder + required components + PlayerSetup
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
    /// Auto-creates CompleteMazeBuilder, required components, and PlayerSetup for quick testing.
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
            Debug.Log($"[MazeBuilderEditor]  Config loaded from JSON:");
            Debug.Log($"  • Maze Size: {config.defaultGridSize}x{config.defaultGridSize}");
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
                Debug.Log(" Created MazeBuilder GameObject");

                // Add required components for testing (editor tool only!)
                var spatialPlacer = mazeGO.AddComponent<SpatialPlacer>();
                var lightPlacementEngine = mazeGO.AddComponent<LightPlacementEngine>();
                var torchPool = mazeGO.AddComponent<TorchPool>();
                Debug.Log(" Added required components");

                // Configure components from JSON config
                Debug.Log("[MazeBuilderEditor]  Configuring components from JSON...");

                // LightPlacementEngine - load torch prefab from Resources
                var torchPrefab = Resources.Load<GameObject>(config.torchPrefab.Replace(".prefab", ""));
                if (torchPrefab != null)
                {
                    lightPlacementEngine.SetTorchPrefab(torchPrefab);
                    Debug.Log($"  • LightPlacementEngine: torch prefab loaded");
                }
                else
                {
                    Debug.LogWarning($"  ️ LightPlacementEngine: torch prefab not found");
                    Debug.LogWarning($"   Run: Tools → Quick Setup Prefabs");
                }

                // TorchPool - auto-initializes
                Debug.Log($"  • TorchPool: ready");

                // Also add EventHandler if not present
                var eventHandler = FindFirstObjectByType<EventHandler>();
                if (eventHandler == null)
                {
                    var eventGO = new GameObject("EventHandler");
                    eventHandler = eventGO.AddComponent<EventHandler>();
                    Debug.Log(" Created EventHandler");
                }

                Debug.Log(" All components configured from JSON!");
            }
            else
            {
                Debug.Log(" Found existing CompleteMazeBuilder");
            }

            // Ensure player exists with PlayerSetup component
            EnsurePlayerWithSetup();

            // Generate maze
            mazeBuilder.GenerateMaze();

            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("   MAZE GENERATED!");
            Debug.Log($"   Maze Size: {mazeBuilder.MazeSize}x{mazeBuilder.MazeSize}");
            Debug.Log($"   Level: {mazeBuilder.CurrentLevel}");
            Debug.Log("   Press Play to test");
            Debug.Log("═══════════════════════════════════════════");
        }

        /// <summary>
        /// Ensure player exists with PlayerSetup component for proper initialization.
        /// Creates player GameObject with PlayerSetup, PlayerController, PlayerStats, and Camera.
        /// </summary>
        private static void EnsurePlayerWithSetup()
        {
            // Check if player already exists
            var existingPlayer = FindFirstObjectByType<PlayerSetup>();
            
            if (existingPlayer != null)
            {
                Debug.Log(" Player with PlayerSetup already in scene");
                return;
            }

            // Create player GameObject
            GameObject playerGO = new GameObject("Player");
            playerGO.transform.position = Vector3.zero;
            playerGO.transform.rotation = Quaternion.identity;

            // Add PlayerSetup (orchestrates player initialization)
            var playerSetup = playerGO.AddComponent<PlayerSetup>();
            Debug.Log(" Added PlayerSetup component");

            // Add required components for PlayerSetup
            playerGO.AddComponent<PlayerController>();
            Debug.Log(" Added PlayerController");

            playerGO.AddComponent<PlayerStats>();
            Debug.Log(" Added PlayerStats");

            // Create camera as child
            GameObject cameraGO = new GameObject("Main Camera");
            cameraGO.transform.SetParent(playerGO.transform);
            cameraGO.transform.localPosition = new Vector3(0f, 1.7f, 0f);  // Eye height
            cameraGO.transform.localRotation = Quaternion.identity;

            // Add Camera component
            var camera = cameraGO.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.fieldOfView = 60f;

            // Add CameraFollow for smooth camera movement
            cameraGO.AddComponent<CameraFollow>();
            Debug.Log(" Added Main Camera with CameraFollow (eye height: 1.7m)");

            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("   PLAYER CREATED WITH PLAYERSETUP!");
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  PlayerSetup will handle:");
            Debug.Log("    • Component initialization");
            Debug.Log("    • Camera positioning");
            Debug.Log("    • Event subscription");
            Debug.Log("    • Spawn point positioning");
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
                Debug.LogWarning("[MazeBuilderEditor] ️ No CompleteMazeBuilder found in scene!");
            }
        }

        [MenuItem("Tools/Maze/Validate Paths")]
        public static void ValidatePaths()
        {
            Debug.Log("[MazeBuilderEditor] ℹ️ Path validation is now automatic - prefabs loaded from JSON config");
            Debug.Log("[MazeBuilderEditor]  If maze generates, paths are valid!");
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
            Debug.Log("   Spawn position cleared");

            // Don't delete MazeBuilder or Player
            Debug.Log("   Maze objects cleared");
            Debug.Log("═══════════════════════════════════════════");
        }

        [MenuItem("Tools/Maze/Show Documentation")]
        public static void ShowDocumentation()
        {
            string docPath = "Assets/Docs/README.md";

            if (System.IO.File.Exists(docPath))
            {
                UnityEditor.EditorUtility.RevealInFinder(docPath);
                Debug.Log("[MazeBuilderEditor]  Opened documentation");
            }
            else
            {
                Debug.LogWarning("[MazeBuilderEditor] ️ Documentation not found at: " + docPath);
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

            if (GUILayout.Button("Next Level (Harder)"))
            {
                NextLevel();
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
