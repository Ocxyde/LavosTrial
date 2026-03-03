# Torch Plug-in System
**Date:** 2026-03-02  
**Unity Version:** 6000.3.7f1 (URP Standard)  
**Architecture:** Plug-in-and-Out

---

## Overview

The torch system has been refactored to follow the **plug-in-and-out** architecture. Torches are now **independently placed** into the maze using `SpatialPlacer`, rather than being automatically generated with it.

### Key Principles

1. **Maze does NOT generate torches** - The maze geometry is built without torches
2. **Torches are plugged in via SpatialPlacer** - Use `SpatialPlacer.PlaceTorches()` to add torches independently
3. **Optional placement** - Torches can be enabled/disabled per generation
4. **Re-pluggable** - Torches can be removed and regenerated without rebuilding the maze

---

## Components

### 1. `SpatialPlacer.cs` (MODIFIED)
**Location:** `Assets/Scripts/Core/08_Environment/SpatialPlacer.cs`

**Purpose:** Universal placement system for all maze objects (including torches)

**Features:**
- Places torches on maze walls after generation
- Configurable probability and spacing
- Supports both BraseroFlame and sprite-based flames
- Context menu commands for editor use
- Also handles: special rooms, chests, enemies, items

**Usage:**
```csharp
// Get reference
var spatialPlacer = FindObjectOfType<SpatialPlacer>();

// Place torches
spatialPlacer.PlaceTorches();

// Remove torches
spatialPlacer.RemoveTorches();

// Regenerate with new distribution
spatialPlacer.RegenerateTorches();
```

**Inspector Settings:**
- `Place Torches` (toggle): Enable/disable torch placement
- `Torch Pool` (reference): Assign TorchPool component
- `Torch Count`: Target number of torches
- `Torch Height Ratio`: Vertical position on wall (0-1)
- `Torch Inset`: Distance from wall face
- `Min Distance Between Torches`: Minimum spacing

---

### 2. `MazeRenderer.cs` (MODIFIED)
**Location:** `Assets/Scripts/Core/06_Maze/MazeRenderer.cs`

**Changes:**
- `BuildGeometry()` NO LONGER places torches automatically
- Wall faces are cached in `_cachedWallFaces` for later use
- `SpatialPlacer` accesses wall faces via reflection (encapsulation)

---

### 3. `MazeIntegration.cs` (MODIFIED)
**Location:** `Assets/Scripts/Core/06_Maze/MazeIntegration.cs`

**Changes:**
- Added `placeTorches` toggle (default: `false`)
- Added `SpatialPlacer` component reference
- Step 6 in generation pipeline: places torches via SpatialPlacer

**Generation Pipeline:**
```
1. Generate Maze (grid)
2. Generate Rooms
3. Place Door Holes
4. Place Doors
5. Build Geometry
6. Place Torches via SpatialPlacer (OPTIONAL) ← Uses SpatialPlacer
```

---

## Setup Instructions

### Option 1: Using MazeIntegration (Recommended)

1. **Add SpatialPlacer component** to your Maze GameObject:
   ```
   Select Maze GameObject → Add Component → SpatialPlacer
   ```

2. **Configure SpatialPlacer settings:**
   - Place Torches: `✓` (enable)
   - Torch Pool: Assign TorchPool component (auto-find on same GameObject)
   - Torch Count: `15` (target number)
   - Torch Height Ratio: `0.55`
   - Min Distance: `6` (matches cell size)

3. **Enable torch placement in MazeIntegration:**
   ```
   MazeIntegration → Place Torches: ✓ (check to enable)
   ```

4. **Generate maze:**
   - Torches will be placed automatically if `Place Torches` is enabled
   - Or call `spatialPlacer.PlaceTorches()` manually

---

### Option 2: Manual Control

1. **Add SpatialPlacer component** to your Maze GameObject

2. **Configure Torch settings in SpatialPlacer:**
   - Place Torches: `✓`
   - Torch Pool: Assign TorchPool component

3. **Generate maze first:**
   ```csharp
   var mazeRenderer = GetComponent<MazeRenderer>();
   mazeRenderer.BuildMaze();
   ```

4. **Place torches when ready:**
   ```csharp
   var spatialPlacer = GetComponent<SpatialPlacer>();
   spatialPlacer.PlaceTorches();
   ```

5. **Remove torches (optional):**
   ```csharp
   spatialPlacer.RemoveTorches();
   ```

---

## Editor Commands

### SpatialPlacer Context Menu

Right-click the `SpatialPlacer` component in the Inspector:

- **Place Torches** - Place torches on maze walls
- **Remove Torches** - Remove all placed torches
- **Regenerate Torches** - Remove and re-place with new distribution

---

## Configuration Examples

### Example 1: Dense Torch Placement
```
Torch Probability: 0.8
Min Distance: 4
Result: Many torches, close together
```

### Example 2: Sparse Torch Placement
```
Torch Probability: 0.3
Min Distance: 12
Result: Few torches, well spaced
```

### Example 3: No Torches (Default)
```
Place Torches: false (in MazeIntegration)
Result: Maze generates without torches
```

---

## Code Examples

### Place Torches After Maze Generation
```csharp
public class MazeController : MonoBehaviour
{
    [SerializeField] private MazeIntegration mazeIntegration;
    [SerializeField] private SpatialPlacer spatialPlacer;

    public void GenerateMazeWithTorches()
    {
        // Generate the maze
        mazeIntegration.GenerateMaze();

        // Place torches independently
        spatialPlacer.PlaceTorches();
    }
}
```

### Toggle Torches On/Off
```csharp
public class TorchToggle : MonoBehaviour
{
    private SpatialPlacer spatialPlacer;
    private bool torchesActive = false;

    void Start()
    {
        spatialPlacer = FindObjectOfType<SpatialPlacer>();
    }

    public void ToggleTorches()
    {
        if (torchesActive)
        {
            spatialPlacer.RemoveTorches();
            torchesActive = false;
        }
        else
        {
            spatialPlacer.PlaceTorches();
            torchesActive = true;
        }
    }
}
```

### Regenerate Torches with New Distribution
```csharp
public void RegenerateTorchesWithNewDistribution()
{
    var spatialPlacer = FindObjectOfType<SpatialPlacer>();
    spatialPlacer.RegenerateTorches();
    Debug.Log($"New torch distribution generated");
}
```

---

## Troubleshooting

### No Torches Appear

**Check:**
1. ✅ `Place Torches` is enabled in SpatialPlacer
2. ✅ `Torch Pool` reference is assigned in SpatialPlacer
3. ✅ Maze has been built (`BuildMaze()` called)
4. ✅ `SpatialPlacer` component is on the same GameObject as `MazeRenderer` and `TorchPool`
5. ✅ `DrawingPool.Instance` exists and has flame textures
6. ✅ `PlaceTorches()` was called after maze generation

### Too Many Torches

**Solution:**
- Decrease `Torch Count`
- Increase `Min Distance Between Torches`

### Too Few Torches

**Solution:**
- Increase `Torch Count`
- Decrease `Min Distance Between Torches`

### Error: "No wall faces found"

**Cause:** `PlaceTorches()` called before `BuildMaze()`

**Solution:** Always build maze first:
```csharp
mazeRenderer.BuildMaze();  // First
spatialPlacer.PlaceTorches();  // Then
```

### Error: "TorchPool reference not assigned"

**Cause:** Missing reference in SpatialPlacer inspector

**Solution:** Drag the GameObject with `TorchPool` component to the `Torch Pool` field in SpatialPlacer inspector

---

## Performance Notes

- **TorchPool** uses object pooling - torches are reused, not destroyed
- **Placement** is O(n) where n = wall face count
- **Recommended max torches:** 200-300 for mobile, 500+ for PC

---

## File Changes Summary

| File | Status | Change |
|------|--------|--------|
| `SpatialPlacer.cs` | MODIFIED | Added torch placement methods |
| `MazeRenderer.cs` | MODIFIED | Decoupled torch placement from geometry, cached wall faces |
| `MazeIntegration.cs` | MODIFIED | Uses SpatialPlacer for torch placement |

---

## See Also

- [`SpatialPlacer.cs`](../Scripts/Core/08_Environment/SpatialPlacer.cs) - Universal placement system
- [`TorchPool.cs`](../Scripts/Core/10_Resources/TorchPool.cs) - Object pooling for torches
- [`TorchController.cs`](../Scripts/Core/10_Resources/TorchController.cs) - Individual torch behavior
- [`MazeRenderer.cs`](../Scripts/Core/06_Maze/MazeRenderer.cs) - Maze geometry rendering
- [`MazeIntegration.cs`](../Scripts/Core/06_Maze/MazeIntegration.cs) - Generation orchestration

---

**Remember:** Run `backup.ps1` after any file changes!
