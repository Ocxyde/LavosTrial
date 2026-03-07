// Copyright (C) 2026 Ocxyde
//
// This file is part of Code.Lavos.
//
// Code.Lavos is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Code.Lavos is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Code.Lavos.  If not, see <https://www.gnu.org/licenses/>.
// StatusEffectData.cs
// Comprehensive status effect data
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Status effect definition and data

// StatusEffectData.cs
// Comprehensive status effect data
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Status effect definition and data
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Status
{
    public enum EffectType { Buff, Debuff, Passive, Temporary, Aura, Triggered, Curse }
    public enum EffectPriority { Low = 0, Normal = 1, High = 2, Critical = 3 }
    public enum StackType { None, Stacks, Refresh, Override }

    /// <summary>
    /// Comprehensive status effect data - handles buffs, debuffs, and all status effects
    /// </summary>
    [Serializable]
    public class StatusEffectData
    {
        // Basic Info
        public string id;
        public string effectName;
        public EffectType effectType;
        public EffectPriority priority;
        public Sprite icon;

        // Duration & Stacking
        public float duration;
        public float tickRate;
        public int maxStacks;
        public StackType stackType;

        // Properties
        public float intensity;
        public bool isDispellable;
        public bool isHidden;
        public string description;
        public string category;

        // Stat Modifiers
        [SerializeField] private StatModifierCollection _statModifiers = new();
        public StatModifierCollection StatModifiers => _statModifiers;

        // DoT / HoT
        public float damageOverTime;
        public DamageType damageType;
        public float healOverTime;

        // Visual/Audio
        public GameObject prefab;
        public AudioClip audioClip;

        // Runtime (non-serialized)
        [NonSerialized] public float remainingTime;
        [NonSerialized] public int currentStacks;
        [NonSerialized] public float nextTickTime;
        [NonSerialized] public float timeApplied;
        [NonSerialized] public string applierId;

        // Properties
        public float MaxDuration => duration;
        public bool IsExpired => remainingTime <= 0f && duration > 0;
        public bool IsInfinite => duration <= 0;
        public bool CanStack => stackType == StackType.Stacks;

        public StatusEffectData()
        {
            effectType = EffectType.Buff;
            priority = EffectPriority.Normal;
            maxStacks = 1;
            stackType = StackType.Refresh;
            isDispellable = true;
            damageType = DamageType.Physical;
        }

        /// <summary>
        /// Add a stat modifier to this effect
        /// </summary>
        public void AddModifier(string statName, ModifierType type, float value)
        {
            _statModifiers.Add(new StatModifier(statName, $"{id}_{statName}", id, type, value, duration));
        }

        /// <summary>
        /// Try to stack with another effect
        /// </summary>
        public bool TryStack(StatusEffectData other)
        {
            if (stackType == StackType.None) return false;

            if (stackType == StackType.Stacks)
            {
                if (maxStacks > 0 && currentStacks >= maxStacks) return false;
                currentStacks++;
                remainingTime = duration;
                return true;
            }

            if (stackType == StackType.Refresh)
            {
                remainingTime = Mathf.Max(remainingTime, other.duration);
                intensity = Mathf.Max(intensity, other.intensity);
                return true;
            }

            if (stackType == StackType.Override)
            {
                currentStacks = other.currentStacks;
                remainingTime = other.duration;
                intensity = other.intensity;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clone this effect for runtime use
        /// </summary>
        public StatusEffectData Clone()
        {
            var clone = new StatusEffectData
            {
                id = this.id,
                effectName = this.effectName,
                effectType = this.effectType,
                priority = this.priority,
                icon = this.icon,
                duration = this.duration,
                tickRate = this.tickRate,
                maxStacks = this.maxStacks,
                stackType = this.stackType,
                intensity = this.intensity,
                isDispellable = this.isDispellable,
                isHidden = this.isHidden,
                description = this.description,
                category = this.category,
                damageOverTime = this.damageOverTime,
                damageType = this.damageType,
                healOverTime = this.healOverTime,
                prefab = this.prefab,
                audioClip = this.audioClip,
                remainingTime = 0f,
                currentStacks = 0,
                nextTickTime = 0f
            };

            foreach (var mod in _statModifiers.Modifiers)
                clone._statModifiers.Add(mod.Clone());

            return clone;
        }

        public string GetDisplayName() => currentStacks > 1 ? $"{effectName} (x{currentStacks})" : effectName;
        public string GetDurationString()
        {
            if (IsInfinite) return "∞";
            return remainingTime >= 60 ? $"{(int)(remainingTime / 60)}:{(int)(remainingTime % 60):D2}" : $"{remainingTime:F1}s";
        }
    }
}
