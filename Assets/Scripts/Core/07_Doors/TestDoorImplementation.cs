// TestDoorImplementation.cs
// Test script for new 3D pixel art door system
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using UnityEngine;
using UnityEngine.InputSystem;
using Code.Lavos.Core;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Test script to verify door cube implementation.
    /// Attach to an empty GameObject in a test scene.
    /// Press T to create a single door, D for double door.
    /// </summary>
    public class TestDoorImplementation : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private Key spawnSingleKey = Key.T;
        [SerializeField] private Key spawnDoubleKey = Key.D;
        [SerializeField] private Key clearKey = Key.C;
        [SerializeField] private Key animateKey = Key.Space;

        [Header("Door Settings")]
        [SerializeField] private float doorHeight = 6f;  // Increased from 3f
        [SerializeField] private float doorWidth = 4f;   // Increased from 2f
        [SerializeField] private Material doorMaterial;

        private GameObject currentDoor;
        private bool isOpen = false;
        private Keyboard keyboard;

        void Awake()
        {
            keyboard = Keyboard.current;
            Debug.Log("[TestDoor] Test script initialized. Press T to spawn door.", this);
        }

        void Update()
        {
            if (keyboard == null)
            {
                keyboard = Keyboard.current;
                return;
            }

            // Debug: Show which keys are pressed
            if (keyboard[spawnSingleKey].wasPressedThisFrame)
            {
                Debug.Log($"[TestDoor] T pressed! Creating single door... Material assigned: {doorMaterial != null}", this);
                CreateSingleDoor();
            }

            if (keyboard[spawnDoubleKey].wasPressedThisFrame)
            {
                Debug.Log($"[TestDoor] D pressed! Creating double door... Material assigned: {doorMaterial != null}", this);
                CreateDoubleDoor();
            }

            if (keyboard[clearKey].wasPressedThisFrame)
            {
                Debug.Log("[TestDoor] C pressed! Clearing doors.", this);
                ClearDoors();
            }

            if (keyboard[animateKey].wasPressedThisFrame && currentDoor != null)
            {
                Debug.Log("[TestDoor] SPACE pressed! Toggling door.", this);
                ToggleDoor();
            }
        }

        void CreateSingleDoor()
        {
            ClearDoors();

            // Create door at test position
            var doorObj = DoorCubeFactory.CreateDoorCube(
                transform.position + Vector3.up * doorHeight / 2,
                transform.rotation,
                isLeftHanded: true,
                parent: transform
            );

            doorObj.name = "Test_SingleDoor";

            Debug.Log($"[TestDoor] Door GameObject created: {doorObj.name}", doorObj);
            Debug.Log($"[TestDoor] Position: {doorObj.transform.position}", doorObj);
            Debug.Log($"[TestDoor] Has MeshFilter: {doorObj.GetComponent<MeshFilter>() != null}", doorObj);
            Debug.Log($"[TestDoor] Has MeshRenderer: {doorObj.GetComponent<MeshRenderer>() != null}", doorObj);

            // Apply material if assigned
            if (doorMaterial != null)
            {
                var renderer = doorObj.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = doorMaterial;
                    Debug.Log($"[TestDoor] ✅ Material applied: {doorMaterial.name}", doorObj);
                }
                else
                {
                    Debug.LogError("[TestDoor] ❌ No MeshRenderer found!", doorObj);
                }
            }
            else
            {
                Debug.LogWarning("[TestDoor] ⚠️ No material assigned! Door will be invisible.", this);
            }

            currentDoor = doorObj;
            isOpen = false;

            Debug.Log($"[TestDoor] ✅ Single door creation complete!", doorObj);
            Debug.Log("[TestDoor] Press SPACE to open/close, C to clear");
        }

        void CreateDoubleDoor()
        {
            ClearDoors();

            Debug.Log("[TestDoor] Creating double door... (Press D)");

            var doors = DoorCubeFactory.CreateDoubleDoor(
                transform.position + Vector3.up * doorHeight / 2,
                transform.rotation,
                parent: transform
            );

            doors[0].transform.parent.name = "Test_DoubleDoor";

            // Apply material if assigned
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

            currentDoor = doors[0].transform.parent.gameObject;
            isOpen = false;

            Debug.Log($"[TestDoor] ✅ Double door created at {currentDoor.transform.position}");
            Debug.Log("[TestDoor] Press SPACE to open/close, C to clear");
        }

        void ClearDoors()
        {
            if (currentDoor != null)
            {
                DestroyImmediate(currentDoor);
                currentDoor = null;
                Debug.Log("[TestDoor] 🗑️ Door cleared");
            }
        }

        void ToggleDoor()
        {
            if (currentDoor == null)
            {
                Debug.LogWarning("[TestDoor] No door to toggle!", this);
                return;
            }

            isOpen = !isOpen;
            
            // Check if this is a double door
            var doorPanels = currentDoor.GetComponentsInChildren<Transform>();
            Transform leftPanel = null;
            Transform rightPanel = null;
            
            foreach (var panel in doorPanels)
            {
                if (panel.name.Contains("LeftDoorPanel"))
                {
                    leftPanel = panel;
                }
                else if (panel.name.Contains("RightDoorPanel"))
                {
                    rightPanel = panel;
                }
            }

            // Double door - animate to absolute angles
            if (leftPanel != null && rightPanel != null)
            {
                Debug.Log("[TestDoor] Double door - toggling open/close", currentDoor);
                
                if (isOpen)
                {
                    // Open: 
                    // - Left door (rotated 180°): +90° local = swings LEFT
                    // - Right door (normal): -90° local = swings RIGHT
                    Debug.Log("[TestDoor] Opening: Left +90°, Right -90°");
                    DoorCubeFactory.AnimateDoorToAbsolute(leftPanel.gameObject, 90f, 1f);
                    DoorCubeFactory.AnimateDoorToAbsolute(rightPanel.gameObject, -90f, 1f);
                }
                else
                {
                    // Close: both to 0°
                    Debug.Log("[TestDoor] Closing: Both to 0°");
                    DoorCubeFactory.AnimateDoorToAbsolute(leftPanel.gameObject, 0f, 1f);
                    DoorCubeFactory.AnimateDoorToAbsolute(rightPanel.gameObject, 0f, 1f);
                }
            }
            else
            {
                // Single door
                float targetAngle = isOpen ? 90f : 0f;
                Debug.Log($"[TestDoor] Single door - animating to {targetAngle}°", currentDoor);
                
                int animatedCount = 0;
                foreach (var panel in doorPanels)
                {
                    if (panel.name.Contains("DoorPanel"))
                    {
                        DoorCubeFactory.AnimateDoorToAbsolute(panel.gameObject, targetAngle, 1f);
                        animatedCount++;
                    }
                }

                if (animatedCount == 0)
                {
                    DoorCubeFactory.AnimateDoorToAbsolute(currentDoor, targetAngle, 1f);
                }
            }
        }

        void OnDrawGizmos()
        {
            if (!showDebugInfo) return;

            // Draw door bounds
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(
                transform.position + Vector3.up * doorHeight / 2,
                new Vector3(doorWidth, doorHeight, 0.2f)
            );

            // Draw pivot points
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(
                transform.position + Vector3.up * doorHeight / 2 + Vector3.left * doorWidth / 2,
                0.1f
            );
            Gizmos.DrawSphere(
                transform.position + Vector3.up * doorHeight / 2 + Vector3.right * doorWidth / 2,
                0.1f
            );

            // Label
            Handles.Label(
                transform.position + Vector3.up * doorHeight,
                "Door Test Zone\nPress T: Single | D: Double | C: Clear | SPACE: Animate"
            );
        }
    }

    // Simple helper for drawing labels in Scene view
    public static class Handles
    {
        public static void Label(Vector3 position, string text)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.Label(position, text);
#endif
        }
    }
}
