// GridMazeCell.cs
// Cell types for custom grid-based maze system
// Unity 6 compatible - UTF-8 encoding - Unix line endings

namespace Code.Lavos.Core
{
    /// <summary>
    /// GridMazeCell - Cell types for grid-based maze generation.
    /// Each cell type fits in 4 bits (0-15) for compact byte storage.
    /// </summary>
    public enum GridMazeCell : byte
    {
        Floor = 0,      // 0000 - Empty space
        Room = 1,       // 0001 - Room interior
        Corridor = 2,   // 0010 - Corridor path
        Wall = 3,       // 0011 - Wall cell
        Door = 4,       // 0100 - Doorway
        SpawnPoint = 5, // 0101 - Spawn/respawn point
        // 6-15 reserved for future types
    }
    
    /// <summary>
    /// GridMazeFlags - Additional cell flags (optional metadata).
    /// Stored in upper 4 bits of each cell byte.
    /// </summary>
    [System.Flags]
    public enum GridMazeFlags : byte
    {
        None = 0,
        HasTorch = 1 << 4,    // 0001 0000
        HasChest = 2 << 4,    // 0010 0000
        HasEnemy = 3 << 4,    // 0011 0000
        IsLit = 4 << 4,       // 0100 0000
        IsLocked = 5 << 4,    // 0101 0000
    }
}
