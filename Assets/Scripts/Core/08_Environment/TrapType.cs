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
        Pit,
        Spike,
        Dart,
        Fire,
        Flame,
        Poison,
        Freeze,
        Electric,
        Shock,
        Ice,
        Teleport,
        Alarm,
        Collapse,
        Explosion
    }
}
