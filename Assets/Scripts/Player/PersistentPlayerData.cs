using System.Collections.Generic;
using UnityEngine;

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

    // Status Effects
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
        maxHealth = 100f;
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
public class StatusEffectData
{
    public string id;
    public string effectName;
    public StatusEffectType type;
    public float duration;
    public float intensity;
    public int currentStacks;
    public float remainingTime;
    public float tickRate;

    public StatusEffectData() { }

    public StatusEffectData(StatusEffect effect)
    {
        if (effect == null) return; // guard against null
        id = effect.id;
        effectName = effect.effectName;
        type = effect.type;
        duration = effect.duration;
        intensity = effect.intensity;
        currentStacks = effect.currentStacks;
        remainingTime = effect.remainingTime;
        tickRate = effect.tickRate;
    }

    public StatusEffect ToStatusEffect()
    {
        StatusEffect effect = new StatusEffect
        {
            id = id,
            effectName = effectName,
            type = type,
            duration = duration,
            intensity = intensity,
            currentStacks = currentStacks,
            remainingTime = remainingTime,
            tickRate = tickRate,
            nextTickTime = Time.time
        };
        return effect;
    }
}

[System.Serializable]
public class InventorySlotData
{
    public int slotIndex;
    public string itemName;
    public int quantity;
    public ItemType itemType;

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

