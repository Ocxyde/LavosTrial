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

        private void Awake()
        {
            // Check if EventHandler already exists
            if (Core.EventHandler.Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            // Create EventHandler
            var go = new GameObject("EventHandler");
            var handler = go.AddComponent<Core.EventHandler>();
            
            // Set debug mode
            if (debugMode)
            {
                handler.debugEvents = true;
            }

            DontDestroyOnLoad(go);

            Debug.Log("[EventHandlerInitializer] EventHandler created and initialized");
        }
    }
}
