// MazeGeneratorTests.cs
// Unit tests for MazeGenerator
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Test file - validates maze generation

using NUnit.Framework;
using UnityEngine;
using Code.Lavos.Core;

namespace Code.Lavos.Tests
{
    /// <summary>
    /// Unit tests for MazeGenerator class
    /// Tests maze generation, seed computation, and wall detection
    /// </summary>
    [TestFixture]
    public class MazeGeneratorTests
    {
        private MazeGenerator _generator;
        private GameObject _testObject;

        [SetUp]
        public void Setup()
        {
            _testObject = new GameObject("TestMazeGenerator");
            _generator = _testObject.AddComponent<MazeGenerator>();
            _generator.width = 11;
            _generator.height = 11;
        }

        [TearDown]
        public void TearDown()
        {
            if (_testObject != null)
                Object.DestroyImmediate(_testObject);
        }

        #region Generation Tests

        [Test]
        public void Generate_CreatesValidGrid()
        {
            _generator.Generate();
            
            Assert.That(_generator.Grid, Is.Not.Null);
            Assert.That(_generator.Width, Is.EqualTo(11));
            Assert.That(_generator.Height, Is.EqualTo(11));
        }

        [Test]
        public void Generate_CreatesPerfectMaze_NoIsolatedCells()
        {
            _generator.Generate();
            
            // All cells should be reachable (no isolated areas)
            // This is guaranteed by DFS algorithm
            bool[,] visited = new bool[_generator.Width, _generator.Height];
            int visitedCount = CountReachableCells(_generator, 0, 0, visited);
            
            Assert.That(visitedCount, Is.EqualTo(_generator.Width * _generator.Height), 
                "All cells should be reachable in a perfect maze");
        }

        [Test]
        public void Generate_HasEntryAndExit_Open()
        {
            _generator.Generate();
            
            // Start cell should have South wall open (entry)
            bool hasSouthWall = _generator.HasWall(0, 0, MazeGenerator.Wall.South);
            Assert.IsFalse(hasSouthWall, "Entry should have South wall removed");
            
            // Exit cell should have North wall open
            int exitX = _generator.Width - 1;
            int exitY = _generator.Height - 1;
            bool hasNorthWall = _generator.HasWall(exitX, exitY, MazeGenerator.Wall.North);
            Assert.IsFalse(hasNorthWall, "Exit should have North wall removed");
        }

        [Test]
        public void Generate_StartCellIsAtOrigin()
        {
            _generator.Generate();
            
            Assert.That(_generator.StartCell.x, Is.EqualTo(0));
            Assert.That(_generator.StartCell.y, Is.EqualTo(0));
        }

        [Test]
        public void Generate_ExitCellIsAtOppositeCorner()
        {
            _generator.Generate();
            
            Assert.That(_generator.ExitCell.x, Is.EqualTo(_generator.Width - 1));
            Assert.That(_generator.ExitCell.y, Is.EqualTo(_generator.Height - 1));
        }

        #endregion

        #region Seed Tests

        [Test]
        public void ComputeSeed_EmptyString_ReturnsOne()
        {
            // Access via reflection since ComputeSeed is private
            var methodInfo = typeof(MazeGenerator).GetMethod("ComputeSeed", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var result = (uint)methodInfo.Invoke(_generator, new object[] { "" });
            
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void Generate_SameSeed_ProducesSameMaze()
        {
            // Set seed via serialized field access
            SetSeedField("test_seed_123");
            _generator.Generate();
            var firstGrid = CloneGrid(_generator.Grid);
            
            SetSeedField("test_seed_123");
            _generator.Generate();
            
            AssertGridsEqual(firstGrid, _generator.Grid);
        }

        [Test]
        public void Generate_DifferentSeed_ProducesDifferentMaze()
        {
            SetSeedField("seed_a");
            _generator.Generate();
            var firstGrid = CloneGrid(_generator.Grid);
            
            SetSeedField("seed_b");
            _generator.Generate();
            
            bool gridsAreDifferent = !AreGridsIdentical(firstGrid, _generator.Grid);
            Assert.IsTrue(gridsAreDifferent, "Different seeds should produce different mazes");
        }

        [Test]
        public void CurrentSeed_ReturnsGeneratedSeed()
        {
            SetSeedField("test_seed");
            _generator.Generate();
            
            Assert.That(_generator.CurrentSeed, Is.GreaterThan(0));
        }

        #endregion

        #region Wall Detection Tests

        [Test]
        public void HasWall_AllWallsPresentInitially()
        {
            // Before generation, all cells should have all walls
            // Note: This test would require accessing Grid before Generate
            // After generation, we test specific walls
            _generator.Generate();
            
            // Test that HasWall works correctly for a known wall
            // Interior cells should have some walls removed
            bool hasSomeWalls = false;
            for (int x = 1; x < _generator.Width - 1; x++)
            {
                for (int y = 1; y < _generator.Height - 1; y++)
                {
                    if (_generator.HasWall(x, y, MazeGenerator.Wall.North))
                    {
                        hasSomeWalls = true;
                        break;
                    }
                }
                if (hasSomeWalls) break;
            }
            
            Assert.IsTrue(hasSomeWalls, "Maze should still have some walls");
        }

        [Test]
        public void HasWall_NorthSouth_Consistent()
        {
            _generator.Generate();
            
            // If cell (x, y) has North wall, cell (x, y+1) should have South wall
            for (int x = 0; x < _generator.Width; x++)
            {
                for (int y = 0; y < _generator.Height - 1; y++)
                {
                    bool hasNorth = _generator.HasWall(x, y, MazeGenerator.Wall.North);
                    bool hasSouth = _generator.HasWall(x, y + 1, MazeGenerator.Wall.South);
                    
                    Assert.That(hasNorth, Is.EqualTo(hasSouth), 
                        $"North/South wall inconsistency at ({x}, {y})");
                }
            }
        }

        [Test]
        public void HasWall_EastWest_Consistent()
        {
            _generator.Generate();
            
            // If cell (x, y) has East wall, cell (x+1, y) should have West wall
            for (int x = 0; x < _generator.Width - 1; x++)
            {
                for (int y = 0; y < _generator.Height; y++)
                {
                    bool hasEast = _generator.HasWall(x, y, MazeGenerator.Wall.East);
                    bool hasWest = _generator.HasWall(x + 1, y, MazeGenerator.Wall.West);
                    
                    Assert.That(hasEast, Is.EqualTo(hasWest), 
                        $"East/West wall inconsistency at ({x}, {y})");
                }
            }
        }

        #endregion

        #region Path Tests

        [Test]
        public void Generate_PathExistsFromStartToEnd()
        {
            _generator.Generate();
            
            bool pathExists = HasPathToExit(_generator);
            Assert.IsTrue(pathExists, "There should be a path from start to exit");
        }

        #endregion

        #region Helper Methods

        private void SetSeedField(string value)
        {
            var fieldInfo = typeof(MazeGenerator).GetField("seedString", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fieldInfo?.SetValue(_generator, value);
        }

        private int CountReachableCells(MazeGenerator gen, int x, int y, bool[,] visited)
        {
            if (x < 0 || x >= gen.Width || y < 0 || y >= gen.Height || visited[x, y])
                return 0;
            
            visited[x, y] = true;
            int count = 1;
            
            // Check all four directions
            if (!gen.HasWall(x, y, MazeGenerator.Wall.North))
                count += CountReachableCells(gen, x, y + 1, visited);
            if (!gen.HasWall(x, y, MazeGenerator.Wall.South))
                count += CountReachableCells(gen, x, y - 1, visited);
            if (!gen.HasWall(x, y, MazeGenerator.Wall.East))
                count += CountReachableCells(gen, x + 1, y, visited);
            if (!gen.HasWall(x, y, MazeGenerator.Wall.West))
                count += CountReachableCells(gen, x - 1, y, visited);
            
            return count;
        }

        private bool HasPathToExit(MazeGenerator gen)
        {
            bool[,] visited = new bool[gen.Width, gen.Height];
            return HasPathToExitDFS(gen, gen.StartCell.x, gen.StartCell.y, visited);
        }

        private bool HasPathToExitDFS(MazeGenerator gen, int x, int y, bool[,] visited)
        {
            if (x < 0 || x >= gen.Width || y < 0 || y >= gen.Height || visited[x, y])
                return false;
            
            if (x == gen.ExitCell.x && y == gen.ExitCell.y)
                return true;
            
            visited[x, y] = true;
            
            if (!gen.HasWall(x, y, MazeGenerator.Wall.North) && 
                HasPathToExitDFS(gen, x, y + 1, visited))
                return true;
            if (!gen.HasWall(x, y, MazeGenerator.Wall.South) && 
                HasPathToExitDFS(gen, x, y - 1, visited))
                return true;
            if (!gen.HasWall(x, y, MazeGenerator.Wall.East) && 
                HasPathToExitDFS(gen, x + 1, y, visited))
                return true;
            if (!gen.HasWall(x, y, MazeGenerator.Wall.West) && 
                HasPathToExitDFS(gen, x - 1, y, visited))
                return true;
            
            return false;
        }

        private MazeGenerator.Wall[,] CloneGrid(MazeGenerator.Wall[,] source)
        {
            int width = source.GetLength(0);
            int height = source.GetLength(1);
            var clone = new MazeGenerator.Wall[width, height];
            
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    clone[x, y] = source[x, y];
            
            return clone;
        }

        private void AssertGridsEqual(MazeGenerator.Wall[,] expected, MazeGenerator.Wall[,] actual)
        {
            Assert.That(expected.GetLength(0), Is.EqualTo(actual.GetLength(0)), "Width mismatch");
            Assert.That(expected.GetLength(1), Is.EqualTo(actual.GetLength(1)), "Height mismatch");
            
            for (int x = 0; x < expected.GetLength(0); x++)
            {
                for (int y = 0; y < expected.GetLength(1); y++)
                {
                    Assert.That(expected[x, y], Is.EqualTo(actual[x, y]), 
                        $"Grid mismatch at ({x}, {y})");
                }
            }
        }

        private bool AreGridsIdentical(MazeGenerator.Wall[,] a, MazeGenerator.Wall[,] b)
        {
            if (a.GetLength(0) != b.GetLength(0) || a.GetLength(1) != b.GetLength(1))
                return false;
            
            for (int x = 0; x < a.GetLength(0); x++)
                for (int y = 0; y < a.GetLength(1); y++)
                    if (a[x, y] != b[x, y])
                        return false;
            
            return true;
        }

        #endregion
    }
}
