# Default Prefab Values - First Time Game

**Date:** 2026-03-05
**Status:** ✅ PRODUCTION READY
**Feature:** Auto-default prefabs for new games

---

## 📦 **DEFAULT PREFAB VALUES**

### **For First-Time Games:**

When player starts game for **FIRST TIME** (no SQLite save exists):

```csharp
// DEFAULT PREFABS (automatically applied)
Wall        → "Prefabs/WallPrefab.prefab"
Door        → "Prefabs/DoorPrefab.prefab"
LockedDoor  → "Prefabs/LockedDoorPrefab.prefab"
SecretDoor  → "Prefabs/SecretDoorPrefab.prefab"
EntranceRoom → "Prefabs/EntranceRoomPrefab.prefab"
ExitRoom    → "Prefabs/ExitRoomPrefab.prefab"
NormalRoom  → "Prefabs/NormalRoomPrefab.prefab"

// DEFAULT MATERIALS
Wall        → "Materials/WallMaterial.mat"
Door        → "Materials/Door_PïxelArt.mat"
Floor       → "Materials/Floor/Stone_Floor.mat"

// DEFAULT TEXTURES
Ground      → "Textures/floor_texture.png"
Wall        → "Textures/wall_texture.png"
Ceiling     → "Textures/ceiling_texture.png"
```

---

## 🎮 **HOW IT WORKS**

### **First Time Game (No Save):**
```
1. Game starts
2. Load from SQLite → NULL (no save)
3. Generate NEW seed
4. Apply DEFAULT prefab values
5. Generate NEW maze
6. Save defaults to SQLite
7. Player plays with defaults
```

### **Subsequent Games (Has Save):**
```
1. Game starts
2. Load from SQLite → EXISTS
3. Check seed match
4. If match: use saved spawn
5. If mismatch: generate NEW maze
6. Load player's prefab choices (override defaults)
7. Player plays with their choices
```

---

## 💾 **SQLITE STORAGE**

### **First Save:**
```sql
INSERT INTO PrefabData (prefabName, prefabPath, timestamp)
VALUES 
  ('Wall', 'Prefabs/WallPrefab.prefab', datetime('now')),
  ('Door', 'Prefabs/DoorPrefab.prefab', datetime('now')),
  ...
```

### **Player Overrides:**
```sql
-- Player changes wall prefab in Inspector
INSERT OR REPLACE INTO PrefabData (prefabName, prefabPath, timestamp)
VALUES ('Wall', 'Prefabs/CustomWallPrefab.prefab', datetime('now'))
```

### **Next Load:**
```sql
-- Load player's choice (overrides default)
SELECT prefabPath FROM PrefabData WHERE prefabName = 'Wall'
-- Returns: 'Prefabs/CustomWallPrefab.prefab'
```

---

## 🔧 **CODE IMPLEMENTATION**

### **Default Values (Static Constants):**
```csharp
private static readonly Dictionary<string, string> DEFAULT_PREFABS = new Dictionary<string, string>
{
    { "Wall", "Prefabs/WallPrefab.prefab" },
    { "Door", "Prefabs/DoorPrefab.prefab" },
    { "LockedDoor", "Prefabs/LockedDoorPrefab.prefab" },
    { "SecretDoor", "Prefabs/SecretDoorPrefab.prefab" },
    { "EntranceRoom", "Prefabs/EntranceRoomPrefab.prefab" },
    { "ExitRoom", "Prefabs/ExitRoomPrefab.prefab" },
    { "NormalRoom", "Prefabs/NormalRoomPrefab.prefab" }
};

private static readonly Dictionary<string, string> DEFAULT_MATERIALS = new Dictionary<string, string>
{
    { "Wall", "Materials/WallMaterial.mat" },
    { "Door", "Materials/Door_PïxelArt.mat" },
    { "Floor", "Materials/Floor/Stone_Floor.mat" },
    { "GroundTexture", "Textures/floor_texture.png" },
    { "WallTexture", "Textures/wall_texture.png" },
    { "CeilingTexture", "Textures/ceiling_texture.png" }
};
```

### **Apply Defaults (First Time):**
```csharp
private void ApplyDefaultPrefabs()
{
    // Apply default prefab paths if not set
    if (string.IsNullOrEmpty(wallPrefabPath)) wallPrefabPath = DEFAULT_PREFABS["Wall"];
    if (string.IsNullOrEmpty(doorPrefabPath)) doorPrefabPath = DEFAULT_PREFABS["Door"];
    // ... etc for all prefabs
    
    Debug.Log("[CompleteMazeBuilder] 📦 Applied DEFAULT prefab/material values for first-time game");
}
```

### **Save with Defaults:**
```csharp
private void SaveSpawnPosition(int cellX, int cellZ, int seed)
{
    // Save prefab assignments (Inspector values OR defaults)
    var prefabs = new Dictionary<string, string>
    {
        { "Wall", string.IsNullOrEmpty(wallPrefabPath) ? DEFAULT_PREFABS["Wall"] : wallPrefabPath },
        { "Door", string.IsNullOrEmpty(doorPrefabPath) ? DEFAULT_PREFABS["Door"] : doorPrefabPath },
        // ... etc
    };
    MazeSaveData.SaveAllPrefabData(prefabs);
    
    Debug.Log("[CompleteMazeBuilder] 💾 Prefab/Material defaults saved for first-time game");
}
```

---

## ✅ **FEATURES**

| Feature | Status |
|---------|--------|
| **Default prefabs** | ✅ Applied on first game |
| **Default materials** | ✅ Applied on first game |
| **Default textures** | ✅ Applied on first game |
| **Save to SQLite** | ✅ Defaults saved |
| **Player override** | ✅ Overrides saved |
| **Load priority** | ✅ Player > Default |
| **No pink textures** | ✅ Defaults prevent pink |
| **No manual setup** | ✅ Works out-of-box |

---

## 🎯 **LOAD PRIORITY**

```
Player's Saved Choices (SQLite) > Inspector Values > Default Values
```

### **Example:**
```
1. First game: Uses defaults
   Wall = "Prefabs/WallPrefab.prefab" (default)

2. Player changes in Inspector:
   Wall = "Prefabs/CustomWall.prefab"

3. Save to SQLite:
   Wall = "Prefabs/CustomWall.prefab" (player choice)

4. Next game:
   Wall = "Prefabs/CustomWall.prefab" (loaded from SQLite)
```

---

## 📋 **BENEFITS**

### **For Players:**
- ✅ **Works immediately** - no setup required
- ✅ **No pink textures** - defaults prevent missing prefabs
- ✅ **Customizable** - can override defaults
- ✅ **Persistent** - choices saved to SQLite

### **For Developers:**
- ✅ **Easy testing** - works out-of-box
- ✅ **Clean defaults** - centralized in code
- ✅ **Flexible** - players can customize
- ✅ **Professional** - proper save system

---

## 🎮 **USAGE**

### **First Time (No Setup Needed):**
```
1. Tools → Maze → Generate Maze
2. Maze generates with DEFAULT prefabs
3. Everything works (no pink textures!)
4. Defaults saved to SQLite
```

### **Customize (Optional):**
```
1. Assign custom prefabs in Inspector
2. Tools → Maze → Generate Maze
3. Custom prefabs used
4. Custom choices saved to SQLite
```

### **Reset to Defaults:**
```
1. Clear Inspector fields (leave empty)
2. Tools → Maze → Generate Maze
3. Defaults applied again
```

---

## ✅ **VERIFICATION**

### **First Game Log:**
```
[CompleteMazeBuilder] 💾 No stored maze data - will generate NEW maze with NEW seed (FIRST TIME GAME)
[CompleteMazeBuilder] 📦 Using DEFAULT prefab/material values
[CompleteMazeBuilder] 📦 Applied DEFAULT prefab/material values for first-time game
[CompleteMazeBuilder] 💾 Maze saved to SQLite: Seed=12345, Spawn=(4, 6)
[CompleteMazeBuilder] 💾 Prefab/Material defaults saved for first-time game
```

### **Subsequent Game Log:**
```
[CompleteMazeBuilder] 💾 Loaded maze data: Seed=12345, Spawn=(4, 6)
[CompleteMazeBuilder] 📂 Prefab data loaded: Wall = Prefabs/CustomWall.prefab
[CompleteMazeBuilder] ✅ Using player's saved prefab choices
```

---

## 🎉 **FINAL RESULT**

**Default Prefab System is:**
- ✅ **Automatic** (applied on first game)
- ✅ **Persistent** (saved to SQLite)
- ✅ **Overrideable** (players can customize)
- ✅ **Professional** (no pink textures)
- ✅ **Easy to use** (works out-of-box)
- ✅ **Clean code** (centralized defaults)

---

**Generated:** 2026-03-05
**Unity Version:** 6000.3.7f1
**Status:** ✅ PRODUCTION READY

---

*Document generated - Unity 6 compatible - UTF-8 encoding - Unix LF*
