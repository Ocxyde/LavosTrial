using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Status
{
    /// <summary>
    /// Type of stat modification
    /// </summary>
    public enum ModifierType
    {
        Additive,       // Flat addition (+10 health)
        Multiplicative, // Percentage (+20% health)
        Override        // Set exact value
    }

    /// <summary>
    /// A modifier applied to a specific stat
    /// </summary>
    [Serializable]
    public class StatModifier
    {
        public string statName;      // Which stat this modifies
        public string id;            // Unique modifier ID
        public string sourceId;      // Source effect/item ID
        public ModifierType type;
        public float value;
        public float duration;       // 0 = permanent
        public bool isPercentage;

        public StatModifier() { }

        public StatModifier(string statName, string id, string sourceId, ModifierType type, float value, float duration = 0f)
        {
            this.statName = statName;
            this.id = id;
            this.sourceId = sourceId;
            this.type = type;
            this.value = value;
            this.duration = duration;
            this.isPercentage = type == ModifierType.Multiplicative;
        }

        public bool IsExpired(float timeSinceApplied) => duration > 0 && timeSinceApplied >= duration;

        public StatModifier Clone() => new StatModifier(statName, id, sourceId, type, value, duration);
    }

    /// <summary>
    /// Collection of stat modifiers with calculation logic
    /// </summary>
    [Serializable]
    public class StatModifierCollection
    {
        [SerializeField] private List<StatModifier> _modifiers = new();

        public IReadOnlyList<StatModifier> Modifiers => _modifiers.AsReadOnly();
        public int Count => _modifiers.Count;

        public void Add(StatModifier modifier) => _modifiers.Add(modifier);

        public void RemoveById(string id) => _modifiers.RemoveAll(m => m.id == id);

        public void RemoveBySource(string sourceId) => _modifiers.RemoveAll(m => m.sourceId == sourceId);

        public void Clear() => _modifiers.Clear();

        /// <summary>
        /// Calculate final value: Base → Additive → Multiplicative → Override
        /// </summary>
        public float Calculate(float baseValue, float currentTime)
        {
            float additive = 0f;
            float multiplier = 1f;
            float? overrideVal = null;

            foreach (var mod in _modifiers)
            {
                if (mod.IsExpired(currentTime - mod.duration)) continue;

                switch (mod.type)
                {
                    case ModifierType.Additive:
                        additive += mod.value;
                        break;
                    case ModifierType.Multiplicative:
                        multiplier *= (1f + mod.value);
                        break;
                    case ModifierType.Override:
                        overrideVal = mod.value;
                        break;
                }
            }

            float result = (baseValue + additive) * multiplier;
            return overrideVal.HasValue ? overrideVal.Value : result;
        }

        public float GetTotalBonus(string statName)
        {
            float total = 0f;
            foreach (var mod in _modifiers)
            {
                if (mod.statName == statName && mod.type == ModifierType.Additive)
                    total += mod.value;
            }
            return total;
        }

        public float GetTotalMultiplier(string statName)
        {
            float mult = 1f;
            foreach (var mod in _modifiers)
            {
                if (mod.statName == statName && mod.type == ModifierType.Multiplicative)
                    mult *= (1f + mod.value);
            }
            return mult;
        }
    }
}
