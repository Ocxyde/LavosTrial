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
        /// Spawn doors at spawn and exit room access points.
        /// </summary>
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
