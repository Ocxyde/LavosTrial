﻿// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8 (no BOM) | Line Endings: Unix LF
// Cell-Based Maze Generation System - Editor Tool: 1-Click Generator

using UnityEngine;
using UnityEditor;
using Code.Lavos.Core.Maze;
using Code.Lavos.Core.Environment;
using System.Collections.Generic;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// CellBasedMazeGeneratorTool - 1-click maze generation editor tool.
    /// 
    /// Features:
    /// - Generate complete maze with 1 click
    /// - Auto-fill empty cells appropriately
    /// - Auto-spawn walls and doors
    /// - Configurable parameters (size, level, seed)
    /// - Visual progress feedback
    /// - Plug-in-Out compliant
    /// 
    /// Usage:
    /// Tools > Cell-Based Maze > Generate Maze (1-Click)
    /// </summary>
    public class CellBasedMazeGeneratorTool : EditorWindow
    {
        // Generation Parameters
        [Header("Maze Settings")]
        [SerializeField] private int mazeWidth = 21;
        [SerializeField] private int mazeHeight = 21;
        [SerializeField] private int level = 5;
        [SerializeField] private int seed = -1; // -1 = random
        
        [Header("Cell Filling")]
        [SerializeField] private bool autoFillEmptyCells = true;
        [SerializeField] private float fillCorridorChance = 0.3f;
        [SerializeField] private float fillRoomChance = 0.1f;
        [SerializeField] private float fillDeadEndChance = 0.2f;
        
        [Header("Wall/Door Spawning")]
        [SerializeField] private bool autoSpawnWalls = true;
        [SerializeField] private bool autoSpawnDoors = true;
        [SerializeField] private bool verifyMaze = true;
        
        // Prefab References (auto-find if not assigned)
        [Header("Prefabs (auto-find if empty)")]
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private GameObject doorPrefab;
        [SerializeField] private GameObject lockedDoorPrefab;
        [SerializeField] private GameObject secretDoorPrefab;
        [SerializeField] private GameObject exitDoorPrefab;
        [SerializeField] private Material wallMaterial;
        
        // Runtime
        private CellBasedMazeGenerator _generator;
        private bool _isGenerating = false;
        private string _statusMessage = "Ready";
        private float _progress = 0f;
        
        [MenuItem("Tools/Cell-Based Maze/Generate Maze (1-Click)")]
        public static void ShowWindow()
        {
            var window = GetWindow<CellBasedMazeGeneratorTool>("1-Click Maze Generator");
            window.minSize = new Vector2(450, 700);
        }
        
        private void OnGUI()
        {
            GUILayout.Label("1-Click Cell-Based Maze Generator", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            // Help box
            EditorGUILayout.HelpBox(
                "Click 'Generate Maze' to create a complete maze with:\n" +
                "- Primary path (longest snake way)\n" +
                "- Decoy paths (L-shape, Spiral, Fork)\n" +
                "- Rooms (3x3, 2 doors each)\n" +
                "- Agreements (chests, enemies, traps)\n" +
                "- Walls and doors spawned\n" +
                "- Auto-verification",
                MessageType.Info);
            
            GUILayout.Space(10);
            
            // Maze Settings
            GUILayout.Label("Maze Settings", EditorStyles.boldLabel);
            mazeWidth = EditorGUILayout.IntField("Width", mazeWidth);
            mazeHeight = EditorGUILayout.IntField("Height", mazeHeight);
            level = EditorGUILayout.IntField("Level (0-39)", Mathf.Clamp(level, 0, 39));
            seed = EditorGUILayout.IntField("Seed (-1 = Random)", seed);
            
            GUILayout.Space(10);
            
            // Cell Filling
            GUILayout.Label("Cell Filling (Empty Cells)", EditorStyles.boldLabel);
            autoFillEmptyCells = EditorGUILayout.Toggle("Auto-Fill Empty Cells", autoFillEmptyCells);
            
            if (autoFillEmptyCells)
            {
                EditorGUI.indentLevel++;
                fillCorridorChance = EditorGUILayout.Slider("Corridor Chance", fillCorridorChance, 0f, 1f);
                fillRoomChance = EditorGUILayout.Slider("Room Chance", fillRoomChance, 0f, 1f);
                fillDeadEndChance = EditorGUILayout.Slider("Dead-End Chance", fillDeadEndChance, 0f, 1f);
                EditorGUI.indentLevel--;
            }
            
            GUILayout.Space(10);
            
            // Wall/Door Spawning
            GUILayout.Label("Wall & Door Spawning", EditorStyles.boldLabel);
            autoSpawnWalls = EditorGUILayout.Toggle("Auto-Spawn Walls", autoSpawnWalls);
            autoSpawnDoors = EditorGUILayout.Toggle("Auto-Spawn Doors", autoSpawnDoors);
            verifyMaze = EditorGUILayout.Toggle("Verify Maze Integrity", verifyMaze);
            
            GUILayout.Space(10);
            
            // Prefabs
            GUILayout.Label("Prefabs", EditorStyles.boldLabel);
            wallPrefab = (GameObject)EditorGUILayout.ObjectField("Wall Prefab", wallPrefab, typeof(GameObject), false);
            doorPrefab = (GameObject)EditorGUILayout.ObjectField("Door Prefab", doorPrefab, typeof(GameObject), false);
            lockedDoorPrefab = (GameObject)EditorGUILayout.ObjectField("Locked Door Prefab", lockedDoorPrefab, typeof(GameObject), false);
            secretDoorPrefab = (GameObject)EditorGUILayout.ObjectField("Secret Door Prefab", secretDoorPrefab, typeof(GameObject), false);
            exitDoorPrefab = (GameObject)EditorGUILayout.ObjectField("Exit Door Prefab", exitDoorPrefab, typeof(GameObject), false);
            wallMaterial = (Material)EditorGUILayout.ObjectField("Wall Material", wallMaterial, typeof(Material), false);
            
            GUILayout.Space(20);
            
            // Generate Button
            GUI.enabled = !_isGenerating;
            
            if (GUILayout.Button("GENERATE MAZE (1-Click)", GUILayout.Height(50)))
            {
                GenerateMaze();
            }
            
            GUI.enabled = true;
            
            GUILayout.Space(10);

            // Progress Bar
            if (_isGenerating)
            {
                Rect progressRect = EditorGUILayout.GetControlRect(GUILayout.Height(20));
                EditorGUI.ProgressBar(progressRect, _progress, _statusMessage);
                Repaint();
            }
            else if (!string.IsNullOrEmpty(_statusMessage))
            {
                EditorGUILayout.HelpBox(_statusMessage, MessageType.None);
            }
            
            GUILayout.Space(20);
            
            // Statistics
            GUILayout.Label("Generation Statistics", EditorStyles.boldLabel);
            if (_generator != null)
            {
                EditorGUILayout.LabelField("Grid Size", $"{mazeWidth}x{mazeHeight}");
                EditorGUILayout.LabelField("Level", level.ToString());
                EditorGUILayout.LabelField("Seed", seed.ToString());
            }
            
            GUILayout.Space(10);
            
            // Quick Actions
            GUILayout.Label("Quick Actions", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Find Prefabs in Resources"))
            {
                AutoFindPrefabs();
            }
            
            if (GUILayout.Button("Reset to Defaults"))
            {
                ResetToDefaults();
            }
        }
        
        private async void GenerateMaze()
        {
            _isGenerating = true;
            _progress = 0.1f;
            _statusMessage = "Initializing generator...";
            
            // Use current seed or generate random
            int currentSeed = seed < 0 ? Random.Range(int.MinValue, int.MaxValue) : seed;
            
            try
            {
                // Step 1: Create generator
                _generator = new CellBasedMazeGenerator();
                _progress = 0.2f;
                _statusMessage = "Generating maze data...";
                
                // Step 2: Generate maze
                var grid = _generator.Generate(mazeWidth, mazeHeight, level, currentSeed, verifyMaze);
                _progress = 0.5f;
                _statusMessage = "Filling empty cells...";
                
                // Step 3: Auto-fill empty cells
                if (autoFillEmptyCells)
                {
                    FillEmptyCells(grid);
                }
                _progress = 0.7f;
                _statusMessage = "Spawning walls and doors...";
                
                // Step 4: Auto-spawn walls and doors
                if (autoSpawnWalls || autoSpawnDoors)
                {
                    // Auto-find prefabs if not assigned
                    if (wallPrefab == null) AutoFindPrefabs();
                    
                    // Create root transforms
                    Transform wallsRoot = GetOrCreateRoot("MazeWalls").transform;
                    Transform doorsRoot = GetOrCreateRoot("MazeDoors").transform;
                    
                    // Clear existing
                    if (autoSpawnWalls) ClearChildren(wallsRoot);
                    if (autoSpawnDoors) ClearChildren(doorsRoot);
                    
                    // Spawn
                    if (autoSpawnWalls && autoSpawnDoors)
                    {
                        _generator.SpawnWallsAndDoors(
                            wallPrefab, doorPrefab, lockedDoorPrefab, secretDoorPrefab, exitDoorPrefab,
                            wallMaterial, wallsRoot, doorsRoot,
                            cellSize: 6f, wallHeight: 4f, wallThickness: 0.3f,
                            wallPivotIsAtMeshCenter: true);
                    }
                }
                
                _progress = 1.0f;
                _statusMessage = $"Maze generated successfully! Seed: {currentSeed}";
                
                Debug.Log($"[1-Click Maze Generator] Complete! Seed: {currentSeed}, Size: {mazeWidth}x{mazeHeight}, Level: {level}");
            }
            catch (System.Exception e)
            {
                _statusMessage = $"Error: {e.Message}";
                Debug.LogError($"[1-Click Maze Generator] Error: {e}");
            }
            finally
            {
                _isGenerating = false;
            }
        }
        
        /// <summary>
        /// Fill empty cells appropriately based on context.
        /// </summary>
        private void FillEmptyCells(MazeCell[,] grid)
        {
            int filledCount = 0;
            
            for (int x = 0; x < mazeWidth; x++)
            {
                for (int y = 0; y < mazeHeight; y++)
                {
                    var cell = grid[x, y];
                    
                    // Skip non-empty cells
                    if (!cell.IsWalkable() || cell.agreement != CellAgreement.None)
                        continue;
                    
                    // Skip primary path cells
                    if (cell.isOnPrimaryPath)
                        continue;
                    
                    // Decide what to fill based on chance and context
                    float roll = Random.value;
                    
                    if (roll < fillRoomChance)
                    {
                        // Fill as room interior
                        cell.cellType = CellType.Room;
                        cell.agreement = CellAgreement.RoomInterior;
                    }
                    else if (roll < fillRoomChance + fillDeadEndChance)
                    {
                        // Fill as dead-end
                        cell.MarkAsDeadEnd();
                    }
                    else if (roll < fillRoomChance + fillDeadEndChance + fillCorridorChance)
                    {
                        // Fill as corridor
                        cell.agreement = CellAgreement.Corridor;
                        cell.MarkAsDecoyPath();
                    }
                    // else leave as empty (will become wall)
                    
                    grid[x, y] = cell;
                    filledCount++;
                }
            }
            
            Debug.Log($"[1-Click Maze Generator] Filled {filledCount} empty cells");
        }
        
        /// <summary>
        /// Auto-find prefabs in Resources folders.
        /// </summary>
        private void AutoFindPrefabs()
        {
            wallPrefab ??= Resources.Load<GameObject>("Prefabs/WallPrefab");
            doorPrefab ??= Resources.Load<GameObject>("Prefabs/DoorPrefab");
            lockedDoorPrefab ??= Resources.Load<GameObject>("Prefabs/LockedDoorPrefab");
            secretDoorPrefab ??= Resources.Load<GameObject>("Prefabs/SecretDoorPrefab");
            exitDoorPrefab ??= Resources.Load<GameObject>("Prefabs/ExitDoorPrefab");
            wallMaterial ??= Resources.Load<Material>("Materials/WallMaterial");
            
            Debug.Log("[1-Click Maze Generator] Auto-found prefabs from Resources");
        }
        
        /// <summary>
        /// Get or create root GameObject.
        /// </summary>
        private GameObject GetOrCreateRoot(string name)
        {
            var existing = GameObject.Find(name);
            if (existing != null)
                return existing;
            
            var root = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(root, "Create Maze Root");
            return root;
        }
        
        /// <summary>
        /// Clear all children of transform.
        /// </summary>
        private void ClearChildren(Transform root)
        {
            var children = new List<Transform>();
            foreach (Transform child in root)
                children.Add(child);
            
            foreach (var child in children)
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }
        }
        
        /// <summary>
        /// Reset to default values.
        /// </summary>
        private void ResetToDefaults()
        {
            mazeWidth = 21;
            mazeHeight = 21;
            level = 5;
            seed = -1;
            autoFillEmptyCells = true;
            fillCorridorChance = 0.3f;
            fillRoomChance = 0.1f;
            fillDeadEndChance = 0.2f;
            autoSpawnWalls = true;
            autoSpawnDoors = true;
            verifyMaze = true;
            
            Debug.Log("[1-Click Maze Generator] Reset to defaults");
        }
    }
}
