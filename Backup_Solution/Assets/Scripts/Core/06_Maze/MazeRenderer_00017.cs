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
// 8-axis wall rendering system
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// MazeRenderer - 8-axis wall rendering system.
    /// Renders cardinal and diagonal walls from MazeData8.
    /// </summary>
    public class MazeRenderer : MonoBehaviour
    {
        #region Fields

        [Header("Prefabs")]
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private GameObject wallDiagPrefab;
        [SerializeField] private GameObject wallCornerPrefab;

        [Header("Materials")]
        [SerializeField] private Material wallMaterial;

        [Header("Dimensions")]
        [SerializeField] private float cellSize = 6f;
        [SerializeField] private float wallHeight = 4f;
        [SerializeField] private float wallThickness = 0.5f;

        #endregion

        #region Private Data

        private MazeData8 mazeData;
        private int mazeSize;
        private uint seed;
        private int currentLevel;

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize renderer with MazeData8 and prefabs.
        /// </summary>
        public void Initialize(MazeData8 data, int mazeSize, uint seed, int currentLevel,
            GameObject wallPrefabRef, GameObject wallDiagPrefabRef, Material wallMatRef,
            float cellSizeVal, float wallHeightVal, float wallThicknessVal)
        {
            this.mazeData = data;
            this.mazeSize = mazeSize;
            this.seed = seed;
            this.currentLevel = currentLevel;
            this.wallPrefab = wallPrefabRef;
            this.wallDiagPrefab = wallDiagPrefabRef;
            this.wallMaterial = wallMatRef;
            this.cellSize = cellSizeVal;
            this.wallHeight = wallHeightVal;
            this.wallThickness = wallThicknessVal;
        }

        #endregion

        #region Main Rendering

        /// <summary>
        /// Render all walls (cardinal + diagonal).
        /// </summary>
        public void RenderWalls()
        {
            if (mazeData == null)
            {
                Debug.LogError("[MazeRenderer] MazeData8 not initialized!");
                return;
            }

            Debug.Log("[MazeRenderer] Rendering 8-axis walls...");

            // Destroy existing walls
            var existingWalls = GameObject.Find("MazeWalls");
            if (existingWalls != null)
            {
                if (Application.isPlaying)
                    Destroy(existingWalls);
                else
                    DestroyImmediate(existingWalls);
            }

            // Find or create walls parent (plug-in-out: try to find existing first)
            GameObject parent = GameObject.Find("MazeWalls");
            if (parent == null)
            {
                parent = new GameObject("MazeWalls");
            }

            int cardinalCount = 0;
            int diagonalCount = 0;

            // Render walls for each cell
            for (int x = 0; x < mazeData.Width; x++)
            {
                for (int z = 0; z < mazeData.Height; z++)
                {
                    var cell = mazeData.GetCell(x, z);

                    // Skip if all walls (boundary)
                    if ((cell & CellFlags8.AllWalls) == CellFlags8.AllWalls)
                        continue;

                    float wx = x * cellSize;
                    float wz = z * cellSize;

                    // Cardinal walls
                    if ((cell & CellFlags8.WallN) != 0) { SpawnWall(x, z, wx, wz, Direction8.N, parent.transform); cardinalCount++; }
                    if ((cell & CellFlags8.WallE) != 0) { SpawnWall(x, z, wx, wz, Direction8.E, parent.transform); cardinalCount++; }
                    if ((cell & CellFlags8.WallS) != 0) { SpawnWall(x, z, wx, wz, Direction8.S, parent.transform); cardinalCount++; }
                    if ((cell & CellFlags8.WallW) != 0) { SpawnWall(x, z, wx, wz, Direction8.W, parent.transform); cardinalCount++; }

                    // Diagonal walls
                    if ((cell & CellFlags8.WallNE) != 0) { SpawnWall(x, z, wx, wz, Direction8.NE, parent.transform); diagonalCount++; }
                    if ((cell & CellFlags8.WallNW) != 0) { SpawnWall(x, z, wx, wz, Direction8.NW, parent.transform); diagonalCount++; }
                    if ((cell & CellFlags8.WallSE) != 0) { SpawnWall(x, z, wx, wz, Direction8.SE, parent.transform); diagonalCount++; }
                    if ((cell & CellFlags8.WallSW) != 0) { SpawnWall(x, z, wx, wz, Direction8.SW, parent.transform); diagonalCount++; }
                }
            }

            Debug.Log($"[MazeRenderer] Walls rendered: {cardinalCount} cardinal, {diagonalCount} diagonal");

            // Publish to ComputeGrid via EventHandler
            if (Application.isPlaying && EventHandler.Instance != null)
            {
                Debug.Log("[MazeRenderer] Publishing grid data to ComputeGrid via EventHandler");
            }
        }

        /// <summary>
        /// Spawn single wall segment.
        /// </summary>
        private void SpawnWall(int x, int z, float wx, float wz, Direction8 dir, Transform parent)
        {
            var (dx, dz) = Direction8Helper.ToOffset(dir);
            bool isDiagonal = Direction8Helper.IsDiagonal(dir);

            GameObject prefab = isDiagonal ? wallDiagPrefab : wallPrefab;
            if (prefab == null) return;

            // Position: center of wall edge
            float wallX = wx + (dx > 0 ? cellSize / 2 : (dx < 0 ? -cellSize / 2 : 0));
            float wallZ = wz + (dz > 0 ? cellSize / 2 : (dz < 0 ? -cellSize / 2 : 0));

            // Rotation
            Quaternion rotation = GetWallRotation(dir, isDiagonal);

            GameObject wall = Instantiate(prefab, new Vector3(wallX, wallHeight / 2, wallZ), rotation);
            wall.name = $"Wall_{dir}_{x}_{z}";
            wall.transform.SetParent(parent);

            // Apply material
            var renderer = wall.GetComponent<MeshRenderer>();
            if (renderer != null && wallMaterial != null)
                renderer.sharedMaterial = wallMaterial;
        }

        /// <summary>
        /// Get wall rotation for direction.
        /// </summary>
        private Quaternion GetWallRotation(Direction8 dir, bool isDiagonal)
        {
            if (isDiagonal)
            {
                float angle = dir switch
                {
                    Direction8.NE => 45f,
                    Direction8.NW => -45f,
                    Direction8.SE => -45f,
                    Direction8.SW => 45f,
                    _ => 0f
                };
                return Quaternion.Euler(0f, angle, 0f);
            }
            else
            {
                float angle = dir switch
                {
                    Direction8.N => 0f,
                    Direction8.E => 90f,
                    Direction8.S => 180f,
                    Direction8.W => 270f,
                    _ => 0f
                };
                return Quaternion.Euler(0f, angle, 0f);
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Build grid bytes for ComputeGrid.
        /// </summary>
        private byte[] BuildGridBytes()
        {
            if (mazeData == null) return null;

            int total = 2 + mazeData.Width * mazeData.Height;
            byte[] data = new byte[total];
            data[0] = (byte)mazeData.Width;
            data[1] = (byte)mazeData.Height;

            int i = 2;
            for (int z = 0; z < mazeData.Height; z++)
            {
                for (int x = 0; x < mazeData.Width; x++)
                {
                    // Convert CellFlags8 to byte (lower 8 bits = walls)
                    var cell = mazeData.GetCell(x, z);
                    data[i++] = (byte)((ushort)cell & 0xFF);
                }
            }

            return data;
        }

        #endregion
    }
}
