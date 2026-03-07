// TestDoubleDoor.cs
// Test double door with configurable swing directions (inswing vs outswing)
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Test script for double doors with configurable swing directions.
    /// 
    /// SWING DIRECTION MATH:
    /// - Outswing: Pivot on OUTSIDE edge (front face, +Z), door swings away from wall
    /// - Inswing: Pivot on INSIDE edge (back face, -Z), door swings into the room
    /// 
    /// DOUBLE DOOR CONFIGURATIONS:
    /// 1. Both OUTSWING - both doors swing outward (toward viewer)
    /// 2. Both INSWING - both doors swing inward (into room)
    /// 3. Left IN / Right OUT - asymmetric
    /// 4. Left OUT / Right IN - asymmetric
    /// 
    /// Controls:
    /// - C: Create door
    /// - X: Clear door
    /// - SPACE: Toggle open/close
    /// - 1: Both OUTSWING
    /// - 2: Both INSWING
    /// - 3: Left IN / Right OUT
    /// - 4: Left OUT / Right IN
    /// </summary>
    public class TestDoubleDoor : MonoBehaviour
    {
        [Header("Wall Settings")]
        [SerializeField] private float wallWidth = 10f;
        [SerializeField] private float wallHeight = 6f;
        [SerializeField] private float wallThickness = 0.5f;

        [Header("Door Settings")]
        [SerializeField] private float doorWidth = 4f;      // Total width for BOTH doors
        [SerializeField] private float doorHeight = 5.5f;
        [SerializeField] private float doorDepth = 0.1f;

        [Header("Swing Configuration")]
        [Tooltip("1=Both Out, 2=Both In, 3=Left In/Right Out, 4=Left Out/Right In")]
        [SerializeField] private SwingMode swingMode = SwingMode.BothOutswing;

        [Header("Visual")]
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material doorMaterial;

        [Header("Test Controls")]
        [SerializeField] private Key createKey = Key.C;
        [SerializeField] private Key clearKey = Key.X;
        [SerializeField] private Key toggleKey = Key.Space;
        [SerializeField] private Key mode1Key = Key.Digit1;
        [SerializeField] private Key mode2Key = Key.Digit2;
        [SerializeField] private Key mode3Key = Key.Digit3;
        [SerializeField] private Key mode4Key = Key.Digit4;

        private GameObject wallObj;
        private GameObject leftDoorObj;
        private GameObject rightDoorObj;
        private Transform leftDoorPivot;
        private Transform rightDoorPivot;
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
                CreateWallWithDoubleDoor();
            }

            if (keyboard[clearKey].wasPressedThisFrame)
            {
                ClearWall();
            }

            if (keyboard[toggleKey].wasPressedThisFrame && leftDoorObj != null && rightDoorObj != null)
            {
                ToggleDoors();
            }

            // Mode switching
            if (keyboard[mode1Key].wasPressedThisFrame)
            {
                swingMode = SwingMode.BothOutswing;
                Debug.Log("[TestDoubleDoor] Mode: BOTH OUTSWING");
                ClearWall();
            }

            if (keyboard[mode2Key].wasPressedThisFrame)
            {
                swingMode = SwingMode.BothInswing;
                Debug.Log("[TestDoubleDoor] Mode: BOTH INSWING");
                ClearWall();
            }

            if (keyboard[mode3Key].wasPressedThisFrame)
            {
                swingMode = SwingMode.LeftInRightOut;
                Debug.Log("[TestDoubleDoor] Mode: LEFT IN / RIGHT OUT");
                ClearWall();
            }

            if (keyboard[mode4Key].wasPressedThisFrame)
            {
                swingMode = SwingMode.LeftOutRightIn;
                Debug.Log("[TestDoubleDoor] Mode: LEFT OUT / RIGHT IN");
                ClearWall();
            }
        }

        void CreateWallWithDoubleDoor()
        {
            ClearWall();

            Debug.Log($"[TestDoubleDoor] Creating wall with DOUBLE DOOR (Mode: {swingMode})...");

            var wallParent = new GameObject("WallWithDoubleDoor");
            wallParent.transform.position = transform.position;
            wallParent.transform.rotation = transform.rotation;

            float holeX = 0f;
            float holeY = 0f;
            float holeZ = 0f;

            CreateWallWithHole(wallParent.transform, holeX, holeY, holeZ);
            CreateDoubleDoors(wallParent.transform, holeX, holeY, holeZ);

            wallObj = wallParent;
            isOpen = false;

            Debug.Log($"[TestDoubleDoor] ✅ Wall created: {wallWidth}x{wallHeight}x{wallThickness}");
            Debug.Log($"[TestDoubleDoor] Double door: {doorWidth}x{doorHeight} (each panel: {doorWidth/2}x{doorHeight})");
            Debug.Log($"[TestDoubleDoor] Mode: {swingMode}");
            Debug.Log($"[TestDoubleDoor] Press SPACE to toggle, 1-4: Mode, X: Clear");
        }

        void CreateWallWithHole(Transform parent, float holeX, float holeY, float holeZ)
        {
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

            if (wallMaterial != null)
            {
                leftWall.GetComponent<Renderer>().sharedMaterial = wallMaterial;
                rightWall.GetComponent<Renderer>().sharedMaterial = wallMaterial;
            }

            Debug.Log($"[TestDoubleDoor] Wall hole: {doorWidth}x{doorHeight}, centered at (0, 0, 0)");
        }

        void CreateDoubleDoors(Transform parent, float holeX, float holeY, float holeZ)
        {
            // ============================================================
            // DOUBLE DOOR MATH - SIMPLE PIVOT MAGNETIZATION
            // ============================================================
            // 
            // The cutting edge of the hole = Pivot point (magnetized)
            // 
            // LEFT DOOR:  pivot snaps to RIGHT edge of hole (center meeting point)
            // RIGHT DOOR: pivot snaps to LEFT edge of hole (center meeting point)
            // 
            // Then just shift on Z for IN/OUT swing:
            //   OUT: pivot Z = +wallThickness/2 (front face)
            //   IN:  pivot Z = -wallThickness/2 (back face)
            //
            // ============================================================

            float halfDoorWidth = doorWidth / 2f;  // Width of EACH door panel
            float meetingPoint = holeX;  // Center of doorway (where doors meet)
            float holeRightEdge = holeX + halfDoorWidth;  // Right edge of hole
            float holeLeftEdge = holeX - halfDoorWidth;   // Left edge of hole

            // ============================================================
            // PIVOT POSITIONS - Magnetized to hole edges, shifted on Z
            // ============================================================

            float leftPivotX, leftPivotZ, rightPivotX, rightPivotZ;

            switch (swingMode)
            {
                case SwingMode.BothOutswing:
                    // Both pivots on OUTSIDE edge (front face, +Z)
                    leftPivotX = meetingPoint;   // Left door hinges at center
                    rightPivotX = meetingPoint;  // Right door hinges at center
                    leftPivotZ = wallThickness / 2f + 0.01f;
                    rightPivotZ = wallThickness / 2f + 0.01f;
                    Debug.Log("[TestDoubleDoor] BOTH OUT: Pivots magnetized to center, shifted to FRONT face (+Z)");
                    break;

                case SwingMode.BothInswing:
                    // Both pivots on INSIDE edge (back face, -Z)
                    leftPivotX = meetingPoint;
                    rightPivotX = meetingPoint;
                    leftPivotZ = -wallThickness / 2f - 0.01f;
                    rightPivotZ = -wallThickness / 2f - 0.01f;
                    Debug.Log("[TestDoubleDoor] BOTH IN: Pivots magnetized to center, shifted to BACK face (-Z)");
                    break;

                case SwingMode.LeftInRightOut:
                    // Left IN (back), Right OUT (front)
                    leftPivotX = meetingPoint;
                    rightPivotX = meetingPoint;
                    leftPivotZ = -wallThickness / 2f - 0.01f;  // Left on back face
                    rightPivotZ = wallThickness / 2f + 0.01f;  // Right on front face
                    Debug.Log("[TestDoubleDoor] LEFT IN / RIGHT OUT: Asymmetric pivot Z");
                    break;

                case SwingMode.LeftOutRightIn:
                    // Left OUT (front), Right IN (back)
                    leftPivotX = meetingPoint;
                    rightPivotX = meetingPoint;
                    leftPivotZ = wallThickness / 2f + 0.01f;   // Left on front face
                    rightPivotZ = -wallThickness / 2f - 0.01f; // Right on back face
                    Debug.Log("[TestDoubleDoor] LEFT OUT / RIGHT IN: Asymmetric pivot Z");
                    break;

                default:
                    leftPivotX = meetingPoint;
                    rightPivotX = meetingPoint;
                    leftPivotZ = wallThickness / 2f;
                    rightPivotZ = wallThickness / 2f;
                    break;
            }

            // ============================================================
            // LEFT DOOR - Hinges at meeting point, extends LEFT
            // ============================================================

            var leftPivotObj = new GameObject("LeftDoorPivot");
            leftPivotObj.transform.parent = parent;
            leftPivotObj.transform.localPosition = new Vector3(leftPivotX, holeY, leftPivotZ);

            // Door panel extends LEFT from pivot (negative X direction)
            leftDoorObj = CreateDoorPanel(-halfDoorWidth, 0, 0, flipY: true);
            leftDoorObj.transform.parent = leftPivotObj.transform;
            leftDoorPivot = leftPivotObj.transform;

            Debug.Log($"[TestDoubleDoor] LEFT pivot: ({leftPivotX:F2}, 0, {leftPivotZ:F2}) → magnetized to hole edge");
            Debug.Log($"[TestDoubleDoor] LEFT panel: extends {halfDoorWidth:F2} units LEFT from pivot");

            // ============================================================
            // RIGHT DOOR - Hinges at meeting point, extends RIGHT
            // ============================================================

            var rightPivotObj = new GameObject("RightDoorPivot");
            rightPivotObj.transform.parent = parent;
            rightPivotObj.transform.localPosition = new Vector3(rightPivotX, holeY, rightPivotZ);

            // Door panel extends RIGHT from pivot (positive X direction)
            rightDoorObj = CreateDoorPanel(0, 0, 0, flipY: false);
            rightDoorObj.transform.parent = rightPivotObj.transform;
            rightDoorPivot = rightPivotObj.transform;

            Debug.Log($"[TestDoubleDoor] RIGHT pivot: ({rightPivotX:F2}, 0, {rightPivotZ:F2}) → magnetized to hole edge");
            Debug.Log($"[TestDoubleDoor] RIGHT panel: extends {halfDoorWidth:F2} units RIGHT from pivot");

            // Apply door material
            if (doorMaterial != null)
            {
                leftDoorObj.GetComponent<Renderer>().sharedMaterial = doorMaterial;
                rightDoorObj.GetComponent<Renderer>().sharedMaterial = doorMaterial;
            }
        }

        GameObject CreateDoorPanel(float x, float y, float z, bool flipY)
        {
            var doorObj = new GameObject("DoorPanel");
            var meshFilter = doorObj.AddComponent<MeshFilter>();
            var meshRenderer = doorObj.AddComponent<MeshRenderer>();

            float panelWidth = doorWidth / 2f; // Each door is half the total width

            var mesh = new Mesh();
            mesh.name = "DoorPanel";

            float halfDepth = doorDepth / 2f;
            Vector3[] vertices = new Vector3[8];

            if (flipY)
            {
                // Left door: extends from x=-panelWidth to x=0 (hinge at x=0)
                vertices[0] = new Vector3(-panelWidth, 0, -halfDepth);
                vertices[1] = new Vector3(0, 0, -halfDepth);
                vertices[2] = new Vector3(0, doorHeight, -halfDepth);
                vertices[3] = new Vector3(-panelWidth, doorHeight, -halfDepth);
                vertices[4] = new Vector3(-panelWidth, 0, halfDepth);
                vertices[5] = new Vector3(0, 0, halfDepth);
                vertices[6] = new Vector3(0, doorHeight, halfDepth);
                vertices[7] = new Vector3(-panelWidth, doorHeight, halfDepth);
            }
            else
            {
                // Right door: extends from x=0 to x=+panelWidth (hinge at x=0)
                vertices[0] = new Vector3(0, 0, -halfDepth);
                vertices[1] = new Vector3(panelWidth, 0, -halfDepth);
                vertices[2] = new Vector3(panelWidth, doorHeight, -halfDepth);
                vertices[3] = new Vector3(0, doorHeight, -halfDepth);
                vertices[4] = new Vector3(0, 0, halfDepth);
                vertices[5] = new Vector3(panelWidth, 0, halfDepth);
                vertices[6] = new Vector3(panelWidth, doorHeight, halfDepth);
                vertices[7] = new Vector3(0, doorHeight, halfDepth);
            }

            int[] triangles = new int[36];
            int i = 0;

            // Front face
            triangles[i++] = 0; triangles[i++] = 1; triangles[i++] = 2;
            triangles[i++] = 0; triangles[i++] = 2; triangles[i++] = 3;

            // Back face
            triangles[i++] = 5; triangles[i++] = 4; triangles[i++] = 7;
            triangles[i++] = 5; triangles[i++] = 7; triangles[i++] = 6;

            // Left face
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

            var collider = doorObj.AddComponent<BoxCollider>();
            collider.size = new Vector3(panelWidth, doorHeight, doorDepth);
            collider.center = new Vector3(flipY ? -panelWidth / 2 : panelWidth / 2, doorHeight / 2, 0);

            doorObj.transform.localPosition = new Vector3(x, y, z);

            return doorObj;
        }

        void ToggleDoors()
        {
            if (leftDoorPivot == null || rightDoorPivot == null) return;

            isOpen = !isOpen;

            // Calculate target angles based on swing mode
            float leftTargetAngle, rightTargetAngle;

            switch (swingMode)
            {
                case SwingMode.BothOutswing:
                    // Both swing OUT: Left rotates CW (-), Right rotates CCW (+)
                    leftTargetAngle = isOpen ? -swingAngle : 0f;
                    rightTargetAngle = isOpen ? swingAngle : 0f;
                    break;

                case SwingMode.BothInswing:
                    // Both swing IN: Left rotates CCW (+), Right rotates CW (-)
                    leftTargetAngle = isOpen ? swingAngle : 0f;
                    rightTargetAngle = isOpen ? -swingAngle : 0f;
                    break;

                case SwingMode.LeftInRightOut:
                    // Left IN (+), Right OUT (+) - both rotate CCW
                    leftTargetAngle = isOpen ? swingAngle : 0f;
                    rightTargetAngle = isOpen ? swingAngle : 0f;
                    break;

                case SwingMode.LeftOutRightIn:
                    // Left OUT (-), Right IN (-) - both rotate CW
                    leftTargetAngle = isOpen ? -swingAngle : 0f;
                    rightTargetAngle = isOpen ? -swingAngle : 0f;
                    break;

                default:
                    leftTargetAngle = 0f;
                    rightTargetAngle = 0f;
                    break;
            }

            Debug.Log($"[TestDoubleDoor] {(isOpen ? "Opening" : "Closing")} doors...");
            Debug.Log($"  Left: {leftTargetAngle}°, Right: {rightTargetAngle}°");

            // Animate left door
            var leftAnimator = leftDoorPivot.GetComponent<DoorAnimator>();
            if (leftAnimator == null)
            {
                leftAnimator = leftDoorPivot.gameObject.AddComponent<DoorAnimator>();
            }
            leftAnimator.AnimateToAbsoluteAngle(leftTargetAngle, 1f);

            // Animate right door
            var rightAnimator = rightDoorPivot.GetComponent<DoorAnimator>();
            if (rightAnimator == null)
            {
                rightAnimator = rightDoorPivot.gameObject.AddComponent<DoorAnimator>();
            }
            rightAnimator.AnimateToAbsoluteAngle(rightTargetAngle, 1f);
        }

        void ClearWall()
        {
            if (wallObj != null)
            {
                DestroyImmediate(wallObj);
                wallObj = null;
                leftDoorObj = null;
                rightDoorObj = null;
                leftDoorPivot = null;
                rightDoorPivot = null;
                Debug.Log("[TestDoubleDoor] Cleared wall and doors");
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

            // Draw pivot points
            float halfDoorWidth = doorWidth / 2f;
            float pivotZ;

            switch (swingMode)
            {
                case SwingMode.BothOutswing:
                    pivotZ = wallThickness / 2f;
                    Gizmos.color = Color.green; // Green = outswing
                    break;
                case SwingMode.BothInswing:
                    pivotZ = -wallThickness / 2f;
                    Gizmos.color = Color.blue; // Blue = inswing
                    break;
                case SwingMode.LeftInRightOut:
                    Gizmos.color = Color.cyan; // Mixed
                    pivotZ = 0;
                    break;
                case SwingMode.LeftOutRightIn:
                    Gizmos.color = Color.magenta; // Mixed
                    pivotZ = 0;
                    break;
                default:
                    pivotZ = 0;
                    Gizmos.color = Color.white;
                    break;
            }

            // Left pivot (at center, extends left)
            Vector3 leftPivotPos = transform.position + new Vector3(0, 0, pivotZ);
            Gizmos.DrawSphere(leftPivotPos, 0.15f);

            // Right pivot (at center, extends right)
            Vector3 rightPivotPos = transform.position + new Vector3(0, 0, pivotZ);
            Gizmos.DrawSphere(rightPivotPos, 0.15f);

#if UNITY_EDITOR
            string modeLabel = swingMode switch
            {
                SwingMode.BothOutswing => "BOTH OUT (green)",
                SwingMode.BothInswing => "BOTH IN (blue)",
                SwingMode.LeftInRightOut => "LEFT IN / RIGHT OUT (cyan)",
                SwingMode.LeftOutRightIn => "LEFT OUT / RIGHT IN (magenta)",
                _ => "Unknown"
            };

            UnityEditor.Handles.Label(
                transform.position + Vector3.up * (wallHeight / 2f + 1f),
                $"Wall: {wallWidth}x{wallHeight}x{wallThickness}\nDouble Door: {doorWidth}x{doorHeight}\nMode: {modeLabel}\nPress C: Create | X: Clear | SPACE: Toggle | 1-4: Mode"
            );
#endif
        }

        public enum SwingMode
        {
            BothOutswing,
            BothInswing,
            LeftInRightOut,
            LeftOutRightIn
        }
    }
}
