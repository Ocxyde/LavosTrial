// PlayerStats.cs
// Player stats wrapper for StatsEngine
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// CORE: MonoBehaviour integration for StatsEngine

using UnityEngine;
using Object = UnityEngine.Object;
using Code.Lavos.Status;
using System.Collections.Generic;

namespace Code.Lavos.Core
{
    /// <summary>
    /// PlayerStats - MonoBehaviour wrapper for StatsEngine system.
    /// Handles Unity-specific integration (events, MonoBehaviour lifecycle, UI spawning).
    ///
    /// All stat calculations are handled by the StatsEngine class in the Status namespace.
    /// Implements IPlayerStats interface for Core plug-in-and-out architecture.
    /// </summary>
    public class PlayerStats : MonoBehaviour, IPlayerStats
    {
        public static PlayerStats Instance { get; private set; }

        // ─── Inspector Settings ────────────────────────────────────────────────
        [Header("Base Stats")]
        [SerializeField] private float maxHealth = 1000f;
        [SerializeField] private float maxMana = 50f;
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float healthRegen = 2f;
        [SerializeField] private float manaRegen = 5f;
        [SerializeField] private float staminaRegen = 0.15f; // Nerfed hard: 0.15 stamina/sec (667 sec to full). Sprint costs 2/sec = net loss of 1.85/sec

        [Header("Combat")]
        [SerializeField] private float invincibilityTime = 0.5f;

        [Header("Status Effects")]
        [SerializeField] private StatusEffectData[] startingEffects;

        // ─── StatsEngine Core ──────────────────────────────────────────────────
        private StatsEngine _statsEngine;

        // ─── State ─────────────────────────────────────────────────────────────
        private float _lastDamageTime;
        private bool _isInvincible;
        private bool _isDead;

        // ─── Properties ────────────────────────────────────────────────────────
        public StatsEngine Engine => _statsEngine;
        public float CurrentHealth => _statsEngine.CurrentHealth;
        public float CurrentMana => _statsEngine.CurrentMana;
        public float CurrentStamina => _statsEngine.CurrentStamina;
        public float MaxHealth => _statsEngine.MaxHealth;
        public float MaxMana => _statsEngine.MaxMana;
        public float MaxStamina => _statsEngine.MaxStamina;
        public bool IsDead => _isDead;
        public System.Collections.Generic.IReadOnlyList<StatusEffectData> ActiveEffects => _statsEngine.ActiveEffects;

        // ─── Events (static for health/death, instance for others) ─────────────
        public static event System.Action<float, float> OnHealthChanged;
        public static event System.Action OnPlayerDied;
        public static event System.Action<DamageInfo, float> OnPlayerDamaged;

        public event System.Action<float, float> OnManaChanged;
        public event System.Action<float, float> OnStaminaChanged;
        public event System.Action<StatusEffectData> OnEffectAdded;
        public event System.Action<StatusEffectData> OnEffectRemoved;

        // ─── Unity Lifecycle ───────────────────────────────────────────────────
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Object.DontDestroyOnLoad(gameObject);

            // Initialize StatsEngine
            _statsEngine = new StatsEngine();
            _statsEngine.SetBaseStats(maxHealth, maxMana, maxStamina, healthRegen, manaRegen, staminaRegen);

            // Subscribe to StatsEngine events and relay through EventHandler
            _statsEngine.OnHealthChanged += (current, max) => {
                OnHealthChanged?.Invoke(current, max);
                if (Core.EventHandler.Instance != null)
                    Core.EventHandler.Instance.InvokePlayerHealthChanged(current, max);
            };
            _statsEngine.OnManaChanged += (current, max) => {
                OnManaChanged?.Invoke(current, max);
                if (Core.EventHandler.Instance != null)
                    Core.EventHandler.Instance.InvokePlayerManaChanged(current, max);
            };
            _statsEngine.OnStaminaChanged += (current, max) => {
                OnStaminaChanged?.Invoke(current, max);
                if (Core.EventHandler.Instance != null)
                    Core.EventHandler.Instance.InvokePlayerStaminaChanged(current, max);
            };
            _statsEngine.OnEffectAdded += (effect) => {
                OnEffectAdded?.Invoke(effect);
            };
            _statsEngine.OnEffectRemoved += (effect) => {
                OnEffectRemoved?.Invoke(effect);
            };

            // Subscribe to EventHandler for centralized event management
            if (Core.EventHandler.Instance != null)
            {
                Core.EventHandler.Instance.SubscribeToPlayerStats(this);
            }

            SpawnUIBars();

            // Apply starting effects
            if (startingEffects != null)
            {
                foreach (var effect in startingEffects)
                {
                    if (effect != null)
                        _statsEngine.ApplyEffect(effect);
                }
            }

            Debug.Log("[PlayerStats] Initialized with EventHandler integration");
        }

        void Start()
        {
            // Notify initial state
            OnHealthChanged?.Invoke(_statsEngine.CurrentHealth, _statsEngine.MaxHealth);
            OnManaChanged?.Invoke(_statsEngine.CurrentMana, _statsEngine.MaxMana);
            OnStaminaChanged?.Invoke(_statsEngine.CurrentStamina, _statsEngine.MaxStamina);
        }

        void OnDestroy()
        {
            // Clear static events
            OnHealthChanged = null;
            OnPlayerDied = null;
            OnPlayerDamaged = null;

            if (Instance == this)
                Instance = null;
        }

        void Update()
        {
            if (_isDead) return;

            HandleInvincibility();
            _statsEngine.UpdateEffects();
            _statsEngine.ApplyRegeneration(Time.deltaTime);
        }

        // ─── Damage System ─────────────────────────────────────────────────────

        /// <summary>
        /// Take damage with type and resistance calculation
        /// </summary>
        public void TakeDamage(Status.DamageInfo damageInfo)
        {
            if (_isDead || _isInvincible) return;

            float finalDamage = _statsEngine.CalculateDamage(damageInfo);
            _statsEngine.ModifyHealth(-finalDamage);

            _lastDamageTime = Time.time;
            _isInvincible = true;

            OnPlayerDamaged?.Invoke(damageInfo, finalDamage);

            Debug.Log($"[PlayerStats] Damage: {damageInfo.type} | Base: {damageInfo.amount} | Final: {finalDamage:F1} | Health: {_statsEngine.CurrentHealth:F0}/{_statsEngine.MaxHealth:F0}");

            if (_statsEngine.CurrentHealth <= 0f) Die();
        }

        /// <summary>
        /// Simple damage method (defaults to Physical)
        /// </summary>
        public void TakeDamage(float amount, DamageType type = DamageType.Physical)
        {
            TakeDamage(new Status.DamageInfo(amount, type));
        }

        // ─── Healing ───────────────────────────────────────────────────────────

        public void Heal(float amount)
        {
            if (_isDead || amount <= 0f) return;
            _statsEngine.ModifyHealth(amount);
        }

        public void FullHeal() => Heal(_statsEngine.MaxHealth - _statsEngine.CurrentHealth);

        public void Revive(float healthAmount)
        {
            _isDead = false;
            _isInvincible = false;
            _statsEngine.ModifyHealth(healthAmount - _statsEngine.CurrentHealth);
            _statsEngine.RestoreMana(_statsEngine.MaxMana);
            _statsEngine.RestoreStamina(_statsEngine.MaxStamina);
            _statsEngine.ClearAllEffects();

            OnHealthChanged?.Invoke(_statsEngine.CurrentHealth, _statsEngine.MaxHealth);
            OnManaChanged?.Invoke(_statsEngine.CurrentMana, _statsEngine.MaxMana);
            OnStaminaChanged?.Invoke(_statsEngine.CurrentStamina, _statsEngine.MaxStamina);

            Debug.Log("[PlayerStats] Player revived.");
        }

        // ─── Resource Management ───────────────────────────────────────────────

        public bool UseHealth(float amount) => _statsEngine.UseHealth(amount);
        public bool UseMana(float amount) => _statsEngine.UseMana(amount);
        public bool UseStamina(float amount) => _statsEngine.UseStamina(amount);

        public void RestoreMana(float amount) => _statsEngine.RestoreMana(amount);
        public void RestoreStamina(float amount) => _statsEngine.RestoreStamina(amount);

        // ─── Status Effects ────────────────────────────────────────────────────

        public void AddEffect(StatusEffectData effect, string applierId = null)
        {
            _statsEngine.ApplyEffect(effect, applierId);
        }

        public void RemoveEffect(string effectId) => _statsEngine.RemoveEffect(effectId);
        public void ClearEffects() => _statsEngine.ClearAllEffects();
        public bool HasEffect(string effectId) => _statsEngine.HasEffect(effectId);
        public float GetEffectIntensity(string effectId) => _statsEngine.GetEffectIntensity(effectId);

        // ─── Stat Modifiers ────────────────────────────────────────────────────

        public void AddModifier(string statName, string id, string sourceId, ModifierType type, float value, float duration = 0f)
        {
            _statsEngine.AddModifier(statName, id, sourceId, type, value, duration);
        }

        public void RemoveModifiersBySource(string sourceId) => _statsEngine.RemoveModifiersBySource(sourceId);

        // ─── Resistances ───────────────────────────────────────────────────────

        public float GetResistanceMultiplier(DamageType type) => _statsEngine.GetResistanceMultiplier(type);
        public float GetResistancePercent(DamageType type) => _statsEngine.GetResistancePercent(type);

        public void ModifyResistance(DamageType type, float amount, ModifierType modifierType, float duration = 0f, string sourceId = null)
        {
            _statsEngine.ModifyResistance(type, amount, modifierType, duration, sourceId);
        }

        // ─── Utility ───────────────────────────────────────────────────────────

        public bool CanAfford(float healthCost = 0f, float manaCost = 0f, float staminaCost = 0f)
        {
            return _statsEngine.CanAfford(healthCost, manaCost, staminaCost);
        }

        public float CalculateDamage(DamageInfo damageInfo) => _statsEngine.CalculateDamage(damageInfo);

        private void HandleInvincibility()
        {
            if (_isInvincible && Time.time - _lastDamageTime > invincibilityTime)
                _isInvincible = false;
        }

        private void Die()
        {
            if (_isDead) return;
            _isDead = true;

            Debug.Log("[PlayerStats] Player died!");
            OnPlayerDied?.Invoke();
            GameManager.Instance?.TriggerGameOver();

            var controller = GetComponent<PlayerController>();
            if (controller != null) controller.enabled = false;
        }

        // ─── UI Integration ────────────────────────────────────────────────────

        // Note: HUD initialization moved to HUD assembly to avoid circular dependency
        // HUD system subscribes to PlayerStats events instead
        private void SpawnUIBars()
        {
            // HUD initialization handled by HUD assembly
            // See: Code.Lavos.HUD.HUDSystem
        }
    }
}
