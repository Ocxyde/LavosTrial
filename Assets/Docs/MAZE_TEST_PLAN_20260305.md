# MAZE GENERATION TEST PLAN

**Date:** 2026-03-05
**Tester:** Ocxyde & BetsyBoop
**Unity Version:** 6000.3.7f1
**Status:** ⏳ READY FOR TESTING

---

## 🎯 **TEST OBJECTIVE**

**Debunk/Verify:** CompleteMazeBuilder system works correctly with:
- ✅ Plug-in-out compliance
- ✅ All values from JSON
- ✅ 9-step simplified generation
- ✅ Zero errors/warnings

---

## 📋 **PRE-TEST CHECKLIST**

### **Required Files:**
- [ ] `Assets/Scripts/Core/06_Maze/CompleteMazeBuilder.cs` exists
- [ ] `Assets/Scripts/Core/06_Maze/GridMazeGenerator.cs` exists
- [ ] `Assets/Scripts/Core/08_Environment/SpatialPlacer.cs` exists
- [ ] `Assets/Scripts/Core/10_Resources/LightPlacementEngine.cs` exists
- [ ] `Assets/Scripts/Core/10_Resources/TorchPool.cs` exists
- [ ] `Config/GameConfig-default.json` exists

### **Required Prefabs:**
- [ ] `Prefabs/WallPrefab.prefab` (or will be auto-created)
- [ ] `Prefabs/DoorPrefab.prefab` (or will be auto-created)
- [ ] `Prefabs/TorchHandlePrefab.prefab` (or will be auto-created)

### **Required Materials:**
- [ ] `Materials/WallMaterial.mat` (or will be auto-created)
- [ ] `Materials/Floor/Stone_Floor.mat` (or will be auto-created)

### **Setup:**
- [ ] Unity 6000.3.7f1 opened
- [ ] No console errors on startup
- [ ] New Input System enabled (Project Settings)

---

## 🧪 **TEST 1: FIRST MAZE GENERATION**

### **Steps:**
1. Open Unity Editor
2. Create new empty scene (or use existing)
3. Open Console window (Ctrl+Shift+C)
4. **Tools → Generate Maze** (or Ctrl+Alt+G)

### **Expected Console Output:**
```
═══════════════════════════════════════════
  MAZE GENERATOR - Auto-Setup & Generation
═══════════════════════════════════════════
[Setup]  Finding components (plug-in-out)...
[Setup]  CompleteMazeBuilder found ✓
[Setup]  EventHandler found ✓
[Setup]  Player found ✓
[MazeBuilderEditor]  Config loaded:
    Maze Size: 21x21
    Cell Size: 6.0m
    Wall Height: 4.0m
═══════════════════════════════════════════
  GENERATING MAZE...
═══════════════════════════════════════════
[CompleteMazeBuilder]  LEVEL 0 - Maze 21x21
[CompleteMazeBuilder]  Ground spawned
[CompleteMazeBuilder]  Entrance room created
[CompleteMazeBuilder]  Outer walls spawned
[CompleteMazeBuilder]  Corridors carved
[CompleteMazeBuilder]  Doors placed
[CompleteMazeBuilder]  Objects placed
[CompleteMazeBuilder]  Maze saved
[CompleteMazeBuilder]  Player spawned INSIDE
═══════════════════════════════════════════
   MAZE GENERATED!
   Size: 21x21
   Level: 0
   Press Play to test
═══════════════════════════════════════════
```

### **Expected Scene Hierarchy:**
```
Scene
├── EventHandler
├── GameManager
├── MazeBuilder
│   ├── CompleteMazeBuilder
│   ├── SpatialPlacer
│   ├── LightPlacementEngine
│   └── TorchPool
├── Player
│   ├── PlayerController
│   └── Main Camera (FPS, local pos: 0,1.7,0)
├── MazeWalls (parent for all walls)
├── GroundFloor
├── Doors (parent for all doors)
└── Torches (parent for all torches)
```

### **Verification:**
- [ ] NO red errors in Console
- [ ] NO yellow warnings (except expected setup warnings)
- [ ] Ground plane visible (textured)
- [ ] Walls surround maze (21x21 grid)
- [ ] Entrance room visible (5x5 clear area)
- [ ] Corridors connect rooms (2 cells wide)
- [ ] Doors placed in openings
- [ ] Torches mounted on walls (30% chance)

### **Pass Criteria:**
✅ Maze generates with ZERO compilation errors
✅ All console messages show green checkmarks
✅ Scene hierarchy matches expected structure

---

## 🧪 **TEST 2: LEVEL PROGRESSION**

### **Steps:**
1. After Test 1 passes
2. **Tools → Next Level (Harder)**
3. **Tools → Generate Maze** again

### **Expected Console Output:**
```
[MazeBuilderEditor]  Level 1 - Maze 13x13
```

*(Note: Level 0 = 21x21 from config, Level 1 = 13x13 from formula)*

### **Verification:**
- [ ] Console shows "Level 1"
- [ ] Maze size changed (check console)
- [ ] New maze generated successfully

### **Pass Criteria:**
✅ Level increments correctly
✅ Maze size changes per formula: `MazeSize = 12 + Level`

---

## 🧪 **TEST 3: CONFIG CHANGES**

### **Steps:**
1. Open `Config/GameConfig-default.json`
2. Change `defaultMazeWidth` from 21 to 15
3. Change `defaultMazeHeight` from 21 to 15
4. Save file
5. **Tools → Clear Maze Objects**
6. **Tools → Generate Maze**

### **Expected:**
- [ ] Maze is now 15x15
- [ ] Console shows "Maze 15x15"
- [ ] Smaller maze visible in Scene view

### **Pass Criteria:**
✅ Config changes apply correctly
✅ No hardcoded values

---

## 🧪 **TEST 4: PLAYER SPAWN**

### **Steps:**
1. Generate maze (Test 1)
2. Press **Play** button
3. Check Player position in Inspector

### **Expected:**
- [ ] Player spawns INSIDE entrance room
- [ ] Player NOT inside walls
- [ ] Player NOT falling through ground
- [ ] Camera at eye level (1.7m)
- [ ] FPS controls work (WASD + Mouse)

### **Controls Test:**
- [ ] W/A/S/D moves player
- [ ] Mouse looks around
- [ ] Space jumps (check stamina bar if HUD present)
- [ ] Shift sprints (check stamina drain)

### **Pass Criteria:**
✅ Player spawns in valid position
✅ Player controls responsive

---

## 🧪 **TEST 5: SAVE/LOAD SYSTEM**

### **Steps:**
1. Generate maze with seed "TestSeed123"
2. Stop Play mode
3. Generate maze again with same seed
4. Compare maze layouts

### **Expected:**
- [ ] Same maze layout both times
- [ ] Same wall positions
- [ ] Same door positions
- [ ] Same torch positions

### **Verification:**
```
# Check saved data location:
Assets/StreamingAssets/MazeStorage/
# Should have .bin files
```

### **Pass Criteria:**
✅ Seed-based generation reproducible
✅ Binary storage working

---

## 🧪 **TEST 6: CONSOLE COMMANDS**

### **Steps:**
1. Open Console window
2. Type commands in Console (if MazeConsoleCommands enabled)

### **Commands to Test:**
```
maze.generate    → Generates maze
maze.status      → Shows level, size, seed
maze.help        → Shows available commands
```

### **Pass Criteria:**
✅ Console commands work (if enabled)

---

## 🧪 **TEST 7: PLUG-IN-OUT COMPLIANCE**

### **Code Review:**
Check `CompleteMazeBuilder.cs`:

```csharp
// ✅ CORRECT - Finds components
private void FindComponents()
{
    spatialPlacer = FindFirstObjectByType<SpatialPlacer>();
    lightPlacementEngine = FindFirstObjectByType<LightPlacementEngine>();
    torchPool = FindFirstObjectByType<TorchPool>();
}

// ❌ WRONG - Creates components (should NOT see this!)
private void CreateComponents()
{
    spatialPlacer = gameObject.AddComponent<SpatialPlacer>();
}
```

### **Verification:**
- [ ] No `new GameObject()` calls in runtime code
- [ ] No `AddComponent<T>()` calls in runtime code
- [ ] All components found via `FindFirstObjectByType<T>()`

### **Pass Criteria:**
✅ 100% plug-in-out compliant

---

## 📊 **TEST RESULTS TEMPLATE**

### **Test Summary:**

| Test | Status | Notes |
|------|--------|-------|
| Test 1: First Generation | ⬜ Pass / ⬜ Fail | |
| Test 2: Level Progression | ⬜ Pass / ⬜ Fail | |
| Test 3: Config Changes | ⬜ Pass / ⬜ Fail | |
| Test 4: Player Spawn | ⬜ Pass / ⬜ Fail | |
| Test 5: Save/Load | ⬜ Pass / ⬜ Fail | |
| Test 6: Console Commands | ⬜ Pass / ⬜ Fail | |
| Test 7: Plug-in-Out | ⬜ Pass / ⬜ Fail | |

### **Issues Found:**

| Issue | Severity | Steps to Reproduce |
|-------|----------|-------------------|
| | Critical / Major / Minor | |

### **Performance Metrics:**

| Metric | Value | Target |
|--------|-------|--------|
| Generation Time | < 1 sec | < 2 sec |
| Frame Rate (Play) | FPS | > 30 FPS |
| Memory Usage | MB | < 512 MB |

---

## 🐛 **KNOWN ISSUES TO WATCH FOR**

1. ⚠️ **Missing Prefabs:** If prefabs don't exist, editor tool auto-creates them
2. ⚠️ **Component Not Found:** Some components may need manual scene setup
3. ⚠️ **First Compile:** First generation may be slower (Unity compiling)

---

## ✅ **POST-TEST ACTIONS**

### **If All Tests Pass:**
1. ✅ Run `backup.ps1` to save working state
2. ✅ Git commit: "Maze generation verified"
3. ✅ Mark as PRODUCTION READY

### **If Tests Fail:**
1. ❌ Document failures in Issues table
2. ❌ Run `scan-project-errors.ps1`
3. ❌ Fix issues before backup

---

## 🚀 **QUICK START**

```bash
# In Unity Editor:
# 1. Generate maze
Ctrl+Alt+G

# 2. Clear maze
Tools → Maze → Clear Maze Objects

# 3. Next level
Tools → Maze → Next Level (Harder)

# 4. Test in Play mode
Press Play button
```

---

**Ready to debunk this maze system!** 🫡🔍

---

*Document generated - Unity 6 (6000.3.7f1) compatible - UTF-8 encoding - Unix LF*
