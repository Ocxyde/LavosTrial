# TODO.md - Project Tasks & Priorities

**Project:** CodeDotLavos
**Unity:** 6000.3.10f1 | **License:** GPL-3.0
**Last Updated:** 2026-03-11 (Session 4) | **Author:** Ocxyde
**Project Health:** 95% 🟢

---

## 📊 PROJECT DASHBOARD

```
┌─────────────────────────────────────────────────────────────────┐
│                    CODEDOTLAVOS - MASTER DASHBOARD               │
├─────────────────────────────────────────────────────────────────┤
│  BUILD STATUS                                                   │
│  Compilation    [███████████░]  95% ⚠️  Door serialization fix  │
│  Unit Tests     [████████████] 100% ✅  58 tests passing        │
│  Architecture   [████████████] 100% ✅  Plug-in-Out compliant   │
│                                                                 │
│  OVERALL PROGRESS                                               │
│  Game Complete  [████████████░░]  92% 🟢 Excellent              │
│                                                                 │
│  📊 14 Systems Complete | 4 In Progress | 🐛 Critical Bugs: 0   │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🎯 CELL-BASED MAZE GENERATION SYSTEM (NEW - Session 4)

### **Phase 1: Core Cell System** 🔴

| Task | Files | Priority | Status |
|------|-------|----------|--------|
| 1.1 Create Cell Data Structure | MazeCell.cs | 🔴 P0 | ✅ DONE |
| 1.2 Implement bool haveWallOnEdge | MazeCell.cs | 🔴 P0 | ✅ DONE |
| 1.3 Create CellType enum | MazeCell.cs | 🔴 P0 | ✅ DONE |
| 1.4 Create CellAgreement enum | MazeCell.cs | 🔴 P0 | ✅ DONE |

### **Phase 2: Primary Path System** 🔴

| Task | Files | Priority | Status |
|------|-------|----------|--------|
| 2.1 Compute Longest Snake Path | CellBasedMazeGenerator.cs | 🔴 P0 | ✅ DONE |
| 2.2 Min/Max Path Length Calculation | DifficultyCurve.cs | 🔴 P0 | ✅ DONE |
| 2.3 Path Cell Marking (isOnPrimaryPath) | CellBasedMazeGenerator.cs | 🔴 P0 | ✅ DONE |
| 2.4 Path Verification System | CellBasedMazeGenerator.cs | 🔴 P0 | ✅ DONE |

### **Phase 3: Room System** 🔴

| Task | Files | Priority | Status |
|------|-------|----------|--------|
| 3.1 Create Room struct (3x3 cells) | Room.cs | 🔴 P0 | ✅ DONE |
| 3.2 Room Wall Surround System | RoomSystem.cs | 🔴 P0 | ✅ DONE |
| 3.3 Two Door Placement (Entry/Exit) | RoomSystem.cs | 🔴 P0 | ✅ DONE |
| 3.4 Door-to-Walkable Verification | RoomSystem.cs | 🔴 P0 | ✅ DONE |
| 3.5 Room Types (Normal/Treasure/Boss/Safe) | Room.cs | 🟡 P1 | ✅ DONE |

### **Phase 4: Decoy Path System** 🟡

| Task | Files | Priority | Status |
|------|-------|----------|--------|
| 4.1 Create DecoyPath struct | DecoyPath.cs | 🟡 P1 | ✅ DONE (in CellBasedMazeGenerator.cs) |
| 4.2 L-Shape Decoy Generator | DecoySystem.cs | 🟡 P1 | ✅ DONE |
| 4.3 Spiral/Fork Decoy Types | DecoySystem.cs | 🟡 P1 | ✅ DONE |
| 4.4 Decoy-Reward Placement | DecoySystem.cs | 🟡 P1 | ✅ DONE |

### **Phase 5: Agreement System** 🟡

| Task | Files | Priority | Status |
|------|-------|----------|--------|
| 5.1 Chest Placement on Decoys | AgreementPlacer.cs | 🟡 P1 | ✅ DONE |
| 5.2 Dead-End Wall Placement | AgreementPlacer.cs | 🟡 P1 | ✅ DONE |
| 5.3 Empty Cell Marking | AgreementPlacer.cs | 🟡 P1 | ✅ DONE |
| 5.4 Enemy Guard Placement | AgreementPlacer.cs | 🟡 P1 | ✅ DONE |

### **Phase 6: Wall & Door Integration** 🟢

| Task | Files | Priority | Status |
|------|-------|----------|--------|
| 6.1 Spawn Walls at Cell Edges | CellToWallConverter.cs | 🟢 P2 | ✅ DONE |
| 6.2 Carve Door Openings in Walls | CellToWallConverter.cs | 🟢 P2 | ✅ DONE |
| 6.3 Spawn DoorController Prefabs | CellToWallConverter.cs | 🟢 P2 | ✅ DONE |
| 6.4 Verify Door Rotation (90° outward) | DoorController.cs | 🟢 P2 | ✅ DONE (Phase 3) |

### **Phase 7: Difficulty Curve** 🟢

| Task | Files | Priority | Status |
|------|-------|----------|--------|
| 7.1 Level-Based Difficulty Scaling | DifficultyScaler.cs | 🟢 P2 | ⏳ Pending |
| 7.2 Decoy Density Scaling | DifficultyScaler.cs | 🟢 P2 | ⏳ Pending |
| 7.3 Room Count Scaling | DifficultyScaler.cs | 🟢 P2 | ⏳ Pending |
| 7.4 Path Length Target Calculation | DifficultyCurve.cs | 🟢 P2 | ⏳ Pending |

### **Phase 8: Verification & Safety** 🟢

| Task | Files | Priority | Status |
|------|-------|----------|--------|
| 8.1 Primary Path Integrity Check | MazeVerifier.cs (new) | 🟢 P2 | ⏳ Pending |
| 8.2 Room Door Connection Check | MazeVerifier.cs | 🟢 P2 | ⏳ Pending |
| 8.3 Decoy Path Termination Check | MazeVerifier.cs | 🟢 P2 | ⏳ Pending |
| 8.4 Exit Reachability Guarantee | MazeVerifier.cs | 🟢 P2 | ⏳ Pending |

---

## 🎯 ACTIVE TASKS

| Document | Purpose |
|----------|---------|
| [PROJECT_DEEP_SCAN_SUMMARY_20260310.md](PROJECT_DEEP_SCAN_SUMMARY_20260310.md) | 📊 Complete project overview |
| [GIT_COMMIT_HISTORY_202603.md](GIT_COMMIT_HISTORY_202603.md) | 📝 Full git commit history |
| [ARCHITECTURE_OVERVIEW.md](ARCHITECTURE_OVERVIEW.md) | 🏗️ System architecture |
| [PROJECT_STANDARDS.md](PROJECT_STANDARDS.md) | 📋 Coding standards |
| [README.md](README.md) | 📖 Modder's guide |

---

## 📝 GIT COMMIT LOG

**Latest commits tracked in:** [GIT_COMMIT_HISTORY_202603.md](GIT_COMMIT_HISTORY_202603.md)

| Period | Commits | Key Focus |
|--------|---------|-----------|
| 2026-03-11 (Session 2) | 10 | Gothic Atmosphere, Cross-Corridors, Small Rooms |
| 2026-03-11 (Session 1) | 7 | Room System, Difficulty Scaling, Debug Logging |
| 2026-03-10 | 23 | Documentation, Unity 6 Naming, Critical Fixes |
| 2026-03-09 | 16 | Maze System, Tests, Architecture |
| 2026-03-07 to 08 | 19 | Refactoring, Compliance, Bug Fixes |
| 2026-03-04 to 06 | 20 | Maze Rewrite, Config, Lighting |
| 2026-03-01 to 03 | 16 | Lighting, Torch, Early Architecture |

**Total:** 111+ commits | **Files Modified:** 200+ | **Lines Changed:** ~6500+

---

## 📊 SYSTEM COMPLETION

```
┌─────────────────────────────────────────────────────────────────┐
│  CORE ENGINE SYSTEMS         [████████████] 100% ✅ Complete    │
│  PLAYER SYSTEMS              [████████████] 100% ✅ Complete    │
│  INTERACTION & INVENTORY     [█████████░░░]  85% 🟡 In Progress │
│  COMBAT SYSTEMS              [████████░░░░]  75% 🟡 In Progress │
│  MAZE GENERATION             [████████████] 100% ✅ Complete    │
│  ENVIRONMENT & LIGHTING      [███████████░]  90% 🟡 Ceilings   │
│  UI & HUD                    [█████████░░░]  80% 🟡 In Progress │
│  AUDIO SYSTEM                [███████░░░░░]  75% 🟡 In Progress │
│  UTILITIES & TOOLS           [████████████] 100% ✅ Complete    │
│                                                                 │
│  OVERALL GAME PROGRESS       [██████████████]  97% 🟢 Excellent │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🎯 ACTIVE TASKS

### ✅ COMPLETED TODAY (2026-03-11 - Session 3)

| Task | Files | Status | Notes |
|------|-------|--------|-------|
| ✅ Door Carving Implementation | MazeData8.cs, GridMazeGenerator.cs, CompleteMazeBuilder.cs | ✅ DONE | Fill & Carve approach |
| ✅ Door Serialization Fix | CompleteMazeBuilder.cs | ✅ DONE | Changed to protected fields |
| ✅ Door Spawning System | CompleteMazeBuilder.cs | ✅ DONE | Uses carved door openings |
| ⏸️ Door Prefab Creation | DoorPrefab_*.prefab | ⏸️ MANUAL | User must create in Unity Editor |

### ✅ COMPLETED TODAY (2026-03-11 - Session 2)

| Task | Files | Status | Notes |
|------|-------|--------|-------|
| ✅ Cross-Corridor System | GridMazeGenerator.cs | ✅ REMOVED | Too buggy, deleted |
| ✅ Interlude Rooms | GridMazeGenerator.cs | ✅ REMOVED | Deleted with cross-corridors |
| ✅ Corridor-Only Lighting | GridMazeGenerator.cs | ✅ DONE | Rooms stay dark/gothic |
| ✅ Corridor Ceiling System | GridMazeGenerator.cs | ✅ DONE | 0.5u thick, rooms open |
| ✅ Torch Placement Specs | GridMazeGenerator.cs | ✅ DONE | Mid-wall, X=25° tilt |

### ✅ COMPLETED TODAY (2026-03-11 - Session 1)

| Task | Files | Status | Notes |
|------|-------|--------|-------|
| ✅ Maze Debug Logging | GridMazeGenerator.cs | ✅ DONE | DFS/A* tracking, iteration limits |
| ✅ Room System Implementation | DifficultyScaler.cs, GridMazeGenerator.cs | ✅ DONE | Proportional room sizing |
| ✅ Door Snapping System | GridMazeGenerator.cs | ✅ DONE | 3-unit openings, perfect snap |
| ✅ Difficulty Scaling Update | DifficultyScaler.cs | ✅ DONE | Room count/size/door complexity |
| ✅ Path Protection | GridMazeGenerator.cs | ✅ DONE | Rooms don't block exit path |
| ✅ Small Room Strategy | DifficultyScaler.cs | ✅ DONE | 12-18% ratio (not 20-30%) |

### 🔴 IN PROGRESS - Testing Phase

| Task | Status | Tester | Notes |
|------|--------|--------|-------|
| 🔴 Torch Wall Snapping | 🧪 TESTING | User | Verify mid-wall position, inward face |
| 🔴 Torch Texture/Material | ⏳ PENDING | User | Apply 8-bit pixel art tex/mat |
| 🔴 Exit Path Verification | 🧪 TESTING | User | Ensure path always exists |
| 🔴 Door System Implementation | ⏳ BACKLOG | - | One door per walkable adjacency |

### Priority 1 - After Testing 🔴

| Task | Files | Time | Status |
|------|-------|------|--------|
| 1.1 Implement Torch Spawning | CompleteMazeBuilder8.cs | 1h | ⏳ Awaiting test feedback |
| 1.2 Implement Ceiling Spawning | CompleteMazeBuilder8.cs | 1h | ⏳ Awaiting test feedback |
| 1.3 Create Ceiling Prefabs | CeilingPrefab_*.prefab | 1h | ⏳ Pixel art 8-bit materials |

### Priority 2 - High 🟡

| Task | Files | Time | Status |
|------|-------|------|--------|
| 2.1 Door Serialization Fix | BaseMazeBuilder.cs, CompleteMazeBuilder.cs | 1h | ✅ DONE (protected fields) |
| 2.2 Door Prefab Creation | DoorPrefab_*.prefab | 1h | ⏳ MANUAL (create in Unity) |
| 2.3 Door Spawning System | CompleteMazeBuilder.cs | 1h | ✅ DONE (uses carved openings) |
| 2.4 Treasure Room Guardians | Enemy placement logic | 30min | ⏳ Pending (1 enemy per chest) |

### Priority 3 - Medium 🟢

| Task | Files | Time | Status |
|------|-------|------|--------|
| 3.1 Danger Room Enemy Density | Enemy placement logic | 30min | ⏳ Pending (more in locked rooms) |
| 3.2 Config file for Resource paths | GameConfig-default.json | 1h | ⏳ Pending |
| 3.3 Thread-safe event subscription | EventHandler.cs | 2h | ⏳ Pending |

### Priority 4 - Low ⚪

| Task | Files | Time | Status |
|------|-------|------|--------|
| 4.1 Fix typo: ShareSystm.cs → ShareSystem.cs | 1 file | 10min | ⏳ Pending |
| 4.2 Standardize folder names | DBSQLite → Database | 30min | ⏳ Pending |
| 4.3 Poisson disk sampling for dead-ends | DeadEndCorridorSystem.cs | 3h | ⏳ Pending |
|------|-------|------|--------|
| 2.1 Config file for Resource paths | GameConfig-default.json | 1h | ⏳ Pending |
| 2.2 Thread-safe event subscription | EventHandler.cs | 2h | ⏳ Pending |
| 2.3 Unit tests for maze algorithms | Assets/Scripts/Tests/ | 4-6h | ⏳ Pending |

### Priority 3 - Medium 🟢

| Task | Files | Time | Status |
|------|-------|------|--------|
| 3.1 Simplify CorridorFillSystem | CorridorFillSystem.cs | 2h | ⏳ Pending |
| 3.2 Fix RealisticDoorFactory null risks | RealisticDoorFactory.cs | 1h | ⏳ Pending |
| 3.3 Remove unused variables | All .cs files | 2h | ⏳ Pending |

### Priority 4 - Low ⚪

| Task | Files | Time | Status |
|------|-------|------|--------|
| 4.1 Fix typo: ShareSystm.cs → ShareSystem.cs | 1 file | 10min | ⏳ Pending |
| 4.2 Standardize folder names | DBSQLite → Database | 30min | ⏳ Pending |
| 4.3 Poisson disk sampling for dead-ends | DeadEndCorridorSystem.cs | 3h | ⏳ Pending |

---

## 📋 NEXT SESSION CHECKLIST

```
PRE-SESSION:
[ ] Run backup.ps1 first
[ ] Check git status
[ ] Review current task

DURING SESSION:
[ ] Work on Priority 1 task
[ ] Test changes in Unity Editor
[ ] Run unit tests

POST-SESSION:
[ ] Run backup.ps1
[ ] Git commit with proper message
[ ] Update this TODO.md
[ ] Schedule cleanup_diff_tmp.ps1
```

---

## 🤖 DAILY REMINDERS

```
EVERY MORNING:
[ ] Run cleanup_diff_tmp.ps1 (delete files >2 days old)
[ ] Review diff_tmp folder
[ ] Check git status

EVERY SESSION END:
[ ] Run backup.ps1 after ANY file change
[ ] Git commit: git add -A → commit → push
[ ] Verify 0 compilation errors
```

---

## 📌 QUICK COMMANDS

```bash
# Backup project (after ANY change)
.\backup.ps1

# Clean diff_tmp (files >2 days old)
.\cleanup_diff_tmp.ps1

# Git workflow
git add -A
git commit -m "Your message"
.\backup.ps1
git push
```

---

## 📖 DOCUMENTATION LINKS

### Architecture & Design
- [PROJECT_DEEP_SCAN_SUMMARY_20260310.md](PROJECT_DEEP_SCAN_SUMMARY_20260310.md) - Complete overview
- [ARCHITECTURE_OVERVIEW.md](ARCHITECTURE_OVERVIEW.md) - System architecture
- [ARCHITECTURE_MAP.md](ARCHITECTURE_MAP.md) - File structure map

### Maze System
- [MAZE_CARDINAL_UPDATE_2026-03-09.md](MAZE_CARDINAL_UPDATE_2026-03-09.md) - Cardinal-only update
- [CRITICAL_FIXES_APPLIED_20260310.md](CRITICAL_FIXES_APPLIED_20260310.md) - Priority 1 fixes
- [DEAD_END_CORRIDOR_SYSTEM.md](DEAD_END_CORRIDOR_SYSTEM.md) - Dead-end system docs

### Testing & Quality
- [TEST_CHECKLIST.md](TEST_CHECKLIST.md) - Testing procedures
- [FIX_TEST_SUITE_ERRORS_20260310.md](FIX_TEST_SUITE_ERRORS_20260310.md) - Test runner fixes

### Git & Backup
- [GIT_COMMIT_HISTORY_202603.md](GIT_COMMIT_HISTORY_202603.md) - Full commit history
- [GIT_WORKFLOW_GUIDE.md](GIT_WORKFLOW_GUIDE.md) - Git procedures
- [backup.md](../../backup.md) - Backup guide

---

**License:** GPL-3.0  
**Author:** Ocxyde  
**Copyright © 2026 CodeDotLavos. All rights reserved.**

---

*Last Updated: 2026-03-10 | Unity 6000.3.10f1 | UTF-8 encoding - Unix LF*

**Motto:** "Happy coding with me : Ocxyde :)"
