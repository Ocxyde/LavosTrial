using UnityEngine;

namespace Code.Lavos
{
    public class TorchController : MonoBehaviour
    {
    [Header("Animation")]
    [SerializeField] private float frameRate = 8f;
    [SerializeField] private float bobAmplitude = 0.04f;
    [SerializeField] private float bobSpeed = 3f;
    [SerializeField] private bool enableFlameFlicker = false;

    [Header("Light")]
    [SerializeField] private float lightBaseIntensity = 1.8f;
    [SerializeField] private float lightFlickerAmount = 0.5f;
    [SerializeField] private float lightFlickerSpeed = 12f;
    [SerializeField] private float lightRange = 7f;
    [SerializeField] private Color lightColor = new Color(1f, 0.55f, 0.1f);

    private FlameAnimator _flameAnimator;
    private BraseroFlame _braseroFlame;
    private Light _light;
    private Transform _flameTransform;
    private float _flickerOffset;
    private Camera _mainCamera;
    private bool _isBraseroMode;

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
        _flickerOffset = Random.Range(0f, 100f);

        _flameAnimator = flameRenderer.gameObject.GetComponent<FlameAnimator>();
        if (_flameAnimator == null)
        {
            _flameAnimator = flameRenderer.gameObject.AddComponent<FlameAnimator>();
            if (_flameAnimator == null)
            {
                Debug.LogError("TorchController: Failed to add FlameAnimator component");
                return;
            }
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
            var validFrames = new System.Collections.Generic.List<Texture2D>();
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
            _light.intensity = lightBaseIntensity;
            _light.shadows = LightShadows.None;
        }
    }

    void Update()
    {
        if (_light == null) return;

        if (_isBraseroMode)
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
        else
        {
            // Original sprite-based flicker
            float flicker = Mathf.PerlinNoise(Time.time * lightFlickerSpeed + _flickerOffset, 0f);
            flicker += Mathf.PerlinNoise(Time.time * lightFlickerSpeed * 0.4f + _flickerOffset + 5f, 0f) * 0.5f;
            flicker /= 1.5f;

            _light.intensity = lightBaseIntensity + (flicker - 0.5f) * lightFlickerAmount;
            _light.intensity = Mathf.Max(0.3f, _light.intensity);

            BillboardFlame();
        }
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

    void OnDestroy()
    {
        if (_flameAnimator != null)
        {
            Destroy(_flameAnimator);
            _flameAnimator = null;
        }
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    //  BRASERO FLAME INITIALIZATION
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public void InitializeBrasero(Light pointLight, BraseroFlame braseroFlame)
    {
        _braseroFlame = braseroFlame;
        _light = pointLight;
        _isBraseroMode = true;
        _flickerOffset = Random.Range(0f, 100f);

        if (_light != null)
        {
            _light.color = lightColor;
            _light.range = lightRange;
            _light.intensity = lightBaseIntensity;
            _light.shadows = LightShadows.None;
        }
    }
}
}
