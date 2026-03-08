## UNIVERSAL LEVEL GENERATOR - SETUP & QUICK START GUIDE

**Version:** 1.0  
**Date:** 2026-03-08  
**Status:** Production Ready

---

## QUICK START (5 MINUTES)

### 1. Verify Installation

All files should be in place:

```
Assets/Scripts/Core/06_Maze/
  ├── LevelData.cs                      ✅
  ├── ProceduralLevelGenerator.cs       ✅
  └── LevelDatabaseManager.cs           ✅

Assets/Scripts/Editor/
  └── UniversalLevelGeneratorTool.cs    ✅

Assets/Docs/
  ├── UNIVERSAL_LEVEL_GENERATOR_DOCUMENTATION.md   ✅
  └── UNIVERSAL_LEVEL_GENERATOR_SETUP.md            ✅ (this file)

Config/
  └── GameConfig-Level3.json            ✅ (reference)
```

### 2. Open the Tool

```
Menu: Tools > Level Generator > Procedural Level Builder
```

A window should open:
```
┌─────────────────────────────────────────────┐
│ UNIVERSAL PROCEDURAL LEVEL GENERATOR       │
│ Exponential Difficulty Scaling | ...        │
├─────────────────────────────────────────────┤
│ [Single Level] [Batch Generation] [Storage] │
│                                             │
│ Level Number: [1___________] (slider 1-50) │
│ Difficulty Mult: [1.0________] (slider)    │
│                                             │
│     [GENERATE LEVEL]  [Load]  [Clear]      │
│                                             │
│ Status: Ready                               │
└─────────────────────────────────────────────┘
```

### 3. Generate First Level

1. Keep "Level Number" at 1
2. Click `GENERATE LEVEL`
3. Wait 1-2 seconds
4. Level 1 appears in scene

### 4. Check Results

In Hierarchy:
```
Scene
├── MazeBuilder_Level1 (new)
│   ├── Ground
│   ├── Walls
│   ├── Doors
│   ├── Objects
│   └── Lighting
├── Player (spawned)
└── ...
```

In Console:
```
[ProceduralLevelGen] Generating Level 1...
[ProceduralLevelGen] Difficulty Factor: 1.35
[ProceduralLevelGen] Scaling parameters for Level 1
[ProceduralLevelGen] Maze Size: 21x21
[ProceduralLevelGen] Enemy Density: 25%
[ProceduralLevelGen] Trap Density: 15%
[ProceduralLevelGen] Level 1 generated successfully in 123.45ms
```

---

## DETAILED SETUP

### Step 1: Scene Preparation

**Required Objects in Scene:**

Create/ensure these exist:

```csharp
// GameObject: "ProceduralLevelGenerator"
var procGen = new GameObject("ProceduralLevelGenerator");
procGen.AddComponent<ProceduralLevelGenerator>();

// GameObject: "LevelDatabaseManager"
var dbMgr = new GameObject("LevelDatabaseManager");
dbMgr.AddComponent<LevelDatabaseManager>();

// Required by maze generation
var gameManager = FindObjectOfType<GameManager>();
if (gameManager == null)
{
    var gmGo = new GameObject("GameManager");
    gameManager = gmGo.AddComponent<GameManager>();
}

// Required for player spawn
var playerPrefab = Resources.Load<GameObject>("Prefabs/PlayerPrefab");
if (playerPrefab == null)
{
    Debug.LogError("PlayerPrefab not found in Resources/Prefabs/");
}
```

**Auto-Setup (Tool Does This):**

The editor tool automatically:
- Creates ProceduralLevelGenerator if missing
- Creates LevelDatabaseManager if missing
- Finds all required components
- Initializes database

### Step 2: Configuration Files

**GameConfig-Level3.json** (example, create similar for other levels if needed):

```json
{
  "levelNumber": 3,
  "levelName": "Ancient Depths - Level 3",
  "defaultMazeWidth": 64,
  "defaultMazeHeight": 64,
  "difficultyFactor": 2.5,
  "enemySpawnDensity": 0.4,
  ...
}
```

**Optional:** Create GameConfig files for specific levels:
- `Config/GameConfig-Level5.json`
- `Config/GameConfig-Level10.json`
- etc.

### Step 3: Initialize Components

In Play mode or via Editor script:

```csharp
using Code.Lavos.Core.Procedural;

// Access singleton instances
var levelGenerator = ProceduralLevelGenerator.Instance;
var levelDatabase = LevelDatabaseManager.Instance;

// They auto-initialize!
```

### Step 4: Verify All APIs

The system integrates with these core systems:

```
Check these exist in scene or are auto-findable:

✓ CompleteMazeBuilder8         (maze generation)
✓ GameManager                  (game state)
✓ ItemEngine                   (items)
✓ DoorsEngine                  (doors)
✓ SpatialPlacer               (placement)
✓ LightPlacementEngine        (lighting)
✓ EnemyPlacer                 (enemies)
✓ ComputeGridEngine           (grid)
✓ PlayerController            (player input)
```

If any are missing, the tool will log warnings but continue with what's available.

---

## USAGE WORKFLOWS

### Workflow 1: Single Level Generation

```
1. Open Tool: Tools > Level Generator > Procedural Level Builder
2. Set Level Number: 5
3. Optional: Enable Custom Seed (reproducibility)
4. Optional: Adjust Difficulty Multiplier
5. Click: GENERATE LEVEL
6. Wait: 200-400ms
7. Result: Level 5 instantiated in scene
8. Auto-saved: Level_5.level.json (if enabled)
```

### Workflow 2: Batch Generation (Pre-caching)

```
1. Open Tool
2. Switch to: "Batch Generation" tab
3. Set Start Level: 1
4. Set End Level: 10
5. Ensure "Save to Disk" enabled
6. Click: GENERATE BATCH
7. Wait: ~3 seconds
8. Result: Levels 1-10 generated and cached
9. Next time: Load instantly from cache
```

### Workflow 3: Load from Storage

```
1. Open Tool
2. Switch to: "Storage Manager" tab
3. See "Cached Levels" list
4. Select Level N
5. Click: Load button (or from Single Level tab)
6. Instant: Level instantiated from saved data
```

### Workflow 4: Monitor Progress

```
1. Open Tool
2. Switch to: "Statistics" tab
3. View last generation stats:
   - Time taken
   - Room count
   - Enemy count
   - Treasure count
   - Trap count
   - Etc.
```

---

## CONFIGURATION EXAMPLES

### Example 1: Easy Mode (Level 1)

```
Level Number: 1
Difficulty Multiplier: 0.5
Custom Seed: False (random each time)
Advanced Settings:
  - Diagonal Walls: OFF
  - Boss Room: OFF
  - Environmental Hazards: OFF
  - Status Effects: OFF
Result: Beginner-friendly level
```

### Example 2: Hard Mode (Level 10)

```
Level Number: 10
Difficulty Multiplier: 2.0
Custom Seed: 12345 (reproducible)
Advanced Settings:
  - Diagonal Walls: ON
  - Boss Room: ON
  - Environmental Hazards: ON
  - Status Effects: ON
Result: Challenging level with all features
```

### Example 3: Insane Mode (Level 30)

```
Level Number: 30
Difficulty Multiplier: 2.5
Custom Seed: True (12345)
Advanced Settings: ALL ON
Custom Densities:
  - Enemy Density: 0.8
  - Trap Density: 0.7
  - Treasure Density: 0.5
Result: Extremely difficult level
```

---

## PROGRAMMATIC USAGE

### Code Example 1: Generate Level from Script

```csharp
using Code.Lavos.Core.Procedural;

var levelGenerator = ProceduralLevelGenerator.Instance;

// Listen to events
levelGenerator.OnGenerationProgress += (step, progress) =>
{
    Debug.Log($"Generation {progress * 100:F0}% complete");
};

levelGenerator.OnLevelGenerated += (levelData) =>
{
    Debug.Log($"Level {levelData.LevelNumber} ready!");
};

// Generate
levelGenerator.GenerateLevel(5);  // Level 5 with default seed
```

### Code Example 2: Save and Load

```csharp
var databaseManager = LevelDatabaseManager.Instance;

// After generation, save it
var levelData = levelGenerator.GetCurrentLevelData();
databaseManager.SaveLevel(levelData, saveToDisk: true);

// Later, load it back
var savedLevel = databaseManager.LoadLevel(5);
if (savedLevel != null)
{
    Debug.Log($"Loaded: {savedLevel.LevelName}");
}
```

### Code Example 3: Batch Processing

```csharp
var levelGenerator = ProceduralLevelGenerator.Instance;
var databaseManager = LevelDatabaseManager.Instance;

// Generate and save levels 1-10
for (int i = 1; i <= 10; i++)
{
    levelGenerator.GenerateLevel(i);
    
    var levelData = levelGenerator.GetCurrentLevelData();
    databaseManager.SaveLevel(levelData, saveToDisk: true);
    
    Debug.Log($"Level {i} saved");
}
```

### Code Example 4: Access Difficulty

```csharp
// Calculate difficulty for any level
var difficulty = LevelDifficultyScaler.CalculateDifficultyFactor(levelNumber: 10);
Debug.Log($"Level 10 difficulty: {difficulty:F2}x");

// Scale parameters
var mazeParams = LevelDifficultyScaler.ScaleMazeParameters(10, baseParams, difficulty);
Debug.Log($"Maze size: {mazeParams.Width}x{mazeParams.Height}");
```

---

## STORAGE MANAGEMENT

### File Organization

```
Persistent Data Path:
  (Platform-specific: C:/Users/.../AppData/Local/...)
  
StreamingAssets/Levels/
  ├── Level_1.level.json      (~200KB)
  ├── Level_2.level.json      (~200KB)
  ├── Level_3.level.json      (~200KB)
  └── Level_N.level.json      (~200KB)
```

### Cleanup Old Levels

```csharp
var databaseManager = LevelDatabaseManager.Instance;

// Delete level 1 (after players progress)
databaseManager.DeleteLevel(1);

// Or delete all below level 5
for (int i = 1; i < 5; i++)
{
    databaseManager.DeleteLevel(i);
}
```

### Check Storage Stats

```csharp
var stats = databaseManager.GetStorageStats();
Debug.Log($"Cached: {stats.CachedLevels} levels");
Debug.Log($"Disk: {stats.DiskLevels} levels");
Debug.Log($"Size: {stats.StorageSize / (1024 * 1024)}MB");
```

---

## PERFORMANCE OPTIMIZATION

### Pre-caching Strategy

**Best Practice:**

```
1. On game start:
   - Generate levels 1-5
   - Cache to disk
   
2. During gameplay:
   - Keep current level in RAM
   - Preload next level in background
   
3. Between levels:
   - Unload current
   - Load next from cache
   - Generate level+2 in background
```

### Memory Management

```csharp
// Monitor memory
var cachedLevelCount = levelDatabase.GetStorageStats().CachedLevels;

if (cachedLevelCount > 10)
{
    // Too many in memory, delete oldest
    for (int i = 1; i <= 5; i++)
    {
        if (i < currentLevel)
        {
            levelDatabase.DeleteLevel(i);
        }
    }
}
```

### Async Generation (Recommended)

```csharp
// Generate in background thread
System.Threading.ThreadPool.QueueUserWorkItem(_ =>
{
    var levelGenerator = ProceduralLevelGenerator.Instance;
    levelGenerator.GenerateLevel(nextLevel);
    // Handle result on main thread
});
```

---

## TROUBLESHOOTING

### Problem: "AssemblyDefinitionException"

**Cause:** Missing assembly definition files

**Solution:**
```
Create: Assets/Scripts/Core/Core.asmdef
Create: Assets/Scripts/Editor/Editor.asmdef
```

### Problem: "CompleteMazeBuilder8 not found"

**Cause:** Component not in scene

**Solution:**
```csharp
var go = new GameObject("MazeBuilder");
go.AddComponent<CompleteMazeBuilder8>();
```

### Problem: "No space on disk"

**Cause:** Too many cached levels

**Solution:**
```
1. Delete old levels: Storage Manager > Delete
2. Or: Manual delete from StreamingAssets/Levels/
3. Or: Set SaveToDisk = false
```

### Problem: "Generation too slow"

**Cause:** Large level numbers

**Solution:**
- Reduce Difficulty Multiplier (makes maze smaller)
- Disable Advanced Features
- Generate on background thread
- Use SSD instead of HDD

### Problem: "Levels not reproducible"

**Cause:** Different seeds each time

**Solution:**
```
1. Enable "Custom Seed" checkbox
2. Use same seed value
3. Note seed in documentation
```

---

## VERIFICATION CHECKLIST

After setup, verify everything works:

```
✓ Tool opens without errors
✓ Can set level 1-50
✓ GENERATE LEVEL button works
✓ Level instantiates in scene
✓ Maze appears
✓ Enemies spawn
✓ Treasures placed
✓ Doors present
✓ Torches lit
✓ Player spawns
✓ Statistics show in tab
✓ Storage Manager shows cached levels
✓ Can load from storage
✓ Can delete levels
✓ Batch generation completes
```

If all pass ✓, you're ready!

---

## NEXT STEPS

1. **Test Different Levels:**
   - Generate Level 1, 5, 10, 20, 50
   - Compare difficulty visually

2. **Monitor Statistics:**
   - Check generation time
   - Check room/enemy counts
   - Verify exponential growth

3. **Test Persistence:**
   - Generate Level 5
   - Close editor
   - Reopen, load Level 5 from Storage Manager

4. **Fine-tune:**
   - Adjust Difficulty Multiplier
   - Enable/disable features
   - Test custom densities

5. **Integrate into Game:**
   - Add to main menu
   - Add level selection UI
   - Implement difficulty choice

---

## SUPPORT

For issues:

1. Check **UNIVERSAL_LEVEL_GENERATOR_DOCUMENTATION.md**
2. Check **Troubleshooting** section above
3. Check Console for error messages
4. Review **Storage Manager** tab for state

---

**Installation Complete!**

You now have a complete, production-ready procedural level generation system integrated with all project APIs.

**Enjoy!**
