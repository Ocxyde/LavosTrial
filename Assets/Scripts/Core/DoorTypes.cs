// DoorTypes.cs
// Door variant and trap type enums for Core assembly
// Unity 6 compatible - UTF-8 encoding - Unix line endings

namespace Code.Lavos.Core
{
    /// <summary>
    /// Door types with different behaviors and interactions.
    /// </summary>
    public enum DoorVariant
    {
        Normal,         // Standard door, opens normally
        Locked,         // Requires key to open
        Trapped,        // Triggers trap when opened
        Secret,         // Hidden door, reveals secret area
        OneWay,         // Can only pass through one direction
        Cursed,         // Applies debuff when opened
        Blessed,        // Applies buff when opened
        Boss            // Boss door, special effects
    }

    /// <summary>
    /// Trap types that can be attached to doors.
    /// </summary>
    public enum DoorTrapType
    {
        None,
        Spike,          // Deals physical damage
        Fire,           // Deals fire damage
        Poison,         // Applies poison DoT
        Freeze,         // Slows/freezes player
        Shock,          // Deals lightning damage
        Teleport,       // Teleports player elsewhere
        Alarm,          // Alerts enemies
        Collapse        // Blocks passage after use
    }
}