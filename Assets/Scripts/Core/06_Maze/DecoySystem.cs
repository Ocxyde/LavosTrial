// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8 (no BOM) | Line Endings: Unix LF
// Cell-Based Maze Generation System - Phase 4: Decoy Path System

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core.Maze
{
    /// <summary>
    /// DecoySystem - Generates misleading paths (L-shape, Spiral, Fork).
    /// 
    /// Purpose:
    /// - Create intentional misdirection (player gets "lost")
    /// - Reward exploration (chests at dead-ends)
    /// - Increase maze complexity without breaking primary path
    /// 
    /// Decoy Types:
    /// - LShape: Simple 90-degree turn -> dead end
    /// - Spiral: Winding path -> loops back or dead end
    /// - Fork: Multiple choices (1 real, others fake)
    /// - Chamber: Small room -> door -> dead end
    /// - Treasure: Leads to chest reward
    /// - Guardian: Enemy blocks path
    /// 
    /// Plug-in-Out Compliant:
    /// - Uses MazeCell (passed data structure)
    /// - No component creation
    /// - All values from parameters
    /// </summary>
    public class DecoySystem
    {
        // Grid Reference
        private MazeCell[,] _grid;
        private int _width;
        private int _height;
        
        // Primary Path Reference
        private List<Vector2Int> _primaryPath;
        
        // Generated Decoys
        private List<DecoyPath> _decoys;
        
        // Configuration
        private float _decoyDensity;
        private int _level;
        
        // Properties
        public List<DecoyPath> Decoys => _decoys;
        public int DecoyCount => _decoys.Count;
        
        /// <summary>
        /// Initialize decoy system.
        /// </summary>
        public void Initialize(MazeCell[,] grid, int width, int height, 
                               List<Vector2Int> primaryPath, float decoyDensity, int level)
        {
            _grid = grid;
            _width = width;
            _height = height;
            _primaryPath = primaryPath;
            _decoyDensity = Mathf.Clamp01(decoyDensity);
            _level = level;
            _decoys = new List<DecoyPath>();
        }
        
        /// <summary>
        /// Generate all decoy paths.
        /// </summary>
        public List<DecoyPath> GenerateDecoys()
        {
            _decoys.Clear();
            
            if (_primaryPath.Count == 0)
            {
                Debug.LogWarning("[DecoySystem] No primary path - cannot create decoys!");
                return _decoys;
            }
            
            // Calculate decoy count based on density
            int decoyCount = Mathf.RoundToInt(_primaryPath.Count * _decoyDensity * 0.3f);
            decoyCount = Mathf.Clamp(decoyCount, 3, 15);
            
            Debug.Log($"[DecoySystem] Generating {decoyCount} decoys (density: {_decoyDensity:P0})");
            
            // Create different decoy types
            int lShapeCount = Mathf.RoundToInt(decoyCount * 0.5f);
            int spiralCount = Mathf.RoundToInt(decoyCount * 0.2f);
            int forkCount = Mathf.RoundToInt(decoyCount * 0.2f);
            int chamberCount = decoyCount - lShapeCount - spiralCount - forkCount;
            
            // Generate L-shape decoys
            for (int i = 0; i < lShapeCount; i++)
            {
                var decoy = CreateLShapeDecoy();
                if (decoy.cells.Count > 0)
                {
                    _decoys.Add(decoy);
                    MarkDecoyCells(decoy);
                }
            }
            
            // Generate spiral decoys
            for (int i = 0; i < spiralCount; i++)
            {
                var decoy = CreateSpiralDecoy();
                if (decoy.cells.Count > 0)
                {
                    _decoys.Add(decoy);
                    MarkDecoyCells(decoy);
                }
            }
            
            // Generate fork decoys
            for (int i = 0; i < forkCount; i++)
            {
                var decoy = CreateForkDecoy();
                if (decoy.cells.Count > 0)
                {
                    _decoys.Add(decoy);
                    MarkDecoyCells(decoy);
                }
            }
            
            // Generate chamber decoys
            for (int i = 0; i < chamberCount; i++)
            {
                var decoy = CreateChamberDecoy();
                if (decoy.cells.Count > 0)
                {
                    _decoys.Add(decoy);
                    MarkDecoyCells(decoy);
                }
            }
            
            Debug.Log($"[DecoySystem] Generated {_decoys.Count} decoy paths");
            return _decoys;
        }
        
        /// <summary>
        /// Create L-shape decoy (90-degree turn -> dead end).
        /// </summary>
        private DecoyPath CreateLShapeDecoy()
        {
            var decoy = new DecoyPath
            {
                type = DecoyType.LShape,
                cells = new List<Vector2Int>()
            };
            
            // Pick random branch point from primary path
            var branchPoint = GetRandomBranchPoint();
            if (branchPoint == Vector2Int.zero) return decoy;
            
            decoy.branchFrom = branchPoint;
            
            // Get primary path direction at branch
            Vector2Int primaryDir = GetPrimaryPathDirection(branchPoint);
            
            // Segment 1: Perpendicular to primary path
            Vector2Int segment1Dir = GetPerpendicularDirection(primaryDir);
            int segment1Length = UnityEngine.Random.Range(2, 5);
            
            var current = branchPoint;
            for (int i = 0; i < segment1Length; i++)
            {
                current += segment1Dir;
                if (IsValidCell(current) && !IsOnPrimaryPath(current))
                {
                    decoy.cells.Add(current);
                }
                else
                {
                    break;
                }
            }
            
            // Segment 2: Turn 90 degrees (L-shape)
            if (decoy.cells.Count > 0)
            {
                Vector2Int segment2Dir = Turn90Degrees(segment1Dir);
                int segment2Length = UnityEngine.Random.Range(1, 3);
                
                for (int i = 0; i < segment2Length; i++)
                {
                    current += segment2Dir;
                    if (IsValidCell(current) && !IsOnPrimaryPath(current))
                    {
                        decoy.cells.Add(current);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            
            // Mark as dead-end with possible reward
            if (decoy.cells.Count > 0)
            {
                decoy.reward = UnityEngine.Random.value < 0.5f ? 
                    CellAgreement.ChestLocation : CellAgreement.DeadEndTerm;
                decoy.hasDoor = false;
            }
            
            return decoy;
        }
        
        /// <summary>
        /// Create spiral decoy (winding path).
        /// </summary>
        private DecoyPath CreateSpiralDecoy()
        {
            var decoy = new DecoyPath
            {
                type = DecoyType.Spiral,
                cells = new List<Vector2Int>()
            };
            
            var branchPoint = GetRandomBranchPoint();
            if (branchPoint == Vector2Int.zero) return decoy;
            
            decoy.branchFrom = branchPoint;
            
            // Spiral directions (clockwise)
            var spiralDirs = new Vector2Int[]
            {
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.up
            };
            
            var current = branchPoint;
            int dirIndex = 0;
            int segmentLength = 2;
            int segments = 0;
            
            while (segments < 4)
            {
                var dir = spiralDirs[dirIndex % 4];
                
                for (int i = 0; i < segmentLength; i++)
                {
                    current += dir;
                    if (IsValidCell(current) && !IsOnPrimaryPath(current))
                    {
                        decoy.cells.Add(current);
                    }
                    else
                    {
                        decoy.reward = CellAgreement.DeadEndTerm;
                        return decoy;
                    }
                }
                
                dirIndex++;
                segments++;
                
                // Increase length every 2 segments
                if (segments % 2 == 0)
                {
                    segmentLength++;
                }
            }
            
            decoy.reward = CellAgreement.DeadEndTerm;
            return decoy;
        }
        
        /// <summary>
        /// Create fork decoy (multiple choices).
        /// </summary>
        private DecoyPath CreateForkDecoy()
        {
            var decoy = new DecoyPath
            {
                type = DecoyType.Fork,
                cells = new List<Vector2Int>()
            };
            
            var branchPoint = GetRandomBranchPoint();
            if (branchPoint == Vector2Int.zero) return decoy;
            
            decoy.branchFrom = branchPoint;
            
            // Create 2-3 branches
            int branchCount = UnityEngine.Random.Range(2, 4);
            var directions = new Vector2Int[]
            {
                Vector2Int.up, Vector2Int.down,
                Vector2Int.left, Vector2Int.right
            };
            
            // Shuffle directions
            ShuffleArray(directions);
            
            for (int i = 0; i < branchCount; i++)
            {
                var dir = directions[i];
                var branchCell = branchPoint + dir;
                
                if (IsValidCell(branchCell) && !IsOnPrimaryPath(branchCell))
                {
                    decoy.cells.Add(branchCell);
                    
                    // Extend branch slightly
                    var extendCell = branchCell + dir;
                    if (IsValidCell(extendCell))
                    {
                        decoy.cells.Add(extendCell);
                    }
                }
            }
            
            // One branch leads somewhere (others are fake)
            decoy.reward = CellAgreement.DeadEndTerm;
            return decoy;
        }
        
        /// <summary>
        /// Create chamber decoy (small room -> door -> dead end).
        /// </summary>
        private DecoyPath CreateChamberDecoy()
        {
            var decoy = new DecoyPath
            {
                type = DecoyType.Chamber,
                cells = new List<Vector2Int>(),
                hasDoor = true
            };
            
            var branchPoint = GetRandomBranchPoint();
            if (branchPoint == Vector2Int.zero) return decoy;
            
            decoy.branchFrom = branchPoint;
            
            // Short corridor to chamber
            var corridorDir = GetRandomValidDirection(branchPoint);
            int corridorLength = UnityEngine.Random.Range(2, 4);
            
            var current = branchPoint;
            for (int i = 0; i < corridorLength; i++)
            {
                current += corridorDir;
                if (IsValidCell(current) && !IsOnPrimaryPath(current))
                {
                    decoy.cells.Add(current);
                }
                else
                {
                    break;
                }
            }
            
            // Small chamber (2x2)
            if (decoy.cells.Count > 0)
            {
                var chamberStart = current;
                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        var chamberCell = chamberStart + new Vector2Int(x, y);
                        if (IsValidCell(chamberCell))
                        {
                            decoy.cells.Add(chamberCell);
                        }
                    }
                }
            }
            
            decoy.reward = UnityEngine.Random.value < 0.3f ? 
                CellAgreement.ChestLocation : CellAgreement.DeadEndTerm;
            return decoy;
        }
        
        /// <summary>
        /// Mark decoy cells in grid.
        /// </summary>
        private void MarkDecoyCells(DecoyPath decoy)
        {
            foreach (var cellPos in decoy.cells)
            {
                if (IsValidCell(cellPos))
                {
                    var cell = _grid[cellPos.x, cellPos.y];
                    cell.MarkAsDecoyPath();
                    
                    // Mark reward type
                    if (decoy.reward == CellAgreement.ChestLocation && cellPos == decoy.cells[decoy.cells.Count - 1])
                    {
                        cell.cellType = CellType.Treasure;
                    }
                    else if (decoy.reward == CellAgreement.DeadEndTerm && cellPos == decoy.cells[decoy.cells.Count - 1])
                    {
                        cell.MarkAsDeadEnd();
                    }
                    
                    _grid[cellPos.x, cellPos.y] = cell;
                }
            }
        }
        
        // Helper Methods
        private Vector2Int GetRandomBranchPoint()
        {
            if (_primaryPath.Count < 3) return Vector2Int.zero;
            
            int index = UnityEngine.Random.Range(1, _primaryPath.Count - 2);
            return _primaryPath[index];
        }
        
        private Vector2Int GetPrimaryPathDirection(Vector2Int pos)
        {
            int index = _primaryPath.IndexOf(pos);
            if (index > 0 && index < _primaryPath.Count - 1)
            {
                var dir = _primaryPath[index + 1] - _primaryPath[index - 1];
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
        
        private Vector2Int GetRandomValidDirection(Vector2Int from)
        {
            var directions = new Vector2Int[]
            {
                Vector2Int.up, Vector2Int.down,
                Vector2Int.left, Vector2Int.right
            };
            
            ShuffleArray(directions);
            
            foreach (var dir in directions)
            {
                var next = from + dir;
                if (IsValidCell(next) && !IsOnPrimaryPath(next))
                {
                    return dir;
                }
            }
            
            return Vector2Int.right;
        }
        
        private bool IsValidCell(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height;
        }
        
        private bool IsOnPrimaryPath(Vector2Int pos)
        {
            return _primaryPath.Contains(pos);
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
}
