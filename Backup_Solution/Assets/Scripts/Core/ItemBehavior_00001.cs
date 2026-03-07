// ItemBehavior.cs
// Base class for all items in ItemEngine system
// Unity 6 compatible - UTF-8 encoding - Unix line endings
// Located in Core assembly for inheritance by DoubleDoor, ChestBehavior, etc.

using UnityEngine;
using Code.Lavos.Core;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Base class for all interactable items.
    /// All items (doors, chests, pickups, switches) should inherit from this class.
    /// </summary>
    public abstract class ItemBehavior : MonoBehaviour
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
        public System.Action<ItemBehavior, GameObject> OnInteract;
        public System.Action<ItemBehavior, GameObject> OnCollect;

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
            if (ItemEngine.Instance != null)
            {
                ItemEngine.Instance.RegisterItem(this);
            }
        }

        /// <summary>
        /// Interact with the item.
        /// </summary>
        public virtual void Interact(GameObject interactor)
        {
            if (!CanInteract)
            {
                Debug.LogWarning($"[ItemBehavior] Cannot interact with {itemType}");
                return;
            }

            _isInteracted = true;
            OnInteract?.Invoke(this, interactor);

            if (interactSound != null && interactPrompt != null)
            {
                // AudioSource.PlayClipAtPoint(interactSound, transform.position);
            }

            Debug.Log($"[ItemBehavior] Interacted with {itemType}");
        }

        /// <summary>
        /// Collect the item.
        /// </summary>
        public virtual void Collect(GameObject collector)
        {
            if (!CanCollect)
            {
                Debug.LogWarning($"[ItemBehavior] Cannot collect {itemType}");
                return;
            }

            _isCollected = true;
            OnCollect?.Invoke(this, collector);

            if (collectSound != null && collectedEffect != null)
            {
                // Instantiate(collectedEffect, transform.position, Quaternion.identity);
            }

            Debug.Log($"[ItemBehavior] Collected {itemType}");

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
        /// Enable the item.
        /// </summary>
        public virtual void Enable()
        {
            _isEnabled = true;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Disable the item.
        /// </summary>
        public virtual void Disable()
        {
            _isEnabled = false;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Reset the item state.
        /// </summary>
        public virtual void Reset()
        {
            _isInteracted = false;
            _isCollected = false;
            _isEnabled = true;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Set the item type.
        /// </summary>
        public void SetItemType(ItemType type)
        {
            itemType = type;
        }

        protected virtual void OnDestroy()
        {
            if (ItemEngine.Instance != null)
            {
                ItemEngine.Instance.UnregisterItem(this);
            }
        }
    }
}
