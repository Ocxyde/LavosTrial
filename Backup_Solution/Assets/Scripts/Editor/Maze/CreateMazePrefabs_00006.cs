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
// CreateMazePrefabs.cs
// Editor script to create all maze prefabs (walls, doors, rooms)
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// USAGE:
//   1. Tools → Create Maze Prefabs
//   2. All prefabs created in Assets/Prefabs/
//   3. All materials created in Assets/Materials/
//   4. Ready to use with CompleteMazeBuilder

using UnityEngine;
using UnityEditor;
using System.IO;

namespace Code.Lavos.Editor
{
    public class CreateMazePrefabs
    {
        [MenuItem("Tools/Create Maze Prefabs")]
        public static void CreateAllPrefabs()
        {
            Debug.Log("[CreateMazePrefabs] ========================================");
            Debug.Log("[CreateMazePrefabs] Creating maze prefabs...");
            Debug.Log("[CreateMazePrefabs] ========================================");

            // Ensure folders exist
            EnsureFolder("Assets/Prefabs");
            EnsureFolder("Assets/Materials");
            EnsureFolder("Assets/Materials/Floor");

            // Create prefabs
            CreateWallPrefab();
            CreateDoorPrefab();
            CreateLockedDoorPrefab();
            CreateSecretDoorPrefab();
            CreateEntranceRoomPrefab();
            CreateExitRoomPrefab();
            CreateNormalRoomPrefab();

            // Create materials
            CreateWallMaterial();

            Debug.Log("[CreateMazePrefabs] ========================================");
            Debug.Log("[CreateMazePrefabs] All prefabs created!");
            Debug.Log("[CreateMazePrefabs] Location: Assets/Prefabs/");
            Debug.Log("[CreateMazePrefabs] ========================================");
        }

        private static void EnsureFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log($"[CreateMazePrefabs]  Created: {path}");
            }
        }

        private static void CreateWallPrefab()
        {
            string prefabPath = "Assets/Prefabs/WallPrefab.prefab";

            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall";
            wall.transform.localScale = new Vector3(6f, 4f, 0.5f);
            
            var collider = wall.GetComponent<BoxCollider>();
            if (collider != null) Object.DestroyImmediate(collider);

            ApplyMaterial(wall, "Assets/Materials/WallMaterial.mat", new Color(0.6f, 0.55f, 0.5f));

            SavePrefab(wall, prefabPath);
            Debug.Log("[CreateMazePrefabs]  WallPrefab (6x4x0.5m)");
        }

        private static void CreateDoorPrefab()
        {
            string prefabPath = "Assets/Prefabs/DoorPrefab.prefab";

            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Door";
            door.transform.localScale = new Vector3(0.5f, 4f, 5.4f);

            ApplyMaterial(door, "Assets/Materials/Door_PïxelArt.mat", new Color(0.5f, 0.35f, 0.2f));
            door.AddComponent<Code.Lavos.Core.DoorsEngine>();

            SavePrefab(door, prefabPath);
            Debug.Log("[CreateMazePrefabs]  DoorPrefab (Normal, wood brown)");
        }

        private static void CreateLockedDoorPrefab()
        {
            string prefabPath = "Assets/Prefabs/LockedDoorPrefab.prefab";

            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "LockedDoor";
            door.transform.localScale = new Vector3(0.5f, 4f, 5.4f);

            ApplyMaterial(door, "Assets/Materials/Door_PïxelArt.mat", new Color(0.7f, 0.3f, 0.3f));
            door.AddComponent<Code.Lavos.Core.DoorsEngine>();

            SavePrefab(door, prefabPath);
            Debug.Log("[CreateMazePrefabs]  LockedDoorPrefab (Red tint)");
        }

        private static void CreateSecretDoorPrefab()
        {
            string prefabPath = "Assets/Prefabs/SecretDoorPrefab.prefab";

            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "SecretDoor";
            door.transform.localScale = new Vector3(0.5f, 4f, 5.4f);

            ApplyMaterial(door, "Assets/Materials/WallMaterial.mat", new Color(0.6f, 0.55f, 0.5f));
            door.AddComponent<Code.Lavos.Core.DoorsEngine>();

            SavePrefab(door, prefabPath);
            Debug.Log("[CreateMazePrefabs]  SecretDoorPrefab (Wall-colored, camouflaged)");
        }

        private static void CreateEntranceRoomPrefab()
        {
            string prefabPath = "Assets/Prefabs/EntranceRoomPrefab.prefab";

            GameObject room = new GameObject("EntranceRoom");
            
            GameObject floor = CreateRoomFloor();
            floor.transform.parent = room.transform;

            // Green marker (SOFT green - less flashy)
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "EntranceMarker";
            marker.transform.parent = room.transform;
            marker.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            marker.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
            ApplyMaterial(marker, null, new Color(0.2f, 0.5f, 0.3f)); // Soft forest green

            SavePrefab(room, prefabPath);
            Debug.Log("[CreateMazePrefabs]  EntranceRoomPrefab (Soft green marker)");
        }

        private static void CreateExitRoomPrefab()
        {
            string prefabPath = "Assets/Prefabs/ExitRoomPrefab.prefab";

            GameObject room = new GameObject("ExitRoom");
            
            GameObject floor = CreateRoomFloor();
            floor.transform.parent = room.transform;

            // Blue marker (SOFT blue - less flashy)
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "ExitMarker";
            marker.transform.parent = room.transform;
            marker.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            marker.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
            ApplyMaterial(marker, null, new Color(0.2f, 0.3f, 0.5f)); // Soft slate blue

            SavePrefab(room, prefabPath);
            Debug.Log("[CreateMazePrefabs]  ExitRoomPrefab (Soft blue marker)");
        }

        private static void CreateNormalRoomPrefab()
        {
            string prefabPath = "Assets/Prefabs/NormalRoomPrefab.prefab";

            GameObject room = new GameObject("NormalRoom");
            
            GameObject floor = CreateRoomFloor();
            floor.transform.parent = room.transform;

            SavePrefab(room, prefabPath);
            Debug.Log("[CreateMazePrefabs]  NormalRoomPrefab (Floor only)");
        }

        private static GameObject CreateRoomFloor()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            floor.transform.localScale = new Vector3(18f, 0.1f, 18f);
            ApplyMaterial(floor, "Assets/Materials/Floor/Stone_Floor.mat", Color.white);
            return floor;
        }

        private static void CreateWallMaterial()
        {
            // Ensure Materials folder exists
            EnsureFolder("Assets/Materials");

            string matPath = "Assets/Materials/WallMaterial.mat";

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            Material mat = new Material(shader);
            mat.color = new Color(0.6f, 0.55f, 0.5f); // Stone gray
            mat.SetFloat("_Smoothness", 0.3f);
            mat.SetFloat("_Metallic", 0f);

            AssetDatabase.CreateAsset(mat, matPath);
            Debug.Log("[CreateMazePrefabs]  WallMaterial (URP Lit, stone gray)");
        }

        private static void ApplyMaterial(GameObject obj, string matPath, Color color)
        {
            Material mat = null;

            if (!string.IsNullOrEmpty(matPath) && File.Exists(matPath))
            {
                mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            }

            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                mat = new Material(shader);
            }

            mat.color = color;

            var renderer = obj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = mat;
            }
        }

        private static void SavePrefab(GameObject obj, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
        }
    }
}
