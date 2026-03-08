// Copyright (C) 2026 Ocxyde
//
// This file is part of Code.Lavos.
//
// Code.Lavos is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Code.Lavos is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Code.Lavos.  If not, see <https://www.gnu.org/licenses/>.
// Lav8s_PixelArt8Bit.cs
// 8-bit pixel-art texture factory — scene MazeLav8s_v1-0_0_0
// Unity 6 compatible - UTF-8 encoding - Unix line endings
// Locale: en_US
//
// Palette: 8-bit dungeon (16 colors max per surface, Point-filtered)
// Surfaces: Wall, Floor, Ceiling, Door (wood/iron), Chest, Stone accent
//
// Usage (runtime):
//   Material mat = Lav8s_PixelArt8Bit.CreateWallMaterial();
//   renderer.material = Lav8s_PixelArt8Bit.CreateDoorMaterial();
//
// All textures are 16x16 px, FilterMode.Point, no mip-maps.
// Cache is cleared automatically when the scene unloads.
//
// PLUG-IN-OUT: static utility — no MonoBehaviour required.
// LOCATION: Assets/Scripts/Ressources/

using UnityEngine;
using UnityEngine.Rendering;

namespace Code.Lavos.Core
{
    /// <summary>
    /// 8-bit pixel-art texture and material factory for MazeLav8s v1.
    /// Generates 16×16 textures with a strict dungeon palette.
    /// Point-filtered (no anti-aliasing) for authentic 8-bit look.
    /// </summary>
    public static class Lav8s_PixelArt8Bit
    {
        // ── Dungeon palette (NES-style 8-bit colors) ────────────────────────
        //  Row 0 — Stone / Wall
        private static readonly Color32 C_STONE_BLACK   = new(10,   8,   8, 255);
        private static readonly Color32 C_STONE_DARK    = new(38,  32,  28, 255);
        private static readonly Color32 C_STONE_MID     = new(62,  52,  45, 255);
        private static readonly Color32 C_STONE_LIGHT   = new(88,  74,  62, 255);
        private static readonly Color32 C_STONE_MORTAR  = new(22,  20,  18, 255);

        //  Row 1 — Floor
        private static readonly Color32 C_FLOOR_BLACK   = new(12,  10,   8, 255);
        private static readonly Color32 C_FLOOR_DARK    = new(30,  24,  18, 255);
        private static readonly Color32 C_FLOOR_MID     = new(50,  40,  30, 255);
        private static readonly Color32 C_FLOOR_CRACK   = new(18,  14,  10, 255);

        //  Row 2 — Wood / Door
        private static readonly Color32 C_WOOD_DARK     = new(55,  32,  10, 255);
        private static readonly Color32 C_WOOD_MID      = new(88,  52,  18, 255);
        private static readonly Color32 C_WOOD_LIGHT    = new(118, 72,  28, 255);
        private static readonly Color32 C_IRON_DARK     = new(35,  35,  38, 255);
        private static readonly Color32 C_IRON_MID      = new(65,  65,  70, 255);
        private static readonly Color32 C_IRON_LIGHT    = new(95,  95, 100, 255);

        //  Row 3 — Chest / Gold
        private static readonly Color32 C_GOLD_DARK     = new(130,  90,  10, 255);
        private static readonly Color32 C_GOLD_MID      = new(190, 145,  20, 255);
        private static readonly Color32 C_GOLD_LIGHT    = new(240, 200,  50, 255);
        private static readonly Color32 C_CHEST_BODY    = new(80,   50,  18, 255);

        //  Row 4 — FX
        private static readonly Color32 C_TORCH_ORANGE  = new(255, 130,  20, 255);
        private static readonly Color32 C_TORCH_YELLOW  = new(255, 220,  60, 255);
        private static readonly Color32 C_MAGIC_BLUE    = new( 40,  80, 200, 255);
        private static readonly Color32 C_ACCENT_RED    = new(160,  20,  10, 255);

        // ── Texture cache ────────────────────────────────────────────────────
        private static Texture2D _wall, _wallCastle, _floor, _ceiling;
        private static Texture2D _doorWood, _doorIron, _chest, _stoneTrim;

        // ── Public material factories ────────────────────────────────────────

        public static Material CreateWallMaterial()
            => PixelMat(GetWallTex());

        public static Material CreateCastleWallMaterial()
            => PixelMat(GetCastleWallTex());

        public static Material CreateFloorMaterial()
            => PixelMat(GetFloorTex());

        public static Material CreateCeilingMaterial()
            => PixelMat(GetCeilingTex());

        public static Material CreateDoorMaterial()
            => PixelMat(GetDoorWoodTex());

        public static Material CreateDoubleDoorMaterial()
            => PixelMat(GetDoorIronTex());

        public static Material CreateChestMaterial()
            => PixelMat(GetChestTex());

        public static Material CreateStoneTrimMaterial()
            => PixelMat(GetStoneTrimTex());

        // ── Apply to renderer ────────────────────────────────────────────────

        public static void ApplyWall(Renderer r)     => r.material = CreateWallMaterial();
        public static void ApplyCastleWall(Renderer r)=> r.material = CreateCastleWallMaterial();
        public static void ApplyFloor(Renderer r)    => r.material = CreateFloorMaterial();
        public static void ApplyCeiling(Renderer r)  => r.material = CreateCeilingMaterial();
        public static void ApplyDoor(Renderer r)     => r.material = CreateDoorMaterial();
        public static void ApplyDoubleDoor(Renderer r)=> r.material = CreateDoubleDoorMaterial();
        public static void ApplyChest(Renderer r)    => r.material = CreateChestMaterial();

        // ── Cache management ─────────────────────────────────────────────────

        public static void ClearCache()
        {
            DestroyTex(ref _wall);    DestroyTex(ref _wallCastle);
            DestroyTex(ref _floor);   DestroyTex(ref _ceiling);
            DestroyTex(ref _doorWood);DestroyTex(ref _doorIron);
            DestroyTex(ref _chest);   DestroyTex(ref _stoneTrim);
        }

        // ── Texture getters (lazy) ───────────────────────────────────────────

        public static Texture2D GetWallTex()       => _wall       ??= BuildWall();
        public static Texture2D GetCastleWallTex() => _wallCastle ??= BuildCastleWall();
        public static Texture2D GetFloorTex()      => _floor      ??= BuildFloor();
        public static Texture2D GetCeilingTex()    => _ceiling    ??= BuildCeiling();
        public static Texture2D GetDoorWoodTex()   => _doorWood   ??= BuildDoorWood();
        public static Texture2D GetDoorIronTex()   => _doorIron   ??= BuildDoorIron();
        public static Texture2D GetChestTex()      => _chest      ??= BuildChest();
        public static Texture2D GetStoneTrimTex()  => _stoneTrim  ??= BuildStoneTrim();

        // ────────────────────────────────────────────────────────────────────
        //  PRIVATE — texture builders
        // ────────────────────────────────────────────────────────────────────

        // 16×16 dungeon stone wall — horizontal brick courses
        private static Texture2D BuildWall()
        {
            var t = Tex16();
            // Fill with mid-tone base
            FillAll(t, C_STONE_MID);
            // Mortar lines (rows 0, 4, 8, 12)
            DrawRow(t, 0,  C_STONE_MORTAR);
            DrawRow(t, 4,  C_STONE_MORTAR);
            DrawRow(t, 8,  C_STONE_MORTAR);
            DrawRow(t, 12, C_STONE_MORTAR);
            // Offset mortar columns per row
            DrawPixel(t, 0, 2, C_STONE_MORTAR); DrawPixel(t, 0, 10, C_STONE_MORTAR);
            DrawPixel(t, 1, 2, C_STONE_MORTAR); DrawPixel(t, 1, 10, C_STONE_MORTAR);
            DrawPixel(t, 2, 2, C_STONE_MORTAR); DrawPixel(t, 2, 10, C_STONE_MORTAR);
            DrawPixel(t, 5, 6, C_STONE_MORTAR); DrawPixel(t, 5, 14, C_STONE_MORTAR);
            DrawPixel(t, 6, 6, C_STONE_MORTAR); DrawPixel(t, 6, 14, C_STONE_MORTAR);
            DrawPixel(t, 7, 6, C_STONE_MORTAR); DrawPixel(t, 7, 14, C_STONE_MORTAR);
            DrawPixel(t, 9, 2, C_STONE_MORTAR); DrawPixel(t, 9, 10, C_STONE_MORTAR);
            DrawPixel(t, 10,2, C_STONE_MORTAR); DrawPixel(t, 10,10, C_STONE_MORTAR);
            DrawPixel(t, 13,6, C_STONE_MORTAR); DrawPixel(t, 13,14, C_STONE_MORTAR);
            DrawPixel(t, 14,6, C_STONE_MORTAR); DrawPixel(t, 14,14, C_STONE_MORTAR);
            // Dark corners on each brick
            DrawPixel(t, 1,  1, C_STONE_DARK);  DrawPixel(t, 1,  9, C_STONE_DARK);
            DrawPixel(t, 3,  5, C_STONE_DARK);  DrawPixel(t, 3, 13, C_STONE_DARK);
            DrawPixel(t, 6,  1, C_STONE_DARK);  DrawPixel(t, 6,  9, C_STONE_DARK);
            DrawPixel(t, 11, 5, C_STONE_DARK);  DrawPixel(t, 11,13, C_STONE_DARK);
            // Highlight pixels (top-left of each brick)
            DrawPixel(t, 1,  3, C_STONE_LIGHT); DrawPixel(t, 1, 11, C_STONE_LIGHT);
            DrawPixel(t, 6,  7, C_STONE_LIGHT); DrawPixel(t, 6, 15, C_STONE_LIGHT);
            t.Apply();
            return t;
        }

        // 16×16 castle wall — large square blocks, arched detail
        private static Texture2D BuildCastleWall()
        {
            var t = Tex16();
            FillAll(t, C_STONE_MID);
            // Large block grid (8×8 blocks, mortar at 0,8)
            DrawRow(t, 0,  C_STONE_BLACK);
            DrawRow(t, 8,  C_STONE_BLACK);
            DrawCol(t, 0,  C_STONE_BLACK);
            DrawCol(t, 8,  C_STONE_BLACK);
            // Stone texture variation
            DrawPixel(t, 2, 2, C_STONE_LIGHT); DrawPixel(t, 2, 10, C_STONE_LIGHT);
            DrawPixel(t, 4, 4, C_STONE_DARK);  DrawPixel(t, 4, 12, C_STONE_DARK);
            DrawPixel(t, 6, 3, C_STONE_LIGHT); DrawPixel(t, 6, 11, C_STONE_LIGHT);
            DrawPixel(t, 10,5, C_STONE_DARK);  DrawPixel(t, 10, 2, C_STONE_DARK);
            DrawPixel(t, 12,7, C_STONE_LIGHT); DrawPixel(t, 12,14, C_STONE_LIGHT);
            DrawPixel(t, 14,3, C_STONE_DARK);  DrawPixel(t, 14,11, C_STONE_DARK);
            // Bottom edge shadow
            DrawRow(t, 1, C_STONE_DARK);
            DrawRow(t, 9, C_STONE_DARK);
            t.Apply();
            return t;
        }

        // 16×16 stone floor — cracked tiles
        private static Texture2D BuildFloor()
        {
            var t = Tex16();
            FillAll(t, C_FLOOR_MID);
            // Tile grid (8×8 tiles)
            DrawRow(t, 0,  C_FLOOR_BLACK);
            DrawRow(t, 8,  C_FLOOR_BLACK);
            DrawCol(t, 0,  C_FLOOR_BLACK);
            DrawCol(t, 8,  C_FLOOR_BLACK);
            // Dark edge on each tile
            DrawRow(t, 1, C_FLOOR_DARK); DrawRow(t, 9, C_FLOOR_DARK);
            DrawCol(t, 1, C_FLOOR_DARK); DrawCol(t, 9, C_FLOOR_DARK);
            // Cracks
            DrawPixel(t, 3, 4, C_FLOOR_CRACK); DrawPixel(t, 4, 5, C_FLOOR_CRACK);
            DrawPixel(t, 5, 4, C_FLOOR_CRACK); DrawPixel(t, 3, 3, C_FLOOR_CRACK);
            DrawPixel(t,11,12, C_FLOOR_CRACK); DrawPixel(t,12,11, C_FLOOR_CRACK);
            DrawPixel(t,10,13, C_FLOOR_CRACK); DrawPixel(t,13,12, C_FLOOR_CRACK);
            t.Apply();
            return t;
        }

        // 16×16 pitch black ceiling with subtle texture
        private static Texture2D BuildCeiling()
        {
            var t = Tex16();
            FillAll(t, new Color32(8, 7, 6, 255));
            DrawRow(t, 0,  C_STONE_BLACK); DrawRow(t, 8,  C_STONE_BLACK);
            DrawCol(t, 0,  C_STONE_BLACK); DrawCol(t, 8,  C_STONE_BLACK);
            DrawPixel(t, 3, 3, new Color32(14,12,10,255));
            DrawPixel(t, 7, 7, new Color32(14,12,10,255));
            DrawPixel(t,11, 5, new Color32(14,12,10,255));
            DrawPixel(t, 5,11, new Color32(14,12,10,255));
            t.Apply();
            return t;
        }

        // 16×16 wooden door — plank + iron bolt pattern
        private static Texture2D BuildDoorWood()
        {
            var t = Tex16();
            // 4 vertical planks (0-3, 4-7, 8-11, 12-15)
            for (int y = 0; y < 16; y++)
            {
                DrawPixel(t, y, 0,  C_WOOD_DARK);
                DrawPixel(t, y, 3,  C_WOOD_DARK);
                DrawPixel(t, y, 4,  C_WOOD_DARK);
                DrawPixel(t, y, 7,  C_WOOD_DARK);
                DrawPixel(t, y, 8,  C_WOOD_DARK);
                DrawPixel(t, y, 11, C_WOOD_DARK);
                DrawPixel(t, y, 12, C_WOOD_DARK);
                DrawPixel(t, y, 15, C_WOOD_DARK);
                DrawPixel(t, y, 1,  C_WOOD_MID);
                DrawPixel(t, y, 5,  C_WOOD_MID);
                DrawPixel(t, y, 9,  C_WOOD_MID);
                DrawPixel(t, y, 13, C_WOOD_MID);
                DrawPixel(t, y, 2,  C_WOOD_LIGHT);
                DrawPixel(t, y, 6,  C_WOOD_LIGHT);
                DrawPixel(t, y, 10, C_WOOD_LIGHT);
                DrawPixel(t, y, 14, C_WOOD_LIGHT);
            }
            // Horizontal cross-brace
            DrawRow(t, 4,  C_IRON_DARK); DrawRow(t, 5,  C_IRON_MID);
            DrawRow(t, 10, C_IRON_DARK); DrawRow(t, 11, C_IRON_MID);
            // Iron bolts (corners of braces)
            DrawPixel(t, 4, 2,  C_IRON_LIGHT); DrawPixel(t, 5, 2,  C_IRON_LIGHT);
            DrawPixel(t, 4, 13, C_IRON_LIGHT); DrawPixel(t, 5, 13, C_IRON_LIGHT);
            DrawPixel(t,10, 2,  C_IRON_LIGHT); DrawPixel(t,11, 2,  C_IRON_LIGHT);
            DrawPixel(t,10, 13, C_IRON_LIGHT); DrawPixel(t,11, 13, C_IRON_LIGHT);
            t.Apply();
            return t;
        }

        // 16×16 castle double-door — heavy iron/stone (manor style)
        private static Texture2D BuildDoorIron()
        {
            var t = Tex16();
            FillAll(t, C_IRON_DARK);
            // Iron panel grid
            DrawRow(t, 0,  C_STONE_BLACK); DrawRow(t, 15, C_STONE_BLACK);
            DrawCol(t, 0,  C_STONE_BLACK); DrawCol(t, 15, C_STONE_BLACK);
            DrawRow(t, 7,  C_IRON_MID);   DrawRow(t, 8, C_IRON_MID);
            DrawCol(t, 7,  C_IRON_MID);   DrawCol(t, 8, C_IRON_MID);
            // Decorative rivets
            DrawPixel(t, 2, 2, C_IRON_LIGHT);  DrawPixel(t, 2, 13, C_IRON_LIGHT);
            DrawPixel(t,13, 2, C_IRON_LIGHT);  DrawPixel(t,13, 13, C_IRON_LIGHT);
            DrawPixel(t, 2, 5, C_IRON_MID);    DrawPixel(t, 2, 10, C_IRON_MID);
            DrawPixel(t,13, 5, C_IRON_MID);    DrawPixel(t,13, 10, C_IRON_MID);
            // Gold handle / ring
            DrawPixel(t, 7, 5, C_GOLD_MID);    DrawPixel(t, 8, 5, C_GOLD_MID);
            DrawPixel(t, 7, 6, C_GOLD_LIGHT);  DrawPixel(t, 8, 6, C_GOLD_LIGHT);
            DrawPixel(t, 7, 9, C_GOLD_MID);    DrawPixel(t, 8, 9, C_GOLD_MID);
            DrawPixel(t, 7,10, C_GOLD_LIGHT);  DrawPixel(t, 8,10, C_GOLD_LIGHT);
            // Arch decoration at top
            DrawPixel(t, 1, 7, C_GOLD_DARK);   DrawPixel(t, 1, 8, C_GOLD_DARK);
            DrawPixel(t,14, 7, C_GOLD_DARK);   DrawPixel(t,14, 8, C_GOLD_DARK);
            t.Apply();
            return t;
        }

        // 16×16 treasure chest — wood body + gold trim + lock
        private static Texture2D BuildChest()
        {
            var t = Tex16();
            FillAll(t, C_CHEST_BODY);
            // Outline
            DrawRow(t, 0,  C_STONE_BLACK); DrawRow(t,15, C_STONE_BLACK);
            DrawRow(t, 7,  C_STONE_BLACK); DrawRow(t, 8, C_STONE_BLACK);
            DrawCol(t, 0,  C_STONE_BLACK); DrawCol(t,15, C_STONE_BLACK);
            // Gold trim on edges
            DrawRow(t, 1,  C_GOLD_DARK);   DrawRow(t,14, C_GOLD_DARK);
            DrawCol(t, 1,  C_GOLD_DARK);   DrawCol(t,14, C_GOLD_DARK);
            // Gold hinge at top
            DrawPixel(t, 9, 4, C_GOLD_MID);  DrawPixel(t, 9, 11, C_GOLD_MID);
            DrawPixel(t,10, 4, C_GOLD_LIGHT);DrawPixel(t,10, 11, C_GOLD_LIGHT);
            // Lock
            DrawPixel(t, 5, 7, C_GOLD_MID);  DrawPixel(t, 5, 8, C_GOLD_MID);
            DrawPixel(t, 6, 7, C_GOLD_LIGHT);DrawPixel(t, 6, 8, C_GOLD_LIGHT);
            DrawPixel(t, 7, 7, C_GOLD_DARK); DrawPixel(t, 7, 8, C_GOLD_DARK);
            // Wood grain
            DrawPixel(t, 3, 5, C_WOOD_DARK); DrawPixel(t,4, 10, C_WOOD_DARK);
            DrawPixel(t,11, 3, C_WOOD_LIGHT);DrawPixel(t,12,  9, C_WOOD_LIGHT);
            t.Apply();
            return t;
        }

        // 16×16 stone trim / archway accents
        private static Texture2D BuildStoneTrim()
        {
            var t = Tex16();
            FillAll(t, C_STONE_DARK);
            DrawRow(t, 0, C_STONE_BLACK);   DrawRow(t,15, C_STONE_BLACK);
            DrawCol(t, 0, C_STONE_BLACK);   DrawCol(t,15, C_STONE_BLACK);
            DrawRow(t, 1, C_STONE_MID);     DrawRow(t,14, C_STONE_MID);
            DrawCol(t, 1, C_STONE_MID);     DrawCol(t,14, C_STONE_MID);
            DrawPixel(t, 7, 7, C_STONE_LIGHT);  DrawPixel(t, 7, 8, C_STONE_LIGHT);
            DrawPixel(t, 8, 7, C_STONE_LIGHT);  DrawPixel(t, 8, 8, C_STONE_LIGHT);
            t.Apply();
            return t;
        }

        // ── Pixel helpers ────────────────────────────────────────────────────

        private static Texture2D Tex16()
        {
            var t = new Texture2D(16, 16, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode   = TextureWrapMode.Repeat,
                anisoLevel = 0
            };
            return t;
        }

        private static Material PixelMat(Texture2D tex)
        {
            // Use URP/Lit shader when available, fallback to Standard
            Shader sh = Shader.Find("Universal Render Pipeline/Lit")
                     ?? Shader.Find("Standard");
            var mat = new Material(sh) { mainTexture = tex };
            mat.SetFloat("_Smoothness", 0f);
            mat.SetFloat("_Metallic",   0f);
            return mat;
        }

        private static void FillAll(Texture2D t, Color32 c)
        {
            var pixels = new Color32[16 * 16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = c;
            t.SetPixels32(pixels);
        }

        private static void DrawRow(Texture2D t, int row, Color32 c)
        {
            for (int x = 0; x < 16; x++) t.SetPixel(x, row, c);
        }

        private static void DrawCol(Texture2D t, int col, Color32 c)
        {
            for (int y = 0; y < 16; y++) t.SetPixel(col, y, c);
        }

        private static void DrawPixel(Texture2D t, int row, int col, Color32 c)
            => t.SetPixel(col, row, c);

        private static void DestroyTex(ref Texture2D t)
        {
            if (t != null) { Object.DestroyImmediate(t); t = null; }
        }
    }
}
