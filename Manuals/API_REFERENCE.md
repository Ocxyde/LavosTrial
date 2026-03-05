# Modding API Reference

**Version:** 1.0
**Date:** 2026-03-05
**For:** Modders and Content Creators

---

## 📖 **TABLE OF CONTENTS**

1. [Overview](#overview)
2. [Configuration](#configuration)
3. [Prefab Reference](#prefab-reference)
4. [Script Reference](#script-reference)
5. [Events](#events)
6. [Examples](#examples)

---

## 🎮 **OVERVIEW**

The CompleteMazeBuilder modding API allows you to:
- ✅ Customize maze generation
- ✅ Add custom prefabs
- ✅ Modify game balance
- ✅ Create custom rooms
- ✅ Add new door types
- ✅ Extend functionality

---

## ⚙️ **CONFIGURATION**

### **Config File Location:**
```
<ProjectRoot>/Config/GameConfig-default.json
```

### **Basic Configuration:**

```json
{
    // Prefab Paths (Required)
    "wallPrefab": "Prefabs/WallPrefab.prefab",
    "doorPrefab": "Prefabs/DoorPrefab.prefab",
    "entranceRoomPrefab": "Prefabs/EntranceRoomPrefab.prefab",
    "exitRoomPrefab": "Prefabs/ExitRoomPrefab.prefab",
    "normalRoomPrefab": "Prefabs/NormalRoomPrefab.prefab",
    
    // Material/Texture Paths (Required)
    "wallMaterial": "Materials/WallMaterial.mat",
    "floorMaterial": "Materials/Floor/Stone_Floor.mat",
    "groundTexture": "Textures/floor_texture.png",
    "wallTexture": "Textures/wall_texture.png",
    "ceilingTexture": "Textures/ceiling_texture.png",
    
    // Maze Generation (Optional)
    "defaultMazeWidth": 21,
    "defaultMazeHeight": 21,
    "defaultCellSize": 6.0
}
```

### **Advanced Configuration (Hidden Features):**

```json
{
    // Game Balance
    "damageScale": 1.0,
    "healthScale": 1.0,
    "enemyHealthScale": 1.0,
    "speedScale": 1.0,
    "staminaDrainScale": 1.0,
    
    // Graphics/Audio
    "graphicsQuality": "Medium",
    "soundVolume": 0.8,
    "musicVolume": 0.6,
    "mouseSensitivity": 1.0,
    "showHUD": true
}
```

**Note:** Some features are intentionally undocumented. Experiment and discover! 🤫

---

## 📦 **PREFAB REFERENCE**

### **Required Prefabs:**

| Prefab | Purpose | Path |
|--------|---------|------|
| **WallPrefab** | Maze walls | `Prefabs/WallPrefab.prefab` |
| **DoorPrefab** | Normal doors | `Prefabs/DoorPrefab.prefab` |
| **LockedDoorPrefab** | Locked doors | `Prefabs/LockedDoorPrefab.prefab` |
| **SecretDoorPrefab** | Secret doors | `Prefabs/SecretDoorPrefab.prefab` |
| **EntranceRoomPrefab** | Entrance room | `Prefabs/EntranceRoomPrefab.prefab` |
| **ExitRoomPrefab** | Exit room | `Prefabs/ExitRoomPrefab.prefab` |
| **NormalRoomPrefab** | Normal rooms | `Prefabs/NormalRoomPrefab.prefab` |

### **Creating Custom Prefabs:**

1. **Create your prefab** in Unity
2. **Save to** `Assets/Prefabs/`
3. **Update path** in `GameConfig-default.json`
4. **Restart** Unity

**Example:**
```json
{
    "wallPrefab": "Prefabs/MyCustomWall.prefab"
}
```

---

## 📜 **SCRIPT REFERENCE**

### **Main Scripts:**

| Script | Purpose | Location |
|--------|---------|----------|
| **CompleteMazeBuilder** | Main maze generator | `Assets/Scripts/Core/06_Maze/` |
| **GameConfig** | Config loader | `Assets/Scripts/Core/06_Maze/` |
| **MazeSaveData** | Save system | `Assets/Scripts/Core/06_Maze/` |
| **MazeGenerator** | Maze algorithm | `Assets/Scripts/Core/06_Maze/` |
| **MazeRenderer** | 3D visualization | `Assets/Scripts/Core/06_Maze/` |

### **Key Classes:**

#### **CompleteMazeBuilder**
```csharp
// Access in code
var mazeBuilder = GetComponent<CompleteMazeBuilder>();

// Generate maze
mazeBuilder.GenerateCompleteMaze();

// Validate paths
mazeBuilder.ValidatePaths();
```

#### **GameConfig**
```csharp
// Access config
var config = GameConfig.Instance;

// Read values
float damage = config.damageScale;
int mazeWidth = config.defaultMazeWidth;

// Save changes
GameConfig.Save();
```

#### **MazeSaveData**
```csharp
// Save maze data
MazeSaveData.SaveMazeData(seed, spawnX, spawnZ, width, height);

// Load maze data
var data = MazeSaveData.LoadMazeData();

// Save player settings
MazeSaveData.SavePlayerSettings("GraphicsQuality", "Ultra");

// Load player settings
string quality = MazeSaveData.LoadPlayerSetting("GraphicsQuality", "Medium");
```

---

## ⚡ **EVENTS**

### **EventHandler Integration:**

The maze system integrates with the central EventHandler:

```csharp
// Subscribe to events
private void OnEnable()
{
    if (EventHandler.Instance != null)
    {
        EventHandler.Instance.OnGameStateChanged += OnGameStateChanged;
    }
}

// Unsubscribe on disable
private void OnDisable()
{
    if (EventHandler.Instance != null)
    {
        EventHandler.Instance.OnGameStateChanged -= OnGameStateChanged;
    }
}

// Handle events
private void OnGameStateChanged(GameManager.GameState newState)
{
    if (newState == GameManager.GameState.Playing)
    {
        // Game started
    }
    else if (newState == GameManager.GameState.Paused)
    {
        // Game paused
    }
}
```

---

## 📝 **EXAMPLES**

### **Example 1: Custom Maze Size**

**Config:**
```json
{
    "defaultMazeWidth": 31,
    "defaultMazeHeight": 31,
    "defaultCellSize": 8.0
}
```

**Result:** Larger maze (31x31 cells, 8m per cell)

---

### **Example 2: Custom Wall Texture**

**Steps:**
1. Create your wall texture
2. Save to `Assets/Textures/MyWallTexture.png`
3. Update config:
```json
{
    "wallTexture": "Textures/MyWallTexture.png"
}
```

**Result:** Your custom texture on all walls

---

### **Example 3: God Mode (Hidden Feature)**

**Config:**
```json
{
    "godMode": true,
    "infiniteStamina": true
}
```

**Result:** Invincible + unlimited stamina! 🔓

---

### **Example 4: Custom Room**

**Steps:**
1. Create room prefab in Unity
2. Add required components (colliders, etc.)
3. Save to `Assets/Prefabs/MyCustomRoom.prefab`
4. Update config:
```json
{
    "normalRoomPrefab": "Prefabs/MyCustomRoom.prefab"
}
```

**Result:** Your custom room spawns in maze!

---

### **Example 5: Save/Load Player Settings**

**Save:**
```csharp
MazeSaveData.SavePlayerSettings("MouseSensitivity", "2.5");
MazeSaveData.SavePlayerSettings("GraphicsQuality", "Ultra");
```

**Load:**
```csharp
float sensitivity = float.Parse(
    MazeSaveData.LoadPlayerSetting("MouseSensitivity", "1.0")
);

string quality = MazeSaveData.LoadPlayerSetting(
    "GraphicsQuality", 
    "Medium"
);
```

---

## 🛠️ **MODDING TIPS**

### **Tip 1: Backup Config**
Before editing `GameConfig-default.json`, make a backup copy!

### **Tip 2: Test Small Changes**
Start with small config changes (maze size, etc.) before creating custom prefabs.

### **Tip 3: Check Console**
Unity Console shows helpful error messages if something goes wrong.

### **Tip 4: Use Comments**
JSON config supports comments! Use `//` to document your changes:
```json
{
    // My custom maze size
    "defaultMazeWidth": 31,
    "defaultMazeHeight": 31
}
```

### **Tip 5: Experiment!**
Some features are intentionally undocumented. Experiment and discover what's possible! 🤫

---

## 🐛 **TROUBLESHOOTING**

### **Issue: Pink Textures**
**Cause:** Prefab path incorrect
**Solution:** Check path in `GameConfig-default.json`

### **Issue: Maze Not Generating**
**Cause:** Missing required component
**Solution:** Run `Tools → Maze → Generate Maze`

### **Issue: Config Not Loading**
**Cause:** JSON syntax error
**Solution:** Validate JSON syntax (use online validator)

### **Issue: Mod Not Working**
**Cause:** Restart required
**Solution:** Restart Unity after config changes

---

## 📞 **SUPPORT**

### **Documentation:**
- Check `Manuals/MANUAL.md` for user guide
- Check `Manuals/TEST_CHECKLIST.md` for testing
- Read code comments (well documented!)

### **Community:**
- Share your mods!
- Create custom mazes!
- Experiment with config!

---

**API Reference Generated:** 2026-03-05
**Version:** 1.0
**Status:** ✅ PRODUCTION READY

---

*Happy Modding! 🎮🔧*
