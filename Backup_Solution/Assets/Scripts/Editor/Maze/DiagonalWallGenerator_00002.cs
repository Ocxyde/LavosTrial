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
// DiagonalWallGenerator.cs
// Editor tool to generate diagonal wall and corner prefabs for 8-axis maze
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   1. Tools → Generate Diagonal Walls & Corners
//   2. Prefabs created in Assets/Resources/Prefabs/
//   3. Assign to CompleteMazeBuilder inspector
//
// FEATURES:
//   - Diagonal walls (45° rotated, fits cell diagonal)
//   - L-corner pieces (internal 90° corners)
//   - Triangle corner pieces (external 45° corners)
//   - All sized for 6m cells with 45° rotation

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// DiagonalWallGenerator - Creates diagonal wall and corner prefabs for 8-axis maze system.
    /// Generates prefabs that perfectly fit the diagonal connections in an 8-directional grid.
    /// </summary>
    public class DiagonalWallGenerator : EditorWindow
    {
        // Cell dimensions (from GameConfig)
        private const float CELL_SIZE = 6.0f;
        private const float WALL_HEIGHT = 4.0f;
        private const float WALL_THICKNESS = 0.5f;

        // Diagonal math: diagonal of 6m cell = 6 * sqrt(2) ≈ 8.485m
        private static readonly float DIAGONAL_LENGTH = CELL_SIZE * Mathf.Sqrt(2f);

        // Window instance
        private static DiagonalWallGenerator window;

        // Scroll position
        private Vector2 scrollPosition;

        // Material selection
        private Material wallMaterial;

        // Menu item
        [MenuItem("Tools/Generate Diagonal Walls & Corners")]
        public static void ShowWindow()
        {
            window = GetWindow<DiagonalWallGenerator>("Diagonal Wall Generator");
            window.minSize = new Vector2(450, 600);
            window.Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Header
            GUILayout.Space(10);
            DrawHeader();
            GUILayout.Space(20);

            // Material selection
            DrawMaterialSection();
            GUILayout.Space(20);

            // Generation buttons
            DrawGenerationButtons();
            GUILayout.Space(20);

            // Info section
            DrawInfoSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };

            EditorGUILayout.LabelField("8-AXIS DIAGONAL WALL GENERATOR", headerStyle);

            GUIStyle subHeaderStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };

            EditorGUILayout.LabelField(
                "Generates diagonal walls and corner pieces for 8-directional maze system.\n" +
                "All prefabs sized for 6m cells with proper 45° rotation.",
                subHeaderStyle
            );
        }

        private void DrawMaterialSection()
        {
            EditorGUILayout.LabelField("MATERIAL SETTINGS", EditorStyles.boldLabel);

            wallMaterial = (Material)EditorGUILayout.ObjectField(
                "Wall Material",
                wallMaterial,
                typeof(Material),
                false
            );

            if (wallMaterial == null)
            {
                // Try to find existing material
                string matPath = "Assets/Resources/Materials/WallMaterial.mat";
                if (System.IO.File.Exists(matPath + ".meta"))
                {
                    wallMaterial = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "No material assigned. Prefabs will use default white material.\n" +
                        "You can assign material later in Inspector.",
                        MessageType.Info
                    );
                }
            }
        }

        private void DrawGenerationButtons()
        {
            EditorGUILayout.LabelField("GENERATION", EditorStyles.boldLabel);

            // Generate all button
            GUI.backgroundColor = new Color(0.4f, 0.8f, 1f);
            if (GUILayout.Button("🎯 GENERATE ALL PREFABS", GUILayout.Height(40)))
            {
                GenerateAllPrefabs();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            // Individual generation buttons
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate Diagonal Walls", GUILayout.Height(30)))
            {
                GenerateDiagonalWalls();
            }

            if (GUILayout.Button("Generate L-Corners", GUILayout.Height(30)))
            {
                GenerateLCorners();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate Triangle Corners", GUILayout.Height(30)))
            {
                GenerateTriangleCorners();
            }

            if (GUILayout.Button("Generate All Corners", GUILayout.Height(30)))
            {
                GenerateLCorners();
                GenerateTriangleCorners();
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Cleanup button
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("🗑️ Delete All Diagonal Prefabs", GUILayout.Height(30)))
            {
                DeleteAllDiagonalPrefabs();
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawInfoSection()
        {
            EditorGUILayout.LabelField("INFORMATION", EditorStyles.boldLabel);

            GUIStyle infoStyle = new GUIStyle(EditorStyles.helpBox)
            {
                wordWrap = true,
                padding = new RectOffset(10, 10, 10, 10)
            };

            EditorGUILayout.BeginVertical(infoStyle);

            EditorGUILayout.LabelField("📐 DIAGONAL WALL SPECS:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"  • Cell Size: {CELL_SIZE}m");
            EditorGUILayout.LabelField($"  • Wall Height: {WALL_HEIGHT}m");
            EditorGUILayout.LabelField($"  • Wall Thickness: {WALL_THICKNESS}m");
            EditorGUILayout.LabelField($"  • Diagonal Length: {DIAGONAL_LENGTH:F3}m (6 × √2)");
            EditorGUILayout.LabelField($"  • Rotation: 45°, 135°, 225°, 315°");

            GUILayout.Space(10);

            EditorGUILayout.LabelField("🔷 L-CORNER SPECS:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("  • Internal 90° corners");
            EditorGUILayout.LabelField("  • Connects two cardinal walls");
            EditorGUILayout.LabelField("  • 4 variants: NE, NW, SE, SW");

            GUILayout.Space(10);

            EditorGUILayout.LabelField("🔺 TRIANGLE CORNER SPECS:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("  • External 45° corners");
            EditorGUILayout.LabelField("  • Caps diagonal wall ends");
            EditorGUILayout.LabelField("  • 8 variants for all diagonal directions");

            GUILayout.Space(10);

            EditorGUILayout.LabelField("📁 OUTPUT LOCATION:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("  Assets/Resources/Prefabs/");

            EditorGUILayout.EndVertical();
        }

        private void GenerateAllPrefabs()
        {
            GenerateDiagonalWalls();
            GenerateLCorners();
            GenerateTriangleCorners();

            EditorUtility.DisplayDialog(
                "Generation Complete",
                "All diagonal wall and corner prefabs have been generated!\n\n" +
                "Location: Assets/Resources/Prefabs/\n\n" +
                "Assign them to CompleteMazeBuilder:\n" +
                "  • wallDiagPrefab → DiagonalWallPrefab.prefab\n" +
                "  • wallCornerPrefab → LCornerPrefab.prefab (or TriangleCornerPrefab.prefab)",
                "OK"
            );
        }

        private void GenerateDiagonalWalls()
        {
            string prefabsFolder = "Assets/Resources/Prefabs";
            EnsureFolderExists(prefabsFolder);

            // Create diagonal wall prefab
            GameObject diagonalWall = CreateDiagonalWallPrefab();

            // Save prefab
            string prefabPath = Path.Combine(prefabsFolder, "DiagonalWallPrefab.prefab");
            PrefabUtility.SaveAsPrefabAsset(diagonalWall, prefabPath);

            // Cleanup scene object
            DestroyImmediate(diagonalWall);

            Debug.Log($"[DiagonalWallGenerator] Created: {prefabPath}");
            Debug.Log($"  - Size: {DIAGONAL_LENGTH:F3}m (diagonal) x {WALL_HEIGHT}m height");
            Debug.Log($"  - Rotation: 45° (NE-SW diagonal)");

            // Also create variants with different rotations
            CreateDiagonalVariant(prefabsFolder, "DiagonalWallPrefab_NE", 45f);
            CreateDiagonalVariant(prefabsFolder, "DiagonalWallPrefab_NW", -45f);
            CreateDiagonalVariant(prefabsFolder, "DiagonalWallPrefab_SE", -45f);
            CreateDiagonalVariant(prefabsFolder, "DiagonalWallPrefab_SW", 45f);

            AssetDatabase.Refresh();
        }

        private GameObject CreateDiagonalWallPrefab()
        {
            GameObject wall = new GameObject("DiagonalWallPrefab");

            // Create mesh
            MeshFilter meshFilter = wall.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = wall.AddComponent<MeshRenderer>();

            // Create diagonal wall mesh
            Mesh mesh = CreateDiagonalWallMesh();
            meshFilter.mesh = mesh;

            // Apply material
            if (wallMaterial != null)
            {
                meshRenderer.sharedMaterial = wallMaterial;
            }
            else
            {
                // Create default material
                Material defaultMat = new Material(Shader.Find("Standard"));
                defaultMat.color = new Color(0.6f, 0.4f, 0.3f); // Brownish
                meshRenderer.sharedMaterial = defaultMat;
            }

            // Add collider
            BoxCollider collider = wall.AddComponent<BoxCollider>();
            collider.size = new Vector3(DIAGONAL_LENGTH, WALL_HEIGHT, WALL_THICKNESS);

            return wall;
        }

        private Mesh CreateDiagonalWallMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "DiagonalWallMesh";

            // Diagonal wall dimensions
            float length = DIAGONAL_LENGTH;
            float height = WALL_HEIGHT;
            float thickness = WALL_THICKNESS;

            float lx = length / 2f;
            float hy = height / 2f;
            float tz = thickness / 2f;

            // Vertices (8 corners of the box)
            Vector3[] vertices = new Vector3[8];

            // Front face (4 corners)
            vertices[0] = new Vector3(-lx, -hy, tz);   // bottom-left
            vertices[1] = new Vector3(lx, -hy, tz);    // bottom-right
            vertices[2] = new Vector3(lx, hy, tz);     // top-right
            vertices[3] = new Vector3(-lx, hy, tz);    // top-left

            // Back face (4 corners)
            vertices[4] = new Vector3(-lx, -hy, -tz);  // bottom-left
            vertices[5] = new Vector3(lx, -hy, -tz);   // bottom-right
            vertices[6] = new Vector3(lx, hy, -tz);    // top-right
            vertices[7] = new Vector3(-lx, hy, -tz);   // top-left

            mesh.vertices = vertices;

            // Triangles (6 faces, 2 triangles each)
            int[] triangles = new int[36];

            // Front face
            triangles[0] = 0; triangles[1] = 1; triangles[2] = 2;
            triangles[3] = 0; triangles[4] = 2; triangles[5] = 3;

            // Back face
            triangles[6] = 5; triangles[7] = 4; triangles[8] = 7;
            triangles[9] = 5; triangles[10] = 7; triangles[11] = 6;

            // Left face
            triangles[12] = 4; triangles[13] = 0; triangles[14] = 3;
            triangles[15] = 4; triangles[16] = 3; triangles[17] = 7;

            // Right face
            triangles[18] = 1; triangles[19] = 5; triangles[20] = 6;
            triangles[21] = 1; triangles[22] = 6; triangles[23] = 2;

            // Top face
            triangles[24] = 3; triangles[25] = 2; triangles[26] = 6;
            triangles[27] = 3; triangles[28] = 6; triangles[29] = 7;

            // Bottom face
            triangles[30] = 4; triangles[31] = 5; triangles[32] = 1;
            triangles[33] = 4; triangles[34] = 1; triangles[35] = 0;

            mesh.triangles = triangles;

            // UVs (simple box mapping)
            Vector2[] uvs = new Vector2[8];
            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(1, 0);
            uvs[2] = new Vector2(1, 1);
            uvs[3] = new Vector2(0, 1);
            uvs[4] = new Vector2(0, 0);
            uvs[5] = new Vector2(1, 0);
            uvs[6] = new Vector2(1, 1);
            uvs[7] = new Vector2(0, 1);
            mesh.uv = uvs;

            // Normals and bounds
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        private void CreateDiagonalVariant(string folder, string name, float rotationY)
        {
            GameObject variant = new GameObject(name);

            // Create mesh
            MeshFilter meshFilter = variant.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = variant.AddComponent<MeshRenderer>();

            // Use same diagonal mesh
            Mesh mesh = CreateDiagonalWallMesh();
            meshFilter.mesh = mesh;

            // Apply material
            if (wallMaterial != null)
            {
                meshRenderer.sharedMaterial = wallMaterial;
            }
            else
            {
                Material defaultMat = new Material(Shader.Find("Standard"));
                defaultMat.color = new Color(0.6f, 0.4f, 0.3f);
                meshRenderer.sharedMaterial = defaultMat;
            }

            // Add collider
            BoxCollider collider = variant.AddComponent<BoxCollider>();
            collider.size = new Vector3(DIAGONAL_LENGTH, WALL_HEIGHT, WALL_THICKNESS);

            // Apply rotation
            variant.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);

            // Save prefab
            string prefabPath = Path.Combine(folder, $"{name}.prefab");
            PrefabUtility.SaveAsPrefabAsset(variant, prefabPath);

            // Cleanup
            DestroyImmediate(variant);

            Debug.Log($"[DiagonalWallGenerator] Created variant: {prefabPath} (rotation: {rotationY}°)");
        }

        private void GenerateLCorners()
        {
            string prefabsFolder = "Assets/Resources/Prefabs";
            EnsureFolderExists(prefabsFolder);

            // Create 4 L-corner variants (internal 90° corners)
            CreateLCornerVariant(prefabsFolder, "LCorner_NW", new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));
            CreateLCornerVariant(prefabsFolder, "LCorner_NE", new Vector3(0, 0, 0), Quaternion.Euler(0, 90, 0));
            CreateLCornerVariant(prefabsFolder, "LCorner_SE", new Vector3(0, 0, 0), Quaternion.Euler(0, 180, 0));
            CreateLCornerVariant(prefabsFolder, "LCorner_SW", new Vector3(0, 0, 0), Quaternion.Euler(0, 270, 0));

            AssetDatabase.Refresh();

            Debug.Log("[DiagonalWallGenerator] Created 4 L-Corner prefabs (internal 90° corners)");
        }

        private void CreateLCornerVariant(string folder, string name, Vector3 position, Quaternion rotation)
        {
            GameObject corner = new GameObject(name);

            // Create two wall segments at 90°
            GameObject wall1 = CreateWallSegment("Wall1", CELL_SIZE / 2f, WALL_HEIGHT, WALL_THICKNESS);
            GameObject wall2 = CreateWallSegment("Wall2", CELL_SIZE / 2f, WALL_HEIGHT, WALL_THICKNESS);

            // Position walls to form L-shape
            wall1.transform.localPosition = new Vector3(-CELL_SIZE / 4f, 0, 0);
            wall1.transform.localRotation = Quaternion.identity;

            wall2.transform.localPosition = new Vector3(0, 0, -CELL_SIZE / 4f);
            wall2.transform.localRotation = Quaternion.Euler(0, 90, 0);

            // Parent to corner
            wall1.transform.SetParent(corner.transform);
            wall2.transform.SetParent(corner.transform);

            // Apply material to children
            foreach (Transform child in corner.transform)
            {
                MeshRenderer renderer = child.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    if (wallMaterial != null)
                    {
                        renderer.sharedMaterial = wallMaterial;
                    }
                    else
                    {
                        Material defaultMat = new Material(Shader.Find("Standard"));
                        defaultMat.color = new Color(0.6f, 0.4f, 0.3f);
                        renderer.sharedMaterial = defaultMat;
                    }
                }
            }

            // Apply overall rotation
            corner.transform.rotation = rotation;

            // Combine meshes for single collider
            CombineMeshes(corner);

            // Save prefab
            string prefabPath = Path.Combine(folder, $"{name}.prefab");
            PrefabUtility.SaveAsPrefabAsset(corner, prefabPath);

            // Cleanup
            DestroyImmediate(corner);

            Debug.Log($"[DiagonalWallGenerator] Created L-Corner: {prefabPath}");
        }

        private GameObject CreateWallSegment(string name, float length, float height, float thickness)
        {
            GameObject wall = new GameObject(name);

            MeshFilter meshFilter = wall.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = wall.AddComponent<MeshRenderer>();

            Mesh mesh = CreateBoxMesh(length, height, thickness);
            meshFilter.mesh = mesh;

            BoxCollider collider = wall.AddComponent<BoxCollider>();
            collider.size = new Vector3(length, height, thickness);

            return wall;
        }

        private Mesh CreateBoxMesh(float width, float height, float depth)
        {
            Mesh mesh = new Mesh();
            mesh.name = "BoxMesh";

            float hx = width / 2f;
            float hy = height / 2f;
            float hz = depth / 2f;

            Vector3[] vertices = new Vector3[8];
            vertices[0] = new Vector3(-hx, -hy, hz);
            vertices[1] = new Vector3(hx, -hy, hz);
            vertices[2] = new Vector3(hx, hy, hz);
            vertices[3] = new Vector3(-hx, hy, hz);
            vertices[4] = new Vector3(-hx, -hy, -hz);
            vertices[5] = new Vector3(hx, -hy, -hz);
            vertices[6] = new Vector3(hx, hy, -hz);
            vertices[7] = new Vector3(-hx, hy, -hz);

            mesh.vertices = vertices;

            int[] triangles = new int[36];
            triangles[0] = 0; triangles[1] = 1; triangles[2] = 2; triangles[3] = 0; triangles[4] = 2; triangles[5] = 3;
            triangles[6] = 5; triangles[7] = 4; triangles[8] = 7; triangles[9] = 5; triangles[10] = 7; triangles[11] = 6;
            triangles[12] = 4; triangles[13] = 0; triangles[14] = 3; triangles[15] = 4; triangles[16] = 3; triangles[17] = 7;
            triangles[18] = 1; triangles[19] = 5; triangles[20] = 6; triangles[21] = 1; triangles[22] = 6; triangles[23] = 2;
            triangles[24] = 3; triangles[25] = 2; triangles[26] = 6; triangles[27] = 3; triangles[28] = 6; triangles[29] = 7;
            triangles[30] = 4; triangles[31] = 5; triangles[32] = 1; triangles[33] = 4; triangles[34] = 1; triangles[35] = 0;

            mesh.triangles = triangles;

            // UVs (simple box mapping)
            Vector2[] uvs = new Vector2[8];
            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(1, 0);
            uvs[2] = new Vector2(1, 1);
            uvs[3] = new Vector2(0, 1);
            uvs[4] = new Vector2(0, 0);
            uvs[5] = new Vector2(1, 0);
            uvs[6] = new Vector2(1, 1);
            uvs[7] = new Vector2(0, 1);
            mesh.uv = uvs;

            // Normals and bounds
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        private void CombineMeshes(GameObject obj)
        {
            // Combine child meshes into single mesh
            MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combines = new CombineInstance[meshFilters.Length];

            for (int i = 0; i < meshFilters.Length; i++)
            {
                combines[i].mesh = meshFilters[i].sharedMesh;
                combines[i].transform = meshFilters[i].transform.localToWorldMatrix;
            }

            MeshFilter combinedFilter = obj.GetComponent<MeshFilter>();
            if (combinedFilter == null)
            {
                combinedFilter = obj.AddComponent<MeshFilter>();
            }

            Mesh combinedMesh = new Mesh();
            combinedMesh.name = "CombinedMesh";
            combinedMesh.CombineMeshes(combines);
            combinedFilter.mesh = combinedMesh;

            // Add combined collider
            MeshCollider meshCollider = obj.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = combinedMesh;

            // Remove child mesh filters and colliders
            foreach (Transform child in obj.transform)
            {
                MeshFilter childFilter = child.GetComponent<MeshFilter>();
                MeshCollider childCollider = child.GetComponent<MeshCollider>();
                BoxCollider childBoxCollider = child.GetComponent<BoxCollider>();

                if (childFilter != null) DestroyImmediate(childFilter);
                if (childCollider != null) DestroyImmediate(childCollider);
                if (childBoxCollider != null) DestroyImmediate(childBoxCollider);
            }
        }

        private void GenerateTriangleCorners()
        {
            string prefabsFolder = "Assets/Resources/Prefabs";
            EnsureFolderExists(prefabsFolder);

            // Create 8 triangle corner variants (external 45° corners)
            string[] names = { "TriangleCorner_N", "TriangleCorner_NE", "TriangleCorner_E", "TriangleCorner_SE",
                              "TriangleCorner_S", "TriangleCorner_SW", "TriangleCorner_W", "TriangleCorner_NW" };
            float[] rotations = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };

            for (int i = 0; i < names.Length; i++)
            {
                CreateTriangleCornerVariant(prefabsFolder, names[i], rotations[i]);
            }

            AssetDatabase.Refresh();

            Debug.Log("[DiagonalWallGenerator] Created 8 Triangle Corner prefabs (external 45° corners)");
        }

        private void CreateTriangleCornerVariant(string folder, string name, float rotationY)
        {
            GameObject corner = new GameObject(name);

            // Create triangular prism mesh for 45° corner cap
            MeshFilter meshFilter = corner.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = corner.AddComponent<MeshRenderer>();

            Mesh mesh = CreateTriangleCornerMesh();
            meshFilter.mesh = mesh;

            // Apply material
            if (wallMaterial != null)
            {
                meshRenderer.sharedMaterial = wallMaterial;
            }
            else
            {
                Material defaultMat = new Material(Shader.Find("Standard"));
                defaultMat.color = new Color(0.6f, 0.4f, 0.3f);
                meshRenderer.sharedMaterial = defaultMat;
            }

            // Apply rotation
            corner.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);

            // Add mesh collider for triangular shape
            MeshCollider collider = corner.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;

            // Save prefab
            string prefabPath = Path.Combine(folder, $"{name}.prefab");
            PrefabUtility.SaveAsPrefabAsset(corner, prefabPath);

            // Cleanup
            DestroyImmediate(corner);

            Debug.Log($"[DiagonalWallGenerator] Created Triangle Corner: {prefabPath} (rotation: {rotationY}°)");
        }

        private Mesh CreateTriangleCornerMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "TriangleCornerMesh";

            // Triangle corner dimensions
            // Right triangle with legs = CELL_SIZE/2, hypotenuse = diagonal wall thickness
            float leg = CELL_SIZE / 2f;
            float height = WALL_HEIGHT;
            float hypotenuse = leg * Mathf.Sqrt(2f);

            // Vertices for triangular prism (6 vertices)
            Vector3[] vertices = new Vector3[6];

            // Bottom triangle
            vertices[0] = new Vector3(0, -height / 2f, 0);           // Right angle corner
            vertices[1] = new Vector3(leg, -height / 2f, 0);         // End of leg 1
            vertices[2] = new Vector3(0, -height / 2f, leg);         // End of leg 2

            // Top triangle
            vertices[3] = new Vector3(0, height / 2f, 0);            // Right angle corner
            vertices[4] = new Vector3(leg, height / 2f, 0);          // End of leg 1
            vertices[5] = new Vector3(0, height / 2f, leg);          // End of leg 2

            mesh.vertices = vertices;

            // Triangles (5 faces: 2 triangles + 3 rectangles)
            int[] triangles = new int[30];

            // Bottom triangle
            triangles[0] = 0; triangles[1] = 1; triangles[2] = 2;

            // Top triangle
            triangles[3] = 3; triangles[4] = 5; triangles[5] = 4;

            // Side face 1 (along leg 1)
            triangles[6] = 0; triangles[7] = 4; triangles[8] = 1;
            triangles[9] = 0; triangles[10] = 3; triangles[11] = 4;

            // Side face 2 (along leg 2)
            triangles[12] = 0; triangles[13] = 2; triangles[14] = 5;
            triangles[15] = 0; triangles[16] = 5; triangles[17] = 3;

            // Diagonal face (hypotenuse)
            triangles[18] = 1; triangles[19] = 4; triangles[20] = 5;
            triangles[21] = 1; triangles[22] = 5; triangles[23] = 2;

            mesh.triangles = triangles;

            // UVs for triangular prism (6 vertices)
            Vector2[] uvs = new Vector2[6];
            uvs[0] = new Vector2(0, 0);  // Bottom corner
            uvs[1] = new Vector2(1, 0);  // End of leg 1
            uvs[2] = new Vector2(0, 1);  // End of leg 2
            uvs[3] = new Vector2(0, 0);  // Top corner
            uvs[4] = new Vector2(1, 0);  // End of leg 1 top
            uvs[5] = new Vector2(0, 1);  // End of leg 2 top
            mesh.uv = uvs;

            // Normals and bounds
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        private void DeleteAllDiagonalPrefabs()
        {
            string prefabsFolder = "Assets/Resources/Prefabs";

            if (!Directory.Exists(prefabsFolder))
            {
                EditorUtility.DisplayDialog("No Prefabs Folder", "The prefabs folder does not exist.", "OK");
                return;
            }

            string[] files = Directory.GetFiles(prefabsFolder, "*.prefab");
            int deletedCount = 0;

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                if (fileName.Contains("Diagonal") || fileName.Contains("Corner"))
                {
                    AssetDatabase.DeleteAsset(file);
                    deletedCount++;
                }
            }

            AssetDatabase.Refresh();

            Debug.Log($"[DiagonalWallGenerator] Deleted {deletedCount} diagonal/corner prefabs");

            EditorUtility.DisplayDialog(
                "Cleanup Complete",
                $"Deleted {deletedCount} diagonal and corner prefabs.",
                "OK"
            );
        }

        private void EnsureFolderExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log($"[DiagonalWallGenerator] Created folder: {path}");
            }
        }
    }
}
#endif
