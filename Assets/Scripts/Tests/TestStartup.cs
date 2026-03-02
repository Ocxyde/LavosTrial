// TestStartup.cs
// Simple test to verify scripts load correctly
// Unity 6 compatible - UTF-8 encoding - Unix line endings
// Uses New Input System

using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Test script - verifies basic Unity functionality
    /// Attach to any GameObject to test if scripts run
    /// Uses New Input System
    /// </summary>
    public class TestStartup : MonoBehaviour
    {
        void Awake()
        {
            Debug.Log("[TestStartup] Awake() called - Scripts are working!", this);
        }

        void Start()
        {
            Debug.Log($"[TestStartup] Start() called - Unity {Application.unityVersion}", this);
        }

        void Update()
        {
            // Press Space to test input (New Input System)
            if (Keyboard.current?.spaceKey?.isPressed == true)
            {
                Debug.Log("[TestStartup] Space key pressed - Input working!", this);
            }
        }
    }
}