// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8  |  Locale: en_US

using UnityEngine;
using UnityEditor;
using Code.Lavos.Core.Environment;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// CreateDoorPrefab - Editor tool to create door prefabs with proper setup.
    /// 
    /// Features:
    /// - Creates door mesh with correct pivot (hinge-based)
    /// - Adds DoorController component
    /// - Configures materials and colliders
    /// - Saves as prefab in Resources/Prefabs/
    /// 
    /// Usage:
    /// Tools > Create Door Prefab
    /// </summary>
    public class CreateDoorPrefab : EditorWindow
    {
        private enum DoorPrefabType
        {
            Normal,
            Locked,
            Secret,
            Exit
        }

        private DoorPrefabType selectedType = DoorPrefabType.Normal;
        private string doorName = "DoorPrefab";
        private Color doorColor = new Color(0.6f, 0.4f, 0.2f); // Brown wood

        [MenuItem("Tools/Doors/Create Door Prefab")]
        public static void ShowWindow()
        {
            var window = GetWindow<CreateDoorPrefab>("Create Door Prefab");
            window.minSize = new Vector2(400, 500);
        }

        private void OnGUI()
        {
            GUILayout.Label("Door Prefab Creator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Door type selection
            selectedType = (DoorPrefabType)EditorGUILayout.EnumPopup("Door Type", selectedType);
            
            // Auto-set name and color based on type
            UpdateDefaults();

            GUILayout.Space(10);

            // Door name
            doorName = EditorGUILayout.TextField("Prefab Name", doorName);

            // Door color
            doorColor = EditorGUILayout.ColorField("Door Color", doorColor);

            GUILayout.Space(10);

            // Settings
            GUILayout.Label("Door Settings", EditorStyles.boldLabel);
            
            GUILayout.Space(5);

            if (GUILayout.Button("Create Door Prefab", GUILayout.Height(40)))
            {
                CreateDoorPrefabAsset();
            }

            GUILayout.Space(20);

            // Instructions
            GUILayout.Label("Instructions", EditorStyles.boldLabel);
            GUILayout.TextArea(
                "1. Select door type (Normal/Locked/Secret/Exit)\n" +
                "2. Adjust name and color if needed\n" +
                "3. Click 'Create Door Prefab'\n" +
                "4. Prefab will be created in Assets/Resources/Prefabs/\n" +
                "5. Assign to CompleteMazeBuilder8 in Inspector", 
                GUILayout.Height(100)
            );

            GUILayout.Space(10);

            // Pivot info
            GUILayout.Label("Pivot Setup (IMPORTANT!)", EditorStyles.boldLabel);
            GUILayout.TextArea(
                "Door pivot MUST be at hinge side, not center!\n\n" +
                "For doors facing North/South:\n" +
                "- Pivot on LEFT or RIGHT edge\n" +
                "- Door rotates around Y axis\n\n" +
                "For doors facing East/West:\n" +
                "- Pivot on LEFT or RIGHT edge\n" +
                "- Door rotates around Y axis",
                GUILayout.Height(120)
            );
        }

        private void UpdateDefaults()
        {
            switch (selectedType)
            {
                case DoorPrefabType.Normal:
                    if (doorName == "DoorPrefab" || doorName.EndsWith("Prefab"))
                        doorName = "DoorPrefab";
                    doorColor = new Color(0.6f, 0.4f, 0.2f); // Brown wood
                    break;
                case DoorPrefabType.Locked:
                    if (doorName == "DoorPrefab" || doorName.EndsWith("Prefab"))
                        doorName = "LockedDoorPrefab";
                    doorColor = new Color(0.7f, 0.7f, 0.7f); // Silver metal
                    break;
                case DoorPrefabType.Secret:
                    if (doorName == "DoorPrefab" || doorName.EndsWith("Prefab"))
                        doorName = "SecretDoorPrefab";
                    doorColor = new Color(0.4f, 0.3f, 0.2f); // Dark brown
                    break;
                case DoorPrefabType.Exit:
                    if (doorName == "DoorPrefab" || doorName.EndsWith("Prefab"))
                        doorName = "ExitDoorPrefab";
                    doorColor = new Color(0.5f, 0.5f, 0.5f); // Gray stone
                    break;
            }
        }

        private void CreateDoorPrefabAsset()
        {
            try
            {
                // Ensure Resources/Prefabs folder exists
                string folderPath = "Assets/Resources/Prefabs";
                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    AssetDatabase.CreateFolder("Assets/Resources", "Prefabs");
                }

                // Create door GameObject
                GameObject door = CreateDoorGameObject();

                // Save as prefab
                string prefabPath = $"{folderPath}/{doorName}.prefab";

                // Delete existing prefab if it exists
                if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
                {
                    AssetDatabase.DeleteAsset(prefabPath);
                }

                PrefabUtility.SaveAsPrefabAsset(door, prefabPath);

                Debug.Log($"[CreateDoorPrefab] Created: {prefabPath}");
                EditorUtility.RevealInFinder(prefabPath);

                // Cleanup - MUST use DestroyImmediate in edit mode
                DestroyImmediate(door);

                EditorUtility.DisplayDialog(
                    "Door Prefab Created",
                    $"Successfully created {doorName} at:\n{prefabPath}\n\n" +
                    $"Don't forget to assign it to CompleteMazeBuilder8!",
                    "OK"
                );
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CreateDoorPrefab] Error: {e.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to create prefab: {e.Message}", "OK");
            }
        }

        private GameObject CreateDoorGameObject()
        {
            // Create parent object (this will be the HINGE/PIVOT point)
            GameObject doorParent = new GameObject("DoorParent");
            
            // Create door mesh as child
            GameObject doorMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorMesh.name = "DoorMesh";
            doorMesh.transform.SetParent(doorParent.transform, false);

            // Set door scale (1u wide, 3u high, 0.3u thick)
            bool isExit = selectedType == DoorPrefabType.Exit;
            float width = isExit ? 1.5f : 1.0f;
            float height = isExit ? 3.5f : 3.0f;
            float thickness = 0.3f;

            doorMesh.transform.localScale = new Vector3(width, height, thickness);

            // CRITICAL: Position mesh so pivot (parent) is at BOTTOM-LEFT corner
            // This is the hinge point for door rotation
            // Mesh center offset: right by half width, up by half height
            doorMesh.transform.localPosition = new Vector3(width / 2f, height / 2f, 0f);

            // Create and assign material
            Material doorMat = CreateDoorMaterial(doorColor);
            Renderer renderer = doorMesh.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = doorMat;
            }

            // Add DoorController component to parent (pivot object)
            DoorController controller = doorParent.AddComponent<DoorController>();
            
            // Configure DoorController - set rotating part to the mesh
            var serializedController = new SerializedObject(controller);
            serializedController.FindProperty("rotatingPart").objectReferenceValue = doorMesh.transform;
            serializedController.FindProperty("hingeOnLeft").boolValue = true; // Pivot at left edge
            serializedController.FindProperty("openAngle").floatValue = 90f; // 90 outward swing
            serializedController.ApplyModifiedProperties();

            return doorParent;
        }

        private Material CreateDoorMaterial(Color color)
        {
            string matName = $"Door_{selectedType}_Mat";
            string matPath = $"Assets/Resources/Materials/{matName}.mat";

            // Ensure Materials folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Materials"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Materials");
            }

            // Try to load pixel art texture first
            string texturePath = $"Assets/Textures/Doors/Door_{selectedType}.png";
            Texture2D doorTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

            // Check if material already exists
            Material existingMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (existingMat != null)
            {
                // Update texture if we generated one
                if (doorTex != null)
                {
                    existingMat.mainTexture = doorTex;
                    AssetDatabase.SaveAssets();
                }
                return existingMat;
            }

            // Create new material with URP Lit shader
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            
            // Assign pixel art texture if available
            if (doorTex != null)
            {
                mat.mainTexture = doorTex;
                mat.color = Color.white; // Use texture color
                Debug.Log($"[CreateDoorPrefab] Using pixel art texture: {texturePath}");
            }
            else
            {
                // Fallback to solid color
                mat.color = color;
                Debug.Log($"[CreateDoorPrefab] No pixel art texture found, using color: {color}");
            }

            mat.name = matName;

            // Save material
            AssetDatabase.CreateAsset(mat, matPath);
            AssetDatabase.SaveAssets();

            return mat;
        }

        private void ConfigureDoorController(DoorController controller, Transform rotatingPart)
        {
            // Set door type
            switch (selectedType)
            {
                case DoorPrefabType.Normal:
                    // Normal door - no lock
                    break;
                case DoorPrefabType.Locked:
                    // Locked door - enable lock
                    var serialized = new SerializedObject(controller);
                    serialized.FindProperty("isLocked").boolValue = true;
                    serialized.ApplyModifiedProperties();
                    break;
                case DoorPrefabType.Secret:
                    // Secret door - subtle appearance
                    break;
                case DoorPrefabType.Exit:
                    // Exit door - larger, always openable
                    break;
            }
        }
    }
}
