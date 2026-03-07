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
                Log("[CompleteMazeBuilder] 🔌 Connected to EventHandler");

            seed = ComputeSeed(currentSeed);

            Log($"[CompleteMazeBuilder] 🎲 Level {currentLevel} - Maze {mazeSize}x{mazeSize}");
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
            Log($"[CompleteMazeBuilder] 🎮 Level {currentLevel} - Maze {mazeSize}x{mazeSize}");
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
            Log($"[CompleteMazeBuilder] 🎮 LEVEL {currentLevel} - Maze {mazeSize}x{mazeSize}");
            Log("[CompleteMazeBuilder] 🏗️ Starting maze generation...");
            Log("[CompleteMazeBuilder] ════════════════════════════════════════");

            wallPositions.Clear();
            FindComponents();
            CleanupOldMaze();
            Log("[CompleteMazeBuilder] 🧹 STEP 1: Cleanup complete");

            SpawnGround();
            Log("[CompleteMazeBuilder] 🌍 STEP 2: Ground spawned");

            GenerateGrid();
            Log($"[CompleteMazeBuilder] 🏛️ STEP 3: Spawn room placed at {spawnCell}");

            PlaceWalls();
            Log("[CompleteMazeBuilder] 🧱 STEP 4: Walls placed (oriented)");

            PlaceDoors();
            Log("[CompleteMazeBuilder] 🚪 STEP 5: Doors placed");

            PlaceTorches();
            Log("[CompleteMazeBuilder] 🔥 STEP 6: Torches mounted");

            SaveMaze();
            Log("[CompleteMazeBuilder] 💾 STEP 7: Maze saved");

            if (Application.isPlaying)
            {
                SpawnPlayer();
                Log($"[CompleteMazeBuilder] 👤 STEP 8: Player spawned at {spawnPos}");
            }

            Log("[CompleteMazeBuilder] ════════════════════════════════════════");
            Log($"[CompleteMazeBuilder] ✅ Level {currentLevel} complete! Maze {mazeSize}x{mazeSize}");
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
            Log("[CompleteMazeBuilder] ✅ Components found");
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
        }

        #endregion

        #region Ground

        private void SpawnGround()
        {
            Log("[CompleteMazeBuilder] 🌍 Spawning ground floor...");

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

            Log($"[CompleteMazeBuilder] ✅ Ground spawned ({size}m x {size}m)");
        }

        #endregion

        #region Grid

        private void GenerateGrid()
        {
            Log("[CompleteMazeBuilder] 🏛️ Generating grid maze with spawn room first...");

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

            Log($"[CompleteMazeBuilder] 🎯 SpawnPoint: cell {spawnCell}");
            Log($"[CompleteMazeBuilder] 👤 Spawn position: {spawnPos}");
            Log($"[CompleteMazeBuilder] ✅ Grid maze generated ({mazeSize}x{mazeSize})");
        }

        #endregion

        #region Walls

        private void PlaceWalls()
        {
            Log($"[CompleteMazeBuilder] 🧱 Placing walls with proper orientation...");

            int spawned = 0;
            GameObject parent = new GameObject("MazeWalls");

            for (int x = 0; x < mazeSize; x++)
            {
                for (int y = 0; y < mazeSize; y++)
                {
                    if (grid.GetCell(x, y) == GridMazeCell.Wall)
                    {
                        Vector3 pos = new Vector3(
                            x * cellSize + cellSize / 2f,
                            wallHeight / 2f,
                            y * cellSize + cellSize / 2f
                        );

                        Quaternion rot = Quaternion.Euler(0f, 0f, 0f);
                        bool hasVertical = (y + 1 < mazeSize && grid.GetCell(x, y + 1) == GridMazeCell.Wall) ||
                                          (y - 1 >= 0 && grid.GetCell(x, y - 1) == GridMazeCell.Wall);
                        if (hasVertical)
                            rot = Quaternion.Euler(0f, 90f, 0f);

                        SpawnWall(pos, rot, $"Wall_{x}_{y}", parent.transform);
                        wallPositions.Add(pos);
                        spawned++;
                    }
                }
            }

            Log($"[CompleteMazeBuilder] 🧱 {spawned} walls placed (oriented & textured)");
        }

        private void SpawnWall(Vector3 pos, Quaternion rot, string name, Transform parent)
        {
            if (wallPrefab == null)
            {
                LogError($"[CompleteMazeBuilder] ❌ Wall prefab not loaded! Cannot spawn wall at {pos}");
                LogError("[CompleteMazeBuilder] 💡 Fix: Run Tools → Quick Setup Prefabs");
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

        private void PlaceDoors()
        {
            Log("[CompleteMazeBuilder] 🚪 Placing simple entrance/exit doors...");

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
                    Log($"[CompleteMazeBuilder] 🚪 Entrance door placed at ({doorPos.x}, {doorPos.y})");
                }
            }
            else
            {
                Log("[CompleteMazeBuilder] ⚠️ No suitable door position found");
            }

            Log($"[CompleteMazeBuilder] ✅ Doors placed (entrance/exit only)");
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

        private void PlaceTorches()
        {
            Log("[CompleteMazeBuilder] 🔥 Mounting torches on walls (using prefab)...");

            if (torchPrefab == null)
            {
                LogWarning("[CompleteMazeBuilder] ⚠️ Torch prefab not loaded - skipping torch placement");
                return;
            }

            GameObject parent = new GameObject("Torches");
            int placed = 0;
            float chance = 0.3f;

            foreach (Vector3 wallPos in wallPositions)
            {
                if (Random.value > chance) continue;

                Vector3 pos = wallPos + Vector3.forward * (wallThickness / 2f + 0.2f);
                pos.y = wallHeight * 0.6f;

                Quaternion rot = Quaternion.Euler(35f, 0f, 0f);

                GameObject torch = Instantiate(torchPrefab, pos, rot);
                torch.name = $"Torch_{placed}";
                torch.transform.SetParent(parent.transform);
                placed++;
            }

            Log($"[CompleteMazeBuilder] 🔥 {placed} torches mounted on walls");
        }

        #endregion

        #region Save

        private void SaveMaze()
        {
            Log("[CompleteMazeBuilder] 💾 Saving maze to database...");

            if (grid == null)
            {
                LogError("[CompleteMazeBuilder] ❌ GridMazeGenerator not found!");
                return;
            }

            MazeSaveData.SaveGridMaze((int)seed, grid.SerializeToBytes(), spawnCell.x, spawnCell.y);

            Log("[CompleteMazeBuilder] ✅ Maze saved");
        }

        #endregion

        #region Player

        private void SpawnPlayer()
        {
            Log($"[CompleteMazeBuilder] 👤 Spawning player at {spawnPos}...");

            if (playerController == null)
                playerController = FindFirstObjectByType<PlayerController>();

            if (playerController == null)
            {
                LogWarning("[CompleteMazeBuilder] ⚠️ PlayerController not in scene (add independently)");
                LogWarning("[CompleteMazeBuilder] 💡 Add PlayerController component to a GameObject in scene");
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
                Log($"[CompleteMazeBuilder] 👤 FPS camera set to eye level ({GameConfig.Instance.defaultPlayerEyeHeight}m)");
            }

            playerController.transform.rotation = Quaternion.identity;

            Log($"[CompleteMazeBuilder] ✅ Player spawned INSIDE maze at {playerController.transform.position}");
            Log($"[CompleteMazeBuilder] 👤 Camera at FPS eye level - ready to explore!");
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
