using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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

    // ─── Stamina ─────────────────────────────────────────────────────────────
    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegen = 10f;
    // Note: staminaDrain removed - now uses 1% of current stamina per frame while sprinting

    // ─── Mana ────────────────────────────────────────────────────────────────
    [Header("Mana")]
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float manaRegen = 5f;
    [SerializeField] private float manaRegenDelay = 2f; // Delay before mana starts regenerating

    // ─── Interaction ─────────────────────────────────────────────────────────
    [Header("Interaction")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactionLayer = ~0;
    [SerializeField] private LayerMask playerLayer = 0;
    [SerializeField] private TextMeshProUGUI interactionPromptText;

    // ─── Composants ──────────────────────────────────────────────────────────
    private CharacterController _controller;
    private Inventory _inventory;

    // ─── Input ───────────────────────────────────────────────────────────────
    private Keyboard _kb;
    private Mouse _mouse;

    // ─── État mouvement ──────────────────────────────────────────────────────
    private Vector3 _velocity;
    private float _xRotation;
    private bool _isGrounded;
    private bool _isMoving;
    private bool _isSprinting;
    private float _currentStamina;

    // ─── État Mana ───────────────────────────────────────────────────────────
    private float _currentMana;
    private float _lastManaUseTime;

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
    public static event System.Action<float, float> OnStaminaChanged;
    public static event System.Action<float, float> OnManaChanged;
    public static event System.Action<string> OnInteractableChanged;

    // ─── Propriétés ──────────────────────────────────────────────────────────
    public Inventory PlayerInventory => _inventory;
    public IInteractable CurrentInteractable => _currentInteractable;
    public bool HasInteractable => _currentInteractable != null;
    public bool IsGrounded => _isGrounded;

    // Mana Properties
    public float CurrentMana => _currentMana;
    public float MaxMana => maxMana;
    public float CurrentManaPercent => maxMana > 0 ? _currentMana / maxMana : 0f;

    // Mana Methods
    public bool UseMana(float amount)
    {
        if (_currentMana >= amount)
        {
            _currentMana -= amount;
            _lastManaUseTime = Time.time;
            OnManaChanged?.Invoke(_currentMana, maxMana);
            return true;
        }
        return false;
    }

    public void RestoreMana(float amount)
    {
        _currentMana = Mathf.Min(_currentMana + amount, maxMana);
        OnManaChanged?.Invoke(_currentMana, maxMana);
    }

    // ═════════════════════════════════════════════════════════════════════════
    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        if (_controller == null) { Debug.LogError("[PlayerController] CharacterController manquant !"); enabled = false; return; }

        _inventory = GetComponent<Inventory>() ?? gameObject.AddComponent<Inventory>();

        _currentStamina = maxStamina;
        _currentMana = maxMana;
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
        HandleStamina();
        HandleMana();
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
        _isSprinting = _kb.leftShiftKey.isPressed && _isMoving && _isGrounded && _currentStamina > 0f;

        Vector3 moveDir = (transform.right * h + transform.forward * v).normalized;
        float speed = _isSprinting ? sprintSpeed : walkSpeed;

        if (_kb.spaceKey.wasPressedThisFrame && _isGrounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        _velocity.y += gravity * Time.deltaTime;

        _controller.Move(moveDir * speed * Time.deltaTime + _velocity * Time.deltaTime);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  STAMINA
    // ─────────────────────────────────────────────────────────────────────────
    private void HandleStamina()
    {
        if (_isSprinting)
        {
            // Drain 1% of current stamina per frame while sprinting
            float drainAmount = _currentStamina * 0.01f;
            _currentStamina = Mathf.Max(_currentStamina - drainAmount, 0f);
        }
        else
        {
            _currentStamina = Mathf.Min(_currentStamina + staminaRegen * Time.deltaTime, maxStamina);
        }

        OnStaminaChanged?.Invoke(_currentStamina, maxStamina);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  MANA
    // ─────────────────────────────────────────────────────────────────────────
    private void HandleMana()
    {
        // Regenerate mana if not used recently
        if (Time.time - _lastManaUseTime > manaRegenDelay)
        {
            _currentMana = Mathf.Min(_currentMana + manaRegen * Time.deltaTime, maxMana);
            OnManaChanged?.Invoke(_currentMana, maxMana);
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
