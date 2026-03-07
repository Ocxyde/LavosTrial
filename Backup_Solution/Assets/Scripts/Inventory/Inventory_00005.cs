
// Inventory.cs
// Inventory management system (Singleton)
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Central inventory manager
using System.Collections.Generic;
using UnityEngine;

namespace Code.Lavos.Core
{
    public class Inventory : MonoBehaviour
    {
        public static Inventory Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private int capacity = 20;

        [Header("Events")]
        public System.Action OnInventoryChanged;

        private List<InventorySlot> _slots = new List<InventorySlot>();

    public int Capacity => capacity;
    public int Count => _slots.Count;
    public IReadOnlyList<InventorySlot> Slots => _slots;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        _slots.Clear();
        for (int i = 0; i < capacity; i++)
        {
            var slot = new InventorySlot();
            slot.slotIndex = i;
            _slots.Add(slot);
        }
    }

    public bool AddItem(ItemData item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;

        if (item.maxStack > 1)
        {
            InventorySlot existingSlot = FindStackableSlot(item, quantity);
            if (existingSlot != null)
            {
                existingSlot.Add(quantity);
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        InventorySlot emptySlot = FindEmptySlot();
        if (emptySlot != null)
        {
            emptySlot.item = item;
            emptySlot.quantity = quantity;
            OnInventoryChanged?.Invoke();
            return true;
        }

        Debug.LogWarning($"[Inventory] No space for {item.itemName}");
        return false;
    }

    public bool RemoveItem(ItemData item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;

        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i].item == item && _slots[i].quantity >= quantity)
            {
                _slots[i].Remove(quantity);
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        Debug.LogWarning($"[Inventory] Cannot remove {item.itemName} - not enough quantity");
        return false;
    }

    public bool UseItem(int slotIndex, GameObject user)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Count) return false;

        InventorySlot slot = _slots[slotIndex];
        if (slot.IsEmpty) return false;

        ItemData item = slot.item;
        item.OnUse(user);

        if (item.itemType == InventoryItemType.Consumable)
        {
            slot.Remove(1);
            OnInventoryChanged?.Invoke();
        }

        return true;
    }

    public InventorySlot GetSlot(int index)
    {
        if (index >= 0 && index < _slots.Count)
            return _slots[index];
        return null;
    }

    public void Clear()
    {
        foreach (var slot in _slots)
        {
            slot.Clear();
        }
        OnInventoryChanged?.Invoke();
    }

    public void Sort()
    {
        _slots.Sort((a, b) =>
        {
            if (a.IsEmpty && b.IsEmpty) return 0;
            if (a.IsEmpty) return 1;
            if (b.IsEmpty) return -1;
            return a.item.itemType.CompareTo(b.item.itemType);
        });
        OnInventoryChanged?.Invoke();
    }

    private InventorySlot FindStackableSlot(ItemData item, int quantity)
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i].item == item && _slots[i].CanStack)
            {
                int spaceLeft = item.maxStack - _slots[i].quantity;
                if (spaceLeft >= quantity)
                    return _slots[i];
            }
        }
        return null;
    }

    private InventorySlot FindEmptySlot()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i].IsEmpty)
                return _slots[i];
        }
        return null;
    }

    public int GetItemCount(ItemData item)
    {
        int count = 0;
        foreach (var slot in _slots)
        {
            if (slot.item == item)
                count += slot.quantity;
        }
        return count;
    }

    public bool HasItem(ItemData item, int quantity = 1)
    {
        return GetItemCount(item) >= quantity;
    }

    public List<InventorySlot> GetAllSlots()
    {
        return new List<InventorySlot>(_slots);
    }
}
