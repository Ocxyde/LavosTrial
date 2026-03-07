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
        [SerializeField] private float wallHeight = 4f;      // Wall is SHORTER than door
        [SerializeField] private float wallThickness = 0.5f;

        [Header("Door Settings")]
        [SerializeField] private float doorWidth = 4f;      // Total width for BOTH doors
        [SerializeField] private float doorHeight = 6f;     // Door is 50% TALLER than wall
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
            // DOUBLE DOOR MATH - BORDER CUTTING EDGE MAGNETIZATION
            // ============================================================
            // 
            // Pivot is at the HOLE'S BORDER CUTTING EDGE (where wall meets opening)
            // NOT at the center of the doorway!
            //
            // LEFT DOOR:  pivot magnetizes to LEFT border of hole
            // RIGHT DOOR: pivot magnetizes to RIGHT border of hole
            //
            // When CLOSED:
            //   Left door:  FREE edge (left side) meets at center
            //   Right door: FREE edge (right side) meets at center
            //   → Doors meet perfectly at center, hinges on outer edges!
            //
            // ============================================================

            float halfDoorWidth = doorWidth / 2f;  // Width of EACH door panel
            float meetingPoint = holeX;  // Center of doorway (where doors meet when closed)

            // ============================================================
            // HOLE BORDER CUTTING EDGES (where pivots magnetize)
            // ============================================================
            // LEFT border of hole:  where left wall ends, opening begins
            // RIGHT border of hole: where right wall ends, opening begins
            
            float leftBorderX = holeX - halfDoorWidth;    // LEFT edge of opening
            float rightBorderX = holeX + halfDoorWidth;   // RIGHT edge of opening
            
            float frontCuttingEdge_Z = wallThickness / 2f;   // Outside face (+Z)
            float backCuttingEdge_Z = -wallThickness / 2f;   // Inside face (-Z)

            // ============================================================
            // MAGNET POINTS FOR EACH DOOR (based on swing mode)
            // ============================================================
            // LEFT door pivot: at LEFT border of hole
            // RIGHT door pivot: at RIGHT border of hole
            // Z is either front or back cutting edge (IN vs OUT swing)

            float leftPivotX = leftBorderX;
            float rightPivotX = rightBorderX;
            float leftPivotZ, rightPivotZ;

            switch (swingMode)
            {
                case SwingMode.BothOutswing:
                    // Both doors magnetize to FRONT cutting edge (outside face)
                    leftPivotZ = frontCuttingEdge_Z;
                    rightPivotZ = frontCuttingEdge_Z;
                    Debug.Log("[TestDoubleDoor] BOTH OUT: Pivots on FRONT cutting edge (+Z)");
                    break;

                case SwingMode.BothInswing:
                    // Both doors magnetize to BACK cutting edge (inside face)
                    leftPivotZ = backCuttingEdge_Z;
                    rightPivotZ = backCuttingEdge_Z;
                    Debug.Log("[TestDoubleDoor] BOTH IN: Pivots on BACK cutting edge (-Z)");
                    break;

                case SwingMode.LeftInRightOut:
                    // Left IN (back), Right OUT (front)
                    leftPivotZ = backCuttingEdge_Z;
                    rightPivotZ = frontCuttingEdge_Z;
                    Debug.Log("[TestDoubleDoor] LEFT IN / RIGHT OUT: Asymmetric Z");
                    break;

                case SwingMode.LeftOutRightIn:
                    // Left OUT (front), Right IN (back)
                    leftPivotZ = frontCuttingEdge_Z;
                    rightPivotZ = backCuttingEdge_Z;
                    Debug.Log("[TestDoubleDoor] LEFT OUT / RIGHT IN: Asymmetric Z");
                    break;

                default:
                    leftPivotZ = frontCuttingEdge_Z;
                    rightPivotZ = frontCuttingEdge_Z;
                    break;
            }

            // ============================================================
            // LEFT DOOR
            // ============================================================
            // Pivot at LEFT border of hole (hinge on left side of door)
            // Door extends RIGHT from pivot toward center
            // Free edge (right side) meets at center when closed

            var leftPivotObj = new GameObject("LeftDoorPivot");
            leftPivotObj.transform.parent = parent;
            leftPivotObj.transform.localPosition = new Vector3(leftPivotX, holeY, leftPivotZ);

            // Left door: hinge on LEFT side, extends RIGHT toward center
            leftDoorObj = CreateDoorPanel_FlipY(halfDoorWidth, doorHeight, doorDepth, extendLeft: false);
            leftDoorObj.transform.parent = leftPivotObj.transform;
            leftDoorObj.transform.localPosition = Vector3.zero; // Hinge snaps to pivot
            leftDoorPivot = leftPivotObj.transform;

            Debug.Log($"[TestDoubleDoor] LEFT: pivot=({leftPivotX:F2}, 0, {leftPivotZ:F2}) on LEFT border, extends RIGHT");

            // ============================================================
            // RIGHT DOOR
            // ============================================================
            // Pivot at RIGHT border of hole (hinge on right side of door)
            // Door extends LEFT from pivot toward center
            // Free edge (left side) meets at center when closed

            var rightPivotObj = new GameObject("RightDoorPivot");
            rightPivotObj.transform.parent = parent;
            rightPivotObj.transform.localPosition = new Vector3(rightPivotX, holeY, rightPivotZ);

            // Right door: hinge on RIGHT side, extends LEFT toward center
            rightDoorObj = CreateDoorPanel_FlipY(halfDoorWidth, doorHeight, doorDepth, extendLeft: true);
            rightDoorObj.transform.parent = rightPivotObj.transform;
            rightDoorObj.transform.localPosition = Vector3.zero; // Hinge snaps to pivot
            rightDoorPivot = rightPivotObj.transform;

            Debug.Log($"[TestDoubleDoor] RIGHT: pivot=({rightPivotX:F2}, 0, {rightPivotZ:F2}) on RIGHT border, extends LEFT");

            // Apply door material
            if (doorMaterial != null)
            {
                leftDoorObj.GetComponent<Renderer>().sharedMaterial = doorMaterial;
                rightDoorObj.GetComponent<Renderer>().sharedMaterial = doorMaterial;
            }
        }

        GameObject CreateDoorPanel_FlipY(float width, float height, float depth, bool extendLeft)
        {
            var doorObj = new GameObject("DoorPanel");
            var meshFilter = doorObj.AddComponent<MeshFilter>();
            var meshRenderer = doorObj.AddComponent<MeshRenderer>();

            var mesh = new Mesh();
            mesh.name = "DoorPanel";

            float halfDepth = depth / 2f;
            Vector3[] vertices = new Vector3[8];

            if (extendLeft)
            {
                // Left door: extends from x=-width to x=0 (hinge at x=0, back edge is RIGHT side)
                vertices[0] = new Vector3(-width, 0, -halfDepth);
                vertices[1] = new Vector3(0, 0, -halfDepth);
                vertices[2] = new Vector3(0, height, -halfDepth);
                vertices[3] = new Vector3(-width, height, -halfDepth);
                vertices[4] = new Vector3(-width, 0, halfDepth);
                vertices[5] = new Vector3(0, 0, halfDepth);
                vertices[6] = new Vector3(0, height, halfDepth);
                vertices[7] = new Vector3(-width, height, halfDepth);
            }
            else
            {
                // Right door: extends from x=0 to x=+width (hinge at x=0, back edge is LEFT side)
                vertices[0] = new Vector3(0, 0, -halfDepth);
                vertices[1] = new Vector3(width, 0, -halfDepth);
                vertices[2] = new Vector3(width, height, -halfDepth);
                vertices[3] = new Vector3(0, height, -halfDepth);
                vertices[4] = new Vector3(0, 0, halfDepth);
                vertices[5] = new Vector3(width, 0, halfDepth);
                vertices[6] = new Vector3(width, height, halfDepth);
                vertices[7] = new Vector3(0, height, halfDepth);
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
            collider.size = new Vector3(width, height, depth);
            collider.center = new Vector3(extendLeft ? -width / 2 : width / 2, height / 2, 0);

            return doorObj;
        }

        void ToggleDoors()
        {
            if (leftDoorPivot == null || rightDoorPivot == null) return;

            isOpen = !isOpen;

            // ============================================================
            // SWING ANGLES - Hinges on OUTER EDGES (borders of hole)
            // ============================================================
            // LEFT door (hinge on LEFT, extends RIGHT toward center):
            //   +90° = swings OUT (toward -Z, toward viewer)
            //   -90° = swings IN (toward +Z, into room)
            //
            // RIGHT door (hinge on RIGHT, extends LEFT toward center):
            //   -90° = swings OUT (toward -Z, toward viewer)
            //   +90° = swings IN (toward +Z, into room)
            // ============================================================

            float leftTargetAngle, rightTargetAngle;

            switch (swingMode)
            {
                case SwingMode.BothOutswing:
                    // Both swing OUT: Left=+90°, Right=-90° (mirror outward)
                    leftTargetAngle = isOpen ? swingAngle : 0f;
                    rightTargetAngle = isOpen ? -swingAngle : 0f;
                    break;

                case SwingMode.BothInswing:
                    // Both swing IN: Left=-90°, Right=+90° (mirror inward)
                    leftTargetAngle = isOpen ? -swingAngle : 0f;
                    rightTargetAngle = isOpen ? swingAngle : 0f;
                    break;

                case SwingMode.LeftInRightOut:
                    // Left IN (-90°), Right OUT (-90°) - same rotation
                    leftTargetAngle = isOpen ? -swingAngle : 0f;
                    rightTargetAngle = isOpen ? -swingAngle : 0f;
                    break;

                case SwingMode.LeftOutRightIn:
                    // Left OUT (+90°), Right IN (+90°) - same rotation
                    leftTargetAngle = isOpen ? swingAngle : 0f;
                    rightTargetAngle = isOpen ? swingAngle : 0f;
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
