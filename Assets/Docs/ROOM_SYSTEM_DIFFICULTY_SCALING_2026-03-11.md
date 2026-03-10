﻿# Room System with Difficulty Scaling - 2026-03-11

**Date:** 2026-03-11  
**Status:** ✅ IMPLEMENTED  
**Unity Version:** 6000.3.10f1  
**Author:** Ocxyde  

---

## 📋 **OVERVIEW**

Implemented a **difficulty-scaled room system** that dynamically adjusts room count, size, and door complexity from minimal (level 0) to maximal (level 39) difficulty.

---

## 🏛️ **ROOM SCALING CURVE**

### **Room Count (MinRooms → MaxRooms)**

| Level | Maze Size | Rooms | Room Size* | Door Types Available |
|-------|-----------|-------|------------|---------------------|
| **0-5** | 13×13 - 17×17 | 2-3 | 3×3 - 5×5 (20%) | Normal only |
| **6-12** | 19×19 - 25×25 | 4-6 | 5×5 - 7×7 (25%) | Normal + Locked (20%) |
| **13-20** | 27×27 - 32×32 | 6-8 | 7×7 - 9×9 (28%) | Normal + Locked (30%) + Secret (10%) |
| **21-30** | 34×34 - 43×43 | 8-10 | 9×9 - 11×11 (30%) | Normal + Locked (30%) + Secret (10%) |
| **31-39** | 45×45 - 51×51 | 10-12 | 11×11 - 13×13 (30%) | Normal + Locked (40%) + Secret (20%) |

\* **Room size is PROPORTIONAL to maze size** (20% at low levels → 30% at high levels)

### **Scaling Formula**

```csharp
// Room count uses power curve (exponent 1.5)
float t = level / MaxLevel;  // 0.0 to 1.0
float curved = Mathf.Pow(t, 1.5f);
int rooms = minRooms + Mathf.RoundToInt((maxRooms - minRooms) * curved);

// Room size is PROPORTIONAL to maze size (NOT fixed steps!)
float ratio = Mathf.Lerp(0.20f, 0.30f, t);  // 20% → 30%
int roomSize = Mathf.RoundToInt(mazeSize * ratio);
// Ensure odd for symmetric center
if (roomSize % 2 == 0) roomSize++;
```

**Why proportional sizing?**
- ✅ Small mazes (level 0) get small rooms (3×3-5×5) - cozy, manageable
- ✅ Large mazes (level 39) get large rooms (11×11-13×13) - epic boss battles
- ✅ Room size matters: larger rooms = more dangerous (space for enemies/traps)
- ✅ No fixed steps - smooth organic scaling with maze difficulty

---

## 🚪 **DOOR SYSTEM**

### **Door Opening Design**

```
┌─────────────┬─────────────┬─────────────┐
│             │             │             │
│   Room A    │   Room B    │   Room C    │
│   (5×5)     │   (7×7)     │   (9×9)     │
│             │             │             │
├───────┬─────┼──────┬──────┼─────┬───────┤
│       │▒▒▒▒▒▒▒▒▒▒▒▒│▒▒▒▒▒▒▒▒▒▒▒▒│       │
│ Wall  │ DOOR  │ Wall  │ DOOR  │  Wall   │
│       │_______│       │_______│         │
└───────┴───────┴───────┴───────┴─────────┘
        ↑               ↑
    3-unit opening  3-unit opening
```

### **Door Opening Specifications**

| Property | Value |
|----------|-------|
| **Opening Width** | 3 units (configurable) |
| **Wall Remaining** | 1 unit on each side |
| **Door Prefab Pivot** | Bottom-center of opening |
| **Snap Precision** | Perfect grid alignment |

### **Door Types**

| Type | Appearance | Behavior | Unlock |
|------|------------|----------|--------|
| **Normal** | Standard wood/metal | Opens both ways | Always |
| **Locked** | Reinforced, locked | Requires key | Level 6+ |
| **Secret** | Blends with wall | Hidden trigger | Level 16+ |

---

## 🗺️ **ROOM PLACEMENT STRATEGY**

### **Placement Algorithm**

1. **Find Valid Positions**
   - Between spawn and exit (not too close)
   - In wall areas (not existing passages)
   - Away from maze edges

2. **Carve Room**
   - Clear N×N area (all walls removed)
   - Size based on level (5×5 → 11×11)

3. **Create Door Openings**
   - 3-unit wide openings on opposite walls
   - North-South OR East-West orientation
   - Mark door positions for spawning

4. **Assign Door Type**
   - Roll against locked/secret chances
   - Mark door type for prefab selection

---

## 📊 **CONFIGURATION (JSON)**

### **MazeConfig Updates**

```json
{
  "MazeConfig": {
    "MinRooms": 2,
    "MaxRooms": 12,
    "BaseRoomSize": 5,
    "DoorOpeningWidth": 3
  }
}
```

### **DifficultyScaler Additions**

```csharp
// Room count scaling
public int RoomCount(int minRooms, int maxRooms, int level)

// Room size scaling
public int RoomSize(int baseRoomSize, int level)

// Door type chances
public float LockedDoorChance(int level)
public float SecretDoorChance(int level)
```

---

## 🔧 **CODE CHANGES**

### **File: `DifficultyScaler.cs`**

**Added Methods:**
```csharp
RoomCount(minRooms, maxRooms, level)      // Returns: 2 → 12 rooms
RoomSize(baseRoomSize, level)              // Returns: 5 → 11 size
LockedDoorChance(level)                    // Returns: 0% → 40%
SecretDoorChance(level)                    // Returns: 0% → 20%
```

### **File: `GridMazeGenerator.cs`**

**Added Methods:**
```csharp
CarveIntermediateRoomsWithDoors(...)       // Main room generation
CarveRoom(...)                             // Clear N×N area
CreateDoorOpening(...)                     // Create 3-unit opening
MarkDoorPositions(...)                     // Store door locations
```

**Added Enum:**
```csharp
enum DoorType { Normal, Locked, Secret }
```

**Updated Config:**
```csharp
MazeConfig.MinRooms         // Default: 2
MazeConfig.MaxRooms         // Default: 12
MazeConfig.BaseRoomSize     // Default: 5
MazeConfig.DoorOpeningWidth // Default: 3
```

---

## 🧪 **TESTING CHECKLIST**

### **In Unity Editor:**

1. **Level 0 Test (Minimal)**
   - Generate maze at level 0
   - **Verify:**
     - ✅ 2-3 rooms (5×5 size)
     - ✅ Normal doors only
     - ✅ Rooms between spawn and exit
     - ✅ Door openings align with corridors

2. **Level 12 Test (Mid-Low)**
   - Generate maze at level 12
   - **Verify:**
     - ✅ 4-6 rooms (7×7 size)
     - ✅ 20% locked doors
     - ✅ No secret doors yet
     - ✅ Rooms have clear entrances/exits

3. **Level 20 Test (Mid-High)**
   - Generate maze at level 20
   - **Verify:**
     - ✅ 6-8 rooms (9×9 size)
     - ✅ 30% locked doors
     - ✅ 10% secret doors
     - ✅ Good room distribution

4. **Level 39 Test (Maximum)**
   - Generate maze at level 39
   - **Verify:**
     - ✅ 10-12 rooms (11×11 size)
     - ✅ 40% locked doors
     - ✅ 20% secret doors
     - ✅ Boss room size (largest)

---

## 📈 **EXPECTED CONSOLE OUTPUT**

### **Level 0 (Minimal):**
```
[GridMazeGenerator] LEVEL 0 | factor=1.000 | size=13 | rooms=2 (size=5)
[GridMazeGenerator] Step 5.5: Carving 2 rooms (size=5×5) at Level 0
[GridMazeGenerator] Door types: locked=0%, secret=0%
[GridMazeGenerator] Room #1 carved at (5,3), size=5×5, doorType=Normal
[GridMazeGenerator]   Doors at: North(5,1), South(5,5)
[GridMazeGenerator] Room #2 carved at (9,8), size=5×5, doorType=Normal
[GridMazeGenerator]   Doors at: West(7,8), East(11,8)
[GridMazeGenerator] Rooms carved: 2/2 (attempts=3)
```

### **Level 20 (Mid-High):**
```
[GridMazeGenerator] LEVEL 20 | factor=1.520 | size=32 | rooms=7 (size=9)
[GridMazeGenerator] Step 5.5: Carving 7 rooms (size=9×9) at Level 20
[GridMazeGenerator] Door types: locked=30%, secret=10%
[GridMazeGenerator] Room #1 carved at (8,6), size=9×9, doorType=Locked
[GridMazeGenerator] Room #2 carved at (15,12), size=9×9, doorType=Normal
[GridMazeGenerator] Room #3 carved at (22,18), size=9×9, doorType=Secret
[GridMazeGenerator] Rooms carved: 7/7 (attempts=9)
```

### **Level 39 (Maximum):**
```
[GridMazeGenerator] LEVEL 39 | factor=3.000 | size=51 | rooms=12 (size=11)
[GridMazeGenerator] Step 5.5: Carving 12 rooms (size=11×11) at Level 39
[GridMazeGenerator] Door types: locked=40%, secret=20%
[GridMazeGenerator] Rooms carved: 12/12 (attempts=15)
```

---

## 🎮 **GAMEPLAY IMPACT**

### **Early Game (Levels 0-5)**
- ✅ Simple mazes with 2-3 small rooms
- ✅ Easy navigation (normal doors only)
- ✅ Quick learning curve

### **Mid Game (Levels 6-20)**
- ✅ Increasing complexity (4-8 rooms)
- ✅ Key hunting gameplay (locked doors)
- ✅ Exploration rewards (secret doors)

### **Late Game (Levels 21-39)**
- ✅ Complex mazes (10-12 large rooms)
- ✅ Strategic key management
- ✅ Hidden boss rooms (secret doors)

---

## 🔍 **DIAGNOSTIC GUIDE**

### **Issue: "No rooms appearing"**

**Check Console for:**

| Log Message | Problem | Solution |
|-------------|---------|----------|
| `Rooms carved: 0/2` | No valid positions | Check margin calculation |
| `Door types: locked=0%` | Low level | Expected at levels 0-5 |
| `attempts=6` | Early termination | Increase maxAttempts |

---

## 📝 **NEXT STEPS**

### **Immediate:**
1. Test room generation at all levels (0, 12, 20, 39)
2. Verify door opening alignment with corridors
3. Check room distribution is balanced

### **Phase 2 (Door Prefabs):**
1. Create door prefabs (Normal, Locked, Secret)
2. Implement door spawning in CompleteMazeBuilder
3. Add door animation (swing/slide)
4. Implement key system for locked doors

### **Phase 3 (Room Variety):**
1. Add room themes (Stone, Brick, Dungeon)
2. Implement boss rooms (larger, decorated)
3. Add treasure rooms (high loot chance)
4. Create trap rooms (hazardous)

---

## 📚 **RELATED FILES**

| File | Purpose |
|------|---------|
| `DifficultyScaler.cs` | Scaling logic (UPDATED) |
| `GridMazeGenerator.cs` | Room generation (UPDATED) |
| `MazeConfig.cs` | Configuration (UPDATED) |
| `CompleteMazeBuilder8.cs` | Door spawning (TODO) |
| `DoorPrefab.prefab` | Door prefabs (TODO) |

---

## 🎯 **COMPLIANCE**

| Standard | Status |
|----------|--------|
| **Unity 6 Naming** | ✅ camelCase/PascalCase |
| **UTF-8 Encoding** | ✅ |
| **Unix LF Line Endings** | ✅ |
| **No Emojis in C#** | ✅ |
| **GPL-3.0 License** | ✅ Headers present |

---

## 📊 **SCALING SUMMARY**

| Level | Rooms | Size | Locked | Secret | Complexity |
|-------|-------|------|--------|--------|------------|
| **0** | 2 | 5×5 | 0% | 0% | Minimal |
| **6** | 4 | 7×7 | 20% | 0% | Low |
| **12** | 6 | 7×7 | 20% | 0% | Medium |
| **16** | 7 | 9×9 | 30% | 10% | Medium-High |
| **20** | 8 | 9×9 | 30% | 10% | High |
| **30** | 10 | 9×9 | 30% | 10% | Very High |
| **39** | 12 | 11×11 | 40% | 20% | Maximum |

---

**Last Updated:** 2026-03-11  
**Document Version:** 1.0  
**Status:** ✅ IMPLEMENTED - READY FOR TESTING

*Happy coding, coder friend!*
