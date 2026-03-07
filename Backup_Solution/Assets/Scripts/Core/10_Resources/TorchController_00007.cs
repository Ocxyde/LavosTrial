// TorchController.cs
// Controller for torch objects with ON/OFF states
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-AND-OUT:
// - Torches have 2 states: ON (flame + light) / OFF (no flame, no light)
// - Flame handles light emission via LightEngine
// - Auto-registers with LightEngine when turned ON

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// TorchController - Manages torch ON/OFF states.
    /// ON: Flame active + Light emission via LightEngine
    /// OFF: No flame + No light
    /// </summary>
    public class TorchController : MonoBehaviour
    {
        #region Inspector Settings

        [Header("Animation")]
        [SerializeField] private float frameRate = 8f;
        [SerializeField] private float bobAmplitude = 0.04f;
        [SerializeField] private float bobSpeed = 3f;
        [SerializeField] private bool enableFlameFlicker = true;

        [Header("Light")]
        [SerializeField] private float lightBaseIntensity = 1.8f;
        [SerializeField] private float lightFlickerAmount = 0.5f;
        [SerializeField] private float lightFlickerSpeed = 12f;
        [SerializeField] private float lightRange = 7f;
        [SerializeField] private Color lightColor = new Color(1f, 0.55f, 0.1f);

        [Header("State")]
        [Tooltip("Start torch in ON state")]
        [SerializeField] private bool startOn = true;

        #endregion

        #region Private Fields

        private FlameAnimator _flameAnimator;
        private BraseroFlame _braseroFlame;
        private Light _light;
        private Transform _flameTransform;
        private float _flickerOffset;
        private Camera _mainCamera;
        private bool _isBraseroMode;
        private bool _isOn;
        private string _lightId;

        #endregion

        #region Public Properties

        public bool IsOn => _isOn;
        public string LightId => _lightId;

        #endregion

        #region Unity Lifecycle

        void Awake()
        {
            _flickerOffset = Random.Range(0f, 100f);
            _lightId = $"Torch_{gameObject.GetInstanceID()}";
        }

        void Start()
        {
            // TurnOn() is called by TorchPool.Get() after InitializeBrasero()
        }

        void Update()
        {
            if (!_isOn || _light == null) return;

            if (_isBraseroMode)
            {
                UpdateBraseroMode();
            }
            else
            {
                UpdateSpriteMode();
            }
        }

        void OnDestroy()
        {
            if (_flameAnimator != null)
            {
                Destroy(_flameAnimator);
                _flameAnimator = null;
            }

            // Unregister from LightEngine
            LightEngine.Instance?.UnregisterLight(transform);
        }

        #endregion

        #region Initialization

        public void Initialize(Texture2D[] flameFrames, Renderer flameRenderer, Light pointLight)
        {
            if (flameRenderer == null)
            {
                Debug.LogWarning("TorchController: flameRenderer is null");
                return;
            }

            _mainCamera = Camera.main;
            _light = pointLight;
            _flameTransform = flameRenderer.transform;

            _flameAnimator = flameRenderer.gameObject.GetComponent<FlameAnimator>();
            if (_flameAnimator == null)
            {
                _flameAnimator = flameRenderer.gameObject.AddComponent<FlameAnimator>();
            }

            float validFrameRate = Mathf.Max(1f, frameRate);
            float validBobAmount = Mathf.Max(0f, bobAmplitude);
            float validBobSpeed = Mathf.Max(0f, bobSpeed);

            _flameAnimator.frameRate = validFrameRate;
            _flameAnimator.bobAmount = validBobAmount;
            _flameAnimator.bobSpeed = validBobSpeed;
            _flameAnimator.enableFlicker = enableFlameFlicker;

            if (flameFrames != null && flameFrames.Length > 0)
            {
                var validFrames = new List<Texture2D>();
                for (int i = 0; i < flameFrames.Length; i++)
                {
                    if (flameFrames[i] != null)
                        validFrames.Add(flameFrames[i]);
                }
                if (validFrames.Count > 0)
                    _flameAnimator.SetFrames(validFrames.ToArray());
            }

            if (_light != null)
            {
                _light.color = lightColor;
                _light.range = lightRange;
                _light.intensity = 0f; // Start off, will be set by TurnOn
                _light.shadows = LightShadows.Soft;
                _light.enabled = false;
            }

            _isBraseroMode = false;
        }

        public void InitializeBrasero(Light pointLight, BraseroFlame braseroFlame)
        {
            _braseroFlame = braseroFlame;
            _light = pointLight;
            _isBraseroMode = true;
            _flameTransform = null;

            if (_light != null)
            {
                _light.color = lightColor;
                _light.range = lightRange;
                _light.intensity = 0f; // Start off
                _light.shadows = LightShadows.Soft;
                _light.enabled = false;
            }
        }

        #endregion

        #region ON/OFF State Control

        /// <summary>
        /// Turn torch ON - Activate flame and register with LightEngine.
        /// </summary>
        public void TurnOn()
        {
            if (_isOn) return;

            _isOn = true;

            // Enable flame visual
            if (_flameAnimator != null)
            {
                _flameAnimator.enabled = true;
            }

            if (_braseroFlame != null)
            {
                _braseroFlame.gameObject.SetActive(true);
            }

            // Register with LightEngine (PLUG-IN-OUT SYSTEM)
            var lightEngine = LightEngine.Instance;
            if (lightEngine != null)
            {
                lightEngine.RegisterLight(
                    transform,
                    _lightId,
                    intensity: lightBaseIntensity * 3f,
                    range: lightRange * 1.5f,
                    color: lightColor,
                    offset: new Vector3(0f, 0.1f, 0f)
                );
            }

            // Enable local light as backup
            if (_light != null)
            {
                _light.enabled = true;
                _light.intensity = lightBaseIntensity * 2f;
                _light.range = lightRange * 1.5f;
                Debug.Log($"[TorchController] {_lightId} light ENABLED - intensity={_light.intensity}, range={_light.range}");
            }
            else
            {
                Debug.LogWarning($"[TorchController] {_lightId} has NO Light component!");
            }
        }

        /// <summary>
        /// Turn torch OFF - Disable flame and light emission.
        /// </summary>
        public void TurnOff()
        {
            if (!_isOn) return;

            _isOn = false;

            // Disable flame visual
            if (_flameAnimator != null)
            {
                _flameAnimator.enabled = false;
            }

            if (_braseroFlame != null)
            {
                _braseroFlame.gameObject.SetActive(false);
            }

            // Unregister from LightEngine (removes dynamic light)
            LightEngine.Instance?.UnregisterLight(transform);

            // Disable local light
            if (_light != null)
            {
                _light.enabled = false;
                _light.intensity = 0f;
            }

            Debug.Log($"[TorchController] {_lightId} turned OFF");
        }

        /// <summary>
        /// Toggle torch ON/OFF state.
        /// </summary>
        public void Toggle()
        {
            if (_isOn)
                TurnOff();
            else
                TurnOn();
        }

        /// <summary>
        /// Set torch state directly.
        /// </summary>
        public void SetState(bool on)
        {
            if (on)
                TurnOn();
            else
                TurnOff();
        }

        #endregion

        #region Update Methods

        private void UpdateBraseroMode()
        {
            // Brasero mode: smoother, more organic flicker
            float flicker = Mathf.PerlinNoise(Time.time * lightFlickerSpeed * 0.5f + _flickerOffset, 0f);
            flicker += Mathf.PerlinNoise(Time.time * lightFlickerSpeed * 0.3f + _flickerOffset + 5f, 0f) * 0.5f;
            flicker = flicker / 1.5f;

            float intensity = lightBaseIntensity + (flicker - 0.5f) * lightFlickerAmount * 1.5f;
            _light.intensity = Mathf.Max(0.5f, intensity);

            // Sync particle intensity with light
            if (_braseroFlame != null)
            {
                _braseroFlame.SetIntensity(flicker);
            }
        }

        private void UpdateSpriteMode()
        {
            // Original sprite-based flicker
            float flicker = Mathf.PerlinNoise(Time.time * lightFlickerSpeed + _flickerOffset, 0f);
            flicker += Mathf.PerlinNoise(Time.time * lightFlickerSpeed * 0.4f + _flickerOffset + 5f, 0f) * 0.5f;
            flicker /= 1.5f;

            _light.intensity = lightBaseIntensity + (flicker - 0.5f) * lightFlickerAmount;
            _light.intensity = Mathf.Max(0.3f, _light.intensity);

            BillboardFlame();
        }

        private void BillboardFlame()
        {
            if (_flameTransform == null || _light == null) return;

            if (_mainCamera == null || !_mainCamera.isActiveAndEnabled)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null) return;
            }

            Vector3 dir = _mainCamera.transform.position - _flameTransform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) return;

            _flameTransform.rotation = Quaternion.LookRotation(-dir.normalized, Vector3.up);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Set light intensity multiplier.
        /// </summary>
        public void SetLightIntensity(float intensity)
        {
            lightBaseIntensity = intensity;
            if (_isOn && _light != null)
            {
                _light.intensity = intensity;
            }
        }

        /// <summary>
        /// Set light range.
        /// </summary>
        public void SetLightRange(float range)
        {
            lightRange = range;
            if (_light != null)
            {
                _light.range = range;
            }
        }

        /// <summary>
        /// Set light color.
        /// </summary>
        public void SetLightColor(Color color)
        {
            lightColor = color;
            if (_light != null)
            {
                _light.color = color;
            }
        }

        #endregion
    }
}
