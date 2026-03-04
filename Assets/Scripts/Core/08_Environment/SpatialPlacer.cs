// SpatialPlacer.cs
// Universal procedural placement engine for all spawnable objects
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Central placement system for maze objects
// - Chests, doors, items
// - Enemies, allies, creatures
// - Special rooms with procedural effects
// - All with optional seed-based generation
//
// Location: Assets/Scripts/Core/08_Environment/

using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.Lavos.Core
{
    /// <summary>
    /// SpatialPlacer - Universal procedural placement engine.
    /// 
    /// Places all spawnable objects in the maze:
    /// - Special rooms (with unique procedural effects)
    /// - Chests (treasure, traps)
    /// - Doors (when enabled)
    /// - Enemies & Allies
    /// - Items & Pickups
    /// - Hidden creatures
    /// 
    /// Features:
    /// - Seed-based procedural generation
    /// - Room-specific atmospheric effects
    /// - Weighted random placement
    /// - Collision avoidance
    /// - Distance constraints
    /// </summary>
    public class SpatialPlacer : MonoBehaviour
    {
        [Header("Generation Settings")]
        [Tooltip("Random seed for procedural generation (0 = random each time)")]
        [SerializeField] private int generationSeed = 0;
        
        [Tooltip("Use seed for reproducible generation")]
        [SerializeField] private bool useSeed = false;
        
        [Tooltip("Maze reference for placement bounds")]
        [SerializeField] private MazeGenerator mazeGenerator;
        
        [Tooltip("Wait for maze generation to complete before placing")]
        [SerializeField] private bool waitForMazeGeneration = true;
        
        [Tooltip("Cell margin from walls (0 = center, 0.4 = 40% from edges)")]
        [Range(0f, 0.45f)]
        [SerializeField] private float cellMargin = 0.2f;

        [Header("Special Rooms")]
        [Tooltip("Enable special room placement")]
        [SerializeField] private bool placeSpecialRooms = true;
        
        [SerializeField] private int specialRoomCount = 3;
        [SerializeField] private float minDistanceBetweenRooms = 20f;
        [SerializeField] private bool preferEdgePlacement = true;
        
        [Header("Room Effects (Procedural)")]
        [Tooltip("Generate room effects procedurally")]
        [SerializeField] private bool proceduralRoomEffects = true;
        
        [Tooltip("Color palette for procedural generation")]
        [SerializeField] private Gradient roomColorPalette;
        
        [Tooltip("Fog density range")]
        [SerializeField] private Vector2 fogDensityRange = new Vector2(0.1f, 0.6f);
        
        [Tooltip("Light intensity range")]
        [SerializeField] private Vector2 lightIntensityRange = new Vector2(0.3f, 0.8f);

        [Header("Chests")]
        [SerializeField] private bool placeChests = true;
        [SerializeField] private GameObject chestPrefab;
        [SerializeField] private int chestCount = 5;
        [SerializeField] private float minDistanceFromChests = 3f;

        [Header("Enemies")]
        [SerializeField] private bool placeEnemies = true;
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private int enemyCount = 8;
        [SerializeField] private float minDistanceFromEnemies = 2f;

        [Header("Items")]
        [SerializeField] private bool placeItems = true;
        [SerializeField] private GameObject[] itemPrefabs;
        [SerializeField] private int itemCount = 10;

        [Header("Torches")]
        [SerializeField] private bool placeTorches = false;  // Torches are NOT placed by default
        [SerializeField] private TorchPool torchPool;
        [SerializeField] private LightPlacementEngine lightPlacementEngine;
        
        [Header("Dynamic Lighting (Difficulty-Based)")]
        [Tooltip("Enable dynamic lighting based on maze level")]
        [SerializeField] private bool useDynamicLighting = true;
        
        [Tooltip("Current maze level (affects lighting)")]
        [SerializeField] private int currentMazeLevel = 1;
        
        [Tooltip("Lighting configuration")]
        [SerializeField] private MazeLightingConfig lightingConfig = new MazeLightingConfig();
        
        // Cached lighting values
        private int cachedTorchCount = 60;
        private float cachedLightRange = 12f;
        private float cachedLightIntensity = 1.8f;
        
        [Header("Manual Override (if not using dynamic)")]
        [SerializeField] private int torchCount = 60;
        [SerializeField] private float torchHeightRatio = 0.55f;
        [SerializeField] private float torchInset = 0.5f;
        [SerializeField] private float minDistanceBetweenTorches = 4f;
        
        [Header("Binary Storage (New System)")]
        [Tooltip("Use binary storage for torch positions (recommended for performance)")]
        [SerializeField] private bool useBinaryStorage = true;
        [Tooltip("Maze ID for binary file naming")]
        [SerializeField] private string mazeId = "Maze_001";

        // Public accessor for placeTorches
        public bool PlaceTorchesEnabled { get => placeTorches; set => placeTorches = value; }

        [Header("Debug")]
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private bool showDebugLogs = true;

        // Internal state
        private int currentSeed;
        private System.Random rng;
        private List<PlacedObject> placedObjects = new List<PlacedObject>();
        private List<SpecialRoom> placedRooms = new List<SpecialRoom>();

        void Awake()
        {
            // Auto-find TorchPool if not assigned
            if (torchPool == null)
            {
                torchPool = GetComponent<TorchPool>();
                if (torchPool == null)
                {
                    Debug.LogWarning("[SpatialPlacer] TorchPool not found on this GameObject. Please add TorchPool component or assign reference.");
                }
            }

            // Auto-find LightPlacementEngine if not assigned
            if (lightPlacementEngine == null)
            {
                lightPlacementEngine = GetComponent<LightPlacementEngine>();
                if (lightPlacementEngine == null)
                {
                    lightPlacementEngine = FindFirstObjectByType<LightPlacementEngine>();
                }
                
                // If still not found, create one automatically
                if (lightPlacementEngine == null)
                {
                    Debug.Log("[SpatialPlacer] LightPlacementEngine not found! Will create when needed");
                }
            }

            // Auto-find MazeGenerator if not assigned
            if (mazeGenerator == null)
            {
                mazeGenerator = GetComponent<MazeGenerator>();
            }

            // PLUG-IN-AND-OUT: Subscribe to maze level changed event
            if (EventHandler.Instance != null)
            {
                EventHandler.Instance.OnMazeLevelChanged += UpdateLightingForLevel;
                Debug.Log("[SpatialPlacer] 🔌 Subscribed to OnMazeLevelChanged event");
            }

            InitializeSeed();
        }
        
        void OnDestroy()
        {
            // PLUG-IN-AND-OUT: Unsubscribe from events
            if (EventHandler.Instance != null)
            {
                EventHandler.Instance.OnMazeLevelChanged -= UpdateLightingForLevel;
            }
        }
        
        /// <summary>
        /// Update lighting settings based on maze difficulty level (called via event).
        /// </summary>
        /// <param name="mazeLevel">Current maze level (1 = easy, 20+ = hard)</param>
        public void UpdateLightingForLevel(int mazeLevel)
        {
            currentMazeLevel = Mathf.Max(1, mazeLevel);  // Update current level
            
            if (lightingConfig == null) return;
            
            // Calculate lighting values for current level
            cachedTorchCount = lightingConfig.GetTorchCountForLevel(currentMazeLevel);
            cachedLightRange = lightingConfig.GetLightRangeForLevel(currentMazeLevel);
            cachedLightIntensity = lightingConfig.GetLightIntensityForLevel(currentMazeLevel);
            
            Debug.Log($"[SpatialPlacer] 🔌 Level {currentMazeLevel}: {cachedTorchCount} torches, {cachedLightRange:F1}m range, {cachedLightIntensity:F2} intensity");
            Debug.Log($"[SpatialPlacer] 🕯️ Lighting Ratio: {lightingConfig.CalculateLightingRatio(currentMazeLevel):P0}");
        }
        
        /// <summary>
        /// Set maze level and update lighting accordingly.
        /// </summary>
        public void SetMazeLevel(int level)
        {
            currentMazeLevel = Mathf.Max(1, level);
            if (useDynamicLighting)
            {
                UpdateLightingForLevel(currentMazeLevel);
            }
        }

        void Start()
        {
            // Don't run in editor pause mode
            if (!Application.isPlaying) return;
            
            InitializeSeed();

            if (waitForMazeGeneration && mazeGenerator != null)
            {
                // Wait for maze to be generated first
                Invoke(nameof(PlaceAllObjects), 0.1f);
            }
            else
            {
                PlaceAllObjects();
            }
        }

        #region Initialization

        void InitializeSeed()
        {
            if (useSeed && generationSeed != 0)
            {
                currentSeed = generationSeed;
            }
            else
            {
                currentSeed = Mathf.RoundToInt(Time.time * 1000f);
            }
            
            rng = new System.Random(currentSeed);
            
            if (showDebugLogs)
            {
                Debug.Log($"[SpatialPlacer] Initialized with seed: {currentSeed}");
            }
        }

        #endregion

        #region Main Placement

        void PlaceAllObjects()
        {
            if (showDebugLogs)
            {
                Debug.Log($"[SpatialPlacer] Starting placement (Seed: {currentSeed})...");
            }

            placedObjects.Clear();
            placedRooms.Clear();

            // Placement order matters!
            // 1. Special Rooms (largest, need most space)
            if (placeSpecialRooms)
            {
                PlaceSpecialRooms();
            }

            // 2. Chests (medium objects)
            if (placeChests)
            {
                PlaceChests();
            }

            // 3. Enemies (dynamic objects)
            if (placeEnemies)
            {
                PlaceEnemies();
            }

            // 4. Items (small pickups)
            if (placeItems)
            {
                PlaceItems();
            }

            // 5. Torches (wall-mounted lighting) - PLUGGED INTO MAZE
            if (placeTorches)
            {
                PlaceTorches();
            }

            if (showDebugLogs)
            {
                Debug.Log($"[SpatialPlacer] Placement complete: {placedObjects.Count} objects placed");
            }
        }

        #endregion

        #region Special Rooms

        void PlaceSpecialRooms()
        {
            if (showDebugLogs)
            {
                Debug.Log($"[SpatialPlacer] Placing {specialRoomCount} special rooms...");
            }
            
            for (int i = 0; i < specialRoomCount; i++)
            {
                Vector3 position = FindValidPosition(
                    minDistance: minDistanceBetweenRooms,
                    preferEdges: preferEdgePlacement,
                    excludeObjects: placedObjects
                );
                
                if (position != Vector3.zero)
                {
                    SpecialRoom room = CreateSpecialRoom(position, i);
                    placedRooms.Add(room);
                    placedObjects.Add(new PlacedObject(position, 10f, PlacedType.Room));
                    
                    if (showDebugLogs)
                    {
                        Debug.Log($"[SpatialPlacer] Placed special room at {position}");
                    }
                }
            }
        }

        SpecialRoom CreateSpecialRoom(Vector3 position, int index)
        {
            // Create room GameObject
            var roomObj = new GameObject($"SpecialRoom_{index + 1}");
            roomObj.transform.position = position;
            roomObj.transform.parent = transform;
            
            // Add SpecialRoom component
            var room = roomObj.AddComponent<SpecialRoom>();
            
            // Configure procedurally or with defaults
            if (proceduralRoomEffects)
            {
                ConfigureProceduralRoom(room, index);
            }
            else
            {
                // Default treasure room config
                room.ambientColor = new Color(0.3f, 0.2f, 0.4f);
                room.fogColor = new Color(0.4f, 0.3f, 0.5f);
                room.fogDensity = 0.3f;
            }
            
            return room;
        }

        void ConfigureProceduralRoom(SpecialRoom room, int index)
        {
            // Generate unique atmosphere for this room
            float roomSeed = currentSeed + index;
            Random.InitState((int)roomSeed);
            
            // Generate colors from palette or randomly
            if (roomColorPalette != null)
            {
                float t = (index + 1f) / (specialRoomCount + 1f);
                Color baseColor = roomColorPalette.Evaluate(t);
                room.ambientColor = baseColor * 0.5f;
                room.fogColor = Color.Lerp(baseColor, Color.white, 0.3f);
            }
            else
            {
                // Random atmospheric colors
                float hue = Random.value;
                float saturation = Random.Range(0.3f, 0.7f);
                float value = Random.Range(0.2f, 0.5f);
                
                room.ambientColor = Color.HSVToRGB(hue, saturation, value);
                room.fogColor = Color.HSVToRGB(hue, saturation * 0.7f, value * 1.5f);
            }
            
            // Fog density
            room.fogDensity = Random.Range(fogDensityRange.x, fogDensityRange.y);
            room.fogStartDistance = Random.Range(2f, 5f);
            room.fogEndDistance = Random.Range(12f, 20f);
            
            // Lighting
            room.roomLightColor = Color.HSVToRGB(Random.value, Random.Range(0.3f, 0.6f), 1f);
            room.roomLightIntensity = Random.Range(lightIntensityRange.x, lightIntensityRange.y);
            
            if (showDebugLogs)
            {
                Debug.Log($"[SpatialPlacer] Room {index} procedural config: Fog={room.fogDensity:F2}, Color={room.fogColor}");
            }
        }

        #endregion

        #region Chests

        void PlaceChests()
        {
            if (chestPrefab == null)
            {
                Debug.LogWarning("[SpatialPlacer] No chest prefab assigned!");
                return;
            }
            
            for (int i = 0; i < chestCount; i++)
            {
                Vector3 position = FindValidPosition(
                    minDistance: minDistanceFromChests,
                    preferEdges: false,
                    excludeObjects: placedObjects
                );
                
                if (position != Vector3.zero)
                {
                    GameObject chest = Instantiate(chestPrefab, position, Quaternion.identity, transform);
                    chest.name = $"Chest_{i + 1}";
                    placedObjects.Add(new PlacedObject(position, 1.5f, PlacedType.Chest));
                }
            }
        }

        #endregion

        #region Enemies

        void PlaceEnemies()
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            {
                Debug.LogWarning("[SpatialPlacer] No enemy prefabs assigned!");
                return;
            }
            
            for (int i = 0; i < enemyCount; i++)
            {
                Vector3 position = FindValidPosition(
                    minDistance: minDistanceFromEnemies,
                    preferEdges: false,
                    excludeObjects: placedObjects
                );
                
                if (position != Vector3.zero)
                {
                    // Pick random enemy type
                    GameObject enemyPrefab = enemyPrefabs[Next(0, enemyPrefabs.Length)];
                    GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity, transform);
                    enemy.name = $"Enemy_{i + 1}_{enemyPrefab.name}";
                    placedObjects.Add(new PlacedObject(position, 1f, PlacedType.Enemy));
                }
            }
        }

        #endregion

        #region Items

        void PlaceItems()
        {
            if (itemPrefabs == null || itemPrefabs.Length == 0)
            {
                Debug.LogWarning("[SpatialPlacer] No item prefabs assigned!");
                return;
            }
            
            for (int i = 0; i < itemCount; i++)
            {
                Vector3 position = FindValidPosition(
                    minDistance: 1f,
                    preferEdges: false,
                    excludeObjects: placedObjects
                );
                
                if (position != Vector3.zero)
                {
                    // Pick random item type
                    GameObject itemPrefab = itemPrefabs[Next(0, itemPrefabs.Length)];
                    GameObject item = Instantiate(itemPrefab, position, Quaternion.identity, transform);
                    item.name = $"Item_{i + 1}_{itemPrefab.name}";
                    placedObjects.Add(new PlacedObject(position, 0.5f, PlacedType.Item));
                }
            }
        }

        #endregion

        #region Torches

        /// <summary>
        /// Place torches on maze walls (plug-in system).
        /// Torches are placed independently from maze generation.
        /// Uses TorchPool for object pooling and MazeRenderer for wall data.
        /// 
        /// NEW: Supports binary storage for performance (no runtime teleportation).
        /// </summary>
        [ContextMenu("Place Torches")]
        public void PlaceTorches()
        {
            Debug.Log($"[SpatialPlacer] PlaceTorches called - placeTorches={placeTorches}, useBinaryStorage={useBinaryStorage}");
            Debug.Log($"[SpatialPlacer] Maze ID: {mazeId}, Seed: {currentSeed}, Level: {currentMazeLevel}");
            
            if (useDynamicLighting)
            {
                Debug.Log($"[SpatialPlacer] 🕯️ Dynamic Lighting: {cachedTorchCount} torches, {cachedLightRange:F1}m range");
            }

            // Clear architect RAM before new placement
            WallPositionArchitect.Clear();

            if (!placeTorches)
            {
                Debug.LogWarning("[SpatialPlacer] Torch placement is disabled! Set placeTorches=true or call PlaceTorchesEnabled=true");
                return;
            }

            if (torchPool == null)
            {
                Debug.LogError("[SpatialPlacer] TorchPool reference not assigned!");
                return;
            }

            if (mazeGenerator == null)
            {
                Debug.LogError("[SpatialPlacer] MazeGenerator reference not found!");
                return;
            }

            // Get MazeRenderer for wall faces and materials
            var mazeRenderer = GetComponent<MazeRenderer>();
            if (mazeRenderer == null)
            {
                Debug.LogError("[SpatialPlacer] MazeRenderer not found!");
                return;
            }

            // ALWAYS use binary storage system (no fallback to legacy!)
            if (lightPlacementEngine == null)
            {
                Debug.LogError("[SpatialPlacer] LightPlacementEngine not found! Cannot use binary storage!");
                return;
            }

            // Use binary storage system
            PlaceTorchesWithBinaryStorage(mazeRenderer);
        }
        
        /// <summary>
        /// NEW SYSTEM: Place torches using binary storage for performance.
        /// 1. Try to load from existing binary file (RAM)
        /// 2. If not found, calculate positions and save to binary
        /// 3. Load from binary and instantiate in batch (no teleportation)
        /// </summary>
        private void PlaceTorchesWithBinaryStorage(MazeRenderer mazeRenderer)
        {
            Debug.Log("[SpatialPlacer] Using BINARY STORAGE system for torch placement");

            // FIRST: Try to load from existing binary file
            bool loadedFromBinary = lightPlacementEngine.LoadAndInstantiateTorches(mazeId, currentSeed);
            
            if (loadedFromBinary)
            {
                Debug.Log($"[SpatialPlacer] ✅ Loaded torches from binary file (RAM)");
                Debug.Log($"[SpatialPlacer] ✅ No calculation needed - instant load!");
                return;  // Done! Loaded from binary
            }
            
            // Binary file doesn't exist yet - need to calculate and save
            Debug.Log("[SpatialPlacer] Binary file not found - calculating positions for first time...");

            // Get wall faces
            var wallFaces = GetWallFacesFromMazeRenderer(mazeRenderer);
            if (wallFaces == null || wallFaces.Count == 0)
            {
                Debug.LogWarning("[SpatialPlacer] No wall faces found. Build maze first!");
                return;
            }

            // Calculate all torch positions
            List<WallPositionArchitect.TorchRecord> torchRecords = CalculateTorchPositions(wallFaces);

            if (torchRecords.Count == 0)
            {
                Debug.LogWarning("[SpatialPlacer] No valid torch positions found");
                return;
            }

            // Save to encrypted binary
            lightPlacementEngine.SaveTorches(mazeId, currentSeed, torchRecords);
            Debug.Log($"[SpatialPlacer] ✅ Saved {torchRecords.Count} torches to binary");

            // Load from binary into RAM and instantiate
            loadedFromBinary = lightPlacementEngine.LoadAndInstantiateTorches(mazeId, currentSeed);
            
            if (loadedFromBinary)
            {
                Debug.Log($"[SpatialPlacer] ✅ Loaded from binary and instantiated");
            }

            // Record in Architect RAM for reference
            foreach (var record in torchRecords)
            {
                WallPositionArchitect.RecordTorchWithGUID(record.position, record.rotation, record.height, record.inset, record.guid);
            }

            Debug.Log($"[SpatialPlacer] ✅ Binary storage complete: {torchRecords.Count} torches");
        }
        
        /// <summary>
        /// Calculate all torch positions (returns records for binary storage).
        /// Uses dynamic lighting values if enabled.
        /// </summary>
        private List<WallPositionArchitect.TorchRecord> CalculateTorchPositions(List<(Vector3, Quaternion)> wallFaces)
        {
            var torchRecords = new List<WallPositionArchitect.TorchRecord>();
            var placedPositions = new List<Vector3>();

            // Use dynamic torch count or manual override
            int actualTorchCount = useDynamicLighting ? cachedTorchCount : torchCount;
            float actualMinDistance = useDynamicLighting ? 
                Mathf.Max(2f, minDistanceBetweenTorches * (cachedLightRange / 12f)) : minDistanceBetweenTorches;
            
            Debug.Log($"[SpatialPlacer] 🕯️ Torch Count: {actualTorchCount} (dynamic: {useDynamicLighting})");
            Debug.Log($"[SpatialPlacer] 🕯️ Min Distance: {actualMinDistance:F1}f");

            // Shuffle wall faces for random distribution
            ShuffleWallFaces(wallFaces);

            foreach (var (wallPos, wallRot) in wallFaces)
            {
                if (torchRecords.Count >= actualTorchCount) break;

                // Probability check
                if (Random.value > GetTorchProbability()) continue;

                // Spacing check (adjusted for lighting range)
                bool tooClose = false;
                foreach (var p in placedPositions)
                {
                    if (Vector3.Distance(wallPos, p) < actualMinDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;

                // Calculate position
                float wallHeight = GetWallHeight();
                float torchY = torchHeightRatio * wallHeight;  // Should be 0.55 * 3.5 = 1.925f
                Vector3 torchPos = new Vector3(wallPos.x, torchY, wallPos.z);
                
                Debug.Log($"[SpatialPlacer] 🕯️ Wall Pos: {wallPos} | Wall Height: {wallHeight} | Torch Y: {torchY} (ratio: {torchHeightRatio})");
                
                Vector3 wallNormal = -(wallRot * Vector3.forward);
                torchPos += wallNormal * torchInset;

                // ═══════════════════════════════════════════════════════════
                // TORCH ROTATION - FLIPPED: Flame faces OUTWARD from wall
                // ═══════════════════════════════════════════════════════════
                // wallRot = rotation that faces INTO the wall
                // Flip 180° on Y so flame faces OUTWARD (away from wall)
                // Handle will be against the wall
                // Tilt 35° on X for better visibility
                Quaternion torchRot = Quaternion.Euler(35f, wallRot.eulerAngles.y + 180f, 0f);
                // ═══════════════════════════════════════════════════════════

                // Create record
                string guid = $"torch_{torchRecords.Count:D4}";
                torchRecords.Add(new WallPositionArchitect.TorchRecord
                {
                    position = torchPos,
                    rotation = torchRot,
                    height = torchY,
                    inset = torchInset,
                    guid = guid
                });

                placedPositions.Add(wallPos);
            }
            
            return torchRecords;
        }
        
        /// <summary>
        /// LEGACY SYSTEM: DISABLED - Causes teleportation! Use PlaceTorchesWithBinaryStorage instead.
        /// </summary>
        private void PlaceTorchesLegacy(MazeRenderer mazeRenderer)
        {
            Debug.LogError("❌ [SpatialPlacer] PlaceTorchesLegacy() is DISABLED!");
            Debug.LogError("❌ [SpatialPlacer] This method causes TELEPORTATION - DO NOT USE!");
            Debug.LogError("❌ [SpatialPlacer] Use PlaceTorchesWithBinaryStorage() instead!");
            throw new System.Exception("PlaceTorchesLegacy() is disabled - causes teleportation!");
        }

        /// <summary>
        /// Remove all placed torches.
        /// </summary>
        [ContextMenu("Remove Torches")]
        public void RemoveTorches()
        {
            if (torchPool != null)
            {
                torchPool.ReleaseAll();
                if (showDebugLogs)
                {
                    Debug.Log("[SpatialPlacer] All torches removed");
                }
            }
        }

        /// <summary>
        /// Regenerate torches with new random distribution.
        /// </summary>
        [ContextMenu("Regenerate Torches")]
        public void RegenerateTorches()
        {
            RemoveTorches();
            PlaceTorches();
        }

        #region Torch Utilities

        private List<(Vector3, Quaternion)> GetWallFacesFromMazeRenderer(MazeRenderer renderer)
        {
            // Access cached wall faces from MazeRenderer
            var type = renderer.GetType();
            var field = type.GetField("_cachedWallFaces",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            if (field == null)
            {
                Debug.LogError("[SpatialPlacer] Cannot access _cachedWallFaces from MazeRenderer!");
                return new List<(Vector3, Quaternion)>();
            }

            return field.GetValue(renderer) as List<(Vector3, Quaternion)>;
        }

        private Transform _torchRoot; // Cache torch root

        private Transform GetTorchRootFromTorchPool()
        {
            // Create torch root if it doesn't exist
            if (_torchRoot == null)
            {
                _torchRoot = new GameObject("ActiveTorches").transform;
                _torchRoot.position = Vector3.zero;
                Debug.Log($"[SpatialPlacer] Created torch root at {_torchRoot.position}");
            }
            return _torchRoot;
        }

        private Material GetSharedFlameMaterial()
        {
            var type = torchPool.GetType();
            var field = type.GetField("_sharedFlameMat",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            return field?.GetValue(torchPool) as Material;
        }

        private Material GetSharedHandleMaterial()
        {
            var type = torchPool.GetType();
            var field = type.GetField("_sharedHandleMat",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            return field?.GetValue(torchPool) as Material;
        }

        private float GetTorchProbability()
        {
            // 100% probability - we want ALL torches placed!
            // Distance check will handle spacing
            return 1.0f;
        }

        private float GetWallHeight()
        {
            // Fixed wall height - no reflection needed
            // Standard maze wall height is 3.5f
            return 3.5f;
        }

        private void ShuffleWallFaces(List<(Vector3, Quaternion)> wallFaces)
        {
            // Fisher-Yates shuffle
            int n = wallFaces.Count;
            while (n > 1)
            {
                n--;
                int k = Next(0, n + 1);
                var temp = wallFaces[n];
                wallFaces[n] = wallFaces[k];
                wallFaces[k] = temp;
            }
        }

        #endregion

        #endregion

        #region Maze-Aware Position Finding

        Vector3 FindValidPosition(float minDistance, bool preferEdges, List<PlacedObject> excludeObjects)
        {
            // CRITICAL: Only place in valid maze cells (NEVER in walls!)
            if (mazeGenerator != null)
            {
                return FindValidCellPosition(minDistance, preferEdges, excludeObjects);
            }
            
            // Fallback to old method if no maze reference
            return FindValidPositionLegacy(minDistance, preferEdges, excludeObjects);
        }

        Vector3 FindValidCellPosition(float minDistance, bool preferEdges, List<PlacedObject> excludeObjects)
        {
            int maxAttempts = 100;
            int attempts = 0;
            
            while (attempts < maxAttempts)
            {
                // Get random cell from maze grid
                Vector2Int cell = preferEdges 
                    ? GetRandomEdgeCell() 
                    : GetRandomCell();
                
                if (cell == Vector2Int.one * -1)
                {
                    attempts++;
                    continue;
                }
                
                // Convert cell to world position (CENTER of cell)
                Vector3 worldPos = CellToWorld(cell);
                
                // Check distance from other placed objects
                if (IsValidPosition(worldPos, minDistance, excludeObjects))
                {
                    return worldPos;
                }
                
                attempts++;
            }
            
            Debug.LogWarning($"[SpatialPlacer] Could not find valid cell after {maxAttempts} attempts");
            return Vector3.zero;
        }

        Vector2Int GetRandomCell()
        {
            if (mazeGenerator == null) return Vector2Int.one * -1;
            
            // Get all valid floor cells
            var validCells = GetValidFloorCells();
            
            if (validCells.Count == 0) return Vector2Int.one * -1;
            
            // Pick random cell
            int index = Next(0, validCells.Count);
            return validCells[index];
        }

        Vector2Int GetRandomEdgeCell()
        {
            if (mazeGenerator == null) return Vector2Int.one * -1;
            
            // Get edge cells (cells on maze perimeter)
            var edgeCells = GetEdgeCells();
            
            if (edgeCells.Count == 0) return GetRandomCell();
            
            // Pick random edge cell
            int index = Next(0, edgeCells.Count);
            return edgeCells[index];
        }

        List<Vector2Int> GetValidFloorCells()
        {
            var validCells = new List<Vector2Int>();
            
            if (mazeGenerator == null) return validCells;
            
            // Iterate through maze grid
            int gridSize = mazeGenerator.gridSize;
            
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    // Check if cell is floor (not wall)
                    if (mazeGenerator.IsCellFloor(x, y))
                    {
                        validCells.Add(new Vector2Int(x, y));
                    }
                }
            }
            
            return validCells;
        }

        List<Vector2Int> GetEdgeCells()
        {
            var edgeCells = new List<Vector2Int>();
            
            if (mazeGenerator == null) return edgeCells;
            
            int gridSize = mazeGenerator.gridSize;
            
            // Top and bottom rows
            for (int x = 0; x < gridSize; x++)
            {
                if (mazeGenerator.IsCellFloor(x, 0))
                    edgeCells.Add(new Vector2Int(x, 0));
                if (mazeGenerator.IsCellFloor(x, gridSize - 1))
                    edgeCells.Add(new Vector2Int(x, gridSize - 1));
            }
            
            // Left and right columns (excluding corners already added)
            for (int y = 1; y < gridSize - 1; y++)
            {
                if (mazeGenerator.IsCellFloor(0, y))
                    edgeCells.Add(new Vector2Int(0, y));
                if (mazeGenerator.IsCellFloor(gridSize - 1, y))
                    edgeCells.Add(new Vector2Int(gridSize - 1, y));
            }
            
            return edgeCells;
        }

        Vector3 CellToWorld(Vector2Int cell)
        {
            // Convert grid cell to world position
            // Center of cell with margin offset
            float cellSize = 6f; // Standard cell size
            float offset = cellSize / 2f;
            float marginOffset = cellSize * cellMargin;
            
            return new Vector3(
                cell.x * cellSize + offset + (cellSize - marginOffset * 2) * 0.5f,
                0,
                cell.y * cellSize + offset + (cellSize - marginOffset * 2) * 0.5f
            );
        }

        Vector3 FindValidPositionLegacy(float minDistance, bool preferEdges, List<PlacedObject> excludeObjects)
        {
            // Legacy method (no maze reference)
            int maxAttempts = 100;
            int attempts = 0;
            
            while (attempts < maxAttempts)
            {
                Vector3 candidate = preferEdges ? GetRandomEdgePosition() : GetRandomPosition();
                
                if (IsValidPosition(candidate, minDistance, excludeObjects))
                {
                    return candidate;
                }
                
                attempts++;
            }
            
            return Vector3.zero;
        }

        Vector3 GetRandomPosition()
        {
            float mazeSize = GetMazeSize();
            float halfSize = mazeSize / 2f;
            
            return new Vector3(
                (float)(rng.NextDouble() * mazeSize - halfSize),
                0,
                (float)(rng.NextDouble() * mazeSize - halfSize)
            );
        }

        Vector3 GetRandomEdgePosition()
        {
            float mazeSize = GetMazeSize();
            float halfSize = mazeSize / 2f;
            
            int edge = Next(0, 4);
            
            return edge switch
            {
                0 => new Vector3((float)(rng.NextDouble() * mazeSize - halfSize), 0, -halfSize),
                1 => new Vector3(halfSize, 0, (float)(rng.NextDouble() * mazeSize - halfSize)),
                2 => new Vector3((float)(rng.NextDouble() * mazeSize - halfSize), 0, halfSize),
                _ => new Vector3(-halfSize, 0, (float)(rng.NextDouble() * mazeSize - halfSize))
            };
        }

        bool IsValidPosition(Vector3 position, float minDistance, List<PlacedObject> excludeObjects)
        {
            foreach (var placed in excludeObjects)
            {
                if (Vector3.Distance(position, placed.Position) < minDistance + placed.Radius)
                {
                    return false;
                }
            }
            
            return true;
        }

        float GetMazeSize()
        {
            if (mazeGenerator != null)
            {
                return mazeGenerator.gridSize * 6f;
            }
            
            return 100f; // Default fallback
        }

        #endregion

        #region Utilities

        int Next(int min, int max)
        {
            return rng.Next(min, max);
        }

        float NextFloat(float min, float max)
        {
            return (float)(rng.NextDouble() * (max - min) + min);
        }

        #endregion

        #region Gizmos

        void OnDrawGizmos()
        {
            if (!showGizmos) return;
            
            // Draw placed objects
            foreach (var placed in placedObjects)
            {
                Gizmos.color = placed.Type switch
                {
                    PlacedType.Room => Color.magenta,
                    PlacedType.Chest => Color.yellow,
                    PlacedType.Enemy => Color.red,
                    PlacedType.Item => Color.green,
                    _ => Color.white
                };
                
                Gizmos.DrawWireSphere(placed.Position, placed.Radius);
            }
        }

        #endregion

        #region Data Structures

        enum PlacedType
        {
            Room,
            Chest,
            Enemy,
            Item,
            Door,
            Creature
        }

        class PlacedObject
        {
            public Vector3 Position { get; }
            public float Radius { get; }
            public PlacedType Type { get; }
            
            public PlacedObject(Vector3 position, float radius, PlacedType type)
            {
                Position = position;
                Radius = radius;
                Type = type;
            }
        }

        #endregion
    }
}
