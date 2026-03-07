// TestSingleDoor.cs
// Test single door with configurable pivot position (inswing vs outswing)
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Test script for single door with configurable swing direction.
    /// 
    /// SWING DIRECTION MATH:
    /// - Outswing: Pivot on OUTSIDE edge of wall, door swings away from wall
    /// - Inswing: Pivot on INSIDE edge of wall, door swings into the room
    /// 
    /// Controls:
    /// - C: Create door
    /// - X: Clear door
    /// - SPACE: Toggle open/close
    /// - 1: Switch to OUTSWING mode
    /// - 2: Switch to INSWING mode
    /// </summary>
    public class TestSingleDoor : MonoBehaviour
    {
        [Header("Wall Settings")]
        [SerializeField] private float wallWidth = 6f;
        [SerializeField] private float wallHeight = 4f;
        [SerializeField] private float wallThickness = 0.5f; // Thickness of the wall

        [Header("Door Settings")]
        [SerializeField] private float doorWidth = 2f;
        [SerializeField] private float doorHeight = 3.5f;
        [SerializeField] private float doorDepth = 0.1f; // Door thickness

        [Header("Swing Configuration")]
        [Tooltip("Outswing = pivot on outside edge, door swings away from wall | Inswing = pivot on inside edge, door swings into the room")]
        [SerializeField] private bool isOutswing = true;

        [Header("Visual")]
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material doorMaterial;

        [Header("Test Controls")]
        [SerializeField] private Key createKey = Key.C;
        [SerializeField] private Key clearKey = Key.X;
        [SerializeField] private Key toggleKey = Key.Space;
        [SerializeField] private Key outswingKey = Key.Digit1;
        [SerializeField] private Key inswingKey = Key.Digit2;

        private GameObject wallObj;
        private GameObject doorObj;
        private bool isOpen = false;
        private Keyboard keyboard;

        // Runtime references
        private Transform doorPivot; // The pivot point (hinge)
        private float swingAngle = 90f; // How far door opens

        void Awake()
        {
            keyboard = Keyboard.current;
        }

        void Update()
        {
            if (keyboard == null)
            {
                keyboard = Keyboard.current;
                return;
            }

            // Create/Destroy
            if (keyboard[createKey].wasPressedThisFrame)
            {
                CreateWallWithDoor();
            }

            if (keyboard[clearKey].wasPressedThisFrame)
            {
                ClearWall();
            }

            // Toggle door
            if (keyboard[toggleKey].wasPressedThisFrame && doorObj != null)
            {
                ToggleDoor();
            }

            // Switch swing mode
            if (keyboard[outswingKey].wasPressedThisFrame)
            {
                isOutswing = true;
                Debug.Log("[TestSingleDoor] Switched to OUTSWING mode (pivot on outside edge)");
                ClearWall();
            }

            if (keyboard[inswingKey].wasPressedThisFrame)
            {
                isOutswing = false;
                Debug.Log("[TestSingleDoor] Switched to INSWING mode (pivot on inside edge)");
                ClearWall();
            }
        }

        void CreateWallWithDoor()
        {
            ClearWall();

            Debug.Log($"[TestSingleDoor] Creating wall with { (isOutswing ? "OUTSWING" : "INSWING")} door...");

            // Create wall parent
            var wallParent = new GameObject("WallWithDoor");
            wallParent.transform.position = transform.position;
            wallParent.transform.rotation = transform.rotation;

            // Calculate door hole position (centered in wall)
            float holeX = 0f; // Center of wall
            float holeY = 0f; // Center height
            float holeZ = 0f; // Center of wall thickness

            // Create wall with hole
            CreateWallWithHole(wallParent.transform, holeX, holeY, holeZ);

            // Create door with correct pivot position
            doorObj = CreateDoorWithPivot(
                wallParent.transform,
                holeX, holeY, holeZ,
                isOutswing
            );

            wallObj = wallParent;
            isOpen = false;

            // Log the math
            Debug.Log($"[TestSingleDoor] ✅ Wall created: {wallWidth}x{wallHeight}x{wallThickness}");
            Debug.Log($"[TestSingleDoor] Door hole: {doorWidth}x{doorHeight}");
            Debug.Log($"[TestSingleDoor] Swing mode: {(isOutswing ? "OUTSWING" : "INSWING")}");
            Debug.Log($"[TestSingleDoor] Press SPACE to toggle, 1=Outswing, 2=Inswing, X=Clear");
        }

        void CreateWallWithHole(Transform parent, float holeX, float holeY, float holeZ)
        {
            // Wall is split into two parts around the door hole
            float halfHoleWidth = doorWidth / 2f;

            // Left wall section
            var leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWall.name = "Wall_Left";
            leftWall.transform.parent = parent;

            float leftWidth = (wallWidth / 2f) - halfHoleWidth;
            float leftX = holeX - halfHoleWidth - (leftWidth / 2f);

            leftWall.transform.localPosition = new Vector3(leftX, holeY, 0);
            leftWall.transform.localScale = new Vector3(leftWidth, wallHeight, wallThickness);

            // Right wall section
            var rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWall.name = "Wall_Right";
            rightWall.transform.parent = parent;

            float rightWidth = (wallWidth / 2f) - halfHoleWidth;
            float rightX = holeX + halfHoleWidth + (rightWidth / 2f);

            rightWall.transform.localPosition = new Vector3(rightX, holeY, 0);
            rightWall.transform.localScale = new Vector3(rightWidth, wallHeight, wallThickness);

            // Apply material
            if (wallMaterial != null)
            {
                leftWall.GetComponent<Renderer>().sharedMaterial = wallMaterial;
                rightWall.GetComponent<Renderer>().sharedMaterial = wallMaterial;
            }

            Debug.Log($"[TestSingleDoor] Left wall: width={leftWidth:F2}, pos=({leftX:F2}, 0, 0)");
            Debug.Log($"[TestSingleDoor] Right wall: width={rightWidth:F2}, pos=({rightX:F2}, 0, 0)");
            Debug.Log($"[TestSingleDoor] Door hole edges: left={holeX - halfHoleWidth:F2}, right={holeX + halfHoleWidth:F2}");
        }

        GameObject CreateDoorWithPivot(Transform parent, float holeX, float holeY, float holeZ, bool outswing)
        {
            // ============================================================
            // THE KEY MATH: Pivot Position Based on Swing Direction
            // ============================================================
            //
            // Wall faces +Z direction (front of wall is at +Z)
            // Wall thickness extends from -Z to +Z
            //
            // OUTSWING: Pivot on OUTSIDE edge (front face of wall, +Z side)
            //           Door swings toward +Z (away from wall, toward viewer)
            //
            // INSWING:  Pivot on INSIDE edge (back face of wall, -Z side)
            //           Door swings toward -Z (into the room, away from viewer)
            //
            // ============================================================

            // Door hole edges (where the door sits when closed)
            float halfHoleWidth = doorWidth / 2f;
            float leftEdge = holeX - halfHoleWidth;   // Left edge of hole
            float rightEdge = holeX + halfHoleWidth;  // Right edge of hole

            // Pivot Z position based on swing direction
            float pivotZ;
            float doorPanelX; // Where the door panel sits relative to pivot
            float swingDirection; // +1 or -1 for swing direction

            if (outswing)
            {
                // OUTSWING: Pivot on OUTSIDE edge (front face, +Z side of wall)
                pivotZ = wallThickness / 2f + 0.01f; // Slightly in front of wall

                // Door hinges on LEFT edge, panel extends RIGHT from pivot
                doorPanelX = leftEdge;
                swingDirection = -1f; // Swings toward -Z (outward, toward viewer)

                Debug.Log($"[TestSingleDoor] OUTSWING math:");
                Debug.Log($"  Pivot position: ({leftEdge:F2}, 0, {pivotZ:F2})");
                Debug.Log($"  Pivot is on OUTSIDE edge (front face of wall)");
                Debug.Log($"  Door panel extends RIGHT from pivot");
                Debug.Log($"  Swings toward -Z (outward, toward viewer)");
            }
            else
            {
                // INSWING: Pivot on INSIDE edge (back face, -Z side of wall)
                pivotZ = -wallThickness / 2f - 0.01f; // Slightly behind wall

                // Door hinges on LEFT edge, panel extends RIGHT from pivot
                doorPanelX = leftEdge;
                swingDirection = 1f; // Swings toward +Z (inward, into room)

                Debug.Log($"[TestSingleDoor] INSWING math:");
                Debug.Log($"  Pivot position: ({leftEdge:F2}, 0, {pivotZ:F2})");
                Debug.Log($"  Pivot is on INSIDE edge (back face of wall)");
                Debug.Log($"  Door panel extends RIGHT from pivot");
                Debug.Log($"  Swings toward +Z (inward, into room)");
            }

            // Create door pivot (empty GameObject that rotates)
            var pivotObj = new GameObject("DoorPivot");
            pivotObj.transform.parent = parent;
            pivotObj.transform.localPosition = new Vector3(leftEdge, holeY, pivotZ);

            // Create door panel (the actual mesh)
            doorObj = CreateDoorPanel(doorPanelX, 0, 0);
            doorObj.transform.parent = pivotObj.transform;

            // Store pivot reference for animation
            doorPivot = pivotObj.transform;

            // Apply door material
            if (doorMaterial != null)
            {
                doorObj.GetComponent<Renderer>().sharedMaterial = doorMaterial;
            }

            Debug.Log($"[TestSingleDoor] Door pivot created at local pos: ({leftEdge:F2}, 0, {pivotZ:F2})");
            Debug.Log($"[TestSingleDoor] Door panel offset from pivot: ({doorPanelX:F2}, 0, 0)");

            return doorObj;
        }

        GameObject CreateDoorPanel(float x, float y, float z)
        {
            // Create door mesh (simple box)
            var doorObj = new GameObject("DoorPanel");

            // Add mesh filter and renderer
            var meshFilter = doorObj.AddComponent<MeshFilter>();
            var meshRenderer = doorObj.AddComponent<MeshRenderer>();

            // Create box mesh for door
            var mesh = new Mesh();
            mesh.name = "DoorPanel";

            // Vertices (door extends from x=0 to x=doorWidth)
            float halfDepth = doorDepth / 2f;
            Vector3[] vertices = new Vector3[8];
            vertices[0] = new Vector3(0, 0, -halfDepth);          // bottom-left-front
            vertices[1] = new Vector3(doorWidth, 0, -halfDepth);  // bottom-right-front
            vertices[2] = new Vector3(doorWidth, doorHeight, -halfDepth); // top-right-front
            vertices[3] = new Vector3(0, doorHeight, -halfDepth); // top-left-front
            vertices[4] = new Vector3(0, 0, halfDepth);           // bottom-left-back
            vertices[5] = new Vector3(doorWidth, 0, halfDepth);   // bottom-right-back
            vertices[6] = new Vector3(doorWidth, doorHeight, halfDepth);  // top-right-back
            vertices[7] = new Vector3(0, doorHeight, halfDepth);  // top-left-back

            // Triangles
            int[] triangles = new int[36];
            int i = 0;

            // Front face
            triangles[i++] = 0; triangles[i++] = 1; triangles[i++] = 2;
            triangles[i++] = 0; triangles[i++] = 2; triangles[i++] = 3;

            // Back face
            triangles[i++] = 5; triangles[i++] = 4; triangles[i++] = 7;
            triangles[i++] = 5; triangles[i++] = 7; triangles[i++] = 6;

            // Left face (hinge side)
            triangles[i++] = 4; triangles[i++] = 0; triangles[i++] = 3;
            triangles[i++] = 4; triangles[i++] = 3; triangles[i++] = 7;

            // Right face
            triangles[i++] = 1; triangles[i++] = 5; triangles[i++] = 6;
            triangles[i++] = 1; triangles[i++] = 6; triangles[i++] = 2;

            // Top face
            triangles[i++] = 3; triangles[i++] = 2; triangles[i++] = 6;
            triangles[i++] = 3; triangles[i++] = 6; triangles[i++] = 7;

            // Bottom face
            triangles[i++] = 4; triangles[i++] = 5; triangles[i++] = 1;
            triangles[i++] = 4; triangles[i++] = 1; triangles[i++] = 0;

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;

            // Add collider
            var collider = doorObj.AddComponent<BoxCollider>();
            collider.size = new Vector3(doorWidth, doorHeight, doorDepth);
            collider.center = new Vector3(doorWidth / 2, doorHeight / 2, 0);

            // Position door panel
            doorObj.transform.localPosition = new Vector3(x, y, z);

            return doorObj;
        }

        void ToggleDoor()
        {
            if (doorPivot == null) return;

            isOpen = !isOpen;

            // Calculate target angle based on swing direction
            float targetAngle = isOpen ? (isOutswing ? -swingAngle : swingAngle) : 0f;

            Debug.Log($"[TestSingleDoor] {(isOpen ? "Opening" : "Closing")} door to {targetAngle}°");

            // Animate door pivot
            var animator = doorPivot.GetComponent<DoorAnimator>();
            if (animator == null)
            {
                animator = doorPivot.gameObject.AddComponent<DoorAnimator>();
            }
            animator.AnimateToAbsoluteAngle(targetAngle, 1f);
        }

        void ClearWall()
        {
            if (wallObj != null)
            {
                DestroyImmediate(wallObj);
                wallObj = null;
                doorObj = null;
                doorPivot = null;
                Debug.Log("[TestSingleDoor] Cleared wall and door");
            }
        }

        void OnDrawGizmos()
        {
            // Draw wall outline
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(
                transform.position,
                new Vector3(wallWidth, wallHeight, wallThickness)
            );

            // Draw door hole
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(
                transform.position,
                new Vector3(doorWidth, doorHeight, 0.1f)
            );

            // Draw pivot point indicator
            float halfHoleWidth = doorWidth / 2f;
            float pivotZ = isOutswing ? wallThickness / 2f : -wallThickness / 2f;
            Vector3 pivotPos = transform.position + new Vector3(-halfHoleWidth, 0, pivotZ);

            Gizmos.color = isOutswing ? Color.green : Color.blue;
            Gizmos.DrawSphere(pivotPos, 0.15f);

            // Label
#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * (wallHeight / 2f + 1f),
                $"Wall: {wallWidth}x{wallHeight}x{wallThickness}\nDoor: {doorWidth}x{doorHeight}\nMode: {(isOutswing ? "OUTSWING (green)" : "INSWING (blue)")}\nPress C: Create | X: Clear | SPACE: Toggle | 1/2: Mode"
            );
#endif
        }
    }
}
