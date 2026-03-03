// PixelArtDoorTextures.cs
// Generates 8-bit pixel art door textures with alpha channel
// Unity 6 compatible - UTF-8 encoding - Unix line endings
// Location: Assets/Art/DoorTextures/
//
// Plug-in-and-Out: Works with RealisticDoorFactory

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Pixel art door texture generator.
    /// Creates 8-bit style textures with alpha channel for doors.
    /// Textures cached for reuse across all doors.
    /// </summary>
    public static class PixelArtDoorTextures
    {
        #region 8-Bit Color Palette

        // Classic 8-bit / 16-bit color palettes
        private static readonly Color32[] WOOD_PALETTE = {
            new Color32(40, 25, 15, 255),    // Dark wood shadow
            new Color32(60, 40, 25, 255),    // Dark wood
            new Color32(80, 55, 35, 255),    // Mid wood
            new Color32(100, 70, 45, 255),   // Light wood
            new Color32(120, 85, 55, 255),   // Highlight
            new Color32(140, 100, 65, 255),  // Bright highlight
        };

        private static readonly Color32[] STONE_PALETTE = {
            new Color32(50, 50, 50, 255),    // Dark stone
            new Color32(70, 70, 70, 255),    // Mid stone
            new Color32(90, 90, 90, 255),    // Light stone
            new Color32(110, 110, 110, 255), // Highlight
            new Color32(40, 40, 45, 255),    // Shadow blue
        };

        private static readonly Color32[] METAL_PALETTE = {
            new Color32(60, 60, 65, 255),    // Dark metal
            new Color32(80, 80, 90, 255),    // Mid metal
            new Color32(100, 100, 110, 255), // Light metal
            new Color32(120, 120, 130, 255), // Highlight
            new Color32(140, 140, 150, 255), // Bright
        };

        private static readonly Color32[] MAGIC_PALETTE = {
            new Color32(60, 40, 80, 255),    // Dark purple
            new Color32(80, 50, 100, 255),   // Mid purple
            new Color32(100, 60, 120, 255),  // Light purple
            new Color32(120, 80, 140, 255),  // Glow
            new Color32(140, 100, 160, 200), // Transparent glow
        };

        private static readonly Color32[] IRON_PALETTE = {
            new Color32(50, 45, 40, 255),    // Dark iron
            new Color32(70, 60, 50, 255),    // Mid iron
            new Color32(90, 75, 60, 255),    // Light iron
            new Color32(110, 90, 70, 255),   // Rust
            new Color32(80, 70, 60, 255),    // Weathered
        };

        #endregion

        #region Cached Textures

        private static Texture2D _woodPanelTex;
        private static Texture2D _woodStileTex;
        private static Texture2D _woodFrameTex;
        private static Texture2D _stonePanelTex;
        private static Texture2D _metalPanelTex;
        private static Texture2D _magicPanelTex;
        private static Texture2D _ironPanelTex;
        private static Texture2D _handleTex;
        private static Texture2D _hingeTex;

        #endregion

        #region Public Getters

        public static Texture2D GetWoodPanelTexture() => _woodPanelTex ??= GenerateWoodPanelTexture(32, 32);
        public static Texture2D GetWoodStileTexture() => _woodStileTex ??= GenerateWoodStileTexture(16, 32);
        public static Texture2D GetWoodFrameTexture() => _woodFrameTex ??= GenerateWoodFrameTexture(16, 16);
        public static Texture2D GetStonePanelTexture() => _stonePanelTex ??= GenerateStonePanelTexture(32, 32);
        public static Texture2D GetMetalPanelTexture() => _metalPanelTex ??= GenerateMetalPanelTexture(32, 32);
        public static Texture2D GetMagicPanelTexture() => _magicPanelTex ??= GenerateMagicPanelTexture(32, 32);
        public static Texture2D GetIronPanelTexture() => _ironPanelTex ??= GenerateIronPanelTexture(32, 32);
        public static Texture2D GetHandleTexture() => _handleTex ??= GenerateHandleTexture(16, 16);
        public static Texture2D GetHingeTexture() => _hingeTex ??= GenerateHingeTexture(8, 16);

        #endregion

        #region Wood Door Textures

        private static Texture2D GenerateWoodPanelTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.filterMode = FilterMode.Point;

            Color32[] pixels = new Color32[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Wood grain pattern (vertical)
                    int grainOffset = (x / 4) + (y / 6);
                    int colorIndex = grainOffset % WOOD_PALETTE.Length;

                    // Add pixel noise for 8-bit feel
                    float noise = (Random.value - 0.5f) * 0.15f;
                    Color32 baseColor = WOOD_PALETTE[colorIndex];
                    pixels[y * width + x] = AdjustColor(baseColor, noise);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateWoodStileTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.filterMode = FilterMode.Point;

            Color32[] pixels = new Color32[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Darker wood for stiles (frame pieces)
                    int colorIndex = (y / 8) % (WOOD_PALETTE.Length - 2);
                    Color32 baseColor = WOOD_PALETTE[colorIndex + 1]; // Skip darkest

                    // Add bevel effect on edges
                    if (x == 0 || x == width - 1)
                    {
                        baseColor = AdjustColor(baseColor, -0.2f); // Darker edge
                    }

                    pixels[y * width + x] = baseColor;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateWoodFrameTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.filterMode = FilterMode.Point;

            Color32[] pixels = new Color32[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Frame uses darker wood
                    int colorIndex = (x + y) / 6 % (WOOD_PALETTE.Length - 1);
                    pixels[y * width + x] = WOOD_PALETTE[colorIndex];
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }

        #endregion

        #region Stone Door Textures

        private static Texture2D GenerateStonePanelTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.filterMode = FilterMode.Point;

            Color32[] pixels = new Color32[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Stone brick pattern
                    int brickX = x / 8;
                    int brickY = y / 6;

                    // Mortar lines
                    if (x % 8 < 2 || y % 6 < 2)
                    {
                        pixels[y * width + x] = new Color32(60, 55, 50, 255); // Mortar
                    }
                    else
                    {
                        // Stone color with offset for brick pattern
                        int colorIndex = (brickX + brickY) % STONE_PALETTE.Length;
                        pixels[y * width + x] = STONE_PALETTE[colorIndex];
                    }
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }

        #endregion

        #region Metal Door Textures

        private static Texture2D GenerateMetalPanelTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.filterMode = FilterMode.Point;

            Color32[] pixels = new Color32[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Riveted metal plates
                    int plateX = x / 16;
                    int plateY = y / 16;

                    // Rivets at corners
                    bool isRivet = (x % 16 < 3 && y % 16 < 3) ||
                                   (x % 16 > 12 && y % 16 < 3) ||
                                   (x % 16 < 3 && y % 16 > 12) ||
                                   (x % 16 > 12 && y % 16 > 12);

                    if (isRivet)
                    {
                        pixels[y * width + x] = new Color32(180, 180, 180, 255); // Rivet
                    }
                    else
                    {
                        // Metal plate with horizontal brush marks
                        int colorIndex = (x / 4) % METAL_PALETTE.Length;
                        pixels[y * width + x] = METAL_PALETTE[colorIndex];
                    }
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }

        #endregion

        #region Magic Door Textures

        private static Texture2D GenerateMagicPanelTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.filterMode = FilterMode.Point;

            Color32[] pixels = new Color32[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Magical swirling pattern
                    float swirl = Mathf.Sin(x * 0.3f) * Mathf.Cos(y * 0.3f);
                    int colorIndex = Mathf.FloorToInt((swirl + 1) * (MAGIC_PALETTE.Length - 1) / 2);

                    // Add glow effect with alpha
                    Color32 baseColor = MAGIC_PALETTE[colorIndex];
                    if ((x + y) % 7 < 3)
                    {
                        // Sparkle
                        baseColor = new Color32(
                            (byte)Mathf.Min(255, baseColor.r + 40),
                            (byte)Mathf.Min(255, baseColor.g + 20),
                            (byte)Mathf.Min(255, baseColor.b + 60),
                            200 // Semi-transparent sparkle
                        );
                    }

                    pixels[y * width + x] = baseColor;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }

        #endregion

        #region Iron Door Textures

        private static Texture2D GenerateIronPanelTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.filterMode = FilterMode.Point;

            Color32[] pixels = new Color32[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Weathered iron with rust patches
                    int rustPatch = ((x / 10) + (y / 10)) % 5;

                    Color32 baseColor;
                    if (rustPatch == 0)
                    {
                        baseColor = IRON_PALETTE[3]; // Rust
                    }
                    else
                    {
                        int colorIndex = (x + y) / 8 % (IRON_PALETTE.Length - 1);
                        baseColor = IRON_PALETTE[colorIndex];
                    }

                    // Add weathering noise
                    float noise = (Random.value - 0.5f) * 0.2f;
                    pixels[y * width + x] = AdjustColor(baseColor, noise);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }

        #endregion

        #region Hardware Textures

        private static Texture2D GenerateHandleTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.filterMode = FilterMode.Point;

            Color32[] pixels = new Color32[width * height];

            // Brass/gold handle
            Color32 brass = new Color32(200, 160, 60, 255);
            Color32 brassHighlight = new Color32(230, 200, 100, 255);
            Color32 brassShadow = new Color32(150, 110, 40, 255);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Circular handle shape
                    float dist = Mathf.Sqrt((x - width / 2f) * (x - width / 2f) +
                                           (y - height / 2f) * (y - height / 2f));

                    if (dist < width / 3f)
                    {
                        // Highlight on top-left
                        if (x < width / 2f && y < height / 2f)
                        {
                            pixels[y * width + x] = brassHighlight;
                        }
                        else if (x > width / 2f && y > height / 2f)
                        {
                            pixels[y * width + x] = brassShadow;
                        }
                        else
                        {
                            pixels[y * width + x] = brass;
                        }
                    }
                    else
                    {
                        pixels[y * width + x] = new Color32(0, 0, 0, 0); // Transparent
                    }
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }

        private static Texture2D GenerateHingeTexture(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.filterMode = FilterMode.Point;

            Color32[] pixels = new Color32[width * height];

            // Bronze/iron hinge
            Color32 bronze = new Color32(180, 140, 80, 255);
            Color32 bronzeDark = new Color32(120, 90, 50, 255);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Rectangular hinge plate
                    if (x < width - 2)
                    {
                        // Rivets
                        if ((y == 2 || y == height - 3) && x < width - 3)
                        {
                            pixels[y * width + x] = bronzeDark;
                        }
                        else
                        {
                            pixels[y * width + x] = bronze;
                        }
                    }
                    else
                    {
                        pixels[y * width + x] = new Color32(0, 0, 0, 0); // Transparent
                    }
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }

        #endregion

        #region Utilities

        private static Color32 AdjustColor(Color32 baseColor, float adjustment)
        {
            int r = Mathf.Clamp((int)(baseColor.r + baseColor.r * adjustment), 0, 255);
            int g = Mathf.Clamp((int)(baseColor.g + baseColor.g * adjustment), 0, 255);
            int b = Mathf.Clamp((int)(baseColor.b + baseColor.b * adjustment), 0, 255);
            return new Color32((byte)r, (byte)g, (byte)b, baseColor.a);
        }

        public static void ClearCache()
        {
            if (_woodPanelTex) Object.DestroyImmediate(_woodPanelTex);
            if (_woodStileTex) Object.DestroyImmediate(_woodStileTex);
            if (_woodFrameTex) Object.DestroyImmediate(_woodFrameTex);
            if (_stonePanelTex) Object.DestroyImmediate(_stonePanelTex);
            if (_metalPanelTex) Object.DestroyImmediate(_metalPanelTex);
            if (_magicPanelTex) Object.DestroyImmediate(_magicPanelTex);
            if (_ironPanelTex) Object.DestroyImmediate(_ironPanelTex);
            if (_handleTex) Object.DestroyImmediate(_handleTex);
            if (_hingeTex) Object.DestroyImmediate(_hingeTex);
        }

        #endregion
    }
}
