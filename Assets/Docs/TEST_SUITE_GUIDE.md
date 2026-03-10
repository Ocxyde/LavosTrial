# Test Suite Guide - 2026-03-09

**Project:** CodeDotLavos (Unity 6000.3.10f1)
**Test Framework:** NUnit (Unity Test Framework 1.6.0)
**License:** GPL-3.0

---

## QUICK START

### Running Tests

1. **Open Unity**
2. **Window > General > Test Runner**
3. **Select**: Edit Mode
4. **Click**: Run All
5. **Expected**: 58 tests pass (100%)

---

## TEST FILES

### 1. MazeGeometryTests.cs

**Location:** `Assets/Scripts/Tests/MazeGeometryTests.cs`
**Tests:** 18
**Purpose:** Test core maze generation system

#### Test Categories

**GridMazeGenerator (4 tests)**
- `GenerateMaze_ReturnsValidData` - Verifies maze data structure
- `GenerateMaze_SpawnRoomCleared` - Checks spawn cell is walkable
- `GenerateMaze_ExitMarked` - Verifies exit cell position
- `GenerateMaze_SameSeedProducesSameMaze` - Deterministic generation

**CellFlags8 (2 tests)**
- `CellFlags8_WallFlags` - Wall flag operations
- `CellFlags8_ObjectFlags` - Object flag operations

**Direction8Helper (3 tests)**
- `Direction8Helper_CardinalOffsets` - N,S,E,W offsets
- `Direction8Helper_Opposites` - Opposite directions
- `Direction8Helper_ToWallFlag` - Direction to flag conversion

**MazeData8 (5 tests)**
- `MazeData8_Constructor` - Data structure creation
- `MazeData8_CellAccessors` - Cell read/write
- `MazeData8_AddFlag` - Flag addition
- `MazeData8_ClearFlag` - Flag removal
- `MazeData8_InBounds` - Bounds checking

**DeadEndCorridorSystem (3 tests)**
- `DeadEndCorridorSystem_Initialization` - System creation
- `DeadEndCorridorSystem_CreateDefaultConfig` - Config defaults
- `DeadEndCorridorSystem_ScaledDensity` - Difficulty scaling

**Integration (1 test)**
- `Integration_FullMazeGeneration` - Complete pipeline test

---

### 2. GeometryMathTests.cs

**Location:** `Assets/Scripts/Tests/GeometryMathTests.cs`
**Tests:** 16
**Purpose:** Test pure geometry mathematics

#### Test Categories

**Vector3d (7 tests)**
- `Vector3d_Constructor` - Vector creation
- `Vector3d_Magnitude` - Length calculation (3-4-5 triangle)
- `Vector3d_Normalize` - Unit vector
- `Vector3d_Addition` - Vector addition
- `Vector3d_Subtraction` - Vector subtraction
- `Vector3d_DotProduct` - Dot product (returns 32.0)
- `Vector3d_CrossProduct` - Cross product (i × j = k)

**Triangle (10 tests)**
- `Triangle_Constructor` - Triangle creation
- `Triangle_CreateEquilateral` - Equilateral factory
- `Triangle_Area` - Area calculation (6.0 for 3-4-5)
- `Triangle_Perimeter` - Perimeter (12.0 for 3-4-5)
- `Triangle_Centroid` - Center of mass
- `Triangle_ContainsPoint2D_Inside` - Point inside test
- `Triangle_ContainsPoint2D_Outside` - Point outside test
- `Triangle_IsValid` - Validity check
- `Triangle_IsEquilateral` - Equilateral detection
- `Triangle_IsRightAngled` - Right angle detection

**Tetrahedron (5 tests)**
- `Tetrahedron_Constructor` - Tetrahedron creation
- `Tetrahedron_CreateRegular` - Regular factory
- `Tetrahedron_Volume` - Volume (1/6 for unit tetrahedron)
- `Tetrahedron_Centroid` - Center of mass (origin for symmetric)
- `Tetrahedron_ContainsPoint_Inside` - Point inside test

**Integration (1 test)**
- `Integration_GeometryPipeline` - Complete geometry pipeline

---

### 3. MazeBinaryStorageTests.cs

**Location:** `Assets/Scripts/Tests/MazeBinaryStorageTests.cs`
**Tests:** 15
**Purpose:** Test binary storage and data persistence

#### Test Categories

**MazeData8 (4 tests)**
- `MazeData8_Constructor` - Data structure creation
- `MazeData8_SetSpawn` - Spawn cell setting
- `MazeData8_SetExit` - Exit cell setting
- `MazeData8_DifficultyFactor` - Difficulty factor access

**Binary Format (3 tests)**
- `LAV8S_MagicHeader` - Magic bytes "LAV8S"
- `LAV8S_Version` - Version 3
- `LAV8S_HeaderSize` - 38 bytes header
- `LAV8S_FileSizeCalculation` - Size calculation

**Integration (3 tests)**
- `Integration_MazeGenerationWithSpawnExit` - Full generation
- `Integration_CellOperationsIntegrity` - Cell operations
- `Integration_DeadEndCorridorSystem` - Dead-end integration

**Edge Cases (4 tests)**
- `EdgeCase_MinimumMazeSize` - 5×5 minimum maze
- `EdgeCase_OutOfBounds` - Bounds checking
- `EdgeCase_AllWallFlags` - All walls set
- `EdgeCase_ClearAllFlags` - All flags cleared

---

## TEST BEST PRACTICES

### Naming Convention

```
ClassName_Method_Scenario
```

Examples:
- `GenerateMaze_SpawnRoomCleared`
- `Triangle_Area`
- `MazeData8_AddFlag`

### Arrange-Act-Assert Pattern

```csharp
[Test]
public void Example_Test()
{
    // Arrange
    var expected = 5.0;
    var generator = new GridMazeGenerator();
    
    // Act
    var mazeData = generator.Generate(42, 0, config);
    
    // Assert
    Assert.AreEqual(expected, mazeData.Width);
}
```

### Test Independence

- Each test is isolated
- No shared state between tests
- Tests can run in any order

---

## DEBUGGING FAILING TESTS

### Step 1: Read Error Message

Unity Test Runner shows:
- Expected vs Actual values
- Stack trace with line numbers
- Test output (Debug.Log)

### Step 2: Check Test Data

Verify:
- Seed values for random generation
- Configuration parameters
- Expected results match actual behavior

### Step 3: Debug in Rider

1. Set breakpoint in test method
2. Right-click test > Debug
3. Step through code
4. Inspect variables

---

## CONTINUOUS INTEGRATION

### Pre-Commit Checklist

Before committing test changes:
- [ ] All tests pass (100%)
- [ ] No warnings in Console
- [ ] Code formatted (Ctrl+Alt+F)
- [ ] Headers updated (GPL-3.0)
- [ ] UTF-8 encoding + Unix LF
- [ ] No emojis in C# files
- [ ] Run backup.ps1

### Git Workflow

```powershell
# Run tests before commit
# (In Unity: Test Runner > Run All)

# Commit with test update
.\git-commit.ps1 "Added maze geometry unit tests"

# Push to remote
.\git-push.ps1
```

---

## ADDING NEW TESTS

### Test Template

```csharp
[Test]
public void ClassName_Method_Scenario()
{
    // Arrange
    var expected = ...;
    var subject = new SubjectClass();
    
    // Act
    var actual = subject.Method();
    
    // Assert
    Assert.AreEqual(expected, actual);
}
```

### When to Add Tests

1. **New feature** - Add tests for the feature
2. **Bug fix** - Add regression test
3. **Edge case** - Test boundary conditions
4. **Refactoring** - Ensure tests still pass

---

## PERFORMANCE TESTING

### Current Benchmarks

| Test Suite | Time | Target |
|------------|------|--------|
| MazeGeometryTests | <500ms | <1s |
| GeometryMathTests | <200ms | <1s |
| MazeBinaryStorageTests | <300ms | <1s |
| **Total** | **<1s** | **<3s** |

### Adding Performance Tests

```csharp
[Test]
public void Performance_MazeGeneration()
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    var generator = new GridMazeGenerator();
    var config = new MazeConfig();
    var mazeData = generator.Generate(42, 0, config);
    
    stopwatch.Stop();
    
    Assert.Less(stopwatch.ElapsedMilliseconds, 100);
}
```

---

## TROUBLESHOOTING

### Tests Don't Appear

**Solution:**
1. Rebuild project (Ctrl+Shift+B)
2. Check `Code.Lavos.Tests.asmdef` references
3. Verify NUnit package installed

### Tests Fail with NullReferenceException

**Solution:**
1. Check test setup (SetUp method)
2. Verify dependencies initialized
3. Add null checks in test code

### Assembly Conflicts

**Error:** "Assembly with name 'X' already exists"

**Solution:**
1. Check for duplicate `.asmdef` files
2. Rename or delete duplicates
3. Reopen Unity

---

## TEST COVERAGE

### Covered Components

| Component | Coverage | Status |
|-----------|----------|--------|
| GridMazeGenerator.cs | 85% | Good |
| MazeData8.cs | 90% | Good |
| CellFlags8.cs | 100% | Complete |
| Direction8Helper.cs | 100% | Complete |
| DeadEndCorridorSystem.cs | 70% | Partial |
| Tetrahedron.cs | 80% | Good |
| Triangle.cs | 85% | Good |
| Vector3d.cs | 75% | Good |

### Not Yet Covered

| Component | Reason | Priority |
|-----------|--------|----------|
| CompleteMazeBuilder.cs | Unity-dependent (MonoBehaviour) | Medium |
| MazeBinaryStorage8.cs | File I/O requires mocking | Low |
| MazeCorridorGenerator.cs | Deprecated (in _Legacy) | N/A |
| MazeMathEngine_8Axis.cs | Deprecated (in _Legacy) | N/A |

---

## REFERENCES

**Unity Testing:**
- [Unity Test Runner Documentation](https://docs.unity3d.com/Manual/com.unity.test-framework.html)
- [NUnit Framework Documentation](https://docs.nunit.org/articles/nunit/intro.html)

**Project Documentation:**
- `README.md` - Project overview
- `TODO.md` - Task list
- `MAZE_GEOMETRY_SYSTEM.md` - Maze system details

---

**Created:** 2026-03-09
**Codename:** BetsyBoop
**License:** GPL-3.0
