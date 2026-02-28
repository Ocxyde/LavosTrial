using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Code.Lavos
{
    public class MazeGenerator : MonoBehaviour
    {
    [System.Flags]
    public enum Wall : byte { None = 0, North = 1, East = 2, South = 4, West = 8, All = 15 }

    [Header("Dimensions")]
    [SerializeField] public int width = 31;
    [SerializeField] public int height = 31;

    [Header("Seed")]
    [SerializeField] private string seedString = "";

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
        _seed = ComputeSeed(seedString);

        Grid = new Wall[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                Grid[x, y] = Wall.All;

        GenerateMazeDFS();
        SetEntryExit();

        Debug.Log($"[MazeGenerator] Generated {width}x{height} | Seed: {_seed}");
    }

    private uint ComputeSeed(string input)
    {
        if (string.IsNullOrEmpty(input))
            return 1;

        using var sha = SHA256.Create();
        byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToUInt32(hash, 0) ^ BitConverter.ToUInt32(hash, 4) ^ BitConverter.ToUInt32(hash, 8);
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
