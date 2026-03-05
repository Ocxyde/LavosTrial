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
// RoomTextureGenerator.cs
// Procedural 2D pixel art texture generation for rooms
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Plug-in-out Architecture:
// - Each room type gets unique texture
// - Textures generated procedurally from seed
// - Cached for performance

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Generates unique 2D pixel art textures for each room type.
    /// Plug-in-out: Access via RoomTextureGenerator.Instance
    /// </summary>
    public class RoomTextureGenerator : MonoBehaviour
    {
        #region Singleton

        private static RoomTextureGenerator _instance;
        public static RoomTextureGenerator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<RoomTextureGenerator>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("RoomTextureGenerator");
                        _instance = go.AddComponent<RoomTextureGenerator>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Inspector Fields

        [Header("Texture Settings")]
        [SerializeField] private int textureWidth = 32;
        [SerializeField] private int textureHeight = 32;
        [SerializeField] private bool usePointFilter = true;
        
        [Header("Caching")]
        [SerializeField] private bool cacheTextures = true;
        [SerializeField] private int maxCachedTextures = 20;

        #endregion

        #region Private Fields

        private Dictionary<string, Texture2D> _textureCache = new();
        private Dictionary<RoomType, Texture2D> _roomTypeTextures = new();
        private string _currentSeed = "";

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("[RoomTextureGenerator] Initialized");
        }

        #endregion

        #region Plug-in-Out API

        /// <summary>
        /// Get texture for specific room type.
        /// Plug-in-out: Call from RoomGenerator or MazeRenderer.
        /// </summary>
        public Texture2D GetRoomTexture(RoomType type, string seed = "")
        {
            // Use current seed if none provided
            if (string.IsNullOrEmpty(seed))
            {
                seed = _currentSeed;
            }

            // Create cache key
            string cacheKey = $"{type}_{seed}";

            // Check cache
            if (cacheTextures && _textureCache.ContainsKey(cacheKey))
            {
                return _textureCache[cacheKey];
            }

            // Generate new texture
            Texture2D texture = GenerateRoomTexture(type, seed);

            // Cache it
            if (cacheTextures)
            {
                if (_textureCache.Count >= maxCachedTextures)
                {
                    ClearOldestCache();
                }
                _textureCache[cacheKey] = texture;
            }

            return texture;
        }

        /// <summary>
        /// Set seed for texture generation.
        /// Plug-in-out: Call from SeedManager or MazeGenerator.
        /// </summary>
        public void SetSeed(string seed)
        {
            _currentSeed = seed;
            Debug.Log($"[RoomTextureGenerator] Seed set: {seed}");
        }

        /// <summary>
        /// Clear all cached textures.
        /// Plug-in-out: Call when changing levels or freeing memory.
        /// </summary>
        public void ClearCache()
        {
            foreach (var tex in _textureCache.Values)
            {
                if (tex != null)
                {
                    Destroy(tex);
                }
            }
            _textureCache.Clear();
            _roomTypeTextures.Clear();
            
            Debug.Log("[RoomTextureGenerator] Cache cleared");
        }

        #endregion

        #region Texture Generation

        /// <summary>
        /// Generate texture for room type.
        /// </summary>
        private Texture2D GenerateRoomTexture(RoomType type, string seed)
        {
            // Use existing PixelArtGenerator for base textures
            Texture2D baseTexture = GetBaseTexture(type);

            // Modify based on seed for uniqueness
            if (!string.IsNullOrEmpty(seed))
            {
                baseTexture = ModifyTextureWithSeed(baseTexture, seed, type);
            }

            return baseTexture;
        }

        /// <summary>
        /// Get base texture for room type.
        /// Uses PixelArtGenerator.cs and PixelArtTextureFactory.cs
        /// </summary>
        private Texture2D GetBaseTexture(RoomType type)
        {
            switch (type)
            {
                case RoomType.Normal:
                    return PixelArtGenerator.GenerateStoneTexture(textureWidth, textureHeight);
                    
                case RoomType.Treasure:
                    return GenerateTreasureRoomTexture();
                    
                case RoomType.Combat:
                    return GenerateCombatRoomTexture();
                    
                case RoomType.Trap:
                    return GenerateTrapRoomTexture();
                    
                case RoomType.Safe:
                    return GenerateSafeRoomTexture();
                    
                case RoomType.Boss:
                    return GenerateBossRoomTexture();
                    
                case RoomType.Secret:
                    return GenerateSecretRoomTexture();
                    
                case RoomType.Puzzle:
                    return GeneratePuzzleRoomTexture();
                    
                default:
                    return PixelArtGenerator.GenerateStoneTexture(textureWidth, textureHeight);
            }
        }

        /// <summary>
        /// Modify texture with seed for uniqueness.
        /// </summary>
        private Texture2D ModifyTextureWithSeed(Texture2D original, string seed, RoomType type)
        {
            // Create new texture
            Texture2D modified = new Texture2D(
                textureWidth, 
                textureHeight, 
                TextureFormat.RGBA32, 
                false
            );
            modified.filterMode = usePointFilter ? FilterMode.Point : FilterMode.Bilinear;

            // Use seed to deterministically modify colors
            int seedHash = seed.GetHashCode();
            Random.InitState(Mathf.Abs(seedHash));

            Color[] pixels = original.GetPixels();

            for (int i = 0; i < pixels.Length; i++)
            {
                // Slight color variation based on seed
                float variation = (Random.value - 0.5f) * 0.15f; // ±7.5% variation
                pixels[i].r = Mathf.Clamp01(pixels[i].r + variation);
                pixels[i].g = Mathf.Clamp01(pixels[i].g + variation);
                pixels[i].b = Mathf.Clamp01(pixels[i].b + variation);
            }

            modified.SetPixels(pixels);
            modified.Apply();

            return modified;
        }

        #endregion

        #region Special Room Textures

        private Texture2D GenerateTreasureRoomTexture()
        {
            // Gold-enriched stone with sparkles
            var canvas = new PixelCanvas(textureWidth, textureHeight);
            
            // Base: Darker stone
            for (int y = 0; y < textureHeight; y++)
            {
                for (int x = 0; x < textureWidth; x++)
                {
                    int brickX = (x / 8) % 2;
                    int brickY = y / 6;
                    bool isMortar = (x % 8 < 1) || (y % 6 < 1);

                    Color pixel;
                    if (isMortar)
                    {
                        pixel = new Color(0.4f, 0.35f, 0.2f); // Gold-tinted mortar
                    }
                    else
                    {
                        int pattern = ((brickX + brickY) % 3);
                        pixel = pattern switch
                        {
                            0 => new Color(0.5f, 0.45f, 0.3f),
                            1 => new Color(0.6f, 0.5f, 0.35f),
                            _ => new Color(0.7f, 0.6f, 0.4f)
                        };
                    }

                    // Random gold sparkles
                    if (Random.value < 0.02f)
                    {
                        pixel = new Color(1f, 0.9f, 0.3f);
                    }

                    canvas.SetPixel(x, y, pixel);
                }
            }

            return canvas.ToTexture();
        }

        private Texture2D GenerateCombatRoomTexture()
        {
            // Darker, battle-worn stone with cracks
            var canvas = new PixelCanvas(textureWidth, textureHeight);
            
            for (int y = 0; y < textureHeight; y++)
            {
                for (int x = 0; x < textureWidth; x++)
                {
                    int brickX = (x / 8) % 2;
                    int brickY = y / 6;
                    bool isMortar = (x % 8 < 1) || (y % 6 < 1);

                    Color pixel;
                    if (isMortar)
                    {
                        pixel = new Color(0.25f, 0.2f, 0.2f); // Dark mortar
                    }
                    else
                    {
                        int pattern = ((brickX + brickY) % 3);
                        pixel = pattern switch
                        {
                            0 => new Color(0.35f, 0.3f, 0.28f),
                            1 => new Color(0.4f, 0.35f, 0.32f),
                            _ => new Color(0.45f, 0.4f, 0.38f)
                        };
                    }

                    // Random crack marks
                    if (Random.value < 0.03f)
                    {
                        pixel = new Color(0.15f, 0.1f, 0.1f);
                    }

                    canvas.SetPixel(x, y, pixel);
                }
            }

            return canvas.ToTexture();
        }

        private Texture2D GenerateTrapRoomTexture()
        {
            // Uneven, suspicious-looking tiles
            var canvas = new PixelCanvas(textureWidth, textureHeight);
            
            for (int y = 0; y < textureHeight; y++)
            {
                for (int x = 0; x < textureWidth; x++)
                {
                    int tileX = x / 4;
                    int tileY = y / 4;
                    
                    // Checkerboard pattern
                    bool isDark = (tileX + tileY) % 2 == 0;
                    
                    Color pixel = isDark ? 
                        new Color(0.3f, 0.25f, 0.35f) :  // Purple-tinted
                        new Color(0.4f, 0.35f, 0.45f);

                    // Random pressure plate hints
                    if (Random.value < 0.01f)
                    {
                        pixel = new Color(0.5f, 0.2f, 0.2f); // Reddish hint
                    }

                    canvas.SetPixel(x, y, pixel);
                }
            }

            return canvas.ToTexture();
        }

        private Texture2D GenerateSafeRoomTexture()
        {
            // Clean, well-maintained stone
            var canvas = new PixelCanvas(textureWidth, textureHeight);
            
            for (int y = 0; y < textureHeight; y++)
            {
                for (int x = 0; x < textureWidth; x++)
                {
                    int brickX = (x / 8) % 2;
                    int brickY = y / 6;
                    bool isMortar = (x % 8 < 1) || (y % 6 < 1);

                    Color pixel;
                    if (isMortar)
                    {
                        pixel = new Color(0.35f, 0.35f, 0.35f); // Clean mortar
                    }
                    else
                    {
                        int pattern = ((brickX + brickY) % 3);
                        pixel = pattern switch
                        {
                            0 => new Color(0.5f, 0.5f, 0.5f),
                            1 => new Color(0.55f, 0.55f, 0.55f),
                            _ => new Color(0.6f, 0.6f, 0.6f)
                        };
                    }

                    canvas.SetPixel(x, y, pixel);
                }
            }

            return canvas.ToTexture();
        }

        private Texture2D GenerateBossRoomTexture()
        {
            // Imposing dark stone with red accents
            var canvas = new PixelCanvas(textureWidth, textureHeight);
            
            for (int y = 0; y < textureHeight; y++)
            {
                for (int x = 0; x < textureWidth; x++)
                {
                    int brickX = (x / 8) % 2;
                    int brickY = y / 6;
                    bool isMortar = (x % 8 < 1) || (y % 6 < 1);

                    Color pixel;
                    if (isMortar)
                    {
                        pixel = new Color(0.2f, 0.15f, 0.15f); // Dark red mortar
                    }
                    else
                    {
                        int pattern = ((brickX + brickY) % 3);
                        pixel = pattern switch
                        {
                            0 => new Color(0.3f, 0.25f, 0.25f),
                            1 => new Color(0.35f, 0.28f, 0.28f),
                            _ => new Color(0.4f, 0.3f, 0.3f)
                        };
                    }

                    // Random red glow spots
                    if (Random.value < 0.015f)
                    {
                        pixel = new Color(0.8f, 0.2f, 0.2f);
                    }

                    canvas.SetPixel(x, y, pixel);
                }
            }

            return canvas.ToTexture();
        }

        private Texture2D GenerateSecretRoomTexture()
        {
            // Mysterious purple-blue stone
            var canvas = new PixelCanvas(textureWidth, textureHeight);
            
            for (int y = 0; y < textureHeight; y++)
            {
                for (int x = 0; x < textureWidth; x++)
                {
                    int brickX = (x / 8) % 2;
                    int brickY = y / 6;
                    bool isMortar = (x % 8 < 1) || (y % 6 < 1);

                    Color pixel;
                    if (isMortar)
                    {
                        pixel = new Color(0.25f, 0.2f, 0.35f); // Purple mortar
                    }
                    else
                    {
                        int pattern = ((brickX + brickY) % 3);
                        pixel = pattern switch
                        {
                            0 => new Color(0.35f, 0.3f, 0.5f),
                            1 => new Color(0.4f, 0.35f, 0.55f),
                            _ => new Color(0.45f, 0.4f, 0.6f)
                        };
                    }

                    // Random magical sparkles
                    if (Random.value < 0.025f)
                    {
                        pixel = new Color(0.6f, 0.5f, 1f);
                    }

                    canvas.SetPixel(x, y, pixel);
                }
            }

            return canvas.ToTexture();
        }

        private Texture2D GeneratePuzzleRoomTexture()
        {
            // Geometric pattern tiles
            var canvas = new PixelCanvas(textureWidth, textureHeight);
            
            for (int y = 0; y < textureHeight; y++)
            {
                for (int x = 0; x < textureWidth; x++)
                {
                    int tileX = x / 4;
                    int tileY = y / 4;
                    
                    // Geometric pattern
                    int pattern = (tileX * 3 + tileY * 7) % 4;
                    
                    Color pixel = pattern switch
                    {
                        0 => new Color(0.4f, 0.35f, 0.3f),
                        1 => new Color(0.5f, 0.45f, 0.4f),
                        2 => new Color(0.45f, 0.4f, 0.5f),
                        _ => new Color(0.4f, 0.45f, 0.5f)
                    };

                    canvas.SetPixel(x, y, pixel);
                }
            }

            return canvas.ToTexture();
        }

        #endregion

        #region Utilities

        private void ClearOldestCache()
        {
            // Remove oldest entry
            if (_textureCache.Count > 0)
            {
                var oldest = new List<KeyValuePair<string, Texture2D>>(_textureCache)[0];
                _textureCache.Remove(oldest.Key);
                if (oldest.Value != null)
                {
                    Destroy(oldest.Value);
                }
            }
        }

        #endregion

        #region Debug

        private void OnGUI()
        {
            if (!Application.isEditor) return;

            GUILayout.BeginArea(new Rect(10, 220, 300, 150));
            GUILayout.Label($"[RoomTextureGenerator DEBUG]");
            GUILayout.Label($"Cached Textures: {_textureCache.Count}");
            GUILayout.Label($"Current Seed: {_currentSeed}");

            if (GUILayout.Button("Clear Cache"))
            {
                ClearCache();
            }

            if (GUILayout.Button("Test Generate All"))
            {
                foreach (RoomType type in System.Enum.GetValues(typeof(RoomType)))
                {
                    Texture2D tex = GetRoomTexture(type, "TEST");
                    Debug.Log($"[RoomTextureGenerator] Generated {type}: {tex}");
                }
            }

            GUILayout.EndArea();
        }

        #endregion
    }
}
