using System;
using UnityEngine;

public static class DrawingManager
{
    public static readonly Color32[] EGA_PALETTE = new Color32[16]
    {
        new(0x00, 0x00, 0x00, 0xFF),
        new(0x00, 0x00, 0xAA, 0xFF),
        new(0x00, 0xAA, 0x00, 0xFF),
        new(0x00, 0xAA, 0xAA, 0xFF),
        new(0xAA, 0x00, 0x00, 0xFF),
        new(0xAA, 0x00, 0xAA, 0xFF),
        new(0xAA, 0x55, 0x00, 0xFF),
        new(0xAA, 0xAA, 0xAA, 0xFF),
        new(0x55, 0x55, 0x55, 0xFF),
        new(0x55, 0x55, 0xFF, 0xFF),
        new(0x55, 0xFF, 0x55, 0xFF),
        new(0x55, 0xFF, 0xFF, 0xFF),
        new(0xFF, 0x55, 0x55, 0xFF),
        new(0xFF, 0x55, 0xFF, 0xFF),
        new(0xFF, 0xFF, 0x55, 0xFF),
        new(0xFF, 0xFF, 0xFF, 0xFF)
    };

    public static Color32 Blend(Color32 a, Color32 b, float t)
    {
        t = Mathf.Clamp01(t);
        return new Color32(
            (byte)(a.r + (b.r - a.r) * t),
            (byte)(a.g + (b.g - a.g) * t),
            (byte)(a.b + (b.b - a.b) * t),
            (byte)(a.a + (b.a - a.a) * t)
        );
    }

    public static Color32 Darken(Color32 c, float amount) => Blend(c, new Color32(0, 0, 0, 0), amount);
    public static Color32 Lighten(Color32 c, float amount) => Blend(c, new Color32(255, 255, 255, 0), amount);

    public static Color32 Grayscale(Color32 c)
    {
        byte v = (byte)((c.r * 299 + c.g * 587 + c.b * 114) / 1000);
        return new Color32(v, v, v, c.a);
    }

    public static void FillRect(Color32[] pixels, int width, int height, int x, int y, int w, int h, Color32 color)
    {
        int x0 = Mathf.Clamp(x, 0, width - 1);
        int y0 = Mathf.Clamp(y, 0, height - 1);
        int x1 = Mathf.Clamp(x + w, 0, width);
        int y1 = Mathf.Clamp(y + h, 0, height);

        for (int py = y0; py < y1; py++)
            for (int px = x0; px < x1; px++)
                pixels[py * width + px] = color;
    }

    public static void DrawRect(Color32[] pixels, int width, int height, int x, int y, int w, int h, Color32 color)
    {
        FillRect(pixels, width, height, x, y, w, 1, color);
        FillRect(pixels, width, height, x, y + h - 1, w, 1, color);
        FillRect(pixels, width, height, x, y, 1, h, color);
        FillRect(pixels, width, height, x + w - 1, y, 1, h, color);
    }

    public static void DrawLine(Color32[] pixels, int width, int height, int x0, int y0, int x1, int y1, Color32 color)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (x0 >= 0 && x0 < width && y0 >= 0 && y0 < height)
                pixels[y0 * width + x0] = color;

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    public static void FillCircle(Color32[] pixels, int width, int height, int cx, int cy, int radius, Color32 color)
    {
        int r2 = radius * radius;
        for (int y = -radius; y <= radius; y++)
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= r2)
                {
                    int px = cx + x;
                    int py = cy + y;
                    if (px >= 0 && px < width && py >= 0 && py < height)
                        pixels[py * width + px] = color;
                }
            }
    }

    public static void DrawCircle(Color32[] pixels, int width, int height, int cx, int cy, int radius, Color32 color)
    {
        int x = radius;
        int y = 0;
        int err = 0;

        while (x >= y)
        {
            PlotCirclePoints(pixels, width, height, cx, cy, x, y, color);

            y++;
            err += 1 + 2 * y;
            if (2 * (err - x) + 1 > 0)
            {
                x--;
                err += 1 - 2 * x;
            }
        }
    }

    private static void PlotCirclePoints(Color32[] pixels, int width, int height, int cx, int cy, int x, int y, Color32 c)
    {
        SetPixelSafe(pixels, width, height, cx + x, cy + y, c);
        SetPixelSafe(pixels, width, height, cx - x, cy + y, c);
        SetPixelSafe(pixels, width, height, cx + x, cy - y, c);
        SetPixelSafe(pixels, width, height, cx - x, cy - y, c);
        SetPixelSafe(pixels, width, height, cx + y, cy + x, c);
        SetPixelSafe(pixels, width, height, cx - y, cy + x, c);
        SetPixelSafe(pixels, width, height, cx + y, cy - x, c);
        SetPixelSafe(pixels, width, height, cx - y, cy - x, c);
    }

    public static void SetPixelSafe(Color32[] pixels, int width, int height, int x, int y, Color32 color)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
            pixels[y * width + x] = color;
    }

    public static Color32 GetPixelSafe(Color32[] pixels, int width, int height, int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
            return pixels[y * width + x];
        return new Color32(0, 0, 0, 0);
    }

    public static void FlipHorizontal(Color32[] pixels, int width, int height)
    {
        for (int y = 0; y < height / 2; y++)
        {
            int y2 = height - 1 - y;
            for (int x = 0; x < width; x++)
            {
                int i1 = y * width + x;
                int i2 = y2 * width + x;
                (pixels[i1], pixels[i2]) = (pixels[i2], pixels[i1]);
            }
        }
    }

    public static void FlipVertical(Color32[] pixels, int width, int height)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width / 2; x++)
            {
                int x2 = width - 1 - x;
                int i1 = y * width + x;
                int i2 = y * width + x2;
                (pixels[i1], pixels[i2]) = (pixels[i2], pixels[i1]);
            }
        }
    }

    public static void Fill(Color32[] pixels, int width, int height, Color32 color)
    {
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
    }

    public static Texture2D CreateTexture(int width, int height, Color32[] pixels, FilterMode filterMode = FilterMode.Point)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            filterMode = filterMode,
            wrapMode = TextureWrapMode.Clamp,
            anisoLevel = 0
        };
        tex.SetPixels32(pixels);
        tex.Apply();
        return tex;
    }

    public static Texture2D CreateTexture(int width, int height, int[] pixels, Color32[] palette, int defaultIndex = 0)
    {
        var px = new Color32[width * height];
        var defaultColor = defaultIndex >= 0 && defaultIndex < palette.Length ? palette[defaultIndex] : new Color32(0, 0, 0, 0);
        for (int i = 0; i < px.Length; i++)
            px[i] = (pixels[i] >= 0 && pixels[i] < palette.Length) ? palette[pixels[i]] : defaultColor;
        return CreateTexture(width, height, px);
    }

    public static bool SaveTexture(Texture2D texture, string folderPath, string fileName)
    {
        try
        {
            if (!System.IO.Directory.Exists(folderPath))
                System.IO.Directory.CreateDirectory(folderPath);

            string fullPath = System.IO.Path.Combine(folderPath, fileName);
            if (!fullPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                fullPath += ".png";

            byte[] pngData = texture.EncodeToPNG();
            if (pngData == null || pngData.Length == 0)
            {
                Debug.LogError($"[DrawingManager] Failed to encode texture to PNG: {fileName}");
                return false;
            }

            System.IO.File.WriteAllBytes(fullPath, pngData);
            Debug.Log($"[DrawingManager] Saved texture: {fullPath}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DrawingManager] Error saving texture '{fileName}': {e.Message}");
            return false;
        }
    }

    public static bool SaveAllFromSpriteSheet(SpriteSheet sheet, string folderPath, string baseName, bool saveIndividualFrames = true)
    {
        try
        {
            if (!System.IO.Directory.Exists(folderPath))
                System.IO.Directory.CreateDirectory(folderPath);

            if (sheet.Texture == null)
                sheet.Bake();

            string fullPath = System.IO.Path.Combine(folderPath, baseName);
            if (!fullPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                fullPath += ".png";

            byte[] pngData = sheet.Texture.EncodeToPNG();
            if (pngData == null || pngData.Length == 0)
            {
                Debug.LogError($"[DrawingManager] Failed to encode spritesheet to PNG: {baseName}");
                return false;
            }

            System.IO.File.WriteAllBytes(fullPath, pngData);
            Debug.Log($"[DrawingManager] Saved spritesheet: {fullPath}");

            if (saveIndividualFrames && sheet.Rows * sheet.Columns > 1)
            {
                int w = sheet.FrameWidth;
                int h = sheet.FrameHeight;
                for (int i = 0; i < sheet.Rows * sheet.Columns; i++)
                {
                    var frameTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                    Color[] frameColors = sheet.Texture.GetPixels((i % sheet.Columns) * w, (i / sheet.Columns) * h, w, h);
                    frameTex.SetPixels(frameColors);
                    frameTex.Apply();

                    string framePath = System.IO.Path.Combine(folderPath, $"{baseName}_frame{i:D2}.png");
                    System.IO.File.WriteAllBytes(framePath, frameTex.EncodeToPNG());
                }
                Debug.Log($"[DrawingManager] Saved {sheet.Rows * sheet.Columns} individual frames");
            }

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DrawingManager] Error saving spritesheet '{baseName}': {e.Message}");
            return false;
        }
    }
}

public class SpriteSheet
{
    public int FrameWidth { get; }
    public int FrameHeight { get; }
    public int Columns { get; }
    public int Rows { get; }
    public Color32[] Pixels { get; }
    public Texture2D Texture { get; private set; }

    public SpriteSheet(int frameWidth, int frameHeight, int columns, int rows)
    {
        FrameWidth = frameWidth;
        FrameHeight = frameHeight;
        Columns = columns;
        Rows = rows;
        Pixels = new Color32[frameWidth * frameHeight * columns * rows];
    }

    public void SetPixel(int frame, int x, int y, Color32 color)
    {
        int fx = (frame % Columns) * FrameWidth;
        int fy = (frame / Columns) * FrameHeight;
        DrawingManager.SetPixelSafe(Pixels, Columns * FrameWidth, Rows * FrameHeight, fx + x, fy + y, color);
    }

    public void FillFrame(int frame, Color32 color)
    {
        int fx = (frame % Columns) * FrameWidth;
        int fy = (frame / Columns) * FrameHeight;
        for (int y = 0; y < FrameHeight; y++)
            for (int x = 0; x < FrameWidth; x++)
                Pixels[(fy + y) * Columns * FrameWidth + (fx + x)] = color;
    }

    public void DrawFrame(int frame, Color32[] src, int srcW, int srcH)
    {
        int fx = (frame % Columns) * FrameWidth;
        int fy = (frame / Columns) * FrameHeight;
        for (int y = 0; y < Mathf.Min(FrameHeight, srcH); y++)
            for (int x = 0; x < Mathf.Min(FrameWidth, srcW); x++)
                Pixels[(fy + y) * Columns * FrameWidth + (fx + x)] = src[y * srcW + x];
    }

    public Texture2D Bake(FilterMode filterMode = FilterMode.Point)
    {
        Texture = DrawingManager.CreateTexture(Columns * FrameWidth, Rows * FrameHeight, Pixels, filterMode);
        return Texture;
    }

    public Color32 GetFramePixel(int frame, int x, int y)
    {
        int fx = (frame % Columns) * FrameWidth;
        int fy = (frame / Columns) * FrameHeight;
        return DrawingManager.GetPixelSafe(Pixels, Columns * FrameWidth, Rows * FrameHeight, fx + x, fy + y);
    }

    public void SetFrameData(int frame, Color[] colors)
    {
        int w = FrameWidth;
        int h = FrameHeight;
        int fx = (frame % Columns) * w;
        int fy = (frame / Columns) * h;

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int srcIdx = y * w + x;
                int dstIdx = (fy + y) * Columns * w + (fx + x);
                if (srcIdx < colors.Length && dstIdx < Pixels.Length)
                    Pixels[dstIdx] = (Color32)colors[srcIdx];
            }
    }
}

public class PixelCanvas
{
    public int Width { get; }
    public int Height { get; }
    public Color32[] Pixels { get; }
    public Color32 DefaultColor { get; set; } = new Color32(0, 0, 0, 0);

    public PixelCanvas(int width, int height)
    {
        Width = width;
        Height = height;
        Pixels = new Color32[width * height];
        Clear();
    }

    public void Clear() => DrawingManager.Fill(Pixels, Width, Height, DefaultColor);

    public void FillRect(int x, int y, int w, int h, Color32 color)
        => DrawingManager.FillRect(Pixels, Width, Height, x, y, w, h, color);

    public void DrawRect(int x, int y, int w, int h, Color32 color)
        => DrawingManager.DrawRect(Pixels, Width, Height, x, y, w, h, color);

    public void DrawLine(int x0, int y0, int x1, int y1, Color32 color)
        => DrawingManager.DrawLine(Pixels, Width, Height, x0, y0, x1, y1, color);

    public void FillCircle(int cx, int cy, int radius, Color32 color)
        => DrawingManager.FillCircle(Pixels, Width, Height, cx, cy, radius, color);

    public void DrawCircle(int cx, int cy, int radius, Color32 color)
        => DrawingManager.DrawCircle(Pixels, Width, Height, cx, cy, radius, color);

    public void SetPixel(int x, int y, Color32 color)
        => DrawingManager.SetPixelSafe(Pixels, Width, Height, x, y, color);

    public Color32 GetPixel(int x, int y)
        => DrawingManager.GetPixelSafe(Pixels, Width, Height, x, y);

    public void FlipHorizontal() => DrawingManager.FlipHorizontal(Pixels, Width, Height);
    public void FlipVertical() => DrawingManager.FlipVertical(Pixels, Width, Height);

    public Texture2D ToTexture(FilterMode filterMode = FilterMode.Point)
        => DrawingManager.CreateTexture(Width, Height, Pixels, filterMode);
}
