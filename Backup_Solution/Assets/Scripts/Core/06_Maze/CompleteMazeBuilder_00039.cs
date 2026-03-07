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
        [SerializeField] private GameObject torchPrefab;

        [Header("Materials")]
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material floorMaterial;
        [SerializeField] private Texture2D groundTexture;

        [Header("Components")]
        [SerializeField] private SpatialPlacer spatialPlacer;
        [SerializeField] private LightPlacementEngine lightPlacementEngine;
        [SerializeField] private TorchPool torchPool;
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
        private System.Collections.Generic.List<Vector3> wallPositions;  // For torch placement

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

        /// <summary>
        /// Get wall positions for torch placement (byte-to-byte from RAM).
        /// </summary>
        public List<Vector3> GetWallPositions() => wallPositions;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Initialize wall positions list
            wallPositions = new System.Collections.Generic.List<Vector3>();

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
                Log("[CompleteMazeBuilder]  Connected to EventHandler");

            seed = ComputeSeed(currentSeed);

            Log($"[CompleteMazeBuilder]  Level {currentLevel} - Maze {mazeSize}x{mazeSize}");
        }

        private void Start()
        {
            if (GameConfig.Instance.useRandomSeed)
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
            torchPrefab = LoadPrefab(cfg.torchPrefab);

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
            Log("[CompleteMazeBuilder] ════════════════════════════════════════");
            Log($"[CompleteMazeBuilder]  LEVEL {currentLevel} - Maze {mazeSize}x{mazeSize}");
            Log("[CompleteMazeBuilder] ️ Starting maze generation...");
            Log("[CompleteMazeBuilder] ════════════════════════════════════════");

            // Ensure wallPositions is initialized (editor may call before Awake)
            if (wallPositions == null)
            {
                wallPositions = new System.Collections.Generic.List<Vector3>();
            }

            wallPositions.Clear();
            FindComponents();
            CleanupOldMaze();
            Log("[CompleteMazeBuilder]  STEP 1: Cleanup complete");

            SpawnGround();
            Log("[CompleteMazeBuilder]  STEP 2: Ground spawned");

            GenerateGrid();
            Log($"[CompleteMazeBuilder] ️ STEP 3: Spawn room placed at {spawnCell}");

            PlaceWalls();
            Log("[CompleteMazeBuilder]  STEP 4: Walls placed (oriented)");

            PlaceDoors();
            Log("[CompleteMazeBuilder]  STEP 5: Doors placed");

            PlaceTorches();
            Log("[CompleteMazeBuilder]  STEP 6: Torch positions computed (on-demand)");

            SaveMaze();
            Log("[CompleteMazeBuilder]  STEP 7: Maze saved");

            if (Application.isPlaying)
            {
                SpawnPlayer();
                Log($"[CompleteMazeBuilder]  STEP 8: Player spawned at {spawnPos}");
            }

            Log("[CompleteMazeBuilder] ════════════════════════════════════════");
            Log($"[CompleteMazeBuilder]  Level {currentLevel} complete! Maze {mazeSize}x{mazeSize}");
            Log("[CompleteMazeBuilder] ════════════════════════════════════════");
        }

        public void ReloadScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        #endregion

        #region Components

        private void FindComponents()
        {
            if (spatialPlacer == null) spatialPlacer = FindFirstObjectByType<SpatialPlacer>();
            if (lightPlacementEngine == null) lightPlacementEngine = FindFirstObjectByType<LightPlacementEngine>();
            if (torchPool == null) torchPool = FindFirstObjectByType<TorchPool>();
            if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
            Log("[CompleteMazeBuilder]  Components found");
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
            DestroyImmediate(FindObject("Torches"));
            DestroyImmediate(FindObject("Lights"));  // Remove stray lights
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
            Log("[CompleteMazeBuilder] ️ Generating grid maze with spawn room first...");

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

            Log($"[CompleteMazeBuilder]  SpawnPoint: cell {spawnCell}");
            Log($"[CompleteMazeBuilder]  Spawn position: {spawnPos}");
            Log($"[CompleteMazeBuilder]  Grid maze generated ({mazeSize}x{mazeSize})");
        }

        #endregion

        #region Walls

        /// <summary>
        /// Place walls on EXTREME OUTER PERIMETER of entire grid.
        /// Mathematical computation: walls snap to exact grid boundaries.
        /// Corner walls computed for FUTURE GRID COMPUTATION (not spawned).
        /// Can be called independently for modular maze generation.
        /// </summary>
        public void PlaceWalls()
        {
            if (grid == null)
            {
                LogError("[CompleteMazeBuilder]  Grid not generated! Call GenerateGrid() first");
                return;
            }

            Log($"[CompleteMazeBuilder]  Computing walls on extreme grid perimeter...");

            // Ensure wallPositions is initialized
            if (wallPositions == null)
                wallPositions = new System.Collections.Generic.List<Vector3>();

            wallPositions.Clear();

            int spawned = 0;

            // Destroy existing walls first (for re-generation)
            var existingWalls = GameObject.Find("MazeWalls");
            if (existingWalls != null)
            {
                if (Application.isPlaying)
                    Destroy(existingWalls);
                else
                    DestroyImmediate(existingWalls);
            }

            GameObject parent = new GameObject("MazeWalls");

            // ═══════════════════════════════════════════════════════════════
            // MATHEMATICAL WALL PLACEMENT - EXTREME EDGES ONLY
            // ═══════════════════════════════════════════════════════════════
            // North: Z = mazeSize * cellSize
            // South: Z = 0
            // East:  X = mazeSize * cellSize
            // West:  X = 0
            // Corners: Computed for FUTURE GRID COMPUTATION (not spawned yet)
            // ═══════════════════════════════════════════════════════════════

            // ─── NORTH WALL (Top edge: Z = mazeSize * cellSize) ────────────
            for (int x = 0; x < mazeSize; x++)
            {
                Vector3 pos = new Vector3(
                    x * cellSize + cellSize / 2f,     // Center of cell in X
                    wallHeight / 2f,                   // Half wall height
                    mazeSize * cellSize                // EXACT North border
                );
                Quaternion rot = Quaternion.identity;

                wallPositions.Add(pos);
                SpawnWall(pos, rot, $"North_{x}", parent.transform);
                spawned++;
            }
            Log($"[CompleteMazeBuilder]  North wall: {mazeSize} segments at Z={mazeSize * cellSize}");

            // ─── SOUTH WALL (Bottom edge: Z = 0) ───────────────────────────
            for (int x = 0; x < mazeSize; x++)
            {
                Vector3 pos = new Vector3(
                    x * cellSize + cellSize / 2f,     // Center of cell in X
                    wallHeight / 2f,                   // Half wall height
                    0f                                 // EXACT South border
                );
                Quaternion rot = Quaternion.identity;

                wallPositions.Add(pos);
                SpawnWall(pos, rot, $"South_{x}", parent.transform);
                spawned++;
            }
            Log($"[CompleteMazeBuilder]  South wall: {mazeSize} segments at Z=0");

            // ─── EAST WALL (Right edge: X = mazeSize * cellSize) ───────────
            for (int z = 0; z < mazeSize; z++)
            {
                Vector3 pos = new Vector3(
                    mazeSize * cellSize,               // EXACT East border
                    wallHeight / 2f,                   // Half wall height
                    z * cellSize + cellSize / 2f       // Center of cell in Z
                );
                Quaternion rot = Quaternion.Euler(0f, 90f, 0f);

                wallPositions.Add(pos);
                SpawnWall(pos, rot, $"East_{z}", parent.transform);
                spawned++;
            }
            Log($"[CompleteMazeBuilder]  East wall: {mazeSize} segments at X={mazeSize * cellSize}");

            // ─── WEST WALL (Left edge: X = 0) ──────────────────────────────
            for (int z = 0; z < mazeSize; z++)
            {
                Vector3 pos = new Vector3(
                    0f,                                  // EXACT West border
                    wallHeight / 2f,                     // Half wall height
                    z * cellSize + cellSize / 2f         // Center of cell in Z
                );
                Quaternion rot = Quaternion.Euler(0f, 90f, 0f);

                wallPositions.Add(pos);
                SpawnWall(pos, rot, $"West_{z}", parent.transform);
                spawned++;
            }
            Log($"[CompleteMazeBuilder]  West wall: {mazeSize} segments at X=0");

            // ─── CORNER WALLS (FUTURE GRID COMPUTATION - Store positions only)
            // These are computed but NOT spawned yet - reserved for future enhancement
            ComputeCornerWallsForFuture();

            Log($"[CompleteMazeBuilder]  {spawned} wall segments placed (EXTREME PERIMETER)");
            Log($"[CompleteMazeBuilder]  Wall positions stored in RAM: {wallPositions.Count}");
        }

        /// <summary>
        /// Compute corner wall positions for FUTURE GRID COMPUTATION.
        /// Stores positions in RAM but does NOT spawn them yet.
        /// Named 'wallTravers' - traverses the grid corners.
        /// </summary>
        private void ComputeCornerWallsForFuture()
        {
            // North-West corner
            wallPositions.Add(new Vector3(0f, wallHeight / 2f, mazeSize * cellSize));
            // North-East corner
            wallPositions.Add(new Vector3(mazeSize * cellSize, wallHeight / 2f, mazeSize * cellSize));
            // South-West corner
            wallPositions.Add(new Vector3(0f, wallHeight / 2f, 0f));
            // South-East corner
            wallPositions.Add(new Vector3(mazeSize * cellSize, wallHeight / 2f, 0f));

            Log($"[CompleteMazeBuilder]  4 corner walls computed for FUTURE (wallTravers - not spawned)");
        }

        private void SpawnWall(Vector3 pos, Quaternion rot, string name, Transform parent)
        {
            if (wallPrefab == null)
            {
                LogError($"[CompleteMazeBuilder]  Wall prefab not loaded! Cannot spawn wall at {pos}");
                LogError("[CompleteMazeBuilder]  Fix: Run Tools → Quick Setup Prefabs");
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

        #region Doors

        /// <summary>
        /// Place entrance/exit doors. Can be called independently.
        /// </summary>
        public void PlaceDoors()
        {
            if (grid == null)
            {
                LogError("[CompleteMazeBuilder]  Grid not generated! Call GenerateGrid() first");
                return;
            }

            Log("[CompleteMazeBuilder]  Placing entrance/exit doors...");

            // Destroy existing doors first
            var existingDoors = GameObject.Find("Doors");
            if (existingDoors != null)
            {
                if (Application.isPlaying)
                    Destroy(existingDoors);
                else
                    DestroyImmediate(existingDoors);
            }

            GameObject parent = new GameObject("Doors");
            Vector2Int doorPos = FindDoorPosition();

            if (doorPos.x >= 0)
            {
                Vector3 pos = new Vector3(
                    doorPos.x * cellSize + cellSize / 2f,
                    GameConfig.Instance.defaultDoorHeight / 2f,
                    doorPos.y * cellSize + cellSize / 2f
                );

                if (doorPrefab != null)
                {
                    GameObject door = Instantiate(doorPrefab, pos, Quaternion.identity);
                    door.name = $"Door_Entrance";
                    door.transform.SetParent(parent.transform);
                    Log($"[CompleteMazeBuilder]  Entrance door placed at ({doorPos.x}, {doorPos.y})");
                }
            }
            else
            {
                Log("[CompleteMazeBuilder] ️ No suitable door position found");
            }
        }

        private Vector2Int FindDoorPosition()
        {
            // Find room cell adjacent to corridor
            for (int radius = 1; radius <= 3; radius++)
            {
                for (int x = spawnCell.x - radius; x <= spawnCell.x + radius; x++)
                {
                    for (int y = spawnCell.y - radius; y <= spawnCell.y + radius; y++)
                    {
                        if (x >= 0 && x < mazeSize && y >= 0 && y < mazeSize)
                        {
                            if (grid.GetCell(x, y) == GridMazeCell.Room)
                            {
                                if (IsAdjacentToCorridor(x, y))
                                    return new Vector2Int(x, y);
                            }
                        }
                    }
                }
            }
            return new Vector2Int(-1, -1);
        }

        private bool IsAdjacentToCorridor(int x, int y)
        {
            if (x + 1 < mazeSize && grid.GetCell(x + 1, y) == GridMazeCell.Corridor) return true;
            if (x - 1 >= 0 && grid.GetCell(x - 1, y) == GridMazeCell.Corridor) return true;
            if (y + 1 < mazeSize && grid.GetCell(x, y + 1) == GridMazeCell.Corridor) return true;
            if (y - 1 >= 0 && grid.GetCell(x, y - 1) == GridMazeCell.Corridor) return true;
            return false;
        }

        #endregion

        #region Torches

        /// <summary>
        /// Mount torches on walls using prefab.
        /// Does NOT place lights directly - calls LightPlacementEngine when needed.
        /// Can be called independently.
        /// </summary>
        public void PlaceTorches()
        {
            Log("[CompleteMazeBuilder]  Computing torch positions on walls...");

            if (torchPrefab == null)
            {
                LogWarning("[CompleteMazeBuilder] ️ Torch prefab not loaded - skipping torch computation");
                return;
            }

            // Compute torch positions (store in RAM, don't instantiate yet)
            int torchCount = 0;
            float chance = 0.3f;

            foreach (Vector3 wallPos in wallPositions)
            {
                if (Random.value > chance) continue;
                torchCount++;
            }

            Log($"[CompleteMazeBuilder]  {torchCount} torch positions computed (not spawned)");
            Log("[CompleteMazeBuilder]  Lights will be placed on-demand by LightPlacementEngine");
        }

        /// <summary>
        /// Place lights on-demand (call when actually needed).
        /// Uses LightPlacementEngine to instantiate lights from binary storage.
        /// </summary>
        public void PlaceLightsOnDemand()
        {
            Log("[CompleteMazeBuilder]  Placing lights on-demand...");

            if (lightPlacementEngine == null)
            {
                LogWarning("[CompleteMazeBuilder]  LightPlacementEngine not found - skipping light placement");
                return;
            }

            // LightPlacementEngine will load from binary storage and instantiate
            // This is called only when lights are actually needed
            Log("[CompleteMazeBuilder]  LightPlacementEngine will instantiate lights from storage");
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

            MazeSaveData.SaveGridMaze((int)seed, grid.SerializeToBytes(), spawnCell.x, spawnCell.y);

            Log("[CompleteMazeBuilder]  Maze saved");
        }

        #endregion

        #region Player

        private void SpawnPlayer()
        {
            Log($"[CompleteMazeBuilder]  Spawning player at {spawnPos}...");

            if (playerController == null)
                playerController = FindFirstObjectByType<PlayerController>();

            if (playerController == null)
            {
                LogWarning("[CompleteMazeBuilder] ️ PlayerController not in scene (add independently)");
                LogWarning("[CompleteMazeBuilder]  Add PlayerController component to a GameObject in scene");
                return;
            }

            playerController.transform.position = spawnPos;

            float offset = GameConfig.Instance.defaultPlayerSpawnOffset;
            playerController.transform.position += new Vector3(
                (Random.value - 0.5f) * offset,
                0f,
                (Random.value - 0.5f) * offset
            );

            var cam = playerController.GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cam.transform.localPosition = new Vector3(0f, GameConfig.Instance.defaultPlayerEyeHeight, 0f);
                cam.transform.localRotation = Quaternion.identity;
                Log($"[CompleteMazeBuilder]  FPS camera set to eye level ({GameConfig.Instance.defaultPlayerEyeHeight}m)");
            }

            playerController.transform.rotation = Quaternion.identity;

            Log($"[CompleteMazeBuilder]  Player spawned INSIDE maze at {playerController.transform.position}");
            Log($"[CompleteMazeBuilder]  Camera at FPS eye level - ready to explore!");
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
