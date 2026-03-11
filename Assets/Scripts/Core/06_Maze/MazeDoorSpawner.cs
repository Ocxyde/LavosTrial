// Copyright (C) 2026 Ocxyde
// GPL-3.0 license - see COPYING
// MazeDoorSpawner.cs - Handles door spawning in maze access points

using UnityEngine;
using Code.Lavos.Core.Advanced;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Handles spawning of doors on access walls in the maze.
    /// Doors are placed at open passages (where there's no wall).
    /// PLUG-IN-OUT: Works with MazeData8 through EventHandler events.
    /// </summary>
    public static class MazeDoorSpawner
    {
        /// <summary>
        /// Spawn doors at ALL open passages in the maze.
        /// Each corridor connection gets a door.
        /// </summary>
        public static void SpawnAllDoors(
            MazeData8 mazeData,
            GameObject normalDoorPrefab,
            GameObject lockedDoorPrefab,
            GameObject secretDoorPrefab,
            float cellSize, float wallHeight, float wallThickness,
            bool wallPivotIsAtMeshCenter,
            float lockedDoorChance = 0.3f,
            float secretDoorChance = 0.1f)
        {
            if (normalDoorPrefab == null)
            {
                Debug.LogError("[MazeDoorSpawner] Normal door prefab is null!");
                return;
            }

            if (mazeData == null)
            {
                Debug.LogError("[MazeDoorSpawner] Cannot spawn doors - mazeData is null!");
                return;
            }

            int doorCount = 0;
            int normalCount = 0;
            int lockedCount = 0;
            int secretCount = 0;

            // Scan all cells for open passages
            for (int x = 0; x < mazeData.Width; x++)
            {
                for (int z = 0; z < mazeData.Height; z++)
                {
                    var cell = mazeData.GetCell(x, z);
                    
                    // Skip if this cell is a wall
                    if ((cell & CellFlags8.AllWalls) != CellFlags8.None)
                        continue;

                    // Check all 4 cardinal directions for open passages
                    Direction8[] cardinals = { Direction8.N, Direction8.S, Direction8.E, Direction8.W };

                    foreach (var dir in cardinals)
                    {
                        // Skip if there's a wall in this direction
                        if (mazeData.HasWall(x, z, dir))
                            continue;

                        // Get neighbor cell
                        var (dx, dz) = Direction8Helper.ToOffset(dir);
                        int nx = x + dx;
                        int nz = z + dz;
                        
                        if (!mazeData.InBounds(nx, nz))
                            continue;

                        // Check if neighbor is also walkable (corridor connection)
                        var neighborCell = mazeData.GetCell(nx, nz);
                        if ((neighborCell & CellFlags8.AllWalls) != CellFlags8.None)
                            continue;

                        // Avoid duplicate doors (only spawn when x < nx or z < nz)
                        if (nx > x || (nx == x && nz > z))
                            continue;

                        // Determine door type based on random chance
                        GameObject doorPrefab = normalDoorPrefab;
                        string doorType = "Normal";
                        float roll = Random.value;

                        if (roll < secretDoorChance && secretDoorPrefab != null)
                        {
                            doorPrefab = secretDoorPrefab;
                            doorType = "Secret";
                            secretCount++;
                        }
                        else if (roll < secretDoorChance + lockedDoorChance && lockedDoorPrefab != null)
                        {
                            doorPrefab = lockedDoorPrefab;
                            doorType = "Locked";
                            lockedCount++;
                        }
                        else
                        {
                            normalCount++;
                        }

                        // Calculate door position at wall edge between cells
                        float doorX = (x + nx) * 0.5f * cellSize;
                        float doorZ = (z + nz) * 0.5f * cellSize;
                        var wallEdgePos = new Vector3(doorX, 0f, doorZ);

                        // Door rotation: N/S doors face east-west, E/W doors face north-south
                        float rotY = (dir == Direction8.E || dir == Direction8.W) ? 90f : 0f;

                        var door = Object.Instantiate(
                            doorPrefab,
                            wallEdgePos,
                            Quaternion.Euler(0f, rotY, 0f));

                        if (door == null)
                        {
                            Debug.LogError($"[MazeDoorSpawner] Failed to instantiate door at {wallEdgePos}");
                            continue;
                        }

                        door.name = $"Door_{doorType}_{doorCount}";
                        door.transform.localScale = new Vector3(cellSize, wallHeight, wallThickness);

                        if (wallPivotIsAtMeshCenter)
                        {
                            var p = door.transform.position;
                            door.transform.position = new Vector3(p.x, wallHeight * 0.5f, p.z);
                        }

                        doorCount++;
                    }
                }
            }

            Debug.Log($"[MazeDoorSpawner] Spawned {doorCount} doors total: {normalCount} normal, {lockedCount} locked, {secretCount} secret");
        }

        /// <summary>
        /// Spawn doors at spawn and exit room access points (legacy method).
        /// </summary>
        [System.Obsolete("Use SpawnAllDoors instead")]
        public static void SpawnDoors(
            MazeData8 mazeData,
            GameObject doorPrefab,
            float cellSize, float wallHeight, float wallThickness,
            bool wallPivotIsAtMeshCenter)
        {
            if (doorPrefab == null)
            {
                Debug.LogError("[MazeDoorSpawner] Cannot spawn doors - prefab is null!");
                return;
            }

            if (mazeData == null)
            {
                Debug.LogError("[MazeDoorSpawner] Cannot spawn doors - mazeData is null!");
                return;
            }

            // Spawn door at spawn room
            SpawnDoorAtRoom(mazeData.SpawnCell.x, mazeData.SpawnCell.z, "Door_Entry",
                mazeData, doorPrefab, cellSize, wallHeight, wallThickness, wallPivotIsAtMeshCenter);

            // Spawn door at exit room
            SpawnDoorAtRoom(mazeData.ExitCell.x, mazeData.ExitCell.z, "Door_Exit",
                mazeData, doorPrefab, cellSize, wallHeight, wallThickness, wallPivotIsAtMeshCenter);

            Debug.Log("[MazeDoorSpawner] Spawned entry and exit doors");
        }

        /// <summary>
        /// Spawn a door at the first open passage from a room cell.
        /// </summary>
        private static void SpawnDoorAtRoom(
            int cx, int cz, string doorName,
            MazeData8 mazeData,
            GameObject doorPrefab,
            float cellSize, float wallHeight, float wallThickness,
            bool wallPivotIsAtMeshCenter)
        {
            // Scan cardinal directions for first open passage
            Direction8[] cardinals = { Direction8.N, Direction8.S, Direction8.E, Direction8.W };

            foreach (var dir in cardinals)
            {
                // Skip if wall is present (door requires an open passage)
                if (mazeData.HasWall(cx, cz, dir)) continue;

                var (dx, dz) = Direction8Helper.ToOffset(dir);
                int nx = cx + dx;
                int nz = cz + dz;
                if (!mazeData.InBounds(nx, nz)) continue;

                // Calculate door position at wall edge
                var wallEdgePos = new Vector3(
                    (cx + 0.5f + dx * 0.5f) * cellSize,
                    0f,
                    (cz + 0.5f + dz * 0.5f) * cellSize
                );

                // Door faces the corridor: N/S doors rotate 0 deg, E/W doors rotate 90 deg
                float rotY = (dir == Direction8.E || dir == Direction8.W) ? 90f : 0f;

                var door = Object.Instantiate(
                    doorPrefab,
                    wallEdgePos,
                    Quaternion.Euler(0f, rotY, 0f));

                if (door == null)
                {
                    Debug.LogError($"[MazeDoorSpawner] Failed to instantiate door at {wallEdgePos}");
                    continue;
                }

                door.name = doorName;
                door.transform.localScale = new Vector3(cellSize, wallHeight, wallThickness);

                if (wallPivotIsAtMeshCenter)
                {
                    var p = door.transform.position;
                    door.transform.position = new Vector3(p.x, wallHeight * 0.5f, p.z);
                }

                // One door per room - stop after first open direction
                break;
            }
        }
    }
}
