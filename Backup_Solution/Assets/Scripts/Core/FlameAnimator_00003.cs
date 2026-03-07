
// FlameAnimator.cs
// Animation system for 8-bit flame effects
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Core system - works with BraseroFlame.cs
using UnityEngine;

namespace Code.Lavos.Core
{
    public class FlameAnimator : MonoBehaviour
    {
    [Header("Settings")]
    public float frameRate = 10f;
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private bool loop = true;
    [Header("Appearance")]
    [SerializeField] private float scaleX = 0.3f;
    [SerializeField] private float scaleY = 0.45f;
    [Header("Light Flicker")]
    [SerializeField] private float flickerIntensity = 0.4f;
    [SerializeField] private float baseIntensity = 1.5f;
    [SerializeField] private float lightRange = 6f;
    [SerializeField] private Color lightColor = new Color(1f, 0.6f, 0.2f);
    [SerializeField] private float _bobAmount = 0.03f;
    [SerializeField] private float _bobSpeed = 4f;
    [SerializeField] private bool _enableFlicker = true;

    public float bobAmount
    {
        get => _bobAmount;
        set => _bobAmount = value;
    }
    public float bobSpeed
    {
        get => _bobSpeed;
        set => _bobSpeed = value;
    }
    public bool enableFlicker
    {
        get => _enableFlicker;
        set => _enableFlicker = value;
    }

    private Texture2D[] _frames;
    private Renderer _renderer;
    private Light _light;
    private MaterialPropertyBlock _mpb;
    private Vector3 _originalPos;
    private float _randomOffset;
    private int _frameIndex;
    private float _timer;
    private bool _isPlaying;
    private bool _hasStarted;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _light = GetComponent<Light>();
        _mpb = new MaterialPropertyBlock();
        _originalPos = transform.localPosition;
        _randomOffset = Random.value * 100f;
        _hasStarted = true;

        if (scaleX > 0f && scaleY > 0f)
        {
            transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        if (_light != null)
        {
            _light.range = lightRange;
            _light.color = lightColor;
        }
    }

    public void SetFrames(Texture2D[] frames)
    {
        _frames = frames;
        if (playOnAwake && frames != null && frames.Length > 0 && _hasStarted)
            Play();
    }

    public void SetFrames(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0) return;
        _frames = new Texture2D[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null)
                _frames[i] = sprites[i].texture;
        }
        if (playOnAwake && _hasStarted)
            Play();
    }

    void OnEnable()
    {
        if (_hasStarted && playOnAwake && _frames != null && _frames.Length > 0)
            Play();
    }

    void OnDisable()
    {
        Stop();
    }

    public void Play()
    {
        if (_frames == null || _frames.Length == 0) return;
        _isPlaying = true;
        _frameIndex = 0;
        _timer = 0f;
        UpdateFrame();
    }

    public void Stop()
    {
        _isPlaying = false;
    }

    public void Reset()
    {
        _frameIndex = 0;
        _timer = 0f;
        UpdateFrame();
    }

    void Update()
    {
        if (!_isPlaying || _frames == null || _frames.Length == 0) return;

        if (frameRate > 0)
        {
            _timer += Time.deltaTime;
            float frameTime = 1f / frameRate;
            if (_timer >= frameTime)
            {
                _timer -= frameTime;
                _frameIndex++;
                if (_frameIndex >= _frames.Length)
                {
                    if (loop)
                        _frameIndex = 0;
                    else
                    {
                        _frameIndex = _frames.Length - 1;
                        Stop();
                        return;
                    }
                }
                UpdateFrame();
            }
        }

        float bob = Mathf.Sin(Time.time * _bobSpeed + _randomOffset) * _bobAmount;
        transform.localPosition = _originalPos + new Vector3(0f, bob, 0f);

        if (_enableFlicker && _light != null)
        {
            float flicker = Mathf.PerlinNoise(Time.time * 8f + _randomOffset, 0f);
            flicker = Mathf.Lerp(1f - flickerIntensity, 1f + flickerIntensity * 0.5f, flicker);
            _light.intensity = baseIntensity * flicker;
        }
    }

    private void UpdateFrame()
    {
        if (_renderer == null || _frames == null || _frameIndex < 0 || _frameIndex >= _frames.Length) return;
        _mpb.SetTexture("_MainTex", _frames[_frameIndex]);
        _renderer.SetPropertyBlock(_mpb);
    }
}
}
