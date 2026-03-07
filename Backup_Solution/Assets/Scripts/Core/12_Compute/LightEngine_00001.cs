// LightEngine.cs
// Central lighting engine for ALL light emission, fog of war, and lightning effects
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-AND-OUT ARCHITECTURE:
// - Single point for ALL lighting in the game
// - Dynamic point lights per torch/object
// - Fog of war / darkness system
// - Lightning / flicker exposure effects
// - Performance optimized with light pooling
// - Emission control for all light sources
//
// USAGE:
//   1. Add LightEngine component to scene
//   2. Light sources auto-register when spawned
//   3. Engine manages all light emission, fog, effects
//
// Location: Assets/Scripts/Core/12_Compute/

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// LightEngine - Central lighting management for ALL light sources.
    /// Handles dynamic lights, fog of war, emission, and atmospheric effects.
    /// </summary>
    public class LightEngine : MonoBehaviour
    {
        #region Singleton

        private static LightEngine _instance;
        public static LightEngine Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<LightEngine>();
                    if (_instance == null)
                    {
                        var go = new GameObject("LightEngine");
                        _instance = go.AddComponent<LightEngine>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Inspector Settings

        [Header("Dynamic Lights")]
        [Tooltip("Enable dynamic lights on all light sources")]
        [SerializeField] private bool enableDynamicLights = true;

        [Tooltip("Default light range")]
        [SerializeField] private float defaultLightRange = 8f;

        [Tooltip("Default light intensity")]
        [SerializeField] private float defaultLightIntensity = 1.2f;

        [Tooltip("Default light color (warm torch glow)")]
        [SerializeField] private Color defaultLightColor = new Color(1f, 0.9f, 0.7f, 1f);

        [Tooltip("Light flicker speed (0 = no flicker)")]
        [SerializeField] private float flickerSpeed = 2f;

        [Tooltip("Flicker intensity (0-1)")]
        [SerializeField] private float flickerAmount = 0.15f;

        [Header("Fog of War / Darkness")]
        [Tooltip("Enable fog of war (darkness in unlit areas)")]
        [SerializeField] private bool enableFogOfWar = true;

        [Tooltip("Base darkness level (0 = bright, 1 = pitch black)")]
        [Range(0f, 1f)]
        [SerializeField] private float baseDarkness = 0.3f;

        [Tooltip("Darkness falloff distance")]
        [SerializeField] private float darknessFalloff = 15f;

        [Tooltip("Darkness smooth transition speed")]
        [SerializeField] private float darknessTransitionSpeed = 0.5f;

        [Header("Lightning / Exposure")]
        [Tooltip("Enable lightning flash effects")]
        [SerializeField] private bool enableLightning = false;

        [Tooltip("Minimum time between lightning flashes")]
        [SerializeField] private float minLightningInterval = 30f;

        [Tooltip("Maximum time between lightning flashes")]
        [SerializeField] private float maxLightningInterval = 120f;

        [Tooltip("Lightning flash intensity multiplier")]
        [SerializeField] private float lightningIntensityMult = 3f;

        [Tooltip("Lightning flash duration")]
        [SerializeField] private float lightningDuration = 0.3f;

        [Header("Emission Control")]
        [Tooltip("Global emission multiplier for all lights")]
        [Range(0f, 2f)]
        [SerializeField] private float globalEmissionMultiplier = 1f;

        [Tooltip("Enable emission flicker")]
        [SerializeField] private bool enableEmissionFlicker = true;

        [Tooltip("Emission flicker speed")]
        [SerializeField] private float emissionFlickerSpeed = 1.5f;

        [Tooltip("Emission flicker amount (0-1)")]
        [SerializeField] private float emissionFlickerAmount = 0.2f;

        [Header("Performance")]
        [Tooltip("Max active dynamic lights")]
        [SerializeField] private int maxDynamicLights = 32;

        [Tooltip("Use baked lighting instead of dynamic")]
        [SerializeField] private bool useBakedLighting = false;

        [Tooltip("Light update interval (0 = every frame)")]
        [SerializeField] private float lightUpdateInterval = 0f;

        [Header("Ambient Light")]
        [Tooltip("Enable ambient light control")]
        [SerializeField] private bool enableAmbientControl = true;

        [Tooltip("Ambient light color")]
        [SerializeField] private Color ambientLightColor = new Color(0.3f, 0.25f, 0.2f, 1f);

        [Tooltip("Ambient intensity follows darkness")]
        [SerializeField] private bool ambientFollowsDarkness = true;

        [Header("Debug")]
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private bool verboseLogging = false;

        #endregion

        #region Light Pool

        private struct LightData
        {
            public Light light;
            public Transform parent;
            public Vector3 offset;
            public float baseIntensity;
            public float baseRange;
            public Color baseColor;
            public float flickerOffset;
            public bool isActive;
            public string sourceId;
        }

        private List<LightData> _lightPool = new List<LightData>();
        private int _activeLightCount = 0;
        private Transform _lightRoot;

        #endregion

        #region Lightning State

        private bool _isLightningActive = false;
        private float _lightningTimer = 0f;
        private float _nextLightningTime = 0f;
        private float _lightningEndTime = 0f;
        private float _currentLightningIntensity = 1f;

        #endregion

        #region Fog of War

        private float _currentDarkness = 0f;
        private float _targetDarkness = 0.3f;
        private float _ambientBaseIntensity = 0.6f;

        #endregion

        #region Emission

        private float _currentEmissionMultiplier = 1f;
        private float _emissionFlickerPhase = 0f;

        #endregion

        #region Unity Lifecycle

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                if (verboseLogging)
                    Debug.Log("[LightEngine] Instance already exists, destroying duplicate");
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        void Start()
        {
            InitializeLightPool();
            ScheduleNextLightning();
            
            // Store base ambient intensity
            _ambientBaseIntensity = RenderSettings.ambientIntensity;
            
            Log($"[LightEngine] Initialized - Max Lights: {maxDynamicLights}, Fog: {enableFogOfWar}, Lightning: {enableLightning}");
        }

        void Update()
        {
            float deltaTime = Time.deltaTime;

            if (enableDynamicLights && !useBakedLighting)
            {
                UpdateFlicker(deltaTime);
                UpdateEmission(deltaTime);
            }

            if (enableFogOfWar)
            {
                UpdateFogOfWar(deltaTime);
            }

            if (enableLightning)
            {
                UpdateLightning(deltaTime);
            }
        }

        void OnDestroy()
        {
            CleanupLights();
            if (_instance == this)
                _instance = null;
        }

        #endregion

        #region Light Pool Management

        private void InitializeLightPool()
        {
            _lightPool.Clear();
            _activeLightCount = 0;

            // Create parent for all lights
            _lightRoot = new GameObject("LightEngine_Lights");
            _lightRoot.transform.SetParent(transform);
            _lightRoot.transform.localPosition = Vector3.zero;

            for (int i = 0; i < maxDynamicLights; i++)
            {
                var lightGO = new GameObject($"Light_{i}");
                lightGO.transform.SetParent(_lightRoot.transform);
                lightGO.transform.localPosition = Vector3.zero;

                var light = lightGO.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = defaultLightColor;
                light.range = defaultLightRange;
                light.intensity = 0f; // Start disabled
                light.shadows = LightShadows.Soft;
                light.enabled = false;
                light.bounceIntensity = 1f;
                light.bakingMask = -1;

                _lightPool.Add(new LightData
                {
                    light = light,
                    parent = lightGO.transform,
                    baseIntensity = defaultLightIntensity,
                    baseRange = defaultLightRange,
                    baseColor = defaultLightColor,
                    flickerOffset = Random.Range(0f, 100f),
                    isActive = false,
                    sourceId = "",
                    offset = Vector3.zero
                });
            }

            Log($"[LightEngine] Light pool initialized with {maxDynamicLights} lights");
        }

        /// <summary>
        /// Register a light source and assign a dynamic light.
        /// Call this when a light source is spawned.
        /// </summary>
        public void RegisterLight(Transform sourceTransform, string sourceId = "", 
                                   float? intensity = null, float? range = null, 
                                   Color? color = null, Vector3? offset = null)
        {
            if (!enableDynamicLights || useBakedLighting)
                return;

            var lightData = GetAvailableLight();
            if (lightData.light == null)
            {
                Log($"[LightEngine] No available lights (max: {maxDynamicLights})");
                return;
            }

            // Configure light
            lightData.light.color = color ?? lightData.baseColor;
            lightData.light.range = range ?? lightData.baseRange;
            lightData.light.intensity = (intensity ?? lightData.baseIntensity) * globalEmissionMultiplier;

            // Position light
            Vector3 lightOffset = offset ?? Vector3.zero;
            lightData.light.transform.SetParent(sourceTransform);
            lightData.light.transform.localPosition = lightOffset;
            lightData.light.transform.localRotation = Quaternion.identity;

            // Enable light
            lightData.light.enabled = true;

            // Store data
            lightData.isActive = true;
            lightData.sourceId = string.IsNullOrEmpty(sourceId) ? $"Light_{_activeLightCount}" : sourceId;
            lightData.offset = lightOffset;

            _activeLightCount++;

            Log($"[LightEngine] Light registered: {lightData.sourceId} at {sourceTransform.position}");
        }

        /// <summary>
        /// Unregister a light source and free its light.
        /// Call this when a light source is despawned.
        /// </summary>
        public void UnregisterLight(Transform sourceTransform)
        {
            for (int i = 0; i < _lightPool.Count; i++)
            {
                if (_lightPool[i].isActive && _lightPool[i].light.transform.parent == sourceTransform)
                {
                    var lightData = _lightPool[i];
                    lightData.light.enabled = false;
                    lightData.light.intensity = 0f;
                    lightData.isActive = false;
                    _activeLightCount--;

                    // Return light to pool parent
                    lightData.light.transform.SetParent(_lightRoot);
                    lightData.light.transform.localPosition = Vector3.zero;

                    _lightPool[i] = lightData;
                    Log($"[LightEngine] Light unregistered: {lightData.sourceId}");
                    return;
                }
            }
        }

        /// <summary>
        /// Unregister a light source by ID.
        /// </summary>
        public void UnregisterLightById(string sourceId)
        {
            for (int i = 0; i < _lightPool.Count; i++)
            {
                if (_lightPool[i].isActive && _lightPool[i].sourceId == sourceId)
                {
                    var lightData = _lightPool[i];
                    lightData.light.enabled = false;
                    lightData.light.intensity = 0f;
                    lightData.isActive = false;
                    _activeLightCount--;

                    lightData.light.transform.SetParent(_lightRoot);
                    lightData.light.transform.localPosition = Vector3.zero;

                    _lightPool[i] = lightData;
                    Log($"[LightEngine] Light unregistered by ID: {sourceId}");
                    return;
                }
            }
        }

        private LightData GetAvailableLight()
        {
            for (int i = 0; i < _lightPool.Count; i++)
            {
                if (!_lightPool[i].isActive)
                {
                    var lightData = _lightPool[i];
                    lightData.isActive = true;
                    _activeLightCount++;
                    _lightPool[i] = lightData;
                    return lightData;
                }
            }

            return new LightData(); // Return empty if none available
        }

        private void CleanupLights()
        {
            foreach (var lightData in _lightPool)
            {
                if (lightData.light != null)
                {
                    Destroy(lightData.light.gameObject);
                }
            }
            _lightPool.Clear();
            _activeLightCount = 0;

            if (_lightRoot != null)
                Destroy(_lightRoot.gameObject);
        }

        #endregion

        #region Flicker Effect

        private void UpdateFlicker(float deltaTime)
        {
            if (flickerSpeed <= 0f || flickerAmount <= 0f)
                return;

            float time = Time.time;

            foreach (var lightData in _lightPool)
            {
                if (!lightData.isActive || lightData.light == null)
                    continue;

                // Perlin noise based flicker
                float flicker = Mathf.PerlinNoise(
                    time * flickerSpeed + lightData.flickerOffset,
                    lightData.flickerOffset
                );

                // Map to intensity range
                float intensityMult = 1f - (flicker * flickerAmount);
                
                // Apply global emission and darkness
                float finalIntensity = lightData.baseIntensity * intensityMult * globalEmissionMultiplier;
                
                if (enableFogOfWar)
                {
                    finalIntensity *= (1f - _currentDarkness);
                }

                // Apply lightning boost
                if (_isLightningActive)
                {
                    finalIntensity *= _currentLightningIntensity;
                }

                lightData.light.intensity = finalIntensity;
            }
        }

        #endregion

        #region Emission Control

        private void UpdateEmission(float deltaTime)
        {
            if (!enableEmissionFlicker || emissionFlickerSpeed <= 0f)
            {
                _currentEmissionMultiplier = globalEmissionMultiplier;
                return;
            }

            _emissionFlickerPhase += deltaTime * emissionFlickerSpeed;
            
            // Smooth sine wave flicker
            float flicker = (Mathf.Sin(_emissionFlickerPhase) + 1f) * 0.5f; // 0-1 range
            _currentEmissionMultiplier = globalEmissionMultiplier * (1f - emissionFlickerAmount * flicker);

            // Apply to all lights
            foreach (var lightData in _lightPool)
            {
                if (!lightData.isActive || lightData.light == null)
                    continue;

                // Only update if not already handled by flicker
                if (flickerSpeed <= 0f)
                {
                    float intensity = lightData.baseIntensity * _currentEmissionMultiplier;
                    if (enableFogOfWar)
                    {
                        intensity *= (1f - _currentDarkness);
                    }
                    lightData.light.intensity = intensity;
                }
            }
        }

        /// <summary>
        /// Set global emission multiplier for all lights.
        /// </summary>
        public void SetGlobalEmission(float multiplier)
        {
            globalEmissionMultiplier = Mathf.Max(0f, multiplier);
        }

        /// <summary>
        /// Pulse emission (for special effects).
        /// </summary>
        public void PulseEmission(float intensity, float duration)
        {
            CancelInvoke(nameof(ResetEmission));
            globalEmissionMultiplier = intensity;
            Invoke(nameof(ResetEmission), duration);
        }

        private void ResetEmission()
        {
            globalEmissionMultiplier = 1f;
        }

        #endregion

        #region Fog of War / Darkness

        private void UpdateFogOfWar(float deltaTime)
        {
            // Smooth darkness transition
            _currentDarkness = Mathf.Lerp(_currentDarkness, _targetDarkness, deltaTime * darknessTransitionSpeed);

            // Update ambient light
            if (enableAmbientControl && ambientFollowsDarkness)
            {
                float ambientMult = 1f - (_currentDarkness * 0.8f);
                RenderSettings.ambientIntensity = _ambientBaseIntensity * ambientMult;
                RenderSettings.ambientLight = ambientLightColor * ambientMult;
            }

            // Update all light intensities based on darkness
            if (!enableDynamicLights || useBakedLighting)
                return;

            float darknessMult = 1f - _currentDarkness;

            foreach (var lightData in _lightPool)
            {
                if (!lightData.isActive || lightData.light == null)
                    continue;

                // Skip if flicker is handling it
                if (flickerSpeed > 0f)
                    continue;

                float targetIntensity = lightData.baseIntensity * darknessMult * globalEmissionMultiplier;

                if (_isLightningActive)
                {
                    targetIntensity *= _currentLightningIntensity;
                }

                lightData.light.intensity = targetIntensity;
            }
        }

        /// <summary>
        /// Set the base darkness level (0 = bright, 1 = pitch black).
        /// </summary>
        public void SetDarkness(float darkness)
        {
            _targetDarkness = Mathf.Clamp01(darkness);
        }

        /// <summary>
        /// Get current darkness level.
        /// </summary>
        public float GetCurrentDarkness() => _currentDarkness;

        /// <summary>
        /// Set ambient light color.
        /// </summary>
        public void SetAmbientColor(Color color)
        {
            ambientLightColor = color;
        }

        /// <summary>
        /// Set ambient base intensity.
        /// </summary>
        public void SetAmbientIntensity(float intensity)
        {
            _ambientBaseIntensity = intensity;
        }

        #endregion

        #region Lightning / Exposure

        private void UpdateLightning(float deltaTime)
        {
            _lightningTimer += deltaTime;

            // Check if it's time for lightning
            if (_lightningTimer >= _nextLightningTime && !_isLightningActive)
            {
                TriggerLightning();
            }

            // End lightning flash
            if (_isLightningActive && _lightningTimer >= _lightningEndTime)
            {
                _isLightningActive = false;
                _currentLightningIntensity = 1f;
                ScheduleNextLightning();
            }
        }

        private void TriggerLightning()
        {
            _isLightningActive = true;
            _lightningEndTime = _lightningTimer + lightningDuration;
            _currentLightningIntensity = lightningIntensityMult;

            Log($"[LightEngine] ⚡ LIGHTNING FLASH!");

            // Flash all lights instantly
            foreach (var lightData in _lightPool)
            {
                if (lightData.isActive && lightData.light != null)
                {
                    lightData.light.intensity = lightData.baseIntensity * lightningIntensityMult * globalEmissionMultiplier;
                }
            }

            // Flash ambient light
            if (enableAmbientControl)
            {
                FlashAmbientLight();
            }
        }

        private void FlashAmbientLight()
        {
            RenderSettings.ambientIntensity = _ambientBaseIntensity * 2f;
            RenderSettings.ambientLight = Color.white;

            CancelInvoke(nameof(RestoreAmbientLight));
            Invoke(nameof(RestoreAmbientLight), lightningDuration);
        }

        private void RestoreAmbientLight()
        {
            RenderSettings.ambientIntensity = _ambientBaseIntensity * (1f - _currentDarkness * 0.8f);
            RenderSettings.ambientLight = ambientLightColor;
        }

        private void ScheduleNextLightning()
        {
            _lightningTimer = 0f;
            _nextLightningTime = Random.Range(minLightningInterval, maxLightningInterval);
        }

        /// <summary>
        /// Manually trigger a lightning flash.
        /// </summary>
        [ContextMenu("Trigger Lightning")]
        public void TriggerManualLightning()
        {
            TriggerLightning();
            ScheduleNextLightning();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Enable or disable dynamic lights.
        /// </summary>
        public void SetDynamicLightsEnabled(bool enabled)
        {
            enableDynamicLights = enabled;

            foreach (var lightData in _lightPool)
            {
                if (lightData.light != null)
                {
                    lightData.light.enabled = enabled && lightData.isActive;
                }
            }
        }

        /// <summary>
        /// Set light color for a specific light source.
        /// </summary>
        public void SetLightColor(string sourceId, Color color)
        {
            foreach (var lightData in _lightPool)
            {
                if (lightData.isActive && lightData.sourceId == sourceId && lightData.light != null)
                {
                    lightData.light.color = color;
                    lightData.baseColor = color;
                    return;
                }
            }
        }

        /// <summary>
        /// Set light range for a specific light source.
        /// </summary>
        public void SetLightRange(string sourceId, float range)
        {
            foreach (var lightData in _lightPool)
            {
                if (lightData.isActive && lightData.sourceId == sourceId && lightData.light != null)
                {
                    lightData.light.range = range;
                    lightData.baseRange = range;
                    return;
                }
            }
        }

        /// <summary>
        /// Set light intensity for a specific light source.
        /// </summary>
        public void SetLightIntensity(string sourceId, float intensity)
        {
            foreach (var lightData in _lightPool)
            {
                if (lightData.isActive && lightData.sourceId == sourceId && lightData.light != null)
                {
                    lightData.baseIntensity = intensity;
                    lightData.light.intensity = intensity * globalEmissionMultiplier;
                    return;
                }
            }
        }

        /// <summary>
        /// Get count of active lights.
        /// </summary>
        public int GetActiveLightCount() => _activeLightCount;

        /// <summary>
        /// Get total lights in pool.
        /// </summary>
        public int GetTotalLightPoolSize() => maxDynamicLights;

        /// <summary>
        /// Get available light count.
        /// </summary>
        public int GetAvailableLightCount() => maxDynamicLights - _activeLightCount;

        /// <summary>
        /// Check if a light source is registered.
        /// </summary>
        public bool IsLightRegistered(string sourceId)
        {
            foreach (var lightData in _lightPool)
            {
                if (lightData.isActive && lightData.sourceId == sourceId)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Unregister all lights.
        /// </summary>
        public void UnregisterAllLights()
        {
            foreach (var lightData in _lightPool)
            {
                if (lightData.isActive && lightData.light != null)
                {
                    lightData.light.enabled = false;
                    lightData.light.intensity = 0f;
                    lightData.isActive = false;
                    lightData.light.transform.SetParent(_lightRoot);
                    lightData.light.transform.localPosition = Vector3.zero;
                }
            }
            _activeLightCount = 0;
        }

        #endregion

        #region Debug

        void OnDrawGizmosSelected()
        {
            if (!showGizmos || Application.isPlaying == false) return;

            // Draw light ranges
            foreach (var lightData in _lightPool)
            {
                if (lightData.isActive && lightData.light != null && lightData.light.enabled)
                {
                    Gizmos.color = new Color(1f, 0.9f, 0.5f, 0.2f);
                    Gizmos.DrawWireSphere(lightData.light.transform.position, lightData.baseRange);
                }
            }

            // Draw darkness zone
            if (enableFogOfWar)
            {
                Gizmos.color = new Color(0f, 0f, 0f, _currentDarkness * 0.3f);
                Gizmos.DrawCube(transform.position, new Vector3(darknessFalloff * 2, 10f, darknessFalloff * 2));
            }
        }

        private void Log(string message)
        {
            if (verboseLogging)
            {
                Debug.Log(message);
            }
        }

        #endregion

        #region Properties

        public bool EnableDynamicLights => enableDynamicLights;
        public bool EnableFogOfWar => enableFogOfWar;
        public bool EnableLightning => enableLightning;
        public float CurrentDarkness => _currentDarkness;
        public bool IsLightningActive => _isLightningActive;
        public int ActiveLightCount => _activeLightCount;
        public float CurrentEmission => _currentEmissionMultiplier;
        public float GlobalEmissionMultiplier => globalEmissionMultiplier;

        #endregion
    }
}
