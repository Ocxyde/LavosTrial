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
// QuickSetupPrefabs.cs
// Quick editor tool to create minimum required prefabs for testing
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   1. Tools → Quick Setup Prefabs
//   2. Prefabs created in Assets/Resources/Prefabs/
//   3. Test maze generation (Ctrl+Alt+G)

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Code.Lavos.Editor
{
    public class QuickSetupPrefabs : MonoBehaviour
    {
        [MenuItem("Tools/Quick Setup Prefabs (For Testing)")]
        public static void SetupMinimumPrefabs()
        {
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  QUICK SETUP - Minimum Required Prefabs");
            Debug.Log("═══════════════════════════════════════════");

            // Create Resources/Prefabs folder
            string prefabsFolder = "Assets/Resources/Prefabs";
            
            if (!Directory.Exists(prefabsFolder))
            {
                Directory.CreateDirectory(prefabsFolder);
                Debug.Log($"✅ Created: {prefabsFolder}");
            }

            // Create Wall Prefab
            CreateWallPrefab(prefabsFolder);

            // Create Torch Prefab
            CreateTorchPrefab(prefabsFolder);

            // Create Door Prefab
            CreateDoorPrefab(prefabsFolder);

            // Create Materials folder
            string materialsFolder = "Assets/Resources/Materials";
            if (!Directory.Exists(materialsFolder))
            {
                Directory.CreateDirectory(materialsFolder);
                Debug.Log($"✅ Created: {materialsFolder}");
            }

            // Create Wall Material
            CreateWallMaterial(materialsFolder);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Auto-assign prefabs to CompleteMazeBuilder if it exists in scene
            AutoAssignPrefabsToBuilder();

            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  ✅ QUICK SETUP COMPLETE!");
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("");
            Debug.Log("📁 Prefabs created in: Assets/Resources/Prefabs/");
            Debug.Log("📁 Materials created in: Assets/Resources/Materials/");
            Debug.Log("🔌 Prefabs auto-assigned to CompleteMazeBuilder");
            Debug.Log("");
            Debug.Log("▶️  Now test: Press Ctrl+Alt+G to generate maze");
            Debug.Log("═══════════════════════════════════════════");
        }

        /// <summary>
        /// Auto-assign created prefabs to CompleteMazeBuilder in scene.
        /// </summary>
        private static void AutoAssignPrefabsToBuilder()
        {
            var builder = Object.FindFirstObjectByType<Code.Lavos.Core.CompleteMazeBuilder>();
            
            if (builder == null)
            {
                Debug.Log("⚠️  No CompleteMazeBuilder in scene - prefabs not auto-assigned");
                return;
            }

            // Load prefabs
            var wallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/WallPrefab.prefab");
            var torchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/TorchHandlePrefab.prefab");
            var doorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/DoorPrefab.prefab");
            var wallMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Materials/WallMaterial.mat");

            // Get serialized object for editing
            var serializedObject = new SerializedObject(builder);

            // Assign wall prefab
            if (wallPrefab != null)
            {
                var wallPrefabProp = serializedObject.FindProperty("wallPrefab");
                if (wallPrefabProp != null)
                {
                    wallPrefabProp.objectReferenceValue = wallPrefab;
                    Debug.Log("✅ Auto-assigned: WallPrefab");
                }
            }

            // Assign torch prefab
            if (torchPrefab != null)
            {
                var torchPrefabProp = serializedObject.FindProperty("torchPrefab");
                if (torchPrefabProp != null)
                {
                    torchPrefabProp.objectReferenceValue = torchPrefab;
                    Debug.Log("✅ Auto-assigned: TorchPrefab");
                }
            }

            // Assign door prefab
            if (doorPrefab != null)
            {
                var doorPrefabProp = serializedObject.FindProperty("doorPrefab");
                if (doorPrefabProp != null)
                {
                    doorPrefabProp.objectReferenceValue = doorPrefab;
                    Debug.Log("✅ Auto-assigned: DoorPrefab");
                }
            }

            // Assign wall material
            if (wallMaterial != null)
            {
                var wallMaterialProp = serializedObject.FindProperty("wallMaterial");
                if (wallMaterialProp != null)
                {
                    wallMaterialProp.objectReferenceValue = wallMaterial;
                    Debug.Log("✅ Auto-assigned: WallMaterial");
                }
            }

            serializedObject.ApplyModifiedProperties();
            Debug.Log("✅ All prefabs auto-assigned to CompleteMazeBuilder");
        }

        private static void CreateWallPrefab(string folder)
        {
            string prefabPath = $"{folder}/WallPrefab.prefab";

            // Check if already exists
            if (File.Exists(prefabPath))
            {
                Debug.Log($"✅ WallPrefab already exists");
                return;
            }

            // Create simple wall cube
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall";
            wall.transform.localScale = new Vector3(6f, 4f, 0.5f);

            // Remove collider (not needed for maze walls)
            var collider = wall.GetComponent<BoxCollider>();
            if (collider != null)
                Object.DestroyImmediate(collider);

            // Apply material
            ApplyWallMaterial(wall);

            // Save as prefab
            PrefabUtility.SaveAsPrefabAsset(wall, prefabPath);
            Object.DestroyImmediate(wall);

            Debug.Log($"✅ Created: WallPrefab.prefab (6x4x0.5m)");
        }

        private static void CreateTorchPrefab(string folder)
        {
            string prefabPath = $"{folder}/TorchHandlePrefab.prefab";

            // Check if already exists
            if (File.Exists(prefabPath))
            {
                Debug.Log($"✅ TorchHandlePrefab already exists");
                return;
            }

            // Create simple torch (cube for handle)
            GameObject torch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            torch.name = "TorchHandle";
            torch.transform.localScale = new Vector3(0.2f, 0.8f, 0.2f);

            // Remove collider
            var collider = torch.GetComponent<BoxCollider>();
            if (collider != null)
                Object.DestroyImmediate(collider);

            // Add point light
            Light light = torch.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.8f, 0.5f);
            light.range = 8f;
            light.intensity = 1f;

            // Save as prefab
            PrefabUtility.SaveAsPrefabAsset(torch, prefabPath);
            Object.DestroyImmediate(torch);

            Debug.Log($"✅ Created: TorchHandlePrefab.prefab (with light)");
        }

        private static void CreateDoorPrefab(string folder)
        {
            string prefabPath = $"{folder}/DoorPrefab.prefab";

            // Check if already exists
            if (File.Exists(prefabPath))
            {
                Debug.Log($"✅ DoorPrefab already exists");
                return;
            }

            // Create simple door (cube for door panel)
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Door";
            door.transform.localScale = new Vector3(0.5f, 4f, 5.4f);

            // Remove collider
            var collider = door.GetComponent<BoxCollider>();
            if (collider != null)
                Object.DestroyImmediate(collider);

            // Save as prefab
            PrefabUtility.SaveAsPrefabAsset(door, prefabPath);
            Object.DestroyImmediate(door);

            Debug.Log($"✅ Created: DoorPrefab.prefab (0.5x4x5.4m)");
        }

        private static void CreateWallMaterial(string folder)
        {
            string materialPath = $"{folder}/WallMaterial.mat";

            // Check if already exists
            if (File.Exists(materialPath))
            {
                Debug.Log($"✅ WallMaterial already exists");
                return;
            }

            // Create simple material
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            Material mat = new Material(shader);
            mat.color = new Color(0.6f, 0.55f, 0.5f); // Stone gray
            mat.SetFloat("_Smoothness", 0.3f);
            mat.SetFloat("_Metallic", 0f);

            AssetDatabase.CreateAsset(mat, materialPath);

            Debug.Log($"✅ Created: WallMaterial.mat (URP Lit, stone gray)");
        }

        private static void ApplyWallMaterial(GameObject obj)
        {
            // Try to load wall material
            Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Materials/WallMaterial.mat");

            if (mat == null)
            {
                // Create fallback material
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                mat = new Material(shader);
                mat.color = new Color(0.6f, 0.55f, 0.5f);
            }

            var renderer = obj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = mat;
            }
        }
    }
}
#endif
