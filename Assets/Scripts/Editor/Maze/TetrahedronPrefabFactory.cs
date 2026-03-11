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
// TetrahedronPrefabFactory.cs
// Creates 2 tetrahedron prefabs with 8-bit pixel art textures
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   1. Tools  Create Tetrahedron Prefabs (8-bit Pixel Art)
//   2. Prefabs created at Assets/Resources/Prefabs/
//   3. Both share same texture & material

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// TetrahedronPrefabFactory - Creates 2 tetrahedron prefabs with 8-bit pixel art style.
    /// Both prefabs share the same texture and material.
    /// </summary>
    public class TetrahedronPrefabFactory : EditorWindow
    {
        // Prefab names
        private const string PREFAB_1_NAME = "Tetrahedron_Origin";
        private const string PREFAB_2_NAME = "Tetrahedron_Variant";

        // Transform settings for Prefab 1
        private Vector3 prefab1Position = Vector3.zero;
        private Vector3 prefab1Scale = Vector3.one;
        private Vector3 prefab1Rotation = new Vector3(-22.5f, -180f, -22.5f);

        // Transform settings for Prefab 2
        private Vector3 prefab2Position = new Vector3(2f, 0f, 0f);
        private Vector3 prefab2Scale = Vector3.one;
        private Vector3 prefab2Rotation = Vector3.zero;

        // Texture settings
        private int textureSize = 64; // 64x64 for 8-bit look
        private Color[] palette8bit;

        // Material
        private Material sharedMaterial;

        // Window instance
        private static TetrahedronPrefabFactory window;

        // Scroll position
        private Vector2 scrollPosition;

        [MenuItem("Tools/Create Tetrahedron Prefabs (8-bit Pixel Art)")]
        public static void ShowWindow()
        {
            window = GetWindow<TetrahedronPrefabFactory>("Tetrahedron Factory");
            window.minSize = new Vector2(450, 550);
            window.Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(10);

            // Header
            DrawHeader();
            GUILayout.Space(15);

            // Prefab 1 settings
            DrawPrefab1Settings();
            GUILayout.Space(15);

            // Prefab 2 settings
            DrawPrefab2Settings();
            GUILayout.Space(15);

            // Texture settings
            DrawTextureSettings();
            GUILayout.Space(20);

            // Create button
            DrawCreateButton();
            GUILayout.Space(15);

            // Info
            DrawInfo();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("TETRAHEDRON PREFAB FACTORY", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Creates 2 tetrahedrons with shared 8-bit pixel art material", EditorStyles.miniLabel);
        }

        private void DrawPrefab1Settings()
        {
            EditorGUILayout.LabelField("PREFAB 1 - Origin", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            prefab1Position = EditorGUILayout.Vector3Field("Position", prefab1Position);
            prefab1Rotation = EditorGUILayout.Vector3Field("Rotation (Euler)", prefab1Rotation);
            prefab1Scale = EditorGUILayout.Vector3Field("Scale", prefab1Scale);
            EditorGUI.indentLevel--;
        }

        private void DrawPrefab2Settings()
        {
            EditorGUILayout.LabelField("PREFAB 2 - Variant", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            prefab2Position = EditorGUILayout.Vector3Field("Position", prefab2Position);
            prefab2Rotation = EditorGUILayout.Vector3Field("Rotation (Euler)", prefab2Rotation);
            prefab2Scale = EditorGUILayout.Vector3Field("Scale", prefab2Scale);
            EditorGUI.indentLevel--;
        }

        private void DrawTextureSettings()
        {
            EditorGUILayout.LabelField("8-BIT PIXEL ART TEXTURE", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            textureSize = EditorGUILayout.IntField("Texture Size (pixels)", textureSize);
            EditorGUILayout.HelpBox("Texture will be generated with 8-bit color palette (256 colors max)", MessageType.Info);
            EditorGUI.indentLevel--;
        }

        private void DrawCreateButton()
        {
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("Create Tetrahedron Prefabs", GUILayout.Height(40)))
            {
                CreateTetrahedronPrefabs();
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawInfo()
        {
            EditorGUILayout.HelpBox(
                "Creates 2 tetrahedron prefabs:\n" +
                $"   {PREFAB_1_NAME} - At origin with rotation (-22.5, -180, -22.5)\n" +
                $"   {PREFAB_2_NAME} - Variant at (2, 0, 0)\n\n" +
                "Both share:\n" +
                "   Same 8-bit pixel art texture (64x64)\n" +
                "   Same material (Standard shader)\n\n" +
                "Output:\n" +
                "  Assets/Resources/Prefabs/\n" +
                "  Assets/Resources/Materials/\n" +
                "  Assets/Resources/Textures/",
                MessageType.Info);
        }

        private void CreateTetrahedronPrefabs()
        {
            // Ensure folders exist
            EnsureFolderExists("Assets/Resources");
            EnsureFolderExists("Assets/Resources/Prefabs");
            EnsureFolderExists("Assets/Resources/Materials");
            EnsureFolderExists("Assets/Resources/Textures");

            // Generate 8-bit pixel art texture
            Texture2D texture = Generate8BitPixelArtTexture();
            string texturePath = "Assets/Resources/Textures/Tetrahedron_8BitTexture.png";
            SaveTexture(texture, texturePath);

            // Create shared material
            Shader shader = Shader.Find("Standard");
            sharedMaterial = new Material(shader);
            sharedMaterial.mainTexture = texture;
            sharedMaterial.color = Color.white;
            sharedMaterial.name = "Tetrahedron_8BitMaterial";

            string matPath = "Assets/Resources/Materials/Tetrahedron_8BitMaterial.mat";
            SaveMaterial(sharedMaterial, matPath);

            // Create Prefab 1 (Origin with rotation)
            GameObject prefab1 = CreateTetrahedron(PREFAB_1_NAME, prefab1Position, prefab1Rotation, prefab1Scale, sharedMaterial);
            string prefab1Path = $"Assets/Resources/Prefabs/{PREFAB_1_NAME}.prefab";
            PrefabUtility.SaveAsPrefabAsset(prefab1, prefab1Path);
            DestroyImmediate(prefab1);

            // Create Prefab 2 (Variant)
            GameObject prefab2 = CreateTetrahedron(PREFAB_2_NAME, prefab2Position, prefab2Rotation, prefab2Scale, sharedMaterial);
            string prefab2Path = $"Assets/Resources/Prefabs/{PREFAB_2_NAME}.prefab";
            PrefabUtility.SaveAsPrefabAsset(prefab2, prefab2Path);
            DestroyImmediate(prefab2);

            // Cleanup
            DestroyImmediate(texture);

            Debug.Log("[TetrahedronFactory] Created 2 tetrahedron prefabs with 8-bit pixel art!");
            Debug.Log($"  Texture: {texturePath}");
            Debug.Log($"  Material: {matPath}");
            Debug.Log($"  Prefab 1: {prefab1Path}");
            Debug.Log($"  Prefab 2: {prefab2Path}");
            Debug.Log($"  Both prefabs share the same texture and material");

            EditorUtility.DisplayDialog(
                "Tetrahedrons Created!",
                $"2 tetrahedron prefabs created successfully!\n\n" +
                $"Both share:\n" +
                $"   8-bit pixel art texture ({textureSize}x{textureSize})\n" +
                $"   Same material\n\n" +
                $"Locations:\n" +
                $"  {prefab1Path}\n" +
                $"  {prefab2Path}",
                "OK"
            );

            AssetDatabase.Refresh();
        }

        private GameObject CreateTetrahedron(string name, Vector3 position, Vector3 rotation, Vector3 scale, Material material)
        {
            // Create parent GameObject
            GameObject tetrahedron = new GameObject(name);
            tetrahedron.transform.position = position;
            tetrahedron.transform.rotation = Quaternion.Euler(rotation);
            tetrahedron.transform.localScale = scale;

            // Add MeshFilter
            MeshFilter meshFilter = tetrahedron.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateTetrahedronMesh();

            // Add MeshRenderer
            MeshRenderer meshRenderer = tetrahedron.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;

            // Add MeshCollider (for proper collision)
            MeshCollider meshCollider = tetrahedron.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = meshFilter.mesh;
            meshCollider.convex = true;

            return tetrahedron;
        }

        private Mesh CreateTetrahedronMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "TetrahedronMesh";

            // Tetrahedron vertices (regular tetrahedron centered at origin)
            // Edge length = sqrt(8/3)  1.633 for unit sphere inscribed
            float size = 1f;
            float h = size * Mathf.Sqrt(2f / 3f); // Height from base to apex
            float r = size * Mathf.Sqrt(1f / 3f); // Radius of base

            Vector3[] vertices = new Vector3[4];
            vertices[0] = new Vector3(0f, h, 0f);                    // Apex (top)
            vertices[1] = new Vector3(-r, -h / 3f, r);               // Base vertex 1
            vertices[2] = new Vector3(r, -h / 3f, r);                // Base vertex 2
            vertices[3] = new Vector3(0f, -h / 3f, -r * 2f);         // Base vertex 3

            mesh.vertices = vertices;

            // Triangles (4 faces, 3 vertices each, clockwise winding)
            int[] triangles = new int[12];
            triangles[0] = 0; triangles[1] = 1; triangles[2] = 2;    // Front face
            triangles[3] = 0; triangles[4] = 2; triangles[5] = 3;    // Right face
            triangles[6] = 0; triangles[7] = 3; triangles[8] = 1;    // Left face
            triangles[9] = 1; triangles[10] = 3; triangles[11] = 2;  // Bottom face

            mesh.triangles = triangles;

            // Normals (for proper lighting)
            mesh.RecalculateNormals();

            // Tangents (for normal mapping if needed)
            mesh.RecalculateTangents();

            // UVs (for texture mapping)
            Vector2[] uvs = new Vector2[4];
            uvs[0] = new Vector2(0.5f, 1f);      // Apex
            uvs[1] = new Vector2(0f, 0f);        // Base 1
            uvs[2] = new Vector2(1f, 0f);        // Base 2
            uvs[3] = new Vector2(0.5f, 0f);      // Base 3

            mesh.uv = uvs;

            // Bounds (for frustum culling)
            mesh.RecalculateBounds();

            return mesh;
        }

        private Texture2D Generate8BitPixelArtTexture()
        {
            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGB565, false);
            texture.filterMode = FilterMode.Point; // Pixel-perfect (no filtering)
            texture.wrapMode = TextureWrapMode.Clamp;

            // Generate 8-bit color palette (retro game style)
            Initialize8BitPalette();

            // Fill texture with pixel art pattern
            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    // Create geometric 8-bit pattern (pyramid/triangle pattern)
                    int cx = textureSize / 2;
                    int cy = textureSize / 2;
                    int dx = Mathf.Abs(x - cx);
                    int dy = Mathf.Abs(y - cy);
                    int dist = dx + dy;

                    // Quantize to 8-bit bands
                    int band = dist / (textureSize / 8);
                    Color color = GetPaletteColor(band % palette8bit.Length);

                    // Add checkerboard pattern for retro feel
                    if ((x / 8 + y / 8) % 2 == 0)
                    {
                        color *= 1.1f; // Brighter
                    }
                    else
                    {
                        color *= 0.9f; // Darker
                    }

                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return texture;
        }

        private void Initialize8BitPalette()
        {
            // Classic 8-bit / 16-bit era color palette (256 colors max)
            // Using a subset of 16 colors for retro aesthetic
            palette8bit = new Color[]
            {
                new Color32(0, 0, 0, 255),       // Black
                new Color32(28, 28, 28, 255),    // Dark gray
                new Color32(68, 68, 68, 255),    // Medium gray
                new Color32(128, 128, 128, 255), // Light gray
                new Color32(188, 188, 188, 255), // Very light gray
                new Color32(255, 255, 255, 255), // White

                new Color32(136, 0, 0, 255),     // Red
                new Color32(204, 68, 68, 255),   // Light red
                new Color32(0, 102, 0, 255),     // Green
                new Color32(68, 188, 68, 255),   // Light green
                new Color32(0, 0, 136, 255),     // Blue
                new Color32(68, 68, 204, 255),   // Light blue

                new Color32(136, 136, 0, 255),   // Yellow-brown
                new Color32(204, 204, 68, 255),  // Yellow
                new Color32(136, 0, 136, 255),   // Purple
                new Color32(204, 68, 204, 255),  // Light purple
            };
        }

        private Color GetPaletteColor(int index)
        {
            if (palette8bit == null) Initialize8BitPalette();
            index = Mathf.Clamp(index, 0, palette8bit.Length - 1);
            return palette8bit[index];
        }

        private void SaveTexture(Texture2D texture, string path)
        {
            // Encode to PNG
            byte[] pngData = texture.EncodeToPNG();

            // Write to file
            File.WriteAllBytes(path, pngData);

            // Import settings for pixel art
            AssetDatabase.ImportAsset(path);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.filterMode = FilterMode.Point; // Pixel-perfect
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.maxTextureSize = textureSize;
                importer.SaveAndReimport();
            }

            Debug.Log($"[TetrahedronFactory] Saved texture: {path}");
        }

        private void SaveMaterial(Material material, string path)
        {
            // Delete existing if present
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            AssetDatabase.CreateAsset(material, path);
            AssetDatabase.SaveAssets();
            Debug.Log($"[TetrahedronFactory] Saved material: {path}");
        }

        private void EnsureFolderExists(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                AssetDatabase.Refresh();
            }
        }
    }
}
#endif
