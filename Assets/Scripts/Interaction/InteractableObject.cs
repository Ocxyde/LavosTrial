// InteractableObject.cs
// Base class for interactable objects
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Interaction system - implements IInteractable

using UnityEngine;

namespace Code.Lavos.Core
{
    [RequireComponent(typeof(Collider))]
    public abstract class InteractableObject : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField] protected string interactionPrompt = "Interact";
        [SerializeField] protected bool canInteract = true;

        public virtual string InteractionPrompt => interactionPrompt;

        public virtual bool CanInteract(PlayerController player)
        {
            return canInteract && player != null;
        }

        public abstract void OnInteract(PlayerController player);

        public virtual void OnHighlightEnter(PlayerController player)
        {
        }

        public virtual void OnHighlightExit(PlayerController player)
        {
        }

        protected virtual void Awake()
        {
            Collider collider = GetComponent<Collider>();
            if (collider != null && !collider.isTrigger)
                collider.isTrigger = true;
        }
    }
}
