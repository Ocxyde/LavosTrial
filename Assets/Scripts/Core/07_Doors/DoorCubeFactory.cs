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
// DoorCubeFactory.cs
// Creates 3D cube doors with pixel art textures
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Generates 3D cube door meshes with pixel art textures.
    /// Replaces old quad-based doors with proper 3D geometry.
    /// </summary>
    public static class DoorCubeFactory
    {
        // Door dimensions (in world units, scaled for pixel art)
        private const float DoorWidth = 1.0f;      // 64 pixels
        private const float DoorHeight = 1.5f;     // 96 pixels
        private const float DoorDepth = 0.125f;    // 8 pixels (thin but visible)
        private const float FrameThickness = 0.0625f; // 4 pixels

        /// <summary>
        /// Creates a single door cube with proper pivot point.
        /// Mesh pivot is at X=0 (left edge when looking at front).
        /// </summary>
        /// <param name="position">World position for the door</param>
        /// <param name="rotation">Base rotation (wall alignment)</param>
        /// <param name="isLeftHanded">If true, hinge is on left; otherwise on right (not used yet)</param>
        /// <param name="parent">Parent transform (usually the door GameObject)</param>
        /// <returns>Door panel GameObject</returns>
        public static GameObject CreateDoorCube(Vector3 position, Quaternion rotation, bool isLeftHanded, Transform parent = null)
        {
            // Create door GameObject
            var doorObj = new GameObject("DoorPanel");
            doorObj.transform.parent = parent;
            doorObj.transform.position = position;
            doorObj.transform.rotation = rotation;

            // Create mesh
            var mesh = CreateDoorMesh();
            var meshFilter = doorObj.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            // Add renderer
            var meshRenderer = doorObj.AddComponent<MeshRenderer>();
            
            // Add collider
            var boxCollider = doorObj.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(DoorWidth, DoorHeight, DoorDepth);
            boxCollider.center = new Vector3(DoorWidth / 2, 0, 0);

            // Pivot is at X=0 (left edge of mesh)
            // Mesh extends from X=0 to X=DoorWidth
            doorObj.transform.localPosition = Vector3.zero;

            return doorObj;
        }

        /// <summary>
        /// Creates a double door with BOTH HINGES AT CENTER.
        /// Uses rotation instead of negative scale to avoid BoxCollider issues.
        /// </summary>
        /// <returns>Array with [leftDoor, rightDoor]</returns>
        public static GameObject[] CreateDoubleDoor(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var doubleDoorObj = new GameObject("DoubleDoor");
            doubleDoorObj.transform.position = position;
            doubleDoorObj.transform.rotation = rotation;
            if (parent != null)
            {
                doubleDoorObj.transform.parent = parent;
            }

            float halfDoorWidth = DoorWidth / 2f;

            // RIGHT DOOR: mesh as-is, extends RIGHT from center, hinge at LEFT edge (center of doorway)
            var rightDoor = CreateDoorCube(
                Vector3.zero,
                Quaternion.identity,
                isLeftHanded: true,
                parent: doubleDoorObj.transform
            );
            rightDoor.name = "RightDoorPanel";
            // Pivot at left edge of mesh, position so pivot is at center of doorway
            // Mesh extends from X=0 to X=DoorWidth, so we need to shift left by half width
            rightDoor.transform.localPosition = new Vector3(-halfDoorWidth, 0, 0);

            // LEFT DOOR: flip mesh so it extends LEFT from center, hinge at RIGHT edge (center of doorway)
            // We create a normal door then flip it
            var leftDoor = CreateDoorCube(
                Vector3.zero,
                Quaternion.identity,
                isLeftHanded: true,
                parent: doubleDoorObj.transform
            );
            leftDoor.name = "LeftDoorPanel";
            // Flip the door so it extends to the left instead of right
            // Scale X by -1 to mirror it, but this causes BoxCollider issues...
            // Instead, rotate 180° around Y and position correctly
            leftDoor.transform.localRotation = Quaternion.Euler(0, 180, 0);
            // After 180° rotation, the left edge (hinge) is now on the right side of the mesh
            // Position so the hinge (now on right) is at center of doorway
            leftDoor.transform.localPosition = new Vector3(halfDoorWidth, 0, 0);

            Debug.Log($"[DoorCubeFactory] Double door created (both hinges at CENTER)");
            Debug.Log($"  Right door: pos={rightDoor.transform.localPosition:F3}, hinge on LEFT, extends RIGHT");
            Debug.Log($"  Left door: pos={leftDoor.transform.localPosition:F3}, rot=180°Y, hinge on RIGHT, extends LEFT");
            Debug.Log($"  Doorway spans from X={-halfDoorWidth:F2} to X={halfDoorWidth:F2}");

            return new[] { leftDoor, rightDoor };
        }

        /// <summary>
        /// Creates a simple box mesh for the door.
        /// Pivot is at X=0 (left edge), mesh extends to X=DoorWidth.
        /// </summary>
        private static Mesh CreateDoorMesh()
        {
            var mesh = new Mesh();
            mesh.name = "DoorCube";

            // Vertices (24 vertices for a box with separate UVs per face)
            Vector3[] vertices = new Vector3[24];
            
            // Front face
            vertices[0] = new Vector3(0, 0, DoorDepth / 2);
            vertices[1] = new Vector3(DoorWidth, 0, DoorDepth / 2);
            vertices[2] = new Vector3(DoorWidth, DoorHeight, DoorDepth / 2);
            vertices[3] = new Vector3(0, DoorHeight, DoorDepth / 2);

            // Back face
            vertices[4] = new Vector3(0, 0, -DoorDepth / 2);
            vertices[5] = new Vector3(DoorWidth, 0, -DoorDepth / 2);
            vertices[6] = new Vector3(DoorWidth, DoorHeight, -DoorDepth / 2);
            vertices[7] = new Vector3(0, DoorHeight, -DoorDepth / 2);

            // Left face
            vertices[8] = new Vector3(0, 0, DoorDepth / 2);
            vertices[9] = new Vector3(0, 0, -DoorDepth / 2);
            vertices[10] = new Vector3(0, DoorHeight, -DoorDepth / 2);
            vertices[11] = new Vector3(0, DoorHeight, DoorDepth / 2);

            // Right face
            vertices[12] = new Vector3(DoorWidth, 0, DoorDepth / 2);
            vertices[13] = new Vector3(DoorWidth, 0, -DoorDepth / 2);
            vertices[14] = new Vector3(DoorWidth, DoorHeight, -DoorDepth / 2);
            vertices[15] = new Vector3(DoorWidth, DoorHeight, DoorDepth / 2);

            // Top face
            vertices[16] = new Vector3(0, DoorHeight, DoorDepth / 2);
            vertices[17] = new Vector3(DoorWidth, DoorHeight, DoorDepth / 2);
            vertices[18] = new Vector3(DoorWidth, DoorHeight, -DoorDepth / 2);
            vertices[19] = new Vector3(0, DoorHeight, -DoorDepth / 2);

            // Bottom face
            vertices[20] = new Vector3(0, 0, DoorDepth / 2);
            vertices[21] = new Vector3(DoorWidth, 0, DoorDepth / 2);
            vertices[22] = new Vector3(DoorWidth, 0, -DoorDepth / 2);
            vertices[23] = new Vector3(0, 0, -DoorDepth / 2);

            // Triangles (36 indices total)
            int[] triangles = new int[36];
            int i = 0;
            
            // Front
            triangles[i++] = 0; triangles[i++] = 1; triangles[i++] = 2;
            triangles[i++] = 0; triangles[i++] = 2; triangles[i++] = 3;
            
            // Back
            triangles[i++] = 5; triangles[i++] = 4; triangles[i++] = 7;
            triangles[i++] = 5; triangles[i++] = 7; triangles[i++] = 6;
            
            // Left
            triangles[i++] = 9; triangles[i++] = 8; triangles[i++] = 11;
            triangles[i++] = 9; triangles[i++] = 11; triangles[i++] = 10;
            
            // Right
            triangles[i++] = 12; triangles[i++] = 13; triangles[i++] = 14;
            triangles[i++] = 12; triangles[i++] = 14; triangles[i++] = 15;
            
            // Top
            triangles[i++] = 17; triangles[i++] = 16; triangles[i++] = 19;
            triangles[i++] = 17; triangles[i++] = 19; triangles[i++] = 18;
            
            // Bottom
            triangles[i++] = 21; triangles[i++] = 20; triangles[i++] = 23;
            triangles[i++] = 21; triangles[i++] = 23; triangles[i++] = 22;

            // UVs for sprite sheet
            Vector2[] uv = new Vector2[24];
            float frameWidth = 64f / 320f;  // 0.2 (first frame of 5)
            float frameHeight = 1f;
            
            // Front face UVs
            uv[0] = new Vector2(0, 0);
            uv[1] = new Vector2(frameWidth, 0);
            uv[2] = new Vector2(frameWidth, frameHeight);
            uv[3] = new Vector2(0, frameHeight);

            // Back face UVs (mirrored)
            uv[4] = new Vector2(frameWidth, 0);
            uv[5] = new Vector2(0, 0);
            uv[6] = new Vector2(0, frameHeight);
            uv[7] = new Vector2(frameWidth, frameHeight);

            // Side/Top/Bottom faces (simple UVs)
            float sideUVWidth = DoorDepth / DoorWidth;
            for (int face = 2; face < 6; face++)
            {
                int baseIdx = face * 4;
                uv[baseIdx] = new Vector2(0, 0);
                uv[baseIdx + 1] = new Vector2(sideUVWidth, 0);
                uv[baseIdx + 2] = new Vector2(sideUVWidth, frameHeight);
                uv[baseIdx + 3] = new Vector2(0, frameHeight);
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>
        /// Creates a material for the door with pixel art texture.
        /// </summary>
        public static Material CreateDoorMaterial(Texture2D spriteSheet)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            var material = new Material(shader);
            material.mainTexture = spriteSheet;
            
            // Pixel art settings
            material.mainTexture.filterMode = FilterMode.Point;
            material.mainTexture.wrapMode = TextureWrapMode.Clamp;
            
            // Color (white to preserve texture colors)
            material.color = Color.white;
            
            return material;
        }

        /// <summary>
        /// Animates door to an absolute angle.
        /// </summary>
        public static void AnimateDoorToAbsolute(GameObject door, float targetAbsoluteAngle, float duration)
        {
            var animator = door.GetComponent<DoorAnimator>();
            if (animator == null)
            {
                animator = door.AddComponent<DoorAnimator>();
            }
            animator.AnimateToAbsoluteAngle(targetAbsoluteAngle, duration);
        }

        /// <summary>
        /// Animates door opening (relative angle).
        /// </summary>
        public static void AnimateDoor(GameObject door, float targetAngle, float duration)
        {
            var animator = door.GetComponent<DoorAnimator>();
            if (animator == null)
            {
                animator = door.AddComponent<DoorAnimator>();
            }
            animator.AnimateToAngle(targetAngle, duration);
        }

        /// <summary>
        /// Animates double door panels (opposite directions).
        /// </summary>
        /// <param name="leftDoor">Left door panel (hinge on left, opens +angle)</param>
        /// <param name="rightDoor">Right door panel (hinge on right, opens -angle)</param>
        /// <param name="targetAngle">Target angle magnitude (e.g., 90)</param>
        /// <param name="duration">Animation duration in seconds</param>
        public static void AnimateDoubleDoor(GameObject leftDoor, GameObject rightDoor, float targetAngle, float duration)
        {
            // Left door opens positive (counterclockwise)
            AnimateDoor(leftDoor, targetAngle, duration);
            
            // Right door opens negative (clockwise)
            AnimateDoor(rightDoor, -targetAngle, duration);
            
            Debug.Log($"[DoorCubeFactory] Double door animation: Left={targetAngle}°, Right={-targetAngle}°");
        }
    }
}
