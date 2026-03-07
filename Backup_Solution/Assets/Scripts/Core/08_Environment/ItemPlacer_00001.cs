// ItemPlacer.cs
// Specialized item placement system
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
    /// ItemPlacer - Specialized item placement.
    /// PLUG-IN-OUT COMPLIANT: Finds components, never creates.
    /// ALL VALUES FROM JSON: Item settings loaded from GameConfig.
    /// </summary>
    public class ItemPlacer : MonoBehaviour
    {
        #region Inspector Fields (From JSON)

        [Header("🎒 Item Settings (From JSON Config)")]
        [Tooltip("Enable item spawning (loaded from JSON)")]
        [SerializeField] private bool enableItemSpawning;
        
        [Tooltip("Chance for item to spawn (loaded from JSON)")]
        [SerializeField] private float itemSpawnChance;
        
        [Tooltip("Max items to spawn (loaded from JSON)")]
        [SerializeField] private int maxItems;

        [Header("🎒 Prefab Reference")]
        [Tooltip("Item prefab (assign in Inspector or Resources/)")]
        [SerializeField] private GameObject itemPrefab;

        [Header("🔌 Component References (Plug-in-Out)")]
        [Tooltip("Auto-finds GridMazeGenerator in scene")]
        [SerializeField] private GridMazeGenerator gridMazeGenerator;

        [Header("🐛 Debug")]
        [SerializeField] private bool showDebugLogs = true;

        #endregion

        #region Private Data

        private int _itemsSpawned;

        #endregion

        #region Public Accessors

        public int ItemsSpawned => _itemsSpawned;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            FindComponents();
            LoadConfig();
        }

        #endregion

        #region Plug-in-Out Compliance

        private void FindComponents()
        {
            if (gridMazeGenerator == null)
                gridMazeGenerator = FindFirstObjectByType<GridMazeGenerator>();
        }

        #endregion

        #region JSON Config Loading

        private void LoadConfig()
        {
            var config = GameConfig.Instance;
            
            enableItemSpawning = config.generateRooms;
            itemSpawnChance = config.defaultDoorSpawnChance;
            maxItems = config.maxRooms;

            if (showDebugLogs)
            {
                Debug.Log($"[ItemPlacer] 📖 Config loaded from JSON:");
                Debug.Log($"  • Enable: {enableItemSpawning}");
                Debug.Log($"  • Spawn Chance: {itemSpawnChance * 100f:F0}%");
                Debug.Log($"  • Max Items: {maxItems}");
            }
        }

        #endregion

        #region Public API

        [ContextMenu("Place Items")]
        public void PlaceAllItems()
        {
            if (!enableItemSpawning)
            {
                if (showDebugLogs)
                    Debug.Log("[ItemPlacer] Item spawning disabled");
                return;
            }

            if (gridMazeGenerator == null)
            {
                Debug.LogError("[ItemPlacer] ❌ GridMazeGenerator not initialized!");
                return;
            }

            _itemsSpawned = 0;
            int targetCount = Random.Range(1, maxItems + 1);
            int attempts = 0;
            int maxAttempts = targetCount * 3;

            while (_itemsSpawned < targetCount && attempts < maxAttempts)
            {
                attempts++;
                
                Vector2Int cell = GetRandomValidCell();
                
                if (cell.x >= 0)
                {
                    PlaceItem(cell);
                    _itemsSpawned++;
                }
            }

            if (showDebugLogs)
            {
                Debug.Log($"[ItemPlacer] 🎒 Spawned {_itemsSpawned} items");
            }
        }

        #endregion

        #region Item Placement

        private void PlaceItem(Vector2Int cell)
        {
            float cellSize = GetCellSize();
            
            Vector3 position = new Vector3(
                cell.x * cellSize + cellSize / 2f,
                0.1f,
                cell.y * cellSize + cellSize / 2f
            );
            
            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);

            if (itemPrefab != null)
            {
                GameObject.Instantiate(itemPrefab, position, rotation);
                _itemsSpawned++;

                // Save to binary storage
                if (_storageReference != null)
                {
                    _storageReference.SaveObjectToBinary("Item", position, rotation);
                }
                
                if (showDebugLogs)
                    Debug.Log($"[ItemPlacer] 🎒 Placed item prefab at {position}");
            }
            else
            {
                if (showDebugLogs)
                    Debug.LogWarning($"[ItemPlacer] ⚠️ No item prefab assigned - skipping");
            }
        }

        private Vector2Int GetRandomValidCell()
        {
            int size = gridMazeGenerator.GridSize;
            int maxAttempts = 20;
            
            for (int i = 0; i < maxAttempts; i++)
            {
                int x = Random.Range(0, size);
                int y = Random.Range(0, size);
                
                var cell = gridMazeGenerator.GetCell(x, y);
                
                if (cell == GridMazeCell.Room || cell == GridMazeCell.Corridor)
                {
                    return new Vector2Int(x, y);
                }
            }
            
            return new Vector2Int(-1, -1);
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

        public void PlaceItemsFromRecords(List<SpatialPlacer.ObjectPlacementRecord> records)
        {
            if (records == null || records.Count == 0) return;

            foreach (var record in records)
            {
                GameObject.Instantiate(itemPrefab, record.Position, record.Rotation);
                _itemsSpawned++;
            }

            if (showDebugLogs)
                Debug.Log($"[ItemPlacer] 🎒 Loaded {_itemsSpawned} items from binary");
        }

        #endregion
    }
}
