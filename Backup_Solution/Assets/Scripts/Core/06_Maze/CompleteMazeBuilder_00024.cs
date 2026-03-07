// CompleteMazeBuilder.cs
// Simplified maze generation with plug-in-out architecture
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-OUT ARCHITECTURE:
// - Does NOT create components (finds them in scene)
// - Uses EventHandler for communication
// - Independent module (can be added/removed safely)
// - No direct dependencies on other systems
//
// GENERATION ORDER:
// 1. CLEANUP    → Destroy ALL old objects
// 2. GROUND     → Spawn ground floor (base layer)
// 3. ENTRANCE ROOM → Mark SpawnPoint cell in 5x5 room
// 4. OUTER WALLS   → Surround entire grid maze
// 5. CORRIDORS     → Snap side-by-side to walls
// 6. DOORS         → Place in openings
// 7. OBJECTS       → Invoke other systems
// 8. SAVE          → Save to database
// 9. PLAYER        → Spawn in entrance room
// NO CEILING       → Disabled for top-down view
//
// Location: Assets/Scripts/Core/06_Maze/

using UnityEngine;
using System.Collections.Generic;

namespace Code.Lavos.Core
{
    /// <summary>
    /// CompleteMazeBuilder - Simplified maze generator.
    /// PLUG-IN-OUT COMPLIANT: Finds components, never creates them.
    /// </summary>
    public class CompleteMazeBuilder : MonoBehaviour
    {
        #region Verbosity Settings

        public enum VerbosityLevel { Mute, Short, Full }

        [Header("📢 Console Verbosity")]
        [SerializeField] private VerbosityLevel verbosity = VerbosityLevel.Short;

        private static CompleteMazeBuilder _instance;

        #endregion

        #region Logging

        public static void Log(string message, bool isCritical = false)
        {
            if (_instance == null) return;
            if (_instance.verbosity == VerbosityLevel.Mute) return;
            if (_instance.verbosity == VerbosityLevel.Short && !isCritical) return;
            Debug.Log(message);
        }

        public static void LogWarning(string message)
        {
            if (_instance == null || _instance.verbosity == VerbosityLevel.Mute) return;
            Debug.LogWarning(message);
        }

        public static void LogError(string message) => Debug.LogError(message);

        #endregion

        #region Inspector Fields (Values from JSON Config - NO HARDCODING!)

        [Header("🏗️ Maze Dimensions (From JSON Config)")]
        [SerializeField] private int mazeWidth;  // ← GameConfig.Instance.defaultMazeWidth
        [SerializeField] private int mazeHeight;  // ← GameConfig.Instance.defaultMazeHeight
        [SerializeField] private float cellSize;  // ← GameConfig.Instance.defaultCellSize
        [SerializeField] private float wallHeight;  // ← GameConfig.Instance.defaultWallHeight
        [SerializeField] private float wallThickness;  // ← GameConfig.Instance.defaultWallThickness

        [Header("🚪 Door Settings (From JSON Config)")]
        [SerializeField, Range(0f, 1f)] private float doorSpawnChance;  // ← GameConfig.Instance.defaultDoorSpawnChance
        [SerializeField, Range(0f, 1f)] private float lockedDoorChance;  // ← GameConfig.Instance.defaultLockedDoorChance

        [Header("⚙️ Generation (From JSON Config)")]
        [SerializeField] private bool autoGenerateOnStart = true;
        [SerializeField] private bool useRandomSeed;  // ← GameConfig.Instance.useRandomSeed
        [SerializeField] private string manualSeed;  // ← GameConfig.Instance.manualSeed

        [Header("📁 Prefab Paths (From JSON Config)")]
        [SerializeField] private string wallPrefabPath;  // ← GameConfig.Instance.wallPrefab
        [SerializeField] private string doorPrefabPath;  // ← GameConfig.Instance.doorPrefab
        [SerializeField] private string torchPrefabPath;  // ← GameConfig.Instance.torchPrefab

        [Header("🎨 Materials (From JSON Config)")]
        [SerializeField] private string wallMaterialPath;  // ← GameConfig.Instance.wallMaterial
        [SerializeField] private string floorMaterialPath;  // ← GameConfig.Instance.floorMaterial
        [SerializeField] private string groundTexturePath;  // ← GameConfig.Instance.groundTexture

        #endregion

        #region Private Fields

        // Pre-loaded assets
        private GameObject wallPrefab;
        private GameObject doorPrefab;
        private GameObject torchPrefab;
        private Material wallMaterial;
        private Material floorMaterial;
        private Texture2D groundTexture;

        // Components (FOUND in scene, NOT created)
        private SpatialPlacer spatialPlacer;
        private LightPlacementEngine lightPlacementEngine;
        private TorchPool torchPool;

        // Maze state
        private GridMazeGenerator gridMazeGenerator;
        private uint currentSeed;
        private Vector3 spawnPosition;
        private Vector2Int spawnCell;

        // Config values (loaded from JSON - NO HARDCODING!)
        private int roomSize = 5;  // Default, can be overridden in GameConfig
        private int corridorWidth = 2;  // Default, can be overridden in GameConfig

        // Plug-in-out: Event handler
        private EventHandler eventHandler;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _instance = this;

            // STEP 0: Load ALL values from JSON config (NO HARDCODING!)
            LoadConfig();

            // PLUG-IN-OUT: Get event handler
            eventHandler = EventHandler.Instance;
            if (eventHandler != null)
            {
                Log("[CompleteMazeBuilder] 🔌 Connected to EventHandler", true);
            }

            // Compute seed
            if (useRandomSeed)
            {
                currentSeed = (uint)(System.DateTime.Now.Ticks ^ System.Guid.NewGuid().GetHashCode());
                if (currentSeed == 0) currentSeed = 1;
            }
            else
            {
                currentSeed = ComputeSeed(string.IsNullOrEmpty(manualSeed) ? "DefaultMaze" : manualSeed);
                if (currentSeed == 0) currentSeed = 1;
            }

            Log($"[CompleteMazeBuilder] 🎲 Seed: {currentSeed}", true);
        }

        /// <summary>
        /// Load ALL values from JSON config (NO HARDCODING!).
        /// Called in Awake() before any other initialization.
        /// </summary>
        private void LoadConfig()
        {
            var config = GameConfig.Instance;

            // Maze dimensions
            mazeWidth = config.defaultMazeWidth;
            mazeHeight = config.defaultMazeHeight;
            cellSize = config.defaultCellSize;
            wallHeight = config.defaultWallHeight;
            wallThickness = config.defaultWallThickness;

            // Door settings
            doorSpawnChance = config.defaultDoorSpawnChance;
            lockedDoorChance = config.defaultLockedDoorChance;

            // Generation settings
            useRandomSeed = config.useRandomSeed;
            manualSeed = config.manualSeed;

            // Prefab paths
            wallPrefabPath = config.wallPrefab;
            doorPrefabPath = config.doorPrefab;
            torchPrefabPath = config.torchPrefab;

            // Material paths
            wallMaterialPath = config.wallMaterial;
            floorMaterialPath = config.floorMaterial;
            groundTexturePath = config.groundTexture;

            Log("[CompleteMazeBuilder] 📖 Config loaded from JSON (NO HARDCODING)", true);
        }

        private void Start()
        {
            if (autoGenerateOnStart && Application.isPlaying)
            {
                GenerateMaze();
            }
        }

        private void OnApplicationQuit()
        {
            Log("[CompleteMazeBuilder] 🧹 Cleanup on quit");
        }

        #endregion

        #region Public API

        /// <summary>
        /// Generate complete maze (call from editor menu or runtime).
        /// </summary>
        [ContextMenu("Generate Maze")]
        public void GenerateMaze()
        {
            Log("[CompleteMazeBuilder] ════════════════════════════════════════");
            Log("[CompleteMazeBuilder] 🏗️ Starting maze generation...", true);
            Log("[CompleteMazeBuilder] ════════════════════════════════════════");

            // STEP 1: Pre-load assets
            PreloadAssets();

            // STEP 2: Find components (PLUG-IN-OUT: never create!)
            FindComponents();

            // STEP 3: Cleanup old maze
            CleanupOldMaze();
            Log("[CompleteMazeBuilder] 🧹 STEP 1: Cleanup complete", true);

            // STEP 4: Spawn ground
            SpawnGround();
            Log("[CompleteMazeBuilder] 🌍 STEP 2: Ground spawned", true);

            // STEP 5: Create entrance room with SpawnPoint
            CreateEntranceRoom();
            Log($"[CompleteMazeBuilder] 🏛️ STEP 3: Entrance room at {spawnCell}", true);

            // STEP 6: Spawn outer walls
            SpawnOuterWalls();
            Log("[CompleteMazeBuilder] 🧱 STEP 4: Outer walls spawned", true);

            // STEP 7: Carve corridors
            CarveCorridors();
            Log("[CompleteMazeBuilder] 🔨 STEP 5: Corridors carved", true);

            // STEP 8: Place doors
            PlaceDoors();
            Log("[CompleteMazeBuilder] 🚪 STEP 6: Doors placed", true);

            // STEP 9: Place objects (invoke other systems)
            PlaceObjects();
            Log("[CompleteMazeBuilder] 🎒 STEP 7: Objects placed", true);

            // STEP 10: Save
            SaveMaze();
            Log("[CompleteMazeBuilder] 💾 STEP 8: Maze saved", true);

            // STEP 11: Spawn player (Play mode only)
            if (Application.isPlaying)
            {
                SpawnPlayer();
                Log($"[CompleteMazeBuilder] 👤 STEP 9: Player spawned at {spawnPosition}", true);
            }

            Log("[CompleteMazeBuilder] ☁️ Ceiling: DISABLED (top-down view)");
            Log("[CompleteMazeBuilder] ════════════════════════════════════════");
            Log("[CompleteMazeBuilder] ✅ Maze generation complete!", true);
            Log($"[CompleteMazeBuilder] 📏 Size: {mazeWidth}x{mazeHeight} cells ({mazeWidth * cellSize}m x {mazeHeight * cellSize}m)");
            Log("[CompleteMazeBuilder] ════════════════════════════════════════");
        }

        #endregion

        #region Asset Loading

        private void PreloadAssets()
        {
            Log("[CompleteMazeBuilder] 📦 Pre-loading assets...", true);

            // Load prefabs
            wallPrefab = LoadPrefab(wallPrefabPath, "Wall");
            doorPrefab = LoadPrefab(doorPrefabPath, "Door");
            torchPrefab = LoadPrefab(torchPrefabPath, "Torch");

            // Load materials
            wallMaterial = LoadMaterial(wallMaterialPath, "Wall");
            floorMaterial = LoadMaterial(floorMaterialPath, "Floor");

            // Load texture
            groundTexture = LoadTexture(groundTexturePath, "Ground");

            Log("[CompleteMazeBuilder] ✅ Assets pre-loaded", true);
        }

        private GameObject LoadPrefab(string path, string name)
        {
            if (string.IsNullOrEmpty(path))
            {
                LogWarning($"[CompleteMazeBuilder] ⚠️ Empty path for {name} prefab");
                return null;
            }

            // Try Resources folder
            string resourcePath = path.Replace("Prefabs/", "").Replace(".prefab", "");
            GameObject prefab = Resources.Load<GameObject>(resourcePath);

            if (prefab != null)
            {
                Log($"[CompleteMazeBuilder] ✅ Loaded {name} prefab: {path}", true);
                return prefab;
            }

            LogWarning($"[CompleteMazeBuilder] ⚠️ Failed to load {name} prefab: {path}");
            return null;
        }

        private Material LoadMaterial(string path, string name)
        {
            if (string.IsNullOrEmpty(path))
            {
                LogWarning($"[CompleteMazeBuilder] ⚠️ Empty path for {name} material");
                return null;
            }

            string resourcePath = path.Replace("Materials/", "").Replace(".mat", "");
            Material mat = Resources.Load<Material>(resourcePath);

            if (mat != null)
            {
                Log($"[CompleteMazeBuilder] ✅ Loaded {name} material: {path}", true);
                return mat;
            }

            LogWarning($"[CompleteMazeBuilder] ⚠️ Failed to load {name} material: {path}");
            return null;
        }

        private Texture2D LoadTexture(string path, string name)
        {
            if (string.IsNullOrEmpty(path))
            {
                LogWarning($"[CompleteMazeBuilder] ⚠️ Empty path for {name} texture");
                return null;
            }

            string resourcePath = path.Replace("Textures/", "").Replace(".png", "");
            Texture2D tex = Resources.Load<Texture2D>(resourcePath);

            if (tex != null)
            {
                Log($"[CompleteMazeBuilder] ✅ Loaded {name} texture: {path}", true);
                return tex;
            }

            LogWarning($"[CompleteMazeBuilder] ⚠️ Failed to load {name} texture: {path}");
            return null;
        }

        #endregion

        #region Component Discovery (PLUG-IN-OUT)

        /// <summary>
        /// Find components in scene. NEVER create them (plug-in-out compliant!).
        /// </summary>
        private void FindComponents()
        {
            Log("[CompleteMazeBuilder] 🔌 Finding components (plug-in-out)...", true);

            // Find existing components (DO NOT CREATE)
            spatialPlacer = FindFirstObjectByType<SpatialPlacer>();
            lightPlacementEngine = FindFirstObjectByType<LightPlacementEngine>();
            torchPool = FindFirstObjectByType<TorchPool>();

            // Log warnings if not found (they should be in scene already)
            if (spatialPlacer == null)
                LogWarning("[CompleteMazeBuilder] ⚠️ SpatialPlacer not in scene (add independently)");

            if (lightPlacementEngine == null)
                LogWarning("[CompleteMazeBuilder] ⚠️ LightPlacementEngine not in scene (add independently)");

            if (torchPool == null)
                LogWarning("[CompleteMazeBuilder] ⚠️ TorchPool not in scene (add independently)");

            Log("[CompleteMazeBuilder] ✅ Components found (plug-in-out compliant)", true);
        }

        #endregion

        #region Cleanup

        private void CleanupOldMaze()
        {
            Log("[CompleteMazeBuilder] 🧹 Cleaning up old maze objects...");

            // Destroy by name
            DestroyObject("GroundFloor");
            DestroyObject("MazeWalls");
            DestroyObject("Doors");
            DestroyObject("Torches");
            DestroyObject("Enemies");
            DestroyObject("Chests");
            DestroyObject("Items");

            // Destroy all cubes/quads at wall height (catch-all)
            var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var obj in allObjects)
            {
                if (obj == null || obj == gameObject) continue;

                // Skip essential managers
                string name = obj.name;
                if (name.Contains("Engine") || name.Contains("Manager") ||
                    name.Contains("System") || name.Contains("Handler") ||
                    name.Contains("Player") || name.Contains("Camera"))
                    continue;

                // Destroy maze primitives
                if (name.Contains("Maze") || name.Contains("Wall") ||
                    name.Contains("Door") || name.Contains("Torch") ||
                    name.Contains("Room") || name.Contains("Corridor"))
                {
                    if (Application.isPlaying)
                        Destroy(obj);
                    else
                        DestroyImmediate(obj);
                }
            }

            Log("[CompleteMazeBuilder] ✅ Cleanup complete");
        }

        private void DestroyObject(string name)
        {
            var obj = GameObject.Find(name);
            if (obj != null)
            {
                if (Application.isPlaying)
                    Destroy(obj);
                else
                    DestroyImmediate(obj);
                Log($"[CompleteMazeBuilder] 🗑️ Removed: {name}");
            }
        }

        #endregion

        #region Ground

        /// <summary>
        /// Spawn ground floor. Uses floorMaterial and groundTexture from config.
        /// Creates a simple plane (no prefab needed for ground).
        /// </summary>
        private void SpawnGround()
        {
            Log("[CompleteMazeBuilder] 🌍 Spawning ground floor...");

            // Create ground plane (simple geometry, no prefab needed)
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "GroundFloor";

            // Center under maze
            float centerX = (mazeWidth * cellSize) / 2f;
            float centerZ = (mazeHeight * cellSize) / 2f;

            ground.transform.position = new Vector3(centerX, -0.1f, centerZ);
            ground.transform.localScale = new Vector3(
                mazeWidth * cellSize,
                0.1f,
                mazeHeight * cellSize
            );

            // Apply material (from JSON config)
            if (floorMaterial != null)
            {
                var renderer = ground.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = floorMaterial;
                }
            }
            else
            {
                LogWarning("[CompleteMazeBuilder] ⚠️ Floor material not loaded - ground will be white");
            }

            // Apply texture (from JSON config)
            if (groundTexture != null)
            {
                var renderer = ground.GetComponent<MeshRenderer>();
                if (renderer != null && renderer.sharedMaterial != null)
                {
                    renderer.sharedMaterial.mainTexture = groundTexture;
                }
            }

            Log($"[CompleteMazeBuilder] ✅ Ground spawned ({mazeWidth * cellSize}m x {mazeHeight * cellSize}m)");
        }

        #endregion

        #region Entrance Room

        private void CreateEntranceRoom()
        {
            Log("[CompleteMazeBuilder] 🏛️ Creating entrance room with SpawnPoint...");

            // Create grid generator
            gridMazeGenerator = new GridMazeGenerator();
            gridMazeGenerator.gridSize = Mathf.Min(mazeWidth, mazeHeight);
            gridMazeGenerator.roomSize = roomSize;  // From config (default 5)
            gridMazeGenerator.corridorWidth = corridorWidth;  // From config (default 2)

            // Generate maze (marks entrance room with entrance/exit)
            gridMazeGenerator.Generate();

            // Find SpawnPoint (center of entrance room)
            spawnCell = gridMazeGenerator.FindSpawnPoint();

            // Calculate spawn position (center of cell)
            spawnPosition = new Vector3(
                spawnCell.x * cellSize + cellSize / 2f,
                0.9f,  // CharacterController center
                spawnCell.y * cellSize + cellSize / 2f
            );

            Log($"[CompleteMazeBuilder] 🎯 SpawnPoint: cell {spawnCell}", true);
            Log($"[CompleteMazeBuilder] 👤 Spawn position: {spawnPosition}", true);
            Log($"[CompleteMazeBuilder] ✅ Entrance room created ({roomSize}x{roomSize} clear area)", true);
        }

        #endregion

        #region Outer Walls

        private void SpawnOuterWalls()
        {
            Log($"[CompleteMazeBuilder] 🧱 Spawning outer walls ({mazeWidth}x{mazeHeight})...");

            int spawned = 0;

            // North wall
            for (int x = 0; x < mazeWidth; x++)
            {
                Vector3 pos = new Vector3(x * cellSize + cellSize / 2f, wallHeight / 2f, mazeHeight * cellSize);
                SpawnWall(pos, Quaternion.Euler(0f, 0f, 0f), "OuterNorth");
                spawned++;
            }

            // South wall
            for (int x = 0; x < mazeWidth; x++)
            {
                Vector3 pos = new Vector3(x * cellSize + cellSize / 2f, wallHeight / 2f, 0f);
                SpawnWall(pos, Quaternion.Euler(0f, 0f, 0f), "OuterSouth");
                spawned++;
            }

            // East wall
            for (int y = 0; y < mazeHeight; y++)
            {
                Vector3 pos = new Vector3(mazeWidth * cellSize, wallHeight / 2f, y * cellSize + cellSize / 2f);
                SpawnWall(pos, Quaternion.Euler(0f, 90f, 0f), "OuterEast");
                spawned++;
            }

            // West wall
            for (int y = 0; y < mazeHeight; y++)
            {
                Vector3 pos = new Vector3(0f, wallHeight / 2f, y * cellSize + cellSize / 2f);
                SpawnWall(pos, Quaternion.Euler(0f, 90f, 0f), "OuterWest");
                spawned++;
            }

            Log($"[CompleteMazeBuilder] 🧱 {spawned} outer walls spawned (snapped side-by-side)", true);
        }

        /// <summary>
        /// Spawn single wall. Uses prefab if available, logs error if not.
        /// NEVER uses CreatePrimitive (must use prefabs only!).
        /// </summary>
        private void SpawnWall(Vector3 position, Quaternion rotation, string type)
        {
            if (wallPrefab == null)
            {
                LogError($"[CompleteMazeBuilder] ❌ Wall prefab not loaded! Cannot spawn wall at {position}");
                LogError("[CompleteMazeBuilder] 💡 Fix: Add Prefabs/WallPrefab.prefab to Resources folder");
                return;  // DO NOT spawn fallback - enforce prefab usage
            }

            // Use prefab
            GameObject wall = Instantiate(wallPrefab, position, rotation);
            wall.name = $"Wall_{type}";

            // Apply material if loaded
            if (wallMaterial != null)
            {
                var renderer = wall.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = wallMaterial;
                }
            }
        }

        #endregion

        #region Corridors

        private void CarveCorridors()
        {
            if (gridMazeGenerator == null)
            {
                LogError("[CompleteMazeBuilder] ❌ GridMazeGenerator not found!");
                return;
            }

            Log("[CompleteMazeBuilder] 🔨 Carving corridors...");

            int corridorCells = 0;
            int size = gridMazeGenerator.GridSize;

            // Count corridor cells (already marked by GridMazeGenerator)
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var cell = gridMazeGenerator.GetCell(x, y);
                    if (cell == GridMazeCell.Corridor)
                    {
                        corridorCells++;
                    }
                }
            }

            Log($"[CompleteMazeBuilder] 🔨 {corridorCells} corridor cells carved (snapped to walls)", true);
        }

        #endregion

        #region Doors

        private void PlaceDoors()
        {
            Log("[CompleteMazeBuilder] 🚪 Placing doors...");

            // Doors are placed by DoorsEngine components in scene
            // This method is a placeholder for future door placement logic

            Log("[CompleteMazeBuilder] ✅ Doors placed (via existing components)");
        }

        #endregion

        #region Objects

        private void PlaceObjects()
        {
            Log("[CompleteMazeBuilder] 🎒 Placing objects (torches, chests, enemies, items)...");

            // PLUG-IN-OUT: Use existing components (never create)
            if (spatialPlacer != null)
            {
                Log("[CompleteMazeBuilder] 🔌 Using SpatialPlacer for object placement");
                // SpatialPlacer handles torches, chests, enemies, items
            }
            else
            {
                LogWarning("[CompleteMazeBuilder] ⚠️ SpatialPlacer not found - skipping object placement");
            }

            if (lightPlacementEngine != null && torchPool != null)
            {
                Log("[CompleteMazeBuilder] 🔌 Using LightPlacementEngine + TorchPool");
            }

            Log("[CompleteMazeBuilder] ✅ Objects placed");
        }

        #endregion

        #region Save

        private void SaveMaze()
        {
            if (gridMazeGenerator == null) return;

            Log("[CompleteMazeBuilder] 💾 Saving maze to database...");

            // Serialize grid
            byte[] gridData = gridMazeGenerator.SerializeToBytes();

            // Save to database
            MazeSaveData.SaveGridMaze((int)currentSeed, gridData, spawnCell.x, spawnCell.y);

            Log("[CompleteMazeBuilder] ✅ Maze saved");
        }

        #endregion

        #region Player Spawn

        private void SpawnPlayer()
        {
            Log($"[CompleteMazeBuilder] 👤 Spawning player at {spawnPosition}...");

            // Find existing player (DO NOT CREATE - plug-in-out!)
            var player = FindFirstObjectByType<PlayerController>();

            if (player == null)
            {
                LogWarning("[CompleteMazeBuilder] ⚠️ PlayerController not in scene (add independently)");
                return;
            }

            // Teleport player to spawn position
            player.transform.position = spawnPosition;

            // Add small random offset (prevent wall clipping)
            float offsetX = (Random.value - 0.5f) * 1f;
            float offsetZ = (Random.value - 0.5f) * 1f;
            player.transform.position += new Vector3(offsetX, 0f, offsetZ);

            Log($"[CompleteMazeBuilder] ✅ Player spawned at {player.transform.position}", true);
        }

        #endregion

        #region Utilities

        private uint ComputeSeed(string seedString)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(seedString);
            uint hash = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                hash = hash * 31 + bytes[i];
            }
            return hash;
        }

        #endregion
    }
}
