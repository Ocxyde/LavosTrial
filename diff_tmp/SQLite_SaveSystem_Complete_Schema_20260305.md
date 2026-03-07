# SQLite Save System - Complete Schema

**Date:** 2026-03-05
**Status:** ✅ PRODUCTION READY
**Database:** SQLite (Saves/MazeDB.sqlite)
**NO JSON - Pure SQL!**

---

## 📋 **COMPLETE SAVE SCHEMA**

```
┌─────────────────────────────────────────────────────────┐
│                    GAME START                           │
│                    (Loading Screen)                     │
└───────────────────┬─────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│  1. LOAD FROM SQLite DB (Saves/MazeDB.sqlite)           │
│     - MazeData (seed, spawn, dimensions)                │
│     - RoomData (room positions, types)                  │
│     - PrefabData (prefab assignments)                   │
│     - PlayerSettings (player choices)                   │
└───────────────────┬─────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│  2. APPLY PROCEDURAL DEFAULTS                           │
│     - Generate new seed (if no save)                    │
│     - Generate spawn position (if no save)              │
│     - Generate prefab paths (if no save)                │
│     - Use default settings (if no player choices)       │
└───────────────────┬─────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│  3. PLAYER PLAYS GAME                                   │
│     - Changes settings                                  │
│     - Makes choices                                     │
│     - OVERRIDES procedural defaults                     │
└───────────────────┬─────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│  4. PLAYER DISCONNECTS / QUITS                          │
└───────────────────┬─────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│  5. SAVE TO SQLite DB                                   │
│     - Player's CHOICES override defaults                │
│     - Store modified values                             │
│     - Next load uses player's choices                   │
└─────────────────────────────────────────────────────────┘
```

---

## 💾 **DATABASE SCHEMA (SQLite)**

### **Table: MazeData**
```sql
CREATE TABLE MazeData (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    seed INTEGER NOT NULL,
    spawnX INTEGER NOT NULL,
    spawnZ INTEGER NOT NULL,
    mazeWidth INTEGER NOT NULL,
    mazeHeight INTEGER NOT NULL,
    timestamp TEXT NOT NULL
);
```

### **Table: RoomData**
```sql
CREATE TABLE RoomData (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    seed INTEGER NOT NULL,
    roomX INTEGER NOT NULL,
    roomZ INTEGER NOT NULL,
    roomType TEXT NOT NULL,  -- "Entrance", "Exit", "Normal"
    timestamp TEXT NOT NULL
);
```

### **Table: PrefabData**
```sql
CREATE TABLE PrefabData (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    prefabName TEXT NOT NULL,
    prefabPath TEXT NOT NULL,
    timestamp TEXT NOT NULL
);
```

### **Table: PlayerSettings** (NEW!)
```sql
CREATE TABLE PlayerSettings (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    settingKey TEXT NOT NULL UNIQUE,
    settingValue TEXT NOT NULL,
    timestamp TEXT NOT NULL
);
```

---

## 📁 **FOLDER STRUCTURE**

```
Project Root/
├── Saves/                      ← Database folder
│   └── MazeDB.sqlite           ← SQLite database
├── Assets/
│   └── Scripts/
│       └── Core/
│           └── 06_Maze/
│               ├── CompleteMazeBuilder.cs
│               └── MazeSaveData.cs          ← Save system handler
```

---

## 🔧 **USAGE EXAMPLES**

### **Save Player Settings (On Change):**
```csharp
// Player changes graphics quality
MazeSaveData.SavePlayerSettings("GraphicsQuality", "Ultra");

// Player changes mouse sensitivity
MazeSaveData.SavePlayerSettings("MouseSensitivity", "2.5");

// Player toggles invert Y
MazeSaveData.SavePlayerSettings("InvertY", "true");
```

### **Load Player Settings (Loading Screen):**
```csharp
// Load all player settings
Dictionary<string, string> playerChoices = MazeSaveData.LoadAllPlayerSettings();

// Apply to game
if (playerChoices.ContainsKey("GraphicsQuality"))
{
    GraphicsSettings.Quality = playerChoices["GraphicsQuality"];
}
else
{
    // No player choice - use procedural default
    GraphicsSettings.Quality = "Medium";  // Default
}
```

### **Save All Settings (On Quit):**
```csharp
// Collect all current player settings
var settings = new Dictionary<string, string>
{
    { "GraphicsQuality", currentGraphicsQuality },
    { "SoundVolume", currentSoundVolume },
    { "MusicVolume", currentMusicVolume },
    { "MouseSensitivity", currentMouseSensitivity },
    { "InvertY", invertY.ToString() },
    { "ShowHUD", showHUD.ToString() }
};

// Save to SQLite
MazeSaveData.SaveAllPlayerSettings(settings);
```

### **Load with Fallback to Defaults:**
```csharp
// Load player's choice, or use default if not exists
string graphicsQuality = MazeSaveData.LoadPlayerSetting(
    "GraphicsQuality", 
    "Medium"  // Default if no player choice
);

// If player previously set "Ultra", returns "Ultra"
// If no previous choice, returns "Medium" (default)
```

---

## ✅ **FEATURES**

| Feature | Status |
|---------|--------|
| **Location** | ✅ Saves/ at project root (NOT in Assets/) |
| **Database** | ✅ SQLite (MazeDB.sqlite) |
| **NO JSON** | ✅ Pure SQL database |
| **Seed Storage** | ✅ Stored with maze data |
| **Prefab Data** | ✅ All prefab paths saved |
| **Room Data** | ✅ Room positions + types |
| **Player Settings** | ✅ Player choices override defaults |
| **Timestamps** | ✅ Auto-generated on save |
| **Plug-in-Out** | ✅ Independent module |
| **Load Screen** | ✅ Load during loading screen |
| **On Quit** | ✅ Save player choices |

---

## 🎮 **INTEGRATION EXAMPLE**

### **CompleteMazeBuilder.cs:**
```csharp
private void Start()
{
    // Load player settings first (loading screen)
    LoadPlayerSettings();
    
    // Generate maze with procedural defaults
    if (autoGenerateOnStart)
    {
        GenerateCompleteMaze();
    }
}

private void LoadPlayerSettings()
{
    // Load from SQLite
    var settings = MazeSaveData.LoadAllPlayerSettings();
    
    // Apply player choices (override defaults)
    if (settings.ContainsKey("MouseSensitivity"))
    {
        mouseSensitivity = float.Parse(settings["MouseSensitivity"]);
    }
    // Else: use default value
}

private void OnApplicationQuit()
{
    // Save player settings on quit
    SavePlayerSettings();
}

private void SavePlayerSettings()
{
    var settings = new Dictionary<string, string>
    {
        { "MouseSensitivity", mouseSensitivity.ToString() },
        { "GraphicsQuality", graphicsQuality },
        { "SoundVolume", soundVolume.ToString() }
    };
    
    MazeSaveData.SaveAllPlayerSettings(settings);
}
```

---

## 📊 **DATA FLOW**

```
First Play (No Save):
┌──────────────────┐
│ Procedural Gen   │
│ - Random seed    │
│ - Random spawn   │
│ - Default prefs  │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│ Player Changes   │
│ - Overrides      │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│ On Quit: Save    │
│ - To SQLite      │
└──────────────────┘

Second Play (Has Save):
┌──────────────────┐
│ Load from SQLite │
│ - Player choices │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│ Apply Choices    │
│ - Override def.  │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│ Player Plays     │
│ - Same settings  │
└──────────────────┘
```

---

## 🗄️ **DATABASE LOCATION**

**Path:** `<ProjectRoot>/Saves/MazeDB.sqlite`

**Example:**
```
D:/travaux_Unity/PeuImporte/Saves/MazeDB.sqlite
```

**NOT in Assets!** This ensures:
- ✅ Database persists across Unity restarts
- ✅ Not reimported by Unity
- ✅ Clean separation of data and code
- ✅ Easy to backup/restore

---

## 🎯 **KEY CONCEPTS**

### **1. Procedural Defaults:**
- Generated on first play
- Used when no save exists
- Overridden by player choices

### **2. Player Choices:**
- Made during gameplay
- Saved on quit/disconnect
- Override defaults on next load

### **3. Load Priority:**
```
Player Settings (SQLite) > Procedural Defaults
```

### **4. Save Timing:**
- **Load:** Loading screen / game start
- **Save:** On quit / disconnect / setting change

---

## ✅ **VERIFICATION**

### **Check Database Exists:**
```csharp
if (MazeSaveData.DatabaseExists())
{
    Debug.Log("✅ Database exists!");
}
```

### **Get Database Path:**
```csharp
string path = MazeSaveData.GetDatabasePath();
Debug.Log($"Database at: {path}");
// Output: Database at: D:/travaux_Unity/PeuImporte/Saves/MazeDB.sqlite
```

### **Check Player Settings:**
```csharp
var settings = MazeSaveData.LoadAllPlayerSettings();
if (settings.Count > 0)
{
    Debug.Log($"✅ {settings.Count} player settings loaded");
}
else
{
    Debug.Log("ℹ️ No player settings - using defaults");
}
```

---

## 🎉 **FINAL RESULT**

**MazeSaveData is now:**
- ✅ **SQLite database** (Saves/MazeDB.sqlite)
- ✅ **Project root storage** (NOT in Assets/)
- ✅ **NO JSON** (Pure SQL!)
- ✅ **Seed-based** (procedural + persistent)
- ✅ **Prefab storage** (all assignments saved)
- ✅ **Room data** (positions + types)
- ✅ **Player settings** (choices override defaults)
- ✅ **Load screen** (load on game start)
- ✅ **On quit** (save player choices)
- ✅ **Plug-in-out** (independent module)

---

**Generated:** 2026-03-05
**Unity Version:** 6000.3.7f1
**Database:** SQLite (Saves/MazeDB.sqlite)
**Schema:** Player Choices Override Defaults
**Status:** ✅ PRODUCTION READY

---

*Document generated - Unity 6 compatible - UTF-8 encoding - Unix LF*
