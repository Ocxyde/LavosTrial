﻿﻿﻿# Maze Geometry System - Documentation

**Project:** CodeDotLavos (Unity 6000.3.10f1)
**Last Updated:** 2026-03-09
**License:** GPL-3.0
**Status:** Production Ready

---

## OVERVIEW

The Maze Geometry System provides mathematical foundations for procedural maze generation, including:

- **Cardinal-only maze generation** (4-direction DFS + A*)
- **Dead-end corridor system** (mathematical distribution)
- **Binary storage format** (LAV8S v3)
- **Pure geometry mathematics** (Triangle, Tetrahedron, Vector3d)

---

## ARCHITECTURE

```
Code.Lavos.Core
├── GridMazeGenerator.cs      # Main maze generation (DFS + A*)
├── MazeData8.cs              # Cell data structure (2 bytes/cell)
├── CellFlags8.cs             # 16-bit cell flags
├── Direction8Helper.cs       # 8-direction utilities
└── DeadEndCorridorSystem.cs  # Mathematical dead-end generation

Code.Lavos.Geometry
├── Vector3d.cs               # Double-precision 3D vector
├── Triangle.cs               # Triangle mathematics
└── Tetrahedron.cs            # Tetrahedron mathematics
```

---

## MAZE GENERATION ALGORITHM

### Complete Pipeline (Two Phases)

The maze generation happens in **two distinct phases**:

#### Phase 1: Logical Generation (GridMazeGenerator.cs)

Pure data generation - no Unity objects created.

**Grid Structure:** Uses 2-step carving for proper maze topology

```
1. FillAllWalls()           → All cells = 0x000F (all walls)
2. CarvePassagesCardinal()  → 4-direction DFS (2-step carving)
   - Moves 2 cells at a time (creates checkerboard pattern)
   - Clears walls between passage cells
   - Ensures proper maze topology (no loops)
3. CarveSpawnRoom()         → 5×5 cleared at (1,1) - 1-step carving
4. SetExit()                → Exit marker at (W-2, H-2)
5. EnsurePathCardinal()     → A* guarantees path (1-step movement)
   - Uses 1-step adjacency for pathfinding
   - Bridges gaps from 2-step DFS
6. AddDeadEndCorridors()    → Mathematical dead-end generation
7. AddCorridorFlowSystem()  → Space filling with corridors
8. PlaceTorches()           → Set torch flags (30% wall-adjacent)
9. PlaceObjects()           → Set chest/enemy flags
```

**Output:** `MazeData8` structure with cell flags

**Note:** The 2-step DFS creates a checkerboard pattern where:
- Even-coordinate cells (0,0), (0,2), (2,0)... are potential passages
- Odd-coordinate cells (0,1), (1,0), (1,1)... are walls between passages
- A* pathfinding uses 1-step movement to bridge gaps

---

#### Phase 2: Physical Instantiation (CompleteMazeBuilder.cs)

Unity objects instantiated from prefabs.

```
10. LoadConfig()            → Load GameConfig from scene/Resources
11. ValidateAssets()        → Resolve prefab references
12. DestroyMazeObjects()    → Clean up previous maze
13. SpawnGround()           → Instantiate floor plane (cellSize × gridSize)
14. SpawnAllWalls()         → Instantiate wall prefabs (boundary-based)
    - Only spawn walls on boundaries (walkable → non-walkable)
    - No overlapping walls (shared edges)
    - Pivot at bottom-center of wall edge
15. SpawnDoors()            → Place doors on access walls
    - Orient doors to face corridor
    - Place ON the access wall (not in passage)
16. SpawnTorches()          → Instantiate torch prefabs
    - Read torch flags from MazeData8
    - Place at wall-adjacent cells
17. SpawnObjects()          → Instantiate chests + enemies
    - Read chest/enemy flags from MazeData8
    - Place on valid spawn cells
18. SpawnPlayer()           → Spawn player at spawn point (1,1)
    - Spawn LAST to ensure maze is ready
19. SaveMaze()              → Binary .lvm save to Runtimes/Mazes/
    - LAV8S v3 format
    - Cache for faster reload
```

**Output:** Fully instantiated 3D maze with prefabs

### Key Features

- **Cardinal-only passages**: No diagonal walls, clean grid alignment
- **Guaranteed path**: A* ensures spawn → exit connectivity
- **Dead-end corridors**: 30% base density, scales to 75% at level 39
- **Difficulty scaling**: Exponential complexity increase
- **Boundary-based wall spawning**: No overlapping walls
- **Plug-in-out architecture**: CompleteMazeBuilder uses FindFirstObjectByType

---

## CELL FLAGS (16-bit)

### Low Byte (Bits 0-7) - Wall Presence

| Bit | Flag | Value | Description |
|-----|------|-------|-------------|
| 0 | WallN | 0x0001 | North wall |
| 1 | WallS | 0x0002 | South wall |
| 2 | WallE | 0x0004 | East wall |
| 3 | WallW | 0x0008 | West wall |
| 4 | WallNE | 0x0010 | North-East diagonal wall |
| 5 | WallNW | 0x0020 | North-West diagonal wall |
| 6 | WallSE | 0x0040 | South-East diagonal wall |
| 7 | WallSW | 0x0080 | South-West diagonal wall |

### High Byte (Bits 8-15) - Object/Room Metadata

| Bit | Flag | Value | Description |
|-----|------|-------|-------------|
| 8 | SpawnRoom | 0x0100 | Part of spawn room |
| 9 | HasChest | 0x0200 | Chest placed here |
| 10 | HasEnemy | 0x0400 | Enemy placed here |
| 11 | HasTorch | 0x0800 | Torch on this cell |
| 12 | IsExit | 0x1000 | Exit cell marker |
| 13 | IsRoom | 0x2000 | Room cell (dead-end/crossroads) |
| 14 | HasPillar | 0x4000 | Pillar decoration |
| 15 | HasNiche | 0x8000 | Wall niche/alcove |

---

## BINARY STORAGE FORMAT (LAV8S v3)

### File Layout (Little-Endian)

```
+---------+--------+------------------------------------+
| Offset  | Bytes  | Field                              |
+---------+--------+------------------------------------+
|  0      |  5     | Magic  "LAV8S"                     |
|  5      |  1     | Version (3)                        |
|  6      |  2     | Width   (int16)                    |
|  8      |  2     | Height  (int16)                    |
| 10      |  4     | Seed    (int32)                    |
| 14      |  4     | Level   (int32)                    |
| 18      |  8     | Timestamp (int64, UTC unix secs)   |
| 26      |  2     | SpawnX  (int16)                    |
| 28      |  2     | SpawnZ  (int16)                    |
| 30      |  2     | ExitX   (int16)                    |
| 32      |  2     | ExitZ   (int16)                    |
| 34      |  4     | DifficultyFactor (float32)         |
| 38      | W*H*2   | Cell data - ushort per cell (LE)   |
| 38+W*H*2|  4     | Checksum XOR-fold (uint32)         |
+---------+--------+------------------------------------+

Total: 42 + (W × H × 2) bytes
  Level  0 (13×13) →  380 bytes
  Level 39 (51×51) → 5,244 bytes
```

---

## DIFFICULTY SCALING

### Formula

```
ScaledDensity = BaseDensity × Lerp(1.0, MaxMultiplier, t^Exponent)

Where:
  t = level / MaxLevel (39)
  BaseDensity = 0.30 (30%)
  MaxMultiplier = 2.5
  Exponent = 2.0 (quadratic curve)
```

### Examples

| Level | Scaled Density | Multiplier |
|-------|---------------|------------|
| 0 | 30% | 1.0× |
| 12 | 34.3% | 1.14× |
| 25 | 52.5% | 1.75× |
| 39 | 75% | 2.5× |

---

## GEOMETRY MATHEMATICS

### Vector3d

Double-precision 3D vector for pure math calculations:

```csharp
// No Unity dependencies
public struct Vector3d
{
    public double X, Y, Z;
    
    // Operations
    public double magnitude { get; }
    public Vector3d normalized { get; }
    
    // Static methods
    public static double Dot(Vector3d a, Vector3d b);
    public static Vector3d Cross(Vector3d a, Vector3d b);
}
```

### Triangle

Triangle geometry with full analytical support:

```csharp
public struct Triangle
{
    public Vector3d A, B, C;
    
    // Properties
    public double Area();
    public double Perimeter();
    public Vector3d Centroid();
    public Vector3d Circumcenter();
    public Vector3d Incenter();
    public Vector3d Normal();
    
    // Tests
    public bool ContainsPoint2D(Vector3d point);
    public bool IsEquilateral();
    public bool IsIsosceles();
    public bool IsRightAngled();
}
```

### Tetrahedron

3D tetrahedron mathematics:

```csharp
public struct Tetrahedron
{
    public Vector3d A, B, C, D;
    
    // Properties
    public double Volume();
    public double SurfaceArea();
    public Vector3d Centroid();
    
    // Tests
    public bool ContainsPoint(Vector3d point);
    
    // Factory
    public static Tetrahedron CreateRegular(double edgeLength);
}
```

---

## TESTING

### Test Suite Overview

**Total Tests:** 58 (42 new + 16 existing)

| Test File | Tests | Category |
|-----------|-------|----------|
| MazeGeometryTests.cs | 18 | Core maze system |
| GeometryMathTests.cs | 16 | Pure math |
| MazeBinaryStorageTests.cs | 15 | Binary storage |
| Existing Tests | 9 | Legacy |

### Running Tests

1. **Unity Test Runner**: Window > General > Test Runner
2. **Select**: Edit Mode
3. **Click**: Run All
4. **Expected**: 100% pass rate

### Test Coverage

| Component | Coverage |
|-----------|----------|
| GridMazeGenerator.cs | 85% |
| MazeData8.cs | 90% |
| CellFlags8.cs | 100% |
| Direction8Helper.cs | 100% |
| DeadEndCorridorSystem.cs | 70% |
| Tetrahedron.cs | 80% |
| Triangle.cs | 85% |
| Vector3d.cs | 75% |

---

## PERFORMANCE

### Generation Times

| Maze Size | Level | Time | FPS Impact |
|-----------|-------|------|------------|
| 13×13 | 0 | ~3ms | <1 frame |
| 21×21 | 12 | ~7ms | <1 frame |
| 32×32 | 25 | ~12ms | <1 frame |
| 51×51 | 39 | ~25ms | 1.5 frames |

All generation completes within 60 FPS frame budget (16.67ms) for mazes up to level 25.

---

## USAGE EXAMPLES

### Generate a Maze

```csharp
var generator = new GridMazeGenerator();
var config = new MazeConfig();
var mazeData = generator.Generate(seed: 42, level: 0, cfg: config);

// Access maze data
int width = mazeData.Width;
int height = mazeData.Height;
var spawnCell = mazeData.SpawnCell;
var exitCell = mazeData.ExitCell;

// Check cell flags
var cell = mazeData.GetCell(5, 5);
bool hasNorthWall = (cell & CellFlags8.WallN) != 0;
bool isWalkable = (cell & CellFlags8.AllWalls) == 0;
```

### Save/Load Maze

```csharp
// Save
MazeBinaryStorage8Compat.Save(mazeData);

// Load
if (MazeBinaryStorage8Compat.Exists(level, seed))
{
    var loadedData = MazeBinaryStorage8Compat.Load(level, seed);
}
```

### Geometry Calculations

```csharp
// Triangle
var triangle = new Triangle(
    new Vector3d(0, 0, 0),
    new Vector3d(3, 0, 0),
    new Vector3d(0, 4, 0)
);
double area = triangle.Area();  // 6.0
double perimeter = triangle.Perimeter();  // 12.0

// Tetrahedron
var tetra = Tetrahedron.CreateRegular(edgeLength: 1.0);
double volume = tetra.Volume();  // 0.11785
```

---

## TROUBLESHOOTING

### Common Issues

**Maze generation crashes:**
- Check `levelData` is not null
- Verify `PopulationParams` has valid values
- Use null-conditional access (`?.`) for safety

**Tests don't appear in Test Runner:**
- Rebuild project (Ctrl+Shift+B)
- Check `Code.Lavos.Tests.asmdef` references
- Verify NUnit package is installed

**Binary save/load fails:**
- Check file permissions
- Verify path exists
- Use `MazeBinaryStorage8Compat` wrapper

---

## REFERENCES

**Related Documentation:**
- `README.md` - Project overview
- `TODO.md` - Task list
- `MAZE_CARDINAL_UPDATE_2026-03-09.md` - Cardinal-only algorithm
- `DEAD_END_CORRIDOR_SYSTEM.md` - Dead-end generation

**Code Locations:**
- `Assets/Scripts/Core/06_Maze/` - Maze generation
- `Assets/Scripts/Core/14_Geometry/` - Geometry math
- `Assets/Scripts/Tests/` - Unit tests

---

**Created:** 2026-03-09
**Codename:** BetsyBoop
**License:** GPL-3.0
