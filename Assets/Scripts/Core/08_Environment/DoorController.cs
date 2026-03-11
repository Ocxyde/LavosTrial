// LavosTrial - CodeDotLavos
// Copyright (C) 2026 CodeDotLavos
// Licensed under GPL-3.0 - see COPYING for details
// Encoding: UTF-8  |  Locale: en_US

using UnityEngine;

namespace Code.Lavos.Core.Environment
{
    /// <summary>
    /// DoorController - Interactive door that rotates outward when opened.
    /// 
    /// Features:
    /// - Outward rotation (90 degrees from wall plane)
    /// - "F" key interaction to open/close
    /// - Smooth animation with configurable speed
    /// - Door state tracking (Closed, Opening, Open, Closing)
    /// - Optional sound effects
    /// - Optional lock system (requires key)
    /// 
    /// Setup:
    /// 1. Create door prefab with pivot at hinge side (not center!)
    /// 2. Set scale: (1.0, 3.0, 0.3) for standard door
    /// 3. Add DoorController component
    /// 4. Assign to CompleteMazeBuilder8 door prefab field
    /// 
    /// Pivot Setup (CRITICAL):
    /// - For N/S facing doors: Pivot on left or right edge (X axis)
    /// - For E/W facing doors: Pivot on left or right edge (Z axis)
    /// - Pivot Y at bottom (floor level)
    /// 
    /// Usage:
    /// - Approach door and press "F" to open/close
    /// - Locked doors require key item
    /// </summary>
    public class DoorController : MonoBehaviour
    {
        #region Enums

        public enum DoorState
        {
            Closed,
            Opening,
            Open,
            Closing
        }

        public enum DoorType
        {
            Normal,
            Locked,
            Secret,
            Exit
        }

        #endregion

        #region Inspector Fields

        [Header("Door Configuration")]
        [Tooltip("Type of door (affects behavior and appearance)")]
        [SerializeField] private DoorType doorType = DoorType.Normal;

        [Tooltip("Rotation angle when fully open (90 = outward swing, -90 = inward)")]
        [SerializeField] [Range(-180f, 180f)] private float openAngle = 90f;

        [Tooltip("Hinge side: left = pivot on left edge, right = pivot on right edge")]
        [SerializeField] private bool hingeOnLeft = true;

        [Header("Animation Settings")]
        [Tooltip("Time to fully open/close the door (seconds)")]
        [SerializeField] [Range(0.1f, 3f)] private float animationDuration = 1.0f;

        [Tooltip("Animation curve (ease in/out recommended)")]
        [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Interaction Settings")]
        [Tooltip("Maximum distance to interact with door")]
        [SerializeField] [Range(1f, 10f)] private float interactionDistance = 3f;

        [Tooltip("Show interaction prompt when in range")]
        [SerializeField] private bool showInteractionPrompt = true;

        [Tooltip("Prompt text to display")]
        [SerializeField] private string promptText = "Open Door";

        [Header("Lock Settings (for Locked doors)")]
        [Tooltip("Is this door initially locked?")]
        [SerializeField] private bool isLocked = false;

        [Tooltip("Required key item ID (if using inventory system)")]
        [SerializeField] private string requiredKeyId = null;

        [Header("Audio (Optional)")]
        [Tooltip("Sound played when door opens")]
        [SerializeField] private AudioClip openSound = null;

        [Tooltip("Sound played when door closes")]
        [SerializeField] private AudioClip closeSound = null;

        [Tooltip("Sound played when door is locked")]
        [SerializeField] private AudioClip lockedSound = null;

        [Header("References (Auto-detected if not assigned)")]
        [Tooltip("Transform that rotates (should be the door mesh)")]
        [SerializeField] private Transform rotatingPart = null;

        [Tooltip("Audio source (auto-created if not assigned)")]
        [SerializeField] private AudioSource audioSource = null;

        #endregion

        #region Private Fields

        private DoorState currentState = DoorState.Closed;
        private float currentRotation = 0f;
        private float animationTimer = 0f;
        private bool isAnimating = false;
        private Quaternion closedRotation;
        private Quaternion openRotation;

        #endregion

        #region Public Properties

        public DoorState CurrentState => currentState;
        public DoorType Type => doorType;
        public bool IsLocked => isLocked;
        public bool IsOpen => currentState == DoorState.Open;
        public bool IsClosed => currentState == DoorState.Closed;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeDoor();
        }

        private void Update()
        {
            if (isAnimating)
            {
                UpdateAnimation();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize door components and calculate rotation angles.
        /// </summary>
        private void InitializeDoor()
        {
            // Setup rotating part
            if (rotatingPart == null)
            {
                rotatingPart = transform;
                Debug.Log($"[DoorController] Using self as rotating part: {gameObject.name}");
            }

            // Setup audio source
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 1f; // 3D audio
                }
            }

            // Store closed rotation
            closedRotation = rotatingPart.localRotation;

            // Calculate open rotation based on door direction
            CalculateOpenRotation();

            Debug.Log($"[DoorController] Initialized: {gameObject.name}, Type={doorType}, Locked={isLocked}");
        }

        /// <summary>
        /// Calculate the target rotation for opened state.
        /// Door rotates around its hinge (local Y axis).
        /// Pivot must be at bottom-left or bottom-right of door mesh.
        /// </summary>
        private void CalculateOpenRotation()
        {
            // Rotate around local Y axis (hinge)
            // Positive angle = outward swing, negative = inward
            openRotation = Quaternion.Euler(0f, openAngle, 0f) * closedRotation;

            Debug.Log($"[DoorController] Rotation calculated: Closed={closedRotation.eulerAngles}, Open={openRotation.eulerAngles} (hinge={(hingeOnLeft ? "left" : "right")})");
        }

        #endregion

        #region Interaction System

        /// <summary>
        /// Check if player can interact with this door.
        /// </summary>
        public bool CanInteract(Transform player)
        {
            if (currentState == DoorState.Opening || currentState == DoorState.Closing)
                return false;

            float distance = Vector3.Distance(player.position, transform.position);
            return distance <= interactionDistance;
        }

        /// <summary>
        /// Get interaction prompt text.
        /// </summary>
        public string GetPromptText()
        {
            if (isLocked)
                return "Locked Door";
            
            return currentState == DoorState.Closed ? "Open Door [F]" : "Close Door [F]";
        }

        /// <summary>
        /// Handle interaction (called when player presses "F").
        /// </summary>
        public void Interact(Transform player)
        {
            if (!CanInteract(player))
            {
                Debug.Log($"[DoorController] Too far or animating: {gameObject.name}");
                return;
            }

            // Check if locked
            if (isLocked)
            {
                PlaySound(lockedSound);
                Debug.Log($"[DoorController] Door is locked: {gameObject.name}");
                return;
            }

            // Toggle door state
            if (currentState == DoorState.Closed)
            {
                OpenDoor();
            }
            else if (currentState == DoorState.Open)
            {
                CloseDoor();
            }
        }

        /// <summary>
        /// Simple distance check for interaction.
        /// </summary>
        public bool IsPlayerInRange(Transform player)
        {
            float distance = Vector3.Distance(player.position, transform.position);
            return distance <= interactionDistance;
        }

        #endregion

        #region Door Control

        /// <summary>
        /// Open the door.
        /// </summary>
        [ContextMenu("Open Door")]
        public void OpenDoor()
        {
            if (currentState != DoorState.Closed)
                return;

            currentState = DoorState.Opening;
            animationTimer = 0f;
            isAnimating = true;

            Debug.Log($"[DoorController] Opening: {gameObject.name}");
        }

        /// <summary>
        /// Close the door.
        /// </summary>
        [ContextMenu("Close Door")]
        public void CloseDoor()
        {
            if (currentState != DoorState.Open)
                return;

            currentState = DoorState.Closing;
            animationTimer = 0f;
            isAnimating = true;

            Debug.Log($"[DoorController] Closing: {gameObject.name}");
        }

        /// <summary>
        /// Lock the door.
        /// </summary>
        public void Lock()
        {
            isLocked = true;
            Debug.Log($"[DoorController] Locked: {gameObject.name}");
        }

        /// <summary>
        /// Unlock the door.
        /// </summary>
        public void Unlock()
        {
            isLocked = false;
            Debug.Log($"[DoorController] Unlocked: {gameObject.name}");
        }

        /// <summary>
        /// Force door to open state (no animation).
        /// </summary>
        public void ForceOpen()
        {
            rotatingPart.localRotation = openRotation;
            currentState = DoorState.Open;
            isAnimating = false;
        }

        /// <summary>
        /// Force door to closed state (no animation).
        /// </summary>
        public void ForceClosed()
        {
            rotatingPart.localRotation = closedRotation;
            currentState = DoorState.Closed;
            isAnimating = false;
        }

        #endregion

        #region Animation

        /// <summary>
        /// Update door animation.
        /// </summary>
        private void UpdateAnimation()
        {
            if (!isAnimating)
                return;

            // Update timer
            animationTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(animationTimer / animationDuration);

            // Apply animation curve
            float curvedProgress = animationCurve.Evaluate(progress);

            // Interpolate rotation
            Quaternion targetRotation = (currentState == DoorState.Opening || currentState == DoorState.Open)
                ? openRotation
                : closedRotation;

            rotatingPart.localRotation = Quaternion.Slerp(
                currentState == DoorState.Opening ? closedRotation : openRotation,
                targetRotation,
                curvedProgress
            );

            // Check if animation complete
            if (progress >= 1f)
            {
                isAnimating = false;
                currentState = (currentState == DoorState.Opening || currentState == DoorState.Open)
                    ? DoorState.Open
                    : DoorState.Closed;

                // Play sound
                if (currentState == DoorState.Open)
                {
                    PlaySound(openSound);
                    Debug.Log($"[DoorController] Fully opened: {gameObject.name}");
                }
                else
                {
                    PlaySound(closeSound);
                    Debug.Log($"[DoorController] Fully closed: {gameObject.name}");
                }
            }
        }

        #endregion

        #region Audio

        /// <summary>
        /// Play audio clip.
        /// </summary>
        private void PlaySound(AudioClip clip)
        {
            if (clip == null || audioSource == null)
                return;

            audioSource.PlayOneShot(clip);
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Draw interaction range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);

            // Draw door swing arc
            Gizmos.color = Color.green;
            Vector3 arcStart = transform.position + transform.forward * 0.5f;
            Vector3 arcEnd = transform.position + Quaternion.Euler(0f, openAngle, 0f) * transform.forward * 0.5f;
            Gizmos.DrawLine(transform.position, arcStart);
            Gizmos.DrawLine(transform.position, arcEnd);
        }

        private void OnDrawGizmos()
        {
            // Show hinge point in editor
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.1f);
        }
#endif

        #endregion
    }
}
