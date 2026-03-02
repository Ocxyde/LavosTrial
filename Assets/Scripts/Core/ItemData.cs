// ItemData.cs
// ScriptableObject data for inventory items
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Part of the Inventory system - works with Inventory.cs

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Inventory-specific item type enum.
    /// (Different from Core.ItemType used for maze/spawning system)
    /// </summary>
    public enum InventoryItemType { Consumable, Equipment, Quest, Key, Resource }
    public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }

    [CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Info")]
        public string id;
        public string itemName;
        [TextArea] public string description;
        public InventoryItemType itemType;
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

            if (itemType != InventoryItemType.Consumable) return;

            // Use direct component reference for better performance
            var playerStats = user.GetComponent<IPlayerStats>();
            if (playerStats == null) return;

            // Direct method calls - no reflection overhead
            if (healthRestore > 0f)
            {
                playerStats.Heal(healthRestore);
            }
            if (manaRestore > 0f)
            {
                playerStats.RestoreMana(manaRestore);
            }
            if (staminaRestore > 0f)
            {
                playerStats.RestoreStamina(staminaRestore);
            }
        }

        public virtual void OnPickup(GameObject picker) => Debug.Log($"[Item] Picked up {itemName}");
        public virtual void OnDrop(GameObject dropper) => Debug.Log($"[Item] Dropped {itemName}");
    }
}
