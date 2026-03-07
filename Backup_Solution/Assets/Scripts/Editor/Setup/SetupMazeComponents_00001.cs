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
//
// SetupMazeComponents.cs
// Auto-setup scene using GameConfig-default.json paths
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// NO HARDCODED VALUES - All paths from GameConfig.Instance
// PLUG-IN-OUT: Finds components, assigns prefabs from config
//
// USAGE:
//   Tools → Setup Maze Components
//   Reads paths from Config/GameConfig-default.json
//   Creates prefabs ONLY if missing at config paths
//

using UnityEngine;
using UnityEditor;
using System.IO;
using Code.Lavos.Core;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// SetupMazeComponents - Auto-setup scene using GameConfig paths.
    /// NO HARDCODED VALUES - All from JSON config.
    /// </summary>
    public class SetupMazeComponents : EditorWindow
    {
        [MenuItem("Tools/Maze/Setup Maze Components")]
        public static void SetupMazeComponentsMenu()
        {
            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  SETUP MAZE COMPONENTS (From JSON Config)");
            Debug.Log("═══════════════════════════════════════════");

            // Load config FIRST - all paths from here
            var config = GameConfig.Instance;

            Debug.Log($"[Setup]  Config loaded from Config/GameConfig-default.json");
            Debug.Log($"[Setup]    Wall Prefab: {config.wallPrefab}");
            Debug.Log($"[Setup]    Door Prefab: {config.doorPrefab}");
            Debug.Log($"[Setup]    Torch Prefab: {config.torchPrefab}");
            Debug.Log($"[Setup]    Wall Material: {config.wallMaterial}");
            Debug.Log($"[Setup]    Floor Material: {config.floorMaterial}");

            // Ensure folders exist
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Prefabs");
            EnsureFolder("Assets/Materials");
            EnsureFolder("Assets/Materials/Floor");
            EnsureFolder("Assets/Textures");

            // Create prefabs ONLY if missing at config paths
            CreatePrefabIfMissing(config.wallPrefab, "Wall");
            CreatePrefabIfMissing(config.doorPrefab, "Door");
            CreateTorchPrefabIfMissing(config.torchPrefab);

            // Create materials ONLY if missing at config paths
            CreateMaterialIfMissing(config.wallMaterial, "WallMaterial");
            CreateMaterialIfMissing(config.floorMaterial, "Floor/Stone_Floor");

            // Create textures ONLY if missing at config paths
            CreateTextureIfMissing(config.groundTexture, "floor_texture");
            CreateTextureIfMissing(config.wallTexture, "wall_texture");

            // Find or create components and ASSIGN from config
            EnsureTorchPool(config);
            EnsureLightPlacementEngine();
            EnsureCompleteMazeBuilder(config);

            Debug.Log("═══════════════════════════════════════════");
            Debug.Log("  SETUP COMPLETE!");
            Debug.Log("  All prefabs/materials created from config paths");
            Debug.Log("  Press Ctrl+Alt+G to generate maze");
            Debug.Log("═══════════════════════════════════════════");
        }

        private static void EnsureFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log($"[Setup]  Created folder: {path}");
            }
        }

        private static void CreatePrefabIfMissing(string configPath, string primitiveType)
        {
            // Convert config path to asset path
            string assetPath = $"Assets/{configPath}";
            
            if (AssetDatabase.LoadAssetAtPath<GameObject>(assetPath) != null)
            {
                Debug.Log($"[Setup]  Prefab exists: {assetPath}");
                return;
            }

            Debug.Log($"[Setup]  Creating prefab: {assetPath}");

            // Create primitive GameObject
            GameObject go = primitiveType switch
            {
                "Wall" => CreateWallPrimitive(),
                "Door" => CreateDoorPrimitive(),
                _ => GameObject.CreatePrimitive(PrimitiveType.Cube)
            };

            go.name = Path.GetFileNameWithoutExtension(configPath);

            // Save as prefab at config path
            EnsureFolder(Path.GetDirectoryName(assetPath));
            PrefabUtility.SaveAsPrefabAsset(go, assetPath);
            DestroyImmediate(go);

            Debug.Log($"[Setup]   Created: {assetPath}");
        }

        private static void CreateTorchPrefabIfMissing(string configPath)
        {
            string assetPath = $"Assets/{configPath}";
            
            if (AssetDatabase.LoadAssetAtPath<GameObject>(assetPath) != null)
            {
                Debug.Log($"[Setup]  TorchPrefab exists: {assetPath}");
                return;
            }

            Debug.Log($"[Setup]  Creating TorchPrefab: {assetPath}");

            // Create torch with Light component
            GameObject torch = new GameObject("TorchHandle");
            
            Light light = torch.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.8f, 0.6f);  // Warm orange
            light.intensity = 1.5f;
            light.range = 10f;
            light.shadows = LightShadows.Soft;

            // Add TorchController for flame animation
            torch.AddComponent<TorchController>();

            // Save as prefab at config path
            EnsureFolder(Path.GetDirectoryName(assetPath));
            PrefabUtility.SaveAsPrefabAsset(torch, assetPath);
            DestroyImmediate(torch);

            Debug.Log($"[Setup]   Created: {assetPath}");
        }

        private static void CreateMaterialIfMissing(string configPath, string name)
        {
            string assetPath = $"Assets/{configPath}";
            
            if (AssetDatabase.LoadAssetAtPath<Material>(assetPath) != null)
            {
                Debug.Log($"[Setup]  Material exists: {assetPath}");
                return;
            }

            Debug.Log($"[Setup]  Creating Material: {assetPath}");

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = name;
            mat.color = name.Contains("Wall") ? Color.gray : new Color(0.8f, 0.75f, 0.7f);

            EnsureFolder(Path.GetDirectoryName(assetPath));
            AssetDatabase.CreateAsset(mat, assetPath);

            Debug.Log($"[Setup]   Created: {assetPath}");
        }

        private static void CreateTextureIfMissing(string configPath, string name)
        {
            string assetPath = $"Assets/{configPath}";
            
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath) != null)
            {
                Debug.Log($"[Setup]  Texture exists: {assetPath}");
                return;
            }

            Debug.Log($"[Setup]  Creating Texture: {assetPath}");

            // Create simple colored texture
            Texture2D tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            Color baseColor = name.Contains("floor") ? new Color(0.6f, 0.55f, 0.5f) : new Color(0.5f, 0.5f, 0.5f);
            
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    // Add simple noise
                    float noise = Random.value * 0.2f;
                    tex.SetPixel(x, y, baseColor * (1f + noise));
                }
            }
            tex.Apply();

            EnsureFolder(Path.GetDirectoryName(assetPath));
            
            byte[] png = tex.EncodeToPNG();
            File.WriteAllBytes(assetPath, png);
            AssetDatabase.ImportAsset(assetPath);

            DestroyImmediate(tex);

            Debug.Log($"[Setup]   Created: {assetPath}");
        }

        private static void EnsureTorchPool(GameConfig config)
        {
            var torchPool = FindFirstObjectByType<TorchPool>();
            
            if (torchPool == null)
            {
                GameObject go = new GameObject("TorchPool");
                torchPool = go.AddComponent<TorchPool>();
                Debug.Log("[Setup]  Created TorchPool component");
            }

            // Assign torch prefab from config
            string torchPrefabPath = $"Assets/{config.torchPrefab}";
            var torchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(torchPrefabPath);
            
            if (torchPrefab != null)
            {
                var serializedObject = new SerializedObject(torchPool);
                var prop = serializedObject.FindProperty("torchHandlePrefab");
                if (prop != null)
                {
                    prop.objectReferenceValue = torchPrefab;
                    serializedObject.ApplyModifiedProperties();
                    Debug.Log($"[Setup]   Assigned TorchPrefab to TorchPool: {config.torchPrefab}");
                }
            }
            else
            {
                Debug.LogWarning($"[Setup]  ️ TorchPrefab not found at: {torchPrefabPath}");
            }
        }

        private static void EnsureLightPlacementEngine()
        {
            var lightEngine = FindFirstObjectByType<LightPlacementEngine>();
            
            if (lightEngine == null)
            {
                GameObject go = new GameObject("LightPlacementEngine");
                lightEngine = go.AddComponent<LightPlacementEngine>();
                Debug.Log("[Setup]  Created LightPlacementEngine component");
            }
        }

        private static void EnsureCompleteMazeBuilder(GameConfig config)
        {
            var mazeBuilder = FindFirstObjectByType<CompleteMazeBuilder>();
            
            if (mazeBuilder == null)
            {
                GameObject go = new GameObject("MazeBuilder");
                mazeBuilder = go.AddComponent<CompleteMazeBuilder>();
                Debug.Log("[Setup]  Created CompleteMazeBuilder component");
            }

            // Assign all prefabs and materials from config
            var serializedObject = new SerializedObject(mazeBuilder);

            AssignField(serializedObject, "wallPrefab", config.wallPrefab, typeof(GameObject));
            AssignField(serializedObject, "doorPrefab", config.doorPrefab, typeof(GameObject));
            AssignField(serializedObject, "torchPrefab", config.torchPrefab, typeof(GameObject));
            AssignField(serializedObject, "wallMaterial", config.wallMaterial, typeof(Material));
            AssignField(serializedObject, "floorMaterial", config.floorMaterial, typeof(Material));
            AssignField(serializedObject, "groundTexture", config.groundTexture, typeof(Texture2D));

            serializedObject.ApplyModifiedProperties();
            Debug.Log("[Setup]   Assigned all prefabs/materials to CompleteMazeBuilder from config");
        }

        private static void AssignField(SerializedObject obj, string fieldName, string configPath, System.Type type)
        {
            string assetPath = $"Assets/{configPath}";
            UnityEngine.Object asset = null;

            if (type == typeof(GameObject))
                asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            else if (type == typeof(Material))
                asset = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            else if (type == typeof(Texture2D))
                asset = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

            var prop = obj.FindProperty(fieldName);
            if (prop != null && asset != null)
            {
                prop.objectReferenceValue = asset;
                Debug.Log($"[Setup]   Assigned {fieldName}: {configPath}");
            }
            else if (asset == null)
            {
                Debug.LogWarning($"[Setup]  ️ {type.Name} not found: {assetPath}");
            }
        }

        // ─── Primitive Helpers ───────────────────────────────────────────────

        private static GameObject CreateWallPrimitive()
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall";
            wall.transform.localScale = new Vector3(6f, 4f, 0.5f);
            
            // Remove collider (not needed for maze walls)
            var collider = wall.GetComponent<BoxCollider>();
            if (collider != null)
                DestroyImmediate(collider);

            return wall;
        }

        private static GameObject CreateDoorPrimitive()
        {
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Door";
            door.transform.localScale = new Vector3(2.5f, 3f, 0.5f);
            
            // Add door animator
            door.AddComponent<DoorAnimation>();

            // Remove collider
            var collider = door.GetComponent<BoxCollider>();
            if (collider != null)
                DestroyImmediate(collider);

            return door;
        }
    }
}
