// Copyright (C) 2026 Ocxyde
//
// This file is part of Code.Lavos.
//
// Code.Lavos is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Code.Lavos is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Code.Lavos.  If not, see <https://www.gnu.org/licenses/>.
// PlayerController.cs
// Player movement, camera, and input
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Player controller with New Input System

using UnityEngine;
using UnityEngine.InputSystem;
using Code.Lavos.Status;

namespace Code.Lavos.Core
{
    /// <summary>
    /// PLAYERCONTROLLER — Mouvement 3D, regard souris, stamina, interaction, bob caméra.
    ///
    /// SETUP Unity :
    ///  1. CharacterController sur le GameObject joueur
    ///  2. Ce script sur le même GameObject
    ///  3. Assigner playerCamera dans l'Inspector (caméra enfant du joueur)
    ///  4. La caméra sera auto-positionnée à hauteur des yeux — ne pas la bouger manuellement.
    ///
    /// Contrôles : WASD = marche | Shift = sprint | Espace = saut | Souris = regard | E = interaction
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        // ─── Déplacement ─────────────────────────────────────────────────────────
        [Header("Déplacement")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 9f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -19.81f;

    // ─── Stamina Costs ───────────────────────────────────────────────────────
    [Header("Stamina")]
    [SerializeField] private float sprintCostPerSecond = 2f; // Flat stamina drain per second while sprinting
    [SerializeField] private float jumpCost = 5f; // Flat stamina cost per jump

    // ─── Caméra / Regard ─────────────────────────────────────────────────────
    [Header("Caméra")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 0.2f;
    [SerializeField] private float maxLookAngle = 80f;
    [SerializeField] private float eyeHeightOffset = 1.7f; // hauteur yeux (mètres depuis pivot) - matches PlayerPrefab camera Y position

    // ─── Head Bob ────────────────────────────────────────────────────────────
    [Header("Head Bob")]
    [SerializeField] private bool bobEnabled = true;
    [SerializeField] private float bobFreqWalk = 8f;    // cycles / seconde en marche
    [SerializeField] private float bobFreqSprint = 13f;   // cycles / seconde en sprint
    [SerializeField] private float bobAmplitudeY = 0.055f; // oscillation verticale (haut/bas)
    [SerializeField] private float bobAmplitudeX = 0.025f; // oscillation latérale (tangage)
    [SerializeField] private float bobSmoothing = 10f;   // lissage retour au repos

    // ─── Interaction ─────────────────────────────────────────────────────────
    [Header("Interaction")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactionLayer = ~0;
    [SerializeField] private LayerMask playerLayer = 0;
    // Note: interactionPromptText moved to HUD assembly to avoid circular dependency

    // ─── Interaction System Reference ────────────────────────────────────────
    // Note: InteractionSystem is now in Core but referenced via interface

    // ─── Composants ──────────────────────────────────────────────────────────
    private CharacterController _controller;
    private PlayerStats _playerStats;
    private CombatSystem _combatSystem;

    // ─── Input ───────────────────────────────────────────────────────────────
    private Keyboard _kb;
    private Mouse _mouse;

    // ─── Game State (Plug-in-and-Out) ───────────────────────────────────────
    private bool _isGamePaused = false;

    // ─── État mouvement ──────────────────────────────────────────────────────
    private Vector3 _velocity;
    private float _xRotation;
    private bool _isGrounded;
    private bool _isMoving;
    private bool _isSprinting;

    // ─── Head Bob interne ────────────────────────────────────────────────────
    private float _bobTimer;                    // accumulateur de phase
    private Vector3 _bobCurrentOffset;            // offset appliqué ce frame
    private Vector3 _bobTargetOffset;             // offset cible (interpolé)
    // Position de repos de la caméra (= eyeHeightOffset, sans bob)
    private Vector3 _camRestPosition;

    // ─── Interaction ─────────────────────────────────────────────────────────
    private IInteractable _currentInteractable;
    private IInteractable _highlightedInteractable;

    // ─── Événements ──────────────────────────────────────────────────────────
    // Delegated to InteractionSystem
    public static event System.Action<string> OnInteractableChanged;

    // ─── Propriétés ──────────────────────────────────────────────────────────
    // Note: PlayerInventory moved to Inventory assembly to avoid circular dependency
    // Delegate to InteractionSystem if available
    public IInteractable CurrentInteractable => _currentInteractable;
    public bool HasInteractable => _currentInteractable != null;
    public bool IsGrounded => _isGrounded;

    /// <summary>
    /// Attempts to cast a spell by consuming mana. Returns false if insufficient mana.
    /// </summary>
    public bool TryCastSpell(float manaCost)
    {
        if (_playerStats == null) return false;
        return _playerStats.UseMana(manaCost);
    }

    // ═════════════════════════════════════════════════════════════════════════
    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        if (_controller == null) { Debug.LogError("[PlayerController] CharacterController manquant !"); enabled = false; return; }

        _playerStats = GetComponent<PlayerStats>();
        _combatSystem = FindFirstObjectByType<CombatSystem>();

        _controller.skinWidth = 0.08f;
        _controller.minMoveDistance = 0.001f;

        // Force initialize Input System
        RefreshInputReferences();
        
        // Debug: Verify input is available
        if (_kb == null)
        {
            Debug.LogWarning("[PlayerController] Keyboard not available in Awake - will retry in Update");
        }
        else
        {
            Debug.Log("[PlayerController] Input system initialized successfully");
        }

        // ── Positionne la caméra à hauteur des yeux ──────────────────────────
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera != null)
        {
            // Verify camera is child of player (plug-in-out pattern)
            if (playerCamera.transform.parent != transform)
            {
                Debug.LogWarning("[PlayerController] Camera is not a child of Player! Auto-fixing...");
                // Auto-fix: Reparent camera to player
                playerCamera.transform.SetParent(transform);
                playerCamera.transform.localPosition = new Vector3(0f, eyeHeightOffset, 0f);
                playerCamera.transform.localRotation = Quaternion.identity;
                Debug.Log("[PlayerController] Camera reparented to player with correct local position");
            }

            // FPS VIEW: Camera at middle of eyes (between eyes, not top of head)
            // For 2m tall CharacterController: eyes at ~1.6m (middle of head)
            // Camera local position: (0, 1.6, 0) - exactly at eye level
            Vector3 targetCamPos = new Vector3(0f, 1.6f, 0f);
            
            // Validate camera local position matches expected eye height
            if (playerCamera.transform.localPosition != targetCamPos)
            {
                Debug.Log($"[PlayerController] Setting FPS camera from {playerCamera.transform.localPosition} to {targetCamPos}");
                playerCamera.transform.localPosition = targetCamPos;
            }
            
            // Disable any CameraFollow component (we want FPS, not third-person)
            CameraFollow camFollow = playerCamera.GetComponent<CameraFollow>();
            if (camFollow != null)
            {
                camFollow.enabled = false;
                Debug.Log("[PlayerController] Disabled CameraFollow for FPS view");
            }
        }
        else
        {
            Debug.LogError("[PlayerController] No camera found! Player will not be able to see.");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ═════════════════════════════════════════════════════════════════════════
    void Update()
    {
        RefreshInputReferences();
        
        // Debug: Check input system
        if (_kb == null)
        {
            if (Time.frameCount % 120 == 0)
                Debug.Log("[PlayerController] Keyboard input not available!");
            return;
        }
        if (_mouse == null)
        {
            if (Time.frameCount % 120 == 0)
                Debug.Log("[PlayerController] Mouse input not available!");
            return;
        }

        // PLUG-IN-AND-OUT: Check game state via event subscription (not direct GameManager access)
        if (_isGamePaused)
            return;

        HandleCursorInput();
        HandleMouseLook();
        HandleMovement();
        HandleHeadBob();

        // Interaction handling - see InteractionSystem.cs for full implementation
        HandleInteraction();
    }
    
    #region Game State Event Handlers (Plug-in-and-Out)
    
    void OnEnable()
    {
        // Subscribe to game state changes
        if (EventHandler.Instance != null)
        {
            EventHandler.Instance.OnGameStateChanged += OnGameStateChanged;
            EventHandler.Instance.OnGamePaused += OnGamePaused;
            EventHandler.Instance.OnGameResumed += OnGameResumed;
        }
    }
    
    void OnDisable()
    {
        // Unsubscribe from game state changes
        if (EventHandler.Instance != null)
        {
            EventHandler.Instance.OnGameStateChanged -= OnGameStateChanged;
            EventHandler.Instance.OnGamePaused -= OnGamePaused;
            EventHandler.Instance.OnGameResumed -= OnGameResumed;
        }
    }
    
    /// <summary>
    /// Handle game state changes via event (plug-in-and-out).
    /// </summary>
    private void OnGameStateChanged(GameManager.GameState newState)
    {
        _isGamePaused = (newState != GameManager.GameState.Playing);
        
        if (newState == GameManager.GameState.Playing)
        {
            EnablePlayerInput();
        }
        else
        {
            DisablePlayerInput();
        }
    }
    
    private void OnGamePaused()
    {
        _isGamePaused = true;
        DisablePlayerInput();
    }
    
    private void OnGameResumed()
    {
        _isGamePaused = false;
        EnablePlayerInput();
    }
    
    private void EnablePlayerInput()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void DisablePlayerInput()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    #endregion

    private void Start()
    {
        // Interaction prompt UI handled by HUD assembly
    }

    private void OnDestroy()
    {
        // Cleanup handled by Unity
    }

    // ─────────────────────────────────────────────────────────────────────────
    private void RefreshInputReferences()
    {
        if (_kb == null)
            _kb = Keyboard.current;
        if (_mouse == null)
            _mouse = Mouse.current;
            
        // Debug: Log if input is still null after refresh
        if (_kb == null && Time.frameCount % 180 == 0)
        {
            Debug.LogWarning("[PlayerController] Keyboard.current is null - New Input System issue?");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  CURSEUR
    // ─────────────────────────────────────────────────────────────────────────
    private void HandleCursorInput()
    {
        if (_kb.escapeKey.wasPressedThisFrame) { UnlockCursor(); return; }
        if (Cursor.visible && _mouse.leftButton.wasPressedThisFrame) LockCursor();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && Cursor.lockState == CursorLockMode.Locked) UnlockCursor();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  REGARD (souris)
    // ─────────────────────────────────────────────────────────────────────────
    private void HandleMouseLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        Vector2 delta = _mouse.delta.ReadValue() * mouseSensitivity;

        // Rotation horizontale → tout le joueur
        transform.Rotate(Vector3.up * delta.x);

        // Rotation verticale → caméra seulement
        _xRotation = Mathf.Clamp(_xRotation - delta.y, -maxLookAngle, maxLookAngle);

        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  DÉPLACEMENT + SAUT + GRAVITÉ
    // ─────────────────────────────────────────────────────────────────────────
    private void HandleMovement()
    {
        // Lock cursor for FPS control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        _isGrounded = _controller.isGrounded;

        // Debug: Log grounded state (less spam)
        if (Time.frameCount % 120 == 0 && !_isGrounded)
        {
            Debug.Log($"[PlayerController] Grounded: {_isGrounded} | Position: {_controller.transform.position}");
        }

        if (_isGrounded && _velocity.y < 0f) _velocity.y = -2f;

        float h = (_kb.dKey.isPressed || _kb.rightArrowKey.isPressed ? 1f : 0f)
                - (_kb.aKey.isPressed || _kb.leftArrowKey.isPressed ? 1f : 0f);
        float v = (_kb.wKey.isPressed || _kb.upArrowKey.isPressed ? 1f : 0f)
                - (_kb.sKey.isPressed || _kb.downArrowKey.isPressed ? 1f : 0f);

        _isMoving = (h != 0f || v != 0f);
        
        // Debug: Log input
        if (_isMoving && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[PlayerController] Input: h={h}, v={v} | Sprint: {_isSprinting}");
        }
        
        // Check sprint condition: shift held + moving + grounded + has stamina
        _isSprinting = _kb.leftShiftKey.isPressed && _isMoving && _isGrounded &&
                       _playerStats != null && _playerStats.CurrentStamina > 1f;

        Vector3 moveDir = (transform.right * h + transform.forward * v).normalized;

        // Calculate speed with sprint bonus (+10% base movement speed when sprinting)
        float baseSpeed = _isSprinting ? sprintSpeed : walkSpeed;
        float speed = _isSprinting ? baseSpeed * 1.10f : baseSpeed; // +10% speed bonus while sprinting

        // Jump with flat stamina cost
        if (_kb.spaceKey.wasPressedThisFrame && _isGrounded)
        {
            // Always allow jump (fallback mode - no stamina check for now)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            
            // Try to consume stamina if systems are available
            if (_combatSystem != null && _combatSystem.CanJump())
            {
                _combatSystem.UseStamina(jumpCost);
            }
            else if (_playerStats != null && _playerStats.CurrentStamina >= jumpCost)
            {
                _playerStats.UseStamina(jumpCost);
            }
            // Jump always works, stamina consumption is secondary
        }

        _velocity.y += gravity * Time.deltaTime;

        _controller.Move(moveDir * speed * Time.deltaTime + _velocity * Time.deltaTime);

        // Consume flat stamina while sprinting (use PlayerStats directly)
        if (_isSprinting)
        {
            float drainAmount = sprintCostPerSecond * Time.deltaTime;

            if (_playerStats != null)
            {
                bool staminaUsed = _playerStats.UseStamina(drainAmount);
                if (!staminaUsed)
                {
                    // Force stop sprinting when stamina depleted
                    _isSprinting = false;
                }
            }
            else
            {
                // Disable sprint if PlayerStats not available
                _isSprinting = false;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  HEAD BOB
    // ─────────────────────────────────────────────────────────────────────────
    private void HandleHeadBob()
    {
        if (playerCamera == null) return;

        bool shouldBob = bobEnabled && _isMoving && _isGrounded;

        if (shouldBob)
        {
            // Fréquence variable selon vitesse
            float freq = _isSprinting ? bobFreqSprint : bobFreqWalk;
            _bobTimer += Time.deltaTime * freq;

            // Oscillation sinusoïdale :
            //   Y  = sin(t)          → haut/bas (deux pas par cycle)
            //   X  = sin(t/2)        → gauche/droite (un balancement par cycle)
            float sinY = Mathf.Sin(_bobTimer);
            float sinX = Mathf.Sin(_bobTimer * 0.5f);

            // Amplitude légèrement amplifiée en sprint
            float ampScale = _isSprinting ? 1.4f : 1f;
            _bobTargetOffset = new Vector3(
                sinX * bobAmplitudeX * ampScale,
                sinY * bobAmplitudeY * ampScale,
                0f
            );
        }
        else
        {
            // Retour progressif à zéro quand arrêté ou en l'air
            _bobTimer = 0f;           // réinitialise la phase → évite le saut de phase
            _bobTargetOffset = Vector3.zero;
        }

        // Lissage fluide vers la cible
        _bobCurrentOffset = Vector3.Lerp(_bobCurrentOffset, _bobTargetOffset, Time.deltaTime * bobSmoothing);

        // Application : position de repos + offset bob
        // FIX: Ensure Z is always 0 to prevent camera drift
        Vector3 finalCamPos = _camRestPosition + _bobCurrentOffset;
        finalCamPos.z = 0f; // Force Z to 0
        playerCamera.transform.localPosition = finalCamPos;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  INTERACTION (E) - Legacy mode (when InteractionSystem not available)
    // ─────────────────────────────────────────────────────────────────────────
    private void HandleInteraction()
    {
        if (_kb.eKey.wasPressedThisFrame && _currentInteractable?.CanInteract(this) == true)
            _currentInteractable.OnInteract(this);

        CheckForInteractable();
    }

    private void CheckForInteractable()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        LayerMask mask = interactionLayer & ~playerLayer;

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, mask))
        {
            IInteractable found = hit.collider.GetComponent<IInteractable>();
            if (found != null)
            {
                if (_highlightedInteractable != found)
                {
                    _highlightedInteractable?.OnHighlightExit(this);
                    _highlightedInteractable = found;
                    _highlightedInteractable.OnHighlightEnter(this);
                }
                _currentInteractable = found;
                UpdateInteractionPrompt(found.InteractionPrompt);
                OnInteractableChanged?.Invoke(found.InteractionPrompt);
                return;
            }
        }

        // Rien trouvé
        _highlightedInteractable?.OnHighlightExit(this);
        _highlightedInteractable = null;
        _currentInteractable = null;
        ClearInteractionPrompt();
        OnInteractableChanged?.Invoke(string.Empty);
    }

    /// <summary>
    /// Update interaction prompt UI (called by InteractionSystem)
    /// Note: UI handling moved to HUD assembly to avoid circular dependency
    /// </summary>
    private void UpdateInteractionPromptUI(string prompt)
    {
        // UI handling moved to HUD assembly
    }

    private void UpdateInteractionPrompt(string prompt)
    {
        // UI handling moved to HUD assembly
    }

    private void ClearInteractionPrompt()
    {
        // UI handling moved to HUD assembly
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  UTILITAIRES PUBLICS
    // ─────────────────────────────────────────────────────────────────────────
    public void LockCursor() { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
    public void UnlockCursor() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    }
}
