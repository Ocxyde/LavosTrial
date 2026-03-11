// Copyright (C) 2026 Ocxyde
// GPL-3.0 license - see COPYING
// MazeObjectSpawner.cs - Handles spawning of torches, chests, and enemies

using System.Collections.Generic;
using UnityEngine;
using Code.Lavos.Core.Advanced;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Handles spawning of objects (torches, chests, enemies) in the maze.
    /// PLUG-IN-OUT: Works with MazeData8 through EventHandler events.
    /// </summary>
    public static class MazeObjectSpawner
    {
        /// <summary>
        /// Spawn torches in the maze based on torch flags in maze data.
        /// Torches face INWARD toward walkable corridor (not outward into wall!)
        /// </summary>
        public static void SpawnTorches(
            MazeData8 mazeData,
            GameObject torchPrefab,
            float cellSize,
            Transform objectsRoot)
        {
            if (torchPrefab == null)
            {
                Debug.LogWarning("[MazeObjectSpawner] Cannot spawn torches - prefab is null!");
                return;
            }

            if (mazeData == null)
            {
                Debug.LogError("[MazeObjectSpawner] Cannot spawn torches - mazeData is null!");
                return;
            }

            int torchCount = 0;
            float wallHeight = 3f; // Standard wall height

            for (int x = 0; x < mazeData.Width; x++)
            {
                for (int z = 0; z < mazeData.Height; z++)
                {
                    var cell = mazeData.GetCell(x, z);

                    if ((cell & CellFlags8.HasTorch) != 0)
                    {
                        // Position at cell center
                        Vector3 pos = new Vector3(
                            (x + 0.5f) * cellSize,
                            wallHeight / 2f,  // Mid-wall height
                            (z + 0.5f) * cellSize
                        );

                        // Find walkable adjacent cell (corridor side)
                        Vector3 walkableDir = FindWalkableDirection(mazeData, x, z);
                        
                        // If no walkable direction found, skip this torch
                        if (walkableDir == Vector3.zero)
                        {
                            Debug.LogWarning($"[MazeObjectSpawner] Torch at ({x},{z}) has no walkable side - skipping!");
                            continue;
                        }

                        // Rotate torch to face INWARD toward walkable cell
                        // Y rotation: face the walkable direction
                        // X rotation: 25° tilt upward (flame points up)
                        Quaternion rotation = Quaternion.LookRotation(walkableDir, Vector3.up);
                        rotation *= Quaternion.Euler(25f, 0f, 0f); // 25° X tilt

                        var torch = Object.Instantiate(torchPrefab, pos, rotation);
                        if (torch != null)
                        {
                            torch.name = $"Torch_{x}_{z}";
                            torch.transform.SetParent(objectsRoot, false);
                            torchCount++;
                        }
                    }
                }
            }

            Debug.Log($"[MazeObjectSpawner] Spawned {torchCount} torches (facing INWARD)");
        }

        /// <summary>
        /// Find the walkable adjacent cell direction (N/S/E/W).
        /// Returns normalized direction vector or Vector3.zero if none found.
        /// </summary>
        private static Vector3 FindWalkableDirection(MazeData8 mazeData, int x, int z)
        {
            // Check all 4 cardinal directions
            var directions = new[]
            {
                (dx: 0, dz: 1, dir: Vector3.forward),   // North
                (dx: 0, dz: -1, dir: -Vector3.forward), // South
                (dx: 1, dz: 0, dir: Vector3.right),     // East
                (dx: -1, dz: 0, dir: -Vector3.right),   // West
            };

            foreach (var (dx, dz, dir) in directions)
            {
                int nx = x + dx;
                int nz = z + dz;

                if (!mazeData.InBounds(nx, nz))
                    continue;

                var neighborCell = mazeData.GetCell(nx, nz);
                bool isWalkable = (neighborCell & CellFlags8.AllWalls) == CellFlags8.None;

                if (isWalkable)
                {
                    return dir; // Found walkable cell!
                }
            }

            return Vector3.zero; // No walkable direction found
        }

        /// <summary>
        /// Spawn objects (chests and enemies) based on maze data flags.
        /// </summary>
        public static void SpawnObjects(
            MazeData8 mazeData,
            GameObject chestPrefab,
            GameObject enemyPrefab,
            float cellSize,
            Transform objectsRoot)
        {
            if (mazeData == null)
            {
                Debug.LogError("[MazeObjectSpawner] Cannot spawn objects - mazeData is null!");
                return;
            }

            int chestsSpawned = 0;
            int enemiesSpawned = 0;

            for (int x = 0; x < mazeData.Width; x++)
            {
                for (int z = 0; z < mazeData.Height; z++)
                {
                    var cell = mazeData.GetCell(x, z);
                    Vector3 pos = new Vector3(
                        (x + 0.5f) * cellSize,
                        0f,
                        (z + 0.5f) * cellSize
                    );

                    // Spawn chest
                    if ((cell & CellFlags8.HasChest) != 0 && chestPrefab != null)
                    {
                        var chest = Object.Instantiate(chestPrefab, pos, Quaternion.identity);
                        if (chest != null)
                        {
                            chest.name = $"Chest_{x}_{z}";
                            chest.transform.SetParent(objectsRoot, false);
                            chestsSpawned++;
                        }
                    }

                    // Spawn enemy
                    if ((cell & CellFlags8.HasEnemy) != 0 && enemyPrefab != null)
                    {
                        var enemy = Object.Instantiate(enemyPrefab, pos, Quaternion.identity);
                        if (enemy != null)
                        {
                            enemy.name = $"Enemy_{x}_{z}";
                            enemy.transform.SetParent(objectsRoot, false);
                            enemiesSpawned++;
                        }
                    }
                }
            }

            Debug.Log($"[MazeObjectSpawner] Spawned {chestsSpawned} chests, {enemiesSpawned} enemies");
        }

        /// <summary>
        /// Find all valid object spawn positions in the maze.
        /// </summary>
        public static List<Vector3> FindSpawnPositions(
            MazeData8 mazeData,
            CellFlags8 flag,
            float cellSize)
        {
            var positions = new List<Vector3>();

            if (mazeData == null) return positions;

            for (int x = 0; x < mazeData.Width; x++)
            {
                for (int z = 0; z < mazeData.Height; z++)
                {
                    var cell = mazeData.GetCell(x, z);
                    if ((cell & flag) != 0)
                    {
                        positions.Add(new Vector3(
                            (x + 0.5f) * cellSize,
                            0f,
                            (z + 0.5f) * cellSize
                        ));
                    }
                }
            }

            return positions;
        }
    }
}
