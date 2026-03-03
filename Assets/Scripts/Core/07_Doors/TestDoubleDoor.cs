// TestDoubleDoor.cs
// Test double door with 6 swing modes including center post and auto-close
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Test script for double doors with 6 configurations:
    /// 1. Both OUTSWING - hinges on outer edges, both swing out
    /// 2. Both INSWING - hinges on outer edges, both swing in
    /// 3. Left IN / Right OUT - asymmetric swing
    /// 4. Left OUT / Right IN - asymmetric swing
    /// 5. CENTER POST - vertical mullion between doors (idle row)
    /// 6. AUTO-CLOSE ON IMPACT - closes on collision, reopens after delay
    /// 
    /// Controls:
    /// - C: Create door
    /// - X: Clear door
    /// - SPACE: Toggle open/close
    /// - 1-6: Switch modes
    /// - I: Simulate impact (mode 6 only)
    /// </summary>
    public class TestDoubleDoor : MonoBehaviour
    {
        [Header("Wall Settings")]
        [SerializeField] private float wallWidth = 10f;
        [SerializeField] private float wallHeight = 4f;
        [SerializeField] private float wallThickness = 0.5f;

        [Header("Door Settings")]
        // Door dimensions calculated to fit wall properly:
        // - Total door width = hole width (doors meet at center with no gap)
        // - Door height = wall height (fills entire opening vertically)
        // - Door depth = wall thickness - 0.1f gap (door sits inside wall)
        [SerializeField] private float doorWidth = 4f;       // Total width for BOTH panels (each = 2f wide)
        [SerializeField] private float doorHeight = 4f;      // EQUAL to wall height (fills entire opening)
        [SerializeField] private float doorDepth = 0.4f;     // Wall thickness - 0.1f gap (sits inside wall)

        [Header("Swing Configuration")]
        [Tooltip("1-4: Standard, 5: Center Post, 6: Auto-Close, 7: Double-Action (saloon)")]
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
        [SerializeField] private Key mode5Key = Key.Digit5;  // 5th mode: Center post
        [SerializeField] private Key mode6Key = Key.Digit6;  // 6th mode: Auto-close on impact
        [SerializeField] private Key mode7Key = Key.Digit7;  // 7th mode: Double-action (saloon)
        [SerializeField] private Key simulateImpactKey = Key.I; // Simulate collision/impact

        private GameObject wallObj;
        private GameObject leftDoorObj;
        private GameObject rightDoorObj;
        private GameObject centerPostObj; // For mode 5
        private Transform leftDoorPivot;
        private Transform rightDoorPivot;
        private bool isOpen = false;
        private Keyboard keyboard;

        // Mode 6: Auto-close on impact
        private bool isImpactMode = false;
        private float autoCloseTimer = 0f;
        private float autoCloseDelay = 3f; // Seconds before reopening

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

            if (keyboard[mode5Key].wasPressedThisFrame)
            {
                swingMode = SwingMode.CenterPost;
                Debug.Log("[TestDoubleDoor] Mode: CENTER POST (idle row between doors)");
                ClearWall();
            }

            if (keyboard[mode6Key].wasPressedThisFrame)
            {
                swingMode = SwingMode.AutoCloseOnImpact;
                Debug.Log("[TestDoubleDoor] Mode: AUTO-CLOSE ON IMPACT (press I to simulate impact)");
                ClearWall();
            }

            if (keyboard[mode7Key].wasPressedThisFrame)
            {
                swingMode = SwingMode.DoubleAction;
                Debug.Log("[TestDoubleDoor] Mode: DOUBLE-ACTION (saloon/kitchen doors - swing both ways)");
                ClearWall();
            }

            // Simulate impact for mode 6
            if (swingMode == SwingMode.AutoCloseOnImpact && keyboard[simulateImpactKey].wasPressedThisFrame)
            {
                SimulateImpact();
            }

            // Mode 6: Check for auto-close after impact
            if (isImpactMode && Time.time >= autoCloseTimer)
            {
                isOpen = false;
                isImpactMode = false;
                AnimateDoors();
                Debug.Log("[TestDoubleDoor] Auto-closing doors after entrance delay");
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
            Debug.Log($"[TestDoubleDoor] Controls: SPACE=Toggle, 1-7=Mode, I=Impact, X=Clear");
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

            Debug.Log($"[TestDoubleDoor] Wall: left section at X={leftX:F2}, right section at X={rightX:F2}");
            Debug.Log($"[TestDoubleDoor] Wall hole: {doorWidth}f wide x {wallHeight}f tall, centered at (0, 0, 0)");
            Debug.Log($"[TestDoubleDoor] Wall top Y={wallHeight/2f:F2}, bottom Y={-wallHeight/2f:F2}");
        }

        void CreateDoubleDoors(Transform parent, float holeX, float holeY, float holeZ)
        {
            // ============================================================
            // DOOR FITTING MATH - From Scratch
            // ============================================================
            // Wall dimensions: Width: 10f, Height: 4f, Thickness: 0.5f
            // Hole dimensions: Width: doorWidth (4f), Height: wallHeight (4f)
            // Door panel: Each 2f wide x 3.8f tall x 0.4f thick
            // Clearances: Top/Bottom: 0.1f, Depth: 0.05f each side
            // ============================================================

            float halfDoorWidth = doorWidth / 2f;
            float meetingPoint = holeX;

            // Hole border cutting edges (where pivots magnetize)
            float holeHalfWidth = doorWidth / 2f;
            float leftBorderX = holeX - holeHalfWidth;
            float rightBorderX = holeX + holeHalfWidth;

            // Pivot Z - door sits flush with wall face
            float frontCuttingEdge_Z = wallThickness / 2f - doorDepth / 2f;
            float backCuttingEdge_Z = -wallThickness / 2f + doorDepth / 2f;

            // Determine pivot positions based on mode
            float leftPivotX = leftBorderX;
            float rightPivotX = rightBorderX;
            float leftPivotZ, rightPivotZ;

            switch (swingMode)
            {
                case SwingMode.BothOutswing:
                    leftPivotZ = frontCuttingEdge_Z;
                    rightPivotZ = frontCuttingEdge_Z;
                    Debug.Log("[TestDoubleDoor] BOTH OUT: Pivots on FRONT cutting edge (+Z)");
                    break;
                case SwingMode.BothInswing:
                    leftPivotZ = backCuttingEdge_Z;
                    rightPivotZ = backCuttingEdge_Z;
                    Debug.Log("[TestDoubleDoor] BOTH IN: Pivots on BACK cutting edge (-Z)");
                    break;
                case SwingMode.LeftInRightOut:
                    leftPivotZ = backCuttingEdge_Z;
                    rightPivotZ = frontCuttingEdge_Z;
                    Debug.Log("[TestDoubleDoor] LEFT IN / RIGHT OUT: Asymmetric Z");
                    break;
                case SwingMode.LeftOutRightIn:
                    leftPivotZ = frontCuttingEdge_Z;
                    rightPivotZ = backCuttingEdge_Z;
                    Debug.Log("[TestDoubleDoor] LEFT OUT / RIGHT IN: Asymmetric Z");
                    break;
                case SwingMode.CenterPost:
                case SwingMode.AutoCloseOnImpact:
                    leftPivotZ = frontCuttingEdge_Z;
                    rightPivotZ = frontCuttingEdge_Z;
                    Debug.Log("[TestDoubleDoor] Default: Pivots on FRONT cutting edge");
                    break;
                case SwingMode.DoubleAction:
                    // Double-action: pivots in CENTER of wall thickness for free swing both ways
                    leftPivotZ = 0f;  // Center of wall thickness
                    rightPivotZ = 0f;
                    Debug.Log("[TestDoubleDoor] DOUBLE-ACTION: Pivots at wall CENTER (Z=0) for free swing");
                    break;
                default:
                    leftPivotZ = frontCuttingEdge_Z;
                    rightPivotZ = frontCuttingEdge_Z;
                    break;
            }

            // LEFT DOOR - Pivot at LEFT border, extends RIGHT toward center
            var leftPivotObj = new GameObject("LeftDoorPivot");
            leftPivotObj.transform.parent = parent;
            leftPivotObj.transform.localPosition = new Vector3(leftPivotX, holeY, leftPivotZ);

            leftDoorObj = CreateDoorPanel_FlipY(halfDoorWidth, doorHeight, doorDepth, extendLeft: false);
            leftDoorObj.transform.parent = leftPivotObj.transform;
            // Offset door vertically so it's centered on pivot
            leftDoorObj.transform.localPosition = new Vector3(0, -doorHeight / 2f, 0);
            leftDoorPivot = leftPivotObj.transform;

            Debug.Log($"[TestDoubleDoor] LEFT: pivot=({leftPivotX:F2}, 0, {leftPivotZ:F2})");
            Debug.Log($"[TestDoubleDoor] LEFT door bottom Y={-doorHeight/2f:F2}, top Y={doorHeight/2f:F2}");

            // RIGHT DOOR - Pivot at RIGHT border, extends LEFT toward center
            var rightPivotObj = new GameObject("RightDoorPivot");
            rightPivotObj.transform.parent = parent;
            rightPivotObj.transform.localPosition = new Vector3(rightPivotX, holeY, rightPivotZ);

            rightDoorObj = CreateDoorPanel_FlipY(halfDoorWidth, doorHeight, doorDepth, extendLeft: true);
            rightDoorObj.transform.parent = rightPivotObj.transform;
            // Offset door vertically so it's centered on pivot
            rightDoorObj.transform.localPosition = new Vector3(0, -doorHeight / 2f, 0);
            rightDoorPivot = rightPivotObj.transform;

            Debug.Log($"[TestDoubleDoor] RIGHT: pivot=({rightPivotX:F2}, 0, {rightPivotZ:F2})");
            Debug.Log($"[TestDoubleDoor] RIGHT door bottom Y={-doorHeight/2f:F2}, top Y={doorHeight/2f:F2}");

            // MODE 5: CENTER POST (Idle Row)
            if (swingMode == SwingMode.CenterPost)
            {
                CreateCenterPost(parent, meetingPoint, holeY, wallHeight, doorDepth);
            }

            // Apply door material
            if (doorMaterial != null)
            {
                leftDoorObj.GetComponent<Renderer>().sharedMaterial = doorMaterial;
                rightDoorObj.GetComponent<Renderer>().sharedMaterial = doorMaterial;
            }
        }

        void CreateCenterPost(Transform parent, float x, float y, float height, float depth)
        {
            // Create vertical post/mullion in center between doors (idle row)
            var postObj = new GameObject("CenterPost");
            postObj.transform.parent = parent;

            float postWidth = 0.2f;
            float postDepth = depth + 0.05f;

            postObj.transform.localPosition = new Vector3(x, y, 0);

            var meshFilter = postObj.AddComponent<MeshFilter>();
            var meshRenderer = postObj.AddComponent<MeshRenderer>();
            var collider = postObj.AddComponent<BoxCollider>();

            var mesh = new Mesh();
            mesh.name = "CenterPost";

            float halfWidth = postWidth / 2f;
            float halfDepth = postDepth / 2f;
            float halfHeight = height / 2f;

            Vector3[] vertices = new Vector3[8];
            vertices[0] = new Vector3(-halfWidth, -halfHeight, -halfDepth);
            vertices[1] = new Vector3(halfWidth, -halfHeight, -halfDepth);
            vertices[2] = new Vector3(halfWidth, halfHeight, -halfDepth);
            vertices[3] = new Vector3(-halfWidth, halfHeight, -halfDepth);
            vertices[4] = new Vector3(-halfWidth, -halfHeight, halfDepth);
            vertices[5] = new Vector3(halfWidth, -halfHeight, halfDepth);
            vertices[6] = new Vector3(halfWidth, halfHeight, halfDepth);
            vertices[7] = new Vector3(-halfWidth, halfHeight, halfDepth);

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

            meshFilter.mesh = mesh;
            collider.size = new Vector3(postWidth, height, postDepth);

            centerPostObj = postObj;

            Debug.Log($"[TestDoubleDoor] Center post created: {postWidth}f x {height}f x {postDepth}f");
        }

        void SimulateImpact()
        {
            // Mode 6: Simulate collision/impact that triggers OPEN
            // Doors open on impact, then auto-close after delay
            Debug.Log("[TestDoubleDoor] IMPACT DETECTED! Opening doors...");

            isOpen = true;
            AnimateDoors();

            isImpactMode = true;
            autoCloseTimer = Time.time + autoCloseDelay;

            Debug.Log($"[TestDoubleDoor] Will auto-close in {autoCloseDelay} seconds...");
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
                // Door extends from x=-width to x=0 (hinge at x=0)
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
                // Door extends from x=0 to x=+width (hinge at x=0)
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
            collider.center = new Vector3(extendLeft ? -width / 2 : width / 2, height / 2, 0);

            return doorObj;
        }

        void ToggleDoors()
        {
            if (leftDoorPivot == null || rightDoorPivot == null) return;

            isOpen = !isOpen;
            AnimateDoors();
        }

        void AnimateDoors()
        {
            float leftTargetAngle, rightTargetAngle;

            switch (swingMode)
            {
                case SwingMode.BothOutswing:
                    leftTargetAngle = isOpen ? swingAngle : 0f;
                    rightTargetAngle = isOpen ? -swingAngle : 0f;
                    break;
                case SwingMode.BothInswing:
                    leftTargetAngle = isOpen ? -swingAngle : 0f;
                    rightTargetAngle = isOpen ? swingAngle : 0f;
                    break;
                case SwingMode.LeftInRightOut:
                    leftTargetAngle = isOpen ? -swingAngle : 0f;
                    rightTargetAngle = isOpen ? -swingAngle : 0f;
                    break;
                case SwingMode.LeftOutRightIn:
                    leftTargetAngle = isOpen ? swingAngle : 0f;
                    rightTargetAngle = isOpen ? swingAngle : 0f;
                    break;
                case SwingMode.CenterPost:
                    leftTargetAngle = isOpen ? swingAngle : 0f;
                    rightTargetAngle = isOpen ? -swingAngle : 0f;
                    break;
                case SwingMode.AutoCloseOnImpact:
                    leftTargetAngle = isOpen ? swingAngle : 0f;
                    rightTargetAngle = isOpen ? -swingAngle : 0f;
                    break;
                case SwingMode.DoubleAction:
                    // Double-action: doors swing BOTH ways (like saloon/kitchen doors)
                    // Toggle between +90° (out) and -90° (in) on each press
                    if (isOpen)
                    {
                        // First press: swing OUT
                        leftTargetAngle = swingAngle;
                        rightTargetAngle = -swingAngle;
                    }
                    else
                    {
                        // Second press: swing IN (opposite direction)
                        leftTargetAngle = -swingAngle;
                        rightTargetAngle = swingAngle;
                    }
                    Debug.Log($"[TestDoubleDoor] DOUBLE-ACTION: Toggle between OUT and IN");
                    break;
                default:
                    leftTargetAngle = 0f;
                    rightTargetAngle = 0f;
                    break;
            }

            Debug.Log($"[TestDoubleDoor] {(isOpen ? "Opening" : "Closing")} doors...");
            Debug.Log($"  Left: {leftTargetAngle}°, Right: {rightTargetAngle}°");

            var leftAnimator = leftDoorPivot.GetComponent<DoorAnimator>();
            if (leftAnimator == null)
            {
                leftAnimator = leftDoorPivot.gameObject.AddComponent<DoorAnimator>();
            }
            leftAnimator.AnimateToAbsoluteAngle(leftTargetAngle, 1f);

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
                centerPostObj = null;
                leftDoorPivot = null;
                rightDoorPivot = null;
                Debug.Log("[TestDoubleDoor] Cleared wall and doors");
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(wallWidth, wallHeight, wallThickness));

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, new Vector3(doorWidth, wallHeight, 0.1f));

#if UNITY_EDITOR
            string modeLabel = swingMode switch
            {
                SwingMode.BothOutswing => "BOTH OUT",
                SwingMode.BothInswing => "BOTH IN",
                SwingMode.LeftInRightOut => "LEFT IN / RIGHT OUT",
                SwingMode.LeftOutRightIn => "LEFT OUT / RIGHT IN",
                SwingMode.CenterPost => "CENTER POST (idle row)",
                SwingMode.AutoCloseOnImpact => "AUTO-CLOSE ON IMPACT",
                SwingMode.DoubleAction => "DOUBLE-ACTION (saloon/kitchen)",
                _ => "Unknown"
            };

            UnityEditor.Handles.Label(
                transform.position + Vector3.up * (wallHeight / 2f + 1f),
                $"Wall: {wallWidth}x{wallHeight}x{wallThickness}\nDouble Door: {doorWidth}x{doorHeight}\nMode: {modeLabel}\nPress C: Create | X: Clear | SPACE: Toggle | 1-6: Mode | I: Impact"
            );
#endif
        }

        public enum SwingMode
        {
            BothOutswing,       // 1: Both OUT (hinges on outer edges)
            BothInswing,        // 2: Both IN (hinges on outer edges)
            LeftInRightOut,     // 3: Left IN / Right OUT (hinges on outer edges)
            LeftOutRightIn,     // 4: Left OUT / Right IN (hinges on outer edges)
            CenterPost,         // 5: Center post/mullion between doors (idle row)
            AutoCloseOnImpact,  // 6: Auto-close on collision, reopen after delay
            DoubleAction        // 7: Double-action (swing BOTH in AND out like saloon/kitchen doors)
        }
    }
}
