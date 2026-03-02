# Door System Verification Checklist

**Date:** 2026-03-01
**Unity Version:** 6000.3.7f1
**Status:** ✅ Ready for Integration

---

## ✅ Pre-Flight Check

### 1. Required Components on Maze GameObject

| Component | Status | Purpose |
|-----------|--------|---------|
| `MazeGenerator` | ⬜ | Generates maze grid |
| `RoomGenerator` | ⬜ | Generates rooms in maze |
| `DoorHolePlacer` | ⬜ | Reserves wall holes for doors |
| `RoomDoorPlacer` | ⬜ | Places doors in holes |
| `MazeRenderer` | ⬜ | Renders maze geometry |
| `TorchPool` | ⬜ | Places torches on walls |

**How to Check:**
1. Select your Maze GameObject
2. Look in Inspector for all components above
3. If missing, run **Add Missing Components** from `DoorSystemSetup` context menu

---

### 2. Component Configuration

#### DoorHolePlacer Settings

| Field | Recommended | Current | Status |
|-------|-------------|---------|--------|
| `holeWidth` | 2.5 | ⬜ | ⬜ |
| `holeHeight` | 3.0 | ⬜ | ⬜ |
| `holeDepth` | 0.5 | ⬜ | ⬜ |
| `doorChancePerWall` | 0.6 (60%) | ⬜ | ⬜ |
| `carveHolesInWalls` | ✓ | ⬜ | ⬜ |

#### RoomDoorPlacer Settings

| Field | Recommended | Current | Status |
|-------|-------------|---------|--------|
| `placeDoorsInHoles` | ✓ | ⬜ | ⬜ |
| `randomizeWallTextures` | ✓ | ⬜ | ⬜ |
| `availableVariants` | Wood, Stone, Metal | ⬜ | ⬜ |
| `availableTrapTypes` | None, None, Poison | ⬜ | ⬜ |
| `wallTextureSets` | (at least 1) | ⬜ | ⬜ |

---

### 3. Wall Texture Sets (Optional but Recommended)

Create at least 3 texture sets for variety:

#### Texture Set 1: Stone Dungeon
```
- setName: "Stone Dungeon"
- baseColor: RGB(120, 120, 120)
- variationColor: RGB(80, 80, 80)
- noiseScale: 0.1
- contrast: 1.2
- tint: RGB(255, 255, 255)
- smoothness: 0.5
```

#### Texture Set 2: Brick Wall
```
- setName: "Brick Wall"
- baseColor: RGB(140, 60, 40)
- variationColor: RGB(100, 40, 30)
- noiseScale: 0.15
- contrast: 1.3
- tint: RGB(255, 240, 230)
- smoothness: 0.3
```

#### Texture Set 3: Wood Panel
```
- setName: "Wood Panel"
- baseColor: RGB(100, 70, 40)
- variationColor: RGB(80, 55, 30)
- noiseScale: 0.2
- contrast: 1.1
- tint: RGB(255, 220, 180)
- smoothness: 0.6
```

---

## 🔧 Setup Steps

### Step 1: Add DoorSystemSetup Helper

1. Select Maze GameObject
2. Add Component → `DoorSystemSetup`
3. Right-click component → **Add Missing Components**
4. Right-click again → **Verify Door System Setup**

### Step 2: Configure DoorHolePlacer

1. Select Maze GameObject
2. Find `DoorHolePlacer` component
3. Set values from table above
4. Enable `showDebugGizmos` for visualization

### Step 3: Configure RoomDoorPlacer

1. Select Maze GameObject
2. Find `RoomDoorPlacer` component
3. Add door variants (Wood, Stone, Metal, Magic, Iron)
4. Add trap types (None, None, Poison, Fire, Lightning)
5. Add 3 `WallTextureSet` entries (see above)

### Step 4: Configure MazeRenderer

1. Select Maze GameObject
2. Find `MazeRenderer` component
3. Set `cellSize` = 4.0
4. Set `wallHeight` = 3.0
5. Adjust `torchProbability` = 0.15 (optional)

### Step 5: Test Generation

1. Press **Play** in Unity
2. Check Console for messages:
   ```
   [MazeGenerator] Generated maze with seed: XXXXX
   [RoomGenerator] Generated X rooms
   [DoorHolePlacer] Placed X door holes
   [RoomDoorPlacer] Placed X doors in holes
   ```
3. Check Scene view for:
   - Rooms with blue wireframe gizmos
   - Door positions with yellow spheres
   - Door holes with cyan wireframes

---

## 🎯 Expected Results

### In Scene View (Gizmos Enabled)

```
┌────────────────────────────────────────────┐
│  Blue wireframes     = Room bounds        │
│  Cyan wireframes     = Door holes          │
│  Yellow spheres      = Placed doors        │
│  Green wireframes    = Wall geometry       │
└────────────────────────────────────────────┘
```

### In Console

```
[MazeGenerator] Generated maze (seed: 12345)
[RoomGenerator] Generated 5 rooms (complexity: 10)
[DoorHolePlacer] Starting hole placement in room walls...
[DoorHolePlacer] Placed 15 door holes
[RoomDoorPlacer] Starting door placement in reserved holes...
[RoomDoorPlacer] Placed Wood door at (5, 3)
[RoomDoorPlacer] Placed Stone door at (8, 3)
[RoomDoorPlacer] Placed 15 doors in holes
[MazeRenderer] Maze build complete
```

---

## ⚠️ Troubleshooting

### No Doors Appearing

**Check:**
1. `placeDoorsInHoles` is enabled ✓
2. `doorChancePerWall` > 0 (try 1.0 for testing)
3. `DoorHolePlacer` is on same GameObject
4. Console shows holes were placed

### No Holes Appearing

**Check:**
1. `carveHolesInWalls` is enabled ✓
2. `RoomGenerator` generated rooms first
3. Rooms have wall segments (not just entrances)
4. Console shows rooms were generated

### Doors Don't Fit Holes

**Check:**
1. Door dimensions are 90% of hole (auto-calculated)
2. Hole dimensions are reasonable (2.5×3×0.5)
3. No scaling issues on parent transforms

### Compilation Errors

**Run:**
```powershell
.\scan-project-errors.ps1
```

Should show:
- ✅ 0 errors
- ⚠️ 2 warnings (non-critical)
- ℹ️ 12 info messages (IL2CPP, namespace)

---

## 📊 Integration Status

| System | Status | Notes |
|--------|--------|-------|
| Maze Generation | ✅ Ready | Generates grid |
| Room Generation | ✅ Ready | Creates rooms |
| Hole Reservation | ✅ Ready | Reserves wall space |
| Door Placement | ✅ Ready | Places in holes |
| Wall Textures | ✅ Ready | Random per door |
| Maze Rendering | ✅ Ready | Builds geometry |
| Torch Placement | ✅ Ready | Lights maze |
| Player Spawning | ✅ Ready | Spawns at start |

---

## 🚀 Ready to Integrate?

**If all checkboxes are ticked:**

1. ✅ All components added
2. ✅ All settings configured
3. ✅ Wall texture sets created
4. ✅ No compilation errors

**Then you're ready!**

### Final Steps:

1. Press **Play** in Unity Editor
2. Watch console for generation messages
3. Walk around in the maze
4. Test doors (interact with E key)
5. Enjoy your procedural dungeon! 🎮

---

## 📝 Quick Commands

### In Unity Editor:

```
Right-click DoorSystemSetup → Verify Setup
Right-click DoorSystemSetup → Add Missing Components
Right-click DoorSystemSetup → Reset to Defaults
```

### In PowerShell (Project Root):

```powershell
# Scan for errors
.\scan-project-errors.ps1

# Backup files
.\backup.ps1

# Apply patches and backup
.\apply-patches-and-backup.ps1
```

---

**Generated:** 2026-03-01 21:20
**Documentation:** `Assets/Docs/DoorSystem_Checklist.md`
**Status:** Ready for integration testing
