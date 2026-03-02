// TrapType.cs
// Trap type enum for Core assembly
// Unity 6 compatible - UTF-8 encoding - Unix line endings

namespace Code.Lavos.Core
{
    /// <summary>
    /// Types of traps that can be placed in the maze.
    /// </summary>
    public enum TrapType
    {
        None,
        Spike,          // Deals physical damage
        Fire,           // Deals fire damage over time
        Poison,         // Applies poison DoT
        Freeze,         // Slows/freezes player
        Shock,          // Deals lightning damage
        Teleport,       // Teleports player to random location
        Alarm,          // Alerts nearby enemies
        Collapse        // Causes ceiling/wall collapse
    }
}