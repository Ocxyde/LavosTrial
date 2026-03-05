# Configuration Guide for Modders

**Version:** 1.0
**Date:** 2026-03-05
**For:** Modders and Content Creators

---

## 📖 **TABLE OF CONTENTS**

1. [Config File Location](#config-file-location)
2. [Basic Configuration](#basic-configuration)
3. [Advanced Configuration](#advanced-configuration)
4. [JSON Comments](#json-comments)
5. [Examples](#examples)
6. [Troubleshooting](#troubleshooting)

---

## 📁 **CONFIG FILE LOCATION**

```
<ProjectRoot>/Config/GameConfig-default.json
```

**Example:**
```
D:\travaux_Unity\PeuImporte\Config\GameConfig-default.json
```

---

## ⚙️ **BASIC CONFIGURATION**

### **Minimal Config (Default):**
```json
{
    "wallPrefab": "Prefabs/WallPrefab.prefab",
    "doorPrefab": "Prefabs/DoorPrefab.prefab",
    "entranceRoomPrefab": "Prefabs/EntranceRoomPrefab.prefab",
    "exitRoomPrefab": "Prefabs/ExitRoomPrefab.prefab",
    "normalRoomPrefab": "Prefabs/NormalRoomPrefab.prefab",
    "wallMaterial": "Materials/WallMaterial.mat",
    "floorMaterial": "Materials/Floor/Stone_Floor.mat",
    "groundTexture": "Textures/floor_texture.png",
    "wallTexture": "Textures/wall_texture.png",
    "ceilingTexture": "Textures/ceiling_texture.png",
    "defaultMazeWidth": 21,
    "defaultMazeHeight": 21,
    "defaultCellSize": 6.0
}
```

### **Required Fields:**

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| **wallPrefab** | string | Required | Path to wall prefab |
| **doorPrefab** | string | Required | Path to door prefab |
| **entranceRoomPrefab** | string | Required | Path to entrance room |
| **exitRoomPrefab** | string | Required | Path to exit room |
| **normalRoomPrefab** | string | Required | Path to normal room |
| **wallMaterial** | string | Required | Path to wall material |
| **floorMaterial** | string | Required | Path to floor material |
| **groundTexture** | string | Required | Path to ground texture |
| **wallTexture** | string | Required | Path to wall texture |
| **ceilingTexture** | string | Required | Path to ceiling texture |
| **defaultMazeWidth** | int | 21 | Maze width in cells |
| **defaultMazeHeight** | int | 21 | Maze height in cells |
| **defaultCellSize** | float | 6.0 | Cell size in meters |

---

## 🔧 **ADVANCED CONFIGURATION**

### **Game Balance:**
```json
{
    "damageScale": 1.0,
    "healthScale": 1.0,
    "enemyHealthScale": 1.0,
    "speedScale": 1.0,
    "staminaDrainScale": 1.0
}
```

### **Graphics/Audio:**
```json
{
    "graphicsQuality": "Medium",
    "soundVolume": 0.8,
    "musicVolume": 0.6,
    "mouseSensitivity": 1.0,
    "showHUD": true
}
```

### **Hidden Features (Discover Yourself!):**
```json
{
    // Some features are intentionally undocumented...
    // Experiment and discover what's possible! 🤫
}
```

---

## 💬 **JSON COMMENTS**

The config file **supports comments!** Lines starting with `//` are ignored:

```json
{
    // My custom maze size
    "defaultMazeWidth": 31,
    "defaultMazeHeight": 31,
    
    // Larger cells for bigger maze
    "defaultCellSize": 8.0,
    
    // Custom wall texture
    "wallTexture": "Textures/MyCustomWall.png"
}
```

**Comments are stripped on load** - they don't affect the game!

---

## 📝 **EXAMPLES**

### **Example 1: Tiny Maze (Testing)**
```json
{
    "defaultMazeWidth": 11,
    "defaultMazeHeight": 11,
    "defaultCellSize": 6.0
}
```

**Result:** Small 11x11 maze (quick testing)

---

### **Example 2: Huge Maze**
```json
{
    "defaultMazeWidth": 51,
    "defaultMazeHeight": 51,
    "defaultCellSize": 6.0
}
```

**Result:** Massive 51x51 maze (hardcore!)

---

### **Example 3: Wide Corridors**
```json
{
    "defaultMazeWidth": 21,
    "defaultMazeHeight": 21,
    "defaultCellSize": 10.0
}
```

**Result:** Same maze size, but wider corridors (10m instead of 6m)

---

### **Example 4: Custom Textures**
```json
{
    "wallTexture": "Textures/BrickWall.png",
    "groundTexture": "Textures/StoneFloor.png",
    "ceilingTexture": "Textures/WoodCeiling.png"
}
```

**Result:** Custom texture pack!

---

### **Example 5: God Mode (Hidden!)**
```json
{
    "godMode": true,
    "infiniteStamina": true,
    "damageScale": 100.0
}
```

**Result:** You're a god-slayer! ⚔️🔥

---

## 🛠️ **HOW TO EDIT**

### **Step 1: Open Config File**
```
Config/GameConfig-default.json
```

**Use any text editor:**
- Notepad
- VS Code (recommended)
- Rider
- Any JSON editor

### **Step 2: Make Changes**
Edit values as needed. Use comments to document your changes!

### **Step 3: Save File**
```
Ctrl+S (save)
```

### **Step 4: Restart Unity**
```
Close Unity → Reopen
```

### **Step 5: Test Changes**
```
Tools → Maze → Generate Maze
Press Play
See your changes!
```

---

## 🐛 **TROUBLESHOOTING**

### **Issue: Config Not Loading**
**Cause:** JSON syntax error
**Solution:** 
1. Check for missing commas
2. Check for trailing commas
3. Validate JSON syntax (use online validator)

### **Issue: Changes Not Applying**
**Cause:** Unity cache
**Solution:**
1. Delete `Saves/MazeDB.sqlite`
2. Restart Unity
3. Generate new maze

### **Issue: Pink Textures**
**Cause:** Prefab path incorrect
**Solution:** Check path in config (must be relative to Assets/)

### **Issue: Game Crashes**
**Cause:** Invalid config value
**Solution:**
1. Delete `Config/GameConfig-default.json`
2. Restart Unity (creates default config)
3. Try again with valid values

---

## 🎯 **BEST PRACTICES**

### **Do:**
- ✅ Backup config before editing
- ✅ Use comments to document changes
- ✅ Test small changes first
- ✅ Check Unity Console for errors
- ✅ Restart Unity after changes

### **Don't:**
- ❌ Edit config while Unity is running
- ❌ Remove required fields
- ❌ Use absolute paths (use relative)
- ❌ Forget to restart Unity
- ❌ Make too many changes at once

---

## 🤫 **HIDDEN FEATURES**

Some configuration options are **intentionally undocumented**.

**Why?** Because **discovery is fun!** 🎮

**Experiment with:**
- `godMode`
- `oneHitKill`
- `infiniteStamina`
- `noClip`
- `damageScale`
- `healthScale`

**You might find something interesting!** ⚔️

---

## 📞 **SUPPORT**

### **Documentation:**
- `Manuals/MANUAL.md` - User guide
- `Manuals/API_REFERENCE.md` - API docs
- `Manuals/TEST_CHECKLIST.md` - Testing guide

### **Code:**
- Read the code comments (well documented!)
- Check `Assets/Scripts/Core/06_Maze/GameConfig.cs`

---

**Configuration Guide Generated:** 2026-03-05
**Version:** 1.0
**Status:** ✅ PRODUCTION READY

---

*Happy Configuring! 🎮⚙️*
