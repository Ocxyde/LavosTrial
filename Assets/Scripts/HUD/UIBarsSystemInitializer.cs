// UIBarsSystemInitializer.cs
// Auto-initializes UIBarsSystem at game start
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// SETUP:
//   1. Create an empty GameObject in your scene (or use an existing manager)
//   2. Attach this script
//   3. That's it - bars will appear automatically
//
// Optional: Link PlayerHealth and PlayerController for automatic bar updates

using UnityEngine;

namespace Unity6.LavosTrial.HUD
{
    /// <summary>
    /// Automatically initializes UIBarsSystem when the game starts.
    /// Optionally links to PlayerHealth and PlayerController for automatic updates.
    /// </summary>
    public class UIBarsSystemInitializer : MonoBehaviour
    {
        [Header("References (optional - will auto-find if not set)")]
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerController playerController;

        [Header("Settings")]
        [SerializeField] private bool autoFindPlayer = true;
        [SerializeField] private string playerTag = "Player";

        private void Awake()
        {
            // Initialize UIBarsSystem
            var uiGO = new GameObject("UIBarsSystem");
            var uiBars = uiGO.AddComponent<UIBarsSystem>();
            DontDestroyOnLoad(uiGO);

            Debug.Log("[UIBarsSystemInitializer] UIBarsSystem initialized");
        }

        private void Start()
        {
            // Find or assign PlayerHealth
            if (playerHealth == null && autoFindPlayer)
            {
                playerHealth = FindPlayerComponent<PlayerHealth>();
            }

            // Find or assign PlayerController
            if (playerController == null && autoFindPlayer)
            {
                playerController = FindPlayerComponent<PlayerController>();
            }

            // Subscribe to events if components found
            SubscribeToEvents();

            // Set initial values
            UpdateInitialValues();
        }

        private void SubscribeToEvents()
        {
            if (playerHealth != null)
            {
                // PlayerHealth uses static events
                PlayerHealth.OnHealthChanged += OnHealthChanged;
                Debug.Log("[UIBarsSystemInitializer] Subscribed to PlayerHealth.OnHealthChanged");
            }

            if (playerController != null)
            {
                // If PlayerController has stamina events, subscribe to them
                // This depends on your PlayerController implementation
                Debug.Log("[UIBarsSystemInitializer] PlayerController found (stamina integration may require PlayerStats)");
            }
        }

        private void UpdateInitialValues()
        {
            if (UIBarsSystem.Instance == null) return;

            // Set initial health values
            if (playerHealth != null)
            {
                // Access via reflection or public properties
                // Assuming PlayerHealth has CurrentHealth and MaxHealth properties
                var currentHealthProp = playerHealth.GetType().GetProperty("CurrentHealth");
                var maxHealthField = playerHealth.GetType().GetField("maxHealth",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (currentHealthProp != null && maxHealthField != null)
                {
                    float current = (float)currentHealthProp.GetValue(playerHealth);
                    float max = (float)maxHealthField.GetValue(playerHealth);
                    UIBarsSystem.Instance.SetHealth(current, max);
                }
            }

            // Set default mana and stamina if no PlayerStats
            UIBarsSystem.Instance.SetMana(100f, 100f);
            UIBarsSystem.Instance.SetStamina(100f, 100f);
        }

        private void OnHealthChanged(float current, float max)
        {
            if (UIBarsSystem.Instance != null)
            {
                UIBarsSystem.Instance.SetHealth(current, max);
            }
        }

        private T FindPlayerComponent<T>() where T : Component
        {
            // Try to find by tag
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                T comp = player.GetComponent<T>();
                if (comp != null)
                {
                    Debug.Log($"[UIBarsSystemInitializer] Found {typeof(T).Name} on Player");
                    return comp;
                }
            }

            // Try to find by type in scene
            T foundComponent = FindFirstObjectByType<T>();
            if (foundComponent != null)
            {
                Debug.Log($"[UIBarsSystemInitializer] Found {typeof(T).Name} via FindFirstObjectByType");
                return foundComponent;
            }

            Debug.LogWarning($"[UIBarsSystemInitializer] {typeof(T).Name} not found");
            return null;
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                PlayerHealth.OnHealthChanged -= OnHealthChanged;
            }
        }
    }
}
