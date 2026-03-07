// ItemEngine.cs
// Central item management system - Plug-in-and-Out architecture
// Unity 6 compatible - UTF-8 encoding - Unix line endings
// Located in Core assembly for centralized item management
//
// Architecture:
//   ItemEngine (central manager)
//   ├── ItemBehavior (base class for all items)
//   │   ├── DoubleDoor (doors with glow/halo)
//   │   ├── ChestBehavior (treasure chests)
//   │   ├── ItemPickupBehavior (collectible items)
//   │   ├── SwitchBehavior (pressure plates, levers)
//   │   └── Custom items...
//   └── SpawnPlacerEngine (procedural placement)

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Central item management engine.
    /// Handles item registration, interaction, and lifecycle.
    /// Designed for plug-in-and-out modularity.
    /// </summary>
    public class ItemEngine : MonoBehaviour
    {
        private static ItemEngine _instance;
        public static ItemEngine Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObject<ItemEngine>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("ItemEngine");
                        _instance = go.AddComponent<ItemEngine>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Item Settings")]
        [SerializeField] private bool enableItems = true;
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private float interactionRange = 3f;

        [Header("Item Registry")]
        private List<ItemBehavior> _registeredItems;
        private Dictionary<Vector3, ItemBehavior> _itemLocations;
        private Dictionary<ItemType, List<ItemBehavior>> _itemsByType;

        // Events
        public System.Action<ItemBehavior> OnItemRegistered;
        public System.Action<ItemBehavior> OnItemUnregistered;
        public System.Action<ItemBehavior, GameObject> OnItemInteracted;
        public System.Action<ItemBehavior> OnItemCollected;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            _registeredItems = new List<ItemBehavior>();
            _itemLocations = new Dictionary<Vector3, ItemBehavior>();
            _itemsByType = new Dictionary<ItemType, List<ItemBehavior>>();

            Debug.Log("[ItemEngine] Initialized");
        }

        /// <summary>
        /// Register an item with the engine.
        /// </summary>
        public void RegisterItem(ItemBehavior item)
        {
            if (item == null || _registeredItems.Contains(item))
            {
                Debug.LogWarning("[ItemEngine] Cannot register null or duplicate item");
                return;
            }

            _registeredItems.Add(item);

            // Register by location
            Vector3 pos = item.transform.position;
            if (!_itemLocations.ContainsKey(pos))
            {
                _itemLocations[pos] = item;
            }

            // Register by type
            if (!_itemsByType.ContainsKey(item.ItemType))
            {
                _itemsByType[item.ItemType] = new List<ItemBehavior>();
            }
            _itemsByType[item.ItemType].Add(item);

            // Subscribe to events
            item.OnInteract += HandleItemInteract;
            item.OnCollect += HandleItemCollect;

            Debug.Log($"[ItemEngine] Registered item: {item.ItemType} at {pos}");
            OnItemRegistered?.Invoke(item);
        }

        /// <summary>
        /// Unregister an item from the engine.
        /// </summary>
        public void UnregisterItem(ItemBehavior item)
        {
            if (item == null || !_registeredItems.Contains(item))
            {
                Debug.LogWarning("[ItemEngine] Cannot unregister null or missing item");
                return;
            }

            // Unsubscribe from events
            item.OnInteract -= HandleItemInteract;
            item.OnCollect -= HandleItemCollect;

            // Remove from lists
            _registeredItems.Remove(item);
            _itemLocations.Remove(item.transform.position);

            if (_itemsByType.ContainsKey(item.ItemType))
            {
                _itemsByType[item.ItemType].Remove(item);
                if (_itemsByType[item.ItemType].Count == 0)
                {
                    _itemsByType.Remove(item.ItemType);
                }
            }

            Debug.Log($"[ItemEngine] Unregistered item: {item.ItemType}");
            OnItemUnregistered?.Invoke(item);
        }

        /// <summary>
        /// Get item at location.
        /// </summary>
        public ItemBehavior GetItemAt(Vector3 position)
        {
            return _itemLocations.TryGetValue(position, out var item) ? item : null;
        }

        /// <summary>
        /// Get all items of a specific type.
        /// </summary>
        public List<ItemBehavior> GetItemsByType(ItemType type)
        {
            return _itemsByType.TryGetValue(type, out var items) ? items : null;
        }

        /// <summary>
        /// Get all registered items.
        /// </summary>
        public List<ItemBehavior> GetAllItems() => new List<ItemBehavior>(_registeredItems);

        /// <summary>
        /// Enable all items.
        /// </summary>
        public void EnableAllItems()
        {
            foreach (var item in _registeredItems)
            {
                item.Enable();
            }
            Debug.Log("[ItemEngine] All items enabled");
        }

        /// <summary>
        /// Disable all items.
        /// </summary>
        public void DisableAllItems()
        {
            foreach (var item in _registeredItems)
            {
                item.Disable();
            }
            Debug.Log("[ItemEngine] All items disabled");
        }

        /// <summary>
        /// Find nearest item to a position.
        /// </summary>
        public ItemBehavior FindNearestItem(Vector3 position, float maxDistance = 10f)
        {
            ItemBehavior nearest = null;
            float nearestDist = float.MaxValue;

            foreach (var item in _registeredItems)
            {
                if (!item.CanInteract) continue;

                float dist = Vector3.Distance(position, item.transform.position);
                if (dist < nearestDist && dist <= maxDistance)
                {
                    nearest = item;
                    nearestDist = dist;
                }
            }

            return nearest;
        }

        private void HandleItemInteract(ItemBehavior item, GameObject interactor)
        {
            Debug.Log($"[ItemEngine] Item interacted: {item.ItemType} by {interactor.name}");
            OnItemInteracted?.Invoke(item, interactor);
        }

        private void HandleItemCollect(ItemBehavior item, GameObject collector)
        {
            Debug.Log($"[ItemEngine] Item collected: {item.ItemType} by {collector.name}");
            OnItemCollected?.Invoke(item);
        }

        #region Debug

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos || _registeredItems == null)
                return;

            foreach (var item in _registeredItems)
            {
                if (item == null) continue;

                Gizmos.color = item.ItemType switch
                {
                    ItemType.Door => Color.blue,
                    ItemType.Chest => Color.yellow,
                    ItemType.Pickup => Color.green,
                    ItemType.Switch => Color.red,
                    _ => Color.gray
                };

                Gizmos.DrawWireSphere(item.transform.position, 0.5f);

                // Draw interaction range
                Gizmos.color = new Color(gizmosColor.r, gizmosColor.g, gizmosColor.b, 0.3f);
                Gizmos.DrawWireSphere(item.transform.position, interactionRange);
            }
        }

        private Color gizmosColor = Color.white;

        #endregion
    }
}
