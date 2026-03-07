// DoorsEngine.cs
// Randomized door system with low traps and interactions
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Plug-in-and-Out Architecture:
// - Inherits from BehaviorEngine for ItemEngine integration
// - Event-driven via EventHandler
// - Modular door types (trap, locked, secret, etc.)

using System;
using UnityEngine;
using Code.Lavos.Status;

#pragma warning disable CS0414 // Disable warnings for unused serialized fields (reserved for future features)

namespace Code.Lavos.Core
{
    /// <summary>
    /// Door types with different behaviors and interactions.
    /// </summary>
    public enum DoorVariant
    {
        Normal,         // Standard door, opens normally
        Locked,         // Requires key to open
        Trapped,        // Triggers trap when opened
        Secret,         // Hidden door, reveals secret area
        OneWay,         // Can only pass through one direction
        Cursed,         // Applies debuff when opened
        Blessed,        // Applies buff when opened
        Boss            // Boss door, special effects
    }

    /// <summary>
    /// Trap types that can be attached to doors.
    /// </summary>
    public enum DoorTrapType
    {
        None,
        Spike,          // Deals physical damage
        Fire,           // Deals fire damage
        Poison,         // Applies poison DoT
        Freeze,         // Slows/freezes player
        Shock,          // Deals lightning damage
        Teleport,       // Teleports player elsewhere
        Alarm,          // Alerts enemies
        Collapse        // Blocks passage after use
    }

    /// <summary>
    /// Procedural door system with randomized variants and traps.
    /// Plug-in-and-Out: Inherits from BehaviorEngine for ItemEngine integration.
    /// Implements IInteractable for E-key interaction.
    /// </summary>
    public class DoorsEngine : BehaviorEngine, IInteractable
    {
        #region Inspector Fields

        [Header("Door Settings")]
        [SerializeField] private DoorVariant doorVariant = DoorVariant.Normal;
        [SerializeField] private DoorTrapType doorTrap = DoorTrapType.None;
        [SerializeField] private float trapDamage = 10f;
        [SerializeField] private float trapDuration = 3f;  // Reserved for future DoT implementation
        
        [Header("Visual")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color lockedColor = Color.red;
        [SerializeField] private Color trappedColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color secretColor = new Color(0.5f, 0.5f, 1f);
        
        [Header("Audio")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        [SerializeField] private AudioClip lockedSound;
        [SerializeField] private AudioClip trapSound;

        [Header("Interaction")]
        // Note: interactionRange inherited from BehaviorEngine (not serialized here to avoid conflict)
        [SerializeField] private bool requireKey = false;
        [SerializeField] private string requiredKeyName = "DoorKey";  // Reserved for future key system
        
        [Header("Events")]
        [SerializeField] private bool broadcastOnOpen = true;
        [SerializeField] private bool broadcastOnClose = true;
        [SerializeField] private bool broadcastOnTrapTrigger = true;

        #endregion

        #region Private Fields

        private bool _isOpen = false;
        private bool _isLocked = false;
        private bool _trapTriggered = false;
        private Material _doorMaterial;
        private Collider _doorCollider;
        private EventHandler _eventHandler;
        private PlayerStats _playerStats;
        private DoorAnimation _doorAnimation;

        #endregion

        #region Properties

        public DoorVariant Variant => doorVariant;
        public DoorTrapType Trap => doorTrap;
        public bool IsOpen => _isOpen;
        public bool IsLocked => _isLocked;
        public bool TrapTriggered => _trapTriggered;
        public bool IsAnimating => _doorAnimation != null && _doorAnimation.IsAnimating;

        #endregion

        #region Unity Lifecycle

        private new void Awake()
        {
            // Set item type for ItemEngine
            SetItemType(ItemType.Door);
            
            // Get references
            _eventHandler = FindFirstObjectByType<EventHandler>();
            
            // Get or add DoorAnimation component
            _doorAnimation = GetComponent<DoorAnimation>();
            if (_doorAnimation == null)
            {
                _doorAnimation = gameObject.AddComponent<DoorAnimation>();
            }
            
            // Setup door
            SetupDoor();
            SetupCollider();
            
            // Call base Awake
            base.Awake();
        }

        private new void OnDestroy()
        {
            // Cleanup materials
            if (_doorMaterial != null)
                Destroy(_doorMaterial);
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Set panel references from DoorFactory.
        /// </summary>
        public void SetPanelReferences(Transform leftPanel, Transform rightPanel)
        {
            if (_doorAnimation != null)
            {
                _doorAnimation.SetPanelReferences(leftPanel, rightPanel);
            }
        }

        /// <summary>
        /// Initialize door with random variant and trap.
        /// Call this when spawning door procedurally.
        /// </summary>
        public void InitializeRandom()
        {
            // Randomize variant
            doorVariant = (DoorVariant)UnityEngine.Random.Range(0, Enum.GetValues(typeof(DoorVariant)).Length);
            
            // Randomize trap (30% chance)
            if (UnityEngine.Random.value < 0.3f)
            {
                doorTrap = (DoorTrapType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(DoorTrapType)).Length);
            }
            else
            {
                doorTrap = DoorTrapType.None;
            }

            // Setup based on variant
            SetupDoor();
        }

        /// <summary>
        /// Initialize door with specific settings.
        /// </summary>
        public void Initialize(DoorVariant variant, DoorTrapType trap, bool locked = false, bool openByDefault = false)
        {
            doorVariant = variant;
            doorTrap = trap;
            _isLocked = locked || (variant == DoorVariant.Locked);
            _isOpen = openByDefault;  // Set initial state

            SetupDoor();
        }

        /// <summary>
        /// Set door sounds from DoorSFXManager.
        /// </summary>
        public void SetDoorSounds(AudioClip openSfx, AudioClip closeSfx, AudioClip lockedSfx)
        {
            openSound = openSfx;
            closeSound = closeSfx;
            lockedSound = lockedSfx;
        }

        private void SetupDoor()
        {
            // Set visual appearance based on variant
            UpdateDoorVisual();

            // Set locked state
            if (doorVariant == DoorVariant.Locked || doorVariant == DoorVariant.Boss)
            {
                _isLocked = true;
            }

            // If door should start open, open it now
            if (_isOpen)
            {
                // Use animation if available
                if (_doorAnimation != null)
                {
                    _doorAnimation.Open();
                    DisableCollider();
                }
                else
                {
                    DisableCollider();
                }
            }
            else
            {
                // Door starts closed - enable collider
                SetupCollider();
            }
        }

        private void SetupCollider()
        {
            // Ensure door has collider for interaction
            _doorCollider = GetComponent<Collider>();
            if (_doorCollider == null)
            {
                _doorCollider = gameObject.AddComponent<BoxCollider>();
            }
        }

        private void UpdateDoorVisual()
        {
            // Get or create material
            var renderer = GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                renderer = gameObject.AddComponent<MeshRenderer>();
            }

            if (_doorMaterial == null)
            {
                _doorMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            }

            // Set color based on variant
            Color color = doorVariant switch
            {
                DoorVariant.Normal => normalColor,
                DoorVariant.Locked => lockedColor,
                DoorVariant.Trapped => trappedColor,
                DoorVariant.Secret => secretColor,
                DoorVariant.Blessed => new Color(0.5f, 1f, 0.5f),
                DoorVariant.Cursed => new Color(0.5f, 0f, 0.5f),
                _ => normalColor
            };

            _doorMaterial.color = color;
            renderer.sharedMaterial = _doorMaterial;
        }

        #endregion

        #region Interaction (Plug-in-and-Out)

        /// <summary>
        /// Override Interact from BehaviorEngine.
        /// Called when player interacts with door (E key).
        /// </summary>
        public override void Interact(GameObject interactor)
        {
            base.Interact(interactor);

            if (interactor == null) return;

            // Get player stats
            _playerStats = interactor.GetComponent<PlayerStats>();

            // Try to open door
            if (_isOpen)
            {
                CloseDoor(interactor);
            }
            else
            {
                OpenDoor(interactor);
            }
        }

        #endregion

        #region Door Logic

        /// <summary>
        /// Attempt to open the door.
        /// </summary>
        public void OpenDoor(GameObject interactor)
        {
            if (_isOpen) return;

            // Check if locked
            if (_isLocked)
            {
                HandleLockedDoor(interactor);
                return;
            }

            // Check for trap
            if (doorTrap != DoorTrapType.None && !_trapTriggered)
            {
                TriggerTrap(interactor);
            }

            // Open the door
            _isOpen = true;
            
            // Use animation if available
            if (_doorAnimation != null)
            {
                _doorAnimation.Open();
                // Disable collider after animation completes
                Invoke(nameof(DisableCollider), 0.5f);
            }
            else
            {
                DisableCollider();
            }

            // Broadcast event
            if (broadcastOnOpen && _eventHandler != null)
            {
                _eventHandler.InvokeDoorOpened(transform.position, doorVariant);
            }

            // Play sound
            if (openSound != null)
            {
                AudioSource.PlayClipAtPoint(openSound, transform.position);
            }

            // Apply variant effects
            ApplyDoorEffect(interactor);

            Debug.Log($"[DoorsEngine] Opened {doorVariant} door at {transform.position}");
        }

        /// <summary>
        /// Close the door.
        /// </summary>
        public void CloseDoor(GameObject interactor)
        {
            if (!_isOpen) return;

            _isOpen = false;
            
            // Use animation if available
            if (_doorAnimation != null)
            {
                _doorAnimation.Close();
                // Re-enable collider immediately
                EnableCollider();
            }
            else
            {
                EnableCollider();
            }

            // Broadcast event
            if (broadcastOnClose && _eventHandler != null)
            {
                _eventHandler.InvokeDoorClosed(transform.position, doorVariant);
            }

            Debug.Log($"[DoorsEngine] Closed {doorVariant} door at {transform.position}");
        }

        /// <summary>
        /// Handle locked door interaction.
        /// </summary>
        private void HandleLockedDoor(GameObject interactor)
        {
            // Check if player has key
            if (requireKey && _playerStats != null)
            {
                // TODO: Check inventory for key
                // For now, just unlock automatically for testing
                UnlockDoor(interactor);
                return;
            }

            // Play locked sound
            if (lockedSound != null)
            {
                AudioSource.PlayClipAtPoint(lockedSound, transform.position);
            }

            // Broadcast event
            if (_eventHandler != null)
            {
                _eventHandler.InvokeDoorLocked(transform.position);
            }

            Debug.Log($"[DoorsEngine] Door is locked at {transform.position}");
        }

        /// <summary>
        /// Unlock the door.
        /// </summary>
        public void UnlockDoor(GameObject interactor)
        {
            _isLocked = false;
            Debug.Log($"[DoorsEngine] Door unlocked at {transform.position}");
            
            // Try to open after unlocking
            OpenDoor(interactor);
        }

        #endregion

        #region IInteractable Implementation (E-Key Interaction)

        /// <summary>
        /// IInteractable: Get interaction prompt.
        /// </summary>
        public string InteractionPrompt => _isOpen ? "Close" : (_isLocked ? "Locked" : "Open");

        /// <summary>
        /// IInteractable: Check if player can interact.
        /// </summary>
        public new bool CanInteract(PlayerController player)
        {
            if (player == null) return false;

            // Check distance
            float distance = Vector3.Distance(player.transform.position, transform.position);
            return distance <= interactionRange;
        }

        /// <summary>
        /// IInteractable: Interact with door (E key).
        /// </summary>
        public new void OnInteract(PlayerController player)
        {
            if (!CanInteract(player)) return;

            // Call base Interact with player GameObject
            Interact(player.gameObject);
        }

        /// <summary>
        /// IInteractable: Show highlight when player looks at door.
        /// </summary>
        public void OnHighlightEnter(PlayerController player)
        {
            // Could add visual highlight effect here
            Debug.Log($"[DoorsEngine] Highlight enter: {InteractionPrompt}");
        }

        /// <summary>
        /// IInteractable: Hide highlight when player looks away.
        /// </summary>
        public void OnHighlightExit(PlayerController player)
        {
            // Could remove visual highlight effect here
        }

        #endregion

        #region Trap System

        /// <summary>
        /// Trigger door trap.
        /// </summary>
        private void TriggerTrap(GameObject interactor)
        {
            _trapTriggered = true;

            Debug.Log($"[DoorsEngine] Trap triggered: {doorTrap}");

            // Apply trap effect
            switch (doorTrap)
            {
                case DoorTrapType.Spike:
                    DealTrapDamage(interactor, DamageType.Physical);
                    break;
                case DoorTrapType.Fire:
                    DealTrapDamage(interactor, DamageType.Fire);
                    break;
                case DoorTrapType.Poison:
                    DealTrapDamage(interactor, DamageType.Poison);
                    ApplyPoison(interactor);
                    break;
                case DoorTrapType.Freeze:
                    DealTrapDamage(interactor, DamageType.Ice);
                    ApplySlow(interactor);
                    break;
                case DoorTrapType.Shock:
                    DealTrapDamage(interactor, DamageType.Lightning);
                    break;
                case DoorTrapType.Teleport:
                    TeleportPlayer(interactor);
                    break;
                case DoorTrapType.Alarm:
                    AlertEnemies();
                    break;
                case DoorTrapType.Collapse:
                    BlockPassage();
                    break;
            }

            // Broadcast event
            if (broadcastOnTrapTrigger && _eventHandler != null)
            {
                _eventHandler.InvokeDoorTrapTriggered(transform.position, doorTrap);
            }

            // Play trap sound
            if (trapSound != null)
            {
                AudioSource.PlayClipAtPoint(trapSound, transform.position);
            }
        }

        /// <summary>
        /// Deal trap damage to player.
        /// </summary>
        private void DealTrapDamage(GameObject target, DamageType type)
        {
            if (target == null) return;

            var playerStats = target.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(trapDamage, type);
            }

            // Broadcast damage event
            if (_eventHandler != null)
            {
                var damageInfo = new DamageInfo(trapDamage, type, "DoorTrap");
                _eventHandler.InvokeDamageTaken(damageInfo, trapDamage);
            }
        }

        /// <summary>
        /// Apply poison DoT to player.
        /// </summary>
        private void ApplyPoison(GameObject target)
        {
            // TODO: Apply poison status effect
            Debug.Log("[DoorsEngine] Poison applied");
        }

        /// <summary>
        /// Apply slow effect to player.
        /// </summary>
        private void ApplySlow(GameObject target)
        {
            // TODO: Apply slow status effect
            Debug.Log("[DoorsEngine] Slow applied");
        }

        /// <summary>
        /// Teleport player to random location.
        /// </summary>
        private void TeleportPlayer(GameObject target)
        {
            if (target == null) return;

            // Teleport slightly behind door
            Vector3 teleportPos = transform.position - transform.forward * 3f;
            target.transform.position = teleportPos;

            Debug.Log("[DoorsEngine] Player teleported");
        }

        /// <summary>
        /// Alert nearby enemies.
        /// </summary>
        private void AlertEnemies()
        {
            // TODO: Alert enemies in radius
            Debug.Log("[DoorsEngine] Enemies alerted!");
        }

        /// <summary>
        /// Block passage after door use.
        /// </summary>
        private void BlockPassage()
        {
            // TODO: Spawn blocking debris
            Debug.Log("[DoorsEngine] Passage blocked");
        }

        #endregion

        #region Variant Effects

        /// <summary>
        /// Apply special effect based on door variant.
        /// </summary>
        private void ApplyDoorEffect(GameObject interactor)
        {
            if (interactor == null) return;

            switch (doorVariant)
            {
                case DoorVariant.Blessed:
                    ApplyBlessing(interactor);
                    break;
                case DoorVariant.Cursed:
                    ApplyCurse(interactor);
                    break;
                case DoorVariant.Secret:
                    // TODO: Reveal secret area
                    break;
                case DoorVariant.Boss:
                    // TODO: Trigger boss encounter
                    break;
            }
        }

        private void ApplyBlessing(GameObject target)
        {
            // TODO: Apply buff
            Debug.Log("[DoorsEngine] Blessing applied");
        }

        private void ApplyCurse(GameObject target)
        {
            // TODO: Apply debuff
            Debug.Log("[DoorsEngine] Curse applied");
        }

        #endregion

        #region Utilities

        private void DisableCollider()
        {
            if (_doorCollider != null)
            {
                _doorCollider.enabled = false;
            }
        }

        private void EnableCollider()
        {
            if (_doorCollider != null)
            {
                _doorCollider.enabled = true;
            }
        }

        #endregion

        #region Gizmos

        private new void OnDrawGizmosSelected()
        {
            // Draw door bounds
            Gizmos.color = doorVariant switch
            {
                DoorVariant.Normal => Color.white,
                DoorVariant.Locked => Color.red,
                DoorVariant.Trapped => Color.orange,
                DoorVariant.Secret => Color.blue,
                _ => Color.white
            };

            Gizmos.DrawWireCube(transform.position, new Vector3(2f, 3f, 0.5f));

            // Draw trap indicator
            if (doorTrap != DoorTrapType.None)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(transform.position + Vector3.up * 2.5f, 0.3f);
            }
        }

        #endregion
    }
}
