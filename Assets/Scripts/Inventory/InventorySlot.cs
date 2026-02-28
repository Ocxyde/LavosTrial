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
