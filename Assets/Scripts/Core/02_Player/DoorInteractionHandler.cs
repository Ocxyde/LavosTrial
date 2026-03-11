// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8  |  Locale: en_US

using UnityEngine;
using Code.Lavos.Core.Environment;

namespace Code.Lavos.Core.Player
{
    /// <summary>
    /// DoorInteractionHandler - Handles player interaction with doors.
    /// 
    /// Attach to Player GameObject.
    /// Detects nearby doors and allows interaction with "F" key.
    /// 
    /// Features:
    /// - Auto-detect doors in range
    /// - Show interaction prompt
    /// - Handle "F" key press
    /// - Visual feedback for interaction
    /// </summary>
    public class DoorInteractionHandler : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [Tooltip("Maximum distance to interact with doors")]
        [SerializeField] [Range(1f, 10f)] private float interactionDistance = 3f;

        [Tooltip("Show interaction prompt (requires UI system)")]
        [SerializeField] private bool showPrompt = true;

        [Tooltip("Key to press for interaction")]
        [SerializeField] private KeyCode interactionKey = KeyCode.F;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        // Current interactable door
        private DoorController currentDoor = null;
        private bool wasInRange = false;

        private void Update()
        {
            // Find nearest door in range
            DoorController nearestDoor = FindNearestDoorInRadius();

            // Update current door
            if (nearestDoor != currentDoor)
            {
                currentDoor = nearestDoor;
            }

            // Check if player entered door range
            if (currentDoor != null && !wasInRange)
            {
                wasInRange = true;
                Debug.Log($"[DoorInteraction] In range of: {currentDoor.gameObject.name}");
            }
            else if (currentDoor == null && wasInRange)
            {
                wasInRange = false;
                Debug.Log("[DoorInteraction] Left door range");
            }

            // Handle interaction input
            if (currentDoor != null && Input.GetKeyDown(interactionKey))
            {
                InteractWithDoor();
            }

            // Debug info
            if (showDebugInfo)
            {
                DrawDebugGizmos();
            }
        }

        /// <summary>
        /// Find nearest door within interaction distance.
        /// </summary>
        private DoorController FindNearestDoorInRadius()
        {
            // Find all DoorController components in scene
            DoorController[] allDoors = FindObjectsOfType<DoorController>();

            DoorController nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var door in allDoors)
            {
                float distance = Vector3.Distance(transform.position, door.transform.position);
                
                if (distance <= interactionDistance && distance < nearestDistance)
                {
                    // Check if door is interactable
                    if (door.CanInteract(transform))
                    {
                        nearest = door;
                        nearestDistance = distance;
                    }
                }
            }

            return nearest;
        }

        /// <summary>
        /// Interact with current door.
        /// </summary>
        private void InteractWithDoor()
        {
            if (currentDoor == null)
                return;

            Debug.Log($"[DoorInteraction] Interacting with: {currentDoor.gameObject.name}");
            currentDoor.Interact(transform);
        }

        /// <summary>
        /// Draw debug gizmos in editor.
        /// </summary>
        private void DrawDebugGizmos()
        {
#if UNITY_EDITOR
            // Draw interaction range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);

            // Draw line to current door
            if (currentDoor != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, currentDoor.transform.position);
                
                // Draw door info
                UnityEditor.Handles.Label(
                    currentDoor.transform.position + Vector3.up * 4f,
                    $"Door: {currentDoor.CurrentState}\n{currentDoor.GetPromptText()}"
                );
            }
#endif
        }

        private void OnGUI()
        {
            if (!showPrompt || currentDoor == null)
                return;

            // Simple interaction prompt
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            // Create background texture
            Texture2D bgTexture = new Texture2D(1, 1);
            bgTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.7f));
            bgTexture.Apply();

            GUIStyle background = new GUIStyle(GUI.skin.box)
            {
                normal = { background = bgTexture }
            };

            GUILayout.BeginArea(new Rect(Screen.width / 2 - 150, Screen.height - 100, 300, 50), background);
            GUILayout.Label(currentDoor.GetPromptText(), style);
            GUILayout.EndArea();
        }
    }
}
