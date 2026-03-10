﻿// Copyright (C) 2026 Ocxyde
//
// This file is part of Code.Lavos.
//
// Code.Lavos is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BoundaryTestSceneSetup.cs
// Editor tool to set up BoundaryTest scene with all required prefabs
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   1. Open BoundaryTest.unity scene
//   2. Tools > Setup Boundary Test Scene
//   3. Wait for prefabs to be created
//   4. Press Play to test maze generation
//
// Location: Assets/Scripts/Editor/

using UnityEngine;
using UnityEditor;
using System.IO;
using Code.Lavos.Core;

namespace Code.Lavos.Editor
{
    public class BoundaryTestSceneSetup
    {
        [MenuItem("Tools/Setup Boundary Test Scene")]
        public static void SetupBoundaryTestScene()
        {
            Debug.Log("[BoundaryTest] Starting scene setup...");

            // Step 1: Create folders
            EnsureFolderExists("Assets/Resources");
            EnsureFolderExists("Assets/Resources/Config");

            // Step 2: Load existing prefabs from Assets/Resources/Prefabs/
            Debug.Log("[BoundaryTest] Loading prefabs from Assets/Resources/Prefabs/...");
            
            var wallPrefab = LoadPrefab("WallPrefab");
            var floorPrefab = LoadPrefab("FloorTilePrefab");
            var torchPrefab = LoadPrefab("TorchHandlePrefab");
            var chestPrefab = LoadPrefab("ChestPrefab");
            var enemyPrefab = LoadPrefab("EnemyPrefab");
            var doorPrefab = LoadPrefab("DoorPrefab");
            var playerPrefab = LoadPrefab("PlayerPrefab");

            // Step 3: Create GameConfig component in scene
            CreateGameConfigComponent();

            // Step 4: Assign to MazeGenerator
            AssignPrefabsToMazeGenerator(
                wallPrefab, floorPrefab, torchPrefab,
                chestPrefab, enemyPrefab, doorPrefab, playerPrefab);

            Debug.Log("[BoundaryTest] Scene setup complete!");
            Debug.Log("[BoundaryTest] All prefabs loaded and assigned");
            Debug.Log("[BoundaryTest] GameConfig component created");
            Debug.Log("[BoundaryTest] Press Play to generate maze");
        }

        private static void CreateGameConfigComponent()
        {
            // Check if GameConfig already exists in scene
            var existingConfig = Object.FindObjectOfType<GameConfig>();
            if (existingConfig != null)
            {
                Debug.Log("[BoundaryTest] GameConfig already exists in scene");
                return;
            }
            
            // Create GameConfig component
            GameObject configObj = new GameObject("GameConfig");
            GameConfig config = configObj.AddComponent<GameConfig>();
            
            // Set writable fields only
            config.CellSize = 6f;
            config.WallHeight = 4f;
            config.WallThickness = 0.2f;
            config.PlayerEyeHeight = 1.7f;
            
            Debug.Log("[BoundaryTest] Created GameConfig component in scene");
        }

        private static GameObject LoadPrefab(string name)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/" + name);
            if (prefab != null)
            {
                Debug.Log($"[BoundaryTest] Loaded: {name}");
            }
            else
            {
                Debug.LogWarning($"[BoundaryTest] NOT FOUND: {name}");
            }
            return prefab;
        }

        private static void EnsureFolderExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static void CreateWallPrefab()
        {
            string path = "Assets/Resources/Prefabs/WallPrefab.prefab";
            
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "WallPrefab";
            wall.transform.localScale = new Vector3(6f, 4f, 0.2f);
            wall.transform.position = new Vector3(0, 2f, 0);
            
            SavePrefab(wall, path);
            Debug.Log("[BoundaryTest] Created WallPrefab");
        }

        private static void CreateFloorPrefab()
        {
            string path = "Assets/Resources/Prefabs/FloorTilePrefab.prefab";
            
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "FloorTilePrefab";
            
            SavePrefab(floor, path);
            Debug.Log("[BoundaryTest] Created FloorTilePrefab");
        }

        private static void CreateTorchPrefab()
        {
            string path = "Assets/Resources/Prefabs/TorchHandlePrefab.prefab";
            
            GameObject torch = new GameObject("TorchHandlePrefab");
            
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.name = "Handle";
            handle.transform.parent = torch.transform;
            handle.transform.localPosition = new Vector3(0, 0.5f, 0);
            handle.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
            
            GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flame.name = "Flame";
            flame.transform.parent = torch.transform;
            flame.transform.localPosition = new Vector3(0, 1.2f, 0);
            flame.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            
            Light light = flame.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.6f, 0.2f);
            light.intensity = 1f;
            light.range = 5f;
            
            SavePrefab(torch, path);
            Debug.Log("[BoundaryTest] Created TorchHandlePrefab");
        }

        private static void CreateChestPrefab()
        {
            string path = "Assets/Resources/Prefabs/ChestPrefab.prefab";
            
            GameObject chest = new GameObject("ChestPrefab");
            
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = "Box";
            box.transform.parent = chest.transform;
            box.transform.localPosition = new Vector3(0, 0.5f, 0);
            box.transform.localScale = new Vector3(1f, 1f, 1f);
            
            SavePrefab(chest, path);
            Debug.Log("[BoundaryTest] Created ChestPrefab");
        }

        private static void CreateEnemyPrefab()
        {
            string path = "Assets/Resources/Prefabs/EnemyPrefab.prefab";
            
            GameObject enemy = new GameObject("EnemyPrefab");
            
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.parent = enemy.transform;
            body.transform.localPosition = new Vector3(0, 1f, 0);
            body.transform.localScale = new Vector3(1f, 2f, 1f);
            
            SavePrefab(enemy, path);
            Debug.Log("[BoundaryTest] Created EnemyPrefab");
        }

        private static void CreateDoorPrefab()
        {
            string path = "Assets/Resources/Prefabs/DoorPrefab.prefab";
            
            GameObject door = new GameObject("DoorPrefab");
            
            GameObject doorMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorMesh.name = "DoorMesh";
            doorMesh.transform.parent = door.transform;
            doorMesh.transform.localPosition = new Vector3(0, 2f, 0);
            doorMesh.transform.localScale = new Vector3(3f, 4f, 0.2f);
            doorMesh.transform.localRotation = Quaternion.Euler(0, 90, 0);
            
            SavePrefab(door, path);
            Debug.Log("[BoundaryTest] Created DoorPrefab");
        }

        private static void CreatePlayerPrefab()
        {
            string path = "Assets/Resources/Prefabs/PlayerPrefab.prefab";
            
            GameObject player = new GameObject("PlayerPrefab");
            
            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.5f;
            
            GameObject camera = new GameObject("Camera");
            camera.AddComponent<Camera>();
            camera.transform.parent = player.transform;
            camera.transform.localPosition = new Vector3(0, 1.7f, 0);
            
            SavePrefab(player, path);
            Debug.Log("[BoundaryTest] Created PlayerPrefab");
        }

        private static void CreateGameConfig()
        {
            string path = "Assets/Resources/Config/GameConfig8-default.json";

            string json = @"{
    ""CellSize"": 6.0,
    ""WallHeight"": 4.0,
    ""defaultCorridorWidth"": 2,
    ""corridorRandomness"": 0.3,
    ""generatePerimeterCorridor"": true
}";
            File.WriteAllText(path, json);
            
            // Force Unity to import the file
            AssetDatabase.Refresh();
            
            Debug.Log("[BoundaryTest] Created GameConfig");
        }

        private static void AssignPrefabsToMazeGenerator(
            GameObject wallPrefab, GameObject floorPrefab, GameObject torchPrefab,
            GameObject chestPrefab, GameObject enemyPrefab, GameObject doorPrefab,
            GameObject playerPrefab)
        {
            // Find or create MazeGenerator in scene
            var mazeGen = Object.FindObjectOfType<CompleteMazeBuilder8>();
            if (mazeGen == null)
            {
                Debug.Log("[BoundaryTest] MazeGenerator not found - creating one...");
                GameObject mazeGenObj = new GameObject("MazeGenerator");
                mazeGen = mazeGenObj.AddComponent<CompleteMazeBuilder8>();
                Debug.Log("[BoundaryTest] Created MazeGenerator GameObject with CompleteMazeBuilder8 component");
            }

            // Use SerializedObject to assign private fields
            var serializedMazeGen = new SerializedObject(mazeGen);
            
            serializedMazeGen.FindProperty("wallPrefab").objectReferenceValue = wallPrefab;
            serializedMazeGen.FindProperty("floorPrefab").objectReferenceValue = floorPrefab;
            serializedMazeGen.FindProperty("torchPrefab").objectReferenceValue = torchPrefab;
            serializedMazeGen.FindProperty("chestPrefab").objectReferenceValue = chestPrefab;
            serializedMazeGen.FindProperty("enemyPrefab").objectReferenceValue = enemyPrefab;
            serializedMazeGen.FindProperty("doorPrefab").objectReferenceValue = doorPrefab;
            serializedMazeGen.FindProperty("playerPrefab").objectReferenceValue = playerPrefab;
            
            serializedMazeGen.ApplyModifiedProperties();

            Debug.Log("[BoundaryTest] Assigned all prefabs to MazeGenerator");
            
            // Log which prefabs were missing
            if (wallPrefab == null) Debug.LogWarning("[BoundaryTest] WARNING: WallPrefab not found!");
            if (floorPrefab == null) Debug.LogWarning("[BoundaryTest] WARNING: FloorTilePrefab not found!");
            if (torchPrefab == null) Debug.LogWarning("[BoundaryTest] WARNING: TorchHandlePrefab not found!");
            if (chestPrefab == null) Debug.LogWarning("[BoundaryTest] WARNING: ChestPrefab not found!");
            if (enemyPrefab == null) Debug.LogWarning("[BoundaryTest] WARNING: EnemyPrefab not found!");
            if (doorPrefab == null) Debug.LogWarning("[BoundaryTest] WARNING: DoorPrefab not found!");
            if (playerPrefab == null) Debug.LogWarning("[BoundaryTest] WARNING: PlayerPrefab not found!");
        }

        private static void SavePrefab(GameObject obj, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
        }
    }
}
