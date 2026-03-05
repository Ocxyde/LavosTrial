// Copyright (C) 2026 Ocxyde
//
// This file is part of PeuImporte.
//
// PeuImporte is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PeuImporte is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PeuImporte.  If not, see <https://www.gnu.org/licenses/>.
// CombatSystem.cs
// Combat engine for damage, healing, and resource management
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Combat calculations and resource consumption
// Works with StatsEngine for stat calculations and EventHandler for broadcasting

using System;
using UnityEngine;
using Code.Lavos.Status;

namespace Code.Lavos.Core
{
    /// <summary>
    /// CombatSystem - Centralized combat engine for damage dealing, healing,
    /// and resource consumption. Integrates with StatsEngine and EventHandler.
    ///
    /// Features:
    /// - Damage calculation with resistances and crits
    /// - Healing with modifiers
    /// - Resource consumption (health, mana, stamina)
    /// - Combat event broadcasting via EventHandler
    /// - Out-of-combat detection for regen bonuses
    /// </summary>
    public class CombatSystem : MonoBehaviour
    {
        #region Inspector Settings

        [Header("Combat Settings")]
        [SerializeField] private float baseCritChance = 0.05f; // 5% base crit chance
        [SerializeField] private float baseCritDamage = 2f; // 200% crit damage
#pragma warning disable CS0414 // Field assigned but its value is never used (reserved for future i-frames system)
        [SerializeField] private float invincibilityTime = 0.5f; // Seconds after damage (reserved for future i-frames system)
#pragma warning restore CS0414

        [Header("Resource Costs")]
        [SerializeField] private float staminaSprintCost = 2f; // Stamina per second while sprinting
        [SerializeField] private float staminaJumpCost = 5f; // Stamina per jump
        [SerializeField] private float staminaDodgeCost = 10f; // Stamina per dodge roll (reserved for future dodge system)

        [Header("Regeneration")]
        [SerializeField] private float outOfCombatDelay = 3f; // Seconds to be considered "out of combat"
        [SerializeField] private float outOfCombatRegenMultiplier = 2f; // 2x regen when out of combat

        #endregion

        #region Private Fields

        private StatsEngine _statsEngine;
        private float _lastDamageTime;
        private float _lastStaminaUseTime = -10f; // Negative for OOC regen at start
        private bool _isInCombat;
        private float _combatTimer;
        private bool _isInvincible;

        #endregion

        #region Properties

        public bool IsInCombat => _isInCombat;
        public float TimeInCombat => _combatTimer;
        public float EffectiveStaminaRegen => GetEffectiveStaminaRegen();
        public float CritChance => baseCritChance;
        public float CritDamage => baseCritDamage;
        public bool IsInvincible => _isInvincible;

        #endregion

        #region Events

        public event Action<DamageInfo, float, GameObject> OnDamageDealt;
        public event Action<DamageInfo, float, GameObject> OnDamageTaken;
        public event Action<float> OnResourceUsed;
#pragma warning disable CS0067 // Event reserved for future stat tracking system
        public event Action<string, float> OnCombatStatChanged;
#pragma warning restore CS0067

        #endregion

        #region Unity Lifecycle

        void Awake()
        {
            DontDestroyOnLoad(gameObject);

            Debug.Log("[CombatSystem] Initialized");
        }

        void Update()
        {
            UpdateCombatState();
        }

        void OnDestroy()
        {
            // Instance cleanup removed (Instance property removed)
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize with a StatsEngine instance
        /// </summary>
        public void Initialize(StatsEngine statsEngine)
        {
            _statsEngine = statsEngine;
            Debug.Log("[CombatSystem] Initialized with StatsEngine");
        }

        /// <summary>
        /// Ensure StatsEngine is initialized (auto-find if needed)
        /// </summary>
        private void EnsureStatsEngine()
        {
            if (_statsEngine == null)
            {
                var playerStats = FindFirstObjectByType<Component>() as IPlayerStats;
                if (playerStats != null && playerStats.Engine != null)
                {
                    _statsEngine = playerStats.Engine;
                    Debug.Log("[CombatSystem] Auto-initialized StatsEngine from PlayerStats");
                }
                else
                {
                    Debug.LogWarning("[CombatSystem] StatsEngine not initialized - stamina methods will fail");
                }
            }
        }

        #endregion

        #region Combat State Management

        private void UpdateCombatState()
        {
            float timeSinceDamage = Time.time - _lastDamageTime;
            float timeSinceStaminaUse = Time.time - _lastStaminaUseTime;

            // In combat if damaged or used stamina within the delay window
            _isInCombat = (timeSinceDamage < outOfCombatDelay || timeSinceStaminaUse < outOfCombatDelay);

            if (_isInCombat)
            {
                _combatTimer += Time.deltaTime;
            }
            else
            {
                _combatTimer = 0f;
            }
        }

        /// <summary>
        /// Check if player is out of combat for regen bonus
        /// </summary>
        public bool IsOutOfCombat()
        {
            return !_isInCombat;
        }

        /// <summary>
        /// Get effective stamina regen with out-of-combat bonus
        /// </summary>
        public float GetEffectiveStaminaRegen()
        {
            if (_statsEngine == null) return 0f;

            float baseRegen = _statsEngine.StaminaRegen;
            float multiplier = IsOutOfCombat() ? outOfCombatRegenMultiplier : 1f;
            return baseRegen * multiplier;
        }

        #endregion

        #region Damage System

        /// <summary>
        /// Deal damage to a target with full combat calculation
        /// </summary>
        public float DealDamage(GameObject source, GameObject target, DamageInfo damageInfo)
        {
            if (target == null || damageInfo.amount <= 0) return 0f;

            // Get target's StatsEngine if they have one
            StatsEngine targetStats = GetTargetStatsEngine(target);

            float finalDamage = damageInfo.amount;

            // Apply resistance if target has stats
            if (targetStats != null)
            {
                finalDamage = targetStats.CalculateDamage(damageInfo);
                targetStats.ModifyHealth(-finalDamage); // Apply damage
            }
            else
            {
                // Simple crit calculation for targets without StatsEngine
                if (damageInfo.isCritical || UnityEngine.Random.value < baseCritChance)
                {
                    finalDamage *= damageInfo.criticalMultiplier;
                }
                
                // Fallback: try to find PlayerStats component or apply directly
                var playerStats = target.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    playerStats.TakeDamage(finalDamage);
                }
                else
                {
                    // Last resort: try to find any health component
                    Debug.LogWarning($"[CombatSystem] Target {target.name} has no StatsEngine or PlayerStats - damage applied directly");
                }
            }

            // Update combat state
            _lastDamageTime = Time.time;

            // Broadcast event
            OnDamageDealt?.Invoke(damageInfo, finalDamage, target);

            if (EventHandler.Instance != null)
            {
                EventHandler.Instance.InvokeDamageDealt(damageInfo, finalDamage);
            }

            Debug.Log($"[CombatSystem] Damage dealt: {finalDamage:F1} ({damageInfo.type}) to {target.name}");

            return finalDamage;
        }

        /// <summary>
        /// Simple damage method
        /// </summary>
        public float DealDamage(GameObject target, float amount, DamageType type = DamageType.Physical)
        {
            var info = new DamageInfo(amount, type);
            return DealDamage(null, target, info);
        }

        /// <summary>
        /// Take damage for the local player
        /// </summary>
        public float TakeDamage(DamageInfo damageInfo)
        {
            if (_statsEngine == null) return 0f;

            float finalDamage = _statsEngine.CalculateDamage(damageInfo);
            _statsEngine.ModifyHealth(-finalDamage);

            _lastDamageTime = Time.time;

            OnDamageTaken?.Invoke(damageInfo, finalDamage, null);

            if (EventHandler.Instance != null)
            {
                EventHandler.Instance.InvokeDamageTaken(damageInfo, finalDamage);
                EventHandler.Instance.InvokePlayerDamaged(finalDamage);
            }

            Debug.Log($"[CombatSystem] Damage taken: {finalDamage:F1} ({damageInfo.type})");

            return finalDamage;
        }

        private StatsEngine GetTargetStatsEngine(GameObject target)
        {
            // Try to get StatsEngine from target
            var playerStats = target.GetComponent<IPlayerStats>();
            return playerStats?.Engine;
        }

        #endregion

        #region Healing System

        /// <summary>
        /// Heal a target
        /// </summary>
        public void Heal(GameObject target, float amount)
        {
            if (target == null || amount <= 0) return;

            var targetStats = GetTargetStatsEngine(target);
            targetStats?.ModifyHealth(amount);

            if (EventHandler.Instance != null)
            {
                EventHandler.Instance.InvokePlayerHealed(amount);
            }

            Debug.Log($"[CombatSystem] Healed {target.name} for {amount:F1}");
        }

        /// <summary>
        /// Full heal for a target
        /// </summary>
        public void FullHeal(GameObject target)
        {
            if (target == null) return;

            var targetStats = GetTargetStatsEngine(target);
            if (targetStats != null)
            {
                float missingHealth = targetStats.MaxHealth - targetStats.CurrentHealth;
                targetStats.ModifyHealth(missingHealth);
            }
        }

        #endregion

        #region Resource Management

        /// <summary>
        /// Use stamina with combat tracking
        /// </summary>
        public bool UseStamina(float amount)
        {
            EnsureStatsEngine();
            
            if (_statsEngine == null)
            {
                Debug.LogWarning("[CombatSystem] Cannot use stamina - StatsEngine not initialized");
                return false;
            }

            bool success = _statsEngine.UseStamina(amount);
            if (success)
            {
                _lastStaminaUseTime = Time.time;
                OnResourceUsed?.Invoke(amount);

                if (EventHandler.Instance != null)
                {
                    EventHandler.Instance.InvokePlayerStaminaUsed(amount);
                }
            }

            return success;
        }

        /// <summary>
        /// Use mana
        /// </summary>
        public bool UseMana(float amount)
        {
            if (_statsEngine == null) return false;

            bool success = _statsEngine.UseMana(amount);
            if (success)
            {
                OnResourceUsed?.Invoke(amount);

                if (EventHandler.Instance != null)
                {
                    EventHandler.Instance.InvokePlayerManaUsed(amount);
                }
            }

            return success;
        }

        /// <summary>
        /// Use health
        /// </summary>
        public bool UseHealth(float amount)
        {
            if (_statsEngine == null) return false;

            bool success = _statsEngine.UseHealth(amount);
            if (success)
            {
                OnResourceUsed?.Invoke(amount);
            }

            return success;
        }

        /// <summary>
        /// Restore stamina
        /// </summary>
        public void RestoreStamina(float amount)
        {
            if (_statsEngine == null) return;

            _statsEngine.RestoreStamina(amount);

            if (EventHandler.Instance != null)
            {
                EventHandler.Instance.InvokePlayerStaminaRestored(amount);
            }
        }

        /// <summary>
        /// Restore mana
        /// </summary>
        public void RestoreMana(float amount)
        {
            if (_statsEngine == null) return;

            _statsEngine.RestoreMana(amount);

            if (EventHandler.Instance != null)
            {
                EventHandler.Instance.InvokePlayerManaRestored(amount);
            }
        }

        #endregion

        #region Resource Cost Helpers

        /// <summary>
        /// Get sprint stamina cost per second
        /// </summary>
        public float GetSprintCost() => staminaSprintCost;

        /// <summary>
        /// Get jump stamina cost
        /// </summary>
        public float GetJumpCost() => staminaJumpCost;

        /// <summary>
        /// Get dodge stamina cost
        /// </summary>
        public float GetDodgeCost() => staminaDodgeCost;

        /// <summary>
        /// Check if can afford sprint
        /// </summary>
        public bool CanSprint() => _statsEngine?.CurrentStamina >= staminaSprintCost;

        /// <summary>
        /// Check if can afford jump
        /// </summary>
        public bool CanJump() => _statsEngine?.CurrentStamina >= staminaJumpCost;

        /// <summary>
        /// Check if can afford dodge
        /// </summary>
        public bool CanDodge() => _statsEngine?.CurrentStamina >= staminaDodgeCost;

        #endregion

        #region Utility

        /// <summary>
        /// Reset combat state (for debugging or special events)
        /// </summary>
        public void ResetCombatState()
        {
            _lastDamageTime = -outOfCombatDelay;
            _lastStaminaUseTime = -outOfCombatDelay;
            _combatTimer = 0f;
        }

        /// <summary>
        /// Get time since last damage taken
        /// </summary>
        public float GetTimeSinceLastDamage() => Time.time - _lastDamageTime;

        /// <summary>
        /// Get time since last stamina use
        /// </summary>
        public float GetTimeSinceLastStaminaUse() => Time.time - _lastStaminaUseTime;

        #endregion
    }
}