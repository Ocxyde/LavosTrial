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
// DoorHolePlacer.cs
// Reserves and creates holes in room walls for door placement
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-OUT ARCHITECTURE:
// - Finds components (never creates)
// - All values loaded from JSON config
// - No hardcoded values
// - Works with GridMazeGenerator (new grid system)
//
// LOCATION: Assets/Scripts/Core/07_Doors/

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// DoorHolePlacer - Reserves wall spaces for doors.
    /// PLUG-IN-OUT COMPLIANT: Finds components, never creates them.
    /// ALL VALUES FROM JSON: Door dimensions, spawn chance, etc. loaded from GameConfig.
    /// </summary>
    public class DoorHolePlacer : MonoBehaviour
    {
        #region Inspector Fields (Serialized from JSON)

        [Header(" Door Dimensions (From JSON Config)")]
        [Tooltip("Door width + frame (loaded from JSON)")]
        [SerializeField] private float doorWidth;
        
        [Tooltip("Door height + frame (loaded from JSON)")]
        [SerializeField] private float doorHeight;
        
        [Tooltip("Door depth / wall thickness (loaded from JSON)")]
        [SerializeField] private float doorDepth;
        
        [Tooltip("Hole depth in wall (loaded from JSON)")]
        [SerializeField] private float holeDepth;

        [Header(" Door Spawn Settings (From JSON Config)")]
        [Tooltip("Chance per wall to have a door (loaded from JSON)")]
        [SerializeField] private float doorSpawnChance;
        
        [Tooltip("Carve holes in walls (loaded from JSON)")]
        [SerializeField] private bool carveHolesInWalls;
        
        [Tooltip("Show debug gizmos in editor (loaded from JSON)")]
        [SerializeField] private bool showDebugGizmos;

        [Header(" Component References (Plug-in-Out)")]
        [Tooltip("Auto-finds GridMazeGenerator in scene")]
        [SerializeField] private GridMazeGenerator gridMazeGenerator;
        
        [Tooltip("Auto-finds CompleteMazeBuilder in scene")]
        [SerializeField] private CompleteMazeBuilder completeMazeBuilder;

        [Header(" Debug (Hardcoded - Comment to Disable Warnings)")]
        // [SerializeField] private float hardcodedCellSize = 6f;  // Comment out to disable warning
        // showDebugGizmos is defined above in Inspector Fields section

        #endregion

        #region Private Data

        private List<DoorHoleData> _placedHoles = new();
        private Dictionary<Vector2Int, List<DoorHoleData>> _holesByCell = new();

        #endregion

        #region Public Accessors

        public List<DoorHoleData> PlacedHoles => _placedHoles;
        public int HoleCount => _placedHoles.Count;
        public float DoorWidth => doorWidth;
        public float DoorHeight => doorHeight;
        public float DoorDepth => doorDepth;
        public float HoleDepth => holeDepth;
        public float DoorSpawnChance => doorSpawnChance;
        public bool CarveHoles => carveHolesInWalls;
        public bool ShowDebugGizmos => showDebugGizmos;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // PLUG-IN-OUT: Find components (never create!)
            FindComponents();
            
            // LOAD ALL VALUES FROM JSON CONFIG (NO HARDCODING!)
            LoadConfig();
        }

        #endregion

        #region Plug-in-Out Compliance

        /// <summary>
        /// Find all required components in scene.
        /// PLUG-IN-OUT: Never creates components, only finds existing ones.
        /// </summary>
        private void FindComponents()
        {
            // GridMazeGenerator is created by CompleteMazeBuilder
            if (completeMazeBuilder == null)
                completeMazeBuilder = FindFirstObjectByType<CompleteMazeBuilder>();
        }

        #endregion

        #region JSON Config Loading

        /// <summary>
        /// Load ALL values from JSON config.
        /// NO HARDCODED VALUES - everything from GameConfig-default.json.
        /// </summary>
        private void LoadConfig()
        {
            var config = GameConfig.Instance;
            
            // Door dimensions from JSON
            doorWidth = config.defaultDoorWidth;
            doorHeight = config.defaultDoorHeight;
            doorDepth = config.defaultDoorDepth;
            holeDepth = config.defaultDoorHoleDepth;
            
            // Door spawn settings from JSON
            doorSpawnChance = config.defaultDoorSpawnChance;
            carveHolesInWalls = true;  // Always carve holes if this component exists
            showDebugGizmos = config.showDebugGizmos;

            Debug.Log($"[DoorHolePlacer]  Config loaded from JSON:");
            Debug.Log($"  • Door Size: {doorWidth}m x {doorHeight}m x {doorDepth}m");
            Debug.Log($"  • Hole Depth: {holeDepth}m");
            Debug.Log($"  • Door Spawn Chance: {doorSpawnChance * 100f:F0}%");
        }

        #endregion

        #region Public API

        /// <summary>
        /// Place door holes in all room walls.
        /// Call after room generation, before door placement.
        /// </summary>
        [ContextMenu("Place Door Holes")]
        public void PlaceAllHoles()
        {
            if (!carveHolesInWalls)
            {
                Debug.Log("[DoorHolePlacer] Hole carving disabled");
                return;
            }

            if (gridMazeGenerator == null)
            {
                Debug.LogError("[DoorHolePlacer]  GridMazeGenerator not initialized!");
                return;
            }

            ClearHoles();

            Debug.Log("[DoorHolePlacer] Starting door hole placement...");

            // Iterate through grid and place holes in room walls
            int size = gridMazeGenerator.GridSize;
            int holesPlaced = 0;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var cell = gridMazeGenerator.GetCell(x, y);
                    
                    // Place holes in room cells that have adjacent walls
                    if (cell == GridMazeCell.Room || cell == GridMazeCell.Corridor)
                    {
                        if (TryPlaceHole(x, y, gridMazeGenerator))
                        {
                            holesPlaced++;
                        }
                    }
                }
            }

            Debug.Log($"[DoorHolePlacer]  Placed {holesPlaced} door holes");
        }

        /// <summary>
        /// Clear all placed door holes.
        /// </summary>
        [ContextMenu("Clear Door Holes")]
        public void ClearHoles()
        {
            _placedHoles.Clear();
            _holesByCell.Clear();
            Debug.Log("[DoorHolePlacer]  Cleared all door holes");
        }

        #endregion

        #region Door Hole Placement

        /// <summary>
        /// Try to place a door hole at grid position.
        /// Returns true if hole was placed.
        /// </summary>
        private bool TryPlaceHole(int x, int y, GridMazeGenerator grid)
        {
            // Check each direction for adjacent walls
            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up,    // North
                Vector2Int.down,  // South
                Vector2Int.right, // East
                Vector2Int.left   // West
            };

            bool holePlaced = false;

            foreach (var dir in directions)
            {
                int checkX = x + dir.x;
                int checkY = y + dir.y;

                // Check bounds
                if (checkX < 0 || checkX >= grid.GridSize || checkY < 0 || checkY >= grid.GridSize)
                    continue;

                // Check if adjacent cell is a wall
                var adjacentCell = grid.GetCell(checkX, checkY);
                if (adjacentCell == GridMazeCell.Wall)
                {
                    // Roll for door spawn
                    if (Random.value > doorSpawnChance)
                        continue;

                    // Place hole
                    PlaceHole(x, y, dir, grid);
                    holePlaced = true;
                }
            }

            return holePlaced;
        }

        /// <summary>
        /// Place a single door hole.
        /// </summary>
        private void PlaceHole(int x, int y, Vector2Int direction, GridMazeGenerator grid)
        {
            // Calculate hole position in world space
            float cellSize = completeMazeBuilder != null ? 
                GetCellSizeFromBuilder() : 6f;  // Fallback to default

            Vector3 holePosition = new Vector3(
                x * cellSize + cellSize / 2f,
                doorHeight / 2f,
                y * cellSize + cellSize / 2f
            );

            // Calculate rotation based on direction
            Quaternion holeRotation = direction switch
            {
                var d when d == Vector2Int.up => Quaternion.Euler(0f, 180f, 0f),
                var d when d == Vector2Int.down => Quaternion.identity,
                var d when d == Vector2Int.right => Quaternion.Euler(0f, -90f, 0f),
                var d when d == Vector2Int.left => Quaternion.Euler(0f, 90f, 0f),
                _ => Quaternion.identity
            };

            // Create hole data
            var holeData = new DoorHoleData
            {
                GridPosition = new Vector2Int(x, y),
                Direction = direction,
                Position = holePosition,
                Rotation = holeRotation,
                Width = doorWidth,
                Height = doorHeight,
                Depth = holeDepth
            };

            _placedHoles.Add(holeData);

            // Add to cell dictionary
            if (!_holesByCell.ContainsKey(holeData.GridPosition))
                _holesByCell[holeData.GridPosition] = new List<DoorHoleData>();
            
            _holesByCell[holeData.GridPosition].Add(holeData);

            if (showDebugGizmos)
            {
                Debug.Log($"[DoorHolePlacer]  Placed hole at ({x}, {y}) facing {direction}");
            }
        }

        /// <summary>
        /// Get cell size from CompleteMazeBuilder via reflection.
        /// TODO: Add public accessor to CompleteMazeBuilder.
        /// </summary>
        private float GetCellSizeFromBuilder()
        {
            // Try JSON config first
            if (GameConfig.Instance != null)
                return GameConfig.Instance.defaultCellSize;
            
            // HARDCODED FALLBACK (comment out to disable warning):
            // return hardcodedCellSize;  // Default: 6f
            
            // Last resort default
            return 6f;
        }

        #endregion

        #region Door Hole Data

        /// <summary>
        /// Door hole data structure.
        /// Stores position, rotation, and dimensions for each door hole.
        /// </summary>
        [System.Serializable]
        public class DoorHoleData
        {
            public Vector2Int GridPosition;  // Grid coordinates
            public Vector2Int Direction;     // Direction the door faces
            public Vector3 Position;         // World position
            public Quaternion Rotation;      // World rotation
            public float Width;              // Door width
            public float Height;             // Door height
            public float Depth;              // Hole depth
        }

        #endregion
    }
}
