
// StatusEffect.cs
// Status effect data wrapper
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Status system - effect data
using UnityEngine;

namespace Code.Lavos.Status
{
    /// <summary>
    /// Type of status effect (buff, debuff, or curse)
    /// </summary>
    public enum StatusEffectType { Buff, Debuff, Curse }

    /// <summary>
    /// Legacy StatusEffect class for backward compatibility.
    /// Wraps StatusEffectData with old API.
    /// </summary>
    public class StatusEffect
    {
        public string id;
        public string effectName;
        public StatusEffectType type;
        public Sprite icon;
        public float duration;
        public float intensity;
        public int maxStacks = 1;
        public float tickRate;

        // Runtime (non-serialized)
        [System.NonSerialized] public float remainingTime;
        [System.NonSerialized] public int currentStacks;
        [System.NonSerialized] public float nextTickTime;

        public float MaxDuration => duration;
        public bool IsExpired => remainingTime <= 0f;

        /// <summary>
        /// Convert to StatusEffectData
        /// </summary>
        public StatusEffectData ToEffectData()
        {
            var data = new StatusEffectData
            {
                id = this.id,
                effectName = this.effectName,
                effectType = type == StatusEffectType.Curse ? EffectType.Curse 
                    : (type == StatusEffectType.Buff ? EffectType.Buff : EffectType.Debuff),
                icon = this.icon,
                duration = this.duration,
                intensity = this.intensity,
                maxStacks = this.maxStacks,
                tickRate = this.tickRate,
                remainingTime = this.remainingTime,
                currentStacks = this.currentStacks,
                nextTickTime = this.nextTickTime
            };
            return data;
        }

        /// <summary>
        /// Create from StatusEffectData
        /// </summary>
        public static StatusEffect FromEffectData(StatusEffectData data)
        {
            return new StatusEffect
            {
                id = data.id,
                effectName = data.effectName,
                type = data.effectType == EffectType.Curse ? StatusEffectType.Curse
                    : (data.effectType == EffectType.Buff ? StatusEffectType.Buff : StatusEffectType.Debuff),
                icon = data.icon,
                duration = data.duration,
                intensity = data.intensity,
                maxStacks = data.maxStacks,
                tickRate = data.tickRate,
                remainingTime = data.remainingTime,
                currentStacks = data.currentStacks,
                nextTickTime = data.nextTickTime
            };
        }

        public StatusEffect Clone()
        {
            return new StatusEffect
            {
                id = this.id,
                effectName = this.effectName,
                type = this.type,
                icon = this.icon,
                duration = this.duration,
                intensity = this.intensity,
                maxStacks = this.maxStacks,
                tickRate = this.tickRate,
                remainingTime = 0f,
                currentStacks = 0,
                nextTickTime = 0f
            };
        }
    }
}
