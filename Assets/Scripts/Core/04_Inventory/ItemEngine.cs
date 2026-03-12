// Copyright (C) 2026 Ocxyde
//
// This file is part of Code.Lavos.
//
// Code.Lavos is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Code.Lavos is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Code.Lavos.  If not, see <https://www.gnu.org/licenses/>.
// ItemEngine.cs
// Central item management system - Plug-in-and-Out architecture
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// CORE SYSTEM: All item behaviors plug into this central manager
// - BehaviorEngine (base class in Core/Base/)
//    DoubleDoor
//    ChestBehavior
//    ItemPickup
//    SwitchBehavior
//    Custom items...

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Central item management engine.
    /// All items plug into this system via BehaviorEngine base class.
    ///
    /// NULL HANDLING (Plug-in-Out Pattern):
    /// The Instance property returns null if:
    /// 1. ItemEngine component not found in scene
    /// 2. Application is quitting (OnDestroy was called)
    ///
    /// Usage Pattern:
    ///   ItemEngine.Instance?.RegisterItem(item);    // Safe - null coalescing
    ///   ItemEngine.Instance?.GetItemsOfType<T>();  // Safe - null coalescing
    ///
    /// Do NOT write:
    ///   ItemEngine.Instance.RegisterItem(item);     // UNSAFE - may NullRef
    ///
    /// This ensures the game gracefully continues even if ItemEngine
    /// is missing (useful for level editors, scene-only tests, etc.).
    /// </summary>
    public class ItemEngine : MonoBehaviour
    {
        private static ItemEngine _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        public static ItemEngine Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning("[ItemEngine] Instance already destroyed, returning null");
                    return null;
                }

                if (_instance != null) return _instance;

                lock (_lock)
                {
                    if (_instance != null) return _instance;

                    _instance = FindAnyObjectByType<ItemEngine>();
                    if (_instance == null)
                    {
                        // No ItemEngine found - log warning (don't create!)
                        Debug.LogWarning("[ItemEngine] No ItemEngine found! Add ItemEngine component to a GameObject in scene.");
                        return null;
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
        private List<BehaviorEngine> _registeredItems = new();
        private Dictionary<Vector3, BehaviorEngine> _itemLocations = new();
        private Dictionary<ItemType, List<BehaviorEngine>> _itemsByType = new();

        // Events
        public System.Action<BehaviorEngine> OnItemRegistered;
        public System.Action<BehaviorEngine> OnItemUnregistered;
        public System.Action<BehaviorEngine, GameObject> OnItemInteracted;
        public System.Action<BehaviorEngine> OnItemCollected;

        // Properties
        public bool EnableItems => enableItems;
        public int TotalItemCount => _registeredItems?.Count ?? 0;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            _registeredItems = new List<BehaviorEngine>();
            _itemLocations = new Dictionary<Vector3, BehaviorEngine>();
            _itemsByType = new Dictionary<ItemType, List<BehaviorEngine>>();

            Debug.Log("[ItemEngine] Initialized - Ready for plug-in items");
        }

        #region Item Management

        /// <summary>
        /// Register an item with the engine (called automatically by BehaviorEngine.Awake).
        /// </summary>
        public void RegisterItem(BehaviorEngine item)
        {
            if (item == null || _registeredItems.Contains(item))
            {
                Debug.LogWarning("[ItemEngine] Cannot register null or duplicate item");
                return;
            }

            _registeredItems.Add(item);

            // Add to location dictionary
            Vector3 pos = item.transform.position;
            if (!_itemLocations.ContainsKey(pos))
            {
                _itemLocations[pos] = item;
            }

            // Add to type dictionary
            if (!_itemsByType.ContainsKey(item.ItemType))
            {
                _itemsByType[item.ItemType] = new List<BehaviorEngine>();
            }
            _itemsByType[item.ItemType].Add(item);

            // Subscribe to item events
            item.OnInteract += HandleItemInteract;
            item.OnCollect += HandleItemCollect;

            OnItemRegistered?.Invoke(item);
            Debug.Log($"[ItemEngine] Registered item: {item.ItemType} at {pos}");
        }

        /// <summary>
        /// Unregister an item from the engine (called automatically by BehaviorEngine.OnDestroy).
        /// </summary>
        public void UnregisterItem(BehaviorEngine item)
        {
            if (item == null || !_registeredItems.Contains(item))
            {
                Debug.LogWarning("[ItemEngine] Cannot unregister null or missing item");
                return;
            }

            _registeredItems.Remove(item);

            // Remove from location dictionary
            Vector3 pos = item.transform.position;
            if (_itemLocations.ContainsKey(pos) && _itemLocations[pos] == item)
            {
                _itemLocations.Remove(pos);
            }

            // Remove from type dictionary
            if (_itemsByType.ContainsKey(item.ItemType))
            {
                _itemsByType[item.ItemType].Remove(item);
            }

            // Unsubscribe from item events
            item.OnInteract -= HandleItemInteract;
            item.OnCollect -= HandleItemCollect;

            OnItemUnregistered?.Invoke(item);
            Debug.Log($"[ItemEngine] Unregistered item: {item.ItemType}");
        }

        /// <summary>
        /// Get item at position.
        /// </summary>
        public BehaviorEngine GetItemAt(Vector3 position)
        {
            _itemLocations.TryGetValue(position, out BehaviorEngine item);
            return item;
        }

        /// <summary>
        /// Get all items of a specific type.
        /// </summary>
        public List<T> GetItemsOfType<T>() where T : BehaviorEngine
        {
            var result = new List<T>();
            foreach (var item in _registeredItems)
            {
                if (item is T specificItem)
                {
                    result.Add(specificItem);
                }
            }
            return result;
        }

        /// <summary>
        /// Get all items within range of a position.
        /// </summary>
        public List<BehaviorEngine> GetItemsInRange(Vector3 position, float range)
        {
            var result = new List<BehaviorEngine>();
            foreach (var item in _registeredItems)
            {
                if (Vector3.Distance(position, item.transform.position) <= range)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        /// <summary>
        /// Get nearest interactable item from position.
        /// </summary>
        public BehaviorEngine GetNearestInteractable(Vector3 position, float maxRange)
        {
            BehaviorEngine nearest = null;
            float nearestDist = maxRange;

            foreach (var item in _registeredItems)
            {
                if (!item.CanInteract) continue;

                float dist = Vector3.Distance(position, item.transform.position);
                if (dist < nearestDist)
                {
                    nearest = item;
                    nearestDist = dist;
                }
            }

            return nearest;
        }

        #endregion

        #region Global Operations

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
        /// Interact with nearest item from position.
        /// </summary>
        public bool InteractWithNearest(Vector3 position, GameObject interactor)
        {
            var item = GetNearestInteractable(position, interactionRange);
            if (item != null)
            {
                item.Interact(interactor);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Collect item at position.
        /// </summary>
        public bool CollectItemAt(Vector3 position, GameObject collector)
        {
            var item = GetItemAt(position);
            if (item != null && item.CanCollect)
            {
                item.Collect(collector);
                return true;
            }
            return false;
        }

        #endregion

        #region Event Handlers

        private void HandleItemInteract(BehaviorEngine item, GameObject interactor)
        {
            OnItemInteracted?.Invoke(item, interactor);
            Debug.Log($"[ItemEngine] Item interacted: {item.ItemType} by {interactor.name}");
        }

        private void HandleItemCollect(BehaviorEngine item, GameObject collector)
        {
            OnItemCollected?.Invoke(item);
            Debug.Log($"[ItemEngine] Item collected: {item.ItemType} by {collector.name}");

            // Auto-unregister collectible items
            if (item.DestroyOnCollect)
            {
                UnregisterItem(item);
            }
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Get item statistics.
        /// </summary>
        public ItemStatistics GetStatistics()
        {
            var stats = new ItemStatistics
            {
                totalItems = _registeredItems.Count,
                doors = GetItemCountByType(ItemType.Door),
                chests = GetItemCountByType(ItemType.Chest),
                pickups = GetItemCountByType(ItemType.Pickup),
                switches = GetItemCountByType(ItemType.Switch),
                interactableCount = 0,
                collectedCount = 0
            };

            foreach (var item in _registeredItems)
            {
                if (item.CanInteract) stats.interactableCount++;
                if (item.IsCollected) stats.collectedCount++;
            }

            return stats;
        }

        private int GetItemCountByType(ItemType type)
        {
            if (_itemsByType.TryGetValue(type, out var items))
            {
                return items.Count;
            }
            return 0;
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos || _registeredItems == null)
                return;

            foreach (var item in _registeredItems)
            {
                if (item == null) continue;

                // Draw item position
                Gizmos.color = item.CanInteract ? Color.green : Color.gray;
                if (item.IsCollected)
                    Gizmos.color = Color.yellow;

                Gizmos.DrawWireSphere(item.transform.position, 0.5f);

                // Draw interaction range
                if (item.CanInteract)
                {
                    Gizmos.color = new Color(0, 1, 0, 0.1f);
                    Gizmos.DrawWireSphere(item.transform.position, interactionRange);
                }
            }
        }

        #endregion

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                _applicationIsQuitting = true;
                
                // Destroy the GameObject when exiting play mode
                // This prevents "objects not cleaned up" warnings
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    #region Statistics

    public struct ItemStatistics
    {
        public int totalItems;
        public int doors;
        public int chests;
        public int pickups;
        public int switches;
        public int interactableCount;
        public int collectedCount;
    }

    #endregion
}
