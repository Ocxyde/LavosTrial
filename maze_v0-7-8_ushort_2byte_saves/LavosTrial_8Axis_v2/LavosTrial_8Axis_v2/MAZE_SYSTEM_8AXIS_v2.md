# MAZE SYSTEM — 8-Axis + Progressive Difficulty ×3
> LavosTrial / CodeDotLavos  
> Date: 2026-03-06  
> Encoding: UTF-8 | Locale: en_US | License: GPL-3.0  
> Namespace: `Code.Lavos.Core`

---

## Files

| File | Path | Role |
|---|---|---|
| `DifficultyScaler.cs` | `Assets/Scripts/Core/06_Maze/` | Progressive ×3 curve engine |
| `MazeData8.cs` | `Assets/Scripts/Core/06_Maze/` | Cell model — `CellFlags8` (ushort) |
| `GridMazeGenerator8.cs` | `Assets/Scripts/Core/06_Maze/` | 8-axis DFS + A* + scaled params |
| `MazeBinaryStorage8.cs` | `Assets/Scripts/Core/06_Maze/` | Binary save/load (LAV8S v2) |
| `CompleteMazeBuilder8.cs` | `Assets/Scripts/Core/06_Maze/` | 12-step orchestrator |
| `GameConfig8.cs` | `Assets/Scripts/Core/06_Maze/` | JSON config + difficulty fields |
| `GameConfig8-default.json` | `Config/` | All runtime values |

---

## Difficulty Curve

```
factor(level) = 1 + (MaxFactor - 1) × (level / MaxLevel) ^ Exponent
```

Default: `MaxFactor=3.0`, `MaxLevel=39`, `Exponent=2.0` (quadratic)

| Level | t    | factor | MazeSize | Enemies | Chests | Torches | WallPenalty |
|------:|-----:|-------:|---------:|--------:|-------:|--------:|------------:|
|  0    | 0.00 |  1.000 |   13×13  |  3.0%   |  5.0%  |  30.0%  |     100     |
|  4    | 0.10 |  1.020 |   17×17  |  3.1%   |  4.9%  |  30.4%  |     102     |
|  8    | 0.21 |  1.082 |   22×22  |  3.2%   |  4.6%  |  31.6%  |     108     |
| 13    | 0.33 |  1.222 |   29×29  |  3.7%   |  4.1%  |  33.3%  |     122     |
| 20    | 0.51 |  1.524 |   42×42  |  4.6%   |  3.3%  |  36.5%  |     152     |
| 28    | 0.72 |  2.035 |   57→51  |  6.1%   |  2.5%  |  40.8%  |     203     |
| 39    | 1.00 |  3.000 |   51×51  |  9.0%   |  1.7%  |  45.0%  |     300     |

Exponent can be changed in JSON without touching code:
- `diffExponent: 1.0` → linear ramp
- `diffExponent: 0.5` → fast early, slow late
- `diffExponent: 3.0` → very slow start, very steep end

---

## CellFlags8 — ushort (2 bytes per cell)

```
Bit  Flag        Meaning
─────────────────────────────────────
 0   WallN       North wall
 1   WallS       South wall
 2   WallE       East wall
 3   WallW       West wall
 4   WallNE      NE diagonal wall
 5   WallNW      NW diagonal wall
 6   WallSE      SE diagonal wall
 7   WallSW      SW diagonal wall
 8   SpawnRoom   Part of spawn room
 9   HasChest    Chest
10   HasEnemy    Enemy
11   HasTorch    Torch
12   IsExit      Exit cell
```

---

## Binary Format — LAV8S v2

File: `Runtimes/Mazes/maze8_L{level:D3}_S{seed}.lvm`

```
Offset    Bytes    Field
─────────────────────────────────────────────
  0        5       Magic "LAV8S"
  5        1       Version (2)
  6        2       Width  (int16, LE)
  8        2       Height (int16, LE)
 10        4       Seed   (int32, LE)
 14        4       Level  (int32, LE)
 18        8       Timestamp (int64, UTC unix)
 26        2       SpawnX (int16)
 28        2       SpawnZ (int16)
 30        2       ExitX  (int16)
 32        2       ExitZ  (int16)
 34        4       DifficultyFactor (float32)  ← new in v2
 38        W×H×2   Cell data — ushort per cell
 38+W*H*2  4       Checksum (uint32, seed=0xCAFE8888)
─────────────────────────────────────────────
Total: 42 + (W × H × 2) bytes
```

---

## Compliance

| Principle | Status |
|---|---|
| Namespace `Code.Lavos.Core` | ✅ All files |
| Plug-in-Out | ✅ `FindFirstObjectByType` only |
| No hardcoded values | ✅ All from JSON |
| Spawn Room First | ✅ Step 3 |
| Player Last | ✅ Step 12 |
| Binary Storage | ✅ LAV8S v2, `Runtimes/Mazes/` |
| Seed determinism | ✅ `System.Random(seed)` |
| Difficulty ×3 progressive | ✅ Power curve, JSON-tunable |
| UTF-8 / en_US | ✅ All files |
| GPL-3.0 headers | ✅ All `.cs` files |
