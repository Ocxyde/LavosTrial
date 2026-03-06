# Project Tasks - Change History

**Project:** CodeDotLavos (Unity 6000.3.7f1)
**Author:** Ocxyde
**License:** GPL-3.0

---

## 📅 **2026-03-06 - SESSION 3: 8-WAY MAZE IMPLEMENTATION**

### **Major Changes:**
- **GridMazeGenerator.cs** - Complete rewrite with 8-way DFS algorithm
  - Levels 0-2: 4-way corridors (tutorial)
  - Levels 3+: 8-way corridors (diagonal walls)
  - Corridor width = 1 cell (always)
  - Proper Unity C# naming conventions (_camelCase)
  - Room-to-maze connection system
  - 6-zone room distribution

- **CompleteMazeBuilder.cs** - Updated to pass level parameter
  - Calls: `grid.Generate(seed, difficultyFactor, currentLevel)`
  - Sets corridor width to 1

- **MazeRenderer.cs** - 8-way wall rotation support
  - WallDirection enum (8 directions with angles)
  - GetWallAngle() helper method
  - SpawnWall() overload with direction parameter
  - Supports: 0°, 45°, 90°, 135°, 180°, 225°, 270°, 315°

### **Bug Fixes:**
- Mathf.Sign() cast fix - Added (int) cast to prevent float-to-int error
  - File: GridMazeGenerator.cs line 466-467
  - Fixed: `int dx = (int)Mathf.Sign(corridorPos.x - x);`

---

## 📅 **2026-03-06 - SESSION 2: MAZE RENDERING REFACTOR**

### **Major Changes:**
- **MazeRenderer.cs** created (extracted from CompleteMazeBuilder)
  - Dedicated wall rendering system
  - Single responsibility principle
  - Handles: Outer perimeter walls, interior walls, ComputeGrid integration

- **CompleteMazeBuilder.cs** updated to use MazeRenderer
  - Removed ~300 lines of wall rendering code
  - Now calls: `mazeRenderer.RenderWalls()`
  - File reduced from ~1050 lines to ~760 lines

- **Maze validation** added (flood-fill algorithm, connectivity check)
  - Auto-regenerates maze if validation fails (one retry)
  - Execution time: ~0.05ms for 21x21 maze

- **Room distribution** improved (quadrant-based placement)
  - Divides grid into 4 quadrants (NE, NW, SE, SW)
  - Places rooms evenly across quadrants
  - Better spatial distribution (no more center clustering)

- **Binary storage paths** fixed (persistentDataPath for cross-platform)
  - ComputeGridData.cs updated
  - SpatialPlacer.cs updated

- **Prefab validation** added (critical prefabs checked in LoadConfig)
  - Checks if wallPrefab is loaded (ERROR if null)
  - Checks if doorPrefab is loaded (WARNING if null)
  - Checks if floorMaterial is loaded (WARNING if null)

### **Tools Created:**
- cleanup-deprecated-maze-files.ps1 (delete legacy files)

---

## 📅 **2026-03-06 - SESSION 1: CLEANUP & COMPLIANCE**

### **High Priority:**
- Reworked `MazeBuilderEditor.cs` - Full plug-in-out compliance (no component creation)
- Updated `SpawnPlacerEngine` references to `SpatialPlacer` in comments
- Verified `SpawnPlacerEngine.cs` already deleted
- Verified old test files already deleted

### **Medium Priority:**
- Cleaned commented code from `MazeIntegration.cs` (~50 lines removed)
- Cleaned commented code from `SeedManager.cs` (~60 lines removed)
- Verified `LightEngine.cs` complete (927 lines)
- Verified `ParticleGenerator.cs` complete (896 lines)
- Created `GridPEnvPlacer.cs` - Wall placement with exact border snapping
- Refactored `CompleteMazeBuilder.PlaceWalls()` - N/W borders, byte-to-byte RAM storage
- Made `PlaceWalls()`, `PlaceDoors()`, `PlaceTorches()` public for modular calls

### **Tools Created:**
- remove-emoji-from-cs.ps1 - Removes emoji from all C# files
- PlugInOutComplianceChecker.cs - Scans for architecture violations
- GIT_INSTRUCTIONS.md - Git commit instructions

---

## 📅 **EARLIER SESSIONS**

### **Maze System Development:**
- GridMazeGenerator rewritten: Room-corridor approach (DFS abandoned)
- DFS maze generation attempts: Multiple rewrites, cell math mismatch identified
- Wall scaling fixed to snap to cell edges
- MazeCorridorGenerator bypassed (using simple corridor carving)
- Random.Range ambiguity fixed (UnityEngine.Random)
- MazeCorridorGenerator created with A* pathfinding (PathFinder integration)

### **Architecture:**
- SeedManager moved to 11_Utilities/ (correct architecture)
- Compute seed system: Destroy after use, reseed immediately
- PathFinder.cs created in 11_Utilities/ (A* algorithm)
- EventHandler extended with OnComputeSeedChanged event
- CompleteMazeBuilder uses SeedManager.ComputeSeed
- GridMazeGenerator uses MazeCorridorGenerator for A* paths
- GameConfig-default.json: Added corridor settings

### **Code Quality:**
- Commented code cleaned from MazeIntegration.cs and SeedManager.cs
- Verified LightEngine.cs and ParticleGenerator.cs are complete (not truncated)
- GridPEnvPlacer.cs created for wall placement with exact border snapping
- CompleteMazeBuilder SIMPLIFIED (verbosity removed, ~500 lines)
- CompleteMazeBuilder OPTIMIZED (~550 lines, verbosity restored)
- CompleteMazeBuilder OPTIMIZED (~500 lines, better perf)
- CompleteMazeBuilder rewritten as MAIN ORCHESTRATOR

---

**Last Updated:** 2026-03-06
**Author:** Ocxyde

---

*Document generated - Unity 6 compatible - UTF-8 encoding - Unix LF*
