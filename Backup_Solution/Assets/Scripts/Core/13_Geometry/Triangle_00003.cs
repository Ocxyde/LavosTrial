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
// Triangle.cs
// Triangle geometry - Pure mathematical representation
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// GEOMETRY: Pure triangle math (not Unity-dependent)
// Location: Assets/Scripts/Core/13_Geometry/
//
// FUTURE IMPLEMENTATION - Placeholder for triangle geometry system

using System;

namespace Code.Lavos.Geometry
{
    /// <summary>
    /// Triangle - Pure mathematical representation of a 2D/3D triangle.
    /// 
    /// A triangle is a polygon with:
    /// - 3 vertices
    /// - 3 edges
    /// - 1 face (itself)
    /// 
    /// This is a PURE MATH class - no Unity dependencies.
    /// For Unity mesh representation, use Unity's Mesh class.
    /// 
    /// FUTURE FEATURES (TODO):
    /// - Area calculation
    /// - Centroid calculation
    /// - Incenter/circumcenter
    /// - Orthocenter
    /// - Barycentric coordinates
    /// - Point-in-triangle test
    /// - Ray-triangle intersection
    /// - Normal calculation (3D)
    /// </summary>
    public struct Triangle
    {
        #region Fields

        /// <summary>
        /// The 3 vertices of the triangle
        /// </summary>
        public Vector3d A, B, C;

        #endregion

        #region Properties

        /// <summary>
        /// Get all vertices as array
        /// </summary>
        public Vector3d[] Vertices => new[] { A, B, C };

        /// <summary>
        /// Number of vertices (always 3)
        /// </summary>
        public const int VertexCount = 3;

        /// <summary>
        /// Number of edges (always 3)
        /// </summary>
        public const int EdgeCount = 3;

        #endregion

        #region Constructor

        /// <summary>
        /// Create a triangle from 3 vertices
        /// </summary>
        public Triangle(Vector3d a, Vector3d b, Vector3d c)
        {
            A = a;
            B = b;
            C = c;
        }

        /// <summary>
        /// Create an equilateral triangle in XY plane centered at origin
        /// </summary>
        public static Triangle CreateEquilateral(double sideLength)
        {
            double h = sideLength * Math.Sqrt(3) / 2;  // Height
            
            return new Triangle(
                new Vector3d(-sideLength / 2, -h / 3, 0),
                new Vector3d(sideLength / 2, -h / 3, 0),
                new Vector3d(0, 2 * h / 3, 0)
            );
        }

        #endregion

        #region Edge Calculations (TODO)

        /// <summary>
        /// Get the 3 edges as vertex pairs
        /// </summary>
        public (Vector3d, Vector3d)[] Edges()
        {
            return new[]
            {
                (A, B),  // Edge AB
                (B, C),  // Edge BC
                (C, A)   // Edge CA
            };
        }

        /// <summary>
        /// Calculate edge lengths
        /// TODO: Implement
        /// </summary>
        public double[] EdgeLengths()
        {
            // TODO: Implement distance formula for each edge
            return new double[] { 0, 0, 0 };
        }

        /// <summary>
        /// Calculate perimeter
        /// TODO: Implement
        /// </summary>
        public double Perimeter()
        {
            // TODO: Sum of all edge lengths
            return 0;
        }

        #endregion

        #region Area Calculations (TODO)

        /// <summary>
        /// Calculate area using Heron's formula
        /// TODO: Implement
        /// </summary>
        public double Area()
        {
            // s = (a + b + c) / 2
            // Area = sqrt(s * (s-a) * (s-b) * (s-c))
            // TODO: Implement
            return 0;
        }

        /// <summary>
        /// Calculate area using cross product (3D)
        /// TODO: Implement
        /// </summary>
        public double Area3D()
        {
            // Area = |AB × AC| / 2
            // TODO: Implement
            return 0;
        }

        #endregion

        #region Center Calculations (TODO)

        /// <summary>
        /// Calculate centroid (center of mass)
        /// TODO: Implement
        /// </summary>
        public Vector3d Centroid()
        {
            // Centroid = (A + B + C) / 3
            // TODO: Implement
            return new Vector3d();
        }

        /// <summary>
        /// Calculate circumcenter (center of circle passing through all vertices)
        /// TODO: Implement
        /// </summary>
        public Vector3d Circumcenter()
        {
            // TODO: Implement circumcenter calculation
            return new Vector3d();
        }

        /// <summary>
        /// Calculate incenter (center of inscribed circle)
        /// TODO: Implement
        /// </summary>
        public Vector3d Incenter()
        {
            // TODO: Implement incenter calculation
            return new Vector3d();
        }

        /// <summary>
        /// Calculate orthocenter (intersection of altitudes)
        /// TODO: Implement
        /// </summary>
        public Vector3d Orthocenter()
        {
            // TODO: Implement orthocenter calculation
            return new Vector3d();
        }

        #endregion

        #region Normal Calculation (TODO)

        /// <summary>
        /// Calculate normal vector (3D triangles only)
        /// TODO: Implement using cross product
        /// </summary>
        public Vector3d Normal()
        {
            // Normal = (B-A) × (C-A), normalized
            // TODO: Implement
            return new Vector3d();
        }

        #endregion

        #region Point Tests (TODO)

        /// <summary>
        /// Check if point is inside triangle (2D)
        /// TODO: Implement using barycentric coordinates
        /// </summary>
        public bool ContainsPoint2D(Vector3d point)
        {
            // TODO: Implement point-in-triangle test
            return false;
        }

        /// <summary>
        /// Check if point is inside triangle (3D, on triangle plane)
        /// TODO: Implement
        /// </summary>
        public bool ContainsPoint3D(Vector3d point)
        {
            // TODO: Implement 3D point-in-triangle test
            return false;
        }

        /// <summary>
        /// Get barycentric coordinates of point
        /// TODO: Implement
        /// </summary>
        public double[] GetBarycentric(Vector3d point)
        {
            // Returns (u, v, w) where point = u*A + v*B + w*C and u+v+w=1
            // TODO: Implement
            return new double[] { 0, 0, 0 };
        }

        #endregion

        #region Intersection Tests (TODO)

        /// <summary>
        /// Test if ray intersects triangle
        /// TODO: Implement using Möller–Trumbore algorithm
        /// </summary>
        public bool IntersectsRay(Vector3d origin, Vector3d direction)
        {
            // TODO: Implement ray-triangle intersection
            return false;
        }

        /// <summary>
        /// Test if line segment intersects triangle
        /// TODO: Implement
        /// </summary>
        public bool IntersectsSegment(Vector3d p1, Vector3d p2)
        {
            // TODO: Implement segment-triangle intersection
            return false;
        }

        /// <summary>
        /// Test if triangle intersects another triangle
        /// TODO: Implement
        /// </summary>
        public bool IntersectsTriangle(Triangle other)
        {
            // TODO: Implement triangle-triangle intersection
            return false;
        }

        #endregion

        #region Operators

        public static Triangle operator +(Triangle t, Vector3d v)
        {
            return new Triangle(t.A + v, t.B + v, t.C + v);
        }

        public static Triangle operator -(Triangle t, Vector3d v)
        {
            return new Triangle(t.A - v, t.B - v, t.C - v);
        }

        public static Triangle operator *(Triangle t, double s)
        {
            return new Triangle(t.A * s, t.B * s, t.C * s);
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Check if triangle is valid (non-degenerate)
        /// TODO: Implement
        /// </summary>
        public bool IsValid()
        {
            // Area should be > 0
            // TODO: Implement
            return false;
        }

        /// <summary>
        /// Check if triangle is equilateral (all sides equal)
        /// TODO: Implement
        /// </summary>
        public bool IsEquilateral(double tolerance = 0.0001)
        {
            // All 3 edges should be equal
            // TODO: Implement
            return false;
        }

        /// <summary>
        /// Check if triangle is isosceles (2 sides equal)
        /// TODO: Implement
        /// </summary>
        public bool IsIsosceles(double tolerance = 0.0001)
        {
            // At least 2 edges should be equal
            // TODO: Implement
            return false;
        }

        /// <summary>
        /// Check if triangle is right-angled
        /// TODO: Implement using Pythagorean theorem
        /// </summary>
        public bool IsRightAngled(double tolerance = 0.0001)
        {
            // a² + b² = c² (for some permutation)
            // TODO: Implement
            return false;
        }

        #endregion
    }
}
