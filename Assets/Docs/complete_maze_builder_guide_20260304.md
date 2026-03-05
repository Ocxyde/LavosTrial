# Complete Maze Builder Guide

**Date:** 2026-03-04  
**File:** `Assets/Scripts/Core/06_Maze/CompleteMazeBuilder.cs`  
**Status:** ✅ **READY**

---

## 🎯 **OVERVIEW**

**CompleteMazeBuilder** is a unified maze generation tool that spawns:
- 🧱 **Walls** with proper materials
- 🕳️ **Hole traps** (spike pits)
- 🚪 **Doors** (normal, locked, secret)
- 🏛️ **Rooms** (entrance, exit, normal with 1 entrance/exit)
- 🔦 **Torches**, chests, enemies, items (via SpatialPlacer)

**All using relative paths** for prefabs, materials, and textures!

---

## 🏗️ **FEATURES**

### **1. Relative Path System**

All paths are relative to `Assets/`:

```csharp
// Inspector configuration
prefabFolder = "Prefabs/"           // → Assets/Prefabs/
materialFolder = "Materials/"       // → Assets/Materials/
textureFolder = "Textures/"         // → Assets/Textures/

// Specific paths
wallPrefabPath = "Prefabs/WallPrefab.prefab"
doorPrefabPath = "Prefabs/DoorPrefab.prefab"
floorMaterialPath = "Materials/Floor/Stone_Floor.mat"
```

### **2. Complete Maze Generation**

One button generates everything:
```
1. Maze layout (algorithm)
2. Walls with holes
3. Doors at passages
4. Rooms (entrance/exit/normal)
5. Objects (torches, chests, enemies, items)
```

### **3. Door System**

- **Normal doors** (60% chance)
- **Locked doors** (30% chance, requires key)
- **Secret doors** (10% chance, blends with walls)

### **4. Room System**

Each room has **1 entrance and 1 exit**:
- **Entrance Room** (start position, green marker)
- **Exit Room** (end position, blue marker)
- **Normal Rooms** (scattered throughout maze)

---

## 📋 **SETUP**

### **Step 1: Create Prefabs**

**In Unity Editor:**
```
Tools → Create Maze Prefabs
```

This creates:
- ✅ `Assets/Prefabs/WallPrefab.prefab`
- ✅ `Assets/Prefabs/HoleTrapPrefab.prefab`
- ✅ `Assets/Prefabs/DoorPrefab.prefab`
- ✅ `Assets/Prefabs/LockedDoorPrefab.prefab`
- ✅ `Assets/Prefabs/SecretDoorPrefab.prefab`
- ✅ `Assets/Prefabs/EntranceRoomPrefab.prefab`
- ✅ `Assets/Prefabs/ExitRoomPrefab.prefab`
- ✅ `Assets/Prefabs/NormalRoomPrefab.prefab`
- ✅ `Assets/Materials/WallMaterial.mat`

### **Step 2: Add CompleteMazeBuilder**

1. **Create GameObject:**
   - Hierarchy → Right-click → Create Empty
   - Rename to "MazeBuilder"

2. **Add Component:**
   - Add Component → `CompleteMazeBuilder`

3. **Configure Paths** (already set by default):
   - All relative paths auto-configured
   - Adjust if needed

### **Step 3: Generate Maze**

**Option A: Context Menu**
```
1. Select MazeBuilder GameObject
2. Inspector → Click "Generate Complete Maze"
```

**Option B: Code**
```csharp
var builder = GetComponent<CompleteMazeBuilder>();
builder.GenerateCompleteMaze();
```

**Option C: Auto-Generate**
```
✓ Enable "autoGenerateOnStart" in Inspector
```

---

## 🎮 **INSPECTOR SETTINGS**

### **📁 Relative Paths**

| Field | Default | Description |
|-------|---------|-------------|
| `prefabFolder` | `"Prefabs/"` | Base prefab folder |
| `materialFolder` | `"Materials/"` | Base material folder |
| `textureFolder` | `"Textures/"` | Base texture folder |

### **🧱 Wall Prefab Paths**

| Field | Default | Description |
|-------|---------|-------------|
| `wallPrefabPath` | `"Prefabs/WallPrefab.prefab"` | Wall prefab |
| `holePrefabPath` | `"Prefabs/HoleTrapPrefab.prefab"` | Hole trap prefab |

### **🚪 Door Prefab Paths**

| Field | Default | Description |
|-------|---------|-------------|
| `doorPrefabPath` | `"Prefabs/DoorPrefab.prefab"` | Normal door |
| `lockedDoorPrefabPath` | `"Prefabs/LockedDoorPrefab.prefab"` | Locked door |
| `secretDoorPrefabPath` | `"Prefabs/SecretDoorPrefab.prefab"` | Secret door |

### **🏛️ Room Prefab Paths**

| Field | Default | Description |
|-------|---------|-------------|
| `entranceRoomPrefabPath` | `"Prefabs/EntranceRoomPrefab.prefab"` | Start room |
| `exitRoomPrefabPath` | `"Prefabs/ExitRoomPrefab.prefab"` | End room |
| `normalRoomPrefabPath` | `"Prefabs/NormalRoomPrefab.prefab"` | Normal room |

### **🎨 Material Paths**

| Field | Default | Description |
|-------|---------|-------------|
| `wallMaterialPath` | `"Materials/WallMaterial.mat"` | Wall material |
| `floorMaterialPath` | `"Materials/Floor/Stone_Floor.mat"` | Floor material |
| `doorMaterialPath` | `"Materials/Door_PïxelArt.mat"` | Door material |

### **🖼️ Texture Paths**

| Field | Default | Description |
|-------|---------|-------------|
| `wallTexturePath` | `"Textures/wall_texture.png"` | Wall texture |
| `doorTexturePath` | `"Textures/door_sprite_sheet.png"` | Door texture |
| `floorTexturePath` | `"Textures/floor_texture.png"` | Floor texture |

### **🏗️ Maze Settings**

| Field | Default | Description |
|-------|---------|-------------|
| `mazeWidth` | `21` | Maze width (cells) |
| `mazeHeight` | `21` | Maze height (cells) |
| `cellSize` | `6f` | Cell size (meters) |
| `wallHeight` | `4f` | Wall height (meters) |

### **🚪 Door Settings**

| Field | Default | Description |
|-------|---------|-------------|
| `doorSpawnChance` | `0.6f` | Door spawn chance (60%) |
| `lockedDoorChance` | `0.3f` | Locked door chance (30%) |
| `secretDoorChance` | `0.1f` | Secret door chance (10%) |

### **🕳️ Hole Trap Settings**

| Field | Default | Description |
|-------|---------|-------------|
| `holeTrapChance` | `0.05f` | Hole trap chance (5%) |

### **🏛️ Room Settings**

| Field | Default | Description |
|-------|---------|-------------|
| `generateRooms` | `true` | Enable room generation |
| `minRooms` | `3` | Minimum rooms |
| `maxRooms` | `8` | Maximum rooms |

### **⚙️ Generation Options**

| Field | Default | Description |
|-------|---------|-------------|
| `autoGenerateOnStart` | `true` | Auto-generate on Start |
| `useRandomSeed` | `true` | Use random seed |
| `manualSeed` | `"MazeSeed2026"` | Manual seed (if random disabled) |

---

## 🔧 **VALIDATE PATHS**

Before generating, validate all paths:

**In Unity Editor:**
```
1. Select MazeBuilder GameObject
2. Inspector → Click "Validate Paths"
3. Check Console for results
```

**Expected Output:**
```
[CompleteMazeBuilder] ✅ Wall Prefab: D:/travaux_Unity/PeuImporte/Assets/Prefabs/WallPrefab.prefab
[CompleteMazeBuilder] ✅ Hole Prefab: D:/travaux_Unity/PeuImporte/Assets/Prefabs/HoleTrapPrefab.prefab
[CompleteMazeBuilder] ✅ Door Prefab: D:/travaux_Unity/PeuImporte/Assets/Prefabs/DoorPrefab.prefab
[CompleteMazeBuilder] ✅ Wall Material: D:/travaux_Unity/PeuImporte/Assets/Materials/WallMaterial.mat
[CompleteMazeBuilder] ✅ Floor Material: D:/travaux_Unity/PeuImporte/Assets/Materials/Floor/Stone_Floor.mat
[CompleteMazeBuilder] ✅ All paths validated successfully!
```

---

## 📊 **GENERATION FLOW**

```
┌─────────────────────────────────────────┐
│  1. GenerateMazeLayout()                │
│     - MazeGenerator.Generate()          │
│     - Creates 21x21 grid                │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│  2. SpawnWallsWithHoles()               │
│     - Iterates grid cells               │
│     - Spawns walls at boundaries        │
│     - 5% chance for hole traps          │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│  3. SpawnDoors()                        │
│     - Finds passages (no wall)          │
│     - 60% chance for door               │
│     - 30% locked, 10% secret            │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│  4. SpawnRooms()                        │
│     - Entrance room (start)             │
│     - Exit room (end)                   │
│     - Normal rooms (3-8 total)          │
│     - Each has 1 entrance + 1 exit      │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│  5. PlaceObjects()                      │
│     - SpatialPlacer.PlaceAll()          │
│     - Torches, chests, enemies, items   │
└─────────────────────────────────────────┘
```

---

## 🎯 **EXAMPLE USAGE**

### **Scene Setup:**

```
Hierarchy:
├── MazeBuilder (CompleteMazeBuilder)
│   ├── mazeWidth: 21
│   ├── mazeHeight: 21
│   ├── autoGenerateOnStart: true
│   └── (all paths configured)
├── LightEngine
├── TorchPool
└── Main Camera
```

### **Code Usage:**

```csharp
using Code.Lavos.Core;

public class MazeController : MonoBehaviour
{
    private CompleteMazeBuilder builder;

    void Start()
    {
        builder = GetComponent<CompleteMazeBuilder>();
        
        // Configure maze size
        builder.SetMazeSize(25, 25);
        
        // Set seed for reproducible generation
        builder.SetSeed("MyCustomSeed123");
        
        // Generate
        builder.GenerateCompleteMaze();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Regenerate Maze"))
        {
            builder.GenerateCompleteMaze();
        }
    }
}
```

---

## 🐛 **TROUBLESHOOTING**

### **Issue: Prefabs not found**

**Solution:**
```
1. Run: Tools → Create Maze Prefabs
2. Verify prefabs in Assets/Prefabs/
3. Check paths in Inspector match actual locations
```

### **Issue: Materials not applied**

**Solution:**
```
1. Verify materials exist in Assets/Materials/
2. Check material paths in Inspector
3. Ensure URP shader is available
```

### **Issue: Rooms not spawning**

**Solution:**
```
1. Enable "generateRooms" in Inspector
2. Increase minRooms/maxRooms
3. Check maze size (needs to be large enough)
```

### **Issue: Doors not spawning**

**Solution:**
```
1. Increase doorSpawnChance (default 0.6)
2. Check maze has passages (not all walls)
3. Verify door prefabs exist
```

---

## 📝 **FILES CREATED**

| File | Purpose |
|------|---------|
| `CompleteMazeBuilder.cs` | Main maze builder script |
| `CreateMazePrefabs.cs` | Editor script to create prefabs |
| `complete_maze_builder_guide.md` | This documentation |

**Locations:**
- `Assets/Scripts/Core/06_Maze/CompleteMazeBuilder.cs`
- `Assets/Scripts/Editor/CreateMazePrefabs.cs`
- `Assets/Docs/complete_maze_builder_guide.md`

---

## 🎮 **QUICK START**

**Complete setup in 3 steps:**

```
1. Tools → Create Maze Prefabs
   (Creates all prefabs/materials)

2. Create GameObject "MazeBuilder"
   Add CompleteMazeBuilder component

3. Press Play (auto-generates on start)
   Or click "Generate Complete Maze" in Inspector
```

**That's it!** 🎉

---

**Generated:** 2026-03-04  
**Unity Version:** 6000.3.7f1  
**Status:** ✅ PRODUCTION READY

---

*Document generated - Unity 6 (6000.3.7f1) compatible - UTF-8 encoding - Unix LF*
