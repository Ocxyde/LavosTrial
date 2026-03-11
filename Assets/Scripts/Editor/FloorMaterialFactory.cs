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
// FloorMaterialFactory.cs
// Generates and saves floor materials as assets
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-AND-OUT:
// - Generates floor textures (stone, wood, tile, etc.)
// - Saves materials to Assets/Materials/Floor/
// - Reusable across scenes - independent plugin module
// - EDITOR ONLY: Uses AssetDatabase (not available in builds)
//
// Location: Assets/Scripts/Editor/

using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Code.Lavos.Core
{
    /// <summary>
    /// FloorMaterialFactory - Generates and saves floor materials.
    /// Supports multiple floor types: Stone, Wood, Tile, etc.
    /// Plug-in module for Core system.
    /// EDITOR ONLY: Uses AssetDatabase for material generation.
    /// </summary>
    public static class FloorMaterialFactory
    {
        #region Floor Types

        public enum FloorType
        {
            Stone,      // Gray stone tiles
            Wood,       // Brown wood planks
            Tile,       // Ceramic tiles
            Brick,      // Red brick
            Marble      // Fancy marble
        }

        #endregion

        #region Configuration

        private const string MATERIALS_FOLDER = "Assets/Materials/Floor";
        private const int TEXTURE_SIZE = 32;

        #endregion

        #region Public API

        /// <summary>
        /// Get or create floor material (Editor only).
        /// </summary>
        #if UNITY_EDITOR
        public static Material GetFloorMaterial(FloorType type = FloorType.Stone)
        {
            string materialPath = $"{MATERIALS_FOLDER}/{type}_Floor.mat";

            // Try to load existing material
            Material existingMat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (existingMat != null)
            {
                Debug.Log($"[FloorFactory] Loaded existing: {type}_Floor.mat");
                return existingMat;
            }

            // Create new material
            Debug.Log($"[FloorFactory] Creating new: {type}_Floor.mat");
            return CreateAndSaveFloorMaterial(type);
        }

        /// <summary>
        /// Generate all floor material variants.
        /// </summary>
        public static void GenerateAllFloorMaterials()
        {
            EnsureMaterialsFolder();

            foreach (FloorType type in System.Enum.GetValues(typeof(FloorType)))
            {
                CreateAndSaveFloorMaterial(type);
            }

            Debug.Log("[FloorFactory]  All floor materials generated!");
        }

        /// <summary>
        /// Create and save floor material (internal, Editor only).
        /// </summary>
        private static Material CreateAndSaveFloorMaterial(FloorType type)
        {
            EnsureMaterialsFolder();

            // Generate texture using PixelCanvas from DrawingManager (global namespace)
            Texture2D texture = GenerateFloorTexture(type);
            if (texture == null)
            {
                Debug.LogError($"[FloorFactory] Failed to generate texture for {type}");
                return null;
            }

            // Save and import texture first
            string texturePath = $"{MATERIALS_FOLDER}/{type}_Floor_Texture.png";
            SaveTexture(texture, texturePath);
            AssetDatabase.ImportAsset(texturePath);
            Texture2D importedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

            // Create material with URP Lit shader
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader == null)
            {
                Debug.LogError("[FloorFactory]  URP Lit shader not found!");
                urpShader = Shader.Find("Standard");
            }

            Material mat = new Material(urpShader);

            // Set texture using _BaseMap for URP (also set _MainTex for compatibility)
            mat.SetTexture("_BaseMap", importedTexture);
            mat.SetTexture("_MainTex", importedTexture);
            mat.SetTextureScale("_BaseMap", new Vector2(1f, 1f));
            mat.SetTextureScale("_MainTex", new Vector2(1f, 1f));

            // URP uses _Smoothness instead of _Glossiness (they're inverses)
            mat.SetFloat("_Smoothness", 0.2f);
            mat.SetFloat("_Metallic", 0f);

            // Set base color to white
            mat.SetColor("_BaseColor", Color.white);
            mat.SetColor("_Color", Color.white);

            // Save material
            string materialPath = $"{MATERIALS_FOLDER}/{type}_Floor.mat";
            AssetDatabase.CreateAsset(mat, materialPath);
            AssetDatabase.SaveAssets();

            // Load and return the saved asset (not the runtime object)
            Material savedMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            Debug.Log($"[FloorFactory] Saved: {materialPath}");

            return savedMaterial;
        }

        private static void SaveTexture(Texture2D texture, string path)
        {
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
        }
        #endif

        #endregion

        #region Internal Methods

        private static Texture2D GenerateFloorTexture(FloorType type)
        {
            return type switch
            {
                FloorType.Stone => GenerateStoneFloor(),
                FloorType.Wood => GenerateWoodFloor(),
                FloorType.Tile => GenerateTileFloor(),
                FloorType.Brick => GenerateBrickFloor(),
                FloorType.Marble => GenerateMarbleFloor(),
                _ => GenerateStoneFloor()
            };
        }

        #endregion

        #region Texture Generators

        private static Texture2D GenerateStoneFloor()
        {
            var canvas = new PixelCanvas(TEXTURE_SIZE, TEXTURE_SIZE);

            // STONE COLORS (gray tones)
            var dark = new Color32(40, 40, 45, 255);   // Dark gray
            var mid = new Color32(60, 60, 65, 255);    // Medium gray
            var light = new Color32(80, 80, 85, 255);  // Light gray
            var mortar = new Color32(30, 30, 35, 255); // Mortar

            // Stone tile pattern (8x8 tiles)
            for (int y = 0; y < TEXTURE_SIZE; y++)
            {
                for (int x = 0; x < TEXTURE_SIZE; x++)
                {
                    // Mortar lines
                    if (x % 8 == 0 || y % 8 == 0)
                    {
                        canvas.SetPixel(x, y, mortar);
                        continue;
                    }

                    // Random stone variation
                    int tile = ((x / 8 + y / 8) * 17 + x * 3 + y * 5) % 5;
                    canvas.SetPixel(x, y, tile switch {
                        0 or 4 => dark,
                        1 or 3 => mid,
                        2 => light,
                        _ => mid
                    });
                }
            }

            return canvas.ToTexture();
        }

        private static Texture2D GenerateWoodFloor()
        {
            var canvas = new PixelCanvas(TEXTURE_SIZE, TEXTURE_SIZE);

            // WOOD COLORS (brown tones)
            var dark = new Color32(60, 40, 25, 255);
            var mid = new Color32(88, 50, 20, 255);
            var light = new Color32(120, 75, 35, 255);
            var grain = new Color32(40, 28, 15, 255);

            // Wood plank pattern (16 pixels wide)
            for (int y = 0; y < TEXTURE_SIZE; y++)
            {
                for (int x = 0; x < TEXTURE_SIZE; x++)
                {
                    // Plank lines
                    if (y % 16 == 0)
                    {
                        canvas.SetPixel(x, y, grain);
                        continue;
                    }

                    // Wood grain
                    int grainPattern = (x + y * 3) % 5;
                    canvas.SetPixel(x, y, grainPattern switch {
                        0 => dark,
                        1 or 3 => mid,
                        2 => light,
                        _ => mid
                    });
                }
            }

            return canvas.ToTexture();
        }

        private static Texture2D GenerateTileFloor()
        {
            var canvas = new PixelCanvas(TEXTURE_SIZE, TEXTURE_SIZE);

            // TILE COLORS
            var tileColor = new Color32(180, 170, 160, 255);
            var mortar = new Color32(100, 100, 100, 255);

            // Ceramic tile pattern
            for (int y = 0; y < TEXTURE_SIZE; y++)
            {
                for (int x = 0; x < TEXTURE_SIZE; x++)
                {
                    if (x % 8 == 0 || y % 8 == 0)
                    {
                        canvas.SetPixel(x, y, mortar);
                    }
                    else
                    {
                        canvas.SetPixel(x, y, tileColor);
                    }
                }
            }

            return canvas.ToTexture();
        }

        private static Texture2D GenerateBrickFloor()
        {
            var canvas = new PixelCanvas(TEXTURE_SIZE, TEXTURE_SIZE);

            // BRICK COLORS
            var brickRed = new Color32(120, 60, 40, 255);
            var brickDark = new Color32(90, 45, 30, 255);
            var mortar = new Color32(120, 120, 115, 255);

            // Brick pattern
            for (int y = 0; y < TEXTURE_SIZE; y++)
            {
                for (int x = 0; x < TEXTURE_SIZE; x++)
                {
                    int offset = (y / 8) % 2 * 8;
                    if ((x + offset) % 16 == 0 || y % 8 == 0)
                    {
                        canvas.SetPixel(x, y, mortar);
                    }
                    else
                    {
                        canvas.SetPixel(x, y, (x + y) % 3 == 0 ? brickDark : brickRed);
                    }
                }
            }

            return canvas.ToTexture();
        }

        private static Texture2D GenerateMarbleFloor()
        {
            var canvas = new PixelCanvas(TEXTURE_SIZE, TEXTURE_SIZE);

            // MARBLE COLORS
            var white = new Color32(240, 240, 245, 255);
            var gray = new Color32(180, 180, 185, 255);
            var vein = new Color32(100, 100, 110, 255);

            // Marble pattern with veins
            for (int y = 0; y < TEXTURE_SIZE; y++)
            {
                for (int x = 0; x < TEXTURE_SIZE; x++)
                {
                    // Vein pattern
                    float veinPattern = Mathf.Sin(x * 0.3f + y * 0.2f) * Mathf.Cos(y * 0.3f);
                    if (veinPattern > 0.8f)
                    {
                        canvas.SetPixel(x, y, vein);
                    }
                    else if (veinPattern > 0.5f)
                    {
                        canvas.SetPixel(x, y, gray);
                    }
                    else
                    {
                        canvas.SetPixel(x, y, white);
                    }
                }
            }

            return canvas.ToTexture();
        }

        #endregion

        #region Utilities

        private static void EnsureMaterialsFolder()
        {
            if (!Directory.Exists(MATERIALS_FOLDER))
            {
                Directory.CreateDirectory(MATERIALS_FOLDER);
                Debug.Log($"[FloorFactory] Created folder: {MATERIALS_FOLDER}");
            }
        }

        #endregion
    }
}
