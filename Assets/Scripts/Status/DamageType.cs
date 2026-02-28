using UnityEngine;

namespace Code.Lavos.Status
{
    /// <summary>
    /// All damage types in the game
    /// </summary>
    public enum DamageType
    {
        Physical,
        Fire,
        Ice,
        Lightning,
        Poison,
        Magic,
        Holy,
        Shadow,
        Bleed,
        Corruption,    // Curse-based damage, drains stats/healing
        True           // Ignores all resistances
    }

    /// <summary>
    /// Damage information with type and context
    /// </summary>
    public struct DamageInfo
    {
        public float amount;
        public DamageType type;
        public string sourceId;
        public bool isCritical;
        public float criticalMultiplier;
        public Vector3 sourcePosition;

        public DamageInfo(float amount, DamageType type = DamageType.Physical, string sourceId = null)
        {
            this.amount = amount;
            this.type = type;
            this.sourceId = sourceId;
            this.isCritical = false;
            this.criticalMultiplier = 2f;
            this.sourcePosition = Vector3.zero;
        }

        public DamageInfo WithCritical(bool critical, float multiplier = 2f)
        {
            var copy = this;
            copy.isCritical = critical;
            copy.criticalMultiplier = multiplier;
            return copy;
        }

        public DamageInfo WithSource(Vector3 position)
        {
            var copy = this;
            copy.sourcePosition = position;
            return copy;
        }

        public float CalculateFinalDamage() => isCritical ? amount * criticalMultiplier : amount;
    }
}
