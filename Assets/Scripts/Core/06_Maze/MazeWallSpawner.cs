// Copyright (C) 2026 Ocxyde
// GPL-3.0 license - see COPYING
// MazeWallSpawner.cs - Handles spawning of cardinal and diagonal walls
// Part of modular CompleteMazeBuilder8 refactoring

using UnityEngine;
using Code.Lavos.Core.Advanced;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Handles spawning of cardinal and diagonal walls in the maze.
    /// Extracted from CompleteMazeBuilder8 for better separation of concerns.
    /// </summary>
    public static class MazeWallSpawner
    {
        /// <summary>
        /// Spawn a cardinal wall (N, S, E, W) at the edge between two cells.
        /// </summary>
        public static void SpawnCardinalWall(
            int cx, int cz, Direction8 dir,
            GameObject wallPrefab, Material wallMaterial,
            float cellSize, float wallHeight, float wallThickness,
            bool wallPivotIsAtMeshCenter, Transform wallsRoot)
        {
            if (wallPrefab == null)
            {
                Debug.LogError("[MazeWallSpawner] Cannot spawn cardinal wall - prefab is null!");
                return;
            }

            // Calculate wall position at edge between cells
            var (dx, dz) = Direction8Helper.ToOffset(dir);
            Vector3 edgePos = new Vector3(
                (cx + 0.5f + dx * 0.5f) * cellSize,
                0f,
                (cz + 0.5f + dz * 0.5f) * cellSize
            );

            // Rotate wall to face correct direction
            Quaternion rot = (dir == Direction8.E || dir == Direction8.W)
                ? Quaternion.Euler(0f, 90f, 0f)
                : Quaternion.identity;

            // Instantiate wall
            var wall = Object.Instantiate(wallPrefab, edgePos, rot, wallsRoot);
            if (wall == null)
            {
                Debug.LogError($"[MazeWallSpawner] Failed to instantiate cardinal wall at {edgePos}");
                return;
            }

            // Scale: X = length along wall, Y = height, Z = thickness
            wall.transform.localScale = new Vector3(cellSize, wallHeight, wallThickness);

            // Apply material if assigned
            if (wallMaterial != null)
            {
                var renderer = wall.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = wallMaterial;
                }
            }

            // If the prefab pivot is at mesh center, offset Y so wall sits on floor
            if (wallPivotIsAtMeshCenter)
            {
                var p = wall.transform.position;
                wall.transform.position = new Vector3(p.x, wallHeight * 0.5f, p.z);
            }

            // Ensure wall has a collider for collision
            BoxCollider collider = wall.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = wall.AddComponent<BoxCollider>();
            }
            collider.enabled = true;
            collider.isTrigger = false;
        }

        /// <summary>
        /// Spawn a diagonal wall (NE, NW, SE, SW) at a corner.
        /// </summary>
        public static void SpawnDiagonalWall(
            int cx, int cz, Direction8 dir,
            GameObject wallDiagPrefab, Material wallDiagMaterial,
            float cellSize, float wallHeight, float diagonalWallThickness,
            bool wallPivotIsAtMeshCenter, Transform wallsRoot)
        {
            if (wallDiagPrefab == null)
            {
                Debug.LogError("[MazeWallSpawner] Cannot spawn diagonal wall - prefab is null!");
                return;
            }

            float h = cellSize * 0.5f;
            Vector3 offset;
            float rotY;

            switch (dir)
            {
                case Direction8.NE: offset = new Vector3( h, 0f,  h); rotY =  45f; break;
                case Direction8.NW: offset = new Vector3(-h, 0f,  h); rotY = -45f; break;
                case Direction8.SE: offset = new Vector3( h, 0f, -h); rotY = -45f; break;
                case Direction8.SW: offset = new Vector3(-h, 0f, -h); rotY =  45f; break;
                default: return;
            }

            Vector3 cornerPos = new Vector3(
                (cx + 0.5f) * cellSize + offset.x,
                0f,
                (cz + 0.5f) * cellSize + offset.z
            );

            var wall = Object.Instantiate(
                wallDiagPrefab,
                cornerPos,
                Quaternion.Euler(0f, rotY, 0f),
                wallsRoot);

            if (wall == null)
            {
                Debug.LogError($"[MazeWallSpawner] Failed to instantiate diagonal wall at {cornerPos}");
                return;
            }

            float diagLength = cellSize * Mathf.Sqrt(2f);
            wall.transform.localScale = new Vector3(diagLength, wallHeight, diagonalWallThickness);

            if (wallDiagMaterial != null)
            {
                var renderer = wall.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = wallDiagMaterial;
                }
            }

            if (wallPivotIsAtMeshCenter)
            {
                var p = wall.transform.position;
                wall.transform.position = new Vector3(p.x, wallHeight * 0.5f, p.z);
            }
        }
    }
}
