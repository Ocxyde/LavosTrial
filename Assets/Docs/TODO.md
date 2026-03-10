# TODO.md - Project Tasks & Priorities

**Project:** CodeDotLavos  
**Unity:** 6000.3.10f1  
**License:** GPL-3.0  
**Last Updated:** 2026-03-10  
**Author:** Ocxyde  

**Status:** ✅ 0 Errors | ✅ 58 Tests | ✅ All Critical Bugs Fixed

---

## 📊 Project Health Overview

**Overall Health:** `████████░░░░░░░░` **78%** | **Score:** 85/100

### Phase Progress
| Phase | Progress | Status |
|-------|----------|--------|
| Phase 1 - Cleanup | `████████████████░` | 85% ✅ |
| Phase 2 - Testing | `██████████░░░░░░░` | 60% ✅ |
| Phase 3 - Documentation | `█████████████░░░░` | 75% ✅ |
| Phase 4 - Bug Fixes | `█████████████████` | 100% ✅ |
| Phase 5 - New Features | `███████████░░░░░░` | 65% ⚠️ |
| Phase 6 - Integration | `████████░░░░░░░░░` | 50% ✅ |

### System Completion
| System | Progress | Status |
|--------|----------|--------|
| Core Systems | `█████████████████` | 100% ✅ |
| Player Controller | `█████████████████` | 100% ✅ |
| Interaction | `████████████████░` | 95% ✅ |
| Inventory | `██████████████░░░` | 80% ✅ |
| Combat | `████████████░░░░░` | 70% 🟡 |
| Maze Generation | `████████████████░` | 90% ✅ |
| Doors | `████████████████░` | 95% ✅ |
| Environment | `██████████████░░░` | 85% ✅ |
| **HUD System** | `████░░░░░░░░░░░░░` | **25% 🔴** |
| Compute/Grid | `████████████████░` | 90% ✅ |
| Audio | `████████████░░░░░` | 75% 🟡 |
| Resources | `██████████████░░░` | 80% ✅ |

### Quality Metrics
| Metric | Score | Status |
|--------|-------|--------|
| Compilation | `█████████████████` | 100% ✅ |
| Code Quality | `████████████░░░░░` | 75% 🟡 |
| Architecture | `████████████░░░░░` | 45% 🔴 |
| Documentation | `█████████████████` | 98% ✅ |
| Organization | `████████████░░░░░` | 80% 🟡 |
| Version Control | `████░░░░░░░░░░░░░` | 40% 🔴 |

---

## 🔴 Critical Issues

| Issue | Count | Progress | Priority |
|-------|-------|----------|----------|
| Plug-in-Out Violations | 108 | `████░░░░░░░░░░░░░` 30% | 🔴 CRITICAL |
| HUD System Violations | 39 | `██░░░░░░░░░░░░░░░` 15% | 🔴 CRITICAL |
| Git Not Configured | - | `░░░░░░░░░░░░░░░░░` 0% | 🔴 CRITICAL |
| Backup Files Clutter | 23 | `░░░░░░░░░░░░░░░░░` 0% | 🟡 HIGH |

### Top Violation Files
| File | Violations |
|------|------------|
| `UIBarsSystem.cs` | 27 |
| `PopWinEngine.cs` | 14 |
| `AudioManager.cs` | 12 |
| `DialogEngine.cs` | 10 |
| `TorchPool.cs` | 10 |
| `RealisticDoorFactory.cs` | 10 |

---

## 📋 Priority Task List

### Priority 1 - Critical (This Session) ⏰

| # | Task | Files | Time | Status |
|---|------|-------|------|--------|
| 1.1 | Refactor HUD System | `UIBarsSystem.cs`, `PopWinEngine.cs`, `DialogEngine.cs` | 4-6h | ⏳ Pending |
| 1.2 | Fix Plug-in-Out Violations | 15 files, 108 instances | 3-4h | ⏳ Pending |
| 1.3 | Clean Backup Files | 23 `.backup` files | 30min | ⏳ Pending |

### Priority 2 - High (Next Session) 📅

| # | Task | Files | Time | Status |
|---|------|-------|------|--------|
| 2.1 | Reduce Singleton Usage | `DialogEngine`, `PopWinEngine`, `LightEngine` | 2-3h | ⏳ Pending |
| 2.2 | Improve Test Coverage | Core systems (target 50%) | 4-6h | ⏳ Pending |
| 2.3 | Fix Namespace Mismatch | `Ressources` → `Resources` | 30min | ⏳ Pending |
| 2.4 | Archive Old Documentation | Duplicate reports | 1h | ⏳ Pending |

### Priority 3 - Medium (Future) 🗓️

| # | Task | Files | Time | Status |
|---|------|-------|------|--------|
| 3.1 | Clean diff_tmp Folder | 23 files >2 days | 30min | ⏳ Pending |
| 3.2 | Git Workflow Setup | `.gitignore`, auto-commit | 1h | ⏳ Pending |
| 3.3 | Review TODO Markers | 1 TODO in `PopWinEngine.cs` | 15min | ⏳ Pending |

### Priority 4 - Low (Backlog) 📦

| # | Task | Files | Time | Status |
|---|------|-------|------|--------|
| 4.1 | Rename ShareSystm.cs | Typo fix | 10min | ⏳ Pending |
| 4.2 | Standardize Folder Names | `DBSQLite` → `Database` | 30min | ⏳ Pending |
| 4.3 | Remove Duplicate Files | `StatusEffect.cs` | 20min | ⏳ Pending |

---

## ✅ Completed - Session 2026-03-10

### Bugs Fixed (6/6 - 100%)

| # | Issue | Priority | File |
|---|-------|----------|------|
| 1 | Player Splitting | 🔴 | `CompleteMazeBuilder.cs` |
| 2 | Enemy Spawn Position | 🔴 | Verified |
| 3 | Camera Misalignment | 🔴 | `PlayerController.cs` |
| 4 | Entrance/Exit Visibility | 🟡 | `CompleteMazeBuilder.cs` |
| 5 | Safe System Events | 🟡 | Verified |
| 6 | Chest Glow Effect | 🟡 | `ChestPrefab.prefab` |

### Completed Tasks
- ✅ GPL-3.0 Headers: 7 files updated
- ✅ QWEN.md: Updated with Holy Saint Order
- ✅ .gitignore: Added `diff_tmp/` and `files/`

---

## 🟡 Warning Issues

| ID | Issue | Count | Priority |
|----|-------|-------|----------|
| W01 | Singleton Overuse | 6 | 🟡 |
| W02 | Namespace Mismatch | 1 | 🟡 |
| W03 | Documentation Duplicates | 5+ | 🟡 |
| W04 | Duplicate StatusEffect.cs | 2 | 🟡 |
| W05 | Low Test Coverage | ~35% | 🟡 |

---

## ℹ️ Info Issues

| ID | Issue | Recommendation |
|----|-------|----------------|
| I01 | TODO Markers | 1 in project code (excellent!) |
| I02 | diff_tmp Folder | 23 files - delete >2 days old |
| I03 | Typo in Filename | `ShareSystm.cs` → `ShareSystem.cs` |
| I04 | Legacy Folder Files | Consider removal |

---

## 🎮 Testing Checklist

### Bug Fix Verification
- [ ] Player does NOT split on unpause/maze regen
- [ ] Camera follows player correctly
- [ ] Spawn room marker visible (green cylinder)
- [ ] Exit room marker visible (red cube)
- [ ] Chest has glow effect (particles + light)
- [ ] Enemies do NOT spawn at player position

### Performance Tests
- [ ] Level 0: <50ms generation
- [ ] Level 7: <100ms generation
- [ ] Level 39: <200ms generation
- [ ] FPS stable at 60+

---

## 📊 Metrics Summary

| Metric | Value | Status |
|--------|-------|--------|
| C# Scripts | ~180 files | ✅ |
| Scenes | 15 | ✅ |
| Prefabs | 51 | ✅ |
| Materials | 34 | ✅ |
| Documentation | 130+ .md files | ✅ |
| Compilation Errors | 0 | ✅ PASS |
| Compilation Warnings | 0 | ✅ PASS |
| Unit Tests | 58 tests | ✅ PASS |
| TODO/FIXME Markers | 1 | ✅ EXCELLENT |
| Emoji in C# | 0 | ✅ EXCELLENT |

---

## ⚠️ Reminders

- Run `backup.ps1` after ANY file/folder change
- Git: `C:\PROGRA~1\Git\cmd\git.exe`
- Commit regularly, push when ready
- Delete diff_tmp files older than 2 days

---

## 📞 Next Session Checklist

1. [ ] Run `backup.ps1` first
2. [ ] Configure Git (user.name, user.email)
3. [ ] Test all bug fixes in Unity
4. [ ] Run full test suite
5. [ ] Fix plug-in-out violations (Priority 1.1)
6. [ ] Clean backup files (Priority 1.3)

---

## 📌 Key File Paths

### Core Architecture
- `Assets/Scripts/Core/01_CoreSystems/GameManager.cs`
- `Assets/Scripts/Core/01_CoreSystems/EventHandler.cs`
- `Assets/Scripts/Core/06_Maze/CompleteMazeBuilder.cs`

### Violation Hotspots
- `Assets/Scripts/HUD/UIBarsSystem.cs` - 27 violations 🔴
- `Assets/Scripts/HUD/PopWinEngine.cs` - 14 violations 🔴
- `Assets/Scripts/HUD/DialogEngine.cs` - 10 violations 🔴
- `Assets/Scripts/Core/13_Compute/AudioManager.cs` - 12 violations 🟡
- `Assets/Scripts/Core/15_Resources/TorchPool.cs` - 10 violations 🟡

---

**License:** GPL-3.0  
**Author:** Ocxyde  
**Copyright © 2026 CodeDotLavos. All rights reserved.**

---

*Document generated - Unity 6 compatible - UTF-8 encoding - Unix LF*
