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
// StatsEngine.cs
// Central stat calculation engine
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Pure C# stat management (no MonoBehaviour)

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Status
{
    /// <summary>
    /// StatsEngine - Centralized manager for all player status effects and stat calculations.
    /// Handles buffs, debuffs, stat modifiers, damage calculations, and resource consumption.
    ///
    /// Features:
    /// - Stat modifiers from effects (additive, multiplicative, override)
    /// - Damage type resistances/vulnerabilities
    /// - Modified stamina/mana consumption rates
    /// - Health/mana/stamina regeneration with modifiers
    /// - Status effect management (buffs, debuffs, passives)
    /// </summary>
    public class StatsEngine
    {
        // ─── Base Stats ────────────────────────────────────────────────────────
        private float _baseMaxHealth = 100f;
        private float _baseMaxMana = 50f;
        private float _baseMaxStamina = 100f;
        private float _baseHealthRegen = 2f;
        private float _baseManaRegen = 5f;
        private float _baseStaminaRegen = 10f;
        private float _baseCriticalChance = 0.05f;
        private float _baseCriticalDamage = 2f;

        // ─── Current Resources ─────────────────────────────────────────────────
        private float _currentHealth;
        private float _currentMana;
        private float _currentStamina;

        // ─── Out of Combat Regeneration ────────────────────────────────────────
        private float _lastStaminaUseTime = -10f; // Negative so OOC regen is active at start
        private const float OutOfCombatDelay = 3f; // Seconds without using stamina to get OOC regen
        private const float OutOfCombatMultiplier = 1.5f; // 1.5x regen when out of combat (nerfed from 2x)

        // ─── Stat Modifiers ────────────────────────────────────────────────────
        private readonly StatModifierCollection _healthModifiers = new();
        private readonly StatModifierCollection _manaModifiers = new();
        private readonly StatModifierCollection _staminaModifiers = new();
        private readonly StatModifierCollection _healthRegenModifiers = new();
        private readonly StatModifierCollection _manaRegenModifiers = new();
        private readonly StatModifierCollection _staminaRegenModifiers = new();
        private readonly StatModifierCollection _healthCostModifiers = new();
        private readonly StatModifierCollection _staminaCostModifiers = new();
        private readonly StatModifierCollection _manaCostModifiers = new();
        private readonly StatModifierCollection _damageModifiers = new();
        private readonly StatModifierCollection _resistanceModifiers = new();

        // ─── Resistances ───────────────────────────────────────────────────────
        private readonly Dictionary<DamageType, float> _resistances = new()
        {
            { DamageType.Physical, 0f },
            { DamageType.Fire, 0f },
            { DamageType.Ice, 0f },
            { DamageType.Lightning, 0f },
            { DamageType.Poison, 0f },
            { DamageType.Magic, 0f },
            { DamageType.Holy, 0f },
            { DamageType.Shadow, 0f },
            { DamageType.Bleed, 0f },
            { DamageType.Corruption, 0f },
            { DamageType.True, 0f }
        };

        // ─── Status Effects ────────────────────────────────────────────────────
        private readonly List<StatusEffectData> _activeEffects = new();
        private readonly Dictionary<string, StatusEffectData> _effectsById = new();

        // ─── Events ────────────────────────────────────────────────────────────
        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnManaChanged;
        public event Action<float, float> OnStaminaChanged;
        public event Action<StatusEffectData> OnEffectAdded;
        public event Action<StatusEffectData> OnEffectRemoved;
#pragma warning disable CS0067 // Event is never used (kept for future damage tracking)
        public event Action<DamageInfo, float> OnDamageTaken;
#pragma warning restore CS0067
        public event Action OnStatsRecalculated;

        // ─── Properties ────────────────────────────────────────────────────────
        public float CurrentHealth => _currentHealth;
        public float CurrentMana => _currentMana;
        public float CurrentStamina => _currentStamina;
        public IReadOnlyList<StatusEffectData> ActiveEffects => _activeEffects.AsReadOnly();

        // Calculated effective stats
        public float MaxHealth => CalculateStat(_baseMaxHealth, _healthModifiers);
        public float MaxMana => CalculateStat(_baseMaxMana, _manaModifiers);
        public float MaxStamina => CalculateStat(_baseMaxStamina, _staminaModifiers);
        public float HealthRegen => CalculateStat(_baseHealthRegen, _healthRegenModifiers);
        public float ManaRegen => CalculateStat(_baseManaRegen, _manaRegenModifiers);
        public float StaminaRegen => CalculateStat(_baseStaminaRegen, _staminaRegenModifiers);
        public float HealthCostMultiplier => 1f + CalculateStat(0f, _healthCostModifiers);
        public float StaminaCostMultiplier => 1f + CalculateStat(0f, _staminaCostModifiers);
        public float ManaCostMultiplier => 1f + CalculateStat(0f, _manaCostModifiers);
        public float DamageMultiplier => 1f + CalculateStat(0f, _damageModifiers);
        public float CriticalChance => _baseCriticalChance;
        public float CriticalDamage => _baseCriticalDamage;

        // ─── Initialization ────────────────────────────────────────────────────
        public StatsEngine()
        {
            _currentHealth = MaxHealth;
            _currentMana = MaxMana;
            _currentStamina = MaxStamina;
        }

        public void SetBaseStats(float maxHealth, float maxMana, float maxStamina,
                                  float healthRegen = 2f, float manaRegen = 5f, float staminaRegen = 10f)
        {
            _baseMaxHealth = maxHealth;
            _baseMaxMana = maxMana;
            _baseMaxStamina = maxStamina;
            _baseHealthRegen = healthRegen;
            _baseManaRegen = manaRegen;
            _baseStaminaRegen = staminaRegen;

            RecalculateAll();
        }

        // ─── Stat Calculation ──────────────────────────────────────────────────

        /// <summary>
        /// Calculate effective stat value after applying all modifiers
        /// </summary>
        public float CalculateStat(float baseValue, StatModifierCollection modifiers)
        {
            return modifiers.Calculate(baseValue, Time.time);
        }

        /// <summary>
        /// Get resistance multiplier for a damage type
        /// </summary>
        public float GetResistanceMultiplier(DamageType type)
        {
            if (!_resistances.TryGetValue(type, out float resistance))
                return 1f;

            // resistance = 0.5 → 50% damage reduction (multiplier = 0.5)
            // resistance = -0.25 → 25% damage increase (multiplier = 1.25)
            return Mathf.Max(0.1f, 1f - resistance);
        }

        /// <summary>
        /// Get resistance percentage (-20% = weakness, +30% = resistance)
        /// </summary>
        public float GetResistancePercent(DamageType type)
        {
            float multiplier = GetResistanceMultiplier(type);
            return (1f - multiplier) * 100f;
        }

        /// <summary>
        /// Recalculate all stats and notify listeners
        /// </summary>
        public void RecalculateAll()
        {
            // Adjust current resources proportionally when max changes
            _currentHealth = Mathf.Min(_currentHealth, MaxHealth);
            _currentMana = Mathf.Min(_currentMana, MaxMana);
            _currentStamina = Mathf.Min(_currentStamina, MaxStamina);

            OnStatsRecalculated?.Invoke();
            OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
            OnManaChanged?.Invoke(_currentMana, MaxMana);
            OnStaminaChanged?.Invoke(_currentStamina, MaxStamina);
        }

        // ─── Stat Modifiers ────────────────────────────────────────────────────

        /// <summary>
        /// Add a stat modifier
        /// </summary>
        public void AddModifier(string statName, string id, string sourceId, ModifierType type, float value, float duration = 0f)
        {
            var modifier = new StatModifier(statName, id, sourceId, type, value, duration);
            GetCollection(statName)?.Add(modifier);
            RecalculateAll();
        }

        /// <summary>
        /// Remove all modifiers from a source
        /// </summary>
        public void RemoveModifiersBySource(string sourceId)
        {
            _healthModifiers.RemoveBySource(sourceId);
            _manaModifiers.RemoveBySource(sourceId);
            _staminaModifiers.RemoveBySource(sourceId);
            _healthRegenModifiers.RemoveBySource(sourceId);
            _manaRegenModifiers.RemoveBySource(sourceId);
            _staminaRegenModifiers.RemoveBySource(sourceId);
            _healthCostModifiers.RemoveBySource(sourceId);
            _staminaCostModifiers.RemoveBySource(sourceId);
            _manaCostModifiers.RemoveBySource(sourceId);
            _damageModifiers.RemoveBySource(sourceId);
            _resistanceModifiers.RemoveBySource(sourceId);

            RecalculateAll();
        }

        private StatModifierCollection GetCollection(string statName) => statName switch
        {
            "health" => _healthModifiers,
            "mana" => _manaModifiers,
            "stamina" => _staminaModifiers,
            "healthRegen" => _healthRegenModifiers,
            "manaRegen" => _manaRegenModifiers,
            "staminaRegen" => _staminaRegenModifiers,
            "healthCost" => _healthCostModifiers,
            "staminaCost" => _staminaCostModifiers,
            "manaCost" => _manaCostModifiers,
            "damage" => _damageModifiers,
            "resistance" => _resistanceModifiers,
            _ => null
        };

        // ─── Resistance Management ─────────────────────────────────────────────

        /// <summary>
        /// Modify resistance for a damage type
        /// </summary>
        public void ModifyResistance(DamageType type, float amount, ModifierType modifierType, float duration = 0f, string sourceId = null)
        {
            var modifier = new StatModifier($"res_{type}", $"res_{type}_{sourceId}", sourceId, modifierType, amount, duration);
            _resistanceModifiers.Add(modifier);

            // Update resistance value
            if (_resistances.ContainsKey(type))
            {
                _resistances[type] = modifierType == ModifierType.Additive
                    ? _resistances[type] + amount
                    : _resistances[type] * (1f + amount);
            }
        }

        /// <summary>
        /// Set base resistance for a damage type
        /// </summary>
        public void SetBaseResistance(DamageType type, float value)
        {
            if (_resistances.ContainsKey(type))
                _resistances[type] = value;
        }

        // ─── Status Effects Management ─────────────────────────────────────────

        /// <summary>
        /// Apply a status effect (buff, debuff, etc.)
        /// </summary>
        public bool ApplyEffect(StatusEffectData effectData, string applierId = null)
        {
            if (effectData == null) return false;

            // Clone for runtime
            var effect = effectData.Clone();
            effect.timeApplied = Time.time;
            effect.remainingTime = effect.IsInfinite ? float.MaxValue : effect.duration;
            effect.currentStacks = 1;
            effect.applierId = applierId;

            // Check for existing effect
            if (_effectsById.TryGetValue(effect.id, out var existing))
            {
                if (existing.TryStack(effect))
                {
                    // Apply stat modifiers for new stack
                    ApplyEffectModifiers(effect);
                    OnEffectAdded?.Invoke(existing);
                    return true;
                }
                return false;
            }

            // Add new effect
            _activeEffects.Add(effect);
            _effectsById[effect.id] = effect;

            // Apply stat modifiers
            ApplyEffectModifiers(effect);

            OnEffectAdded?.Invoke(effect);
            RecalculateAll();

            // Spawn VFX if any
            if (effect.prefab != null)
            {
                // VFX spawning would go here
            }

            return true;
        }

        /// <summary>
        /// Remove a status effect
        /// </summary>
        public void RemoveEffect(string effectId)
        {
            if (!_effectsById.TryGetValue(effectId, out var effect))
                return;

            // Remove stat modifiers
            RemoveModifiersBySource(effectId);

            _activeEffects.Remove(effect);
            _effectsById.Remove(effectId);

            OnEffectRemoved?.Invoke(effect);
            RecalculateAll();
        }

        /// <summary>
        /// Remove all effects of a specific type
        /// </summary>
        public void RemoveEffectsByType(EffectType type)
        {
            var toRemove = new List<string>();
            foreach (var effect in _activeEffects)
            {
                if (effect.effectType == type)
                    toRemove.Add(effect.id);
            }

            foreach (var id in toRemove)
                RemoveEffect(id);
        }

        /// <summary>
        /// Remove all dispellable effects
        /// </summary>
        public void DispelAll()
        {
            var toRemove = new List<string>();
            foreach (var effect in _activeEffects)
            {
                if (effect.isDispellable)
                    toRemove.Add(effect.id);
            }

            foreach (var id in toRemove)
                RemoveEffect(id);
        }

        /// <summary>
        /// Check if player has a specific effect
        /// </summary>
        public bool HasEffect(string effectId) => _effectsById.ContainsKey(effectId);

        /// <summary>
        /// Get effect intensity
        /// </summary>
        public float GetEffectIntensity(string effectId)
        {
            if (!_effectsById.TryGetValue(effectId, out var effect))
                return 0f;
            return effect.intensity * effect.currentStacks;
        }

        /// <summary>
        /// Apply stat modifiers from an effect
        /// </summary>
        private void ApplyEffectModifiers(StatusEffectData effect)
        {
            if (effect.StatModifiers == null) return;

            foreach (var mod in effect.StatModifiers.Modifiers)
            {
                var collection = GetCollection(mod.statName);
                if (collection != null)
                {
                    collection.Add(new StatModifier(
                        mod.statName,
                        mod.id,
                        effect.id,
                        mod.type,
                        mod.value * effect.currentStacks,
                        effect.remainingTime
                    ));
                }
            }
        }

        /// <summary>
        /// Update all active effects
        /// </summary>
        public void UpdateEffects()
        {
            var toRemove = new List<string>();

            foreach (var effect in _activeEffects)
            {
                if (!effect.IsInfinite)
                {
                    effect.remainingTime -= Time.deltaTime;
                }

                // Tick DoT/HoT
                if (effect.tickRate > 0f && Time.time >= effect.nextTickTime)
                {
                    ApplyEffectTick(effect);
                    effect.nextTickTime = Time.time + effect.tickRate;
                }

                if (effect.IsExpired)
                    toRemove.Add(effect.id);
            }

            foreach (var id in toRemove)
                RemoveEffect(id);
        }

        /// <summary>
        /// Apply effect tick (DoT/HoT)
        /// </summary>
        private void ApplyEffectTick(StatusEffectData effect)
        {
            float intensity = effect.intensity * effect.currentStacks;

            // Damage over time
            if (effect.damageOverTime > 0)
            {
                var damageInfo = new DamageInfo(
                    effect.damageOverTime * intensity,
                    effect.damageType,
                    effect.id
                );
                // Damage application handled by PlayerStats
            }

            // Heal over time
            if (effect.healOverTime > 0)
            {
                // Healing handled by PlayerStats
            }

            // Corruption curse - reduces healing received
            if (effect.effectType == EffectType.Curse && effect.damageType == DamageType.Corruption)
            {
                // Apply healing reduction debuff
                // Handled by stat modifiers on healthRegen
            }
        }

        // ─── Resource Management ───────────────────────────────────────────────

        /// <summary>
        /// Use health with cost modifiers applied (for abilities that cost health)
        /// </summary>
        public bool UseHealth(float baseAmount)
        {
            float actualCost = baseAmount * HealthCostMultiplier;
            actualCost = Mathf.Max(0.1f, actualCost);

            if (_currentHealth < actualCost) return false;

            _currentHealth -= actualCost;
            OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
            return true;
        }

        /// <summary>
        /// Use mana with cost modifiers applied
        /// </summary>
        public bool UseMana(float baseAmount)
        {
            float actualCost = baseAmount * ManaCostMultiplier;
            actualCost = Mathf.Max(0.1f, actualCost);

            if (_currentMana < actualCost) return false;

            _currentMana -= actualCost;
            OnManaChanged?.Invoke(_currentMana, MaxMana);
            return true;
        }

        /// <summary>
        /// Use stamina with cost modifiers applied
        /// </summary>
        public bool UseStamina(float baseAmount)
        {
            float actualCost = baseAmount * StaminaCostMultiplier;
            actualCost = Mathf.Max(0.01f, actualCost);

            if (_currentStamina < actualCost) return false;

            _currentStamina -= actualCost;
            _lastStaminaUseTime = Time.time; // Track last stamina use for OOC regen
            OnStaminaChanged?.Invoke(_currentStamina, MaxStamina);
            return true;
        }

        /// <summary>
        /// Restore mana
        /// </summary>
        public void RestoreMana(float amount)
        {
            _currentMana = Mathf.Min(_currentMana + amount, MaxMana);
            OnManaChanged?.Invoke(_currentMana, MaxMana);
        }

        /// <summary>
        /// Restore stamina
        /// </summary>
        public void RestoreStamina(float amount)
        {
            _currentStamina = Mathf.Min(_currentStamina + amount, MaxStamina);
            OnStaminaChanged?.Invoke(_currentStamina, MaxStamina);
        }

        /// <summary>
        /// Modify health directly (for damage/healing)
        /// </summary>
        public void ModifyHealth(float delta)
        {
            _currentHealth = Mathf.Clamp(_currentHealth + delta, 0f, MaxHealth);
            OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
        }

        /// <summary>
        /// Check if player can afford costs
        /// </summary>
        public bool CanAfford(float healthCost = 0f, float manaCost = 0f, float staminaCost = 0f)
        {
            return _currentHealth >= healthCost * HealthCostMultiplier &&
                   _currentMana >= manaCost * ManaCostMultiplier &&
                   _currentStamina >= staminaCost * StaminaCostMultiplier;
        }

        // ─── Damage Calculation ────────────────────────────────────────────────

        /// <summary>
        /// Calculate final damage after resistances and modifiers
        /// </summary>
        public float CalculateDamage(DamageInfo damageInfo)
        {
            float damage = damageInfo.amount;

            // Apply resistance
            float resistanceMultiplier = GetResistanceMultiplier(damageInfo.type);
            damage *= resistanceMultiplier;

            // Apply damage modifiers
            damage *= DamageMultiplier;

            // Critical hit
            if (damageInfo.isCritical || UnityEngine.Random.value < _baseCriticalChance)
            {
                damage *= damageInfo.criticalMultiplier;
            }

            return Mathf.Max(0f, damage);
        }

        // ─── Regeneration ──────────────────────────────────────────────────────

        /// <summary>
        /// Apply regeneration for all resources
        /// Out-of-combat stamina regen: 2x regen after 3 seconds without using stamina
        /// </summary>
        public void ApplyRegeneration(float deltaTime)
        {
            if (HealthRegen > 0 && _currentHealth < MaxHealth)
            {
                _currentHealth = Mathf.Min(_currentHealth + HealthRegen * deltaTime, MaxHealth);
                OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
            }

            if (ManaRegen > 0 && _currentMana < MaxMana)
            {
                _currentMana = Mathf.Min(_currentMana + ManaRegen * deltaTime, MaxMana);
                OnManaChanged?.Invoke(_currentMana, MaxMana);
            }

            if (StaminaRegen > 0 && _currentStamina < MaxStamina)
            {
                // Check if out of combat (no stamina use for X seconds)
                float timeSinceStaminaUse = Time.time - _lastStaminaUseTime;
                float regenMultiplier = (timeSinceStaminaUse >= OutOfCombatDelay) 
                    ? OutOfCombatMultiplier 
                    : 1f;
                
                float effectiveRegen = StaminaRegen * regenMultiplier;
                _currentStamina = Mathf.Min(_currentStamina + effectiveRegen * deltaTime, MaxStamina);
                OnStaminaChanged?.Invoke(_currentStamina, MaxStamina);
            }
        }

        // ─── Utility ───────────────────────────────────────────────────────────

        /// <summary>
        /// Clear all status effects
        /// </summary>
        public void ClearAllEffects()
        {
            var ids = new List<string>(_effectsById.Keys);
            foreach (var id in ids)
                RemoveEffect(id);
        }

        /// <summary>
        /// Get all effects by category
        /// </summary>
        public List<StatusEffectData> GetEffectsByCategory(string category)
        {
            var result = new List<StatusEffectData>();
            foreach (var effect in _activeEffects)
            {
                if (effect.category == category)
                    result.Add(effect);
            }
            return result;
        }

        /// <summary>
        /// Get all buffs
        /// </summary>
        public List<StatusEffectData> GetBuffs()
        {
            var result = new List<StatusEffectData>();
            foreach (var effect in _activeEffects)
            {
                if (effect.effectType == EffectType.Buff || effect.effectType == EffectType.Passive)
                    result.Add(effect);
            }
            return result;
        }

        /// <summary>
        /// Get all debuffs (including curses)
        /// </summary>
        public List<StatusEffectData> GetDebuffs()
        {
            var result = new List<StatusEffectData>();
            foreach (var effect in _activeEffects)
            {
                if (effect.effectType == EffectType.Debuff || effect.effectType == EffectType.Curse)
                    result.Add(effect);
            }
            return result;
        }

        /// <summary>
        /// Get all curses
        /// </summary>
        public List<StatusEffectData> GetCurses()
        {
            var result = new List<StatusEffectData>();
            foreach (var effect in _activeEffects)
            {
                if (effect.effectType == EffectType.Curse)
                    result.Add(effect);
            }
            return result;
        }
    }
}
