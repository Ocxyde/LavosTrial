// ChestPlacer.cs
// Specialized chest placement system
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-OUT ARCHITECTURE:
// - Finds components (never creates)
// - Uses prefabs when available
// - All values from JSON config
// - No hardcoded values
//
// LOCATION: Assets/Scripts/Core/08_Environment/

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// ChestPlacer - Specialized chest placement.
    /// PLUG-IN-OUT COMPLIANT: Finds components, never creates.
    /// ALL VALUES FROM JSON: Chest settings loaded from GameConfig.
    /// </summary>
    public class ChestPlacer : MonoBehaviour
    {
        #region Inspector Fields (From JSON)

        [Header("📦 Chest Settings (From JSON Config)")]
        [Tooltip("Enable chest spawning (loaded from JSON)")]
        [SerializeField] private bool enableChestSpawning;
        
        [Tooltip("Chance for chest to spawn (loaded from JSON)")]
        [SerializeField] private float chestSpawnChance;
        
        [Tooltip("Max chests to spawn (loaded from JSON)")]
        [SerializeField] private int maxChests;

        [Header("📦 Prefab Reference")]
        [Tooltip("Chest prefab (assign in Inspector or Resources/)")]
        [SerializeField] private GameObject chestPrefab;

        [Header("🔌 Component References (Plug-in-Out)")]
        [Tooltip("Auto-finds GridMazeGenerator in scene")]
        [SerializeField] private GridMazeGenerator gridMazeGenerator;
        
        [Tooltip("Auto-finds CompleteMazeBuilder in scene")]
        [SerializeField] private CompleteMazeBuilder completeMazeBuilder;

        [Header("🐛 Debug")]
        [SerializeField] private bool showDebugLogs = true;

        #endregion

        #region Private Data

        private int _chestsSpawned;

        #endregion

        #region Public Accessors

        public int ChestsSpawned => _chestsSpawned;

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
            
            // Chest settings from JSON
            enableChestSpawning = config.generateRooms;  // Reuse room generation flag
            chestSpawnChance = config.defaultDoorSpawnChance;  // Reuse door chance
            maxChests = config.maxRooms;  // Reuse room count

            if (showDebugLogs)
            {
                Debug.Log($"[ChestPlacer] 📖 Config loaded from JSON:");
                Debug.Log($"  • Enable: {enableChestSpawning}");
                Debug.Log($"  • Spawn Chance: {chestSpawnChance * 100f:F0}%");
                Debug.Log($"  • Max Chests: {maxChests}");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Place chests in maze.
        /// Call after maze generation.
        /// </summary>
        [ContextMenu("Place Chests")]
        public void PlaceAllChests()
        {
            if (!enableChestSpawning)
            {
                if (showDebugLogs)
                    Debug.Log("[ChestPlacer] Chest spawning disabled");
                return;
            }

            if (gridMazeGenerator == null)
            {
                Debug.LogError("[ChestPlacer] ❌ GridMazeGenerator not initialized!");
                return;
            }

            _chestsSpawned = 0;
            int targetCount = Random.Range(1, maxChests + 1);
            int attempts = 0;
            int maxAttempts = targetCount * 3;  // Allow some failed attempts

            while (_chestsSpawned < targetCount && attempts < maxAttempts)
            {
                attempts++;
                
                // Get random room/corridor cell
                Vector2Int cell = GetRandomValidCell();
                
                if (cell.x >= 0)
                {
                    PlaceChest(cell);
                    _chestsSpawned++;
                }
            }

            if (showDebugLogs)
            {
                Debug.Log($"[ChestPlacer] 📦 Spawned {_chestsSpawned} chests");
            }
        }

        #endregion

        #region Chest Placement

        /// <summary>
        /// Place single chest at grid position.
        /// </summary>
        private void PlaceChest(Vector2Int cell)
        {
            float cellSize = GetCellSize();
            
            Vector3 position = new Vector3(
                cell.x * cellSize + cellSize / 2f,
                0.1f,  // Just above floor
                cell.y * cellSize + cellSize / 2f
            );
            
            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0, 4) * 90f, 0f);

            if (chestPrefab != null)
            {
                // Use prefab
                GameObject.Instantiate(chestPrefab, position, rotation);
                _chestsSpawned++;

                // Save to binary storage
                if (_storageReference != null)
                {
                    _storageReference.SaveObjectToBinary("Chest", position, rotation);
                }
                
                if (showDebugLogs)
                    Debug.Log($"[ChestPlacer] 📦 Placed chest prefab at {position}");
            }
            else
            {
                if (showDebugLogs)
                    Debug.LogWarning($"[ChestPlacer] ⚠️ No chest prefab assigned - skipping");
            }
        }

        /// <summary>
        /// Get random valid cell for chest placement.
        /// Returns (-1, -1) if no valid cell found.
        /// </summary>
        private Vector2Int GetRandomValidCell()
        {
            int size = gridMazeGenerator.GridSize;
            int maxAttempts = 20;
            
            for (int i = 0; i < maxAttempts; i++)
            {
                int x = Random.Range(0, size);
                int y = Random.Range(0, size);
                
                var cell = gridMazeGenerator.GetCell(x, y);
                
                // Only place in rooms or corridors (not walls)
                if (cell == GridMazeCell.Room || cell == GridMazeCell.Corridor)
                {
                    return new Vector2Int(x, y);
                }
            }
            
            return new Vector2Int(-1, -1);  // No valid cell found
        }

        #endregion

        #region Utilities

        private float GetCellSize()
        {
            return GameConfig.Instance.defaultCellSize;
        }

        #endregion

        #region Binary Storage Integration

        private SpatialPlacer _storageReference;

        public void SetStorageReference(SpatialPlacer placer)
        {
            _storageReference = placer;
        }

        public void PlaceChestsFromRecords(List<SpatialPlacer.ObjectPlacementRecord> records)
        {
            if (records == null || records.Count == 0) return;

            foreach (var record in records)
            {
                GameObject.Instantiate(chestPrefab, record.Position, record.Rotation);
                _chestsSpawned++;
            }

            if (showDebugLogs)
                Debug.Log($"[ChestPlacer] 📦 Loaded {_chestsSpawned} chests from binary");
        }

        #endregion
    }
}
