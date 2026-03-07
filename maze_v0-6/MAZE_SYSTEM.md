# 🗃️ MAZE SYSTEM — Binary Storage Module
> Supplement to the main README.md  
> Date: 2026-03-06

---

## 📦 New Files Added

| File | Location | Role |
|---|---|---|
| `MazeData.cs` | `Assets/Scripts/Core/06_Maze/` | Data model + cell flags |
| `GridMazeGenerator.cs` | `Assets/Scripts/Core/06_Maze/` | DFS + A* generation |
| `MazeBinaryStorage.cs` | `Assets/Scripts/Core/06_Maze/` | Binary save/load (.lvm) |
| `CompleteMazeBuilder.cs` | `Assets/Scripts/Core/06_Maze/` | Main orchestrator (updated) |
| `GameConfig.cs` | `Assets/Scripts/Core/06_Maze/` | JSON config loader |
| `GameConfig-default.json` | `Config/` | Default values |

---

## 🔑 Binary Format — `.lvm` (LaVos Maze)

Files are saved to: `<ProjectRoot>/Runtimes/Mazes/` (editor)  
or `Application.persistentDataPath/Mazes/` (build)

**File name:** `maze_L{level:D3}_S{seed}.lvm`  
Example: `maze_L000_S-847291034.lvm`

```
Offset   Bytes   Field
──────────────────────────────────────────────
 0        5      Magic "LAVOS"
 5        1      Version (1)
 6        2      Width  (int16, LE)
 8        2      Height (int16, LE)
10        4      Seed   (int32, LE)
14        4      Level  (int32, LE)
18        8      Timestamp (int64, UTC unix seconds)
26        2      SpawnX (int16)
28        2      SpawnZ (int16)
30        2      ExitX  (int16)
32        2      ExitZ  (int16)
34        W×H    Cell data — 1 byte per cell (CellFlags enum)
34+W×H    4      Checksum XOR-fold (uint32)
──────────────────────────────────────────────
Total: 38 + W×H bytes
  Level 0 (12×12) → 182 bytes
  Level 39 (51×51) → 2,639 bytes
```

**Cell byte bit layout:**

| Bit | Flag      | Meaning              |
|-----|-----------|----------------------|
|  0  | WallN     | North wall present   |
|  1  | WallS     | South wall present   |
|  2  | WallE     | East wall present    |
|  3  | WallW     | West wall present    |
|  4  | SpawnRoom | Part of spawn room   |
|  5  | HasChest  | Chest placed here    |
|  6  | HasEnemy  | Enemy placed here    |
|  7  | HasTorch  | Torch on this cell   |

---

## ⚙️ Generation Algorithm

```
Seed → System.Random(seed)
   │
   ├─ 1. FillAllWalls   — every cell = 0b00001111 (all walls on)
   │
   ├─ 2. RecursiveBacktracker (DFS)
   │      Start at (1,1), shuffle 4 directions per step
   │      Carve passages by clearing wall flags on both sides
   │      Result: perfect maze (no loops, every cell reachable)
   │
   ├─ 3. SpawnRoom      — 5×5 area at (1,1) fully cleared
   │
   ├─ 4. Exit           — placed at (W-2, H-2)
   │
   ├─ 5. A* path        — guarantees spawn→exit corridor exists
   │      Crossing existing walls costs 10×, open cells cost 1×
   │      Carves missing walls along the optimal path
   │
   ├─ 6. Torches        — 30% of wall-adjacent non-spawn cells
   │
   └─ 7. Objects        — chests (3%) and enemies (5%) on open cells
```

---

## 💾 Storage API

```csharp
// Save
MazeBinaryStorage.Save(mazeData);           // → Runtimes/Mazes/maze_L000_S1234.lvm

// Load
MazeData data = MazeBinaryStorage.Load(level: 0, seed: 1234);

// Cache check (used by CompleteMazeBuilder to skip regen)
bool cached = MazeBinaryStorage.Exists(level, seed);

// Delete one entry
MazeBinaryStorage.Delete(level, seed);

// Wipe all cached mazes
MazeBinaryStorage.PurgeAll();

// Debug info
string info = MazeBinaryStorage.FileInfo(level, seed);
// → "182 bytes | modified 2026-03-06 14:22:01"
```

---

## ✅ Compliance

| Principle              | Status |
|------------------------|--------|
| Plug-in-Out            | ✅ FindFirstObjectByType everywhere |
| No Hardcoded Values    | ✅ All values from GameConfig-default.json |
| Spawn Room First       | ✅ Step 3 in pipeline |
| Player Last            | ✅ Step 12 (always final) |
| Binary Storage         | ✅ .lvm format, Runtimes/Mazes/ |
| Seed Determinism       | ✅ System.Random(seed) — identical output per seed |
| Cache Re-use           | ✅ Exists() check before regenerating |
