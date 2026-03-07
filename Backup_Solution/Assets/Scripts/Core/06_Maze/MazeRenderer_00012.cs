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
// MazeRenderer.cs
// DEDICATED WALL RENDERING - Extracted from CompleteMazeBuilder
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-OUT: Finds components, never creates
// ALL VALUES FROM JSON: No hardcoded values
//
// RESPONSIBILITY:
// - Render walls from GridMazeCell data
// - Publish grid data to ComputeGridEngine
// - Manage wall prefabs and materials
//

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// MazeRenderer - Dedicated wall rendering system.
    /// Extracted from CompleteMazeBuilder for single responsibility.
    ///
    /// FEATURES:
    /// - Outer perimeter walls
    /// - Interior boundary walls
    /// - ComputeGrid integration (binary storage)
    /// - Material/texture application
    /// - 8-way wall support (N, NE, E, SE, S, SW, W, NW)
    ///
    /// PLUG-IN-OUT: Uses EventHandler for event publishing.
    /// </summary>
    public class MazeRenderer : MonoBehaviour
    {
        #region Wall Direction Enum

        /// <summary>
        /// 8-way wall directions with rotation angles.
        /// </summary>
        private enum WallDirection
        {
            North = 0,      // 0 degrees
            NorthEast = 1,  // 45 degrees
            East = 2,       // 90 degrees
            SouthEast = 3,  // 135 degrees
            South = 4,      // 180 degrees
            SouthWest = 5,  // 225 degrees
            West = 6,       // 270 degrees
            NorthWest = 7   // 315 degrees
        }

        #endregion
        #region Fields

        [Header("Prefabs")]
        [SerializeField] private GameObject wallPrefab;

        [Header("Materials")]
        [SerializeField] private Material wallMaterial;

        [Header("Dimensions")]
        [SerializeField] private float cellSize = 6f;
        [SerializeField] private float wallHeight = 4f;
        [SerializeField] private float wallThickness = 0.5f;

        #endregion

        #region Private Data

        private GridMazeGenerator grid;
        private int mazeSize;
        private uint seed;
        private int currentLevel;

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize renderer with grid data and prefabs.
        /// </summary>
        public void Initialize(GridMazeGenerator grid, int mazeSize, uint seed, int currentLevel, 
            GameObject wallPrefabRef, Material wallMatRef, float cellSizeVal, float wallHeightVal, float wallThicknessVal)
        {
            this.grid = grid;
            this.mazeSize = mazeSize;
            this.seed = seed;
            this.currentLevel = currentLevel;
            this.wallPrefab = wallPrefabRef;
            this.wallMaterial = wallMatRef;
            this.cellSize = cellSizeVal;
            this.wallHeight = wallHeightVal;
            this.wallThickness = wallThicknessVal;
        }

        #endregion

        #region Main Rendering

        /// <summary>
        /// Render all walls (outer + interior).
        /// Also publishes to ComputeGridEngine via EventHandler.
        /// </summary>
        public void RenderWalls()
        {
            if (grid == null)
            {
                Debug.LogError("[MazeRenderer] Grid not initialized!");
                return;
            }

            Debug.Log("[MazeRenderer] ========================================");
            Debug.Log("[MazeRenderer] Rendering walls from grid...");
            Debug.Log($"[MazeRenderer] Grid size: {mazeSize}x{mazeSize}, Cell size: {cellSize}m");

            int spawned = 0;

            // Destroy existing walls first
            var existingWalls = GameObject.Find("MazeWalls");
            if (existingWalls != null)
            {
                if (Application.isPlaying)
                    Destroy(existingWalls);
                else
                    DestroyImmediate(existingWalls);
            }

            GameObject parent = new GameObject("MazeWalls");

            // Build grid data for compute grid (byte-to-byte)
            byte[] gridBytes = BuildGridBytesForComputeGrid();

            // Publish to ComputeGridEngine via EventHandler (plug-in-out!)
            if (Application.isPlaying && EventHandler.Instance != null)
            {
                string mazeId = $"Maze_{currentLevel:D3}";
                EventHandler.Instance.InvokeComputeGridSaveRequested(mazeId, gridBytes, (int)seed);
                Debug.Log($"[MazeRenderer] Published compute grid save event: {mazeId}");
            }

            Debug.Log("[MazeRenderer] Placing outer perimeter walls...");
            PlaceOuterPerimeterWalls(parent.transform, ref spawned);
            Debug.Log($"[MazeRenderer] Outer walls placed: {spawned} walls");

            int interiorStart = spawned;
            Debug.Log("[MazeRenderer] Placing interior walls...");
            PlaceInteriorWalls(parent.transform, ref spawned);
            Debug.Log($"[MazeRenderer] Interior walls placed: {spawned - interiorStart} walls");

            Debug.Log($"[MazeRenderer] ========================================");
            Debug.Log($"[MazeRenderer] {spawned} wall segments placed TOTAL");
            Debug.Log($"[MazeRenderer] Wall dimensions: {cellSize}m x {wallHeight}m x {wallThickness}m");
            Debug.Log($"[MazeRenderer] ========================================");
        }

        #endregion

        #region Grid Data

        /// <summary>
        /// Build grid byte array from current grid state.
        /// This is the data that will be sent to ComputeGridEngine.
        /// </summary>
        private byte[] BuildGridBytesForComputeGrid()
        {
            byte[] gridBytes = new byte[mazeSize * mazeSize];
            int index = 0;

            for (int z = 0; z < mazeSize; z++)
            {
                for (int x = 0; x < mazeSize; x++)
                {
                    GridMazeCell cell = grid.GetCell(x, z);

                    // Map GridMazeCell to ComputeGridEngine.GridCell
                    byte computeCell = cell switch
                    {
                        GridMazeCell.Floor => (byte)ComputeGridEngine.GridCell.Floor,
                        GridMazeCell.Room => (byte)ComputeGridEngine.GridCell.Room,
                        GridMazeCell.Corridor => (byte)ComputeGridEngine.GridCell.Corridor,
                        GridMazeCell.Wall => (byte)ComputeGridEngine.GridCell.Wall,
                        GridMazeCell.SpawnPoint => (byte)ComputeGridEngine.GridCell.SpawnPoint,
                        _ => (byte)ComputeGridEngine.GridCell.Floor
                    };

                    gridBytes[index++] = computeCell;
                }
            }

            Debug.Log($"[MazeRenderer] Grid bytes built: {gridBytes.Length} bytes");
            return gridBytes;
        }

        #endregion

        #region Outer Perimeter Walls

        /// <summary>
        /// Place walls on outer perimeter (north, south, east, west).
        /// Walls are snapped to cell edges (boundaries), not centers.
        /// </summary>
        private void PlaceOuterPerimeterWalls(Transform parent, ref int spawned)
        {
            // NORTH WALL (Z = mazeSize * cellSize) - on outer edge
            for (int x = 0; x < mazeSize; x++)
            {
                Vector3 pos = new Vector3(
                    x * cellSize + cellSize / 2f,
                    wallHeight / 2f,
                    mazeSize * cellSize
                );
                SpawnWall(pos, Quaternion.identity, $"North_{x}", parent);
                spawned++;
            }

            // SOUTH WALL (Z = 0) - on outer edge
            for (int x = 0; x < mazeSize; x++)
            {
                Vector3 pos = new Vector3(
                    x * cellSize + cellSize / 2f,
                    wallHeight / 2f,
                    0f
                );
                SpawnWall(pos, Quaternion.identity, $"South_{x}", parent);
                spawned++;
            }

            // EAST WALL (X = mazeSize * cellSize) - on outer edge
            for (int z = 0; z < mazeSize; z++)
            {
                Vector3 pos = new Vector3(
                    mazeSize * cellSize,
                    wallHeight / 2f,
                    z * cellSize + cellSize / 2f
                );
                SpawnWall(pos, Quaternion.Euler(0f, 90f, 0f), $"East_{z}", parent);
                spawned++;
            }

            // WEST WALL (X = 0) - on outer edge
            for (int z = 0; z < mazeSize; z++)
            {
                Vector3 pos = new Vector3(
                    0f,
                    wallHeight / 2f,
                    z * cellSize + cellSize / 2f
                );
                SpawnWall(pos, Quaternion.Euler(0f, 90f, 0f), $"West_{z}", parent);
                spawned++;
            }
        }

        #endregion

        #region Interior Walls

        /// <summary>
        /// Place interior walls between adjacent cells (Room/Corridor vs Floor).
        /// Walls are snapped to cell edges (grid lines), not cell centers.
        /// Checks all 4 directions: North, South, East, West.
        /// </summary>
        private void PlaceInteriorWalls(Transform parent, ref int spawned)
        {
            int wallPlacedCount = 0;
            int skippedCount = 0;

            for (int x = 0; x < mazeSize; x++)
            {
                for (int y = 0; y < mazeSize; y++)
                {
                    GridMazeCell current = grid.GetCell(x, y);

                    // Only check boundaries from Room/Corridor cells
                    if (current != GridMazeCell.Room && current != GridMazeCell.Corridor)
                    {
                        skippedCount++;
                        continue;
                    }

                    // Check NORTH boundary - wall on horizontal grid line at y
                    if (y > 0)
                    {
                        GridMazeCell north = grid.GetCell(x, y - 1);
                        if (NeedsWallBetween(current, north))
                        {
                            Vector3 pos = new Vector3(
                                x * cellSize + cellSize / 2f,
                                wallHeight / 2f,
                                y * cellSize
                            );
                            SpawnWall(pos, Quaternion.identity, $"Wall_N_{x}_{y}", parent);
                            spawned++;
                            wallPlacedCount++;
                        }
                    }

                    // Check SOUTH boundary - wall on horizontal grid line at y+1
                    if (y + 1 < mazeSize)
                    {
                        GridMazeCell south = grid.GetCell(x, y + 1);
                        if (NeedsWallBetween(current, south))
                        {
                            Vector3 pos = new Vector3(
                                x * cellSize + cellSize / 2f,
                                wallHeight / 2f,
                                (y + 1) * cellSize
                            );
                            SpawnWall(pos, Quaternion.identity, $"Wall_S_{x}_{y}", parent);
                            spawned++;
                            wallPlacedCount++;
                        }
                    }

                    // Check WEST boundary - wall on vertical grid line at x
                    if (x > 0)
                    {
                        GridMazeCell west = grid.GetCell(x - 1, y);
                        if (NeedsWallBetween(current, west))
                        {
                            Vector3 pos = new Vector3(
                                x * cellSize,
                                wallHeight / 2f,
                                y * cellSize + cellSize / 2f
                            );
                            SpawnWall(pos, Quaternion.Euler(0f, 90f, 0f), $"Wall_W_{x}_{y}", parent);
                            spawned++;
                            wallPlacedCount++;
                        }
                    }

                    // Check EAST boundary - wall on vertical grid line at x+1
                    if (x + 1 < mazeSize)
                    {
                        GridMazeCell east = grid.GetCell(x + 1, y);
                        if (NeedsWallBetween(current, east))
                        {
                            Vector3 pos = new Vector3(
                                (x + 1) * cellSize,
                                wallHeight / 2f,
                                y * cellSize + cellSize / 2f
                            );
                            SpawnWall(pos, Quaternion.Euler(0f, 90f, 0f), $"Wall_E_{x}_{y}", parent);
                            spawned++;
                            wallPlacedCount++;
                        }
                    }
                }
            }

            Debug.Log($"[MazeRenderer]  Interior walls: {wallPlacedCount} placed, {skippedCount} skipped (not room/corridor)");
        }

        /// <summary>
        /// Determine if wall needed between two adjacent cells.
        /// </summary>
        private bool NeedsWallBetween(GridMazeCell a, GridMazeCell b)
        {
            bool aWalkable = (a == GridMazeCell.Room || a == GridMazeCell.Corridor);
            bool bWalkable = (b == GridMazeCell.Room || b == GridMazeCell.Corridor);
            return aWalkable != bWalkable;
        }

        #endregion

        #region Wall Spawning

        /// <summary>
        /// Get rotation angle for wall direction.
        /// </summary>
        private float GetWallAngle(WallDirection direction)
        {
            switch (direction)
            {
                case WallDirection.North:     return 0f;
                case WallDirection.NorthEast: return 45f;
                case WallDirection.East:      return 90f;
                case WallDirection.SouthEast: return 135f;
                case WallDirection.South:     return 180f;
                case WallDirection.SouthWest: return 225f;
                case WallDirection.West:      return 270f;
                case WallDirection.NorthWest: return 315f;
                default:                      return 0f;
            }
        }

        /// <summary>
        /// Spawn single wall segment at specified position with 8-way rotation support.
        /// Wall prefab should be sized to match cellSize (width=cellSize, height=wallHeight, depth=wallThickness).
        /// Wall is positioned on cell edge (snapped to grid line).
        /// </summary>
        private void SpawnWall(Vector3 pos, Quaternion rot, string name, Transform parent)
        {
            if (wallPrefab == null)
            {
                Debug.LogError($"[MazeRenderer] Wall prefab not loaded! Cannot spawn wall at {pos}");
                Debug.LogError("[MazeRenderer] Fix: Run Tools -> Quick Setup Prefabs");
                Debug.LogError("[MazeRenderer] Wall prefab should be sized: width=cellSize, height=wallHeight, depth=wallThickness");
                return;
            }

            Debug.Log($"[MazeRenderer]  Spawning wall: {wallPrefab.name} at {pos}");

            GameObject wall = Instantiate(wallPrefab, pos, rot);
            wall.name = $"Wall_{name}";
            wall.transform.SetParent(parent);

            // Scale wall to match cell dimensions if needed
            var transform1 = wall.transform;
            transform1.localScale = new Vector3(cellSize, wallHeight, wallThickness);
            Debug.Log($"[MazeRenderer]  Wall scaled to: {cellSize}m x {wallHeight}m x {wallThickness}m");

            if (wallMaterial != null)
            {
                var r = wall.GetComponent<MeshRenderer>();
                if (r != null)
                {
                    r.sharedMaterial = wallMaterial;
                    Debug.Log($"[MazeRenderer]  Wall material applied: {wallMaterial.name}");
                }
                else
                {
                    Debug.LogWarning($"[MazeRenderer]  Wall {name} has no MeshRenderer");
                }
            }
            else
            {
                Debug.LogWarning($"[MazeRenderer]  Wall material not loaded - using prefab default");
            }
        }

        /// <summary>
        /// Spawn wall with 8-way directional rotation.
        /// </summary>
        private void SpawnWall(Vector3 pos, WallDirection direction, string name, Transform parent)
        {
            float angle = GetWallAngle(direction);
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
            SpawnWall(pos, rotation, name, parent);
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Destroy object in play mode or editor.
        /// </summary>
        private void DestroyImmediate(GameObject obj)
        {
            if (obj != null)
                Destroy(obj);
        }

        #endregion
    }
}
