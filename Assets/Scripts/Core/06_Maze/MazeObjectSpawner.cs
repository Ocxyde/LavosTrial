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
        /// Torches SNAP to wall surface at mid-position, facing INWARD toward walkable corridor.
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
            float torchYOffset = 0.5f; // Slightly above mid-wall (better visibility)

            for (int x = 0; x < mazeData.Width; x++)
            {
                for (int z = 0; z < mazeData.Height; z++)
                {
                    var cell = mazeData.GetCell(x, z);

                    if ((cell & CellFlags8.HasTorch) != 0)
                    {
                        // Find walkable adjacent cell (corridor side)
                        (Vector3 dir, int wallX, int wallZ) = FindWalkableDirection(mazeData, x, z);
                        
                        // If no walkable direction found, skip this torch
                        if (dir == Vector3.zero)
                        {
                            Debug.LogWarning($"[MazeObjectSpawner] Torch at ({x},{z}) has no walkable side - skipping!");
                            continue;
                        }

                        // Position: CENTER of wall cell, mid-height
                        // Wall cell is BETWEEN current cell (x,z) and walkable neighbor
                        int torchCellX = wallX;
                        int torchCellZ = wallZ;
                        
                        Vector3 pos = new Vector3(
                            (torchCellX + 0.5f) * cellSize,  // Mid-position X
                            wallHeight / 2f + torchYOffset,   // Mid-height + offset
                            (torchCellZ + 0.5f) * cellSize   // Mid-position Z
                        );

                        // Snap torch to wall surface (flush, not floating)
                        // Place in wall cell, facing OUTWARD into corridor
                        float snapOffset = 0.15f; // Snap to wall surface
                        pos -= dir * snapOffset; // Move INTO wall cell (not walkable)

                        // Rotate torch to face OUTWARD into the corridor
                        // TORCH.fbx default forward (Z+) should face away from wall
                        Quaternion rotation;
                        if (dir == Vector3.forward)      // North wall → face north (outward)
                            rotation = Quaternion.identity;
                        else if (dir == -Vector3.forward) // South wall → face south (outward)
                            rotation = Quaternion.Euler(0f, 180f, 0f);
                        else if (dir == Vector3.right)    // East wall → face east (outward)
                            rotation = Quaternion.Euler(0f, 90f, 0f);
                        else if (dir == -Vector3.right)   // West wall → face west (outward)
                            rotation = Quaternion.Euler(0f, -90f, 0f);
                        else
                            rotation = Quaternion.identity;

                        var torch = Object.Instantiate(torchPrefab, pos, rotation);
                        if (torch != null)
                        {
                            torch.name = $"Torch_{x}_{z}";
                            torch.transform.SetParent(objectsRoot, false);
                            torchCount++;
                            
                            // Debug: Log torch position and rotation
                            Debug.Log($"[MazeObjectSpawner] Torch spawned at ({x},{z}): pos={pos:F2}, dir={dir}, rot={rotation.eulerAngles} (OUTWARD, facing corridor)");
                        }
                    }
                }
            }

            Debug.Log($"[MazeObjectSpawner] Spawned {torchCount} torches (snapped to walls, facing INWARD)");
        }

        /// <summary>
        /// Find the walkable adjacent cell direction (N/S/E/W).
        /// Returns direction vector AND the wall cell position (between current and neighbor).
        /// </summary>
        private static (Vector3 dir, int x, int z) FindWalkableDirection(MazeData8 mazeData, int x, int z)
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
                    // Return current cell position (torch spawns here, facing neighbor)
                    return (dir, x, z);
                }
            }

            return (Vector3.zero, 0, 0); // No walkable direction found
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
