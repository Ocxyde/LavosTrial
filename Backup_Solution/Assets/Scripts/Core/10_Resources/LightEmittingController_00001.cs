// LightEmittingController.cs
// Generic controller for ALL light-emitting objects (lamps, candles, lanterns, braziers, etc.)
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-AND-OUT:
// - Unified ON/OFF state system for all light sources
// - Supports multiple light types: Candle, Lamp, Lantern, Brazier, Torch, Custom
// - Auto-registers with LightEngine when turned ON
// - Configurable light properties per type
//
// USAGE:
//   Add this component to any light-emitting object
//   Configure light type and properties in Inspector
//   Call TurnOn()/TurnOff() to control light emission

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Type of light-emitting object.
    /// Each type has default properties for quick setup.
    /// </summary>
    public enum LightEmittingType
    {
        Candle,     // Small, warm, gentle flicker
        Lamp,       // Medium, steady light
        Lantern,    // Portable, moderate flicker
        Brazier,    // Large, strong flicker
        Torch,      // Handheld, active flicker
        Chandelier, // Hanging, elegant
        Fireplace,  // Very large, dynamic
        Magic,      // Unusual colors, pulse effect
        Custom      // Fully configurable
    }

    /// <summary>
    /// LightEmittingController - Generic controller for all light-emitting objects.
    /// ON: Light active + emission via LightEngine
    /// OFF: Light disabled + no emission
    /// </summary>
    public class LightEmittingController : MonoBehaviour
    {
        #region Inspector Settings

        [Header("Light Type")]
        [Tooltip("Type of light source (affects default properties)")]
        [SerializeField] private LightEmittingType lightType = LightEmittingType.Lamp;

        [Tooltip("Custom light color (ignored if UseTypeDefaults is true)")]
        [SerializeField] private Color customLightColor = Color.white;

        [Tooltip("Custom light intensity (ignored if UseTypeDefaults is true)")]
        [SerializeField] private float customLightIntensity = 1f;

        [Tooltip("Custom light range (ignored if UseTypeDefaults is true)")]
        [SerializeField] private float customLightRange = 5f;

        [Tooltip("Use default properties for selected light type")]
        [SerializeField] private bool useTypeDefaults = true;

        [Header("State")]
        [Tooltip("Start in ON state")]
        [SerializeField] private bool startOn = true;

        [Tooltip("Light offset from object position")]
        [SerializeField] private Vector3 lightOffset = new Vector3(0f, 0.2f, 0f);

        [Header("Flicker")]
        [Tooltip("Enable light flicker effect")]
        [SerializeField] private bool enableFlicker = true;

        [Tooltip("Flicker speed")]
        [SerializeField] private float flickerSpeed = 2f;

        [Tooltip("Flicker amount (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float flickerAmount = 0.2f;

        [Header("Visual")]
        [Tooltip("Visual effect GameObject to enable/disable")]
        [SerializeField] private GameObject visualEffect;

        [Tooltip("Particle system for flame/smoke")]
        [SerializeField] private ParticleSystem particleSystem;

        [Header("Audio")]
        [Tooltip("Sound played when turning on")]
        [SerializeField] private AudioClip turnOnSound;

        [Tooltip("Sound played when turning off")]
        [SerializeField] private AudioClip turnOffSound;

        [Tooltip("Audio source (auto-created if null)")]
        [SerializeField] private AudioSource audioSource;

        #endregion

        #region Private Fields

        private Light _light;
        private bool _isOn;
        private string _lightId;
        private float _flickerOffset;
        private float _flickerTimer;
        private Color _baseColor;
        private float _baseIntensity;
        private float _baseRange;

        #endregion

        #region Public Properties

        public bool IsOn => _isOn;
        public string LightId => _lightId;
        public LightEmittingType LightType => lightType;
        public Light LightComponent => _light;

        #endregion

        #region Unity Lifecycle

        void Awake()
        {
            _flickerOffset = Random.Range(0f, 100f);
            _lightId = $"{lightType}_{gameObject.GetInstanceID()}";
            
            // Get or create Light component
            _light = GetComponent<Light>();
            if (_light == null)
            {
                _light = gameObject.AddComponent<Light>();
                _light.type = LightType.Point;
                _light.shadows = LightShadows.Soft;
            }

            // Get or create AudioSource
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 0.5f;
                }
            }

            // Apply type defaults or custom settings
            ApplyLightSettings();
        }

        void Start()
        {
            if (startOn)
            {
                TurnOn();
            }
            else
            {
                TurnOff();
            }
        }

        void Update()
        {
            if (!_isOn || !enableFlicker) return;

            UpdateFlicker();
        }

        void OnDestroy()
        {
            // Unregister from LightEngine
            LightEngine.Instance?.UnregisterLight(transform);
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Apply light settings based on type or custom values.
        /// </summary>
        public void ApplyLightSettings()
        {
            if (_light == null) return;

            if (useTypeDefaults)
            {
                ApplyTypeDefaults();
            }
            else
            {
                _baseColor = customLightColor;
                _baseIntensity = customLightIntensity;
                _baseRange = customLightRange;
            }

            _light.color = _baseColor;
            _light.range = _baseRange;
            
            if (_isOn)
            {
                _light.intensity = _baseIntensity;
                _light.enabled = true;
            }
        }

        /// <summary>
        /// Apply default settings for light type.
        /// </summary>
        private void ApplyTypeDefaults()
        {
            switch (lightType)
            {
                case LightEmittingType.Candle:
                    _baseColor = new Color(1f, 0.85f, 0.5f);
                    _baseIntensity = 0.8f;
                    _baseRange = 4f;
                    flickerSpeed = 3f;
                    flickerAmount = 0.25f;
                    lightOffset = new Vector3(0f, 0.15f, 0f);
                    break;

                case LightEmittingType.Lamp:
                    _baseColor = new Color(1f, 0.9f, 0.6f);
                    _baseIntensity = 1.2f;
                    _baseRange = 6f;
                    flickerSpeed = 2f;
                    flickerAmount = 0.15f;
                    lightOffset = new Vector3(0f, 0.3f, 0f);
                    break;

                case LightEmittingType.Lantern:
                    _baseColor = new Color(1f, 0.88f, 0.55f);
                    _baseIntensity = 1.5f;
                    _baseRange = 8f;
                    flickerSpeed = 2.5f;
                    flickerAmount = 0.2f;
                    lightOffset = new Vector3(0f, 0f, 0f);
                    break;

                case LightEmittingType.Brazier:
                    _baseColor = new Color(1f, 0.6f, 0.3f);
                    _baseIntensity = 2.5f;
                    _baseRange = 12f;
                    flickerSpeed = 1.5f;
                    flickerAmount = 0.3f;
                    lightOffset = new Vector3(0f, 0.5f, 0f);
                    break;

                case LightEmittingType.Torch:
                    _baseColor = new Color(1f, 0.55f, 0.2f);
                    _baseIntensity = 1.8f;
                    _baseRange = 7f;
                    flickerSpeed = 4f;
                    flickerAmount = 0.35f;
                    lightOffset = new Vector3(0f, 0.25f, 0f);
                    break;

                case LightEmittingType.Chandelier:
                    _baseColor = new Color(1f, 0.95f, 0.7f);
                    _baseIntensity = 2f;
                    _baseRange = 10f;
                    flickerSpeed = 1f;
                    flickerAmount = 0.1f;
                    lightOffset = new Vector3(0f, -0.5f, 0f);
                    break;

                case LightEmittingType.Fireplace:
                    _baseColor = new Color(1f, 0.5f, 0.25f);
                    _baseIntensity = 3f;
                    _baseRange = 15f;
                    flickerSpeed = 1.2f;
                    flickerAmount = 0.4f;
                    lightOffset = new Vector3(0f, 1f, 0f);
                    break;

                case LightEmittingType.Magic:
                    _baseColor = new Color(0.3f, 0.8f, 1f);
                    _baseIntensity = 2f;
                    _baseRange = 9f;
                    flickerSpeed = 5f;
                    flickerAmount = 0.5f;
                    lightOffset = new Vector3(0f, 0.2f, 0f);
                    break;

                case LightEmittingType.Custom:
                default:
                    _baseColor = customLightColor;
                    _baseIntensity = customLightIntensity;
                    _baseRange = customLightRange;
                    break;
            }
        }

        #endregion

        #region ON/OFF State Control

        /// <summary>
        /// Turn light ON - Activate emission.
        /// </summary>
        public void TurnOn()
        {
            if (_isOn) return;

            _isOn = true;

            // Enable visual effect
            if (visualEffect != null)
            {
                visualEffect.SetActive(true);
            }

            // Enable particle system
            if (particleSystem != null)
            {
                particleSystem.Play();
            }

            // Register with LightEngine for dynamic lighting
            LightEngine.Instance?.RegisterLight(
                transform,
                _lightId,
                intensity: _baseIntensity,
                range: _baseRange,
                color: _baseColor,
                offset: lightOffset
            );

            // Enable local light
            if (_light != null)
            {
                _light.enabled = true;
                _light.intensity = _baseIntensity;
            }

            // Play sound
            if (turnOnSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(turnOnSound);
            }

            Debug.Log($"[LightEmittingController] {_lightId} turned ON");
        }

        /// <summary>
        /// Turn light OFF - Disable emission.
        /// </summary>
        public void TurnOff()
        {
            if (!_isOn) return;

            _isOn = false;

            // Disable visual effect
            if (visualEffect != null)
            {
                visualEffect.SetActive(false);
            }

            // Stop particle system
            if (particleSystem != null)
            {
                particleSystem.Stop();
            }

            // Unregister from LightEngine
            LightEngine.Instance?.UnregisterLight(transform);

            // Disable local light
            if (_light != null)
            {
                _light.enabled = false;
                _light.intensity = 0f;
            }

            // Play sound
            if (turnOffSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(turnOffSound);
            }

            Debug.Log($"[LightEmittingController] {_lightId} turned OFF");
        }

        /// <summary>
        /// Toggle light ON/OFF.
        /// </summary>
        public void Toggle()
        {
            if (_isOn)
                TurnOff();
            else
                TurnOn();
        }

        /// <summary>
        /// Set light state directly.
        /// </summary>
        public void SetState(bool on)
        {
            if (on)
                TurnOn();
            else
                TurnOff();
        }

        #endregion

        #region Flicker Effect

        private void UpdateFlicker()
        {
            if (_light == null || flickerSpeed <= 0f || flickerAmount <= 0f) return;

            _flickerTimer += Time.deltaTime;
            
            // Perlin noise based flicker
            float flicker = Mathf.PerlinNoise(
                _flickerTimer * flickerSpeed + _flickerOffset,
                _flickerOffset
            );

            // Map to intensity range
            float intensityMult = 1f - (flicker * flickerAmount);
            _light.intensity = _baseIntensity * intensityMult;

            // Magic type: also pulse color
            if (lightType == LightEmittingType.Magic)
            {
                float hueShift = Mathf.Sin(_flickerTimer * 2f) * 0.1f;
                _light.color = ShiftHue(_baseColor, hueShift);
            }
        }

        private Color ShiftHue(Color color, float shift)
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            h = (h + shift + 1f) % 1f;
            return Color.HSVToRGB(h, s, v);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Set light intensity.
        /// </summary>
        public void SetIntensity(float intensity)
        {
            _baseIntensity = intensity;
            if (_isOn && _light != null)
            {
                _light.intensity = intensity;
            }
        }

        /// <summary>
        /// Set light range.
        /// </summary>
        public void SetRange(float range)
        {
            _baseRange = range;
            if (_light != null)
            {
                _light.range = range;
            }
        }

        /// <summary>
        /// Set light color.
        /// </summary>
        public void SetColor(Color color)
        {
            _baseColor = color;
            if (_light != null)
            {
                _light.color = color;
            }
        }

        /// <summary>
        /// Set flicker enabled.
        /// </summary>
        public void SetFlickerEnabled(bool enabled)
        {
            enableFlicker = enabled;
        }

        /// <summary>
        /// Set visual effect active.
        /// </summary>
        public void SetVisualEffectActive(GameObject effect)
        {
            visualEffect = effect;
        }

        /// <summary>
        /// Set particle system.
        /// </summary>
        public void SetParticleSystem(ParticleSystem ps)
        {
            particleSystem = ps;
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        void OnValidate()
        {
            if (useTypeDefaults && Application.isPlaying)
            {
                ApplyTypeDefaults();
            }
        }
#endif

        #endregion
    }
}
