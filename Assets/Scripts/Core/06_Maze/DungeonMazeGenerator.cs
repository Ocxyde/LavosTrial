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
// DungeonMazeGenerator.cs
// Advanced procedural dungeon maze generator with AI-based difficulty scaling
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core.Advanced
{
    /// <summary>
    /// DungeonMazeGenerator - Advanced procedural dungeon maze generator with AI-based difficulty scaling.
    /// 
    /// Combines multiple generation techniques for complex, varied dungeon layouts:
    /// - 8-axis procedural generation with DFS (Depth-First Search)
    /// - Multi-corridor entrance/exit with branching dead-ends
    /// - Trap room systems (spikes, lava, darkness)
    /// - Treasure chambers guarded by enemies
    /// - Winding corridors and crossroads
    /// - AI-based adaptive difficulty scaling
    /// 
    /// Generation Pipeline:
    /// 1. Initialize maze data structure
    /// 2. Fill all walls (solid block)
    /// 3. DFS carve main passages (8-directional)
    /// 4. Carve spawn room (55 guaranteed safe area)
    /// 5. Carve exit room (distant, difficult to reach)
    /// 6. Identify and expand dead-ends into chambers
    /// 7. Identify and expand crossroads into meeting halls
    /// 8. Place trap rooms in strategic dead-ends
    /// 9. Place treasure chambers guarded by enemies
    /// 10. Carve winding corridors for complexity
    /// 11. Calculate AI difficulty metrics
    /// 12. Place torches, chests, enemies based on difficulty
    /// 13. Guarantee A* path from spawn to exit
    /// 
    /// Architecture: Plug-in-out - finds all components, never creates them
    /// Replaces GridMazeGenerator8 while maintaining compatibility
    /// </summary>
    public sealed class DungeonMazeGenerator
    {
        // 
        // INTERNAL STATE
        // 
        private DungeonMazeData _mazeData;
        private System.Random _rng;
        private bool[,] _visited;
        private List<(int x, int z)> _deadEnds;
        private List<(int x, int z)> _crossroads;
        private List<(int x, int z)> _trapRooms;
        private List<(int x, int z)> _treasureRooms;
        private AIAdaptiveDifficulty _aiDifficulty;

        // 
        // PUBLIC API - Generate complete dungeon
        // 

        /// <summary>
        /// Generate a complete advanced dungeon maze with all features.
        /// </summary>
        /// <param name="seed">Random seed for reproducibility. Same seed generates identical dungeon.</param>
        /// <param name="level">Difficulty level (0-39). Higher levels = larger mazes, more traps, more treasure.</param>
        /// <param name="cfg">Dungeon maze configuration with parameters from JSON config.</param>
        /// <returns>Generated DungeonMazeData with complete structure, spawn/exit, traps, treasure, and AI metrics.</returns>
        /// 
        /// <remarks>
        /// <para><strong>Generation Pipeline (13 Phases):</strong></para>
        /// <list type="number">
        /// <item><description><strong>Initialize:</strong> Create maze data structure with dimensions</description></item>
        /// <item><description><strong>Solid Block:</strong> Fill all cells with walls (8-axis)</description></item>
        /// <item><description><strong>DFS Carving:</strong> 8-directional depth-first search passages</description></item>
        /// <item><description><strong>Spawn Room:</strong> 55 guaranteed safe area at start</description></item>
        /// <item><description><strong>Exit Room:</strong> Distant room, difficult to reach</description></item>
        /// <item><description><strong>Chamber Expansion:</strong> Expand dead-ends and crossroads (2525+ mazes only)</description></item>
        /// <item><description><strong>Trap Rooms:</strong> Place spike/lava/darkness traps strategically</description></item>
        /// <item><description><strong>Treasure Chambers:</strong> Place guarded treasure rooms</description></item>
        /// <item><description><strong>Winding Corridors:</strong> Add labyrinthine paths for complexity (2525+ only)</description></item>
        /// <item><description><strong>AI Analysis:</strong> Calculate adaptive difficulty multiplier</description></item>
        /// <item><description><strong>Path Guarantee:</strong> Ensure A* path from spawn to exit</description></item>
        /// <item><description><strong>Object Placement:</strong> Torches, chests, enemies based on density</description></item>
        /// <item><description><strong>Finalize:</strong> Store generation time and metrics</description></item>
        /// </list>
        /// 
        /// <para><strong>Difficulty Scaling:</strong></para>
        /// <para>Level 0: Base size, 1.0 difficulty, few traps/treasures</para>
        /// <para>Level 39: 2.5 size, 3.0 AI difficulty, maximum traps/treasures</para>
        /// 
        /// <para><strong>Special Features:</strong></para>
        /// <para>- Chamber expansion skipped for mazes &lt; 2525 (preserves wall structure)</para>
        /// <para>- Treasure rooms marked but not cleared for small mazes</para>
        /// <para>- AI adaptive factor based on trap/treasure distribution and path complexity</para>
        /// </remarks>
        public DungeonMazeData Generate(int seed, int level, DungeonMazeConfig cfg)
        {
            ValidateConfig(cfg);

            var scaler = cfg.Difficulty;
            int size = scaler.MazeSize(level, cfg.BaseSize, cfg.MinSize, cfg.MaxSize);

            _rng = new System.Random(seed);
            _mazeData = new DungeonMazeData(size, size, seed, level)
            {
                DifficultyFactor = scaler.Factor(level),
                Config = cfg,
            };

            _visited = new bool[size, size];
            _deadEnds = new List<(int, int)>();
            _crossroads = new List<(int, int)>();
            _trapRooms = new List<(int, int)>();
            _treasureRooms = new List<(int, int)>();
            _aiDifficulty = new AIAdaptiveDifficulty(level, cfg.AISettings);

            Debug.Log($"[DungeonGen] Level {level} | Size {size}x{size} | " +
                      $"Difficulty {_mazeData.DifficultyFactor:F2} | Seed {seed}");

            // === PHASE 1: SOLID BLOCK ===
            FillAllWalls();
            Debug.Log($"[DungeonGen] Phase 1: All walls filled ({size}x{size} cells) - SOLID BLOCK");

            // === PHASE 2: CARVE ENTRANCE/EXIT ROOMS (4x4) WITH DOORS ===
            int roomSize = 4;  // 4x4 rooms - compact but functional
            CarveEntranceRoomWithDoor(1, 1, roomSize);
            CarveExitRoomWithDoor(size - 1 - roomSize, size - 1 - roomSize, roomSize);
            Debug.Log($"[DungeonGen] Phase 2: Rooms carved ({roomSize}x{roomSize}) with unlocked doors");

            // === PHASE 3: DFS MAZE CARVING (respects rooms) ===
            CarveMazeDFS(1 + roomSize, 1 + (roomSize / 2));  // Start from entrance room edge
            Debug.Log($"[DungeonGen] Phase 3: DFS maze carving complete");

            // === PHASE 4: A* PATH GUARANTEE - CARVE MAZE-LIKE PATH ===
            bool pathExists = EnsurePathAStar();
            if (!pathExists)
            {
                Debug.LogWarning("[DungeonGen] DFS did not create path - A* will carve maze-like connection");
            }
            else
            {
                Debug.Log("[DungeonGen] Phase 4: A* validated path exists (entrance  exit)");
            }

            // === PHASE 4: Chamber Expansion ===
            // Skip chamber expansion for mazes < 25x25 to preserve walls
            // Chamber expansion clears too many walls in small/medium mazes
            if (_mazeData.Width >= 25 && _mazeData.Height >= 25)
            {
                IdentifyDeadEndsAndCrossroads();
                ExpandChambers(cfg.ChamberExpansionRadius);
                Debug.Log($"[DungeonGen] Phase 4: {_deadEnds.Count} dead-ends | " +
                          $"{_crossroads.Count} crossroads (chambers expanded)");
            }
            else
            {
                Debug.Log($"[DungeonGen] Phase 4: Skipped chamber expansion for maze ({_mazeData.Width}x{_mazeData.Height})");
            }

            // === PHASE 5: Trap Room Placement ===
            PlaceTrapRooms(cfg.TrapDensity);
            Debug.Log($"[DungeonGen] Phase 5: {_trapRooms.Count} trap rooms placed");

            // === PHASE 6: Treasure Chamber Placement ===
            // Skip treasure room clearing for mazes < 25x25 to preserve walls
            if (_mazeData.Width >= 25 && _mazeData.Height >= 25)
            {
                PlaceTreasureRooms(cfg.TreasureDensity);
                Debug.Log($"[DungeonGen] Phase 6: {_treasureRooms.Count} treasure rooms placed");
            }
            else
            {
                // Just mark treasure rooms without clearing walls
                PlaceTreasureRoomsNoClear(cfg.TreasureDensity);
                Debug.Log($"[DungeonGen] Phase 6: {_treasureRooms.Count} treasure rooms marked (no clearing)");
            }

            // === PHASE 7: Winding Corridors ===
            // Skip winding corridors for mazes < 25x25 to preserve walls
            if (_mazeData.Width >= 25 && _mazeData.Height >= 25)
            {
                CarveLabyrinthinePaths(cfg.CorridorWindingFactor);
                Debug.Log($"[DungeonGen] Phase 7: Winding corridors carved");
            }
            else
            {
                Debug.Log($"[DungeonGen] Phase 7: Skipped winding corridors for maze ({_mazeData.Width}x{_mazeData.Height})");
            }

            // === PHASE 8: AI Difficulty Calculation ===
            _aiDifficulty.AnalyzeMaze(_mazeData, _trapRooms.Count, _treasureRooms.Count);
            _mazeData.AIAdaptiveFactor = _aiDifficulty.ComputedDifficultyMultiplier;
            Debug.Log($"[DungeonGen] Phase 8: AI difficulty = {_mazeData.AIAdaptiveFactor:F2}");

            // === PHASE 9: Guaranteed Path ===
            EnsurePathToExit();
            Debug.Log($"[DungeonGen] Phase 9: Guaranteed spawn-to-exit path");

            // === PHASE 10: Object Placement ===
            PlaceTorches(cfg.TorchChance * _mazeData.DifficultyFactor);
            PlaceEnemies(cfg.EnemyDensity);
            PlaceChests(cfg.ChestDensity);
            Debug.Log($"[DungeonGen] Phase 10: Objects placed");

            Debug.Log($"[DungeonGen] COMPLETE - Generation took {_mazeData.GenerationTimeMs:F1}ms");
            return _mazeData;
        }

        // 
        // PHASE 1: Initialize Walls
        // 
        private void FillAllWalls()
        {
            int w = _mazeData.Width;
            int h = _mazeData.Height;

            for (int x = 0; x < w; x++)
            {
                for (int z = 0; z < h; z++)
                {
                    var cell = _mazeData.GetCell(x, z);
                    cell |= CellFlags8.Wall_All;
                    _mazeData.SetCell(x, z, cell);
                }
            }
        }

        // 
        // PHASE 2: Main Passages - 8-axis DFS
        // 
        private void CarvePassages8(int startX, int startZ)
        {
            var stack = new Stack<(int, int)>();
            stack.Push((startX, startZ));
            _visited[startX, startZ] = true;

            // DFS carves passages through the maze
            // We visit most cells to ensure connectivity, but preserve some walls
            int maxCellsToVisit = Mathf.RoundToInt(_mazeData.Width * _mazeData.Height * 0.85f);
            int cellsVisited = 0;

            while (stack.Count > 0 && cellsVisited < maxCellsToVisit)
            {
                var (cx, cz) = stack.Peek();
                var unvisited = GetUnvisitedNeighbors8(cx, cz);

                if (unvisited.Count == 0)
                {
                    stack.Pop();
                }
                else
                {
                    var (nx, nz, dir) = unvisited[_rng.Next(unvisited.Count)];
                    CarvePassageToNeighbor(cx, cz, nx, nz, dir);
                    _visited[nx, nz] = true;
                    stack.Push((nx, nz));
                    cellsVisited++;
                }
            }

            Debug.Log($"[DungeonGen] DFS carved {cellsVisited}/{maxCellsToVisit} cells ({cellsVisited * 100 / (_mazeData.Width * _mazeData.Height)}% of maze)");
        }

        private List<(int nx, int nz, Direction8 dir)> GetUnvisitedNeighbors8(int x, int z)
        {
            var neighbors = new List<(int, int, Direction8)>();
            
            Direction8[] allDirections = (Direction8[])Enum.GetValues(typeof(Direction8));
            
            foreach (var dir in allDirections)
            {
                var (dx, dz) = Direction8Helper.ToOffset(dir);
                // Move 2 steps to carve passages with walls between them
                int nx = x + dx * 2;
                int nz = z + dz * 2;

                if (_mazeData.InBounds(nx, nz) && !_visited[nx, nz])
                {
                    neighbors.Add((nx, nz, dir));
                }
            }

            return neighbors;
        }

        private void CarvePassageToNeighbor(int cx, int cz, int nx, int nz, Direction8 dir)
        {
            var curCell = _mazeData.GetCell(cx, cz);
            var neiCell = _mazeData.GetCell(nx, nz);

            // Remove wall from current cell in direction of neighbor
            uint wallFlag = Direction8Helper.ToWallFlag(dir);
            curCell &= ~wallFlag;

            // Remove opposite wall from neighbor
            Direction8 oppositeDir = Direction8Helper.Opposite(dir);
            uint oppositeWallFlag = Direction8Helper.ToWallFlag(oppositeDir);
            neiCell &= ~oppositeWallFlag;

            // For diagonal passages, also clear the intermediate cell (the corner)
            // But DON'T clear all walls - just remove the corner walls to create a smooth passage
            if (Direction8Helper.IsDiagonal(dir))
            {
                var (dx, dz) = Direction8Helper.ToOffset(dir);
                // Intermediate cell is 1 step in the direction
                int intermediateX = cx + dx;
                int intermediateZ = cz + dz;
                if (_mazeData.InBounds(intermediateX, intermediateZ))
                {
                    var intermediateCell = _mazeData.GetCell(intermediateX, intermediateZ);
                    // Remove the two cardinal walls that face the current and neighbor cells
                    // This creates a diagonal passage corner without completely clearing the cell
                    var cardinalWall1 = Direction8Helper.ToWallFlag(GetCardinalDirection(dx, 0));
                    var cardinalWall2 = Direction8Helper.ToWallFlag(GetCardinalDirection(0, dz));
                    intermediateCell &= ~cardinalWall1;
                    intermediateCell &= ~cardinalWall2;
                    _mazeData.SetCell(intermediateX, intermediateZ, intermediateCell);
                }
            }

            _mazeData.SetCell(cx, cz, curCell);
            _mazeData.SetCell(nx, nz, neiCell);
        }

        // Helper to get cardinal direction from offset
        private static Direction8 GetCardinalDirection(int dx, int dz)
        {
            if (dx > 0) return Direction8.E;
            if (dx < 0) return Direction8.W;
            if (dz > 0) return Direction8.N;
            if (dz < 0) return Direction8.S;
            return Direction8.N; // Default
        }

        // 
        // PHASE 3: Spawn & Exit Rooms
        // 
        private void CarveSpawnRoom(int centerX, int centerZ, int radius)
        {
            ClearRoomAround(centerX, centerZ, radius);
            _mazeData.SetSpawnRoom(centerX, centerZ, radius);
        }

        private void CarveExitRoom(int centerX, int centerZ, int radius)
        {
            ClearRoomAround(centerX, centerZ, radius);
            _mazeData.SetExitRoom(centerX, centerZ, radius);
        }

        private void ClearRoomAround(int centerX, int centerZ, int radius)
        {
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                for (int z = centerZ - radius; z <= centerZ + radius; z++)
                {
                    if (_mazeData.InBounds(x, z))
                    {
                        // Clear all walls to create open room space
                        _mazeData.SetCell(x, z, 0);
                    }
                }
            }
        }

        // Connect a room to the main maze by carving a passage from its edge
        private void ConnectRoomToMaze(int roomCenterX, int roomCenterZ, int radius)
        {
            // Try to carve passages in all 4 cardinal directions from room edge
            var directions = new (int dx, int dz, Direction8 dir)[]
            {
                (0, 1, Direction8.N),   // North
                (0, -1, Direction8.S),  // South
                (1, 0, Direction8.E),   // East
                (-1, 0, Direction8.W),  // West
            };

            foreach (var (dx, dz, dir) in directions)
            {
                // Start from room edge
                int startX = roomCenterX + dx * (radius + 1);
                int startZ = roomCenterZ + dz * (radius + 1);

                // Carve a passage until we hit an already-carved cell
                for (int i = 0; i < 5; i++)
                {
                    int cx = startX + dx * i;
                    int cz = startZ + dz * i;

                    if (!_mazeData.InBounds(cx, cz))
                        break;

                    var cell = _mazeData.GetCell(cx, cz);

                    // If cell already has passages (some walls removed), stop here
                    if ((cell & CellFlags8.Wall_All) != CellFlags8.Wall_All)
                        break;

                    // Clear passage through this cell completely
                    _mazeData.SetCell(cx, cz, 0);
                    ClearWallsAroundCell(cx, cz);
                }
            }
        }

        // 
        // NEW: ENTRANCE/EXIT ROOMS WITH DOORS (4x4)
        // 

        /// <summary>
        /// Carve a 4x4 entrance room with an unlocked door to the maze.
        /// Player can easily enter/exit through this door.
        /// </summary>
        private void CarveEntranceRoomWithDoor(int roomX, int roomZ, int roomSize)
        {
            // Step 1: Clear 4x4 area (make walkable)
            for (int x = roomX; x < roomX + roomSize; x++)
            for (int z = roomZ; z < roomZ + roomSize; z++)
            {
                if (_mazeData.InBounds(x, z))
                {
                    _mazeData.SetCell(x, z, 0);  // Clear all walls
                    _mazeData.MarkAsSpawnRoom(x, z);  // Mark as spawn room cell
                }
            }

            // Step 2: Carve doorway on East edge of room (connects to maze)
            int doorX = roomX + roomSize;  // East edge
            int doorZ = roomZ + (roomSize / 2);  // Center of room
            if (_mazeData.InBounds(doorX, doorZ))
            {
                _mazeData.SetCell(doorX, doorZ, 0);  // Clear wall at door position
            }

            // Step 3: Mark door position in maze data (for CompleteMazeBuilder to spawn door)
            // Door faces East (from room into maze)
            Debug.Log($"[DungeonGen] Entrance room: {roomSize}x{roomSize} at ({roomX},{roomZ}) with door at ({doorX},{doorZ})");

            // Set spawn point in center of entrance room
            int spawnX = roomX + (roomSize / 2);
            int spawnZ = roomZ + (roomSize / 2);
            _mazeData.SetSpawn(spawnX, spawnZ);
        }

        /// <summary>
        /// Carve a 4x4 exit room with an unlocked door to the maze.
        /// Player can easily exit through this door.
        /// </summary>
        private void CarveExitRoomWithDoor(int roomX, int roomZ, int roomSize)
        {
            // Step 1: Clear 4x4 area (make walkable)
            for (int x = roomX; x < roomX + roomSize; x++)
            for (int z = roomZ; z < roomZ + roomSize; z++)
            {
                if (_mazeData.InBounds(x, z))
                {
                    _mazeData.SetCell(x, z, 0);  // Clear all walls
                    _mazeData.MarkAsExitRoom(x, z);  // Mark as exit room cell
                }
            }

            // Step 2: Carve doorway on West edge of room (connects to maze)
            int doorX = roomX - 1;  // West edge
            int doorZ = roomZ + (roomSize / 2);  // Center of room
            if (_mazeData.InBounds(doorX, doorZ))
            {
                _mazeData.SetCell(doorX, doorZ, 0);  // Clear wall at door position
            }

            // Set exit point in center of exit room
            int exitX = roomX + (roomSize / 2);
            int exitZ = roomZ + (roomSize / 2);
            _mazeData.SetExit(exitX, exitZ);

            Debug.Log($"[DungeonGen] Exit room: {roomSize}x{roomSize} at ({roomX},{roomZ}) with door at ({doorX},{doorZ})");
        }

        // 
        // PHASE 3: DFS MAZE CARVING (creates winding passages)
        // 

        /// <summary>
        /// DFS maze carving that creates winding passages through solid block.
        /// Uses 8-direction carving for more organic maze structure.
        /// Respects entrance/exit rooms - won't carve into them.
        /// </summary>
        private void CarveMazeDFS(int startX, int startZ)
        {
            var stack = new Stack<(int x, int z)>();
            var visited = new bool[_mazeData.Width, _mazeData.Height];

            stack.Push((startX, startZ));
            visited[startX, startZ] = true;

            int cellsCarved = 0;
            // Carve 60-75% of maze to leave some walls intact for structure
            int maxCells = Mathf.RoundToInt(_mazeData.Width * _mazeData.Height * 0.65f);

            while (stack.Count > 0 && cellsCarved < maxCells)
            {
                var (cx, cz) = stack.Peek();
                var neighbors = GetUnvisitedNeighborsDFS(cx, cz, visited);

                if (neighbors.Count == 0)
                {
                    stack.Pop();
                }
                else
                {
                    var (nx, nz, dir) = neighbors[_rng.Next(neighbors.Count)];
                    CarvePassageDFS(cx, cz, nx, nz, dir);
                    visited[nx, nz] = true;
                    stack.Push((nx, nz));
                    cellsCarved++;
                }
            }

            Debug.Log($"[DungeonGen] DFS carved {cellsCarved} cells ({cellsCarved * 100 / (_mazeData.Width * _mazeData.Height)}% of maze)");
        }
        
        private List<(int x, int z, Direction8 dir)> GetUnvisitedNeighborsDFS(int x, int z, bool[,] visited)
        {
            var neighbors = new List<(int, int, Direction8)>();
            
            // 8 directions for more interesting maze
            Direction8[] directions = {
                Direction8.N, Direction8.S, Direction8.E, Direction8.W,
                Direction8.NE, Direction8.NW, Direction8.SE, Direction8.SW
            };
            
            foreach (var dir in directions)
            {
                var (dx, dz) = Direction8Helper.ToOffset(dir);
                // Move 2 cells for DFS (creates walls between passages)
                int nx = x + dx * 2;
                int nz = z + dz * 2;
                
                if (_mazeData.InBounds(nx, nz) && !visited[nx, nz])
                {
                    // Don't carve into entrance/exit rooms
                    if (!_mazeData.IsSpawnRoom(nx, nz) && !_mazeData.IsExitRoom(nx, nz))
                    {
                        neighbors.Add((nx, nz, dir));
                    }
                }
            }
            
            return neighbors;
        }
        
        private void CarvePassageDFS(int cx, int cz, int nx, int nz, Direction8 dir)
        {
            var (dx, dz) = Direction8Helper.ToOffset(dir);
            
            // Clear walls from current cell
            var curCell = _mazeData.GetCell(cx, cz);
            curCell &= ~Direction8Helper.ToWallFlag(dir);
            _mazeData.SetCell(cx, cz, curCell);
            
            // Clear walls from neighbor cell (opposite direction)
            var neiCell = _mazeData.GetCell(nx, nz);
            neiCell &= ~Direction8Helper.ToWallFlag(Direction8Helper.Opposite(dir));
            _mazeData.SetCell(nx, nz, neiCell);
            
            // For diagonal passages, clear intermediate cell
            if (Direction8Helper.IsDiagonal(dir))
            {
                int ix = cx + dx;
                int iz = cz + dz;
                if (_mazeData.InBounds(ix, iz))
                {
                    var intCell = _mazeData.GetCell(ix, iz);
                    intCell &= ~Direction8Helper.ToWallFlag(GetCardinalDir(dx, 0));
                    intCell &= ~Direction8Helper.ToWallFlag(GetCardinalDir(0, dz));
                    _mazeData.SetCell(ix, iz, intCell);
                }
            }
        }
        
        private static Direction8 GetCardinalDir(int dx, int dz)
        {
            if (dx > 0) return Direction8.E;
            if (dx < 0) return Direction8.W;
            if (dz > 0) return Direction8.N;
            if (dz < 0) return Direction8.S;
            return Direction8.N;
        }

        // 
        // PHASE 4: A* PATH GUARANTEE (carves maze-like paths)
        // 

        /// <summary>
        /// Use A* to find path from entrance to exit.
        /// If path exists, carve it wider. If no path exists, carve a winding maze path.
        /// Returns true if path already existed, false if we had to carve.
        /// </summary>
        private bool EnsurePathAStar()
        {
            var spawn = _mazeData.SpawnCell;
            var exit = _mazeData.ExitCell;

            // Run A* pathfinding through existing carved areas
            var path = FindPathAStar(spawn.x, spawn.z, exit.x, exit.z);

            if (path != null && path.Count > 0)
            {
                // Path exists - carve it wider for better walkability
                CarvePathWider(path);
                Debug.Log($"[DungeonGen] A*: Path found with {path.Count} cells - carved wider");
                return true;
            }

            // No path exists - carve a new winding maze path
            Debug.Log("[DungeonGen] A*: No path found - carving winding maze path");
            CarveMinimumConnection(spawn.x, spawn.z, exit.x, exit.z);
            return false;
        }

        /// <summary>
        /// Carve an existing path wider by clearing adjacent walls.
        /// </summary>
        private void CarvePathWider(List<(int x, int z)> path)
        {
            foreach (var (x, z) in path)
            {
                if (_mazeData.InBounds(x, z))
                {
                    var cell = _mazeData.GetCell(x, z);
                    cell &= ~CellFlags8.Wall_All;
                    _mazeData.SetCell(x, z, cell);
                }
            }
        }
        
        private List<(int x, int z)> FindPathAStar(int startX, int startZ, int goalX, int goalZ)
        {
            var openSet = new List<(int x, int z, int g, int h)>();
            var closedSet = new HashSet<(int, int)>();
            var cameFrom = new Dictionary<(int, int), (int, int)>();
            
            openSet.Add((startX, startZ, 0, ManhattanDistance(startX, startZ, goalX, goalZ)));
            
            while (openSet.Count > 0)
            {
                // Get node with lowest f = g + h
                openSet.Sort((a, b) => (a.g + a.h).CompareTo(b.g + b.h));
                var current = openSet[0];
                openSet.RemoveAt(0);
                closedSet.Add((current.x, current.z));
                
                // Check if reached goal
                if (current.x == goalX && current.z == goalZ)
                {
                    // Reconstruct path
                    return ReconstructPath(cameFrom, (goalX, goalZ));
                }
                
                // Check neighbors (4 cardinal directions)
                var neighbors = new[] {
                    (current.x + 1, current.z), (current.x - 1, current.z),
                    (current.x, current.z + 1), (current.x, current.z - 1)
                };
                
                foreach (var (nx, nz) in neighbors)
                {
                    if (!_mazeData.InBounds(nx, nz) || closedSet.Contains((nx, nz)))
                        continue;
                    
                    int newG = current.g + 10; // Cost to move
                    
                    var existing = openSet.Find(n => n.x == nx && n.z == nz);
                    if (existing.x == 0)
                    {
                        // New node
                        int h = ManhattanDistance(nx, nz, goalX, goalZ);
                        openSet.Add((nx, nz, newG, h));
                        cameFrom[(nx, nz)] = (current.x, current.z);
                    }
                    else if (newG < existing.g)
                    {
                        // Better path found
                        int idx = openSet.IndexOf(existing);
                        openSet[idx] = (nx, nz, newG, existing.h);
                        cameFrom[(nx, nz)] = (current.x, current.z);
                    }
                }
            }
            
            return null; // No path found
        }
        
        private int ManhattanDistance(int x1, int z1, int x2, int z2)
        {
            return Mathf.Abs(x2 - x1) + Mathf.Abs(z2 - z1);
        }
        
        private List<(int x, int z)> ReconstructPath(
            Dictionary<(int, int), (int, int)> cameFrom, 
            (int x, int z) current)
        {
            var path = new List<(int, int)>();
            while (cameFrom.ContainsKey(current))
            {
                path.Add(current);
                current = cameFrom[current];
            }
            path.Reverse();
            return path;
        }
        
        /// <summary>
        /// Carve a maze-like path from start to goal using A* waypoints.
        /// Creates winding corridors with turns and dead-ends, not a straight line.
        /// </summary>
        private void CarveMinimumConnection(int startX, int startZ, int goalX, int goalZ)
        {
            // Validate start position
            if (!_mazeData.InBounds(startX, startZ))
            {
                Debug.LogError($"[DungeonGen] CarveMazePath: Start position ({startX}, {startZ}) out of bounds");
                return;
            }

            // Use A* to find the path
            var path = FindPathAStar(startX, startZ, goalX, goalZ);
            
            if (path == null || path.Count == 0)
            {
                // Fallback: carve a winding path manually with random detours
                CarveWindingPath(startX, startZ, goalX, goalZ);
                return;
            }

            // Carve the A* path with some widening for variety
            foreach (var (x, z) in path)
            {
                if (_mazeData.InBounds(x, z))
                {
                    var cell = _mazeData.GetCell(x, z);
                    cell &= ~CellFlags8.Wall_All;
                    _mazeData.SetCell(x, z, cell);
                }
            }

            Debug.Log($"[DungeonGen] A*: Maze path carved with {path.Count} cells");
        }

        /// <summary>
        /// Carve a winding path with random detours and turns when A* fails.
        /// Creates a more interesting maze-like corridor.
        /// </summary>
        private void CarveWindingPath(int startX, int startZ, int goalX, int goalZ)
        {
            int x = startX;
            int z = startZ;
            int cellsCarved = 0;
            int maxIterations = Mathf.Abs(goalX - startX) + Mathf.Abs(goalZ - startZ) + 10;
            int iterations = 0;

            // Add random detours to make the path more maze-like
            while ((x != goalX || z != goalZ) && iterations < maxIterations)
            {
                iterations++;
                
                // 30% chance to take a detour (perpendicular move)
                if (_rng.NextDouble() < 0.3f && cellsCarved > 2)
                {
                    // Try to move perpendicular to main direction
                    int detourDx = 0, detourDz = 0;
                    if (x != goalX)
                    {
                        // Moving horizontally, detour vertically
                        detourDz = _rng.Next(0, 2) == 0 ? 1 : -1;
                    }
                    else
                    {
                        // Moving vertically, detour horizontally
                        detourDx = _rng.Next(0, 2) == 0 ? 1 : -1;
                    }

                    int nextX = x + detourDx;
                    int nextZ = z + detourDz;

                    if (_mazeData.InBounds(nextX, nextZ))
                    {
                        x = nextX;
                        z = nextZ;
                        CarvePathCell(x, z);
                        cellsCarved++;
                        continue;
                    }
                }

                // Move toward goal
                if (x != goalX)
                {
                    int nextX = x + (goalX > x ? 1 : -1);
                    if (_mazeData.InBounds(nextX, z))
                    {
                        x = nextX;
                        CarvePathCell(x, z);
                        cellsCarved++;
                    }
                    else
                    {
                        break; // Blocked
                    }
                }
                else if (z != goalZ)
                {
                    int nextZ = z + (goalZ > z ? 1 : -1);
                    if (_mazeData.InBounds(x, nextZ))
                    {
                        z = nextZ;
                        CarvePathCell(x, z);
                        cellsCarved++;
                    }
                    else
                    {
                        break; // Blocked
                    }
                }
            }

            Debug.Log($"[DungeonGen] Winding path carved: {cellsCarved} cells");
        }

        /// <summary>
        /// Carve a single cell in the path, clearing walls and ensuring connectivity.
        /// </summary>
        private void CarvePathCell(int x, int z)
        {
            if (!_mazeData.InBounds(x, z)) return;

            var cell = _mazeData.GetCell(x, z);
            cell &= ~CellFlags8.Wall_All;
            _mazeData.SetCell(x, z, cell);
        }

        // 
        // PHASE 4: Identify Chambers
        // 
        private void IdentifyDeadEndsAndCrossroads()
        {
            for (int x = 0; x < _mazeData.Width; x++)
            {
                for (int z = 0; z < _mazeData.Height; z++)
                {
                    int openCount = CountOpenCardinalDirections(x, z);

                    // Dead-end: exactly 1 open direction
                    if (openCount == 1)
                    {
                        _deadEnds.Add((x, z));
                    }
                    // Crossroad: 3+ open directions
                    else if (openCount >= 3)
                    {
                        _crossroads.Add((x, z));
                    }
                }
            }
        }

        private int CountOpenCardinalDirections(int x, int z)
        {
            var cell = _mazeData.GetCell(x, z);
            int count = 0;

            if ((cell & CellFlags8.Wall_N) == 0) count++;
            if ((cell & CellFlags8.Wall_S) == 0) count++;
            if ((cell & CellFlags8.Wall_E) == 0) count++;
            if ((cell & CellFlags8.Wall_W) == 0) count++;

            return count;
        }

        private void ExpandChambers(int expansionRadius)
        {
            // Expand dead-ends into small rooms
            foreach (var (dx, dz) in _deadEnds)
            {
                if (!_mazeData.IsSpawnRoom(dx, dz) && !_mazeData.IsExitRoom(dx, dz))
                {
                    ClearRoomAround(dx, dz, expansionRadius);
                    var cell = _mazeData.GetCell(dx, dz);
                    cell |= CellFlags8.IsRoom;
                    _mazeData.SetCell(dx, dz, cell);
                }
            }

            // Expand crossroads into larger meeting halls
            foreach (var (cx, cz) in _crossroads)
            {
                if (!_mazeData.IsSpawnRoom(cx, cz) && !_mazeData.IsExitRoom(cx, cz))
                {
                    ClearRoomAround(cx, cz, expansionRadius + 1);
                    var cell = _mazeData.GetCell(cx, cz);
                    cell |= CellFlags8.IsHall;
                    _mazeData.SetCell(cx, cz, cell);
                }
            }
        }

        // 
        // PHASE 5: Trap Rooms
        // 
        private void PlaceTrapRooms(float density)
        {
            int targetCount = Mathf.Max(1, Mathf.RoundToInt(_deadEnds.Count * density));
            
            for (int i = 0; i < targetCount && _deadEnds.Count > 0; i++)
            {
                int idx = _rng.Next(_deadEnds.Count);
                var (tx, tz) = _deadEnds[idx];
                _deadEnds.RemoveAt(idx);

                if (!_mazeData.IsSpawnRoom(tx, tz) && !_mazeData.IsExitRoom(tx, tz))
                {
                    var cell = _mazeData.GetCell(tx, tz);
                    cell |= CellFlags8.IsTrapRoom;
                    _mazeData.SetCell(tx, tz, cell);
                    _trapRooms.Add((tx, tz));
                }
            }
        }

        // 
        // PHASE 6: Treasure Rooms
        // 
        private void PlaceTreasureRooms(float density)
        {
            int targetCount = Mathf.Max(1, Mathf.RoundToInt(_deadEnds.Count * density));

            for (int i = 0; i < targetCount && _deadEnds.Count > 0; i++)
            {
                int idx = _rng.Next(_deadEnds.Count);
                var (tx, tz) = _deadEnds[idx];
                _deadEnds.RemoveAt(idx);

                if (!_mazeData.IsSpawnRoom(tx, tz) && !_mazeData.IsExitRoom(tx, tz))
                {
                    ClearRoomAround(tx, tz, 1);
                    var cell = _mazeData.GetCell(tx, tz);
                    cell |= CellFlags8.IsTreasureRoom;

                    // Place guardian enemy
                    if (_rng.NextDouble() < 0.7f)
                    {
                        cell |= CellFlags8.HasEnemy;
                    }

                    _mazeData.SetCell(tx, tz, cell);
                    _treasureRooms.Add((tx, tz));
                }
            }
        }

        // Variant that marks treasure rooms without clearing walls
        private void PlaceTreasureRoomsNoClear(float density)
        {
            int targetCount = Mathf.Max(1, Mathf.RoundToInt(_deadEnds.Count * density));

            for (int i = 0; i < targetCount && _deadEnds.Count > 0; i++)
            {
                int idx = _rng.Next(_deadEnds.Count);
                var (tx, tz) = _deadEnds[idx];
                _deadEnds.RemoveAt(idx);

                if (!_mazeData.IsSpawnRoom(tx, tz) && !_mazeData.IsExitRoom(tx, tz))
                {
                    var cell = _mazeData.GetCell(tx, tz);
                    cell |= CellFlags8.IsTreasureRoom;

                    // Place guardian enemy
                    if (_rng.NextDouble() < 0.7f)
                    {
                        cell |= CellFlags8.HasEnemy;
                    }

                    _mazeData.SetCell(tx, tz, cell);
                    _treasureRooms.Add((tx, tz));
                }
            }
        }

        // 
        // PHASE 7: Winding Corridors
        // 
        private void CarveLabyrinthinePaths(float windingFactor)
        {
            // Add secondary passages for complexity
            int pathCount = Mathf.RoundToInt(_crossroads.Count * windingFactor);

            for (int i = 0; i < pathCount; i++)
            {
                int x = _rng.Next(1, _mazeData.Width - 1);
                int z = _rng.Next(1, _mazeData.Height - 1);

                // Carve short winding segments
                for (int step = 0; step < 4; step++)
                {
                    if (!_mazeData.InBounds(x, z)) break;

                    var cell = _mazeData.GetCell(x, z);
                    if (cell != 0)
                    {
                        Direction8[] dirs = { Direction8.N, Direction8.S, Direction8.E, Direction8.W };
                        var dir = dirs[_rng.Next(dirs.Length)];
                        var (dx, dz) = Direction8Helper.ToOffset(dir);
                        x += dx;
                        z += dz;
                    }
                }
            }
        }

        // 
        // PHASE 8: AI Difficulty (already calculated)
        // 

        // 
        // PHASE 9: Guarantee Path to Exit
        // 
        // ReSharper disable Unity.PerformanceAnalysis
        private void EnsurePathToExit()
        {
            // Try to find existing path
            var path = FindPathToExit();
            if (path != null)
            {
                foreach (var (x, z) in path)
                {
                    _mazeData.MarkAsMainPath(x, z);
                }
                Debug.Log($"[DungeonGen] EnsurePathToExit: Found existing path with {path.Count} cells");
            }
            else
            {
                // No path exists - carve one from exit back towards spawn
                Debug.Log("[DungeonGen] EnsurePathToExit: No path found, carving guaranteed path...");
                CarveGuaranteedPathToExit();
            }
        }

        private void CarveGuaranteedPathToExit()
        {
            // Start from exit and carve towards spawn using simple greedy algorithm
            var spawn = _mazeData.SpawnCell;
            var exit = _mazeData.ExitCell;
            
            int cx = exit.x;
            int cz = exit.z;
            int pathLength = 0;

            // Clear exit room edge first
            _mazeData.SetCell(cx, cz, 0);
            ClearWallsAroundCell(cx, cz);

            while (cx != spawn.x || cz != spawn.z)
            {
                // Move towards spawn
                if (cx < spawn.x)
                {
                    // Move east - clear this cell completely
                    cx++;
                    _mazeData.SetCell(cx, cz, 0);
                    ClearWallsAroundCell(cx, cz);
                    pathLength++;
                }
                else if (cx > spawn.x)
                {
                    // Move west - clear this cell completely
                    cx--;
                    _mazeData.SetCell(cx, cz, 0);
                    ClearWallsAroundCell(cx, cz);
                    pathLength++;
                }
                else if (cz < spawn.z)
                {
                    // Move north - clear this cell completely
                    cz++;
                    _mazeData.SetCell(cx, cz, 0);
                    ClearWallsAroundCell(cx, cz);
                    pathLength++;
                }
                else if (cz > spawn.z)
                {
                    // Move south - clear this cell completely
                    cz--;
                    _mazeData.SetCell(cx, cz, 0);
                    ClearWallsAroundCell(cx, cz);
                    pathLength++;
                }
            }
            
            // Clear spawn room edge
            _mazeData.SetCell(spawn.x, spawn.z, 0);
            ClearWallsAroundCell(spawn.x, spawn.z);
            
            Debug.Log($"[DungeonGen] Carved guaranteed path: {pathLength} cells from exit to spawn");
        }

        // Clear all wall flags from a cell and its 4 cardinal neighbors
        // This ensures no walls block the path
        private void ClearWallsAroundCell(int x, int z)
        {
            // Clear walls on this cell pointing outward
            var cell = _mazeData.GetCell(x, z);
            cell &= ~CellFlags8.Wall_N;
            cell &= ~CellFlags8.Wall_S;
            cell &= ~CellFlags8.Wall_E;
            cell &= ~CellFlags8.Wall_W;
            _mazeData.SetCell(x, z, cell);

            // Clear walls on neighbors pointing into this cell
            if (_mazeData.InBounds(x, z + 1))
            {
                var north = _mazeData.GetCell(x, z + 1);
                north &= ~CellFlags8.Wall_S;
                _mazeData.SetCell(x, z + 1, north);
            }
            if (_mazeData.InBounds(x, z - 1))
            {
                var south = _mazeData.GetCell(x, z - 1);
                south &= ~CellFlags8.Wall_N;
                _mazeData.SetCell(x, z - 1, south);
            }
            if (_mazeData.InBounds(x + 1, z))
            {
                var east = _mazeData.GetCell(x + 1, z);
                east &= ~CellFlags8.Wall_W;
                _mazeData.SetCell(x + 1, z, east);
            }
            if (_mazeData.InBounds(x - 1, z))
            {
                var west = _mazeData.GetCell(x - 1, z);
                west &= ~CellFlags8.Wall_E;
                _mazeData.SetCell(x - 1, z, west);
            }
        }

        private List<(int, int)> FindPathToExit()
        {
            // Simplified pathfinding - ensure connectivity
            var spawn = _mazeData.SpawnCell;
            var exit = _mazeData.ExitCell;

            var visited = new HashSet<(int, int)>();
            var queue = new Queue<(int, int, List<(int, int)>)>();
            queue.Enqueue((spawn.x, spawn.z, new List<(int, int)> { (spawn.x, spawn.z) }));

            while (queue.Count > 0)
            {
                var (x, z, path) = queue.Dequeue();

                if (x == exit.x && z == exit.z)
                {
                    return path;
                }

                if (visited.Contains((x, z)))
                    continue;

                visited.Add((x, z));

                // Check cardinal neighbors
                Direction8[] cardinals = { Direction8.N, Direction8.S, Direction8.E, Direction8.W };
                foreach (var dir in cardinals)
                {
                    if (_mazeData.HasWall(x, z, dir))
                        continue;

                    var (dx, dz) = Direction8Helper.ToOffset(dir);
                    int nx = x + dx;
                    int nz = z + dz;

                    if (_mazeData.InBounds(nx, nz) && !visited.Contains((nx, nz)))
                    {
                        var newPath = new List<(int, int)>(path) { (nx, nz) };
                        queue.Enqueue((nx, nz, newPath));
                    }
                }
            }

            return null;
        }

        // 
        // PHASE 10: Object Placement
        // 
        private void PlaceTorches(float chance)
        {
            for (int x = 0; x < _mazeData.Width; x++)
            {
                for (int z = 0; z < _mazeData.Height; z++)
                {
                    if (_rng.NextDouble() < chance)
                    {
                        var cell = _mazeData.GetCell(x, z);
                        cell |= CellFlags8.HasTorch;
                        _mazeData.SetCell(x, z, cell);
                    }
                }
            }
        }

        private void PlaceEnemies(float density)
        {
            int count = Mathf.RoundToInt(_mazeData.Width * _mazeData.Height * density * _mazeData.DifficultyFactor);

            for (int i = 0; i < count; i++)
            {
                int x = _rng.Next(_mazeData.Width);
                int z = _rng.Next(_mazeData.Height);

                if (!_mazeData.IsSpawnRoom(x, z))
                {
                    var cell = _mazeData.GetCell(x, z);
                    cell |= CellFlags8.HasEnemy;
                    _mazeData.SetCell(x, z, cell);
                }
            }
        }

        private void PlaceChests(float density)
        {
            int count = Mathf.RoundToInt(_mazeData.Width * _mazeData.Height * density);

            for (int i = 0; i < count; i++)
            {
                int x = _rng.Next(_mazeData.Width);
                int z = _rng.Next(_mazeData.Height);

                if (!_mazeData.IsSpawnRoom(x, z))
                {
                    var cell = _mazeData.GetCell(x, z);
                    cell |= CellFlags8.HasChest;
                    _mazeData.SetCell(x, z, cell);
                }
            }
        }

        // 
        // Validation
        // 
        private void ValidateConfig(DungeonMazeConfig cfg)
        {
            if (cfg == null)
                throw new ArgumentNullException(nameof(cfg));

            if (cfg.TrapDensity < 0 || cfg.TrapDensity > 1)
                throw new ArgumentException("TrapDensity must be 0-1");

            if (cfg.TreasureDensity < 0 || cfg.TreasureDensity > 1)
                throw new ArgumentException("TreasureDensity must be 0-1");
        }
    }
}
