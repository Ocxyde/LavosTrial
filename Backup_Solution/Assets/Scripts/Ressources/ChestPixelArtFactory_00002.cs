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
// ChestPixelArtFactory.cs
// Procedural pixel-art texture generator for treasure chests
// Unity 6 compatible - UTF-8 encoding - Unix line endings
// Uses PixelArtTextureFactory patterns for consistency
// Location: Assets/Scripts/Ressources/

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Procedural pixel-art texture generator for treasure chests.
    /// Creates 8-bit style chest textures with customizable features.
    /// Caches textures for reuse; call ClearCache() when resources need to be rebuilt.
    /// </summary>
    public static class ChestPixelArtFactory
    {
        private static Texture2D _standardChestTex;
        private static Texture2D _goldChestTex;
        private static Texture2D _ironChestTex;

        // 8-bit pixel art color palette (vibrant, classic style)
        private static readonly Color32 WoodDark = new(52, 28, 12, 255);
        private static readonly Color32 WoodMid = new(88, 50, 20, 255);
        private static readonly Color32 WoodLight = new(120, 75, 35, 255);
        private static readonly Color32 GoldBright = new(255, 220, 60, 255);
        private static readonly Color32 GoldDark = new(200, 160, 40, 255);
        private static readonly Color32 Iron = new(70, 75, 85, 255);
        private static readonly Color32 IronHighlight = new(120, 130, 145, 255);
        private static readonly Color32 GemRed = new(220, 40, 40, 255);
        private static readonly Color32 GemGlow = new(255, 100, 100, 255);
        private static readonly Color32 IronDark = new(50, 55, 65, 255);
        private static readonly Color32 GoldShiny = new(255, 240, 100, 255);

        public static Texture2D GetStandardChestTexture() => _standardChestTex ??= BuildStandardChestTexture();
        public static Texture2D GetGoldChestTexture() => _goldChestTex ??= BuildGoldChestTexture();
        public static Texture2D GetIronChestTexture() => _ironChestTex ??= BuildIronChestTexture();

        public static void ClearCache()
        {
            DestroyTexture(ref _standardChestTex);
            DestroyTexture(ref _goldChestTex);
            DestroyTexture(ref _ironChestTex);
        }

        private static void DestroyTexture(ref Texture2D tex)
        {
            if (tex) { UnityEngine.Object.DestroyImmediate(tex); tex = null; }
        }

        /// <summary>
        /// Build standard treasure chest texture with ruby gem.
        /// </summary>
        private static Texture2D BuildStandardChestTexture()
        {
            const int w = 32;
            const int h = 32;
            var canvas = new PixelCanvas(w, h);
            canvas.DefaultColor = WoodMid;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color32 pixel = WoodMid;

                    // === LID SECTION (top 40%) ===
                    bool isLid = y < h * 0.4f;

                    if (isLid)
                    {
                        // Lid wood with grain
                        int grainPattern = ((x / 3) + (y / 2)) % 3;
                        pixel = grainPattern switch
                        {
                            0 => WoodDark,
                            1 => WoodMid,
                            _ => WoodLight
                        };

                        // Lid gold trim (border)
                        if (y < 4 || x < 3 || x >= w - 3)
                        {
                            pixel = ((x + y) % 2 == 0) ? GoldBright : GoldDark;
                        }

                        // Decorative gold pattern on lid
                        if (y >= 4 && y < 8 && x >= 8 && x < w - 8)
                        {
                            if ((x + y) % 4 == 0)
                                pixel = GoldBright;
                        }

                        // Center ruby gem on lid
                        if (y >= 5 && y <= 9 && x >= w / 2 - 3 && x <= w / 2 + 3)
                        {
                            int gemDist = Mathf.Abs(x - w / 2) + Mathf.Abs(y - 7);
                            if (gemDist <= 2)
                                pixel = GemRed;
                            else if (gemDist == 3)
                                pixel = GemGlow;
                        }
                    }
                    // === BODY SECTION (bottom 60%) ===
                    else
                    {
                        // Body wood grain (horizontal planks)
                        int plankRow = (y - h / 2) / 5;
                        int grainPattern = ((x / 4) + plankRow) % 3;
                        pixel = grainPattern switch
                        {
                            0 => WoodDark,
                            1 => WoodMid,
                            _ => WoodLight
                        };

                        // Metal bands (horizontal reinforcement)
                        int bodyY = y - h / 2;
                        if (bodyY < 3 || bodyY >= (h / 2) - 3)
                        {
                            // Band with rivets
                            pixel = ((x % 8) < 2) ? IronHighlight : Iron;
                        }

                        // Vertical metal straps
                        if ((x < 4 || x >= w - 4 || x >= w / 2 - 2 && x <= w / 2 + 1))
                        {
                            if (y >= h * 0.4f + 2 && y < h - 3)
                            {
                                pixel = ((y % 6) < 2) ? IronHighlight : Iron;
                            }
                        }

                        // Ornate lock plate (center)
                        int lockCenterY = h / 2 + 4;
                        int lockCenterX = w / 2;
                        int lockDist = Mathf.Max(Mathf.Abs(x - lockCenterX), Mathf.Abs(y - lockCenterY));

                        if (lockDist <= 4 && y >= h * 0.45f)
                        {
                            // Lock plate background
                            pixel = GoldDark;

                            // Lock keyhole
                            int keyholeDist = Mathf.Max(Mathf.Abs(x - lockCenterX), Mathf.Abs(y - lockCenterY));
                            if (keyholeDist <= 2)
                            {
                                pixel = Iron;
                            }
                            // Keyhole opening (cross shape)
                            if ((x == lockCenterX && Mathf.Abs(y - lockCenterY) <= 1) ||
                                (y == lockCenterY && Mathf.Abs(x - lockCenterX) <= 1))
                            {
                                pixel = new Color32(30, 30, 35, 255); // Dark keyhole
                            }

                            // Gold studs around lock
                            if (lockDist == 4 && (x + y) % 2 == 0)
                            {
                                pixel = GoldBright;
                            }
                        }
                    }

                    // Edge highlighting (3D effect)
                    if (!isLid && x == 3)
                        pixel = LightenColor(pixel, 30);
                    if (!isLid && x == w - 4)
                        pixel = DarkenColor(pixel, 30);

                    canvas.SetPixel(x, y, pixel);
                }
            }

            return canvas.ToTexture();
        }

        /// <summary>
        /// Build golden chest texture (rare/premium variant).
        /// </summary>
        private static Texture2D BuildGoldChestTexture()
        {
            const int w = 32;
            const int h = 32;
            var canvas = new PixelCanvas(w, h);
            canvas.DefaultColor = GoldMid;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color32 pixel = GoldMid;

                    bool isLid = y < h * 0.4f;

                    if (isLid)
                    {
                        // Gold grain pattern
                        int grainPattern = ((x / 3) + (y / 2)) % 3;
                        pixel = grainPattern switch
                        {
                            0 => GoldDark,
                            1 => GoldMid,
                            _ => GoldShiny
                        };

                        // Ornate trim
                        if (y < 4 || x < 3 || x >= w - 3)
                        {
                            pixel = ((x + y) % 2 == 0) ? GemRed : GoldBright;
                        }

                        // Center blue gem (sapphire)
                        if (y >= 5 && y <= 9 && x >= w / 2 - 3 && x <= w / 2 + 3)
                        {
                            int gemDist = Mathf.Abs(x - w / 2) + Mathf.Abs(y - 7);
                            if (gemDist <= 2)
                                pixel = new Color32(40, 40, 220, 255); // Sapphire
                            else if (gemDist == 3)
                                pixel = new Color32(100, 100, 255, 255); // Sapphire glow
                        }
                    }
                    else
                    {
                        // Gold body with decorative patterns
                        int pattern = (x * 3 + y * 5) % 4;
                        pixel = pattern switch
                        {
                            0 => GoldDark,
                            1 => GoldMid,
                            2 => GoldShiny,
                            _ => GoldBright
                        };

                        // Lock plate (ornate)
                        int lockCenterY = h / 2 + 4;
                        int lockCenterX = w / 2;
                        int lockDist = Mathf.Max(Mathf.Abs(x - lockCenterX), Mathf.Abs(y - lockCenterY));

                        if (lockDist <= 4 && y >= h * 0.45f)
                        {
                            pixel = GemRed;
                            if (x == lockCenterX && y == lockCenterY)
                                pixel = GoldBright;
                        }
                    }

                    canvas.SetPixel(x, y, pixel);
                }
            }

            return canvas.ToTexture();
        }

        private static readonly Color32 GoldMid = new(200, 160, 40, 255);

        /// <summary>
        /// Build iron chest texture (common/basic variant).
        /// </summary>
        private static Texture2D BuildIronChestTexture()
        {
            const int w = 32;
            const int h = 32;
            var canvas = new PixelCanvas(w, h);
            canvas.DefaultColor = Iron;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color32 pixel = Iron;

                    bool isLid = y < h * 0.4f;

                    if (isLid)
                    {
                        // Iron plating with rivets
                        int rivetPattern = (x % 8 == 0 || x % 8 == 7) && (y % 8 == 0 || y % 8 == 7) ? 1 : 0;
                        pixel = rivetPattern == 1 ? IronHighlight : IronDark;

                        // Reinforced edges
                        if (y < 4 || x < 3 || x >= w - 3)
                        {
                            pixel = IronHighlight;
                        }
                    }
                    else
                    {
                        // Iron body with plate pattern
                        int plateX = (x / 8) % 2;
                        int plateY = y / 8;
                        bool isMortar = (x % 8 < 1) || (y % 8 < 1);

                        if (isMortar)
                        {
                            pixel = IronDark;
                        }
                        else
                        {
                            int pattern = ((plateX + plateY) % 3);
                            pixel = pattern switch
                            {
                                0 => IronDark,
                                1 => Iron,
                                _ => IronHighlight
                            };
                        }

                        // Lock plate (simple)
                        int lockCenterY = h / 2 + 4;
                        int lockCenterX = w / 2;
                        int lockDist = Mathf.Max(Mathf.Abs(x - lockCenterX), Mathf.Abs(y - lockCenterY));

                        if (lockDist <= 3 && y >= h * 0.45f)
                        {
                            pixel = IronDark;
                            if (x == lockCenterX && y == lockCenterY)
                                pixel = new Color32(20, 20, 25, 255); // Keyhole
                        }
                    }

                    canvas.SetPixel(x, y, pixel);
                }
            }

            return canvas.ToTexture();
        }

        private static Color32 LightenColor(Color32 c, int amount)
        {
            return new Color32(
                (byte)Mathf.Min(255, c.r + amount),
                (byte)Mathf.Min(255, c.g + amount),
                (byte)Mathf.Min(255, c.b + amount),
                c.a
            );
        }

        private static Color32 DarkenColor(Color32 c, int amount)
        {
            return new Color32(
                (byte)Mathf.Max(0, c.r - amount),
                (byte)Mathf.Max(0, c.g - amount),
                (byte)Mathf.Max(0, c.b - amount),
                c.a
            );
        }

        /// <summary>
        /// Get chest texture by chest type.
        /// </summary>
        public static Texture2D GetChestTexture(ChestType type)
        {
            return type switch
            {
                ChestType.Gold => GetGoldChestTexture(),
                ChestType.Iron => GetIronChestTexture(),
                _ => GetStandardChestTexture()
            };
        }
    }

    /// <summary>
    /// Chest type enumeration for texture variants.
    /// </summary>
    public enum ChestType { Standard, Gold, Iron }
}
