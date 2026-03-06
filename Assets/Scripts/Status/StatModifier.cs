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
// StatModifier.cs
// Stat modifier system for buffs, debuffs, and status effects
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Status system - works with StatsEngine.cs and StatusEffectData.cs

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
        /// <summary>Flat addition (+10 health)</summary>
        Additive,
        /// <summary>Percentage (+20% health)</summary>
        Multiplicative,
        /// <summary>Set exact value</summary>
        Override
    }

    /// <summary>
    /// A modifier applied to a specific stat with duration tracking
    /// </summary>
    [Serializable]
    public class StatModifier
    {
        /// <summary>Name of the stat being modified (e.g., "health", "mana", "damage")</summary>
        public string statName;
        /// <summary>Unique identifier for this modifier instance</summary>
        public string id;
        /// <summary>Source effect or item that created this modifier</summary>
        public string sourceId;
        /// <summary>Type of modification (additive, multiplicative, or override)</summary>
        public ModifierType type;
        /// <summary>Value of the modification</summary>
        public float value;
        /// <summary>Duration in seconds (0 = permanent)</summary>
        public float duration;
        /// <summary>Whether this is a percentage-based modifier</summary>
        public bool isPercentage;

        /// <summary>
        /// Creates a new StatModifier with default values
        /// </summary>
        public StatModifier() { }

        /// <summary>
        /// Creates a new StatModifier with specified values
        /// </summary>
        /// <param name="statName">Name of the stat to modify</param>
        /// <param name="id">Unique modifier ID</param>
        /// <param name="sourceId">Source effect/item ID</param>
        /// <param name="type">Type of modification</param>
        /// <param name="value">Modifier value</param>
        /// <param name="duration">Duration in seconds (0 = permanent)</param>
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

        /// <summary>
        /// Checks if this modifier has expired
        /// </summary>
        /// <param name="timeSinceApplied">Time elapsed since the modifier was applied</param>
        /// <returns>True if the modifier has expired (only for timed modifiers)</returns>
        public bool IsExpired(float timeSinceApplied) => duration > 0 && timeSinceApplied >= duration;

        /// <summary>
        /// Creates a copy of this modifier
        /// </summary>
        /// <returns>A new StatModifier with identical values</returns>
        public StatModifier Clone() => new StatModifier(statName, id, sourceId, type, value, duration);
    }

    /// <summary>
    /// Collection of stat modifiers with calculation logic.
    /// Applies modifiers in order: Base → Additive → Multiplicative → Override
    /// </summary>
    [Serializable]
    public class StatModifierCollection
    {
        [SerializeField] private List<StatModifier> _modifiers = new();

        /// <summary>Read-only list of all modifiers</summary>
        public IReadOnlyList<StatModifier> Modifiers => _modifiers.AsReadOnly();
        /// <summary>Number of modifiers in this collection</summary>
        public int Count => _modifiers.Count;

        /// <summary>
        /// Adds a modifier to the collection
        /// </summary>
        /// <param name="modifier">The modifier to add</param>
        public void Add(StatModifier modifier) => _modifiers.Add(modifier);

        /// <summary>
        /// Removes all modifiers with the specified ID
        /// </summary>
        /// <param name="id">The modifier ID to remove</param>
        public void RemoveById(string id) => _modifiers.RemoveAll(m => m.id == id);

        /// <summary>
        /// Removes all modifiers from the specified source
        /// </summary>
        /// <param name="sourceId">The source ID to remove</param>
        public void RemoveBySource(string sourceId) => _modifiers.RemoveAll(m => m.sourceId == sourceId);

        /// <summary>
        /// Removes all modifiers from the collection
        /// </summary>
        public void Clear() => _modifiers.Clear();

        /// <summary>
        /// Calculate final value after applying all valid modifiers
        /// Order: Base → Additive → Multiplicative → Override
        /// </summary>
        /// <param name="baseValue">The base stat value</param>
        /// <param name="currentTime">Current game time for duration checks</param>
        /// <returns>The final calculated stat value</returns>
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

        /// <summary>
        /// Gets the total additive bonus for a specific stat
        /// </summary>
        /// <param name="statName">The stat name to sum bonuses for</param>
        /// <returns>Total additive bonus value</returns>
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

        /// <summary>
        /// Gets the total multiplier for a specific stat
        /// </summary>
        /// <param name="statName">The stat name to calculate multiplier for</param>
        /// <returns>Total multiplier (1.0 = no change)</returns>
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
