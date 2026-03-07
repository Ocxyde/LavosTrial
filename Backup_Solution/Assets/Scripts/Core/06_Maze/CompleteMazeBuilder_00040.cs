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

using System.Collections.Generic;
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

        [Header("Game State")]
        [SerializeField] private int currentLevel = 0;
        [SerializeField] private string currentSeed = "MazeSeed2026";

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
        public string CurrentSeed => currentSeed;

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
            mazeSize = 12 + currentLevel;
            mazeSize = Mathf.Clamp(mazeSize, 12, 51);

            if (EventHandler.Instance != null)
                Log("[CompleteMazeBuilder] Connected to EventHandler");

            seed = ComputeSeed(currentSeed);

            Log($"[CompleteMazeBuilder] Level {currentLevel} - Maze {mazeSize}x{mazeSize}");
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
            mazeSize = Mathf.Clamp(12 + currentLevel, 12, 51);
            Log($"[CompleteMazeBuilder]  Level {currentLevel} - Maze {mazeSize}x{mazeSize}");
        }

        public void SetSeed(string s)
        {
            currentSeed = s;
            seed = ComputeSeed(s);
        }

        #endregion

        #region Config Loading

        private void LoadConfig()
        {
            var cfg = GameConfig.Instance;
            cellSize = cfg.defaultCellSize;
            wallHeight = cfg.defaultWallHeight;
            wallThickness = cfg.defaultWallThickness;

            wallPrefab = LoadPrefab(cfg.wallPrefab);
            doorPrefab = LoadPrefab(cfg.doorPrefab);

            wallMaterial = LoadMaterial(cfg.wallMaterial);
            floorMaterial = LoadMaterial(cfg.floorMaterial);
            groundTexture = LoadTexture(cfg.groundTexture);
        }

        private GameObject LoadPrefab(string path) =>
            Resources.Load<GameObject>(path.Replace("Assets/Resources/", "").Replace(".prefab", ""));

        private Material LoadMaterial(string path) =>
            Resources.Load<Material>(path.Replace("Assets/Resources/", "").Replace(".mat", ""));

        private Texture2D LoadTexture(string path) =>
            Resources.Load<Texture2D>(path.Replace("Assets/Resources/", "").Replace(".png", ""));

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

            PlaceWalls();
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

            if (floorMaterial != null)
            {
                var r = ground.GetComponent<MeshRenderer>();
                if (r != null) r.sharedMaterial = floorMaterial;
            }

            if (groundTexture != null)
            {
                var r = ground.GetComponent<MeshRenderer>();
                if (r != null && r.sharedMaterial != null)
                    r.sharedMaterial.mainTexture = groundTexture;
            }

            Log($"[CompleteMazeBuilder]  Ground spawned ({size}m x {size}m)");
        }

        #endregion

        #region Grid

        private void GenerateGrid()
        {
            Log("[CompleteMazeBuilder] Generating grid maze with spawn room first...");

            grid = new GridMazeGenerator();
            grid.gridSize = mazeSize;
            grid.roomSize = GameConfig.Instance.defaultRoomSize;
            grid.corridorWidth = GameConfig.Instance.defaultCorridorWidth;
            grid.Generate();

            spawnCell = grid.FindSpawnPoint();
            spawnPos = new Vector3(
                spawnCell.x * cellSize + cellSize / 2f,
                GameConfig.Instance.defaultPlayerEyeHeight,
                spawnCell.y * cellSize + cellSize / 2f
            );

            Log($"[CompleteMazeBuilder] SpawnPoint: cell {spawnCell}");
            Log($"[CompleteMazeBuilder]  Spawn position: {spawnPos}");
            Log($"[CompleteMazeBuilder]  Grid maze generated ({mazeSize}x{mazeSize})");
        }

        #endregion

        #region Walls

        /// <summary>
        /// Place walls on outer perimeter and interior cell boundaries.
        /// Also applies walls byte-to-byte to ComputeGridEngine.
        /// </summary>
        public void PlaceWalls()
        {
            if (grid == null)
            {
                LogError("[CompleteMazeBuilder] Grid not generated! Call GenerateGrid() first");
                return;
            }

            Log("[CompleteMazeBuilder] Computing walls from grid...");

            // Initialize compute grid engine (byte-to-byte storage)
            ComputeGridEngine computeGrid = null;
            if (Application.isPlaying)
            {
                computeGrid = FindFirstObjectByType<ComputeGridEngine>();
                if (computeGrid != null)
                {
                    computeGrid.SetGridSize(mazeSize);
                    computeGrid.SetMazeId($"Maze_{currentLevel:D3}");
                    computeGrid.SetMazeSeed((int)seed);
                    computeGrid.ClearGrid();
                    Log("[CompleteMazeBuilder] ComputeGridEngine initialized for byte-to-byte storage");
                }
            }

            int spawned = 0;

            // Destroy existing walls first
            var existingWalls = GameObject.Find("MazeWalls");
            if (existingWalls != null)
            {
                if (Application.isPlaying)
                    Destroy(existingWalls);
                else
                    DestroyImmediate(existingWalls);
            }

            GameObject parent = new GameObject("MazeWalls");

            // Apply walls to compute grid (byte-to-byte) BEFORE spawning
            ApplyWallsToComputeGrid(computeGrid);

            PlaceOuterPerimeterWalls(parent.transform, ref spawned);
            PlaceInteriorWalls(parent.transform, ref spawned);

            Log($"[CompleteMazeBuilder] {spawned} wall segments placed");
        }

        /// <summary>
        /// Apply wall data byte-to-byte to ComputeGridEngine.
        /// This writes wall positions directly to RAM storage.
        /// </summary>
        private void ApplyWallsToComputeGrid(ComputeGridEngine computeGrid)
        {
            if (computeGrid == null) return;

            Log("[CompleteMazeBuilder] Applying walls byte-to-byte to ComputeGrid...");

            // Apply outer perimeter walls
            for (int x = 0; x < mazeSize; x++)
            {
                // North wall
                computeGrid.SetCell(x, mazeSize - 1, ComputeGridEngine.GridCell.Wall);
                // South wall
                computeGrid.SetCell(x, 0, ComputeGridEngine.GridCell.Wall);
            }

            for (int z = 0; z < mazeSize; z++)
            {
                // East wall
                computeGrid.SetCell(mazeSize - 1, z, ComputeGridEngine.GridCell.Wall);
                // West wall
                computeGrid.SetCell(0, z, ComputeGridEngine.GridCell.Wall);
            }

            // Apply interior walls from grid
            for (int x = 0; x < mazeSize; x++)
            {
                for (int z = 0; z < mazeSize; z++)
                {
                    GridMazeCell cell = grid.GetCell(x, z);
                    
                    // Map GridMazeCell to ComputeGridEngine.GridCell
                    ComputeGridEngine.GridCell computeCell = cell switch
                    {
                        GridMazeCell.Floor => ComputeGridEngine.GridCell.Floor,
                        GridMazeCell.Room => ComputeGridEngine.GridCell.Room,
                        GridMazeCell.Corridor => ComputeGridEngine.GridCell.Corridor,
                        GridMazeCell.Wall => ComputeGridEngine.GridCell.Wall,
                        GridMazeCell.SpawnPoint => ComputeGridEngine.GridCell.SpawnPoint,
                        _ => ComputeGridEngine.GridCell.Floor
                    };

                    computeGrid.SetCell(x, z, computeCell);
                }
            }

            Log($"[CompleteMazeBuilder] Walls applied byte-to-byte to RAM ({computeGrid.GetMemoryUsageKB()}KB)");
        }

        /// <summary>
        /// Place walls on outer perimeter (north, south, east, west).
        /// </summary>
        private void PlaceOuterPerimeterWalls(Transform parent, ref int spawned)
        {
            // NORTH WALL (Z = mazeSize * cellSize)
            for (int x = 0; x < mazeSize; x++)
            {
                Vector3 pos = new Vector3(
                    x * cellSize + cellSize / 2f,
                    wallHeight / 2f,
                    mazeSize * cellSize
                );
                SpawnWall(pos, Quaternion.identity, $"North_{x}", parent);
                spawned++;
            }

            // SOUTH WALL (Z = 0)
            for (int x = 0; x < mazeSize; x++)
            {
                Vector3 pos = new Vector3(
                    x * cellSize + cellSize / 2f,
                    wallHeight / 2f,
                    0f
                );
                SpawnWall(pos, Quaternion.identity, $"South_{x}", parent);
                spawned++;
            }

            // EAST WALL (X = mazeSize * cellSize)
            for (int z = 0; z < mazeSize; z++)
            {
                Vector3 pos = new Vector3(
                    mazeSize * cellSize,
                    wallHeight / 2f,
                    z * cellSize + cellSize / 2f
                );
                SpawnWall(pos, Quaternion.Euler(0f, 90f, 0f), $"East_{z}", parent);
                spawned++;
            }

            // WEST WALL (X = 0)
            for (int z = 0; z < mazeSize; z++)
            {
                Vector3 pos = new Vector3(
                    0f,
                    wallHeight / 2f,
                    z * cellSize + cellSize / 2f
                );
                SpawnWall(pos, Quaternion.Euler(0f, 90f, 0f), $"West_{z}", parent);
                spawned++;
            }
        }

        /// <summary>
        /// Place interior walls between adjacent cells (Room/Corridor vs Floor).
        /// </summary>
        private void PlaceInteriorWalls(Transform parent, ref int spawned)
        {
            int interiorCount = 0;

            for (int x = 0; x < mazeSize; x++)
            {
                for (int y = 0; y < mazeSize; y++)
                {
                    GridMazeCell current = grid.GetCell(x, y);

                    if (current != GridMazeCell.Room && current != GridMazeCell.Corridor)
                        continue;

                    // Check EAST boundary
                    if (x + 1 < mazeSize)
                    {
                        GridMazeCell east = grid.GetCell(x + 1, y);
                        if (NeedsWallBetween(current, east))
                        {
                            Vector3 pos = new Vector3(
                                (x + 1) * cellSize,
                                wallHeight / 2f,
                                y * cellSize + cellSize / 2f
                            );
                            SpawnWall(pos, Quaternion.Euler(0f, 90f, 0f), $"Wall_E_{x}_{y}", parent);
                            spawned++;
                            interiorCount++;
                        }
                    }

                    // Check SOUTH boundary
                    if (y + 1 < mazeSize)
                    {
                        GridMazeCell south = grid.GetCell(x, y + 1);
                        if (NeedsWallBetween(current, south))
                        {
                            Vector3 pos = new Vector3(
                                x * cellSize + cellSize / 2f,
                                wallHeight / 2f,
                                (y + 1) * cellSize
                            );
                            SpawnWall(pos, Quaternion.identity, $"Wall_S_{x}_{y}", parent);
                            spawned++;
                            interiorCount++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determine if wall needed between two adjacent cells.
        /// </summary>
        private bool NeedsWallBetween(GridMazeCell a, GridMazeCell b)
        {
            bool aWalkable = (a == GridMazeCell.Room || a == GridMazeCell.Corridor);
            bool bWalkable = (b == GridMazeCell.Room || b == GridMazeCell.Corridor);
            return aWalkable != bWalkable;
        }

        #endregion

        #region Exit Door

        private void SpawnWall(Vector3 pos, Quaternion rot, string name, Transform parent)
        {
            if (wallPrefab == null)
            {
                LogError($"[CompleteMazeBuilder] Wall prefab not loaded! Cannot spawn wall at {pos}");
                LogError("[CompleteMazeBuilder] Fix: Run Tools -> Quick Setup Prefabs");
                return;
            }

            GameObject wall = Instantiate(wallPrefab, pos, rot);
            wall.name = $"Wall_{name}";
            wall.transform.SetParent(parent);

            if (wallMaterial != null)
            {
                var r = wall.GetComponent<MeshRenderer>();
                if (r != null) r.sharedMaterial = wallMaterial;
            }
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

            // Save to legacy MazeSaveData (PlayerPrefs)
            MazeSaveData.SaveGridMaze((int)seed, grid.SerializeToBytes(), spawnCell.x, spawnCell.y);

            // Save to ComputeGridEngine (byte-to-byte binary storage)
            if (Application.isPlaying)
            {
                ComputeGridEngine computeGrid = FindFirstObjectByType<ComputeGridEngine>();
                if (computeGrid != null && computeGrid.IsInitialized)
                {
                    computeGrid.SaveToBinary();
                    Log("[CompleteMazeBuilder]  Maze saved to ComputeGrid binary storage");
                }
            }

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
                (Random.value - 0.5f) * offset,
                0f,
                (Random.value - 0.5f) * offset
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

        #region Utilities

        private uint ComputeSeed(string s)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(s);
            uint hash = 0;
            for (int i = 0; i < bytes.Length; i++)
                hash = hash * 31 + bytes[i];
            return hash;
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
