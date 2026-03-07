// Copyright (C) 2026 Ocxyde
//
// This file is part of PeuImporte.
//
// PeuImporte is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PeuImporte is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PeuImporte.  If not, see <https://www.gnu.org/licenses/>.
// MazeGenerator.cs
// Procedural maze generation algorithm
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Core system - generates maze layout

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Code.Lavos.Core
{
    public class MazeGenerator : MonoBehaviour
    {
    [System.Flags]
    public enum Wall : byte { None = 0, North = 1, East = 2, South = 4, West = 8, All = 15 }

    [Header("Dimensions")]
    [SerializeField] public int width = 31;
    [SerializeField] public int height = 31;
    
    [Header("Dynamic Sizing (Level-Based)")]
    [Tooltip("Enable dynamic maze size based on level")]
    [SerializeField] private bool useDynamicSize = true;
    [Tooltip("Minimum maze size (level 1-5)")]
    [SerializeField] private int minMazeSize = 21;
    [Tooltip("Maximum maze size (level 20+)")]
    [SerializeField] private int maxMazeSize = 51;
    [Tooltip("Level at which max size is reached")]
    [SerializeField] private int maxLevel = 20;
    
    private int currentMazeLevel = 1;
    
    public void SetMazeLevel(int level)
    {
        currentMazeLevel = Mathf.Max(1, level);
        if (useDynamicSize)
        {
            // Calculate maze size based on level
            // Level 1-5: minMazeSize (21)
            // Level 20+: maxMazeSize (51)
            // Level 6-19: progressively larger
            float progress = (float)(currentMazeLevel - 1) / (maxLevel - 1);
            int size = Mathf.RoundToInt(Mathf.Lerp(minMazeSize, maxMazeSize, progress));
            
            // Ensure odd number for proper corridors
            if (size % 2 == 0) size++;
            
            width = size;
            height = size;
            
            Debug.Log($"[MazeGenerator] 📏 Level {currentMazeLevel}: {width}x{height} (dynamic size)");
        }
        else
        {
            Debug.Log($"[MazeGenerator] 📏 Level {currentMazeLevel}: {width}x{height} (fixed size)");
        }
    }

    public Wall[,] Grid { get; private set; }
    public int Width => width;
    public int Height => height;
    public Vector2Int StartCell { get; private set; }
    public Vector2Int ExitCell { get; private set; }

    private uint _seed;
    public uint CurrentSeed => _seed;
    private readonly int[] _dx = { 0, 1, 0, -1 };
    private readonly int[] _dy = { 1, 0, -1, 0 };
    private readonly Wall[] _opposite = { Wall.South, Wall.West, Wall.North, Wall.East };

    public void Generate()
    {
        // Use default seed (SeedManager removed)
        string seedString = "DEFAULT";
        Debug.Log("[MazeGenerator] Using default seed");

        _seed = ComputeSeed(seedString);

        Grid = new Wall[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                Grid[x, y] = Wall.All;

        GenerateMazeDFS();
        SetEntryExit();

        // Calculate complexity based on seed
        int complexity = CalculateComplexity();

        Debug.Log($"[MazeGenerator] Generated {width}x{height} | Seed: {_seed} (String: {seedString}) | Complexity: {complexity}");
    }

    /// <summary>
    /// Calculate maze complexity based on seed and dimensions.
    /// </summary>
    private int CalculateComplexity()
    {
        // Complexity = maze size + seed length factor
        int sizeFactor = width * height;
        string seedString = "DEFAULT"; // SeedManager removed
        int seedFactor = seedString.Length * 10;
        return (sizeFactor + seedFactor) / 100;
    }

    /// <summary>
    /// Generate maze and advance seed progression.
    /// Plug-in-out: Call this when loading a new level/scene.
    /// </summary>
    public void GenerateNextLevel()
    {
        Generate(); // SeedManager removed
    }

    /// <summary>
    /// Generate maze with specific seed (bypass SeedManager).
    /// For testing only.
    /// </summary>
    public void GenerateWithSeed(string seed)
    {
        _seed = ComputeSeed(seed);

        Grid = new Wall[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                Grid[x, y] = Wall.All;

        GenerateMazeDFS();
        SetEntryExit();

        Debug.Log($"[MazeGenerator] Generated {width}x{height} | Seed: {_seed} (Test: {seed})");
    }

    /// <summary>
    /// Check if a cell is floor (passable, not a solid wall)
    /// Used by SpatialPlacer for object placement - NEVER place in walls!
    /// </summary>
    public bool IsCellFloor(int x, int y)
    {
        // Bounds check
        if (x < 0 || x >= width || y < 0 || y >= height)
            return false;
        
        // Cell is floor if it has at least one opening (not Wall.All)
        Wall cell = Grid[x, y];
        
        // Empty cell or cell with some openings = floor (passable)
        // Solid Wall.All = not floor (solid wall)
        return cell != Wall.All;
    }

    /// <summary>
    /// Get grid size (for SpatialPlacer)
    /// </summary>
    public int gridSize => Mathf.Max(width, height);

    private uint ComputeSeed(string input)
    {
        if (string.IsNullOrEmpty(input))
            return 1;

        using var sha = SHA256.Create();
        byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToUInt32(hash, 0) ^ BitConverter.ToUInt32(hash, 4) ^ BitConverter.ToUInt32(hash, 8);
    }

    /// <summary>
    /// Generate a random alpha-digital seed string.
    /// </summary>
    public static string GenerateRandomSeed(int length = 8)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        StringBuilder sb = new StringBuilder();
        
        for (int i = 0; i < length; i++)
        {
            sb.Append(chars[UnityEngine.Random.Range(0, chars.Length)]);
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// Get human-readable seed string.
    /// </summary>
    public string GetSeedString()
    {
        return _seed.ToString(); // SeedManager removed
    }

    private void GenerateMazeDFS()
    {
        bool[,] visited = new bool[width, height];
        var stack = new Stack<Vector2Int>();

        visited[0, 0] = true;
        stack.Push(Vector2Int.zero);

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            List<int> neighbors = GetUnvisitedNeighbors(current, visited);

            if (neighbors.Count > 0)
            {
                int dir = neighbors[UnityEngine.Random.Range(0, neighbors.Count)];
                Vector2Int next = new Vector2Int(current.x + _dx[dir], current.y + _dy[dir]);

                Grid[current.x, current.y] &= (Wall)~(1 << dir);
                Grid[next.x, next.y] &= (Wall)~(byte)_opposite[dir];

                visited[next.x, next.y] = true;
                stack.Push(next);
            }
            else
            {
                stack.Pop();
            }
        }
    }

    private List<int> GetUnvisitedNeighbors(Vector2Int cell, bool[,] visited)
    {
        var result = new List<int>(4);

        for (int i = 0; i < 4; i++)
        {
            int nx = cell.x + _dx[i];
            int ny = cell.y + _dy[i];
            if (nx >= 0 && nx < width && ny >= 0 && ny < height && !visited[nx, ny])
                result.Add(i);
        }

        return result;
    }

    private void SetEntryExit()
    {
        StartCell = new Vector2Int(0, 0);
        ExitCell = new Vector2Int(width - 1, height - 1);
        Grid[StartCell.x, StartCell.y] &= ~Wall.South;
        Grid[ExitCell.x, ExitCell.y] &= ~Wall.North;
    }

    public bool HasWall(int x, int y, Wall wall) => (Grid[x, y] & wall) != 0;
}
}
