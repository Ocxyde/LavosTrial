﻿﻿﻿﻿// Copyright (C) 2026 Ocxyde
// GPL-3.0 license - see COPYING
// MazeObjectSpawner.cs - Handles spawning of torches, chests, and enemies

using System.Collections.Generic;
using UnityEngine;
using Code.Lavos.Core.Advanced;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Handles spawning of objects (torches, chests, enemies) in the maze.
    /// </summary>
    public static class MazeObjectSpawner
    {
        /// <summary>
        /// Spawn torches in the maze based on torch flags in maze data.
        /// </summary>
        public static void SpawnTorches(
            DungeonMazeData mazeData,
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

            for (int x = 0; x < mazeData.Width; x++)
            {
                for (int z = 0; z < mazeData.Height; z++)
                {
                    var cell = mazeData.GetCell(x, z);

                    if ((cell & (uint)CellFlags8.HasTorch) != 0)
                    {
                        Vector3 pos = new Vector3(
                            (x + 0.5f) * cellSize,
                            0f,
                            (z + 0.5f) * cellSize
                        );

                        var torch = Object.Instantiate(torchPrefab, pos, Quaternion.identity);
                        if (torch != null)
                        {
                            torch.name = $"Torch_{x}_{z}";
                            torch.transform.SetParent(objectsRoot, false);
                            torchCount++;
                        }
                    }
                }
            }

            Debug.Log($"[MazeObjectSpawner] Spawned {torchCount} torches");
        }

        /// <summary>
        /// Spawn objects (chests and enemies) based on maze data flags.
        /// </summary>
        public static void SpawnObjects(
            DungeonMazeData mazeData,
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
                    if ((cell & (uint)CellFlags8.HasChest) != 0 && chestPrefab != null)
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
                    if ((cell & (uint)CellFlags8.HasEnemy) != 0 && enemyPrefab != null)
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
            DungeonMazeData mazeData,
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
                    if ((cell & (uint)flag) != 0)
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
