// UIBarsSystemStandalone.cs
// Simple standalone initializer for UIBarsSystem
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// SETUP IN UNITY:
//   1. In your scene, create an empty GameObject (or use an existing manager)
//   2. Add this component: "UIBarsSystemStandalone"
//   3. Optionally assign Player in Inspector (auto-finds if empty)
//   4. Play - bars will appear automatically
//
// The bars will automatically update via PlayerStats (unified health, mana, stamina system).

using UnityEngine;
using Code.Lavos;

namespace Unity6.LavosTrial.HUD
{
    /// <summary>
    /// Standalone initializer for UIBarsSystem.
    /// Attach to any GameObject in your scene.
    /// Links to PlayerStats for health, mana, and stamina updates.
    /// </summary>
    public class UIBarsSystemStandalone : MonoBehaviour
    {
        [Header("Player References (auto-find if empty)")]
        [Tooltip("Player GameObject with PlayerStats component")]
        [SerializeField] private GameObject player;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private PlayerStats _playerStats;
        private PlayerController _playerController;

        private void Awake()
        {
            // The UIBarsSystem will auto-initialize via its own Awake()
            // Just creating the GameObject is enough
            var uiGO = new GameObject("UIBarsSystem");
            uiGO.AddComponent<UIBarsSystem>();
            DontDestroyOnLoad(uiGO);

            if (showDebugLogs)
                Debug.Log("[UIBarsSystemStandalone] UIBarsSystem created");
        }

        private void Start()
        {
            // Find player if not assigned
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
                if (player == null)
                {
                    // Try to find any GameObject with PlayerStats
                    _playerStats = FindFirstObjectByType<PlayerStats>();
                    if (_playerStats != null)
                    {
                        player = _playerStats.gameObject;
                    }
                }
            }

            // Get components from player
            if (player != null)
            {
                _playerStats = player.GetComponent<PlayerStats>();
                _playerController = player.GetComponent<PlayerController>();
            }

            // Subscribe to PlayerStats events (static and instance)
            if (_playerStats != null)
            {
                PlayerStats.OnHealthChanged += OnHealthChanged;
                _playerStats.OnManaChanged += OnManaChanged;
                _playerStats.OnStaminaChanged += OnStaminaChanged;
                if (showDebugLogs)
                    Debug.Log($"[UIBarsSystemStandalone] Linked to PlayerStats on '{player.name}'");
            }
            else
            {
                if (showDebugLogs)
                    Debug.LogWarning("[UIBarsSystemStandalone] No PlayerStats found - bars won't update automatically");
            }

            // Set initial values
            SetInitialValues();
        }

        private void SetInitialValues()
        {
            if (UIBarsSystem.Instance == null) return;

            // Health, Mana, Stamina from PlayerStats
            if (_playerStats != null)
            {
                UIBarsSystem.Instance.SetHealth(_playerStats.CurrentHealth, _playerStats.MaxHealth);
                UIBarsSystem.Instance.SetMana(_playerStats.CurrentMana, _playerStats.MaxMana);
                UIBarsSystem.Instance.SetStamina(_playerStats.CurrentStamina, _playerStats.MaxStamina);
            }
            else
            {
                // Default values
                UIBarsSystem.Instance.SetHealth(1000f, 1000f);
                UIBarsSystem.Instance.SetMana(100f, 100f);
                UIBarsSystem.Instance.SetStamina(100f, 100f);
            }

            if (showDebugLogs)
                Debug.Log("[UIBarsSystemStandalone] Initial values set");
        }

        private void OnHealthChanged(float current, float max)
        {
            if (UIBarsSystem.Instance != null)
            {
                UIBarsSystem.Instance.SetHealth(current, max);
            }
        }

        private void OnStaminaChanged(float current, float max)
        {
            if (UIBarsSystem.Instance != null)
            {
                UIBarsSystem.Instance.SetStamina(current, max);
            }
        }

        private void OnManaChanged(float current, float max)
        {
            if (UIBarsSystem.Instance != null)
            {
                UIBarsSystem.Instance.SetMana(current, max);
            }
        }

        private void OnDestroy()
        {
            if (_playerStats != null)
            {
                PlayerStats.OnHealthChanged -= OnHealthChanged;
                _playerStats.OnManaChanged -= OnManaChanged;
                _playerStats.OnStaminaChanged -= OnStaminaChanged;
            }
        }
    }
}
