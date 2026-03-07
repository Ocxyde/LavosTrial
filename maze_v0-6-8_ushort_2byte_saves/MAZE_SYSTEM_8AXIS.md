# MAZE SYSTEM — 8-Axis Module
> LavosTrial / CodeDotLavos  
> Date: 2026-03-06  
> Encoding: UTF-8 | Locale: en_US | License: GPL-3.0

---

## Files

| File | Path | Role |
|---|---|---|
| `MazeData8.cs` | `Assets/Scripts/Core/06_Maze/` | Cell model — `CellFlags8` (ushort) + `Direction8` |
| `GridMazeGenerator8.cs` | `Assets/Scripts/Core/06_Maze/` | 8-axis DFS + A* generator |
| `MazeBinaryStorage8.cs` | `Assets/Scripts/Core/06_Maze/` | Byte-exact binary save/load (`LAV8S` format) |
| `CompleteMazeBuilder8.cs` | `Assets/Scripts/Core/06_Maze/` | Orchestrator MonoBehaviour (12-step pipeline) |
| `GameConfig8.cs` | `Assets/Scripts/Core/06_Maze/` | JSON config loader / MonoBehaviour |
| `GameConfig8-default.json` | `Config/` | Default runtime values |

---

## CellFlags8 — ushort (2 bytes per cell)

```
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

---

## Binary Format — `.lvm` (LAV8S v2)

Save directory: `<ProjectRoot>/Runtimes/Mazes/` (editor)  
File name: `maze8_L{level:D3}_S{seed}.lvm`

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
  Level  0 (12×12) →   326 bytes
  Level  9 (21×21) →   924 bytes
  Level 39 (51×51) → 5,240 bytes
```

Checksum seed: `0xCAFE8888` (distinguishes v2 / 8-axis from v1 / 4-axis).

---

## Generation Algorithm

```
System.Random(seed)
  │
  ├─ 1. FillAllWalls     every cell = CellFlags8.AllWalls (0x00FF)
  │
  ├─ 2. DFS (8 axes)     shuffle all 8 directions each step
  │       Cardinal step   : 2 cells along one axis
  │       Diagonal step   : 2 cells along both axes (dx=±2, dz=±2)
  │       Intermediate    : cell at (cx+dx, cz+dz) fully cleared
  │       Result          : perfect maze, all cells reachable
  │
  ├─ 3. SpawnRoom         5×5 at (1,1) — all wall flags cleared
  │
  ├─ 4. Exit              placed at (W-2, H-2), IsExit flag set
  │
  ├─ 5. A* (8-axis)       Chebyshev heuristic
  │       Cardinal cost   = 10
  │       Diagonal cost   = 14  (≈ √2 × 10)
  │       Wall penalty    = +100 per wall crossed
  │       Traces back path and carves any missing walls
  │
  ├─ 6. Torches           30% of wall-adjacent non-spawn cells
  │
  └─ 7. Objects           Chests (3%) + Enemies (5%) on open cells
```

---

## Wall Rendering

| Wall type | Prefab | Rotation | Scale |
|---|---|---|---|
| Cardinal N/S | `WallPrefab` | `Quaternion.identity` | `(cellSize, wallHeight, 0.2)` |
| Cardinal E/W | `WallPrefab` | `Euler(0, 90, 0)` | same |
| Diagonal NE/SW | `WallDiagPrefab` | `Euler(0, +45, 0)` | `(cellSize×√2, wallHeight, 0.2)` |
| Diagonal NW/SE | `WallDiagPrefab` | `Euler(0, -45, 0)` | same |

Set `diagonalWalls: false` in JSON to disable diagonal wall spawning  
(passages still exist in data — only visual mesh is suppressed).

---

## Storage API

```csharp
// Save
MazeBinaryStorage8.Save(mazeData8);

// Load (returns null if not cached)
MazeData8 data = MazeBinaryStorage8.Load(level, seed);

// Cache check
bool hit = MazeBinaryStorage8.Exists(level, seed);

// Cleanup
MazeBinaryStorage8.Delete(level, seed);
MazeBinaryStorage8.PurgeAll();

// Debug
string info = MazeBinaryStorage8.FileInfo(level, seed);
// "326 bytes | modified 2026-03-06 18:20:00"
```

---

## Compliance

| Principle | Status |
|---|---|
| Plug-in-Out | ✅ `FindFirstObjectByType` everywhere, never `AddComponent` |
| No hardcoded values | ✅ All from `GameConfig8-default.json` |
| Spawn Room First | ✅ Step 3 — before A*, before walls |
| Player Last | ✅ Step 12 — always final |
| Binary Storage | ✅ `LAV8S` v2, `Runtimes/Mazes/`, ushort per cell |
| Seed determinism | ✅ `System.Random(seed)` — identical output per seed |
| Cache re-use | ✅ `Exists()` checked before regenerating |
| UTF-8 / en_US | ✅ All files |
| GPL-3.0 headers | ✅ All `.cs` files |
