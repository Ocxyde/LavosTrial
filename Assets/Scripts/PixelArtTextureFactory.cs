using UnityEngine;

public static class PixelArtTextureFactory
{
    private static Texture2D _wallTex, _floorTex, _ceilTex, _handleTex;
    private static Texture2D[] _flameFrames;

    private static readonly Color32 WallMortar = new(28, 26, 24, 255);
    private static readonly Color32 WallDark = new(55, 50, 45, 255);
    private static readonly Color32 WallMid = new(75, 68, 60, 255);
    private static readonly Color32 WallLight = new(95, 86, 76, 255);
    private static readonly Color32 FloorDark = new(30, 26, 20, 255);
    private static readonly Color32 FloorMid = new(48, 42, 32, 255);
    private static readonly Color32 CeilDark = new(14, 13, 12, 255);
    private static readonly Color32 CeilMid = new(22, 20, 18, 255);

    private static readonly Color32[] WALL_PALETTE = { WallMortar, WallDark, WallMid, WallLight };
    private static readonly Color32[] FLOOR_PALETTE = { FloorDark, FloorMid };
    private static readonly Color32[] CEIL_PALETTE = { CeilDark, CeilMid };

    public static Texture2D GetWallTexture() => _wallTex ??= BuildWallTexture();
    public static Texture2D GetFloorTexture() => _floorTex ??= BuildFloorTexture();
    public static Texture2D GetCeilingTexture() => _ceilTex ??= BuildCeilingTexture();
    public static Texture2D GetTorchHandleTexture() => _handleTex ??= BuildTorchHandleTexture();

    public static Texture2D[] GetFlameFrames()
    {
        if (_flameFrames == null)
        {
            _flameFrames = new Texture2D[6];
            for (int i = 0; i < 6; i++) _flameFrames[i] = BuildFlameFrame(i);
        }
        return _flameFrames;
    }

    public static void ClearCache()
    {
        DestroyTexture(ref _wallTex);
        DestroyTexture(ref _floorTex);
        DestroyTexture(ref _ceilTex);
        DestroyTexture(ref _handleTex);
        if (_flameFrames != null) { for (int i = 0; i < _flameFrames.Length; i++) DestroyTexture(ref _flameFrames[i]); _flameFrames = null; }
    }

    private static void DestroyTexture(ref Texture2D tex)
    {
        if (tex) { UnityEngine.Object.DestroyImmediate(tex); tex = null; }
    }

    public static void ExportAllToDisk(string folderPath)
    {
        ClearCache();
        DrawingManager.SaveTexture(GetWallTexture(), folderPath, "wall");
        DrawingManager.SaveTexture(GetFloorTexture(), folderPath, "floor");
        DrawingManager.SaveTexture(GetCeilingTexture(), folderPath, "ceiling");
        DrawingManager.SaveTexture(GetTorchHandleTexture(), folderPath, "torch_handle");

        var flames = new SpriteSheet(16, 24, 6, 1);
        for (int i = 0; i < 6; i++)
        {
            var frame = BuildFlameFrame(i);
            Color[] colors = frame.GetPixels();
            flames.SetFrameData(i, colors);
            UnityEngine.Object.DestroyImmediate(frame);
        }
        DrawingManager.SaveAllFromSpriteSheet(flames, folderPath, "flame", true);
        Debug.Log($"[PixelArtTextureFactory] Exported all textures to: {folderPath}");
    }

    private static Texture2D BuildWallTexture()
    {
        const int size = 32;
        var canvas = new PixelCanvas(size, size);
        canvas.DefaultColor = WallMid;

        for (int y = 0; y < size; y++)
        {
            int row = y / 8;
            int offset = (row % 2 == 0) ? 0 : 8;
            if (y % 8 == 0)
            {
                canvas.FillRect(0, y, size, 1, WallMortar);
                continue;
            }

            for (int x = 0; x < size; x++)
            {
                int brickX = (x + offset) % 16;
                canvas.SetPixel(x, y, brickX == 0 ? WallMortar : WALL_PALETTE[(row * 7 + (x + offset) / 16 * 13) % 4]);
            }
        }
        return canvas.ToTexture();
    }

    private static Texture2D BuildFloorTexture()
    {
        const int size = 32;
        var canvas = new PixelCanvas(size, size);
        canvas.DefaultColor = FloorMid;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                if (x % 16 == 0 || y % 16 == 0)
                {
                    canvas.SetPixel(x, y, new Color32(20, 18, 14, 255));
                    continue;
                }
                int tile = ((x / 16 + y / 16) * 17 + x * 3 + y * 5) % 5;
                canvas.SetPixel(x, y, tile switch { 0 or 1 => FloorDark, 3 => new Color32(62, 55, 42, 255), _ => FloorMid });
            }
        return canvas.ToTexture();
    }

    private static Texture2D BuildCeilingTexture()
    {
        const int size = 32;
        var canvas = new PixelCanvas(size, size);
        canvas.DefaultColor = CeilMid;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int pattern = (x * 3 + y * 7 + x * y) % 7;
                canvas.SetPixel(x, y, pattern switch { 0 or 1 => CeilDark, 5 => new Color32(32, 29, 26, 255), _ => CeilMid });
            }
        return canvas.ToTexture();
    }

    private static Texture2D BuildTorchHandleTexture()
    {
        const int w = 8, h = 24;
        var canvas = new PixelCanvas(w, h);

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int pattern = (y * 7 + x * 3) % 5;
                canvas.SetPixel(x, y, pattern switch { 0 => new Color32(35, 20, 10, 255), 1 => new Color32(45, 28, 15, 255), _ => new Color32(40, 25, 12, 255) });
                if (y < 2 || y > h - 3) canvas.SetPixel(x, y, new Color32(15, 10, 5, 255));
            }
        return canvas.ToTexture();
    }

    private static Texture2D BuildFlameFrame(int frame)
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
}
