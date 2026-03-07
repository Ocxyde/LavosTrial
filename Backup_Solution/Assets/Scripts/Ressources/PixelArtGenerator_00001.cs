// PixelArtGenerator.cs
// Procedural pixel art texture generation for doors and surfaces
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Generates pixel art textures for doors, walls, and surfaces.
    /// </summary>
    public static class PixelArtGenerator
    {
        #region Wood Textures
        
        public static Texture2D GenerateWoodTexture(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            
            Color[] pixels = new Color[width * height];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Wood grain pattern
                    int grainPattern = ((x / 4) + (y / 3)) % 3;
                    Color woodColor = grainPattern switch
                    {
                        0 => new Color(0.2f, 0.12f, 0.08f),
                        1 => new Color(0.27f, 0.2f, 0.12f),
                        _ => new Color(0.35f, 0.26f, 0.16f)
                    };
                    
                    // Add noise
                    float noise = Random.value * 0.1f;
                    woodColor += new Color(noise, noise, noise);
                    
                    pixels[y * width + x] = woodColor;
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            return texture;
        }
        
        #endregion
        
        #region Stone Textures
        
        public static Texture2D GenerateStoneTexture(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            
            Color[] pixels = new Color[width * height];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Stone brick pattern
                    int brickX = (x / 8) % 2;
                    int brickY = y / 6;
                    
                    bool isMortar = (x % 8 < 1) || (y % 6 < 1);
                    
                    Color stoneColor;
                    if (isMortar)
                    {
                        stoneColor = new Color(0.3f, 0.3f, 0.3f); // Mortar
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
                    
                    // Add noise
                    float noise = Random.value * 0.08f;
                    stoneColor += new Color(noise, noise, noise);
                    
                    pixels[y * width + x] = stoneColor;
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            return texture;
        }
        
        #endregion
        
        #region Metal Textures
        
        public static Texture2D GenerateMetalTexture(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            
            Color[] pixels = new Color[width * height];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Metal plate pattern
                    bool isRivet = (x % 8 == 0 || x % 8 == 7) && (y % 8 == 0 || y % 8 == 7);
                    
                    Color metalColor;
                    if (isRivet)
                    {
                        metalColor = new Color(0.7f, 0.65f, 0.5f); // Rivet
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
                    
                    // Add metallic sheen
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
        
        #region Magic Textures
        
        public static Texture2D GenerateMagicTexture(int width, int height, Color baseColor)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            
            Color[] pixels = new Color[width * height];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Swirling magic pattern
                    float time = Time.time * 0.5f;
                    float swirl = Mathf.Sin(x * 0.2f + time) * Mathf.Cos(y * 0.2f + time);
                    
                    Color magicColor = baseColor;
                    magicColor.r += swirl * 0.2f;
                    magicColor.g += swirl * 0.2f;
                    magicColor.b += swirl * 0.2f;
                    
                    // Add glow
                    float glow = Mathf.PerlinNoise(x * 0.1f + time, y * 0.1f + time) * 0.3f;
                    magicColor += new Color(glow, glow, glow);
                    
                    pixels[y * width + x] = magicColor;
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            return texture;
        }
        
        #endregion
        
        #region Surface Textures (Floor/Wall/Ceiling)
        
        public static Texture2D GenerateFloorTexture(int width, int height)
        {
            return GenerateStoneTexture(width, height);
        }
        
        public static Texture2D GenerateWallTexture(int width, int height)
        {
            return GenerateStoneTexture(width, height);
        }
        
        public static Texture2D GenerateCeilingTexture(int width, int height)
        {
            return GenerateStoneTexture(width, height);
        }
        
        #endregion
    }
}
