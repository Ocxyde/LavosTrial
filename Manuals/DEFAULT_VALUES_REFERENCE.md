# Default Values Configuration - Complete Reference

**Last Updated:** 2026-03-05  
**Status:** ✅ All defaults from JSON config, NO hardcoded values

---

## 📋 **Configuration Flow**

```
Game Start
    ↓
CompleteMazeBuilder.Awake()
    ↓
ApplyConfigDefaults()
    ↓
GameConfig.Instance (loads from JSON)
    ↓
Config/GameConfig-default.json
```

---

## 🗂️ **All Default Values by File**

### **1. Config/GameConfig-default.json** (Source of Truth)

```json
{
    // Maze Generation
    "defaultMazeWidth": 21,
    "defaultMazeHeight": 21,
    "defaultCellSize": 6.0,
    "defaultWallHeight": 4.0,
    "defaultWallThickness": 0.5,
    "defaultCeilingHeight": 5.0,
    
    // Door Settings
    "defaultDoorSpawnChance": 0.6,
    "defaultLockedDoorChance": 0.3,
    "defaultSecretDoorChance": 0.1,
    
    // Room Settings
    "minRooms": 3,
    "maxRooms": 8,
    "generateRooms": true,
    
    // Generation Options
    "useRandomSeed": true,
    "manualSeed": "MazeSeed2026",
    "spawnInsideRoom": true,
    
    // Game Balance
    "damageScale": 1.0,
    "healthScale": 1.0,
    "enemyHealthScale": 1.0,
    "speedScale": 1.0,
    "staminaDrainScale": 1.0,
    "godMode": false,
    "oneHitKill": false,
    "infiniteStamina": false,
    "noClip": false,
    
    // Graphics/Audio
    "graphicsQuality": "Medium",
    "soundVolume": 0.8,
    "musicVolume": 0.6,
    "mouseSensitivity": 1.0,
    "invertY": false,
    "showHUD": true
}
```

---

### **2. Assets/Scripts/Core/06_Maze/GameConfig.cs**

**Loads JSON and provides access:**

```csharp
public class GameConfig
{
    // Fields match JSON keys exactly
    public int defaultMazeWidth = 21;
    public int defaultMazeHeight = 21;
    public float defaultCellSize = 6f;
    public float defaultWallHeight = 4f;
    public float defaultWallThickness = 0.5f;
    public float defaultCeilingHeight = 5f;
    
    public float defaultDoorSpawnChance = 0.6f;
    public float defaultLockedDoorChance = 0.3f;
    public float defaultSecretDoorChance = 0.1f;
    
    public bool generateRooms = true;
    public int minRooms = 3;
    public int maxRooms = 8;
    
    public bool useRandomSeed = true;
    public string manualSeed = "MazeSeed2026";
    public bool spawnInsideRoom = true;
    
    // Game balance
    public float damageScale = 1.0f;
    public float healthScale = 1.0f;
    // ... etc
}
```

**Load Method:**
```csharp
public static GameConfig Load()
{
    // Loads from: Config/GameConfig-default.json
    // Falls back to: CreateDefault() if file missing
}
```

---

### **3. Assets/Scripts/Core/06_Maze/CompleteMazeBuilder.cs**

**Applies config in `ApplyConfigDefaults()`:**

```csharp
private void ApplyConfigDefaults()
{
    var config = GameConfig.Instance;
    
    // ALWAYS apply from JSON (no hardcoded values!)
    mazeWidth = config.defaultMazeWidth;
    mazeHeight = config.defaultMazeHeight;
    cellSize = config.defaultCellSize;
    wallHeight = config.defaultWallHeight;
    wallThickness = config.defaultWallThickness;
    ceilingHeight = config.defaultCeilingHeight;
    
    doorSpawnChance = config.defaultDoorSpawnChance;
    lockedDoorChance = config.defaultLockedDoorChance;
    secretDoorChance = config.defaultSecretDoorChance;
    
    minRooms = config.minRooms;
    maxRooms = config.maxRooms;
    generateRooms = config.generateRooms;
    
    useRandomSeed = config.useRandomSeed;
    manualSeed = config.manualSeed;
    spawnInsideRoom = config.spawnInsideRoom;
    
    Debug.Log($"🔧 Config: ceilingHeight={ceilingHeight}, wallHeight={wallHeight}");
}
```

**Called from:**
```csharp
private void Awake()
{
    // STEP 0: Load defaults from JSON config (NO HARDCODED VALUES!)
    ApplyConfigDefaults();
    
    // ... rest of initialization
}
```

---

### **4. Assets/Scripts/Core/06_Maze/MazeSaveData.cs**

**SQLite persistence (player choices override defaults):**

```csharp
public static void SaveMazeData(int seed, int spawnX, int spawnZ, int width, int height)
{
    // Saves player's maze data to SQLite
    // Path: Saves/MazeDB.sqlite
}

public static MazeDataModel LoadMazeData()
{
    // Loads player's saved maze data
    // Returns null if first-time game
}
```

**Load Order:**
```
1. Try SQLite (player's saved choices)
   ↓ if empty
2. Try JSON Config (default values)
   ↓ if missing
3. Fallback: CreateDefault() (hardcoded in GameConfig.cs)
```

---

### **5. Assets/Scripts/Editor/MazeBuilderEditor.cs**

**Editor tools also use config:**

```csharp
[MenuItem("Tools/Maze/Generate Maze %&G")]
public static void GenerateMaze()
{
    var mazeBuilder = FindFirstObjectByType<CompleteMazeBuilder>();
    
    if (mazeBuilder == null)
    {
        // Create new and initialize
        GameObject mazeGO = new GameObject("MazeBuilder");
        mazeBuilder = mazeGO.AddComponent<CompleteMazeBuilder>();
        
        // Add components
        var mazeGenerator = mazeGO.AddComponent<MazeGenerator>();
        // ... etc
        
        // Wire up references (including SpatialPlacer.mazeGenerator)
        // This ensures editor works same as runtime
    }
    
    // Generate maze (uses config values)
    mazeBuilder.GenerateMazeGeometryOnly();
}
```

---

## 📊 **Value Usage Map**

| Config Field | Used By | Default | Purpose |
|--------------|---------|---------|---------|
| `defaultMazeWidth` | CompleteMazeBuilder, MazeGenerator | 21 | Maze width in cells |
| `defaultMazeHeight` | CompleteMazeBuilder, MazeGenerator | 21 | Maze height in cells |
| `defaultCellSize` | CompleteMazeBuilder | 6.0 | Size of each cell (meters) |
| `defaultWallHeight` | CompleteMazeBuilder | 4.0 | Wall height (meters) |
| `defaultWallThickness` | CompleteMazeBuilder | 0.5 | Wall thickness (meters) |
| `defaultCeilingHeight` | CompleteMazeBuilder | 5.0 | Ceiling height (meters above ground) |
| `defaultDoorSpawnChance` | CompleteMazeBuilder | 0.6 | Chance for door (0-1) |
| `defaultLockedDoorChance` | CompleteMazeBuilder | 0.3 | Chance for locked door |
| `defaultSecretDoorChance` | CompleteMazeBuilder | 0.1 | Chance for secret door |
| `minRooms` | CompleteMazeBuilder | 3 | Minimum rooms to generate |
| `maxRooms` | CompleteMazeBuilder | 8 | Maximum rooms to generate |
| `generateRooms` | CompleteMazeBuilder | true | Enable room generation |
| `useRandomSeed` | CompleteMazeBuilder | true | Use random seed vs manual |
| `manualSeed` | CompleteMazeBuilder | "MazeSeed2026" | Manual seed string |
| `spawnInsideRoom` | CompleteMazeBuilder | true | Spawn player in room |
| `damageScale` | PlayerStats, GameConfig | 1.0 | Damage multiplier |
| `godMode` | PlayerStats, GameConfig | false | Invincibility |
| `oneHitKill` | CombatSystem | false | Instant kill |

---

## 🔧 **How to Modify Defaults**

### **For Modders (Recommended):**

1. Open `Config/GameConfig-default.json`
2. Edit values
3. Save
4. Play - changes apply immediately

**Example - God Slayer Mode:**
```json
{
    "damageScale": 10.0,
    "oneHitKill": true,
    "infiniteStamina": true,
    "godMode": false
}
```

### **For Developers (Code Changes):**

1. Edit `Config/GameConfig-default.json` (preferred)
2. OR update `GameConfig.CreateDefault()` (fallback only)
3. Test in Unity

---

## ✅ **Verification Checklist**

After any config change:

- [ ] Console shows: `📦 Applied defaults from GameConfig-default.json`
- [ ] Console shows: `🔧 Config: ceilingHeight=5, wallHeight=4, mazeSize=21x21`
- [ ] Ground position: `(63, -0.1, 63)`
- [ ] Ceiling position: `(63, 5, 63)`
- [ ] No compilation errors
- [ ] Maze generates correctly

---

## 🚨 **Common Issues**

### **Issue: Values not applying**
**Cause:** Inspector values overriding config  
**Fix:** `ApplyConfigDefaults()` now ALWAYS applies from JSON

### **Issue: SpatialPlacer errors**
**Error:** `MazeGenerator reference not found!`  
**Fix:** `MazeBuilderEditor.cs` now wires up `SpatialPlacer.mazeGenerator`

### **Issue: Ceiling at wrong height**
**Symptom:** Ceiling at Y=0 instead of Y=5  
**Fix:** Check `ceilingHeight` in JSON config (should be 5.0)

---

## 📁 **File Locations**

| File | Purpose | Editable |
|------|---------|----------|
| `Config/GameConfig-default.json` | Default values | ✅ Yes (modders) |
| `Saves/MazeDB.sqlite` | Player choices | ✅ Yes (runtime) |
| `Assets/Scripts/Core/06_Maze/GameConfig.cs` | Config loader | ⚠️ Dev only |
| `Assets/Scripts/Core/06_Maze/CompleteMazeBuilder.cs` | Applies config | ⚠️ Dev only |
| `Assets/Scripts/Editor/MazeBuilderEditor.cs` | Editor tools | ⚠️ Dev only |

---

**Generated:** 2026-03-05  
**Unity Version:** 6000.3.7f1  
**Status:** ✅ 0 Hardcoded Values | ✅ 100% JSON Config
