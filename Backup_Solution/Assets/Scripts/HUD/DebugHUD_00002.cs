using UnityEngine;
using UnityEngine.InputSystem;
using Code.Lavos;

namespace Unity6.LavosTrial.HUD
{
    /// <summary>
    /// Simple debug HUD to verify health system is working
    /// Shows current health value in top-left corner
    /// </summary>
    public class DebugHUD : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private Key toggleKey = Key.F1;

        private Rect _windowRect;
        private string _healthInfo = "N/A";
        private string _manaInfo = "N/A";
        private string _staminaInfo = "N/A";
        private Keyboard _keyboard;

        void Awake()
        {
            _keyboard = Keyboard.current;
            _windowRect = new Rect(10, 10, 300, 150);
            SubscribeToEvents();
            UpdateInfo();
        }

        void Update()
        {
            if (_keyboard != null && _keyboard[toggleKey].wasPressedThisFrame)
            {
                showDebugInfo = !showDebugInfo;
            }

            UpdateInfo();

            // Test input
            if (_keyboard != null)
            {
                if (_keyboard[Key.Digit1].wasPressedThisFrame) TestHealth(1.0f);
                if (_keyboard[Key.Digit5].wasPressedThisFrame) TestHealth(0.5f);
                if (_keyboard[Key.Digit9].wasPressedThisFrame) TestHealth(0.1f);

                if (PlayerStats.Instance != null)
                {
                    if (_keyboard[Key.Q].wasPressedThisFrame) PlayerStats.Instance.RestoreMana(100f);
                    if (_keyboard[Key.E].wasPressedThisFrame) PlayerStats.Instance.UseMana(50f);
                    if (_keyboard[Key.A].wasPressedThisFrame) PlayerStats.Instance.RestoreStamina(100f);
                    if (_keyboard[Key.D].wasPressedThisFrame) PlayerStats.Instance.UseStamina(50f);
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
            if (PlayerStats.Instance != null)
            {
                PlayerStats.OnHealthChanged += OnHealthChanged;
                PlayerStats.Instance.OnManaChanged += OnManaChanged;
                PlayerStats.Instance.OnStaminaChanged += OnStaminaChanged;
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
            if (PlayerStats.Instance != null)
            {
                _healthInfo = $"Health: {PlayerStats.Instance.CurrentHealth:F0}/{PlayerStats.Instance.MaxHealth:F0}";
                _manaInfo = $"Mana: {PlayerStats.Instance.CurrentMana:F0}/{PlayerStats.Instance.MaxMana:F0}";
                _staminaInfo = $"Stamina: {PlayerStats.Instance.CurrentStamina:F0}/{PlayerStats.Instance.MaxStamina:F0}";
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
            if (PlayerStats.Instance != null)
            {
                float targetHealth = PlayerStats.Instance.MaxHealth * percent;
                float diff = targetHealth - PlayerStats.Instance.CurrentHealth;
                if (diff > 0)
                    PlayerStats.Instance.Heal(diff);
                else
                    PlayerStats.Instance.TakeDamage(-diff);
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
