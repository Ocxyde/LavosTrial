# Quick Maze Test Setup
**Date:** 2026-03-02

---

## Easiest Way (1 click!)

1. **Open Unity** and wait for compilation

2. **Menu:** `Tools → Maze Test → Quick Setup (Current Scene)`

3. **Press Play!**

That's it! A new GameObject will be created with all components configured.

---

## What Gets Created

The quick setup creates a GameObject named "MazeGenerator" with:

- ✅ MazeGenerator (21x21 maze)
- ✅ MazeRenderer (wall rendering)
- ✅ TorchPool (torch object pooling)
- ✅ SpatialPlacer (torch placement enabled, 15 torches)
- ✅ MazeIntegration (orchestration)
- ✅ TestMazeWithTorches (test controller)
- ✅ RoomGenerator (rooms enabled)
- ✅ DoorHolePlacer (door holes)
- ✅ RoomDoorPlacer (doors)

**Default Settings:**
- Maze: 21x21
- Torches: 15 (50% probability)
- Rooms: 3-6
- Doors: 60% chance
- Auto-generate on start: Yes

---

## Controls

Once in Play mode:

- **R** - Regenerate maze (new seed)
- **T** - Toggle torches on/off
- **Debug UI** - Top-left corner shows stats

---

## Manual Setup (Alternative)

If you prefer manual control:

1. **Create empty GameObject**
   ```
   Right-click Hierarchy → Create Empty
   Name: "MazeGenerator"
   ```

2. **Add components:**
   ```
   Add Component → MazeGenerator
   Add Component → MazeRenderer
   Add Component → TorchPool
   Add Component → SpatialPlacer
   Add Component → MazeIntegration
   Add Component → TestMazeWithTorches
   ```

3. **Configure** (see inspector settings in TEST_MAZE_WITH_TORCHES.md)

---

## Troubleshooting

### "MazeIntegration not found"
- Make sure all components are on the **same GameObject**
- Or use the menu: `Tools → Maze Test → Quick Setup`

### "SpatialPlacer not found"
- Add SpatialPlacer component to the MazeGenerator GameObject
- Or use the quick setup menu

### Torches don't appear
- Check `Place Torches` is enabled in SpatialPlacer
- Verify `Torch Pool` reference is assigned
- Check console for errors

### Console errors about DrawingPool
- Make sure DrawingPool exists in your scene
- It should be on a GameObject (can be the same as MazeGenerator)

---

## Menu Options

**Tools → Maze Test:**
- **Quick Setup (Current Scene)** - One-click setup
- **Setup Test Scene** - Window with options

---

## Next Steps

1. ✅ Run quick setup
2. ✅ Press Play
3. ✅ Watch maze generate with torches
4. ✅ Press R to regenerate
5. ✅ Press T to toggle torches
6. ✅ Run `backup.ps1` after verification!

---

**Documentation:**
- [`TEST_SETUP_COMPLETE.md`](./TEST_SETUP_COMPLETE.md) - Full test guide
- [`TORCH_PLUGIN_SYSTEM.md`](./TORCH_PLUGIN_SYSTEM.md) - Torch system docs
