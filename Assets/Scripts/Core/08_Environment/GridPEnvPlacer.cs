// Copyright (C) 2026 Ocxyde
//
// This file is part of PeuImporte.
//
// PeuImporte is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PeuImporte is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PeuImporte.  If not, see <https://www.gnu.org/licenses/>.
//
// GridPEnvPlacer.cs
// Grid-based environment placer - walls snap to cell borders
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-OUT ARCHITECTURE:
// - Finds CompleteMazeBuilder (never creates)
// - Places walls on exact cell borders
// - All positions saved byte-to-byte in RAM
//
// LOCATION: Assets/Scripts/Core/08_Environment/

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// GridPEnvPlacer - Grid-based environment placer.
    /// Places walls on EXACT cell borders for perfect snapping.
    /// PLUG-IN-OUT COMPLIANT: Finds CompleteMazeBuilder, never creates.
    /// </summary>
    public class GridPEnvPlacer : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Component References (Plug-in-Out)")]
        [Tooltip("Auto-finds CompleteMazeBuilder")]
        [SerializeField] private CompleteMazeBuilder mazeBuilder;

        [Header("Wall Settings")]
        [Tooltip("Wall prefab to instantiate")]
        [SerializeField] private GameObject wallPrefab;

        [Tooltip("Wall material")]
        [SerializeField] private Material wallMaterial;

        [Tooltip("Wall height in meters")]
        [SerializeField] private float wallHeight = 4.0f;

        [Tooltip("Cell size in meters")]
        [SerializeField] private float cellSize = 6.0f;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        #endregion

        #region Private Data

        private GridMazeGenerator grid;
        private int mazeSize;
        private List<Vector3> wallPositions;  // Byte-to-byte RAM storage

        #endregion

        #region Public Accessors

        public List<Vector3> WallPositions => wallPositions;
        public int WallsPlaced => wallPositions?.Count ?? 0;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            FindComponents();
            wallPositions = new List<Vector3>();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Find CompleteMazeBuilder (plug-in-out: never create).
        /// </summary>
        private void FindComponents()
        {
            if (mazeBuilder == null)
                mazeBuilder = FindFirstObjectByType<CompleteMazeBuilder>();

            if (mazeBuilder == null)
            {
                Debug.LogError("[GridPEnvPlacer]  CompleteMazeBuilder not found!");
                return;
            }

            Log("[GridPEnvPlacer]  Found CompleteMazeBuilder");
        }

        #endregion

        #region Wall Placement

        /// <summary>
        /// Place walls on N/W borders of wall cells.
        /// Walls snap to EXACT cell borders for perfect alignment.
        /// Positions saved byte-to-byte in RAM for torch placement.
        /// </summary>
        public void PlaceWalls()
        {
            if (mazeBuilder == null)
            {
                Debug.LogError("[GridPEnvPlacer]  CompleteMazeBuilder not found!");
                return;
            }

            // Get grid from maze builder via public API (plug-in-out compliant)
            grid = mazeBuilder.GetGrid();
            if (grid == null)
            {
                Debug.LogError("[GridPEnvPlacer]  Grid not generated!");
                return;
            }

            mazeSize = grid.gridSize;

            Log("[GridPEnvPlacer]  Placing walls on N/W borders (exact snap)...");

            // Clear existing positions
            if (wallPositions == null)
                wallPositions = new List<Vector3>();
            wallPositions.Clear();

            int spawned = 0;

            // Destroy existing walls
            var existingWalls = GameObject.Find("MazeWalls");
            if (existingWalls != null)
            {
                if (Application.isPlaying)
                    Destroy(existingWalls);
                else
                    DestroyImmediate(existingWalls);
            }

            GameObject parent = new GameObject("MazeWalls");

            // Place walls on N/W borders
            for (int x = 0; x < mazeSize; x++)
            {
                for (int y = 0; y < mazeSize; y++)
                {
                    if (grid.GetCell(x, y) == GridMazeCell.Wall)
                    {
                        // Check North border (cell above is NOT wall)
                        bool needsNorth = (y + 1 >= mazeSize || grid.GetCell(x, y + 1) != GridMazeCell.Wall);

                        // Check West border (cell left is NOT wall)
                        bool needsWest = (x - 1 < 0 || grid.GetCell(x - 1, y) != GridMazeCell.Wall);

                        // Place North border wall (horizontal)
                        if (needsNorth)
                        {
                            // EXACT border position: z = (y + 1) * cellSize
                            Vector3 pos = new Vector3(
                                x * cellSize + cellSize / 2f,  // Center of cell in X
                                wallHeight / 2f,
                                (y + 1) * cellSize              // EXACT border in Z
                            );
                            Quaternion rot = Quaternion.identity;

                            // Save byte-to-byte in RAM
                            wallPositions.Add(pos);

                            SpawnWall(pos, rot, $"Wall_{x}_{y}_N", parent.transform);
                            spawned++;
                        }

                        // Place West border wall (vertical)
                        if (needsWest)
                        {
                            // EXACT border position: x = x * cellSize
                            Vector3 pos = new Vector3(
                                x * cellSize,                   // EXACT border in X
                                wallHeight / 2f,
                                y * cellSize + cellSize / 2f    // Center of cell in Z
                            );
                            Quaternion rot = Quaternion.Euler(0f, 90f, 0f);

                            // Save byte-to-byte in RAM
                            wallPositions.Add(pos);

                            SpawnWall(pos, rot, $"Wall_{x}_{y}_W", parent.transform);
                            spawned++;
                        }
                    }
                }
            }

            Log($"[GridPEnvPlacer]  {spawned} walls placed (N/W borders, exact snap)");
            Log($"[GridPEnvPlacer]  Positions saved in RAM: {wallPositions.Count}");
        }

        private void SpawnWall(Vector3 pos, Quaternion rot, string name, Transform parent)
        {
            if (wallPrefab == null)
            {
                Debug.LogError("[GridPEnvPlacer]  Wall prefab not assigned!");
                return;
            }

            GameObject wall = Instantiate(wallPrefab, pos, rot);
            wall.name = name;
            wall.transform.SetParent(parent);

            if (wallMaterial != null)
            {
                var renderer = wall.GetComponent<MeshRenderer>();
                if (renderer != null)
                    renderer.sharedMaterial = wallMaterial;
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Clear all wall positions from RAM.
        /// </summary>
        public void ClearWallPositions()
        {
            if (wallPositions != null)
                wallPositions.Clear();
        }

        /// <summary>
        /// Get wall positions for torch placement (byte-to-byte from RAM).
        /// </summary>
        public List<Vector3> GetWallPositions()
        {
            return wallPositions;
        }

        #endregion

        #region Debug

        private void Log(string message)
        {
            if (showDebugLogs)
                Debug.Log(message);
        }

        #endregion
    }
}
