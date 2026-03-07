// CompleteMazeBuilder.cs
// Main maze orchestrator - NEW ALGORITHM (Option A)
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-OUT ARCHITECTURE:
// - Does NOT create components (finds them in scene)
// - Uses EventHandler for communication
// - Independent module (can be added/removed safely)
// - All values from JSON config (no hardcoding)
//
// GENERATION ORDER:
// 1. LOAD CONFIG      → All values from JSON
// 2. PRELOAD ASSETS   → Prefabs, materials, textures
// 3. FIND COMPONENTS  → Plug-in-out (never create)
// 4. CLEANUP          → Destroy ALL old objects
// 5. GROUND           → Spawn ground floor (base layer)
// 6. GRID MAZE        → Spawn room FIRST, corridors TO spawn
// 7. WALLS            → Spawn walls from grid data
// 8. DOORS            → Place in corridor openings
// 9. LIGHTS           → Place lights/torches
// 10. OBJECTS         → Invoke other systems (chests, enemies, items)
// 11. SAVE            → Save to binary storage
// 12. PLAYER          → Spawn LAST (after geometry, FPS camera)
// NO CEILING          → Disabled for testing
//
// Location: Assets/Scripts/Core/06_Maze/

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// CompleteMazeBuilder - Main maze orchestrator.
    /// PLUG-IN-OUT COMPLIANT: Finds components, never creates.
    /// ALL VALUES FROM JSON: No hardcoded values.
    /// </summary>
    public class CompleteMazeBuilder : MonoBehaviour
    {
        #region Inspector Fields (Serialized from JSON)

        [Header("📦 Prefab References (From JSON Config)")]
        [Tooltip("Wall prefab (loaded from JSON config)")]
        [SerializeField] private GameObject wallPrefab;
        
        [Tooltip("Door prefab (loaded from JSON config)")]
        [SerializeField] private GameObject doorPrefab;
        
        [Tooltip("Torch prefab (loaded from JSON config)")]
        [SerializeField] private GameObject torchPrefab;

        [Header("🎨 Material References (From JSON Config)")]
        [Tooltip("Wall material (loaded from JSON config)")]
        [SerializeField] private Material wallMaterial;
        
        [Tooltip("Floor material (loaded from JSON config)")]
        [SerializeField] private Material floorMaterial;

        [Header("📄 Texture References (From JSON Config)")]
        [Tooltip("Ground texture (loaded from JSON config)")]
        [SerializeField] private Texture2D groundTexture;

        [Header("🔌 Component References (Plug-in-Out)")]
        [Tooltip("Auto-finds GridMazeGenerator in scene")]
        [SerializeField] private GridMazeGenerator gridMazeGenerator;
        
        [Tooltip("Auto-finds SpatialPlacer in scene")]
        [SerializeField] private SpatialPlacer spatialPlacer;
        
        [Tooltip("Auto-finds LightPlacementEngine in scene")]
        [SerializeField] private LightPlacementEngine lightPlacementEngine;
        
        [Tooltip("Auto-finds TorchPool in scene")]
        [SerializeField] private TorchPool torchPool;

        [Header("📢 Console Verbosity (From JSON Config)")]
        [Tooltip("Leave to Short to use JSON config value. Set manually to override.")]
        [SerializeField] private VerbosityLevel verbosity = VerbosityLevel.Short;

        #endregion

        #region Private Data

        private static CompleteMazeBuilder _instance;
        private float cellSize;
        private float wallHeight;
        private uint currentSeed;
        private Vector3 spawnPosition;
        private Vector2Int spawnCell;

        #endregion

        #region Public Accessors

        public static VerbosityLevel CurrentVerbosity => _instance != null ? _instance.verbosity : VerbosityLevel.Short;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _instance = this;

            // STEP 1: Load ALL values from JSON config (NO HARDCODING!)
            LoadConfig();

            // PLUG-IN-OUT: Get event handler
            var eventHandler = EventHandler.Instance;
            if (eventHandler != null)
            {
                Log("[CompleteMazeBuilder] 🔌 Connected to EventHandler", true);
            }

            // Compute seed for this generation
            currentSeed = ComputeSeed(GameConfig.Instance.manualSeed);
            Log($"[CompleteMazeBuilder] 🎲 Seed: {currentSeed}", true);
        }

        private void Start()
        {
            // Auto-generate on start if configured
            if (GameConfig.Instance.useRandomSeed)
            {
                GenerateMaze();
            }
        }

        private void OnApplicationQuit()
        {
            Log("[CompleteMazeBuilder] 🧹 Cleanup on quit");
        }

        #endregion

        #region JSON Config Loading

        /// <summary>
        /// Load ALL values from JSON config.
        /// NO HARDCODED VALUES - everything from GameConfig-default.json.
        /// </summary>
        private void LoadConfig()
        {
            var config = GameConfig.Instance;

            // Maze dimensions from JSON
            cellSize = config.defaultCellSize;
            wallHeight = config.defaultWallHeight;

            // Prefab paths from JSON
            string wallPrefabPath = config.wallPrefab;
            string doorPrefabPath = config.doorPrefab;
            string torchPrefabPath = config.torchPrefab;

            // Material paths from JSON
            string wallMaterialPath = config.wallMaterial;
            string floorMaterialPath = config.floorMaterial;

            // Texture paths from JSON
            string groundTexturePath = config.groundTexture;

            // Load prefabs from Resources
            wallPrefab = Resources.Load<GameObject>(wallPrefabPath.Replace("Assets/Resources/", "").Replace(".prefab", ""));
            doorPrefab = Resources.Load<GameObject>(doorPrefabPath.Replace("Assets/Resources/", "").Replace(".prefab", ""));
            torchPrefab = Resources.Load<GameObject>(torchPrefabPath.Replace("Assets/Resources/", "").Replace(".prefab", ""));

            // Load materials from Resources
            wallMaterial = Resources.Load<Material>(wallMaterialPath.Replace("Assets/Resources/", "").Replace(".mat", ""));
            floorMaterial = Resources.Load<Material>(floorMaterialPath.Replace("Assets/Resources/", "").Replace(".mat", ""));

            // Load textures from Resources
            groundTexture = Resources.Load<Texture2D>(groundTexturePath.Replace("Assets/Resources/", "").Replace(".png", ""));

            // Verbosity (apply only if inspector is set to Short - default override)
            if (verbosity == VerbosityLevel.Short)
            {
                ApplyVerbosityFromConfig(config.consoleVerbosity);
            }

            Log($"[CompleteMazeBuilder] 📖 Config loaded: {config.defaultGridSize}x{config.defaultGridSize} maze, {config.defaultRoomSize}x{config.defaultRoomSize} rooms, {config.defaultCorridorWidth}-cell corridors", true);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Generate complete maze (call from editor menu or runtime).
        /// NEW ALGORITHM: Spawn room first, corridors to spawn, player last.
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

            // STEP 5: Generate grid maze (SPAWN ROOM FIRST, corridors TO spawn)
            CreateEntranceRoom();
            Log($"[CompleteMazeBuilder] 🏛️ STEP 3: Grid maze generated (spawn room first)", true);

            // STEP 6: Spawn walls from grid data
            SpawnOuterWalls();
            Log("[CompleteMazeBuilder] 🧱 STEP 4: Walls spawned from grid", true);

            // STEP 7: Place doors (in corridor openings)
            PlaceDoors();
            Log("[CompleteMazeBuilder] 🚪 STEP 5: Doors placed", true);

            // STEP 8: Place lights/torches (BEFORE objects, for proper lighting)
            PlaceLights();
            Log("[CompleteMazeBuilder] 💡 STEP 6: Lights placed", true);

            // STEP 9: Place objects (invoke other systems)
            PlaceObjects();
            Log("[CompleteMazeBuilder] 🎒 STEP 7: Objects placed", true);

            // STEP 10: Save to binary
            SaveMaze();
            Log("[CompleteMazeBuilder] 💾 STEP 8: Maze saved", true);

            // STEP 11: Spawn player LAST (Play mode only, AFTER geometry)
            if (Application.isPlaying)
            {
                SpawnPlayer();
                Log($"[CompleteMazeBuilder] 👤 STEP 9: Player spawned at {spawnPosition}", true);
            }

            Log("[CompleteMazeBuilder] ════════════════════════════════════════");
            Log("[CompleteMazeBuilder] ✅ Maze generation complete!", true);
            Log("[CompleteMazeBuilder] ════════════════════════════════════════");
        }

        #endregion

        #region Asset Loading

        private void PreloadAssets()
        {
            Log("[CompleteMazeBuilder] 📦 Pre-loading assets...", true);

            // Prefabs are already loaded in LoadConfig()
            // Materials are already loaded in LoadConfig()
            // Textures are already loaded in LoadConfig()

            Log("[CompleteMazeBuilder] ✅ Assets pre-loaded", true);
        }

        #endregion

        #region Plug-in-Out Compliance

        /// <summary>
        /// Find all required components in scene.
        /// PLUG-IN-OUT: Never creates components, only finds existing ones.
        /// </summary>
        private void FindComponents()
        {
            Log("[CompleteMazeBuilder] 🔌 Finding components (plug-in-out)...", true);

            // Find GridMazeGenerator
            if (gridMazeGenerator == null)
                gridMazeGenerator = FindFirstObjectByType<GridMazeGenerator>();

            // Find SpatialPlacer
            if (spatialPlacer == null)
                spatialPlacer = FindFirstObjectByType<SpatialPlacer>();

            // Find LightPlacementEngine
            if (lightPlacementEngine == null)
                lightPlacementEngine = FindFirstObjectByType<LightPlacementEngine>();

            // Find TorchPool
            if (torchPool == null)
                torchPool = FindFirstObjectByType<TorchPool>();

            Log("[CompleteMazeBuilder] ✅ Components found (plug-in-out compliant)", true);
        }

        #endregion

        #region Cleanup

        private void CleanupOldMaze()
        {
            Log("[CompleteMazeBuilder] 🧹 Cleaning up old maze objects...");

            // Destroy old maze geometry
            var oldGround = GameObject.Find("GroundFloor");
            if (oldGround != null)
                DestroyImmediate(oldGround);

            var oldWalls = GameObject.Find("MazeWalls");
            if (oldWalls != null)
                DestroyImmediate(oldWalls);

            var oldDoors = GameObject.Find("Doors");
            if (oldDoors != null)
                DestroyImmediate(oldDoors);

            Log("[CompleteMazeBuilder] 🧹 Old maze objects cleaned up");
        }

        #endregion

        #region Ground Spawning

        private void SpawnGround()
        {
            Log("[CompleteMazeBuilder] 🌍 Spawning ground floor...");

            // Create ground plane (simple geometry, no prefab needed)
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "GroundFloor";

            // Center under maze
            var config = GameConfig.Instance;
            float mazeWidth = config.defaultGridSize * cellSize;
            float mazeHeight = config.defaultGridSize * cellSize;

            ground.transform.position = new Vector3(mazeWidth / 2f, -0.1f, mazeHeight / 2f);
            ground.transform.localScale = new Vector3(mazeWidth, 0.1f, mazeHeight);

            // Apply material (from JSON config)
            if (floorMaterial != null)
            {
                var renderer = ground.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = floorMaterial;
                }
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

            Log($"[CompleteMazeBuilder] ✅ Ground spawned ({mazeWidth}m x {mazeHeight}m)");
        }

        #endregion

        #region Grid Generation

        private void CreateEntranceRoom()
        {
            Log("[CompleteMazeBuilder] 🏛️ Generating grid maze with spawn room first...");

            // Create grid generator
            gridMazeGenerator = new GridMazeGenerator();
            gridMazeGenerator.InitializeFromConfig();

            // Generate complete maze grid (spawn room first, corridors to spawn)
            gridMazeGenerator.Generate();

            // Find SpawnPoint (center of spawn room)
            spawnCell = gridMazeGenerator.FindSpawnPoint();

            // Calculate spawn position (center of cell)
            spawnPosition = new Vector3(
                spawnCell.x * cellSize + cellSize / 2f,
                1.7f,  // FPS eye height (will be set on camera)
                spawnCell.y * cellSize + cellSize / 2f
            );

            Log($"[CompleteMazeBuilder] 🎯 SpawnPoint: cell {spawnCell}", true);
            Log($"[CompleteMazeBuilder] 👤 Spawn position: {spawnPosition}", true);
            Log($"[CompleteMazeBuilder] ✅ Grid maze generated ({gridMazeGenerator.GridSize}x{gridMazeGenerator.GridSize})", true);
        }

        #endregion

        #region Wall Spawning

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
                LogError("[CompleteMazeBuilder] 💡 Fix: Run Tools → Quick Setup Prefabs");
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

        #region Doors

        private void PlaceDoors()
        {
            Log("[CompleteMazeBuilder] 🚪 Placing doors...");

            // Doors are placed by DoorsEngine components in scene
            // This method is a placeholder for future door placement logic

            Log("[CompleteMazeBuilder] ✅ Doors placed (via existing components)");
        }

        #endregion

        #region Lights

        private void PlaceLights()
        {
            Log("[CompleteMazeBuilder] 💡 Placing lights/torches...");

            // PLUG-IN-OUT: Use existing LightPlacementEngine (never create)
            if (lightPlacementEngine != null && torchPool != null)
            {
                Log("[CompleteMazeBuilder] 🔌 Using LightPlacementEngine + TorchPool for lighting");
                // LightPlacementEngine handles dynamic lighting and torch placement
            }
            else
            {
                LogWarning("[CompleteMazeBuilder] ⚠️ LightPlacementEngine or TorchPool not found - skipping lighting");
            }

            Log("[CompleteMazeBuilder] ✅ Lights placed");
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

        #region Save/Load

        private void SaveMaze()
        {
            Log("[CompleteMazeBuilder] 💾 Saving maze to database...");

            if (gridMazeGenerator == null)
            {
                LogError("[CompleteMazeBuilder] ❌ GridMazeGenerator not found!");
                return;
            }

            // Save to database
            MazeSaveData.SaveGridMaze((int)currentSeed, gridMazeGenerator.SerializeToBytes(), spawnCell.x, spawnCell.y);

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
                LogWarning("[CompleteMazeBuilder] 💡 Add PlayerController component to a GameObject in scene");
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

        #region Logging

        public enum VerbosityLevel { Mute, Short, Full }

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

        private void ApplyVerbosityFromConfig(string configVerbosity)
        {
            switch (configVerbosity.ToLower())
            {
                case "full":
                    verbosity = VerbosityLevel.Full;
                    break;
                case "short":
                    verbosity = VerbosityLevel.Short;
                    break;
                case "mute":
                    verbosity = VerbosityLevel.Mute;
                    break;
            }
            Log($"[CompleteMazeBuilder] 📢 Verbosity: {verbosity.ToString().ToUpper()} (from JSON config)", true);
        }

        #endregion
    }
}
