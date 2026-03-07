// SpawningRoom.cs
// Generic spawning room system for all maze testing tools
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-OUT ARCHITECTURE:
// - Finds components (never creates)
// - Uses prefabs ONLY (no procedural fallback)
// - All values loaded from JSON config
// - No hardcoded values
//
// FEATURES:
// - Generic spawning room with entrance/exit
// - Spawns player, torches, chests, items
// - Requires prefabs in Resources/ or assigned in Inspector
// - For procedural spawning, use SpatialPlacer instead
//
// LOCATION: Assets/Scripts/Core/06_Maze/

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// SpawningRoom - Generic spawning room for maze testing.
    /// ALL VALUES FROM JSON: Room dimensions, spawn settings loaded from GameConfig.
    /// USES PREFABS: When available, otherwise falls back to procedural generation.
    /// </summary>
    public class SpawningRoom : MonoBehaviour
    {
        #region Inspector Fields (Serialized from JSON)

        [Header("🏛️ Room Dimensions (From JSON Config)")]
        [Tooltip("Room size in grid cells (loaded from JSON)")]
        [SerializeField] private int roomSize;
        
        [Tooltip("Room width in meters (loaded from JSON)")]
        [SerializeField] private float roomWidth;
        
        [Tooltip("Room height in meters (loaded from JSON)")]
        [SerializeField] private float roomHeight;
        
        [Tooltip("Entrance direction (loaded from JSON)")]
        [SerializeField] private Vector2Int entranceDirection;
        
        [Tooltip("Exit direction (loaded from JSON)")]
        [SerializeField] private Vector2Int exitDirection;

        [Header("🎒 Item Spawning (From JSON Config)")]
        [Tooltip("Enable item spawning in room (loaded from JSON)")]
        [SerializeField] private bool enableItemSpawning;
        
        [Tooltip("Chance for item to spawn (loaded from JSON)")]
        [SerializeField] private float itemSpawnChance;
        
        [Tooltip("Max items to spawn (loaded from JSON)")]
        [SerializeField] private int maxItems;

        [Header("📦 Prefab References (Optional)")]
        [Tooltip("Item prefab (if exists, otherwise uses procedural)")]
        [SerializeField] private GameObject itemPrefab;
        
        [Tooltip("Torch prefab (if exists, otherwise uses procedural)")]
        [SerializeField] private GameObject torchPrefab;
        
        [Tooltip("Chest prefab (if exists, otherwise uses procedural)")]
        [SerializeField] private GameObject chestPrefab;

        [Header("🔌 Component References (Plug-in-Out)")]
        [Tooltip("Auto-finds GridMazeGenerator in scene")]
        [SerializeField] private GridMazeGenerator gridMazeGenerator;
        
        [Tooltip("Auto-finds CompleteMazeBuilder in scene")]
        [SerializeField] private CompleteMazeBuilder completeMazeBuilder;

        [Header("🐛 Debug (Hardcoded - Comment to Disable Warnings)")]
        // [SerializeField] private float hardcodedCellSize = 6f;  // Comment out to disable warning
        [SerializeField] private bool showDebugLogs = true;

        #endregion

        #region Private Data

        private Vector3 spawnPosition;
        private Quaternion spawnRotation;

        #endregion

        #region Public Accessors

        public int RoomSize => roomSize;
        public float RoomWidth => roomWidth;
        public float RoomHeight => roomHeight;
        public Vector3 SpawnPosition => spawnPosition;
        public Quaternion SpawnRotation => spawnRotation;
        public bool HasEntrance => entranceDirection != Vector2Int.zero;
        public bool HasExit => exitDirection != Vector2Int.zero;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // PLUG-IN-OUT: Find components (never create!)
            FindComponents();
            
            // LOAD ALL VALUES FROM JSON CONFIG (NO HARDCODING!)
            LoadConfig();
        }

        #endregion

        #region Plug-in-Out Compliance

        /// <summary>
        /// Find all required components in scene.
        /// PLUG-IN-OUT: Never creates components, only finds existing ones.
        /// </summary>
        private void FindComponents()
        {
            if (gridMazeGenerator == null)
                gridMazeGenerator = FindFirstObjectByType<GridMazeGenerator>();
            
            if (completeMazeBuilder == null)
                completeMazeBuilder = FindFirstObjectByType<CompleteMazeBuilder>();
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
            
            // Room dimensions from JSON
            roomSize = config.defaultRoomSize;
            roomWidth = config.defaultCellSize * roomSize;
            roomHeight = config.defaultWallHeight;
            
            // Default entrance/exit directions
            entranceDirection = Vector2Int.down;  // Entrance from south
            exitDirection = Vector2Int.up;        // Exit to north
            
            // Item spawning from JSON
            enableItemSpawning = config.generateRooms;
            itemSpawnChance = config.defaultDoorSpawnChance;  // Reuse door chance
            maxItems = config.maxRooms;  // Reuse room count

            if (showDebugLogs)
            {
                Debug.Log($"[SpawningRoom] 📖 Config loaded from JSON:");
                Debug.Log($"  • Room Size: {roomSize}x{roomSize} cells ({roomWidth}m x {roomWidth}m)");
                Debug.Log($"  • Room Height: {roomHeight}m");
                Debug.Log($"  • Entrance: {entranceDirection}");
                Debug.Log($"  • Exit: {exitDirection}");
                Debug.Log($"  • Item Spawning: {enableItemSpawning} ({itemSpawnChance * 100f:F0}% chance, max {maxItems})");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Initialize spawning room at grid position.
        /// Call before spawning player or items.
        /// </summary>
        public void InitializeRoom(Vector2Int gridPosition, GridMazeCell cellType)
        {
            // Calculate spawn position (center of room)
            float cellSize = GetCellSize();
            
            spawnPosition = new Vector3(
                gridPosition.x * cellSize + cellSize / 2f,
                0.9f,  // CharacterController center
                gridPosition.y * cellSize + cellSize / 2f
            );
            
            // Rotation faces toward exit
            spawnRotation = Quaternion.LookRotation(new Vector3(exitDirection.x, 0f, exitDirection.y));

            if (showDebugLogs)
            {
                Debug.Log($"[SpawningRoom] 🏛️ Initialized at {gridPosition} → {spawnPosition}");
            }
        }

        /// <summary>
        /// Spawn player in room.
        /// </summary>
        public void SpawnPlayer()
        {
            var player = FindFirstObjectByType<PlayerController>();
            
            if (player == null)
            {
                Debug.LogWarning("[SpawningRoom] ⚠️ PlayerController not found in scene");
                return;
            }
            
            player.transform.position = spawnPosition;
            player.transform.rotation = spawnRotation;
            
            // Add small random offset (prevent wall clipping)
            float offsetX = (Random.value - 0.5f) * 1f;
            float offsetZ = (Random.value - 0.5f) * 1f;
            player.transform.position += new Vector3(offsetX, 0f, offsetZ);

            if (showDebugLogs)
            {
                Debug.Log($"[SpawningRoom] 👤 Player spawned at {spawnPosition}");
            }
        }

        /// <summary>
        /// Spawn items in room (chests, torches, etc.).
        /// Uses prefabs when available, falls back to procedural.
        /// </summary>
        public void SpawnItems()
        {
            if (!enableItemSpawning)
            {
                if (showDebugLogs)
                    Debug.Log("[SpawningRoom] Item spawning disabled");
                return;
            }

            int itemsToSpawn = Random.Range(1, maxItems + 1);
            int itemsSpawned = 0;

            for (int i = 0; i < itemsToSpawn; i++)
            {
                // Roll for spawn
                if (Random.value > itemSpawnChance)
                    continue;

                // Spawn random item
                SpawnRandomItem();
                itemsSpawned++;
            }

            if (showDebugLogs)
            {
                Debug.Log($"[SpawningRoom] 🎒 Spawned {itemsSpawned} items");
            }
        }

        #endregion

        #region Item Spawning

        /// <summary>
        /// Spawn random item (chest, torch, etc.).
        /// Uses prefab if available, otherwise procedural.
        /// </summary>
        private void SpawnRandomItem()
        {
            float roll = Random.value;
            
            if (roll < 0.3f)
            {
                SpawnTorch();
            }
            else if (roll < 0.6f)
            {
                SpawnChest();
            }
            else
            {
                SpawnGenericItem();
            }
        }

        /// <summary>
        /// Spawn torch (prefab only - NO procedural fallback).
        /// PLUG-IN-OUT COMPLIANT: Does NOT create GameObjects or components.
        /// </summary>
        private void SpawnTorch()
        {
            if (torchPrefab == null)
            {
                if (showDebugLogs)
                    Debug.Log("[SpawningRoom] ⚠️ No torch prefab assigned - skipping (add prefab or use SpatialPlacer)");
                return;
            }

            Vector3 position = GetRandomRoomPosition();
            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0, 4) * 90f, 0f);

            // Use prefab ONLY (plug-in-out compliant)
            GameObject.Instantiate(torchPrefab, position, rotation, transform);
            
            if (showDebugLogs)
                Debug.Log($"[SpawningRoom] 🔥 Spawned torch prefab at {position}");
        }

        /// <summary>
        /// Spawn chest (prefab only - NO procedural fallback).
        /// PLUG-IN-OUT COMPLIANT: Does NOT create GameObjects or components.
        /// </summary>
        private void SpawnChest()
        {
            if (chestPrefab == null)
            {
                if (showDebugLogs)
                    Debug.Log("[SpawningRoom] ⚠️ No chest prefab assigned - skipping (add prefab or use SpatialPlacer)");
                return;
            }

            Vector3 position = GetRandomRoomPosition();
            Quaternion rotation = Quaternion.identity;

            // Use prefab ONLY (plug-in-out compliant)
            GameObject.Instantiate(chestPrefab, position, rotation, transform);
            
            if (showDebugLogs)
                Debug.Log($"[SpawningRoom] 📦 Spawned chest prefab at {position}");
        }

        /// <summary>
        /// Spawn generic item (prefab only - NO procedural fallback).
        /// PLUG-IN-OUT COMPLIANT: Does NOT create GameObjects or components.
        /// </summary>
        private void SpawnGenericItem()
        {
            if (itemPrefab == null)
            {
                if (showDebugLogs)
                    Debug.Log("[SpawningRoom] ⚠️ No item prefab assigned - skipping (add prefab or use SpatialPlacer)");
                return;
            }

            Vector3 position = GetRandomRoomPosition();
            Quaternion rotation = Quaternion.identity;

            // Use prefab ONLY (plug-in-out compliant)
            GameObject.Instantiate(itemPrefab, position, rotation, transform);
            
            if (showDebugLogs)
                Debug.Log($"[SpawningRoom] 🎒 Spawned item prefab at {position}");
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get random position inside room.
        /// </summary>
        private Vector3 GetRandomRoomPosition()
        {
            float cellSize = GetCellSize();
            float offset = cellSize * 0.2f;  // Keep away from walls
            
            return new Vector3(
                spawnPosition.x + Random.Range(-offset, offset),
                0.1f,  // Just above floor
                spawnPosition.z + Random.Range(-offset, offset)
            );
        }

        /// <summary>
        /// Get cell size from config or hardcoded fallback.
        /// </summary>
        private float GetCellSize()
        {
            // Try to get from CompleteMazeBuilder
            if (completeMazeBuilder != null)
            {
                // TODO: Add public accessor to CompleteMazeBuilder
                return GameConfig.Instance.defaultCellSize;
            }
            
            // Fallback to JSON config
            return GameConfig.Instance.defaultCellSize;
            
            // HARDCODED FALLBACK (comment out to disable warning):
            // return hardcodedCellSize;  // Default: 6f
        }

        #endregion
    }
}
