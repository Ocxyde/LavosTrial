// Copyright (C) 2026 Ocxyde
//
// This file is part of PeuImporte.
//
// PeuImporte is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PeuImporte is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PeuImporte.  If not, see <https://www.gnu.org/licenses/>.
// EventHandlerInitializer.cs
// Auto-creates EventHandler on scene load
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// SETUP: Add this script to any GameObject in your FIRST scene
// The EventHandler will be created automatically and persist

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Auto-initializes the EventHandler system.
    /// Add this to any GameObject in your first scene.
    /// </summary>
    public class EventHandlerInitializer : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool subscribeToPlayerStats = true;

        private void Awake()
        {
            // Check if EventHandler already exists
            if (EventHandler.Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            // Create EventHandler GameObject
            var handlerGO = new GameObject("EventHandler");
            var handler = handlerGO.AddComponent<EventHandler>();
            
            // Set debug mode
            handler.debugEvents = debugMode;

            // Make persistent
            DontDestroyOnLoad(handlerGO);

            // Auto-subscribe to PlayerStats if available
            if (subscribeToPlayerStats)
            {
                var playerStats = FindFirstObjectByType<Component>() as IPlayerStats;
                if (playerStats != null)
                {
                    handler.SubscribeToPlayerStats(playerStats);
                }
            }

            Debug.Log("[EventHandlerInitializer] EventHandler created and initialized");
        }
    }
}
