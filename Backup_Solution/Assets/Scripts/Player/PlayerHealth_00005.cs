// PlayerHealth.cs
// Player health management
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Player system - health component

// PlayerHealth.cs
// Player health management
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Player system - health component
using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// PLAYERHEALTH - Player health management
    ///
    /// SETUP:
    ///  1. Attach to player GameObject
    ///  2. Set maxHealth in Inspector
    ///  3. Call TakeDamage() from enemies/traps
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private float maxHealth = 1000f;
        [SerializeField] private float invincibilityTime = 0.5f;

        [Header("Regeneration (optional)")]
        [SerializeField] private bool regenEnabled = false;
        [SerializeField] private float regenAmount = 5f;
        [SerializeField] private float regenDelay = 3f;

        public float CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }
        public float MaxHealth => maxHealth;

        private float _lastDamageTime;
        private bool _isInvincible;

        public static event System.Action<float, float> OnHealthChanged;
        public static event System.Action OnPlayerDied;
        public static event System.Action<float> OnPlayerDamaged;

        void Awake()
        {
            CurrentHealth = maxHealth;
        }

        void Start()
        {
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        void Update()
        {
            if (_isInvincible && Time.time - _lastDamageTime > invincibilityTime)
                _isInvincible = false;

            if (regenEnabled && !IsDead && CurrentHealth < maxHealth)
            {
                if (Time.time - _lastDamageTime >= regenDelay)
                    Heal(regenAmount * Time.deltaTime);
            }
        }

        public void TakeDamage(float damage)
        {
            if (IsDead || _isInvincible) return;

            CurrentHealth -= damage;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, maxHealth);
            _lastDamageTime = Time.time;
            _isInvincible = true;

            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            OnPlayerDamaged?.Invoke(damage);

            Debug.Log($"[PlayerHealth] Damage: {damage} | Health: {CurrentHealth}/{maxHealth}");

            if (CurrentHealth <= 0f)
                Die();
        }

        public void Heal(float amount)
        {
            if (IsDead) return;

            CurrentHealth += amount;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, maxHealth);

            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void FullHeal()
        {
            Heal(maxHealth);
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;

            Debug.Log("[PlayerHealth] Player died!");
            OnPlayerDied?.Invoke();
            GameManager.Instance?.TriggerGameOver();

            var controller = GetComponent<PlayerController>();
            if (controller != null)
                controller.enabled = false;
        }
    }
}
