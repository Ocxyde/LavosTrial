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
// FloorPrefabGenerator.cs
// Editor tool to generate floor tile prefab for maze cells
// Uses PixelArtGenerator for 2D pixel art stone texture
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   1. Tools -> Maze -> Generate Floor Prefab
//   2. Prefab created in Assets/Resources/Prefabs/
//   3. Assign to CompleteMazeBuilder inspector
//
// FEATURES:
//   - Floor tile sized to fit cell grid (from GameConfig)
//   - 2D pixel art stone texture (consistent with project style)
//   - URP Lit material with matte stone finish
//   - No hardcoded values (JSON-driven via GameConfig)

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using Code.Lavos.Core;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// FloorPrefabGenerator - Creates floor tile prefab for maze cells.
    /// Generates a prefab that perfectly fits each cell in the grid.
    /// Uses PixelArtGenerator for consistent 2D pixel art style.
    /// Values from GameConfig (no hardcoding).
    /// </summary>
    public class FloorPrefabGenerator : EditorWindow
    {
        // Prefab output paths
        private const string PREFAB_PATH = "Assets/Resources/Prefabs/FloorTilePrefab.prefab";
        private const string MATERIAL_PATH = "Assets/Resources/Materials/FloorTileMaterial.mat";
        private const string TEXTURE_PATH = "Assets/Resources/Materials/FloorTileTexture.png";

        // Texture settings (pixel art resolution)
        private const int TEXTURE_SIZE = 64;

        // Menu item
        [MenuItem("Tools/Maze/Generate Floor Prefab")]
        public static void GenerateFloorPrefab()
        {
            // Get cell dimensions from GameConfig (no hardcoding)
            GameConfig config = FindFirstObjectByType<GameConfig>();
            float cellSize = config != null ? config.CellSize : 6.0f;
            float floorThickness = 1.0f; // Standard floor thickness

            if (config == null)
            {
                Debug.LogWarning("[FloorPrefab] GameConfig not found in scene. Using default cell size (6.0m).");
            }

            // Ensure folders exist
            string prefabFolder = "Assets/Resources/Prefabs";
            string materialFolder = "Assets/Resources/Materials";
            
            if (!Directory.Exists(prefabFolder))
            {
                Directory.CreateDirectory(prefabFolder);
            }
            if (!Directory.Exists(materialFolder))
            {
                Directory.CreateDirectory(materialFolder);
            }
            AssetDatabase.Refresh();

            // Check if prefab already exists
            string prefabPath = PREFAB_PATH;
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (existingPrefab != null)
            {
                if (!EditorUtility.DisplayDialog(
                    "Floor Prefab Exists",
                    $"Floor prefab already exists at:\n{prefabPath}\n\nOverwrite?",
                    "Overwrite",
                    "Cancel"))
                {
                    return;
                }
                AssetDatabase.DeleteAsset(prefabPath);
            }

            // Create material first (with pixel art stone texture)
            Material floorMaterial = CreateFloorMaterial(cellSize);

            // Create floor tile
            GameObject floorTile = CreateFloorTile(floorMaterial, cellSize, floorThickness);

            // Create prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(floorTile, prefabPath);

            // Clean up temp object
            DestroyImmediate(floorTile);

            if (prefab != null)
            {
                Debug.Log($"[FloorPrefab] Created: {prefabPath}");
                Debug.Log($"[FloorPrefab] Size: {cellSize}m x {cellSize}m x {floorThickness}m");
                Debug.Log($"[FloorPrefab] Material: {MATERIAL_PATH}");
                Debug.Log($"[FloorPrefab] Texture: {TEXTURE_SIZE}x{TEXTURE_SIZE} pixel art stone");
            }
            else
            {
                Debug.LogError($"[FloorPrefab] Failed to create prefab at {prefabPath}");
            }
        }

        /// <summary>
        /// Create a floor material with 2D pixel art stone texture.
        /// Uses PixelArtGenerator.GenerateStoneTexture() for consistency.
        /// </summary>
        private static Material CreateFloorMaterial(float cellSize)
        {
            // Check if material already exists
            Material existingMat = AssetDatabase.LoadAssetAtPath<Material>(MATERIAL_PATH);
            if (existingMat != null)
            {
                AssetDatabase.DeleteAsset(MATERIAL_PATH);
            }

            // Create new material with URP Lit shader
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard"); // Fallback to built-in
            }

            Material material = new Material(shader);
            material.name = "FloorTileMaterial";

            // Generate pixel art stone texture using existing PixelArtGenerator
            Texture2D texture = PixelArtGenerator.GenerateStoneTexture(TEXTURE_SIZE, TEXTURE_SIZE);
            texture.name = "FloorTileTexture";
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Point; // Keep pixel art sharp

            // Save texture as PNG
            byte[] pngData = texture.EncodeToPNG();
            File.WriteAllBytes(TEXTURE_PATH, pngData);
            AssetDatabase.Refresh();

            // Load the saved texture (proper import settings)
            Texture2D savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(TEXTURE_PATH);

            // Apply texture to material
            material.mainTexture = savedTexture;
            material.color = new Color(0.55f, 0.5f, 0.45f, 1.0f); // Granite base tint

            // Set material properties for stone look
            material.SetFloat("_Metallic", 0.0f);
            material.SetFloat("_Smoothness", 0.2f); // Low smoothness for matte stone

            // Set texture tiling (repeat across floor surface)
            material.mainTextureScale = new Vector2(cellSize, cellSize);

            // Save material
            AssetDatabase.CreateAsset(material, MATERIAL_PATH);
            AssetDatabase.SaveAssets();

            Debug.Log($"[FloorPrefab] Created pixel art stone material: {MATERIAL_PATH}");

            return material;
        }

        /// <summary>
        /// Create a floor tile GameObject with proper dimensions and material.
        /// </summary>
        private static GameObject CreateFloorTile(Material material, float cellSize, float floorThickness)
        {
            // Create cube primitive (will be scaled to flat tile)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "FloorTilePrefab";

            // Remove collider (not needed for floor tiles)
            var collider = floor.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyImmediate(collider);
            }

            // Scale to cell size with 1 unit thickness
            floor.transform.localScale = new Vector3(cellSize, floorThickness, cellSize);

            // Get or add MeshRenderer
            var renderer = floor.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                renderer = floor.AddComponent<MeshRenderer>();
            }

            // Apply the pixel art stone material
            if (material != null)
            {
                renderer.sharedMaterial = material;
            }

            // Add MeshFilter if missing
            var filter = floor.GetComponent<MeshFilter>();
            if (filter == null)
            {
                filter = floor.AddComponent<MeshFilter>();
            }

            return floor;
        }

        [MenuItem("Tools/Maze/Generate Floor Prefab", true)]
        public static bool ValidateGenerateFloorPrefab()
        {
            // Always available
            return true;
        }
    }
}
#endif
