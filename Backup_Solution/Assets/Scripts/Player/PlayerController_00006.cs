// PlayerController.cs
// Player movement, camera, and input
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Player controller with New Input System

// PlayerController.cs
// Player movement, camera, and input
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Player controller with New Input System
using TMPro;
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

    // ─── Caméra / Regard ─────────────────────────────────────────────────────
    [Header("Caméra")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 0.2f;
    [SerializeField] private float maxLookAngle = 80f;
    [SerializeField] private float eyeHeightOffset = 0.75f; // hauteur yeux (mètres depuis pivot)

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
    [SerializeField] private TextMeshProUGUI interactionPromptText;

    // ─── Composants ──────────────────────────────────────────────────────────
    private CharacterController _controller;
    private Inventory _inventory;
    private PlayerStats _playerStats;

    // ─── Input ───────────────────────────────────────────────────────────────
    private Keyboard _kb;
    private Mouse _mouse;

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
    public static event System.Action<string> OnInteractableChanged;

    // ─── Propriétés ──────────────────────────────────────────────────────────
    public Inventory PlayerInventory => _inventory;
    public IInteractable CurrentInteractable => _currentInteractable;
    public bool HasInteractable => _currentInteractable != null;
    public bool IsGrounded => _isGrounded;

    /// <summary>
    /// Attempts to cast a spell by consuming mana. Returns false if insufficient mana.
    /// </summary>
    public bool TryCastSpell(float manaCost)
    {
        if (PlayerStats.Instance == null) return false;
        return PlayerStats.Instance.UseMana(manaCost);
    }

    // ═════════════════════════════════════════════════════════════════════════
    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        if (_controller == null) { Debug.LogError("[PlayerController] CharacterController manquant !"); enabled = false; return; }

        _inventory = GetComponent<Inventory>() ?? gameObject.AddComponent<Inventory>();
        _playerStats = GetComponent<PlayerStats>();

        _controller.skinWidth = 0.08f;
        _controller.minMoveDistance = 0.001f;

        RefreshInputReferences();

        // ── Positionne la caméra à hauteur des yeux ──────────────────────────
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera != null)
        {
            _camRestPosition = new Vector3(0f, eyeHeightOffset, 0f);
            playerCamera.transform.localPosition = _camRestPosition;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ═════════════════════════════════════════════════════════════════════════
    void Update()
    {
        RefreshInputReferences();
        if (_kb == null || _mouse == null) return;

        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        HandleCursorInput();
        HandleMouseLook();
        HandleMovement();
        HandleHeadBob();
        HandleInteraction();
    }

    // ─────────────────────────────────────────────────────────────────────────
    private void RefreshInputReferences()
    {
        _kb ??= Keyboard.current;
        _mouse ??= Mouse.current;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  CURSEUR
    // ─────────────────────────────────────────────────────────────────────────
    private void HandleCursorInput()
    {
        if (_kb.escapeKey.wasPressedThisFrame) { UnlockCursor(); return; }
        if (Cursor.visible && _mouse.leftButton.wasPressedThisFrame) LockCursor();
    }

    void OnApplicationFocus(bool hasFocus)
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
        if (Cursor.lockState != CursorLockMode.Locked) return;

        _isGrounded = _controller.isGrounded;
        if (_isGrounded && _velocity.y < 0f) _velocity.y = -2f;

        float h = (_kb.dKey.isPressed || _kb.rightArrowKey.isPressed ? 1f : 0f)
                - (_kb.aKey.isPressed || _kb.leftArrowKey.isPressed ? 1f : 0f);
        float v = (_kb.wKey.isPressed || _kb.upArrowKey.isPressed ? 1f : 0f)
                - (_kb.sKey.isPressed || _kb.downArrowKey.isPressed ? 1f : 0f);

        _isMoving = (h != 0f || v != 0f);
        
        // Check sprint condition: shift held + moving + grounded + has stamina
        _isSprinting = _kb.leftShiftKey.isPressed && _isMoving && _isGrounded &&
                       PlayerStats.Instance != null && PlayerStats.Instance.CurrentStamina > 0f;

        Vector3 moveDir = (transform.right * h + transform.forward * v).normalized;
        
        // Calculate speed with sprint bonus (+5% base movement speed)
        float baseSpeed = _isSprinting ? sprintSpeed : walkSpeed;
        float speed = _isSprinting ? baseSpeed * 1.05f : baseSpeed; // +5% speed bonus while sprinting

        if (_kb.spaceKey.wasPressedThisFrame && _isGrounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        _velocity.y += gravity * Time.deltaTime;

        _controller.Move(moveDir * speed * Time.deltaTime + _velocity * Time.deltaTime);

        // Consume stamina while sprinting (1% of current stamina per second)
        if (_isSprinting && PlayerStats.Instance != null)
        {
            float drainAmount = PlayerStats.Instance.CurrentStamina * 0.01f * Time.deltaTime;
            bool success = PlayerStats.Instance.UseStamina(drainAmount);
            if (!success)
            {
                Debug.LogWarning($"[PlayerController] Failed to consume stamina! Current: {PlayerStats.Instance.CurrentStamina}, Requested: {drainAmount}");
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
        playerCamera.transform.localPosition = _camRestPosition + _bobCurrentOffset;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  INTERACTION (E)
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

    private void UpdateInteractionPrompt(string prompt)
    {
        if (interactionPromptText != null) interactionPromptText.text = $"[E] {prompt}";
    }

    private void ClearInteractionPrompt()
    {
        if (interactionPromptText != null) interactionPromptText.text = string.Empty;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  UTILITAIRES PUBLICS
    // ─────────────────────────────────────────────────────────────────────────
    public void LockCursor() { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
    public void UnlockCursor() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
}
