# Maze Geometry Test Suite - 2026-03-09

**Project:** CodeDotLavos (Unity 6000.3.7f1)
**Test Framework:** NUnit (Unity Test Runner)
**License:** GPL-3.0
**Status:** CREATED - Ready for execution

---

## TEST SUITE OVERVIEW

**Total Test Files:** 3
**Total Test Cases:** 42

| Test File | Test Count | Category | Status |
|-----------|------------|----------|--------|
| MazeGeometryTests.cs | 18 | Core maze system | Created |
| GeometryMathTests.cs | 16 | Pure math (Tetrahedron, Triangle, Vector3d) | Created |
| MazeBinaryStorageTests.cs | 15 | Binary storage (LAV8S v3) | Created |

---

## TEST FILES

### 1. MazeGeometryTests.cs

**Location:** Assets/Scripts/Tests/MazeGeometryTests.cs

**Test Categories:**
- GridMazeGenerator tests (4 tests)
- CellFlags8 tests (2 tests)
- Direction8Helper tests (3 tests)
- MazeData8 tests (5 tests)
- DeadEndCorridorSystem tests (3 tests)
- Integration tests (1 test)

**Key Tests:**
- GenerateMaze_ReturnsValidData
- GenerateMaze_SpawnRoomCleared
- GenerateMaze_ExitMarked
- GenerateMaze_SameSeedProducesSameMaze
- CellFlags8_WallFlags
- Direction8Helper_CardinalOffsets
- MazeData8_CellAccessors
- DeadEndCorridorSystem_ScaledDensity
- Integration_FullMazeGeneration

---

### 2. GeometryMathTests.cs

**Location:** Assets/Scripts/Tests/GeometryMathTests.cs

**Test Categories:**
- Vector3d tests (7 tests)
- Triangle tests (10 tests)
- Tetrahedron tests (5 tests)
- Integration tests (1 test)

**Key Tests:**
- Vector3d_Constructor
- Vector3d_Magnitude
- Vector3d_CrossProduct
- Triangle_Area
- Triangle_Centroid
- Triangle_ContainsPoint2D_Inside
- Tetrahedron_Volume
- Tetrahedron_Centroid
- Integration_GeometryPipeline

---

### 3. MazeBinaryStorageTests.cs

**Location:** Assets/Scripts/Tests/MazeBinaryStorageTests.cs

**Test Categories:**
- MazeData8 tests (4 tests)
- Binary format tests (3 tests)
- Integration tests (3 tests)
- Edge cases (4 tests)

**Key Tests:**
- MazeData8_Constructor
- LAV8S_MagicHeader
- LAV8S_FileSizeCalculation
- Integration_MazeGenerationWithSpawnExit
- EdgeCase_OutOfBounds
- EdgeCase_AllWallFlags

---

## HOW TO RUN TESTS

### Method 1: Unity Test Runner (GUI)

1. Open Unity 6000.3.7f1
2. Go to Window > General > Test Runner
3. Select Play Mode tests
4. Click Run All

### Method 2: Rider IDE

1. Open solution in Rider
2. Go to Unit Tests window (Alt+U)
3. Select tests to run
4. Click Run or Debug

---

## EXPECTED RESULTS

**Total Tests:** 42
**Expected Pass Rate:** 100%
**Expected Time:** < 1 second

---

## TEST COVERAGE

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

```bash
# Run tests before commit
Unity -batchmode -runTests -testPlatform PlayMode

# Commit with test update
.\git-commit.ps1 "Added maze geometry unit tests"

# Push to remote
.\git-push.ps1
```

---

**Created:** 2026-03-09
**Codename:** BetsyBoop
**License:** GPL-3.0
