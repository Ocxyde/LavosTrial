// InteractionSystem.cs
// Centralized interaction manager for all game interactions
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Single point for all interactions - routes through EventHandler
// Handles: E-key interactions, combat actions, trap triggers, spell/weapon usage

using System;
using UnityEngine;
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
    /// </summary>
    public class InteractionSystem : MonoBehaviour
    {
        public static InteractionSystem Instance { get; private set; }

        #region Inspector Settings

        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private LayerMask interactionLayerMask = ~0;
        [SerializeField] private LayerMask playerLayerMask = 0;

        [Header("Input")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private KeyCode attackKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode spellKey = KeyCode.Mouse1;
        [SerializeField] private KeyCode useItemKey = KeyCode.F;

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
                playerController = FindFirstObjectByType<PlayerController>();
            if (playerStats == null)
                playerStats = FindFirstObjectByType<PlayerStats>();
            if (combatSystem == null)
                combatSystem = FindFirstObjectByType<CombatSystem>();
            if (inventory == null)
                inventory = FindFirstObjectByType<Inventory>();

            _interactionCamera = Camera.main;

            Debug.Log("[InteractionSystem] Initialized");
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
        /// Handle E-key interaction with objects
        /// </summary>
        private void HandleInteractionInput()
        {
            if (Input.GetKeyDown(interactKey) && _currentInteractable?.CanInteract(playerController) == true)
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
            // Primary attack (left click)
            if (Input.GetKeyDown(attackKey))
            {
                PerformAttack();
            }

            // Spell cast (right click)
            if (Input.GetKeyDown(spellKey))
            {
                PerformCastSpell();
            }

            // Use item (F key)
            if (Input.GetKeyDown(useItemKey))
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

            // Check if we have a weapon equipped
            var weapon = inventory?.EquippedWeapon;
            
            if (weapon != null)
            {
                // Weapon attack - consume stamina if needed
                float staminaCost = weapon.staminaCost > 0 ? weapon.staminaCost : 5f;
                
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
        /// Cast a spell
        /// </summary>
        private void PerformCastSpell()
        {
            if (combatSystem == null || playerStats == null) return;

            // Check if we have a spell equipped/selected
            var spell = inventory?.EquippedSpell;
            
            if (spell != null)
            {
                float manaCost = spell.manaCost;
                
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
                    
                    // Broadcast failure through EventHandler
                    if (EventHandler.Instance != null)
                    {
                        // Could add OnSpellFailed event here
                    }
                }
            }
            else
            {
                Debug.LogWarning("[InteractionSystem] No spell equipped");
            }
        }

        /// <summary>
        /// Use an item from inventory
        /// </summary>
        private void PerformUseItem()
        {
            if (inventory == null || playerStats == null) return;

            // Use selected/first usable item
            var item = inventory.GetSelectedItem();
            
            if (item != null)
            {
                if (inventory.UseItem(item))
                {
                    // Item used successfully
                    if (EventHandler.Instance != null)
                    {
                        EventHandler.Instance.InvokeItemUsed(item, 1);
                    }
                    
                    OnInteractionPerformed?.Invoke("Use_Item", playerController.gameObject);
                }
            }
            else
            {
                Debug.LogWarning("[InteractionSystem] No item selected to use");
            }
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
                        float damage = weapon.damage > 0 ? weapon.damage : 10f;
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
                        float damage = spell.damage > 0 ? spell.damage : 15f;
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
