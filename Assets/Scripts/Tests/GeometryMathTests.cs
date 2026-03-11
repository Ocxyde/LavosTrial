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
// GeometryMathTests.cs
// Unit tests for pure geometry mathematics
// Unity 6 compatible - UTF-8 encoding - Unix line endings
// Location: Assets/Scripts/Tests/

using NUnit.Framework;
using Code.Lavos.Geometry;
using System;

namespace Code.Lavos.Tests
{
    [TestFixture]
    public class GeometryMathTests
    {
        #region Vector3d Tests

        [Test]
        public void Vector3d_Constructor()
        {
            var vec = new Vector3d(3.0, 4.0, 0.0);
            Assert.AreEqual(3.0, vec.X);
            Assert.AreEqual(4.0, vec.Y);
            Assert.AreEqual(0.0, vec.Z);
        }

        [Test]
        public void Vector3d_Magnitude()
        {
            var vec = new Vector3d(3.0, 4.0, 0.0);
            Assert.AreEqual(5.0, vec.magnitude, 1e-10);
        }

        [Test]
        public void Vector3d_Normalize()
        {
            var vec = new Vector3d(3.0, 4.0, 0.0);
            var normalized = vec.normalized;
            Assert.AreEqual(0.6, normalized.X, 1e-10);
            Assert.AreEqual(0.8, normalized.Y, 1e-10);
            Assert.AreEqual(1.0, normalized.magnitude, 1e-10);
        }

        [Test]
        public void Vector3d_Addition()
        {
            var a = new Vector3d(1.0, 2.0, 3.0);
            var b = new Vector3d(4.0, 5.0, 6.0);
            var result = a + b;
            Assert.AreEqual(5.0, result.X);
            Assert.AreEqual(7.0, result.Y);
            Assert.AreEqual(9.0, result.Z);
        }

        [Test]
        public void Vector3d_Subtraction()
        {
            var a = new Vector3d(5.0, 7.0, 9.0);
            var b = new Vector3d(1.0, 2.0, 3.0);
            var result = a - b;
            Assert.AreEqual(4.0, result.X);
            Assert.AreEqual(5.0, result.Y);
            Assert.AreEqual(6.0, result.Z);
        }

        [Test]
        public void Vector3d_DotProduct()
        {
            var a = new Vector3d(1.0, 2.0, 3.0);
            var b = new Vector3d(4.0, 5.0, 6.0);
            double dot = Vector3d.Dot(a, b);
            Assert.AreEqual(32.0, dot, 1e-10);
        }

        [Test]
        public void Vector3d_CrossProduct()
        {
            var i = new Vector3d(1.0, 0.0, 0.0);
            var j = new Vector3d(0.0, 1.0, 0.0);
            var result = Vector3d.Cross(i, j);
            Assert.AreEqual(0.0, result.X, 1e-10);
            Assert.AreEqual(0.0, result.Y, 1e-10);
            Assert.AreEqual(1.0, result.Z, 1e-10);
        }

        #endregion

        #region Triangle Tests

        [Test]
        public void Triangle_Constructor()
        {
            var a = new Vector3d(0.0, 0.0, 0.0);
            var b = new Vector3d(3.0, 0.0, 0.0);
            var c = new Vector3d(0.0, 4.0, 0.0);
            var triangle = new Triangle(a, b, c);
            
            Assert.AreEqual(0.0, triangle.A.X);
            Assert.AreEqual(3.0, triangle.B.X);
        }

        [Test]
        public void Triangle_CreateEquilateral()
        {
            var triangle = Triangle.CreateEquilateral(sideLength: 6.0);
            Assert.IsTrue(triangle.IsValid());
            var edges = triangle.EdgeLengths();
            Assert.AreEqual(edges[0], edges[1], 1e-10);
            Assert.AreEqual(edges[1], edges[2], 1e-10);
        }

        [Test]
        public void Triangle_Area()
        {
            var a = new Vector3d(0.0, 0.0, 0.0);
            var b = new Vector3d(3.0, 0.0, 0.0);
            var c = new Vector3d(0.0, 4.0, 0.0);
            var triangle = new Triangle(a, b, c);
            double area = triangle.Area();
            Assert.AreEqual(6.0, area, 1e-10);
        }

        [Test]
        public void Triangle_Perimeter()
        {
            var a = new Vector3d(0.0, 0.0, 0.0);
            var b = new Vector3d(3.0, 0.0, 0.0);
            var c = new Vector3d(0.0, 4.0, 0.0);
            var triangle = new Triangle(a, b, c);
            double perimeter = triangle.Perimeter();
            Assert.AreEqual(12.0, perimeter, 1e-10);
        }

        [Test]
        public void Triangle_Centroid()
        {
            var a = new Vector3d(0.0, 0.0, 0.0);
            var b = new Vector3d(3.0, 0.0, 0.0);
            var c = new Vector3d(0.0, 3.0, 0.0);
            var triangle = new Triangle(a, b, c);
            var centroid = triangle.Centroid();
            Assert.AreEqual(1.0, centroid.X, 1e-10);
            Assert.AreEqual(1.0, centroid.Y, 1e-10);
        }

        [Test]
        public void Triangle_ContainsPoint2D_Inside()
        {
            var a = new Vector3d(0.0, 0.0, 0.0);
            var b = new Vector3d(4.0, 0.0, 0.0);
            var c = new Vector3d(0.0, 4.0, 0.0);
            var triangle = new Triangle(a, b, c);
            var point = new Vector3d(1.0, 1.0, 0.0);
            Assert.IsTrue(triangle.ContainsPoint2D(point));
        }

        [Test]
        public void Triangle_ContainsPoint2D_Outside()
        {
            var a = new Vector3d(0.0, 0.0, 0.0);
            var b = new Vector3d(4.0, 0.0, 0.0);
            var c = new Vector3d(0.0, 4.0, 0.0);
            var triangle = new Triangle(a, b, c);
            var point = new Vector3d(3.0, 3.0, 0.0);
            Assert.IsFalse(triangle.ContainsPoint2D(point));
        }

        [Test]
        public void Triangle_IsValid()
        {
            var a = new Vector3d(0.0, 0.0, 0.0);
            var b = new Vector3d(1.0, 0.0, 0.0);
            var c = new Vector3d(0.0, 1.0, 0.0);
            var triangle = new Triangle(a, b, c);
            Assert.IsTrue(triangle.IsValid());
        }

        [Test]
        public void Triangle_IsEquilateral()
        {
            var triangle = Triangle.CreateEquilateral(sideLength: 6.0);
            Assert.IsTrue(triangle.IsEquilateral());
        }

        [Test]
        public void Triangle_IsRightAngled()
        {
            var a = new Vector3d(0.0, 0.0, 0.0);
            var b = new Vector3d(3.0, 0.0, 0.0);
            var c = new Vector3d(0.0, 4.0, 0.0);
            var triangle = new Triangle(a, b, c);
            Assert.IsTrue(triangle.IsRightAngled());
        }

        #endregion

        #region Tetrahedron Tests

        [Test]
        public void Tetrahedron_Constructor()
        {
            var a = new Vector3d(0.0, 0.0, 0.0);
            var b = new Vector3d(1.0, 0.0, 0.0);
            var c = new Vector3d(0.0, 1.0, 0.0);
            var d = new Vector3d(0.0, 0.0, 1.0);
            var tetra = new Tetrahedron(a, b, c, d);
            
            Assert.AreEqual(0.0, tetra.A.X);
            Assert.AreEqual(4, Tetrahedron.VertexCount);
            Assert.AreEqual(6, Tetrahedron.EdgeCount);
        }

        [Test]
        public void Tetrahedron_CreateRegular()
        {
            var tetra = Tetrahedron.CreateRegular(edgeLength: 1.0);
            Assert.IsTrue(tetra.Volume() > 0);
        }

        [Test]
        public void Tetrahedron_Volume()
        {
            var a = new Vector3d(0.0, 0.0, 0.0);
            var b = new Vector3d(1.0, 0.0, 0.0);
            var c = new Vector3d(0.0, 1.0, 0.0);
            var d = new Vector3d(0.0, 0.0, 1.0);
            var tetra = new Tetrahedron(a, b, c, d);
            double volume = tetra.Volume();
            Assert.AreEqual(1.0 / 6.0, volume, 1e-10);
        }

        [Test]
        public void Tetrahedron_Centroid()
        {
            var a = new Vector3d(1.0, 1.0, 1.0);
            var b = new Vector3d(1.0, -1.0, -1.0);
            var c = new Vector3d(-1.0, 1.0, -1.0);
            var d = new Vector3d(-1.0, -1.0, 1.0);
            var tetra = new Tetrahedron(a, b, c, d);
            var centroid = tetra.Centroid();
            Assert.AreEqual(0.0, centroid.X, 1e-10);
            Assert.AreEqual(0.0, centroid.Y, 1e-10);
            Assert.AreEqual(0.0, centroid.Z, 1e-10);
        }

        [Test]
        public void Tetrahedron_ContainsPoint_Inside()
        {
            var a = new Vector3d(0.0, 0.0, 0.0);
            var b = new Vector3d(2.0, 0.0, 0.0);
            var c = new Vector3d(0.0, 2.0, 0.0);
            var d = new Vector3d(0.0, 0.0, 2.0);
            var tetra = new Tetrahedron(a, b, c, d);
            var point = new Vector3d(0.3, 0.3, 0.3);
            Assert.IsTrue(tetra.ContainsPoint(point));
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Integration_GeometryPipeline()
        {
            var a = new Vector3d(0.0, 0.0, 0.0);
            var b = new Vector3d(3.0, 0.0, 0.0);
            var c = new Vector3d(0.0, 4.0, 0.0);
            var triangle = new Triangle(a, b, c);
            
            double area = triangle.Area();
            double perimeter = triangle.Perimeter();
            var centroid = triangle.Centroid();
            
            Assert.AreEqual(6.0, area, 1e-10);
            Assert.AreEqual(12.0, perimeter, 1e-10);
            Assert.AreEqual(1.0, centroid.X, 1e-10);
        }

        #endregion
    }
}