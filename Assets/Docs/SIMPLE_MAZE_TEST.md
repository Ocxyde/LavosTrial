# Simple Maze Test - Clean & Simple

**Location:** `Assets/Scripts/Core/06_Maze/SimpleMazeTest.cs`  
**Unity Version:** 6000.3.7f1  
**Last Updated:** 2026-03-03  

---

## 🎯 Purpose

**CLEAN & SIMPLE** maze test with only:
- ✅ Maze (wide corridors)
- ✅ Player (with camera)
- ✅ Torches (with flames)

**NO** rooms, doors, enemies, items, or complexity.

---

## ⚡ Quick Start

### Method 1: Editor Menu (Fastest)

1. **Menu:** `Tools → Simple Maze Test → Create Test`
2. **Press Play**
3. **Done!**

### Method 2: Manual

1. **Create empty GameObject**
2. **Add component:** `SimpleMazeTest`
3. **Press Play**

---

## 🎮 Features

| Feature | Value |
|---------|-------|
| **Maze Size** | 41x41 cells |
| **Corridor Width** | 8m (extra wide) |
| **Wall Height** | 4m |
| **Torch Count** | 60 torches |
| **Min Torch Distance** | 5m |
| **Camera Distance** | 4m (third-person) |

---

## 🎮 Controls

| Action | Key |
|--------|-----|
| **Move Forward** | `W` |
| **Move Backward** | `S` |
| **Move Left** | `A` |
| **Move Right** | `D` |
| **Sprint** | `Shift` (hold) |
| **Jump** | `Space` |
| **Look Around** | `Mouse` |
| **Regenerate Maze** | `R` |
| **Toggle Torches** | `T` |

---

## 📊 Debug UI

Shows:
- Maze dimensions
- Corridor width
- Generation status
- Active torch count
- Player position
- Controls reminder

---

## 🔧 Settings (Inspector)

### Maze Settings

| Field | Default | Description |
|-------|---------|-------------|
| `Maze Width` | 41 | Cells (odd numbers work best) |
| `Maze Height` | 41 | Cells (odd numbers work best) |
| `Cell Size` | 8.0 | Meters (corridor width) |
| `Wall Height` | 4.0 | Meters |

### Torch Settings

| Field | Default | Description |
|-------|---------|-------------|
| `Torch Count` | 60 | Number of torches |
| `Min Torch Distance` | 5.0 | Minimum spacing (meters) |

### Player Settings

| Field | Default | Description |
|-------|---------|-------------|
| `Spawn Player` | ✓ | Auto-spawn on start |
| `Camera Distance` | 4.0 | Third-person view distance |

---

## 🏗️ Architecture

```
SimpleMazeTest (MonoBehaviour)
    │
    ├─→ MazeGenerator (creates maze data)
    ├─→ MazeRenderer (builds geometry)
    ├─→ TorchPool (object pooling)
    ├─→ SpatialPlacer (PLUG-IN for torch placement)
    │
    └─→ Player (spawned at center)
         ├─→ CharacterController
         ├─→ PlayerStats
         ├─→ PlayerController
         └─→ CameraFollow
```

---

## 🔥 Torch System

Torches are placed by **SpatialPlacer** (universal placement PLUG-IN):

1. **Maze generates** (walls, floor, ceiling)
2. **SpatialPlacer.PlaceTorches()** is called
3. **TorchPool** creates torches with **BraseroFlame** particles
4. **60 torches** placed on walls with 5m spacing

**Flame Type:** BraseroFlame (particle system)
- Orange/yellow particles
- Animated upward motion
- Flicker effect
- Point light for illumination

---

## 🎯 Player Spawn

Player spawns at **center of maze**:

```csharp
Center Cell = (20, 20) for 41x41 maze
World Position = (164, 1, 164)
Player Feet = y=0.10 (on ground)
Player Eyes = y=1.00 (comfortable view)
```

**Guaranteed to be in a corridor** (not inside wall).

---

## 🐛 Troubleshooting

### Can't Move

**Problem:** Cursor not locked

**Fix:** Click in Game view to lock cursor

---

### Can't See Torches

**Problem:** Torches not placed

**Fix:** Press `T` to toggle torches, or check Console for errors

---

### Player Stuck in Wall

**Problem:** Spawn position invalid

**Fix:** Press `R` to regenerate maze

---

### Flames Not Visible

**Problem:** Particle system not working

**Fix:** Check Console for "BraseroFlame" errors

---

## 📝 Console Output

Expected on Play:

```
=== Simple Maze Test ===
Maze Size: 41x41 cells
Corridor Width: 8m
Torch Count: 60
[SimpleMazeTest] Ground plane created (240x240m)
[MazeGenerator] Generated 41x41 | Seed: XXXXX
[MazeRenderer] Maze geometry built
[SpatialPlacer] Placed 60 torches on XXXX wall faces
[SimpleMazeTest] Player spawned at (164.0, 1.0, 164.0)
[SimpleMazeTest] Controls: WASD=Move, Shift=Sprint, Space=Jump, Mouse=Look
=== Maze Generation Complete ===
```

---

## 🚀 Next Steps

After testing:

1. **Adjust settings** in Inspector (maze size, torch count, etc.)
2. **Add features** (rooms, doors, enemies) if needed
3. **Save as prefab** for reuse

---

## 📚 Related Files

| File | Purpose |
|------|---------|
| `SimpleMazeTest.cs` | Main test script |
| `SimpleMazeTestEditor.cs` | Editor helper |
| `SpatialPlacer.cs` | Torch placement PLUG-IN |
| `TorchPool.cs` | Object pooling |
| `BraseroFlame.cs` | Particle flame effect |

---

**Created:** 2026-03-03  
**Status:** ✅ Production Ready  
**Complexity:** Simple (maze + player + torches only)
