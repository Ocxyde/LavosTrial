// BehaviorEngine.cs
// Base class for all items in ItemEngine system - Plug-in-and-Out architecture
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// All item behaviors should inherit from this class and plug into ItemEngine

using UnityEngine;
using Code.Lavos.Core;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Base class for all interactable items.
    /// Inherit from this class to create plug-in items for ItemEngine.
    /// </summary>
    public abstract class BehaviorEngine : MonoBehaviour
    {
        [Header("Item Settings")]
        [SerializeField] protected ItemType itemType = ItemType.Generic;
        [SerializeField] protected bool canInteract = true;
        [SerializeField] protected bool canCollect = false;
        [SerializeField] protected bool destroyOnCollect = false;
        [SerializeField] protected float interactionRange = 3f;

        [Header("Visual Feedback")]
        [SerializeField] protected GameObject interactPrompt;
        [SerializeField] protected GameObject collectedEffect;

        [Header("Audio")]
        [SerializeField] protected AudioClip interactSound;
        [SerializeField] protected AudioClip collectSound;

        // State
        protected bool _isInteracted = false;
        protected bool _isCollected = false;
        protected bool _isEnabled = true;

        // Events
        public System.Action<BehaviorEngine, GameObject> OnInteract;
        public System.Action<BehaviorEngine, GameObject> OnCollect;

        // Properties
        public ItemType ItemType => itemType;
        public bool CanInteract => canInteract && _isEnabled && !_isCollected;
        public bool CanCollect => canCollect && _isEnabled && !_isCollected;
        public bool DestroyOnCollect => destroyOnCollect;
        public bool IsCollected => _isCollected;
        public bool IsEnabled => _isEnabled;

        protected virtual void Awake()
        {
            // Auto-register with ItemEngine on start
            ItemEngine.Instance?.RegisterItem(this);
        }

        /// <summary>
        /// Interact with this item.
        /// Override in derived classes for specific behavior.
        /// </summary>
        public virtual void Interact(GameObject interactor)
        {
            if (!CanInteract)
            {
                Debug.LogWarning($"[BehaviorEngine] Cannot interact with {itemType}");
                return;
            }

            _isInteracted = true;

            // Play sound
            if (interactSound != null)
            {
                AudioSource.PlayClipAtPoint(interactSound, transform.position);
            }

            // Show prompt
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(true);
                Invoke(nameof(HidePrompt), 1f);
            }

            OnInteract?.Invoke(this, interactor);
            Debug.Log($"[BehaviorEngine] Interacted with {itemType}");
        }

        /// <summary>
        /// Collect this item.
        /// Override in derived classes for specific behavior.
        /// </summary>
        public virtual void Collect(GameObject collector)
        {
            if (!CanCollect)
            {
                Debug.LogWarning($"[BehaviorEngine] Cannot collect {itemType}");
                return;
            }

            _isCollected = true;

            // Spawn collect effect
            if (collectedEffect != null)
            {
                Instantiate(collectedEffect, transform.position, Quaternion.identity);
            }

            // Play sound
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
            }

            OnCollect?.Invoke(this, collector);
            Debug.Log($"[BehaviorEngine] Collected {itemType}");

            // Destroy if set
            if (destroyOnCollect)
            {
                Destroy(gameObject);
            }
            else
            {
                _isEnabled = false;
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Enable this item.
        /// </summary>
        public virtual void Enable()
        {
            _isEnabled = true;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Disable this item.
        /// </summary>
        public virtual void Disable()
        {
            _isEnabled = false;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Reset item to initial state.
        /// </summary>
        public virtual void Reset()
        {
            _isInteracted = false;
            _isCollected = false;
            _isEnabled = true;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Check if interactor is in range.
        /// </summary>
        public bool IsInRange(Vector3 position)
        {
            return Vector3.Distance(position, transform.position) <= interactionRange;
        }

        /// <summary>
        /// Show interaction prompt.
        /// </summary>
        public void ShowPrompt()
        {
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(true);
            }
        }

        /// <summary>
        /// Hide interaction prompt.
        /// </summary>
        public void HidePrompt()
        {
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }

        /// <summary>
        /// Set item type at runtime.
        /// </summary>
        public void SetItemType(ItemType type)
        {
            itemType = type;
        }

        protected virtual void OnDestroy()
        {
            ItemEngine.Instance?.UnregisterItem(this);
        }

        protected virtual void OnDrawGizmosSelected()
        {
            // Draw interaction range
            Gizmos.color = CanInteract ? Color.green : Color.gray;
            Gizmos.DrawWireSphere(transform.position, interactionRange);

            // Draw item type indicator
            Gizmos.color = GetItemTypeColor();
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }

        protected Color GetItemTypeColor()
        {
            return itemType switch
            {
                ItemType.Door => Color.blue,
                ItemType.Chest => Color.yellow,
                ItemType.Pickup => Color.green,
                ItemType.Switch => Color.red,
                ItemType.Key => Color.magenta,
                _ => Color.gray
            };
        }
    }
}
