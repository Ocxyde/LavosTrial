// CompleteMazeBuilder.cs
// Unified maze generation with plug-in-out architecture
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-OUT ARCHITECTURE:
// - Subscribes to EventHandler for game state
// - Publishes maze generation events
// - Independent module (can be added/removed safely)
// - No direct dependencies on other core systems
//
// USAGE:
//   1. Add CompleteMazeBuilder component to GameObject
//   2. Tools → Generate Maze (editor menu)
//   3. Press Play - maze auto-generates
//
// Location: Assets/Scripts/Core/06_Maze/

using UnityEngine;
using System.Collections.Generic;

namespace Code.Lavos.Core
{
    /// <summary>
    /// CompleteMazeBuilder - Auto-generates complete maze with walls, ground, ceiling, doors, rooms.
    ///
    /// PLUG-IN-OUT COMPLIANT:
    /// - Uses EventHandler for core system communication
    /// - Independent component that can be added/removed safely
    /// - Integrates with SpatialPlacer, LightPlacementEngine, TorchPool
    /// - Publishes events for other systems to subscribe
    ///
    /// VERBOSITY LEVELS:
    /// - Mute: No console output
    /// - Short: Only critical messages (errors, warnings, key milestones)
    /// - Full: All debug messages (default, for testing)
    /// </summary>
    public class CompleteMazeBuilder : MonoBehaviour
    {
        #region Verbosity Settings

        /// <summary>
        /// Console output verbosity level.
        /// Loaded from GameConfig-default.json (consoleVerbosity field).
        /// </summary>
        public enum VerbosityLevel
        {
            Mute,    // No output
            Short,   // Critical only (errors, warnings, key milestones)
            Full     // All debug messages (for testing)
        }

        [Header("📢 Console Verbosity (Override JSON config)")]
        [Tooltip("Leave to 'Full' to use JSON config value. Set manually to override.")]
        [SerializeField] private VerbosityLevel verbosity = VerbosityLevel.Full;

        // Static instance for global access
        private static CompleteMazeBuilder _instance;
        public static VerbosityLevel CurrentVerbosity => _instance != null ? _instance.verbosity : VerbosityLevel.Short;

        #endregion

        #region Logging Helpers

        /// <summary>
        /// Log message based on verbosity level.
        /// </summary>
        public static void Log(string message, bool isCritical = false)
        {
            if (_instance == null) return;

            switch (_instance.verbosity)
            {
                case VerbosityLevel.Mute:
                    return;
                case VerbosityLevel.Short:
                    if (isCritical)
                        Debug.Log(message);
                    return;
                case VerbosityLevel.Full:
                    Debug.Log(message);
                    return;
            }
        }

        /// <summary>
        /// Log warning (always shown unless Mute).
        /// </summary>
        public static void LogWarning(string message)
        {
            if (_instance == null || _instance.verbosity == VerbosityLevel.Mute) return;
            Debug.LogWarning(message);
        }

        /// <summary>
        /// Log error (always shown).
        /// </summary>
        public static void LogError(string message)
        {
            Debug.LogError(message);
        }

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

        #region Inspector Settings

        [Header("🏗️ Maze Dimensions")]
        [Tooltip("Maze width in cells (11 for testing)")]
        [SerializeField] private int mazeWidth = 11;  // Small for testing

        [Tooltip("Maze height in cells (11 for testing)")]
        [SerializeField] private int mazeHeight = 11;  // Small for testing
        
        [Tooltip("Size of each cell in meters")]
        [SerializeField] private float cellSize = 6f;
        
        [Tooltip("Wall height in meters")]
        [SerializeField] private float wallHeight = 4f;
        
        [Tooltip("Wall thickness in meters")]
        [SerializeField] private float wallThickness = 0.5f;
        
        [Tooltip("Ceiling height in meters (above ground)")]
        [SerializeField] private float ceilingHeight;  // ← From GameConfig-default.json

        [Header("🚪 Door Settings")]
        [Tooltip("Chance for a passage to have a door (0-1) - From JSON config")]
        [Range(0f, 1f)]
        [SerializeField] private float doorSpawnChance;  // ← From GameConfig-default.json

        [Tooltip("Chance for door to be locked (0-1) - From JSON config")]
        [Range(0f, 1f)]
        [SerializeField] private float lockedDoorChance;  // ← From GameConfig-default.json

        [Tooltip("Chance for door to be secret (0-1) - From JSON config")]
        [Range(0f, 1f)]
        [SerializeField] private float secretDoorChance;  // ← From GameConfig-default.json

        [Header("🏛️ Room Settings")]
        [Tooltip("Generate special rooms (entrance/exit/normal) - From JSON config")]
        [SerializeField] private bool generateRooms;  // ← From GameConfig-default.json

        [Tooltip("Minimum number of rooms - From JSON config")]
        [SerializeField] private int minRooms;  // ← From GameConfig-default.json

        [Tooltip("Maximum number of rooms - From JSON config")]
        [SerializeField] private int maxRooms;  // ← From GameConfig-default.json

        [Header("👤 Player Spawn")]
        [Tooltip("Player spawns inside entrance room (auto-calculated) - From JSON config")]
        [SerializeField] private bool spawnInsideRoom;  // ← From GameConfig-default.json

        [Header("⚙️ Generation Options")]
        [Tooltip("Auto-generate maze on Start")]
        [SerializeField] private bool autoGenerateOnStart = true;

        [Tooltip("Use random seed (false = use manual seed) - From JSON config")]
        [SerializeField] private bool useRandomSeed;  // ← From GameConfig-default.json

        [Tooltip("Manual seed for reproducible generation - From JSON config")]
        [SerializeField] private string manualSeed;  // ← From GameConfig-default.json
        
        [Header("📁 Prefab Paths (Assign in Inspector or SQLite)")]
        [Tooltip("Wall prefab - drag from Assets/Prefabs/ or uses SQLite default")]
        [SerializeField] private string wallPrefabPath = "";
        [Tooltip("Door prefab - drag from Assets/Prefabs/ or uses SQLite default")]
        [SerializeField] private string doorPrefabPath = "";
        [Tooltip("Locked door prefab - drag from Assets/Prefabs/ or uses SQLite default")]
        [SerializeField] private string lockedDoorPrefabPath = "";
        [Tooltip("Secret door prefab - drag from Assets/Prefabs/ or uses SQLite default")]
        [SerializeField] private string secretDoorPrefabPath = "";
        [Tooltip("Entrance room prefab - drag from Assets/Prefabs/ or uses SQLite default")]
        [SerializeField] private string entranceRoomPrefabPath = "";
        [Tooltip("Exit room prefab - drag from Assets/Prefabs/ or uses SQLite default")]
        [SerializeField] private string exitRoomPrefabPath = "";
        [Tooltip("Normal room prefab - drag from Assets/Prefabs/ or uses SQLite default")]
        [SerializeField] private string normalRoomPrefabPath = "";
        
        [Header("📁 Material/Texture Paths")]
        [Tooltip("Wall material - drag from Assets/Materials/ or uses SQLite default")]
        [SerializeField] private string wallMaterialPath = "";
        [Tooltip("Door material - drag from Assets/Materials/ or uses SQLite default")]
        [SerializeField] private string doorMaterialPath = "";
        [Tooltip("Floor material - drag from Assets/Materials/ or uses SQLite default")]
        [SerializeField] private string floorMaterialPath = "";
        [Tooltip("Ground texture - drag from Assets/Textures/ or uses SQLite default")]
        [SerializeField] private string groundTexturePath = "";
        [Tooltip("Wall texture - drag from Assets/Textures/ or uses SQLite default")]
        [SerializeField] private string wallTexturePath = "";
        [Tooltip("Ceiling texture - drag from Assets/Textures/ or uses SQLite default")]
        [SerializeField] private string ceilingTexturePath = "";

        #endregion

        #region Private Fields

        private GridMazeGenerator gridMazeGenerator;  // NEW: Custom maze system
        private MazeGenerator mazeGenerator;  // OLD: Legacy system (disabled)
        private SpatialPlacer spatialPlacer;
        private LightPlacementEngine lightPlacementEngine;
        private LightEngine lightEngine;
        private TorchPool torchPool;

        private uint currentSeed;
        private System.Random rng;

        // Track door positions for proper snapping
        private List<DoorPosition> doorPositions = new List<DoorPosition>();

        // Track entrance room position for player spawn
        private Vector3 entranceRoomPosition;
        private Vector2Int entranceRoomCell;  // ✅ Store actual room cell!
        
        // Persistent storage keys (for PlayerPrefs)
        private const string SPAWN_X_KEY = "MazeSpawnX";
        private const string SPAWN_Z_KEY = "MazeSpawnZ";
        private const string SPAWN_SEED_KEY = "MazeSpawnSeed";
        
        // Plug-in-and-Out: Event handler reference
        private EventHandler eventHandler;

        private struct DoorPosition
        {
            public Vector3 position;
            public Quaternion rotation;
            public int x, y;
            public string direction;
            public string type;
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Set static instance for global access
            _instance = this;

            // STEP 0: Load defaults from JSON config (NO HARDCODED VALUES!)
            ApplyConfigDefaults();

            // PLUG-IN-OUT: Get event handler (central hub for communication)
            eventHandler = EventHandler.Instance;
            if (eventHandler != null)
            {
                Log("[CompleteMazeBuilder] 🔌 Connected to EventHandler (plug-in-out ready)", true);
            }
            else
            {
                Log("[CompleteMazeBuilder] ℹ️ EventHandler not in scene - running standalone (OK for testing)");
            }

            // STEP 1: Create TorchPool FIRST (required by other components)
            torchPool = FindFirstObjectByType<TorchPool>();
            if (torchPool == null)
            {
                var torchGO = new GameObject("TorchPool");
                torchPool = torchGO.AddComponent<TorchPool>();
                Log("[CompleteMazeBuilder] ✅ TorchPool created", true);
            }

            // STEP 2: Add required components
            mazeGenerator = GetOrAddComponent<MazeGenerator>();  // ✅ Save reference!
            GetOrAddComponent<MazeRenderer>();
            spatialPlacer = GetOrAddComponent<SpatialPlacer>();
            lightPlacementEngine = GetOrAddComponent<LightPlacementEngine>();

            // STEP 3: Find or create LightEngine
            lightEngine = FindFirstObjectByType<LightEngine>();
            if (lightEngine == null)
            {
                var lightGO = new GameObject("LightEngine");
                lightEngine = lightGO.AddComponent<LightEngine>();
            }

            // STEP 4: Wire up references
            if (spatialPlacer != null && torchPool != null)
            {
                var torchPoolField = typeof(SpatialPlacer).GetField("torchPool",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (torchPoolField != null)
                {
                    torchPoolField.SetValue(spatialPlacer, torchPool);
                    Debug.Log("[CompleteMazeBuilder] 🔌 TorchPool assigned to SpatialPlacer");
                }

                var lpeField = typeof(SpatialPlacer).GetField("lightPlacementEngine",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (lpeField != null && lightPlacementEngine != null)
                {
                    lpeField.SetValue(spatialPlacer, lightPlacementEngine);
                    Debug.Log("[CompleteMazeBuilder] 🔌 LightPlacementEngine assigned to SpatialPlacer");
                }
            }

            // Initialize seed (NEVER use 0 - always generate valid seed)
            if (useRandomSeed)
            {
                // Generate unique seed from timestamp + random
                currentSeed = (uint)(System.DateTime.Now.Ticks ^ System.Guid.NewGuid().GetHashCode());
                if (currentSeed == 0) currentSeed = 1;  // Ensure never 0
            }
            else
            {
                // Use manual seed (compute from string)
                currentSeed = ComputeSeed(string.IsNullOrEmpty(manualSeed) ? "DefaultMazeSeed" : manualSeed);
                if (currentSeed == 0) currentSeed = 1;  // Ensure never 0
            }
            rng = new System.Random((int)currentSeed);

            Debug.Log("[CompleteMazeBuilder] 🏗️ Component initialized");
            Debug.Log($"[CompleteMazeBuilder] 📏 Maze: {mazeWidth}x{mazeHeight}, Cell: {cellSize}m");
            Debug.Log($"[CompleteMazeBuilder] 🎲 Seed: {currentSeed} (never 0!)");

            // Apply verbosity from JSON config
            ApplyVerbosityFromConfig();
        }

        /// <summary>
        /// Apply verbosity level from GameConfig-default.json.
        /// Called in Awake() after config is loaded.
        /// </summary>
        private void ApplyVerbosityFromConfig()
        {
            var config = GameConfig.Instance;
            if (config == null)
            {
                LogWarning("[CompleteMazeBuilder] ⚠️ GameConfig not loaded - using default verbosity");
                return;
            }

            // Parse verbosity from config string
            VerbosityLevel configVerbosity;
            switch (config.consoleVerbosity.ToLower())
            {
                case "mute":
                    configVerbosity = VerbosityLevel.Mute;
                    break;
                case "short":
                    configVerbosity = VerbosityLevel.Short;
                    break;
                case "full":
                    configVerbosity = VerbosityLevel.Full;
                    break;
                default:
                    LogWarning($"[CompleteMazeBuilder] ⚠️ Invalid verbosity '{config.consoleVerbosity}' in config - using 'short'");
                    configVerbosity = VerbosityLevel.Short;
                    break;
            }

            // Only apply config value if inspector is set to Full (default override)
            if (verbosity == VerbosityLevel.Full)
            {
                verbosity = configVerbosity;
                Log($"[CompleteMazeBuilder] 📢 Verbosity loaded from config: {configVerbosity}", true);
            }
            else
            {
                Log($"[CompleteMazeBuilder] 📢 Verbosity overridden in Inspector: {verbosity} (config value: {configVerbosity})");
            }
        }

        /// <summary>
        /// Apply default values from GameConfig-default.json (NO HARDCODED VALUES!).
        /// ALWAYS applies from JSON (Inspector values are ignored for config fields).
        /// </summary>
        private void ApplyConfigDefaults()
        {
            var config = GameConfig.Instance;

            // Load prefab paths from JSON (CRITICAL!)
            LoadFromJSONConfig();

            // ALWAYS apply from JSON config (no hardcoded values!)
            mazeWidth = config.defaultMazeWidth;
            mazeHeight = config.defaultMazeHeight;
            cellSize = config.defaultCellSize;
            wallHeight = config.defaultWallHeight;
            wallThickness = config.defaultWallThickness;
            ceilingHeight = config.defaultCeilingHeight;
            
            // Apply door settings
            doorSpawnChance = config.defaultDoorSpawnChance;
            lockedDoorChance = config.defaultLockedDoorChance;
            secretDoorChance = config.defaultSecretDoorChance;
            
            // Apply room settings
            minRooms = config.minRooms;
            maxRooms = config.maxRooms;
            generateRooms = config.generateRooms;
            
            // Apply generation options
            useRandomSeed = config.useRandomSeed;
            manualSeed = config.manualSeed;
            spawnInsideRoom = config.spawnInsideRoom;
            
            Debug.Log("[CompleteMazeBuilder] 📦 Applied defaults from GameConfig-default.json");
            Debug.Log($"[CompleteMazeBuilder] 🔧 Config: ceilingHeight={ceilingHeight}, wallHeight={wallHeight}, mazeSize={mazeWidth}x{mazeHeight}");
        }

        private void OnEnable()
        {
            // PLUG-IN-OUT: Subscribe to core events
            if (eventHandler != null)
            {
                eventHandler.OnGameStateChanged += OnGameStateChanged;
            }
        }

        private void OnDisable()
        {
            // PLUG-IN-OUT: Unsubscribe from core events
            if (eventHandler != null)
            {
                eventHandler.OnGameStateChanged -= OnGameStateChanged;
            }
        }
        
        /// <summary>
        /// Release all RAM on game quit (Alt+F4, close, etc.).
        /// PLUG-IN-OUT: Clean resource management.
        /// </summary>
        private void OnApplicationQuit()
        {
            Debug.Log("[CompleteMazeBuilder] 🧹 Releasing RAM on game quit...");

            // Save player settings before quit
            SavePlayerSettingsOnQuit();

            // Clear runtime data (not persistent data)
            doorPositions?.Clear();

            // Destroy LightEngine if it exists (prevent scene leak)
            if (lightEngine != null)
            {
                if (Application.isPlaying)
                    Destroy(lightEngine.gameObject);
                else
                    DestroyImmediate(lightEngine.gameObject);
                Debug.Log("[CompleteMazeBuilder] 🗑️ LightEngine destroyed");
            }

            // Release references
            mazeGenerator = null;
            spatialPlacer = null;
            lightPlacementEngine = null;
            torchPool = null;
            lightEngine = null;

            // Force garbage collection
            System.GC.Collect();
            
            Debug.Log("[CompleteMazeBuilder] ✅ RAM released - clean quit");
        }
        
        /// <summary>
        /// Save player settings on quit (Alt+F4, close, etc.).
        /// </summary>
        private void SavePlayerSettingsOnQuit()
        {
            var settings = new Dictionary<string, string>
            {
                { "MouseSensitivity", "1.0" },  // Would use actual values
                { "GraphicsQuality", "Medium" },
                { "SoundVolume", "0.8" }
            };
            
            MazeSaveData.SaveAllPlayerSettings(settings);
            Debug.Log("[CompleteMazeBuilder] 💾 Player settings saved on quit");
        }

        /// <summary>
        /// Handle game state changes via EventHandler (plug-in-out compliance).
        /// </summary>
        private void OnGameStateChanged(GameManager.GameState newState)
        {
            if (newState == GameManager.GameState.Playing)
            {
                Debug.Log("[CompleteMazeBuilder] ▶️ Game resumed - maze active");
            }
            else if (newState == GameManager.GameState.Paused)
            {
                Debug.Log("[CompleteMazeBuilder] ⏸️ Game paused - maze frozen");
            }
        }

        private void Start()
        {
            if (autoGenerateOnStart)
            {
                GenerateCompleteMaze();  // This will spawn player in Play mode
            }
            else
            {
                // If maze already exists (generated in editor), just spawn player
                if (Application.isPlaying)
                {
                    SpawnPlayer();
                }
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Generate complete maze with walls, ground, ceiling, doors, rooms.
        /// PLUG-IN-OUT: Publishes MazeGenerated event on completion.
        /// </summary>
        [ContextMenu("Generate Complete Maze")]
        public void GenerateCompleteMaze()
        {
            GenerateMazeGeometryOnly();
            
            // Spawn player (only in Play mode!)
            if (Application.isPlaying)
            {
                SpawnPlayer();
            }
            else
            {
                Debug.Log("[CompleteMazeBuilder] ℹ️ Editor mode - player will spawn in Play mode");
            }
        }
        
        /// <summary>
        /// Generate maze geometry only (no player). For editor use.
        /// Player spawns automatically in Play mode.
        ///
        /// GENERATION ORDER (Plug-in-Out Compliant):
        /// 1. CLEANUP - Remove ALL old objects (fresh start)
        /// 2. GROUND - Spawn ground floor FIRST (base layer)
        /// 3. VIRTUAL GRID - Create grid & place rooms (memory only)
        /// 4. CORRIDORS - Carve corridors in grid (memory only)
        /// 5. WALLS - Read grid, spawn walls ONLY where marked
        /// 6. DOORS - Place doors in wall openings
        /// 7. OBJECTS - Place torches, chests, enemies, items
        /// 8. SAVE - Save grid to database (binary)
        /// 9. PLAYER - Spawn player in room center (Play mode only)
        /// </summary>
        public void GenerateMazeGeometryOnly()
        {
            Debug.Log("[CompleteMazeBuilder] ════════════════════════════════════════");
            Debug.Log("[CompleteMazeBuilder] 🏗️ Starting maze generation (FRESH START)...");
            Debug.Log("[CompleteMazeBuilder] ════════════════════════════════════════");

            doorPositions.Clear();
            entranceRoomPosition = Vector3.zero;
            entranceRoomCell = new Vector2Int(-1, -1);

            // STEP 1: CLEANUP - Remove ALL old maze objects (fresh start)
            CleanupOldMazeObjects();

            // STEP 2: GROUND - Spawn ground floor FIRST (base layer for everything)
            SpawnGroundFloor();
            Debug.Log("[CompleteMazeBuilder] 🌍 Step 1: Ground spawned (base layer)");

            // STEP 3: VIRTUAL GRID - Create grid & place rooms (memory only, no GameObjects)
            CreateVirtualGridAndPlaceRooms();
            Debug.Log($"[CompleteMazeBuilder] 🏛️ Step 2: Rooms placed in virtual grid at {entranceRoomCell}");

            // STEP 4: CORRIDORS - Carve corridors in grid (memory only)
            GenerateCorridors();
            Debug.Log("[CompleteMazeBuilder] 🔨 Step 3: Corridors carved in grid");

            // STEP 5: WALLS - Read grid & spawn walls ONLY where grid has walls
            SpawnWallsFromGrid();
            Debug.Log("[CompleteMazeBuilder] 🧱 Step 4: Walls spawned from grid (rooms/corridors are CLEAR)");

            // STEP 6: DOORS - Place doors in wall openings
            SpawnDoors();
            Debug.Log("[CompleteMazeBuilder] 🚪 Step 5: Doors placed");

            // STEP 7: OBJECTS - Place torches, chests, enemies, items
            PlaceObjects();
            Debug.Log("[CompleteMazeBuilder] 🎒 Step 6: Objects placed (torches, chests, enemies, items)");

            // STEP 8: SAVE - Save grid to database (binary format - 1 byte per cell)
            SaveGridToDatabase();
            Debug.Log("[CompleteMazeBuilder] 💾 Step 7: Grid saved to database");

            // [OPTIONAL] STEP 9: CEILING - Spawn ceiling (disabled for testing)
            // SpawnCeiling();
            Debug.Log("[CompleteMazeBuilder] ☁️ Ceiling SKIPPED (testing mode - top-down view)");

            Debug.Log("[CompleteMazeBuilder] ════════════════════════════════════════");
            Debug.Log("[CompleteMazeBuilder] ✅ Maze geometry complete!");
            Debug.Log($"[CompleteMazeBuilder] 📏 Dimensions: {mazeWidth}x{mazeHeight} cells ({mazeWidth * cellSize}m x {mazeHeight * cellSize}m)");
            Debug.Log($"[CompleteMazeBuilder] 🏛️ Rooms: Placed FIRST, corridors carved around");
            Debug.Log($"[CompleteMazeBuilder] 👤 Player spawn: {entranceRoomPosition}");
            Debug.Log("[CompleteMazeBuilder] ════════════════════════════════════════");
        }
        
        /// <summary>
        /// Save grid maze to database (binary format - 1 byte per cell).
        /// Called after maze generation.
        /// </summary>
        private void SaveGridToDatabase()
        {
            if (gridMazeGenerator == null) return;
            
            // Serialize grid to bytes
            byte[] gridData = gridMazeGenerator.SerializeToBytes();
            
            // Save to database
            MazeSaveData.SaveGridMaze((int)currentSeed, gridData, entranceRoomCell.x, entranceRoomCell.y);
        }
        
        /// <summary>
        /// Load grid maze from database.
        /// Returns true if loaded successfully.
        /// </summary>
        private bool LoadGridFromDatabase()
        {
            // Try to load from database
            byte[] gridData = MazeSaveData.LoadGridMaze((int)currentSeed);
            
            if (gridData == null)
            {
                Debug.Log("[CompleteMazeBuilder] 💾 No saved grid found - will generate new maze");
                return false;
            }
            
            // Create new grid generator and deserialize
            gridMazeGenerator = new GridMazeGenerator();
            gridMazeGenerator.DeserializeFromBytes(gridData);
            
            // Set spawn position from saved data
            entranceRoomCell = gridMazeGenerator.FindSpawnPoint();
            entranceRoomPosition = new Vector3(
                entranceRoomCell.x * cellSize + cellSize / 2f,
                0.9f,
                entranceRoomCell.y * cellSize + cellSize / 2f
            );
            
            Debug.Log("[CompleteMazeBuilder] 📂 Loaded grid maze from database");
            return true;
        }
        
        /// <summary>
        /// Clean up ALL scene objects except essential managers.
        /// Called on scene load to ensure FRESH start (only ground will be spawned).
        /// </summary>
        private void CleanupOldMazeObjects()
        {
            Debug.Log("[CompleteMazeBuilder] 🧹 Cleaning up ALL scene objects (fresh start)...");

            // DESTROY ALL maze-related objects (comprehensive cleanup)
            DestroyOldObject("Ceiling");
            DestroyOldObject("GroundFloor");
            DestroyOldObject("MazeWalls");
            DestroyOldObject("Doors");
            DestroyOldObject("Torches");
            DestroyOldObject("Enemies");
            DestroyOldObject("Chests");
            DestroyOldObject("Items");
            DestroyOldObject("MazeObjects");
            DestroyOldObject("RoomObjects");
            DestroyOldObject("CorridorObjects");
            DestroyOldObject("WallObjects");
            DestroyOldObject("DoorObjects");
            DestroyOldObject("TorchObjects");
            DestroyOldObject("EnemyObjects");
            DestroyOldObject("ChestObjects");
            DestroyOldObject("ItemObjects");

            // Find and destroy ANY object with "Maze" in name (catch-all)
            var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var obj in allObjects)
            {
                if (obj == null) continue;
                if (obj.name.Contains("Maze") || obj.name.Contains("Wall") || 
                    obj.name.Contains("Door") || obj.name.Contains("Torch") ||
                    obj.name.Contains("Room") || obj.name.Contains("Corridor"))
                {
                    // Don't destroy the CompleteMazeBuilder itself!
                    if (obj == gameObject) continue;
                    // Don't destroy essential managers
                    if (obj.name.Contains("Engine") || obj.name.Contains("Manager") || 
                        obj.name.Contains("System") || obj.name.Contains("Handler"))
                        continue;
                    
                    if (Application.isPlaying)
                        Destroy(obj);
                    else
                        DestroyImmediate(obj);
                    Debug.Log($"[CompleteMazeBuilder] 🗑️ Cleaned up: {obj.name}");
                }
            }

            Debug.Log("[CompleteMazeBuilder] ✅ Cleanup complete - SCENE IS NOW EMPTY (ready for fresh generation)");
        }

        /// <summary>
        /// Helper to destroy an old object by name.
        /// </summary>
        private void DestroyOldObject(string name)
        {
            var obj = GameObject.Find(name);
            if (obj != null)
            {
                if (Application.isPlaying)
                    Destroy(obj);
                else
                    DestroyImmediate(obj);
                Debug.Log($"[CompleteMazeBuilder] 🗑️ Removed: {name}");
            }
        }

        /// <summary>
        /// Validate all prefab/material/texture paths.
        /// </summary>
        [ContextMenu("Validate Paths")]
        public bool ValidatePaths()
        {
            Debug.Log("[CompleteMazeBuilder] 🔍 Validating paths...");
            bool allValid = true;
            string basePath = Application.dataPath + "/";

            allValid &= ValidatePath(basePath + wallPrefabPath, "Wall Prefab");
            allValid &= ValidatePath(basePath + doorPrefabPath, "Door Prefab");
            allValid &= ValidatePath(basePath + lockedDoorPrefabPath, "Locked Door Prefab");
            allValid &= ValidatePath(basePath + secretDoorPrefabPath, "Secret Door Prefab");
            allValid &= ValidatePath(basePath + wallMaterialPath, "Wall Material");
            allValid &= ValidatePath(basePath + doorMaterialPath, "Door Material");
            allValid &= ValidatePath(basePath + floorMaterialPath, "Floor Material");
            allValid &= ValidatePath(basePath + groundTexturePath, "Ground Texture");
            allValid &= ValidatePath(basePath + wallTexturePath, "Wall Texture");
            allValid &= ValidatePath(basePath + ceilingTexturePath, "Ceiling Texture");

            if (!allValid)
            {
                Debug.LogWarning("[CompleteMazeBuilder] ⚠️ Some paths invalid!");
                Debug.LogWarning("[CompleteMazeBuilder] 💡 Run: Tools → Create Maze Prefabs");
            }
            else
            {
                Debug.Log("[CompleteMazeBuilder] ✅ All paths valid!");
            }

            return allValid;
        }

        #endregion

        #region Generation Methods

        /// <summary>
        /// STEP 1: Create virtual grid & place rooms FIRST (NEW SYSTEM).
        /// Uses GridMazeGenerator instead of legacy MazeGenerator.
        /// </summary>
        private void CreateVirtualGridAndPlaceRooms()
        {
            Debug.Log("[CompleteMazeBuilder] 🔲 Creating NEW grid maze system...");
            
            // Create NEW custom maze generator
            gridMazeGenerator = new GridMazeGenerator();
            gridMazeGenerator.gridSize = mazeWidth;  // Use inspector values
            gridMazeGenerator.roomSize = 5;  // 5x5 rooms (spacious)
            gridMazeGenerator.corridorWidth = 2;  // 2 cells wide
            
            // Generate maze (rooms + corridors + spawn points)
            gridMazeGenerator.Generate();
            
            // Find the marked SpawnPoint cell (center of entrance room)
            Vector2Int spawnCell = gridMazeGenerator.FindSpawnPoint();
            
            // Store spawn position (EXACT center cell marked as SpawnPoint)
            entranceRoomCell = spawnCell;
            entranceRoomPosition = new Vector3(
                spawnCell.x * cellSize + cellSize / 2f,  // Center of spawn cell
                0.9f,  // Feet on ground (CharacterController center)
                spawnCell.y * cellSize + cellSize / 2f   // Center of spawn cell
            );
            
            Debug.Log($"[CompleteMazeBuilder] 🎯 SpawnPoint cell: {spawnCell}");
            Debug.Log($"[CompleteMazeBuilder] 👤 Spawn position: {entranceRoomPosition}");
            Debug.Log($"[CompleteMazeBuilder] ✅ Grid maze created (SpawnPoint marked in entrance room)");
        }

        /// <summary>
        /// Place rooms in grid, carving out 3x3 clear areas.
        /// </summary>
        private void PlaceRoomsInGrid(MazeGenerator.Wall[,] grid)
        {
            int numRooms = Random.Range(minRooms, maxRooms + 1);
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            
            Debug.Log($"[CompleteMazeBuilder] 🏛️ Placing {numRooms} rooms FIRST...");
            
            // Place entrance room (top-left area)
            Vector2Int entrancePos = FindValidRoomPosition(grid, 0, 0, width / 3, height / 2);
            if (entrancePos.x >= 0)
            {
                SpawnRoom(entrancePos, "Entrance", entranceRoomPrefabPath, hasEntrance: true, hasExit: true);
                
                // Store spawn position (CENTER of 3x3 room)
                entranceRoomCell = new Vector2Int(entrancePos.x + 1, entrancePos.y + 1);
                entranceRoomPosition = new Vector3(
                    (entrancePos.x + 1.5f) * cellSize,
                    0.9f,
                    (entrancePos.y + 1.5f) * cellSize
                );
                
                // Carve out 3x3 room area (NO WALLS inside room)
                MarkRoomCellsClear(grid, entrancePos);
                MarkRoomDoors(grid, entrancePos);
                
                Debug.Log($"[CompleteMazeBuilder] 🏛️ Entrance room at ({entrancePos.x}, {entrancePos.y}), spawn at {entranceRoomPosition}");
            }
            
            // Place exit room (bottom-right area)
            Vector2Int exitPos = FindValidRoomPosition(grid, width * 2 / 3, height / 2, width, height);
            if (exitPos.x >= 0)
            {
                SpawnRoom(exitPos, "Exit", exitRoomPrefabPath, hasEntrance: true, hasExit: true);
                MarkRoomCellsClear(grid, exitPos);
                MarkRoomDoors(grid, exitPos);
                Debug.Log($"[CompleteMazeBuilder] 🏛️ Exit room at ({exitPos.x}, {exitPos.y})");
            }
            
            // Place normal rooms
            int roomsSpawned = entrancePos.x >= 0 ? 1 : 0;
            while (roomsSpawned < numRooms)
            {
                Vector2Int roomPos = FindValidRoomPosition(grid, 0, 0, width, height);
                if (roomPos.x >= 0)
                {
                    SpawnRoom(roomPos, "Normal", normalRoomPrefabPath, hasEntrance: true, hasExit: true);
                    MarkRoomCellsClear(grid, roomPos);
                    MarkRoomDoors(grid, roomPos);
                    roomsSpawned++;
                }
                else
                {
                    break;
                }
            }
            
            Debug.Log($"[CompleteMazeBuilder] ✅ {roomsSpawned} rooms placed (3x3 clear areas carved in grid)");
        }

        /// <summary>
        /// STEP 2: Generate corridors between rooms.
        /// Already done by GridMazeGenerator - this is a no-op.
        /// </summary>
        private void GenerateCorridors()
        {
            // Corridors already carved by GridMazeGenerator.Generate()
            Debug.Log("[CompleteMazeBuilder] 🔨 Corridors already carved (by GridMazeGenerator)");
        }

        /// <summary>
        /// STEP 6: Read grid and spawn walls ONLY where grid has walls.
        /// Uses NEW GridMazeGenerator system.
        /// Rooms and Corridors are CLEAR (no interior walls).
        /// </summary>
        private void SpawnWallsFromGrid()
        {
            if (gridMazeGenerator == null)
            {
                Debug.LogError("[CompleteMazeBuilder] ❌ GridMazeGenerator not found!");
                return;
            }
            
            int size = gridMazeGenerator.GridSize;
            int wallsSpawned = 0;
            
            Debug.Log($"[CompleteMazeBuilder] 🧱 Spawning walls from grid ({size}x{size})...");
            
            // Spawn walls based on grid cell types
            // Walls go BETWEEN cells (at cell edges)
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var cell = gridMazeGenerator.GetCell(x, y);
                    Vector3 cellPos = new Vector3(x * cellSize + cellSize / 2f, wallHeight / 2f, y * cellSize + cellSize / 2f);
                    
                    // Rooms and Corridors are CLEAR - no walls inside!
                    // Only Floor cells get walls
                    if (cell == GridMazeCell.Floor)
                    {
                        // Check adjacent cells - if neighbor is Room/Corridor, add wall
                        if (x + 1 < size)
                        {
                            var eastNeighbor = gridMazeGenerator.GetCell(x + 1, y);
                            if (eastNeighbor == GridMazeCell.Room || eastNeighbor == GridMazeCell.Corridor)
                            {
                                SpawnWall(cellPos + Vector3.right * (cellSize / 2f), Quaternion.Euler(0f, 0f, 0f), x, y, "East");
                                wallsSpawned++;
                            }
                        }
                        if (y + 1 < size)
                        {
                            var northNeighbor = gridMazeGenerator.GetCell(x, y + 1);
                            if (northNeighbor == GridMazeCell.Room || northNeighbor == GridMazeCell.Corridor)
                            {
                                SpawnWall(cellPos + Vector3.forward * (cellSize / 2f), Quaternion.Euler(0f, 90f, 0f), x, y, "North");
                                wallsSpawned++;
                            }
                        }
                    }
                }
            }
            
            // Add outer perimeter walls
            SpawnOuterPerimeterWalls(size, size);
            
            Debug.Log($"[CompleteMazeBuilder] 🧱 {wallsSpawned} wall segments spawned (Rooms/Corridors are CLEAR!)");
        }

        private void SpawnOuterPerimeterWalls(int width, int height)
        {
            int outerWallsSpawned = 0;
            
            // North & South outer walls
            for (int x = 0; x < width; x++)
            {
                SpawnWall(new Vector3(x * cellSize + cellSize / 2f, wallHeight / 2f, height * cellSize),
                    Quaternion.Euler(0f, 90f, 0f), x, height - 1, "OuterNorth");
                outerWallsSpawned++;

                SpawnWall(new Vector3(x * cellSize + cellSize / 2f, wallHeight / 2f, 0f),
                    Quaternion.Euler(0f, 90f, 0f), x, 0, "OuterSouth");
                outerWallsSpawned++;
            }
            
            // East & West outer walls
            for (int y = 0; y < height; y++)
            {
                SpawnWall(new Vector3(width * cellSize, wallHeight / 2f, y * cellSize + cellSize / 2f),
                    Quaternion.Euler(0f, 0f, 0f), width - 1, y, "OuterEast");
                outerWallsSpawned++;

                SpawnWall(new Vector3(0f, wallHeight / 2f, y * cellSize + cellSize / 2f),
                    Quaternion.Euler(0f, 0f, 0f), 0, y, "OuterWest");
                outerWallsSpawned++;
            }
        }

        private void GenerateMazeLayout()
        {
            mazeGenerator = GetComponent<MazeGenerator>();
            if (mazeGenerator == null)
            {
                Debug.LogError("[CompleteMazeBuilder] ❌ MazeGenerator component missing!");
                return;
            }

            // Configure maze size - fields are PUBLIC [SerializeField]
            mazeGenerator.width = mazeWidth;
            mazeGenerator.height = mazeHeight;

            Debug.Log($"[CompleteMazeBuilder] 📐 Set MazeGenerator size: {mazeGenerator.width}x{mazeGenerator.height}");

            // ALWAYS generate maze first (creates the grid)
            mazeGenerator.Generate();
            Debug.Log($"[CompleteMazeBuilder] ✅ Maze layout generated ({mazeWidth}x{mazeHeight})");
        }

        private void SpawnGroundFloor()
        {
            // Check if ground already exists (safety check)
            var existingGround = GameObject.Find("GroundFloor");
            if (existingGround != null)
            {
                Debug.LogWarning("[CompleteMazeBuilder] ⚠️ GroundFloor already exists - destroying and recreating...");
                if (Application.isPlaying)
                    Destroy(existingGround);
                else
                    DestroyImmediate(existingGround);
            }

            // Simple ground floor (like ceiling but at bottom)
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "GroundFloor";

            // Position: Center the ground under the maze
            // Maze spans from (0,0,0) to (mazeWidth*cellSize, 0, mazeHeight*cellSize)
            float centerX = (mazeWidth * cellSize) / 2f;
            float centerZ = (mazeHeight * cellSize) / 2f;

            ground.transform.position = new Vector3(centerX, -0.1f, centerZ);
            ground.transform.localScale = new Vector3(mazeWidth * cellSize, 0.1f, mazeHeight * cellSize);

            // Apply floor material (uses existing Stone_Floor.mat from FloorMaterialFactory)
            ApplyMaterial(ground, floorMaterialPath);

            Debug.Log($"[CompleteMazeBuilder] 🌍 Spawned ground floor at ({centerX}, -0.1, {centerZ}), size: {ground.transform.localScale.x}m x {ground.transform.localScale.z}m");
            Debug.Log($"[CompleteMazeBuilder] 🌍 Maze bounds: X[0 to {mazeWidth * cellSize}m], Z[0 to {mazeHeight * cellSize}m]");
        }

        private void SpawnCeiling()
        {
            // Check if ceiling already exists (safety check)
            var existingCeiling = GameObject.Find("Ceiling");
            if (existingCeiling != null)
            {
                Debug.LogWarning("[CompleteMazeBuilder] ⚠️ Ceiling already exists - destroying and recreating...");
                if (Application.isPlaying)
                    Destroy(existingCeiling);
                else
                    DestroyImmediate(existingCeiling);
            }

            // Create ceiling covering entire maze
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling";

            float centerX = (mazeWidth * cellSize) / 2f;
            float centerZ = (mazeHeight * cellSize) / 2f;

            ceiling.transform.position = new Vector3(centerX, ceilingHeight, centerZ);
            ceiling.transform.localScale = new Vector3(mazeWidth * cellSize, 0.1f, mazeHeight * cellSize);

            // Apply ceiling material with texture
            ApplyMaterial(ceiling, floorMaterialPath);
            ApplyTexture(ceiling, ceilingTexturePath);

            Debug.Log($"[CompleteMazeBuilder] ☁️ Spawned ceiling at ({centerX}, {ceilingHeight}, {centerZ}), size: {ceiling.transform.localScale.x}m x {ceiling.transform.localScale.z}m");
        }

        // [OBSOLETE] Use SpawnWallsFromGrid() instead
        // private void SpawnWalls() { ... }
        
        private void SpawnWall(Vector3 position, Quaternion rotation, int x, int y, string direction)
        {
            // Try to load wall prefab
            GameObject wallPrefab = LoadPrefab(wallPrefabPath);

            GameObject wall;
            if (wallPrefab != null)
            {
                wall = Instantiate(wallPrefab, position, rotation);
            }
            else
            {
                // Fallback: create wall cube
                wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = $"Wall_{x}_{y}_{direction}";
                wall.transform.position = position;
                wall.transform.rotation = rotation;
                wall.transform.localScale = new Vector3(cellSize, wallHeight, wallThickness);
            }

            wall.name = $"Wall_{x}_{y}_{direction}";

            // Apply material AND texture
            ApplyMaterial(wall, wallMaterialPath);
            ApplyTexture(wall, wallTexturePath);
        }

        private void SpawnDoors()
        {
            var gridField = mazeGenerator.GetType().GetProperty("Grid");
            var grid = gridField?.GetValue(mazeGenerator) as MazeGenerator.Wall[,];

            if (grid == null) return;

            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            int doorsSpawned = 0;

            // Spawn doors at passage points (where walls are missing between cells)
            for (int x = 0; x < width - 1; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    var cell = grid[x, y];
                    Vector3 cellPos = new Vector3(x * cellSize + cellSize / 2f, 0f, y * cellSize + cellSize / 2f);

                    if ((cell & MazeGenerator.Wall.East) == 0)
                    {
                        if (Random.value < doorSpawnChance)
                        {
                            Vector3 doorPos = new Vector3(
                                (x + 1) * cellSize,
                                wallHeight / 2f,
                                y * cellSize + cellSize / 2f
                            );
                            Quaternion doorRot = Quaternion.Euler(0f, 0f, 0f);

                            string doorType = DetermineDoorType();
                            string prefabPath = GetDoorPrefabPath(doorType);

                            SpawnDoor(doorPos, doorRot, x, y, "East", doorType, prefabPath);
                            doorsSpawned++;

                            doorPositions.Add(new DoorPosition {
                                position = doorPos,
                                rotation = doorRot,
                                x = x, y = y,
                                direction = "East",
                                type = doorType
                            });
                        }
                    }

                    if ((cell & MazeGenerator.Wall.South) == 0)
                    {
                        if (Random.value < doorSpawnChance)
                        {
                            Vector3 doorPos = new Vector3(
                                x * cellSize + cellSize / 2f,
                                wallHeight / 2f,
                                y * cellSize
                            );
                            Quaternion doorRot = Quaternion.Euler(0f, 90f, 0f);

                            string doorType = DetermineDoorType();
                            string prefabPath = GetDoorPrefabPath(doorType);

                            SpawnDoor(doorPos, doorRot, x, y, "South", doorType, prefabPath);
                            doorsSpawned++;

                            doorPositions.Add(new DoorPosition {
                                position = doorPos,
                                rotation = doorRot,
                                x = x, y = y,
                                direction = "South",
                                type = doorType
                            });
                        }
                    }
                }
            }

            Debug.Log($"[CompleteMazeBuilder] 🚪 Spawned {doorsSpawned} doors (snapped to wall gaps)");
        }

        private string DetermineDoorType()
        {
            float roll = Random.value;

            if (roll < secretDoorChance)
                return "Secret";
            else if (roll < secretDoorChance + lockedDoorChance)
                return "Locked";
            else
                return "Normal";
        }

        private string GetDoorPrefabPath(string doorType)
        {
            switch (doorType)
            {
                case "Secret": return secretDoorPrefabPath;
                case "Locked": return lockedDoorPrefabPath;
                default: return doorPrefabPath;
            }
        }

        private void SpawnDoor(Vector3 position, Quaternion rotation, int x, int y, string direction, string doorType, string prefabPath, bool openByDefault = false)
        {
            GameObject doorPrefab = LoadPrefab(prefabPath);

            GameObject door;
            if (doorPrefab != null)
            {
                door = Instantiate(doorPrefab, position, rotation);
            }
            else
            {
                door = GameObject.CreatePrimitive(PrimitiveType.Cube);
                door.transform.position = position;
                door.transform.rotation = rotation;
                door.transform.localScale = new Vector3(wallThickness, wallHeight, cellSize * 0.9f);
            }

            door.name = $"{doorType}Door_{x}_{y}_{direction}";

            ApplyMaterial(door, doorMaterialPath);

            var doorsEngine = door.AddComponent<DoorsEngine>();
            doorsEngine.Initialize(DoorVariant.Normal, DoorTrapType.None, locked: false, openByDefault: openByDefault);

            Debug.Log($"[CompleteMazeBuilder] 🚪 {doorType} door at ({x}, {y}) {direction} ({(openByDefault ? "OPEN" : "CLOSED")})");
        }
        
        /// <summary>
        /// Spawn mechanical exit door (single door, accessible from both sides).
        /// PLUG-IN-OUT: Uses DoorsEngine component for mechanical operation.
        /// Located at maze perimeter (south wall center).
        /// 
        /// This is the FINAL EXIT door - simple, single door.
        /// For special exit room with saloon doors at entrance, see SpawnSpecialExitRoom() below.
        /// </summary>
        private void SpawnMechanicalExitDoor()
        {
            int width = mazeWidth;
            int height = mazeHeight;

            // Exit door position: center of south outer wall
            int exitX = width / 2;
            int exitZ = height;  // South perimeter

            Vector3 exitPos = new Vector3(
                exitX * cellSize + cellSize / 2f,
                wallHeight / 2f,
                exitZ * cellSize
            );

            Quaternion exitRot = Quaternion.Euler(0f, 90f, 0f);  // Facing north (into maze)

            Debug.Log($"[CompleteMazeBuilder] 🚪 Spawning MECHANICAL EXIT DOOR at ({exitX}, {exitZ})");

            // Create exit door GameObject
            GameObject exitDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            exitDoor.name = "MechanicalExitDoor";
            exitDoor.transform.position = exitPos;
            exitDoor.transform.rotation = exitRot;
            exitDoor.transform.localScale = new Vector3(wallThickness * 2f, wallHeight, cellSize * 1.5f);

            // Apply door material
            ApplyMaterial(exitDoor, doorMaterialPath);

            // Add DoorsEngine component (mechanical operation)
            var doorsEngine = exitDoor.AddComponent<DoorsEngine>();
            
            // Initialize door as unlocked normal door (player can exit)
            doorsEngine.Initialize(DoorVariant.Normal, DoorTrapType.None, locked: false);
            
            // Note: interactionRange is protected in BehaviorEngine, set via serialized field or method
            // For now, default range (3f) from BehaviorEngine is used

            // Add interaction trigger
            var collider = exitDoor.GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.isTrigger = false;  // Solid door
            }

            Debug.Log($"[CompleteMazeBuilder] ✅ Mechanical exit door placed at {exitPos}");
            Debug.Log($"[CompleteMazeBuilder] 🔓 Door is UNLOCKED - player can exit maze!");
            Debug.Log($"[CompleteMazeBuilder] ⚙️ DoorsEngine attached - mechanical operation enabled");
        }
        
        // ============================================================================
        // SPECIAL EXIT ROOM (COMMENTED OUT - FOR LATER TESTING)
        // ============================================================================
        // Creates a rectangular room that extends maze south by 1 room
        // ENTRANCE: Saloon-style double-sided doors (from maze into room)
        // EXIT: Simple single door (south wall, final exit)
        // ============================================================================
        
        /*
        /// <summary>
        /// Spawn SPECIAL EXIT ROOM with saloon doors at entrance.
        /// 
        /// ROOM LAYOUT:
        /// ┌─────────────────────────────────┐
        /// │         MAZE ENDS HERE          │
        /// ├─────────────────────────────────┤
        /// │  🚪  🚪  🚪     │               │
        /// │ Saloon Doors    │ EXIT ROOM     │
        /// │ (Entrance)      │               │
        /// │                 │               │
        /// │                 │      🚪       │
        /// │                 │   Final Exit  │
        /// └─────────────────┴───────┬───────┘
        ///                    SOUTH (outside)
        /// 
        /// FEATURES:
        /// - Rectangular room (extends maze south)
        /// - ENTRANCE: 3 saloon doors (double-sided, from maze)
        /// - EXIT: 1 simple door (south wall, final exit)
        /// - Saloon doors swing both ways (push/pull)
        /// - Final exit is simple single door
        /// 
        /// PLUG-IN-OUT: Uses DoorsEngine for all doors.
        /// </summary>
        private void SpawnSpecialExitRoom()
        {
            int width = mazeWidth;
            int height = mazeHeight;
            
            // Exit room position: south of maze, centered
            int roomX = width / 2 - 1;  // Center room
            int roomZ = height;  // Just south of maze
            
            Debug.Log($"[CompleteMazeBuilder] 🏛️ Spawning SPECIAL EXIT ROOM at ({roomX}, {roomZ})");
            
            // Create exit room floor
            GameObject roomFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roomFloor.name = "ExitRoom_Floor";
            roomFloor.transform.position = new Vector3(
                roomX * cellSize + cellSize,  // Center of 2-cell wide room
                -0.1f,
                roomZ * cellSize + cellSize / 2f
            );
            roomFloor.transform.localScale = new Vector3(
                cellSize * 2f,  // 2 cells wide
                0.1f,
                cellSize * 2f   // 2 cells deep
            );
            ApplyMaterial(roomFloor, floorMaterialPath);
            
            // Create exit room ceiling
            GameObject roomCeiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roomCeiling.name = "ExitRoom_Ceiling";
            roomCeiling.transform.position = new Vector3(
                roomX * cellSize + cellSize,
                ceilingHeight,
                roomZ * cellSize + cellSize / 2f
            );
            roomCeiling.transform.localScale = new Vector3(
                cellSize * 2f,
                0.1f,
                cellSize * 2f
            );
            ApplyMaterial(roomCeiling, ceilingTexturePath);
            
            // Create 3 saloon-style double-sided doors at ENTRANCE (north wall of room)
            float doorSpacing = cellSize / 3f;  // Even spacing
            for (int i = 0; i < 3; i++)
            {
                float doorX = (roomX * cellSize) + (doorSpacing * (i + 0.5f)) + (doorSpacing / 2f);
                Vector3 doorPos = new Vector3(
                    doorX,
                    wallHeight / 2f,
                    roomZ * cellSize  // North wall of room (entrance from maze)
                );
                
                Quaternion doorRot = Quaternion.Euler(0f, 90f, 0f);  // Facing south (into room)
                
                // Create saloon-style double-sided door
                CreateSaloonDoor(doorPos, doorRot, i + 1, isEntrance: true);
            }
            
            // Create FINAL EXIT door (south wall of room - simple single door)
            Vector3 exitPos = new Vector3(
                roomX * cellSize + cellSize,  // Center of room
                wallHeight / 2f,
                roomZ * cellSize + cellSize * 2f  // South wall of room
            );
            
            Quaternion exitRot = Quaternion.Euler(0f, 90f, 0f);  // Facing north (into room)
            
            CreateSimpleExitDoor(exitPos, exitRot);
            
            Debug.Log("[CompleteMazeBuilder] ✅ Special exit room spawned!");
            Debug.Log("[CompleteMazeBuilder] 🚪 3 saloon doors at entrance (from maze)");
            Debug.Log("[CompleteMazeBuilder] 🚪 1 simple door at exit (to outside)");
        }
        
        /// <summary>
        /// Create a SALOON-STYLE DOUBLE-SIDED DOOR.
        /// 
        /// FEATURES:
        /// - Two door panels (left and right)
        /// - Double-sided (visible from both sides)
        /// - Flip/flap style (swings both ways)
        /// - Saloon-style (classic western swing doors)
        /// - Auto-swing when player approaches
        /// 
        /// Used for ENTRANCE to exit room (from maze).
        /// </summary>
        private void CreateSaloonDoor(Vector3 position, Quaternion rotation, int doorNumber, bool isEntrance = false)
        {
            // Create door frame
            GameObject doorFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorFrame.name = $"SaloonDoor_{doorNumber}_Frame";
            doorFrame.transform.position = position;
            doorFrame.transform.rotation = rotation;
            doorFrame.transform.localScale = new Vector3(0.1f, wallHeight, cellSize * 0.8f);
            
            // Left door panel (swings left)
            GameObject leftPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftPanel.name = $"SaloonDoor_{doorNumber}_Left";
            leftPanel.transform.parent = doorFrame.transform;
            leftPanel.transform.localPosition = new Vector3(-cellSize * 0.25f, 0f, 0f);
            leftPanel.transform.localScale = new Vector3(0.05f, wallHeight * 0.9f, cellSize * 0.35f);
            ApplyMaterial(leftPanel, doorMaterialPath);

            // Add DoorsEngine to left panel
            var leftDoorsEngine = leftPanel.AddComponent<DoorsEngine>();
            leftDoorsEngine.Initialize(DoorVariant.Normal, DoorTrapType.None, locked: false);
            // Note: auto-swing behavior would require additional physics/animation setup

            // Right door panel (swings right)
            GameObject rightPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightPanel.name = $"SaloonDoor_{doorNumber}_Right";
            rightPanel.transform.parent = doorFrame.transform;
            rightPanel.transform.localPosition = new Vector3(cellSize * 0.25f, 0f, 0f);
            rightPanel.transform.localScale = new Vector3(0.05f, wallHeight * 0.9f, cellSize * 0.35f);
            ApplyMaterial(rightPanel, doorMaterialPath);

            // Add DoorsEngine to right panel
            var rightDoorsEngine = rightPanel.AddComponent<DoorsEngine>();
            rightDoorsEngine.Initialize(DoorVariant.Normal, DoorTrapType.None, locked: false);
            // Note: auto-swing behavior would require additional physics/animation setup

            string location = isEntrance ? "entrance" : "exit";
            Debug.Log($"[CompleteMazeBuilder] 🚪 Saloon door {doorNumber} created at {location} (double-sided, swing both ways)");
        }
        
        /// <summary>
        /// Create a SIMPLE SINGLE DOOR for final exit.
        /// 
        /// FEATURES:
        /// - Single door panel
        /// - Accessible from both sides
        /// - Manual operation (press E)
        /// - Unlocked (player can exit)
        /// 
        /// Used for FINAL EXIT (south wall of exit room).
        /// </summary>
        private void CreateSimpleExitDoor(Vector3 position, Quaternion rotation)
        {
            GameObject exitDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            exitDoor.name = "FinalExitDoor";
            exitDoor.transform.position = position;
            exitDoor.transform.rotation = rotation;
            exitDoor.transform.localScale = new Vector3(wallThickness * 2f, wallHeight, cellSize * 1.5f);

            // Apply door material
            ApplyMaterial(exitDoor, doorMaterialPath);

            // Add DoorsEngine component
            var doorsEngine = exitDoor.AddComponent<DoorsEngine>();
            doorsEngine.Initialize(DoorVariant.Normal, DoorTrapType.None, locked: false);
            // Note: interactionRange uses default (3f) from BehaviorEngine

            Debug.Log($"[CompleteMazeBuilder] ✅ Final exit door placed at {position}");
            Debug.Log($"[CompleteMazeBuilder] 🔓 Door is UNLOCKED - player can exit to outside world!");
        }
        */
        
        // ============================================================================
        // [OBSOLETE] Using PlaceRoomsInGrid() instead - rooms placed in virtual grid BEFORE corridors
        // private void SpawnRoomsInExistingMaze() { ... }
        // private void SpawnRooms() { ... }
        
        // [OBSOLETE] Special exit room for later implementation  
        // private void SpawnMechanicalExitDoor() { ... }
        // private void SpawnSpecialExitRoom() { ... }

        #endregion

        #region Utility Methods
        
        /// <summary>
        /// Mark 3x3 room cells as CLEAR (no walls) so corridors connect properly.
        /// </summary>
        private void MarkRoomCellsClear(MazeGenerator.Wall[,] grid, Vector2Int roomPos)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            // Clear all walls in 3x3 room area
            for (int dx = 0; dx < 3 && roomPos.x + dx < width; dx++)
            {
                for (int dy = 0; dy < 3 && roomPos.y + dy < height; dy++)
                {
                    var oldWall = grid[roomPos.x + dx, roomPos.y + dy];
                    grid[roomPos.x + dx, roomPos.y + dy] = MazeGenerator.Wall.None;
                    Debug.Log($"[CompleteMazeBuilder] 🔓 Cleared cell ({roomPos.x + dx}, {roomPos.y + dy}): {oldWall} → None");
                }
            }

            Debug.Log($"[CompleteMazeBuilder] 🏛️ Room at ({roomPos.x}, {roomPos.y}) marked as CLEAR (3x3 area, no walls inside)");
        }
        
        /// <summary>
        /// Mark room door positions in grid so corridors connect properly.
        /// Rooms have doors on EAST (entrance) and WEST (exit) sides.
        /// REMOVES wall flags from adjacent cells to create door openings.
        /// </summary>
        private void MarkRoomDoors(MazeGenerator.Wall[,] grid, Vector2Int roomPos)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            // Room is 3x3 cells, centered at (roomPos.x+1, roomPos.y+1)
            // East door (entrance from maze) - remove EAST wall from room cell
            int roomEastCellX = roomPos.x + 2;  // East edge of room (last cell inside room)
            int roomEastCellZ = roomPos.y + 1;  // Center of 3-tall room
            if (roomEastCellX < width && roomEastCellZ < height)
            {
                // Remove EAST wall from room cell (creates doorway)
                grid[roomEastCellX, roomEastCellZ] &= ~MazeGenerator.Wall.East;
                Debug.Log($"[CompleteMazeBuilder] 🚪 Removed east wall at ({roomEastCellX}, {roomEastCellZ}) for door");
            }

            // West door (exit to maze) - remove WEST wall from room cell
            int roomWestCellX = roomPos.x + 1;  // West edge of room (first cell inside room)
            int roomWestCellZ = roomPos.y + 1;  // Center of 3-tall room
            if (roomWestCellX >= 0 && roomWestCellZ < height)
            {
                // Remove WEST wall from room cell (creates doorway)
                grid[roomWestCellX, roomWestCellZ] &= ~MazeGenerator.Wall.West;
                Debug.Log($"[CompleteMazeBuilder] 🚪 Removed west wall at ({roomWestCellX}, {roomWestCellZ}) for door");
            }
        }

        #endregion

        private void SpawnRooms()
        {
            int numRooms = Random.Range(minRooms, maxRooms + 1);
            Debug.Log($"[CompleteMazeBuilder] 🏛️ Generating {numRooms} rooms (each with 1 entrance + 1 exit)...");

            var gridField = mazeGenerator.GetType().GetProperty("Grid");
            var grid = gridField?.GetValue(mazeGenerator) as MazeGenerator.Wall[,];

            if (grid == null) return;

            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            // Spawn entrance room (start - top-left area) - PLAYER SPAWNS HERE
            Vector2Int entrancePos = FindValidRoomPosition(grid, 0, 0, width / 3, height / 2);
            if (entrancePos.x >= 0)
            {
                SpawnRoom(entrancePos, "Entrance", entranceRoomPrefabPath, hasEntrance: true, hasExit: true);
                // Store entrance room cell for player spawn (CENTER of 3x3 room, not corner!)
                // Room spans cells (entrancePos.x to entrancePos.x+2, entrancePos.y to entrancePos.y+2)
                // Center is at (entrancePos.x+1, entrancePos.y+1)
                entranceRoomCell = new Vector2Int(entrancePos.x + 1, entrancePos.y + 1);
                // Set player spawn position (center of entrance room)
                entranceRoomPosition = new Vector3(
                    entrancePos.x * cellSize + cellSize * 1.5f,
                    1f,
                    entrancePos.y * cellSize + cellSize * 1.5f
                );
                
                // ✅ SAVE TO PERSISTENT STORAGE (RAM/PlayerPrefs)
                SaveSpawnPosition(entranceRoomCell.x, entranceRoomCell.y, (int)currentSeed);
                Debug.Log($"[CompleteMazeBuilder] 💾 Spawn position saved to persistent storage: ({entranceRoomCell.x}, {entranceRoomCell.y})");
            }

            // Spawn exit room (end - bottom-right area)
            Vector2Int exitPos = FindValidRoomPosition(grid, width * 2 / 3, height / 2, width, height);
            if (exitPos.x >= 0)
            {
                SpawnRoom(exitPos, "Exit", exitRoomPrefabPath, hasEntrance: true, hasExit: true);
            }

            // Spawn normal rooms (each with 1 entrance + 1 exit)
            int roomsSpawned = 2;
            while (roomsSpawned < numRooms)
            {
                Vector2Int roomPos = FindValidRoomPosition(grid, 0, 0, width, height);
                if (roomPos.x >= 0)
                {
                    SpawnRoom(roomPos, "Normal", normalRoomPrefabPath, hasEntrance: true, hasExit: true);
                    roomsSpawned++;
                }
                else
                {
                    break;
                }
            }

            Debug.Log($"[CompleteMazeBuilder] 🏛️ Spawned {roomsSpawned} rooms (each with 1 entrance + 1 exit)");
        }

        private Vector2Int FindValidRoomPosition(MazeGenerator.Wall[,] grid, int minX, int minY, int maxX, int maxY)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            // Try to find valid position with more attempts
            for (int i = 0; i < 50; i++)  // Increased from 20 to 50
            {
                int x = Random.Range(minX, Mathf.Min(maxX, width - 3));
                int y = Random.Range(minY, Mathf.Min(maxY, height - 3));

                bool clear = true;
                for (int dx = 0; dx < 3 && clear; dx++)
                {
                    for (int dy = 0; dy < 3 && clear; dy++)
                    {
                        // Allow some walls - just need mostly clear space for room
                        var cell = grid[x + dx, y + dy];
                        if (cell != MazeGenerator.Wall.None && cell != MazeGenerator.Wall.North && cell != MazeGenerator.Wall.South && cell != MazeGenerator.Wall.East && cell != MazeGenerator.Wall.West)
                        {
                            // Cell has multiple walls - not clear enough
                            clear = false;
                        }
                    }
                }

                if (clear)
                {
                    return new Vector2Int(x, y);
                }
            }

            // Fallback: return a default position near entrance (top-left area)
            Debug.Log("[CompleteMazeBuilder] ⚠️ No perfect room position found - using default (1, 1)");
            return new Vector2Int(1, 1);
        }

        private void SpawnRoom(Vector2Int position, string roomType, string prefabPath, bool hasEntrance, bool hasExit)
        {
            // ALWAYS create procedural room (floor + ceiling + doors, NO walls!)
            // Walls will be spawned by SpawnWalls() with proper doorways
            GameObject room = new GameObject($"{roomType}Room_{position.x}_{position.y}");
            Vector3 roomPos = new Vector3(
                position.x * cellSize + cellSize * 1.5f,
                0f,
                position.y * cellSize + cellSize * 1.5f
            );
            room.transform.position = roomPos;

            // Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.parent = room.transform;
            floor.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            floor.transform.localScale = new Vector3(cellSize * 3f, 0.1f, cellSize * 3f);
            ApplyMaterial(floor, floorMaterialPath);
            ApplyTexture(floor, groundTexturePath);

            // Ceiling
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling";
            ceiling.transform.parent = room.transform;
            ceiling.transform.localPosition = new Vector3(0f, ceilingHeight, 0f);
            ceiling.transform.localScale = new Vector3(cellSize * 3f, 0.1f, cellSize * 3f);
            ApplyMaterial(ceiling, floorMaterialPath);
            ApplyTexture(ceiling, ceilingTexturePath);

            // DOORS at entrance/exit (NOT walls - doors!)
            if (hasEntrance)
            {
                // East side door (entrance from maze)
                Vector3 doorPos = new Vector3(cellSize * 1.5f, wallHeight / 2f, 0f);
                Quaternion doorRot = Quaternion.Euler(0f, 0f, 0f);
                SpawnDoor(doorPos, doorRot, position.x + 2, position.y + 1, "East", "Normal", doorPrefabPath, openByDefault: true);
            }
            
            if (hasExit)
            {
                // West side door (exit to maze)
                Vector3 doorPos = new Vector3(-cellSize * 1.5f, wallHeight / 2f, 0f);
                Quaternion doorRot = Quaternion.Euler(0f, 180f, 0f);
                SpawnDoor(doorPos, doorRot, position.x, position.y + 1, "West", "Normal", doorPrefabPath, openByDefault: true);
            }

            room.name = $"{roomType}Room_{position.x}_{position.y}";
            Debug.Log($"[CompleteMazeBuilder] 🏛️ {roomType} room spawned (floor + ceiling + doors, NO walls)");
        }

        private void CreateRoomWall(Transform parent, Vector3 localPos, Quaternion localRot, string name)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = $"Wall_{name}";
            wall.transform.parent = parent;
            wall.transform.localPosition = localPos;
            wall.transform.localRotation = localRot;
            wall.transform.localScale = new Vector3(cellSize * 3f, wallHeight, wallThickness);
            ApplyMaterial(wall, wallMaterialPath);
            ApplyTexture(wall, wallTexturePath);
        }

        private void CreateRoomWallWithGap(Transform parent, Vector3 localPos, Quaternion localRot, string name, bool hasGap)
        {
            if (hasGap)
            {
                GameObject wall1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall1.name = $"Wall_{name}_Left";
                wall1.transform.parent = parent;
                wall1.transform.localPosition = localPos + Vector3.right * (cellSize * 0.75f);
                wall1.transform.localRotation = localRot;
                wall1.transform.localScale = new Vector3(cellSize * 1.5f, wallHeight, wallThickness);
                ApplyMaterial(wall1, wallMaterialPath);
                ApplyTexture(wall1, wallTexturePath);

                GameObject wall2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall2.name = $"Wall_{name}_Right";
                wall2.transform.parent = parent;
                wall2.transform.localPosition = localPos - Vector3.right * (cellSize * 0.75f);
                wall2.transform.localRotation = localRot;
                wall2.transform.localScale = new Vector3(cellSize * 1.5f, wallHeight, wallThickness);
                ApplyMaterial(wall2, wallMaterialPath);
                ApplyTexture(wall2, wallTexturePath);
            }
            else
            {
                CreateRoomWall(parent, localPos, localRot, name);
            }
        }

        #region Player Spawn

        /// <summary>
        /// Save spawn position to database (SQLite - Saves/MazeDB.sqlite).
        /// Called AFTER maze generation with NEW seed.
        /// Procedurally persistent - changes with seed, persists across sessions.
        /// </summary>
        private void SaveSpawnPosition(int cellX, int cellZ, int seed)
        {
            // Save to SQLite database (Saves/ folder at project root)
            MazeSaveData.SaveMazeData(seed, cellX, cellZ, mazeWidth, mazeHeight);
            
            // Save room data
            var rooms = new List<RoomDataModel>
            {
                new RoomDataModel { X = cellX, Z = cellZ, Type = "Entrance" }
            };
            MazeSaveData.SaveRoomData(seed, rooms);
            
            // Save prefab assignments (Inspector values - NO HARDCODED DEFAULTS!)
            var prefabs = new Dictionary<string, string>
            {
                { "Wall", wallPrefabPath },
                { "Door", doorPrefabPath },
                { "EntranceRoom", entranceRoomPrefabPath },
                { "ExitRoom", exitRoomPrefabPath },
                { "NormalRoom", normalRoomPrefabPath }
            };
            MazeSaveData.SaveAllPrefabData(prefabs);
            
            // Save material/texture assignments
            var materials = new Dictionary<string, string>
            {
                { "Wall", wallMaterialPath },
                { "Door", doorMaterialPath },
                { "Floor", floorMaterialPath },
                { "GroundTexture", groundTexturePath },
                { "WallTexture", wallTexturePath },
                { "CeilingTexture", ceilingTexturePath }
            };
            MazeSaveData.SaveAllPrefabData(materials);
            
            Debug.Log($"[CompleteMazeBuilder] 💾 Maze saved to SQLite: Seed={seed}, Spawn=({cellX}, {cellZ})");
        }
        
        /// <summary>
        /// Load spawn position from database (SQLite - Saves/MazeDB.sqlite).
        /// Returns stored position if seed matches, otherwise forces regeneration.
        /// Called during loading screen.
        /// Loads prefab paths from SQLite (NO HARDCODED VALUES!).
        /// </summary>
        private Vector2Int LoadSpawnPosition()
        {
            var mazeData = MazeSaveData.LoadMazeData();
            
            if (mazeData == null)
            {
                Debug.Log("[CompleteMazeBuilder] 💾 No stored maze data - will generate NEW maze with NEW seed (FIRST TIME GAME)");
                Debug.Log("[CompleteMazeBuilder] 📦 Loading prefab paths from SQLite defaults...");
                LoadPrefabPathsFromSQLite();  // Load from SQLite (NOT hardcoded!)
                return new Vector2Int(-1, -1);  // Invalid, force new generation
            }
            
            if (mazeData.Seed != (int)currentSeed)
            {
                Debug.Log($"[CompleteMazeBuilder] 💾 Seed mismatch (stored: {mazeData.Seed}, current: {currentSeed}) - generating NEW maze");
                LoadPrefabPathsFromSQLite();  // Load from SQLite
                return new Vector2Int(-1, -1);  // Seed changed, force new generation
            }
            
            Debug.Log($"[CompleteMazeBuilder] 💾 Loaded maze data: Seed={mazeData.Seed}, Spawn=({mazeData.SpawnX}, {mazeData.SpawnZ})");
            return new Vector2Int(mazeData.SpawnX, mazeData.SpawnZ);
        }

        /// <summary>
        /// Clear stored spawn position and maze data (for cleanup).
        /// Used by editor tools to reset maze state.
        /// </summary>
        public void ClearSpawnPosition()
        {
            // Clear maze data from database
            MazeSaveData.ClearMazeData();
            
            // Reset local state
            entranceRoomCell = new Vector2Int(-1, -1);
            entranceRoomPosition = Vector3.zero;
            
            Debug.Log("[CompleteMazeBuilder] 🗑️ Spawn position and maze data cleared");
        }

        /// <summary>
        /// Load prefab/material paths from JSON config file.
        /// NO HARDCODED VALUES - all from Config/GameConfig.json!
        /// Supports modding (god-slayer mode, damage scale, etc.)
        /// </summary>
        private void LoadPrefabPathsFromSQLite()
        {
            // First try to load from SQLite (player's saved choices)
            var prefabs = MazeSaveData.LoadAllPrefabData();
            
            // Apply loaded prefab paths (empty if first-time)
            if (prefabs.ContainsKey("Wall") && !string.IsNullOrEmpty(prefabs["Wall"])) wallPrefabPath = prefabs["Wall"];
            if (prefabs.ContainsKey("Door") && !string.IsNullOrEmpty(prefabs["Door"])) doorPrefabPath = prefabs["Door"];
            if (prefabs.ContainsKey("EntranceRoom") && !string.IsNullOrEmpty(prefabs["EntranceRoom"])) entranceRoomPrefabPath = prefabs["EntranceRoom"];
            if (prefabs.ContainsKey("ExitRoom") && !string.IsNullOrEmpty(prefabs["ExitRoom"])) exitRoomPrefabPath = prefabs["ExitRoom"];
            if (prefabs.ContainsKey("NormalRoom") && !string.IsNullOrEmpty(prefabs["NormalRoom"])) normalRoomPrefabPath = prefabs["NormalRoom"];
            
            // If still empty (first-time game), load from JSON config
            if (string.IsNullOrEmpty(wallPrefabPath))
            {
                Debug.Log("[CompleteMazeBuilder] 📦 First-time game - loading defaults from JSON config...");
                LoadFromJSONConfig();
            }
            else
            {
                Debug.Log("[CompleteMazeBuilder] 📦 Loaded prefab paths from SQLite (player's choices)");
            }
        }
        
        /// <summary>
        /// Load default values from JSON config file (Config/GameConfig.json).
        /// Supports modding - edit JSON to change defaults!
        /// </summary>
        private void LoadFromJSONConfig()
        {
            var config = GameConfig.Instance;
            
            // Load prefab paths from JSON
            wallPrefabPath = config.wallPrefab;
            doorPrefabPath = config.doorPrefab;
            entranceRoomPrefabPath = config.entranceRoomPrefab;
            exitRoomPrefabPath = config.exitRoomPrefab;
            normalRoomPrefabPath = config.normalRoomPrefab;
            
            // Load material/texture paths from JSON
            wallMaterialPath = config.wallMaterial;
            doorMaterialPath = config.doorMaterial;
            floorMaterialPath = config.floorMaterial;
            groundTexturePath = config.groundTexture;
            wallTexturePath = config.wallTexture;
            ceilingTexturePath = config.ceilingTexture;
            
            // Load game balance settings (GOD-SLAYER MODE!)
            Debug.Log($"[CompleteMazeBuilder] 🎮 Config loaded: damageScale={config.damageScale}, godMode={config.godMode}, oneHitKill={config.oneHitKill}");
            
            if (config.godMode || config.oneHitKill || config.infiniteStamina)
            {
                Debug.Log("[CompleteMazeBuilder] 🔓 GOD MODE ENABLED - You are a god-slayer! ⚔️");
            }
            
            Debug.Log("[CompleteMazeBuilder] 📦 Loaded defaults from JSON config (Config/GameConfig.json)");
        }
        
        /// <summary>
        /// Clear stored maze data from database (force regeneration on next load).
        /// </summary>
        public void ClearMazeData()
        {
            MazeSaveData.ClearMazeData();
            Debug.Log("[CompleteMazeBuilder] 🗑️ Cleared maze data from database - will generate new maze");
        }

        /// <summary>
        /// Spawn player INSIDE entrance room - guaranteed safe spawn (no walls inside rooms!).
        /// Rooms are 3x3 cells with clear interior - perfect spawn zone.
        /// Uses persistent storage if available, otherwise finds room position.
        /// </summary>
        private void SpawnPlayer()
        {
            Debug.Log("[CompleteMazeBuilder] 👤 SpawnPlayer() called");
            
            // Destroy any existing Player objects first
            var existingPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            if (existingPlayers != null && existingPlayers.Length > 0)
            {
                Debug.Log($"[CompleteMazeBuilder] 👤 Found {existingPlayers.Length} existing Player(s), removing...");
                foreach (var oldPlayer in existingPlayers)
                {
                    if (Application.isPlaying)
                        Destroy(oldPlayer.gameObject);
                    else
                        DestroyImmediate(oldPlayer.gameObject);
                }
            }
            
            // Find or create player with FPS controller
            var player = FindFirstObjectByType<PlayerController>();
            Debug.Log($"[CompleteMazeBuilder] 👤 FindFirstObjectByType<PlayerController> returned: {(player == null ? "NULL" : "Found")}");
            
            if (player == null)
            {
                Debug.Log("[CompleteMazeBuilder] 👤 Creating new FPS player...");

                var playerGO = new GameObject("Player");
                Debug.Log($"[CompleteMazeBuilder] 👤 Created GameObject: {playerGO.name}");

                var controller = playerGO.AddComponent<CharacterController>();
                controller.radius = 0.4f;
                controller.height = 1.8f;
                controller.center = new Vector3(0f, 0.9f, 0f);
                Debug.Log("[CompleteMazeBuilder] 👤 Added CharacterController");

                player = playerGO.AddComponent<PlayerController>();
                Debug.Log("[CompleteMazeBuilder] 👤 Added PlayerController");

                // CRITICAL: Create and parent MainCamera for FPS view
                var mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    // No main camera exists - create one
                    var cameraGO = new GameObject("MainCamera");
                    mainCamera = cameraGO.AddComponent<Camera>();
                    cameraGO.tag = "MainCamera";
                    
                    // Set camera as child of player (FPS-style)
                    cameraGO.transform.SetParent(playerGO.transform);
                    cameraGO.transform.localPosition = new Vector3(0f, 1.75f, 0f); // Eye height
                    cameraGO.transform.localRotation = Quaternion.identity;
                    
                    Debug.Log("[CompleteMazeBuilder] 👁️ Created and parented MainCamera (FPS view)");
                }
                else
                {
                    // Main camera exists - parent it to player
                    mainCamera.transform.SetParent(playerGO.transform);
                    mainCamera.transform.localPosition = new Vector3(0f, 1.75f, 0f); // Eye height
                    mainCamera.transform.localRotation = Quaternion.identity;
                    
                    Debug.Log("[CompleteMazeBuilder] 👁️ Parented existing MainCamera to player (FPS view)");
                }

                Debug.Log("[CompleteMazeBuilder] 👤 FPS player created successfully");
            }
            
            // Use pre-calculated entrance room position (EXACT center of 5x5 room)
            Vector3 spawnPos;
            
            if (entranceRoomPosition != Vector3.zero)
            {
                spawnPos = entranceRoomPosition;
                Debug.Log($"[CompleteMazeBuilder] 🏛️ Using entrance room center: {spawnPos}");
            }
            else
            {
                // Fallback (shouldn't happen)
                spawnPos = new Vector3(4 * cellSize + cellSize / 2f, 0.9f, 4 * cellSize + cellSize / 2f);
                Debug.Log($"[CompleteMazeBuilder] ⚠️ Using fallback spawn: {spawnPos}");
            }
            
            // Teleport player to spawn position
            if (player != null)
            {
                player.transform.position = spawnPos;
                Debug.Log($"[CompleteMazeBuilder] 👤 Player spawned INSIDE entrance room: {spawnPos}");
                Debug.Log("[CompleteMazeBuilder] 👤 Room interior is CLEAR - no walls!");
                Debug.Log("[CompleteMazeBuilder] 👤 Press WASD to explore the maze!");
                Debug.Log("[CompleteMazeBuilder] 👤 Camera at eye height (1.75m) - FPS view ready");
            }
            else
            {
                Debug.LogError("[CompleteMazeBuilder] ❌ Player is NULL - cannot set spawn position!");
            }
        }
        
        /// <summary>
        /// Get world position for a specific maze cell.
        /// Used for spawning player INSIDE entrance room (at actual room position).
        /// Room interiors are CLEAR (no walls) - safe spawn zone!
        /// </summary>
        private Vector3 GetSpawnPositionFromMaze(int cellX, int cellZ)
        {
            // Calculate world position from cell coordinates
            // Cell center = cellIndex * cellSize + half cell
            float spawnX = cellX * cellSize + cellSize / 2f;
            float spawnY = 0.9f;  // CharacterController center - puts feet ON ground
            float spawnZ = cellZ * cellSize + cellSize / 2f;
            
            Debug.Log($"[CompleteMazeBuilder] 🎯 Spawn cell ({cellX}, {cellZ}) → World position: ({spawnX}, {spawnY}, {spawnZ})");
            Debug.Log($"[CompleteMazeBuilder] 🏰 Room interior is CLEAR - no walls inside!");
            
            // ✅ Add small random offset to ensure not clipping into corner walls
            float offsetX = (UnityEngine.Random.value - 0.5f) * 1f;  // ±0.5m
            float offsetZ = (UnityEngine.Random.value - 0.5f) * 1f;  // ±0.5m
            
            spawnX += offsetX;
            spawnZ += offsetZ;
            
            Debug.Log($"[CompleteMazeBuilder] 🎲 Added offset: ({offsetX}, {offsetZ}) → Final: ({spawnX}, {spawnY}, {spawnZ})");
            
            return new Vector3(spawnX, spawnY, spawnZ);
        }

        #endregion

        #region Object Placement

        private void PlaceObjects()
        {
            // Step 8a: Place torches using SpatialPlacer
            PlaceTorchesSimple();

            // Step 8b: Place other objects (chests, enemies, items) using SpatialPlacer
            spatialPlacer = GetComponent<SpatialPlacer>();
            if (spatialPlacer != null)
            {
                // Disable torch placement in SpatialPlacer (already done above)
                spatialPlacer.PlaceTorchesEnabled = false;
                spatialPlacer.PlaceAll();
                Debug.Log("[CompleteMazeBuilder] ✅ Objects placed (torches, chests, enemies, items)");
            }
            else
            {
                Debug.LogWarning("[CompleteMazeBuilder] ⚠️ SpatialPlacer not found");
            }
        }

        private void PlaceTorchesSimple()
        {
            // Use SpatialPlacer directly for torch placement
            spatialPlacer = GetComponent<SpatialPlacer>();
            if (spatialPlacer == null)
            {
                Debug.LogWarning("[CompleteMazeBuilder] ⚠️ SpatialPlacer not found, skipping torches");
                return;
            }

            // Ensure TorchPool is assigned
            if (torchPool == null)
            {
                torchPool = FindFirstObjectByType<TorchPool>();
            }
            
            // Wire up TorchPool to SpatialPlacer
            if (torchPool != null)
            {
                var torchPoolField = typeof(SpatialPlacer).GetField("torchPool", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (torchPoolField != null)
                {
                    torchPoolField.SetValue(spatialPlacer, torchPool);
                }
            }
            
            // Ensure MazeRenderer is available
            var mazeRenderer = GetComponent<MazeRenderer>();
            
            // Call PlaceTorches directly (SpatialPlacer will use TorchPool)
            spatialPlacer.PlaceTorchesEnabled = true;
            spatialPlacer.PlaceTorches();
            
            Debug.Log("[CompleteMazeBuilder] 🎆 Torches placed via SpatialPlacer + TorchPool");
        }

        #endregion

        #region Helper Methods

        private T GetOrAddComponent<T>() where T : Component
        {
            T component = GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
                Debug.Log($"[CompleteMazeBuilder] ✅ Added component: {typeof(T).Name}");
            }
            return component;
        }

        private GameObject LoadPrefab(string relativePath)
        {
            string fullPath = "Assets/" + relativePath;

            // Try Resources folder
            string resourcesPath = relativePath.Replace("Resources/", "");
            if (relativePath.StartsWith("Resources/"))
            {
                GameObject prefab = Resources.Load<GameObject>(resourcesPath.Replace(".prefab", ""));
                if (prefab != null) return prefab;
            }

            // Load via AssetDatabase (editor only)
            #if UNITY_EDITOR
            UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fullPath);
            if (obj is GameObject)
            {
                return obj as GameObject;
            }
            #endif

            Debug.LogWarning($"[CompleteMazeBuilder] ⚠️ Prefab not found: {fullPath}");
            return null;
        }

        private void ApplyMaterial(GameObject obj, string materialPath)
        {
            string fullPath = "Assets/" + materialPath;
            Material mat = null;

            #if UNITY_EDITOR
            mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(fullPath);
            #endif

            if (mat != null)
            {
                var renderer = obj.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.material = mat;
                }
            }
        }

        private void ApplyTexture(GameObject obj, string texturePath)
        {
            string fullPath = "Assets/" + texturePath;
            Texture2D tex = null;

            #if UNITY_EDITOR
            tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
            #endif

            if (tex != null)
            {
                var renderer = obj.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.material.mainTexture = tex;
                }
            }
        }

        private bool ValidatePath(string fullPath, string name)
        {
            if (System.IO.File.Exists(fullPath))
            {
                Debug.Log($"[CompleteMazeBuilder] ✅ {name}: {fullPath}");
                return true;
            }
            else
            {
                Debug.LogError($"[CompleteMazeBuilder] ❌ {name} NOT FOUND: {fullPath}");
                return false;
            }
        }

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
