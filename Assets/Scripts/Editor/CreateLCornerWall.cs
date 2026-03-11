// Copyright (C) 2026 Ocxyde
// GPL-3.0 license - see COPYING
// CreateLCornerWall.cs - Editor tool to create L-shaped wall prefab

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Code.Lavos.Editor
{
    public static class CreateLCornerWall
    {
        private const string PREFAB_PATH = "Assets/Prefabs/Walls/L_Corner.prefab";
        private const float WALL_HEIGHT = 3f;
        private const float WALL_THICKNESS = 0.3f;
        private const float CELL_SIZE = 6f;

        [MenuItem("Tools/Lavos/Create L-Corner Wall Prefab")]
        public static void CreatePrefab()
        {
            // Create parent object
            GameObject lCorner = new GameObject("L_Corner");
            lCorner.tag = "Untagged";

            // Create wall materials (placeholder - untextured)
            Material placeholderMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            placeholderMat.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Grey placeholder

            // Wall 1 - North-South wall (6m long)
            GameObject wall1 = CreateWallSegment("Wall_NS", 
                new Vector3(0f, WALL_HEIGHT / 2f, 0f), 
                Quaternion.identity, 
                CELL_SIZE, WALL_HEIGHT, WALL_THICKNESS,
                placeholderMat);
            wall1.transform.SetParent(lCorner.transform, false);

            // Wall 2 - East-West wall (6m long), connected to Wall 1
            GameObject wall2 = CreateWallSegment("Wall_EW",
                new Vector3(0f, WALL_HEIGHT / 2f, 0f),
                Quaternion.Euler(0f, 90f, 0f),
                CELL_SIZE, WALL_HEIGHT, WALL_THICKNESS,
                placeholderMat);
            wall2.transform.SetParent(lCorner.transform, false);

            // Position wall2 to form L-shape (connected at corner)
            wall2.transform.localPosition = new Vector3(-CELL_SIZE / 2f, WALL_HEIGHT / 2f, CELL_SIZE / 2f);

            // Create prefab
            EnsureFolderExists("Assets/Prefabs/Walls");
            
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(lCorner, PREFAB_PATH);
            
            Debug.Log($"[LCorner]  Created prefab: {PREFAB_PATH}");
            
            // Clean up scene object
            Object.DestroyImmediate(lCorner);
        }

        private static GameObject CreateWallSegment(string name, Vector3 position, Quaternion rotation, 
            float length, float height, float thickness, Material material)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            
            // Set scale: length (X), height (Y), thickness (Z)
            wall.transform.localScale = new Vector3(length, height, thickness);
            wall.transform.position = position;
            wall.transform.rotation = rotation;

            // Apply placeholder material
            MeshRenderer renderer = wall.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            // Remove collider (walls will use mesh collider or custom collision)
            Collider collider = wall.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            return wall;
        }

        private static void EnsureFolderExists(string folderPath)
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
