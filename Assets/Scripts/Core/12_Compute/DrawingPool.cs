// DrawingPool.cs
// Object pool for procedural textures
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Ressources system - texture pooling

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    public class DrawingPool : MonoBehaviour
    {
    private static DrawingPool _instance;
    public static DrawingPool Instance => _instance ??= FindFirstObjectByType<DrawingPool>();

    private readonly Dictionary<string, Texture2D> _texturePool = new();
    private readonly Dictionary<string, Sprite> _spritePool = new();
    private readonly Dictionary<string, Sprite[]> _spriteArrayPool = new();

    private bool _initialized;
    private uint _currentSeed;

    [Header("Pre-generation")]
    [SerializeField] private int wallVariants = 4;
    [SerializeField] private int floorVariants = 4;
    [SerializeField] private int ceilVariants = 4;
    [SerializeField] private int torchVariants = 2;
    [SerializeField] private int flameFrameCount = 6;

    public uint CurrentSeed => _currentSeed;
    public bool IsInitialized => _initialized;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize(uint seed)
    {
        if (_initialized && _currentSeed == seed)
            return;

        Clear();
        _currentSeed = seed;
        UnityEngine.Random.InitState((int)seed);
        GenerateAllVariants();
        _initialized = true;
        Debug.Log($"[DrawingPool] Initialized with seed: {seed}");
    }

    public void Clear()
    {
        foreach (var tex in _texturePool.Values)
            if (tex) UnityEngine.Object.DestroyImmediate(tex);
        _texturePool.Clear();

        foreach (var spr in _spritePool.Values)
            if (spr) UnityEngine.Object.DestroyImmediate(spr);
        _spritePool.Clear();

        foreach (var arr in _spriteArrayPool.Values)
            if (arr != null)
                for (int i = 0; i < arr.Length; i++)
                    if (arr[i]) UnityEngine.Object.DestroyImmediate(arr[i]);
        _spriteArrayPool.Clear();

        _initialized = false;
    }

    private void GenerateAllVariants()
    {
        for (int i = 0; i < wallVariants; i++)
        {
            var tex = GenerateWallVariant(i);
            _texturePool[$"wall_{i}"] = tex;
            _spritePool[$"wall_{i}"] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 1f, 0, SpriteMeshType.FullRect);
        }

        for (int i = 0; i < floorVariants; i++)
        {
            var tex = GenerateFloorVariant(i);
            _texturePool[$"floor_{i}"] = tex;
            _spritePool[$"floor_{i}"] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 1f, 0, SpriteMeshType.FullRect);
        }

        for (int i = 0; i < ceilVariants; i++)
        {
            var tex = GenerateCeilingVariant(i);
            _texturePool[$"ceiling_{i}"] = tex;
            _spritePool[$"ceiling_{i}"] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 1f, 0, SpriteMeshType.FullRect);
        }

        for (int i = 0; i < torchVariants; i++)
        {
            var tex = GenerateTorchVariant(i);
            _texturePool[$"torch_{i}"] = tex;
            _spritePool[$"torch_{i}"] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 1f, 0, SpriteMeshType.FullRect);
        }

        var flameFrames = new Sprite[flameFrameCount];
        for (int i = 0; i < flameFrameCount; i++)
        {
            var tex = GenerateFlameFrame(i);
            _texturePool[$"flame_{i}"] = tex;
            flameFrames[i] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 1f, 0, SpriteMeshType.FullRect);
        }
        _spriteArrayPool["flames"] = flameFrames;
    }

    private Texture2D GenerateWallVariant(int variant)
    {
        const int size = 32;
        var canvas = new PixelCanvas(size, size);
        int seedOffset = variant * 1000 + (int)_currentSeed;
        var rng = new System.Random(seedOffset);

        var mortar = new Color32(28, 26, 24, 255);
        var dark = new Color32((byte)(55 + rng.Next(-5, 6)), (byte)(50 + rng.Next(-5, 6)), (byte)(45 + rng.Next(-5, 6)), 255);
        var mid = new Color32((byte)(75 + rng.Next(-8, 9)), (byte)(68 + rng.Next(-8, 9)), (byte)(60 + rng.Next(-8, 9)), 255);
        var light = new Color32((byte)(95 + rng.Next(-8, 9)), (byte)(86 + rng.Next(-8, 9)), (byte)(76 + rng.Next(-8, 9)), 255);

        for (int y = 0; y < size; y++)
        {
            int row = y / 8;
            int offset = (row % 2 == 0) ? 0 : 8;
            if (y % 8 == 0) { canvas.FillRect(0, y, size, 1, mortar); continue; }

            for (int x = 0; x < size; x++)
            {
                int brickX = (x + offset) % 16;
                canvas.SetPixel(x, y, brickX == 0 ? mortar : new[] { dark, mid, light, mid }[(row * 7 + (x + offset) / 16 * 13 + variant) % 4]);
            }
        }
        return canvas.ToTexture();
    }

    private Texture2D GenerateFloorVariant(int variant)
    {
        const int size = 32;
        var canvas = new PixelCanvas(size, size);
        int seedOffset = variant * 1000 + (int)_currentSeed;
        var rng = new System.Random(seedOffset);

        var dark = new Color32((byte)(30 + rng.Next(-3, 4)), (byte)(26 + rng.Next(-3, 4)), (byte)(20 + rng.Next(-3, 4)), 255);
        var mid = new Color32((byte)(48 + rng.Next(-5, 6)), (byte)(42 + rng.Next(-5, 6)), (byte)(32 + rng.Next(-5, 6)), 255);
        var tileColor = new Color32(62, 55, 42, 255);

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                if (x % 16 == 0 || y % 16 == 0)
                {
                    canvas.SetPixel(x, y, new Color32(20, 18, 14, 255));
                    continue;
                }
                int tile = ((x / 16 + y / 16) * 17 + x * 3 + y * 5 + variant) % 5;
                canvas.SetPixel(x, y, tile switch { 0 or 1 => dark, 3 => tileColor, _ => mid });
            }
        return canvas.ToTexture();
    }

    private Texture2D GenerateCeilingVariant(int variant)
    {
        const int size = 32;
        var canvas = new PixelCanvas(size, size);
        int seedOffset = variant * 1000 + (int)_currentSeed;
        var rng = new System.Random(seedOffset);

        var dark = new Color32(14, 13, 12, 255);
        var mid = new Color32((byte)(22 + rng.Next(-2, 3)), (byte)(20 + rng.Next(-2, 3)), (byte)(18 + rng.Next(-2, 3)), 255);
        var special = new Color32(32, 29, 26, 255);

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int pattern = (x * 3 + y * 7 + x * y + variant) % 7;
                canvas.SetPixel(x, y, pattern switch { 0 or 1 => dark, 5 => special, _ => mid });
            }
        return canvas.ToTexture();
    }

    private Texture2D GenerateTorchVariant(int variant)
    {
        const int w = 8, h = 24;
        var canvas = new PixelCanvas(w, h);
        int seedOffset = variant * 1000 + (int)_currentSeed;
        var rng = new System.Random(seedOffset);

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int pattern = (y * 7 + x * 3 + variant) % 5;
                canvas.SetPixel(x, y, pattern switch
                {
                    0 => new Color32((byte)(35 + rng.Next(-3, 4)), (byte)(20 + rng.Next(-2, 3)), (byte)(10 + rng.Next(-2, 3)), 255),
                    1 => new Color32((byte)(45 + rng.Next(-3, 4)), (byte)(28 + rng.Next(-2, 3)), (byte)(15 + rng.Next(-2, 3)), 255),
                    _ => new Color32((byte)(40 + rng.Next(-3, 4)), (byte)(25 + rng.Next(-2, 3)), (byte)(12 + rng.Next(-2, 3)), 255)
                });
                if (y < 2 || y > h - 3) canvas.SetPixel(x, y, new Color32(15, 10, 5, 255));
            }
        return canvas.ToTexture();
    }

    private Texture2D GenerateFlameFrame(int frame)
    {
        const int w = 16, h = 24;
        var canvas = new PixelCanvas(w, h);
        canvas.DefaultColor = Color.clear;

        int[] heights = frame switch
        {
            0 => new[] { 0, 3, 6, 10, 13, 15, 16, 15, 13, 10, 7, 5, 3, 1, 0, 0, 0 },
            1 => new[] { 0, 2, 5, 9, 12, 14, 16, 16, 14, 11, 8, 6, 4, 2, 0, 0, 0 },
            2 => new[] { 0, 1, 4, 8, 11, 14, 17, 17, 15, 12, 9, 7, 5, 3, 1, 0, 0 },
            3 => new[] { 0, 0, 3, 7, 10, 13, 16, 16, 14, 11, 8, 6, 4, 2, 0, 0, 0 },
            4 => new[] { 0, 1, 3, 6, 10, 13, 15, 15, 13, 10, 7, 5, 3, 1, 0, 0, 0 },
            _ => new[] { 0, 2, 4, 8, 11, 13, 14, 13, 11, 9, 6, 4, 2, 1, 0, 0, 0 }
        };

        int shift = frame switch { 0 => 0, 1 => 1, 2 => 2, 3 => 1, 4 => 0, 5 => -1, _ => 0 };

        for (int x = 0; x < w; x++)
        {
            int sx = Mathf.Clamp(x - shift, 0, w - 1);
            int maxH = heights[sx];
            float edgeDist = Mathf.Abs(x - w / 2f) / (w / 2f);

            for (int y = 0; y < maxH && y < h; y++)
            {
                float t = (float)y / maxH;
                float alpha = 1f;
                if (edgeDist > 0.5f) alpha = Mathf.Lerp(1f, 0f, (edgeDist - 0.5f) * 2f);
                if (y > maxH * 0.8f) alpha *= Mathf.Lerp(1f, 0.3f, (y - maxH * 0.8f) / (maxH * 0.2f));

                byte a = (byte)(alpha * 255);
                canvas.SetPixel(x, y, t switch
                {
                    < 0.15f => new Color32(120, 20, 0, a),
                    < 0.30f => new Color32(180, 40, 0, a),
                    < 0.45f => new Color32(220, 80, 10, a),
                    < 0.60f => new Color32(255, 140, 20, a),
                    < 0.75f => new Color32(255, 200, 50, a),
                    < 0.88f => new Color32(255, 230, 100, a),
                    _ => new Color32(255, 255, 200, a)
                });
            }
        }
        return canvas.ToTexture();
    }

    public Texture2D GetWall(int variant = 0) => GetTexture($"wall_{variant % wallVariants}");
    public Texture2D GetFloor(int variant = 0) => GetTexture($"floor_{variant % floorVariants}");
    public Texture2D GetCeiling(int variant = 0) => GetTexture($"ceiling_{variant % ceilVariants}");
    public Texture2D GetTorch(int variant = 0) => GetTexture($"torch_{variant % torchVariants}");

    public Sprite GetWallSprite(int variant = 0) => GetSprite($"wall_{variant % wallVariants}");
    public Sprite GetFloorSprite(int variant = 0) => GetSprite($"floor_{variant % floorVariants}");
    public Sprite GetCeilingSprite(int variant = 0) => GetSprite($"ceiling_{variant % ceilVariants}");
    public Sprite GetTorchSprite(int variant = 0) => GetSprite($"torch_{variant % torchVariants}");

    public Sprite[] GetFlameFrames() => GetSpriteArray("flames");

    public Texture2D[] GetFlameTextures()
    {
        var sprites = GetFlameFrames();
        if (sprites == null || sprites.Length == 0) return null;

        var textures = new Texture2D[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            textures[i] = sprites[i]?.texture;
        }
        return textures;
    }

    public Texture2D GetTexture(string key) => _texturePool.TryGetValue(key, out var tex) ? tex : null;
    public Sprite GetSprite(string key) => _spritePool.TryGetValue(key, out var spr) ? spr : null;
    public Sprite[] GetSpriteArray(string key) => _spriteArrayPool.TryGetValue(key, out var arr) ? arr : null;

    public int WallVariantCount => wallVariants;
    public int FloorVariantCount => floorVariants;
    public int CeilVariantCount => ceilVariants;
    public int TorchVariantCount => torchVariants;

    public void ExportAllToDisk(string folderPath)
    {
        foreach (var kvp in _texturePool)
        {
            DrawingManager.SaveTexture(kvp.Value, folderPath, kvp.Key);
        }
        Debug.Log($"[DrawingPool] Exported {_texturePool.Count} textures to: {folderPath}");
    }
}
}
