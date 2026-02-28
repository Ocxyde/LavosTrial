using UnityEngine;

public enum ItemType { Consumable, Equipment, Quest, Key, Resource }
public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }

[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string id;
    public string itemName;
    [TextArea] public string description;
    public ItemType itemType;
    public ItemRarity rarity = ItemRarity.Common;
    public Sprite icon;
    public GameObject prefab;

    [Header("Stats")]
    public int value;
    public int maxStack = 1;
    public bool isUnique;
    public bool isDroppable = true;
    public bool isTradable = true;

    [Header("Usage")]
    public float cooldownTime;
    public string useSound;

    [Header("Equipment")]
    public string slotType;
    public float damageBonus;
    public float defenseBonus;
    public float speedBonus;

    [Header("Consumable")]
    public float healthRestore;
    public float manaRestore;
    public float staminaRestore;
    public float duration;

    public virtual void OnUse(GameObject user)
    {
        Debug.Log($"[Item] Using {itemName}");

        if (itemType != ItemType.Consumable) return;

        var stats = user.GetComponent<PlayerStats>();
        if (stats == null) return;

        if (healthRestore > 0f) stats.Heal(healthRestore);
        if (manaRestore > 0f) stats.RestoreMana(manaRestore);
        if (staminaRestore > 0f) stats.RestoreStamina(staminaRestore);
    }

    public virtual void OnPickup(GameObject picker) => Debug.Log($"[Item] Picked up {itemName}");
    public virtual void OnDrop(GameObject dropper) => Debug.Log($"[Item] Dropped {itemName}");
}
