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
// PathFinder.cs
// PATHFINDING UTILITIES - A* algorithm for corridor routing
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// PLUG-IN-OUT: Independent utility class (no dependencies)
// - A* pathfinding algorithm
// - Seed-based randomness for procedural variation
// - Reusable for AI, corridor generation, etc.
// - All values from JSON config
//
// USAGE:
//   PathFinder.FindPath(grid, start, end, randomness)
//   PathFinder.FindAllPaths(grid, points, seed)
//   PathFinder.ValidateConnectivity(grid)
//
// Location: Assets/Scripts/Core/11_Utilities/

using UnityEngine;
using System.Collections.Generic;

namespace Code.Lavos.Core
{
    /// <summary>
    /// PathFinder - A* pathfinding for maze corridors.
    /// Static utility class with seed-based randomness.
    ///
    /// FEATURES:
    /// - A* algorithm with heuristic optimization
    /// - Randomized costs for procedural variation
    /// - Supports 4-directional movement (N, S, E, W)
    /// - Reusable for AI pathfinding later
    ///
    /// PLUG-IN-OUT: No Unity dependencies, pure C# logic.
    /// </summary>
    public static class PathFinder
    {
        #region Public API

        /// <summary>
        /// Find optimal path between two points with randomness.
        /// </summary>
        /// <param name="grid">Grid maze cell data</param>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <param name="randomness">0.0=straight, 1.0=chaotic (from JSON config)</param>
        /// <returns>List of cells forming the path</returns>
        public static List<Vector2Int> FindPath(GridMazeCell[,] grid, Vector2Int start, Vector2Int end, float randomness = 0.3f)
        {
            int gridSize = grid.GetLength(0);

            // Validate input
            if (!IsValidCell(grid, start) || !IsValidCell(grid, end))
            {
                Debug.LogWarning($"[PathFinder] Invalid start/end positions");
                return CreateDirectPath(start, end, gridSize);
            }

            // Same position - no path needed
            if (start == end)
            {
                return new List<Vector2Int> { start };
            }

            // A* implementation
            PriorityQueue openSet = new PriorityQueue();
            HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
            Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float>();

            // Initialize start node
            gScore[start] = 0f;
            float hScore = GetHeuristic(start, end);
            openSet.Enqueue(new PathNode(start, 0f, hScore));

            int maxIterations = gridSize * gridSize * 2; // Safety limit
            int iterations = 0;

            while (openSet.Count > 0 && iterations < maxIterations)
            {
                iterations++;
                PathNode current = openSet.Dequeue();

                // Reached destination
                if (current.Position == end)
                {
                    List<Vector2Int> path = RetracePath(cameFrom, start, end);
                    Debug.Log($"[PathFinder] Path found: {path.Count} cells (iterations: {iterations})");
                    return path;
                }

                closedSet.Add(current.Position);

                // Check 4 neighbors (N, S, E, W)
                Vector2Int[] directions = {
                    new Vector2Int(0, 1),   // North
                    new Vector2Int(0, -1),  // South
                    new Vector2Int(1, 0),   // East
                    new Vector2Int(-1, 0)   // West
                };

                foreach (Vector2Int dir in directions)
                {
                    Vector2Int neighbor = current.Position + dir;

                    // Skip invalid or closed cells
                    if (!IsValidCell(grid, neighbor) || closedSet.Contains(neighbor))
                        continue;

                    // Calculate cost with randomness
                    float tentativeG = gScore[current.Position] + GetMovementCost(grid, neighbor, randomness);

                    // Better path found
                    if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current.Position;
                        gScore[neighbor] = tentativeG;
                        float h = GetHeuristic(neighbor, end);
                        openSet.Enqueue(new PathNode(neighbor, tentativeG, h));
                    }
                }
            }

            // No path found - use direct fallback
            Debug.LogWarning($"[PathFinder] No path found after {iterations} iterations, using direct route");
            return CreateDirectPath(start, end, gridSize);
        }

        /// <summary>
        /// Find paths connecting multiple points (minimum spanning tree).
        /// </summary>
        /// <param name="grid">Grid maze cell data</param>
        /// <param name="points">List of points to connect</param>
        /// <param name="seed">Random seed for procedural variation</param>
        /// <param name="randomness">0.0=straight, 1.0=chaotic</param>
        /// <returns>List of all path segments</returns>
        public static List<List<Vector2Int>> FindAllPaths(GridMazeCell[,] grid, List<Vector2Int> points, uint seed, float randomness = 0.3f)
        {
            List<List<Vector2Int>> allPaths = new List<List<Vector2Int>>();

            if (points.Count < 2)
            {
                Debug.LogWarning($"[PathFinder] Not enough points to connect ({points.Count})");
                return allPaths;
            }

            Debug.Log($"[PathFinder] Connecting {points.Count} points with MST algorithm...");

            // Prim's algorithm for minimum spanning tree
            HashSet<Vector2Int> connected = new HashSet<Vector2Int>();
            connected.Add(points[0]);

            List<Vector2Int> unconnected = new List<Vector2Int>(points);
            unconnected.RemoveAt(0);

            // Use System.Random for reproducible randomness
            System.Random rng = new System.Random((int)seed);

            int pathsCreated = 0;
            while (unconnected.Count > 0)
            {
                // Find closest unconnected point (with randomized weights)
                Vector2Int bestFrom = Vector2Int.zero;
                Vector2Int bestTo = Vector2Int.zero;
                float bestDistance = float.MaxValue;

                foreach (Vector2Int from in connected)
                {
                    foreach (Vector2Int to in unconnected)
                    {
                        float dist = GetRandomizedDistance(from, to, rng);
                        if (dist < bestDistance)
                        {
                            bestDistance = dist;
                            bestFrom = from;
                            bestTo = to;
                        }
                    }
                }

                // Find path between best pair
                List<Vector2Int> path = FindPath(grid, bestFrom, bestTo, randomness);
                allPaths.Add(path);
                pathsCreated++;

                connected.Add(bestTo);
                unconnected.Remove(bestTo);
            }

            Debug.Log($"[PathFinder] Created {pathsCreated} path segments");
            return allPaths;
        }

        /// <summary>
        /// Validate that all walkable cells are connected.
        /// </summary>
        /// <param name="grid">Grid maze cell data</param>
        /// <returns>True if all walkable cells are reachable</returns>
        public static bool ValidateConnectivity(GridMazeCell[,] grid)
        {
            int gridSize = grid.GetLength(0);
            int walkableCount = 0;
            Vector2Int firstWalkable = Vector2Int.zero;

            // Find first walkable cell
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    if (IsWalkable(grid[x, y]))
                    {
                        walkableCount++;
                        if (firstWalkable == Vector2Int.zero)
                        {
                            firstWalkable = new Vector2Int(x, y);
                        }
                    }
                }
            }

            if (walkableCount == 0)
            {
                Debug.LogWarning("[PathFinder] No walkable cells found");
                return true; // Technically connected (empty)
            }

            // Flood fill from first walkable cell
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(firstWalkable);
            visited.Add(firstWalkable);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();

                Vector2Int[] neighbors = {
                    new Vector2Int(0, 1),
                    new Vector2Int(0, -1),
                    new Vector2Int(1, 0),
                    new Vector2Int(-1, 0)
                };

                foreach (Vector2Int dir in neighbors)
                {
                    Vector2Int neighbor = current + dir;
                    if (IsValidCell(grid, neighbor) &&
                        IsWalkable(grid[neighbor.x, neighbor.y]) &&
                        !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            bool connected = visited.Count == walkableCount;
            Debug.Log($"[PathFinder] Connectivity: {visited.Count}/{walkableCount} cells reachable");

            if (!connected)
            {
                Debug.LogWarning($"[PathFinder] Maze is NOT fully connected! Missing: {walkableCount - visited.Count} cells");
            }

            return connected;
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Heuristic: Manhattan distance (4-directional movement).
        /// </summary>
        private static float GetHeuristic(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        /// <summary>
        /// Get movement cost with randomness.
        /// Prefers existing corridors/rooms for efficiency.
        /// </summary>
        private static float GetMovementCost(GridMazeCell[,] grid, Vector2Int cell, float randomness)
        {
            float baseCost = 1.0f;

            // Prefer existing corridors/rooms (cheaper to extend)
            GridMazeCell cellType = grid[cell.x, cell.y];
            if (cellType == GridMazeCell.Corridor || cellType == GridMazeCell.Room)
            {
                baseCost = 0.5f;
            }

            // Add randomness for procedural variation (use Unity Random - runtime OK)
            float random = UnityEngine.Random.value * randomness;
            return baseCost + random;
        }

        /// <summary>
        /// Get randomized distance between two points.
        /// </summary>
        private static float GetRandomizedDistance(Vector2Int a, Vector2Int b, System.Random rng)
        {
            float baseDist = Vector2Int.Distance(a, b);
            float random = (float)rng.NextDouble() * 0.5f; // 0.0 to 0.5
            return baseDist * (1.0f + random);
        }

        /// <summary>
        /// Check if cell is within grid bounds.
        /// </summary>
        private static bool IsValidCell(GridMazeCell[,] grid, Vector2Int cell)
        {
            int gridSize = grid.GetLength(0);
            return cell.x >= 0 && cell.x < gridSize && cell.y >= 0 && cell.y < gridSize;
        }

        /// <summary>
        /// Check if cell type is walkable.
        /// </summary>
        private static bool IsWalkable(GridMazeCell cellType)
        {
            return cellType == GridMazeCell.Floor ||
                   cellType == GridMazeCell.Room ||
                   cellType == GridMazeCell.Corridor ||
                   cellType == GridMazeCell.SpawnPoint ||
                   cellType == GridMazeCell.Door;
        }

        /// <summary>
        /// Retrace path from end to start using cameFrom map.
        /// </summary>
        private static List<Vector2Int> RetracePath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int start, Vector2Int end)
        {
            List<Vector2Int> path = new List<Vector2Int>();
            Vector2Int current = end;

            while (current != start)
            {
                path.Add(current);
                if (!cameFrom.TryGetValue(current, out current))
                {
                    Debug.LogWarning("[PathFinder] Path retracing failed - disconnected");
                    break;
                }
            }

            path.Reverse();
            return path;
        }

        /// <summary>
        /// Create direct L-shaped path (fallback when A* fails).
        /// </summary>
        private static List<Vector2Int> CreateDirectPath(Vector2Int start, Vector2Int end, int gridSize)
        {
            List<Vector2Int> path = new List<Vector2Int>();

            // Clamp to grid bounds
            int startX = Mathf.Clamp(start.x, 0, gridSize - 1);
            int startY = Mathf.Clamp(start.y, 0, gridSize - 1);
            int endX = Mathf.Clamp(end.x, 0, gridSize - 1);
            int endY = Mathf.Clamp(end.y, 0, gridSize - 1);

            // Horizontal segment first
            int x = startX;
            while (x != endX)
            {
                path.Add(new Vector2Int(x, startY));
                x += (int)Mathf.Sign(endX - x);
            }

            // Then vertical segment
            int y = startY;
            while (y != endY)
            {
                path.Add(new Vector2Int(x, y));
                y += (int)Mathf.Sign(endY - y);
            }

            path.Add(new Vector2Int(endX, endY));
            return path;
        }

        #endregion

        #region PathNode (A* Node)

        /// <summary>
        /// A* pathfinding node.
        /// </summary>
        private class PathNode
        {
            public Vector2Int Position;
            public float G; // Cost from start
            public float H; // Heuristic to end
            public float F => G + H; // Total cost

            public PathNode(Vector2Int pos, float g, float h)
            {
                Position = pos;
                G = g;
                H = h;
            }
        }

        #endregion

        #region Priority Queue (Min-Heap for A*)

        /// <summary>
        /// Simple priority queue for A* open set.
        /// Orders nodes by F score (lowest first).
        /// </summary>
        private class PriorityQueue
        {
            private List<PathNode> elements = new List<PathNode>();
            public int Count => elements.Count;

            public void Enqueue(PathNode item)
            {
                elements.Add(item);
                // Sort by F score (ascending)
                elements.Sort((a, b) => a.F.CompareTo(b.F));
            }

            public PathNode Dequeue()
            {
                if (elements.Count == 0)
                {
                    return null;
                }
                PathNode item = elements[0];
                elements.RemoveAt(0);
                return item;
            }

            public bool Contains(Vector2Int position)
            {
                return elements.Exists(n => n.Position == position);
            }
        }

        #endregion

        #region Debug Utilities

        /// <summary>
        /// Print path to console for debugging.
        /// </summary>
        public static void DebugPrintPath(List<Vector2Int> path, string label = "Path")
        {
            if (path == null || path.Count == 0)
            {
                Debug.Log($"[PathFinder] {label}: (empty)");
                return;
            }

            string pathStr = string.Join("  ", path.ConvertAll(p => $"({p.x},{p.y})"));
            Debug.Log($"[PathFinder] {label}: {path.Count} cells - {pathStr}");
        }

        /// <summary>
        /// Visualize path in editor (Gizmos).
        /// Call from OnDrawGizmos().
        /// </summary>
        public static void DrawGizmos(List<Vector2Int> path, float cellSize, Color color)
        {
            if (path == null || path.Count < 2) return;

            Gizmos.color = color;
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 start = new Vector3(
                    path[i].x * cellSize + cellSize / 2f,
                    0.5f,
                    path[i].y * cellSize + cellSize / 2f
                );
                Vector3 end = new Vector3(
                    path[i + 1].x * cellSize + cellSize / 2f,
                    0.5f,
                    path[i + 1].y * cellSize + cellSize / 2f
                );
                Gizmos.DrawLine(start, end);
            }
        }

        #endregion
    }
}
