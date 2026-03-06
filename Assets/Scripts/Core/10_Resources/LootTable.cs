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
// LootTable.cs
// Loot table definition for chest rewards and item drops
// Unity 6 compatible - UTF-8 encoding - Unix line endings
// Located in Core assembly for use by SpatialPlacer and ChestBehavior

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
