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
// DamageType.cs
// Damage type enumeration for combat and resistance system
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Status system - used by StatsEngine.cs for damage calculation

using UnityEngine;

namespace Code.Lavos.Status
{
    /// <summary>
    /// All damage types in the game. Each type can have different resistances and effects.
    /// </summary>
    public enum DamageType
    {
        /// <summary>Standard physical damage from weapons and attacks</summary>
        Physical,
        /// <summary>Fire damage, often paired with burning effects</summary>
        Fire,
        /// <summary>Ice damage, often paired with slowing effects</summary>
        Ice,
        /// <summary>Lightning damage, often paired with stunning effects</summary>
        Lightning,
        /// <summary>Poison damage, often paired with damage-over-time</summary>
        Poison,
        /// <summary>Magical damage from spells</summary>
        Magic,
        /// <summary>Holy damage, effective against undead/dark creatures</summary>
        Holy,
        /// <summary>Shadow damage, effective against holy creatures</summary>
        Shadow,
        /// <summary>Bleed damage, damage-over-time from wounds</summary>
        Bleed,
        /// <summary>Curse-based damage that drains stats and healing</summary>
        Corruption,
        /// <summary>True damage that ignores all resistances</summary>
        True
    }

    /// <summary>
    /// Damage information with type and context for combat calculations
    /// </summary>
    public struct DamageInfo
    {
        /// <summary>Base damage amount before modifiers</summary>
        public float amount;
        /// <summary>Type of damage for resistance calculation</summary>
        public DamageType type;
        /// <summary>ID of the damage source (skill, effect, etc.)</summary>
        public string sourceId;
        /// <summary>Whether this is a critical hit</summary>
        public bool isCritical;
        /// <summary>Critical hit multiplier (default: 2x)</summary>
        public float criticalMultiplier;
        /// <summary>Position of the damage source for directional effects</summary>
        public Vector3 sourcePosition;

        /// <summary>
        /// Creates a new DamageInfo with basic damage information
        /// </summary>
        /// <param name="amount">Base damage amount</param>
        /// <param name="type">Damage type (default: Physical)</param>
        /// <param name="sourceId">Source identifier (default: null)</param>
        public DamageInfo(float amount, DamageType type = DamageType.Physical, string sourceId = null)
        {
            this.amount = amount;
            this.type = type;
            this.sourceId = sourceId;
            this.isCritical = false;
            this.criticalMultiplier = 2f;
            this.sourcePosition = Vector3.zero;
        }

        /// <summary>
        /// Creates a copy with critical hit settings
        /// </summary>
        /// <param name="critical">Whether this is a critical hit</param>
        /// <param name="multiplier">Critical multiplier (default: 2x)</param>
        /// <returns>A new DamageInfo with critical settings applied</returns>
        public DamageInfo WithCritical(bool critical, float multiplier = 2f)
        {
            var copy = this;
            copy.isCritical = critical;
            copy.criticalMultiplier = multiplier;
            return copy;
        }

        /// <summary>
        /// Creates a copy with source position for directional effects
        /// </summary>
        /// <param name="position">Source position vector</param>
        /// <returns>A new DamageInfo with source position set</returns>
        public DamageInfo WithSource(Vector3 position)
        {
            var copy = this;
            copy.sourcePosition = position;
            return copy;
        }

        /// <summary>
        /// Calculates final damage including critical multiplier
        /// </summary>
        /// <returns>Final damage amount after critical calculation</returns>
        public float CalculateFinalDamage() => isCritical ? amount * criticalMultiplier : amount;
    }
}
