// GroundPlaneGenerator.cs
// Creates flat cube ground with 2D pixel art stone texture
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// FEATURES:
// - Flat cube (not quad) for proper trap functionality
// - 2D pixel art stone texture (no smooth, flat style)
// - Configurable stone pattern
// - Ready for hole traps
//
// Location: Assets/Scripts/Core/08_Environment/

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// GroundPlaneGenerator - Creates flat cube ground with pixel art stone texture.
    /// </summary>
    public static class GroundPlaneGenerator
    {
        /// <summary>
        /// Create flat cube ground with stone texture.
        /// </summary>
        public static GameObject CreateGroundCube(float size, int resolution = 32)
        {
            // Create cube primitive (flat, wide)
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "GroundPlane";
            
            // Scale to flat wide cube (positioned BELOW y=0 to avoid z-fighting)
            ground.transform.position = new Vector3(0f, -0.1f, 0f);  // Lower to prevent z-fighting
            ground.transform.localScale = new Vector3(size, 0.1f, size);  // Very flat (0.1m thick)
            
            // Remove collider if not needed (optional)
            // var collider = ground.GetComponent<BoxCollider>();
            // if (collider != null) Object.Destroy(collider);
            
            // Generate pixel art stone texture
            Texture2D stoneTexture = CreatePixelArtStoneTexture(resolution, resolution);
            Debug.Log($"[GroundPlane] Generated {resolution}x{resolution} pixel art stone texture");

            // Apply texture to ground
            var renderer = ground.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                // Try URP shader first, then Standard, then Unlit as fallback
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    Debug.LogWarning("[GroundPlane] URP shader not found, trying Standard shader");
                    shader = Shader.Find("Standard");
                }
                if (shader == null)
                {
                    Debug.LogWarning("[GroundPlane] Standard shader not found, using Unlit/Texture");
                    shader = Shader.Find("Unlit/Texture");
                }
                
                if (shader != null)
                {
                    Material material = new Material(shader);
                    material.mainTexture = stoneTexture;
                    material.color = Color.white;
                    
                    // Set material properties based on shader type
                    if (shader.name.Contains("URP") || shader.name.Contains("Standard"))
                    {
                        material.SetFloat("_Glossiness", 0f);  // No smoothness
                        material.SetFloat("_Metallic", 0f);   // Not metallic
                    }
                    
                    renderer.material = material;
                    Debug.Log($"[GroundPlane] Material created with shader: {shader.name}");
                }
                else
                {
                    Debug.LogError("[GroundPlane] No valid shader found! Ground will be untextured.");
                }
            }
            else
            {
                Debug.LogError("[GroundPlane] MeshRenderer not found on ground object!");
            }
            
            Debug.Log($"[GroundPlane] Created {size}x{size}m flat cube ground with pixel art stone texture");
            
            return ground;
        }
        
        /// <summary>
        /// Create 2D pixel art stone texture.
        /// </summary>
        public static Texture2D CreatePixelArtStoneTexture(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;  // Pixel art (no smoothing)
            texture.wrapMode = TextureWrapMode.Repeat;
            
            // Stone color palette (gray tones, pixel art style)
            Color[] stoneColors = new Color[]
            {
                new Color(0.45f, 0.42f, 0.40f),  // Dark gray
                new Color(0.50f, 0.47f, 0.45f),  // Medium-dark gray
                new Color(0.55f, 0.52f, 0.50f),  // Medium gray
                new Color(0.60f, 0.57f, 0.55f),  // Medium-light gray
                new Color(0.35f, 0.32f, 0.30f),  // Very dark gray (cracks)
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
                        baseColor = new Color(0.25f, 0.22f, 0.20f);  // Dark mortar
                    }
                    
                    // Add random stone variation
                    if (!isMortar && Random.value < 0.1f)
                    {
                        // Random darker spot (stone imperfection)
                        baseColor *= 0.8f;
                    }
                    
                    texture.SetPixel(x, y, baseColor);
                }
            }
            
            texture.Apply();
            
            Debug.Log($"[GroundPlane] Generated {width}x{height} pixel art stone texture (block size: {blockSize})");
            
            return texture;
        }
        
        /// <summary>
        /// Create ground with hole trap at specific position.
        /// </summary>
        public static void CreateHoleTrap(GameObject ground, Vector3 holePosition, float holeSize = 2f)
        {
            Debug.Log($"[GroundPlane] Creating hole trap at {holePosition} (size: {holeSize}m)");
            // Hole trap implementation will modify ground mesh
            // This is a placeholder for future trap system
        }
    }
}
