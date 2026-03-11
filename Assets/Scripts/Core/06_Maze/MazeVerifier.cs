// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8 (no BOM) | Line Endings: Unix LF
// Cell-Based Maze Generation System - Phase 8: Verification & Safety

using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core.Maze
{
    /// <summary>
    /// MazeVerifier - Verifies maze integrity and safety.
    /// 
    /// Responsibilities:
    /// - Verify primary path is walkable (spawn to exit)
    /// - Verify all rooms have 2 doors (entry + exit)
    /// - Verify decoy paths terminate (no infinite loops)
    /// - Verify exit is reachable from spawn
    /// - Auto-fix issues when possible
    /// - Report verification statistics
    /// 
    /// Plug-in-Out Compliant:
    /// - Uses passed grid data (never fetches)
    /// - No component creation
    /// - All values from parameters
    /// </summary>
    public class MazeVerifier
    {
        // Grid Reference
        private MazeCell[,] _grid;
        private int _width;
        private int _height;
        
        // Path References
        private List<Vector2Int> _primaryPath;
        private List<DecoyPath> _decoys;
        private List<Room> _rooms;
        
        // Spawn/Exit
        private Vector2Int _spawn;
        private Vector2Int _exit;
        
        // Statistics
        private int _issuesFound;
        private int _issuesFixed;
        private bool _isVerified;
        
        // Results
        public bool IsVerified => _isVerified;
        public int IssuesFound => _issuesFound;
        public int IssuesFixed => _issuesFixed;
        public string Report { get; private set; }
        
        /// <summary>
        /// Initialize verifier.
        /// </summary>
        public void Initialize(
            MazeCell[,] grid, int width, int height,
            List<Vector2Int> primaryPath, List<DecoyPath> decoys, List<Room> rooms,
            Vector2Int spawn, Vector2Int exit)
        {
            _grid = grid;
            _width = width;
            _height = height;
            _primaryPath = primaryPath;
            _decoys = decoys;
            _rooms = rooms;
            _spawn = spawn;
            _exit = exit;
            
            _issuesFound = 0;
            _issuesFixed = 0;
            _isVerified = false;
            Report = string.Empty;
        }
        
        /// <summary>
        /// Verify entire maze.
        /// </summary>
        public bool VerifyAll()
        {
            Debug.Log("[MazeVerifier] Starting verification...");
            
            _issuesFound = 0;
            _issuesFixed = 0;
            
            // Verify primary path integrity
            VerifyPrimaryPath();
            
            // Verify exit reachability
            VerifyExitReachable();
            
            // Verify room doors
            VerifyRoomDoors();
            
            // Verify decoy termination
            VerifyDecoyTermination();
            
            // Verify no isolated cells
            VerifyNoIsolatedCells();
            
            // Generate report
            GenerateReport();
            
            _isVerified = _issuesFound == 0 || _issuesFound == _issuesFixed;
            
            Debug.Log($"[MazeVerifier] Complete: {_issuesFound} issues, {_issuesFixed} fixed, Verified: {_isVerified}");
            return _isVerified;
        }
        
        /// <summary>
        /// Verify primary path is walkable.
        /// </summary>
        private void VerifyPrimaryPath()
        {
            if (_primaryPath == null || _primaryPath.Count == 0)
            {
                _issuesFound++;
                Debug.LogError("[MazeVerifier] Primary path is null or empty!");
                return;
            }
            
            foreach (var cellPos in _primaryPath)
            {
                if (!IsValidCell(cellPos))
                {
                    _issuesFound++;
                    Debug.LogWarning($"[MazeVerifier] Primary path cell out of bounds: {cellPos}");
                    continue;
                }
                
                var cell = _grid[cellPos.x, cellPos.y];
                if (!cell.IsWalkable())
                {
                    _issuesFound++;
                    Debug.LogWarning($"[MazeVerifier] Primary path cell blocked: {cellPos}");
                    
                    // Auto-fix: clear the cell
                    cell = MazeCell.CreateEmpty();
                    cell.MarkAsPrimaryPath(cell.pathIndex);
                    _grid[cellPos.x, cellPos.y] = cell;
                    _issuesFixed++;
                }
            }
        }
        
        /// <summary>
        /// Verify exit is reachable from spawn.
        /// </summary>
        private void VerifyExitReachable()
        {
            if (!PathExists(_spawn, _exit))
            {
                _issuesFound++;
                Debug.LogError("[MazeVerifier] Exit is NOT reachable from spawn!");
                
                // Auto-fix: carve direct path
                CarveDirectPath(_spawn, _exit);
                _issuesFixed++;
            }
            else
            {
                Debug.Log("[MazeVerifier] Exit is reachable from spawn ✓");
            }
        }
        
        /// <summary>
        /// Verify all rooms have 2 doors.
        /// </summary>
        private void VerifyRoomDoors()
        {
            if (_rooms == null) return;
            
            foreach (var room in _rooms)
            {
                if (!room.doorsConnected)
                {
                    _issuesFound++;
                    Debug.LogWarning($"[MazeVerifier] Room at {room.center} has disconnected doors!");
                    
                    // Auto-fix: carve corridors to doors
                    if (IsValidCell(room.entryDoor))
                    {
                        var neighbor = room.entryDoor + Direction8Helper.ToOffset(room.entryDirection);
                        neighbor = new Vector2Int(neighbor.x, neighbor.y);
                        if (IsValidCell(neighbor))
                        {
                            var cell = _grid[neighbor.x, neighbor.y];
                            cell = MazeCell.CreateEmpty();
                            _grid[neighbor.x, neighbor.y] = cell;
                        }
                    }
                    
                    if (IsValidCell(room.exitDoor))
                    {
                        var neighbor = room.exitDoor + Direction8Helper.ToOffset(room.exitDirection);
                        neighbor = new Vector2Int(neighbor.x, neighbor.y);
                        if (IsValidCell(neighbor))
                        {
                            var cell = _grid[neighbor.x, neighbor.y];
                            cell = MazeCell.CreateEmpty();
                            _grid[neighbor.x, neighbor.y] = cell;
                        }
                    }
                    
                    _issuesFixed++;
                    room.doorsConnected = true;
                }
            }
        }
        
        /// <summary>
        /// Verify all decoy paths terminate (dead-ends).
        /// </summary>
        private void VerifyDecoyTermination()
        {
            if (_decoys == null) return;
            
            foreach (var decoy in _decoys)
            {
                if (decoy.cells.Count == 0)
                {
                    _issuesFound++;
                    Debug.LogWarning("[MazeVerifier] Empty decoy path found!");
                    continue;
                }
                
                // Check last cell is marked as dead-end
                var endCell = decoy.cells[decoy.cells.Count - 1];
                if (IsValidCell(endCell))
                {
                    var cell = _grid[endCell.x, endCell.y];
                    if (!cell.isDeadEnd && decoy.reward != CellAgreement.ChestLocation)
                    {
                        _issuesFound++;
                        Debug.LogWarning($"[MazeVerifier] Decoy path does not terminate: {endCell}");
                        
                        // Auto-fix: mark as dead-end
                        cell.MarkAsDeadEnd();
                        _grid[endCell.x, endCell.y] = cell;
                        _issuesFixed++;
                    }
                }
            }
        }
        
        /// <summary>
        /// Verify no isolated walkable cells exist.
        /// </summary>
        private void VerifyNoIsolatedCells()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var cell = _grid[x, y];
                    if (cell.IsWalkable() && !cell.isOnPrimaryPath && !cell.isOnDecoyPath)
                    {
                        // Check if connected to primary path
                        var pos = new Vector2Int(x, y);
                        if (!PathExists(pos, _spawn))
                        {
                            _issuesFound++;
                            Debug.LogWarning($"[MazeVerifier] Isolated walkable cell at {pos}");
                            
                            // Auto-fix: carve connection to nearest path
                            CarveConnectionToPath(pos);
                            _issuesFixed++;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Check if path exists between two points.
        /// </summary>
        private bool PathExists(Vector2Int start, Vector2Int end)
        {
            var visited = new HashSet<Vector2Int>();
            var queue = new Queue<Vector2Int>();
            
            queue.Enqueue(start);
            visited.Add(start);
            
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                
                if (current == end)
                    return true;
                
                // Check 4 cardinal directions
                var directions = new Vector2Int[]
                {
                    Vector2Int.up, Vector2Int.down,
                    Vector2Int.left, Vector2Int.right
                };
                
                foreach (var dir in directions)
                {
                    var next = current + dir;
                    if (IsValidCell(next) && !visited.Contains(next))
                    {
                        var cell = _grid[next.x, next.y];
                        if (cell.IsWalkable())
                        {
                            visited.Add(next);
                            queue.Enqueue(next);
                        }
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Carve direct path between two points.
        /// </summary>
        private void CarveDirectPath(Vector2Int start, Vector2Int end)
        {
            // Simple line carving (not optimal but works for emergency fix)
            var current = start;
            
            while (current != end)
            {
                if (IsValidCell(current))
                {
                    var cell = _grid[current.x, current.y];
                    cell = MazeCell.CreateEmpty();
                    _grid[current.x, current.y] = cell;
                }
                
                // Move toward end
                if (current.x < end.x) current.x++;
                else if (current.x > end.x) current.x--;
                else if (current.y < end.y) current.y++;
                else if (current.y > end.y) current.y--;
            }
        }
        
        /// <summary>
        /// Carve connection from isolated cell to nearest path.
        /// </summary>
        private void CarveConnectionToPath(Vector2Int from)
        {
            // Find nearest primary path cell
            Vector2Int? nearestPath = null;
            float nearestDist = float.MaxValue;
            
            foreach (var pathCell in _primaryPath)
            {
                float dist = Vector2Int.Distance(from, pathCell);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestPath = pathCell;
                }
            }
            
            if (nearestPath.HasValue)
            {
                CarveDirectPath(from, nearestPath.Value);
            }
        }
        
        /// <summary>
        /// Generate verification report.
        /// </summary>
        private void GenerateReport()
        {
            Report = $"MazeVerifier Report:\n" +
                    $"- Issues Found: {_issuesFound}\n" +
                    $"- Issues Fixed: {_issuesFixed}\n" +
                    $"- Verified: {_isVerified}\n" +
                    $"- Primary Path: {_primaryPath?.Count ?? 0} cells\n" +
                    $"- Decoy Paths: {_decoys?.Count ?? 0}\n" +
                    $"- Rooms: {_rooms?.Count ?? 0}";
        }
        
        /// <summary>
        /// Check if cell position is valid.
        /// </summary>
        private bool IsValidCell(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height;
        }
    }
}
