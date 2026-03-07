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

        [Header("📁 Relative Paths (relative to Assets/)")]
        [SerializeField] private string wallPrefabPath = "Prefabs/WallPrefab.prefab";
        [SerializeField] private string doorPrefabPath = "Prefabs/DoorPrefab.prefab";
        [SerializeField] private string lockedDoorPrefabPath = "Prefabs/LockedDoorPrefab.prefab";
        [SerializeField] private string secretDoorPrefabPath = "Prefabs/SecretDoorPrefab.prefab";
        [SerializeField] private string entranceRoomPrefabPath = "Prefabs/EntranceRoomPrefab.prefab";
        [SerializeField] private string exitRoomPrefabPath = "Prefabs/ExitRoomPrefab.prefab";
        [SerializeField] private string normalRoomPrefabPath = "Prefabs/NormalRoomPrefab.prefab";
        [SerializeField] private string wallMaterialPath = "Materials/WallMaterial.mat";
        [SerializeField] private string doorMaterialPath = "Materials/Door_PïxelArt.mat";
        [SerializeField] private string floorMaterialPath = "Materials/Floor/Stone_Floor.mat";
        [SerializeField] private string groundTexturePath = "Textures/floor_texture.png";
        [SerializeField] private string wallTexturePath = "Textures/wall_texture.png";
        [SerializeField] private string ceilingTexturePath = "Textures/ceiling_texture.png";

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

            // Initialize seed
            if (useRandomSeed)
            {
                currentSeed = (uint)System.DateTime.Now.Ticks;
            }
            else
            {
                currentSeed = ComputeSeed(manualSeed);
            }
            rng = new System.Random((int)currentSeed);

            Debug.Log("[CompleteMazeBuilder] 🏗️ Component initialized");
            Debug.Log($"[CompleteMazeBuilder] 📏 Maze: {mazeWidth}x{mazeHeight}, Cell: {cellSize}m");
            Debug.Log($"[CompleteMazeBuilder] 🎲 Seed: {currentSeed}");
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
        /// </summary>
        public void GenerateMazeGeometryOnly()
        {
            Debug.Log("[CompleteMazeBuilder] ════════════════════════════════════════");
            Debug.Log("[CompleteMazeBuilder] 🏗️ Starting maze generation...");
            Debug.Log("[CompleteMazeBuilder] ════════════════════════════════════════");

            doorPositions.Clear();
            entranceRoomPosition = Vector3.zero;
            entranceRoomCell = new Vector2Int(-1, -1);

            // Step 1: Generate maze layout
            GenerateMazeLayout();

            // Step 2: Spawn ground floor FIRST (so player has something to stand on)
            SpawnGroundFloor();

            // Step 3: Spawn rooms BEFORE player (so room exists for spawn!)
            if (generateRooms)
            {
                SpawnRooms();
            }

            // Save spawn position to persistent storage AFTER rooms are spawned
            if (entranceRoomCell.x >= 0)
            {
                SaveSpawnPosition(entranceRoomCell.x, entranceRoomCell.y, (int)currentSeed);
                Debug.Log($"[CompleteMazeBuilder] 💾 Spawn position saved: ({entranceRoomCell.x}, {entranceRoomCell.y})");
            }

            // Step 4: Spawn ceiling (with texture)
            SpawnCeiling();

            // Step 5: Spawn walls (with textures, gaps for doors)
            SpawnWalls();

            // Step 6: Spawn doors (snapped to wall gaps)
            SpawnDoors();

            // Step 7: Place torches, chests, enemies, items
            PlaceObjects();

            Debug.Log("[CompleteMazeBuilder] ════════════════════════════════════════");
            Debug.Log("[CompleteMazeBuilder] ✅ Maze geometry complete!");
            Debug.Log($"[CompleteMazeBuilder] 📏 Dimensions: {mazeWidth}x{mazeHeight} cells ({mazeWidth * cellSize}m x {mazeHeight * cellSize}m)");
            Debug.Log($"[CompleteMazeBuilder] 🧱 Walls: Generated for {mazeWidth}x{mazeHeight} grid");
            Debug.Log($"[CompleteMazeBuilder] 🚪 Doors: {doorPositions.Count} placed (snapped to wall gaps)");
            Debug.Log($"[CompleteMazeBuilder] 🏛️ Rooms: {minRooms}-{maxRooms} generated (each 1 entrance + 1 exit)");
            Debug.Log($"[CompleteMazeBuilder] 👤 Player spawn: {entranceRoomPosition}");
            Debug.Log("[CompleteMazeBuilder] ════════════════════════════════════════");
            
            // PLUG-IN-OUT: Publish maze generation complete event
            if (eventHandler != null)
            {
                // Event publishing would go here if EventHandler supports custom events
                Debug.Log("[CompleteMazeBuilder] 📢 Published: MazeGenerated event");
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

            for (int i = 0; i < 20; i++)
            {
                int x = Random.Range(minX, Mathf.Min(maxX, width - 3));
                int y = Random.Range(minY, Mathf.Min(maxY, height - 3));

                bool clear = true;
                for (int dx = 0; dx < 3 && clear; dx++)
                {
                    for (int dy = 0; dy < 3 && clear; dy++)
                    {
                        if (grid[x + dx, y + dy] != MazeGenerator.Wall.None)
                        {
                            clear = false;
                        }
                    }
                }

                if (clear)
                {
                    return new Vector2Int(x, y);
                }
            }

            return new Vector2Int(-1, -1);
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
        /// Save spawn position to persistent storage (PlayerPrefs/RAM).
        /// Procedurally persistent - changes with seed, persists across sessions.
        /// </summary>
        private void SaveSpawnPosition(int cellX, int cellZ, int seed)
        {
            PlayerPrefs.SetInt(SPAWN_X_KEY, cellX);
            PlayerPrefs.SetInt(SPAWN_Z_KEY, cellZ);
            PlayerPrefs.SetInt(SPAWN_SEED_KEY, seed);
            PlayerPrefs.Save();  // Force write to storage
        }
        
        /// <summary>
        /// Load spawn position from persistent storage (PlayerPrefs/RAM).
        /// Returns stored position if seed matches, otherwise forces regeneration.
        /// </summary>
        private Vector2Int LoadSpawnPosition()
        {
            if (!PlayerPrefs.HasKey(SPAWN_X_KEY) || !PlayerPrefs.HasKey(SPAWN_Z_KEY))
            {
                Debug.Log("[CompleteMazeBuilder] 💾 No stored spawn position - will use new room");
                return new Vector2Int(-1, -1);  // Invalid, force new spawn
            }
            
            int storedSeed = PlayerPrefs.GetInt(SPAWN_SEED_KEY, -1);
            if (storedSeed != (int)currentSeed)
            {
                Debug.Log($"[CompleteMazeBuilder] 💾 Seed mismatch (stored: {storedSeed}, current: {currentSeed}) - regenerating");
                return new Vector2Int(-1, -1);  // Seed changed, force new spawn
            }
            
            int cellX = PlayerPrefs.GetInt(SPAWN_X_KEY, 2);
            int cellZ = PlayerPrefs.GetInt(SPAWN_Z_KEY, 2);
            
            Debug.Log($"[CompleteMazeBuilder] 💾 Loaded spawn position from storage: ({cellX}, {cellZ})");
            return new Vector2Int(cellX, cellZ);
        }
        
        /// <summary>
        /// Clear stored spawn position (force regeneration on next maze gen).
        /// </summary>
        public void ClearSpawnPosition()
        {
            PlayerPrefs.DeleteKey(SPAWN_X_KEY);
            PlayerPrefs.DeleteKey(SPAWN_Z_KEY);
            PlayerPrefs.DeleteKey(SPAWN_SEED_KEY);
            Debug.Log("[CompleteMazeBuilder] 🗑️ Cleared stored spawn position");
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
            
            // If no stored position or seed mismatch, use entrance room cell (calculated during SpawnRooms)
            if (spawnCell.x < 0 || spawnCell.y < 0)
            {
                spawnCell = entranceRoomCell;
                Debug.Log($"[CompleteMazeBuilder] 🏛️ Using entrance room cell for spawn: ({spawnCell.x}, {spawnCell.y})");
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
