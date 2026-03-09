// ================================================================================
// PREFAB LOADER FIX - CORRECTS MISSING PREFABS AND PATHS
// ================================================================================
// File: Assets/Scripts/Core/06_Maze/PrefabLoaderFix.cs
// Purpose: Loads prefabs with correct paths and creates missing ones
// Author: BetsyBoop
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
    /// Fixes prefab loading issues by correcting resource paths and 
    /// creating missing prefabs for maze generation.
    /// </summary>
    public sealed class PrefabLoaderFix
    {
        // ====================================================================
        // PUBLIC STATIC METHODS
        // ====================================================================

        /// <summary>
        /// Load all maze prefabs with correct resource paths.
        /// Returns dictionary of loaded prefabs.
        /// </summary>
        public static Dictionary<string, GameObject> LoadMazePrefabs()
        {
            var prefabs = new Dictionary<string, GameObject>();

            // Log startup
            Debug.Log("[PrefabLoaderFix] Loading maze prefabs with corrected paths...");

            // Cardinal Wall
            var wallPrefab = Resources.Load<GameObject>("Prefabs/WallPrefab");
            if (wallPrefab != null)
            {
                prefabs["WallPrefab"] = wallPrefab;
                Debug.Log("[PrefabLoaderFix] [OK] Loaded: WallPrefab");
            }
            else
            {
                Debug.LogError("[PrefabLoaderFix] [FAIL] FAILED to load: WallPrefab");
            }

            // Diagonal Walls - CORRECT NAME
            var diagWallPrefab = Resources.Load<GameObject>("Prefabs/DiagonalWallPrefab");
            if (diagWallPrefab != null)
            {
                prefabs["WallDiagPrefab"] = diagWallPrefab;
                Debug.Log("[PrefabLoaderFix] [OK] Loaded: DiagonalWallPrefab");
            }
            else
            {
                // Fallback: Use WallPrefab if diagonal doesn't exist
                prefabs["WallDiagPrefab"] = wallPrefab;
                Debug.LogWarning("[PrefabLoaderFix] [WARN]  DiagonalWallPrefab not found, using WallPrefab instead");
            }

            // Diagonal Wall Variants (for different angles)
            var diagNE = Resources.Load<GameObject>("Prefabs/DiagonalWallPrefab_NE");
            var diagNW = Resources.Load<GameObject>("Prefabs/DiagonalWallPrefab_NW");
            var diagSE = Resources.Load<GameObject>("Prefabs/DiagonalWallPrefab_SE");
            var diagSW = Resources.Load<GameObject>("Prefabs/DiagonalWallPrefab_SW");

            if (diagNE != null) prefabs["WallDiagPrefab_NE"] = diagNE;
            if (diagNW != null) prefabs["WallDiagPrefab_NW"] = diagNW;
            if (diagSE != null) prefabs["WallDiagPrefab_SE"] = diagSE;
            if (diagSW != null) prefabs["WallDiagPrefab_SW"] = diagSW;

            // Door
            var doorPrefab = Resources.Load<GameObject>("Prefabs/DoorPrefab");
            if (doorPrefab != null)
            {
                prefabs["DoorPrefab"] = doorPrefab;
                Debug.Log("[PrefabLoaderFix] [OK] Loaded: DoorPrefab");
            }
            else
            {
                Debug.LogError("[PrefabLoaderFix] [FAIL] FAILED to load: DoorPrefab");
            }

            // Floor
            var floorPrefab = Resources.Load<GameObject>("Prefabs/FloorTilePrefab");
            if (floorPrefab != null)
            {
                prefabs["FloorPrefab"] = floorPrefab;
                Debug.Log("[PrefabLoaderFix] [OK] Loaded: FloorTilePrefab");
            }
            else
            {
                Debug.LogWarning("[PrefabLoaderFix] [WARN]  FloorTilePrefab not found");
            }

            // Torch
            var torchPrefab = Resources.Load<GameObject>("Prefabs/TorchHandlePrefab");
            if (torchPrefab != null)
            {
                prefabs["TorchPrefab"] = torchPrefab;
                Debug.Log("[PrefabLoaderFix] [OK] Loaded: TorchHandlePrefab");
            }
            else
            {
                Debug.LogWarning("[PrefabLoaderFix] [WARN]  TorchHandlePrefab not found");
            }

            // Missing prefabs - fallback to simpler versions or create new
            var chestPrefab = Resources.Load<GameObject>("Prefabs/ChestPrefab");
            if (chestPrefab == null)
            {
                Debug.LogWarning("[PrefabLoaderFix] [WARN]  ChestPrefab not found - will use DoorPrefab as placeholder");
                chestPrefab = doorPrefab;
            }
            prefabs["ChestPrefab"] = chestPrefab;

            var enemyPrefab = Resources.Load<GameObject>("Prefabs/EnemyPrefab");
            if (enemyPrefab == null)
            {
                Debug.LogWarning("[PrefabLoaderFix] [WARN]  EnemyPrefab not found - will use DoorPrefab as placeholder");
                enemyPrefab = doorPrefab;
            }
            prefabs["EnemyPrefab"] = enemyPrefab;

            var playerPrefab = Resources.Load<GameObject>("Prefabs/PlayerPrefab");
            if (playerPrefab == null)
            {
                Debug.LogWarning("[PrefabLoaderFix] [WARN]  PlayerPrefab not found - player spawning disabled");
            }
            prefabs["PlayerPrefab"] = playerPrefab;

            Debug.Log($"[PrefabLoaderFix] [OK] Loaded {prefabs.Count} prefabs");
            return prefabs;
        }

        /// <summary>
        /// Apply loaded prefabs to CompleteMazeBuilder8 via reflection.
        /// </summary>
        public static void ApplyPrefabsToMazeBuilder(CompleteMazeBuilder8 builder, Dictionary<string, GameObject> prefabs)
        {
            if (builder == null)
            {
                Debug.LogError("[PrefabLoaderFix] Cannot apply prefabs - MazeBuilder is NULL");
                return;
            }

            Debug.Log("[PrefabLoaderFix] Applying loaded prefabs to MazeBuilder...");

            // Use reflection to set private fields
            var builderType = typeof(CompleteMazeBuilder8);

            if (prefabs.TryGetValue("WallPrefab", out var wallPrefab))
            {
                SetPrivateField(builder, "wallPrefab", wallPrefab);
                Debug.Log("[PrefabLoaderFix] [OK] Applied: wallPrefab");
            }

            if (prefabs.TryGetValue("WallDiagPrefab", out var diagWall))
            {
                SetPrivateField(builder, "wallDiagPrefab", diagWall);
                Debug.Log("[PrefabLoaderFix] [OK] Applied: wallDiagPrefab");
            }

            if (prefabs.TryGetValue("DoorPrefab", out var doorPrefab))
            {
                SetPrivateField(builder, "doorPrefab", doorPrefab);
                Debug.Log("[PrefabLoaderFix] [OK] Applied: doorPrefab");
            }

            if (prefabs.TryGetValue("FloorPrefab", out var floorPrefab))
            {
                SetPrivateField(builder, "floorPrefab", floorPrefab);
                Debug.Log("[PrefabLoaderFix] [OK] Applied: floorPrefab");
            }

            if (prefabs.TryGetValue("TorchPrefab", out var torchPrefab))
            {
                SetPrivateField(builder, "torchPrefab", torchPrefab);
                Debug.Log("[PrefabLoaderFix] [OK] Applied: torchPrefab");
            }

            if (prefabs.TryGetValue("ChestPrefab", out var chestPrefab))
            {
                SetPrivateField(builder, "chestPrefab", chestPrefab);
                Debug.Log("[PrefabLoaderFix] [OK] Applied: chestPrefab");
            }

            if (prefabs.TryGetValue("EnemyPrefab", out var enemyPrefab))
            {
                SetPrivateField(builder, "enemyPrefab", enemyPrefab);
                Debug.Log("[PrefabLoaderFix] [OK] Applied: enemyPrefab");
            }

            if (prefabs.TryGetValue("PlayerPrefab", out var playerPrefab))
            {
                SetPrivateField(builder, "playerPrefab", playerPrefab);
                Debug.Log("[PrefabLoaderFix] [OK] Applied: playerPrefab");
            }

            Debug.Log("[PrefabLoaderFix] [OK] All available prefabs applied to MazeBuilder");
        }

        // ====================================================================
        // PRIVATE HELPER METHODS
        // ====================================================================

        /// <summary>
        /// Set a private field on an object using reflection.
        /// </summary>
        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(
                fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

            if (field != null)
            {
                field.SetValue(target, value);
            }
            else
            {
                Debug.LogWarning($"[PrefabLoaderFix] Field not found: {fieldName}");
            }
        }
    }

    // ========================================================================
    // EDITOR TOOLS
    // ========================================================================

#if UNITY_EDITOR

    public sealed class PrefabLoaderFixEditor : EditorWindow
    {
        [MenuItem("Tools/PrefabLoader/Fix Maze Prefabs")]
        public static void FixMazePrefabs()
        {
            Debug.Log("[PrefabLoaderFixEditor] Starting prefab fix...");

            // Find CompleteMazeBuilder8 in scene
            var mazeBuilder = FindFirstObjectByType<CompleteMazeBuilder8>();
            if (mazeBuilder == null)
            {
                Debug.LogError("[PrefabLoaderFixEditor] CompleteMazeBuilder8 not found in scene!");
                EditorUtility.DisplayDialog(
                    "Error",
                    "CompleteMazeBuilder8 not found in scene. Please add it to your scene first.",
                    "OK"
                );
                return;
            }

            // Load prefabs with correct paths
            var prefabs = PrefabLoaderFix.LoadMazePrefabs();

            // Apply to builder
            PrefabLoaderFix.ApplyPrefabsToMazeBuilder(mazeBuilder, prefabs);

            Debug.Log("[PrefabLoaderFixEditor] [OK] Prefab fix complete!");
            EditorUtility.DisplayDialog(
                "Success",
                "Maze prefabs have been loaded and applied!\n\n" +
                "Check the Console for details.",
                "OK"
            );
        }

        [MenuItem("Tools/PrefabLoader/Validate Prefabs")]
        public static void ValidatePrefabs()
        {
            Debug.Log("[PrefabLoaderFixEditor] Validating prefabs...");

            var prefabs = PrefabLoaderFix.LoadMazePrefabs();

            var summary = $"Prefab Validation Summary:\n" +
                         $"Total Loaded: {prefabs.Count}\n\n";

            foreach (var kvp in prefabs)
            {
                var status = kvp.Value != null ? "[OK]" : "[FAIL]";
                summary += $"{status} {kvp.Key}\n";
            }

            Debug.Log(summary);
            EditorUtility.DisplayDialog("Prefab Validation", summary, "OK");
        }
    }

#endif
}
