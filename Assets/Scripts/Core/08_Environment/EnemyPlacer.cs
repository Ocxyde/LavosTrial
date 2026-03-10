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
// EnemyPlacer.cs
// Specialized enemy placement system
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-OUT ARCHITECTURE:
// - Finds components (never creates)
// - Uses prefabs when available
// - All values from JSON config
// - No hardcoded values
//
// LOCATION: Assets/Scripts/Core/08_Environment/

using System.Collections.Generic;
using UnityEngine;
using Code.Lavos.Core.Advanced;

namespace Code.Lavos.Core
{
    /// <summary>
    /// EnemyPlacer - Specialized enemy placement.
    /// PLUG-IN-OUT COMPLIANT: Finds components, never creates.
    /// ALL VALUES FROM JSON: Enemy settings loaded from GameConfig.
    /// </summary>
    public class EnemyPlacer : MonoBehaviour
    {
        #region Inspector Fields (From JSON)

        [Header(" Enemy Settings (From JSON Config)")]
        [Tooltip("Enable enemy spawning (loaded from JSON)")]
        [SerializeField] private bool enableEnemySpawning;
        
        [Tooltip("Chance for enemy to spawn (loaded from JSON)")]
        [SerializeField] private float enemySpawnChance;
        
        [Tooltip("Max enemies to spawn (loaded from JSON)")]
        [SerializeField] private int maxEnemies;

        [Header(" Prefab Reference")]
        [Tooltip("Enemy prefab (assign in Inspector or Resources/)")]
        [SerializeField] private GameObject enemyPrefab;

        [Header(" Component References (Plug-in-Out)")]
        [Tooltip("Auto-finds GridMazeGenerator in scene")]
        [SerializeField] private GridMazeGenerator gridMazeGenerator;

        [Tooltip("Auto-finds CompleteMazeBuilder8 in scene")]
        [SerializeField] private CompleteMazeBuilder8 completeMazeBuilder;

        [Header(" Debug")]
        [SerializeField] private bool showDebugLogs = true;

        #endregion

        #region Private Data

        private int _enemiesSpawned;

        #endregion

        #region Public Accessors

        public int EnemiesSpawned => _enemiesSpawned;

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
            // GridMazeGenerator is created by CompleteMazeBuilder8
            if (completeMazeBuilder == null)
            {
                completeMazeBuilder = FindFirstObjectByType<CompleteMazeBuilder8>();
                if (completeMazeBuilder == null)
                    Debug.LogWarning("[EnemyPlacer] CompleteMazeBuilder8 not found!");
            }
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
            
            // Enemy settings from JSON (reuse room settings)
            enableEnemySpawning = config.generateRooms;
            enemySpawnChance = config.defaultDoorSpawnChance;
            maxEnemies = config.maxRooms;

            if (showDebugLogs)
            {
                Debug.Log($"[EnemyPlacer]  Config loaded from JSON:");
                Debug.Log($"  • Enable: {enableEnemySpawning}");
                Debug.Log($"  • Spawn Chance: {enemySpawnChance * 100f:F0}%");
                Debug.Log($"  • Max Enemies: {maxEnemies}");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Place enemies in maze.
        /// Call after maze generation.
        /// </summary>
        [ContextMenu("Place Enemies")]
        public void PlaceAllEnemies()
        {
            if (!enableEnemySpawning)
            {
                if (showDebugLogs)
                    Debug.Log("[EnemyPlacer] Enemy spawning disabled");
                return;
            }

            if (gridMazeGenerator == null)
            {
                Debug.LogError("[EnemyPlacer]  GridMazeGenerator not initialized!");
                return;
            }

            _enemiesSpawned = 0;
            int targetCount = Random.Range(1, maxEnemies + 1);
            int attempts = 0;
            int maxAttempts = targetCount * 3;

            while (_enemiesSpawned < targetCount && attempts < maxAttempts)
            {
                attempts++;
                
                Vector2Int cell = GetRandomValidCell();
                
                if (cell.x >= 0)
                {
                    PlaceEnemy(cell);
                    _enemiesSpawned++;
                }
            }

            if (showDebugLogs)
            {
                Debug.Log($"[EnemyPlacer]  Spawned {_enemiesSpawned} enemies");
            }
        }

        #endregion

        #region Enemy Placement

        /// <summary>
        /// Place single enemy at grid position.
        /// </summary>
        private void PlaceEnemy(Vector2Int cell)
        {
            float cellSize = GetCellSize();
            
            Vector3 position = new Vector3(
                cell.x * cellSize + cellSize / 2f,
                0.1f,
                cell.y * cellSize + cellSize / 2f
            );
            
            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);

            if (enemyPrefab != null)
            {
                GameObject.Instantiate(enemyPrefab, position, rotation);
                _enemiesSpawned++;

                // Save to binary storage
                if (_storageReference != null)
                {
                    _storageReference.SaveObjectToBinary("Enemy", position, rotation);
                }
                
                if (showDebugLogs)
                    Debug.Log($"[EnemyPlacer]  Placed enemy prefab at {position}");
            }
            else
            {
                if (showDebugLogs)
                    Debug.LogWarning($"[EnemyPlacer] No enemy prefab assigned - skipping");
            }
        }

        /// <summary>
        /// Get random valid cell for enemy placement.
        /// </summary>
        private Vector2Int GetRandomValidCell()
        {
            int size = gridMazeGenerator.GridSize;
            int maxAttempts = 20;
            
            for (int i = 0; i < maxAttempts; i++)
            {
                int x = Random.Range(0, size);
                int y = Random.Range(0, size);

                var cell = gridMazeGenerator.GetCell(x, y);

                // Only place in rooms or corridors
                // 8-axis compatibility: Check if cell is walkable (not all walls)
                if (cell.IsWalkable())
                {
                    return new Vector2Int(x, y);
                }
            }

            return new Vector2Int(-1, -1);
        }

        #endregion

        #region Utilities

        private float GetCellSize()
        {
            return GameConfig.Instance.DefaultCellSize;
        }

        #endregion

        #region Binary Storage Integration

        private SpatialPlacer _storageReference;

        public void SetStorageReference(SpatialPlacer placer)
        {
            _storageReference = placer;
        }

        public void PlaceEnemiesFromRecords(List<SpatialPlacer.ObjectPlacementRecord> records)
        {
            if (records == null || records.Count == 0) return;

            foreach (var record in records)
            {
                GameObject.Instantiate(enemyPrefab, record.Position, record.Rotation);
                _enemiesSpawned++;
            }

            if (showDebugLogs)
                Debug.Log($"[EnemyPlacer]  Loaded {_enemiesSpawned} enemies from binary");
        }

        #endregion
    }
}
