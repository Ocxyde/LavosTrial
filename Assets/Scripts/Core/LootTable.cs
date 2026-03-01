// LootTable.cs
// Loot table definition for chest rewards and item drops
// Unity 6 compatible - UTF-8 encoding - Unix line endings
// Located in Core assembly for use by SpawnPlacerEngine and ChestBehavior

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Loot table for configurable random rewards.
    /// </summary>
    [System.Serializable]
    public class LootTable
    {
        [System.Serializable]
        public class LootEntry
        {
            public ItemData item;
            public float weight = 1f;
            public int minQuantity = 1;
            public int maxQuantity = 1;
        }

        public LootEntry[] entries;

        /// <summary>
        /// Get random loot based on weighted probabilities.
        /// </summary>
        public ItemData GetRandomLoot()
        {
            if (entries == null || entries.Length == 0) return null;

            float totalWeight = 0f;
            foreach (var entry in entries)
            {
                totalWeight += entry.weight;
            }

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var entry in entries)
            {
                cumulative += entry.weight;
                if (roll <= cumulative)
                {
                    return entry.item;
                }
            }

            return null;
        }
    }
}
