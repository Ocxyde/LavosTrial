# Test Maze with Torches - Setup Complete
**Date:** 2026-03-02  
**Status:** ✅ Ready to Test

---

## What's Ready

### 1. Test Script Created
**File:** `Assets/Scripts/Core/06_Maze/TestMazeWithTorches.cs`

**Features:**
- Auto-generates maze on start
- Places torches via SpatialPlacer
- Debug UI with stats
- Keyboard shortcuts (R = regenerate, T = toggle torches)
- Context menu commands for editor

---

### 2. Test Scene Created
**File:** `Assets/Scenes/TestMazeWithTorches.unity`

**Pre-configured with:**
- MazeGenerator component
- MazeIntegration component
- MazeRenderer component
- TorchPool component
- SpatialPlacer component (with torch placement enabled)
- TestMazeWithTorches controller
- Camera (top-down view)
- Lighting (ambient + fog)

**Settings:**
- Maze size: 21x21
- Torches: Enabled (15 torches)
- Auto-generate on start: Yes
- Random seed: Yes

---

## How to Test

### Quick Test (1 minute):

1. **Open scene:**
   ```
   Assets/Scenes/TestMazeWithTorches.unity
   ```

2. **Press Play**

3. **Maze should generate with:**
   - Walls and corridors
   - 10-25 torches on walls (orange glow)
   - Fog and atmospheric lighting

4. **Test controls:**
   - Press **R** - Regenerate maze (new seed)
   - Press **T** - Toggle torches on/off

5. **Watch debug UI** (top-left corner) for stats

---

### Manual Test (Editor):

1. **Select MazeGenerator GameObject**

2. **Right-click TestMazeWithTorches component:**
   - **Generate Test Maze** - Create new maze
   - **Regenerate (New Seed)** - New random seed
   - **Regenerate (Same Seed)** - Same seed, different layout
   - **Toggle Torches** - Enable/disable
   - **Place Torches** - Manual placement
   - **Remove Torches** - Remove all torches

---

## Expected Results

### ✅ Success:
- Maze generates with walls and corridors
- Torches appear on wall faces (orange glow)
- Debug UI shows generation stats
- R key regenerates maze
- T key toggles torches

### ❌ If Torches Don't Appear:

1. **Check console for errors**
2. **Verify SpatialPlacer settings:**
   - `Place Torches` = ✓
   - `Torch Pool` = assigned
3. **Check DrawingPool:**
   - Must exist in scene
   - Must have flame textures
4. **Try manual placement:**
   - Right-click SpatialPlacer → Place Torches

---

## Configuration

### Adjust Maze Size:
```
TestMazeWithTorches → Maze Width: 15-31
TestMazeWithTorches → Maze Height: 15-31
```

### Adjust Torch Count:
```
SpatialPlacer → Torch Count: 10-30
```

### Adjust Torch Spacing:
```
SpatialPlacer → Min Distance Between Torches: 4-12
```

---

## Files Modified/Created

| File | Status | Purpose |
|------|--------|---------|
| `TestMazeWithTorches.cs` | NEW | Test controller script |
| `TestMazeWithTorches.cs.meta` | NEW | Unity meta file |
| `TestMazeWithTorches.unity` | NEW | Test scene |
| `TORCH_PLUGIN_SYSTEM.md` | UPDATED | Documentation |
| `TEST_MAZE_WITH_TORCHES.md` | NEW | Setup guide |
| `SpatialPlacer.cs` | MODIFIED | Added torch placement |
| `MazeRenderer.cs` | MODIFIED | Cached wall faces |
| `MazeIntegration.cs` | MODIFIED | Uses SpatialPlacer |

---

## Next Steps

1. ✅ **Open Unity** (if not already open)
2. ✅ **Wait for compilation** (if cache was cleared)
3. ✅ **Open scene:** `Assets/Scenes/TestMazeWithTorches.unity`
4. ✅ **Press Play**
5. ✅ **Verify torches appear**
6. ✅ **Test R and T keys**
7. ✅ **Run `backup.ps1`** after verification!

---

## Troubleshooting Quick Reference

| Issue | Solution |
|-------|----------|
| Black walls | Check DrawingPool initialization |
| No torches | Enable `Place Torches` in SpatialPlacer |
| Console errors | Check component references |
| Maze doesn't generate | Verify SeedManager exists |
| Camera black | Check URP asset assignment |

---

**Documentation:**
- [`TORCH_PLUGIN_SYSTEM.md`](./TORCH_PLUGIN_SYSTEM.md) - Full torch system docs
- [`TEST_MAZE_WITH_TORCHES.md`](./TEST_MAZE_WITH_TORCHES.md) - Detailed setup guide

**Remember:** Run `backup.ps1` after any file changes!
