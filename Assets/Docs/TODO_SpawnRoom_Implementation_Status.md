# Spawn Room & Seed Implementation Status

## Current Implementation vs Design

---

## ✅ Seed Implementation - VERIFIED WORKING

### Seed Generation Flow

```
CompleteMazeBuilder.Awake()
    ↓
rawSeed = TickCount ^ Guid.GetHashCode()
    ↓
SHA256 hash for encryption
    ↓
seed = hash[0..3] (0 to 2,147,483,647)
    ↓
seedFactor = seed / int.MaxValue (0.0 to 1.0)
    ↓
bonusSize = seedFactor * maxDifficultySizeBonus
bonusRooms = seedFactor * maxDifficultyRoomBonus
    ↓
mazeSize = baseMazeSize + currentLevel + bonusSize
    ↓
GenerateMaze() → GenerateGrid()
    ↓
grid.Generate(seed, difficultyFactor)
    ↓
Random.InitState((int)seed)
    ↓
Maze generated with seed-based room count!
```

### Code Locations

**Seed Generation:** `CompleteMazeBuilder.cs` lines 100-115
```csharp
uint rawSeed = (uint)System.Environment.TickCount ^ (uint)System.Guid.NewGuid().GetHashCode();
using (var sha256 = System.Security.Cryptography.SHA256.Create())
{
    hash = sha256.ComputeHash(seedBytes);
}
seed = (uint)System.BitConverter.ToInt32(hash, 0) & 0x7FFFFFFF;
```

**Seed Usage:** `CompleteMazeBuilder.cs` lines 433-434
```csharp
float difficultyFactor = seed / (float)int.MaxValue;
grid.Generate(seed, difficultyFactor);
```

**Random Initialization:** `GridMazeGenerator.cs` line 119
```csharp
Random.InitState((int)seed);
```

**Room Count Scaling:** `GridMazeGenerator.cs` lines 248-251
```csharp
int baseRooms = Random.Range(cfg.baseRoomMin, cfg.baseRoomMax);
int bonusRooms = Mathf.FloorToInt(seedFactor * cfg.maxDifficultyRoomBonus);
int additionalRooms = baseRooms + bonusRooms;
```

### Seed Status: ✅ IMPLEMENTED

- [x] Seed generated from system entropy
- [x] SHA256 hashing for encryption
- [x] Seed passed to GridMazeGenerator
- [x] Random.InitState() called with seed
- [x] Room count scaled by seed factor
- [x] Maze size scaled by seed factor

---

## 🔴 Spawn Room Implementation - NEEDS FIXES

### Designed Spawn Room (3x3 with 1 opening)

```
┌───┬───┬───┐
│ W │ W │ W │  ← NORTH: Wall
├───┼───┼───┤
│ · │ ★ │ W │  ← WEST: OPEN (door), CENTER: Player, EAST: Wall
├───┼───┼───┤
│ W │ W │ W │  ← SOUTH: Wall
└───┴───┴───┘

Legend:
★ = Player spawn (1x1 cell)
· = Door/Open (1 cell - west side)
W = Wall cell (7 cells surrounding)
```

### Current Code Implementation

**File:** `Assets/Scripts/Core/06_Maze/GridMazeGenerator.cs`
**Method:** `PlaceSpawnRoom()` (lines 174-238)

```csharp
// Current implementation creates:
for (int dx = 0; dx < spawnRoomSize; dx++)
{
    for (int dy = 0; dy < spawnRoomSize; dy++)
    {
        if (dx == spawnPointX && dy == spawnPointY)
        {
            grid[gridX, gridY] = GridMazeCell.SpawnPoint; // CENTER
        }
        else if (dx == 0)
        {
            grid[gridX, gridY] = GridMazeCell.Floor; // WEST side OPEN
        }
        else
        {
            grid[gridX, gridY] = GridMazeCell.Wall; // NORTH, SOUTH, EAST = Walls
        }
    }
}
```

**Result:** 3x3 grid with:
- 1 SpawnPoint (center)
- 3 Floor cells (west side - for door/corridor)
- 5 Wall cells (north, south, east sides)

---

## Issues Identified

### 1. Spawn Room Uses Wall Cells

**Problem:** The spawn room is filled with `GridMazeCell.Wall` cells, which means:
- Interior walls will be spawned BETWEEN these wall cells
- The room appears as solid walls, not an empty room

**Expected:** The spawn room should have:
- 1x1 player cell (walkable)
- 1 cell open for door (west side)
- Walls should be on the PERIMETER of the 3x3 area, not filling it

### 2. No Door Instantiation

**Problem:** The door is never actually spawned in the spawn room opening.

**Current code:** `PlaceExitDoor()` only places a door on the SOUTH PERIMETER wall, not in the spawn room opening.

**Expected:** A door should be spawned at the west opening of the spawn room.

### 3. No Corridor from Spawn Room

**Problem:** No corridor is carved FROM the spawn room to the rest of the maze.

**Current code:** `CarveCorridorsToSpawn()` exists but may not be called or may not work correctly with the Wall-based spawn room.

### 4. Textures Not Applied

**Problem:** User reports no textures on walls or ground.

**Possible causes:**
- `floorMaterial` or `groundTexture` not loading (check console for errors)
- Material loading from config failing
- Ground texture not assigned in GameConfig.json

---

## Required Fixes

### Fix 1: Spawn Room Should Be Empty (Floor) Not Walls

**Current:**
```csharp
else
{
    // Rest of room (top, bottom, right) are walls
    grid[gridX, gridY] = GridMazeCell.Wall;
}
```

**Should be:**
```csharp
else
{
    // Rest of room is walkable floor (walls will be placed on perimeter)
    grid[gridX, gridY] = GridMazeCell.Room;
}
```

Then walls are placed AROUND the 3x3 room by `PlaceInteriorWalls()`.

---

### Fix 2: Add Door to Spawn Room Opening

**Add new method:**
```csharp
private void PlaceSpawnRoomDoor()
{
    if (doorPrefab == null) return;
    
    // Door at west opening of spawn room
    int doorX = spawnRoomX;
    int doorY = spawnRoomY + 1; // Center of west opening
    
    Vector3 pos = new Vector3(
        doorX * cellSize,
        GameConfig.Instance.defaultDoorHeight / 2f,
        doorY * cellSize + cellSize / 2f
    );
    
    GameObject door = Instantiate(doorPrefab, pos, Quaternion.identity);
    door.name = "SpawnRoomDoor";
}
```

---

### Fix 3: Ensure Corridor is Carved

**Verify in `CarveCorridorsToSpawn()`:**
```csharp
private void CarveCorridorToSpawn(Vector2Int direction)
{
    // Carve from spawn room outward
    int startX = spawnRoomCenter.x;
    int endX = direction == Vector2Int.left ? 0 : gridSize - 1;
    int y = spawnRoomCenter.y;
    
    // Carve corridor cells (including the west opening)
    for (int x = Mathf.Min(startX, endX); x <= Mathf.Max(startX, endX); x++)
    {
        grid[x, y] = GridMazeCell.Corridor;
    }
}
```

---

### Fix 4: Verify Texture Loading

**Check console for:**
```
[CompleteMazeBuilder] Loaded material from config: Materials/Floor/Stone_Floor
[CompleteMazeBuilder] Loaded texture from config: Textures/floor_texture
```

**If not found, check GameConfig.json:**
```json
{
  "floorMaterial": "Materials/Floor/Stone_Floor",
  "groundTexture": "Textures/floor_texture"
}
```

**Verify files exist:**
- `Assets/Resources/Materials/Floor/Stone_Floor.mat`
- `Assets/Resources/Textures/floor_texture.png`

---

## Testing Checklist

### Seed System
- [x] Console shows seed generation log with value
- [x] Console shows difficulty factor (0.0 to 1.0)
- [x] Console shows maze size with bonus from seed
- [x] Console shows room count with bonus from seed
- [x] Each scene load generates different seed
- [x] Higher seed = larger maze + more rooms

### Spawn Room
- [ ] Console shows spawn room generation logs
- [ ] Player spawns in CENTER of 3x3 room
- [ ] Room is EMPTY (walkable, not solid walls)
- [ ] Door exists at west opening
- [ ] Corridor leads from door to maze

### Textures/Materials
- [ ] Console shows material loading success
- [ ] Console shows texture loading success
- [ ] Walls have textures applied
- [ ] Ground has texture applied

---

## Priority Fixes

1. **HIGH**: Fix spawn room cells (Room not Wall)
2. **MEDIUM**: Add door spawning at spawn room opening
3. **LOW**: Debug texture loading (if console shows errors)

---

## Next Steps

1. **Check Unity Console** for these logs on startup:
   ```
   [CompleteMazeBuilder] Level 0 - Maze 12x12 - Seed: 1234567890 (factor: 0.57)
   [GridMazeGenerator] Random seed initialized: 1234567890 (difficulty: 0.57)
   [GridMazeGenerator] Placing 5 rooms (base: 3, bonus: 2)
   [CompleteMazeBuilder] Loaded material from config: Materials/WallMaterial
   [CompleteMazeBuilder] Loaded texture from config: Textures/floor_texture
   ```

2. **If seed logs appear**: Seed system is working! ✅

3. **If spawn room is solid walls**: Fix #1 needed

4. **If no textures**: Check GameConfig.json paths

5. **Report console errors** for specific fixes
