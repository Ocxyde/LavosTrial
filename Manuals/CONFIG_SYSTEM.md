# Configuration System Documentation

## ✅ NO HARDCODED VALUES - All Defaults from JSON Config

### Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                  CompleteMazeBuilder                         │
│                                                               │
│  Awake():                                                    │
│    1️⃣ ApplyConfigDefaults() ← From GameConfig.Instance     │
│       ↓                                                       │
│    2️⃣ GameConfig.Load() ← From Config/GameConfig-default.json
│       ↓ (if file missing)                                     │
│    3️⃣ CreateDefault() ← Fallback (same values as JSON)      │
└─────────────────────────────────────────────────────────────┘
```

### Configuration Files

#### 1. **Primary: `Config/GameConfig-default.json`** (Read-Only Default)
Location: `D:\travaux_Unity\PeuImporte\Config\GameConfig-default.json`

```json
{
    "defaultDoorSpawnChance": 0.6,
    "defaultLockedDoorChance": 0.3,
    "defaultSecretDoorChance": 0.1,
    "minRooms": 3,
    "maxRooms": 8,
    "generateRooms": true,
    "useRandomSeed": true,
    "manualSeed": "MazeSeed2026",
    ...
}
```

#### 2. **Fallback: `GameConfig.CreateDefault()`** (Same values)
Only used if JSON file is missing or corrupted.

---

## 📋 All Configurable Values

### Maze Generation
| Field | JSON Key | Default |
|-------|----------|---------|
| Maze Width | `defaultMazeWidth` | 21 |
| Maze Height | `defaultMazeHeight` | 21 |
| Cell Size | `defaultCellSize` | 6.0 |
| Wall Height | `defaultWallHeight` | 4.0 |
| Wall Thickness | `defaultWallThickness` | 0.5 |
| Ceiling Height | `defaultCeilingHeight` | 5.0 |

### Door Settings
| Field | JSON Key | Default |
|-------|----------|---------|
| Door Spawn Chance | `defaultDoorSpawnChance` | 0.6 |
| Locked Door Chance | `defaultLockedDoorChance` | 0.3 |
| Secret Door Chance | `defaultSecretDoorChance` | 0.1 |

### Room Settings
| Field | JSON Key | Default |
|-------|----------|---------|
| Generate Rooms | `generateRooms` | true |
| Min Rooms | `minRooms` | 3 |
| Max Rooms | `maxRooms` | 8 |

### Generation Options
| Field | JSON Key | Default |
|-------|----------|---------|
| Use Random Seed | `useRandomSeed` | true |
| Manual Seed | `manualSeed` | "MazeSeed2026" |
| Spawn Inside Room | `spawnInsideRoom` | true |

### Game Balance (Moddable!)
| Field | JSON Key | Default |
|-------|----------|---------|
| Damage Scale | `damageScale` | 1.0 |
| Health Scale | `healthScale` | 1.0 |
| Enemy Health Scale | `enemyHealthScale` | 1.0 |
| Speed Scale | `speedScale` | 1.0 |
| God Mode | `godMode` | false |
| One-Hit Kill | `oneHitKill` | false |
| Infinite Stamina | `infiniteStamina` | false |
| No Clip | `noClip` | false |

### Graphics/Audio
| Field | JSON Key | Default |
|-------|----------|---------|
| Graphics Quality | `graphicsQuality` | "Medium" |
| Sound Volume | `soundVolume` | 0.8 |
| Music Volume | `musicVolume` | 0.6 |
| Mouse Sensitivity | `mouseSensitivity` | 1.0 |
| Invert Y | `invertY` | false |
| Show HUD | `showHUD` | true |

---

## 🔧 How to Mod (For Modders)

### Step 1: Edit JSON Config
Open `Config/GameConfig-default.json` in any text editor.

### Step 2: Change Values
Example - Make it GOD-SLAYER mode:
```json
{
    "damageScale": 10.0,
    "godMode": false,
    "oneHitKill": true,
    "infiniteStamina": true
}
```

### Step 3: Save & Play
Unity will automatically load the new values on next run.

---

## 📊 Plug-In-Out Compliance

### ✅ TRUE - No Hardcoded Values

| Claim | Verification |
|-------|-------------|
| **All defaults from JSON** | ✅ `Config/GameConfig-default.json` |
| **Fallback exists** | ✅ `CreateDefault()` (same values) |
| **Moddable** | ✅ Edit JSON file |
| **SQLite for player choices** | ✅ `DB_SQLite/` folder |

### Load Order
```
1. SQLite (player's saved choices)
   ↓ if empty
2. JSON Config (default values)
   ↓ if missing
3. CreateDefault() (hardcoded fallback - same as JSON)
```

---

## 📁 File Locations

| File | Purpose | Editable |
|------|---------|----------|
| `Config/GameConfig-default.json` | Default values | ✅ Yes (modders) |
| `DB_SQLite/GameDB.db` | Player choices | ✅ Yes (runtime) |
| `Assets/Scripts/Core/06_Maze/GameConfig.cs` | Config loader | ⚠️ Dev only |
| `Assets/Scripts/Core/06_Maze/CompleteMazeBuilder.cs` | Maze generator | ⚠️ Dev only |

---

## 🎯 Benefits

1. **No Hardcoded Values**: All defaults in JSON file
2. **Mod-Friendly**: Edit JSON, no code changes needed
3. **Player Persistence**: SQLite stores player choices
4. **Safe Fallback**: `CreateDefault()` ensures game always runs
5. **Plug-In-Out**: Independent modules, event-driven

---

**Last Updated**: 2026-03-05  
**Unity Version**: 6000.3.7f1 (Unity 6)
