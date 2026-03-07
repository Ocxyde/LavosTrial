# Clean Maze Setup Guide - 8-Axis with Diagonal Walls

**Version:** 1.0  
**Date:** 2026-03-07  
**Unity:** 6000.3.7f1

---

## 📋 PREREQUISITES

### Required Prefabs (in `Assets/Resources/Prefabs/`):

- ✅ `WallPrefab.prefab` - Cardinal walls (N, E, S, W)
- ✅ `DiagonalWallPrefab.prefab` - Diagonal walls (NE, NW, SE, SW)
- ✅ `DoorPrefab.prefab` - Doors
- ✅ `TorchHandlePrefab.prefab` - Torches
- ✅ `FloorTilePrefab.prefab` - Floor tiles (optional)

### Required Config:

- ✅ `Config/GameConfig8-default.json` with `"diagonalWalls": true`

---

## 🎯 STEP-BY-STEP SETUP

### Step 1: Create New Scene

1. `File → New Scene → Empty Scene`
2. Save as `Scenes/MazeTest.unity`

---

### Step 2: Add Required Components

Create empty GameObjects and add these scripts:

#### A. GameConfig (REQUIRED)
```
GameObject: "GameConfig"
Component: Code.Lavos.Core.GameConfig
```
- This reads `GameConfig8-default.json` automatically
- Ensures `diagonalWalls: true` is loaded

#### B. CompleteMazeBuilder (REQUIRED)
```
GameObject: "CompleteMazeBuilder"
Component: Code.Lavos.Core.CompleteMazeBuilder
```

**Assign in Inspector:**
| Field | Value |
|-------|-------|
| `wallPrefab` | `Prefabs/WallPrefab.prefab` |
| `wallDiagPrefab` | `Prefabs/DiagonalWallPrefab.prefab` |
| `doorPrefab` | `Prefabs/DoorPrefab.prefab` |
| `torchPrefab` | `Prefabs/TorchHandlePrefab.prefab` |
| `chestPrefab` | (optional) |
| `enemyPrefab` | (optional) |
| `floorTilePrefab` | `Prefabs/FloorTilePrefab.prefab` |

---

### Step 3: Add Player

#### Option A: Use Existing Player Prefab
```
1. Drag player prefab into scene
2. Position at (0, 1, 0)
3. Add PlayerController script if not present
```

#### Option B: Simple Capsule (Testing)
```
1. Create → 3D Object → Capsule
2. Name: "Player"
3. Position: (0, 1, 0)
4. Add CharacterController component
5. Add PlayerController script
```

---

### Step 4: Add Camera

#### Option A: FPS Camera
```
1. Create → Camera
2. Position: (0, 1.7, 0) (eye height)
3. Add CameraController or use mouse look script
```

#### Option B: Use Player Camera
- If player prefab has camera, skip this step

---

### Step 5: Add Lighting

```
1. Create → Light → Directional Light
2. Rotation: (50, -30, 0)
3. Intensity: 1.0
4. Enable Shadows
```

---

### Step 6: Verify Setup

**In CompleteMazeBuilder Inspector, verify:**
- [ ] `wallPrefab` assigned
- [ ] `wallDiagPrefab` assigned (for diagonal walls!)
- [ ] `doorPrefab` assigned
- [ ] `torchPrefab` assigned
- [ ] GameConfig is in scene

---

### Step 7: Generate Maze

**Method 1: Auto-Generate on Play**
```
1. Press Play
2. Maze generates automatically
3. Check Console for errors
```

**Method 2: Editor Preview (Optional)**
```
Tools → Maze → Preview Maze (1-Click Render)
```
- Shows maze in Editor without entering Play mode
- Useful for visual verification

---

## 🔍 VERIFICATION CHECKLIST

After pressing Play, check:

### Console (should see):
```
✅ [CompleteMazeBuilder] Generating maze...
✅ [CompleteMazeBuilder] Maze generated: XxX cells
✅ [CompleteMazeBuilder] Spawned X cardinal walls
✅ [CompleteMazeBuilder] Spawned X diagonal walls
```

### Hierarchy (should see):
```
✅ MazeGround
✅ MazeWalls
  ├── Wall_0_0_N
  ├── Wall_0_0_NE (diagonal!)
  └── ...
✅ MazeObjects
  ├── Torches
  ├── Chests
  └── Enemies
```

### Scene View (should see):
```
✅ Grid of walls with diagonal connections
✅ No gaps between wall segments
✅ Diagonal walls at 45° angles connecting corners
```

---

## 🐛 TROUBLESHOOTING

### No Diagonal Walls Appear

**Check 1:** Is `wallDiagPrefab` assigned?
```
CompleteMazeBuilder Inspector → wallDiagPrefab field
```

**Check 2:** Is diagonal walls enabled in config?
```json
// Config/GameConfig8-default.json
{
    "diagonalWalls": true
}
```

**Check 3:** Check Console for diagonal wall count
```
Look for: "Spawned X diagonal walls"
```

### Walls Don't Connect / Have Gaps

**Issue:** Wall prefab pivot point incorrect

**Fix:**
1. Open `WallPrefab.prefab`
2. Ensure pivot is at **bottom center** of wall
3. Wall should extend from pivot in +Z direction (for N-S walls)
4. Save prefab

### Pink/Missing Textures

**Issue:** Material paths incorrect

**Fix:**
1. Check wall prefab has material assigned
2. Check material has texture assigned
3. Verify texture exists in project

### Maze Not Generating

**Check:**
1. GameConfig component in scene?
2. CompleteMazeBuilder component in scene?
3. All required prefabs assigned?
4. Console for error messages

---

## 📐 WALL PREFAB SPECIFICATIONS

### Cardinal Wall (`WallPrefab.prefab`)
```
Size: X=6m, Y=4m, Z=0.2m (or similar)
Pivot: Bottom center
Rotation: 0° (faces +Z for N-S walls)
```

### Diagonal Wall (`DiagonalWallPrefab.prefab`)
```
Size: X=8.485m (6×√2), Y=4m, Z=0.2m
Pivot: Bottom center
Rotation: 0° (will be rotated 45°/-45° at runtime)
```

### Floor Tile (`FloorTilePrefab.prefab`)
```
Size: X=6m, Y=1m, Z=6m
Pivot: Center or top surface
```

---

## 🎮 PLAY MODE CONTROLS

```
W A S D  - Move
Mouse    - Look around
Space    - Jump
Shift    - Sprint
E        - Interact (doors, chests)
```

---

## 📝 NOTES

### How Diagonal Walls Work

1. **Generation:** `GridMazeGenerator.FillAllWalls()` sets all 8 walls per cell
2. **Carving:** DFS algorithm clears walls when creating passages
3. **Spawning:** `CompleteMazeBuilder.SpawnAllWalls()` checks each cell for wall flags
4. **Rendering:** Diagonal walls use separate prefab, rotated 45°/-45°

### CellFlags8 Layout
```
Bit 0-3: Cardinal walls (N, S, E, W)
Bit 4-7: Diagonal walls (NE, NW, SE, SW)
Bit 8+:  Objects (SpawnRoom, HasChest, HasEnemy, HasTorch, IsExit)
```

### Diagonal Wall Math
```
Cell size: 6m
Diagonal length: 6 × √2 ≈ 8.485m
Rotation: NE=45°, NW=-45°, SE=-45°, SW=45°
Position: Corner of cell (not edge)
```

---

## ✅ QUICK TEST

**Minimal setup for testing:**

1. Empty scene
2. GameConfig GameObject
3. CompleteMazeBuilder GameObject
4. Assign `wallPrefab` and `wallDiagPrefab`
5. Press Play

**Expected result:** Maze with both cardinal and diagonal walls!

---

**Document:** Clean_Maze_Setup_Guide.md  
**Location:** `Assets/Docs/`  
**Status:** ✅ Production Ready
