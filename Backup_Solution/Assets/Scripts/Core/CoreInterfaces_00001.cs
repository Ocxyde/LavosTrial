// CoreInterfaces.cs
// Interfaces for plug-in-and-out architecture
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// Core defines interfaces, plugins (Player, Inventory, etc.) implement them

namespace Code.Lavos.Core
{
    /// <summary>
    /// Interface for player stats - implemented by PlayerStats in Player assembly
    /// </summary>
    public interface IPlayerStats
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        float CurrentMana { get; }
        float MaxMana { get; }
        float CurrentStamina { get; }
        float MaxStamina { get; }
        
        void TakeDamage(float amount, DamageType damageType);
        void Heal(float amount);
        void UseMana(float amount);
        void UseStamina(float amount);
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
    /// </summary>
    public interface IInventory
    {
        bool UseItem(int slotIndex, UnityEngine.GameObject user);
        InventorySlot[] GetAllSlots();
    }

    /// <summary>
    /// Simple inventory slot data (Core-only, no external dependencies)
    /// </summary>
    public struct InventorySlot
    {
        public ItemData item;
        public int quantity;
        public bool IsEmpty => item == null || quantity <= 0;
    }

    /// <summary>
    /// Interface for interactable objects - implemented in Core
    /// </summary>
    public interface IInteractable
    {
        string GetInteractionPrompt();
        bool CanInteract(UnityEngine.GameObject interactor);
        void OnInteract(UnityEngine.GameObject interactor);
        void OnHighlightEnter(UnityEngine.GameObject interactor);
        void OnHighlightExit(UnityEngine.GameObject interactor);
    }

    /// <summary>
    /// Interface for maze renderer - implemented by MazeRenderer in Ressources assembly
    /// </summary>
    public interface IMazeRenderer
    {
        void BuildMaze();
        void SetWall(int x, int y, MazeGenerator.Wall walls);
    }
}
