// TestSingleDoor.cs
// Test single door with proper fitting math and swing configurations
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Test script for single door with proper fitting math.
    /// 
    /// DOOR FITTING MATH:
    /// - Pivot magnetizes to hole cutting edge (border of opening)
    /// - Door sits inside wall thickness with 0.05f gap each side
    /// - Door height = wall height - 0.2f gap (0.1f top + 0.1f bottom)
    /// - Door depth = wall thickness - 0.1f gap
    /// 
    /// SWING MODES:
    /// 1. OUTSWING - Pivot on front face, swings toward viewer (-Z)
    /// 2. INSWING - Pivot on back face, swings into room (+Z)
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
        [SerializeField] private float wallThickness = 0.5f;

        [Header("Door Settings")]
        // Door dimensions calculated to fit wall properly:
        // - Door width = hole width (fills opening)
        // - Door height = wall height (fills entire opening vertically)
        // - Door depth = wall thickness - 0.1f gap (sits inside wall)
        [SerializeField] private float doorWidth = 2f;
        [SerializeField] private float doorHeight = 4f;      // EQUAL to wall height (fills opening)
        [SerializeField] private float doorDepth = 0.4f;     // Wall thickness - 0.1f gap

        [Header("Swing Configuration")]
        [Tooltip("1=OUTSWING (front face), 2=INSWING (back face)")]
        [SerializeField] private SwingMode swingMode = SwingMode.Outswing;

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
        private Transform doorPivot;
        private bool isOpen = false;
        private Keyboard keyboard;

        private float swingAngle = 90f;

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

            if (keyboard[createKey].wasPressedThisFrame)
            {
                CreateWallWithDoor();
            }

            if (keyboard[clearKey].wasPressedThisFrame)
            {
                ClearWall();
            }

            if (keyboard[toggleKey].wasPressedThisFrame && doorObj != null)
            {
                ToggleDoor();
            }

            // Mode switching
            if (keyboard[outswingKey].wasPressedThisFrame)
            {
                swingMode = SwingMode.Outswing;
                Debug.Log("[TestSingleDoor] Mode: OUTSWING (pivot on front face)");
                ClearWall();
            }

            if (keyboard[inswingKey].wasPressedThisFrame)
            {
                swingMode = SwingMode.Inswing;
                Debug.Log("[TestSingleDoor] Mode: INSWING (pivot on back face)");
                ClearWall();
            }
        }

        void CreateWallWithDoor()
        {
            ClearWall();

            Debug.Log($"[TestSingleDoor] Creating wall with {swingMode} door...");

            var wallParent = new GameObject("WallWithDoor");
            wallParent.transform.position = transform.position;
            wallParent.transform.rotation = transform.rotation;

            float holeX = 0f;
            float holeY = 0f;
            float holeZ = 0f;

            CreateWallWithHole(wallParent.transform, holeX, holeY, holeZ);
            CreateDoor(wallParent.transform, holeX, holeY, holeZ);

            wallObj = wallParent;
            isOpen = false;

            Debug.Log($"[TestSingleDoor] ✅ Wall created: {wallWidth}x{wallHeight}x{wallThickness}");
            Debug.Log($"[TestSingleDoor] Door: {doorWidth}x{doorHeight}x{doorDepth}");
            Debug.Log($"[TestSingleDoor] Mode: {swingMode}");
            Debug.Log($"[TestSingleDoor] Controls: SPACE=Toggle, 1=Out, 2=In, X=Clear");
        }

        void CreateWallWithHole(Transform parent, float holeX, float holeY, float holeZ)
        {
            // ============================================================
            // WALL WITH ACTUAL HOLE - Door fits INSIDE the opening
            // ============================================================
            // Wall is split into sections around the door hole:
            //   - Left section: from left edge to door hole
            //   - Right section: from door hole to right edge
            //   - Top section (lintel): above door hole
            //   - Bottom section (sill): below door hole (optional)
            //
            // Door will fit INSIDE this hole with proper clearances
            // ============================================================

            float halfHoleWidth = doorWidth / 2f;
            float halfHoleHeight = wallHeight / 2f; // Hole is full wall height

            // Left wall section (from left edge to door hole)
            var leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWall.name = "Wall_Left";
            leftWall.transform.parent = parent;

            float leftWidth = (wallWidth / 2f) - halfHoleWidth;
            float leftX = holeX - halfHoleWidth - (leftWidth / 2f);

            leftWall.transform.localPosition = new Vector3(leftX, holeY, 0);
            leftWall.transform.localScale = new Vector3(leftWidth, wallHeight, wallThickness);

            // Right wall section (from door hole to right edge)
            var rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWall.name = "Wall_Right";
            rightWall.transform.parent = parent;

            float rightWidth = (wallWidth / 2f) - halfHoleWidth;
            float rightX = holeX + halfHoleWidth + (rightWidth / 2f);

            rightWall.transform.localPosition = new Vector3(rightX, holeY, 0);
            rightWall.transform.localScale = new Vector3(rightWidth, wallHeight, wallThickness);

            // Top wall section (lintel above door) - optional, for visual
            // Commented out since hole is full height
            /*
            var topWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topWall.name = "Wall_Top";
            topWall.transform.parent = parent;
            float topHeight = 0.5f;
            float topY = holeY + wallHeight / 2f + topHeight / 2f;
            topWall.transform.localPosition = new Vector3(holeX, topY, 0);
            topWall.transform.localScale = new Vector3(doorWidth, topHeight, wallThickness);
            */

            if (wallMaterial != null)
            {
                leftWall.GetComponent<Renderer>().sharedMaterial = wallMaterial;
                rightWall.GetComponent<Renderer>().sharedMaterial = wallMaterial;
            }

            Debug.Log($"[TestSingleDoor] Wall hole created: {doorWidth}f wide x {wallHeight}f tall");
            Debug.Log($"[TestSingleDoor] Hole edges: left={holeX - halfHoleWidth:F2}, right={holeX + halfHoleWidth:F2}");
            Debug.Log($"[TestSingleDoor] Wall top Y={wallHeight/2f:F2}, bottom Y={-wallHeight/2f:F2}");
            Debug.Log($"[TestSingleDoor] Door height = wall height (fills entire opening)");
        }

        void CreateDoor(Transform parent, float holeX, float holeY, float holeZ)
        {
            // ============================================================
            // SINGLE DOOR FITTING MATH
            // ============================================================
            // Wall: Width=6f, Height=4f, Thickness=0.5f
            // Hole: Width=doorWidth (2f), Height=wallHeight (4f)
            // Door: Width=2f, Height=3.8f, Depth=0.4f
            //
            // Clearances:
            //   Top/Bottom: 0.1f gap each
            //   Depth: 0.05f gap each side (door sits inside wall)
            //
            // Pivot Position:
            //   OUTSWING: X=left border, Z=front face (+0.05f)
            //   INSWING:  X=left border, Z=back face (-0.05f)
            //
            // Door swings from pivot (hinge on left edge)
            // ============================================================

            float halfDoorWidth = doorWidth / 2f;
            float meetingPoint = holeX;

            // Hole border cutting edges
            float leftBorderX = holeX - halfDoorWidth;   // LEFT edge of opening
            float rightBorderX = holeX + halfDoorWidth;  // RIGHT edge of opening

            // Pivot Z based on swing mode
            float pivotZ;
            float swingDirection;

            switch (swingMode)
            {
                case SwingMode.Outswing:
                    // OUTSWING: Pivot on FRONT face (outside)
                    // Door sits flush with front of wall
                    pivotZ = wallThickness / 2f - doorDepth / 2f;  // +0.05f
                    swingDirection = -1f;  // Swings toward -Z (outward)
                    Debug.Log($"[TestSingleDoor] OUTSWING: Pivot on FRONT face (Z={pivotZ:F3})");
                    break;

                case SwingMode.Inswing:
                    // INSWING: Pivot on BACK face (inside)
                    // Door sits flush with back of wall
                    pivotZ = -wallThickness / 2f + doorDepth / 2f;  // -0.05f
                    swingDirection = 1f;  // Swings toward +Z (inward)
                    Debug.Log($"[TestSingleDoor] INSWING: Pivot on BACK face (Z={pivotZ:F3})");
                    break;

                default:
                    pivotZ = wallThickness / 2f - doorDepth / 2f;
                    swingDirection = -1f;
                    break;
            }

            // Create door pivot at LEFT border of hole
            var pivotObj = new GameObject("DoorPivot");
            pivotObj.transform.parent = parent;
            pivotObj.transform.localPosition = new Vector3(leftBorderX, holeY, pivotZ);

            // Create door panel (hinge on LEFT edge, extends RIGHT)
            doorObj = CreateDoorPanel(doorWidth, doorHeight, doorDepth, extendRight: true);
            doorObj.transform.parent = pivotObj.transform;
            // Offset door vertically so it's centered on pivot
            doorObj.transform.localPosition = new Vector3(0, -doorHeight / 2f, 0);
            doorPivot = pivotObj.transform;

            Debug.Log($"[TestSingleDoor] Door pivot: ({leftBorderX:F2}, 0, {pivotZ:F2})");
            Debug.Log($"[TestSingleDoor] Door extends RIGHT from pivot");
            Debug.Log($"[TestSingleDoor] Door bottom Y={-doorHeight/2f:F2}, top Y={doorHeight/2f:F2}");
            Debug.Log($"[TestSingleDoor] Wall bottom Y={-wallHeight/2f:F2}, top Y={wallHeight/2f:F2}");

            // Apply door material
            if (doorMaterial != null)
            {
                doorObj.GetComponent<Renderer>().sharedMaterial = doorMaterial;
            }
        }

        GameObject CreateDoorPanel(float width, float height, float depth, bool extendRight)
        {
            var doorObj = new GameObject("DoorPanel");
            var meshFilter = doorObj.AddComponent<MeshFilter>();
            var meshRenderer = doorObj.AddComponent<MeshRenderer>();

            var mesh = new Mesh();
            mesh.name = "DoorPanel";

            float halfDepth = depth / 2f;
            Vector3[] vertices = new Vector3[8];

            if (extendRight)
            {
                // Door extends from x=0 to x=+width (hinge at x=0, LEFT edge)
                vertices[0] = new Vector3(0, 0, -halfDepth);
                vertices[1] = new Vector3(width, 0, -halfDepth);
                vertices[2] = new Vector3(width, height, -halfDepth);
                vertices[3] = new Vector3(0, height, -halfDepth);
                vertices[4] = new Vector3(0, 0, halfDepth);
                vertices[5] = new Vector3(width, 0, halfDepth);
                vertices[6] = new Vector3(width, height, halfDepth);
                vertices[7] = new Vector3(0, height, halfDepth);
            }
            else
            {
                // Door extends from x=-width to x=0 (hinge at x=0, RIGHT edge)
                vertices[0] = new Vector3(-width, 0, -halfDepth);
                vertices[1] = new Vector3(0, 0, -halfDepth);
                vertices[2] = new Vector3(0, height, -halfDepth);
                vertices[3] = new Vector3(-width, height, -halfDepth);
                vertices[4] = new Vector3(-width, 0, halfDepth);
                vertices[5] = new Vector3(0, 0, halfDepth);
                vertices[6] = new Vector3(0, height, halfDepth);
                vertices[7] = new Vector3(-width, height, halfDepth);
            }

            int[] triangles = new int[36];
            int i = 0;
            triangles[i++] = 0; triangles[i++] = 1; triangles[i++] = 2;
            triangles[i++] = 0; triangles[i++] = 2; triangles[i++] = 3;
            triangles[i++] = 5; triangles[i++] = 4; triangles[i++] = 7;
            triangles[i++] = 5; triangles[i++] = 7; triangles[i++] = 6;
            triangles[i++] = 4; triangles[i++] = 0; triangles[i++] = 3;
            triangles[i++] = 4; triangles[i++] = 3; triangles[i++] = 7;
            triangles[i++] = 1; triangles[i++] = 5; triangles[i++] = 6;
            triangles[i++] = 1; triangles[i++] = 6; triangles[i++] = 2;
            triangles[i++] = 3; triangles[i++] = 2; triangles[i++] = 6;
            triangles[i++] = 3; triangles[i++] = 6; triangles[i++] = 7;
            triangles[i++] = 4; triangles[i++] = 5; triangles[i++] = 1;
            triangles[i++] = 4; triangles[i++] = 1; triangles[i++] = 0;

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            meshFilter.mesh = mesh;

            var collider = doorObj.AddComponent<BoxCollider>();
            collider.size = new Vector3(width, height, depth);
            collider.center = new Vector3(extendRight ? width / 2 : -width / 2, height / 2, 0);

            return doorObj;
        }

        void ToggleDoor()
        {
            if (doorPivot == null) return;

            isOpen = !isOpen;

            float targetAngle = isOpen ? swingAngle * GetSwingDirection() : 0f;

            Debug.Log($"[TestSingleDoor] {(isOpen ? "Opening" : "Closing")} door to {targetAngle}°");

            var animator = doorPivot.GetComponent<DoorAnimator>();
            if (animator == null)
            {
                animator = doorPivot.gameObject.AddComponent<DoorAnimator>();
            }
            animator.AnimateToAbsoluteAngle(targetAngle, 1f);
        }

        float GetSwingDirection()
        {
            // OUTSWING: swings toward -Z (negative angle)
            // INSWING: swings toward +Z (positive angle)
            return swingMode == SwingMode.Outswing ? -1f : 1f;
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
            Gizmos.DrawWireCube(transform.position, new Vector3(wallWidth, wallHeight, wallThickness));

            // Draw door hole
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, new Vector3(doorWidth, wallHeight, 0.1f));

            // Draw pivot point
            float halfDoorWidth = doorWidth / 2f;
            float pivotZ = swingMode == SwingMode.Outswing 
                ? wallThickness / 2f - doorDepth / 2f 
                : -wallThickness / 2f + doorDepth / 2f;
            
            Vector3 pivotPos = transform.position + new Vector3(-halfDoorWidth, 0, pivotZ);

            Gizmos.color = swingMode == SwingMode.Outswing ? Color.green : Color.blue;
            Gizmos.DrawSphere(pivotPos, 0.15f);

#if UNITY_EDITOR
            string modeLabel = swingMode == SwingMode.Outswing ? "OUTSWING (green)" : "INSWING (blue)";

            UnityEditor.Handles.Label(
                transform.position + Vector3.up * (wallHeight / 2f + 1f),
                $"Wall: {wallWidth}x{wallHeight}x{wallThickness}\nDoor: {doorWidth}x{doorHeight}x{doorDepth}\nMode: {modeLabel}\nPress C: Create | X: Clear | SPACE: Toggle | 1/2: Mode"
            );
#endif
        }

        public enum SwingMode
        {
            Outswing,   // 1: Pivot on front face, swings outward (-Z)
            Inswing     // 2: Pivot on back face, swings inward (+Z)
        }
    }
}
