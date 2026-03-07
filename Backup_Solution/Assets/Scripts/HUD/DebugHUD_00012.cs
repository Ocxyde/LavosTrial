// DebugHUD.cs
// Debug UI for testing and development
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the HUD system - debug only

using UnityEngine;
using UnityEngine.InputSystem;
using Code.Lavos;
using Code.Lavos.Status;
using Code.Lavos.Core;

namespace Code.Lavos.HUD
{
    /// <summary>
    /// Simple debug HUD to verify health system is working
    /// Shows current health value in top-left corner
    /// </summary>
    public class DebugHUD : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool showDebugInfo = false;  // Disabled by default
        [SerializeField] private bool enableDebugHUD = false;  // Set to true to enable, false to disable completely

        private Rect _windowRect;
        private string _healthInfo = "N/A";
        private string _manaInfo = "N/A";
        private string _staminaInfo = "N/A";
        private Keyboard _keyboard;
        private MonoBehaviour _playerStats;

        void Awake()
        {
            // If DebugHUD is disabled, destroy this GameObject immediately
            if (!enableDebugHUD)
            {
                Destroy(gameObject);
                return;
            }

            _keyboard = Keyboard.current;
            _windowRect = new Rect(10, 10, 300, 150);
            
            // Find PlayerStats using reflection to avoid circular dependency
            var allObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var obj in allObjects)
            {
                if (obj.GetType().Name == "PlayerStats")
                {
                    _playerStats = obj;
                    break;
                }
            }
            
            SubscribeToEvents();
            UpdateInfo();
        }

        void Update()
        {
            // Toggle debug info with F1
            if (_keyboard != null && _keyboard[Key.F1] != null && _keyboard[Key.F1].wasPressedThisFrame)
            {
                showDebugInfo = !showDebugInfo;
                Debug.Log($"[DebugHUD] Debug info {(showDebugInfo ? "enabled" : "disabled")}");
            }

            UpdateInfo();

            // Test input using reflection to avoid circular dependency
            if (_keyboard != null && _playerStats != null)
            {
                var statsType = _playerStats.GetType();
                
                if (_keyboard[Key.Digit1].wasPressedThisFrame) TestHealth(1.0f);
                if (_keyboard[Key.Digit5].wasPressedThisFrame) TestHealth(0.5f);
                if (_keyboard[Key.Digit9].wasPressedThisFrame) TestHealth(0.1f);

                if (_keyboard[Key.Q].wasPressedThisFrame)
                {
                    var restoreManaMethod = statsType.GetMethod("RestoreMana");
                    restoreManaMethod?.Invoke(_playerStats, new object[] { 100f });
                }
                if (_keyboard[Key.E].wasPressedThisFrame)
                {
                    var useManaMethod = statsType.GetMethod("UseMana");
                    useManaMethod?.Invoke(_playerStats, new object[] { 50f });
                }
                if (_keyboard[Key.A].wasPressedThisFrame)
                {
                    var restoreStaminaMethod = statsType.GetMethod("RestoreStamina");
                    restoreStaminaMethod?.Invoke(_playerStats, new object[] { 100f });
                }
                if (_keyboard[Key.D].wasPressedThisFrame)
                {
                    var useStaminaMethod = statsType.GetMethod("UseStamina");
                    useStaminaMethod?.Invoke(_playerStats, new object[] { 50f });
                }
            }
        }

        void OnGUI()
        {
            if (!showDebugInfo) return;

            GUI.backgroundColor = new Color(0, 0, 0, 0.7f);
            GUI.Window(0, _windowRect, DrawWindow, "Debug HUD (F1 to toggle)");
        }

        void DrawWindow(int windowID)
        {
            GUILayout.Label(_healthInfo);
            GUILayout.Label(_manaInfo);
            GUILayout.Label(_staminaInfo);
            GUILayout.Space(10);
            GUILayout.Label("Press 1-9 to test health");
            GUILayout.Label("Press Q/E to test mana");
            GUILayout.Label("Press A/D to test stamina");

            GUI.DragWindow();
        }

        void SubscribeToEvents()
        {
            if (_playerStats != null)
            {
                var statsType = _playerStats.GetType();
                var onHealthEvent = statsType.GetEvent("OnHealthChanged", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                var onManaEvent = statsType.GetEvent("OnManaChanged", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                var onStaminaEvent = statsType.GetEvent("OnStaminaChanged", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                
                if (onHealthEvent != null) onHealthEvent.AddEventHandler(null, new System.Action<float, float>(OnHealthChanged));
                if (onManaEvent != null) onManaEvent.AddEventHandler(_playerStats, new System.Action<float, float>(OnManaChanged));
                if (onStaminaEvent != null) onStaminaEvent.AddEventHandler(_playerStats, new System.Action<float, float>(OnStaminaChanged));
            }
            else
            {
                var playerHealth = FindFirstObjectByType<PlayerHealth>();
                if (playerHealth != null)
                {
                    PlayerHealth.OnHealthChanged += OnHealthChangedLegacy;
                }
            }
        }

        void UpdateInfo()
        {
            if (_playerStats != null)
            {
                var statsType = _playerStats.GetType();
                var currentHealthProp = statsType.GetProperty("CurrentHealth");
                var maxHealthProp = statsType.GetProperty("MaxHealth");
                var currentManaProp = statsType.GetProperty("CurrentMana");
                var maxManaProp = statsType.GetProperty("MaxMana");
                var currentStaminaProp = statsType.GetProperty("CurrentStamina");
                var maxStaminaProp = statsType.GetProperty("MaxStamina");

                _healthInfo = $"Health: {(currentHealthProp?.GetValue(_playerStats) is float ch ? ch : 0f):F0}/{(maxHealthProp?.GetValue(_playerStats) is float mh ? mh : 0f):F0}";
                _manaInfo = $"Mana: {(currentManaProp?.GetValue(_playerStats) is float cm ? cm : 0f):F0}/{(maxManaProp?.GetValue(_playerStats) is float mm ? mm : 0f):F0}";
                _staminaInfo = $"Stamina: {(currentStaminaProp?.GetValue(_playerStats) is float cs ? cs : 0f):F0}/{(maxStaminaProp?.GetValue(_playerStats) is float ms ? ms : 0f):F0}";
            }
            else
            {
                var playerHealth = FindFirstObjectByType<PlayerHealth>();
                if (playerHealth != null)
                {
                    _healthInfo = $"Health: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth} (PlayerHealth)";
                    _manaInfo = "Mana: N/A";
                    _staminaInfo = "Stamina: N/A";
                }
                else
                {
                    _healthInfo = "Health: No player found!";
                    _manaInfo = "Mana: N/A";
                    _staminaInfo = "Stamina: N/A";
                }
            }
        }

        void OnHealthChanged(float current, float max) => UpdateInfo();
        void OnManaChanged(float current, float max) => UpdateInfo();
        void OnStaminaChanged(float current, float max) => UpdateInfo();
        void OnHealthChangedLegacy(float current, float max) => UpdateInfo();

        void TestHealth(float percent)
        {
            if (_playerStats != null)
            {
                var statsType = _playerStats.GetType();
                var maxHealthProp = statsType.GetProperty("MaxHealth");
                var currentHealthProp = statsType.GetProperty("CurrentHealth");
                var healMethod = statsType.GetMethod("Heal");
                var takeDamageMethod = statsType.GetMethod("TakeDamage");

                if (maxHealthProp != null && currentHealthProp != null)
                {
                    float maxHealth = maxHealthProp.GetValue(_playerStats) is float mh ? mh : 100f;
                    float currentHealth = currentHealthProp.GetValue(_playerStats) is float ch ? ch : 100f;
                    float targetHealth = maxHealth * percent;
                    float diff = targetHealth - currentHealth;

                    if (diff > 0 && healMethod != null)
                        healMethod.Invoke(_playerStats, new object[] { diff });
                    else if (diff < 0 && takeDamageMethod != null)
                        takeDamageMethod.Invoke(_playerStats, new object[] { -diff });
                }
            }
            else
            {
                var playerHealth = FindFirstObjectByType<PlayerHealth>();
                if (playerHealth != null)
                {
                    // Set health by dealing damage or healing
                    float targetHealth = playerHealth.MaxHealth * percent;
                    float diff = targetHealth - playerHealth.CurrentHealth;
                    if (diff > 0)
                        playerHealth.Heal(diff);
                    else
                        playerHealth.TakeDamage(-diff);
                }
            }
        }
    }
}
