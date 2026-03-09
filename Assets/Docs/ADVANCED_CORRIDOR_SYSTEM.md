# Advanced Corridor System Documentation
**Date:** 2026-03-09  
**Author:** Ocxyde  
**License:** GPL v3  
**Project:** CodeDotLavos (BetsyBoop)

---

## Overview

The Advanced Corridor System enhances the pathway from Entrance (A) to Exit (B) with varied corridor widths, decorative elements, and landmark features. This system is part of the `PassageFirstMazeGenerator` class.

---

## Features

### 1. Corridor Width Variations

Corridors along the main A->B pathway now have dynamic widths:

- **Base Width:** 1-2 cells (configurable via `SpawnRoomSize`)
- **Wide Sections:** 2-3 cells at landmarks and junctions
- **Natural Variation:** Random width changes for organic feel

```csharp
// Width varies naturally along the path
int currentWidth = GetCorridorWidthAtStep(step, baseWidth);
```

### 2. Landmark Features

Landmarks are placed at regular intervals along the main passage:

#### Plazas (3x3 Open Areas)
- Small open spaces for encounters or treasures
- Created every ~1/8th of the maze length
- Provides visual breathing room

#### Junctions (Cross-Shaped)
- Four-directional intersections
- Creates decision points for players
- Marked with pillar decorations

#### Gates (Narrowed Passages)
- Choke points with pillar markers
- Visual transition between maze sections
- Strategic defensive positions

### 3. Corridor Decorations

#### Pillars
- Placed at landmark corners (gates, junctions, plazas)
- Marked with `CellFlags8.HasPillar` flag
- Instantiated by `MazePlacementEngine` as decorative columns

#### Wall Niches
- Small alcoves carved into corridor walls
- 1 in 15 chance per corridor cell
- Can hold statues, torches, or hidden treasures
- Marked with `CellFlags8.HasNiche` flag

#### Arches
- Placed at corridor transitions
- Every ~1/6th of maze width
- Marks significant passage points
- Uses `CellFlags8.HasTorch` flag (interpreted by placement engine)

### 4. Curves and Bulges

- 15% chance for diagonal bulge at each step
- Creates organic, non-linear corridors
- Adds visual interest and complexity

---

## Generation Pipeline

The enhanced `PassageFirstMazeGenerator` follows this pipeline:

```
PHASE 1: Fill All Walls
  └─> Initialize grid with solid walls

PHASE 2: Carve Main Passage A->B with Variations
  └─> Width variations
  └─> Landmark placement (plazas, junctions, gates)
  └─> Curve additions (diagonal bulges)

PHASE 3: Carve Branch Passages
  └─> Secondary corridors from main path
  └─> Winding factor controls density

PHASE 4: Expand Chambers
  └─> Dead-end expansion into rooms
  └─> Room cell flagging

PHASE 5: Add Corridor Decorations
  └─> Pillars at landmarks
  └─> Wall niches along corridors
  └─> Arches at transitions

PHASE 6: Place Objects
  └─> Torches (skip decoration cells)
  └─> Enemies (in dead-end chambers)
  └─> Chests (in dead-end chambers)

PHASE 7: Set Spawn/Exit
  └─> Mark entrance (A) at (1, 1)
  └─> Mark exit (B) at (size-2, size-2)
```

---

## Configuration

### DungeonMazeConfig Parameters

```csharp
// Corridor settings
public int SpawnRoomSize = 2;          // Base passage width
public float CorridorWindingFactor = 0.3f;  // Branch density

// Decoration settings (implicit)
// - Landmark interval: maze width / 8
// - Niche chance: 1 in 15
// - Arch interval: maze width / 6
// - Curve chance: 15%
```

### Enabling Passage-First Mode

In `DungeonMazeConfig`:

```csharp
public bool UsePassageFirst = true;  // Enable advanced corridor system
```

---

## Cell Flags

New flags added to `CellFlags8` enum:

| Flag | Bit | Value | Description |
|------|-----|-------|-------------|
| `HasPillar` | 14 | 0x4000 | Pillar decoration at landmark |
| `HasNiche` | 15 | 0x8000 | Wall niche/alcove for decorations |

**Note:** Arch markers use `HasTorch` flag (0x0800) and are interpreted by `MazePlacementEngine` based on context.

---

## Integration with MazePlacementEngine

The `MazePlacementEngine` reads decoration flags and instantiates appropriate prefabs:

```csharp
// Pseudocode for MazePlacementEngine integration
if (cell.HasPillar)
{
    Instantiate(pillarPrefab, position, rotation);
}

if (cell.HasNiche)
{
    Instantiate(nicheStatuePrefab, position, rotation);
    // Or place treasure/torch in niche
}

if (cell.HasTorch && IsArchContext(cell))
{
    Instantiate(archPrefab, position, rotation);
}
else if (cell.HasTorch)
{
    Instantiate(torchPrefab, position, rotation);
}
```

---

## Visual Layout Example

```
A = Entrance (1,1)          J = Junction
B = Exit (size-2, size-2)   P = Plaza
# = Pillar                  ~ = Curve/Bulge
. = Corridor              * = Niche
= = Arch

    A . . ~ . . J ~ . . P ~ . . J ~ . . B
        *   #   *       *   #   *   #
            =               =
```

---

## Performance Considerations

- **Memory:** Decoration data stored in `DungeonMazeData` (RAM)
- **Generation Time:** +5-10% overhead for decoration placement
- **Runtime:** Minimal - flags checked during instantiation only

---

## Future Enhancements

1. **Dynamic Lighting:** Torches in niches, spotlights on pillars
2. **Sound Zones:** Echo effects in plazas, dampened sound in narrow corridors
3. **Trap Integration:** Spike traps in narrow passages, falling rocks in plazas
4. **Environmental Storytelling:** Banners on pillars, murals in niches
5. **Procedural Decoration Prefabs:** Variety packs for pillars, arches, statues

---

## Files Modified

| File | Changes |
|------|---------|
| `PassageFirstMazeGenerator.cs` | Added corridor variations, landmarks, decorations |
| `MazeData8.cs` | Added `HasPillar`, `HasNiche` flags |

---

## Testing

To test the new corridor system:

1. Enable `UsePassageFirst = true` in `DungeonMazeConfig`
2. Generate maze via Tools > Generate Maze
3. Check console for decoration counts:
   ```
   [PassageFirst] Plaza added at (x,z)
   [PassageFirst] Junction added at (x,z)
   [PassageFirst] Gate added at (x,z)
   [PassageFirst] Pillars added at N positions
   [PassageFirst] Wall niches added: N
   [PassageFirst] Arches added: N
   ```

---

## Git Commit Message

```
feat: Advanced corridor system with landmarks and decorations

- Add corridor width variations along A->B pathway
- Implement landmark features (plazas, junctions, gates)
- Add corridor decorations (pillars, wall niches, arches)
- Add CellFlags8.HasPillar and HasNiche flags
- Enhance PassageFirstMazeGenerator with 7-phase pipeline
- Update documentation

Enables rich, varied corridor experiences from entrance to exit.
```

---

## Compliance

- [x] Unix LF line endings
- [x] UTF-8 encoding
- [x] GPL v3 license headers
- [x] C# Unity naming conventions
- [x] Plug-in-out architecture (no dependencies)
- [x] Documentation in Assets/Docs/
- [ ] Tests in proper test folders (pending)

**Reminder:** Run `backup.ps1` after all changes and commit to git.
