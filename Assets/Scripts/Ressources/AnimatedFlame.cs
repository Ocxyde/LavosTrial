// Copyright (C) 2026 Ocxyde
//
// This file is part of PeuImporte.
//
// PeuImporte is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PeuImporte is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PeuImporte.  If not, see <https://www.gnu.org/licenses/>.
// AnimatedFlame.cs
// Animated 8-bit flame sprite
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Ressources system - flame animation

using UnityEngine;

namespace Code.Lavos.Core
{
    public class AnimatedFlame : MonoBehaviour
    {
    [Header("Animation")]
    [SerializeField] private float frameRate = 8f;
    [SerializeField] private bool autoStart = true;

    [Header("Visual")]
    [SerializeField] private float bobAmplitude = 0.04f;
    [SerializeField] private float bobSpeed = 3f;

    private Renderer _renderer;
    private MaterialPropertyBlock _mpb;
    private Sprite[] _frames;
    private int _currentFrame;
    private float _frameTimer;
    private float _bobOffset;
    private Vector3 _baseLocalPos;
    private bool _isInitialized;

    public int CurrentFrame => _currentFrame;
    public float FrameRate
    {
        get => frameRate;
        set => frameRate = value;
    }

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();
    }

    public void Initialize(Sprite[] frames, int startFrame = 0)
    {
        if (frames == null || frames.Length == 0)
        {
            Debug.LogWarning("[AnimatedFlame] No frames provided!");
            return;
        }

        _frames = frames;
        _currentFrame = Mathf.Clamp(startFrame, 0, _frames.Length - 1);
        _bobOffset = Random.Range(0f, 100f);
        _baseLocalPos = transform.localPosition;
        _isInitialized = true;

        if (autoStart)
            SetFrame(_currentFrame);
    }

    public void Initialize(Texture2D[] frames, int startFrame = 0)
    {
        if (frames == null || frames.Length == 0)
        {
            Debug.LogWarning("[AnimatedFlame] No texture frames provided!");
            return;
        }

        _frames = new Sprite[frames.Length];
        for (int i = 0; i < frames.Length; i++)
        {
            _frames[i] = Sprite.Create(frames[i], new Rect(0, 0, frames[i].width, frames[i].height), Vector2.one * 0.5f);
        }

        Initialize(_frames, startFrame);
    }

    void OnEnable()
    {
        if (_isInitialized && _frames != null && _frames.Length > 0)
            SetFrame(_currentFrame);
    }

    void Update()
    {
        if (!_isInitialized || _frames == null || _frames.Length == 0) return;

        _frameTimer += Time.deltaTime;
        if (_frameTimer >= 1f / frameRate)
        {
            _frameTimer = 0f;
            _currentFrame = (_currentFrame + 1) % _frames.Length;
            SetFrame(_currentFrame);
        }

        float bob = Mathf.Sin(Time.time * bobSpeed + _bobOffset) * bobAmplitude;
        transform.localPosition = _baseLocalPos + new Vector3(0f, bob, 0f);
    }

    private void SetFrame(int frame)
    {
        if (_renderer == null || _frames == null || frame < 0 || frame >= _frames.Length) return;

        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetTexture("_MainTex", _frames[frame].texture);
        _renderer.SetPropertyBlock(_mpb);
    }

    public void SetFrameRate(float fps) => frameRate = fps;
    public void Pause() => autoStart = false;
    public void Play() => autoStart = true;
    public void Reset() => _currentFrame = 0;
}

public static class FlameGenerator
{
    public static Texture2D[] GenerateFlameFrames(int frameCount = 6, int width = 16, int height = 24, uint? seed = null)
    {
        var rng = seed.HasValue ? new System.Random((int)seed.Value) : new System.Random();
        var frames = new Texture2D[frameCount];

        for (int f = 0; f < frameCount; f++)
        {
            var canvas = new PixelCanvas(width, height);
            canvas.DefaultColor = Color.clear;

            int[] heights = GetFlameHeights(f, rng);
            int shift = GetFlameShift(f);

            for (int x = 0; x < width; x++)
            {
                int sx = Mathf.Clamp(x - shift, 0, width - 1);
                int maxH = heights[sx];
                float edgeDist = Mathf.Abs(x - width / 2f) / (width / 2f);

                for (int y = 0; y < maxH && y < height; y++)
                {
                    float t = (float)y / maxH;
                    float alpha = 1f;

                    if (edgeDist > 0.5f)
                        alpha = Mathf.Lerp(1f, 0f, (edgeDist - 0.5f) * 2f);

                    if (y > maxH * 0.8f)
                        alpha *= Mathf.Lerp(1f, 0.3f, (y - maxH * 0.8f) / (maxH * 0.2f));

                    byte a = (byte)(alpha * 255);
                    canvas.SetPixel(x, y, GetFlameColor(t, a, rng));
                }
            }

            frames[f] = canvas.ToTexture();
        }

        return frames;
    }

    public static Sprite[] GenerateFlameSprites(int frameCount = 6, int width = 16, int height = 24, uint? seed = null)
    {
        var frames = GenerateFlameFrames(frameCount, width, height, seed);
        var sprites = new Sprite[frameCount];

        for (int i = 0; i < frames.Length; i++)
        {
            sprites[i] = Sprite.Create(frames[i], new Rect(0, 0, width, height), Vector2.one * 0.5f);
        }

        return sprites;
    }

    public static Texture2D GenerateFlameSpritesheet(int frameCount = 6, int width = 16, int height = 24, uint? seed = null)
    {
        var frames = GenerateFlameFrames(frameCount, width, height, seed);
        var sheet = new SpriteSheet(width, height, frameCount, 1);

        for (int i = 0; i < frames.Length; i++)
        {
            var colors = frames[i].GetPixels();
            sheet.SetFrameData(i, colors);
        }

        return sheet.Bake();
    }

    private static int[] GetFlameHeights(int frame, System.Random rng)
    {
        int variation = rng.Next(-1, 2);
        return frame switch
        {
            0 => new[] { 0, 3, 6, 10, 13, 15, 16, 15, 13, 10, 7, 5, 3, 1, 0, 0, 0 },
            1 => new[] { 0, 2, 5, 9, 12, 14, 16, 16, 14, 11, 8, 6, 4, 2, 0, 0, 0 },
            2 => new[] { 0, 1, 4, 8, 11, 14, 17, 17, 15, 12, 9, 7, 5, 3, 1, 0, 0 },
            3 => new[] { 0, 0, 3, 7, 10, 13, 16, 16, 14, 11, 8, 6, 4, 2, 0, 0, 0 },
            4 => new[] { 0, 1, 3, 6, 10, 13, 15, 15, 13, 10, 7, 5, 3, 1, 0, 0, 0 },
            _ => new[] { 0, 2, 4, 8, 11, 13, 14, 13, 11, 9, 6, 4, 2, 1, 0, 0, 0 }
        };
    }

    private static int GetFlameShift(int frame)
    {
        return frame switch { 0 => 0, 1 => 1, 2 => 2, 3 => 1, 4 => 0, 5 => -1, _ => 0 };
    }

    private static Color32 GetFlameColor(float t, byte alpha, System.Random rng)
    {
        int colorVar = rng.Next(-10, 11);

        return t switch
        {
            < 0.15f => new Color32((byte)(120 + colorVar), 20, 0, alpha),
            < 0.30f => new Color32((byte)(180 + colorVar), (byte)(40 + rng.Next(-5, 6)), 0, alpha),
            < 0.45f => new Color32((byte)(220 + colorVar), (byte)(80 + rng.Next(-5, 6)), 10, alpha),
            < 0.60f => new Color32(255, (byte)(140 + rng.Next(-5, 6)), (byte)(20 + rng.Next(-3, 4)), alpha),
            < 0.75f => new Color32(255, (byte)(200 + rng.Next(-5, 6)), (byte)(50 + rng.Next(-5, 6)), alpha),
            < 0.88f => new Color32(255, (byte)(230 + rng.Next(-3, 4)), (byte)(100 + rng.Next(-5, 6)), alpha),
            _ => new Color32(255, 255, (byte)(200 + rng.Next(-10, 11)), alpha)
        };
    }
}
}
