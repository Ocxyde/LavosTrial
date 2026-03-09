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
// PersistentPlayerData.cs
// Persistent player data between sessions
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Player system - save/load data

using System.Collections.Generic;
using UnityEngine;
using Code.Lavos.Status;

namespace Code.Lavos.Player
{
    [System.Serializable]
    public class PersistentPlayerData
    {
        // Health/Mana/Stamina
        public float currentHealth;
        public float maxHealth;
        public float currentMana;
        public float maxMana;
        public float currentStamina;
        public float maxStamina;

        // Status Effects (using new Status system)
        public List<StatusEffectData> activeEffects = new List<StatusEffectData>();

        // Inventory
        public List<InventorySlotData> inventorySlots = new List<InventorySlotData>();
        public int inventoryCapacity;

        // Player Position (for respawning)
        public Vector3 lastPosition;
        public Quaternion lastRotation;

        // Other stats
        public int level;
        public int experience;
        public float movementSpeed;
        public float jumpForce;

        public PersistentPlayerData()
        {
            // Default values
            maxHealth = 1000f;
            maxMana = 50f;
            maxStamina = 100f;
            currentHealth = maxHealth;
            currentMana = maxMana;
            currentStamina = maxStamina;
            inventoryCapacity = 20;
            level = 1;
            experience = 0;
            movementSpeed = 5f;
            jumpForce = 7f;
        }
    }

    [System.Serializable]
    public class InventorySlotData
    {
        public int slotIndex;
        public string itemName;
        public int quantity;
        public InventoryItemType itemType;

        public InventorySlotData() { }

        public InventorySlotData(InventorySlot slot)
        {
            slotIndex = slot.slotIndex;
            if (slot.item != null)
            {
                itemName = slot.item.itemName;
                quantity = slot.quantity;
                itemType = slot.item.itemType;
            }
        }

        public InventorySlot ToInventorySlot()
        {
            InventorySlot slot = new InventorySlot
            {
                slotIndex = slotIndex,
                quantity = quantity
            };
            return slot;
        }
    }
}

