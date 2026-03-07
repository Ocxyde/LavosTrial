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

            // Generate random seed from system entropy (truly random each load)
            uint rawSeed = (uint)System.Environment.TickCount ^ (uint)System.Guid.NewGuid().GetHashCode();
            
            // Hash seed for better distribution and encryption (SHA256 -> first 4 bytes)
            byte[] seedBytes = System.BitConverter.GetBytes(rawSeed);
            byte[] hash;
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                hash = sha256.ComputeHash(seedBytes);
            }
            seed = (uint)System.BitConverter.ToInt32(hash, 0) & 0x7FFFFFFF; // Ensure positive
            
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
            
            // Calculate difficulty factor from seed (same calculation as Awake)
            float difficultyFactor = seed / (float)int.MaxValue;
            grid.Generate(seed, difficultyFactor);

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
        /// Also publishes events to ComputeGridEngine via EventHandler.
        /// </summary>
        public void PlaceWalls()
        {
            if (grid == null)
            {
                LogError("[CompleteMazeBuilder] Grid not generated! Call GenerateGrid() first");
                return;
            }

            Log("[CompleteMazeBuilder] Computing walls from grid...");

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

            // Build grid data for compute grid (byte-to-byte)
            byte[] gridBytes = BuildGridBytesForComputeGrid();

            // Publish to ComputeGridEngine via EventHandler (plug-in-out!)
            if (Application.isPlaying && EventHandler.Instance != null)
            {
                string mazeId = $"Maze_{currentLevel:D3}";
                EventHandler.Instance.InvokeComputeGridSaveRequested(mazeId, gridBytes, (int)seed);
                Log($"[CompleteMazeBuilder] Published compute grid save event: {mazeId}");
            }

            PlaceOuterPerimeterWalls(parent.transform, ref spawned);
            PlaceInteriorWalls(parent.transform, ref spawned);

            Log($"[CompleteMazeBuilder] {spawned} wall segments placed");
        }

        /// <summary>
        /// Build grid byte array from current grid state.
        /// This is the data that will be sent to ComputeGridEngine.
        /// </summary>
        private byte[] BuildGridBytesForComputeGrid()
        {
            byte[] gridBytes = new byte[mazeSize * mazeSize];
            int index = 0;

            for (int z = 0; z < mazeSize; z++)
            {
                for (int x = 0; x < mazeSize; x++)
                {
                    GridMazeCell cell = grid.GetCell(x, z);

                    // Map GridMazeCell to ComputeGridEngine.GridCell
                    byte computeCell = cell switch
                    {
                        GridMazeCell.Floor => (byte)ComputeGridEngine.GridCell.Floor,
                        GridMazeCell.Room => (byte)ComputeGridEngine.GridCell.Room,
                        GridMazeCell.Corridor => (byte)ComputeGridEngine.GridCell.Corridor,
                        GridMazeCell.Wall => (byte)ComputeGridEngine.GridCell.Wall,
                        GridMazeCell.SpawnPoint => (byte)ComputeGridEngine.GridCell.SpawnPoint,
                        _ => (byte)ComputeGridEngine.GridCell.Floor
                    };

                    gridBytes[index++] = computeCell;
                }
            }

            Log($"[CompleteMazeBuilder] Grid bytes built: {gridBytes.Length} bytes");
            return gridBytes;
        }

        /// <summary>
        /// Place walls on outer perimeter (north, south, east, west).
        /// Walls are snapped to cell edges (boundaries), not centers.
        /// </summary>
        private void PlaceOuterPerimeterWalls(Transform parent, ref int spawned)
        {
            // NORTH WALL (Z = mazeSize * cellSize) - on outer edge
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

            // SOUTH WALL (Z = 0) - on outer edge
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

            // EAST WALL (X = mazeSize * cellSize) - on outer edge
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

            // WEST WALL (X = 0) - on outer edge
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
        /// Walls are snapped to cell edges (grid lines), not cell centers.
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

                    // Check EAST boundary - wall on vertical grid line at x+1
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

                    // Check SOUTH boundary - wall on horizontal grid line at y+1
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

        /// <summary>
        /// Spawn single wall segment at specified position.
        /// Wall prefab is centered on cell edge (snapped to grid line).
        /// </summary>
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

            // Save grid data only (no seed storage - procedural generation)
            MazeSaveData.SaveGridMaze(grid.SerializeToBytes(), spawnCell.x, spawnCell.y);

            // ComputeGridEngine already received the data via EventHandler in PlaceWalls()
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

        #region Logging

        // Simple logging - always shows (short verbosity)
        public static void Log(string msg) => Debug.Log(msg);
        public static void LogWarning(string msg) => Debug.LogWarning(msg);
        public static void LogError(string msg) => Debug.LogError(msg);

        #endregion
    }
}
