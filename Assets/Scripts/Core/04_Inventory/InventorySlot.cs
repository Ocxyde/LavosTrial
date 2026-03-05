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
// InventorySlot.cs
// Inventory slot data structure for plug-in-and-out inventory system
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Inventory system - works with Inventory.cs and InventoryUI.cs

namespace Code.Lavos.Core
{
    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int quantity;
        public int slotIndex;

        public bool IsEmpty => item == null || quantity <= 0;
        public bool CanStack => item != null && quantity < item.maxStack;
        public bool IsFull => item != null && quantity >= item.maxStack;

        public InventorySlot()
        {
            item = null;
            quantity = 0;
        }

        public InventorySlot(ItemData itemData, int qty = 1)
        {
            item = itemData;
            quantity = qty;
        }

        public void Clear()
        {
            item = null;
            quantity = 0;
        }

        public void Add(int amount = 1)
        {
            if (item != null)
                quantity += amount;
        }

        public bool Remove(int amount = 1)
        {
            if (item != null && quantity >= amount)
            {
                quantity -= amount;
                if (quantity <= 0)
                    Clear();
                return true;
            }
            return false;
        }

        public InventorySlot Clone()
        {
            return new InventorySlot(item, quantity);
        }
    }
}
