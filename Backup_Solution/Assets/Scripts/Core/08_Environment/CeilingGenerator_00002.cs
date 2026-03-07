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
// CeilingGenerator.cs
// Creates flat cube ceiling with 2D pixel art dark stone texture
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// FEATURES:
// - Flat cube (not quad) for proper trap functionality
// - 2D pixel art dark stone texture (no smooth, flat style)
// - Configurable stone pattern (darker than ground)
// - Ready for ceiling traps
//
// Location: Assets/Scripts/Core/08_Environment/

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// CeilingGenerator - Creates flat cube ceiling with pixel art dark stone texture.
    /// </summary>
    public static class CeilingGenerator
    {
        /// <summary>
        /// Create flat cube ceiling with dark stone texture.
        /// </summary>
        public static GameObject CreateCeilingCube(float size, int resolution = 32)
        {
            // Create cube primitive (flat, wide)
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "CeilingPlane";
            
            // Scale to flat wide cube (positioned at ceiling height)
            ceiling.transform.position = new Vector3(0f, 3.5f, 0f);  // At wall top (3.5m high)
            ceiling.transform.localScale = new Vector3(size, 0.1f, size);  // Very flat (0.1m thick)
            
            // Generate pixel art dark stone texture
            Texture2D stoneTexture = CreatePixelArtDarkStoneTexture(resolution, resolution);
            
            // Apply texture to ceiling
            var renderer = ceiling.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                // Create material with stone texture
                var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                if (material == null || material.shader == null)
                {
                    material = new Material(Shader.Find("Standard"));
                }
                
                if (material != null)
                {
                    material.mainTexture = stoneTexture;
                    material.color = Color.white;
                    material.SetFloat("_Glossiness", 0f);  // No smoothness (flat stone)
                    material.SetFloat("_Metallic", 0f);   // Not metallic
                    renderer.material = material;
                }
            }
            
            Debug.Log($"[CeilingPlane] Created {size}x{size}m flat cube ceiling with pixel art dark stone texture");
            
            return ceiling;
        }
        
        /// <summary>
        /// Create 2D pixel art dark stone texture (darker than ground).
        /// </summary>
        public static Texture2D CreatePixelArtDarkStoneTexture(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;  // Pixel art (no smoothing)
            texture.wrapMode = TextureWrapMode.Repeat;
            
            // Dark stone color palette (darker than ground, ceiling shadows)
            Color[] stoneColors = new Color[]
            {
                new Color(0.30f, 0.28f, 0.26f),  // Very dark gray
                new Color(0.35f, 0.32f, 0.30f),  // Dark gray
                new Color(0.40f, 0.37f, 0.35f),  // Medium-dark gray
                new Color(0.45f, 0.42f, 0.40f),  // Medium gray
                new Color(0.25f, 0.22f, 0.20f),  // Almost black (deep shadows)
            };
            
            // Generate stone pattern (blocky, pixel art style)
            int blockSize = Mathf.Max(2, width / 16);  // Stone block size
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Create blocky stone pattern
                    int blockX = x / blockSize;
                    int blockY = y / blockSize;
                    
                    // Add variation between blocks
                    int colorIndex = ((blockX + blockY * 3) % stoneColors.Length);
                    Color baseColor = stoneColors[colorIndex];
                    
                    // Add mortar lines (darker between blocks)
                    bool isMortar = (x % blockSize == 0) || (y % blockSize == 0);
                    if (isMortar)
                    {
                        baseColor = new Color(0.15f, 0.12f, 0.10f);  // Very dark mortar
                    }
                    
                    // Add random stone variation
                    if (!isMortar && Random.value < 0.1f)
                    {
                        // Random darker spot (stone imperfection)
                        baseColor *= 0.7f;
                    }
                    
                    texture.SetPixel(x, y, baseColor);
                }
            }
            
            texture.Apply();
            
            Debug.Log($"[CeilingPlane] Generated {width}x{height} pixel art dark stone texture (block size: {blockSize})");
            
            return texture;
        }
    }
}
