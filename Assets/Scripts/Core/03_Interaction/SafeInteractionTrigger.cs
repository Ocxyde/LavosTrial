// Copyright (C) 2026 Ocxyde
//
// This file is part of Code.Lavos.
//
// Code.Lavos is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// SafeInteractionTrigger.cs
// Detects player proximity for safe interaction
// Unity 6000.10f1 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Manages collision-based player detection for interaction trigger range

using UnityEngine;

namespace Code.Lavos.Interaction
{
    public class SafeInteractionTrigger : MonoBehaviour
    {
        [Header("Trigger Configuration")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private bool debugMode = false;

        private Transform playerTransform;
        private bool isPlayerInRange = false;

        private void Start()
        {
            FindPlayer();
            SetupTriggerCollider();
        }

        private void Update()
        {
            UpdatePlayerProximity();
        }

        private void FindPlayer()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                playerTransform = playerObject.transform;
            else if (debugMode)
                Debug.LogWarning("[SafeInteractionTrigger] Player GameObject with 'Player' tag not found");
        }

        private void SetupTriggerCollider()
        {
            Collider triggerCollider = GetComponent<Collider>();

            if (triggerCollider == null)
            {
                BoxCollider newCollider = gameObject.AddComponent<BoxCollider>();
                newCollider.isTrigger = true;
                newCollider.size = new Vector3(interactionRange * 2f, 2f, interactionRange * 2f);

                if (debugMode)
                    Debug.Log("[SafeInteractionTrigger] Created trigger collider");
            }
            else if (!triggerCollider.isTrigger)
            {
                triggerCollider.isTrigger = true;

                if (debugMode)
                    Debug.Log("[SafeInteractionTrigger] Existing collider set as trigger");
            }
        }

        private void UpdatePlayerProximity()
        {
            if (playerTransform == null)
                return;

            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            isPlayerInRange = distanceToPlayer <= interactionRange;
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.CompareTag("Player"))
            {
                isPlayerInRange = true;

                if (debugMode)
                    Debug.Log("[SafeInteractionTrigger] Player entered interaction range");
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            if (collider.CompareTag("Player"))
            {
                isPlayerInRange = false;

                if (debugMode)
                    Debug.Log("[SafeInteractionTrigger] Player left interaction range");
            }
        }

        public bool IsPlayerInRange()
        {
            return isPlayerInRange;
        }

        public float GetInteractionRange()
        {
            return interactionRange;
        }
    }
}
