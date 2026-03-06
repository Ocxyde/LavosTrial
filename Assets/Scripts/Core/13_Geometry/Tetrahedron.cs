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
// Tetrahedron.cs
// Tetrahedron geometry - Pure mathematical representation
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// GEOMETRY: Pure tetrahedron math (not Unity-dependent)
// Location: Assets/Scripts/Core/13_Geometry/
//
// FUTURE IMPLEMENTATION - Placeholder for tetrahedron system

using System;

namespace Code.Lavos.Geometry
{
    /// <summary>
    /// Tetrahedron - Pure mathematical representation of a 3D tetrahedron.
    /// 
    /// A tetrahedron is a polyhedron with:
    /// - 4 vertices
    /// - 6 edges
    /// - 4 triangular faces
    /// 
    /// This is a PURE MATH class - no Unity dependencies.
    /// For Unity mesh representation, use TetrahedronMesh in 10_Mesh/
    /// 
    /// FUTURE FEATURES (TODO):
    /// - Vertex/edge/face calculations
    /// - Volume and surface area
    /// - Centroid calculation
    /// - Insphere/circumsphere
    /// - Collision detection
    /// - Subdivision algorithms
    /// </summary>
    public struct Tetrahedron
    {
        #region Fields

        /// <summary>
        /// The 4 vertices of the tetrahedron
        /// </summary>
        public Vector3d A, B, C, D;

        #endregion

        #region Properties

        /// <summary>
        /// Get all vertices as array
        /// </summary>
        public Vector3d[] Vertices => new[] { A, B, C, D };

        /// <summary>
        /// Number of vertices (always 4)
        /// </summary>
        public const int VertexCount = 4;

        /// <summary>
        /// Number of edges (always 6)
        /// </summary>
        public const int EdgeCount = 6;

        /// <summary>
        /// Number of faces (always 4)
        /// </summary>
        public const int FaceCount = 4;

        #endregion

        #region Constructor

        /// <summary>
        /// Create a tetrahedron from 4 vertices
        /// </summary>
        public Tetrahedron(Vector3d a, Vector3d b, Vector3d c, Vector3d d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        /// <summary>
        /// Create a regular tetrahedron centered at origin with given edge length
        /// </summary>
        public static Tetrahedron CreateRegular(double edgeLength)
        {
            // Regular tetrahedron vertices
            double h = edgeLength * Math.Sqrt(6) / 3;  // Height
            double r = edgeLength * Math.Sqrt(3) / 3;  // Base circumradius

            return new Tetrahedron(
                new Vector3d(0, h / 4, r),           // Top vertex
                new Vector3d(-edgeLength / 2, -h * 3 / 4, -r / 2),  // Base vertex 1
                new Vector3d(edgeLength / 2, -h * 3 / 4, -r / 2),   // Base vertex 2
                new Vector3d(0, -h * 3 / 4, r)       // Base vertex 3
            );
        }

        #endregion

        #region Calculations (TODO - Future Implementation)

        /// <summary>
        /// Calculate volume of tetrahedron
        /// TODO: Implement using scalar triple product
        /// </summary>
        public double Volume()
        {
            // V = |(AB · (AC × AD))| / 6
            // TODO: Implement
            return 0;
        }

        /// <summary>
        /// Calculate surface area (sum of 4 triangular faces)
        /// TODO: Implement
        /// </summary>
        public double SurfaceArea()
        {
            // TODO: Calculate area of each face and sum
            return 0;
        }

        /// <summary>
        /// Calculate centroid (center of mass)
        /// TODO: Implement
        /// </summary>
        public Vector3d Centroid()
        {
            // Centroid = (A + B + C + D) / 4
            // TODO: Implement
            return new Vector3d();
        }

        /// <summary>
        /// Check if point is inside tetrahedron
        /// TODO: Implement using barycentric coordinates
        /// </summary>
        public bool ContainsPoint(Vector3d point)
        {
            // TODO: Implement
            return false;
        }

        #endregion

        #region Operators

        public static Tetrahedron operator +(Tetrahedron t, Vector3d v)
        {
            return new Tetrahedron(t.A + v, t.B + v, t.C + v, t.D + v);
        }

        public static Tetrahedron operator -(Tetrahedron t, Vector3d v)
        {
            return new Tetrahedron(t.A - v, t.B - v, t.C - v, t.D - v);
        }

        #endregion
    }

    /// <summary>
    /// Double-precision 3D vector for pure math calculations
    /// (Avoids Unity dependency in geometry code)
    /// </summary>
    public struct Vector3d
    {
        public double X, Y, Z;

        public Vector3d(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3d operator +(Vector3d a, Vector3d b)
            => new Vector3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Vector3d operator -(Vector3d a, Vector3d b)
            => new Vector3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Vector3d operator *(Vector3d v, double s)
            => new Vector3d(v.X * s, v.Y * s, v.Z * s);
    }
}
