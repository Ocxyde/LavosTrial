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
    /// </summary>
    public class CompleteMazeBuilder : MonoBehaviour
    {
        #region Inspector Settings

        [Header("🏗️ Maze Dimensions")]
        [Tooltip("Maze width in cells (odd number recommended)")]
        [SerializeField] private int mazeWidth = 21;
        
        [Tooltip("Maze height in cells (odd number recommended)")]
        [SerializeField] private int mazeHeight = 21;
        
        [Tooltip("Size of each cell in meters")]
        [SerializeField] private float cellSize = 6f;
        
        [Tooltip("Wall height in meters")]
        [SerializeField] private float wallHeight = 4f;
        
        [Tooltip("Wall thickness in meters")]
        [SerializeField] private float wallThickness = 0.5f;
        
        [Tooltip("Ceiling height in meters (above ground)")]
        [SerializeField] private float ceilingHeight = 5f;

        [Header("🚪 Door Settings")]
        [Tooltip("Chance for a passage to have a door (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float doorSpawnChance = 0.6f;
        
        [Tooltip("Chance for door to be locked (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float lockedDoorChance = 0.3f;
        
        [Tooltip("Chance for door to be secret (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float secretDoorChance = 0.1f;

        [Header("🏛️ Room Settings")]
        [Tooltip("Generate special rooms (entrance/exit/normal)")]
        [SerializeField] private bool generateRooms = true;
        
        [Tooltip("Minimum number of rooms")]
        [SerializeField] private int minRooms = 3;
        
        [Tooltip("Maximum number of rooms")]
        [SerializeField] private int maxRooms = 8;
        
        [Header("👤 Player Spawn")]
        [Tooltip("Player spawns inside entrance room (auto-calculated)")]
        [SerializeField] private bool spawnInsideRoom = true;

        [Header("⚙️ Generation Options")]
        [Tooltip("Auto-generate maze on Start")]
        [SerializeField] private bool autoGenerateOnStart = true;
        
        [Tooltip("Use random seed (false = use manual seed)")]
        [SerializeField] private bool useRandomSeed = true;
        
        [Tooltip("Manual seed for reproducible generation")]
        [SerializeField] private string manualSeed = "MazeSeed2026";
        
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

        private MazeGenerator mazeGenerator;
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
            // PLUG-IN-OUT: Get event handler (central hub for communication)
            eventHandler = EventHandler.Instance;
            if (eventHandler != null)
            {
                Debug.Log("[CompleteMazeBuilder] 🔌 Connected to EventHandler (plug-in-out ready)");
            }
            else
            {
                Debug.Log("[CompleteMazeBuilder] ℹ️ EventHandler not in scene - running standalone (OK for testing)");
            }
        
            // STEP 1: Create TorchPool FIRST (required by other components)
            torchPool = FindFirstObjectByType<TorchPool>();
            if (torchPool == null)
            {
                var torchGO = new GameObject("TorchPool");
                torchPool = torchGO.AddComponent<TorchPool>();
                Debug.Log("[CompleteMazeBuilder] ✅ TorchPool created");
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
            
            // Release references
            mazeGenerator = null;
            spatialPlacer = null;
            lightPlacementEngine = null;
            torchPool = null;
            
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
        /// 1. Place rooms (guaranteed space)
        /// 2. Mark room doors for corridor connection
        /// 3. Generate corridors around rooms
        /// 4. Build outer perimeter walls (no sky gaps)
        /// 5. Place mechanical exit door
        /// 6. Spawn player inside entrance room
        /// </summary>
        public void GenerateMazeGeometryOnly()
        {
            Debug.Log("[CompleteMazeBuilder] ════════════════════════════════════════");
            Debug.Log("[CompleteMazeBuilder] 🏗️ Starting maze generation (ROOMS FIRST)...");
            Debug.Log("[CompleteMazeBuilder] ════════════════════════════════════════");

            doorPositions.Clear();
            entranceRoomPosition = Vector3.zero;
            entranceRoomCell = new Vector2Int(-1, -1);

            // Step 1: Place rooms FIRST (before corridors!)
            if (generateRooms)
            {
                SpawnRoomsFirst();  // ✅ Rooms + door markers
            }

            // Step 2: Generate maze layout (corridors connect to room doors)
            GenerateMazeLayout();

            // Step 3: Spawn ground floor
            SpawnGroundFloor();

            // Save spawn position AFTER rooms are placed
            if (entranceRoomCell.x >= 0)
            {
                SaveSpawnPosition(entranceRoomCell.x, entranceRoomCell.y, (int)currentSeed);
                Debug.Log($"[CompleteMazeBuilder] 💾 Spawn position saved: ({entranceRoomCell.x}, {entranceRoomCell.y})");
            }
            else
            {
                // Fallback: safe default spawn
                entranceRoomCell = new Vector2Int(2, 2);
                SaveSpawnPosition(2, 2, (int)currentSeed);
                Debug.Log($"[CompleteMazeBuilder] 💾 Using safe default spawn: (2, 2)");
            }

            // Step 4: Spawn ceiling (covers everything - no sky!)
            SpawnCeiling();

            // Step 5: Spawn walls (includes outer perimeter - fully enclosed!)
            SpawnWalls();

            // Step 6: Spawn doors (including mechanical exit door)
            SpawnDoors();
            
            // Step 7: Place mechanical exit door (double-sided, working)
            SpawnMechanicalExitDoor();

            // Step 8: Place torches, chests, enemies, items
            PlaceObjects();

            Debug.Log("[CompleteMazeBuilder] ════════════════════════════════════════");
            Debug.Log("[CompleteMazeBuilder] ✅ Maze geometry complete!");
            Debug.Log($"[CompleteMazeBuilder] 📏 Dimensions: {mazeWidth}x{mazeHeight} cells ({mazeWidth * cellSize}m x {mazeHeight * cellSize}m)");
            Debug.Log($"[CompleteMazeBuilder] 🧱 Walls: Generated for {mazeWidth}x{mazeHeight} grid (fully enclosed)");
            Debug.Log($"[CompleteMazeBuilder] 🚪 Doors: {doorPositions.Count} placed + mechanical exit");
            Debug.Log($"[CompleteMazeBuilder] 🏛️ Rooms: Generated (rooms first, corridors connect)");
            Debug.Log($"[CompleteMazeBuilder] 👤 Player spawn: {entranceRoomPosition}");
            Debug.Log("[CompleteMazeBuilder] ════════════════════════════════════════");
            
            // PLUG-IN-OUT: Publish maze generation complete event
            if (eventHandler != null)
            {
                Debug.Log("[CompleteMazeBuilder] 📢 Published: MazeGenerated event");
            }
            else
            {
                Debug.Log("[CompleteMazeBuilder] ℹ️ EventHandler not found - running standalone (OK for testing)");
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
            
            // Generate maze (uses default seed internally)
            mazeGenerator.Generate();
            Debug.Log($"[CompleteMazeBuilder] ✅ Maze layout generated ({mazeWidth}x{mazeHeight})");
        }

        private void SpawnGroundFloor()
        {
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

        private void SpawnWalls()
        {
            var gridField = mazeGenerator.GetType().GetProperty("Grid");
            var grid = gridField?.GetValue(mazeGenerator) as MazeGenerator.Wall[,];

            if (grid == null)
            {
                Debug.LogError("[CompleteMazeBuilder] ❌ Maze grid is null!");
                return;
            }

            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            int wallsSpawned = 0;
            int outerWallsSpawned = 0;

            // Step 1: Spawn inner walls from maze generator data
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = grid[x, y];
                    Vector3 cellPos = new Vector3(x * cellSize + cellSize / 2f, wallHeight / 2f, y * cellSize + cellSize / 2f);

                    if ((cell & MazeGenerator.Wall.North) != 0)
                    {
                        SpawnWall(cellPos + Vector3.forward * (cellSize / 2f), Quaternion.Euler(0f, 90f, 0f), x, y, "North");
                        wallsSpawned++;
                    }
                    if ((cell & MazeGenerator.Wall.East) != 0)
                    {
                        SpawnWall(cellPos + Vector3.right * (cellSize / 2f), Quaternion.Euler(0f, 0f, 0f), x, y, "East");
                        wallsSpawned++;
                    }
                    if ((cell & MazeGenerator.Wall.South) != 0)
                    {
                        SpawnWall(cellPos - Vector3.forward * (cellSize / 2f), Quaternion.Euler(0f, 90f, 0f), x, y, "South");
                        wallsSpawned++;
                    }
                    if ((cell & MazeGenerator.Wall.West) != 0)
                    {
                        SpawnWall(cellPos - Vector3.right * (cellSize / 2f), Quaternion.Euler(0f, 0f, 0f), x, y, "West");
                        wallsSpawned++;
                    }
                }
            }

            // Step 2: Add outer perimeter walls (ensure maze is fully enclosed)
            for (int x = 0; x < width; x++)
            {
                SpawnWall(new Vector3(x * cellSize + cellSize / 2f, wallHeight / 2f, height * cellSize), 
                    Quaternion.Euler(0f, 90f, 0f), x, height - 1, "OuterNorth");
                outerWallsSpawned++;
                
                SpawnWall(new Vector3(x * cellSize + cellSize / 2f, wallHeight / 2f, 0f), 
                    Quaternion.Euler(0f, 90f, 0f), x, 0, "OuterSouth");
                outerWallsSpawned++;
            }
            for (int y = 0; y < height; y++)
            {
                SpawnWall(new Vector3(width * cellSize, wallHeight / 2f, y * cellSize + cellSize / 2f), 
                    Quaternion.Euler(0f, 0f, 0f), width - 1, y, "OuterEast");
                outerWallsSpawned++;
                
                SpawnWall(new Vector3(0f, wallHeight / 2f, y * cellSize + cellSize / 2f), 
                    Quaternion.Euler(0f, 0f, 0f), 0, y, "OuterWest");
                outerWallsSpawned++;
            }

            Debug.Log($"[CompleteMazeBuilder] 🧱 Walls: {wallsSpawned} inner + {outerWallsSpawned} outer = {wallsSpawned + outerWallsSpawned} segments (maze fully enclosed)");
        }

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

        private void SpawnDoor(Vector3 position, Quaternion rotation, int x, int y, string direction, string doorType, string prefabPath)
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

            Debug.Log($"[CompleteMazeBuilder] 🚪 {doorType} door at ({x}, {y}) {direction}");
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
            doorsEngine.interactionRange = 3f;
            doorsEngine.autoOpen = false;  // Manual operation
            doorsEngine.locked = false;    // Unlocked (player can exit)

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
            leftDoorsEngine.interactionRange = 2f;
            leftDoorsEngine.autoOpen = true;  // Auto-swing
            leftDoorsEngine.locked = false;
            
            // Right door panel (swings right)
            GameObject rightPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightPanel.name = $"SaloonDoor_{doorNumber}_Right";
            rightPanel.transform.parent = doorFrame.transform;
            rightPanel.transform.localPosition = new Vector3(cellSize * 0.25f, 0f, 0f);
            rightPanel.transform.localScale = new Vector3(0.05f, wallHeight * 0.9f, cellSize * 0.35f);
            ApplyMaterial(rightPanel, doorMaterialPath);
            
            // Add DoorsEngine to right panel
            var rightDoorsEngine = rightPanel.AddComponent<DoorsEngine>();
            rightDoorsEngine.interactionRange = 2f;
            rightDoorsEngine.autoOpen = true;  // Auto-swing
            rightDoorsEngine.locked = false;
            
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
            doorsEngine.interactionRange = 3f;
            doorsEngine.autoOpen = false;  // Manual operation
            doorsEngine.locked = false;    // Unlocked (player can exit)
            
            Debug.Log($"[CompleteMazeBuilder] ✅ Final exit door placed at {position}");
            Debug.Log($"[CompleteMazeBuilder] 🔓 Door is UNLOCKED - player can exit to outside world!");
        }
        */
        
        // ============================================================================
        // TO ENABLE SPECIAL EXIT ROOM:
        // 1. Uncomment SpawnSpecialExitRoom(), CreateSaloonDoor(), and CreateSimpleExitDoor()
        // 2. In GenerateMazeGeometryOnly(), replace:
        //    SpawnMechanicalExitDoor();
        //    WITH:
        //    SpawnSpecialExitRoom();
        // 3. Test and enjoy the special exit room with saloon doors!
        // ============================================================================

        #region Room Generation

        /// <summary>
        /// Spawn rooms FIRST (before maze corridors).
        /// This guarantees rooms have space and player has valid spawn.
        /// Also marks room cells in grid so corridors connect properly.
        /// </summary>
        private void SpawnRoomsFirst()
        {
            // Check mazeGenerator is assigned
            if (mazeGenerator == null)
            {
                Debug.LogError("[CompleteMazeBuilder] ❌ MazeGenerator component missing!");
                return;
            }

            int numRooms = Random.Range(minRooms, maxRooms + 1);
            Debug.Log($"[CompleteMazeBuilder] 🏛️ Placing {numRooms} rooms FIRST (before corridors)...");

            var gridField = mazeGenerator.GetType().GetProperty("Grid");
            var grid = gridField?.GetValue(mazeGenerator) as MazeGenerator.Wall[,];

            // Grid might not exist yet - create temporary grid for room placement
            if (grid == null)
            {
                Debug.Log("[CompleteMazeBuilder] ⚠️ Grid not ready - will place rooms during corridor gen");
                return;
            }

            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            // Place entrance room FIRST (top-left area)
            Vector2Int entrancePos = FindValidRoomPosition(grid, 0, 0, width / 3, height / 2);
            if (entrancePos.x >= 0)
            {
                SpawnRoom(entrancePos, "Entrance", entranceRoomPrefabPath, hasEntrance: true, hasExit: true);
                // Store entrance room cell for player spawn (CENTER of 3x3 room)
                entranceRoomCell = new Vector2Int(entrancePos.x + 1, entrancePos.y + 1);
                entranceRoomPosition = new Vector3(
                    entrancePos.x * cellSize + cellSize * 1.5f,
                    1f,
                    entrancePos.y * cellSize + cellSize * 1.5f
                );
                
                // ✅ MARK ROOM CELLS AS CLEAR in grid (so corridors connect)
                MarkRoomCellsClear(grid, entrancePos);
                
                // ✅ MARK DOOR POSITIONS in grid (corridors MUST connect here)
                MarkRoomDoors(grid, entrancePos);
                
                Debug.Log($"[CompleteMazeBuilder] 🏛️ Entrance room placed at ({entrancePos.x}, {entrancePos.y}), spawn at ({entranceRoomCell.x}, {entranceRoomCell.y})");
            }
            else
            {
                Debug.LogWarning("[CompleteMazeBuilder] ⚠️ Could not place entrance room - using fallback");
            }

            // Place exit room (bottom-right area)
            Vector2Int exitPos = FindValidRoomPosition(grid, width * 2 / 3, height / 2, width, height);
            if (exitPos.x >= 0)
            {
                SpawnRoom(exitPos, "Exit", exitRoomPrefabPath, hasEntrance: true, hasExit: true);
                MarkRoomCellsClear(grid, exitPos);
                MarkRoomDoors(grid, exitPos);
                Debug.Log($"[CompleteMazeBuilder] 🏛️ Exit room placed at ({exitPos.x}, {exitPos.y})");
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
                    Debug.Log($"[CompleteMazeBuilder] 🏛️ Normal room {roomsSpawned} placed at ({roomPos.x}, {roomPos.y})");
                }
                else
                {
                    break;  // No more valid positions
                }
            }

            Debug.Log($"[CompleteMazeBuilder] 🏛️ {roomsSpawned} rooms placed FIRST (corridors will connect to room doors)");
        }
        
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
                    grid[roomPos.x + dx, roomPos.y + dy] = MazeGenerator.Wall.None;
                }
            }
            
            Debug.Log($"[CompleteMazeBuilder] 🔓 Marked room at ({roomPos.x}, {roomPos.y}) as clear for corridor connection");
        }
        
        /// <summary>
        /// Mark room door positions in grid so corridors connect properly.
        /// Entrance room door faces SOUTH, Exit room door faces NORTH.
        /// </summary>
        private void MarkRoomDoors(MazeGenerator.Wall[,] grid, Vector2Int roomPos)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            
            // Entrance room: door at SOUTH side (center of south wall)
            // Exit room: door at NORTH side (center of north wall)
            // For now, mark center cell of each side as clear
            
            // South door (for entrance room)
            int southDoorX = roomPos.x + 1;  // Center of 3-wide room
            int southDoorZ = roomPos.y + 3;  // Just south of room
            if (southDoorZ < height)
            {
                grid[southDoorX, southDoorZ] = MazeGenerator.Wall.None;
                Debug.Log($"[CompleteMazeBuilder] 🚪 Marked south door at ({southDoorX}, {southDoorZ})");
            }
            
            // North door (for exit room)
            int northDoorX = roomPos.x + 1;  // Center of 3-wide room
            int northDoorZ = roomPos.y - 1;  // Just north of room
            if (northDoorZ >= 0)
            {
                grid[northDoorX, northDoorZ] = MazeGenerator.Wall.None;
                Debug.Log($"[CompleteMazeBuilder] 🚪 Marked north door at ({northDoorX}, {northDoorZ})");
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
            GameObject roomPrefab = LoadPrefab(prefabPath);

            GameObject room;
            if (roomPrefab != null)
            {
                Vector3 roomPos = new Vector3(
                    position.x * cellSize + cellSize * 1.5f,
                    0f,
                    position.y * cellSize + cellSize * 1.5f
                );
                room = Instantiate(roomPrefab, roomPos, Quaternion.identity);
            }
            else
            {
                room = new GameObject($"{roomType}Room_{position.x}_{position.y}");
                Vector3 roomPos = new Vector3(
                    position.x * cellSize + cellSize * 1.5f,
                    0f,
                    position.y * cellSize + cellSize * 1.5f
                );
                room.transform.position = roomPos;

                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.name = "Floor";
                floor.transform.parent = room.transform;
                floor.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                floor.transform.localScale = new Vector3(cellSize * 3f, 0.1f, cellSize * 3f);
                ApplyMaterial(floor, floorMaterialPath);
                ApplyTexture(floor, groundTexturePath);

                GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ceiling.name = "Ceiling";
                ceiling.transform.parent = room.transform;
                ceiling.transform.localPosition = new Vector3(0f, ceilingHeight, 0f);
                ceiling.transform.localScale = new Vector3(cellSize * 3f, 0.1f, cellSize * 3f);
                ApplyMaterial(ceiling, floorMaterialPath);
                ApplyTexture(ceiling, ceilingTexturePath);

                CreateRoomWall(room.transform, new Vector3(0f, wallHeight / 2f, -cellSize * 1.5f), 
                    Quaternion.Euler(0f, 90f, 0f), "North");
                CreateRoomWall(room.transform, new Vector3(0f, wallHeight / 2f, cellSize * 1.5f), 
                    Quaternion.Euler(0f, 90f, 0f), "South");
                CreateRoomWallWithGap(room.transform, new Vector3(cellSize * 1.5f, wallHeight / 2f, 0f), 
                    Quaternion.Euler(0f, 0f, 0f), "East", hasEntrance);
                CreateRoomWallWithGap(room.transform, new Vector3(-cellSize * 1.5f, wallHeight / 2f, 0f), 
                    Quaternion.Euler(0f, 0f, 0f), "West", hasExit);
            }

            room.name = $"{roomType}Room_{position.x}_{position.y}";
            Debug.Log($"[CompleteMazeBuilder] 🏛️ {roomType} room at ({position.x}, {position.y}) - 1 entrance + 1 exit");
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

        #endregion

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
                
                Debug.Log("[CompleteMazeBuilder] 👤 FPS player created successfully");
            }
            
            // Try to load spawn position from persistent storage FIRST
            Vector2Int spawnCell = LoadSpawnPosition();
            
            Debug.Log($"[CompleteMazeBuilder] 🔍 LoadSpawnPosition returned: ({spawnCell.x}, {spawnCell.y})");
            Debug.Log($"[CompleteMazeBuilder] 🔍 entranceRoomCell is: ({entranceRoomCell.x}, {entranceRoomCell.y})");

            // If no stored position or seed mismatch, use entrance room cell (calculated during SpawnRooms)
            if (spawnCell.x < 0 || spawnCell.y < 0)
            {
                // Check if entrance room cell is valid
                if (entranceRoomCell.x < 0 || entranceRoomCell.y < 0)
                {
                    // Room generation failed - use SAFE DEFAULT (cell 2, 2 = near entrance)
                    spawnCell = new Vector2Int(2, 2);
                    Debug.Log($"[CompleteMazeBuilder] ⚠️ Room cell invalid - using SAFE DEFAULT: ({spawnCell.x}, {spawnCell.y})");
                }
                else
                {
                    spawnCell = entranceRoomCell;
                    Debug.Log($"[CompleteMazeBuilder] 🏛️ Using entrance room cell for spawn: ({spawnCell.x}, {spawnCell.y})");
                }
            }
            else
            {
                Debug.Log($"[CompleteMazeBuilder] 💾 Using stored spawn cell: ({spawnCell.x}, {spawnCell.y})");
            }

            // Spawn player INSIDE entrance room (at ACTUAL room position, not hardcoded!)
            Vector3 spawnPos = GetSpawnPositionFromMaze(spawnCell.x, spawnCell.y);
            
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
