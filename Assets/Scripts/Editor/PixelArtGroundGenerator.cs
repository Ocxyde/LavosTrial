// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8 (no BOM) | Line Endings: Unix LF
// Cell-Based Maze Generation System - Editor Tool: Pixel Art Ground Generator

using UnityEngine;
using UnityEditor;
using System.IO;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// PixelArtGroundGenerator - Creates 8-bit pixel art stone ground textures.
    /// </summary>
    public class PixelArtGroundGenerator : EditorWindow
    {
        [MenuItem("Tools/Generate Pixel Art Ground Texture")]
        public static void GenerateGroundTexture()
        {
            string folderPath = "Assets/Textures/Ground";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/Textures", "Ground");
            }
            
            Texture2D texture = CreateStoneGroundTexture(64, 64);
            string path = $"{folderPath}/Ground_Stone_PixelArt.png";
            SaveTexture(texture, path);
            
            Material groundMat = CreateGroundMaterial(texture);
            
            AssetDatabase.Refresh();
            
            Debug.Log($"[PixelArtGroundGenerator] Created: {path}");
            
            EditorUtility.DisplayDialog(
                "Ground Texture Created",
                $"Stone ground texture created!\n\nTexture: {path}\nMaterial: Assets/Textures/Ground/Ground_Stone_Mat.mat",
                "OK"
            );
        }
        
        private static Texture2D CreateStoneGroundTexture(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Repeat;
            
            Color[] stoneColors = new Color[]
            {
                new Color32(105, 105, 105, 255),
                new Color32(128, 128, 128, 255),
                new Color32(150, 150, 150, 255),
                new Color32(110, 100, 90, 255),
                new Color32(139, 125, 110, 255),
                new Color32(90, 90, 95, 255),
                new Color32(160, 155, 150, 255),
                new Color32(80, 80, 85, 255)
            };
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int blockX = x / 4;
                    int blockY = y / 4;
                    int colorIndex = Mathf.Abs((blockX * 7 + blockY * 13) % stoneColors.Length);
                    
                    int localX = x % 4;
                    int localY = y % 4;
                    bool isEdge = (localX == 0 || localX == 3 || localY == 0 || localY == 3);
                    
                    Color baseColor = stoneColors[colorIndex];
                    if (isEdge) baseColor *= 0.8f;
                    
                    float noise = Mathf.PerlinNoise(x * 0.3f, y * 0.3f) * 0.1f;
                    baseColor += new Color(noise, noise, noise);
                    
                    texture.SetPixel(x, y, baseColor);
                }
            }
            
            texture.Apply();
            return texture;
        }
        
        private static Material CreateGroundMaterial(Texture2D texture)
        {
            string matPath = "Assets/Textures/Ground/Ground_Stone_Mat.mat";
            Material existingMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (existingMat != null)
            {
                existingMat.mainTexture = texture;
                AssetDatabase.SaveAssets();
                return existingMat;
            }
            
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.mainTexture = texture;
            mat.color = Color.white;
            mat.name = "Ground_Stone_Mat";
            
            AssetDatabase.CreateAsset(mat, matPath);
            AssetDatabase.SaveAssets();
            return mat;
        }
        
        private static void SaveTexture(Texture2D texture, string path)
        {
            byte[] pngData = texture.EncodeToPNG();
            File.WriteAllBytes(path, pngData);
            AssetDatabase.ImportAsset(path);
            
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.filterMode = FilterMode.Point;
                importer.compressionQuality = 0;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.SaveAndReimport();
            }
        }
    }
}
