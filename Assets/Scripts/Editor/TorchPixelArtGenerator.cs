// Copyright (C) 2026 Ocxyde
// GPL-3.0 license - see COPYING
// TorchPixelArtGenerator.cs - Editor tool to generate 8-bit pixel art torch textures

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// Editor tool to generate 8-bit pixel art textures for torch prefab.
    /// Gothic dark/shadow grey style, 32x32 pixels.
    /// </summary>
    public static class TorchPixelArtGenerator
    {
        private const string TEXTURE_FOLDER = "Assets/Textures/Torch";
        private const string MATERIAL_FOLDER = "Assets/Materials/Torch";
        private const int TEXTURE_SIZE = 32;
        private const int BLOCK_SIZE = 1; // 1:1 pixel art

        [MenuItem("Tools/Lavos/Generate Torch Pixel Art Textures")]
        public static void GenerateTorchTextures()
        {
            EnsureFoldersExist();

            // Generate torch handle texture (dark iron/gothic metal)
            Texture2D handleTexture = CreateHandleTexture();
            SaveTexture(handleTexture, $"{TEXTURE_FOLDER}/Torch_Handle.png");

            // Generate torch flame texture (subtle gothic fire)
            Texture2D flameTexture = CreateFlameTexture();
            SaveTexture(flameTexture, $"{TEXTURE_FOLDER}/Torch_Flame.png");

            // Generate torch base texture (shadow grey stone/metal)
            Texture2D baseTexture = CreateBaseTexture();
            SaveTexture(baseTexture, $"{TEXTURE_FOLDER}/Torch_Base.png");

            // Create materials
            CreateTorchMaterials(handleTexture, flameTexture, baseTexture);

            AssetDatabase.Refresh();
            Debug.Log("[TorchPixelArt] ✅ Generated 8-bit pixel art torch textures and materials!");
        }

        private static void EnsureFoldersExist()
        {
            if (!Directory.Exists(TEXTURE_FOLDER))
            {
                Directory.CreateDirectory(TEXTURE_FOLDER);
                Debug.Log($"[TorchPixelArt] Created folder: {TEXTURE_FOLDER}");
            }

            if (!Directory.Exists(MATERIAL_FOLDER))
            {
                Directory.CreateDirectory(MATERIAL_FOLDER);
                Debug.Log($"[TorchPixelArt] Created folder: {MATERIAL_FOLDER}");
            }
        }

        /// <summary>
        /// Create torch handle texture - dark iron gothic metal style.
        /// </summary>
        private static Texture2D CreateHandleTexture()
        {
            Texture2D texture = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point; // Pixel art

            // Gothic dark iron palette
            Color darkIron = new Color(0.15f, 0.12f, 0.1f);      // Dark shadow grey
            Color iron = new Color(0.25f, 0.22f, 0.2f);          // Base iron
            Color ironHighlight = new Color(0.35f, 0.32f, 0.3f); // Light highlight
            Color ironShadow = new Color(0.08f, 0.06f, 0.05f);   // Deep shadow

            for (int y = 0; y < TEXTURE_SIZE; y++)
            {
                for (int x = 0; x < TEXTURE_SIZE; x++)
                {
                    // Create vertical handle pattern
                    float normalizedX = (float)x / TEXTURE_SIZE;
                    float normalizedY = (float)y / TEXTURE_SIZE;

                    // Center is lighter (cylindrical highlight)
                    float centerDist = Mathf.Abs(normalizedX - 0.5f) * 2f;

                    Color baseColor;
                    if (centerDist < 0.3f)
                        baseColor = ironHighlight;
                    else if (centerDist < 0.6f)
                        baseColor = iron;
                    else
                        baseColor = darkIron;

                    // Add horizontal bands (decorative rings)
                    if (y % 8 == 0 || y % 8 == 1)
                        baseColor *= 0.7f;

                    // Add noise for metal texture
                    float noise = Random.value * 0.15f;
                    baseColor.r += noise;
                    baseColor.g += noise;
                    baseColor.b += noise;

                    // Clamp
                    baseColor.r = Mathf.Clamp01(baseColor.r);
                    baseColor.g = Mathf.Clamp01(baseColor.g);
                    baseColor.b = Mathf.Clamp01(baseColor.b);

                    texture.SetPixel(x, y, baseColor);
                }
            }

            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Create torch flame texture - subtle gothic fire (dim orange/red).
        /// </summary>
        private static Texture2D CreateFlameTexture()
        {
            Texture2D texture = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            // Gothic flame palette (dim, shadowy fire)
            Color flameCore = new Color(0.8f, 0.3f, 0.1f, 1.0f);     // Orange core
            Color flameMid = new Color(0.6f, 0.2f, 0.05f, 0.8f);     // Mid flame
            Color flameEdge = new Color(0.4f, 0.1f, 0.02f, 0.4f);    // Edge
            Color flameTransparent = new Color(0.3f, 0.05f, 0.01f, 0.1f); // Fading edge

            // Clear to transparent
            for (int y = 0; y < TEXTURE_SIZE; y++)
            {
                for (int x = 0; x < TEXTURE_SIZE; x++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }

            // Draw flame shape (teardrop)
            for (int y = 0; y < TEXTURE_SIZE; y++)
            {
                for (int x = 0; x < TEXTURE_SIZE; x++)
                {
                    float normalizedY = (float)y / TEXTURE_SIZE;
                    float normalizedX = (float)x / TEXTURE_SIZE;

                    // Flame is wider at bottom, narrower at top
                    float maxWidth = 0.3f + (0.2f * (1f - normalizedY));
                    float centerX = 0.5f;
                    float distFromCenter = Mathf.Abs(normalizedX - centerX);

                    if (distFromCenter < maxWidth && normalizedY > 0.1f)
                    {
                        // Distance from flame edge
                        float edgeDist = distFromCenter / maxWidth;

                        Color flameColor;
                        if (edgeDist < 0.3f)
                            flameColor = flameCore;
                        else if (edgeDist < 0.6f)
                            flameColor = flameMid;
                        else
                            flameColor = flameEdge;

                        // Fade at top and bottom
                        float fade = 1f;
                        if (normalizedY < 0.2f) fade = normalizedY / 0.2f;
                        if (normalizedY > 0.8f) fade = (1f - normalizedY) / 0.2f;

                        flameColor.a *= fade;

                        // Add flicker noise
                        float noise = Random.value * 0.2f;
                        flameColor.r += noise;
                        flameColor.g += noise * 0.5f;

                        texture.SetPixel(x, y, flameColor);
                    }
                }
            }

            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Create torch base texture - shadow grey stone/metal mount.
        /// </summary>
        private static Texture2D CreateBaseTexture()
        {
            Texture2D texture = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            // Shadow grey gothic stone palette
            Color darkStone = new Color(0.12f, 0.1f, 0.09f);
            Color stone = new Color(0.2f, 0.18f, 0.16f);
            Color stoneHighlight = new Color(0.28f, 0.25f, 0.23f);

            for (int y = 0; y < TEXTURE_SIZE; y++)
            {
                for (int x = 0; x < TEXTURE_SIZE; x++)
                {
                    float normalizedX = (float)x / TEXTURE_SIZE;
                    float normalizedY = (float)y / TEXTURE_SIZE;

                    // Base color
                    Color baseColor = stone;

                    // Add stone block pattern
                    if (x % 8 < 2 || y % 8 < 2)
                        baseColor = darkStone;

                    // Add highlight at edges (beveled look)
                    if (x < 3 || x > TEXTURE_SIZE - 4 || y < 3 || y > TEXTURE_SIZE - 4)
                        baseColor = stoneHighlight;

                    // Add noise for stone texture
                    float noise = Random.value * 0.1f;
                    baseColor.r += noise;
                    baseColor.g += noise;
                    baseColor.b += noise;

                    texture.SetPixel(x, y, baseColor);
                }
            }

            texture.Apply();
            return texture;
        }

        private static void SaveTexture(Texture2D texture, string path)
        {
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            Debug.Log($"[TorchPixelArt] Saved: {path}");
        }

        private static void CreateTorchMaterials(Texture2D handleTexture, Texture2D flameTexture, Texture2D baseTexture)
        {
            // Handle material
            Material handleMat = CreateMaterial(handleTexture, "Universal Render Pipeline/Lit", "Torch_Handle");
            
            // Flame material (emissive, transparent)
            Material flameMat = CreateFlameMaterial(flameTexture);
            
            // Base material
            Material baseMat = CreateMaterial(baseTexture, "Universal Render Pipeline/Lit", "Torch_Base");

            AssetDatabase.SaveAssets();
            Debug.Log("[TorchPixelArt] ✅ Created 3 torch materials!");
        }

        private static Material CreateMaterial(Texture2D texture, string shaderName, string materialName)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogError($"[TorchPixelArt] Shader '{shaderName}' not found!");
                return null;
            }

            Material mat = new Material(shader);
            mat.SetTexture("_BaseMap", texture);
            mat.SetTexture("_MainTex", texture);
            mat.SetTextureScale("_BaseMap", new Vector2(1f, 1f));
            mat.SetTextureScale("_MainTex", new Vector2(1f, 1f));
            mat.SetFloat("_Smoothness", 0.2f);
            mat.SetFloat("_Metallic", 0.5f);

            string matPath = $"{MATERIAL_FOLDER}/{materialName}.mat";
            AssetDatabase.CreateAsset(mat, matPath);
            Debug.Log($"[TorchPixelArt] Created material: {matPath}");

            return mat;
        }

        private static Material CreateFlameMaterial(Texture2D flameTexture)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                Debug.LogError("[TorchPixelArt] URP Unlit shader not found!");
                return null;
            }

            Material mat = new Material(shader);
            mat.SetTexture("_BaseMap", flameTexture);
            mat.SetTexture("_MainTex", flameTexture);
            mat.SetFloat("_SurfaceType", 1f); // Transparent
            mat.SetFloat("_BlendMode", 0f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;

            // Add emissive for glow
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.5f, 0.2f, 1f));

            string matPath = $"{MATERIAL_FOLDER}/Torch_Flame.mat";
            AssetDatabase.CreateAsset(mat, matPath);
            Debug.Log($"[TorchPixelArt] Created flame material: {matPath}");

            return mat;
        }
    }
}
#endif
