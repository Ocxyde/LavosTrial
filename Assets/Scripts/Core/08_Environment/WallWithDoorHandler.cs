// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8  |  Locale: en_US

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core.Environment
{
    /// <summary>
    /// WallWithDoorHandler - Spawns walls with integrated door openings.
    /// 
    /// Features:
    /// - Spawns wall segments with door openings carved out
    /// - Automatically places door prefabs in openings
    /// - Handles wall orientation (N/S/E/W)
    /// - Supports multiple door types (Normal/Locked/Secret/Exit)
    /// - Perfect wall-door alignment (no clipping)
    /// 
    /// Usage:
    /// Called by CompleteMazeBuilder8 during maze generation.
    /// </summary>
    public static class WallWithDoorHandler
    {
        /// <summary>
        /// Spawn all walls with integrated doors based on maze data.
        /// </summary>
        public static void SpawnWallsWithDoors(
            MazeData8 mazeData,
            GameObject wallPrefab,
            GameObject[] doorPrefabs,
            Material wallMaterial,
            Transform wallsRoot,
            Transform doorsRoot,
            float cellSize,
            float wallHeight,
            float wallThickness,
            bool wallPivotIsAtMeshCenter)
        {
            if (mazeData == null || wallPrefab == null || wallsRoot == null)
            {
                Debug.LogError("[WallWithDoorHandler] Invalid parameters!");
                return;
            }

            int width = mazeData.Width;
            int height = mazeData.Height;

            HashSet<(int x, int z, Direction8 dir)> doorPositions = new HashSet<(int, int, Direction8)>();

            foreach (var doorOpening in mazeData.DoorOpenings)
            {
                (int dx, int dz) = Direction8Helper.ToOffset(doorOpening.Direction);
                int wallX = doorOpening.X - dx;
                int wallZ = doorOpening.Z - dz;

                doorPositions.Add((wallX, wallZ, doorOpening.Direction));
            }

            Debug.Log($"[WallWithDoorHandler] Found {doorPositions.Count} door positions");

            int wallsSpawned = 0;
            int doorsSpawned = 0;

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    var cell = mazeData.GetCell(x, z);

                    foreach (var dir in new[] { Direction8.N, Direction8.S, Direction8.E, Direction8.W })
                    {
                        if (mazeData.HasWall(x, z, dir))
                        {
                            if (doorPositions.Contains((x, z, dir)))
                            {
                                SpawnWallWithDoorOpening(
                                    x, z, dir, mazeData,
                                    wallPrefab, doorPrefabs, wallMaterial,
                                    wallsRoot, doorsRoot,
                                    cellSize, wallHeight, wallThickness,
                                    wallPivotIsAtMeshCenter);
                                doorsSpawned++;
                            }
                            else
                            {
                                SpawnSolidWall(
                                    x, z, dir, mazeData,
                                    wallPrefab, wallMaterial,
                                    wallsRoot,
                                    cellSize, wallHeight, wallThickness,
                                    wallPivotIsAtMeshCenter);
                            }
                            wallsSpawned++;
                        }
                    }
                }
            }

            Debug.Log($"[WallWithDoorHandler] Complete: {wallsSpawned} walls, {doorsSpawned} doors");
        }

        private static void SpawnSolidWall(
            int x, int z, Direction8 direction, MazeData8 mazeData,
            GameObject wallPrefab, Material wallMaterial,
            Transform parent,
            float cellSize, float wallHeight, float wallThickness,
            bool wallPivotIsAtMeshCenter)
        {
            Vector3 position = GetWallPosition(x, z, direction, cellSize, wallHeight, wallThickness, wallPivotIsAtMeshCenter);
            Quaternion rotation = GetWallRotation(direction);

            GameObject wall = Object.Instantiate(wallPrefab, position, rotation, parent);
            wall.name = $"Wall_{x}_{z}_{direction}";

            var renderer = wall.GetComponent<Renderer>();
            if (renderer != null && wallMaterial != null)
            {
                renderer.sharedMaterial = wallMaterial;
            }
        }

        private static void SpawnWallWithDoorOpening(
            int x, int z, Direction8 direction, MazeData8 mazeData,
            GameObject wallPrefab, GameObject[] doorPrefabs, Material wallMaterial,
            Transform wallsParent, Transform doorsParent,
            float cellSize, float wallHeight, float wallThickness,
            bool wallPivotIsAtMeshCenter)
        {
            DoorOpening? doorOpening = FindDoorOpening(mazeData, x, z, direction);
            if (doorOpening == null)
            {
                Debug.LogWarning($"[WallWithDoorHandler] No door opening found at ({x},{z}) {direction}");
                SpawnSolidWall(x, z, direction, mazeData, wallPrefab, wallMaterial, wallsParent, cellSize, wallHeight, wallThickness, wallPivotIsAtMeshCenter);
                return;
            }

            Vector3 wallPosition = GetWallWithDoorPosition(x, z, direction, cellSize, wallHeight, wallThickness, wallPivotIsAtMeshCenter);
            Quaternion wallRotation = GetWallRotation(direction);

            GameObject wall = SpawnWallSegment(
                wallPosition, wallRotation, wallPrefab, wallMaterial, wallsParent,
                cellSize, wallHeight, wallThickness,
                hasDoorOpening: true, doorOpeningWidth: 1.0f);

            wall.name = $"Wall_Door_{x}_{z}_{direction}";

            SpawnDoorPrefab(
                x, z, direction, doorOpening.Value,
                doorPrefabs, doorsParent,
                cellSize, wallHeight);
        }

        private static GameObject SpawnWallSegment(
            Vector3 position, Quaternion rotation,
            GameObject wallPrefab, Material wallMaterial,
            Transform parent,
            float cellSize, float wallHeight, float wallThickness,
            bool hasDoorOpening = false, float doorOpeningWidth = 1.0f)
        {
            if (!hasDoorOpening)
            {
                GameObject wall = Object.Instantiate(wallPrefab, position, rotation, parent);
                var renderer = wall.GetComponent<Renderer>();
                if (renderer != null && wallMaterial != null)
                {
                    renderer.sharedMaterial = wallMaterial;
                }
                return wall;
            }

            GameObject wallParent = new GameObject("WallWithDoor");
            wallParent.transform.position = position;
            wallParent.transform.rotation = rotation;
            wallParent.transform.SetParent(parent, false);

            float segmentWidth = (cellSize - doorOpeningWidth) / 2f;

            Vector3 leftPos = new Vector3(-segmentWidth / 2f - doorOpeningWidth / 2f, 0, 0);
            GameObject leftWall = Object.Instantiate(wallPrefab, leftPos, Quaternion.identity, wallParent.transform);
            leftWall.transform.localScale = new Vector3(segmentWidth / cellSize, 1f, 1f);
            ApplyMaterial(leftWall, wallMaterial);

            Vector3 rightPos = new Vector3(segmentWidth / 2f + doorOpeningWidth / 2f, 0, 0);
            GameObject rightWall = Object.Instantiate(wallPrefab, rightPos, Quaternion.identity, wallParent.transform);
            rightWall.transform.localScale = new Vector3(segmentWidth / cellSize, 1f, 1f);
            ApplyMaterial(rightWall, wallMaterial);

            return wallParent;
        }

        private static void SpawnDoorPrefab(
            int x, int z, Direction8 direction, DoorOpening doorOpening,
            GameObject[] doorPrefabs, Transform parent,
            float cellSize, float wallHeight)
        {
            GameObject doorPrefab = GetDoorPrefabForType(doorOpening.Type, doorPrefabs);
            if (doorPrefab == null)
            {
                Debug.LogWarning($"[WallWithDoorHandler] No prefab for door type {doorOpening.Type}");
                return;
            }

            Vector3 doorPosition = GetDoorPosition(x, z, direction, cellSize, wallHeight);
            Quaternion doorRotation = GetDoorRotation(direction);

            GameObject door = Object.Instantiate(doorPrefab, doorPosition, doorRotation, parent);
            door.name = $"Door_{doorOpening.Type}_{x}_{z}";

            Debug.Log($"[WallWithDoorHandler] Spawned door: {doorOpening.Type} at ({x},{z})");
        }

        #region Helper Methods

        private static Vector3 GetWallPosition(int x, int z, Direction8 direction,
            float cellSize, float wallHeight, float wallThickness, bool wallPivotIsAtMeshCenter)
        {
            float posX = x * cellSize + cellSize / 2f;
            float posZ = z * cellSize + cellSize / 2f;
            float posY = wallPivotIsAtMeshCenter ? wallHeight / 2f : 0f;

            switch (direction)
            {
                case Direction8.N: posZ += cellSize / 2f; break;
                case Direction8.S: posZ -= cellSize / 2f; break;
                case Direction8.E: posX += cellSize / 2f; break;
                case Direction8.W: posX -= cellSize / 2f; break;
            }

            return new Vector3(posX, posY, posZ);
        }

        private static Vector3 GetWallWithDoorPosition(int x, int z, Direction8 direction,
            float cellSize, float wallHeight, float wallThickness, bool wallPivotIsAtMeshCenter)
        {
            return GetWallPosition(x, z, direction, cellSize, wallHeight, wallThickness, wallPivotIsAtMeshCenter);
        }

        private static Vector3 GetDoorPosition(int x, int z, Direction8 direction, float cellSize, float wallHeight)
        {
            float posX = x * cellSize + cellSize / 2f;
            float posZ = z * cellSize + cellSize / 2f;

            switch (direction)
            {
                case Direction8.N: posZ += cellSize / 2f - 0.15f; break;
                case Direction8.S: posZ -= cellSize / 2f + 0.15f; break;
                case Direction8.E: posX += cellSize / 2f - 0.15f; break;
                case Direction8.W: posX -= cellSize / 2f + 0.15f; break;
            }

            return new Vector3(posX, wallHeight / 2f, posZ);
        }

        private static Quaternion GetWallRotation(Direction8 direction)
        {
            switch (direction)
            {
                case Direction8.N:
                case Direction8.S:
                    return Quaternion.Euler(0f, 90f, 0f);
                case Direction8.E:
                case Direction8.W:
                    return Quaternion.Euler(0f, 0f, 0f);
                default:
                    return Quaternion.identity;
            }
        }

        private static Quaternion GetDoorRotation(Direction8 direction)
        {
            switch (direction)
            {
                case Direction8.N: return Quaternion.Euler(0f, 90f, 0f);
                case Direction8.S: return Quaternion.Euler(0f, 270f, 0f);
                case Direction8.E: return Quaternion.Euler(0f, 0f, 0f);
                case Direction8.W: return Quaternion.Euler(0f, 180f, 0f);
                default: return Quaternion.identity;
            }
        }

        private static GameObject GetDoorPrefabForType(MazeDoorType type, GameObject[] doorPrefabs)
        {
            if (doorPrefabs == null || doorPrefabs.Length < 4)
                return null;

            return type switch
            {
                MazeDoorType.Normal => doorPrefabs[0],
                MazeDoorType.Locked => doorPrefabs[1],
                MazeDoorType.Secret => doorPrefabs[2],
                MazeDoorType.Exit => doorPrefabs[3],
                _ => doorPrefabs[0],
            };
        }

        private static DoorOpening? FindDoorOpening(MazeData8 mazeData, int x, int z, Direction8 direction)
        {
            foreach (var door in mazeData.DoorOpenings)
            {
                if (door.Direction == direction)
                {
                    (int dx, int dz) = Direction8Helper.ToOffset(direction);
                    int wallX = door.X - dx;
                    int wallZ = door.Z - dz;

                    if (wallX == x && wallZ == z)
                    {
                        return door;
                    }
                }
            }
            return null;
        }

        private static void ApplyMaterial(GameObject wall, Material material)
        {
            if (material == null) return;

            var renderer = wall.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        #endregion
    }
}
