# FIX - Corridor / Door / Wall Pivot
> Date: 2026-03-07
> Author: BetsyBoop / CodeDotLavos
> Files changed: MazeData8.cs, GridMazeGenerator8.cs, CompleteMazeBuilder8.cs
> Diffs stored in: diff_tmp/

---

## Problems fixed

### 1. Corridors did not lead to rooms

**Root cause:** The DFS carved single-cell-wide passages with no concept of
a room at dead-ends or junctions. The maze looked like a grid of corridors
with no destination spaces.

**Fix:** `CarveRooms()` added to `GridMazeGenerator8` (step 3.5).
After the DFS, every cell with exactly 1 open cardinal neighbour (dead end)
or 3+ open cardinal neighbours (crossroads / junction) is expanded into a
room of configurable radius (`RoomRadius` in `MazeConfig8`, default = 1 -> 3x3).
Each room cell receives the `IsRoom` flag so the builder can later query it.

### 2. Doors were floating at cell center, unoriented

**Root cause:** `SpawnDoors()` called `PlaceAtCell()` which places objects
at `CellCenter(cx, cz)` - the geometric center of the cell, with no rotation.

**Fix:** `SpawnDoorOnAccessWall()` replaces `PlaceAtCell()` for doors.
It scans cardinal directions of the spawn/exit cell, finds the first open
passage (no wall flag), and places the door prefab:
- ON the wall edge (same edge-center formula as `SpawnCardinalWall`)
- Rotated to face the corridor (N/S -> 0 deg, E/W -> 90 deg Y)
- With the same Y-pivot correction as walls

### 3. Wall pivot was not centered on cell edge

**Root cause:** Wall position was computed as `CellCenter + offset` where
`CellCenter` was already at `y = wallHeight * 0.5`, then the wall was scaled
to height `wh` - causing the wall to extend above and below its intended edge.

**Fix:** `SpawnCardinalWall()` and `SpawnDiagonalWall()` now compute position
as the **bottom-center of the wall edge at y = 0**:

```
edgePos.x = (cx + 0.5 + dx * 0.5) * cellSize
edgePos.y = 0
edgePos.z = (cz + 0.5 + dz * 0.5) * cellSize
```

If `wallPivotIsAtMeshCenter = true` (Inspector toggle, default on for the
standard Unity cube primitive), the position is then shifted up by `wh * 0.5`
so the wall stands correctly on the floor.
Set `wallPivotIsAtMeshCenter = false` if your custom WallPrefab has its pivot
already at the bottom edge of the mesh.

---

## New fields

### MazeConfig8
| Field | Type | Default | Description |
|---|---|---|---|
| `RoomRadius` | int | 1 | Room size at dead-ends / crossroads. 1 = 3x3, 2 = 5x5 |

### CompleteMazeBuilder8 Inspector
| Field | Type | Default | Description |
|---|---|---|---|
| `wallPivotIsAtMeshCenter` | bool | true | Enables Y offset for Unity primitive cube prefabs |

### CellFlags8
| Flag | Bit | Value | Description |
|---|---|---|---|
| `IsRoom` | 13 | 0x2000 | Set on cells carved as rooms by `CarveRooms()` |

---

## Binary format version bump

Version 2 -> 3 (LAV8S v3).
Purge `Runtimes/Mazes/*.lvm` before testing - old files are incompatible
because `IsRoom` is a new flag bit that was not present in v2 data.

---

## Checklist before applying

1. Run backup.ps1
2. git commit -am "pre-fix: before corridor-door-wall-pivot"
3. Apply the three files
4. Delete Runtimes/Mazes/*.lvm (stale cache)
5. In Unity: verify wallPivotIsAtMeshCenter matches your WallPrefab setup
6. Play test: dead-end corridors should now open into 3x3 rooms,
   doors appear on wall edges facing the corridor
