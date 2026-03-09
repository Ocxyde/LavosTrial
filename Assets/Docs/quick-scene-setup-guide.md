# Quick Scene Setup Guide

**Date:** 2026-03-04  
**Unity Version:** 6000.3.7f1  
**System:** Plug-in-and-Out Architecture  

---

## 🚀 Quick Start

### Option 1: PowerShell Script (Recommended)

```powershell
.\quick-scene-setup.ps1
```

This prepares the scene file and gives you instructions.

### Option 2: Unity Editor Menu

1. Open Unity
2. Open `Assets/Scenes/FpsMazeTest_Fresh.unity`
3. **Tools → Quick Scene Setup → Generate Complete Scene**
4. Or press **Ctrl+Alt+G**

---

## 📦 What Gets Generated

### Maze System
| Component | Configuration |
|-----------|---------------|
| **MazeGenerator** | 31x31 cells, fixed size |
| **MazeRenderer** | Cell size: 6m (wide corridors), Wall height: 3.5m |
| **MazeIntegration** | 3-8 rooms, 60% door chance, Level 1 |

### Environment
| Component | Configuration |
|-----------|---------------|
| **GroundPlaneGenerator** | 200m stone tile floor |
| **CeilingGenerator** | 200m dark stone ceiling |
| **Wall Textures** | Stone dungeon style |

### Lighting
| Component | Configuration |
|-----------|---------------|
| **LightEngine** | Dynamic lights + Fog of war |
| **LightPlacementEngine** | Auto-registers all light sources |
| **TorchPool** | 60 torches with BraseroFlame particles |
| **Light Range** | 12m |
| **Light Intensity** | 1.8 (warm orange: RGB 1.0, 0.9, 0.7) |

### Interactive Objects
| Object | Count | Status |
|--------|-------|--------|
| **Torches** | 60 | ✅ Placed on walls |
| **Chests** | 5 | ✅ Ready (prefab required) |
| **Enemies** | 8 | ⏸ Prefabs ready (not spawned) |
| **Items** | 10 | ⏸ Prefabs required |

### Player
| Feature | Value |
|---------|-------|
| **View** | FPS (eye height: 1.7m) |
| **Movement** | WASD + Mouse |
| **Sprint** | Left Shift |
| **Jump** | Space |
| **Head Bob** | Enabled (walking + sprinting) |

---

## 🎮 Controls

### Movement
- **WASD** - Move
- **Mouse** - Look around
- **Shift** - Sprint (+10% speed, costs stamina)
- **Space** - Jump (costs stamina)

### Test Controls
- **R** - Regenerate maze (new seed)
- **G** - Regenerate (same seed)
- **T** - Toggle torches
- **Space** - Clear all

---

## 🔧 Plug-in-and-Out Architecture

This setup uses your **central hub system**:

```
┌──────────────────┐
│   MazeTest       │  ← Single GameObject
│   (Container)    │
└────────┬─────────┘
         │
         ├─► MazeGenerator      (maze generation)
         ├─► MazeRenderer       (wall textures)
         ├─► MazeIntegration    (rooms + doors + holes)
         ├─► SpatialPlacer      (torches, chests, enemies, items)
         ├─► TorchPool          (torch instancing)
         ├─► LightPlacementEngine (light registration)
         ├─► LightEngine        (dynamic lighting + fog)
         ├─► GroundPlaneGenerator (floor)
         ├─► CeilingGenerator   (ceiling)
         └─► FpsMazeTest        (test controller)
```

**All components work independently but pivot around the MazeTest GameObject.**

---

## 📝 Scene Settings

### Render Settings
```yaml
Fog: Enabled
  Mode: Linear
  Density: 0.005
  Start: 10m
  End: 80m
  Color: RGB(0.08, 0.05, 0.03) - dark brown

Ambient Light:
  Sky Color: RGB(0.6, 0.55, 0.5) - bright stone
  Intensity: 1.5
  Mode: Trilinear
```

### Maze Settings
```yaml
Dimensions: 31x31 cells
Cell Size: 6m (wide corridors)
Wall Height: 3.5m
Total Size: ~186m x 186m
```

### Lighting Settings
```yaml
Dynamic Lights: Enabled
Light Range: 12m
Light Intensity: 1.8
Light Color: Warm orange (torch glow)
Fog of War: Enabled
Darkness Falloff: 15m
```

---

## 🐛 Troubleshooting

### Scene is too dark
1. Check LightEngine is enabled
2. Verify torches are placed (console log: "Placed X torches")
3. Reduce fog density to 0.003
4. Increase ambient intensity to 2.0

### Walls are white/pink
1. Check MazeRenderer has stone material assigned
2. Verify `autoGenerateFloorMaterials = true`
3. Check Console for material errors

### Torches not emitting light
1. Verify TorchPool.useBraseroFlame = true
2. Check LightEngine.enableDynamicLights = true
3. Ensure torches are turned ON (console: "Torch_XXX turned ON")

### No rooms generated
1. Check MazeIntegration.generateRooms = true
2. Verify minRooms/maxRooms settings (3-8)
3. Check console for room generation logs

### Doors missing
1. Check MazeIntegration.generateDoors = true
2. Verify doorChance = 0.6 (60%)
3. Ensure door holes are placed first

---

## 📂 Files Created/Modified

| File | Purpose |
|------|---------|
| `Assets/Scripts/Editor/QuickSceneSetup.cs` | Editor tool script |
| `Assets/Scripts/Editor/QuickSceneSetup.cs.meta` | Unity metadata |
| `Assets/Scenes/FpsMazeTest_Fresh.unity` | Test scene |
| `Assets/Scenes/FpsMazeTest_Fresh.unity.meta` | Scene metadata |
| `quick-scene-setup.ps1` | PowerShell setup script |

---

## 🔄 Workflow

### First Time Setup
```powershell
# 1. Run setup script
.\quick-scene-setup.ps1

# 2. Open Unity
# 3. Open scene: Assets/Scenes/FpsMazeTest_Fresh.unity
# 4. Run: Tools → Quick Scene Setup → Generate Complete Scene
# 5. Press Play

# 6. After testing, backup
.\backup.ps1
```

### Regenerate Maze
```powershell
# In Unity Editor:
# Select MazeTest → FpsMazeTest component
# Click "Regenerate (New Seed)" or press R in-game
```

### Clear Scene
```powershell
# In Unity Editor:
# Tools → Quick Scene Setup → Clear Scene
# Or press Ctrl+Alt+X
```

---

## 🎯 Next Steps

### After Scene is Working
1. **Add enemy prefabs** to SpatialPlacer.enemyPrefabs
2. **Add item prefabs** to SpatialPlacer.itemPrefabs
3. **Add chest prefab** to SpatialPlacer.chestPrefab
4. **Test combat system** with enemies
5. **Balance difficulty** via lighting config

### Content Development
- Create more room variants
- Design enemy types
- Balance torch placement
- Add sound effects
- Create particle effects

---

## 📊 Performance

| Metric | Expected Value |
|--------|----------------|
| **Generation Time** | < 2 seconds |
| **Torch Count** | 60 instances |
| **Dynamic Lights** | 60 point lights |
| **Frame Rate** | 60+ FPS |
| **Memory** | < 100 MB |

---

## 📚 Related Documentation

- [ARCHITECTURE_OVERVIEW.md](../ARCHITECTURE_OVERVIEW.md)
- [TODO.md](TODO.md)
- [LightEngine.cs](../Assets/Scripts/Core/12_Compute/LightEngine.cs)
- [SpatialPlacer.cs](../Assets/Scripts/Core/08_Environment/SpatialPlacer.cs)

---

**Generated:** 2026-03-04  
**Status:** ✅ Ready to Use  

---

**Happy Testing!** 🎮✨
