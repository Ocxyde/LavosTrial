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
