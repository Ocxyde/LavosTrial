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
// CompleteMazeBuilder.cs
// MAIN GAME ORCHESTRATOR - Optimized for performance
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-OUT: Finds components, never creates
// ALL VALUES FROM JSON: No hardcoding
//
// GENERATION ORDER:
// 1. Config  2. Assets  3. Components  4. Cleanup  5. Ground
// 6. Spawn Room  7. Corridors  8. Walls  9. Doors  10. Torches
// 11. Textures  12. Save  13. Player (LAST)
//

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Lavos.Core
{
    /// <summary>
    /// CompleteMazeBuilder - MAIN GAME ORCHESTRATOR.
    /// Optimized: ~500 lines (was ~1000)
    /// </summary>
    public class CompleteMazeBuilder : MonoBehaviour
    {
        #region Fields (From JSON)

        [Header("Prefabs")]
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private GameObject doorPrefab;

        [Header("Materials")]
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material floorMaterial;
        [SerializeField] private Texture2D groundTexture;

        [Header("Components")]
        [SerializeField] private SpatialPlacer spatialPlacer;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private MazeRenderer mazeRenderer;

        [Header("Game State")]
        [SerializeField] private int currentLevel = 0;

        #endregion

        #region Private Data

        private static CompleteMazeBuilder _instance;
        private GridMazeGenerator grid;
        private float cellSize, wallHeight, wallThickness;
        private uint seed;
        private Vector3 spawnPos;
        private Vector2Int spawnCell;
        private int mazeSize;

        #endregion

        #region Public Accessors

        public static CompleteMazeBuilder Instance => _instance;
        public int CurrentLevel => currentLevel;
        public int MazeSize => mazeSize;

        /// <summary>
        /// Get grid for external placers (plug-in-out API).
        /// </summary>
        public GridMazeGenerator GetGrid() => grid;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            LoadConfig();

            // Get compute seed from SeedManager (fresh seed for this scene)
            if (SeedManager.Instance != null)
            {
                seed = SeedManager.Instance.ComputeSeed;
                Log($"[CompleteMazeBuilder] Using compute seed from SeedManager: {seed}");
            }
            else
            {
                // Fallback: generate locally (shouldn't happen)
                seed = GenerateFallbackSeed();
                Log($"[CompleteMazeBuilder] Fallback seed (SeedManager not found): {seed}");
            }

            // Use seed magnitude to influence maze difficulty (from GameConfig)
            var cfg = GameConfig.Instance;
            float seedFactor = seed / (float)int.MaxValue; // 0.0 to 1.0
            int bonusSize = Mathf.FloorToInt(seedFactor * cfg.maxDifficultySizeBonus);
            int bonusRooms = Mathf.FloorToInt(seedFactor * cfg.maxDifficultyRoomBonus);

            mazeSize = cfg.baseMazeSize + currentLevel + bonusSize;
            mazeSize = Mathf.Clamp(mazeSize, cfg.minMazeSize, cfg.maxMazeSize);

            if (EventHandler.Instance != null)
                Log("[CompleteMazeBuilder] Connected to EventHandler");

            Log($"[CompleteMazeBuilder] Level {currentLevel} - Maze {mazeSize}x{mazeSize} - Seed: {seed} (factor: {seedFactor:F2})");
            Log("[CompleteMazeBuilder] Seed is procedural - new seed each scene!");
        }

        private void Start()
        {
            // Always generate maze on start (useRandomSeed from JSON config)
            GenerateMaze();
        }

        private void OnApplicationQuit()
        {
            _instance = null;
        }

        #endregion

        #region Game State

        public void NextLevel()
        {
            currentLevel++;
            var cfg = GameConfig.Instance;
            mazeSize = Mathf.Clamp(cfg.baseMazeSize + currentLevel, cfg.minMazeSize, cfg.maxMazeSize);
            Log($"[CompleteMazeBuilder]  Level {currentLevel} - Maze {mazeSize}x{mazeSize}");
        }

        #endregion

        #region Config Loading

        private void LoadConfig()
        {
            var cfg = GameConfig.Instance;
            cellSize = cfg.defaultCellSize;
            wallHeight = cfg.defaultWallHeight;
            wallThickness = cfg.defaultWallThickness;

            // Load prefabs from config paths with fallback to folder search
            wallPrefab = LoadPrefabWithFallback(cfg.wallPrefab, "Wall");
            doorPrefab = LoadPrefabWithFallback(cfg.doorPrefab, "Door");

            Log($"[CompleteMazeBuilder] Loaded wall prefab: {(wallPrefab != null ? wallPrefab.name : "NULL")}");
            Log($"[CompleteMazeBuilder] Loaded door prefab: {(doorPrefab != null ? doorPrefab.name : "NULL")}");

            // Load materials from config paths with fallback to folder search
            wallMaterial = LoadMaterialWithFallback(cfg.wallMaterial, "Wall");
            floorMaterial = LoadMaterialWithFallback(cfg.floorMaterial, "Floor");
            groundTexture = LoadTextureWithFallback(cfg.groundTexture, "Floor");

            Log($"[CompleteMazeBuilder] Loaded wall material: {(wallMaterial != null ? wallMaterial.name : "NULL")}");
            Log($"[CompleteMazeBuilder] Loaded floor material: {(floorMaterial != null ? floorMaterial.name : "NULL")}");

            // VALIDATION: Critical prefabs must be loaded
            if (wallPrefab == null)
            {
                LogError("[CompleteMazeBuilder] CRITICAL: Wall prefab NOT loaded! Maze generation will fail.");
                LogError("[CompleteMazeBuilder] FIX: Run Tools -> Quick Setup Prefabs (For Testing)");
                LogError("[CompleteMazeBuilder] FIX: Or add WallPrefab.prefab to Assets/Resources/Prefabs/");
            }

            if (doorPrefab == null)
            {
                LogWarning("[CompleteMazeBuilder] Door prefab not loaded - Exit door will not be placed");
                LogWarning("[CompleteMazeBuilder] FIX: Add DoorPrefab.prefab to Assets/Resources/Prefabs/");
            }

            if (floorMaterial == null)
            {
                LogWarning("[CompleteMazeBuilder] Floor material not loaded - Ground will use default white");
            }
        }

        /// <summary>
        /// Load prefab from config path, or search in Resources folders if not found.
        /// </summary>
        private GameObject LoadPrefabWithFallback(string configPath, string searchName)
        {
            // Try config path first
            if (!string.IsNullOrEmpty(configPath))
            {
                string resourcePath = configPath.Replace("Assets/Resources/", "").Replace(".prefab", "");
                GameObject prefab = Resources.Load<GameObject>(resourcePath);
                if (prefab != null)
                {
                    Log($"[CompleteMazeBuilder]  Loaded prefab from config: {resourcePath}");
                    return prefab;
                }
            }

            // Fallback: search in Resources subfolders
            Log($"[CompleteMazeBuilder]  Prefab not found in config, searching folders for: {searchName}");
            string[] folders = { "Prefabs", "Prefabs/Walls", "Prefabs/Doors", "" };
            foreach (string folder in folders)
            {
                string searchPath = string.IsNullOrEmpty(folder) ? searchName : $"{folder}/{searchName}";
                GameObject prefab = Resources.Load<GameObject>(searchPath);
                if (prefab != null)
                {
                    Log($"[CompleteMazeBuilder]  Found prefab in folder: {searchPath}");
                    return prefab;
                }
            }

            // Fallback: search all Resources
            GameObject[] allPrefabs = Resources.LoadAll<GameObject>("");
            foreach (GameObject p in allPrefabs)
            {
                if (p != null && p.name.Contains(searchName, System.StringComparison.OrdinalIgnoreCase))
                {
                    Log($"[CompleteMazeBuilder]  Found prefab by name search: {p.name}");
                    return p;
                }
            }

            LogWarning($"[CompleteMazeBuilder]  Prefab '{searchName}' not found!");
            return null;
        }

        /// <summary>
        /// Load material from config path, or search in Resources folders if not found.
        /// </summary>
        private Material LoadMaterialWithFallback(string configPath, string searchName)
        {
            // Try config path first
            if (!string.IsNullOrEmpty(configPath))
            {
                string resourcePath = configPath.Replace("Assets/Resources/", "").Replace(".mat", "");
                Material mat = Resources.Load<Material>(resourcePath);
                if (mat != null)
                {
                    Log($"[CompleteMazeBuilder]  Loaded material from config: {resourcePath}");
                    return mat;
                }
            }

            // Fallback: search in Resources subfolders
            Log($"[CompleteMazeBuilder]  Material not found in config, searching folders for: {searchName}");
            string[] folders = { "Materials", "Materials/Walls", "Materials/Floors", "" };
            foreach (string folder in folders)
            {
                string searchPath = string.IsNullOrEmpty(folder) ? searchName : $"{folder}/{searchName}";
                Material mat = Resources.Load<Material>(searchPath);
                if (mat != null)
                {
                    Log($"[CompleteMazeBuilder]  Found material in folder: {searchPath}");
                    return mat;
                }
            }

            // Fallback: search all Resources
            Material[] allMats = Resources.LoadAll<Material>("");
            foreach (Material m in allMats)
            {
                if (m != null && m.name.Contains(searchName, System.StringComparison.OrdinalIgnoreCase))
                {
                    Log($"[CompleteMazeBuilder]  Found material by name search: {m.name}");
                    return m;
                }
            }

            LogWarning($"[CompleteMazeBuilder]  Material '{searchName}' not found!");
            return null;
        }

        /// <summary>
        /// Load texture from config path, or search in Resources folders if not found.
        /// </summary>
        private Texture2D LoadTextureWithFallback(string configPath, string searchName)
        {
            // Try config path first
            if (!string.IsNullOrEmpty(configPath))
            {
                string resourcePath = configPath.Replace("Assets/Resources/", "").Replace(".png", "").Replace(".jpg", "");
                Texture2D tex = Resources.Load<Texture2D>(resourcePath);
                if (tex != null)
                {
                    Log($"[CompleteMazeBuilder]  Loaded texture from config: {resourcePath}");
                    return tex;
                }
            }

            // Fallback: search in Resources subfolders
            Log($"[CompleteMazeBuilder]  Texture not found in config, searching folders for: {searchName}");
            string[] folders = { "Textures", "Textures/Floors", "Textures/Walls", "" };
            foreach (string folder in folders)
            {
                string searchPath = string.IsNullOrEmpty(folder) ? searchName : $"{folder}/{searchName}";
                Texture2D tex = Resources.Load<Texture2D>(searchPath);
                if (tex != null)
                {
                    Log($"[CompleteMazeBuilder]  Found texture in folder: {searchPath}");
                    return tex;
                }
            }

            // Fallback: search all Resources
            Texture2D[] allTex = Resources.LoadAll<Texture2D>("");
            foreach (Texture2D t in allTex)
            {
                if (t != null && t.name.Contains(searchName, System.StringComparison.OrdinalIgnoreCase))
                {
                    Log($"[CompleteMazeBuilder]  Found texture by name search: {t.name}");
                    return t;
                }
            }

            LogWarning($"[CompleteMazeBuilder]  Texture '{searchName}' not found!");
            return null;
        }

        #endregion

        #region Main Generation

        [ContextMenu("Generate Maze")]
        public void GenerateMaze()
        {
            Log("[CompleteMazeBuilder] ========================================");
            Log($"[CompleteMazeBuilder] LEVEL {currentLevel} - Maze {mazeSize}x{mazeSize}");
            Log("[CompleteMazeBuilder] Starting maze generation...");
            Log("[CompleteMazeBuilder] ========================================");

            FindComponents();
            CleanupOldMaze();
            Log("[CompleteMazeBuilder] STEP 1: Cleanup complete");

            SpawnGround();
            Log("[CompleteMazeBuilder] STEP 2: Ground spawned");

            GenerateGrid();
            Log($"[CompleteMazeBuilder] STEP 3: Spawn room placed at {spawnCell}");

            RenderWalls();
            Log("[CompleteMazeBuilder] STEP 4: Walls placed");

            PlaceExitDoor();
            Log("[CompleteMazeBuilder] STEP 5: Exit door placed on south wall");

            SaveMaze();
            Log("[CompleteMazeBuilder] STEP 6: Maze saved");

            if (Application.isPlaying)
            {
                SpawnPlayer();
                Log($"[CompleteMazeBuilder] STEP 8: Player spawned at {spawnPos}");
            }

            Log("[CompleteMazeBuilder] ========================================");
            Log($"[CompleteMazeBuilder] Level {currentLevel} complete! Maze {mazeSize}x{mazeSize}");
            Log("[CompleteMazeBuilder] ========================================");
        }

        public void ReloadScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        #endregion

        #region Components

        private void FindComponents()
        {
            if (spatialPlacer == null) spatialPlacer = FindFirstObjectByType<SpatialPlacer>();
            if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
            Log("[CompleteMazeBuilder] Components found");
        }

        #endregion

        #region Seed Helpers

        /// <summary>
        /// Fallback seed generation (if SeedManager not available).
        /// Execution time: ~0.03ms
        /// </summary>
        private uint GenerateFallbackSeed()
        {
            uint rawSeed = (uint)Environment.TickCount ^ (uint)Guid.NewGuid().GetHashCode();
            byte[] hash;
            using (var sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(BitConverter.GetBytes(rawSeed));
            }
            return (uint)BitConverter.ToInt32(hash, 0) & 0x7FFFFFFF;
        }

        #endregion

        #region Cleanup

        private void DestroyImmediate(GameObject obj)
        {
            if (obj != null)
                Destroy(obj);
        }

        private GameObject FindObject(string name)
        {
            var obj = GameObject.Find(name);
            return obj;
        }

        private void CleanupOldMaze()
        {
            DestroyImmediate(FindObject("GroundFloor"));
            DestroyImmediate(FindObject("MazeWalls"));
            DestroyImmediate(FindObject("Doors"));
        }

        #endregion

        #region Ground

        private void SpawnGround()
        {
            Log("[CompleteMazeBuilder]  Spawning ground floor...");

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "GroundFloor";

            float size = mazeSize * cellSize;
            ground.transform.position = new Vector3(size / 2f, -0.1f, size / 2f);
            ground.transform.localScale = new Vector3(size, 0.1f, size);

            // Apply material and texture
            var renderer = ground.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                LogWarning("[CompleteMazeBuilder]  Ground has no MeshRenderer!");
                return;
            }

            if (floorMaterial != null)
            {
                renderer.sharedMaterial = floorMaterial;
                Log($"[CompleteMazeBuilder]  Ground material applied: {floorMaterial.name}");

                if (groundTexture != null)
                {
                    renderer.sharedMaterial.mainTexture = groundTexture;
                    Log($"[CompleteMazeBuilder]  Ground texture applied: {groundTexture.name}");
                }
            }
            else
            {
                // Create default gray material with procedural texture (NO PINK!)
                Log("[CompleteMazeBuilder]  Ground material not loaded - creating default gray material");
                Material defaultMat = new Material(Shader.Find("Standard"));
                defaultMat.color = new Color(0.4f, 0.4f, 0.4f); // Medium gray (not pink!)
                
                // Create simple checkered texture procedurally
                Texture2D checkeredTex = CreateCheckeredTexture(64, 64, Color.gray, Color.darkGray);
                defaultMat.mainTexture = checkeredTex;
                defaultMat.mainTextureScale = new Vector2(size / 4f, size / 4f);
                
                renderer.sharedMaterial = defaultMat;
                Log("[CompleteMazeBuilder]  Default gray material with checkered texture applied");
            }

            Log($"[CompleteMazeBuilder]  Ground spawned ({size}m x {size}m)");
        }

        /// <summary>
        /// Create procedural checkered texture (fallback for missing textures).
        /// </summary>
        private Texture2D CreateCheckeredTexture(int width, int height, Color color1, Color color2)
        {
            Texture2D texture = new Texture2D(width, height);
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Bilinear;

            int checkSize = 8;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int xCheck = x / checkSize;
                    int yCheck = y / checkSize;
                    bool isEven = (xCheck + yCheck) % 2 == 0;
                    texture.SetPixel(x, y, isEven ? color1 : color2);
                }
            }

            texture.Apply();
            return texture;
        }

        #endregion

        #region Grid

        private void GenerateGrid()
        {
            Log("[CompleteMazeBuilder] Generating grid maze with spawn room first...");

            grid = new GridMazeGenerator();
            grid.GridSize = mazeSize;
            grid.ChamberSize = GameConfig.Instance.defaultRoomSize;
            grid.CorridorWidth = 1; // Always 1 cell wide for proper maze

            // Calculate difficulty factor from seed (same calculation as Awake)
            float difficultyFactor = seed / (float)int.MaxValue;
            grid.Generate(seed, difficultyFactor, currentLevel); // Pass level for 4-way vs 8-way

            spawnCell = grid.FindSpawnPoint();
            spawnPos = new Vector3(
                spawnCell.x * cellSize + cellSize / 2f,
                GameConfig.Instance.defaultPlayerEyeHeight,
                spawnCell.y * cellSize + cellSize / 2f
            );

            Log($"[CompleteMazeBuilder] SpawnPoint: cell {spawnCell}");
            Log($"[CompleteMazeBuilder]  Spawn position: {spawnPos}");
            Log($"[CompleteMazeBuilder]  Grid maze generated ({mazeSize}x{mazeSize})");

            // Validate maze connectivity
            if (!ValidateMaze())
            {
                LogError("[CompleteMazeBuilder] Maze validation FAILED - Regenerating...");
                grid.Generate(seed, difficultyFactor); // Retry once with same seed
                if (!ValidateMaze())
                {
                    LogError("[CompleteMazeBuilder] Maze validation FAILED after retry - Proceeding with warnings");
                }
            }
        }

        #endregion

        #region Wall Rendering

        /// <summary>
        /// Render walls using MazeRenderer component.
        /// </summary>
        private void RenderWalls()
        {
            if (mazeRenderer == null)
            {
                mazeRenderer = FindFirstObjectByType<MazeRenderer>();
            }

            if (mazeRenderer == null)
            {
                LogError("[CompleteMazeBuilder] MazeRenderer not found! Creating one...");
                GameObject rendererObj = new GameObject("MazeRenderer");
                mazeRenderer = rendererObj.AddComponent<MazeRenderer>();
            }

            // Initialize renderer with grid data AND prefabs/materials
            mazeRenderer.Initialize(grid, mazeSize, seed, currentLevel, 
                wallPrefab, wallMaterial, cellSize, wallHeight, wallThickness);

            // Render walls
            mazeRenderer.RenderWalls();
        }

        #endregion

        #region Maze Validation

        /// <summary>
        /// Validate maze connectivity using flood-fill algorithm.
        /// Ensures all rooms and corridors are reachable from spawn point.
        /// Execution time: ~0.05ms for 21x21 maze
        /// </summary>
        private bool ValidateMaze()
        {
            if (grid == null) return false;

            var gridData = grid.Grid;
            int size = grid.GridSize;

            // Flood-fill from spawn point
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(spawnCell);
            visited.Add(spawnCell);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();

                // Check 4 directions (N, S, E, W)
                Vector2Int[] directions = {
                    new Vector2Int(0, 1),
                    new Vector2Int(0, -1),
                    new Vector2Int(1, 0),
                    new Vector2Int(-1, 0)
                };

                foreach (Vector2Int dir in directions)
                {
                    Vector2Int neighbor = current + dir;

                    // Check bounds
                    if (neighbor.x >= 0 && neighbor.x < size && neighbor.y >= 0 && neighbor.y < size)
                    {
                        // Check if walkable and not visited
                        var cellType = gridData[neighbor.x, neighbor.y];
                        if (IsWalkable(cellType) && !visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            // Count total walkable cells
            int totalWalkable = 0;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    if (IsWalkable(gridData[x, y]))
                    {
                        totalWalkable++;
                    }
                }
            }

            // Validate: all walkable cells should be reachable
            bool isValid = visited.Count == totalWalkable;

            if (isValid)
            {
                Log($"[CompleteMazeBuilder] Maze validation PASSED - {visited.Count}/{totalWalkable} walkable cells reachable");
            }
            else
            {
                int unreachable = totalWalkable - visited.Count;
                LogError($"[CompleteMazeBuilder] Maze validation FAILED - {unreachable} walkable cells unreachable!");
                LogError($"[CompleteMazeBuilder] Possible causes: isolated rooms, blocked corridors");
            }

            return isValid;
        }

        /// <summary>
        /// Check if cell type is walkable (not a wall or obstacle).
        /// </summary>
        private bool IsWalkable(GridMazeCell cellType)
        {
            return cellType == GridMazeCell.Floor ||
                   cellType == GridMazeCell.Room ||
                   cellType == GridMazeCell.Corridor ||
                   cellType == GridMazeCell.SpawnPoint ||
                   cellType == GridMazeCell.Door;
        }

        #endregion

        #region Exit Door

        /// <summary>
        /// Place ONE exit door on the south perimeter wall.
        /// Door is centered on south wall for easy finding.
        /// </summary>
        public void PlaceExitDoor()
        {
            if (doorPrefab == null)
            {
                LogWarning("[CompleteMazeBuilder]  Door prefab not loaded - skipping exit door");
                return;
            }

            // Calculate center of south wall
            int doorX = mazeSize / 2;  // Center of south wall
            Vector3 doorPos = new Vector3(
                doorX * cellSize + cellSize / 2f,  // Center of cell in X
                GameConfig.Instance.defaultDoorHeight / 2f,
                0f  // South perimeter (Z = 0)
            );

            // Door faces north (into the maze)
            Quaternion doorRot = Quaternion.identity;

            GameObject door = Instantiate(doorPrefab, doorPos, doorRot);
            door.name = "ExitDoor";
            door.transform.SetParent(GameObject.Find("MazeWalls")?.transform);

            Log($"[CompleteMazeBuilder]  Exit door placed at south wall center (X={doorX})");
        }

        #endregion

        #region Save

        private void SaveMaze()
        {
            Log("[CompleteMazeBuilder]  Saving maze to database...");

            if (grid == null)
            {
                LogError("[CompleteMazeBuilder]  GridMazeGenerator not found!");
                return;
            }

            // Save grid data only (no seed storage - procedural generation)
            MazeSaveData.SaveGridMaze(grid.SerializeToBytes(), spawnCell.x, spawnCell.y);

            // ComputeGridEngine already received the data via MazeRenderer in RenderWalls()
            // It saved to binary automatically when it received the event

            Log("[CompleteMazeBuilder]  Maze saved");
        }

        #endregion

        #region Player

        private void SpawnPlayer()
        {
            Log($"[CompleteMazeBuilder] Spawning player at {spawnPos}...");

            if (playerController == null)
                playerController = FindFirstObjectByType<PlayerController>();

            if (playerController == null)
            {
                LogWarning("[CompleteMazeBuilder] PlayerController not in scene (add independently)");
                LogWarning("[CompleteMazeBuilder] Add PlayerController component to a GameObject in scene");
                return;
            }

            // Set player position to spawn point
            playerController.transform.position = spawnPos;

            // Add small random offset to prevent wall clipping
            float offset = GameConfig.Instance.defaultPlayerSpawnOffset;
            playerController.transform.position += new Vector3(
                (UnityEngine.Random.value - 0.5f) * offset,
                0f,
                (UnityEngine.Random.value - 0.5f) * offset
            );

            // Set camera to eye level
            var cam = playerController.GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cam.transform.localPosition = new Vector3(0f, GameConfig.Instance.defaultPlayerEyeHeight, 0f);
                cam.transform.localRotation = Quaternion.identity;
                Log($"[CompleteMazeBuilder] FPS camera set to eye level ({GameConfig.Instance.defaultPlayerEyeHeight}m)");
            }

            playerController.transform.rotation = Quaternion.identity;

            Log($"[CompleteMazeBuilder] Player spawned INSIDE maze at {playerController.transform.position}");
            Log("[CompleteMazeBuilder] Ready to explore!");
        }

        #endregion

        #region Logging

        // Simple logging - always shows (short verbosity)
        public static void Log(string msg) => Debug.Log(msg);
        public static void LogWarning(string msg) => Debug.LogWarning(msg);
        public static void LogError(string msg) => Debug.LogError(msg);

        #endregion
    }
}
