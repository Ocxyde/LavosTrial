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
// MazeGeometryTests.cs
// Unit tests for maze geometry system
// Unity 6 compatible - UTF-8 encoding - Unix line endings
// Location: Assets/Scripts/Tests/

using NUnit.Framework;
using System;
using Code.Lavos.Core;

namespace Code.Lavos.Tests
{
    [TestFixture]
    public class MazeGeometryTests
    {
        [Test]
        public void GenerateMaze_ReturnsValidData()
        {
            var generator = new GridMazeGenerator();
            var config = new MazeConfig();
            var mazeData = generator.Generate(seed: 42, level: 0, cfg: config);
            
            Assert.IsNotNull(mazeData);
            Assert.Greater(mazeData.Width, 0);
            Assert.Greater(mazeData.Height, 0);
        }

        [Test]
        public void GenerateMaze_SpawnRoomCleared()
        {
            var generator = new GridMazeGenerator();
            var config = new MazeConfig();
            var mazeData = generator.Generate(seed: 42, level: 0, cfg: config);

            Assert.IsNotNull(mazeData.SpawnCell);
            
            // Verify spawn cell is at expected position (1,1)
            Assert.AreEqual(1, mazeData.SpawnCell.x);
            Assert.AreEqual(1, mazeData.SpawnCell.z);
            
            // Verify spawn cell is walkable (no walls)
            var spawnCell = mazeData.GetCell(mazeData.SpawnCell.x, mazeData.SpawnCell.z);
            Assert.IsTrue((spawnCell & CellFlags8.AllWalls) == 0, "Spawn cell should be walkable");
        }

        [Test]
        public void GenerateMaze_ExitMarked()
        {
            var generator = new GridMazeGenerator();
            var config = new MazeConfig();
            var mazeData = generator.Generate(seed: 42, level: 0, cfg: config);

            Assert.IsNotNull(mazeData.ExitCell);
            
            // Verify exit cell is at expected position (width-2, height-2)
            Assert.AreEqual(mazeData.Width - 2, mazeData.ExitCell.x);
            Assert.AreEqual(mazeData.Height - 2, mazeData.ExitCell.z);
            
            // Verify exit cell is walkable (no walls)
            var exitCell = mazeData.GetCell(mazeData.ExitCell.x, mazeData.ExitCell.z);
            Assert.IsTrue((exitCell & CellFlags8.AllWalls) == 0, "Exit cell should be walkable");
        }

        [Test]
        public void GenerateMaze_SameSeedProducesSameMaze()
        {
            var generator = new GridMazeGenerator();
            var config = new MazeConfig();
            var maze1 = generator.Generate(seed: 12345, level: 0, cfg: config);
            var maze2 = generator.Generate(seed: 12345, level: 0, cfg: config);
            
            Assert.AreEqual(maze1.Width, maze2.Width);
            Assert.AreEqual(maze1.Seed, maze2.Seed);
            Assert.AreEqual(maze1.GetCell(0, 0), maze2.GetCell(0, 0));
        }

        [Test]
        public void CellFlags8_WallFlags()
        {
            CellFlags8 flags = CellFlags8.None;
            flags |= CellFlags8.WallN;
            flags |= CellFlags8.WallE;
            
            Assert.IsTrue((flags & CellFlags8.WallN) != 0);
            Assert.IsTrue((flags & CellFlags8.WallE) != 0);
            Assert.IsTrue((flags & CellFlags8.WallS) == 0);
            Assert.IsTrue((flags & CellFlags8.WallW) == 0);
        }

        [Test]
        public void CellFlags8_ObjectFlags()
        {
            CellFlags8 flags = CellFlags8.None;
            flags |= CellFlags8.HasChest;
            flags |= CellFlags8.HasTorch;
            
            Assert.IsTrue((flags & CellFlags8.HasChest) != 0);
            Assert.IsTrue((flags & CellFlags8.HasTorch) != 0);
        }

        [Test]
        public void Direction8Helper_CardinalOffsets()
        {
            var north = Direction8Helper.ToOffset(Direction8.N);
            Assert.AreEqual(0, north.dx);
            Assert.AreEqual(1, north.dz);

            var south = Direction8Helper.ToOffset(Direction8.S);
            Assert.AreEqual(0, south.dx);
            Assert.AreEqual(-1, south.dz);

            var east = Direction8Helper.ToOffset(Direction8.E);
            Assert.AreEqual(1, east.dx);
            Assert.AreEqual(0, east.dz);

            var west = Direction8Helper.ToOffset(Direction8.W);
            Assert.AreEqual(-1, west.dx);
            Assert.AreEqual(0, west.dz);
        }

        [Test]
        public void Direction8Helper_Opposites()
        {
            Assert.AreEqual(Direction8.S, Direction8Helper.Opposite(Direction8.N));
            Assert.AreEqual(Direction8.N, Direction8Helper.Opposite(Direction8.S));
            Assert.AreEqual(Direction8.W, Direction8Helper.Opposite(Direction8.E));
            Assert.AreEqual(Direction8.E, Direction8Helper.Opposite(Direction8.W));
        }

        [Test]
        public void Direction8Helper_ToWallFlag()
        {
            Assert.AreEqual(CellFlags8.WallN, Direction8Helper.ToWallFlag(Direction8.N));
            Assert.AreEqual(CellFlags8.WallS, Direction8Helper.ToWallFlag(Direction8.S));
            Assert.AreEqual(CellFlags8.WallE, Direction8Helper.ToWallFlag(Direction8.E));
            Assert.AreEqual(CellFlags8.WallW, Direction8Helper.ToWallFlag(Direction8.W));
        }

        [Test]
        public void MazeData8_Constructor()
        {
            var data = new MazeData8(width: 21, height: 21, seed: 42, level: 0);
            
            Assert.AreEqual(21, data.Width);
            Assert.AreEqual(21, data.Height);
            Assert.AreEqual(42, data.Seed);
        }

        [Test]
        public void MazeData8_CellAccessors()
        {
            var data = new MazeData8(width: 10, height: 10, seed: 42, level: 0);
            data.SetCell(5, 5, CellFlags8.WallN | CellFlags8.WallE);
            var cell = data.GetCell(5, 5);
            
            Assert.AreEqual(CellFlags8.WallN | CellFlags8.WallE, cell);
        }

        [Test]
        public void MazeData8_AddFlag()
        {
            var data = new MazeData8(width: 10, height: 10, seed: 42, level: 0);
            data.SetCell(3, 3, CellFlags8.None);
            data.AddFlag(3, 3, CellFlags8.HasChest);
            var cell = data.GetCell(3, 3);
            
            Assert.IsTrue((cell & CellFlags8.HasChest) != 0);
        }

        [Test]
        public void MazeData8_ClearFlag()
        {
            var data = new MazeData8(width: 10, height: 10, seed: 42, level: 0);
            data.SetCell(4, 4, CellFlags8.WallN | CellFlags8.WallS);
            data.ClearFlag(4, 4, CellFlags8.WallN);
            var cell = data.GetCell(4, 4);
            
            Assert.IsTrue((cell & CellFlags8.WallS) != 0);
            Assert.IsTrue((cell & CellFlags8.WallN) == 0);
        }

        [Test]
        public void MazeData8_InBounds()
        {
            var data = new MazeData8(width: 10, height: 10, seed: 42, level: 0);
            
            Assert.IsTrue(data.InBounds(0, 0));
            Assert.IsTrue(data.InBounds(9, 9));
            Assert.IsFalse(data.InBounds(-1, 0));
            Assert.IsFalse(data.InBounds(10, 5));
        }

        [Test]
        public void DeadEndCorridorSystem_Initialization()
        {
            var system = new DeadEndCorridorSystem();
            Assert.IsNotNull(system);
            Assert.AreEqual(0, system.TotalCount);
        }

        [Test]
        public void DeadEndCorridorSystem_CreateDefaultConfig()
        {
            var config = DeadEndCorridorSystem.CreateDefaultConfig();
            Assert.IsNotNull(config);
            Assert.Greater(config.BaseDensity, 0);
        }

        [Test]
        public void DeadEndCorridorSystem_ScaledDensity()
        {
            var system = new DeadEndCorridorSystem();
            float density0 = system.CalculateScaledDensity(0);
            float density39 = system.CalculateScaledDensity(39);
            
            Assert.Greater(density0, 0);
            Assert.Greater(density39, density0);
        }

        [Test]
        public void Integration_FullMazeGeneration()
        {
            var generator = new GridMazeGenerator();
            var config = new MazeConfig();
            var mazeData = generator.Generate(seed: 42, level: 0, cfg: config);
            
            Assert.IsNotNull(mazeData);
            Assert.Greater(mazeData.Width, 5);
            Assert.IsNotNull(mazeData.SpawnCell);
            Assert.IsNotNull(mazeData.ExitCell);
            Assert.AreNotEqual(mazeData.SpawnCell.x, mazeData.ExitCell.x);
        }
    }
}