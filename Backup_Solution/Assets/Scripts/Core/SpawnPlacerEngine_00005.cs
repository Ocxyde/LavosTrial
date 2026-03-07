// SpawnPlacerEngine.cs
// Procedural item placement engine for maze
// Unity 6 compatible - UTF-8 encoding - Unix line endings
// Plug-in-and-Out modular architecture

using UnityEngine;
using Code.Lavos.Core;
using System.Collections.Generic;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Places items (doors, chests, etc.) procedurally in the maze.
    /// Integrates with ItemEngine for centralized management.
    /// </summary>
    public class SpawnPlacerEngine : MonoBehaviour
    {
        [Header("General Settings")]
        [SerializeField] private bool autoPlaceOnStart = true;
        [SerializeField] private bool showDebugGizmos = true;

        [Header("Door Settings")]
        [SerializeField] private bool placeDoors = true;
        [SerializeField] private DoorType startDoorType = DoorType.Start;
        [SerializeField] private DoorType exitDoorType = DoorType.Exit;
        [SerializeField] private bool placeRandomDoors = true;
        [SerializeField] private float randomDoorChance = 0.1f;
        [SerializeField] private float doorLuminanceMin = 0.8f;
        [SerializeField] private float doorLuminanceMax = 1.5f;

        [Header("Chest Settings")]
        [SerializeField] private bool placeChests = true;
        [SerializeField] private float chestDensity = 0.05f;
        [SerializeField] private int minChests = 2;
        [SerializeField] private int maxChests = 5;
        [SerializeField] private LootTable defaultLootTable;

        [Header("Item Settings")]
        [SerializeField] private bool placePickups = false;
        [SerializeField] private float pickupDensity = 0.03f;
        [SerializeField] private GameObject[] pickupPrefabs;

        [Header("Trap Settings")]
        [SerializeField] private bool placeTraps = true;
        [SerializeField] private float trapDensity = 0.08f; // 8% of valid cells
        [SerializeField] private int minTraps = 3;
        [SerializeField] private int maxTraps = 8;
        [SerializeField] private float minDistanceBetweenTraps = 8f; // Large distance between traps
        [SerializeField] private TrapType[] availableTrapTypes;

        [Header("Excluded Cells")]
        [SerializeField] private List<Vector2Int> excludedCells;

        private MazeGenerator _mazeGenerator;
        private ItemEngine _itemEngine;
        private List<PlacedItemInfo> _placedItems;

        public int PlacedItemCount => _placedItems?.Count ?? 0;
        public List<PlacedItemInfo> PlacedItems => _placedItems;

        private void Awake()
        {
            _placedItems = new List<PlacedItemInfo>();
        }

        private void Start()
        {
            _mazeGenerator = GetComponent<MazeGenerator>();
            _itemEngine = ItemEngine.Instance;

            if (autoPlaceOnStart && _mazeGenerator != null)
            {
                Invoke(nameof(PlaceAllItems), 0.2f);
            }
        }

        /// <summary>
        /// Place all items in the maze.
        /// </summary>
        [ContextMenu("Place All Items")]
        public void PlaceAllItems()
        {
            if (_mazeGenerator == null)
            {
                Debug.LogError("[SpawnPlacerEngine] MazeGenerator not found!");
                return;
            }

            ClearAllItems();

            Debug.Log("[SpawnPlacerEngine] Starting item placement...");

            if (placeDoors)
            {
                PlaceDoors();
            }

            if (placeChests)
            {
                PlaceChests();
            }

            if (placePickups)
            {
                PlacePickups();
            }

            if (placeTraps)
            {
                PlaceTraps();
            }

            Debug.Log($"[SpawnPlacerEngine] Placement complete! Total: {_placedItems.Count} items");
        }

        #region Door Placement

        [ContextMenu("Place Doors")]
        public void PlaceDoors()
        {
            // Place start door
            PlaceDoorAtCell(Vector2Int.zero, startDoorType, doorLuminanceMax);

            // Place exit door
            Vector2Int exitCell = new Vector2Int(_mazeGenerator.Width - 1, _mazeGenerator.Height - 1);
            PlaceDoorAtCell(exitCell, exitDoorType, doorLuminanceMax * 1.2f);

            // Place random doors
            if (placeRandomDoors)
            {
                List<Vector2Int> validCells = GetValidDoorCells();

                foreach (var cell in validCells)
                {
                    if (Random.value < randomDoorChance)
                    {
                        float luminance = Random.Range(doorLuminanceMin, doorLuminanceMax);
                        PlaceDoorAtCell(cell, DoorType.Random, luminance);
                    }
                }
            }
        }

        private List<Vector2Int> GetValidDoorCells()
        {
            List<Vector2Int> validCells = new List<Vector2Int>();

            for (int x = 0; x < _mazeGenerator.Width; x++)
            {
                for (int y = 0; y < _mazeGenerator.Height; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);

                    // Skip start and exit
                    if (cell == Vector2Int.zero) continue;
                    if (cell.x == _mazeGenerator.Width - 1 && cell.y == _mazeGenerator.Height - 1) continue;

                    // Skip excluded cells
                    if (excludedCells != null && excludedCells.Contains(cell)) continue;

                    validCells.Add(cell);
                }
            }

            return validCells;
        }

        private DoubleDoor PlaceDoorAtCell(Vector2Int cell, DoorType type, float luminance)
        {
            Vector3 worldPos = CellToWorldPosition(cell);

            // Determine door rotation based on maze walls
            Quaternion rotation = GetDoorRotation(cell);

            GameObject doorObj = new GameObject($"Door_{cell.x}_{cell.y}");
            doorObj.transform.position = worldPos;
            doorObj.transform.rotation = rotation;

            DoubleDoor door = doorObj.AddComponent<DoubleDoor>();
            door.Initialize(4f, 3f, type, luminance);

            var info = new PlacedItemInfo
            {
                cell = cell,
                itemType = ItemType.Door,
                instance = doorObj,
                worldPosition = worldPos
            };
            _placedItems.Add(info);

            Debug.Log($"[SpawnPlacerEngine] Placed {type} door at {cell}");
            return door;
        }

        private Quaternion GetDoorRotation(Vector2Int cell)
        {
            // Check adjacent walls to determine door orientation
            var walls = _mazeGenerator.Grid[cell.x, cell.y];

            // Simple heuristic: if north/south walls are open, door faces east-west
            // If east/west walls are open, door faces north-south
            bool hasNorthWall = (walls & MazeGenerator.Wall.North) != 0;
            bool hasSouthWall = (walls & MazeGenerator.Wall.South) != 0;

            if (hasNorthWall && hasSouthWall)
            {
                return Quaternion.Euler(0, 90, 0); // Face east-west
            }
            else
            {
                return Quaternion.Euler(0, 0, 0); // Face north-south
            }
        }

        #endregion

        #region Chest Placement

        [ContextMenu("Place Chests")]
        public void PlaceChests()
        {
            List<Vector2Int> validCells = GetValidChestCells();

            int totalCells = validCells.Count;
            int chestCount = Mathf.Clamp(
                Mathf.RoundToInt(totalCells * chestDensity),
                minChests,
                maxChests
            );

            for (int i = 0; i < chestCount; i++)
            {
                if (validCells.Count == 0) break;

                int index = Random.Range(0, validCells.Count);
                Vector2Int cell = validCells[index];
                validCells.RemoveAt(index);

                PlaceChestAtCell(cell);
            }
        }

        private List<Vector2Int> GetValidChestCells()
        {
            List<Vector2Int> validCells = new List<Vector2Int>();

            for (int x = 0; x < _mazeGenerator.Width; x++)
            {
                for (int y = 0; y < _mazeGenerator.Height; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);

                    // Skip start and exit
                    if (cell == Vector2Int.zero) continue;
                    if (cell.x == _mazeGenerator.Width - 1 && cell.y == _mazeGenerator.Height - 1) continue;

                    // Skip excluded cells
                    if (excludedCells != null && excludedCells.Contains(cell)) continue;

                    validCells.Add(cell);
                }
            }

            return validCells;
        }

        private ChestBehavior PlaceChestAtCell(Vector2Int cell)
        {
            Vector3 worldPos = CellToWorldPosition(cell);
            worldPos.y = 0.6f; // Half chest height

            GameObject chestObj = new GameObject($"Chest_{cell.x}_{cell.y}");
            chestObj.transform.position = worldPos;

            ChestBehavior chest = chestObj.AddComponent<ChestBehavior>();
            chest.Initialize(1.5f, 1.2f, defaultLootTable);

            var info = new PlacedItemInfo
            {
                cell = cell,
                itemType = ItemType.Chest,
                instance = chestObj,
                worldPosition = worldPos
            };
            _placedItems.Add(info);

            Debug.Log($"[SpawnPlacerEngine] Placed chest at {cell}");
            return chest;
        }

        #endregion

        #region Pickup Placement

        [ContextMenu("Place Pickups")]
        public void PlacePickups()
        {
            if (pickupPrefabs == null || pickupPrefabs.Length == 0)
            {
                Debug.LogWarning("[SpawnPlacerEngine] No pickup prefabs assigned!");
                return;
            }

            List<Vector2Int> validCells = GetValidPickupCells();
            int count = Mathf.RoundToInt(validCells.Count * pickupDensity);

            for (int i = 0; i < count; i++)
            {
                if (validCells.Count == 0) break;

                int index = Random.Range(0, validCells.Count);
                Vector2Int cell = validCells[index];
                validCells.RemoveAt(index);

                PlacePickupAtCell(cell);
            }
        }

        private List<Vector2Int> GetValidPickupCells()
        {
            List<Vector2Int> validCells = new List<Vector2Int>();

            for (int x = 0; x < _mazeGenerator.Width; x++)
            {
                for (int y = 0; y < _mazeGenerator.Height; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);

                    if (cell == Vector2Int.zero) continue;
                    if (cell.x == _mazeGenerator.Width - 1 && cell.y == _mazeGenerator.Height - 1) continue;
                    if (excludedCells != null && excludedCells.Contains(cell)) continue;

                    validCells.Add(cell);
                }
            }

            return validCells;
        }

        private GameObject PlacePickupAtCell(Vector2Int cell)
        {
            Vector3 worldPos = CellToWorldPosition(cell);
            worldPos.y = 0.5f;

            GameObject prefab = pickupPrefabs[Random.Range(0, pickupPrefabs.Length)];
            GameObject pickup = Instantiate(prefab, worldPos, Quaternion.identity);

            var info = new PlacedItemInfo
            {
                cell = cell,
                itemType = ItemType.Pickup,
                instance = pickup,
                worldPosition = worldPos
            };
            _placedItems.Add(info);

            return pickup;
        }

        #endregion

        #region Utility Methods

        private Vector3 CellToWorldPosition(Vector2Int cell)
        {
            float cellSize = 4f;
            float x = cell.x * cellSize + cellSize * 0.5f;
            float z = cell.y * cellSize + cellSize * 0.5f;
            return new Vector3(x, 0f, z);
        }

        /// <summary>
        /// Clear all placed items.
        /// </summary>
        [ContextMenu("Clear All Items")]
        public void ClearAllItems()
        {
            if (_placedItems == null)
                return;

            foreach (var info in _placedItems)
            {
                if (info.instance != null)
                {
                    Destroy(info.instance);
                }
            }

            _placedItems.Clear();
            Debug.Log("[SpawnPlacerEngine] All items cleared");
        }

        /// <summary>
        /// Get item at cell.
        /// </summary>
        public PlacedItemInfo GetItemAtCell(Vector2Int cell)
        {
            return _placedItems.Find(info => info.cell == cell);
        }

        /// <summary>
        /// Remove item at cell.
        /// </summary>
        public void RemoveItemAtCell(Vector2Int cell)
        {
            var info = _placedItems.Find(i => i.cell == cell);
            if (info.instance != null)
            {
                Destroy(info.instance);
                _placedItems.Remove(info);
            }
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos || _placedItems == null)
                return;

            foreach (var info in _placedItems)
            {
                Gizmos.color = info.itemType switch
                {
                    ItemType.Door => Color.blue,
                    ItemType.Chest => Color.yellow,
                    ItemType.Pickup => Color.green,
                    ItemType.Switch => Color.red,
                    _ => Color.gray
                };

                Gizmos.DrawWireSphere(info.worldPosition, 0.5f);

                // Draw cell position
                Vector3 cellPos = new Vector3(
                    info.cell.x * 4f + 2f,
                    0.1f,
                    info.cell.y * 4f + 2f
                );
                Gizmos.DrawWireCube(cellPos, Vector3.one * 0.3f);
            }
        }

        #endregion

        #region Trap Placement

        [ContextMenu("Place Traps")]
        public void PlaceTraps()
        {
            if (!placeTraps) return;

            List<Vector2Int> validCells = GetValidTrapCells();
            int trapCount = Mathf.RoundToInt(validCells.Count * trapDensity);
            trapCount = Mathf.Clamp(trapCount, minTraps, maxTraps);

            Debug.Log($"[SpawnPlacerEngine] Placing {trapCount} traps (density: {trapDensity * 100}%)");

            int placed = 0;
            List<Vector2Int> trapPositions = new List<Vector2Int>();

            foreach (var cell in validCells)
            {
                if (placed >= trapCount) break;

                // Check minimum distance from other traps
                bool tooClose = false;
                foreach (var existingTrap in trapPositions)
                {
                    float distance = Vector2Int.Distance(cell, existingTrap);
                    if (distance < minDistanceBetweenTraps)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose) continue;

                // Place trap
                TrapType trapType = GetRandomTrapType();
                PlaceTrapAtCell(cell, trapType);
                trapPositions.Add(cell);
                placed++;
            }

            Debug.Log($"[SpawnPlacerEngine] Placed {placed} traps with min distance {minDistanceBetweenTraps}f");
        }

        private void PlaceTrapAtCell(Vector2Int cell, TrapType trapType)
        {
            Vector3 worldPos = CellToWorldPosition(cell);

            // Create trap
            GameObject trapGO = new GameObject($"Trap_{trapType}_{cell.x}_{cell.y}");
            trapGO.transform.position = worldPos;

            // Add trap behavior
            TrapBehavior trap = trapGO.AddComponent<TrapBehavior>();
            
            // Configure trap based on type
            float damage = GetTrapDamage(trapType);
            float radius = GetTrapRadius(trapType);
            float cooldown = GetTrapCooldown(trapType);

            trap.Initialize(trapType, damage, radius, cooldown);

            // Add to placed items
            var info = new PlacedItemInfo
            {
                cell = cell,
                worldPosition = worldPos,
                itemType = ItemType.Switch,
                instance = trapGO,
                prefab = null
            };
            _placedItems.Add(info);

            Debug.Log($"[SpawnPlacerEngine] Placed {trapType} trap at {cell}");
        }

        private TrapType GetRandomTrapType()
        {
            if (availableTrapTypes != null && availableTrapTypes.Length > 0)
            {
                return availableTrapTypes[Random.Range(0, availableTrapTypes.Length)];
            }

            // Default to spike trap
            return TrapType.Spike;
        }

        private float GetTrapDamage(TrapType type)
        {
            switch (type)
            {
                case TrapType.Spike: return 20f;
                case TrapType.Pit: return 30f;
                case TrapType.Dart: return 15f;
                case TrapType.Flame: return 25f;
                case TrapType.Poison: return 10f; // DoT
                case TrapType.Electric: return 35f;
                case TrapType.Ice: return 10f; // + slow
                case TrapType.Explosion: return 50f; // AOE
                default: return 20f;
            }
        }

        private float GetTrapRadius(TrapType type)
        {
            switch (type)
            {
                case TrapType.Spike: return 1.5f;
                case TrapType.Pit: return 2f;
                case TrapType.Dart: return 1f;
                case TrapType.Flame: return 2f;
                case TrapType.Poison: return 3f;
                case TrapType.Electric: return 2.5f;
                case TrapType.Ice: return 2f;
                case TrapType.Explosion: return 4f;
                default: return 1.5f;
            }
        }

        private float GetTrapCooldown(TrapType type)
        {
            switch (type)
            {
                case TrapType.Spike: return 2f;
                case TrapType.Pit: return 5f;
                case TrapType.Dart: return 3f;
                case TrapType.Flame: return 1f;
                case TrapType.Poison: return 0.5f; // Fast DoT
                case TrapType.Electric: return 4f;
                case TrapType.Ice: return 2f;
                case TrapType.Explosion: return 10f;
                default: return 2f;
            }
        }

        private List<Vector2Int> GetValidTrapCells()
        {
            List<Vector2Int> validCells = new List<Vector2Int>();

            for (int x = 0; x < _mazeGenerator.Width; x++)
            {
                for (int y = 0; y < _mazeGenerator.Height; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);

                    // Skip excluded cells
                    if (excludedCells != null && excludedCells.Contains(cell))
                        continue;

                    // Skip start and exit cells
                    if (cell == Vector2Int.zero)
                        continue;
                    if (cell.x == _mazeGenerator.Width - 1 && cell.y == _mazeGenerator.Height - 1)
                        continue;

                    // Check if cell is walkable
                    var cellData = _mazeGenerator.Grid[x, y];
                    if (cellData == Wall.Floor)
                    {
                        validCells.Add(cell);
                    }
                }
            }

            return validCells;
        }

        #endregion

        private void OnDestroy()
        {
            ClearAllItems();
        }
    }

    /// <summary>
    /// Placed item information.
    /// </summary>
    [System.Serializable]
    public class PlacedItemInfo
    {
        public Vector2Int cell;
        public ItemType itemType;
        public GameObject instance;
        public Vector3 worldPosition;
    }
}
