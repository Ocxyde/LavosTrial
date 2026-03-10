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
        [SerializeField] protected ItemType ItemTypeValue = ItemType.Generic;
        [SerializeField] protected bool CanInteractValue = true;
        [SerializeField] protected bool CanCollectValue = false;
        [SerializeField] protected bool DestroyOnCollectValue = false;
        [SerializeField] protected float InteractionRangeValue = 3f;

        [Header("Visual Feedback")]
        [SerializeField] protected GameObject InteractPrompt;
        [SerializeField] protected GameObject CollectedEffect;

        [Header("Audio")]
        [SerializeField] protected AudioClip InteractSound;
        [SerializeField] protected AudioClip CollectSound;

        // State
        protected bool _isInteracted = false;
        protected bool _isCollected = false;
        protected bool _isEnabled = true;

        // Events
        public System.Action<BehaviorEngine, GameObject> OnInteract;
        public System.Action<BehaviorEngine, GameObject> OnCollect;

        // Properties
        public ItemType ItemType => ItemTypeValue;
        public bool CanInteract => CanInteractValue && _isEnabled && !_isCollected;
        public bool CanCollect => CanCollectValue && _isEnabled && !_isCollected;
        public bool DestroyOnCollect => DestroyOnCollectValue;
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
                Debug.LogWarning($"[BehaviorEngine] Cannot interact with {ItemType}");
                return;
            }

            _isInteracted = true;

            // Play sound
            if (InteractSound != null)
            {
                AudioSource.PlayClipAtPoint(InteractSound, transform.position);
            }

            // Show prompt
            if (InteractPrompt != null)
            {
                InteractPrompt.SetActive(true);
                Invoke(nameof(HidePrompt), 1f);
            }

            OnInteract?.Invoke(this, interactor);
            Debug.Log($"[BehaviorEngine] Interacted with {ItemType}");
        }

        /// <summary>
        /// Collect this item.
        /// Override in derived classes for specific behavior.
        /// </summary>
        public virtual void Collect(GameObject collector)
        {
            if (!CanCollect)
            {
                Debug.LogWarning($"[BehaviorEngine] Cannot collect {ItemType}");
                return;
            }

            _isCollected = true;

            // Spawn collect effect
            if (CollectedEffect != null)
            {
                Instantiate(CollectedEffect, transform.position, Quaternion.identity);
            }

            // Play sound
            if (CollectSound != null)
            {
                AudioSource.PlayClipAtPoint(CollectSound, transform.position);
            }

            OnCollect?.Invoke(this, collector);
            Debug.Log($"[BehaviorEngine] Collected {ItemType}");

            // Destroy if set
            if (DestroyOnCollectValue)
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
            return Vector3.Distance(position, transform.position) <= InteractionRangeValue;
        }

        /// <summary>
        /// Show interaction prompt.
        /// </summary>
        public void ShowPrompt()
        {
            if (InteractPrompt != null)
            {
                InteractPrompt.SetActive(true);
            }
        }

        /// <summary>
        /// Hide interaction prompt.
        /// </summary>
        public void HidePrompt()
        {
            if (InteractPrompt != null)
            {
                InteractPrompt.SetActive(false);
            }
        }

        /// <summary>
        /// Set item type at runtime.
        /// </summary>
        public void SetItemType(ItemType type)
        {
            ItemTypeValue = type;
        }

        protected virtual void OnDestroy()
        {
            ItemEngine.Instance?.UnregisterItem(this);
        }

        protected virtual void OnDrawGizmosSelected()
        {
            // Draw interaction range
            Gizmos.color = CanInteract ? Color.green : Color.gray;
            Gizmos.DrawWireSphere(transform.position, InteractionRangeValue);

            // Draw item type indicator
            Gizmos.color = GetItemTypeColor();
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }

        protected Color GetItemTypeColor()
        {
            return ItemTypeValue switch
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
