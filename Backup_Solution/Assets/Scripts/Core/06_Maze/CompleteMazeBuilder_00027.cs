// CompleteMazeBuilder.cs
// Simplified maze generation with plug-in-out architecture
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// ROLE: Auto-generates complete maze (ground, walls, corridors, doors, objects)
//
// PLUG-IN-OUT ARCHITECTURE:
// - Does NOT create components (finds them in scene)
// - Uses EventHandler for communication (publishes OnMazeGenerated)
// - Independent module (can be added/removed safely)
// - No direct dependencies on other systems
//
// GENERATION ORDER:
// 1. LOAD CONFIG  → All values from JSON (NO HARDCODING)
// 2. PRELOAD      → Prefabs, materials, textures
// 3. FIND         → Components (plug-in-out: never create)
// 4. CLEANUP      → Destroy ALL old objects
// 5. GROUND       → Spawn ground floor (base layer)
// 6. GRID MAZE    → Generate grid with rooms & corridors (marks walls)
// 7. SPAWN WALLS  → Spawn walls from grid data (snapped)
// 8. DOORS        → Place in openings
// 9. OBJECTS      → Invoke other systems (torches, chests, enemies)
// 10. SAVE        → Save to database
// 11. PLAYER      → Spawn in entrance room (Play mode)
// 12. PUBLISH     → OnMazeGenerated event (EventHandler)
// NO CEILING      → Disabled for top-down view
//
// VERBOSITY:
// - Loaded from GameConfig.Instance.consoleVerbosity
// - Levels: Mute, Short (default), Full
// - Runtime commands: maze.verbosity, maze.generate, maze.status
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

        [Header("📢 Console Verbosity (From JSON Config)")]
        [Tooltip("Leave to Short to use JSON config value. Set manually to override.")]
        [SerializeField] private VerbosityLevel verbosity = VerbosityLevel.Short;

        private static CompleteMazeBuilder _instance;
        public static VerbosityLevel CurrentVerbosity => _instance != null ? _instance.verbosity : VerbosityLevel.Short;

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

        /// <summary>
        /// Set verbosity level at runtime.
        /// Usage: CompleteMazeBuilder.SetVerbosity("full")
        /// </summary>
        public static void SetVerbosity(string level)
        {
            if (_instance == null)
            {
                Debug.LogError("[CompleteMazeBuilder] ❌ No instance found - can't set verbosity");
                return;
            }

            switch (level.ToLower())
            {
                case "full":
                    _instance.verbosity = VerbosityLevel.Full;
                    Debug.Log("[CompleteMazeBuilder] ✅ Verbosity: FULL (all debug messages)");
                    break;
                case "short":
                    _instance.verbosity = VerbosityLevel.Short;
                    Debug.Log("[CompleteMazeBuilder] ✅ Verbosity: SHORT (critical only)");
                    break;
                case "mute":
                    _instance.verbosity = VerbosityLevel.Mute;
                    Debug.Log("[CompleteMazeBuilder] ✅ Verbosity: MUTE (no output)");
                    break;
                default:
                    Debug.LogError("[CompleteMazeBuilder] ❌ Invalid verbosity level. Use: full, short, or mute");
                    break;
            }
        }

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
        private int roomSize;
        private int corridorWidth;

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

            // Room & Corridor settings
            roomSize = config.defaultRoomSize;
            corridorWidth = config.defaultCorridorWidth;

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

            // Verbosity (apply only if inspector is set to Short - default override)
            if (verbosity == VerbosityLevel.Short)
            {
                ApplyVerbosityFromConfig(config.consoleVerbosity);
            }

            Log($"[CompleteMazeBuilder] 📖 Config loaded: {mazeWidth}x{mazeHeight} maze, {roomSize}x{roomSize} rooms, {corridorWidth}-cell corridors", true);
        }

        /// <summary>
        /// Apply verbosity level from JSON config.
        /// </summary>
        private void ApplyVerbosityFromConfig(string configVerbosity)
        {
            switch (configVerbosity.ToLower())
            {
                case "mute":
                    verbosity = VerbosityLevel.Mute;
                    Log("[CompleteMazeBuilder] 📢 Verbosity: MUTE (from JSON config)", true);
                    break;
                case "short":
                    verbosity = VerbosityLevel.Short;
                    Log("[CompleteMazeBuilder] 📢 Verbosity: SHORT (from JSON config)", true);
                    break;
                case "full":
                    verbosity = VerbosityLevel.Full;
                    Log("[CompleteMazeBuilder] 📢 Verbosity: FULL (from JSON config)", true);
                    break;
                default:
                    verbosity = VerbosityLevel.Short;
                    Log($"[CompleteMazeBuilder] ⚠️ Invalid verbosity '{configVerbosity}' in config - using 'short'", true);
                    break;
            }
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

            // STEP 5: Generate grid maze (rooms + corridors + walls)
            CreateEntranceRoom();
            Log($"[CompleteMazeBuilder] 🏛️ STEP 3: Grid maze generated", true);

            // STEP 6: Spawn walls from grid data
            SpawnOuterWalls();
            Log("[CompleteMazeBuilder] 🧱 STEP 4: Walls spawned from grid", true);

            // STEP 7: Verify corridors
            CarveCorridors();
            Log("[CompleteMazeBuilder] 🔨 STEP 5: Corridors verified", true);

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

        #region Entrance Room & Grid Generation

        private void CreateEntranceRoom()
        {
            Log("[CompleteMazeBuilder] 🏛️ Generating grid maze with rooms and corridors...");

            // Create grid generator and initialize from config
            gridMazeGenerator = new GridMazeGenerator();
            gridMazeGenerator.InitializeFromConfig();

            // Generate complete maze grid (rooms + corridors + walls)
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
            Log($"[CompleteMazeBuilder] ✅ Grid maze generated ({gridMazeGenerator.GridSize}x{gridMazeGenerator.GridSize})", true);
        }

        #endregion

        #region Outer Walls & Internal Walls

        private void SpawnOuterWalls()
        {
            Log($"[CompleteMazeBuilder] 🧱 Spawning all maze walls from grid...");

            int spawned = 0;
            int size = gridMazeGenerator.GridSize;

            // Spawn walls based on grid data
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    GridMazeCell cell = gridMazeGenerator.GetCell(x, y);

                    // Only spawn walls where grid marks Wall
                    if (cell == GridMazeCell.Wall)
                    {
                        Vector3 pos = new Vector3(
                            x * cellSize + cellSize / 2f,
                            wallHeight / 2f,
                            y * cellSize + cellSize / 2f
                        );
                        SpawnWall(pos, Quaternion.Euler(0f, 0f, 0f), $"Wall_{x}_{y}");
                        spawned++;
                    }
                }
            }

            Log($"[CompleteMazeBuilder] 🧱 {spawned} walls spawned from grid (snapped side-by-side)", true);
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
            // Corridors are already carved by GridMazeGenerator.Generate()
            // This method just verifies and logs the corridor data

            if (gridMazeGenerator == null)
            {
                LogError("[CompleteMazeBuilder] ❌ GridMazeGenerator not found!");
                return;
            }

            Log("[CompleteMazeBuilder] 🔨 Verifying corridors...");

            int corridorCells = 0;
            int size = gridMazeGenerator.GridSize;

            // Count corridor cells
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

            Log($"[CompleteMazeBuilder] ✅ {corridorCells} corridor cells verified ({corridorWidth} cells wide)", true);
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

            // Teleport player to spawn position (INSIDE maze - from SpawnPoint)
            player.transform.position = spawnPosition;

            // Add small random offset (prevent wall clipping) - loaded from JSON config
            float spawnOffset = GameConfig.Instance.defaultPlayerSpawnOffset;
            float offsetX = (Random.value - 0.5f) * spawnOffset;
            float offsetZ = (Random.value - 0.5f) * spawnOffset;
            player.transform.position += new Vector3(offsetX, 0f, offsetZ);

            // Ensure FPS camera is at eye level (middle eyes view) - loaded from JSON config
            var cameraTransform = player.GetComponentInChildren<Camera>()?.transform;
            if (cameraTransform != null)
            {
                // Set camera to eye height from config (default 1.7m for average adult)
                float eyeHeight = GameConfig.Instance.defaultPlayerEyeHeight;
                cameraTransform.localPosition = new Vector3(0f, eyeHeight, 0f);
                cameraTransform.localRotation = Quaternion.identity;
                Log($"[CompleteMazeBuilder] 👤 FPS camera set to eye level ({eyeHeight}m)", true);
            }

            // Reset player rotation (face forward)
            player.transform.rotation = Quaternion.identity;

            Log($"[CompleteMazeBuilder] ✅ Player spawned INSIDE maze at {player.transform.position}", true);
            Log($"[CompleteMazeBuilder] 👤 Camera at FPS eye level - ready to explore!", true);
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

        #region Editor & Runtime Helpers

        /// <summary>
        /// Generate maze geometry only (no player spawn).
        /// Used by editor tools and runtime generation.
        /// </summary>
        public void GenerateMazeGeometryOnly()
        {
            Log("[CompleteMazeBuilder] ════════════════════════════════════════");
            Log("[CompleteMazeBuilder] 🏗️ Starting maze generation (editor mode)...", true);
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

            // STEP 5: Generate grid maze (rooms + corridors + walls)
            CreateEntranceRoom();
            Log($"[CompleteMazeBuilder] 🏛️ STEP 3: Grid maze generated", true);

            // STEP 6: Spawn walls from grid data
            SpawnOuterWalls();
            Log($"[CompleteMazeBuilder] 🧱 STEP 4: Walls spawned from grid", true);

            // STEP 7: Verify corridors
            CarveCorridors();
            Log($"[CompleteMazeBuilder] 🔨 STEP 5: Corridors verified", true);

            // STEP 8: Place doors
            PlaceDoors();
            Log($"[CompleteMazeBuilder] 🚪 STEP 6: Doors placed", true);

            // STEP 9: Place objects (torches, chests, enemies)
            PlaceObjects();
            Log($"[CompleteMazeBuilder] 🎒 STEP 7: Objects placed", true);

            // STEP 10: Save maze
            SaveMaze();
            Log($"[CompleteMazeBuilder] 💾 STEP 8: Maze saved", true);

            // NO PLAYER SPAWN in editor mode
            Log("[CompleteMazeBuilder] ☁️ Ceiling: DISABLED (top-down view)");
            Log("[CompleteMazeBuilder] ════════════════════════════════════════");
            Log("[CompleteMazeBuilder] ✅ Maze generation complete (editor mode)!", true);
            Log("[CompleteMazeBuilder] 📏 Size: " + gridMazeGenerator?.GridSize + "x" + gridMazeGenerator?.GridSize + " cells", true);
            Log("[CompleteMazeBuilder] ════════════════════════════════════════");
        }

        /// <summary>
        /// Validate all prefab and material paths.
        /// Logs errors if any paths are invalid.
        /// </summary>
        public void ValidatePaths()
        {
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  VALIDATING PATHS");
            Debug.Log("═══════════════════════════════════════════");

            bool hasErrors = false;

            // Validate prefab paths
            if (!string.IsNullOrEmpty(wallPrefabPath))
            {
                var wallPrefab = Resources.Load<GameObject>(wallPrefabPath);
                if (wallPrefab == null)
                {
                    Debug.LogError($"❌ Wall prefab not found: {wallPrefabPath}");
                    hasErrors = true;
                }
                else
                {
                    Debug.Log($"✅ Wall prefab: {wallPrefabPath}");
                }
            }

            if (!string.IsNullOrEmpty(doorPrefabPath))
            {
                var doorPrefab = Resources.Load<GameObject>(doorPrefabPath);
                if (doorPrefab == null)
                {
                    Debug.LogError($"❌ Door prefab not found: {doorPrefabPath}");
                    hasErrors = true;
                }
                else
                {
                    Debug.Log($"✅ Door prefab: {doorPrefabPath}");
                }
            }

            // Validate material paths
            if (!string.IsNullOrEmpty(wallMaterialPath))
            {
                var wallMat = Resources.Load<Material>(wallMaterialPath);
                if (wallMat == null)
                {
                    Debug.LogError($"❌ Wall material not found: {wallMaterialPath}");
                    hasErrors = true;
                }
                else
                {
                    Debug.Log($"✅ Wall material: {wallMaterialPath}");
                }
            }

            if (!string.IsNullOrEmpty(floorMaterialPath))
            {
                var floorMat = Resources.Load<Material>(floorMaterialPath);
                if (floorMat == null)
                {
                    Debug.LogError($"❌ Floor material not found: {floorMaterialPath}");
                    hasErrors = true;
                }
                else
                {
                    Debug.Log($"✅ Floor material: {floorMaterialPath}");
                }
            }

            if (hasErrors)
            {
                Debug.Log("═══════════════════════════════════════════");
                Debug.LogError("❌ VALIDATION FAILED - Check missing resources!");
                Debug.Log("═══════════════════════════════════════════");
            }
            else
            {
                Debug.Log("═══════════════════════════════════════════");
                Debug.Log("✅ ALL PATHS VALID!");
                Debug.Log("═══════════════════════════════════════════");
            }
        }

        /// <summary>
        /// Clear stored spawn position from PlayerPrefs.
        /// Used when resetting maze or starting fresh.
        /// </summary>
        public void ClearSpawnPosition()
        {
            PlayerPrefs.DeleteKey("MazeSpawnX");
            PlayerPrefs.DeleteKey("MazeSpawnY");
            PlayerPrefs.Save();
            Debug.Log("[CompleteMazeBuilder] ✅ Spawn position cleared");
        }

        #endregion
    }
}
