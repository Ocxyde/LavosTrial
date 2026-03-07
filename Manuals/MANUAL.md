# CompleteMazeBuilder - User Manual

**Version:** 1.1
**Date:** 2026-03-07
**Unity Version:** 6000.3.7f1

---

## 📖 **TABLE OF CONTENTS**

1. [Introduction](#introduction)
2. [Installation](#installation)
3. [Quick Start](#quick-start)
4. [Configuration](#configuration)
5. [Gameplay](#gameplay)
6. [Modding](#modding)
7. [Editor Tools](#editor-tools)
8. [Troubleshooting](#troubleshooting)
9. [Technical Details](#technical-details)

---

## 🎮 **INTRODUCTION**

**CompleteMazeBuilder** is a procedural maze generation system for Unity.

### **Features:**
- ✅ Procedural maze generation
- ✅ Rooms with corridors
- ✅ Doors (normal, locked, secret)
- ✅ Torches with dynamic lighting
- ✅ Player spawn inside maze
- ✅ Fully enclosed (no sky gaps)
- ✅ Mechanical exit door
- ✅ SQLite save system
- ✅ JSON configuration
- ✅ Plug-in-out architecture

---

## 📦 **INSTALLATION**

### **Requirements:**
- Unity 6000.3.7f1 or later
- New Input System
- Rider IDE (recommended)

### **Steps:**
1. Copy `Assets/Scripts/Core/06_Maze/` to your project
2. Copy `Config/` folder to project root
3. Copy `Saves/` folder to project root
4. Open Unity and let it compile

---

## 🚀 **QUICK START**

### **Generate Your First Maze:**

1. **In Unity Editor:**
   - Go to `Tools → Maze → Generate Maze` (or press `Ctrl+Alt+G`)

2. **Press Play:**
   - Maze generates automatically
   - Player spawns inside entrance room

3. **Controls:**
   - **W A S D** - Move
   - **Mouse** - Look around
   - **Space** - Jump
   - **Shift** - Sprint
   - **E** - Interact (doors, chests)

---

## ⚙️ **CONFIGURATION**

### **Config File Location:**
```
<ProjectRoot>/Config/GameConfig-default.json
```

### **Basic Configuration:**

```json
{
    "wallPrefab": "Prefabs/WallPrefab.prefab",
    "doorPrefab": "Prefabs/DoorPrefab.prefab",
    "entranceRoomPrefab": "Prefabs/EntranceRoomPrefab.prefab",
    "exitRoomPrefab": "Prefabs/ExitRoomPrefab.prefab",
    "normalRoomPrefab": "Prefabs/NormalRoomPrefab.prefab",
    "defaultMazeWidth": 21,
    "defaultMazeHeight": 21,
    "defaultCellSize": 6.0
}
```

### **Supported Settings:**

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
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

### **Comments in JSON:**

The config file supports comments! Lines starting with `//` are ignored:

```json
{
    // This is a comment - ignored by the game
    "defaultMazeWidth": 21,
    "defaultMazeHeight": 21  // Inline comment also works
}
```

---

## 🎯 **GAMEPLAY**

### **Objective:**
Navigate through the procedurally generated maze and find the exit!

### **Tips:**
1. **Explore carefully** - mazes are different every time
2. **Look for torches** - they light the way
3. **Try doors** - some are locked, some are secret
4. **Find the exit** - it's a special mechanical door

### **Difficulty:**
The game is designed to be challenging but fair. Some features may be hidden...

---

## 🔧 **MODDING**

### **Basic Modding:**

1. **Open** `Config/GameConfig-default.json`
2. **Edit** values (paths, maze size, etc.)
3. **Save** the file
4. **Restart** the game

### **Advanced Modding:**

The config file supports many more settings than documented here. **Experiment and discover what's possible!**

**Some features are intentionally undocumented.** 🤫

### **Creating Custom Prefabs:**

1. Create your prefab in Unity
2. Save to `Assets/Prefabs/`
3. Update path in `GameConfig-default.json`
4. Restart game

---

## 🛠️ **EDITOR TOOLS**

### **MazePreviewEditor - Visual Maze Preview (No Play Mode)**

The `MazePreviewEditor` allows you to preview the entire maze in the Editor without entering Play mode.

#### **Access Methods:**

1. **Menu:** `Tools → Maze → Preview Maze (1-Click Render)`
2. **Context Menu:** Select `CompleteMazeBuilder` → Right-click → `Preview Maze in Editor`

#### **Features:**

- ✅ **Instant Preview** - Generate maze without entering Play mode
- ✅ **Visual Verification** - Check maze layout before testing
- ✅ **Prefab-Based** - Uses existing prefabs from `Resources/`
- ✅ **Editor-Only** - Objects tagged "EditorOnly", excluded from builds
- ✅ **JSON-Driven** - All values from `GameConfig`
- ✅ **8-Axis Support** - Cardinal + diagonal walls

#### **Usage:**

1. **Select** the `CompleteMazeBuilder` in your scene
2. **Run** the preview via menu or right-click
3. **Inspect** the generated maze in the Scene view
4. **Clear** with `Tools → Maze → Clear Preview` when done

#### **What Gets Generated:**

| Element | Description |
|---------|-------------|
| **Walls** | Cardinal walls (N, E, S, W) |
| **Diagonal Walls** | If enabled in config (NE, NW, SE, SW) |
| **Torches** | At torch positions from maze data |
| **Chests** | At chest positions from maze data |
| **Enemies** | At enemy positions from maze data |

#### **Notes:**

- Preview objects are parented under `_previewRoot` in the hierarchy
- Objects are tagged `EditorOnly` - automatically excluded from builds
- Preview is cleared when generating a new maze or closing the scene
- Missing prefabs are logged as warnings (not errors)

#### **Troubleshooting:**

| Issue | Solution |
|-------|----------|
| **Preview empty** | Check prefabs exist in `Resources/` folder |
| **Pink prefabs** | Verify material paths in `GameConfig` |
| **Walls misaligned** | Check `CellSize` and `WallHeight` config values |
| **Diagonal walls missing** | Ensure `DiagonalWalls: true` in config + prefab exists |

---

## ❓ **TROUBLESHOOTING**

### **Common Issues:**

#### **Pink Textures:**
- **Cause:** Prefab paths incorrect
- **Solution:** Check paths in `GameConfig-default.json`

#### **Maze Not Generating:**
- **Cause:** Missing required components
- **Solution:** Run `Tools → Maze → Generate Maze`

#### **Player Stuck:**
- **Cause:** Spawn position invalid
- **Solution:** Delete `Saves/MazeDB.sqlite` and restart

#### **Config Not Loading:**
- **Cause:** JSON syntax error
- **Solution:** Check JSON syntax (use a JSON validator)

### **Reset to Defaults:**

Delete `Config/GameConfig-default.json` - it will be recreated with defaults.

---

## 🛩️ **TECHNICAL DETAILS**

### **Save System:**
- **Location:** `Saves/MazeDB.sqlite`
- **Format:** SQLite database
- **Stores:** Seed, spawn position, player choices

### **Config System:**
- **Location:** `Config/GameConfig-default.json`
- **Format:** JSON (with comment support)
- **Load Priority:** SQLite > JSON > Defaults

### **Performance:**
- **RAM Cleanup:** Automatic on quit
- **Garbage Collection:** Forced on quit
- **Resource Management:** Clean reference release

### **Architecture:**
- **Plug-in-Out:** EventHandler integrated
- **Modular:** Independent components
- **Extensible:** Easy to add features

---

## 📞 **SUPPORT**

### **Documentation:**
- Read the code comments
- Check `diff_tmp/` folder for development docs

### **Community:**
- Share your discoveries!
- Create custom mazes!
- Experiment with config!

---

## 🎉 **ENJOY THE GAME!**

**CompleteMazeBuilder** is designed to be **discovered**, not just played.

**Experiment. Explore. Enjoy!** 🎮

---

**Manual Generated:** 2026-03-05
**Unity Version:** 6000.3.7f1
**Status:** ✅ PRODUCTION READY

---

*Document generated - Unity 6 compatible - UTF-8 encoding - Unix LF*
