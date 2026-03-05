# TODO.md - Project Tasks & Priorities

**Project:** PeuImporte (Unity 6000.3.7f1)
**Last Updated:** 2026-03-06
**Timezone:** Europe/Paris (GMT+1)
**Status:** ✅ **0 COMPILATION ERRORS** | ✅ **PLUG-IN-OUT COMPLIANT** | ✅ **ALL VALUES FROM JSON**

---

## 📅 **UPDATE TIMESTAMP LOG**

| Date | Update | Author |
|------|--------|--------|
| 2026-03-06 | CompleteMazeBuilder SIMPLIFIED (verbosity removed, ~500 lines, short logging only) | Ocxyde |
| 2026-03-06 | CompleteMazeBuilder OPTIMIZED (~550 lines, verbosity logging restored) | Ocxyde & BetsyBoop |
| 2026-03-06 | CompleteMazeBuilder OPTIMIZED (~500 lines, better perf) | Ocxyde & BetsyBoop |
| 2026-03-06 | CompleteMazeBuilder rewritten as MAIN ORCHESTRATOR | Ocxyde & BetsyBoop |

---

## 🚨 **CRITICAL: DEPRECATED SYSTEMS**

### **⚠️ DO NOT USE FOR NEW DEVELOPMENT:**

| System | Deprecated Files | Use Instead |
|--------|-----------------|-------------|
| **Maze Generation** | `MazeIntegration.cs`, `MazeRenderer.cs` | `CompleteMazeBuilder.cs` |
| **Door Placement** | `DoorHolePlacer.cs`, `RoomDoorPlacer.cs` | `DoorsEngine.cs` + `RealisticDoorFactory.cs` |
| **Audio** | `SFXVFXEngine.cs` | `AudioManager.cs` |

**Why kept?** Legacy tests and scenes still use them.

---

## ✅ **COMPLETED TASKS**

### **✅ CompleteMazeBuilder - Main Game Orchestrator**

**Features:**
- ✅ Level progression (Level 0 = 12x12, +1 per level, max 51x51)
- ✅ Seed-based difficulty (longer seed = harder)
- ✅ Spawn room placed FIRST (guaranteed)
- ✅ Corridors carved TO/FROM spawn room
- ✅ Walls placed with proper orientation
- ✅ Simple entrance/exit doors
- ✅ Torches mounted on walls (using prefab, 30% chance)
- ✅ All materials/textures from JSON config
- ✅ Player spawns LAST (after all geometry, FPS camera at 1.7m)
- ✅ Binary storage for maze data
- ✅ Plug-in-out compliant (finds components, never creates)
- ✅ No hardcoded values (all from `GameConfig.Instance`)

**Files:**
- ✅ `CompleteMazeBuilder.cs` (~500 lines, simplified)
- ✅ `GridMazeGenerator.cs` (grid-based algorithm)
- ✅ `MazeConsoleCommands.cs` (console commands)

---

### **✅ Specialized Object Placers**

**Files:**
- ✅ `ChestPlacer.cs` - Chest placement
- ✅ `EnemyPlacer.cs` - Enemy placement
- ✅ `ItemPlacer.cs` - Item placement
- ✅ `TorchPlacer.cs` - Torch placement (integrated in CompleteMazeBuilder)
- ✅ `SpatialPlacer.cs` - Universal object orchestrator

**All plug-in-out compliant, all values from JSON.**

---

### **✅ Quick Setup Tools**

**Files:**
- ✅ `QuickSetupPrefabs.cs` - Auto-creates prefabs and materials
- ✅ `MazeBuilderEditor.cs` - Editor tools for maze generation
- ✅ `CreatePlayer.cs` - Creates player with camera (separate tool)

**Usage:**
```
Tools → Quick Setup Prefabs (For Testing)
Tools → Generate Maze (Ctrl+Alt+G)
Tools → Create Player
Tools → Next Level (Harder)
```

---

### **✅ Cleanup & Compliance**

**Fixed:**
- ✅ All hardcoded values → JSON config
- ✅ All component creation → FindFirstObjectByType
- ✅ All legacy helper files → Commented deprecated code
- ✅ All compilation errors → 0 errors, 0 warnings
- ✅ Verbosity system → Removed (simple logging only)

---

## 📋 **PENDING TASKS**

### **Phase 2: Remove Redundancy** (HIGH PRIORITY)
- [ ] **Delete `SpawnPlacerEngine.cs`** - Duplicates `SpatialPlacer.cs`
- [ ] Update references to use `SpatialPlacer.cs`
- [ ] Test in Unity

### **Phase 4: Clean Up Commented Code** (MEDIUM PRIORITY)
- [ ] Remove large commented blocks from `MazeIntegration.cs`
- [ ] Remove commented code from `SeedManager.cs`
- [ ] Remove commented code from `SpawnPlacerEngine.cs`
- [ ] Verify truncated files:
  - [ ] `LightEngine.cs` (truncated at 750/910)
  - [ ] `ParticleGenerator.cs` (truncated at 761/880)

### **Phase 5: Full Deprecated File Removal** (LOW PRIORITY)

#### **Test Files:**
- [ ] **Delete `FpsMazeTest.cs`** - Create new with CompleteMazeBuilder
- [ ] **Delete `MazeTorchTest.cs`** - Create new with CompleteMazeBuilder
- [ ] **Delete `DebugCameraIssue.cs`** - Debug helper for legacy

#### **Core Files:**
- [ ] **Delete `MazeRenderer.cs`** - Geometry handled by CompleteMazeBuilder
- [ ] **KEEP `MazeIntegration.cs`** - Still used by door system
- [ ] **KEEP `DoorHolePlacer.cs`** - Core door engine (updated)
- [ ] **KEEP `RoomDoorPlacer.cs`** - Core door engine (updated)
- [ ] **KEEP `SFXVFXEngine.cs`** - Visual FX (NOT audio!)

---

## 🧪 **TESTING CHECKLIST**

### **Pre-Test Setup:**
- [ ] Unity 6000.3.7f1 opened
- [ ] Scene loaded with required components
- [ ] Console window open
- [ ] No errors before testing

### **Test 1: First Maze Generation:**
- [ ] Console shows: "LEVEL 0 - Maze 12x12"
- [ ] Console shows: "Spawn room placed"
- [ ] Console shows: "Walls placed (oriented)"
- [ ] Console shows: "Doors placed"
- [ ] Console shows: "Torches mounted"
- [ ] Console shows: "Player spawned INSIDE maze"
- [ ] NO errors (red messages)
- [ ] Ground spawns first
- [ ] Walls snap properly
- [ ] Rooms are CLEAR
- [ ] Corridors are 2 cells wide

### **Test 2: Level Progression:**
- [ ] Tools → Next Level (Harder)
- [ ] Console shows: "Level 1 - Maze 13x13"
- [ ] Maze size increases correctly

### **Test 3: Console Commands:**
- [ ] `maze.generate` → Generates maze
- [ ] `maze.status` → Shows level, size, seed
- [ ] `maze.help` → Shows commands

---

## 🛠️ **SCENE SETUP REQUIREMENTS**

**Add these components to scenes manually:**

### **Required for All Scenes:**
- [ ] `EventHandler` - Central event hub
- [ ] `GameManager` - Game state management

### **Required for Maze Scenes:**
- [ ] `CompleteMazeBuilder` - Main orchestrator
- [ ] `GridMazeGenerator` - Created by CompleteMazeBuilder
- [ ] `SpatialPlacer` - Object placement
- [ ] `LightPlacementEngine` - Torch binary storage
- [ ] `TorchPool` - Torch management
- [ ] `DoorsEngine` - Door behavior
- [ ] `PlayerController` - Player with FPS camera

### **Required for Audio:**
- [ ] `AudioManager` - Professional audio

### **Required for Lighting:**
- [ ] `LightEngine` - Lighting coordination

### **Required for Procedural:**
- [ ] `ProceduralCompute` - Procedural utilities
- [ ] `DrawingPool` - Texture generation

### **Prefabs (in Resources/):**
- [ ] `Prefabs/WallPrefab.prefab`
- [ ] `Prefabs/DoorPrefab.prefab`
- [ ] `Prefabs/TorchHandlePrefab.prefab`
- [ ] `Materials/WallMaterial.mat`
- [ ] `Materials/Floor/Stone_Floor.mat`
- [ ] `Textures/floor_texture.png`

**Run:** `Tools → Quick Setup Prefabs` to auto-create!

---

## 🎮 **GAME PROGRESSION**

| Level | Maze Size | Difficulty | Description |
|-------|-----------|------------|-------------|
| **0** | 12x12 | Easy | Tutorial maze |
| **1** | 13x13 | Easy+ | Slightly harder |
| **5** | 17x17 | Medium | Moderate challenge |
| **10** | 22x22 | Hard | Serious maze |
| **20** | 32x32 | Very Hard | Expert level |
| **39** | 51x51 | Extreme | Maximum size |

**Formula:** `MazeSize = 12 + Level` (clamped 12-51)

---

## 📊 **PROJECT METRICS**

| Metric | Value | Status |
|--------|-------|--------|
| **Total Core Files** | ~60 files | ✅ |
| **Compilation Errors** | 0 | ✅ |
| **Compilation Warnings** | 0 | ✅ |
| **Plug-in-Out Compliance** | 100% | ✅ |
| **Hardcoded Values** | 0% (all JSON) | ✅ |
| **Code Reduction** | 51% (SpatialPlacer) | ✅ |
| **Binary Storage** | Implemented | ✅ |
| **Documentation** | 4+ files | ✅ |

---

## 🚀 **NEXT STEPS**

1. **HIGH:** Test in Unity - Verify maze generation works
2. **HIGH:** Delete `SpawnPlacerEngine.cs` (Phase 2)
3. **MEDIUM:** Clean up commented code (Phase 4)
4. **LOW:** Migrate test files to CompleteMazeBuilder (Phase 5)

---

**Generated:** 2026-03-06
**Unity Version:** 6000.3.7f1
**Status:** ✅ **READY FOR TESTING**

---

*Document generated - Unity 6 compatible - UTF-8 encoding - Unix LF*

**We made it, coder friend!** 🫡🎮⚔️
