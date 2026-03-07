// InteractionSystem.cs
// Centralized interaction manager for all game interactions
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Single point for all interactions - routes through EventHandler
// Handles: E-key interactions, combat actions, trap triggers, spell/weapon usage
// Uses New Input System

using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Code.Lavos.Status;

namespace Code.Lavos.Core
{
    /// <summary>
    /// InteractionSystem - Centralized manager for all player interactions.
    /// Replaces duplicate interaction logic with single event-driven system.
    ///
    /// Features:
    /// - E-key interactions with IInteractable objects
    /// - Combat actions (attack, cast, use item)
    /// - Trap triggers
    /// - Spell/weapon usage
    /// - All actions broadcast through EventHandler
    /// - New Input System compatible
    /// </summary>
    public class InteractionSystem : MonoBehaviour
    {
        public static InteractionSystem Instance { get; private set; }

        #region Inspector Settings

        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private LayerMask interactionLayerMask = ~0;
        [SerializeField] private LayerMask playerLayerMask = 0;

        [Header("Input Actions")]
        [SerializeField] private InputActionReference interactAction;
        [SerializeField] private InputActionReference attackAction;
        [SerializeField] private InputActionReference spellAction;
        [SerializeField] private InputActionReference useItemAction;

        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private CombatSystem combatSystem;
        [SerializeField] private Inventory inventory;

        #endregion

        #region Private Fields

        private IInteractable _currentInteractable;
        private IInteractable _highlightedInteractable;
        private Camera _interactionCamera;

        // Input actions
        private InputAction _interactAction;
        private InputAction _attackAction;
        private InputAction _spellAction;
        private InputAction _useItemAction;

        #endregion

        #region Properties

        public IInteractable CurrentInteractable => _currentInteractable;
        public bool HasInteractable => _currentInteractable != null;
        public float InteractionRange => interactionRange;

        #endregion

        #region Events

        /// <summary>
        /// Fired when interactable object changes (for UI prompts)
        /// </summary>
        public event Action<string> OnInteractableChanged;
        
        /// <summary>
        /// Static event for PlayerController to subscribe to (legacy compatibility)
        /// </summary>
        public static event System.Action<string> OnInteractableChangedStatic;

        /// <summary>
        /// Fired when any interaction occurs (for logging/achievements)
        /// </summary>
        public event Action<string, GameObject> OnInteractionPerformed;

        #endregion

        #region Unity Lifecycle

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Auto-find references if not assigned
            if (playerController == null)
            {
                playerController = FindFirstObjectByType<PlayerController>();
                if (playerController == null)
                    Debug.LogWarning("[InteractionSystem] PlayerController not found!");
            }
            if (playerStats == null)
            {
                playerStats = FindFirstObjectByType<PlayerStats>();
                if (playerStats == null)
                    Debug.LogWarning("[InteractionSystem] PlayerStats not found!");
            }
            if (combatSystem == null)
            {
                combatSystem = FindFirstObjectByType<CombatSystem>();
                if (combatSystem == null)
                    Debug.LogWarning("[InteractionSystem] CombatSystem not found!");
            }
            if (inventory == null)
            {
                inventory = FindFirstObjectByType<Inventory>();
                if (inventory == null)
                    Debug.LogWarning("[InteractionSystem] Inventory not found!");
            }

            _interactionCamera = Camera.main;

            // Setup input actions
            SetupInputActions();

            Debug.Log("[InteractionSystem] Initialized");
        }

        void OnEnable()
        {
            // Enable input actions
            _interactAction?.Enable();
            _attackAction?.Enable();
            _spellAction?.Enable();
            _useItemAction?.Enable();
        }

        void OnDisable()
        {
            // Disable input actions
            _interactAction?.Disable();
            _attackAction?.Disable();
            _spellAction?.Disable();
            _useItemAction?.Disable();
        }

        void Update()
        {
            if (GameManager.Instance == null ||
                GameManager.Instance.CurrentState != GameManager.GameState.Playing)
                return;

            if (playerController == null) return;

            HandleInteractionInput();
            HandleCombatInput();
            CheckForInteractable();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        #endregion

        #region Interaction Input Handling

        /// <summary>
        /// Setup input action references
        /// </summary>
        private void SetupInputActions()
        {
            // Try to get actions from InputActionReferences
            _interactAction = interactAction?.action ?? InputSystem.FindAction("Player/Interact");
            _attackAction = attackAction?.action ?? InputSystem.FindAction("Player/Attack");
            _spellAction = spellAction?.action ?? InputSystem.FindAction("Player/Look"); // Fallback
            _useItemAction = useItemAction?.action ?? InputSystem.FindAction("Player/Previous"); // Fallback

            // If still null, create from PlayerInput component on player
            if (_interactAction == null || _attackAction == null)
            {
                var playerInput = playerController?.GetComponent<PlayerInput>();
                if (playerInput != null)
                {
                    _interactAction ??= playerInput.actions.Find("Interact");
                    _attackAction ??= playerInput.actions.Find("Attack");
                }
            }

            Debug.Log("[InteractionSystem] Input actions setup complete");
        }

        /// <summary>
        /// Handle E-key interaction with objects
        /// </summary>
        private void HandleInteractionInput()
        {
            bool interactPressed = _interactAction?.IsPressed() == true ||
                                 Keyboard.current?.eKey?.isPressed == true;

            if (interactPressed && _currentInteractable?.CanInteract(playerController) == true)
            {
                PerformInteraction(_currentInteractable);
            }
        }

        /// <summary>
        /// Perform interaction with an interactable object
        /// </summary>
        private void PerformInteraction(IInteractable interactable)
        {
            if (interactable == null) return;

            // Broadcast through EventHandler
            if (EventHandler.Instance != null)
            {
                EventHandler.Instance.InvokeItemUsed(
                    new ItemData { itemName = interactable.InteractionPrompt },
                    1
                );
            }

            // Execute interaction
            interactable.OnInteract(playerController);

            // Log interaction
            OnInteractionPerformed?.Invoke("Interact", playerController?.gameObject);

            Debug.Log($"[InteractionSystem] Interacted with: {interactable.InteractionPrompt}");
        }

        #endregion

        #region Combat Input Handling

        /// <summary>
        /// Handle combat actions (attack, cast, use item)
        /// </summary>
        private void HandleCombatInput()
        {
            // Primary attack (left click or gamepad button)
            bool attackPressed = _attackAction?.IsPressed() == true ||
                                Mouse.current?.leftButton?.isPressed == true ||
                                Keyboard.current?.enterKey?.isPressed == true;

            if (attackPressed)
            {
                PerformAttack();
            }

            // Spell cast (right click or secondary button)
            bool spellPressed = _spellAction?.IsPressed() == true ||
                               Mouse.current?.rightButton?.isPressed == true;

            if (spellPressed)
            {
                PerformCastSpell();
            }

            // Use item (F key or gamepad button)
            bool useItemPressed = _useItemAction?.IsPressed() == true ||
                                 Keyboard.current?.fKey?.isPressed == true;

            if (useItemPressed)
            {
                PerformUseItem();
            }
        }

        /// <summary>
        /// Perform basic attack
        /// </summary>
        private void PerformAttack()
        {
            if (combatSystem == null || playerStats == null) return;

            // Check if we have a weapon equipped (first equipment item in inventory)
            ItemData weapon = GetEquippedWeapon();
            
            if (weapon != null)
            {
                // Weapon attack - consume stamina if needed
                float staminaCost = weapon.damageBonus > 0 ? 8f : 5f; // Base cost for weapon attacks
                
                if (playerStats.CurrentStamina >= staminaCost)
                {
                    playerStats.UseStamina(staminaCost);
                    
                    // Perform weapon attack (raycast or animation)
                    ExecuteWeaponAttack(weapon);
                    
                    OnInteractionPerformed?.Invoke("Attack_Weapon", playerController.gameObject);
                }
                else
                {
                    Debug.LogWarning("[InteractionSystem] Not enough stamina for weapon attack");
                }
            }
            else
            {
                // Basic unarmed attack
                ExecuteUnarmedAttack();
                OnInteractionPerformed?.Invoke("Attack_Unarmed", playerController.gameObject);
            }
        }

        /// <summary>
        /// Get equipped weapon from inventory
        /// </summary>
        private ItemData GetEquippedWeapon()
        {
            if (inventory == null) return null;
            
            // Find first equipment item with damage bonus
            var allSlots = inventory.GetAllSlots();
            foreach (var slot in allSlots)
            {
                if (!slot.IsEmpty && slot.item.itemType == InventoryItemType.Equipment)
                {
                    // Check if it's a weapon (has damage bonus)
                    if (slot.item.damageBonus > 0f)
                    {
                        return slot.item;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Cast a spell
        /// </summary>
        private void PerformCastSpell()
        {
            if (combatSystem == null || playerStats == null) return;

            // Check if we have a spell equipped ( Equipment item with mana cost association)
            ItemData spell = GetEquippedSpell();
            
            if (spell != null)
            {
                // Use manaRestore field as mana cost indicator (convention for spell items)
                float manaCost = spell.manaRestore > 0 ? spell.manaRestore : 20f;
                
                if (playerStats.CurrentMana >= manaCost)
                {
                    playerStats.UseMana(manaCost);
                    
                    // Cast the spell
                    ExecuteSpellCast(spell);
                    
                    // Broadcast through EventHandler
                    if (EventHandler.Instance != null)
                    {
                        EventHandler.Instance.InvokePlayerManaUsed(manaCost);
                    }
                    
                    OnInteractionPerformed?.Invoke("Cast_Spell", playerController.gameObject);
                }
                else
                {
                    Debug.LogWarning("[InteractionSystem] Not enough mana to cast spell");
                }
            }
            else
            {
                Debug.LogWarning("[InteractionSystem] No spell equipped");
            }
        }

        /// <summary>
        /// Get equipped spell from inventory
        /// </summary>
        private ItemData GetEquippedSpell()
        {
            if (inventory == null) return null;
            
            // Find first equipment item that could be a spell (has mana restore value)
            var allSlots = inventory.GetAllSlots();
            foreach (var slot in allSlots)
            {
                if (!slot.IsEmpty && slot.item.itemType == InventoryItemType.Equipment)
                {
                    // Check if it's a spell (has mana restore - used as mana cost indicator)
                    if (slot.item.manaRestore > 0f || slot.item.damageBonus > 0f)
                    {
                        return slot.item;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Use an item from inventory
        /// </summary>
        private void PerformUseItem()
        {
            if (inventory == null || playerStats == null) return;

            // Use first usable item (non-equipment, consumable)
            ItemData item = GetFirstUsableItem();
            
            if (item != null)
            {
                // Find the slot index for this item
                var allSlots = inventory.GetAllSlots();
                for (int i = 0; i < allSlots.Count; i++)
                {
                    if (allSlots[i].item == item)
                    {
                        if (inventory.UseItem(i, playerController?.gameObject))
                        {
                            // Item used successfully
                            if (EventHandler.Instance != null)
                            {
                                EventHandler.Instance.InvokeItemUsed(item, 1);
                            }
                            
                            OnInteractionPerformed?.Invoke("Use_Item", playerController?.gameObject);
                        }
                        return;
                    }
                }
            }
            else
            {
                Debug.LogWarning("[InteractionSystem] No item selected to use");
            }
        }

        /// <summary>
        /// Get first usable item from inventory
        /// </summary>
        private ItemData GetFirstUsableItem()
        {
            if (inventory == null) return null;
            
            // Find first consumable item
            var allSlots = inventory.GetAllSlots();
            foreach (var slot in allSlots)
            {
                if (!slot.IsEmpty && slot.item.itemType == InventoryItemType.Consumable)
                {
                    return slot.item;
                }
            }
            return null;
        }

        #endregion

        #region Interaction Detection

        /// <summary>
        /// Check for interactable objects in front of player
        /// </summary>
        private void CheckForInteractable()
        {
            if (_interactionCamera == null) return;

            Ray ray = new Ray(_interactionCamera.transform.position, _interactionCamera.transform.forward);
            LayerMask mask = interactionLayerMask & ~playerLayerMask;

            if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, mask))
            {
                IInteractable found = hit.collider.GetComponent<IInteractable>();
                
                if (found != null)
                {
                    if (_highlightedInteractable != found)
                    {
                        _highlightedInteractable?.OnHighlightExit(playerController);
                        _highlightedInteractable = found;
                        _highlightedInteractable.OnHighlightEnter(playerController);
                    }
                    
                    _currentInteractable = found;
                    OnInteractableChanged?.Invoke(found.InteractionPrompt);
                    OnInteractableChangedStatic?.Invoke(found.InteractionPrompt);
                    return;
                }
            }

            // Nothing found
            _highlightedInteractable?.OnHighlightExit(playerController);
            _highlightedInteractable = null;
            _currentInteractable = null;
            OnInteractableChanged?.Invoke(string.Empty);
            OnInteractableChangedStatic?.Invoke(string.Empty);
        }

        #endregion

        #region Combat Execution Methods

        /// <summary>
        /// Execute weapon attack (override for custom attack logic)
        /// </summary>
        protected virtual void ExecuteWeaponAttack(ItemData weapon)
        {
            // Simple raycast attack
            if (_interactionCamera != null)
            {
                Ray ray = new Ray(_interactionCamera.transform.position, _interactionCamera.transform.forward);
                
                if (Physics.Raycast(ray, out RaycastHit hit, 5f))
                {
                    var enemy = hit.collider.GetComponent<Ennemi>();
                    if (enemy != null && combatSystem != null)
                    {
                        // Use damageBonus field from ItemData
                        float damage = weapon.damageBonus > 0 ? weapon.damageBonus : 10f;
                        var damageInfo = new DamageInfo(damage, DamageType.Physical);
                        combatSystem.DealDamage(playerController?.gameObject, enemy.gameObject, damageInfo);
                    }
                }
            }
        }

        /// <summary>
        /// Execute unarmed attack
        /// </summary>
        protected virtual void ExecuteUnarmedAttack()
        {
            // Very weak basic attack
            if (_interactionCamera != null)
            {
                Ray ray = new Ray(_interactionCamera.transform.position, _interactionCamera.transform.forward);
                
                if (Physics.Raycast(ray, out RaycastHit hit, 3f))
                {
                    var enemy = hit.collider.GetComponent<Ennemi>();
                    if (enemy != null && combatSystem != null)
                    {
                        var damageInfo = new DamageInfo(2f, DamageType.Physical); // Weak damage
                        combatSystem.DealDamage(playerController?.gameObject, enemy.gameObject, damageInfo);
                    }
                }
            }
        }

        /// <summary>
        /// Execute spell cast (override for custom spell logic)
        /// </summary>
        protected virtual void ExecuteSpellCast(ItemData spell)
        {
            // Simple projectile or hitscan spell
            if (_interactionCamera != null)
            {
                Ray ray = new Ray(_interactionCamera.transform.position, _interactionCamera.transform.forward);
                
                if (Physics.Raycast(ray, out RaycastHit hit, 10f))
                {
                    var enemy = hit.collider.GetComponent<Ennemi>();
                    if (enemy != null && combatSystem != null)
                    {
                        // Use damageBonus field from ItemData
                        float damage = spell.damageBonus > 0 ? spell.damageBonus : 15f;
                        var damageInfo = new DamageInfo(damage, DamageType.Magic);
                        combatSystem.DealDamage(playerController?.gameObject, enemy.gameObject, damageInfo);
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Manually trigger interaction with current object
        /// </summary>
        public void Interact()
        {
            if (_currentInteractable != null)
            {
                PerformInteraction(_currentInteractable);
            }
        }

        /// <summary>
        /// Manually trigger attack
        /// </summary>
        public void Attack()
        {
            PerformAttack();
        }

        /// <summary>
        /// Manually trigger spell cast
        /// </summary>
        public void CastSpell()
        {
            PerformCastSpell();
        }

        /// <summary>
        /// Manually trigger item use
        /// </summary>
        public void UseItem()
        {
            PerformUseItem();
        }

        /// <summary>
        /// Set the interaction camera (for VR or multi-camera setups)
        /// </summary>
        public void SetInteractionCamera(Camera camera)
        {
            _interactionCamera = camera;
        }

        /// <summary>
        /// Set interaction range at runtime
        /// </summary>
        public void SetInteractionRange(float range)
        {
            interactionRange = range;
        }

        #endregion

        #region Utility

        /// <summary>
        /// Clear current interaction (for debugging or state reset)
        /// </summary>
        public void ClearInteraction()
        {
            _highlightedInteractable?.OnHighlightExit(playerController);
            _highlightedInteractable = null;
            _currentInteractable = null;
            OnInteractableChanged?.Invoke(string.Empty);
        }

        #endregion
    }
}
