// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8 (no BOM) | Line Endings: Unix LF
// Cell-Based Maze Generation System - Phase 6: Wall & Door Integration

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core.Maze
{
    /// <summary>
    /// CellToWallConverter - Converts cell-based maze to wall/door prefabs.
    /// 
    /// Responsibilities:
    /// - Read MazeCell[,] grid (from CellBasedMazeGenerator)
    /// - Spawn wall prefabs at cell edges
    /// - Spawn door prefabs in doorways
    /// - Integrate with existing WallWithDoorHandler
    /// - Maintain Plug-in-Out compliance
    /// 
    /// Plug-in-Out Compliant:
    /// - Uses passed prefabs (never creates)
    /// - Uses passed transforms (never creates)
    /// - All values from parameters
    /// </summary>
    public class CellToWallConverter
    {
        // Grid Reference
        private MazeCell[,] _grid;
        private int _width;
        private int _height;
        
        // Prefabs
        private GameObject _wallPrefab;
        private GameObject _doorPrefab;
        private GameObject _lockedDoorPrefab;
        private GameObject _secretDoorPrefab;
        private GameObject _exitDoorPrefab;
        private Material _wallMaterial;
        
        // Configuration
        private float _cellSize;
        private float _wallHeight;
        private float _wallThickness;
        private bool _wallPivotIsAtMeshCenter;
        
        // Transforms
        private Transform _wallsRoot;
        private Transform _doorsRoot;
        
        // Statistics
        private int _wallsSpawned;
        private int _doorsSpawned;
        
        /// <summary>
        /// Initialize converter.
        /// </summary>
        public void Initialize(
            MazeCell[,] grid, int width, int height,
            GameObject wallPrefab, GameObject doorPrefab,
            GameObject lockedDoorPrefab, GameObject secretDoorPrefab,
            GameObject exitDoorPrefab, Material wallMaterial,
            Transform wallsRoot, Transform doorsRoot,
            float cellSize, float wallHeight, float wallThickness,
            bool wallPivotIsAtMeshCenter)
        {
            _grid = grid;
            _width = width;
            _height = height;
            
            _wallPrefab = wallPrefab;
            _doorPrefab = doorPrefab;
            _lockedDoorPrefab = lockedDoorPrefab;
            _secretDoorPrefab = secretDoorPrefab;
            _exitDoorPrefab = exitDoorPrefab;
            _wallMaterial = wallMaterial;
            
            _wallsRoot = wallsRoot;
            _doorsRoot = doorsRoot;
            
            _cellSize = cellSize;
            _wallHeight = wallHeight;
            _wallThickness = wallThickness;
            _wallPivotIsAtMeshCenter = wallPivotIsAtMeshCenter;
            
            _wallsSpawned = 0;
            _doorsSpawned = 0;
        }
        
        /// <summary>
        /// Convert all cells to walls and doors.
        /// </summary>
        public void ConvertAll()
        {
            Debug.Log("[CellToWallConverter] Converting cells to walls/doors...");
            
            // Spawn walls at cell edges
            SpawnAllWalls();
            
            // Spawn doors in doorways
            SpawnAllDoors();
            
            Debug.Log($"[CellToWallConverter] Spawned {_wallsSpawned} walls, {_doorsSpawned} doors");
        }
        
        /// <summary>
        /// Spawn all walls at cell edges.
        /// </summary>
        private void SpawnAllWalls()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var cell = _grid[x, y];
                    
                    // Spawn wall on each edge that has a wall
                    if (cell.hasNorthWall)
                        SpawnWallEdge(x, y, Direction8.N);
                    
                    if (cell.hasSouthWall)
                        SpawnWallEdge(x, y, Direction8.S);
                    
                    if (cell.hasEastWall)
                        SpawnWallEdge(x, y, Direction8.E);
                    
                    if (cell.hasWestWall)
                        SpawnWallEdge(x, y, Direction8.W);
                }
            }
        }
        
        /// <summary>
        /// Spawn wall at cell edge.
        /// </summary>
        private void SpawnWallEdge(int x, int y, Direction8 direction)
        {
            // Calculate wall position
            Vector3 position = GetWallPosition(x, y, direction);
            Quaternion rotation = GetWallRotation(direction);
            
            // Spawn wall prefab
            GameObject wall = Object.Instantiate(_wallPrefab, position, rotation, _wallsRoot);
            wall.name = $"Wall_{x}_{y}_{direction}";
            
            // Apply material
            var renderer = wall.GetComponent<Renderer>();
            if (renderer != null && _wallMaterial != null)
            {
                renderer.sharedMaterial = _wallMaterial;
            }
            
            _wallsSpawned++;
        }
        
        /// <summary>
        /// Spawn all doors in doorways.
        /// </summary>
        private void SpawnAllDoors()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var cell = _grid[x, y];
                    
                    // Check if cell is a doorway
                    if (cell.agreement == CellAgreement.Doorway)
                    {
                        // Determine door type from context
                        MazeDoorType doorType = DetermineDoorType(x, y);
                        SpawnDoor(x, y, doorType);
                    }
                }
            }
        }
        
        /// <summary>
        /// Determine door type from cell context.
        /// </summary>
        private MazeDoorType DetermineDoorType(int x, int y)
        {
            var cell = _grid[x, y];
            if (cell.isOnPrimaryPath)
            {
                if (Mathf.Abs(x - (_width - 2)) <= 1 && Mathf.Abs(y - (_height - 2)) <= 1)
                {
                    return MazeDoorType.Exit;
                }
            }
            
            return MazeDoorType.Normal;
        }
        
        /// <summary>
        /// Spawn door at position.
        /// </summary>
        private void SpawnDoor(int x, int y, MazeDoorType type)
        {
            GameObject doorPrefab = GetDoorPrefab(type);
            if (doorPrefab == null)
            {
                Debug.LogWarning($"[CellToWallConverter] No prefab for door type {type}");
                return;
            }
            
            Vector3 position = GetDoorPosition(x, y);
            Quaternion rotation = GetDoorRotation(x, y);
            
            GameObject door = Object.Instantiate(doorPrefab, position, rotation, _doorsRoot);
            door.name = $"Door_{type}_{x}_{y}";
            
            _doorsSpawned++;
        }
        
        /// <summary>
        /// Get door prefab for type.
        /// </summary>
        private GameObject GetDoorPrefab(MazeDoorType type)
        {
            return type switch
            {
                MazeDoorType.Locked => _lockedDoorPrefab ?? _doorPrefab,
                MazeDoorType.Secret => _secretDoorPrefab ?? _doorPrefab,
                MazeDoorType.Exit => _exitDoorPrefab ?? _doorPrefab,
                _ => _doorPrefab,
            };
        }
        
        /// <summary>
        /// Get wall position for cell edge.
        /// </summary>
        private Vector3 GetWallPosition(int x, int y, Direction8 direction)
        {
            float posX = x * _cellSize + _cellSize / 2f;
            float posY = _wallPivotIsAtMeshCenter ? _wallHeight / 2f : 0f;
            float posZ = y * _cellSize + _cellSize / 2f;
            
            switch (direction)
            {
                case Direction8.N: posZ += _cellSize / 2f; break;
                case Direction8.S: posZ -= _cellSize / 2f; break;
                case Direction8.E: posX += _cellSize / 2f; break;
                case Direction8.W: posX -= _cellSize / 2f; break;
            }
            
            return new Vector3(posX, posY, posZ);
        }
        
        /// <summary>
        /// Get wall rotation for direction.
        /// </summary>
        private Quaternion GetWallRotation(Direction8 direction)
        {
            switch (direction)
            {
                case Direction8.N:
                case Direction8.S:
                    return Quaternion.Euler(0f, 90f, 0f);
                case Direction8.E:
                case Direction8.W:
                    return Quaternion.Euler(0f, 0f, 0f);
                default:
                    return Quaternion.identity;
            }
        }
        
        /// <summary>
        /// Get door position for cell.
        /// </summary>
        private Vector3 GetDoorPosition(int x, int y)
        {
            float posX = x * _cellSize + _cellSize / 2f;
            float posY = _wallHeight / 2f;
            float posZ = y * _cellSize + _cellSize / 2f;
            
            return new Vector3(posX, posY, posZ);
        }
        
        /// <summary>
        /// Get door rotation based on adjacent walls.
        /// </summary>
        private Quaternion GetDoorRotation(int x, int y)
        {
            bool hasNorthWall = y + 1 < _height && _grid[x, y + 1].hasSouthWall;
            bool hasSouthWall = y - 1 >= 0 && _grid[x, y - 1].hasNorthWall;
            
            if (hasNorthWall || hasSouthWall)
            {
                return Quaternion.Euler(0f, 0f, 0f);
            }
            else
            {
                return Quaternion.Euler(0f, 90f, 0f);
            }
        }
        
        /// <summary>
        /// Get conversion statistics.
        /// </summary>
        public string GetStatistics()
        {
            return $"CellToWallConverter: {_wallsSpawned} walls, {_doorsSpawned} doors";
        }
    }
}
