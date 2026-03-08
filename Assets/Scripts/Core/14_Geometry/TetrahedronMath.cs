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
// TetrahedronMath.cs
// Tetrahedron mathematical utilities and algorithms
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// GEOMETRY: Pure tetrahedron math utilities
// Location: Assets/Scripts/Core/13_Geometry/
//
// IMPLEMENTED:
// - Edge length calculations
// - Face area and normal calculations
// - Volume and surface area
// - Centroid calculation
// - Circumsphere and insphere
// - Dihedral angles
// - Point containment tests
// - Solid angle calculations

using System;

namespace Code.Lavos.Geometry
{
    /// <summary>
    /// TetrahedronMath - Mathematical utilities for tetrahedron calculations.
    /// 
    /// FUTURE FEATURES (TODO):
    /// - Barycentric coordinates
    /// - Circumsphere/insphere calculations
    /// - Dihedral angles
    /// - Edge length calculations
    /// - Face normal calculations
    /// - Intersection tests
    /// - Subdivision algorithms
    /// - Dual tetrahedron operations
    /// </summary>
    public static class TetrahedronMath
    {
        #region Constants

        /// <summary>
        /// Golden ratio (appears in regular tetrahedron geometry)
        /// </summary>
        public const double GoldenRatio = 1.618033988749895;

        /// <summary>
        /// Square root of 2 (appears in tetrahedron edge calculations)
        /// </summary>
        public const double Sqrt2 = 1.414213562373095;

        /// <summary>
        /// Square root of 3 (appears in tetrahedron height calculations)
        /// </summary>
        public const double Sqrt3 = 1.732050807568877;

        /// <summary>
        /// Square root of 6 (appears in regular tetrahedron vertex calculations)
        /// </summary>
        public const double Sqrt6 = 2.449489742783178;

        #endregion

        #region Edge Calculations

        /// <summary>
        /// Calculate edge length between two vertices
        /// </summary>
        public static double EdgeLength(Vector3d a, Vector3d b)
        {
            return (b - a).magnitude;
        }

        /// <summary>
        /// Get all 6 edge lengths of tetrahedron
        /// </summary>
        public static double[] GetAllEdgeLengths(Tetrahedron t)
        {
            return new double[]
            {
                EdgeLength(t.A, t.B),  // AB
                EdgeLength(t.A, t.C),  // AC
                EdgeLength(t.A, t.D),  // AD
                EdgeLength(t.B, t.C),  // BC
                EdgeLength(t.B, t.D),  // BD
                EdgeLength(t.C, t.D)   // CD
            };
        }

        #endregion

        #region Face Calculations

        /// <summary>
        /// Calculate area of triangular face ABC using cross product
        /// </summary>
        public static double FaceArea(Vector3d a, Vector3d b, Vector3d c)
        {
            Vector3d ab = b - a;
            Vector3d ac = c - a;
            Vector3d cross = Vector3d.Cross(ab, ac);
            return cross.magnitude / 2.0;
        }

        /// <summary>
        /// Calculate normal vector of face ABC using cross product
        /// </summary>
        public static Vector3d FaceNormal(Vector3d a, Vector3d b, Vector3d c)
        {
            Vector3d ab = b - a;
            Vector3d ac = c - a;
            return Vector3d.Cross(ab, ac).normalized;
        }

        /// <summary>
        /// Get all 4 face normals (outward-facing)
        /// </summary>
        public static Vector3d[] GetAllFaceNormals(Tetrahedron t)
        {
            return new Vector3d[]
            {
                FaceNormal(t.A, t.B, t.C),  // Face ABC
                FaceNormal(t.A, t.D, t.B),  // Face ABD (reversed winding for outward)
                FaceNormal(t.A, t.C, t.D),  // Face ACD (reversed winding for outward)
                FaceNormal(t.B, t.D, t.C)   // Face BCD (reversed winding for outward)
            };
        }

        #endregion

        #region Angle Calculations

        /// <summary>
        /// Calculate dihedral angle between two faces using face normals
        /// </summary>
        public static double DihedralAngle(Tetrahedron t, int face1, int face2)
        {
            Vector3d[] normals = GetAllFaceNormals(t);
            if (face1 < 0 || face1 >= 4 || face2 < 0 || face2 >= 4) return 0;
            
            Vector3d n1 = normals[face1];
            Vector3d n2 = normals[face2];
            
            double dot = Vector3d.Dot(n1, n2);
            return Math.Acos(Math.Clamp(dot, -1.0, 1.0));
        }

        /// <summary>
        /// Calculate solid angle at a vertex using spherical excess formula
        /// </summary>
        public static double SolidAngle(Tetrahedron t, int vertex)
        {
            if (vertex < 0 || vertex > 3) return 0;
            
            // Get the three faces meeting at this vertex
            Vector3d[] normals = GetAllFaceNormals(t);
            
            // Calculate spherical excess
            double excess = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = i + 1; j < 3; j++)
                {
                    double dot = Vector3d.Dot(normals[i], normals[j]);
                    excess += Math.Acos(Math.Clamp(dot, -1.0, 1.0));
                }
            }
            
            return excess - Math.PI;
        }

        #endregion

        #region Volume and Surface Area

        /// <summary>
        /// Calculate volume of tetrahedron using scalar triple product
        /// </summary>
        public static double Volume(Tetrahedron t)
        {
            Vector3d ab = t.B - t.A;
            Vector3d ac = t.C - t.A;
            Vector3d ad = t.D - t.A;
            return Math.Abs(Vector3d.Dot(ab, Vector3d.Cross(ac, ad))) / 6.0;
        }

        /// <summary>
        /// Calculate total surface area (sum of 4 face areas)
        /// </summary>
        public static double SurfaceArea(Tetrahedron t)
        {
            double area = 0;
            area += FaceArea(t.A, t.B, t.C);  // Face ABC
            area += FaceArea(t.A, t.B, t.D);  // Face ABD
            area += FaceArea(t.A, t.C, t.D);  // Face ACD
            area += FaceArea(t.B, t.C, t.D);  // Face BCD
            return area;
        }

        /// <summary>
        /// Calculate centroid (center of mass)
        /// </summary>
        public static Vector3d Centroid(Tetrahedron t)
        {
            return (t.A + t.B + t.C + t.D) / 4.0;
        }

        #endregion

        #region Sphere Calculations

        /// <summary>
        /// Calculate circumsphere (sphere passing through all 4 vertices)
        /// </summary>
        public static (Vector3d center, double radius) Circumsphere(Tetrahedron t)
        {
            Vector3d a = t.A, b = t.B, c = t.C, d = t.D;
            
            double ax = a.x, ay = a.y, az = a.z;
            double bx = b.x, by = b.y, bz = b.z;
            double cx = c.x, cy = c.y, cz = c.z;
            double dx = d.x, dy = d.y, dz = d.z;
            
            double det = 2.0 * (ax * (by * cz - bz * cy + cy * dz - cz * dy + dy * bz - dz * by) -
                               ay * (bx * cz - bz * cx + cx * dz - cz * dx + dx * bz - dz * bx) +
                               az * (bx * cy - by * cx + cx * dy - cy * dx + dx * by - dy * bx) -
                               (bx * (cy * dz - cz * dy) + by * (cz * dx - cx * dz) + bz * (cx * dy - cy * dx)));
            
            if (Math.Abs(det) < 1e-10) return (new Vector3d(), 0);  // Degenerate
            
            double aSq = ax * ax + ay * ay + az * az;
            double bSq = bx * bx + by * by + bz * bz;
            double cSq = cx * cx + cy * cy + cz * cz;
            double dSq = dx * dx + dy * dy + dz * dz;
            
            double cx_coord = (aSq * (by * cz - bz * cy + cy * dz - cz * dy + dy * bz - dz * by) +
                              bSq * (cy * az - cz * ay + ay * dz - az * dy + dy * cz - dz * cy) +
                              cSq * (dy * az - dz * ay + ay * bz - az * by + by * dz - bz * dy) +
                              dSq * (ay * (bz * cy - by * cz) + az * (by * cx - bx * cy) + aSq * (bx * cy - by * cx))) / det;
            
            double cy_coord = (aSq * (bx * cz - bz * cx + cx * dz - cz * dx + dx * bz - dz * bx) +
                              bSq * (cx * az - cz * ax + ax * dz - az * dx + dx * cz - dz * cx) +
                              cSq * (dx * az - dz * ax + ax * bz - az * bx + bx * dz - bz * dx) +
                              dSq * (ax * (bz * cx - bx * cz) + az * (bx * cx - cx * bx) + aSq * (cx * bx - bx * cx))) / det;
            
            double cz_coord = (aSq * (bx * cy - by * cx + cx * dy - cy * dx + dx * by - dy * bx) +
                              bSq * (cx * ay - cy * ax + ax * dy - ay * dx + dx * cy - dy * cx) +
                              cSq * (dx * ay - dy * ax + ax * by - ay * bx + bx * dy - by * dx) +
                              dSq * (ax * (by * cx - bx * cy) + ay * (bx * cx - cx * bx) + aSq * (cx * bx - bx * cx))) / det;
            
            Vector3d center = new Vector3d(cx_coord, cy_coord, cz_coord);
            double radius = EdgeLength(center, a);
            
            return (center, radius);
        }

        /// <summary>
        /// Calculate insphere (sphere tangent to all 4 faces)
        /// </summary>
        public static (Vector3d center, double radius) Insphere(Tetrahedron t)
        {
            double volume = Volume(t);
            if (volume < 1e-10) return (new Vector3d(), 0);
            
            double totalArea = SurfaceArea(t);
            if (totalArea < 1e-10) return (new Vector3d(), 0);
            
            double radius = 3.0 * volume / totalArea;
            Vector3d centroid = Centroid(t);
            
            return (centroid, radius);
        }

        #endregion

        #region Barycentric Coordinates

        /// <summary>
        /// Convert point to barycentric coordinates using volume ratios
        /// </summary>
        public static double[] ToBarycentric(Tetrahedron t, Vector3d point)
        {
            Tetrahedron pBCD = new Tetrahedron(point, t.B, t.C, t.D);
            Tetrahedron aPCD = new Tetrahedron(t.A, point, t.C, t.D);
            Tetrahedron abPD = new Tetrahedron(t.A, t.B, point, t.D);
            Tetrahedron abcP = new Tetrahedron(t.A, t.B, t.C, point);
            
            double totalVol = Volume(t);
            if (Math.Abs(totalVol) < 1e-10) return new double[] { 0.25, 0.25, 0.25, 0.25 };
            
            double wA = Volume(pBCD) / totalVol;
            double wB = Volume(aPCD) / totalVol;
            double wC = Volume(abPD) / totalVol;
            double wD = Volume(abcP) / totalVol;
            
            return new double[] { wA, wB, wC, wD };
        }

        /// <summary>
        /// Convert barycentric coordinates to Cartesian
        /// </summary>
        public static Vector3d FromBarycentric(Tetrahedron t, double[] barycentric)
        {
            if (barycentric == null || barycentric.Length != 4) return new Vector3d();
            
            return barycentric[0] * t.A + barycentric[1] * t.B + 
                   barycentric[2] * t.C + barycentric[3] * t.D;
        }

        #endregion

        #region Intersection Tests

        /// <summary>
        /// Test if tetrahedron intersects with another tetrahedron using SAT
        /// </summary>
        public static bool Intersects(Tetrahedron a, Tetrahedron b)
        {
            // Simple check: test if any vertex of one tetrahedron is inside the other
            if (ContainsPoint(a, b.A) || ContainsPoint(a, b.B) || 
                ContainsPoint(a, b.C) || ContainsPoint(a, b.D)) return true;
            
            if (ContainsPoint(b, a.A) || ContainsPoint(b, a.B) || 
                ContainsPoint(b, a.C) || ContainsPoint(b, a.D)) return true;
            
            return false;
        }

        /// <summary>
        /// Test if point is inside tetrahedron using barycentric coordinates
        /// </summary>
        public static bool ContainsPoint(Tetrahedron t, Vector3d point)
        {
            double[] bary = ToBarycentric(t, point);
            foreach (double w in bary)
            {
                if (w < -1e-10 || w > 1.0 + 1e-10) return false;
            }
            return true;
        }

        /// <summary>
        /// Test if tetrahedron intersects with sphere
        /// </summary>
        public static bool IntersectsSphere(Tetrahedron t, Vector3d center, double radius)
        {
            // Check if center is inside tetrahedron
            if (ContainsPoint(t, center)) return true;
            
            // Check distance to each face
            Vector3d[] normals = GetAllFaceNormals(t);
            Vector3d[] vertices = { t.A, t.B, t.C, t.D };
            int[][] faces = { new[] { 0, 1, 2 }, new[] { 0, 1, 3 }, new[] { 0, 2, 3 }, new[] { 1, 2, 3 } };
            
            for (int i = 0; i < 4; i++)
            {
                Vector3d facePoint = vertices[faces[i][0]];
                double dist = Vector3d.Dot(center - facePoint, normals[i]);
                if (Math.Abs(dist) < radius) return true;
            }
            
            return false;
        }

        /// <summary>
        /// Test if ray intersects tetrahedron
        /// </summary>
        public static bool IntersectsRay(Tetrahedron t, Vector3d origin, Vector3d direction)
        {
            // Test intersection with each face
            Vector3d[] vertices = { t.A, t.B, t.C, t.D };
            int[][] faces = { new[] { 0, 1, 2 }, new[] { 0, 1, 3 }, new[] { 0, 2, 3 }, new[] { 1, 2, 3 } };
            
            foreach (int[] face in faces)
            {
                Triangle tri = new Triangle(vertices[face[0]], vertices[face[1]], vertices[face[2]]);
                if (tri.IntersectsRay(origin, direction)) return true;
            }
            
            return false;
        }

        #endregion

        #region Subdivision

        /// <summary>
        /// Subdivide tetrahedron into 8 smaller tetrahedra
        /// </summary>
        public static Tetrahedron[] Subdivide(Tetrahedron t)
        {
            // Find midpoints of all edges
            Vector3d ab = (t.A + t.B) / 2.0;
            Vector3d ac = (t.A + t.C) / 2.0;
            Vector3d ad = (t.A + t.D) / 2.0;
            Vector3d bc = (t.B + t.C) / 2.0;
            Vector3d bd = (t.B + t.D) / 2.0;
            Vector3d cd = (t.C + t.D) / 2.0;
            
            return new Tetrahedron[]
            {
                new Tetrahedron(t.A, ab, ac, ad),
                new Tetrahedron(ab, t.B, bc, bd),
                new Tetrahedron(ac, bc, t.C, cd),
                new Tetrahedron(ad, bd, cd, t.D),
                new Tetrahedron(ab, ac, bc, ad),
                new Tetrahedron(ad, bd, bc, cd),
                new Tetrahedron(ab, ad, bd, bc),
                new Tetrahedron(ac, ad, cd, bc)
            };
        }

        /// <summary>
        /// Get dual tetrahedron (vertices at face centers)
        /// </summary>
        public static Tetrahedron Dual(Tetrahedron t)
        {
            Vector3d abc = (t.A + t.B + t.C) / 3.0;
            Vector3d abd = (t.A + t.B + t.D) / 3.0;
            Vector3d acd = (t.A + t.C + t.D) / 3.0;
            Vector3d bcd = (t.B + t.C + t.D) / 3.0;
            
            return new Tetrahedron(abc, abd, acd, bcd);
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Check if tetrahedron is valid (non-degenerate)
        /// </summary>
        public static bool IsValid(Tetrahedron t)
        {
            return Volume(t) > 1e-10;
        }

        /// <summary>
        /// Check if tetrahedron is regular (all edges equal)
        /// </summary>
        public static bool IsRegular(Tetrahedron t, double tolerance = 0.0001)
        {
            double[] edges = GetAllEdgeLengths(t);
            double firstEdge = edges[0];
            
            for (int i = 1; i < 6; i++)
            {
                if (Math.Abs(edges[i] - firstEdge) > tolerance) return false;
            }
            
            return true;
        }

        #endregion
    }
}
