// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8 (no BOM) | Line Endings: Unix LF
// Cell-Based Maze Generation System - Phase 5: Agreement System

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core.Maze
{
    /// <summary>
    /// AgreementPlacer - Places agreements (chests, enemies, traps) in maze.
    /// 
    /// Responsibilities:
    /// - Place chests on decoy dead-ends
    /// - Place enemies in dangerous rooms
    /// - Place traps in corridors
    /// - Mark safe zones (no enemies)
    /// - Balance difficulty based on level
    /// 
    /// Plug-in-Out Compliant:
    /// - Uses MazeCell (passed data structure)
    /// - No component creation
    /// - All values from parameters
    /// </summary>
    public class AgreementPlacer
    {
        // Grid Reference
        private MazeCell[,] _grid;
        private int _width;
        private int _height;
        
        // Configuration
        private int _level;
        private float _chestDensity;
        private float _enemyDensity;
        private float _trapDensity;
        
        // Statistics
        private int _chestsPlaced;
        private int _enemiesPlaced;
        private int _trapsPlaced;
        
        /// <summary>
        /// Initialize agreement placer.
        /// </summary>
        public void Initialize(MazeCell[,] grid, int width, int height, int level)
        {
            _grid = grid;
            _width = width;
            _height = height;
            _level = level;
            
            // Scale densities with level
            _chestDensity = Mathf.Lerp(0.03f, 0.08f, level / 39.0f);
            _enemyDensity = Mathf.Lerp(0.02f, 0.06f, level / 39.0f);
            _trapDensity = Mathf.Lerp(0.01f, 0.04f, level / 39.0f);
            
            _chestsPlaced = 0;
            _enemiesPlaced = 0;
            _trapsPlaced = 0;
        }
        
        /// <summary>
        /// Place all agreements in maze.
        /// </summary>
        public void PlaceAllAgreements(List<DecoyPath> decoys, List<Room> rooms)
        {
            Debug.Log("[AgreementPlacer] Placing agreements...");
            
            // Place chests on decoy dead-ends
            PlaceChestsOnDecoys(decoys);
            
            // Place enemies in rooms and corridors
            PlaceEnemies(rooms);
            
            // Place traps in corridors
            PlaceTraps();
            
            // Mark safe zones
            MarkSafeZones(rooms);
            
            Debug.Log($"[AgreementPlacer] Placed {_chestsPlaced} chests, {_enemiesPlaced} enemies, {_trapsPlaced} traps");
        }
        
        /// <summary>
        /// Place chests on decoy dead-ends.
        /// </summary>
        private void PlaceChestsOnDecoys(List<DecoyPath> decoys)
        {
            foreach (var decoy in decoys)
            {
                if (decoy.cells.Count == 0) continue;
                
                // 50% chance for chest at dead-end
                if (decoy.reward == CellAgreement.ChestLocation || UnityEngine.Random.value < 0.5f)
                {
                    var endCell = decoy.cells[decoy.cells.Count - 1];
                    PlaceChest(endCell);
                }
            }
        }
        
        /// <summary>
        /// Place chest at position.
        /// </summary>
        private void PlaceChest(Vector2Int pos)
        {
            if (!IsValidCell(pos)) return;
            
            var cell = _grid[pos.x, pos.y];
            cell.cellType = CellType.Treasure;
            cell.agreement = CellAgreement.ChestLocation;
            _grid[pos.x, pos.y] = cell;
            
            _chestsPlaced++;
        }
        
        /// <summary>
        /// Place enemies in rooms and corridors.
        /// </summary>
        private void PlaceEnemies(List<Room> rooms)
        {
            // Place enemies in rooms
            foreach (var room in rooms)
            {
                if (room.type == RoomType.Safe) continue; // No enemies in safe rooms
                
                var cells = room.GetRoomCells();
                int enemyCount = Mathf.RoundToInt(cells.Length * _enemyDensity);
                enemyCount = Mathf.Clamp(enemyCount, 1, 3);
                
                for (int i = 0; i < enemyCount; i++)
                {
                    var randomCell = cells[Random.Range(0, cells.Length)];
                    if (IsValidCell(randomCell))
                    {
                        PlaceEnemy(randomCell);
                    }
                }
            }
            
            // Place enemies in dead-end corridors
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var cell = _grid[x, y];
                    if (cell.isDeadEnd && UnityEngine.Random.value < _enemyDensity * 2f)
                    {
                        PlaceEnemy(new Vector2Int(x, y));
                    }
                }
            }
        }
        
        /// <summary>
        /// Place enemy at position.
        /// </summary>
        private void PlaceEnemy(Vector2Int pos)
        {
            if (!IsValidCell(pos)) return;
            
            var cell = _grid[pos.x, pos.y];
            if (cell.cellType == CellType.Treasure || cell.cellType == CellType.Safe) return;
            
            cell.cellType = CellType.EnemyGuard;
            cell.agreement = CellAgreement.EnemyLocation;
            _grid[pos.x, pos.y] = cell;
            
            _enemiesPlaced++;
        }
        
        /// <summary>
        /// Place traps in corridors.
        /// </summary>
        private void PlaceTraps()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var cell = _grid[x, y];
                    
                    // Place trap in corridor cells
                    if (cell.agreement == CellAgreement.Corridor && 
                        !cell.isOnPrimaryPath &&
                        UnityEngine.Random.value < _trapDensity)
                    {
                        PlaceTrap(new Vector2Int(x, y));
                    }
                }
            }
        }
        
        /// <summary>
        /// Place trap at position.
        /// </summary>
        private void PlaceTrap(Vector2Int pos)
        {
            if (!IsValidCell(pos)) return;
            
            var cell = _grid[pos.x, pos.y];
            if (cell.cellType != CellType.Empty) return;
            
            cell.cellType = CellType.Trap;
            cell.agreement = CellAgreement.TrapLocation;
            _grid[pos.x, pos.y] = cell;
            
            _trapsPlaced++;
        }
        
        /// <summary>
        /// Mark safe zones (no enemies).
        /// </summary>
        private void MarkSafeZones(List<Room> rooms)
        {
            // Mark spawn room as safe
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    var pos = new Vector2Int(x + 1, y + 1);
                    if (IsValidCell(pos))
                    {
                        var cell = _grid[pos.x, pos.y];
                        cell.cellType = CellType.Safe;
                        _grid[pos.x, pos.y] = cell;
                    }
                }
            }
            
            // Mark safe rooms
            foreach (var room in rooms)
            {
                if (room.type == RoomType.Safe)
                {
                    var cells = room.GetRoomCells();
                    foreach (var cellPos in cells)
                    {
                        if (IsValidCell(cellPos))
                        {
                            var cell = _grid[cellPos.x, cellPos.y];
                            cell.cellType = CellType.Safe;
                            _grid[cellPos.x, cellPos.y] = cell;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Check if cell position is valid.
        /// </summary>
        private bool IsValidCell(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height;
        }
        
        /// <summary>
        /// Get placement statistics.
        /// </summary>
        public string GetStatistics()
        {
            return $"AgreementPlacer: {_chestsPlaced} chests, {_enemiesPlaced} enemies, {_trapsPlaced} traps";
        }
    }
}
