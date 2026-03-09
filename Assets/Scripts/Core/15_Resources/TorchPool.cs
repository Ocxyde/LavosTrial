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
// TorchPool.cs
// REAL OBJECT POOLING for torches - Reuse instead of destroy
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-AND-OUT:
// - Pools torch objects for reuse (zero GC allocations)
// - Get() → Activate from pool (or create if empty)
// - Release() → Return to pool (disable, don't destroy)
// - ReleaseAll() → Return all to pool (ready for reuse)
//
// SETUP: Attach to the same GameObject as MazeRenderer.

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// TORCHPOOL - Real object pooling for torches.
    /// Reuses torch objects instead of creating/destroying (zero GC allocations).
    /// Performance: Pre-creates pool at start, reuses throughout game lifetime.
    /// </summary>
    public class TorchPool : MonoBehaviour
    {
        // ─── Pool Settings ─────────────────────────────────────────────────────
        [Header("Pool Settings")]
        [Tooltip("Pre-create this many torches at start (avoids runtime instantiation)")]
        [SerializeField] private int initialPoolSize = 60;

        [Tooltip("Expand pool if more torches needed (or fail if false)")]
        [SerializeField] private bool canExpand = true;

        [Tooltip("Pre-warm pool on Start (recommended for best performance)")]
        [SerializeField] private bool prewarmOnStart = true;

        // ─── Torch Prefab ──────────────────────────────────────────────────────
        [Header("Torch Prefab")]
        [Tooltip("Torch handle with EffectSocket + LightSocket inside")]
        [SerializeField] private GameObject torchHandlePrefab;
        
        // Public getter for LightPlacementEngine
        public GameObject TorchHandlePrefab => torchHandlePrefab;

        [Tooltip("Use BraseroFlame particle system")]
        [SerializeField] private bool useBraseroFlame = true;

        // ─── The Pool (inactive torches ready for reuse) ───────────────────────
        private readonly Queue<GameObject> _pool = new Queue<GameObject>();
        private readonly List<GameObject> _activeTorches = new List<GameObject>();

        // ─── Shared Materials ──────────────────────────────────────────────────
        private Material _sharedFlameMat;
        private Material _sharedHandleMat;
        private bool _materialsInitialized;

        // ─── Stats ─────────────────────────────────────────────────────────────
        public int PoolSize => _pool.Count;
        public int ActiveCount => _activeTorches.Count;
        public int TotalCreated => _activeTorches.Count + _pool.Count;
        public int PeakUsage { get; private set; }

        // ───────────────────────────────────────────────────────────────────────
        //  UNITY LIFECYCLE
        // ───────────────────────────────────────────────────────────────────────

        void Start()
        {
            if (prewarmOnStart && initialPoolSize > 0)
            {
                PrewarmPool(initialPoolSize);
                Debug.Log($"[TorchPool]  Pre-warmed {initialPoolSize} torches (zero GC at runtime)");
            }
        }

        void OnDestroy()
        {
            // Clean up all pooled objects
            foreach (var go in _pool)
            {
                if (go != null)
                    Destroy(go);
            }
            _pool.Clear();
            _activeTorches.Clear();
        }

        // ───────────────────────────────────────────────────────────────────────
        //  POOL INITIALIZATION
        // ───────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Pre-create pool objects at start (avoids runtime instantiation).
        /// </summary>
        public void PrewarmPool(int count)
        {
            Debug.Log($"[TorchPool] Pre-warming {count} torches...");

            for (int i = 0; i < count; i++)
            {
                var torch = CreateNewTorch();
                torch.SetActive(false);
                _pool.Enqueue(torch);
            }

            Debug.Log($"[TorchPool]  Pool pre-warmed: {count} torches ready");
        }

        // ───────────────────────────────────────────────────────────────────────
        //  GET TORCH FROM POOL (REAL POOLING)
        // ───────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Get a torch from pool (reuse) or create new if pool empty.
        /// ZERO GC allocations if pool has available torches.
        /// </summary>
        public TorchController Get(Vector3 position, Quaternion rotation,
                                   Transform parent, Texture2D[] flameFrames,
                                   Material flameMat, Material handleMat)
        {
            InitializeMaterials(flameMat, handleMat, flameFrames);

            GameObject go;

            //  REAL POOLING: Try to get from pool first
            if (_pool.Count > 0)
            {
                go = _pool.Dequeue();
                Debug.Log($"[TorchPool] REUSED from pool (remaining: {_pool.Count})");
            }
            else
            {
                // Pool empty - create new (only if canExpand)
                if (canExpand)
                {
                    go = CreateNewTorch();
                    Debug.Log($"[TorchPool]  Created new (pool was empty)");
                }
                else
                {
                    Debug.LogWarning("[TorchPool] Pool exhausted and canExpand=false!");
                    return null;
                }
            }

            // Configure torch
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.rotation = rotation;
            go.SetActive(true);

            // Get controller
            var ctrl = go.GetComponent<TorchController>();
            if (ctrl == null)
            {
                Debug.LogError("[TorchPool] TorchController component missing!");
                return null;
            }

            // Initialize and turn on
            if (useBraseroFlame)
            {
                var effectSocket = go.GetComponentInChildren<ParticleSystem>();
                var light = go.GetComponentInChildren<Light>();

                if (light != null)
                {
                    ctrl.InitializeBrasero(light, null);
                    ctrl.TurnOn();

                    if (effectSocket != null)
                    {
                        effectSocket.Play();
                        Debug.Log($"[TorchPool]  Torch at {position} - Light ON + Particles playing");
                    }
                    else
                    {
                        Debug.Log($"[TorchPool]  Torch at {position} - Light ON (no particles)");
                    }
                }
                else
                {
                    Debug.LogWarning($"[TorchPool]  Light missing at {position}!");
                    SetupSpriteMode(go, flameFrames, ctrl);
                }
            }
            else
            {
                SetupSpriteMode(go, flameFrames, ctrl);
            }

            _activeTorches.Add(go);

            // Track peak usage
            if (_activeTorches.Count > PeakUsage)
                PeakUsage = _activeTorches.Count;

            return ctrl;
        }

        // ───────────────────────────────────────────────────────────────────────
        //  RELEASE TORCH BACK TO POOL (REAL POOLING)
        // ───────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Release single torch back to pool (disable, don't destroy).
        /// ZERO GC allocations - torch is reused later.
        /// </summary>
        public void Release(TorchController controller)
        {
            if (controller == null || controller.gameObject == null)
                return;

            var go = controller.gameObject;

            // Turn off light
            controller.TurnOff();

            // Remove from active list
            _activeTorches.Remove(go);

            //  REAL POOLING: Return to pool instead of destroying
            go.SetActive(false);
            go.transform.SetParent(transform); // Reparent to pool container
            _pool.Enqueue(go);

            Debug.Log($"[TorchPool] Returned to pool (size: {_pool.Count})");
        }

        // ───────────────────────────────────────────────────────────────────────
        //  RELEASE ALL TORCHES TO POOL
        // ───────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Release all active torches back to pool (ready for reuse).
        /// ZERO GC allocations - all torches are reused later.
        /// Call before maze regeneration.
        /// </summary>
        public void ReleaseAll()
        {
            Debug.Log($"[TorchPool] Releasing {_activeTorches.Count} torches to pool...");

            // Copy list to avoid modification during iteration
            var toRelease = new List<GameObject>(_activeTorches);

            foreach (var go in toRelease)
            {
                if (go != null)
                {
                    var ctrl = go.GetComponent<TorchController>();
                    Release(ctrl);
                }
            }

            Debug.Log($"[TorchPool]  All torches returned to pool (pool size: {_pool.Count})");
        }

        // ───────────────────────────────────────────────────────────────────────
        //  DESTROY ALL (end of session only)
        // ───────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Destroy all torches (pool + active). Only call at end of game session.
        /// </summary>
        public void DestroyAll()
        {
            foreach (var go in _pool)
            {
                if (go != null)
                    Destroy(go);
            }
            _pool.Clear();

            foreach (var go in _activeTorches)
            {
                if (go != null)
                    Destroy(go);
            }
            _activeTorches.Clear();

            Debug.Log("[TorchPool]  All torches destroyed");
        }

        // ───────────────────────────────────────────────────────────────────────
        //  INTERNAL HELPERS
        // ───────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a new torch object (called when pool is empty and canExpand=true).
        /// </summary>
        private GameObject CreateNewTorch()
        {
            GameObject go;

            if (torchHandlePrefab != null)
            {
                // Instantiate from prefab
                go = Instantiate(torchHandlePrefab, transform);
            }
            else
            {
                // Build from scratch
                go = BuildTorchObject();
                go.transform.SetParent(transform);
            }

            go.name = "Torch_Pooled";
            return go;
        }

        /// <summary>
        /// Initializes shared materials if not already done.
        /// </summary>
        private void InitializeMaterials(Material flameMat, Material handleMat, Texture2D[] flameFrames)
        {
            if (_materialsInitialized) return;

            if (handleMat != null)
            {
                _sharedHandleMat = new Material(handleMat);
            }

            if (flameMat != null)
            {
                _sharedFlameMat = new Material(flameMat);
            }
            else if (flameFrames != null && flameFrames.Length > 0 && flameFrames[0] != null)
            {
                Debug.LogWarning("[TorchPool] Flame material is null, creating fallback material");
                var fallbackShader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Transparent");
                if (fallbackShader != null)
                {
                    _sharedFlameMat = new Material(fallbackShader);
                    _sharedFlameMat.mainTexture = flameFrames[0];
                }
            }

            _materialsInitialized = true;
        }

        private void SetupSpriteMode(GameObject go, Texture2D[] flameFrames, TorchController ctrl)
        {
            var flameMR = go.transform.Find("Flame")?.GetComponent<Renderer>();
            var light = go.transform.Find("FlameLight")?.GetComponent<Light>();
            ctrl.Initialize(flameFrames, flameMR, light);
        }

        // ───────────────────────────────────────────────────────────────────────
        //  TORCH CONSTRUCTION (fallback if no prefab)
        // ───────────────────────────────────────────────────────────────────────

        private GameObject BuildTorchObject()
        {
            var torchGO = new GameObject("Torch");

            // ─── Handle (stick) ────────────────────────────────────────────────
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handle.name = "Handle";
            handle.transform.SetParent(torchGO.transform);
            handle.transform.localPosition = new Vector3(0f, 0f, 0f);
            handle.transform.localRotation = Quaternion.Euler(25f, 0f, 0f);
            handle.transform.localScale = new Vector3(0.08f, 0.35f, 0.08f);

            if (_sharedHandleMat != null)
                handle.GetComponent<MeshRenderer>().sharedMaterial = _sharedHandleMat;

            Destroy(handle.GetComponent<BoxCollider>());

            if (useBraseroFlame)
            {
                // ─── Brasero Flame (Particle System) ───────────────────────────
                var flame = new GameObject("BraseroFlame");
                flame.transform.SetParent(torchGO.transform);
                flame.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                flame.transform.localRotation = Quaternion.identity;
                flame.transform.localScale = new Vector3(1f, 1f, 1f);
                flame.AddComponent<BraseroFlame>();

                // ─── Light ─────────────────────────────────────────────────────
                var lightGO = new GameObject("FlameLight");
                lightGO.transform.SetParent(torchGO.transform);
                lightGO.transform.localPosition = new Vector3(0f, 0.35f, 0f);

                var pointLight = lightGO.AddComponent<Light>();
                pointLight.type = LightType.Point;
                pointLight.range = 15f;
                pointLight.intensity = 5f;
                pointLight.color = new Color(1f, 0.7f, 0.3f);
                pointLight.shadows = LightShadows.None;  //  OPTIMIZED: No shadows (performance)
                pointLight.enabled = true;
                pointLight.bounceIntensity = 1.5f;

                // ─── Controller ────────────────────────────────────────────────
                var ctrl = torchGO.AddComponent<TorchController>();
                ctrl.InitializeBrasero(pointLight, flame.GetComponent<BraseroFlame>());
                ctrl.TurnOn();
            }
            else
            {
                // ─── Flame (billboard 2D pixel art) ────────────────────────────
                var flame = GameObject.CreatePrimitive(PrimitiveType.Quad);
                flame.name = "Flame";
                flame.transform.SetParent(torchGO.transform);
                flame.transform.localPosition = new Vector3(0f, 0.22f, 0.06f);
                flame.transform.localRotation = Quaternion.Euler(25f, 0f, 0f);
                flame.transform.localScale = new Vector3(0.3f, 0.45f, 1f);

                if (_sharedFlameMat != null)
                    flame.GetComponent<MeshRenderer>().sharedMaterial = _sharedFlameMat;

                Destroy(flame.GetComponent<MeshCollider>());

                // ─── Light ─────────────────────────────────────────────────────
                var lightGO = new GameObject("FlameLight");
                lightGO.transform.SetParent(torchGO.transform);
                lightGO.transform.localPosition = new Vector3(0f, 0.35f, 0f);

                var pointLight = lightGO.AddComponent<Light>();
                pointLight.type = LightType.Point;
                pointLight.range = 15f;
                pointLight.intensity = 5f;
                pointLight.color = new Color(1f, 0.7f, 0.3f);
                pointLight.shadows = LightShadows.None;  //  OPTIMIZED: No shadows (performance)
                pointLight.enabled = true;
                pointLight.bounceIntensity = 1.5f;

                // ─── Controller ────────────────────────────────────────────────
                torchGO.AddComponent<TorchController>();
            }

            return torchGO;
        }

        // ───────────────────────────────────────────────────────────────────────
        //  DEBUG / STATS
        // ───────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Get pool statistics for debugging.
        /// </summary>
        public string GetStats()
        {
            return $"[TorchPool] Active: {ActiveCount} | Pool: {PoolSize} | Total: {TotalCreated} | Peak: {PeakUsage}";
        }
    }
}
