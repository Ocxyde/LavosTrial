// CoreInterfaces.cs
// Interfaces for plug-in-and-out architecture
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Core defines interfaces, plugins (Player, Inventory, etc.) implement them

using Code.Lavos.Status;

namespace Code.Lavos.Core
{
    /// <summary>
    /// Interface for player stats - implemented by PlayerStats in Player assembly
    /// </summary>
    public interface IPlayerStats
    {
        StatsEngine Engine { get; }
        float CurrentHealth { get; }
        float MaxHealth { get; }
        float CurrentMana { get; }
        float MaxMana { get; }
        float CurrentStamina { get; }
        float MaxStamina { get; }

        event System.Action<float, float> OnManaChanged;
        event System.Action<float, float> OnStaminaChanged;

        void TakeDamage(float amount, DamageType damageType);
        void Heal(float amount);
        bool UseMana(float amount);
        bool UseStamina(float amount);
        void RestoreMana(float amount);
        void RestoreStamina(float amount);
    }

    /// <summary>
    /// Interface for player controller - implemented by PlayerController in Player assembly
    /// </summary>
    public interface IPlayerController
    {
        UnityEngine.GameObject gameObject { get; }
        UnityEngine.Transform transform { get; }
        bool IsAlive { get; }
        void ApplyKnockback(UnityEngine.Vector3 direction, float force);
    }

    /// <summary>
    /// Interface for inventory - implemented by Inventory in Inventory assembly
    /// Note: InventorySlot class is defined separately in Core
    /// </summary>
    public interface IInventory
    {
        bool UseItem(int slotIndex, UnityEngine.GameObject user);
        InventorySlot[] GetAllSlots();
    }

    /// <summary>
    /// Interface for maze renderer - implemented by MazeRenderer in Ressources assembly
    /// </summary>
    public interface IMazeRenderer
    {
        void BuildMaze();
    }
}
