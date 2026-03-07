// TestDoorImplementation.cs
// Test script for new 3D pixel art door system
// Unity 6 compatible - UTF-8 encoding - Unix line endings

using UnityEngine;
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
        [SerializeField] private KeyCode spawnSingleKey = KeyCode.T;
        [SerializeField] private KeyCode spawnDoubleKey = KeyCode.D;
        [SerializeField] private KeyCode clearKey = KeyCode.C;
        [SerializeField] private KeyCode animateKey = KeyCode.Space;

        [Header("Door Settings")]
        [SerializeField] private float doorHeight = 3f;
        [SerializeField] private float doorWidth = 2f;
        [SerializeField] private Material doorMaterial;

        private GameObject currentDoor;
        private bool isOpen = false;

        void Update()
        {
            // Spawn single door
            if (Input.GetKeyDown(spawnSingleKey))
            {
                CreateSingleDoor();
            }

            // Spawn double door
            if (Input.GetKeyDown(spawnDoubleKey))
            {
                CreateDoubleDoor();
            }

            // Clear all doors
            if (Input.GetKeyDown(clearKey))
            {
                ClearDoors();
            }

            // Toggle door animation
            if (Input.GetKeyDown(animateKey) && currentDoor != null)
            {
                ToggleDoor();
            }
        }

        void CreateSingleDoor()
        {
            ClearDoors();

            Debug.Log("[TestDoor] Creating single door... (Press T)");

            // Create door at test position
            var doorObj = DoorCubeFactory.CreateDoorCube(
                transform.position + Vector3.up * doorHeight / 2,
                transform.rotation,
                isLeftHanded: true,
                parent: transform
            );

            doorObj.name = "Test_SingleDoor";

            // Apply material if assigned
            if (doorMaterial != null)
            {
                var renderer = doorObj.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = doorMaterial;
                }
            }

            currentDoor = doorObj;
            isOpen = false;

            Debug.Log($"[TestDoor] ✅ Single door created at {doorObj.transform.position}");
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
            if (currentDoor == null) return;

            isOpen = !isOpen;
            float targetAngle = isOpen ? 90f : 0f;

            Debug.Log($"[TestDoor] {'\u25B6'} Animating door to {targetAngle}°");

            // Animate all child door panels
            var doorPanels = currentDoor.GetComponentsInChildren<Transform>();
            foreach (var panel in doorPanels)
            {
                if (panel.name.Contains("DoorPanel"))
                {
                    DoorCubeFactory.AnimateDoor(panel.gameObject, targetAngle, 1f);
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
