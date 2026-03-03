// ArtFactory.cs
// Complete pixel art texturization engine
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// ART: Centralized pixel art generation for all game assets
// - Wood, stone, metal, magic textures
// - Door textures (8-bit palettes)
// - Chest textures
// - Torch/flame animations
// - Texture caching system

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// ArtFactory - Complete pixel art texturization engine.
    /// Generates all pixel art textures for the game from one centralized system.
    /// 
    /// Features:
    /// - Procedural texture generation (wood, stone, metal, magic)
    /// - Door textures with 8-bit palettes
    /// - Chest textures and sprites
    /// - Torch flame animations
    /// - Texture caching for performance
    /// - Export to disk functionality
    /// </summary>
    public static class ArtFactory
    {
        #region Color Palettes

        // 8-Bit Wood Palette
        private static readonly Color32[] WOOD_PALETTE = {
            new Color32(40, 25, 15, 255),    // Dark wood shadow
            new Color32(60, 40, 25, 255),    // Dark wood
            new Color32(80, 55, 35, 255),    // Mid wood
            new Color32(100, 70, 45, 255),   // Light wood
            new Color32(120, 85, 55, 255),   // Highlight
            new Color32(140, 100, 65, 255),  // Bright highlight
        };

        // 8-Bit Stone Palette
        private static readonly Color32[] STONE_PALETTE = {
            new Color32(50, 50, 50, 255),    // Dark stone
            new Color32(70, 70, 70, 255),    // Mid stone
            new Color32(90, 90, 90, 255),    // Light stone
            new Color32(110, 110, 110, 255), // Highlight
            new Color32(40, 40, 45, 255),    // Shadow blue
        };

        // 8-Bit Metal Palette
        private static readonly Color32[] METAL_PALETTE = {
            new Color32(60, 60, 65, 255),    // Dark metal
            new Color32(80, 80, 90, 255),    // Mid metal
            new Color32(100, 100, 110, 255), // Light metal
            new Color32(120, 120, 130, 255), // Highlight
            new Color32(140, 140, 150, 255), // Bright
        };

        // 8-Bit Magic Palette
        private static readonly Color32[] MAGIC_PALETTE = {
            new Color32(60, 40, 80, 255),    // Dark purple
            new Color32(80, 50, 100, 255),   // Mid purple
            new Color32(100, 60, 120, 255),  // Light purple
            new Color32(120, 70, 140, 255),  // Highlight
            new Color32(140, 80, 160, 255),  // Bright
        };

        // Wall Colors (Dungeon)
        private static readonly Color32 WallMortar = new(28, 26, 24, 255);
        private static readonly Color32 WallDark = new(55, 50, 45, 255);
        private static readonly Color32 WallMid = new(75, 68, 60, 255);
        private static readonly Color32 WallLight = new(95, 86, 76, 255);
        private static readonly Color32[] WALL_PALETTE = { WallMortar, WallDark, WallMid, WallLight };

        // Floor Colors
        private static readonly Color32 FloorDark = new(30, 26, 20, 255);
        private static readonly Color32 FloorMid = new(48, 42, 32, 255);
        private static readonly Color32[] FLOOR_PALETTE = { FloorDark, FloorMid };

        // Ceiling Colors
        private static readonly Color32 CeilDark = new(14, 13, 12, 255);
        private static readonly Color32 CeilMid = new(22, 20, 18, 255);
        private static readonly Color32[] CEIL_PALETTE = { CeilDark, CeilMid };

        #endregion

        #region Texture Cache

        // Cached textures
        private static Texture2D _wallTex, _floorTex, _ceilTex, _handleTex;
        private static Texture2D[] _flameFrames;
        private static Texture2D _woodDoorTex, _stoneDoorTex, _metalDoorTex, _magicDoorTex;
        private static Texture2D _chestWoodTex, _chestMetalTex;

        #endregion

        #region Public API - Get Textures

        /// <summary>
        /// Get wall texture (cached)
        /// </summary>
        public static Texture2D GetWallTexture() => _wallTex ??= BuildWallTexture();

        /// <summary>
        /// Get floor texture (cached)
        /// </summary>
        public static Texture2D GetFloorTexture() => _floorTex ??= BuildFloorTexture();

        /// <summary>
        /// Get ceiling texture (cached)
        /// </summary>
        public static Texture2D GetCeilingTexture() => _ceilTex ??= BuildCeilingTexture();

        /// <summary>
        /// Get torch handle texture (cached)
        /// </summary>
        public static Texture2D GetTorchHandleTexture() => _handleTex ??= BuildTorchHandleTexture();

        /// <summary>
        /// Get flame animation frames (cached)
        /// </summary>
        public static Texture2D[] GetFlameFrames()
        {
            if (_flameFrames == null)
            {
                _flameFrames = new Texture2D[6];
                for (int i = 0; i < 6; i++) _flameFrames[i] = BuildFlameFrame(i);
            }
            return _flameFrames;
        }

        /// <summary>
        /// Get door texture by type (cached)
        /// </summary>
        public static Texture2D GetDoorTexture(DoorType type)
        {
            return type switch
            {
                DoorType.Wood => _woodDoorTex ??= BuildWoodDoorTexture(),
                DoorType.Stone => _stoneDoorTex ??= BuildStoneDoorTexture(),
                DoorType.Metal => _metalDoorTex ??= BuildMetalDoorTexture(),
                DoorType.Magic => _magicDoorTex ??= BuildMagicDoorTexture(),
                _ => _woodDoorTex ??= BuildWoodDoorTexture()
            };
        }

        /// <summary>
        /// Get chest texture by type (cached)
        /// </summary>
        public static Texture2D GetChestTexture(ChestType type)
        {
            return type switch
            {
                ChestType.Wood => _chestWoodTex ??= BuildWoodChestTexture(),
                ChestType.Metal => _chestMetalTex ??= BuildMetalChestTexture(),
                _ => _chestWoodTex ??= BuildWoodChestTexture()
            };
        }

        #endregion

        #region Texture Types

        public enum DoorType { Wood, Stone, Metal, Magic }
        public enum ChestType { Wood, Metal }

        #endregion

        #region Cache Management

        /// <summary>
        /// Clear all cached textures
        /// </summary>
        public static void ClearCache()
        {
            DestroyTexture(ref _wallTex);
            DestroyTexture(ref _floorTex);
            DestroyTexture(ref _ceilTex);
            DestroyTexture(ref _handleTex);
            DestroyTexture(ref _woodDoorTex);
            DestroyTexture(ref _stoneDoorTex);
            DestroyTexture(ref _metalDoorTex);
            DestroyTexture(ref _magicDoorTex);
            DestroyTexture(ref _chestWoodTex);
            DestroyTexture(ref _chestMetalTex);

            if (_flameFrames != null)
            {
                for (int i = 0; i < _flameFrames.Length; i++)
                    DestroyTexture(ref _flameFrames[i]);
                _flameFrames = null;
            }
        }

        private static void DestroyTexture(ref Texture2D tex)
        {
            if (tex) { UnityEngine.Object.DestroyImmediate(tex); tex = null; }
        }

        #endregion

        #region Export System

        /// <summary>
        /// Export all textures to disk
        /// </summary>
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

            Debug.Log($"[ArtFactory] Exported all textures to: {folderPath}");
        }

        #endregion

        #region Wall/Floor/Ceiling Textures

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
                    canvas.SetPixel(x, y, pattern switch
                    {
                        0 => new Color32(35, 20, 10, 255),
                        1 => new Color32(45, 28, 15, 255),
                        _ => new Color32(40, 25, 12, 255)
                    });
                    if (y < 2 || y > h - 3) canvas.SetPixel(x, y, new Color32(15, 10, 5, 255));
                }
            return canvas.ToTexture();
        }

        #endregion

        #region Flame Textures

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

        #endregion

        #region Door Textures

        private static Texture2D BuildWoodDoorTexture()
        {
            const int w = 64, h = 96;
            var canvas = new PixelCanvas(w, h);

            // Wood planks
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int plankY = y / 16;
                    int plankPattern = (plankY % 2 == 0) ? 0 : 4;
                    int woodIdx = ((x / 8) + plankPattern + (y / 4)) % WOOD_PALETTE.Length;
                    canvas.SetPixel(x, y, WOOD_PALETTE[woodIdx]);
                }
            }

            // Door frame
            canvas.FillRect(0, 0, w, 4, WOOD_PALETTE[0]);  // Top
            canvas.FillRect(0, h - 4, w, 4, WOOD_PALETTE[0]);  // Bottom
            canvas.FillRect(0, 0, 4, h, WOOD_PALETTE[0]);  // Left
            canvas.FillRect(w - 4, 0, 4, h, WOOD_PALETTE[0]);  // Right

            return canvas.ToTexture();
        }

        private static Texture2D BuildStoneDoorTexture()
        {
            const int w = 64, h = 96;
            var canvas = new PixelCanvas(w, h);

            // Stone bricks
            for (int y = 0; y < h; y++)
            {
                int row = y / 12;
                int offset = (row % 2 == 0) ? 0 : 8;
                if (y % 12 == 0)
                {
                    canvas.FillRect(0, y, w, 1, STONE_PALETTE[0]);
                    continue;
                }

                for (int x = 0; x < w; x++)
                {
                    int brickX = (x + offset) % 16;
                    canvas.SetPixel(x, y, brickX == 0 ? STONE_PALETTE[0] : STONE_PALETTE[(row * 7 + (x + offset) / 16 * 13) % 4]);
                }
            }

            return canvas.ToTexture();
        }

        private static Texture2D BuildMetalDoorTexture()
        {
            const int w = 64, h = 96;
            var canvas = new PixelCanvas(w, h);

            // Metal plates
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool isRivet = (x % 16 == 0 || x % 16 == 15) && (y % 16 == 0 || y % 16 == 15);
                    int pattern = ((x / 16) + (y / 16)) % 2;

                    if (isRivet)
                        canvas.SetPixel(x, y, METAL_PALETTE[4]);
                    else
                        canvas.SetPixel(x, y, pattern == 0 ? METAL_PALETTE[1] : METAL_PALETTE[2]);
                }
            }

            return canvas.ToTexture();
        }

        private static Texture2D BuildMagicDoorTexture()
        {
            const int w = 64, h = 96;
            var canvas = new PixelCanvas(w, h);

            // Swirling magic pattern
            float time = Time.time * 0.5f;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float swirl = Mathf.Sin(x * 0.1f + time) * Mathf.Cos(y * 0.1f + time);
                    int magicIdx = Mathf.Clamp((int)((swirl + 1) * MAGIC_PALETTE.Length / 2), 0, MAGIC_PALETTE.Length - 1);
                    canvas.SetPixel(x, y, MAGIC_PALETTE[magicIdx]);
                }
            }

            return canvas.ToTexture();
        }

        #endregion

        #region Chest Textures

        private static Texture2D BuildWoodChestTexture()
        {
            const int w = 32, h = 24;
            var canvas = new PixelCanvas(w, h);

            // Wood body
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int woodIdx = ((x / 4) + (y / 3)) % WOOD_PALETTE.Length;
                    canvas.SetPixel(x, y, WOOD_PALETTE[woodIdx]);
                }
            }

            // Metal band
            canvas.FillRect(0, 8, w, 2, METAL_PALETTE[2]);
            canvas.FillRect(0, 16, w, 2, METAL_PALETTE[2]);

            // Lock
            canvas.FillRect(w / 2 - 2, 10, 4, 4, METAL_PALETTE[4]);

            return canvas.ToTexture();
        }

        private static Texture2D BuildMetalChestTexture()
        {
            const int w = 32, h = 24;
            var canvas = new PixelCanvas(w, h);

            // Metal body
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool isRivet = (x % 8 == 0 || x % 8 == 7) && (y % 8 == 0 || y % 8 == 7);
                    int pattern = ((x / 8) + (y / 8)) % 2;

                    if (isRivet)
                        canvas.SetPixel(x, y, METAL_PALETTE[4]);
                    else
                        canvas.SetPixel(x, y, pattern == 0 ? METAL_PALETTE[1] : METAL_PALETTE[2]);
                }
            }

            return canvas.ToTexture();
        }

        #endregion

        #region Particle Textures

        private static Texture2D _smokeTex, _sparkTex, _dustTex;

        /// <summary>
        /// Get particle texture by type (cached)
        /// </summary>
        public static Texture2D GetParticleTexture(ParticleType type)
        {
            return type switch
            {
                ParticleType.Flame => GetFlameFrames()[0],
                ParticleType.Smoke => _smokeTex ??= BuildSmokeTexture(),
                ParticleType.Spark => _sparkTex ??= BuildSparkTexture(),
                ParticleType.Dust => _dustTex ??= BuildDustTexture(),
                _ => GetFlameFrames()[0]
            };
        }

        /// <summary>
        /// Particle types for texture generation
        /// </summary>
        public enum ParticleType { Flame, Smoke, Spark, Dust, Magic }

        private static Texture2D BuildSmokeTexture()
        {
            const int size = 16;
            var canvas = new PixelCanvas(size, size);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Mathf.Sqrt(Mathf.Pow(x - size / 2f, 2) + Mathf.Pow(y - size / 2f, 2));
                    float alpha = Mathf.Clamp01(1f - dist / (size / 2f));
                    byte a = (byte)(alpha * 128);  // Semi-transparent
                    canvas.SetPixel(x, y, new Color32(128, 128, 128, a));
                }
            }

            return canvas.ToTexture();
        }

        private static Texture2D BuildSparkTexture()
        {
            const int size = 8;
            var canvas = new PixelCanvas(size, size);
            canvas.DefaultColor = Color.clear;

            // Bright yellow/white spark
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Mathf.Sqrt(Mathf.Pow(x - size / 2f, 2) + Mathf.Pow(y - size / 2f, 2));
                    if (dist < size / 3f)
                    {
                        byte brightness = (byte)(255 - dist * 40);
                        canvas.SetPixel(x, y, new Color32(brightness, brightness, 100, 255));
                    }
                }
            }

            return canvas.ToTexture();
        }

        private static Texture2D BuildDustTexture()
        {
            const int size = 4;
            var canvas = new PixelCanvas(size, size);
            canvas.DefaultColor = Color.clear;

            // Tiny dust motes
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (Random.value > 0.5f)
                    {
                        byte gray = (byte)(200 + Random.value * 55);
                        canvas.SetPixel(x, y, new Color32(gray, gray, gray, 180));
                    }
                }
            }

            return canvas.ToTexture();
        }

        #endregion

        #region Utility Textures (Legacy Support)

        /// <summary>
        /// Generate wood texture (legacy - use GetDoorTexture instead)
        /// </summary>
        public static Texture2D GenerateWoodTexture(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int grainPattern = ((x / 4) + (y / 3)) % 3;
                    Color woodColor = grainPattern switch
                    {
                        0 => new Color(0.2f, 0.12f, 0.08f),
                        1 => new Color(0.27f, 0.2f, 0.12f),
                        _ => new Color(0.35f, 0.26f, 0.16f)
                    };

                    float noise = Random.value * 0.1f;
                    woodColor += new Color(noise, noise, noise);

                    pixels[y * width + x] = woodColor;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        /// <summary>
        /// Generate stone texture (legacy - use GetWallTexture instead)
        /// </summary>
        public static Texture2D GenerateStoneTexture(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int brickX = (x / 8) % 2;
                    int brickY = y / 6;
                    bool isMortar = (x % 8 < 1) || (y % 6 < 1);

                    Color stoneColor;
                    if (isMortar)
                    {
                        stoneColor = new Color(0.3f, 0.3f, 0.3f);
                    }
                    else
                    {
                        int pattern = ((brickX + brickY) % 3);
                        stoneColor = pattern switch
                        {
                            0 => new Color(0.4f, 0.38f, 0.35f),
                            1 => new Color(0.45f, 0.42f, 0.4f),
                            _ => new Color(0.5f, 0.47f, 0.45f)
                        };
                    }

                    float noise = Random.value * 0.08f;
                    stoneColor += new Color(noise, noise, noise);

                    pixels[y * width + x] = stoneColor;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        /// <summary>
        /// Generate metal texture (legacy)
        /// </summary>
        public static Texture2D GenerateMetalTexture(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isRivet = (x % 8 == 0 || x % 8 == 7) && (y % 8 == 0 || y % 8 == 7);

                    Color metalColor;
                    if (isRivet)
                    {
                        metalColor = new Color(0.7f, 0.65f, 0.5f);
                    }
                    else
                    {
                        int pattern = ((x / 8) + (y / 8)) % 2;
                        metalColor = pattern switch
                        {
                            0 => new Color(0.5f, 0.48f, 0.45f),
                            _ => new Color(0.55f, 0.52f, 0.5f)
                        };
                    }

                    float sheen = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.1f;
                    metalColor += new Color(sheen, sheen, sheen);

                    pixels[y * width + x] = metalColor;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        #endregion
    }
}
