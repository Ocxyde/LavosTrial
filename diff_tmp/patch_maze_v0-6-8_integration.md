# PATCH: maze_v0-6-8_ushort_2byte_saves Integration

**Date:** 2026-03-06
**Status:** ⏳ PENDING REVIEW
**Unity Version:** 6000.3.7f1
**Backup:** ✅ Completed (`backup.ps1`)

---

## 📦 SOURCE FOLDER

**Location:** `maze_v0-6-8_ushort_2byte_saves/`

**Contents:**
```
maze_v0-6-8_ushort_2byte_saves/
├── CompleteMazeBuilder8.cs         ← 8-axis orchestrator
├── GameConfig8.cs                  ← 8-axis config loader
├── GameConfig8-default.json        ← 8-axis config values
├── GridMazeGenerator8.cs           ← 8-axis DFS + A* generator
├── LavosTrial_8Axis.zip            ← Archive
├── MAZE_SYSTEM_8AXIS.md            ← Documentation
├── MazeBinaryStorage8.cs           ← Binary save/load (.lvm format)
├── MazeData8.cs                    ← Cell model (ushort, 8-axis)
└── LavosTrial_8Axis/
    └── LavosTrial_8Axis/
        ├── MAZE_SYSTEM_8AXIS.md
        └── Assets/Scripts/Core/06_Maze/
            ├── CompleteMazeBuilder8.cs
            ├── GameConfig8.cs
            ├── GridMazeGenerator8.cs
            ├── MazeBinaryStorage8.cs
            └── MazeData8.cs
```

---

## 🔍 KEY DIFFERENCES: Current vs 8-Axis

| Feature | Current System | 8-Axis System (v0-6-8) |
|---------|---------------|------------------------|
| **Cell Type** | `GridMazeCell` (byte) | `CellFlags8` (ushort) |
| **Cell Size** | 1 byte | 2 bytes |
| **Directions** | 4-axis (N,E,S,W) | 8-axis (N,NE,E,SE,S,SW,W,NW) |
| **Wall Flags** | Cell type enum | Bit flags (bits 0-7) |
| **Object Flags** | Separate storage | Bit flags (bits 8-12) |
| **Algorithm** | DFS only | DFS + A* pathfinding |
| **Save Format** | `.bin` (raw grid) | `.lvm` (LAV8S v2 format) |
| **Spawn** | Single cell (1, gridSize/2) | 5×5 room at (1,1) |
| **Exit** | South wall center | (W-2, H-2) with IsExit flag |
| **File Size** | 2 + (W×H) bytes | 38 + (W×H×2) bytes |

---

## 📊 CELLFLAGS8 BIT LAYOUT

```
ushort (2 bytes) = 16 bits

Bit  Flag        Meaning
─────────────────────────────────────────────
 0   WallN       North wall present
 1   WallS       South wall present
 2   WallE       East wall present
 3   WallW       West wall present
 4   WallNE      North-East diagonal wall
 5   WallNW      North-West diagonal wall
 6   WallSE      South-East diagonal wall
 7   WallSW      South-West diagonal wall
 ─── (high byte) ──────────────────────────
 8   SpawnRoom   Part of the spawn room
 9   HasChest    Chest on this cell
10   HasEnemy    Enemy on this cell
11   HasTorch    Torch on this cell
12   IsExit      Exit cell
13-15             (reserved)
```

**Value:** `0x00FF` = All 8 walls present

---

## 🏗️ BINARY FORMAT (.lvm - LAV8S v2)

```
Offset    Bytes    Field
──────────────────────────────────────────────────
  0        5       Magic "LAV8S"
  5        1       Version (2)
  6        2       Width  (int16, LE)
  8        2       Height (int16, LE)
 10        4       Seed   (int32, LE)
 14        4       Level  (int32, LE)
 18        8       Timestamp (int64, UTC unix secs)
 26        2       SpawnX (int16)
 28        2       SpawnZ (int16)
 30        2       ExitX  (int16)
 32        2       ExitZ  (int16)
 34        W×H×2   Cell data — ushort per cell (LE)
 34+W*H*2  4       Checksum XOR-fold (uint32)
──────────────────────────────────────────────────
Total: 38 + (W × H × 2) bytes

Examples:
  Level  0 (12×12) →   326 bytes
  Level  9 (21×21) →   924 bytes
  Level 39 (51×51) → 5,240 bytes
```

**Checksum seed:** `0xCAFE8888` (distinguishes v2/8-axis from v1/4-axis)

---

## 📝 PATCH OPTIONS

### **Option A: Keep Current System (RECOMMENDED)**
**Status:** Current system is working
- ✅ Pure maze with exit corridor
- ✅ Walls snap to grid
- ✅ Validation passes
- ✅ Player can escape
- ✅ Simpler (1 byte/cell, 4-axis)

**Action:** Archive v0-6-8 as reference, continue with current system

---

### **Option B: Merge 8-Axis Features**
**Features to consider merging:**
1. **A* Pathfinding** - Ensure optimal path to exit
2. **Binary Format** - More robust save system with checksums
3. **Object Flags** - Store chest/enemy/torch in cell data
4. **5×5 Spawn Room** - Larger spawn area

**Files to create:**
- `MazeData8.cs` → New cell model
- `MazeBinaryStorage8.cs` → Enhanced save system
- Update `GridMazeGenerator.cs` → Add A* pathfinding
- Update `CompleteMazeBuilder.cs` → Use new storage

---

### **Option C: Full 8-Axis Replacement**
**Replace current system entirely with v0-6-8**

**Files to copy:**
```
FROM: maze_v0-6-8_ushort_2byte_saves/
TO:   Assets/Scripts/Core/06_Maze/

GridMazeGenerator8.cs       → GridMazeGenerator.cs (replace)
MazeData8.cs                → NEW FILE
MazeBinaryStorage8.cs       → NEW FILE
CompleteMazeBuilder8.cs     → CompleteMazeBuilder.cs (replace)
GameConfig8.cs              → GameConfig.cs (replace)
GameConfig8-default.json    → GameConfig-default.json (replace)
```

**WARNING:** This is a **BREAKING CHANGE**:
- ❌ Existing `.bin` saves incompatible
- ❌ Wall rendering needs update (8-axis vs 4-axis)
- ❌ Larger save files (2 bytes/cell vs 1 byte)

---

## ✅ RECOMMENDATION

**Use Option A** - Keep current system, archive v0-6-8 as reference

**Reasons:**
1. ✅ Current system works (validation passes, player can escape)
2. ✅ Simpler code (4-axis vs 8-axis)
3. ✅ Smaller save files (1 byte/cell vs 2 bytes)
4. ✅ Walls snap to grid perfectly
5. ✅ No breaking changes

**Archive v0-6-8 for future reference:**
- 8-axis DFS algorithm is interesting
- A* pathfinding could be useful later
- Binary format with checksums is robust
- Keep as `maze_v0-6-8_ushort_2byte_saves/` (already archived)

---

## 📋 FILES STATUS

| File | Current System | v0-6-8 System | Status |
|------|---------------|---------------|--------|
| `GridMazeGenerator.cs` | ✅ Working | `GridMazeGenerator8.cs` | Keep current |
| `CompleteMazeBuilder.cs` | ✅ Working | `CompleteMazeBuilder8.cs` | Keep current |
| `GameConfig.cs` | ✅ Working | `GameConfig8.cs` | Keep current |
| `MazeSaveData.cs` | Basic `.bin` | `MazeBinaryStorage8.cs` (`.lvm`) | Consider merge |
| `MazeData8.cs` | N/A | NEW | Archive |
| `GridMazeCell.cs` | ✅ Working | `CellFlags8` (ushort) | Keep current |

---

## 🚀 NEXT STEPS

### **If Option A (Recommended):**
```bash
# No changes needed - current system works!
# v0-6-8 already archived in maze_v0-6-8_ushort_2byte_saves/

git add diff_tmp/grid_maze_exit_fix_20260306.md
git commit -m "docs: Archive maze v0-6-8 8-axis system reference

- maze_v0-6-8_ushort_2byte_saves/ contains 8-axis maze system
- Not merged - current 4-axis system works correctly
- Kept for future reference (A* pathfinding, binary format)
- Current system: pure maze, exit corridor, wall snapping

Co-authored-by: BetsyBoop"
```

### **If Option B (Merge Features):**
```bash
# Create enhanced binary storage with checksums
# Add A* pathfinding to ensure exit path
# Merge object flags into cell data

# Files to create/modify:
# - MazeBinaryStorage.cs (add checksums)
# - GridMazeGenerator.cs (add A* pathfinding)
# - GridMazeCell.cs (add object flags)
```

### **If Option C (Full Replacement):**
```bash
# WARNING: BREAKING CHANGE
# Copy v0-6-8 files to Assets/Scripts/Core/06_Maze/
# Update wall rendering for 8-axis
# Migrate save format

# NOT RECOMMENDED - current system works!
```

---

## 📊 COMPARISON METRICS

| Metric | Current | v0-6-8 | Winner |
|--------|---------|--------|--------|
| **Code Complexity** | Low | Medium | ✅ Current |
| **Save File Size** | Small (1B/cell) | Large (2B/cell) | ✅ Current |
| **Wall Rendering** | Simple (4-axis) | Complex (8-axis) | ✅ Current |
| **Pathfinding** | DFS only | DFS + A* | ✅ v0-6-8 |
| **Save Robustness** | Basic | Checksums | ✅ v0-6-8 |
| **Object Storage** | Separate | In-cell flags | ✅ v0-6-8 |
| **Spawn Area** | Single cell | 5×5 room | Tie |
| **Validation** | Passes | Passes | Tie |

**Overall:** Current system is better for this project ✅

---

## ✅ DECISION

**Recommendation:** **Option A** - Keep current system

**Reasons:**
1. ✅ Works correctly (validation passes)
2. ✅ Player can escape (exit corridor)
3. ✅ Walls snap to grid perfectly
4. ✅ Simpler code (easier to maintain)
5. ✅ Smaller save files
6. ✅ No breaking changes

**v0-6-8 Status:** Archived for future reference

---

*Document generated - Unity 6 (6000.3.7f1) compatible - UTF-8 encoding - Unix LF*

**Current system recommended, coder friend!** 🫡⚔️
