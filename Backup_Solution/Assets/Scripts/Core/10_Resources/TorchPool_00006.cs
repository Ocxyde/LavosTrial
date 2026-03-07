// TorchPool.cs
// Object pool for torch prefabs
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Ressources system - torch pooling

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// TORCHPOOL - Object pool for torch objects in the maze.
    ///
    /// FEATURES:
    /// - Torches are created once and disabled (not destroyed) for zero allocation on reuse
    /// - Automatically creates new torches if pool is exhausted
    /// - Supports both BraseroFlame (particle) and sprite-based flame modes
    /// - Shared materials to reduce memory footprint
    ///
    /// SETUP: Attach to the same GameObject as MazeRenderer.
    /// </summary>
    public class TorchPool : MonoBehaviour
    {
        // ─── Pool ──────────────────────────────────────────────────────────────
        private readonly List<GameObject> _pool = new List<GameObject>();
        private readonly List<TorchController> _active = new List<TorchController>();
        private Transform _poolRoot;

        // ─── Shared Materials (avoid allocations per torch) ────────────────────
        private Material _sharedFlameMat;
        private Material _sharedHandleMat;
        private bool _materialsInitialized;

        // ─── Brasero Flame ─────────────────────────────────────────────────────
        [Tooltip("Use BraseroFlame particle system instead of sprite-based flames")]
        [SerializeField] private bool useBraseroFlame = true;

        // ─── Unity Lifecycle ───────────────────────────────────────────────────
        void Awake()
        {
            _poolRoot = new GameObject("TorchPool_Inactive").transform;
            _poolRoot.SetParent(transform);
            _poolRoot.gameObject.SetActive(false);
        }

        // ───────────────────────────────────────────────────────────────────────
        //  GET A TORCH (from pool or new creation)
        // ───────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets a torch from the pool or creates a new one, positioned and ready to use.
        /// </summary>
        /// <param name="position">World position for the torch</param>
        /// <param name="rotation">Rotation for the torch</param>
        /// <param name="activeParent">Parent transform for active torches</param>
        /// <param name="flameFrames">Flame animation frames (for sprite mode)</param>
        /// <param name="flameMat">Flame material (optional, fallback created if null)</param>
        /// <param name="handleMat">Handle material</param>
        /// <returns>TorchController for the activated torch</returns>
        public TorchController Get(Vector3 position, Quaternion rotation,
                                   Transform activeParent, Texture2D[] flameFrames,
                                   Material flameMat, Material handleMat)
        {
            InitializeMaterials(flameMat, handleMat, flameFrames);

            GameObject go;

            if (_pool.Count > 0)
            {
                // Reuse from pool
                go = _pool[_pool.Count - 1];
                _pool.RemoveAt(_pool.Count - 1);
            }
            else
            {
                // Create new torch
                go = BuildTorchObject();
            }

            // Reposition and attach to active parent
            go.transform.SetParent(activeParent);
            go.transform.position = position;
            go.transform.rotation = rotation;
            go.SetActive(true);

            // Initialize controller
            var ctrl = go.GetComponent<TorchController>();
            
            if (ctrl == null)
            {
                Debug.LogError("[TorchPool] TorchController component missing!");
                return null;
            }

            if (useBraseroFlame)
            {
                var braseroFlame = go.transform.Find("BraseroFlame")?.GetComponent<BraseroFlame>();
                var light = go.transform.Find("FlameLight")?.GetComponent<Light>();

                if (braseroFlame != null && light != null)
                {
                    ctrl.InitializeBrasero(light, braseroFlame);
                }
                else
                {
                    Debug.LogWarning("[TorchPool] BraseroFlame setup incomplete, falling back to sprite mode");
                    SetupSpriteMode(go, flameFrames, ctrl);
                }
            }
            else
            {
                SetupSpriteMode(go, flameFrames, ctrl);
            }

            // NOTE: Light registration is handled by TorchController.TurnOn()
            // The torch starts in its default state (ON by default)
            // Don't register here - let the controller manage its own state

            _active.Add(ctrl);
            return ctrl;
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
                // Fallback: create basic material from texture
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
        //  RELEASE ALL ACTIVE TORCHES → back to pool
        // ───────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Deactivates all active torches and returns them to the pool.
        /// Call before each maze regeneration.
        /// </summary>
        public void ReleaseAll()
        {
            foreach (var ctrl in _active)
            {
                if (ctrl == null) continue;
                var go = ctrl.gameObject;

                // Turn off torch (this unregisters from LightEngine)
                ctrl.TurnOff();

                // Deactivate and return to pool
                go.SetActive(false);
                go.transform.SetParent(_poolRoot);
                _pool.Add(go);
            }
            _active.Clear();
            Debug.Log($"[TorchPool] {_pool.Count} torch(es) returned to pool.");
        }

        // ───────────────────────────────────────────────────────────────────────
        //  COMPLETE DESTRUCTION (end of session)
        // ───────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Destroys all torches (pool + active). Call at end of game session.
        /// </summary>
        public void DestroyAll()
        {
            ReleaseAll();
            foreach (var go in _pool)
                if (go != null) Destroy(go);
            _pool.Clear();

            if (_sharedFlameMat != null) { Destroy(_sharedFlameMat); _sharedFlameMat = null; }
            if (_sharedHandleMat != null) { Destroy(_sharedHandleMat); _sharedHandleMat = null; }
            _materialsInitialized = false;
        }

        void OnDestroy() => DestroyAll();

        // ───────────────────────────────────────────────────────────────────────
        //  TORCH CONSTRUCTION (internal)
        // ───────────────────────────────────────────────────────────────────────

        private GameObject BuildTorchObject()
        {
            var torchGO = new GameObject("Torch");

            // ─── DEBUG: Visible sphere to see torch position ───────────────────
            var debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            debugSphere.name = "DebugVisual";
            debugSphere.transform.SetParent(torchGO.transform);
            debugSphere.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            debugSphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); // Bigger = easier to see!
            var sphereRenderer = debugSphere.GetComponent<MeshRenderer>();
            if (sphereRenderer != null)
            {
                // Use Unlit/Color shader - always available, no texture needed
                var debugMat = new Material(Shader.Find("Unlit/Color"));
                if (debugMat == null || debugMat.shader == null)
                {
                    // Fallback to Standard if Unlit/Color not found
                    debugMat = new Material(Shader.Find("Standard"));
                }
                if (debugMat != null && debugMat.shader != null)
                {
                    debugMat.color = Color.red;
                    sphereRenderer.material = debugMat;
                }
            }
            Destroy(debugSphere.GetComponent<SphereCollider>());
            // ────────────────────────────────────────────────────────────────────

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
                Debug.Log($"[TorchPool] Creating BraseroFlame torch");
                
                // ─── Brasero Flame (Particle System) ───────────────────────────
                var flame = new GameObject("BraseroFlame");
                flame.transform.SetParent(torchGO.transform);
                flame.transform.localPosition = new Vector3(0f, 0.25f, 0.05f);
                flame.transform.localRotation = Quaternion.identity;
                var brasero = flame.AddComponent<BraseroFlame>();
                Debug.Log($"[TorchPool] BraseroFlame component added");

                // ─── FALLBACK: Simple visible flame quad ───────────────────────
                var flameQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                flameQuad.name = "FlameVisual";
                flameQuad.transform.SetParent(torchGO.transform);
                // Move it FORWARD from wall (not inward!) and UP
                flameQuad.transform.localPosition = new Vector3(0f, 0.45f, 0.2f); // 20cm OUT from wall
                flameQuad.transform.localRotation = Quaternion.identity; // Face camera
                flameQuad.transform.localScale = new Vector3(0.6f, 0.8f, 1f); // BIGGER!
                var quadRenderer = flameQuad.GetComponent<MeshRenderer>();
                Debug.Log($"[TorchPool] Flame quad created at {flameQuad.transform.localPosition}, scale: {flameQuad.transform.localScale}");
                
                if (quadRenderer != null)
                {
                    var flameMat = new Material(Shader.Find("Unlit/Color"));
                    Debug.Log($"[TorchPool] Unlit/Color shader: {Shader.Find("Unlit/Color") != null}");
                    
                    if (flameMat == null || flameMat.shader == null)
                    {
                        Debug.Log("[TorchPool] Unlit/Color not found, trying Standard");
                        flameMat = new Material(Shader.Find("Standard"));
                    }
                    if (flameMat != null && flameMat.shader != null)
                    {
                        flameMat.color = new Color(1f, 0.5f, 0.1f, 1f); // Orange flame
                        quadRenderer.material = flameMat;
                        quadRenderer.enabled = true; // FORCE ENABLE
                        Debug.Log($"[TorchPool] Flame material applied: {flameMat.shader.name}");
                        Debug.Log($"[TorchPool] Quad renderer enabled: {quadRenderer.enabled}");
                        Debug.Log($"[TorchPool] Quad GameObject active: {flameQuad.activeSelf}");
                        Debug.Log($"[TorchPool] Quad world position: {flameQuad.transform.position}");
                    }
                    else
                    {
                        Debug.LogError("[TorchPool] FAILED to create flame material!");
                    }
                }
                else
                {
                    Debug.LogError("[TorchPool] FAILED to get MeshRenderer from quad!");
                }
                Destroy(flameQuad.GetComponent<MeshCollider>());
                
                // ─── VERIFY TORCH GAMEOBJECT ───────────────────────────────────
                Debug.Log($"[TorchPool] Torch GO: {torchGO.name}, Active: {torchGO.activeSelf}");
                Debug.Log($"[TorchPool] Torch children: {torchGO.transform.childCount}");
                for (int i = 0; i < torchGO.transform.childCount; i++)
                {
                    var child = torchGO.transform.GetChild(i);
                    Debug.Log($"[TorchPool]   Child {i}: {child.name}, Active: {child.gameObject.activeSelf}");
                }
                // ────────────────────────────────────────────────────────────────

                // ─── Light ─────────────────────────────────────────────────────
                var lightGO = new GameObject("FlameLight");
                lightGO.transform.SetParent(torchGO.transform);
                lightGO.transform.localPosition = new Vector3(0f, 0.4f, 0.08f);

                var pointLight = lightGO.AddComponent<Light>();
                pointLight.type = LightType.Point;
                pointLight.range = 10f;
                pointLight.intensity = 2.5f;
                pointLight.color = new Color(1f, 0.6f, 0.3f);
                pointLight.shadows = LightShadows.None;

                // ─── Controller ────────────────────────────────────────────────
                var ctrl = torchGO.AddComponent<TorchController>();
                ctrl.InitializeBrasero(pointLight, flame.GetComponent<BraseroFlame>());
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
                lightGO.transform.localPosition = new Vector3(0f, 0.3f, 0.08f);

                var pointLight = lightGO.AddComponent<Light>();
                pointLight.type = LightType.Point;
                pointLight.range = 10f;
                pointLight.intensity = 2.5f;
                pointLight.color = new Color(1f, 0.6f, 0.3f);
                pointLight.shadows = LightShadows.None;

                // ─── Controller ────────────────────────────────────────────────
                torchGO.AddComponent<TorchController>();
            }

            return torchGO;
        }

        // ─── Stats (debug) ─────────────────────────────────────────────────────

        /// <summary>Number of currently active torches</summary>
        public int ActiveCount => _active.Count;

        /// <summary>Number of torches in the inactive pool</summary>
        public int PooledCount => _pool.Count;

        /// <summary>Total torches (active + pooled)</summary>
        public int TotalCount => _active.Count + _pool.Count;

        // ─── Debug Controls ────────────────────────────────────────────────────

        /// <summary>
        /// Turn all active torches ON.
        /// </summary>
        [ContextMenu("Turn All Torches ON")]
        public void TurnAllOn()
        {
            foreach (var ctrl in _active)
            {
                if (ctrl != null)
                    ctrl.TurnOn();
            }
            Debug.Log($"[TorchPool] Turned ON {_active.Count} torches");
        }

        /// <summary>
        /// Turn all active torches OFF.
        /// </summary>
        [ContextMenu("Turn All Torches OFF")]
        public void TurnAllOff()
        {
            foreach (var ctrl in _active)
            {
                if (ctrl != null)
                    ctrl.TurnOff();
            }
            Debug.Log($"[TorchPool] Turned OFF {_active.Count} torches");
        }

        /// <summary>
        /// Toggle all active torches.
        /// </summary>
        [ContextMenu("Toggle All Torches")]
        public void ToggleAll()
        {
            foreach (var ctrl in _active)
            {
                if (ctrl != null)
                    ctrl.Toggle();
            }
        }
    }
}
