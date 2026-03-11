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
// SimpleDiagonalWallFactory.cs
// Creates simple diagonal wall prefab from cube primitive (not quad)
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   1. Tools  Create Simple Diagonal Wall Prefab
//   2. Prefab created at Assets/Resources/Prefabs/DiagonalWallPrefab.prefab
//   3. CompleteMazeBuilder auto-loads it as wallDiagPrefab

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using Code.Lavos.Core;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// SimpleDiagonalWallFactory - Creates diagonal wall prefab using cube primitive.
    /// Uses a simple cube rotated 45 with scale (1, 1, 0.5).
    /// Output: DiagonalWallPrefab.prefab (replaces existing if present)
    /// </summary>
    public class SimpleDiagonalWallFactory : EditorWindow
    {
        // Prefab settings - User requested: (1, 1, 0.5)
        private Vector3 cubeScale = new Vector3(1f, 1f, 0.5f);
        private float rotationY = 45f;

        // Material
        private Material wallMaterial;

        // Window instance
        private static SimpleDiagonalWallFactory window;

        // Prefab name (fixed - matches CompleteMazeBuilder expectation)
        private const string PREFAB_NAME = "DiagonalWallPrefab";

        [MenuItem("Tools/Create Simple Diagonal Wall Prefab")]
        public static void ShowWindow()
        {
            window = GetWindow<SimpleDiagonalWallFactory>("Simple Diagonal Wall");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);

            // Header
            EditorGUILayout.LabelField("SIMPLE DIAGONAL WALL FACTORY", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Creates diagonal wall from cube primitive", EditorStyles.miniLabel);

            GUILayout.Space(15);

            // Scale settings
            EditorGUILayout.LabelField("Cube Scale", EditorStyles.boldLabel);
            cubeScale = EditorGUILayout.Vector3Field("Scale (X, Y, Z)", cubeScale);

            // Rotation
            EditorGUILayout.LabelField("Rotation", EditorStyles.boldLabel);
            rotationY = EditorGUILayout.Slider("Y Rotation (degrees)", rotationY, 0f, 360f);

            GUILayout.Space(15);

            // Material
            EditorGUILayout.LabelField("Material", EditorStyles.boldLabel);
            wallMaterial = (Material)EditorGUILayout.ObjectField(
                "Wall Material", wallMaterial, typeof(Material), false);

            GUILayout.Space(20);

            // Create button
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("Create Diagonal Wall Prefab", GUILayout.Height(40)))
            {
                CreateDiagonalWallPrefab();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            // Info
            EditorGUILayout.HelpBox(
                "This creates a simple diagonal wall prefab using a cube primitive.\n" +
                "The cube is rotated 45 on Y axis and scaled to (1, 1, 0.5).\n" +
                "Output: Assets/Resources/Prefabs/SimpleDiagonalWallPrefab.prefab",
                MessageType.Info);
        }

        private void CreateDiagonalWallPrefab()
        {
            string prefabsFolder = "Assets/Resources/Prefabs";
            EnsureFolderExists(prefabsFolder);

            // Create empty GameObject
            GameObject wall = new GameObject(PREFAB_NAME);

            // Add cube primitive (NOT quad - user requested cube)
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "CubeMesh";
            cube.transform.SetParent(wall.transform, false);

            // Apply scale (1, 1, 0.5) as requested
            cube.transform.localScale = cubeScale;

            // Apply rotation (45 on Y axis for diagonal)
            cube.transform.localRotation = Quaternion.Euler(0f, rotationY, 0f);

            // Apply material
            Renderer renderer = cube.GetComponent<Renderer>();
            if (wallMaterial != null)
            {
                renderer.sharedMaterial = wallMaterial;
            }
            else
            {
                // Try to load wall material from GameConfig
                var gameConfig = GameConfig.Instance;
                if (gameConfig != null)
                {
                    // Load material from Resources (using path from GameConfig)
                    wallMaterial = Resources.Load<Material>(gameConfig.WallMaterial);
                    
                    // If material not found, try to load texture and create material
                    if (wallMaterial == null)
                    {
                        Texture2D wallTexture = Resources.Load<Texture2D>(gameConfig.WallTexture);
                        if (wallTexture != null)
                        {
                            // Create material with texture
                            Shader standardShader = Shader.Find("Standard");
                            wallMaterial = new Material(standardShader);
                            wallMaterial.mainTexture = wallTexture;
                            wallMaterial.color = Color.white;
                            
                            // Save the new material
                            string matPath = "Assets/Resources/Materials/DiagonalWallMaterial.mat";
                            EnsureFolderExists("Assets/Resources/Materials");
                            
                            if (AssetDatabase.LoadAssetAtPath<Material>(matPath) != null)
                            {
                                AssetDatabase.DeleteAsset(matPath);
                            }
                            AssetDatabase.CreateAsset(wallMaterial, matPath);
                            AssetDatabase.SaveAssets();
                            
                            Debug.Log($"[SimpleDiagonalWallFactory] Created material with texture: {matPath}");
                        }
                    }
                }

                // Apply material (loaded or null)
                if (wallMaterial != null)
                {
                    renderer.sharedMaterial = wallMaterial;
                    Debug.Log($"[SimpleDiagonalWallFactory] Applied material: {wallMaterial.name}");
                }
                else
                {
                    // Fallback: Create default material with color
                    Material defaultMat = new Material(Shader.Find("Standard"));
                    defaultMat.color = new Color(0.6f, 0.4f, 0.3f); // Brownish wall color
                    renderer.sharedMaterial = defaultMat;
                    Debug.LogWarning("[SimpleDiagonalWallFactory] No wall material/texture found, using default color");
                }
            }

            // Ensure collider exists (cube already has one)
            // The BoxCollider is already on the cube, sized to the scaled mesh

            // Save as prefab (overwrite if exists)
            string prefabPath = Path.Combine(prefabsFolder, $"{PREFAB_NAME}.prefab");
            PrefabUtility.SaveAsPrefabAsset(wall, prefabPath);

            // Cleanup scene object
            DestroyImmediate(wall);

            Debug.Log($"[SimpleDiagonalWallFactory] Created: {prefabPath}");
            Debug.Log($"  - Cube Scale: {cubeScale}");
            Debug.Log($"  - Rotation: {rotationY} on Y axis");
            Debug.Log($"  - Uses cube primitive (not quad)");
            Debug.Log($"  - Name: {PREFAB_NAME} (ready for CompleteMazeBuilder)");

            EditorUtility.DisplayDialog(
                "Prefab Created",
                $"Diagonal wall prefab created successfully!\n\n" +
                $"Location: {prefabPath}\n\n" +
                $"This prefab is ready to use!\n" +
                $"CompleteMazeBuilder will auto-load it as 'wallDiagPrefab'.",
                "OK"
            );

            AssetDatabase.Refresh();
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
