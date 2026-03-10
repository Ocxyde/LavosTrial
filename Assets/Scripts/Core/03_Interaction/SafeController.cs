// Copyright (C) 2026 Ocxyde
//
// This file is part of Code.Lavos.
//
// Code.Lavos is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// SafeController.cs
// Main orchestrator for safe interaction, item distribution, and destruction timer
// Unity 6000.10f1 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Manages safe interaction via F key, item pickup, animation, and self-destruction after 60 seconds

using UnityEngine;
using UnityEngine.InputSystem;
using Code.Lavos.Core;

namespace Code.Lavos.Interaction
{
    public class SafeController : MonoBehaviour
    {
        [Header("Safe Configuration")]
        [SerializeField] private float _destructionDelaySeconds = 60f;
        [SerializeField] private bool _debugMode = false;

        [Header("Component References")]
        private SafeAnimationController _animationController;
        private SafeItemContainer _itemContainer;
        private SafeInteractionTrigger _interactionTrigger;
        private EventHandler _eventHandler;

        private bool _hasBeenInteracted = false;
        private float _destructionTimer = 0f;
        private bool _isBeingDestroyed = false;

        private void Awake()
        {
            ValidateComponents();
        }

        private void Start()
        {
            FindAndInitializeComponents();
        }

        private void Update()
        {
            HandleInteractionInput();
            HandleDestructionTimer();
        }

        private void ValidateComponents()
        {
            if (_animationController == null)
                _animationController = GetComponent<SafeAnimationController>();

            if (_itemContainer == null)
                _itemContainer = GetComponent<SafeItemContainer>();

            if (_interactionTrigger == null)
                _interactionTrigger = GetComponent<SafeInteractionTrigger>();

            if (_debugMode)
            {
                if (_animationController == null)
                    Debug.LogWarning("[SafeController] SafeAnimationController not found on this GameObject");
                if (_itemContainer == null)
                    Debug.LogWarning("[SafeController] SafeItemContainer not found on this GameObject");
                if (_interactionTrigger == null)
                    Debug.LogWarning("[SafeController] SafeInteractionTrigger not found on this GameObject");
            }
        }

        private void FindAndInitializeComponents()
        {
            _eventHandler = FindFirstObjectByType<EventHandler>();

            if (_eventHandler == null && _debugMode)
                Debug.LogWarning("[SafeController] EventHandler not found in scene");
        }

        private void HandleInteractionInput()
        {
            if (_hasBeenInteracted || _isBeingDestroyed)
                return;

            if (!CanPlayerInteract())
                return;

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.fKey.wasPressedThisFrame)
            {
                OnPlayerInteraction();
            }
        }

        private bool CanPlayerInteract()
        {
            if (_interactionTrigger == null)
                return false;

            return _interactionTrigger.IsPlayerInRange();
        }

        private void OnPlayerInteraction()
        {
            if (_debugMode)
                Debug.Log("[SafeController] Player interaction detected (F key)");

            _hasBeenInteracted = true;

            if (_animationController != null)
                _animationController.PlayOpenAnimation();

            if (_itemContainer != null)
                _itemContainer.DistributeItems();

            BroadcastSafeOpened();

            _destructionTimer = _destructionDelaySeconds;
        }

        private void HandleDestructionTimer()
        {
            if (!_hasBeenInteracted || _isBeingDestroyed)
                return;

            _destructionTimer -= Time.deltaTime;

            if (_debugMode && _destructionTimer > 0f && _destructionTimer < 2f)
                Debug.Log($"[SafeController] Safe will be destroyed in {_destructionTimer:F2} seconds");

            if (_destructionTimer <= 0f)
            {
                DestroySafe();
            }
        }

        private void BroadcastSafeOpened()
        {
            if (_eventHandler == null)
            {
                if (_debugMode)
                    Debug.LogWarning("[SafeController] Cannot broadcast safe opened - EventHandler not found");
                return;
            }

            // Broadcast safe opened event with safe's position
            _eventHandler.InvokeSafeOpened(transform.position);
            
            if (_debugMode)
                Debug.Log($"[SafeController] Broadcasted OnSafeOpened event at {transform.position}");
        }

        private void DestroySafe()
        {
            if (_isBeingDestroyed)
                return;

            _isBeingDestroyed = true;

            if (_debugMode)
                Debug.Log("[SafeController] Destroying safe after 60 seconds");

            Destroy(gameObject);
        }

        public float GetRemainingDestructionTime()
        {
            if (!_hasBeenInteracted)
                return -1f;

            return Mathf.Max(0f, _destructionTimer);
        }

        public bool HasBeenInteracted()
        {
            return _hasBeenInteracted;
        }
    }
}
