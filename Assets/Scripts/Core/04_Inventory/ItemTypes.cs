// ItemTypes.cs
// Shared item type enums for Core and Ressources assemblies
// Unity 6 compatible - UTF-8 encoding - Unix line endings

namespace Code.Lavos.Core
{
    /// <summary>
    /// Item types for maze spawning system.
    /// Used by SpawnPlacerEngine and related systems.
    /// </summary>
    public enum ItemType
    {
        Generic,
        Door,
        Chest,
        Pickup,
        Switch,
        Key,
        Lever,
        Button,
        Portal
    }

    /// <summary>
    /// Door placement types.
    /// Used by SpawnPlacerEngine and DoubleDoor.
    /// </summary>
    public enum DoorType
    {
        Start,      // Starting point door (bright)
        Exit,       // Exit door (brightest)
        Random,     // Randomly placed door
        Secret      // Hidden door (dim)
    }
}
