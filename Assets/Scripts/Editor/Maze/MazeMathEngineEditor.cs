// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8 | Locale: en_US
//
// MazeMathEngineEditor.cs
// Editor tool for maze generation using MazeMathEngine_8Axis
// Pure mathematics procedural generation with DFS and A* pathfinding
//
// USAGE:
//   Tools -> Lavos -> MazeMath Engine Generator
//   Auto-creates ALL required components if missing
//   Press Play to test

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using Code.Lavos.Core;
using Code.Lavos.Core.Advanced;
using Object = UnityEngine.Object;

namespace Code.Lavos.Tools.Advanced
{
    /// <summary>
    /// MazeMathEngineEditor - Editor tool for maze generation using pure math engine.
    /// Uses MazeMathEngine_8Axis for procedural generation mathematics.
    /// Plug-in-out compliant: finds existing components, creates only if missing.
    /// </summary>
    public sealed class MazeMathEngineEditor : EditorWindow
    {
        // Window state
        private Vector2 _scrollPosition;
        private int _randomSeed = -1;
        private bool _useFixedSeed = false;
        private int _mazeSize = 21;
        private int _dfsIterations = 100;

        // Generated result cache
        private MazeGenerationResult _lastResult;
        private float _lastGenerationTime;

        #region Menu Items

        [MenuItem("Tools/Legacy/MazeMath Engine Generator")]
        public static void ShowWindow()
        {
            GetWindow<MazeMathEngineEditor>("MazeMath Engine");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Setup scene with all required components (plug-in-out compliant).
        /// Finds existing components, creates only if missing.
        /// </summary>
        private static void SetupMazeComponents()
        {
            EnsureComponentExists<GameConfig>("GameConfig");
            EnsureComponentExists<CompleteMazeBuilder8>("MazeBuilder");
            Debug.Log("[MazeMathEngine] Scene setup complete");
        }

        /// <summary>
        /// Ensures a component exists on a GameObject, creating one if necessary.
        /// </summary>
        /// <typeparam name="T">The component type to ensure exists.</typeparam>
        /// <param name="gameObjectName">Name for the GameObject if creation is needed.</param>
        private static void EnsureComponentExists<T>(string gameObjectName) where T : Component
        {
            if (UnityEngine.Object.FindFirstObjectByType<T>() == null)
            {
                Debug.Log($"[MazeMathEngine] Creating {typeof(T).Name}...");
                var go = new GameObject(gameObjectName);
                go.AddComponent<T>();
            }
        }

        /// <summary>
        /// Generate maze using MazeMathEngine_8Axis pure mathematics.
        /// </summary>
        private void GenerateMaze()
        {
            SetupMazeComponents();

            var config = GameConfig.Instance;
            if (config == null)
            {
                Debug.LogError("[MazeMathEngine]  GameConfig not found!");
                EditorUtility.DisplayDialog("Error", "GameConfig not found in scene!", "OK");
                return;
            }

            var mazeBuilder = Object.FindFirstObjectByType<CompleteMazeBuilder8>();
            if (mazeBuilder == null)
            {
                Debug.LogError("[MazeMathEngine]  CompleteMazeBuilder8 not found after setup!");
                EditorUtility.DisplayDialog("Error", "CompleteMazeBuilder8 not found!", "OK");
                return;
            }

            // Determine seed
            int seed = _useFixedSeed ? _randomSeed : (int)(DateTime.Now.Ticks % int.MaxValue);

            // Get maze size
            int size = _mazeSize;

            Debug.Log("");
            Debug.Log("  MAZEMATH ENGINE - Pure Math Generation");
            Debug.Log("");
            Debug.Log($"[MazeMathEngine]  Generating {size}x{size} maze with seed {seed}...");

            float t0 = Time.realtimeSinceStartup;

            // Create and run MazeMathEngine_8Axis
            var mathEngine = new MazeMathEngine_8Axis(size, size, seed);
            _lastResult = mathEngine.GenerateMaze(_dfsIterations);

            _lastGenerationTime = (Time.realtimeSinceStartup - t0) * 1000f;

            // Log generation statistics
            Debug.Log($"[MazeMathEngine]  Generation complete in {_lastGenerationTime:F2}ms");
            Debug.Log($"[MazeMathEngine]  Carved: {_lastResult.CarvedCells}/{_lastResult.TotalCells} ({_lastResult.ComplexityFactor * 100f:F1}%)");
            Debug.Log($"[MazeMathEngine]  Start: {_lastResult.StartPoint}, Exit: {_lastResult.ExitPoint}");
            Debug.Log($"[MazeMathEngine]  Dead-ends: {_lastResult.DeadEnds.Count}, Corridors: {_lastResult.Corridors.Count}");
            Debug.Log("");

            // Show success dialog
            EditorUtility.DisplayDialog(
                "Maze Generated",
                $"Maze generated in {_lastGenerationTime:F2}ms\n" +
                $"Size: {size}x{size}\n" +
                $"Carved: {_lastResult.CarvedCells}/{_lastResult.TotalCells}\n" +
                $"Complexity: {_lastResult.ComplexityFactor * 100f:F1}%",
                "OK");

            Repaint();
        }

        #endregion

        #region Editor Window UI

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // Header
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("MazeMath Engine 8-AXIS", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Pure Mathematics Procedural Generation", EditorStyles.miniLabel);

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "This tool uses MazeMathEngine_8Axis for pure mathematical maze generation.\n" +
                "Features: DFS carving, A* pathfinding, 8-axis wall calculations.\n" +
                "Guaranteed path from Start (A) to Exit (B).",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Generate button
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("Generate Maze", GUILayout.Height(40)))
            {
                GenerateMaze();
            }
            GUI.backgroundColor = Color.white;

            if (_lastResult != null)
            {
                EditorGUILayout.LabelField($"Generated in {_lastGenerationTime:F2}ms", EditorStyles.miniLabel);
            }

            EditorGUILayout.Space(15);

            // Seed settings
            EditorGUILayout.LabelField("Random Seed", EditorStyles.boldLabel);
            _useFixedSeed = EditorGUILayout.Toggle("Use Fixed Seed", _useFixedSeed);

            if (_useFixedSeed)
            {
                _randomSeed = EditorGUILayout.IntField("Seed Value", _randomSeed);
            }
            else
            {
                EditorGUILayout.HelpBox("Random seed will be generated from system ticks", MessageType.None);
            }

            EditorGUILayout.Space(10);

            // Maze size
            EditorGUILayout.LabelField("Maze Size", EditorStyles.boldLabel);
            _mazeSize = EditorGUILayout.IntSlider("Grid Size", _mazeSize, 11, 51);
            EditorGUILayout.HelpBox("Recommended: 21x21 for standard mazes", MessageType.None);

            EditorGUILayout.Space(10);

            // DFS iterations
            EditorGUILayout.LabelField("Generation Settings", EditorStyles.boldLabel);
            _dfsIterations = EditorGUILayout.IntSlider("DFS Iterations", _dfsIterations, 50, 500);
            EditorGUILayout.HelpBox(
                "Higher iterations = more complex mazes with more dead-ends\n" +
                "Lower iterations = simpler, more direct paths",
                MessageType.None);

            // Preview section
            if (_lastResult != null)
            {
                EditorGUILayout.Space(20);
                EditorGUILayout.LabelField("Generation Result", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"Size: {_lastResult.Width}x{_lastResult.Height}");
                EditorGUILayout.LabelField($"Start: {_lastResult.StartPoint}");
                EditorGUILayout.LabelField($"Exit: {_lastResult.ExitPoint}");
                EditorGUILayout.LabelField($"Carved: {_lastResult.CarvedCells}/{_lastResult.TotalCells}");
                EditorGUILayout.LabelField($"Complexity: {_lastResult.ComplexityFactor * 100f:F1}%");
                EditorGUILayout.LabelField($"Dead-ends: {_lastResult.DeadEnds.Count}");
                EditorGUILayout.LabelField($"Corridors: {_lastResult.Corridors.Count}");
                EditorGUI.indentLevel--;

                // ASCII preview
                EditorGUILayout.Space(10);
                if (GUILayout.Button("Show ASCII Preview in Console"))
                {
                    ShowASCIIPreview();
                }
            }

            // Documentation
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Documentation", EditorStyles.boldLabel);
            if (GUILayout.Button("Open Documentation"))
            {
                OpenDocumentation();
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Display ASCII preview of generated maze in console.
        /// </summary>
        private void ShowASCIIPreview()
        {
            if (_lastResult == null) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("");
            sb.AppendLine($"  MAZE PREVIEW {_lastResult.Width}x{_lastResult.Height}");
            sb.AppendLine("");

            for (int z = _lastResult.Height - 1; z >= 0; z--)
            {
                for (int x = 0; x < _lastResult.Width; x++)
                {
                    char c;
                    if (x == _lastResult.StartPoint.x && z == _lastResult.StartPoint.z)
                        c = 'A'; // Start
                    else if (x == _lastResult.ExitPoint.x && z == _lastResult.ExitPoint.z)
                        c = 'B'; // Exit
                    else if (_lastResult.MazeGrid[x, z] == MazeMathEngine_8Axis.CELL_WALL)
                        c = '#'; // Wall
                    else
                        c = '.'; // Passage

                    sb.Append(c);
                }
                sb.AppendLine();
            }

            sb.AppendLine("");
            sb.AppendLine("  Legend: A=Start, B=Exit, #=Wall, .=Passage");
            sb.AppendLine("");

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Open documentation file.
        /// </summary>
        private void OpenDocumentation()
        {
            string docPath = "Assets/Docs/MAZEMATH_ENGINE_TOOL.md";
            string fullPath = System.IO.Path.Combine(Application.dataPath, "..", docPath);

            if (System.IO.File.Exists(fullPath))
            {
                EditorUtility.RevealInFinder(fullPath);
                Debug.Log($"[MazeMathEngine]  Documentation: {docPath}");
            }
            else
            {
                Debug.LogWarning($"[MazeMathEngine]  Documentation not found: {docPath}");
            }
        }

        #endregion
    }
}

#endif
