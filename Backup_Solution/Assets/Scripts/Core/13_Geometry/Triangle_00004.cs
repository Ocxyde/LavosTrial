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
// IMPLEMENTED:
// - Edge calculations (lengths, perimeter)
// - Area calculations (Heron's formula, cross product)
// - Center calculations (centroid, circumcenter, incenter, orthocenter)
// - Normal calculation
// - Point tests (ContainsPoint2D, ContainsPoint3D, barycentric)
// - Intersection tests (ray, segment, triangle)
// - Triangle classification (valid, equilateral, isosceles, right-angled)

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

        #region Edge Calculations

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
        /// </summary>
        public double[] EdgeLengths()
        {
            return new double[]
            {
                (B - A).magnitude,  // Edge AB
                (C - B).magnitude,  // Edge BC
                (A - C).magnitude   // Edge CA
            };
        }

        /// <summary>
        /// Calculate perimeter
        /// </summary>
        public double Perimeter()
        {
            double[] edges = EdgeLengths();
            return edges[0] + edges[1] + edges[2];
        }

        #endregion

        #region Area Calculations

        /// <summary>
        /// Calculate area using Heron's formula
        /// </summary>
        public double Area()
        {
            double[] edges = EdgeLengths();
            double a = edges[0], b = edges[1], c = edges[2];
            double s = Perimeter() / 2.0;  // Semi-perimeter
            return Math.Sqrt(s * (s - a) * (s - b) * (s - c));
        }

        /// <summary>
        /// Calculate area using cross product (3D)
        /// </summary>
        public double Area3D()
        {
            Vector3d ab = B - A;
            Vector3d ac = C - A;
            Vector3d cross = Vector3d.Cross(ab, ac);
            return cross.magnitude / 2.0;
        }

        #endregion

        #region Center Calculations

        /// <summary>
        /// Calculate centroid (center of mass)
        /// </summary>
        public Vector3d Centroid()
        {
            return (A + B + C) / 3.0;
        }

        /// <summary>
        /// Calculate circumcenter (center of circle passing through all vertices)
        /// </summary>
        public Vector3d Circumcenter()
        {
            double d = 2.0 * (A.x * (B.y - C.y) + B.x * (C.y - A.y) + C.x * (A.y - B.y));
            if (Math.Abs(d) < 1e-10) return new Vector3d();  // Degenerate triangle
            
            double ux = ((A.x * A.x + A.y * A.y) * (B.y - C.y) + 
                        (B.x * B.x + B.y * B.y) * (C.y - A.y) + 
                        (C.x * C.x + C.y * C.y) * (A.y - B.y)) / d;
            double uy = ((A.x * A.x + A.y * A.y) * (C.x - B.x) + 
                        (B.x * B.x + B.y * B.y) * (A.x - C.x) + 
                        (C.x * C.x + C.y * C.y) * (B.x - A.x)) / d;
            return new Vector3d(ux, uy, (A.z + B.z + C.z) / 3.0);
        }

        /// <summary>
        /// Calculate incenter (center of inscribed circle)
        /// </summary>
        public Vector3d Incenter()
        {
            double[] edges = EdgeLengths();
            double a = edges[0], b = edges[1], c = edges[2];
            double perimeter = Perimeter();
            if (perimeter < 1e-10) return new Vector3d();
            
            double x = (a * A.x + b * B.x + c * C.x) / perimeter;
            double y = (a * A.y + b * B.y + c * C.y) / perimeter;
            double z = (a * A.z + b * B.z + c * C.z) / perimeter;
            return new Vector3d(x, y, z);
        }

        /// <summary>
        /// Calculate orthocenter (intersection of altitudes)
        /// </summary>
        public Vector3d Orthocenter()
        {
            double d = 2.0 * (A.x * (B.y - C.y) + B.x * (C.y - A.y) + C.x * (A.y - B.y));
            if (Math.Abs(d) < 1e-10) return new Vector3d();  // Degenerate triangle
            
            // Calculate using the relationship with circumcenter and centroid
            Vector3d circumcenter = Circumcenter();
            Vector3d centroid = Centroid();
            return 3.0 * centroid - 2.0 * circumcenter;
        }

        #endregion

        #region Normal Calculation

        /// <summary>
        /// Calculate normal vector (3D triangles only)
        /// </summary>
        public Vector3d Normal()
        {
            Vector3d ab = B - A;
            Vector3d ac = C - A;
            Vector3d normal = Vector3d.Cross(ab, ac);
            return normal.normalized;
        }

        #endregion

        #region Point Tests

        /// <summary>
        /// Check if point is inside triangle (2D) using barycentric coordinates
        /// </summary>
        public bool ContainsPoint2D(Vector3d point)
        {
            double[] bary = GetBarycentric(point);
            return bary[0] >= 0 && bary[1] >= 0 && bary[2] >= 0 && 
                   Math.Abs(bary[0] + bary[1] + bary[2] - 1.0) < 1e-10;
        }

        /// <summary>
        /// Check if point is inside triangle (3D, on triangle plane)
        /// </summary>
        public bool ContainsPoint3D(Vector3d point)
        {
            // Project to 2D by dropping the smallest component of normal
            Vector3d normal = Normal();
            double absX = Math.Abs(normal.x);
            double absY = Math.Abs(normal.y);
            double absZ = Math.Abs(normal.z);
            
            Vector3d a2D, b2D, c2D, p2D;
            if (absX > absY && absX > absZ)
            {
                a2D = new Vector3d(0, A.y, A.z);
                b2D = new Vector3d(0, B.y, B.z);
                c2D = new Vector3d(0, C.y, C.z);
                p2D = new Vector3d(0, point.y, point.z);
            }
            else if (absY > absX && absY > absZ)
            {
                a2D = new Vector3d(A.x, 0, A.z);
                b2D = new Vector3d(B.x, 0, B.z);
                c2D = new Vector3d(C.x, 0, C.z);
                p2D = new Vector3d(point.x, 0, point.z);
            }
            else
            {
                a2D = new Vector3d(A.x, A.y, 0);
                b2D = new Vector3d(B.x, B.y, 0);
                c2D = new Vector3d(C.x, C.y, 0);
                p2D = new Vector3d(point.x, point.y, 0);
            }
            
            Triangle t2D = new Triangle(a2D, b2D, c2D);
            return t2D.ContainsPoint2D(p2D);
        }

        /// <summary>
        /// Get barycentric coordinates of point
        /// </summary>
        public double[] GetBarycentric(Vector3d point)
        {
            Vector3d v0 = C - A;
            Vector3d v1 = B - A;
            Vector3d v2 = point - A;
            
            double dot00 = Vector3d.Dot(v0, v0);
            double dot01 = Vector3d.Dot(v0, v1);
            double dot02 = Vector3d.Dot(v0, v2);
            double dot11 = Vector3d.Dot(v1, v1);
            double dot12 = Vector3d.Dot(v1, v2);
            
            double denom = dot00 * dot11 - dot01 * dot01;
            if (Math.Abs(denom) < 1e-10) return new double[] { 1.0 / 3.0, 1.0 / 3.0, 1.0 / 3.0 };
            
            double invDenom = 1.0 / denom;
            double u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            double v = (dot00 * dot12 - dot01 * dot02) * invDenom;
            double w = 1.0 - u - v;
            
            return new double[] { w, v, u };
        }

        #endregion

        #region Intersection Tests

        /// <summary>
        /// Test if ray intersects triangle using Möller–Trumbore algorithm
        /// </summary>
        public bool IntersectsRay(Vector3d origin, Vector3d direction)
        {
            Vector3d edge1 = B - A;
            Vector3d edge2 = C - A;
            Vector3d h = Vector3d.Cross(direction, edge2);
            double a = Vector3d.Dot(edge1, h);
            
            if (Math.Abs(a) < 1e-10) return false;  // Ray parallel to triangle
            
            double f = 1.0 / a;
            Vector3d s = origin - A;
            double u = f * Vector3d.Dot(s, h);
            
            if (u < 0.0 || u > 1.0) return false;
            
            Vector3d q = Vector3d.Cross(s, edge1);
            double v = f * Vector3d.Dot(direction, q);
            
            if (v < 0.0 || u + v > 1.0) return false;
            
            double t = f * Vector3d.Dot(edge2, q);
            return t > 1e-10;  // Ray intersects triangle
        }

        /// <summary>
        /// Test if line segment intersects triangle
        /// </summary>
        public bool IntersectsSegment(Vector3d p1, Vector3d p2)
        {
            Vector3d direction = p2 - p1;
            if (!IntersectsRay(p1, direction)) return false;
            
            // Check if intersection point is within segment
            Vector3d edge1 = B - A;
            Vector3d edge2 = C - A;
            Vector3d h = Vector3d.Cross(direction, edge2);
            double a = Vector3d.Dot(edge1, h);
            
            if (Math.Abs(a) < 1e-10) return false;
            
            double f = 1.0 / a;
            Vector3d s = p1 - A;
            double u = f * Vector3d.Dot(s, h);
            
            if (u < 0.0 || u > 1.0) return false;
            
            Vector3d q = Vector3d.Cross(s, edge1);
            double v = f * Vector3d.Dot(direction, q);
            
            if (v < 0.0 || u + v > 1.0) return false;
            
            double t = f * Vector3d.Dot(edge2, q);
            return t >= 0.0 && t <= 1.0;  // Intersection within segment
        }

        /// <summary>
        /// Test if triangle intersects another triangle
        /// </summary>
        public bool IntersectsTriangle(Triangle other)
        {
            // Check if any edge of this triangle intersects the other triangle
            var edges = Edges();
            foreach (var (start, end) in edges)
            {
                if (other.IntersectsSegment(start, end)) return true;
            }
            
            // Check if any edge of other triangle intersects this triangle
            var otherEdges = other.Edges();
            foreach (var (start, end) in otherEdges)
            {
                if (IntersectsSegment(start, end)) return true;
            }
            
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
        /// </summary>
        public bool IsValid()
        {
            return Area() > 1e-10;
        }

        /// <summary>
        /// Check if triangle is equilateral (all sides equal)
        /// </summary>
        public bool IsEquilateral(double tolerance = 0.0001)
        {
            double[] edges = EdgeLengths();
            return Math.Abs(edges[0] - edges[1]) < tolerance &&
                   Math.Abs(edges[1] - edges[2]) < tolerance;
        }

        /// <summary>
        /// Check if triangle is isosceles (2 sides equal)
        /// </summary>
        public bool IsIsosceles(double tolerance = 0.0001)
        {
            double[] edges = EdgeLengths();
            return Math.Abs(edges[0] - edges[1]) < tolerance ||
                   Math.Abs(edges[1] - edges[2]) < tolerance ||
                   Math.Abs(edges[0] - edges[2]) < tolerance;
        }

        /// <summary>
        /// Check if triangle is right-angled using Pythagorean theorem
        /// </summary>
        public bool IsRightAngled(double tolerance = 0.0001)
        {
            double[] edges = EdgeLengths();
            double a = edges[0], b = edges[1], c = edges[2];
            
            // Check all permutations (a² + b² = c²)
            return Math.Abs(a * a + b * b - c * c) < tolerance ||
                   Math.Abs(a * a + c * c - b * b) < tolerance ||
                   Math.Abs(b * b + c * c - a * a) < tolerance;
        }

        #endregion
    }
}
