# DIFF: Option C - Full 8-Axis Replacement

**Date:** 2026-03-06
**Status:** ⏳ PENDING (User Request - Option 3)
**Unity Version:** 6000.3.7f1
**Warning:** ⚠️ **BREAKING CHANGE** - Do NOT apply without testing

---

## 📋 OVERVIEW

**Source:** `maze_v0-6-8_ushort_2byte_saves/`
**Target:** `Assets/Scripts/Core/06_Maze/`
**Changes:** Complete maze system replacement (4-axis → 8-axis)

---

## ⚠️ BREAKING CHANGES

| Aspect | Current System | 8-Axis System (v0-6-8) | Impact |
|--------|---------------|------------------------|--------|
| **Cell Type** | `GridMazeCell` (byte) | `CellFlags8` (ushort) | 🔴 Save format incompatible |
| **Cell Size** | 1 byte | 2 bytes | 🔴 Save files 2x larger |
| **Directions** | 4 (N,E,S,W) | 8 (N,NE,E,SE,S,SW,W,NW) | 🔴 Wall rendering needs update |
| **Save Format** | `.bin` (raw) | `.lvm` (LAV8S v2) | 🔴 Different file format |
| **Namespace** | `Code.Lavos.Core` | `LavosTrial.Core.Maze` | 🔴 Using directives must change |
| **Spawn** | Single cell (1, gridSize/2) | 5×5 room at (1,1) | 🔴 Different spawn logic |
| **API** | `Generate(seed, difficulty, level)` | `Generate(seed, level, config)` | 🔴 Method signature change |

---

## 📁 FILES TO REPLACE

### **1. GridMazeGenerator.cs**

**Current:** 492 lines (4-axis DFS + exit corridor)
**Replace with:** `GridMazeGenerator8.cs` (328 lines, 8-axis DFS + A*)

```diff
--- Assets/Scripts/Core/06_Maze/GridMazeGenerator.cs
+++ Assets/Scripts/Core/06_Maze/GridMazeGenerator.cs
@@ -1,492 +1,328 @@
-// Copyright (C) 2026 Ocxyde
-// GridMazeGenerator.cs
-// PURE MAZE generation - NO rooms, just corridors and walls
-namespace Code.Lavos.Core
+// LavosTrial - CodeDotLavos
+// GridMazeGenerator8.cs
+// 8-axis maze generation with DFS + A*
+namespace LavosTrial.Core.Maze
 
-public class GridMazeGenerator
+public sealed class GridMazeGenerator8
 {
-    // 4-way direction arrays
-    private static readonly int[] _directionsX4 = { 0,  1,  0, -1 };
-    private static readonly int[] _directionsY4 = { 1,  0, -1,  0 };
-    
-    // Cell type enum
-    public GridMazeCell[,] Grid { get; }
-    public Vector2Int SpawnPoint { get; }
-    
-    // DFS carves corridors
-    private void CarveMazeWithDfs() { ... }
-    
-    // Exit corridor to south wall
-    public void CarveExitToSouth() { ... }
+    // 8-way direction enum + helper
+    private sealed class Node { ... } // A* node
+    
+    // MazeData8 with ushort cells
+    public MazeData8 Generate(int seed, int level, MazeConfig8 cfg) { ... }
+    
+    // DFS over 8 axes (dx=±2, dz=±2)
+    private void CarvePassages8(...) { ... }
+    
+    // A* guaranteed path (Chebyshev heuristic)
+    private void EnsurePath(...) { ... }
+    
+    // 5×5 spawn room at (1,1)
+    private void CarveSpawnRoom(...) { ... }
 }
```

**Key Differences:**
- Uses `MazeData8` instead of `GridMazeCell[,]`
- 8-axis DFS (step = 2 cells)
- A* pathfinding for guaranteed exit path
- 5×5 spawn room instead of single cell
- Different API signature

---

### **2. CompleteMazeBuilder.cs**

**Current:** 785 lines (13-step generation)
**Replace with:** `CompleteMazeBuilder8.cs` (418 lines, 12-step pipeline)

```diff
--- Assets/Scripts/Core/06_Maze/CompleteMazeBuilder.cs
+++ Assets/Scripts/Core/06_Maze/CompleteMazeBuilder.cs
@@ -1,785 +1,418 @@
-// Copyright (C) 2026 Ocxyde
-// CompleteMazeBuilder.cs
-// MAIN GAME ORCHESTRATOR
-namespace Code.Lavos.Core
+// LavosTrial - CodeDotLavos
+// CompleteMazeBuilder8.cs
+// 8-axis maze orchestrator
+namespace LavosTrial.Core.Maze
 
-public class CompleteMazeBuilder : MonoBehaviour
+public sealed class CompleteMazeBuilder8 : MonoBehaviour
 {
-    [SerializeField] private GameObject _wallPrefab;
-    [SerializeField] private GameObject _doorPrefab;
-    [SerializeField] private int _currentLevel = 0;
-    
-    private GridMazeGenerator _grid;
-    private float _cellSize, _wallHeight;
-    
-    // 13-step generation:
-    // 1. Config, 2. Assets, 3. Components, 4. Cleanup,
-    // 5. Ground, 6. Grid (DFS), 7. Exit Corridor,
-    // 8. Walls, 9. Doors, 10. Save, 11. Player
-    
-    private void GenerateGrid()
-    {
-        _grid = new GridMazeGenerator();
-        _grid.GridSize = _mazeSize;
-        _grid.Generate(_seed, difficultyFactor, _currentLevel);
-        _grid.CarveExitToSouth(); // Ensure exit path
-    }
-    
-    private void RenderWalls()
-    {
-        // 4-axis wall placement
-    }
+    [SerializeField] private GameObject wallPrefab;      // Cardinal
+    [SerializeField] private GameObject wallDiagPrefab;  // Diagonal
+    [SerializeField] private GameObject wallCornerPrefab;
+    [SerializeField] private GameObject torchPrefab;
+    [SerializeField] private GameObject chestPrefab;
+    [SerializeField] private GameObject enemyPrefab;
+    
+    private MazeData8 _mazeData;
+    private GridMazeGenerator8 _generator;
+    
+    // 12-step pipeline:
+    // 1. Config, 2. Assets, 3. Components, 4. Cleanup,
+    // 5. Ground, 6. Spawn Room, 7. Corridors (8-axis),
+    // 8. Walls (cardinal+diagonal), 9. Doors, 10. Torches,
+    // 11. Save (.lvm), 12. Player
+    
+    public void GenerateMaze()
+    {
+        // Cache-first approach
+        if (MazeBinaryStorage8.Exists(currentLevel, currentSeed))
+            _mazeData = MazeBinaryStorage8.Load(currentLevel, currentSeed);
+        else
+            _mazeData = _generator.Generate(currentSeed, currentLevel, _config.MazeCfg);
+        
+        SpawnAllWalls(); // Cardinal + diagonal
+        SpawnTorches();  // From cell flags
+        SpawnObjects();  // Chests + enemies from flags
+    }
 }
```

**Key Differences:**
- Sealed class (cannot inherit)
- Uses `MazeData8` instead of `GridMazeGenerator`
- Binary storage with `.lvm` format
- Diagonal wall prefabs required
- Objects spawned from cell flags (not separate placement)

---

### **3. GameConfig.cs**

**Current:** JSON config loader
**Replace with:** `GameConfig8.cs` + `MazeConfig8`

```diff
--- Assets/Scripts/Core/06_Maze/GameConfig.cs
+++ Assets/Scripts/Core/06_Maze/GameConfig.cs
@@ -1,200 +1,80 @@
-// Copyright (C) 2026 Ocxyde
-// GameConfig.cs - JSON configuration
-namespace Code.Lavos.Core
+// LavosTrial - CodeDotLavos
+// GameConfig8.cs - 8-axis maze config
+namespace LavosTrial.Core.Maze
 
-public class GameConfig : MonoBehaviour
+public sealed class GameConfig8 : MonoBehaviour
 {
-    public float defaultCellSize = 6.0f;
-    public float defaultWallHeight = 4.0f;
-    public int defaultGridSize = 21;
-    public int defaultRoomSize = 5;
-    // ... many fields
+    public float CellSize = 6.0f;
+    public float WallHeight = 4.0f;
+    public float PlayerEyeHeight = 1.7f;
+    public MazeConfig8 MazeCfg = new MazeConfig8();
 }
+
+// NEW: MazeConfig8 class
+[Serializable]
+public sealed class MazeConfig8
+{
+    public int   BaseSize       = 12;
+    public int   MinSize        = 12;
+    public int   MaxSize        = 51;
+    public int   SpawnRoomSize  = 5;
+    public float TorchChance    = 0.30f;
+    public float ChestDensity   = 0.03f;
+    public float EnemyDensity   = 0.05f;
+    public bool  DiagonalWalls  = true;
+}
```

---

### **4. NEW FILES**

#### **MazeData8.cs** (NEW)
```csharp
// NEW FILE: Assets/Scripts/Core/06_Maze/MazeData8.cs
// 80 lines - Cell model with ushort flags

namespace LavosTrial.Core.Maze
{
    [Flags]
    public enum CellFlags8 : ushort
    {
        None      = 0,
        WallN     = 1 << 0,   // 0x0001
        WallS     = 1 << 1,   // 0x0002
        WallE     = 1 << 2,   // 0x0004
        WallW     = 1 << 3,   // 0x0008
        WallNE    = 1 << 4,   // 0x0010
        WallNW    = 1 << 5,   // 0x0020
        WallSE    = 1 << 6,   // 0x0040
        WallSW    = 1 << 7,   // 0x0080
        SpawnRoom = 1 << 8,   // 0x0100
        HasChest  = 1 << 9,   // 0x0200
        HasEnemy  = 1 << 10,  // 0x0400
        HasTorch  = 1 << 11,  // 0x0800
        IsExit    = 1 << 12,  // 0x1000
        AllWalls  = 0x00FF,
    }
    
    public enum Direction8 { N, S, E, W, NE, NW, SE, SW }
    
    public sealed class MazeData8
    {
        public int Width, Height, Seed, Level;
        public (int x, int z) SpawnCell, ExitCell;
        
        private readonly CellFlags8[,] _cells;
        
        public CellFlags8 GetCell(int x, int z) => _cells[x, z];
        public void SetCell(int x, int z, CellFlags8 f) => _cells[x, z] = f;
        public bool HasWall(int x, int z, Direction8 d) => ...
    }
}
```

#### **MazeBinaryStorage8.cs** (NEW)
```csharp
// NEW FILE: Assets/Scripts/Core/06_Maze/MazeBinaryStorage8.cs
// 200 lines - .lvm binary format with checksums

namespace LavosTrial.Core.Maze
{
    public static class MazeBinaryStorage8
    {
        // Save format: "LAV8S" magic + header (34 bytes) + cells (W×H×2) + checksum (4 bytes)
        // Total: 38 + (W × H × 2) bytes
        // Level 0 (12×12) → 326 bytes
        // Level 39 (51×51) → 5,240 bytes
        
        public static bool Save(MazeData8 data) { ... }
        public static MazeData8 Load(int level, int seed) { ... }
        public static bool Exists(int level, int seed) { ... }
    }
}
```

#### **CellFlags8.cs** (NEW - or merge into MazeData8.cs)
```csharp
// NEW FILE: Assets/Scripts/Core/06_Maze/CellFlags8.cs
// Enum and Direction8 helper (already in MazeData8.cs above)
```

---

### **5. GameConfig-default.json**

**Current:** `Config/GameConfig-default.json`
**Replace with:** `Config/GameConfig8-default.json`

```diff
--- Config/GameConfig-default.json
+++ Config/GameConfig8-default.json
@@ -1,80 +1,20 @@
 {
-    "wallPrefab": "Prefabs/WallPrefab.prefab",
-    "doorPrefab": "Prefabs/DoorPrefab.prefab",
-    "defaultCellSize": 6.0,
-    "defaultWallHeight": 4.0,
-    "defaultGridSize": 21,
-    "defaultRoomSize": 5,
-    "defaultCorridorWidth": 1,
-    "defaultPlayerEyeHeight": 1.7,
-    "defaultPlayerSpawnOffset": 0.5
+    "cellSize": 6.0,
+    "wallHeight": 4.0,
+    "playerEyeHeight": 1.7,
+    "playerSpawnOffset": 0.5,
+    "mazeBaseSize": 12,
+    "mazeMinSize": 12,
+    "mazeMaxSize": 51,
+    "spawnRoomSize": 5,
+    "torchChance": 0.30,
+    "chestDensity": 0.03,
+    "enemyDensity": 0.05,
+    "diagonalWalls": true
 }
```

---

## 🔧 INTEGRATION STEPS (If Applied)

### **Step 1: Backup Current Files**
```bash
# Already done - backup.ps1 completed
```

### **Step 2: Copy Files**
```
FROM: maze_v0-6-8_ushort_2byte_saves/
TO:   Assets/Scripts/Core/06_Maze/

GridMazeGenerator8.cs       → GridMazeGenerator.cs (replace)
MazeData8.cs                → NEW FILE
MazeBinaryStorage8.cs       → NEW FILE
CompleteMazeBuilder8.cs     → CompleteMazeBuilder.cs (replace)
GameConfig8.cs              → GameConfig.cs (replace)
```

### **Step 3: Copy Config**
```
FROM: maze_v0-6-8_ushort_2byte_saves/GameConfig8-default.json
TO:   Config/GameConfig8-default.json
```

### **Step 4: Update Prefabs**
**New prefabs required:**
- `wallDiagPrefab` - Diagonal wall segment (45°)
- `wallCornerPrefab` - Corner cap (optional)
- `chestPrefab` - Chest object
- `enemyPrefab` - Enemy object

### **Step 5: Update Using Directives**
```csharp
// In all files that use maze system:
-using Code.Lavos.Core;
+using LavosTrial.Core.Maze;
```

### **Step 6: Update Wall Rendering**
**MazeRenderer.cs must be updated to handle:**
- 8 wall directions (not just 4)
- Diagonal walls (NE, NW, SE, SW)
- Wall flags from `CellFlags8` enum
- Cell data from `ushort` instead of `GridMazeCell`

---

## ⚠️ KNOWN ISSUES / TODO

### **Wall Rendering (CRITICAL)**
Current `MazeRenderer.cs` expects `GridMazeCell` enum:
```csharp
// Current code:
if (cell == GridMazeCell.Wall) { ... }

// 8-axis system:
if ((cellFlags & CellFlags8.WallN) != 0) { ... }
```

**MazeRenderer must be completely rewritten** to:
1. Read `CellFlags8` (ushort) instead of `GridMazeCell` (byte)
2. Place 8 wall types (N,NE,E,SE,S,SW,W,NW)
3. Handle diagonal wall prefabs
4. Render corner caps where needed

### **Save System Migration**
Current saves use `.bin` format (1 byte/cell).
8-axis uses `.lvm` format (2 bytes/cell + header + checksum).

**Options:**
1. Delete old `.bin` saves (start fresh)
2. Write migration tool (`.bin` → `.lvm`)

### **Namespace Changes**
All code using maze system must change:
```csharp
-using Code.Lavos.Core;
+using LavosTrial.Core.Maze;
```

---

## 📊 COMPARISON TABLE

| Feature | Current | 8-Axis (v0-6-8) | Winner |
|---------|---------|-----------------|--------|
| **Code Size** | ~1800 lines | ~1100 lines | ✅ 8-Axis |
| **Cell Storage** | 1 byte | 2 bytes | ✅ Current |
| **Save Format** | Basic `.bin` | Robust `.lvm` | ✅ 8-Axis |
| **Wall Directions** | 4 | 8 | ✅ 8-Axis |
| **Pathfinding** | DFS + exit corridor | DFS + A* | ✅ 8-Axis |
| **Object Storage** | Separate | In-cell flags | ✅ 8-Axis |
| **Validation** | Passes | Passes | Tie |
| **Complexity** | Low | Medium | ✅ Current |
| **Maintenance** | Easy | Moderate | ✅ Current |
| **Prefabs** | Wall, Door | Wall, WallDiag, WallCorner, Torch, Chest, Enemy | ✅ Current |

---

## ✅ RECOMMENDATION (REITERATED)

**DO NOT APPLY** - Keep current system

**Reasons:**
1. ✅ Current system **WORKS** (player can escape)
2. ⚠️ 8-axis requires **wall rendering rewrite**
3. ⚠️ **Breaking change** (save format incompatible)
4. ⚠️ **More prefabs** required (diagonal walls, corners)
5. ⚠️ **Namespace changes** throughout codebase
6. ✅ Current system is **simpler** (easier to maintain)

**Archive v0-6-8 for future reference** - already in `maze_v0-6-8_ushort_2byte_saves/`

---

## 📝 GIT COMMIT (If Applied - NOT RECOMMENDED)

```bash
git add Assets/Scripts/Core/06_Maze/GridMazeGenerator.cs
git add Assets/Scripts/Core/06_Maze/MazeData8.cs
git add Assets/Scripts/Core/06_Maze/MazeBinaryStorage8.cs
git add Assets/Scripts/Core/06_Maze/CompleteMazeBuilder.cs
git add Assets/Scripts/Core/06_Maze/GameConfig.cs
git add Config/GameConfig8-default.json

git commit -m "refactor: Replace maze system with 8-axis version (v0-6-8)

BREAKING CHANGE:
- Cell type: byte → ushort (2 bytes/cell)
- Save format: .bin → .lvm (LAV8S v2)
- Directions: 4-axis → 8-axis (N,NE,E,SE,S,SW,W,NW)
- Namespace: Code.Lavos.Core → LavosTrial.Core.Maze
- Spawn: single cell → 5×5 room

New features:
- A* pathfinding for guaranteed exit path
- Binary storage with checksums
- Object flags in cell data (chest, enemy, torch)
- Diagonal wall support

WARNING: Wall rendering needs update for 8-axis!

```

---

## 📁 FILES IN THIS DIFF

| File | Action | Lines | Purpose |
|------|--------|-------|---------|
| `GridMazeGenerator.cs` | Replace | 328 | 8-axis DFS + A* |
| `MazeData8.cs` | NEW | 200 | Cell model (ushort) |
| `MazeBinaryStorage8.cs` | NEW | 200 | .lvm save format |
| `CompleteMazeBuilder.cs` | Replace | 418 | 8-axis orchestrator |
| `GameConfig.cs` | Replace | 80 | 8-axis config |
| `GameConfig8-default.json` | NEW | 20 | Config values |

**Total:** 6 files, ~1246 lines

---

*Document generated - Unity 6 (6000.3.7f1) compatible - UTF-8 encoding - Unix LF*

**Diff complete - DO NOT APPLY without wall rendering update!** 🫡⚠️
