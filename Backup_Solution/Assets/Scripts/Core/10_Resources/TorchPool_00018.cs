// TorchPool.cs
// Single torch instancing system - ONE torch prefab, instanced at all wall positions
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-AND-OUT:
// - Creates ONE torch with particles + light
// - Instances it at all wall positions
// - All instances share same visual/light settings
// - No pooling - just position instances
//
// SETUP: Attach to the same GameObject as MazeRenderer.

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// TORCHPOOL - Single torch instancing system.
    /// Creates ONE torch prefab and instances it at all wall positions.
    /// </summary>
    public class TorchPool : MonoBehaviour
    {
        // ─── Single Torch Prefab ───────────────────────────────────────────────
        [Header("Torch Prefab")]
        [Tooltip("Torch handle with EffectSocket + LightSocket inside")]
        [SerializeField] private GameObject torchHandlePrefab;
        
        [Tooltip("Use BraseroFlame particle system")]
        [SerializeField] private bool useBraseroFlame = true;

        // ─── Active Instances ──────────────────────────────────────────────────
        private readonly List<GameObject> _instances = new List<GameObject>();
        private readonly List<TorchController> _controllers = new List<TorchController>();

        // ─── Shared Materials ──────────────────────────────────────────────────
        private Material _sharedFlameMat;
        private Material _sharedHandleMat;
        private bool _materialsInitialized;

        // ───────────────────────────────────────────────────────────────────────
        //  GET TORCH INSTANCE AT POSITION
        // ───────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Create or reuse a torch instance at the specified position.
        /// </summary>
        public TorchController Get(Vector3 position, Quaternion rotation,
                                   Transform parent, Texture2D[] flameFrames,
                                   Material flameMat, Material handleMat)
        {
            InitializeMaterials(flameMat, handleMat, flameFrames);

            GameObject go;

            if (torchHandlePrefab != null)
            {
                // Instance from prefab
                go = Instantiate(torchHandlePrefab, parent);
            }
            else
            {
                // Create new torch
                go = BuildTorchObject();
                go.transform.SetParent(parent);
            }

            // Set position and rotation
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
                // Find components by searching ALL children recursively
                var effectSocket = go.GetComponentInChildren<ParticleSystem>();
                var light = go.GetComponentInChildren<Light>();

                Debug.Log($"[TorchPool] Torch at {position} - Particles: {effectSocket != null}, Light: {light != null}");

                if (light != null)
                {
                    // Initialize controller with just the light
                    ctrl.InitializeBrasero(light, null);
                    ctrl.TurnOn();
                    
                    // Enable particle system if it exists
                    if (effectSocket != null)
                    {
                        effectSocket.Play();
                        Debug.Log($"[TorchPool] ✅ Torch ON + Particles playing");
                    }
                    else
                    {
                        Debug.Log($"[TorchPool] ✅ Torch ON (no particles)");
                    }
                }
                else
                {
                    Debug.LogWarning($"[TorchPool] ❌ Light missing!");
                    SetupSpriteMode(go, flameFrames, ctrl);
                }
            }
            else
            {
                SetupSpriteMode(go, flameFrames, ctrl);
            }

            _instances.Add(go);
            _controllers.Add(ctrl);
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
        /// Destroys all torch instances.
        /// Call before each maze regeneration.
        /// </summary>
        public void ReleaseAll()
        {
            foreach (var go in _instances)
            {
                if (go != null)
                {
                    // Turn off light first
                    var ctrl = go.GetComponent<TorchController>();
                    ctrl?.TurnOff();
                    
                    // Destroy instance
                    if (Application.isPlaying)
                        Destroy(go);
                    else
                        DestroyImmediate(go);
                }
            }
            _instances.Clear();
            _controllers.Clear();
        }

        // ───────────────────────────────────────────────────────────────────────
        //  DESTROY ALL TORCHES (end of session)
        // ───────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Destroys all torches. Call at end of game session.
        /// </summary>
        public void DestroyAll()
        {
            ReleaseAll();
        }

        void OnDestroy() => DestroyAll();

        // ───────────────────────────────────────────────────────────────────────
        //  TORCH CONSTRUCTION (internal)
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
                flame.transform.localPosition = new Vector3(0f, 0.25f, 0.05f);
                flame.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                flame.transform.localScale = new Vector3(1f, 1f, 1f);
                var brasero = flame.AddComponent<BraseroFlame>();

                // ─── FALLBACK: Simple visible flame quad ───────────────────────
                var flameQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                flameQuad.name = "FlameVisual";
                flameQuad.transform.SetParent(torchGO.transform);
                flameQuad.transform.localPosition = new Vector3(0f, 0.35f, 0.15f);
                flameQuad.transform.localRotation = Quaternion.identity;
                flameQuad.transform.localScale = new Vector3(0.2f, 0.3f, 1f);
                var quadRenderer = flameQuad.GetComponent<MeshRenderer>();
                
                if (quadRenderer != null)
                {
                    var flameMat = new Material(Shader.Find("Unlit/Color"));
                    
                    if (flameMat == null || flameMat.shader == null)
                    {
                        flameMat = new Material(Shader.Find("Standard"));
                    }
                    if (flameMat != null && flameMat.shader != null)
                    {
                        flameMat.color = new Color(1f, 0.5f, 0.1f, 1f);
                        quadRenderer.material = flameMat;
                        quadRenderer.enabled = true;
                    }
                }
                Destroy(flameQuad.GetComponent<MeshCollider>());

                // ─── Light ─────────────────────────────────────────────────────
                var lightGO = new GameObject("FlameLight");
                lightGO.transform.SetParent(torchGO.transform);
                lightGO.transform.localPosition = new Vector3(0f, 0.35f, 0f);

                var pointLight = lightGO.AddComponent<Light>();
                pointLight.type = LightType.Point;
                pointLight.range = 15f;
                pointLight.intensity = 5f;
                pointLight.color = new Color(1f, 0.7f, 0.3f);
                pointLight.shadows = LightShadows.Soft;
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
                pointLight.shadows = LightShadows.Soft;
                pointLight.enabled = true;
                pointLight.bounceIntensity = 1.5f;

                // ─── Controller ────────────────────────────────────────────────
                torchGO.AddComponent<TorchController>();
            }

            return torchGO;
        }

        // ─── Stats (debug) ─────────────────────────────────────────────────────

        /// <summary>Number of currently active torch instances</summary>
        public int ActiveCount => _instances.Count;
    }
}
