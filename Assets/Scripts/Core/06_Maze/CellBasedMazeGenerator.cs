// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8 (no BOM) | Line Endings: Unix LF
// Cell-Based Maze Generation System - Phase 2: Primary Path System

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core.Maze
{
    /// <summary>
    /// CellBasedMazeGenerator - Generates mazes using cell-based approach.
    /// 
    /// Core Principles:
    /// - Longest snake path is SACRED (never broken)
    /// - Walls at cell EDGES (not inside cells)
    /// - Decoy paths for misdirection (L-shape corridors)
    /// - Rooms with 2 doors (entry + exit, both walkable)
    /// - Difficulty scales with level (more decoys = harder)
    /// 
    /// Plug-in-Out Compliant:
    /// - Uses PathFinder (passed in, never created)
    /// - Uses DifficultyCurve (data structure)
    /// - No hardcoded values (all from parameters/config)
    /// </summary>
    public class CellBasedMazeGenerator
    {
        // Maze Data
        private MazeCell[,] _grid;
        private int _width;
        private int _height;
        
        // Path Tracking
        private List<Vector2Int> _primaryPath;
        private List<DecoyPath> _decoyPaths;
        private DecoySystem _decoySystem;
        
        // Difficulty
        private DifficultyCurve _difficulty;
        private int _level;
        
        // Spawn/Exit
        private Vector2Int _spawn;
        private Vector2Int _exit;
        
        // Properties
        public MazeCell[,] Grid => _grid;
        public int Width => _width;
        public int Height => _height;
        public List<Vector2Int> PrimaryPath => _primaryPath;
        public DifficultyCurve Difficulty => _difficulty;
        public Vector2Int Spawn => _spawn;
        public Vector2Int Exit => _exit;
        
        /// <summary>
        /// Generate complete cell-based maze with verification.
        /// </summary>
        public MazeCell[,] Generate(int width, int height, int level, int seed, bool verify = true)
        {
            // Load config (Plug-in-Out: find, never create)
            var config = CellBasedMazeConfig.Load();
            
            // Initialize
            _width = width > 0 ? width : config.defaultWidth;
            _height = height > 0 ? height : config.defaultHeight;
            _level = level;
            _primaryPath = new List<Vector2Int>();
            _decoyPaths = new List<DecoyPath>();
            
            // Set random seed
            UnityEngine.Random.InitState(seed);
            
            // Compute difficulty curve
            _difficulty = DifficultyCurve.FromLevel(level, Mathf.Max(width, height));
            _difficulty.Log();
            
            // Step 1: Initialize all cells as walls
            InitializeAllWalls();
            
            // Step 2: Set spawn and exit positions
            SetSpawnAndExit();
            
            // Step 3: Compute longest snake path (SACRED)
            ComputeLongestSnakePath();
            
            // Step 4: Mark primary path cells
            MarkPrimaryPathCells();
            
            // Step 5: Create decoy paths (L-shape misdirections)
            CreateDecoyPaths();
            
            // Step 6: Place agreements (rooms, chests, dead-ends)
            PlaceAgreements();
            
            // Step 7: Verify primary path integrity
            VerifyPrimaryPath();
            
            // Step 8: Full maze verification
            if (verify)
            {
                VerifyMaze();
            }
            
            // Step 9: Log completion
            LogGenerationSummary();
            
            return _grid;
        }
        
        // Step 1: Initialize All Walls
        private void InitializeAllWalls()
        {
            _grid = new MazeCell[_width, _height];
            
            for (int x = 0; x < _width; x++)
            {
                for (int z = 0; z < _height; z++)
                {
                    _grid[x, z] = MazeCell.CreateWall();
                }
            }
            
            Debug.Log($"[CellBasedMazeGenerator] Initialized {_width}x{_height} grid (all walls)");
        }
        
        // Step 2: Set Spawn and Exit Positions
        private void SetSpawnAndExit()
        {
            _spawn = new Vector2Int(1, 1);
            _exit = new Vector2Int(_width - 2, _height - 2);
            
            _grid[_spawn.x, _spawn.y] = MazeCell.CreateEmpty();
            _grid[_exit.x, _exit.y] = MazeCell.CreateEmpty();
            
            Debug.Log($"[CellBasedMazeGenerator] Spawn: {_spawn}, Exit: {_exit}");
        }
        
        // Step 3: Compute Longest Snake Path
        private void ComputeLongestSnakePath()
        {
            _primaryPath.Clear();
            var visited = new bool[_width, _height];
            
            var path = new List<Vector2Int>();
            DFS_LongestPath(_spawn, visited, path);
            
            _primaryPath = new List<Vector2Int>(path);
            _difficulty.actualPathLength = _primaryPath.Count;
            
            Debug.Log($"[CellBasedMazeGenerator] Longest snake path: {_primaryPath.Count} cells");
        }
        
        private void DFS_LongestPath(Vector2Int current, bool[,] visited, List<Vector2Int> path)
        {
            visited[current.x, current.y] = true;
            path.Add(current);
            
            var directions = new Vector2Int[]
            {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };
            
            ShuffleArray(directions);
            
            bool foundExtension = false;
            
            foreach (var dir in directions)
            {
                var next = current + dir;
                
                if (IsValidCell(next) && !visited[next.x, next.y])
                {
                    foundExtension = true;
                    DFS_LongestPath(next, visited, path);
                }
            }
            
            if (!foundExtension && current == _exit)
            {
                return;
            }
            
            if (current != _exit && !foundExtension)
            {
                visited[current.x, current.y] = false;
                path.RemoveAt(path.Count - 1);
            }
        }
        
        // Step 4: Mark Primary Path Cells
        private void MarkPrimaryPathCells()
        {
            for (int i = 0; i < _primaryPath.Count; i++)
            {
                var cellPos = _primaryPath[i];
                var cell = _grid[cellPos.x, cellPos.y];
                cell.MarkAsPrimaryPath(i);
                _grid[cellPos.x, cellPos.y] = cell;
            }
            
            Debug.Log($"[CellBasedMazeGenerator] Marked {_primaryPath.Count} primary path cells");
        }
        
        // Step 5: Create Decoy Paths (using DecoySystem)
        private void CreateDecoyPaths()
        {
            _decoySystem = new DecoySystem();
            _decoySystem.Initialize(_grid, _width, _height, _primaryPath, 
                                    _difficulty.decoyDensity, _level);
            _decoyPaths = _decoySystem.GenerateDecoys();
        }
        
        // Step 6: Place Agreements (using AgreementPlacer)
        private void PlaceAgreements()
        {
            var placer = new AgreementPlacer();
            placer.Initialize(_grid, _width, _height, _level);
            placer.PlaceAllAgreements(_decoyPaths, new List<Room>());
        }
        
        // Step 7: Verify Primary Path Integrity
        private void VerifyPrimaryPath()
        {
            // Load config (Plug-in-Out: find, never create)
            var config = CellBasedMazeConfig.Load();
            
            bool allWalkable = true;

            foreach (var cellPos in _primaryPath)
            {
                var cell = _grid[cellPos.x, cellPos.y];
                if (!cell.IsWalkable())
                {
                    Debug.LogWarning($"[CellBasedMazeGenerator] Primary path cell blocked at {cellPos}!");
                    allWalkable = false;
                    
                    cell = MazeCell.CreateEmpty();
                    cell.MarkAsPrimaryPath(cell.pathIndex);
                    _grid[cellPos.x, cellPos.y] = cell;
                }
            }
            
            if (allWalkable)
            {
                Debug.Log("[CellBasedMazeGenerator] Primary path integrity verified");
            }
            else
            {
                Debug.LogWarning("[CellBasedMazeGenerator] Primary path had issues - fixed!");
            }
        }
        
        // Step 8: Full Maze Verification (using MazeVerifier)
        public bool VerifyMaze()
        {
            var verifier = new MazeVerifier();
            verifier.Initialize(
                _grid, _width, _height,
                _primaryPath, _decoyPaths, new List<Room>(),
                _spawn, _exit);
            
            bool isVerified = verifier.VerifyAll();
            Debug.Log($"[CellBasedMazeGenerator] {verifier.Report}");
            
            return isVerified;
        }
        
        // Step 8b: Save Maze to RAM/Storage (using existing MazeBinaryStorage8)
        public bool SaveMazeToStorage()
        {
            // Convert MazeCell[,] to MazeData8 for compatibility with existing storage
            var mazeData = ConvertToMazeData8();
            
            // Use existing MazeBinaryStorage8 (Plug-in-Out: reuse, don't duplicate)
            bool saved = MazeBinaryStorage8.Save(mazeData);
            
            if (saved)
            {
                Debug.Log($"[CellBasedMazeGenerator] Maze saved to storage: Level {_level}, Seed {_seed}");
            }
            
            return saved;
        }
        
        // Step 8c: Load Maze from RAM/Storage (using existing MazeBinaryStorage8)
        public static MazeCell[,] LoadMazeFromStorage(int level, int seed, out int width, out int height)
        {
            width = 0;
            height = 0;
            
            // Use existing MazeBinaryStorage8 (Plug-in-Out: reuse, don't duplicate)
            var mazeData = MazeBinaryStorage8.Load(level, seed);
            
            if (mazeData == null)
            {
                Debug.LogWarning($"[CellBasedMazeGenerator] No maze found in storage: Level {level}, Seed {seed}");
                return null;
            }
            
            // Convert MazeData8 to MazeCell[,]
            var grid = ConvertFromMazeData8(mazeData, out width, out height);
            
            Debug.Log($"[CellBasedMazeGenerator] Maze loaded from storage: Level {level}, Seed {seed}");
            return grid;
        }
        
        /// <summary>
        /// Convert MazeCell[,] to MazeData8 for storage compatibility.
        /// </summary>
        private MazeData8 ConvertToMazeData8()
        {
            var mazeData = new MazeData8(_width, _height, _seed, _level);
            mazeData.SetSpawn(_spawn.x, _spawn.y);
            mazeData.SetExit(_exit.x, _exit.y);
            mazeData.DifficultyFactor = _difficulty.difficulty;
            
            // Convert cell flags
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var cell = _grid[x, y];
                    CellFlags8 flags = CellFlags8.None;
                    
                    // Map edge walls to CellFlags8
                    if (cell.hasNorthWall) flags |= CellFlags8.WallN;
                    if (cell.hasSouthWall) flags |= CellFlags8.WallS;
                    if (cell.hasEastWall) flags |= CellFlags8.WallE;
                    if (cell.hasWestWall) flags |= CellFlags8.WallW;
                    
                    // Map cell types to flags
                    if (cell.isOnPrimaryPath) flags |= CellFlags8.IsRoom;
                    if (cell.isDeadEnd) flags |= CellFlags8.HasChest;
                    
                    mazeData.SetCell(x, y, flags);
                }
            }
            
            return mazeData;
        }
        
        /// <summary>
        /// Convert MazeData8 to MazeCell[,] for generation.
        /// </summary>
        private static MazeCell[,] ConvertFromMazeData8(MazeData8 mazeData, out int width, out int height)
        {
            width = mazeData.Width;
            height = mazeData.Height;
            var grid = new MazeCell[width, height];
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var flags = mazeData.GetCell(x, y);
                    var cell = new MazeCell();
                    
                    // Map CellFlags8 to edge walls
                    cell.hasNorthWall = (flags & CellFlags8.WallN) != 0;
                    cell.hasSouthWall = (flags & CellFlags8.WallS) != 0;
                    cell.hasEastWall = (flags & CellFlags8.WallE) != 0;
                    cell.hasWestWall = (flags & CellFlags8.WallW) != 0;
                    
                    // Map flags to cell properties
                    cell.isOnPrimaryPath = (flags & CellFlags8.IsRoom) != 0;
                    cell.isDeadEnd = (flags & CellFlags8.HasChest) != 0;
                    
                    grid[x, y] = cell;
                }
            }
            
            return grid;
        }
        
        // Step 9: Convert to Walls/Doors (using CellToWallConverter)
        public void SpawnWallsAndDoors(
            GameObject wallPrefab, GameObject doorPrefab,
            GameObject lockedDoorPrefab, GameObject secretDoorPrefab,
            GameObject exitDoorPrefab, Material wallMaterial,
            Transform wallsRoot, Transform doorsRoot,
            float cellSize = 0, float wallHeight = 0, float wallThickness = 0,
            bool wallPivotIsAtMeshCenter = true)
        {
            // Load config if values not provided (Plug-in-Out: find, never create)
            var config = CellBasedMazeConfig.Load();
            
            float finalCellSize = cellSize > 0 ? cellSize : config.cellSize;
            float finalWallHeight = wallHeight > 0 ? wallHeight : config.wallHeight;
            float finalWallThickness = wallThickness > 0 ? wallThickness : config.wallThickness;
            
            var converter = new CellToWallConverter();
            converter.Initialize(
                _grid, _width, _height,
                wallPrefab, doorPrefab, lockedDoorPrefab, secretDoorPrefab, exitDoorPrefab, wallMaterial,
                wallsRoot, doorsRoot,
                finalCellSize, finalWallHeight, finalWallThickness, wallPivotIsAtMeshCenter);
            converter.ConvertAll();
            Debug.Log($"[CellBasedMazeGenerator] {converter.GetStatistics()}");
        }
        
        // Logging
        private void LogGenerationSummary()
        {
            Debug.Log($"[CellBasedMazeGenerator] === GENERATION COMPLETE ===");
            Debug.Log($"[CellBasedMazeGenerator] Grid: {_width}x{_height}");
            Debug.Log($"[CellBasedMazeGenerator] Level: {_level} (Difficulty: {_difficulty.difficulty:P0})");
            Debug.Log($"[CellBasedMazeGenerator] Primary Path: {_primaryPath.Count} cells");
            Debug.Log($"[CellBasedMazeGenerator] Decoy Paths: {_decoyPaths.Count}");
            Debug.Log($"[CellBasedMazeGenerator] Path Efficiency: {_difficulty.PathEfficiency:P0}");
            Debug.Log($"[CellBasedMazeGenerator] Winding Factor: {_difficulty.WindingFactor:F1}x");
        }
        
        // Helper Methods
        private bool IsValidCell(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height;
        }
        
        private bool IsOnPrimaryPath(Vector2Int pos)
        {
            return _primaryPath.Contains(pos);
        }
        
        private bool CanFitRoom(Vector2Int center)
        {
            int roomSize = 3;
            int half = roomSize / 2;
            
            for (int x = -half; x <= half; x++)
            {
                for (int z = -half; z <= half; z++)
                {
                    var checkPos = center + new Vector2Int(x, z);
                    if (!IsValidCell(checkPos) || IsOnPrimaryPath(checkPos))
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        private void MarkRoomArea(Vector2Int center)
        {
            int roomSize = 3;
            int half = roomSize / 2;
            
            for (int x = -half; x <= half; x++)
            {
                for (int z = -half; z <= half; z++)
                {
                    var pos = center + new Vector2Int(x, z);
                    if (IsValidCell(pos))
                    {
                        var cell = _grid[pos.x, pos.y];
                        cell.cellType = CellType.Room;
                        cell.agreement = CellAgreement.RoomInterior;
                        _grid[pos.x, pos.y] = cell;
                    }
                }
            }
        }
        
        private Vector2Int GetPrimaryPathDirection(Vector2Int pos)
        {
            int index = _primaryPath.IndexOf(pos);
            if (index > 0 && index < _primaryPath.Count - 1)
            {
                var dir = _primaryPath[index + 1] - _primaryPath[index - 1];
                // Normalize manually for Vector2Int (Mathf.Sign returns float, cast to int)
                return new Vector2Int((int)Mathf.Sign(dir.x), (int)Mathf.Sign(dir.y));
            }
            return Vector2Int.right;
        }
        
        private Vector2Int GetPerpendicularDirection(Vector2Int dir)
        {
            if (dir == Vector2Int.up || dir == Vector2Int.down)
                return UnityEngine.Random.value > 0.5f ? Vector2Int.right : Vector2Int.left;
            else
                return UnityEngine.Random.value > 0.5f ? Vector2Int.up : Vector2Int.down;
        }
        
        private Vector2Int Turn90Degrees(Vector2Int dir)
        {
            if (dir == Vector2Int.up) return Vector2Int.right;
            if (dir == Vector2Int.right) return Vector2Int.down;
            if (dir == Vector2Int.down) return Vector2Int.left;
            if (dir == Vector2Int.left) return Vector2Int.up;
            return dir;
        }
        
        private void ShuffleArray<T>(T[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                T temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }
    }
    
    [System.Serializable]
    public struct DecoyPath
    {
        public DecoyType type;
        public List<Vector2Int> cells;
        public Vector2Int branchFrom;
        public CellAgreement reward;
        public bool hasDoor;
    }
    
    public enum DecoyType
    {
        LShape,
        Spiral,
        Fork,
        Chamber,
        Treasure,
        Guardian
    }
}
