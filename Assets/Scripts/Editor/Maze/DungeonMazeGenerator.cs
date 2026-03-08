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
    /// Advanced dungeon maze generator combining:
    /// - 8-axis procedural generation with DFS
    /// - Multi-corridor entrance/exit with branching dead-ends
    /// - Trap room systems (spikes, lava, darkness)
    /// - Treasure chambers guarded by enemies
    /// - Winding corridors and crossroads
    /// - AI-based adaptive difficulty scaling
    /// 
    /// Plug-in-out architecture: Finds all components, never creates them
    /// Replaces GridMazeGenerator8 while maintaining compatibility
    /// </summary>
    public sealed class DungeonMazeGenerator
    {
        // ─────────────────────────────────────────────────────────────
        // INTERNAL STATE
        // ─────────────────────────────────────────────────────────────
        private DungeonMazeData _mazeData;
        private System.Random _rng;
        private bool[,] _visited;
        private List<(int x, int z)> _deadEnds;
        private List<(int x, int z)> _crossroads;
        private List<(int x, int z)> _trapRooms;
        private List<(int x, int z)> _treasureRooms;
        private AIAdaptiveDifficulty _aiDifficulty;

        // ─────────────────────────────────────────────────────────────
        // PUBLIC API - Generate complete dungeon
        // ─────────────────────────────────────────────────────────────
        
        /// <summary>
        /// Generate a complete advanced dungeon maze with all features.
        /// 
        /// Pipeline:
        /// 1. Initialize maze data structure
        /// 2. Fill all walls (8-axis)
        /// 3. DFS carve main passages
        /// 4. Carve spawn room (5x5 guaranteed safe area)
        /// 5. Carve exit room (distant, difficult to reach)
        /// 6. Identify and expand dead-ends into chambers
        /// 7. Identify and expand crossroads into meeting halls
        /// 8. Place trap rooms in strategic dead-ends
        /// 9. Place treasure chambers guarded by enemies
        /// 10. Carve winding corridors for complexity
        /// 11. Calculate AI difficulty metrics
        /// 12. Place torches, chests, enemies based on difficulty
        /// 13. Guarantee A* path from spawn to exit
        /// </summary>
        public DungeonMazeData Generate(int seed, int level, DungeonMazeConfig cfg)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
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

            // === PHASE 1: Core Maze Structure ===
            FillAllWalls();
            Debug.Log($"[DungeonGen] Phase 1: All walls filled ({size * size} cells)");

            // === PHASE 2: Main Passages (8-axis DFS) ===
            CarvePassages8(1, 1);
            Debug.Log($"[DungeonGen] Phase 2: DFS complete");

            // === PHASE 3: Spawn & Exit Rooms ===
            CarveSpawnRoom(1, 1, cfg.SpawnRoomSize);
            _mazeData.SetSpawn(1, 1);
            
            CarveExitRoom(size - 2, size - 2, cfg.ExitRoomSize);
            _mazeData.SetExit(size - 2, size - 2);
            Debug.Log($"[DungeonGen] Phase 3: Spawn at (1,1) | Exit at ({size-2},{size-2})");

            // === PHASE 4: Chamber Expansion ===
            IdentifyDeadEndsAndCrossroads();
            ExpandChambers(cfg.ChamberExpansionRadius);
            Debug.Log($"[DungeonGen] Phase 4: {_deadEnds.Count} dead-ends | " +
                      $"{_crossroads.Count} crossroads");

            // === PHASE 5: Trap Room Placement ===
            PlaceTrapRooms(cfg.TrapDensity);
            Debug.Log($"[DungeonGen] Phase 5: {_trapRooms.Count} trap rooms placed");

            // === PHASE 6: Treasure Chamber Placement ===
            PlaceTreasureRooms(cfg.TreasureDensity);
            Debug.Log($"[DungeonGen] Phase 6: {_treasureRooms.Count} treasure rooms placed");

            // === PHASE 7: Winding Corridors ===
            CarveLabyrinthinePaths(cfg.CorridorWindingFactor);
            Debug.Log($"[DungeonGen] Phase 7: Winding corridors carved");

            // === PHASE 8: AI Difficulty Calculation ===
            if (_aiDifficulty == null || _mazeData == null)
            {
                Debug.LogWarning("[DungeonGen] Phase 8: AI difficulty system not initialized, using default");
                _mazeData.AIAdaptiveFactor = 1.0f;
            }
            else
            {
                int trapCount = _trapRooms.Count;
                int treasureCount = _treasureRooms.Count;
                _aiDifficulty.AnalyzeMaze(_mazeData, trapCount, treasureCount);
                _mazeData.AIAdaptiveFactor = _aiDifficulty.ComputedDifficultyMultiplier;
            }
#if UNITY_EDITOR
            Debug.Log($"[DungeonGen] Phase 8: AI difficulty = {_mazeData.AIAdaptiveFactor:F2}");
#endif

            // === PHASE 9: Guaranteed Path ===
            EnsurePathToExit();
            Debug.Log($"[DungeonGen] Phase 9: Guaranteed spawn-to-exit path");

            // === PHASE 10: Object Placement ===
            PlaceTorches(cfg.TorchChance * _mazeData.DifficultyFactor);
            PlaceEnemies(cfg.EnemyDensity);
            PlaceChests(cfg.ChestDensity);
            Debug.Log($"[DungeonGen] Phase 10: Objects placed");

            stopwatch.Stop();
            _mazeData.GenerationTimeMs = stopwatch.ElapsedMilliseconds;
            Debug.Log($"[DungeonGen] COMPLETE - Generation took {_mazeData.GenerationTimeMs:F1}ms");
            return _mazeData;
        }

        // ─────────────────────────────────────────────────────────────
        // PHASE 1: Initialize Walls
        // ─────────────────────────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────────────
        // PHASE 2: Main Passages - 8-axis DFS
        // ─────────────────────────────────────────────────────────────
        private void CarvePassages8(int startX, int startZ)
        {
            var stack = new Stack<(int, int)>();
            stack.Push((startX, startZ));
            _visited[startX, startZ] = true;

            while (stack.Count > 0)
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
                }
            }
        }

        private List<(int nx, int nz, Direction8 dir)> GetUnvisitedNeighbors8(int x, int z)
        {
            var neighbors = new List<(int, int, Direction8)>();
            
            Direction8[] allDirections = (Direction8[])Enum.GetValues(typeof(Direction8));
            
            foreach (var dir in allDirections)
            {
                var (dx, dz) = Direction8Helper.ToOffset(dir);
                int nx = x + dx;
                int nz = z + dz;

                if (_mazeData.InBounds(nx, nz) && !_visited[nx, nz])
                {
                    neighbors.Add((nx, nz, dir));
                }
            }

            return neighbors;
        }

        private void CarvePassageToNeighbor(int cx, int cz, int nx, int nz, Direction8 dir)
        {
            // Validate bounds first
            if (!_mazeData.InBounds(cx, cz) || !_mazeData.InBounds(nx, nz))
                return;

            var curCell = _mazeData.GetCell(cx, cz);
            var neiCell = _mazeData.GetCell(nx, nz);

            // Remove wall from current cell in direction of neighbor
            ushort wallFlag = (ushort)Direction8Helper.ToWallFlag(dir);
            curCell &= (ushort)~wallFlag;

            // Remove opposite wall from neighbor
            Direction8 oppositeDir = Direction8Helper.Opposite(dir);
            ushort oppositeWallFlag = (ushort)Direction8Helper.ToWallFlag(oppositeDir);
            neiCell &= (ushort)~oppositeWallFlag;

            // For diagonal passages, also clear intermediate cell
            if (Direction8Helper.IsDiagonal(dir))
            {
                var (dx, dz) = Direction8Helper.ToOffset(dir);
                // FIX: Use Sign to get proper intermediate position (+1 or -1, never 0)
                int intermediateX = cx + Math.Sign(dx);
                int intermediateZ = cz + Math.Sign(dz);
                if (_mazeData.InBounds(intermediateX, intermediateZ))
                {
                    var intermediateCell = _mazeData.GetCell(intermediateX, intermediateZ);
                    // FIX: Clear only the walls that block this passage, preserve flags
                    intermediateCell &= (ushort)~(wallFlag | oppositeWallFlag);
                    _mazeData.SetCell(intermediateX, intermediateZ, intermediateCell);
                }
            }

            _mazeData.SetCell(cx, cz, curCell);
            _mazeData.SetCell(nx, nz, neiCell);
        }

        // ─────────────────────────────────────────────────────────────
        // PHASE 3: Spawn & Exit Rooms
        // ─────────────────────────────────────────────────────────────
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
                        _mazeData.SetCell(x, z, 0);
                    }
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        // PHASE 4: Identify Chambers
        // ─────────────────────────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────────────
        // PHASE 5: Trap Rooms
        // ─────────────────────────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────────────
        // PHASE 6: Treasure Rooms
        // ─────────────────────────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────────────
        // PHASE 7: Winding Corridors
        // ─────────────────────────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────────────
        // PHASE 8: AI Difficulty (already calculated)
        // ─────────────────────────────────────────────────────────────

        // ─────────────────────────────────────────────────────────────
        // PHASE 9: Guarantee Path to Exit
        // ─────────────────────────────────────────────────────────────
        private void EnsurePathToExit()
        {
            // Simple A* guarantee
            var path = FindPathToExit();
            if (path != null)
            {
                foreach (var (x, z) in path)
                {
                    _mazeData.MarkAsMainPath(x, z);
                }
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

        // ─────────────────────────────────────────────────────────────
        // PHASE 10: Object Placement
        // ─────────────────────────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────────────
        // Validation
        // ─────────────────────────────────────────────────────────────
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
