// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8 | Locale: en_US

// ─────────────────────────────────────────────────────────────────────────────
// [DEPRECATED - 2026-03-09]
// This file is LEGACY and should be archived or removed.
//
// REPLACED BY: GridMazeGenerator.cs (Cardinal-only DFS + A* algorithm)
// REASON: 8-axis system replaced with cardinal-only (4-direction) passages
//         for cleaner wall alignment and better maze structure.
//
// This file uses 8-direction DFS which creates diagonal passages that cause:
// - Wall alignment issues (diagonal gaps)
// - Complex collision detection
// - Inconsistent corridor geometry
//
// DO NOT USE in new code. Migrate to GridMazeGenerator.cs instead.
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.Collections.Generic;
using System.Linq;

namespace Code.Lavos.Core.Advanced
{
    /// <summary>
    /// [DEPRECATED] Use GridMazeGenerator.cs instead
    ///
    /// MazeMathEngine_8Axis - Pure mathematics for 8-axis procedural maze generation
    /// Generates mazes from Start A to Exit B with random corridors and dead-ends
    /// All calculations are grid-based (x, z) coordinates
    /// Handles wall prefab placement on 8-axis system (N, S, E, W, NE, NW, SE, SW)
    ///
    /// Features:
    /// - Procedural generation with random seed
    /// - Guaranteed path from Start to Exit
    /// - Random dead-ends and crossroads
    /// - 8-axis wall direction calculations
    /// - Grid-based cell mathematics
    /// - No Unity dependencies - pure C# math
    /// </summary>
    public class MazeMathEngine_8Axis
    {
        private Random _rng;
        private int _width;
        private int _height;
        private bool[,] _visited;
        private byte[,] _maze;
        private List<(int x, int z)> _deadEnds;
        private List<(int x, int z)> _corridors;
        private (int x, int z) _startPoint;
        private (int x, int z) _exitPoint;

        // ─────────────────────────────────────────────────────────────────────
        // CONSTANTS & CELL TYPE DEFINITIONS
        // ─────────────────────────────────────────────────────────────────────

        public const byte CELL_WALL = 0xFF;      // All walls present
        public const byte CELL_PASSAGE = 0x00;   // No walls (carvedthrough)
        public const byte CELL_UNVISITED = 0xAA; // Not yet processed

        // 8-Axis Direction offsets (x, z) - Z is vertical in grid
        private static readonly Dictionary<int, (int dx, int dz)> Direction8Offsets = new()
        {
            { 0, ( 0,  1) },  // N  - North (up)
            { 1, ( 0, -1) },  // S  - South (down)
            { 2, ( 1,  0) },  // E  - East (right)
            { 3, (-1,  0) },  // W  - West (left)
            { 4, ( 1,  1) },  // NE - Northeast
            { 5, (-1,  1) },  // NW - Northwest
            { 6, ( 1, -1) },  // SE - Southeast
            { 7, (-1, -1) },  // SW - Southwest
        };

        // Wall flags for each direction (for cell wall tracking)
        private static readonly Dictionary<int, byte> Direction8WallFlags = new()
        {
            { 0, 0x01 },  // N  - Wall North
            { 1, 0x02 },  // S  - Wall South
            { 2, 0x04 },  // E  - Wall East
            { 3, 0x08 },  // W  - Wall West
            { 4, 0x10 },  // NE - Wall NorthEast diagonal
            { 5, 0x20 },  // NW - Wall NorthWest diagonal
            { 6, 0x40 },  // SE - Wall SouthEast diagonal
            { 7, 0x80 },  // SW - Wall SouthWest diagonal
        };

        // ─────────────────────────────────────────────────────────────────────
        // INITIALIZATION
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Initialize maze generator with dimensions and random seed
        /// </summary>
        public MazeMathEngine_8Axis(int width, int height, int randomSeed = -1)
        {
            _width = width;
            _height = height;
            _rng = randomSeed < 0 ? new Random() : new Random(randomSeed);
            _maze = new byte[width, height];
            _visited = new bool[width, height];
            _deadEnds = new List<(int x, int z)>();
            _corridors = new List<(int x, int z)>();

            // Initialize all cells as unvisited walls
            for (int x = 0; x < width; x++)
                for (int z = 0; z < height; z++)
                    _maze[x, z] = CELL_WALL;
        }

        // ─────────────────────────────────────────────────────────────────────
        // PUBLIC API - MAZE GENERATION
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Generate complete maze from Start A to Exit B
        /// Returns maze data with guaranteed path
        /// </summary>
        public MazeGenerationResult GenerateMaze(int depthFirstIterations = -1)
        {
            if (depthFirstIterations < 0)
                depthFirstIterations = (_width * _height) / 2;

            // Step 1: Establish Start (A) and Exit (B) points
            _startPoint = FindOptimalStartPoint();
            _exitPoint = FindOptimalExitPoint();

            // Step 2: Carve passages using depth-first search (8-axis)
            CarvePassages8DepthFirst(_startPoint.x, _startPoint.z);

            // Step 3: Identify dead-ends and create crossroads
            IdentifyDeadEndsAndCrossroads();

            // Step 4: Ensure guaranteed path from Start to Exit
            EnsurePathStartToExit();

            // Step 5: Add random dead-ends for maze complexity
            AddRandomDeadEnds();

            // Step 6: Calculate corridors (all carved passages)
            CalculateCorridorList();

            return new MazeGenerationResult
            {
                Width = _width,
                Height = _height,
                MazeGrid = _maze,
                StartPoint = _startPoint,
                ExitPoint = _exitPoint,
                DeadEnds = _deadEnds.ToList(),
                Corridors = _corridors.ToList(),
                TotalCells = _width * _height,
                CarvedCells = CountCarvedCells(),
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // CORE ALGORITHMS - DEPTH-FIRST CARVING
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Depth-first search with 8-axis carving
        /// Creates main corridor structure
        /// </summary>
        private void CarvePassages8DepthFirst(int startX, int startZ)
        {
            var stack = new Stack<(int x, int z)>();
            stack.Push((startX, startZ));
            _visited[startX, startZ] = true;
            _maze[startX, startZ] = CELL_PASSAGE;

            while (stack.Count > 0)
            {
                var current = stack.Peek();
                var unvisited = GetUnvisitedNeighbors8(current.x, current.z);

                if (unvisited.Count > 0)
                {
                    // Randomly choose direction (8-axis)
                    int dirIndex = _rng.Next(unvisited.Count);
                    int direction = unvisited[dirIndex];

                    var (dx, dz) = Direction8Offsets[direction];
                    int nx = current.x + dx;
                    int nz = current.z + dz;

                    // Carve passage between current and neighbor
                    CarveWallBetween(current.x, current.z, nx, nz, direction);

                    _visited[nx, nz] = true;
                    _maze[nx, nz] = CELL_PASSAGE;
                    stack.Push((nx, nz));
                }
                else
                {
                    stack.Pop();
                }
            }
        }

        /// <summary>
        /// Get all unvisited neighbors in 8 directions
        /// </summary>
        private List<int> GetUnvisitedNeighbors8(int x, int z)
        {
            var unvisited = new List<int>();

            for (int dir = 0; dir < 8; dir++)
            {
                var (dx, dz) = Direction8Offsets[dir];
                int nx = x + dx;
                int nz = z + dz;

                if (IsInBounds(nx, nz) && !_visited[nx, nz])
                {
                    unvisited.Add(dir);
                }
            }

            return unvisited;
        }

        /// <summary>
        /// Carve wall between two adjacent cells in given direction
        /// </summary>
        private void CarveWallBetween(int x1, int z1, int x2, int z2, int direction)
        {
            // Remove wall flag from current cell
            _maze[x1, z1] &= (byte)~Direction8WallFlags[direction];

            // Remove opposite wall flag from neighbor
            int oppositeDir = GetOppositeDirection(direction);
            _maze[x2, z2] &= (byte)~Direction8WallFlags[oppositeDir];
        }

        // ─────────────────────────────────────────────────────────────────────
        // PATHFINDING & GUARANTEES
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Ensure guaranteed path from Start A to Exit B using A* pathfinding
        /// </summary>
        private void EnsurePathStartToExit()
        {
            var path = FindPath8(_startPoint, _exitPoint);

            if (path == null || path.Count == 0)
            {
                // No path found - carve direct corridor
                CarveDirectPath(_startPoint, _exitPoint);
            }
        }

        /// <summary>
        /// A* pathfinding in 8-axis maze
        /// </summary>
        private List<(int x, int z)> FindPath8((int x, int z) start, (int x, int z) goal)
        {
            var openSet = new HashSet<(int x, int z)> { start };
            var cameFrom = new Dictionary<(int x, int z), (int x, int z)>();
            var gScore = new Dictionary<(int x, int z), float> { [start] = 0 };
            var fScore = new Dictionary<(int x, int z), float> { [start] = Heuristic8(start, goal) };

            while (openSet.Count > 0)
            {
                var current = openSet.OrderBy(pos => fScore.GetValueOrDefault(pos, float.MaxValue)).First();

                if (current == goal)
                    return ReconstructPath(cameFrom, current);

                openSet.Remove(current);

                foreach (int dir in Enumerable.Range(0, 8))
                {
                    var (dx, dz) = Direction8Offsets[dir];
                    var neighbor = (x: current.x + dx, z: current.z + dz);

                    if (!IsInBounds(neighbor.x, neighbor.z) || !IsCarved(neighbor.x, neighbor.z))
                        continue;

                    float tentativeGScore = gScore[current] + Distance8(dir);

                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = gScore[neighbor] + Heuristic8(neighbor, goal);

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            return null; // No path found
        }

        /// <summary>
        /// Carve direct path between two points (fallback)
        /// </summary>
        private void CarveDirectPath((int x, int z) from, (int x, int z) to)
        {
            int x = from.x;
            int z = from.z;

            while (x != to.x || z != to.z)
            {
                if (x < to.x) { x++; }
                else if (x > to.x) { x--; }
                else if (z < to.z) { z++; }
                else if (z > to.z) { z--; }

                if (IsInBounds(x, z))
                    _maze[x, z] = CELL_PASSAGE;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // ANALYSIS & FEATURES
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Identify dead-ends and crossroads
        /// </summary>
        private void IdentifyDeadEndsAndCrossroads()
        {
            for (int x = 1; x < _width - 1; x++)
            {
                for (int z = 1; z < _height - 1; z++)
                {
                    if (!IsCarved(x, z)) continue;

                    int openDirs = CountOpenDirections8(x, z);

                    if (openDirs == 1)
                        _deadEnds.Add((x, z));  // Dead-end (only 1 exit)
                }
            }
        }

        /// <summary>
        /// Add random dead-ends for additional maze complexity
        /// </summary>
        private void AddRandomDeadEnds()
        {
            int targetDeadEnds = Math.Max(5, (_width * _height) / 20);

            while (_deadEnds.Count < targetDeadEnds)
            {
                int x = _rng.Next(1, _width - 1);
                int z = _rng.Next(1, _height - 1);

                if (IsCarved(x, z) && !_deadEnds.Contains((x, z)))
                {
                    // Try to carve a branch from this cell
                    for (int dir = 0; dir < 8; dir++)
                    {
                        var (dx, dz) = Direction8Offsets[dir];
                        int nx = x + dx;
                        int nz = z + dz;

                        if (IsInBounds(nx, nz) && !IsCarved(nx, nz))
                        {
                            _maze[nx, nz] = CELL_PASSAGE;
                            _deadEnds.Add((nx, nz));
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculate all corridor cells (carved passages)
        /// </summary>
        private void CalculateCorridorList()
        {
            _corridors.Clear();
            for (int x = 0; x < _width; x++)
                for (int z = 0; z < _height; z++)
                    if (IsCarved(x, z))
                        _corridors.Add((x, z));
        }

        // ─────────────────────────────────────────────────────────────────────
        // UTILITY MATH FUNCTIONS
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Heuristic for A* - Chebyshev distance (8-axis)
        /// </summary>
        private float Heuristic8((int x, int z) a, (int x, int z) b)
        {
            return Math.Max(Math.Abs(a.x - b.x), Math.Abs(a.z - b.z));
        }

        /// <summary>
        /// Distance for A* g-score calculation
        /// Cardinal = 1.0, Diagonal = 1.41
        /// </summary>
        private float Distance8(int direction)
        {
            return direction < 4 ? 1.0f : 1.41f; // Cardinal vs diagonal
        }

        /// <summary>
        /// Get opposite direction (8-axis)
        /// </summary>
        private int GetOppositeDirection(int dir) => dir switch
        {
            0 => 1,  // N <-> S
            1 => 0,
            2 => 3,  // E <-> W
            3 => 2,
            4 => 7,  // NE <-> SW
            5 => 6,  // NW <-> SE
            6 => 5,
            7 => 4,
            _ => 0
        };

        /// <summary>
        /// Count open connections in 8 directions
        /// </summary>
        private int CountOpenDirections8(int x, int z)
        {
            int count = 0;
            for (int dir = 0; dir < 8; dir++)
            {
                var (dx, dz) = Direction8Offsets[dir];
                if (IsCarved(x + dx, z + dz))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Count carved cells in maze
        /// </summary>
        private int CountCarvedCells()
        {
            int count = 0;
            for (int x = 0; x < _width; x++)
                for (int z = 0; z < _height; z++)
                    if (IsCarved(x, z))
                        count++;
            return count;
        }

        /// <summary>
        /// Find optimal start point (Top-left area)
        /// </summary>
        private (int x, int z) FindOptimalStartPoint()
        {
            return (2, 2);
        }

        /// <summary>
        /// Find optimal exit point (Bottom-right area)
        /// </summary>
        private (int x, int z) FindOptimalExitPoint()
        {
            return (_width - 3, _height - 3);
        }

        /// <summary>
        /// Check if cell is carved (passage, not wall)
        /// </summary>
        private bool IsCarved(int x, int z)
        {
            return IsInBounds(x, z) && _maze[x, z] != CELL_WALL;
        }

        /// <summary>
        /// Check if coordinates are within bounds
        /// </summary>
        private bool IsInBounds(int x, int z)
        {
            return x >= 0 && x < _width && z >= 0 && z < _height;
        }

        /// <summary>
        /// Reconstruct path from A* pathfinding
        /// </summary>
        private List<(int x, int z)> ReconstructPath(Dictionary<(int x, int z), (int x, int z)> cameFrom, (int x, int z) current)
        {
            var path = new List<(int x, int z)> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }
            return path;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // OUTPUT RESULT STRUCTURE
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Complete maze generation result with all computed data
    /// </summary>
    public class MazeGenerationResult
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[,] MazeGrid { get; set; }
        public (int x, int z) StartPoint { get; set; }
        public (int x, int z) ExitPoint { get; set; }
        public List<(int x, int z)> DeadEnds { get; set; }
        public List<(int x, int z)> Corridors { get; set; }
        public int TotalCells { get; set; }
        public int CarvedCells { get; set; }

        /// <summary>
        /// Calculate maze complexity factor (carved cells / total cells)
        /// </summary>
        public float ComplexityFactor => (float)CarvedCells / TotalCells;

        /// <summary>
        /// Get wall configuration for a specific cell in 8 directions
        /// Returns array of 8 bools: [N, S, E, W, NE, NW, SE, SW]
        /// </summary>
        public bool[] GetWallConfiguration(int x, int z)
        {
            var walls = new bool[8];
            byte cellValue = MazeGrid[x, z];

            for (int dir = 0; dir < 8; dir++)
            {
                walls[dir] = (cellValue & (1 << dir)) != 0;
            }

            return walls;
        }

        /// <summary>
        /// Get wall prefab positions for a grid cell
        /// Returns dictionary mapping direction → position offset
        /// </summary>
        public Dictionary<string, (int dx, int dz)> GetWallPrefabPositions()
        {
            return new Dictionary<string, (int dx, int dz)>
            {
                { "N",  ( 0,  1) },
                { "S",  ( 0, -1) },
                { "E",  ( 1,  0) },
                { "W",  (-1,  0) },
                { "NE", ( 1,  1) },
                { "NW", (-1,  1) },
                { "SE", ( 1, -1) },
                { "SW", (-1, -1) },
            };
        }
    }
}
