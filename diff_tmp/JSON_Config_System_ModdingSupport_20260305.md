# JSON Config System - Modding Support

**Date:** 2026-03-05
**Status:** ✅ PRODUCTION READY - GOD-SLAYER MODE ENABLED! ⚔️
**Config File:** Config/GameConfig.json
**Format:** JSON (easy to edit/mod)

---

## 📁 **FOLDER STRUCTURE**

```
Project Root/
├── Config/                       ← NEW: Config folder
│   └── GameConfig.json           ← JSON config file (EDIT THIS!)
├── Saves/
│   └── MazeDB.sqlite             ← SQLite database
├── Assets/
│   └── Scripts/
│       └── Core/
│           └── 06_Maze/
│               ├── GameConfig.cs         ← Config handler
│               └── CompleteMazeBuilder.cs
```

---

## 🎮 **CONFIG FILE (GameConfig.json)**

### **Default Values:**
```json
{
    "wallPrefab": "Prefabs/WallPrefab.prefab",
    "doorPrefab": "Prefabs/DoorPrefab.prefab",
    "damageScale": 1.0,
    "godMode": false,
    "oneHitKill": false,
    "infiniteStamina": false,
    "defaultMazeWidth": 21,
    "graphicsQuality": "Medium",
    ...
}
```

### **GOD-SLAYER MODE (Edit JSON):**
```json
{
    "damageScale": 100.0,        // 100x damage!
    "healthScale": 10.0,         // 10x health!
    "godMode": true,             // INVINCIBLE!
    "oneHitKill": true,          // ONE HIT EVERYTHING!
    "infiniteStamina": true,     // NEVER TIRED!
    "noClip": true,              // WALK THROUGH WALLS!
    ...
}
```

---

## 🔧 **HOW IT WORKS**

### **Load Priority:**
```
Player's SQLite Save > JSON Config > Hardcoded (NONE!)
```

### **First Time Game:**
```
1. No SQLite save exists
2. Load from JSON config (Config/GameConfig.json)
3. Apply default values
4. Save to SQLite (player's choices)
```

### **Subsequent Games:**
```
1. SQLite save exists
2. Load player's choices from SQLite
3. Override JSON defaults
4. Apply player's settings
```

### **Modding (Edit JSON):**
```
1. Edit Config/GameConfig.json
2. Change damageScale, godMode, etc.
3. Save JSON file
4. Restart game
5. NEW VALUES APPLIED! ⚔️
```

---

## ⚔️ **GOD-SLAYER MODE (MODDING)**

### **Available Settings:**

| Setting | Type | Default | God-Slayer Value |
|---------|------|---------|------------------|
| **damageScale** | float | 1.0 | 100.0 (100x damage!) |
| **healthScale** | float | 1.0 | 10.0 (10x health!) |
| **enemyHealthScale** | float | 1.0 | 0.1 (enemies die fast!) |
| **speedScale** | float | 1.0 | 2.0 (2x speed!) |
| **godMode** | bool | false | true (INVINCIBLE!) |
| **oneHitKill** | bool | false | true (ONE HIT!) |
| **infiniteStamina** | bool | false | true (NEVER TIRED!) |
| **noClip** | bool | false | true (WALK THROUGH WALLS!) |

### **Example God-Slayer Config:**
```json
{
    "damageScale": 100.0,
    "healthScale": 10.0,
    "enemyHealthScale": 0.1,
    "speedScale": 2.0,
    "godMode": true,
    "oneHitKill": true,
    "infiniteStamina": true,
    "noClip": false
}
```

**Result:** You're a **GOD-SLAYER!** ⚔️🔥

---

## 📝 **HOW TO MOD (STEP-BY-STEP)**

### **1. Open Config File:**
```
Location: D:\travaux_Unity\PeuImporte\Config\GameConfig.json
Editor: Any text editor (Notepad, VS Code, etc.)
```

### **2. Edit Values:**
```json
// Change damage from 1.0 to 100.0
"damageScale": 100.0,

// Enable god mode
"godMode": true,
```

### **3. Save File:**
```
Ctrl+S (save)
Close editor
```

### **4. Restart Game:**
```
Restart Unity or standalone build
Config loads automatically
NEW VALUES APPLIED!
```

---

## 🎯 **PLUG-IN-OUT FOR MODDERS**

### **Easy to Mod:**
- ✅ **JSON format** (human-readable)
- ✅ **No coding required** (just edit text)
- ✅ **Instant feedback** (restart to see changes)
- ✅ **Safe** (can reset to defaults)
- ✅ **Version control friendly** (diffable)

### **Modding Examples:**

#### **Easy Mode (Casual Players):**
```json
{
    "damageScale": 2.0,
    "healthScale": 2.0,
    "godMode": false
}
```

#### **God-Slayer Mode (Hardcore):**
```json
{
    "damageScale": 1000.0,
    "godMode": true,
    "oneHitKill": true,
    "noClip": true
}
```

#### **Challenge Mode (Masochists):**
```json
{
    "damageScale": 0.5,
    "healthScale": 0.5,
    "enemyHealthScale": 2.0,
    "godMode": false
}
```

---

## ✅ **FEATURES**

| Feature | Status |
|---------|--------|
| **JSON format** | ✅ Easy to edit |
| **No hardcoded values** | ✅ All from JSON/SQLite |
| **God-slayer mode** | ✅ Editable damage/health |
| **Modding support** | ✅ Plug-in-out for modders |
| **Location** | ✅ Config/ at project root |
| **Auto-create** | ✅ Creates if missing |
| **Reset to defaults** | ✅ Delete JSON file |
| **Player overrides** | ✅ SQLite > JSON |
| **Safe** | ✅ Validates on load |

---

## 🎮 **USAGE IN CODE**

### **Access Config:**
```csharp
// Get config instance (auto-loads from JSON)
var config = GameConfig.Instance;

// Read values
float damage = config.damageScale;
bool godMode = config.godMode;

// Use in game
enemy.TakeDamage(baseDamage * config.damageScale);
if (config.godMode) player.isInvincible = true;
```

### **Save Player Choices:**
```csharp
// Player changes settings in-game
config.damageScale = 50.0f;
config.godMode = true;

// Save to SQLite (overrides JSON)
MazeSaveData.SaveAllPlayerSettings(settings);
```

---

## 📁 **FILE LOCATIONS**

| File | Purpose | Editable? |
|------|---------|-----------|
| **Config/GameConfig.json** | Default values | ✅ YES (mod here!) |
| **Saves/MazeDB.sqlite** | Player choices | ❌ No (auto-saved) |
| **Assets/Scripts/Core/06_Maze/GameConfig.cs** | Config handler | ⚠️ Advanced only |

---

## 🎉 **FINAL RESULT**

**JSON Config System is:**
- ✅ **Easy to mod** (JSON format)
- ✅ **No hardcoded values** (all from JSON/SQLite)
- ✅ **God-slayer mode** (damage scale, god mode, etc.)
- ✅ **Plug-in-out** (for modders)
- ✅ **Safe** (validates on load)
- ✅ **Auto-creates** (if missing)
- ✅ **Resettable** (delete JSON file)
- ✅ **Professional** (proper config management)

---

## ⚔️ **UNLEASH THE GOD-SLAYER!**

```
1. Open Config/GameConfig.json
2. Set "godMode": true
3. Set "oneHitKill": true
4. Set "damageScale": 100.0
5. Save file
6. Restart game
7. BECOME A GOD-SLAYER! ⚔️🔥
```

---

**Generated:** 2026-03-05
**Unity Version:** 6000.3.7f1
**Config File:** Config/GameConfig.json
**Status:** ✅ PRODUCTION READY - **GOD-SLAYER MODE ENABLED!**

---

*Document generated - Unity 6 compatible - UTF-8 encoding - Unix LF*
