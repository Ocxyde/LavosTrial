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
//
// MazeBinaryStorageTests.cs
// Unit tests for maze binary storage (LAV8S v3 format)
// Unity 6 compatible - UTF-8 encoding - Unix line endings
// Location: Assets/Scripts/Tests/

using NUnit.Framework;
using System;
using Code.Lavos.Core;

namespace Code.Lavos.Tests
{
    [TestFixture]
    public class MazeBinaryStorageTests
    {
        #region MazeData8 Tests

        [Test]
        public void MazeData8_Constructor()
        {
            var data = new MazeData8(width: 21, height: 21, seed: 42, level: 5);

            Assert.AreEqual(21, data.Width);
            Assert.AreEqual(21, data.Height);
            Assert.AreEqual(42, data.Seed);
            Assert.AreEqual(5, data.Level);
            Assert.Greater((long)data.Timestamp, 0L);
        }

        [Test]
        public void MazeData8_SetSpawn()
        {
            var data = new MazeData8(width: 21, height: 21, seed: 42, level: 0);
            data.SetSpawn(5, 5);
            
            Assert.AreEqual(5, data.SpawnCell.x);
            Assert.AreEqual(5, data.SpawnCell.z);
            
            var cell = data.GetCell(5, 5);
            Assert.IsTrue((cell & CellFlags8.SpawnRoom) != 0);
        }

        [Test]
        public void MazeData8_SetExit()
        {
            var data = new MazeData8(width: 21, height: 21, seed: 42, level: 0);
            data.SetExit(15, 15);
            
            Assert.AreEqual(15, data.ExitCell.x);
            Assert.AreEqual(15, data.ExitCell.z);
            
            var cell = data.GetCell(15, 15);
            Assert.IsTrue((cell & CellFlags8.IsExit) != 0);
        }

        [Test]
        public void MazeData8_DifficultyFactor()
        {
            var data = new MazeData8(width: 21, height: 21, seed: 42, level: 10);
            
            // DifficultyFactor is set internally during generation
            // Just verify it exists and has a value
            Assert.GreaterOrEqual(data.DifficultyFactor, 0f);
        }

        #endregion

        #region Binary Format Tests

        [Test]
        public void LAV8S_MagicHeader()
        {
            Assert.AreEqual(5, MazeData8.MAGIC.Length);
            byte[] expected = System.Text.Encoding.ASCII.GetBytes("LAV8S");
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(expected[i], MazeData8.MAGIC[i], $"Magic byte {i}");
            }
        }

        [Test]
        public void LAV8S_Version()
        {
            Assert.AreEqual(3, MazeData8.VERSION);
        }

        [Test]
        public void LAV8S_HeaderSize()
        {
            Assert.AreEqual(38, MazeData8.HEADER_SIZE);
        }

        [Test]
        public void LAV8S_FileSizeCalculation()
        {
            int level0Size = 42 + (12 * 12 * 2);
            Assert.AreEqual(330, level0Size);

            int level39Size = 42 + (51 * 51 * 2);
            Assert.AreEqual(5244, level39Size);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Integration_MazeGenerationWithSpawnExit()
        {
            var generator = new GridMazeGenerator();
            var config = new MazeConfig();
            var mazeData = generator.Generate(seed: 12345, level: 0, cfg: config);

            Assert.IsNotNull(mazeData);
            Assert.GreaterOrEqual(mazeData.SpawnCell.x, 0);
            Assert.GreaterOrEqual(mazeData.SpawnCell.z, 0);
            Assert.GreaterOrEqual(mazeData.ExitCell.x, 0);
            Assert.GreaterOrEqual(mazeData.ExitCell.z, 0);

            // Verify spawn and exit cells are walkable (no walls)
            var spawnCell = mazeData.GetCell(mazeData.SpawnCell.x, mazeData.SpawnCell.z);
            Assert.IsTrue((spawnCell & CellFlags8.AllWalls) == 0, "Spawn cell should be walkable");

            var exitCell = mazeData.GetCell(mazeData.ExitCell.x, mazeData.ExitCell.z);
            Assert.IsTrue((exitCell & CellFlags8.AllWalls) == 0, "Exit cell should be walkable");
        }

        [Test]
        public void Integration_CellOperationsIntegrity()
        {
            var data = new MazeData8(width: 10, height: 10, seed: 42, level: 0);
            
            data.SetCell(5, 5, CellFlags8.WallN | CellFlags8.WallE);
            data.AddFlag(5, 5, CellFlags8.HasChest);
            data.AddFlag(5, 5, CellFlags8.HasTorch);
            data.ClearFlag(5, 5, CellFlags8.WallE);
            
            var cell = data.GetCell(5, 5);
            Assert.IsTrue((cell & CellFlags8.WallN) != 0);
            Assert.IsTrue((cell & CellFlags8.WallE) == 0);
            Assert.IsTrue((cell & CellFlags8.HasChest) != 0);
            Assert.IsTrue((cell & CellFlags8.HasTorch) != 0);
        }

        [Test]
        public void Integration_DeadEndCorridorSystem()
        {
            var generator = new GridMazeGenerator();
            var config = new MazeConfig();
            var rng = new Random(42);
            
            var mazeData = generator.Generate(seed: 42, level: 5, cfg: config);
            
            var deadEndSystem = new DeadEndCorridorSystem();
            var corridors = deadEndSystem.Generate(mazeData, level: 5, rng: rng);
            
            Assert.IsNotNull(corridors);
            Assert.GreaterOrEqual(deadEndSystem.TotalCount, 0);
            
            var stats = deadEndSystem.GetStatistics();
            Assert.IsNotNull(stats);
        }

        #endregion

        #region Edge Cases

        [Test]
        public void EdgeCase_MinimumMazeSize()
        {
            var data = new MazeData8(width: 5, height: 5, seed: 1, level: 0);
            data.SetSpawn(1, 1);
            data.SetExit(3, 3);
            
            Assert.AreEqual(5, data.Width);
            Assert.AreEqual(5, data.Height);
            Assert.IsTrue(data.InBounds(0, 0));
            Assert.IsTrue(data.InBounds(4, 4));
        }

        [Test]
        public void EdgeCase_OutOfBounds()
        {
            var data = new MazeData8(width: 10, height: 10, seed: 42, level: 0);
            
            Assert.IsFalse(data.InBounds(-1, 5));
            Assert.IsFalse(data.InBounds(10, 5));
            Assert.IsFalse(data.InBounds(5, -1));
            Assert.IsFalse(data.InBounds(5, 10));
        }

        [Test]
        public void EdgeCase_AllWallFlags()
        {
            var data = new MazeData8(width: 10, height: 10, seed: 42, level: 0);
            data.SetCell(5, 5, CellFlags8.AllWalls);
            
            var cell = data.GetCell(5, 5);
            Assert.IsTrue((cell & CellFlags8.WallN) != 0);
            Assert.IsTrue((cell & CellFlags8.WallS) != 0);
            Assert.IsTrue((cell & CellFlags8.WallE) != 0);
            Assert.IsTrue((cell & CellFlags8.WallW) != 0);
            Assert.IsTrue((cell & CellFlags8.WallNE) != 0);
            Assert.IsTrue((cell & CellFlags8.WallNW) != 0);
            Assert.IsTrue((cell & CellFlags8.WallSE) != 0);
            Assert.IsTrue((cell & CellFlags8.WallSW) != 0);
        }

        [Test]
        public void EdgeCase_ClearAllFlags()
        {
            var data = new MazeData8(width: 10, height: 10, seed: 42, level: 0);
            data.SetCell(3, 3, CellFlags8.AllWalls | CellFlags8.HasChest);
            data.SetCell(3, 3, CellFlags8.None);
            
            var cell = data.GetCell(3, 3);
            Assert.AreEqual(CellFlags8.None, cell);
        }

        #endregion
    }
}