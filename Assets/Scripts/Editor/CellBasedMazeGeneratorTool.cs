// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8 (no BOM) | Line Endings: Unix LF
// Cell-Based Maze Generation System - Editor Tool: 1-Click Generator

using UnityEngine;
using UnityEditor;
using Code.Lavos.Core.Maze;
using Code.Lavos.Core.Environment;
using Code.Lavos.Core.Player;
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
        [SerializeField] private bool autoSpawnGround = true;
        [SerializeField] private bool surroundWithPerimeterWalls = true;
        [SerializeField] private bool markEntryExitPoints = true;
        [SerializeField] private bool verifyMaze = true;
        
        // Prefab References (auto-find if not assigned)
        [Header("Prefabs (auto-find if empty)")]
        [SerializeField] private GameObject groundPlanePrefab;
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
            autoSpawnGround = EditorGUILayout.Toggle("Auto-Spawn Ground Plane", autoSpawnGround);
            autoSpawnWalls = EditorGUILayout.Toggle("Auto-Spawn Walls", autoSpawnWalls);
            autoSpawnDoors = EditorGUILayout.Toggle("Auto-Spawn Doors", autoSpawnDoors);
            surroundWithPerimeterWalls = EditorGUILayout.Toggle("Surround with Perimeter Walls", surroundWithPerimeterWalls);
            markEntryExitPoints = EditorGUILayout.Toggle("Mark Entry/Exit Points", markEntryExitPoints);
            verifyMaze = EditorGUILayout.Toggle("Verify Maze Integrity", verifyMaze);
            
            GUILayout.Space(10);
            
            // Prefabs
            GUILayout.Label("Prefabs", EditorStyles.boldLabel);
            groundPlanePrefab = (GameObject)EditorGUILayout.ObjectField("Ground Plane Prefab", groundPlanePrefab, typeof(GameObject), false);
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
                _progress = 0.6f;
                _statusMessage = "Spawning ground plane...";
                
                // Step 4: Auto-spawn ground plane
                if (autoSpawnGround)
                {
                    Transform groundRoot = GetOrCreateRoot("MazeGround").transform;
                    ClearChildren(groundRoot);
                    SpawnGroundPlane(groundRoot, mazeWidth, mazeHeight);
                }
                _progress = 0.7f;
                _statusMessage = "Spawning walls and doors...";

                // Step 5: Auto-spawn walls and doors
                Transform wallsRoot = null;
                Transform doorsRoot = null;

                if (autoSpawnWalls || autoSpawnDoors)
                {
                    // Auto-find prefabs if not assigned
                    if (wallPrefab == null) AutoFindPrefabs();

                    // Create root transforms
                    wallsRoot = GetOrCreateRoot("MazeWalls").transform;
                    doorsRoot = GetOrCreateRoot("MazeDoors").transform;

                    // Clear existing
                    if (autoSpawnWalls) ClearChildren(wallsRoot);
                    if (autoSpawnDoors) ClearChildren(doorsRoot);

                    // Spawn internal walls and doors
                    if (autoSpawnWalls && autoSpawnDoors)
                    {
                        _generator.SpawnWallsAndDoors(
                            wallPrefab, doorPrefab, lockedDoorPrefab, secretDoorPrefab, exitDoorPrefab,
                            wallMaterial, wallsRoot, doorsRoot,
                            cellSize: 6f, wallHeight: 4f, wallThickness: 0.3f,
                            wallPivotIsAtMeshCenter: true);
                    }
                }

                // Surround with perimeter walls (independent of internal walls)
                if (surroundWithPerimeterWalls)
                {
                    if (wallsRoot == null)
                    {
                        wallsRoot = GetOrCreateRoot("MazeWalls").transform;
                        ClearChildren(wallsRoot);
                    }
                    if (wallPrefab == null) AutoFindPrefabs();
                    SpawnPerimeterWalls(wallsRoot, mazeWidth, mazeHeight);
                }

                // Step 6: Mark entry/exit points
                if (markEntryExitPoints)
                {
                    MarkEntryExitPoints(mazeWidth, mazeHeight);
                }
                
                // Step 7: Spawn player LAST (beside entry point)
                _progress = 0.9f;
                _statusMessage = "Spawning player...";
                SpawnPlayer(currentSeed);
                
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
        /// Spawn ground plane.
        /// </summary>
        private void SpawnGroundPlane(Transform parent, int width, int height)
        {
            // Try to find ground material
            Material groundMat = groundPlanePrefab != null ? 
                groundPlanePrefab.GetComponent<Renderer>()?.sharedMaterial : null;
            
            groundMat ??= Resources.Load<Material>("Materials/Ground_Stone_Mat");
            
            if (groundMat == null)
            {
                Debug.LogWarning("[1-Click Maze Generator] No ground material found!");
                return;
            }
            
            // Create ground plane
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "GroundPlane";
            ground.transform.SetParent(parent, false);
            
            // Scale to fit maze
            float groundSize = Mathf.Max(width, height) * 6f; // 6m per cell
            ground.transform.localScale = new Vector3(groundSize / 10f, 1f, groundSize / 10f);
            ground.transform.position = new Vector3(width * 3f, 0f, height * 3f);
            
            // Assign material
            var renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = groundMat;
            }
            
            // Remove collider (optional, keeps scene cleaner)
            var collider = ground.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            
            Debug.Log($"[1-Click Maze Generator] Spawned ground plane: {groundSize}x{groundSize}");
        }
        
        /// <summary>
        /// Spawn perimeter walls around entire grid (with colliders).
        /// </summary>
        private void SpawnPerimeterWalls(Transform parent, int width, int height)
        {
            if (wallPrefab == null)
            {
                Debug.LogWarning("[1-Click Maze Generator] No wall prefab for perimeter!");
                return;
            }
            
            int perimeterWalls = 0;
            float cellSize = 6f;
            float wallHeight = 4f;
            
            // North and South walls
            for (int x = 0; x < width; x++)
            {
                // North wall
                Vector3 northPos = new Vector3(
                    x * cellSize + cellSize / 2f,
                    wallHeight / 2f,
                    height * cellSize + cellSize / 2f
                );
                GameObject northWall = Object.Instantiate(wallPrefab, northPos, Quaternion.Euler(0f, 90f, 0f), parent);
                northWall.name = $"PerimeterWall_N_{x}";
                EnsureWallCollider(northWall);
                perimeterWalls++;
                
                // South wall
                Vector3 southPos = new Vector3(
                    x * cellSize + cellSize / 2f,
                    wallHeight / 2f,
                    -cellSize / 2f
                );
                GameObject southWall = Object.Instantiate(wallPrefab, southPos, Quaternion.Euler(0f, 90f, 0f), parent);
                southWall.name = $"PerimeterWall_S_{x}";
                EnsureWallCollider(southWall);
                perimeterWalls++;
            }
            
            // East and West walls
            for (int y = 0; y < height; y++)
            {
                // East wall
                Vector3 eastPos = new Vector3(
                    width * cellSize + cellSize / 2f,
                    wallHeight / 2f,
                    y * cellSize + cellSize / 2f
                );
                GameObject eastWall = Object.Instantiate(wallPrefab, eastPos, Quaternion.identity, parent);
                eastWall.name = $"PerimeterWall_E_{y}";
                EnsureWallCollider(eastWall);
                perimeterWalls++;
                
                // West wall
                Vector3 westPos = new Vector3(
                    -cellSize / 2f,
                    wallHeight / 2f,
                    y * cellSize + cellSize / 2f
                );
                GameObject westWall = Object.Instantiate(wallPrefab, westPos, Quaternion.identity, parent);
                westWall.name = $"PerimeterWall_W_{y}";
                EnsureWallCollider(westWall);
                perimeterWalls++;
            }
            
            Debug.Log($"[1-Click Maze Generator] Spawned {perimeterWalls} perimeter walls (with colliders)");
        }
        
        /// <summary>
        /// Ensure wall has a collider for player collision.
        /// </summary>
        private void EnsureWallCollider(GameObject wall)
        {
            // Check if wall already has a collider
            var collider = wall.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = true; // Ensure it's enabled
                return;
            }
            
            // Add box collider if missing
            var boxCollider = wall.AddComponent<BoxCollider>();
            boxCollider.enabled = true;
            
            // Adjust collider size to match wall
            var renderer = wall.GetComponent<Renderer>();
            if (renderer != null)
            {
                boxCollider.size = renderer.bounds.size;
                boxCollider.center = renderer.bounds.center - wall.transform.position;
            }
        }
        
        /// <summary>
        /// Mark entry and exit points with markers.
        /// </summary>
        private void MarkEntryExitPoints(int width, int height)
        {
            // Entry point (1, 1)
            Vector3 entryPos = new Vector3(
                1 * 6f + 3f,
                0.5f,
                1 * 6f + 3f
            );
            CreateMarker(entryPos, "ENTRY_POINT", Color.green);
            
            // Exit point (width-2, height-2)
            Vector3 exitPos = new Vector3(
                (width - 2) * 6f + 3f,
                0.5f,
                (height - 2) * 6f + 3f
            );
            CreateMarker(exitPos, "EXIT_POINT", Color.red);
            
            Debug.Log($"[1-Click Maze Generator] Marked entry ({entryPos}) and exit ({exitPos}) points");
        }
        
        /// <summary>
        /// Create marker sphere at position.
        /// </summary>
        private void CreateMarker(Vector3 position, string name, Color color)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = name;
            marker.transform.position = position;
            marker.transform.localScale = new Vector3(1f, 1f, 1f);
            
            // Make marker glow
            var renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material emissiveMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                emissiveMat.color = color;
                emissiveMat.EnableKeyword("_EMISSION");
                emissiveMat.SetColor("_EmissionColor", color * 2f);
                renderer.sharedMaterial = emissiveMat;
            }
            
            // Remove collider
            var collider = marker.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }
        }
        
        /// <summary>
        /// Spawn player LAST (beside entry point, with movement/interaction).
        /// </summary>
        private void SpawnPlayer(int seed)
        {
            // Try to find player prefab
            GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/Player");
            
            if (playerPrefab == null)
            {
                Debug.LogWarning("[1-Click Maze Generator] No player prefab found! Creating basic player...");
                playerPrefab = CreateBasicPlayerPrefab();
            }
            
            if (playerPrefab == null) return;
            
            // Spawn player BESIDE entry point (not on it)
            // Entry is at (1, 1), spawn at (1.5, 1.5) to be slightly offset
            Vector3 spawnPos = new Vector3(
                (1 + 0.5f) * 6f + 3f,  // X: 1.5 cells
                0f,                     // Y: ground level
                (1 + 0.5f) * 6f + 3f    // Z: 1.5 cells
            );
            
            GameObject player = Object.Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            player.name = "Player";
            
            // Ensure player has required components for movement/interaction
            EnsurePlayerComponents(player);
            
            Debug.Log($"[1-Click Maze Generator] Spawned player at {spawnPos} (beside entry point)");
        }
        
        /// <summary>
        /// Ensure player has all required components for movement and interaction.
        /// </summary>
        private void EnsurePlayerComponents(GameObject player)
        {
            // Check/add Rigidbody for physics
            var rigidbody = player.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = player.AddComponent<Rigidbody>();
                rigidbody.mass = 70f;
                rigidbody.linearDamping = 0.1f;
                rigidbody.angularDamping = 0.5f;
                rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
            
            // Check/add CharacterController for movement
            var characterController = player.GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = player.AddComponent<CharacterController>();
                characterController.height = 2f;
                characterController.radius = 0.5f;
                characterController.stepOffset = 0.3f;
            }
            
            // Check/add DoorInteractionHandler (door interaction with F key)
            var doorInteraction = player.GetComponent<DoorInteractionHandler>();
            if (doorInteraction == null)
            {
                doorInteraction = player.AddComponent<DoorInteractionHandler>();
            }
            
            Debug.Log("[1-Click Maze Generator] Player components verified (movement + interaction ready)");
        }
        
        /// <summary>
        /// Create basic player prefab if none exists.
        /// </summary>
        private GameObject CreateBasicPlayerPrefab()
        {
            // Create player GameObject
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            
            // Add capsule for visuals
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "Visuals";
            capsule.transform.SetParent(player.transform, false);
            var capsuleCollider = capsule.GetComponent<Collider>();
            if (capsuleCollider != null) capsuleCollider.enabled = false;
            
            // Add components
            EnsurePlayerComponents(player);
            
            // Save as prefab
            string prefabPath = "Assets/Resources/Prefabs/Player.prefab";
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Prefabs");
            }
            
            PrefabUtility.SaveAsPrefabAsset(player, prefabPath);
            Debug.Log($"[1-Click Maze Generator] Created player prefab: {prefabPath}");
            
            return player;
        }
        
        /// <summary>
        /// Auto-find prefabs in Resources folders.
        /// </summary>
        private void AutoFindPrefabs()
        {
            groundPlanePrefab = Resources.Load<GameObject>("Prefabs/GroundPlane");
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
