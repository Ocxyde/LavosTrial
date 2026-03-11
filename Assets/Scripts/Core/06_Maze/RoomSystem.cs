﻿﻿﻿﻿﻿﻿// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8 (no BOM) | Line Endings: Unix LF
// Cell-Based Maze Generation System - Phase 3: Room System

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core.Maze
{
    /// <summary>
    /// RoomSystem - Manages room generation, wall surrounding, and door placement.
    /// 
    /// Responsibilities:
    /// - Generate 3x3 rooms along primary path
    /// - Surround rooms with walls
    /// - Place 2 doors (entry + exit)
    /// - Verify doors lead to walkable cells
    /// - Support room types (Normal/Treasure/Boss/Safe)
    /// 
    /// Plug-in-Out Compliant:
    /// - Uses MazeCell (passed data structure)
    /// - No component creation
    /// - All values from parameters
    /// </summary>
    public class RoomSystem
    {
        // Room Configuration
        private int _roomSize = 3;
        private List<Room> _rooms;
        
        // Grid Reference
        private MazeCell[,] _grid;
        private int _width;
        private int _height;
        
        // Primary Path Reference
        private List<Vector2Int> _primaryPath;
        
        // Properties
        public List<Room> Rooms => _rooms;
        public int RoomCount => _rooms.Count;
        
        /// <summary>
        /// Initialize room system.
        /// </summary>
        public void Initialize(MazeCell[,] grid, int width, int height, List<Vector2Int> primaryPath)
        {
            _grid = grid;
            _width = width;
            _height = height;
            _primaryPath = primaryPath;
            _rooms = new List<Room>();
        }
        
        /// <summary>
        /// Generate rooms along primary path.
        /// </summary>
        public List<Room> GenerateRooms(int roomCount)
        {
            _rooms.Clear();
            
            if (_primaryPath.Count == 0)
            {
                Debug.LogWarning("[RoomSystem] No primary path - cannot place rooms!");
                return _rooms;
            }
            
            // Distribute rooms along primary path
            int step = _primaryPath.Count / (roomCount + 1);
            
            for (int i = 0; i < roomCount; i++)
            {
                int index = (i + 1) * step;
                if (index < _primaryPath.Count)
                {
                    var center = _primaryPath[index];
                    
                    if (CanFitRoom(center))
                    {
                        var room = Room.Create(center, _roomSize);
                        _rooms.Add(room);
                        
                        // Clear room area
                        ClearRoomArea(ref room);
                        
                        // Surround with walls
                        SurroundWithWalls(ref room);
                        
                        // Place 2 doors (entry + exit)
                        PlaceTwoDoors(ref room);
                        
                        // Verify doors lead to walkable cells
                        VerifyDoorsConnected(ref room);
                        
                        room.Log();
                    }
                }
            }
            
            Debug.Log($"[RoomSystem] Generated {_rooms.Count} rooms");
            return _rooms;
        }
        
        /// <summary>
        /// Check if room can fit at position.
        /// </summary>
        private bool CanFitRoom(Vector2Int center)
        {
            int half = _roomSize / 2;

            // Check all room cells are in bounds
            for (int x = -half; x <= half; x++)
            {
                for (int y = -half; y <= half; y++)
                {
                    var pos = center + new Vector2Int(x, y);
                    if (pos.x < 0 || pos.x >= _width || pos.y < 0 || pos.y >= _height)
                    {
                        return false;
                    }
                }
            }

            // Check no overlap with existing rooms
            foreach (var existingRoom in _rooms)
            {
                int dx = Mathf.Abs(center.x - existingRoom.center.x);
                int dy = Mathf.Abs(center.y - existingRoom.center.y);

                if (dx < _roomSize + 1 && dy < _roomSize + 1)
                {
                    return false; // Too close to existing room
                }
            }

            return true;
        }

        /// <summary>
        /// Clear room area (make all interior cells empty).
        /// </summary>
        private void ClearRoomArea(ref Room room)
        {
            var cells = room.GetRoomCells();

            foreach (var cellPos in cells)
            {
                if (cellPos.x >= 0 && cellPos.x < _width && cellPos.y >= 0 && cellPos.y < _height)
                {
                    var cell = _grid[cellPos.x, cellPos.y];
                    cell = MazeCell.CreateEmpty();
                    cell.cellType = CellType.Room;
                    cell.agreement = CellAgreement.RoomInterior;
                    _grid[cellPos.x, cellPos.y] = cell;
                }
            }
            
            room.isCleared = true;
        }
        
        /// <summary>
        /// Surround room with walls on all 4 sides.
        /// </summary>
        private void SurroundWithWalls(ref Room room)
        {
            var boundary = room.GetBoundaryCells();

            foreach (var boundaryPos in boundary)
            {
                if (boundaryPos.x >= 0 && boundaryPos.x < _width && boundaryPos.y >= 0 && boundaryPos.y < _height)
                {
                    var cell = _grid[boundaryPos.x, boundaryPos.y];

                    // Determine which wall edge to set
                    int dx = boundaryPos.x - room.center.x;
                    int dy = boundaryPos.y - room.center.y;

                    if (dy > room.HalfHeight) // North boundary
                        cell.hasSouthWall = true;
                    else if (dy < -room.HalfHeight) // South boundary
                        cell.hasNorthWall = true;
                    else if (dx > room.HalfWidth) // East boundary
                        cell.hasWestWall = true;
                    else if (dx < -room.HalfWidth) // West boundary
                        cell.hasEastWall = true;

                    cell.cellType = CellType.Wall;
                    _grid[boundaryPos.x, boundaryPos.y] = cell;
                }
            }

            room.isSurrounded = true;
        }
        
        /// <summary>
        /// Place 2 doors (entry + exit) on opposite or adjacent walls.
        /// </summary>
        private void PlaceTwoDoors(ref Room room)
        {
            var validPositions = FindValidDoorPositions(room);
            
            if (validPositions.Count < 2)
            {
                Debug.LogWarning($"[RoomSystem] Room at {room.center} has only {validPositions.Count} valid door positions!");
                ForceCarveDoorPositions(ref room);
                return;
            }
            
            // Select entry and exit (prefer opposite walls)
            var entry = validPositions[0];
            var exit = SelectOppositeWall(entry, validPositions);
            
            room.entryDoor = entry.pos;
            room.entryDirection = entry.dir;
            room.exitDoor = exit.pos;
            room.exitDirection = exit.dir;
            
            // Carve door openings
            CarveDoorOpening(room.entryDoor, room.entryDirection);
            CarveDoorOpening(room.exitDoor, room.exitDirection);
        }
        
        /// <summary>
        /// Find valid door positions (walls adjacent to walkable cells).
        /// </summary>
        private List<(Vector2Int pos, Direction8 dir)> FindValidDoorPositions(Room room)
        {
            var positions = new List<(Vector2Int pos, Direction8 dir)>();

            // Check North wall
            var northPos = room.center + new Vector2Int(0, room.HalfHeight);
            var northNeighbor = room.center + new Vector2Int(0, room.HalfHeight + 1);
            if (IsValidDoorPosition(northPos, northNeighbor, Direction8.N))
                positions.Add((northPos, Direction8.N));

            // Check South wall
            var southPos = room.center + new Vector2Int(0, -room.HalfHeight);
            var southNeighbor = room.center + new Vector2Int(0, -room.HalfHeight - 1);
            if (IsValidDoorPosition(southPos, southNeighbor, Direction8.S))
                positions.Add((southPos, Direction8.S));

            // Check East wall
            var eastPos = room.center + new Vector2Int(room.HalfWidth, 0);
            var eastNeighbor = room.center + new Vector2Int(room.HalfWidth + 1, 0);
            if (IsValidDoorPosition(eastPos, eastNeighbor, Direction8.E))
                positions.Add((eastPos, Direction8.E));

            // Check West wall
            var westPos = room.center + new Vector2Int(-room.HalfWidth, 0);
            var westNeighbor = room.center + new Vector2Int(-room.HalfWidth - 1, 0);
            if (IsValidDoorPosition(westPos, westNeighbor, Direction8.W))
                positions.Add((westPos, Direction8.W));

            return positions;
        }
        
        /// <summary>
        /// Check if door position is valid (leads to walkable cell).
        /// </summary>
        private bool IsValidDoorPosition(Vector2Int doorPos, Vector2Int neighbor, Direction8 dir)
        {
            if (!IsValidCell(doorPos) || !IsValidCell(neighbor))
                return false;
            
            // Neighbor must be walkable (corridor or path)
            var neighborCell = _grid[neighbor.x, neighbor.y];
            return neighborCell.IsWalkable();
        }
        
        /// <summary>
        /// Select door position on opposite wall.
        /// </summary>
        private (Vector2Int pos, Direction8 dir) SelectOppositeWall(
            (Vector2Int pos, Direction8 dir) entry,
            List<(Vector2Int pos, Direction8 dir)> positions)
        {
            // Find opposite direction
            Direction8 opposite = GetOppositeDirection(entry.dir);

            // Look for position on opposite wall
            foreach (var item in positions)
            {
                if (item.dir == opposite && item.pos != entry.pos)
                {
                    return item;
                }
            }

            // If no opposite, select any adjacent wall
            foreach (var item in positions)
            {
                if (item.pos != entry.pos)
                {
                    return item;
                }
            }

            // Fallback: use entry (not ideal but works)
            return entry;
        }
        
        /// <summary>
        /// Force carve door positions if not enough valid positions found.
        /// </summary>
        private void ForceCarveDoorPositions(ref Room room)
        {
            // Carve corridors to make positions valid
            Debug.Log($"[RoomSystem] Force carving doors for room at {room.center}");
            
            // North door
            var northPos = room.center + new Vector2Int(0, room.HalfHeight);
            var northNeighbor = room.center + new Vector2Int(0, room.HalfHeight + 1);
            if (IsValidCell(northPos) && IsValidCell(northNeighbor))
            {
                CarveCorridorTo(northNeighbor);
                room.entryDoor = northPos;
                room.entryDirection = Direction8.N;
            }
            
            // South door
            var southPos = room.center + new Vector2Int(0, -room.HalfHeight);
            var southNeighbor = room.center + new Vector2Int(0, -room.HalfHeight - 1);
            if (IsValidCell(southPos) && IsValidCell(southNeighbor))
            {
                CarveCorridorTo(southNeighbor);
                room.exitDoor = southPos;
                room.exitDirection = Direction8.S;
            }
        }
        
        /// <summary>
        /// Carve door opening (remove wall at door position).
        /// </summary>
        private void CarveDoorOpening(Vector2Int doorPos, Direction8 direction)
        {
            if (!IsValidCell(doorPos)) return;
            
            var cell = _grid[doorPos.x, doorPos.y];
            cell.RemoveWall(direction);
            cell.agreement = CellAgreement.Doorway;
            _grid[doorPos.x, doorPos.y] = cell;
        }
        
        /// <summary>
        /// Carve corridor to position (make walkable).
        /// </summary>
        private void CarveCorridorTo(Vector2Int pos)
        {
            if (!IsValidCell(pos)) return;
            
            var cell = _grid[pos.x, pos.y];
            cell = MazeCell.CreateEmpty();
            cell.agreement = CellAgreement.Corridor;
            _grid[pos.x, pos.y] = cell;
        }
        
        /// <summary>
        /// Verify both doors lead to walkable cells.
        /// </summary>
        private void VerifyDoorsConnected(ref Room room)
        {
            bool entryConnected = IsDoorConnected(room.entryDoor, room.entryDirection);
            bool exitConnected = IsDoorConnected(room.exitDoor, room.exitDirection);
            
            room.doorsConnected = entryConnected && exitConnected;
            
            if (!room.doorsConnected)
            {
                Debug.LogWarning($"[RoomSystem] Room at {room.center} has disconnected doors!");

                // Fix: carve corridors to connect doors
                if (!entryConnected)
                {
                    var entryOffset = Direction8Helper.ToOffset(room.entryDirection);
                    CarveCorridorTo(room.entryDoor + new Vector2Int(entryOffset.dx, entryOffset.dz));
                }

                if (!exitConnected)
                {
                    var exitOffset = Direction8Helper.ToOffset(room.exitDirection);
                    CarveCorridorTo(room.exitDoor + new Vector2Int(exitOffset.dx, exitOffset.dz));
                }

                room.doorsConnected = true;
            }
        }

        /// <summary>
        /// Check if door leads to walkable cell.
        /// </summary>
        private bool IsDoorConnected(Vector2Int doorPos, Direction8 direction)
        {
            var offset = Direction8Helper.ToOffset(direction);
            var neighbor = doorPos + new Vector2Int(offset.dx, offset.dz);
            
            if (!IsValidCell(neighbor)) return false;
            
            var neighborCell = _grid[neighbor.x, neighbor.y];
            return neighborCell.IsWalkable();
        }
        
        /// <summary>
        /// Get opposite direction.
        /// </summary>
        private Direction8 GetOppositeDirection(Direction8 dir)
        {
            return Direction8Helper.Opposite(dir);
        }
        
        /// <summary>
        /// Check if cell position is valid.
        /// </summary>
        private bool IsValidCell(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height;
        }
        
        /// <summary>
        /// Set room type (for treasure/boss/safe rooms).
        /// </summary>
        public void SetRoomType(ref Room room, RoomType type)
        {
            room.type = type;
            
            // Mark room cells with type
            var cells = room.GetRoomCells();
            foreach (var cellPos in cells)
            {
                if (IsValidCell(cellPos))
                {
                    var cell = _grid[cellPos.x, cellPos.y];
                    
                    switch (type)
                    {
                        case RoomType.Treasure:
                            cell.cellType = CellType.Treasure;
                            break;
                        case RoomType.Safe:
                            cell.cellType = CellType.Safe;
                            break;
                        case RoomType.Boss:
                        case RoomType.Trap:
                            cell.cellType = CellType.EnemyGuard;
                            break;
                    }
                    
                    _grid[cellPos.x, cellPos.y] = cell;
                }
            }
        }
    }
}
