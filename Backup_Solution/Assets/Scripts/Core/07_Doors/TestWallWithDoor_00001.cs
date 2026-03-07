// TestWallWithDoor.cs
// Creates a test wall with door hole to verify door math
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using UnityEngine;
using UnityEngine.InputSystem;
using Code.Lavos.Core;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Test script to create a wall with door hole and properly positioned door.
    /// Shows the correct math for door/wall alignment.
    /// </summary>
    public class TestWallWithDoor : MonoBehaviour
    {
        [Header("Wall Settings")]
        [SerializeField] private float wallWidth = 10f;
        [SerializeField] private float wallHeight = 6f;
        [SerializeField] private float wallThickness = 1f;
        
        [Header("Door Settings")]
        [SerializeField] private float doorWidth = 4f;
        [SerializeField] private float doorHeight = 6f;
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material doorMaterial;
        
        [Header("Test Controls")]
        [SerializeField] private Key createKey = Key.C;
        [SerializeField] private Key clearKey = Key.X;
        [SerializeField] private Key toggleDoorKey = Key.Space;
        
        private GameObject wallObj;
        private GameObject[] doors;
        private bool isDoorOpen = false;
        private Keyboard keyboard;
        
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
            
            if (keyboard[toggleDoorKey].wasPressedThisFrame && doors != null)
            {
                ToggleDoor();
            }
        }
        
        void CreateWallWithDoor()
        {
            ClearWall();
            
            Debug.Log("[TestWall] Creating wall with door hole...");
            
            // Create wall parent
            var wallParent = new GameObject("WallWithDoor");
            wallParent.transform.position = transform.position;
            wallParent.transform.rotation = transform.rotation;
            
            // Calculate door hole position (centered in wall)
            float holeX = 0f; // Center of wall
            float holeY = 0f; // Center height
            float holeZ = wallThickness / 2f + 0.01f; // Slightly in front of wall
            
            // Create wall with hole (two separate cubes)
            CreateWallWithHole(wallParent.transform, holeX, holeY, holeZ);
            
            // Create double door IN the hole
            doors = CreateDoorInHole(wallParent.transform, holeX, holeY, holeZ);
            
            wallObj = wallParent;
            isDoorOpen = false;
            
            Debug.Log("[TestWall] ✅ Wall with door created!");
            Debug.Log($"[TestWall] Wall: {wallWidth}x{wallHeight}x{wallThickness}");
            Debug.Log($"[TestWall] Door hole: {doorWidth}x{doorHeight} at ({holeX}, {holeY}, {holeZ})");
            Debug.Log($"[TestWall] Press SPACE to toggle door, X to clear");
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
            
            Debug.Log($"[TestWall] Left wall: pos=({leftX:F2}, 0, 0), width={leftWidth:F2}");
            Debug.Log($"[TestWall] Right wall: pos=({rightX:F2}, 0, 0), width={rightWidth:F2}");
            Debug.Log($"[TestWall] Door hole edges: left={holeX - halfHoleWidth:F2}, right={holeX + halfHoleWidth:F2}");
        }
        
        GameObject[] CreateDoorInHole(Transform parent, float holeX, float holeY, float holeZ)
        {
            // Create door parent at hole center
            var doorParent = new GameObject("Doors");
            doorParent.transform.parent = parent;
            doorParent.transform.localPosition = new Vector3(holeX, holeY, holeZ);

            // Create double door
            // DoorCubeFactory creates doors with hinges at center, spanning the doorway
            var doors = DoorCubeFactory.CreateDoubleDoor(
                Vector3.zero,
                Quaternion.identity,
                parent: doorParent.transform
            );

            Debug.Log("[TestWall] Door positions (relative to hole center):");
            Debug.Log($"  Left door: pos={doors[0].transform.localPosition}, rot={doors[0].transform.localEulerAngles}");
            Debug.Log($"  Right door: pos={doors[1].transform.localPosition}, rot={doors[1].transform.localEulerAngles}");

            // Apply door material
            if (doorMaterial != null)
            {
                foreach (var door in doors)
                {
                    var renderer = door.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        renderer.sharedMaterial = doorMaterial;
                    }
                }
            }

            return doors;
        }
        
        void ToggleDoor()
        {
            if (doors == null || doors.Length != 2) return;

            isDoorOpen = !isDoorOpen;

            if (isDoorOpen)
            {
                // Open: both swing OUTWARD (away from center)
                // Left door: hinge on RIGHT (at center), extends LEFT → rotate -90° (clockwise) to swing toward -Z
                // Right door: hinge on LEFT (at center), extends RIGHT → rotate +90° (counter-clockwise) to swing toward -Z
                Debug.Log("[TestWall] Opening doors (both outward)...");
                DoorCubeFactory.AnimateDoorToAbsolute(doors[0].gameObject, -90f, 1f);  // Left: 0° → -90°
                DoorCubeFactory.AnimateDoorToAbsolute(doors[1].gameObject, 90f, 1f);   // Right: 0° → +90°
            }
            else
            {
                // Close: both return to 0° (flat in the doorway)
                Debug.Log("[TestWall] Closing doors...");
                DoorCubeFactory.AnimateDoorToAbsolute(doors[0].gameObject, 0f, 1f);
                DoorCubeFactory.AnimateDoorToAbsolute(doors[1].gameObject, 0f, 1f);
            }
        }
        
        void ClearWall()
        {
            if (wallObj != null)
            {
                DestroyImmediate(wallObj);
                wallObj = null;
                doors = null;
                Debug.Log("[TestWall] Cleared wall");
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
                transform.position + Vector3.forward * (wallThickness / 2f + 0.01f),
                new Vector3(doorWidth, doorHeight, 0.1f)
            );
            
            // Label
#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * (wallHeight / 2f + 1f),
                $"Wall: {wallWidth}x{wallHeight}x{wallThickness}\nDoor: {doorWidth}x{doorHeight}\nPress C: Create | X: Clear | SPACE: Toggle"
            );
#endif
        }
    }
}
