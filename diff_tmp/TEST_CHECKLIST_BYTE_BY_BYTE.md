# TEST CHECKLIST - Byte-by-Byte Grid Maze System

**Date:** 2026-03-06
**Feature:** Byte-by-Byte Grid Placement with RAM Cache
**Status:** ⏳ READY FOR TESTING

---

## 🎯 **PRE-TEST SETUP**

### **Required:**
- [ ] Unity 6000.3.7f1 opened
- [ ] Scene loaded (any maze test scene)
- [ ] Console window open
- [ ] No errors in Console before testing

### **Components Needed in Scene:**
- [ ] CompleteMazeBuilder (on GameObject)
- [ ] SpatialPlacer (on GameObject)
- [ ] LightPlacementEngine (on GameObject)
- [ ] TorchPool (on GameObject)
- [ ] TorchHandlePrefab assigned to TorchPool

---

## 🧪 **TEST 1: First Maze Generation (CTRL+ALT+G)**

### **Expected Console Output:**
```
✅ [CompleteMazeBuilder] 📖 Reading JSON config: wallPrefab='Prefabs/WallPrefab.prefab'
✅ [CompleteMazeBuilder] 🔧 Cached prefab paths: wall='Prefabs/WallPrefab.prefab', door='Prefabs/DoorPrefab.prefab'
✅ [CompleteMazeBuilder] ✅ Config loaded into RAM cache (byte-by-byte storage)
✅ [CompleteMazeBuilder] 🧹 Step 1: Scene cleanup complete (EMPTY)
✅ [CompleteMazeBuilder] 🌍 Step 2: Ground spawned (base layer)
✅ [CompleteMazeBuilder] 🔲 Step 3-4: Empty grid created + Entrance room marked at (x, y)
✅ [CompleteMazeBuilder] 👤 Step 5: Player spawn calculated at (x, y, z)
✅ [CompleteMazeBuilder] 🔨 Step 6-7: Rooms + Corridors marked in grid
✅ [CompleteMazeBuilder] 🧱 Step 8: Outer walls marked
✅ [CompleteMazeBuilder] 🧱 Step 9: Walls spawned from grid (byte-by-byte, snapped)
✅ [CompleteMazeBuilder] 🚪 Step 10: Doors placed
✅ [CompleteMazeBuilder] 🎒 Step 11: Objects placed
✅ [CompleteMazeBuilder] 💾 Step 12: Grid saved (byte-by-byte)
✅ [CompleteMazeBuilder] ☁️ Ceiling SKIPPED (testing mode)
```

### **Visual Checks:**
- [ ] **NO wall artifacts** before generation (scene is empty except ground)
- [ ] Ground plane spawns first (flat, no walls)
- [ ] Walls spawn ON grid boundaries (not outside)
- [ ] Rooms are CLEAR (no interior walls)
- [ ] Corridors are 2-cell wide (walkable)
- [ ] Outer perimeter walls surround maze
- [ ] Walls snap properly to grid (no gaps/overlaps)
- [ ] No "too many walls" (optimized count)

### **Console Checks:**
- [ ] **NO errors** (red messages)
- [ ] **NO warnings** about missing prefabs (except torch if not assigned)
- [ ] **NO "ReloadScene failed"** error
- [ ] Config cache loads correctly (shows paths)
- [ ] Grid size logged correctly

---

## 🧪 **TEST 2: Second Maze Generation (CTRL+ALT+G Again)**

### **Expected Console Output:**
```
✅ [CompleteMazeBuilder] 📖 Config already loaded from cache (RAM)
✅ [CompleteMazeBuilder] 🧹 Step 1: Scene cleanup complete (EMPTY)
✅ [CompleteMazeBuilder] 🗑️ Cleaned up old cube primitive: Cube
✅ [CompleteMazeBuilder] 🗑️ Cleaned up old quad primitive: Quad
```

### **Visual Checks:**
- [ ] Old maze destroyed completely
- [ ] New maze generates cleanly
- [ ] No duplicate walls/objects
- [ ] No artifacts from previous generation

---

## 🧪 **TEST 3: Verbosity System**

### **Test Commands (Press ~ to open console):**
```
maze.verbosity full
maze.verbosity short
maze.verbosity mute
maze.status
```

### **Expected Behavior:**
- [ ] `full` → All debug messages shown
- [ ] `short` → Only critical messages (errors, warnings, milestones)
- [ ] `mute` → No console output
- [ ] `status` → Shows current verbosity level

### **JSON Config Check:**
- [ ] Edit `Config/GameConfig-default.json`
- [ ] Change `"consoleVerbosity": "short"` to `"full"`
- [ ] Restart Unity
- [ ] Verify verbosity is "full" on startup

---

## 🧪 **TEST 4: Grid Size Optimization**

### **Check Wall Count:**
```
Small maze (11x11): ~50-80 walls
Medium maze (21x21): ~150-250 walls
Large maze (31x31): ~300-450 walls
```

### **Expected:**
- [ ] Wall count matches maze size
- [ ] No "too many walls" warning
- [ ] Walls only where needed (not over-used)
- [ ] Performance is smooth (no lag)

---

## 🧪 **TEST 5: RAM Cache**

### **Check Cache Loading:**
```
✅ [CompleteMazeBuilder] 📖 Config already loaded from cache (RAM)
```

### **Expected:**
- [ ] Config loads ONCE (first generation)
- [ ] Subsequent generations use cache
- [ ] No repeated JSON file reads
- [ ] Faster generation after first load

---

## 🐛 **KNOWN ISSUES TO VERIFY:**

### **Should be FIXED:**
- [ ] ❌ → ✅ Wall artifacts on first launch
- [ ] ❌ → ✅ Empty prefab paths ("Assets/")
- [ ] ❌ → ✅ Walls outside ground grid
- [ ] ❌ → ✅ Too many walls for tiny maze
- [ ] ❌ → ✅ Config not loading from JSON

### **May Still Exist:**
- [ ] ⚠️ "ReloadScene failed" (Unity Editor issue - restart Unity)
- [ ] ⚠️ Torch prefab not assigned (assign in TorchPool Inspector)
- [ ] ⚠️ LightPlacementEngine not found (add component to scene)
- [ ] ⚠️ No SpawnPoint found (GridMazeGenerator issue)

---

## ✅ **PASS CRITERIA**

### **Must Pass (Critical):**
1. [ ] No compilation errors
2. [ ] No runtime errors (red console messages)
3. [ ] Maze generates without crashes
4. [ ] Walls snap to grid properly
5. [ ] Rooms are clear (no interior walls)
6. [ ] Config loads from JSON
7. [ ] RAM cache works (second gen uses cache)

### **Should Pass (Important):**
8. [ ] No wall artifacts before generation
9. [ ] Walls only on grid boundaries (not outside)
10. [ ] Optimized wall count (not over-used)
11. [ ] Verbosity system works
12. [ ] Cleanup removes old objects

### **Nice to Pass (Optional):**
13. [ ] Console commands work
14. [ ] Torches spawn (if prefab assigned)
15. [ ] No warnings at all

---

## 📊 **TEST RESULTS**

| Test | Status | Notes |
|------|--------|-------|
| First Generation | ⏳ PENDING | |
| Second Generation | ⏳ PENDING | |
| Verbosity System | ⏳ PENDING | |
| Grid Optimization | ⏳ PENDING | |
| RAM Cache | ⏳ PENDING | |

**Overall Status:** ⏳ **READY FOR TESTING**

---

## 🚀 **NEXT STEPS**

### **If All Tests Pass:**
1. ✅ Run `backup.ps1`
2. ✅ Generate diff files: `.\generate-diff-files.ps1`
3. ✅ Git commit with prepared message
4. ✅ Test in fresh Unity project

### **If Tests Fail:**
1. ❌ Note errors/warnings
2. ❌ Report to Ocxyde
3. ❌ Fix issues
4. ❌ Re-test

---

**Generated:** 2026-03-06
**Tester:** Ocxyde
**Status:** ⏳ **READY FOR TESTING**

---

*Test carefully, report all issues!* 🧪🔍
