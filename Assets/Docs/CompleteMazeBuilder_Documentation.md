# CompleteMazeBuilder - Documentation

**Date:** 2026-03-04
**Unity Version:** 6000.3.7f1
**Location:** Assets/Scripts/Core/06_Maze/

---

## 🎯 **OVERVIEW**

CompleteMazeBuilder is a **plug-in-out compliant** maze generation system that creates complete, playable mazes with:
- Walls with textures
- Ground floor
- Ceiling
- Doors (normal, locked, secret)
- Rooms (entrance, exit, normal)
- Player spawn inside entrance room
- Torches with dynamic lighting

---

## 🏗️ **PLUG-IN-OUT COMPLIANCE**

### **EventHandler Integration:**
```csharp
// Subscribes to core events
private void OnEnable()
{
    if (eventHandler != null)
    {
        eventHandler.OnGameStateChanged += OnGameStateChanged;
    }
}

// Unsubscribes on disable
private void OnDisable()
{
    if (eventHandler != null)
    {
        eventHandler.OnGameStateChanged -= OnGameStateChanged;
    }
}
```

### **Independent Module:**
- ✅ Can be added/removed safely
- ✅ No direct dependencies on other systems
- ✅ Uses EventHandler for communication
- ✅ Publishes events on completion

---

## 📁 **FOLDER STRUCTURE**

```
Assets/Scripts/
├── Core/06_Maze/
│   └── CompleteMazeBuilder.cs    ← Main generator
│
├── Editor/
│   └── MazeBuilderEditor.cs      ← Editor tools
│
└── Docs/
    └── CompleteMazeBuilder.md    ← This file
```

---

## 🎮 **USAGE**

### **In Editor:**
1. **Tools → Maze → Generate Maze** (Ctrl+Alt+G)
2. Maze generates automatically
3. Press Play to explore

### **In Code:**
```csharp
var mazeBuilder = GetComponent<CompleteMazeBuilder>();
mazeBuilder.GenerateCompleteMaze();
```

---

## ⚙️ **INSPECTOR SETTINGS**

### **Maze Dimensions:**
- `mazeWidth`: 21 (cells)
- `mazeHeight`: 21 (cells)
- `cellSize`: 6m
- `wallHeight`: 4m
- `wallThickness`: 0.5m
- `ceilingHeight`: 5m

### **Door Settings:**
- `doorSpawnChance`: 0.6 (60%)
- `lockedDoorChance`: 0.3 (30%)
- `secretDoorChance`: 0.1 (10%)

### **Room Settings:**
- `generateRooms`: true
- `minRooms`: 3
- `maxRooms`: 8

### **Player Spawn:**
- `spawnRoomCellX`: 2 (center of 3x3 room)
- `spawnRoomCellZ`: 2 (center of 3x3 room)

---

## 🏰 **ROOM SPAWN SYSTEM**

### **Why Spawn Inside Room?**
- ✅ **Guaranteed clear space** (no walls inside rooms)
- ✅ **Safe spawn** (player won't clip into walls)
- ✅ **Consistent** (always spawns in entrance room)
- ✅ **Changes with seed** (different maze = different room position)

### **Room Layout:**
```
┌───┬───┬───┐
│ W │ W │ W │
├───┼───┼───┤
│ W │ ● │ W │  ← Spawn at ● (cell 2,2)
├───┼───┼───┤
│ W │ ▓ │ W │  ← Exit through ▓ (door)
└───┴───┴───┘
```

---

## 🔌 **EVENT INTEGRATION**

### **Subscribes To:**
- `OnGameStateChanged` - Handles play/pause states

### **Publishes:**
- `MazeGenerated` (via EventHandler) - Notifies other systems

---

## 🛠️ **EDITOR TOOLS**

### **Menu Items:**
- **Tools → Maze → Generate Maze** (Ctrl+Alt+G)
- **Tools → Maze → Validate Paths**
- **Tools → Maze → Clear Maze Objects**
- **Tools → Maze → Show Documentation**
- **Tools → Maze → Maze Builder Window**

---

## 📋 **GENERATION ORDER**

1. Generate maze layout
2. Spawn ground floor
3. Spawn rooms (entrance, exit, normal)
4. **Spawn player INSIDE entrance room** ✅
5. Spawn ceiling
6. Spawn walls (inner + outer perimeter)
7. Spawn doors
8. Place torches, chests, enemies, items

---

## ✅ **VERIFICATION**

### **Console Output:**
```
[CompleteMazeBuilder] 🔌 Connected to EventHandler
[CompleteMazeBuilder] 🏗️ Component initialized
[CompleteMazeBuilder] 📐 Set MazeGenerator size: 21x21
[CompleteMazeBuilder] 🏛️ Generating 5 rooms...
[CompleteMazeBuilder] 🎯 Spawn cell (2, 2) → World position: (15.00, 0.90, 15.00)
[CompleteMazeBuilder] 🏰 Room interior is CLEAR - no walls inside!
[CompleteMazeBuilder] 👤 Player spawned INSIDE entrance room: (15.00, 0.90, 15.00)
[CompleteMazeBuilder] 🧱 Walls: 882 inner + 84 outer = 966 segments
[CompleteMazeBuilder] 🎆 Torches placed via SpatialPlacer + TorchPool
[CompleteMazeBuilder] ✅ Maze generation complete!
```

### **Visual Checks:**
- ✅ Player inside 3x3 room (clear space)
- ✅ Can walk around inside room
- ✅ Can exit through room door
- ✅ Torches on walls with animated flames
- ✅ Maze fully enclosed with outer walls

---

## 🎯 **PLUG-IN-OUT CHECKLIST**

| Requirement | Status |
|-------------|--------|
| EventHandler Integration | ✅ Subscribes/Unsubscribes |
| No Direct Dependencies | ✅ Uses events |
| Independent Module | ✅ Can disable safely |
| Central Hub Communication | ✅ Publishes events |
| GameManager Pivot | ✅ Revolves around hub |
| Proper Folder Structure | ✅ Core/, Editor/, Tests/ |

---

**Generated:** 2026-03-04
**Status:** ✅ PRODUCTION READY - PLUG-IN-OUT COMPLIANT

---

*Document generated - Unity 6 (6000.3.7f1) compatible - UTF-8 encoding - Unix LF*
