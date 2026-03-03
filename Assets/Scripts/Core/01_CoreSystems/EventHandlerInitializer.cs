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
