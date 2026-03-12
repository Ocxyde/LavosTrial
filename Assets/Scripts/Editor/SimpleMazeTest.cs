// Copyright (C) 2026 Ocxyde
// GPL-3.0 license
// SimpleMazeTest.cs - Minimal 1-Click Maze with Spotlight
// Unity 6 compatible - UTF-8 encoding - Unix LF
//
// USAGE:
//   Menu: Tools → Maze → Simple Test (Spotlight)
//   Creates minimal maze with single spotlight for testing
//
// PURPOSE:
//   Quick validation that basic maze generation works
//   No complex systems - just walls, floor, and light

using UnityEngine;
using UnityEditor;
using Code.Lavos.Core;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// SimpleMazeTest - Minimal 1-click maze generation.
    /// Creates basic maze with spotlight for quick testing.
    /// </summary>
    public class SimpleMazeTest : MonoBehaviour
    {
        #region Menu Item

        [MenuItem("Tools/Maze/Simple Test (Spotlight)")]
        public static void SimpleTest()
        {
            Debug.Log("");
            Debug.Log("═══ SIMPLE MAZE TEST (Spotlight) ═══");
            Debug.Log("");

            // Step 1: Clean scene
            Debug.Log("[1/4] Cleaning scene...");
            CleanupScene();

            // Step 2: Create minimal components
            Debug.Log("[2/4] Creating minimal components...");
            SetupMinimalComponents();

            // Step 3: Generate maze
            Debug.Log("[3/4] Generating maze...");
            GenerateSimpleMaze();

            // Step 4: Add spotlight
            Debug.Log("[4/4] Adding spotlight...");
            AddSpotlight();

            Debug.Log("");
            Debug.Log("✅ SIMPLE MAZE READY - Press Play!");
            Debug.Log("═══ ═══ ═══ ═══ ═══ ═══ ═══ ═══");
            Debug.Log("");
        }

        #endregion

        #region Cleanup

        private static void CleanupScene()
        {
            // Destroy existing maze objects
            var walls = GameObject.FindGameObjectsWithTag("Wall");
            foreach (var wall in walls) DestroyImmediate(wall);

            var floors = GameObject.FindObjectsOfType<MeshRenderer>();
            foreach (var floor in floors)
            {
                if (floor.gameObject.name.Contains("Floor") || floor.gameObject.name.Contains("Ground"))
                    DestroyImmediate(floor.gameObject);
            }

            Debug.Log("  ✓ Scene cleaned");
        }

        #endregion

        #region Minimal Setup

        private static void SetupMinimalComponents()
        {
            // Create GameManager
            if (FindFirstObjectByType<GameManager>() == null)
            {
                var go = new GameObject("GameManager");
                go.AddComponent<GameManager>();
            }

            // Create EventHandler
            if (FindFirstObjectByType<EventHandler>() == null)
            {
                var go = new GameObject("EventHandler");
                go.AddComponent<EventHandler>();
            }

            // Create CompleteMazeBuilder8
            var mazeBuilder = FindFirstObjectByType<CompleteMazeBuilder8>();
            if (mazeBuilder == null)
            {
                var go = new GameObject("MazeBuilder");
                mazeBuilder = go.AddComponent<CompleteMazeBuilder8>();
            }

            // Configure for simple generation
            mazeBuilder.UseGuaranteedPathGenerator = false;
            mazeBuilder.UsePassageFirstGenerator = false;

            Debug.Log("  ✓ Components created");
        }

        #endregion

        #region Generate Maze

        private static void GenerateSimpleMaze()
        {
            var mazeBuilder = FindFirstObjectByType<CompleteMazeBuilder8>();
            if (mazeBuilder != null)
            {
                // Set simple config
                mazeBuilder.SetLevelAndSeed(0, 12345);
                mazeBuilder.GenerateMaze();
                Debug.Log("  ✓ Maze generated (21x21, Seed: 12345)");
            }
            else
            {
                Debug.LogError("  ✗ MazeBuilder not found!");
            }
        }

        #endregion

        #region Add Spotlight

        private static void AddSpotlight()
        {
            // Create spotlight at center of maze
            var light = new GameObject("Spotlight");
            light.transform.position = new Vector3(0, 10, 0);

            var lightComp = light.AddComponent<Light>();
            lightComp.type = LightType.Spot;
            lightComp.color = Color.white;
            lightComp.intensity = 1.5f;
            lightComp.range = 50f;
            lightComp.spotAngle = 90f;
            lightComp.shadows = LightShadows.Soft;

            // Point down at maze
            light.transform.rotation = Quaternion.Euler(90, 0, 0);

            Debug.Log("  ✓ Spotlight added at (0, 10, 0)");
        }

        #endregion
    }
}
