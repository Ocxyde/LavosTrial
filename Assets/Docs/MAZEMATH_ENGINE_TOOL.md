# MazeMath Engine 8-AXIS - Editor Tool Documentation

**Copyright (C) 2026 CodeDotLavos**  
**Licensed under GPL-3.0 - see COPYING for details**  
**Encoding: UTF-8 | Locale: en_US**  
**Unity 6 (6000.3.7f1) Compatible**

---

## Overview

The **MazeMath Engine Editor** is a Unity Editor tool that provides procedural maze generation using the `MazeMathEngine_8Axis` pure mathematics system. It offers a visual interface and menu commands for generating 8-axis mazes with guaranteed paths from start to exit.

---

## Features

- **Pure Mathematical Generation**: Uses `MazeMathEngine_8Axis` for all calculations
- **8-Axis Movement**: Supports N, S, E, W, NE, NW, SE, SW directions
- **DFS Carving**: Depth-first search algorithm for passage creation
- **A* Pathfinding**: Guarantees path from Start (A) to Exit (B)
- **Dead-end Detection**: Identifies and creates complex dead-end structures
- **ASCII Preview**: Visual console preview of generated mazes
- **Plug-in-Out Compliant**: Finds existing components, creates only if missing

---

## Installation

### Prerequisites

1. Unity 6 (6000.3.7f1) or compatible
2. Rider IDE (recommended)
3. New Input System
4. Code.Lavos core framework

### Files Required

```
Assets/
├── Scripts/
│   ├── Core/
│   │   └── 06_Maze/
│   │       ├── MazeMathEngine_8Axis.cs      # Core math engine
│   │       ├── CompleteMazeBuilder8.cs      # Runtime maze builder
│   │       ├── GameConfig.cs                # Configuration
│   │       └── DungeonMazeData.cs           # Maze data structure
│   └── Editor/
│       └── Maze/
│           └── MazeMathEngineEditor.cs      # Editor tool (this)
│   └── Tools/
│       └── Advanced/
│           └── MazeMathEngineEditor.cs      # (namespace location)
└── Docs/
    └── MAZEMATH_ENGINE_TOOL.md              # This documentation
```

---

## Usage

### Editor Window

1. **Menu**: `Tools → Lavos → MazeMath Engine Generator`
2. Opens interactive editor window
3. Configure seed, size, and DFS iterations
4. Click "Generate Maze" button
5. View ASCII preview in console
6. Auto-creates required components if missing

---

## Editor Window Controls

### Random Seed

- **Use Fixed Seed**: Toggle to use a specific random seed
- **Seed Value**: Enter specific seed for reproducible mazes
- **Random** (default): Auto-generates seed each time

### Maze Size

- **Grid Size Slider**: 11x11 to 51x51
- **Recommended**: 21x21 for standard mazes
- Larger sizes = more complex, longer generation time

### Generation Settings

- **DFS Iterations**: 50-500
  - Higher = more complex, more dead-ends
  - Lower = simpler, more direct paths

### Generate Button

- Large green button
- Triggers maze generation
- Logs statistics to Console

### Generation Result Panel

Displays after successful generation:
- Maze dimensions
- Start/Exit coordinates
- Carved cell count
- Complexity percentage
- Dead-end and corridor counts

### ASCII Preview Button

- Shows maze layout in Console
- Legend:
  - `A` = Start point
  - `B` = Exit point
  - `#` = Wall
  - `.` = Passage

---

## Architecture

### Plug-in-Out Compliance

This tool follows strict plug-in-out architecture:

```csharp
// FINDS existing components first
var mazeBuilder = Object.FindFirstObjectByType<CompleteMazeBuilder8>();

// Creates ONLY if missing
if (mazeBuilder == null)
{
    var builderGO = new GameObject("MazeBuilder");
    mazeBuilder = builderGO.AddComponent<CompleteMazeBuilder8>();
}
```

**Never** uses `AddComponent` on existing objects without checking first.

### Integration with CompleteMazeBuilder8

The Editor tool works with `CompleteMazeBuilder8` runtime component:

1. Editor tool generates mathematical maze data
2. `CompleteMazeBuilder8` uses data for Unity object spawning
3. Walls, doors, torches, enemies spawned as prefabs
4. Binary cache saved to `Runtimes/Mazes/`

### MazeMathEngine_8Axis

Pure C# math engine (no Unity dependencies):

- **Cell Types**: `CELL_WALL` (0xFF), `CELL_PASSAGE` (0x00)
- **Directions**: 8-axis enum (N, S, E, W, NE, NW, SE, SW)
- **Algorithms**:
  - Depth-first search carving
  - A* pathfinding
  - Chebyshev distance heuristic
  - Dead-end identification

---

## Configuration

### GameConfig Integration

Uses `GameConfig.Instance` for runtime parameters:

```csharp
var config = GameConfig.Instance;
int size = config.defaultGridSize;      // 21
float cellSize = config.defaultCellSize; // 6.0f
float wallHeight = config.defaultWallHeight; // 3.0f
```

### JSON Configuration

Can also load from JSON:

```json
{
  "BaseSize": 21,
  "MinSize": 15,
  "MaxSize": 51,
  "CellSize": 6.0,
  "WallHeight": 3.0,
  "MazeCfg": {
    "SpawnRoomSize": 2,
    "TorchChance": 0.25,
    "EnemyDensity": 0.03,
    "ChestDensity": 0.05
  }
}
```

---

## Console Output

### Generation Log

```
═══════════════════════════════════════════
  MAZEMATH ENGINE - Pure Math Generation
═══════════════════════════════════════════
[MazeMathEngine]  Generating 21x21 maze with seed 12345...
[MazeMathEngine]  Creating GameConfig...
[MazeMathEngine]  Creating CompleteMazeBuilder8...
[MazeMathEngine]  Scene setup complete
[MazeMathEngine]  Generation complete in 45.23ms
[MazeMathEngine]  Carved: 187/441 (42.4%)
[MazeMathEngine]  Start: (2, 2), Exit: (18, 18)
[MazeMathEngine]  Dead-ends: 23, Corridors: 187
═══════════════════════════════════════════
```

### ASCII Preview Example

```
═══════════════════════════════════════════
  MAZE PREVIEW 21x21
═══════════════════════════════════════════
#####################
#A........#.........#
###.###.###.#####.#.#
#...#...#...#.....#.#
#.###.#####.#####.#.#
#.#...#...#.....#.#.#
#.#####.###.###.#.#.#
#.....#.#...#...#...#
#.###.#.###.#####.###
#.#...#.#...#.....#.#
#.#####.###.#####.#.#
#.....#...#.#.....#.#
###.#####.#.#.#####.#
#...#.....#.#.#.....#
#.###.#######.#####.#
#.#...#.......#.....#
#.#####.#####.#####.#
#.....#.#...#.#.....#
#####.#.#.###.#####.#
#.........#.......B.#
#####################
═══════════════════════════════════════════
  Legend: A=Start, B=Exit, #=Wall, .=Passage
═══════════════════════════════════════════
```

---

## Troubleshooting

### CompleteMazeBuilder8 Not Found

**Error**: `[MazeMathEngineEditor] CompleteMazeBuilder8 not found after setup!`

**Solution**:
1. Run `Tools → Maze → Setup Scene` menu command
2. Or manually create empty GameObject
3. Add `CompleteMazeBuilder8` component
4. Assign required prefabs in Inspector

### GameConfig Not Found

**Error**: `[MazeMathEngineEditor] GameConfig not found!`

**Solution**:
1. Ensure `GameConfig` component exists in scene
2. Check `Assets/Resources/Config/GameConfig8-default.json` exists
3. Run `Tools → Maze → Setup Scene` to auto-create

### Maze Generation Fails

**Symptoms**: No maze appears, no errors

**Solution**:
1. Check Console for errors
2. Verify wall/door prefabs assigned to `CompleteMazeBuilder8`
3. Ensure materials assigned (wallMaterial, etc.)
4. Check maze size is within bounds (11-51)

---

## Performance

### Generation Times

| Maze Size | Average Time | Carved Cells |
|-----------|-------------|--------------|
| 11x11     | 5-10ms      | ~60          |
| 21x21     | 20-50ms     | ~180         |
| 31x31     | 50-100ms    | ~400         |
| 51x51     | 150-300ms   | ~1100        |

### Optimization Tips

1. Use smaller mazes during development (21x21)
2. Reduce DFS iterations for faster testing
3. Cache generated mazes (automatic in `Runtimes/Mazes/`)
4. Use fixed seed for reproducible testing

---

## API Reference

### MazeMathEngine_8Axis

```csharp
// Constructor
public MazeMathEngine_8Axis(int width, int height, int randomSeed = -1)

// Generate complete maze
public MazeGenerationResult GenerateMaze(int depthFirstIterations = -1)
```

### MazeGenerationResult

```csharp
public int Width { get; set; }
public int Height { get; set; }
public byte[,] MazeGrid { get; set; }
public (int x, int z) StartPoint { get; set; }
public (int x, int z) ExitPoint { get; set; }
public List<(int x, int z)> DeadEnds { get; set; }
public List<(int x, int z)> Corridors { get; set; }
public int TotalCells { get; set; }
public int CarvedCells { get; set; }
public float ComplexityFactor { get; } // carved / total
```

---

## Compliance Checklist

- [x] UTF-8 encoding
- [x] Unix LF line endings
- [x] Unity 6 (6000.3.7f1) compatible
- [x] Plug-in-out architecture
- [x] GPL-3.0 license headers
- [x] No emojis in C# files
- [x] C# Unity naming conventions
- [x] Relative paths for assets
- [x] Documentation in `Assets/Docs/`
- [x] Editor tools in `Assets/Scripts/Editor/`

---

## See Also

- `MazeMathEngine_8Axis.cs` - Core mathematical engine
- `CompleteMazeBuilder8.cs` - Runtime maze builder
- `DungeonMazeGenerator.cs` - Advanced dungeon generation
- `GameConfig.cs` - Configuration system
- `Assets/Docs/complete_maze_builder_guide_20260304.md` - Full builder guide

---

## License

This software is licensed under the GNU General Public License v3.0 (GPL-3.0).  
See the `COPYING` file in the project root for full license text.

**CodeDotLavos** - Copyright (C) 2026
