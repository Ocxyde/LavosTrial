# Test Maze with Torches - Quick Setup Guide
**Date:** 2026-03-02  
**Unity Version:** 6000.3.7f1 (URP Standard)

---

## Quick Test (5 minutes)

### Step 1: Create Test Scene

1. **Create new scene:**
   ```
   File → New Scene → Empty Scene
   Save as: Assets/Scenes/TestMazeWithTorches.unity
   ```

2. **Create empty GameObject:**
   ```
   Right-click in Hierarchy → Create Empty
   Name it: "MazeGenerator"
   ```

3. **Add required components** (in this order):
   ```
   Add Component → MazeIntegration
   Add Component → MazeGenerator
   Add Component → MazeRenderer
   Add Component → TorchPool
   Add Component → SpatialPlacer
   Add Component → SeedManager (if not in scene)
   Add Component → DrawingPool (if not in scene)
   ```

4. **Add test controller:**
   ```
   Add Component → TestMazeWithTorches
   ```

---

### Step 2: Configure Components

#### MazeIntegration Settings:
- `Auto Generate On Start`: ✓
- `Use Random Seed`: ✓
- `Place Torches`: ✓ (enable for torch testing)
- `Maze Width`: 21
- `Maze Height`: 21

#### SpatialPlacer Settings:
- `Place Torches`: ✓ (enable)
- `Torch Pool`: Drag the MazeGenerator GameObject here
- `Torch Count`: 15
- `Torch Height Ratio`: 0.55
- `Min Distance Between Torches`: 6

#### TestMazeWithTorches Settings:
- `Auto Generate On Start`: ✓
- `Use Random Seed`: ✓
- `Place Torches`: ✓
- `Torch Count`: 15
- `Show Debug UI`: ✓

---

### Step 3: Add Player (Optional)

1. **Add player prefab:**
   ```
   Drag your player prefab into the scene
   OR
   Assign playerPrefab in MazeRenderer inspector
   ```

2. **Position at start:**
   ```
   Player will auto-spawn at maze start cell
   ```

---

### Step 4: Add Lighting (Recommended)

1. **Add ambient light:**
   ```
   Window → Rendering → Lighting
   Environment → Skybox Material: (assign your skybox)
   Environment Lighting → Source: Gradient
   Environment Lighting → Gradient: Dark brown/orange
   ```

2. **Add fog:**
   ```
   Window → Rendering → Lighting
   Environment → Fog: ✓
   Fog Color: Dark brown (0.08, 0.05, 0.03)
   Fog Mode: Linear
   Fog Start: 12
   Fog End: 90
   ```

---

### Step 5: Test!

1. **Press Play** in Unity

2. **Maze should generate automatically** with:
   - Walls and corridors
   - Rooms (if RoomGenerator is present)
   - Doors (if DoorHolePlacer + RoomDoorPlacer present)
   - **Torches on walls** (if enabled)

3. **Use keyboard shortcuts:**
   - **R** - Regenerate maze (new seed)
   - **T** - Toggle torches on/off

4. **Use debug UI** (top-left corner):
   - Click buttons to generate/regenerate
   - Place/remove torches manually
   - View stats

---

## Troubleshooting

### Maze doesn't generate:
- ✅ Check console for errors
- ✅ Ensure all components are on same GameObject
- ✅ Verify SeedManager exists in scene
- ✅ Verify DrawingPool exists and is initialized

### Torches don't appear:
- ✅ `Place Torches` enabled in SpatialPlacer?
- ✅ `Torch Pool` reference assigned?
- ✅ `DrawingPool.Instance` has flame textures?
- ✅ Try manual: Right-click SpatialPlacer → Place Torches

### Black walls / missing textures:
- ✅ DrawingPool initialized with correct seed
- ✅ URP configured correctly
- ✅ Materials assigned in MazeRenderer

---

## Editor Commands

### Right-click TestMazeWithTorches component:

- **Generate Test Maze** - Generate with current settings
- **Regenerate (New Seed)** - New random seed
- **Regenerate (Same Seed)** - Same seed, new layout
- **Toggle Torches** - Enable/disable torches
- **Place Torches** - Add torches manually
- **Remove Torches** - Remove all torches

---

## Expected Results

### With Torches Enabled:
- Maze generates with walls
- 10-25 torches placed on wall faces (random distribution)
- Warm orange lighting from torches
- Fog and atmospheric effects

### With Torches Disabled:
- Maze generates with walls
- No torches placed
- Darker maze (ambient only)

---

## Performance Tips

- **Smaller mazes (15x15):** Fast generation, good for testing
- **Medium mazes (21x21):** Balanced, recommended for testing
- **Large mazes (31x31):** Slower, more torches, better visuals

### Torch Count Recommendations:
- **15x15 maze:** 8-12 torches
- **21x21 maze:** 15-25 torches
- **31x31 maze:** 25-40 torches

---

## Next Steps

1. ✅ Test basic maze generation
2. ✅ Test torch placement
3. ✅ Test torch toggle (T key)
4. ✅ Test regeneration (R key)
5. ✅ Adjust torch count and spacing
6. ✅ Run `backup.ps1` after verification!

---

**See Also:**
- [`TORCH_PLUGIN_SYSTEM.md`](./TORCH_PLUGIN_SYSTEM.md) - Full torch system docs
- [`SPATIAL_PLACER.md`](./SPATIAL_PLACER.md) - SpatialPlacer documentation
- [`MAZE_INTEGRATION.md`](./MAZE_INTEGRATION.md) - Maze generation docs
