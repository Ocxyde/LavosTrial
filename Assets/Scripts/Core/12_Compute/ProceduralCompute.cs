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
// ProceduralCompute.cs
// Centralized procedural generation system for ALL generative processes
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-AND-OUT ARCHITECTURE:
// - Single point for ALL procedural generation
// - Handles: Textures, Materials, Meshes, Patterns
// - Seed-based reproducible generation
// - Event-driven (requests via EventHandler)
// - Caches results for performance
//
// ️ IMPORTANT: Add ProceduralCompute to your scene manually.
// Do NOT rely on auto-creation (plug-in-out violation).
//
// USAGE:
//   ProceduralCompute.Instance.GenerateFloor(type, seed)
//   ProceduralCompute.Instance.GenerateWall(type, seed)
//   ProceduralCompute.Instance.GenerateMaterial(type, seed)
//
// Location: Assets/Scripts/Core/12_Compute/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// ProceduralCompute - Central procedural generation system.
    /// Handles all generative processes: textures, materials, meshes, patterns.
    /// 
    /// ️ Must be added to scene manually. Auto-creation is a fallback only.
    /// </summary>
    public class ProceduralCompute : MonoBehaviour
    {
        #region Singleton

        private static ProceduralCompute _instance;
        public static ProceduralCompute Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<ProceduralCompute>();
                    if (_instance == null)
                    {
                        // ️ FALLBACK ONLY: Should be added to scene manually
                        // This auto-creation is a plug-in-out violation
                        Debug.LogWarning("[ProceduralCompute] ️ Not found in scene - auto-creating (add manually!)");
                        var go = new GameObject("ProceduralCompute");
                        _instance = go.AddComponent<ProceduralCompute>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion
        
        #region Procedural Types
        
        public enum TextureType
        {
            Floor,
            Wall,
            Ceiling,
            Door,
            Torch
        }
        
        public enum MaterialType
        {
            Stone,
            Wood,
            Tile,
            Brick,
            Marble,
            Metal,
            Magic
        }
        
        public enum PatternType
        {
            Tiles,
            Planks,
            Bricks,
            Veins,
            Noise
        }
        
        #endregion
        
        #region Cache
        
        private Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();
        private Dictionary<string, Material> _materialCache = new Dictionary<string, Material>();
        private uint _currentSeed = 0;
        
        #endregion
        
        #region Properties
        
        public uint CurrentSeed => _currentSeed;
        
        #endregion
        
        #region Initialization
        
        public void Initialize(uint seed)
        {
            _currentSeed = seed;
            ClearCache();
            Debug.Log($"[ProceduralCompute] Initialized with seed: {seed}");
        }
        
        public void ClearCache()
        {
            _textureCache.Clear();
            _materialCache.Clear();
            Debug.Log("[ProceduralCompute] Cache cleared");
        }
        
        #endregion
        
        #region Public API - Texture Generation
        
        /// <summary>
        /// Generate procedural texture.
        /// </summary>
        public Texture2D GenerateTexture(TextureType type, MaterialType material, PatternType pattern, int size = 32)
        {
            string cacheKey = $"{type}_{material}_{pattern}_{size}_{_currentSeed}";
            
            // Check cache
            if (_textureCache.TryGetValue(cacheKey, out Texture2D cached))
            {
                Debug.Log($"[ProceduralCompute] Texture cache hit: {cacheKey}");
                return cached;
            }
            
            // Generate new
            Debug.Log($"[ProceduralCompute] Generating texture: {cacheKey}");
            Texture2D texture = GenerateTextureInternal(type, material, pattern, size);
            _textureCache[cacheKey] = texture;
            return texture;
        }
        
        /// <summary>
        /// Generate floor texture.
        /// </summary>
        public Texture2D GenerateFloor(MaterialType material, int size = 32)
        {
            return GenerateTexture(TextureType.Floor, material, PatternType.Tiles, size);
        }
        
        /// <summary>
        /// Generate wall texture.
        /// </summary>
        public Texture2D GenerateWall(MaterialType material, int size = 32)
        {
            return GenerateTexture(TextureType.Wall, material, PatternType.Bricks, size);
        }
        
        /// <summary>
        /// Generate ceiling texture.
        /// </summary>
        public Texture2D GenerateCeiling(MaterialType material, int size = 32)
        {
            return GenerateTexture(TextureType.Ceiling, material, PatternType.Noise, size);
        }
        
        #endregion
        
        #region Public API - Material Generation
        
        /// <summary>
        /// Generate procedural material.
        /// </summary>
        public Material GenerateMaterial(MaterialType type, TextureType textureType = TextureType.Floor)
        {
            string cacheKey = $"Mat_{type}_{textureType}_{_currentSeed}";
            
            // Check cache
            if (_materialCache.TryGetValue(cacheKey, out Material cached))
            {
                Debug.Log($"[ProceduralCompute] Material cache hit: {cacheKey}");
                return cached;
            }
            
            // Generate new
            Debug.Log($"[ProceduralCompute] Generating material: {cacheKey}");
            
            // Get texture
            Texture2D tex = GenerateTexture(textureType, type, GetPatternForType(type), 32);
            
            // Create material
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material mat = new Material(shader);
            mat.mainTexture = tex;
            mat.mainTextureScale = new Vector2(1f, 1f);
            mat.SetFloat("_Glossiness", GetGlossinessForType(type));
            mat.SetFloat("_Metallic", GetMetallicForType(type));
            
            _materialCache[cacheKey] = mat;
            return mat;
        }
        
        #endregion
        
        #region Internal Generation
        
        private Texture2D GenerateTextureInternal(TextureType type, MaterialType material, PatternType pattern, int size)
        {
            var canvas = new PixelCanvas(size, size);
            
            return (type, material) switch
            {
                (TextureType.Floor, MaterialType.Stone) => GenerateStoneFloor(canvas, size),
                (TextureType.Floor, MaterialType.Wood) => GenerateWoodFloor(canvas, size),
                (TextureType.Floor, MaterialType.Tile) => GenerateTileFloor(canvas, size),
                (TextureType.Floor, MaterialType.Brick) => GenerateBrickFloor(canvas, size),
                (TextureType.Floor, MaterialType.Marble) => GenerateMarbleFloor(canvas, size),
                (TextureType.Wall, MaterialType.Stone) => GenerateStoneWall(canvas, size),
                (TextureType.Wall, MaterialType.Brick) => GenerateBrickWall(canvas, size),
                (TextureType.Ceiling, _) => GenerateCeiling(canvas, size),
                _ => GenerateStoneFloor(canvas, size)
            };
        }
        
        #endregion
        
        #region Texture Generators
        
        private Texture2D GenerateStoneFloor(PixelCanvas canvas, int size)
        {
            var rng = new System.Random((int)_currentSeed);
            
            // STONE COLORS (gray tones)
            var dark = new Color32((byte)(40 + rng.Next(-5, 6)), (byte)(40 + rng.Next(-5, 6)), (byte)(45 + rng.Next(-5, 6)), 255);
            var mid = new Color32((byte)(60 + rng.Next(-5, 6)), (byte)(60 + rng.Next(-5, 6)), (byte)(65 + rng.Next(-5, 6)), 255);
            var light = new Color32((byte)(80 + rng.Next(-5, 6)), (byte)(80 + rng.Next(-5, 6)), (byte)(85 + rng.Next(-5, 6)), 255);
            var mortar = new Color32(30, 30, 35, 255);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (x % 8 == 0 || y % 8 == 0)
                    {
                        canvas.SetPixel(x, y, mortar);
                        continue;
                    }
                    
                    int tile = ((x / 8 + y / 8) * 17 + x * 3 + y * 5 + (int)_currentSeed) % 5;
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
        
        private Texture2D GenerateWoodFloor(PixelCanvas canvas, int size)
        {
            var rng = new System.Random((int)_currentSeed);
            
            // WOOD COLORS (brown tones)
            var dark = new Color32((byte)(60 + rng.Next(-5, 6)), (byte)(40 + rng.Next(-5, 6)), (byte)(25 + rng.Next(-5, 6)), 255);
            var mid = new Color32((byte)(88 + rng.Next(-5, 6)), (byte)(50 + rng.Next(-5, 6)), (byte)(20 + rng.Next(-5, 6)), 255);
            var light = new Color32((byte)(120 + rng.Next(-5, 6)), (byte)(75 + rng.Next(-5, 6)), (byte)(35 + rng.Next(-5, 6)), 255);
            var grain = new Color32(40, 28, 15, 255);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (y % 16 == 0)
                    {
                        canvas.SetPixel(x, y, grain);
                        continue;
                    }
                    
                    int grainPattern = (x + y * 3 + (int)_currentSeed) % 5;
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
        
        private Texture2D GenerateTileFloor(PixelCanvas canvas, int size)
        {
            var tileColor = new Color32(180, 170, 160, 255);
            var mortar = new Color32(100, 100, 100, 255);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
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
        
        private Texture2D GenerateBrickFloor(PixelCanvas canvas, int size)
        {
            var brickRed = new Color32(120, 60, 40, 255);
            var brickDark = new Color32(90, 45, 30, 255);
            var mortar = new Color32(120, 120, 115, 255);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int offset = (y / 8) % 2 * 8;
                    if ((x + offset) % 16 == 0 || y % 8 == 0)
                    {
                        canvas.SetPixel(x, y, mortar);
                    }
                    else
                    {
                        canvas.SetPixel(x, y, (x + y + (int)_currentSeed) % 3 == 0 ? brickDark : brickRed);
                    }
                }
            }
            
            return canvas.ToTexture();
        }
        
        private Texture2D GenerateMarbleFloor(PixelCanvas canvas, int size)
        {
            var white = new Color32(240, 240, 245, 255);
            var gray = new Color32(180, 180, 185, 255);
            var vein = new Color32(100, 100, 110, 255);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float veinPattern = Mathf.Sin(x * 0.3f + y * 0.2f + _currentSeed * 0.01f) * Mathf.Cos(y * 0.3f);
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
        
        private Texture2D GenerateStoneWall(PixelCanvas canvas, int size)
        {
            return GenerateStoneFloor(canvas, size);  // Similar pattern
        }
        
        private Texture2D GenerateBrickWall(PixelCanvas canvas, int size)
        {
            return GenerateBrickFloor(canvas, size);  // Similar pattern
        }
        
        private Texture2D GenerateCeiling(PixelCanvas canvas, int size)
        {
            var rng = new System.Random((int)_currentSeed);
            var dark = new Color32(14, 13, 12, 255);
            var mid = new Color32((byte)(22 + rng.Next(-2, 3)), (byte)(20 + rng.Next(-2, 3)), (byte)(18 + rng.Next(-2, 3)), 255);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int pattern = (x * 3 + y * 7 + x * y + (int)_currentSeed) % 7;
                    canvas.SetPixel(x, y, pattern switch {
                        0 or 1 => dark,
                        _ => mid
                    });
                }
            }
            
            return canvas.ToTexture();
        }
        
        #endregion
        
        #region Utilities
        
        private PatternType GetPatternForType(MaterialType type)
        {
            return type switch
            {
                MaterialType.Stone => PatternType.Tiles,
                MaterialType.Wood => PatternType.Planks,
                MaterialType.Tile => PatternType.Tiles,
                MaterialType.Brick => PatternType.Bricks,
                MaterialType.Marble => PatternType.Veins,
                _ => PatternType.Tiles
            };
        }
        
        private float GetGlossinessForType(MaterialType type)
        {
            return type switch
            {
                MaterialType.Marble => 0.6f,
                MaterialType.Tile => 0.5f,
                MaterialType.Metal => 0.7f,
                MaterialType.Wood => 0.2f,
                _ => 0.3f
            };
        }
        
        private float GetMetallicForType(MaterialType type)
        {
            return type switch
            {
                MaterialType.Metal => 0.8f,
                MaterialType.Marble => 0.1f,
                _ => 0f
            };
        }
        
        #endregion
        
        #region Event Handlers (Plug-in-and-Out)
        
        void Awake()
        {
            // Subscribe to material/texture request events
            if (EventHandler.Instance != null)
            {
                EventHandler.Instance.OnMaterialRequested += OnMaterialRequested;
                EventHandler.Instance.OnTextureRequested += OnTextureRequested;
                Debug.Log("[ProceduralCompute]  Subscribed to material/texture events");
            }
        }
        
        void OnDestroy()
        {
            // Unsubscribe from events
            if (EventHandler.Instance != null)
            {
                EventHandler.Instance.OnMaterialRequested -= OnMaterialRequested;
                EventHandler.Instance.OnTextureRequested -= OnTextureRequested;
            }
        }
        
        /// <summary>
        /// Handle material request event (plug-in-and-out).
        /// </summary>
        private void OnMaterialRequested(ProceduralCompute.MaterialType type, ProceduralCompute.TextureType textureType)
        {
            Material mat = GenerateMaterial(type, textureType);
            Debug.Log($"[ProceduralCompute] Generated material: {type}_{textureType}");
            // Material is cached and can be retrieved via GetCachedMaterial
        }
        
        /// <summary>
        /// Handle texture request event (plug-in-and-out).
        /// </summary>
        private void OnTextureRequested(ProceduralCompute.TextureType type, ProceduralCompute.MaterialType material)
        {
            Texture2D tex = GenerateTexture(type, material, GetPatternForType(material), 32);
            Debug.Log($"[ProceduralCompute] Generated texture: {type}_{material}");
            // Texture is cached and can be retrieved via GetCachedTexture
        }
        
        #endregion
    }
}
