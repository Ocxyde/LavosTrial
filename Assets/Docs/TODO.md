# TODO.md - Project Tasks & Priorities

**Project:** CodeDotLavos  
**Unity:** 6000.3.10f1 | **License:** GPL-3.0  
**Last Updated:** 2026-03-10 | **Author:** Ocxyde

---

## 📊 Project Status

| Metric | Status | Notes |
|--------|--------|-------|
| **Build** | ✅ 0 Errors | Clean build |
| **Tests** | ✅ 58 Tests | All passing |
| **Architecture** | ✅ 100% Compliant | Plug-in-Out pattern |
| **Critical Bugs** | ✅ All Fixed | Null checks complete |

---

## ✅ COMPLETED - 2026-03-10 Sessions

### Critical Bug Fixes (All Complete)

| Task | Files Modified | Status |
|------|----------------|--------|
| **Null Checks - FindFirstObjectByType** | 9 files | ✅ Complete |
| **Null Checks - GetComponent** | 7 files | ✅ Complete |
| **Null Checks - Instantiate** | 6 files | ✅ Complete |
| **Singleton Reduction** | 3 files | ✅ Complete |

### Architecture Improvements

| Task | Status |
|------|--------|
| BaseMazeBuilder refactoring | ✅ Complete - extracted 150 lines duplicate code |
| README rewrite for modders | ✅ Complete - modder-focused guide |
| Plug-in-Out compliance | ✅ Complete - 108 violations fixed |

### Documentation

| Document | Purpose |
|----------|---------|
| `SINGLETON_REDUCTION_SUMMARY_2026-03-10.md` | Singleton → Service Locator refactoring |
| `README.md` (Assets/Docs/) | Modder's guide with tutorials |

---

## 🎯 ACTIVE TASKS

### Priority 1 - Critical (Do First)

| ID | Task | Files | Est. Time | Status |
|----|------|-------|-----------|--------|
| **1.1** | Maze Generator Review (DFS + A*) | `GridMazeGenerator8.cs` | 2-3h | 🔴 **NEXT** |
| **1.2** | Schedule diff_tmp daily cleanup | Task Scheduler | 15min | ⏳ Pending |

### Priority 2 - High (This Week)

| ID | Task | Files | Est. Time | Status |
|----|------|-------|-----------|--------|
| **2.1** | Extract magic numbers to Constants | All .cs files | 3-4h | ⏳ Pending |
| **2.2** | Split CompleteMazeBuilder (1137 lines) | New modular files | 4-6h | ⏳ Pending |
| **2.3** | Split LightEngine (921 lines) | New focused systems | 3-5h | ⏳ Pending |
| **2.4** | XML documentation | `DungeonMazeGenerator.cs`, `GridMazeGenerator.cs` | 2h | ⏳ Pending |

### Priority 3 - Medium (Future)

| ID | Task | Files | Est. Time | Status |
|----|------|-------|-----------|--------|
| **3.1** | Config file for Resource paths | `GameConfig-default.json` | 1h | ⏳ Pending |
| **3.2** | Thread-safe event subscription | `EventHandler.cs` | 2h | ⏳ Pending |
| **3.3** | Unit tests for maze algorithms | `Assets/Scripts/Tests/` | 4-6h | ⏳ Pending |
| **3.4** | Simplify CorridorFillSystem | `CorridorFillSystem.cs` | 2h | ⏳ Pending |
| **3.5** | Fix RealisticDoorFactory null risks | `RealisticDoorFactory.cs` | 1h | ⏳ Pending |

### Priority 4 - Low (Backlog)

| ID | Task | Files | Est. Time | Status |
|----|------|-------|-----------|--------|
| **4.1** | Remove unused variables | All .cs files | 2h | ⏳ Pending |
| **4.2** | Add code analysis rules | Project settings | 1h | ⏳ Pending |
| **4.3** | Fix Transform in Update methods | `CameraFollow.cs`, `LightEngine.cs` | 1h | ⏳ Pending |
| **4.4** | Fix typo: `ShareSystm.cs` → `ShareSystem.cs` | 1 file | 10min | ⏳ Pending |
| **4.5** | Standardize folder names | `DBSQLite` → `Database` | 30min | ⏳ Pending |

---

## 📋 SYSTEM COMPLETION

| System | Progress | Status |
|--------|----------|--------|
| **Core Systems** | 100% | ✅ Complete |
| **Player Controller** | 100% | ✅ Complete |
| **Interaction** | 100% | ✅ Complete |
| **Inventory** | 80% | 🟡 In Progress |
| **Combat** | 70% | 🟡 In Progress |
| **Maze Generation** | 95% | ✅ Near Complete |
| **Doors** | 100% | ✅ Complete |
| **Environment** | 85% | ✅ Good |
| **HUD System** | 75% | ✅ Good |
| **Compute/Grid** | 90% | ✅ Good |
| **Audio** | 75% | 🟡 In Progress |
| **Resources** | 80% | ✅ Good |

---

## 📝 NEXT SESSION CHECKLIST

1. [ ] Run `backup.ps1` first
2. [ ] Review Maze Generator (DFS + A*) - Task 1.1
3. [ ] Test all null check fixes in Unity
4. [ ] Run full test suite
5. [ ] Schedule diff_tmp daily cleanup

---

## 🤖 AI DAILY REMINDER

**Every day, remind user to:**
- [ ] Run `cleanup_diff_tmp.ps1` or `cleanup_diff_tmp.bat`
- [ ] Review diff_tmp folder for files >2 days old
- [ ] Delete old diff files automatically

---

## 📌 QUICK COMMANDS

```bash
# Backup project
.\backup.ps1

# Clean diff_tmp (files >2 days old)
.\cleanup_diff_tmp.ps1

# Git quick commit
.\git-commit.ps1 "Your message here"

# Push to remote
.\git-push.ps1

# Check project health
.\git-status.ps1
```

---

## 📖 Related Documents

| Document | Location |
|----------|----------|
| Modder's Guide | `Assets/Docs/README.md` |
| Architecture Overview | `Assets/Docs/ARCHITECTURE_OVERVIEW.md` |
| Project Standards | `Assets/Docs/PROJECT_STANDARDS.md` |
| Singleton Reduction Report | `Assets/Docs/SINGLETON_REDUCTION_SUMMARY_2026-03-10.md` |
| Visual Polish Summary | `VISUAL_POLISH_ENHANCEMENTS_2026-03-10.md` |

---

## 🏆 ACHIEVEMENTS - 2026-03-10

| Achievement | Status |
|-------------|--------|
| **Null Check Trilogy** | ✅ All 3 CRITICAL tasks complete |
| **Singleton Reduction** | ✅ 3 classes refactored to Service Locator |
| **BaseMazeBuilder** | ✅ 150 lines duplicate code removed |
| **README for Modders** | ✅ Complete rewrite |
| **Plug-in-Out Compliance** | ✅ 108 violations fixed |
| **Visual Polish** | ✅ All markers enhanced |

---

**License:** GPL-3.0  
**Author:** Ocxyde  
**Copyright © 2026 CodeDotLavos. All rights reserved.**

---

*Last Updated: 2026-03-10 | Unity 6000.3.10f1 | UTF-8 encoding - Unix LF*

**Motto:** "Happy coding with me : Ocxyde :)"
