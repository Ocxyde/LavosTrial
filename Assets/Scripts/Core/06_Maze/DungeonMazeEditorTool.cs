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
// DungeonMazeEditorTool.cs
// Complete editor tool for advanced dungeon maze generation with preview
// Unity 6 compatible - UTF-8 encoding - Unix line endings

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using Code.Lavos.Core.Advanced;

namespace Code.Lavos.Tools.Advanced
{
    /// <summary>
    /// Editor tool for dungeon maze generation and visualization.
    /// Features:
    /// - Real-time maze preview in scene view
    /// - Parameter adjustment with live update
    /// - Save/Load configurations
    /// - Visual difficulty indicator
    /// - Trap/Treasure/Path visualization
    /// </summary>
    public sealed class DungeonMazeEditorTool : EditorWindow
    {
        private DungeonMazeGenerator _generator;
        private DungeonMazeData _currentMaze;
        private DungeonMazeConfig _config;
        private int _seed = 12345;
        private int _level = 0;
        private Vector2 _scrollPosition;
        private bool _showPreview = true;
        private bool _showTrapRooms = true;
        private bool _showTreasureRooms = true;
        private bool _showMainPath = false;
        private bool _advancedSettings = false;
        private float _generationTime = 0;

        [MenuItem("Tools/Lavos/Dungeon Maze Generator")]
        public static void ShowWindow()
        {
            GetWindow<DungeonMazeEditorTool>("Dungeon Maze");
        }

        private void OnEnable()
        {
            _config = DungeonMazeConfig.CreateDefault();
            _generator = new DungeonMazeGenerator();
        }

        private void OnGUI()
        {
            GUILayout.Label("DUNGEON MAZE GENERATOR - Advanced Edition", EditorStyles.boldLabel);
            GUILayout.Space(10);

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            // === GENERATION CONTROLS ===
            GUILayout.Label("Generation Controls", EditorStyles.boldLabel);
            _seed = EditorGUILayout.IntField("Seed", _seed);
            _level = EditorGUILayout.IntSlider("Level", _level, 0, 10);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Generate Maze", GUILayout.Height(40)))
                {
                    GenerateMaze();
                }

                if (GUILayout.Button("Clear", GUILayout.Height(40)))
                {
                    _currentMaze = null;
                }

                if (GUILayout.Button("Random Seed", GUILayout.Height(40)))
                {
                    _seed = Random.Range(int.MinValue, int.MaxValue);
                }
            }
            GUILayout.EndHorizontal();

            if (_currentMaze != null)
            {
                GUILayout.Label($"Generated in {_generationTime:F2}ms", EditorStyles.miniLabel);
            }

            GUILayout.Space(15);

            // === PREVIEW OPTIONS ===
            GUILayout.Label("Preview Options", EditorStyles.boldLabel);
            _showPreview = EditorGUILayout.Toggle("Show Preview", _showPreview);
            _showTrapRooms = EditorGUILayout.Toggle("Show Trap Rooms", _showTrapRooms);
            _showTreasureRooms = EditorGUILayout.Toggle("Show Treasure Rooms", _showTreasureRooms);
            _showMainPath = EditorGUILayout.Toggle("Show Main Path", _showMainPath);

            GUILayout.Space(15);

            // === CORE PARAMETERS ===
            GUILayout.Label("Core Parameters", EditorStyles.boldLabel);
            _config.BaseSize = EditorGUILayout.IntSlider("Base Size", _config.BaseSize, 11, 51);
            _config.MinSize = EditorGUILayout.IntField("Min Size", _config.MinSize);
            _config.MaxSize = EditorGUILayout.IntField("Max Size", _config.MaxSize);
            _config.CellSize = EditorGUILayout.Slider("Cell Size", _config.CellSize, 2.0f, 10.0f);
            _config.WallHeight = EditorGUILayout.Slider("Wall Height", _config.WallHeight, 1.0f, 10.0f);

            GUILayout.Space(15);

            // === ROOM PARAMETERS ===
            GUILayout.Label("Room Configuration", EditorStyles.boldLabel);
            _config.SpawnRoomSize = EditorGUILayout.IntSlider("Spawn Room Size", _config.SpawnRoomSize, 1, 4);
            _config.ExitRoomSize = EditorGUILayout.IntSlider("Exit Room Size", _config.ExitRoomSize, 1, 4);
            _config.ChamberExpansionRadius = EditorGUILayout.IntSlider("Chamber Expansion", _config.ChamberExpansionRadius, 0, 3);

            GUILayout.Space(15);

            // === DANGER & TREASURE ===
            GUILayout.Label("Danger & Treasure", EditorStyles.boldLabel);
            _config.TrapDensity = EditorGUILayout.Slider("Trap Density", _config.TrapDensity, 0.0f, 1.0f);
            _config.TreasureDensity = EditorGUILayout.Slider("Treasure Density", _config.TreasureDensity, 0.0f, 1.0f);
            _config.BossRoomCount = EditorGUILayout.IntSlider("Boss Rooms", _config.BossRoomCount, 0, 3);

            GUILayout.Space(15);

            // === COMPLEXITY ===
            GUILayout.Label("Complexity", EditorStyles.boldLabel);
            _config.CorridorWindingFactor = EditorGUILayout.Slider("Corridor Winding", _config.CorridorWindingFactor, 0.0f, 1.0f);
            _config.DeadEndExpansionChance = EditorGUILayout.Slider("Dead-End Expansion", _config.DeadEndExpansionChance, 0.0f, 1.0f);

            GUILayout.Space(15);

            // === OBJECT PLACEMENT ===
            GUILayout.Label("Object Placement", EditorStyles.boldLabel);
            _config.TorchChance = EditorGUILayout.Slider("Torch Chance", _config.TorchChance, 0.0f, 1.0f);
            _config.EnemyDensity = EditorGUILayout.Slider("Enemy Density", _config.EnemyDensity, 0.0f, 0.2f);
            _config.ChestDensity = EditorGUILayout.Slider("Chest Density", _config.ChestDensity, 0.0f, 0.2f);

            GUILayout.Space(15);

            // === ADVANCED SETTINGS ===
            _advancedSettings = EditorGUILayout.Foldout(_advancedSettings, "Advanced Settings");
            if (_advancedSettings)
            {
                EditorGUI.indentLevel++;

                _config.AllowDiagonalWalls = EditorGUILayout.Toggle("Allow Diagonal Walls", _config.AllowDiagonalWalls);
                _config.GuaranteedPathRequired = EditorGUILayout.Toggle("Guarantee Path", _config.GuaranteedPathRequired);
                _config.MultiEntranceEnabled = EditorGUILayout.Toggle("Multi-Entrance", _config.MultiEntranceEnabled);

                GUILayout.Space(10);

                GUILayout.Label("AI Difficulty", EditorStyles.boldLabel);
                _config.AISettings.EnableAdaptivity = EditorGUILayout.Toggle("Enable AI Adaptivity", _config.AISettings.EnableAdaptivity);
                _config.AISettings.MinAdaptiveFactor = EditorGUILayout.Slider("Min Factor", _config.AISettings.MinAdaptiveFactor, 0.5f, 2.0f);
                _config.AISettings.MaxAdaptiveFactor = EditorGUILayout.Slider("Max Factor", _config.AISettings.MaxAdaptiveFactor, 1.0f, 3.0f);
                _config.AISettings.LearnFromPlayerPerformance = EditorGUILayout.Toggle("Learn from Performance", _config.AISettings.LearnFromPlayerPerformance);

                EditorGUI.indentLevel--;
            }

            GUILayout.Space(15);

            // === DIFFICULTY INDICATOR ===
            if (_currentMaze != null)
            {
                GUILayout.Label("Difficulty Metrics", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Base Difficulty", _currentMaze.DifficultyFactor.ToString("F2"));
                EditorGUILayout.LabelField("AI Adaptive Factor", _currentMaze.AIAdaptiveFactor.ToString("F2"));

                float combined = _currentMaze.DifficultyFactor * _currentMaze.AIAdaptiveFactor;
                string difficulty = combined switch
                {
                    < 0.8f => "Very Easy",
                    < 1.0f => "Easy",
                    < 1.2f => "Normal",
                    < 1.5f => "Hard",
                    < 1.8f => "Very Hard",
                    _ => "Insane",
                };
                EditorGUILayout.LabelField("Final Difficulty", difficulty);
            }

            GUILayout.Space(15);

            // === SAVE/LOAD ===
            GUILayout.Label("Configuration", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Save Config"))
                {
                    SaveConfig();
                }
                if (GUILayout.Button("Load Config"))
                {
                    LoadConfig();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
        }

        private void GenerateMaze()
        {
            if (!_config.IsValid())
            {
                EditorUtility.DisplayDialog("Error", "Invalid configuration", "OK");
                return;
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _currentMaze = _generator.Generate(_seed, _level, _config);
            stopwatch.Stop();
            _generationTime = stopwatch.ElapsedMilliseconds;

            Debug.Log($"Dungeon maze generated: {_currentMaze.Width}x{_currentMaze.Height} at Level {_level}");
            EditorUtility.DisplayDialog("Success", $"Maze generated in {_generationTime:F1}ms", "OK");

            SceneView.RepaintAll();
        }

        private void SaveConfig()
        {
            string path = EditorUtility.SaveFilePanel("Save Dungeon Config", "Assets/Config", "dungeon_config", "json");
            if (!string.IsNullOrEmpty(path))
            {
                string json = _config.ToJson();
                System.IO.File.WriteAllText(path, json);
                Debug.Log($"Config saved: {path}");
            }
        }

        private void LoadConfig()
        {
            string path = EditorUtility.OpenFilePanel("Load Dungeon Config", "Assets/Config", "json");
            if (!string.IsNullOrEmpty(path))
            {
                string json = System.IO.File.ReadAllText(path);
                _config = DungeonMazeConfig.LoadFromJson(json);
                Debug.Log($"Config loaded: {path}");
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!_showPreview || _currentMaze == null)
                return;

            DrawMazePreview();
        }

        private void DrawMazePreview()
        {
            float cellSize = _config.CellSize;

            for (int x = 0; x < _currentMaze.Width; x++)
            {
                for (int z = 0; z < _currentMaze.Height; z++)
                {
                    var cell = _currentMaze.GetCell(x, z);
                    Vector3 cellCenter = new Vector3(x * cellSize + cellSize / 2, 0, z * cellSize + cellSize / 2);

                    // Draw trap rooms
                    if (_showTrapRooms && (cell & CellFlags8.IsTrapRoom) != 0)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube(cellCenter + Vector3.up * 0.5f, new Vector3(cellSize * 0.8f, 1, cellSize * 0.8f));
                    }

                    // Draw treasure rooms
                    if (_showTreasureRooms && (cell & CellFlags8.IsTreasureRoom) != 0)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawCube(cellCenter + Vector3.up * 0.5f, new Vector3(cellSize * 0.8f, 1, cellSize * 0.8f));
                    }

                    // Draw main path
                    if (_showMainPath && (cell & CellFlags8.IsMainPath) != 0)
                    {
                        Gizmos.color = new Color(0, 1, 0, 0.5f);
                        Gizmos.DrawCube(cellCenter + Vector3.up * 0.3f, new Vector3(cellSize * 0.5f, 0.5f, cellSize * 0.5f));
                    }
                }
            }

            // Draw spawn and exit
            var spawn = _currentMaze.SpawnCell;
            var exit = _currentMaze.ExitCell;

            Gizmos.color = Color.green;
            Gizmos.DrawCube(new Vector3(spawn.x * cellSize + cellSize / 2, 1, spawn.z * cellSize + cellSize / 2), Vector3.one * 1.5f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(new Vector3(exit.x * cellSize + cellSize / 2, 1, exit.z * cellSize + cellSize / 2), Vector3.one * 1.5f);
        }
    }
}

#endif
