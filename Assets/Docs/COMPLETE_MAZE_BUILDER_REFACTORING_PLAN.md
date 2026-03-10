# CompleteMazeBuilder8 Refactoring Plan

**Date:** 2026-03-10  
**Status:** Modules Created, Integration Pending  
**Goal:** Split 1141-line god class into focused, maintainable modules

---

## Current State

**CompleteMazeBuilder.cs:** 1141 lines  
**Responsibilities:**
- Config loading
- Asset validation
- Wall spawning (cardinal + diagonal)
- Door spawning
- Object spawning (torches, chests, enemies)
- Marker spawning (entrance/exit visual effects)
- Player spawning
- Binary save/load

---

## Modules Created

### 1. MazeWallSpawner.cs (~180 lines)
**Purpose:** Spawn cardinal and diagonal walls  

**Methods:**
- `SpawnCardinalWall()` - N/S/E/W walls
- `SpawnDiagonalWall()` - NE/NW/SE/SW walls

**Usage:**
```csharp
MazeWallSpawner.SpawnCardinalWall(
    cx, cz, dir,
    wallPrefab, wallMaterial,
    cellSize, wallHeight, wallThickness,
    wallPivotIsAtMeshCenter, wallsRoot);
```

---

### 2. MazeDoorSpawner.cs (~140 lines)
**Purpose:** Spawn doors on access walls  

**Methods:**
- `SpawnDoors()` - Spawn all doors in maze
- `SpawnDoorOnAccessWall()` - Spawn single door
- `GetDoorDirection()` - Extract direction from cell flags

**Usage:**
```csharp
MazeDoorSpawner.SpawnDoors(
    mazeData, doorPrefab,
    cellSize, wallHeight, wallThickness,
    wallPivotIsAtMeshCenter);
```

---

### 3. MazeObjectSpawner.cs (~160 lines)
**Purpose:** Spawn torches, chests, enemies  

**Methods:**
- `SpawnTorches()` - Spawn all torches
- `SpawnObjects()` - Spawn chests and enemies
- `FindSpawnPositions()` - Find valid spawn positions

**Usage:**
```csharp
MazeObjectSpawner.SpawnTorches(
    mazeData, torchPrefab, cellSize, objectsRoot);

MazeObjectSpawner.SpawnObjects(
    mazeData, chestPrefab, enemyPrefab,
    cellSize, objectsRoot);
```

---

### 4. MazeMarkerSpawner.cs (~200 lines)
**Purpose:** Spawn entrance/exit visual markers  

**Methods:**
- `SpawnRoomMarkers()` - Spawn both markers
- `SpawnEnhancedMarker()` - Single marker with ring, light, particles
- `SpawnFloatingRing()` - Rotating ring mesh
- `CreateTorusMesh()` - Generate ring geometry

**Includes:** `MazeRingRotator` component for animation

**Usage:**
```csharp
MazeMarkerSpawner.SpawnRoomMarkers(
    mazeData, cellSize,
    markerHeight, markerScale, markerLightIntensity);
```

---

## Integration Steps

### Step 1: Update SpawnAllWalls()

Replace the inline wall spawning with:

```csharp
protected override void SpawnAllWalls()
{
    if (wallPrefab == null || _mazeData == null || _config == null)
    {
        Debug.LogError("[MazeBuilder8] Cannot spawn walls - missing data!");
        return;
    }

    int cardinalCount = 0, diagonalCount = 0;
    float cs = _config.CellSize, wh = _config.WallHeight;

    for (int z = 0; z < _mazeData.Height; z++)
    {
        for (int x = 0; x < _mazeData.Width; x++)
        {
            var cell = _mazeData.GetCell(x, z);

            // Cardinal walls
            if ((cell & CellFlags8.Wall_N) != CellFlags8.None)
            {
                MazeWallSpawner.SpawnCardinalWall(x, z, Direction8.N, ...);
                cardinalCount++;
            }
            // Repeat for S, E, W...

            // Diagonal walls
            if ((cell & CellFlags8.Wall_NE) != CellFlags8.None)
            {
                MazeWallSpawner.SpawnDiagonalWall(x, z, Direction8.NE, ...);
                diagonalCount++;
            }
            // Repeat for NW, SE, SW...
        }
    }

    Debug.Log($"[MazeBuilder8] Spawned {cardinalCount} cardinal, {diagonalCount} diagonal walls");
}
```

### Step 2: Replace SpawnDoors()

```csharp
private void SpawnDoors()
{
    MazeDoorSpawner.SpawnDoors(
        _mazeData, doorPrefab,
        _config.CellSize, _config.WallHeight,
        WallThickness, wallPivotIsAtMeshCenter);
}
```

### Step 3: Replace SpawnTorches() and SpawnObjects()

```csharp
private void SpawnTorches()
{
    MazeObjectSpawner.SpawnTorches(
        _mazeData, torchPrefab, _config.CellSize, _objectsRoot);
}

private void SpawnObjects()
{
    MazeObjectSpawner.SpawnObjects(
        _mazeData, chestPrefab, enemyPrefab,
        _config.CellSize, _objectsRoot);
}
```

### Step 4: Replace SpawnRoomMarkers()

```csharp
private void SpawnRoomMarkers()
{
    MazeMarkerSpawner.SpawnRoomMarkers(
        _mazeData, _config.CellSize,
        markerHeight, markerScale, markerLightIntensity);
}
```

---

## Benefits

| Aspect | Before | After |
|--------|--------|-------|
| File Size | 1141 lines | ~600 lines |
| Responsibilities | 8 major | 1 (orchestration) |
| Testability | Hard | Easy |
| Reusability | None | High |
| Maintainability | Low | High |

---

## Next Steps

1. ✅ Create MazeWallSpawner.cs
2. ✅ Create MazeDoorSpawner.cs
3. ✅ Create MazeObjectSpawner.cs
4. ✅ Create MazeMarkerSpawner.cs
5. ⏳ Update CompleteMazeBuilder8.cs to use modules
6. ⏳ Test maze generation
7. ⏳ Remove old inline methods

---

## LightEngine.cs Refactoring (Next)

**Similar approach:**
- DynamicLightManager.cs - Manage dynamic light pool
- FogOfWarController.cs - Darkness/fog system
- LightningEffectSystem.cs - Lightning flash effects
- LightPoolManager.cs - Light pooling and recycling

**Estimated:** 4 new files, ~900 lines total

---

**License:** GPL-3.0  
**Author:** Ocxyde  
**Copyright © 2026 CodeDotLavos. All rights reserved.**
