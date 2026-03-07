using UnityEngine;
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
        [SerializeField] private KeyCode toggleKey = KeyCode.F1;

        private Rect _windowRect;
        private string _healthInfo = "N/A";
        private string _manaInfo = "N/A";
        private string _staminaInfo = "N/A";

        void Awake()
        {
            _windowRect = new Rect(10, 10, 300, 150);
            SubscribeToEvents();
            UpdateInfo();
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                showDebugInfo = !showDebugInfo;
            }
            
            UpdateInfo();
            
            // Test input
            if (Input.GetKeyDown(KeyCode.Alpha1)) TestHealth(1.0f);
            if (Input.GetKeyDown(KeyCode.Alpha5)) TestHealth(0.5f);
            if (Input.GetKeyDown(KeyCode.Alpha9)) TestHealth(0.1f);
            
            if (PlayerStats.Instance != null)
            {
                if (Input.GetKeyDown(KeyCode.Q)) PlayerStats.Instance.RestoreMana(100f);
                if (Input.GetKeyDown(KeyCode.E)) PlayerStats.Instance.UseMana(50f);
                if (Input.GetKeyDown(KeyCode.A)) PlayerStats.Instance.RestoreStamina(100f);
                if (Input.GetKeyDown(KeyCode.D)) PlayerStats.Instance.UseStamina(50f);
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
