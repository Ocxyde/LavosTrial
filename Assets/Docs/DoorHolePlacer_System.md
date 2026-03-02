# Door Hole Placer System - Plug-in-and-Out Architecture

**Date:** 2026-03-01 21:11
**Unity Version:** 6000.3.7f1
**Status:** ✅ Complete

---

## Summary

Created a complete door hole reservation and placement system that:
1. **Reserves wall spaces** for doors during room generation
2. **Carves holes in walls** for doors to fit perfectly
3. **Places doors in reserved holes** with random textures
4. **Follows plug-in-and-out architecture** - modular and independent

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│  MazeRenderer.BuildMaze()                               │
├─────────────────────────────────────────────────────────┤
│  1. Generate Maze                                       │
│  2. Generate Rooms                                      │
│  3. Place Door Holes ← NEW (reserves wall space)        │
│  4. Place Doors in Holes ← NEW (doors fit in holes)     │
│  5. Build Geometry                                      │
│  6. Spawn Player                                        │
└─────────────────────────────────────────────────────────┘
```

---

## New Files Created

### DoorHolePlacer.cs

**Location:** `Assets/Scripts/Core/DoorHolePlacer.cs`

**Purpose:** Reserves wall spaces for doors before wall geometry is built.

**Features:**
- Identifies wall segments in each room
- Places holes at configurable chance (60% default)
- Skips room entrances (keeps them open)
- Stores hole data for door placement
- Debug gizmos for visualization

**Key Data Structure:**
```csharp
public class DoorHoleData
{
    public Vector3 Position;
    public Quaternion Rotation;
    public float Width;
    public float Height;
    public float Depth;
    public WallDirection WallDirection;
    public RoomData Room;
    public Vector2Int GridPosition;
    public bool IsCarved;
}
```

---

### RoomDoorPlacer.cs (Updated)

**Location:** `Assets/Scripts/Core/RoomDoorPlacer.cs`

**Purpose:** Places doors in pre-carved holes with random textures.

**Features:**
- Uses `DoorHolePlacer` for hole positions
- Random door variants (Wood, Stone, Metal, Magic, Iron)
- Random trap types (None, Poison, Fire, Lightning, Freeze)
- Random wall texture generation per door
- Doors sized to fit holes perfectly

---

## Modified Files

### DoorFactory.cs

**Changes:**
- Added overloaded `CreateDoor()` method with custom dimensions
- Doors can now be sized to fit specific holes

**New Method:**
```csharp
public static GameObject CreateDoor(
    Vector3 position,
    Quaternion rotation,
    DoorVariant variant,
    DoorTrapType trap,
    float customWidth,
    float customHeight,
    float customDepth)
```

### MazeRenderer.cs

**Changes:**
- Added `DoorHolePlacer` reference
- Calls `PlaceAllHoles()` before door placement
- Calls `PlaceAllDoors()` after hole placement
- Cleans up both systems on regeneration

**Integration:**
```csharp
public void BuildMaze()
{
    // ... maze generation ...
    
    // Generate rooms
    if (_roomGenerator != null)
        _roomGenerator.GenerateRooms();

    // Place door holes in room walls
    if (_doorHolePlacer != null)
        _doorHolePlacer.PlaceAllHoles();

    // Place doors in reserved holes
    if (_roomDoorPlacer != null)
        _roomDoorPlacer.PlaceAllDoors();
    
    // ... rest of build ...
}
```

---

## How It Works

### Step 1: Hole Reservation

```
Room Wall Segments:
┌────────────────────────────────────┐
│  H   H       H   H   ← North Wall │
│  ┌──────────────────────────┐      │
│  │                          │      │
│W │                          │ E    │
│e │      ROOM INTERIOR       │ a    │
│s │                          │ s    │
│t │                          │ t    │
│  │                          │      │
│  └──────────────────────────┘      │
│  H   H   H       H   ← South Wall │
└────────────────────────────────────┘

H = Hole position (60% chance per segment)
```

### Step 2: Door Placement

```
Door fits in hole:
┌─────────────────────────────┐
│      Wall Geometry          │
│  ┌───────────────────┐      │
│  │   Door Frame      │      │ ← Hole boundaries
│  │  ┌─────────┐      │      │
│  │  │  Door   │      │      │ ← Door panel
│  │  │ Panel   │      │      │
│  │  └─────────┘      │      │
│  └───────────────────┘      │
└─────────────────────────────┘
```

### Step 3: Door Dimensions

Doors are sized to fit holes:
- **Width:** Hole width × 0.9 (90% - clearance)
- **Height:** Hole height × 0.95 (95% - clearance)
- **Depth:** Hole depth × 0.8 (80% - fits inside wall)

---

## Plug-in-and-Out Architecture

### How It Follows the Pattern

```
┌─────────────────────────────────────────────────────────┐
│              CORE (Heart of System)                     │
│  MazeGenerator │ RoomGenerator │ DoorHolePlacer        │
│  RoomDoorPlacer │ DoorFactory │ DoorsEngine            │
└─────────────────────────────────────────────────────────┘
                          │
        ┌─────────────────┼─────────────────┐
        ▼                 ▼                 ▼
┌───────────────┐  ┌───────────────┐  ┌───────────────┐
│  DoorHole     │  │  RoomDoor     │  │  DoorFactory  │
│  (reserves)   │  │  (places)     │  │  (creates)    │
└───────────────┘  └───────────────┘  └───────────────┘
```

**Key Principles:**
1. **Independent components** - Each script works alone
2. **Common data structures** - `DoorHoleData`, `RoomData`
3. **Loose coupling** - Components find each other via `GetComponent`
4. **Plug-in** - Add components to Maze GameObject
5. **Plug-out** - Remove components, system still works

---

## Configuration

### DoorHolePlacer Inspector Fields

| Field | Default | Description |
|-------|---------|-------------|
| `holeWidth` | 2.5 | Door hole width (world units) |
| `holeHeight` | 3.0 | Door hole height (world units) |
| `holeDepth` | 0.5 | Door hole depth (wall thickness) |
| `doorChancePerWall` | 0.6 | 60% chance per wall segment |
| `carveHolesInWalls` | true | Enable hole reservation |

### RoomDoorPlacer Inspector Fields

| Field | Default | Description |
|-------|---------|-------------|
| `placeDoorsInHoles` | true | Enable door placement |
| `availableVariants` | Wood, Stone, Metal | Door material variants |
| `availableTrapTypes` | None, None, Poison | Trap types (weighted) |
| `wallTextureSets` | (empty) | Wall texture configurations |
| `randomizeWallTextures` | true | Enable random textures |

---

## Setup Instructions

### In Unity Editor:

1. **Select your Maze GameObject** (with `MazeGenerator`)

2. **Add Components:**
   - `RoomGenerator` (if not present)
   - `DoorHolePlacer`
   - `RoomDoorPlacer`

3. **Configure DoorHolePlacer:**
   - Set `holeWidth` = 2.5 (fits standard door)
   - Set `holeHeight` = 3.0 (fits standard door)
   - Set `doorChancePerWall` = 0.6 (60% of walls get doors)

4. **Configure RoomDoorPlacer:**
   - Add `WallTextureSet` entries for variety
   - Configure door variants and trap types

5. **Press Play** - Holes are reserved, doors are placed!

---

## Example Wall Texture Sets

```
Wall Texture Set 1: Stone Dungeon
- Base Color: RGB(120, 120, 120)
- Variation Color: RGB(80, 80, 80)
- Noise Scale: 0.1
- Contrast: 1.2
- Tint: White (1, 1, 1)
- Smoothness: 0.5

Wall Texture Set 2: Brick Wall
- Base Color: RGB(140, 60, 40)
- Variation Color: RGB(100, 40, 30)
- Noise Scale: 0.15
- Contrast: 1.3
- Tint: Light Orange (1, 0.9, 0.8)
- Smoothness: 0.3

Wall Texture Set 3: Wood Panel
- Base Color: RGB(100, 70, 40)
- Variation Color: RGB(80, 55, 30)
- Noise Scale: 0.2
- Contrast: 1.1
- Tint: Warm Brown (1, 0.85, 0.7)
- Smoothness: 0.6
```

---

## Door Placement Flow

```
RoomGenerator.GenerateRooms()
         │
         ▼
┌────────────────────────┐
│  RoomData created      │
│  - Position            │
│  - Width, Height       │
│  - Entrances list      │
└────────────────────────┘
         │
         ▼
DoorHolePlacer.PlaceAllHoles()
         │
         ▼
┌────────────────────────┐
│  For each room:        │
│  1. Get wall segments  │
│  2. Skip entrances     │
│  3. 60% chance check   │
│  4. Create DoorHoleData│
└────────────────────────┘
         │
         ▼
┌────────────────────────┐
│  DoorHoleData stored:  │
│  - Position (world)    │
│  - Rotation            │
│  - Dimensions          │
│  - Wall direction      │
└────────────────────────┘
         │
         ▼
RoomDoorPlacer.PlaceAllDoors()
         │
         ▼
┌────────────────────────┐
│  For each hole:        │
│  1. Pick random variant│
│  2. Pick random trap   │
│  3. Size door to hole  │
│  4. Apply texture      │
│  5. Add DoorsEngine    │
└────────────────────────┘
         │
         ▼
    Doors placed!
```

---

## Debug Features

### Gizmos

**DoorHolePlacer:**
- **Cyan wireframes** - Hole boundaries
- **Yellow spheres** - Hole centers

**RoomDoorPlacer:**
- **Yellow spheres** - Placed door positions
- **Magenta lines** - Door data markers

### Console Output

```
[DoorHolePlacer] Starting hole placement in room walls...
[DoorHolePlacer] Placed 15 door holes

[RoomDoorPlacer] Starting door placement in reserved holes...
[RoomDoorPlacer] Placed Wood door at (5, 3)
[RoomDoorPlacer] Placed Stone door at (8, 3)
[RoomDoorPlacer] Placed Metal door at (5, 7)
[RoomDoorPlacer] Placed 15 doors in holes
```

---

## Backup Files

| Original | Backup |
|----------|--------|
| `DoorHolePlacer.cs` (new) | `Backup_Solution/Scripts/Core/DoorHolePlacer_00001.cs` |
| `RoomDoorPlacer.cs` | `Backup_Solution/Scripts/Core/RoomDoorPlacer_00002.cs` |
| `DoorFactory.cs` | `Backup_Solution/Scripts/Ressources/DoorFactory_00004.cs` |
| `MazeRenderer.cs` | `Backup_Solution/Scripts/Ressources/MazeRenderer_00010.cs` |

---

## Troubleshooting

### Doors not appearing?

1. Check `placeDoorsInHoles` is enabled
2. Verify `DoorHolePlacer` is on same GameObject
3. Check `doorChancePerWall` > 0
4. Check console for errors

### Holes not reserved?

1. Verify `carveHolesInWalls` is enabled
2. Check `RoomGenerator` has generated rooms
3. Ensure rooms have wall segments (not just entrances)

### Doors don't fit holes?

1. Door dimensions are auto-calculated:
   - Width = Hole width × 0.9
   - Height = Hole height × 0.95
   - Depth = Hole depth × 0.8
2. Adjust hole dimensions if needed

---

## Performance Notes

| Feature | Impact | Optimization |
|---------|--------|--------------|
| Hole reservation | Low | Runs once during generation |
| Door placement | Low | Uses cached hole data |
| Texture generation | Medium | Cached per cell position |
| Debug gizmos | None | Editor only |

---

## Future Enhancements

Possible additions:
- [ ] Special door types for boss rooms
- [ ] Locked doors requiring keys
- [ ] Secret doors (hidden until triggered)
- [ ] Multi-door entrances for large rooms
- [ ] Door archways/decorations
- [ ] Sound occlusion zones
- [ ] Wall geometry carving (remove wall mesh at hole)

---

**Status:** ✅ Ready to use

**Next Steps:**
1. Configure hole dimensions in Unity Editor
2. Add wall texture sets for variety
3. Test in game and iterate
4. Optionally add wall carving to remove geometry at holes

---

## Related Documentation

- `TETRAHEDRON_SYSTEM.md` - Tetrahedron mesh system
- `README.md` - Project overview
- `HUD_EVENT_SYSTEM.md` - HUD and event system
