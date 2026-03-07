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
        /// </summary>
        /// <param name="position">World position for the door</param>
        /// <param name="rotation">Base rotation (wall alignment)</param>
        /// <param name="isLeftHanded">If true, hinge is on left; otherwise on right</param>
        /// <param name="parent">Parent transform (usually the door GameObject)</param>
        /// <returns>Door panel GameObject</returns>
        public static GameObject CreateDoorCube(Vector3 position, Quaternion rotation, bool isLeftHanded, Transform parent = null)
        {
            // Create door GameObject
            var doorObj = new GameObject("DoorPanel");
            doorObj.transform.parent = parent;
            doorObj.transform.position = position;
            doorObj.transform.rotation = rotation;

            // Create cube mesh
            var mesh = CreateDoorMesh();
            var meshFilter = doorObj.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            // Add renderer
            var meshRenderer = doorObj.AddComponent<MeshRenderer>();
            
            // Add collider
            var boxCollider = doorObj.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(DoorWidth, DoorHeight, DoorDepth);
            
            // Adjust collider position to match pivot
            float colliderX = isLeftHanded ? DoorWidth / 2 : -DoorWidth / 2;
            boxCollider.center = new Vector3(colliderX, 0, 0);

            // Set pivot point (hinge side)
            float pivotX = isLeftHanded ? -DoorWidth / 2 + FrameThickness : DoorWidth / 2 - FrameThickness;
            doorObj.transform.localPosition = new Vector3(pivotX, 0, 0);

            return doorObj;
        }

        /// <summary>
        /// Creates a double door (two panels).
        /// </summary>
        public static GameObject[] CreateDoubleDoor(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            // Create parent for both doors
            var doubleDoorObj = new GameObject("DoubleDoor");
            doubleDoorObj.transform.position = position;
            doubleDoorObj.transform.rotation = rotation;
            if (parent != null)
            {
                doubleDoorObj.transform.parent = parent;
            }

            // Left door (hinge on left)
            var leftDoor = CreateDoorCube(
                Vector3.zero,
                Quaternion.identity,
                isLeftHanded: true,
                parent: doubleDoorObj.transform
            );
            leftDoor.name = "LeftDoorPanel";
            leftDoor.transform.localPosition = new Vector3(-DoorWidth / 2, 0, 0);

            // Right door (hinge on right)
            var rightDoor = CreateDoorCube(
                Vector3.zero,
                Quaternion.identity,
                isLeftHanded: false,
                parent: doubleDoorObj.transform
            );
            rightDoor.name = "RightDoorPanel";
            rightDoor.transform.localPosition = new Vector3(DoorWidth / 2, 0, 0);

            return new[] { leftDoor, rightDoor };
        }

        /// <summary>
        /// Creates a simple box mesh for the door.
        /// </summary>
        private static Mesh CreateDoorMesh()
        {
            var mesh = new Mesh();
            mesh.name = "DoorCube";

            // Vertices (16 vertices for a box with separate UVs per face)
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

            // Triangles (6 faces x 2 triangles x 3 vertices = 36 indices)
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

            // UVs for sprite sheet (5 frames across)
            // Each face gets appropriate UV mapping
            Vector2[] uv = new Vector2[24];
            
            // For pixel art, we'll use the first frame (closed door)
            // UV coordinates for sprite sheet (320x96, 5 frames of 64 pixels each)
            float frameWidth = 64f / 320f;  // 0.2
            float frameHeight = 1f;          // Full height
            
            // Front face UVs (main door face with pixel art)
            uv[0] = new Vector2(0, 0);
            uv[1] = new Vector2(frameWidth, 0);
            uv[2] = new Vector2(frameWidth, frameHeight);
            uv[3] = new Vector2(0, frameHeight);

            // Back face UVs (mirrored)
            uv[4] = new Vector2(frameWidth, 0);
            uv[5] = new Vector2(0, 0);
            uv[6] = new Vector2(0, frameHeight);
            uv[7] = new Vector2(frameWidth, frameHeight);

            // Side faces (simple gradient or wood pattern)
            float sideUVWidth = DoorDepth / DoorWidth; // Very narrow
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
        /// Animates door opening.
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
    }

    /// <summary>
    /// Simple door animation component.
    /// </summary>
    public class DoorAnimator : MonoBehaviour
    {
        private float startAngle = 0f;
        private float targetAngle = 90f;
        private float duration = 1f;
        private float elapsed = 0f;
        private bool isAnimating = false;

        public void AnimateToAngle(float angle, float dur)
        {
            startAngle = transform.localEulerAngles.y;
            targetAngle = angle;
            duration = dur;
            elapsed = 0f;
            isAnimating = true;
        }

        void Update()
        {
            if (!isAnimating) return;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            // Smooth step for natural motion
            t = Mathf.SmoothStep(0, 1, t);
            
            float currentAngle = Mathf.Lerp(startAngle, targetAngle, t);
            transform.localEulerAngles = new Vector3(0, currentAngle, 0);

            if (t >= 1f)
            {
                isAnimating = false;
            }
        }
    }
}
