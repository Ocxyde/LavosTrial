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
// LightEmittingPool.cs
// Object pool for ANY light-emitting objects (lamps, candles, lanterns, braziers, etc.)
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-AND-OUT:
// - Unified pooling system for all light-emitting prefabs
// - Supports multiple light types via LightEmittingController
// - Zero allocation on reuse (objects disabled, not destroyed)
// - Auto-registers lights with LightEngine when turned ON
//
// USAGE:
//   1. Add this component to a manager GameObject
//   2. Assign prefabs for each light type you need
//   3. Call Get() to spawn a light, Release() to return it

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// LightEmittingPool - Universal pool for light-emitting objects.
    /// Pools any prefab with LightEmittingController component.
    /// </summary>
    public class LightEmittingPool : MonoBehaviour
    {
        #region Inspector Settings

        [Header("Pool Settings")]
        [Tooltip("Initial pool size per type")]
        [SerializeField] private int initialPoolSize = 5;

        [Tooltip("Maximum pool size per type")]
        [SerializeField] private int maxPoolSize = 20;

        [Tooltip("Auto-expand pool if exhausted")]
        [SerializeField] private bool autoExpand = true;

        [Header("Light Prefabs")]
        [Tooltip("Candle prefab")]
        [SerializeField] private GameObject candlePrefab;

        [Tooltip("Lamp prefab")]
        [SerializeField] private GameObject lampPrefab;

        [Tooltip("Lantern prefab")]
        [SerializeField] private GameObject lanternPrefab;

        [Tooltip("Brazier prefab")]
        [SerializeField] private GameObject brazierPrefab;

        [Tooltip("Torch prefab")]
        [SerializeField] private GameObject torchPrefab;

        [Tooltip("Chandelier prefab")]
        [SerializeField] private GameObject chandelierPrefab;

        [Tooltip("Fireplace prefab")]
        [SerializeField] private GameObject fireplacePrefab;

        [Tooltip("Magic light prefab")]
        [SerializeField] private GameObject magicLightPrefab;

        [Tooltip("Custom prefabs (assign LightEmittingController on each)")]
        [SerializeField] private List<GameObject> customPrefabs = new List<GameObject>();

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        #endregion

        #region Pool Data

        private class LightPool
        {
            public List<GameObject> available = new List<GameObject>();
            public List<LightEmittingController> active = new List<LightEmittingController>();
            public Transform inactiveParent;
        }

        private Dictionary<LightEmittingType, LightPool> _pools = new Dictionary<LightEmittingType, LightPool>();
        private Transform _poolRoot;

        #endregion

        #region Unity Lifecycle

        void Awake()
        {
            //  ACCEPTABLE: Create pool root for organization
            // TODO (Future): Could find existing instead
            _poolRoot = new GameObject("LightEmittingPool_Inactive").transform;
            _poolRoot.SetParent(transform);
            _poolRoot.gameObject.SetActive(false);

            // Initialize pools for each type
            InitializePool(LightEmittingType.Candle, candlePrefab);
            InitializePool(LightEmittingType.Lamp, lampPrefab);
            InitializePool(LightEmittingType.Lantern, lanternPrefab);
            InitializePool(LightEmittingType.Brazier, brazierPrefab);
            InitializePool(LightEmittingType.Torch, torchPrefab);
            InitializePool(LightEmittingType.Chandelier, chandelierPrefab);
            InitializePool(LightEmittingType.Fireplace, fireplacePrefab);
            InitializePool(LightEmittingType.Magic, magicLightPrefab);

            // Initialize custom prefabs
            for (int i = 0; i < customPrefabs.Count; i++)
            {
                if (customPrefabs[i] != null)
                {
                    var controller = customPrefabs[i].GetComponent<LightEmittingController>();
                    if (controller != null)
                    {
                        InitializePool(controller.LightType, customPrefabs[i]);
                    }
                    else
                    {
                        Debug.LogWarning($"[LightEmittingPool] Custom prefab {i} missing LightEmittingController");
                    }
                }
            }

            Log($"[LightEmittingPool] Initialized with {initialPoolSize} per type");
        }

        void OnDestroy()
        {
            ReleaseAll();
            
            if (_poolRoot != null)
            {
                Destroy(_poolRoot.gameObject);
            }
        }

        #endregion

        #region Pool Management

        /// <summary>
        /// Initialize a pool for a specific light type.
        /// </summary>
        private void InitializePool(LightEmittingType type, GameObject prefab)
        {
            if (prefab == null)
            {
                Log($"[LightEmittingPool] No prefab assigned for {type}");
                return;
            }

            //  ACCEPTABLE: Create pool parent for organization
            // TODO (Future): Could find existing instead
            var pool = new LightPool
            {
                inactiveParent = new GameObject($"{type}_Inactive").transform
            };
            pool.inactiveParent.SetParent(_poolRoot);

            // Pre-instantiate pool objects
            for (int i = 0; i < initialPoolSize; i++)
            {
                var go = Instantiate(prefab, pool.inactiveParent);
                if (go == null)
                {
                    Debug.LogError($"[LightEmittingPool] Failed to instantiate {type} prefab for pool!");
                    continue;
                }
                go.name = $"{type}_{i}";
                go.SetActive(false);
                pool.available.Add(go);
            }

            _pools[type] = pool;
            Log($"[LightEmittingPool] Pool created for {type} ({initialPoolSize} objects)");
        }

        /// <summary>
        /// Get a light-emitting object from the pool.
        /// </summary>
        /// <param name="type">Type of light to get</param>
        /// <param name="position">World position</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="parent">Optional parent transform</param>
        /// <param name="startOn">Start in ON state</param>
        /// <returns>LightEmittingController or null if unavailable</returns>
        public LightEmittingController Get(LightEmittingType type, Vector3 position, 
                                            Quaternion rotation = default, 
                                            Transform parent = null, 
                                            bool startOn = true)
        {
            if (!_pools.ContainsKey(type))
            {
                Debug.LogError($"[LightEmittingPool] No pool for type {type}");
                return null;
            }

            var pool = _pools[type];
            GameObject go;

            if (pool.available.Count > 0)
            {
                // Get from pool
                go = pool.available[pool.available.Count - 1];
                pool.available.RemoveAt(pool.available.Count - 1);
            }
            else if (autoExpand && GetActiveCount(type) < maxPoolSize)
            {
                // Create new (auto-expand)
                var prefab = GetPrefabForType(type);
                if (prefab == null) return null;

                go = Instantiate(prefab, pool.inactiveParent);
                if (go == null)
                {
                    Debug.LogError($"[LightEmittingPool] Failed to instantiate {type} prefab (auto-expand)!");
                    return null;
                }
                go.name = $"{type}_{pool.active.Count}";
                Log($"[LightEmittingPool] Auto-expanded pool for {type}");
            }
            else
            {
                Debug.LogWarning($"[LightEmittingPool] Pool exhausted for {type} (max: {maxPoolSize})");
                return null;
            }

            // Configure and activate
            go.transform.parent = parent;
            go.transform.position = position;
            go.transform.rotation = rotation == default ? Quaternion.identity : rotation;
            go.SetActive(true);

            // Get controller and turn on
            var controller = go.GetComponent<LightEmittingController>();
            if (controller == null)
            {
                Debug.LogError($"[LightEmittingPool] {type} missing LightEmittingController");
                return null;
            }

            if (startOn)
            {
                controller.TurnOn();
            }

            pool.active.Add(controller);
            Log($"[LightEmittingPool] Got {type} at {position}");
            return controller;
        }

        /// <summary>
        /// Get a light with default rotation.
        /// </summary>
        public LightEmittingController Get(LightEmittingType type, Vector3 position, 
                                            Transform parent = null, bool startOn = true)
        {
            return Get(type, position, Quaternion.identity, parent, startOn);
        }

        /// <summary>
        /// Release a light back to the pool.
        /// </summary>
        public void Release(LightEmittingController controller)
        {
            if (controller == null) return;

            var type = controller.LightType;
            if (!_pools.ContainsKey(type)) return;

            var pool = _pools[type];
            
            // Turn off (unregisters from LightEngine)
            controller.TurnOff();

            // Return to pool
            var go = controller.gameObject;
            go.SetActive(false);
            go.transform.SetParent(pool.inactiveParent);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            pool.available.Add(go);
            pool.active.Remove(controller);

            Log($"[LightEmittingPool] Released {type}");
        }

        /// <summary>
        /// Release all lights of a specific type.
        /// </summary>
        public void ReleaseAll(LightEmittingType type)
        {
            if (!_pools.ContainsKey(type)) return;

            var pool = _pools[type];
            
            // Copy list to avoid modification during iteration
            var toRelease = new List<LightEmittingController>(pool.active);
            foreach (var controller in toRelease)
            {
                Release(controller);
            }

            Log($"[LightEmittingPool] Released all {type}");
        }

        /// <summary>
        /// Release ALL lights of ALL types.
        /// </summary>
        [ContextMenu("Release All Lights")]
        public void ReleaseAll()
        {
            foreach (var kvp in _pools)
            {
                var pool = kvp.Value;
                var toRelease = new List<LightEmittingController>(pool.active);
                foreach (var controller in toRelease)
                {
                    Release(controller);
                }
            }

            Log($"[LightEmittingPool] Released all lights");
        }

        /// <summary>
        /// Turn all active lights of a type ON.
        /// </summary>
        [ContextMenu("Turn All ON")]
        public void TurnAllOn(LightEmittingType type)
        {
            if (!_pools.ContainsKey(type)) return;

            foreach (var controller in _pools[type].active)
            {
                if (controller != null)
                    controller.TurnOn();
            }

            Log($"[LightEmittingPool] Turned ON all {type}");
        }

        /// <summary>
        /// Turn all active lights of a type OFF.
        /// </summary>
        [ContextMenu("Turn All OFF")]
        public void TurnAllOff(LightEmittingType type)
        {
            if (!_pools.ContainsKey(type)) return;

            foreach (var controller in _pools[type].active)
            {
                if (controller != null)
                    controller.TurnOff();
            }

            Log($"[LightEmittingPool] Turned OFF all {type}");
        }

        /// <summary>
        /// Toggle all active lights of a type.
        /// </summary>
        [ContextMenu("Toggle All")]
        public void ToggleAll(LightEmittingType type)
        {
            if (!_pools.ContainsKey(type)) return;

            foreach (var controller in _pools[type].active)
            {
                if (controller != null)
                    controller.Toggle();
            }
        }

        #endregion

        #region Utilities

        private GameObject GetPrefabForType(LightEmittingType type)
        {
            return type switch
            {
                LightEmittingType.Candle => candlePrefab,
                LightEmittingType.Lamp => lampPrefab,
                LightEmittingType.Lantern => lanternPrefab,
                LightEmittingType.Brazier => brazierPrefab,
                LightEmittingType.Torch => torchPrefab,
                LightEmittingType.Chandelier => chandelierPrefab,
                LightEmittingType.Fireplace => fireplacePrefab,
                LightEmittingType.Magic => magicLightPrefab,
                _ => null
            };
        }

        /// <summary>
        /// Get count of active lights for a type.
        /// </summary>
        public int GetActiveCount(LightEmittingType type)
        {
            if (!_pools.ContainsKey(type)) return 0;
            return _pools[type].active.Count;
        }

        /// <summary>
        /// Get count of available lights for a type.
        /// </summary>
        public int GetAvailableCount(LightEmittingType type)
        {
            if (!_pools.ContainsKey(type)) return 0;
            return _pools[type].available.Count;
        }

        /// <summary>
        /// Get total active lights across all types.
        /// </summary>
        public int GetTotalActiveCount()
        {
            int total = 0;
            foreach (var pool in _pools.Values)
            {
                total += pool.active.Count;
            }
            return total;
        }

        /// <summary>
        /// Get total available lights across all types.
        /// </summary>
        public int GetTotalAvailableCount()
        {
            int total = 0;
            foreach (var pool in _pools.Values)
            {
                total += pool.available.Count;
            }
            return total;
        }

        /// <summary>
        /// Check if a light type is available.
        /// </summary>
        public bool IsTypeAvailable(LightEmittingType type)
        {
            if (!_pools.ContainsKey(type)) return false;
            return _pools[type].available.Count > 0 || autoExpand;
        }

        private void Log(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log(message);
            }
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        void OnGUI()
        {
            if (!showDebugLogs) return;

            GUILayout.BeginArea(new UnityEngine.Rect(10, 10, 300, 400));
            GUILayout.BeginVertical("box");
            GUILayout.Label(" Light Pool Stats ");

            foreach (var kvp in _pools)
            {
                GUILayout.Label($"{kvp.Key}: Active={kvp.Value.active.Count}, Available={kvp.Value.available.Count}");
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
#endif

        #endregion
    }
}
