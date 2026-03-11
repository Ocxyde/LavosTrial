# Maze Generation - Fill & Carve Approach

**Date:** 2026-03-11
**Author:** Ocxyde (User Idea)
**Status:** 📝 Proposed

---

## 💡 Core Concept

**"Fill the whole maze with walls, then carve the longest way through to the exit. Then replenish with furnish and rooms."**

**PLUS:** Carve door openings directly into walls during generation.

---

## Algorithm Overview

### Phase 1: Fill All Walls
```
Start with a 100% solid grid
Every cell = Wall
No paths, no corridors, no rooms
Just solid rock
```

### Phase 2: Carve Main Path
```
Find the LONGEST possible path from spawn to exit
Use A* or DFS algorithm
Carve this path through the solid walls
This becomes the main corridor
Result: Guaranteed path from start → finish
```

### Phase 3: Add Branches + Carve Doors
```
Carve secondary corridors from main path
Create dead-ends for exploration
Add loops and alternative routes

DOOR CARVING:
- Mark wall cells where corridors intersect
- Carve door openings (full height: floor to ceiling)
- Cut through entire wall thickness
- Position at center of wall segments
- Save door positions for prefab spawning later
```

### Phase 4: Expand Rooms
```
Select certain corridor intersections
Expand into open rooms (3x3, 5x5, etc.)
Rooms feel like "clearings in solid rock"
Place special content (guards, treasures)

DOOR CARVING (Rooms):
- Carve door openings at room entrances
- Full wall height openings
- Frame with door prefab later
```

### Phase 5: Furnish
```
Place torches on walls (lighting)
Spawn chests in rooms/dead-ends (loot)
Place enemies (density = difficulty)
Add traps, decorations, atmosphere
Spawn door prefabs in carved openings
```

---

## Door Carving Details

### Door Opening Specifications:
```
Width:  1.0 - 1.5 meters (fits standard door prefab)
Height: Full wall height (3m, floor to ceiling)
Depth:  Full wall thickness (0.3m)
Position: Centered on wall segment
```

### Door Carving Process:
```
1. Identify wall cells between two walkable cells
2. Mark center of wall for door opening
3. Remove wall geometry at door position
4. Store door position + rotation
5. Spawn door prefab in opening (later phase)
```

### Visual Example - Door Carving:
```
BEFORE (Solid Wall):          AFTER (Door Carved):
┌───────────────────┐         ┌───────────────────┐
│███████│███████████│         │███████│███████████│
│       │           │         │       │           │
│       │           │         │       │           │
│███████│███████████│         │███████░░░█████████│ ← Door opening
│       │           │         │       │           │    (carved out)
│       │           │         │       │           │
│███████│███████████│         │███████│███████████│
└───────────────────┘         └───────────────────┘

Legend:
█ = Wall
░ = Door opening (carved)
  = Corridor (walkable)
```

---

## Full Visual Example

```
PHASE 1: Fill All Walls          PHASE 2: Carve Main Path
┌─────────────────────┐          ┌─────────────────────┐
│█████████████████████│          │█████████████████████│
│█████████████████████│          │█████████████████████│
│█████████████████████│          │█    ████████████████│
│█████████████████████│   →      │█    ████████████████│
│█████████████████████│          │█    ████████████████│
│█████████████████████│          │███████████    ██████│
│█████████████████████│          │███████████    ██████│
│█████████████████████│          │███████████    ██████│
│█████████████████████│          │███████████    ██████│
│█████████████████████│          │███████████    ██████│
│█████████████████████│          │███████████    ██████│
│█████████████████████│          │███████████    ██████│
│█████████████████████│          │███████████    ██████│
│█████████████████████│          │███████████    ██████│
│█████████████████████│          │███████████    ██████│
│█████████████████████│          │███████████    ██████│
│█████████████████████│          │███████████    ██████│
│█████████████████████│          │███████████    ██████│
│█████████████████████│          │███████████    ██████│
│█████████████████████│          │███████████    ██████│
│█████████████████████│          │███████████    ██████│
└─────────────────────┘          └─────────────────────┘
  All solid walls                    Main path carved


PHASE 3: Branches + Doors       PHASE 4: Expand Rooms
┌─────────────────────┐          ┌─────────────────────┐
│█████████████████████│          │█████████████████████│
│█████████████████████│          │█████████████████████│
│█░░░█████████████████│          │█░░░█████████████████│
│█    ████████████████│          │█    ████████████████│
│█  ██████████████████│          │█  ██████████████████│
│███████████░░░███████│          │███████████░░░███████│
│███████████    ██████│          │███████████    ██████│
│███████████░░░███████│          │███████████    ██████│
│███████████    ██████│          │███████████    ██████│
│███████████░░░███████│          │███████████    ██████│
│███████████    ██████│          │███████████    ██████│
│███████████░░░███████│          │███████████    ██████│
│███████████    ██████│          │███████████    ██████│
│███████████░░░███████│          │███████████    ██████│
│███████████    ██████│          │███████████    ██████│
│███████████░░░███████│          │███████████    ██████│
│███████████    ██████│          │███████████    ██████│
│███████████░░░███████│          │███████████    ██████│
│███████████    ██████│          │███████████    ██████│
│███████████░░░███████│          │███████████    ██████│
│███████████    ██████│          │███████████    ██████│
└─────────────────────┘          └─────────────────────┘
  Branches carved,                 Rooms expanded
  doors carved (░)


PHASE 5: Furnish (Final Result)
┌─────────────────────┐
│█████████████████████│
│█████████████████████│
│█🔥 [D]🔥█████████████│
│█    ████████████████│
│█  📦████████████████│
│███████████[D]░░█████│
│███████████ 🔥  ██████│
│███████████[D]░░█████│
│███████████  👹 ██████│
│███████████[D]░░█████│
│███████████    ██████│
│███████████[D]░░█████│
│███████████    ██████│
│███████████[D]░░█████│
│███████████    ██████│
│███████████[D]░░█████│
│███████████    ██████│
│███████████[D]░░█████│
│███████████    ██████│
│███████████[D]░░█████│
│███████████    ██████│
└─────────────────────┘
  🔥 = Torches
  📦 = Chest
  👹 = Enemy/Guardian
  [D] = Door (prefab spawned in carved opening)
  ░ = Carved opening
```

---

## Benefits

| Benefit | Description |
|---------|-------------|
| ✅ **Guaranteed Path** | Always solvable - main path carved first |
| ✅ **Organic Feel** | Like natural caves carved in solid rock |
| ✅ **Room Impact** | Rooms feel special (clearings in darkness) |
| ✅ **Doors Integrated** | Carved during generation, prefabs spawned after |
| ✅ **Difficulty Control** | Adjust path length, branch density, room count |
| ✅ **Performance** | Start solid, only carve what's needed |
| ✅ **Flexibility** | Easy to add themes, variations, biomes |

---

## Implementation Plan

| Step | Task | Files | Time |
|------|------|-------|------|
| 1 | Fill grid 100% walls | `GridMazeGenerator.cs` | 30min |
| 2 | Carve spawn→exit path | `PathFinder.cs` + `GridMazeGenerator` | 1h |
| 3 | Add branches + carve doors | `GridMazeGenerator.cs` | 1.5h |
| 4 | Expand rooms + door frames | `RoomExpander.cs` (new) | 1-2h |
| 5 | Furnish + spawn doors | `MazeObjectSpawner.cs` | 30min |

**Total Estimated Time:** 4 - 5 hours

---

## Code Structure (Proposed)

```csharp
public class FillAndCarveMazeGenerator
{
    // Phase 1: Fill all walls
    void FillAllWalls() { }
    
    // Phase 2: Carve main path
    void CarveMainPath() { }
    
    // Phase 3: Add branches + carve doors
    void CarveBranches() { }
    void CarveDoorOpenings() { }
    
    // Phase 4: Expand rooms
    void ExpandRooms() { }
    
    // Phase 5: Furnish
    void FurnishMaze() { }
    void SpawnDoorPrefabs() { }
    
    // Main generation pipeline
    public void Generate()
    {
        FillAllWalls();
        CarveMainPath();
        CarveBranches();
        CarveDoorOpenings();
        ExpandRooms();
        FurnishMaze();
        SpawnDoorPrefabs();
    }
}
```

---

## Door Carving - Technical Details

### Method 1: Voxel Removal
```csharp
void CarveDoorOpening(int wallX, int wallZ, Direction direction)
{
    // Mark cell as "door opening" (not walkable, not solid wall)
    mazeData.SetCell(wallX, wallZ, CellFlags.DoorOpening);
    
    // Store door position for prefab spawning
    doorPositions.Add(new DoorPosition(wallX, wallZ, direction));
}
```

### Method 2: Geometry Cutting
```csharp
void CutDoorGeometry(int wallX, int wallZ, Direction direction)
{
    // Find wall segment at this position
    // Remove geometry where door should be
    // Leave frame for door prefab
}
```

### Door Data Structure:
```csharp
public struct DoorPosition
{
    public int x, z;           // Grid position
    public Direction facing;   // Which way door faces
    public DoorType type;      // Normal, Locked, Secret
}
```

---

## Next Steps

1. **Review and approve** this design (with door carving)
2. **Create implementation task** in TODO.md
3. **Start with Phase 1** (Fill all walls)
4. **Test each phase** before moving to next

---

**License:** GPL-3.0
**Copyright © 2026 CodeDotLavos. All rights reserved.**

---

*Last Updated: 2026-03-11 | Unity 6000.3.10f1 | UTF-8 encoding - Unix LF*

**Motto:** "Happy coding with me : Ocxyde :)"
