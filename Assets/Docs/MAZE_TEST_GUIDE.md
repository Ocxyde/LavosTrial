# Maze Test System - Setup & Debugging Guide

**Location:** `Assets/Docs/MAZE_TEST_GUIDE.md`  
**Unity Version:** 6000.3.7f1  
**Last Updated:** 2026-03-03  
**Test Script:** `MazeTorchTest.cs` (NEW - Simplified)

---

## 🎯 Overview

The Maze Test System allows you to quickly generate and test procedural mazes with torch placement in Unity.

**New:** Simplified test script `MazeTorchTest.cs` replaces the old `TestMazeWithTorches.cs`.

---

## ⚡ Quick Setup (Recommended)

### Method 1: Editor Menu (Fastest)

1. **Open Unity Editor**
2. **Create/Open a test scene**
3. **Menu:** `Tools → Maze Test → Quick Setup (Current Scene)`
4. **Press Play**
5. **Use keyboard controls:**
   - `[R]` - Regenerate maze (new random seed)
   - `[G]` - Regenerate maze (same seed)
   - `[T]` - Toggle torches on/off
   - `[Space]` - Clear all

### Method 2: Manual Setup

1. **Create empty GameObject** (name it "MazeTest")
2. **Add these components** (all on the same GameObject):
   - `MazeGenerator`
   - `MazeRenderer`
   - `TorchPool`
   - `SpatialPlacer`
   - `MazeIntegration`
   - `TestMazeWithTorches`
3. **Configure settings** in Inspector
4. **Press Play**

---

## 🔧 Component Configuration

### Required Components (All on same GameObject)

| Component | Purpose | Required Settings |
|-----------|---------|-------------------|
| `MazeGenerator` | Procedural maze generation | Width: 21, Height: 21 |
| `MazeRenderer` | Visual maze rendering | Auto-configures |
| `TorchPool` | Object pooling for torches | No config needed |
| `SpatialPlacer` | Places torches on walls | Place Torches: ✓ |
| `MazeIntegration` | Coordinates all systems | Maze Width/Height: 21 |
| `MazeTorchTest` | Test harness & controls | Auto-generate on Start: ✓ |

### Optional Components

| Component | Purpose |
|-----------|---------|
| `RoomGenerator` | Generate rooms in maze |
| `DoorHolePlacer` | Reserve wall space for doors |
| `RoomDoorPlacer` | Place doors in maze |

---

## 🎮 MazeTorchTest Settings

### Generation Settings

| Field | Default | Description |
|-------|---------|-------------|
| `Auto Generate On Start` | ✓ | Generate maze when scene starts |
| `Use Random Seed` | ✓ | New seed each generation (reproducible if off) |
| `Test Seed` | "MazeTest2026" | Custom seed string |

### Maze Dimensions

| Field | Default | Description |
|-------|---------|-------------|
| `Maze Width` | 21 | Must be odd for perfect mazes |
| `Maze Height` | 21 | Must be odd for perfect mazes |

### Torch Settings

| Field | Default | Description |
|-------|---------|-------------|
| `Enable Torches` | ✓ | Enable torch placement |
| `Torch Count` | 15 | Target number of torches |
| `Min Torch Distance` | 6.0 | Minimum spacing between torches |

### Debug Options

| Field | Default | Description |
|-------|---------|-------------|
| `Show Debug UI` | ✓ | Show overlay with stats |
| `Verbose Logging` | ✗ | Detailed console logs |

### Test Player Settings

| Field | Default | Description |
|-------|---------|-------------|
| `Spawn Test Player` | ✓ | Auto-spawn player with camera |
| `Camera Distance` | 3.5m | Distance from player (eye level view) |
| `Camera Height` | 1.7m | Camera height offset |
| `Spawn Ground Plane` | ✓ | Add ground plane for testing |

---

## 🎮 Player Controls

### Movement

| Key | Action |
|-----|--------|
| `[W]` | Move Forward |
| `[A]` | Move Left |
| `[S]` | Move Backward |
| `[D]` | Move Right |
| `[Shift]` | Sprint (+10% speed) |
| `[Space]` | Jump |

### Camera

| Input | Action |
|-------|--------|
| `Mouse Move` | Look Around |
| `Mouse Wheel` | Zoom In/Out |

### Test Controls

| Key | Action |
|-----|--------|
| `[R]` | Regenerate maze (new random seed) |
| `[G]` | Regenerate maze (same seed) |
| `[T]` | Toggle torches on/off |
| `[Space]` | Clear all torches and reset |

---

## 🐛 Common Issues & Fixes

### Issue 1: "TorchPool reference not assigned!"

**Error:**
```
[SpatialPlacer] TorchPool reference not assigned!
```

**Cause:** `TorchPool` component is missing or not on the same GameObject as `SpatialPlacer`.

**Fix:**
1. Select your MazeTest GameObject
2. Add `TorchPool` component (if missing)
3. Or use: `Tools → Maze Test → Quick Setup`

**Code Fix (Auto-find in Awake):**
```csharp
void Awake()
{
    // Auto-find TorchPool if not assigned
    if (torchPool == null)
    {
        torchPool = GetComponent<TorchPool>();
        if (torchPool == null)
        {
            Debug.LogWarning("[SpatialPlacer] TorchPool not found on this GameObject.");
        }
    }
}
```

---

### Issue 2: "MazeGenerator reference not found!"

**Error:**
```
[SpatialPlacer] MazeGenerator reference not found!
```

**Fix:**
1. Ensure `MazeGenerator` is on the same GameObject
2. Or assign the reference in Inspector

---

### Issue 3: Maze not generating

**Symptoms:** Press Play, nothing happens

**Checklist:**
- [ ] `TestMazeWithTorches` component enabled?
- [ ] `Auto Generate On Start` checked?
- [ ] All required components present?
- [ ] Check Console for errors

**Debug Steps:**
1. Enable `Verbose Logging` in TestMazeWithTorches
2. Check Console for "[TestMazeWithTorches]" messages
3. Verify components via right-click context menu

---

### Issue 4: Torches not appearing

**Symptoms:** Maze generates, but no torches

**Checklist:**
- [ ] `Place Torches` enabled in SpatialPlacer?
- [ ] `Place Torches` enabled in TestMazeWithTorches?
- [ ] `TorchPool` component present?
- [ ] Maze generated first? (torches need walls)

**Debug Steps:**
1. Press `[T]` to toggle torch placement
2. Check Console for "[SpatialPlacer]" messages
3. Verify wall faces exist (maze must generate first)

---

## 🛠️ Editor Tools

### Context Menu Actions

Right-click `MazeTorchTest` component in Inspector:
- `Generate Maze` - Generate immediately
- `Regenerate (New Random Seed)` - New seed
- `Regenerate (Same Seed)` - Reproducible test
- `Toggle Torches` - Enable/disable torches
- `Place Torches` - Manual placement
- `Remove Torches` - Clear all torches
- `Clear All` - Reset everything

Right-click `SpatialPlacer` component in Inspector:
- `Place Torches` - Place torches on walls
- `Remove Torches` - Clear torches
- `Place All Objects` - Place all spawnable objects

---

## 📊 Debug UI Overlay

When running, the debug UI shows:

```
═══ Maze + Torches Test ═══

Maze: ✅ Generated
Torches: ✅ Active

Seed: 8f3a2b1c...
Size: 21x21
Target Torches: 15

Generations: 3
Maze Time: 0.045s
Torch Time: 0.012s
Total Time: 0.057s

═══ Controls ═══
[R] Regenerate (New Seed)
[G] Regenerate (Same Seed)
[T] Toggle Torches
```

---

## 🧪 Testing Workflow

### 1. Initial Setup
```
Unity Editor → Tools → Maze Test → Quick Setup
```

### 2. Verify Components
```
Select MazeTest GameObject
Check all 6 components in Inspector
```

### 3. First Test
```
Press Play
Verify maze generates
Verify debug UI appears
```

### 4. Test Regeneration
```
Press [R] - New random seed
Press [G] - Same seed (verify identical maze)
Press [T] - Toggle torches
```

### 5. Performance Check
```
Watch debug UI timing stats
Maze Time: Should be < 0.1s
Torch Time: Should be < 0.05s
```

---

## 📝 Component Architecture

```
┌─────────────────────────────────────────────────────────┐
│              MazeTest GameObject                        │
├─────────────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌─────────────────┐              │
│  │ MazeGenerator   │  │ MazeRenderer    │              │
│  │ - width: 21     │  │ - wallFaces[]   │              │
│  │ - height: 21    │  │ - materials[]   │              │
│  └────────┬────────┘  └────────┬────────┘              │
│           │                    │                       │
│           └────────┬───────────┘                       │
│                    │                                   │
│  ┌─────────────────┴─────────────────┐                │
│  │      MazeIntegration              │                │
│  │  - Coordinates gen + render       │                │
│  └─────────────────┬─────────────────┘                │
│                    │                                   │
│  ┌─────────────────┴─────────────────┐                │
│  │      SpatialPlacer                │                │
│  │  - Places torches on walls        │                │
│  └─────────────────┬─────────────────┘                │
│                    │                                   │
│  ┌─────────────────┴─────────────────┐                │
│  │      TorchPool                    │                │
│  │  - Object pool for torches        │                │
│  └───────────────────────────────────┘                │
│                                                       │
│  ┌───────────────────────────────────┐                │
│  │      MazeTorchTest                │                │
│  │  - Test harness & controls        │                │
│  │  - Debug UI overlay               │                │
│  └───────────────────────────────────┘                │
└─────────────────────────────────────────────────────────┘
```

---

## 🔍 Troubleshooting Commands

### Check Component Setup
```csharp
// In Unity Console (Play Mode):
Debug.Log(GetComponent<MazeGenerator>() != null);
Debug.Log(GetComponent<TorchPool>() != null);
Debug.Log(GetComponent<SpatialPlacer>() != null);
```

### Force Regeneration
```csharp
// In TestMazeWithTorches context menu:
Right-click → Generate Test Maze
```

### Reset to Defaults
```
1. Delete MazeTest GameObject
2. Tools → Maze Test → Quick Setup
```

---

## 📚 Related Documentation

- `ARCHITECTURE_OVERVIEW.md` - Full project architecture
- `TODO.md` - Development roadmap
- `README.md` - Project overview

---

## ✅ Verification Checklist

Before reporting issues, verify:

- [ ] All 6 components on same GameObject
- [ ] No Console errors
- [ ] Debug UI visible in Play Mode
- [ ] Maze generates when pressing Play
- [ ] Torches appear on walls
- [ ] Keyboard controls work ([R], [G], [T])

---

**Generated:** 2026-03-03  
**Status:** ✅ Production Ready  
**Backup Required:** ⚠️ Run `.\backup.ps1` after any changes
