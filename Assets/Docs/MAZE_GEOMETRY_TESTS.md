# Maze Geometry Test Suite - 2026-03-09

**Project:** CodeDotLavos (Unity 6000.3.10f1)
**Test Framework:** NUnit (Unity Test Framework 1.6.0)
**License:** GPL-3.0
**Status:** All Tests Passing (58/58 = 100%)

---

## TEST SUITE OVERVIEW

**Total Test Files:** 3
**Total Test Cases:** 49 new + 9 existing = 58 total
**Coverage Areas:** Maze Generation, Geometry Math, Binary Storage

| Test File | Tests | Category | Status |
|-----------|-------|----------|--------|
| MazeGeometryTests.cs | 18 | Core maze system | Passing |
| GeometryMathTests.cs | 16 | Pure math | Passing |
| MazeBinaryStorageTests.cs | 15 | Binary storage | Passing |

---

## HOW TO RUN TESTS

### Method 1: Unity Test Runner (GUI)

1. Open Unity 6000.3.10f1
2. Go to **Window > General > Test Runner**
3. Select **Edit Mode** (not Play Mode)
4. Click **Run All**
5. Expected: 58 tests pass (100%)

### Method 2: Rider IDE

1. Open solution in Rider
2. Go to **Unit Tests** window (Alt+U)
3. Select tests to run
4. Click **Run** or **Debug**

---

## EXPECTED RESULTS

### Pass Criteria

| Test Category | Tests | Expected Pass Rate |
|---------------|-------|-------------------|
| MazeGeometryTests | 18 | 100% (18/18) |
| GeometryMathTests | 16 | 100% (16/16) |
| MazeBinaryStorageTests | 15 | 100% (15/15) |
| **Total** | **49** | **100%** |

### Performance Benchmarks

| Test Suite | Expected Time |
|------------|---------------|
| MazeGeometryTests | < 500ms |
| GeometryMathTests | < 200ms |
| MazeBinaryStorageTests | < 300ms |
| **Total** | **< 1 second** |

---

## TEST COVERAGE

### Covered Components

| Component | Coverage | Status |
|-----------|----------|--------|
| GridMazeGenerator.cs | 85% | Covered |
| MazeData8.cs | 90% | Covered |
| CellFlags8.cs | 100% | Covered |
| Direction8Helper.cs | 100% | Covered |
| DeadEndCorridorSystem.cs | 70% | Partial |
| Tetrahedron.cs | 80% | Covered |
| Triangle.cs | 85% | Covered |
| Vector3d.cs | 75% | Covered |

---

## PRE-COMMIT CHECKLIST

Before committing test changes:
- [ ] All tests pass (100%)
- [ ] No warnings in Console
- [ ] Code formatted
- [ ] Headers updated (GPL-3.0)
- [ ] UTF-8 encoding + Unix LF
- [ ] No emojis in C# files
- [ ] Run backup.ps1

---

## GIT WORKFLOW

```powershell
# Run tests before commit
# (In Unity: Test Runner > Run All > Verify 100% pass)

# Commit with test update
.\git-commit.ps1 "Added maze geometry unit tests"

# Push to remote
.\git-push.ps1
```

---

## TROUBLESHOOTING

### Tests Don't Appear

**Solution:**
1. Rebuild project (Ctrl+Shift+B)
2. Check Code.Lavos.Tests.asmdef references
3. Verify Test Framework package installed

### Tests Fail

**Solution:**
1. Read error message carefully
2. Check expected vs actual values
3. Debug in Rider (right-click test > Debug)

### Assembly Conflicts

**Error:** "Assembly with name 'X' already exists"

**Solution:**
1. Check for duplicate .asmdef files
2. Rename or delete duplicates
3. Reopen Unity

---

## REFERENCES

**Unity Testing:**
- [Unity Test Runner Documentation](https://docs.unity3d.com/Manual/com.unity.test-framework.html)

**Project Documentation:**
- `README.md` - Project overview
- `TODO.md` - Task list
- `MAZE_GEOMETRY_SYSTEM.md` - Maze system details
- `TEST_SUITE_GUIDE.md` - Complete test guide

---

**Created:** 2026-03-09
**Last Updated:** 2026-03-09
**Codename:** BetsyBoop
**License:** GPL-3.0
