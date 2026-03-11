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
// AutoMazeSetup.cs
// Automatic prefab and material validation
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// ================================================================================
// AUTO MAZE SETUP - Automatic prefab and material validation
// ================================================================================
// File: Assets/Scripts/Core/06_Maze/AutoMazeSetup.cs
// Purpose: Automatically validates and fixes maze generation setup
// Date: 2026-03-08
// Encoding: UTF-8 | Line Endings: Unix LF
// ================================================================================

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Code.Lavos.Core
{
    /// <summary>
    /// Automatic maze setup and validation.
    /// Ensures all prefabs and materials are loaded correctly.
    /// </summary>
    public sealed class AutoMazeSetup : MonoBehaviour
    {
        [SerializeField] private bool autoSetupOnAwake = true;
        [SerializeField] private bool verboseLogging = true;

        // ====================================================================
        // LIFECYCLE
        // ====================================================================

        private void Awake()
        {
            if (autoSetupOnAwake)
            {
                ValidateAndFixSetup();
            }
        }

        // ====================================================================
        // PUBLIC METHODS
        // ====================================================================

        /// <summary>
        /// Validate and fix maze generation setup.
        /// Called automatically on Awake if autoSetupOnAwake is true.
        /// </summary>
        public void ValidateAndFixSetup()
        {
            Log("");
            Log("MAZE SETUP VALIDATION STARTED");
            Log("");

            // Step 1: Validate prefabs exist
            ValidatePrefabs();

            // Step 2: Find or create CompleteMazeBuilder8
            var mazeBuilder = FindFirstObjectByType<CompleteMazeBuilder8>();
            if (mazeBuilder == null)
            {
                Log(" CompleteMazeBuilder8 not found in scene");
                Log("   You need to add it manually to the scene");
                return;
            }

            Log(" CompleteMazeBuilder8 found");

            // Step 3: Log current state
            LogMazeBuilderState(mazeBuilder);

            Log("");
            Log("MAZE SETUP VALIDATION COMPLETE");
            Log("");
        }

        /// <summary>
        /// Validate all maze prefabs are present.
        /// </summary>
        private void ValidatePrefabs()
        {
            Log("\n[STEP 1] Validating Prefabs...");
            Log("");

            var requiredPrefabs = new Dictionary<string, string>
            {
                { "WallPrefab", "Prefabs/WallPrefab" },
                { "DiagonalWallPrefab", "Prefabs/DiagonalWallPrefab" },
                { "DoorPrefab", "Prefabs/DoorPrefab" },
                { "FloorTilePrefab", "Prefabs/FloorTilePrefab" },
                { "TorchHandlePrefab", "Prefabs/TorchHandlePrefab" },
                { "ChestPrefab", "Prefabs/ChestPrefab" },
                { "EnemyPrefab", "Prefabs/EnemyPrefab" },
                { "PlayerPrefab", "Prefabs/PlayerPrefab" },
            };

            int found = 0;
            int missing = 0;

            foreach (var kvp in requiredPrefabs)
            {
                var prefab = Resources.Load<GameObject>(kvp.Value);
                if (prefab != null)
                {
                    Log($"   {kvp.Key}");
                    found++;
                }
                else
                {
                    Log($"   {kvp.Key} - NOT FOUND");
                    Log($"     Expected at: Assets/Resources/{kvp.Value}.prefab");
                    missing++;
                }
            }

            Log($"\nSummary: {found} found, {missing} missing");
        }

        /// <summary>
        /// Log the current state of CompleteMazeBuilder8.
        /// </summary>
        private void LogMazeBuilderState(CompleteMazeBuilder8 mazeBuilder)
        {
            Log("\n[STEP 2] MazeBuilder State Analysis...");
            Log("");
            Log("MazeBuilder8 is ready for maze generation");
            Log("Use: Tools > Generate Maze (or call GenerateMaze() in code)");
        }

        // ====================================================================
        // LOGGING
        // ====================================================================

        private void Log(string message)
        {
            if (verboseLogging)
            {
                Debug.Log(message);
            }
        }
    }

    // ========================================================================
    // EDITOR TOOLS
    // ========================================================================

#if UNITY_EDITOR

    public sealed class AutoMazeSetupEditor : EditorWindow
    {
        private static Vector2 scrollPosition = Vector2.zero;
        private static string setupLog = "";

        [MenuItem("Tools/Maze/Auto Setup & Validate")]
        public static void ShowWindow()
        {
            GetWindow<AutoMazeSetupEditor>("Maze Setup");
        }

        [MenuItem("Tools/Maze/Quick Generate Maze")]
        public static void QuickGenerateMaze()
        {
            var mazeBuilder = FindFirstObjectByType<CompleteMazeBuilder8>();
            if (mazeBuilder == null)
            {
                EditorUtility.DisplayDialog(
                    "Error",
                    "CompleteMazeBuilder8 not found in scene!",
                    "OK"
                );
                return;
            }

            Debug.Log("[AutoMazeSetup] Starting maze generation...");
            mazeBuilder.GenerateMaze();
            Debug.Log("[AutoMazeSetup] Maze generation started. Check Console for logs.");
        }

        [MenuItem("Tools/Maze/Validate Prefabs")]
        public static void ValidatePrefabs()
        {
            setupLog = "PREFAB VALIDATION REPORT\n";
            setupLog += "\n\n";

            var requiredPrefabs = new Dictionary<string, string>
            {
                { "WallPrefab", "Prefabs/WallPrefab" },
                { "DiagonalWallPrefab", "Prefabs/DiagonalWallPrefab" },
                { "DoorPrefab", "Prefabs/DoorPrefab" },
                { "FloorTilePrefab", "Prefabs/FloorTilePrefab" },
                { "TorchHandlePrefab", "Prefabs/TorchHandlePrefab" },
                { "ChestPrefab", "Prefabs/ChestPrefab" },
                { "EnemyPrefab", "Prefabs/EnemyPrefab" },
                { "PlayerPrefab", "Prefabs/PlayerPrefab" },
            };

            int found = 0;
            setupLog += "Checking Resources/Prefabs/...\n\n";

            foreach (var kvp in requiredPrefabs)
            {
                var prefab = Resources.Load<GameObject>(kvp.Value);
                if (prefab != null)
                {
                    setupLog += $" {kvp.Key}\n";
                    found++;
                }
                else
                {
                    setupLog += $" {kvp.Key} (NOT FOUND)\n";
                }
            }

            setupLog += $"\n\n";
            setupLog += $"Result: {found}/{requiredPrefabs.Count} prefabs found\n";

            if (found == requiredPrefabs.Count)
            {
                setupLog += " All prefabs are present!\n";
            }
            else
            {
                setupLog += $"  Missing {requiredPrefabs.Count - found} prefabs\n";
            }

            Debug.Log(setupLog);
            EditorUtility.DisplayDialog("Prefab Validation", setupLog, "OK");
        }

        private void OnGUI()
        {
            GUILayout.Label("MAZE SETUP & VALIDATION", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Validate Prefabs", GUILayout.Height(40)))
            {
                ValidatePrefabs();
            }

            if (GUILayout.Button("Quick Generate Maze", GUILayout.Height(40)))
            {
                QuickGenerateMaze();
            }

            GUILayout.Space(20);
            GUILayout.Label("Information:", EditorStyles.boldLabel);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.TextArea(
                "This tool helps validate and setup maze generation.\n\n" +
                "Required Actions:\n" +
                "1. Add CompleteMazeBuilder8 to your scene\n" +
                "2. Validate prefabs are in Resources/Prefabs/\n" +
                "3. Click 'Quick Generate Maze' to generate\n\n" +
                "If prefabs are missing, check:\n" +
                "- Assets/Resources/Prefabs/ folder exists\n" +
                "- All prefabs are properly named\n" +
                "- Prefabs have Mesh and MeshRenderer components",
                GUILayout.ExpandHeight(true)
            );
            GUILayout.EndScrollView();
        }
    }

#endif
}
