# Project Documentation - PeuImporte

**Unity Version:** 6000.3.7f1  
**Render Pipeline:** URP Standard  
**Input System:** New Input System  
**Coding Standard:** Unity 6  
**Last Updated:** 2026-03-01

---

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── Core/              # Core systems (GameManager, ItemEngine, etc.)
│   ├── Player/            # Player systems (Controller, Stats, Health)
│   ├── HUD/               # UI systems (Bars, Dialogs, Popups)
│   ├── Inventory/         # Inventory management
│   ├── Status/            # Status effects & stats
│   ├── Ressources/        # Resources & generators
│   ├── Gameplay/          # Gameplay elements
│   ├── Ennemies/          # Enemy AI
│   ├── Interaction/       # Interaction system
│   ├── Tests/             # Unit tests
│   └── Editor/            # Editor tools
├── DB_SQLite/             # Database system
├── Settings/              # URP settings
├── Docs/                  # This documentation
└── InputSystem_Actions.inputactions
```

---

## 🎯 Core Architecture

### Plug-in-and-Out System

The project uses a modular plug-in architecture centered around core manager classes:

```
┌─────────────────────────────────────────────────────────┐
│                    GameManager                          │
│              (Central Game State Singleton)             │
└─────────────────────────────────────────────────────────┘
                          │
        ┌─────────────────┼─────────────────┐
        ▼                 ▼                 ▼
┌───────────────┐  ┌───────────────┐  ┌───────────────┐
│  ItemEngine   │  │ PlayerController│  │  HUDSystem   │
│  (Items Mgr)  │  │  (New Input)   │  │   (UI/UX)    │
└───────┬───────┘  └───────┬───────┘  └───────┬───────┘
        │                  │                  │
        ▼                  ▼                  ▼
┌───────────────┐  ┌───────────────┐  ┌───────────────┐
│BehaviorEngine │  │ PlayerStats   │  │ DialogEngine │
│  (Base Class) │  │  (StatsEngine)│  │  (Floating)  │
└───────┬───────┘  └───────┬───────┘  └───────────────┘
        │                  │                  │
        ▼                  ▼                  ▼
┌───────────────┐  ┌───────────────┐  ┌───────────────┐
│ DoubleDoor    │  │ StatsEngine   │  │ PopWinEngine │
│ ChestBehavior │  │ (Pure C#)     │  │ (Windows)    │
└───────────────┘  └───────────────┘  └───────────────┘
```

---

## 📦 Key Systems

### 1. **Core Systems** (`Assets/Scripts/Core/`)

| Script | Purpose | Status |
|--------|---------|--------|
| `GameManager.cs` | Central game state singleton | ✅ Complete |
| `ItemEngine.cs` | Item registry & management | ✅ Complete |
| `BehaviorEngine.cs` | Base class for interactables | ✅ Complete |
| `MazeGenerator.cs` | Procedural maze generation | ✅ Complete |
| `DrawingManager.cs` | Texture generation | ✅ Complete |
| `ParticleGenerator.cs` | Particle VFX | ✅ Complete |

### 2. **Player Systems** (`Assets/Scripts/Player/`)

| Script | Purpose | Status |
|--------|---------|--------|
| `PlayerController.cs` | Movement, camera, input | ✅ Complete |
| `PlayerStats.cs` | Stats wrapper (StatsEngine) | ✅ Complete |
| `PlayerHealth.cs` | Health management | ✅ Complete |
| `PersistentPlayerData.cs` | Save/load data | ✅ Complete |

**Features:**
- ✅ New Input System (WASD + Mouse)
- ✅ Sprint system (10% speed boost, 1% stamina/sec)
- ✅ Jump system (1% stamina per jump)
- ✅ Camera follow with head bob
- ✅ Interaction system (E key)

### 3. **HUD & UI Systems** (`Assets/Scripts/HUD/`)

| Script | Purpose | Status |
|--------|---------|--------|
| `UIBarsSystem.cs` | Health/Mana/Stamina bars | ✅ Complete |
| `DialogEngine.cs` | Floating text + dialogs | ✅ Complete |
| `PopWinEngine.cs` | Popup windows + inventories | ✅ Complete |
| `HUDSystem.cs` | Main HUD manager | ✅ Complete |
| `HUDEngine.cs` | HUD registry | ✅ Complete |

**Features:**
- ✅ Real-time bar updates (events)
- ✅ Color interpolation (based on %)
- ✅ Floating combat text (damage/heal)
- ✅ Dialog system (bottom-left, resizable)
- ✅ Inventory windows (slot-based)
- ✅ Stats board window (prepend feature)

### 4. **Status System** (`Assets/Scripts/Status/`)

| Script | Purpose | Status |
|--------|---------|--------|
| `StatsEngine.cs` | Pure C# stat calculations | ✅ Complete |
| `StatusEffectData.cs` | Effect definitions | ✅ Complete |
| `StatModifier.cs` | Stat modifiers | ✅ Complete |
| `DamageType.cs` | Damage types | ✅ Complete |

**Features:**
- ✅ Buff/Debuff system
- ✅ Stat modifiers (additive, multiplicative, override)
- ✅ Damage resistances/vulnerabilities
- ✅ DoT/HoT (damage/heal over time)
- ✅ Status effect icons with timers

### 5. **Inventory System** (`Assets/Scripts/Inventory/`)

| Script | Purpose | Status |
|--------|---------|--------|
| `Inventory.cs` | Inventory manager (Singleton) | ✅ Complete |
| `InventorySlot.cs` | Slot data structure | ✅ Complete |
| `InventoryUI.cs` | UI display | ✅ Complete |
| `InventorySlotUI.cs` | UI slot component | ✅ Complete |
| `ItemPickup.cs` | World pickups | ✅ Complete |

**Features:**
- ✅ Stackable items
- ✅ Grid-based UI
- ✅ Drag & drop (can be added)
- ✅ Item categories (Consumable, Equipment, etc.)

### 6. **Database System** (`Assets/DB_SQLite/`)

| Script | Purpose | Status |
|--------|---------|--------|
| `DatabaseManager.cs` | JSON persistence | ✅ Complete |
| `DatabaseSaveLoadHelper.cs` | Save/load utilities | ✅ Complete |
| `DatabaseConfig.cs` | Configuration | ✅ Complete |

**Features:**
- ✅ JSON-based save system
- ✅ Cross-platform (Windows, Linux, macOS)
- ✅ Player data persistence
- ✅ Inventory save/load

---

## 🎮 Gameplay Features

### Movement & Controls

| Action | Input | Stamina Cost |
|--------|-------|--------------|
| Move | WASD / Arrow Keys | None |
| Sprint | Hold Shift | 1% current/sec |
| Jump | Space | 1% current/jump |
| Look | Mouse | None |
| Interact | E | None |
| Camera Toggle | Tab | None |

### Combat System

- ✅ Damage types (Physical, Fire, Ice, Lightning, etc.)
- ✅ Critical hits (5% base chance, 150% damage)
- ✅ Resistance system (per damage type)
- ✅ Invincibility frames (0.5s after hit)
- ✅ Floating damage numbers

### Status Effects

- ✅ Buffs (positive effects)
- ✅ Debuffs (negative effects)
- ✅ Curses (special debuffs)
- ✅ Stacking effects
- ✅ Duration timers
- ✅ Icon display with fade

---

## 🛠️ Development Tools

### Automation Scripts

Located in project root:

| Script | Purpose |
|--------|---------|
| `backup.ps1` | Smart backup system |
| `apply-patches-and-backup.ps1` | Run patches + backup |
| `scan-project-errors.ps1` | Scan for issues |
| `fix-all-issues.ps1` | Auto-fix problems |
| `clear-unity-cache.bat` | Clear Unity cache |
| `git-quick.bat` | Git operations menu |
| `git-commit.ps1` | Quick commit (with backup) |
| `git-push.ps1` | Push to remote |
| `git-pull.ps1` | Pull from remote |
| `git-status.ps1` | Detailed status |

### Git Workflow

```bash
# Quick commit (auto-backup)
.\git-commit.ps1 "Fixed player movement"

# Push to remote
.\git-push.ps1

# Check status
.\git-status.ps1
```

---

## 📊 Code Quality

| Metric | Status |
|--------|--------|
| **Total C# Files** | 100+ |
| **Compilation Errors** | 0 ✅ |
| **Warnings** | 0 ✅ |
| **UTF-8 Encoding** | 100% ✅ |
| **Unix LF Line Endings** | 100% ✅ |
| **Unity 6 Headers** | 100% ✅ |
| **New Input System** | 100% ✅ |
| **URP Compatible** | 100% ✅ |

---

## 🚀 Getting Started

### 1. Open Project

1. Open Unity Hub
2. Click "Add" → Select `D:\travaux_Unity\PeuImporte`
3. Open with Unity 6000.3.7f1

### 2. Verify Setup

1. Check Console for errors (should be 0)
2. Verify Input System is enabled (Project Settings)
3. Verify URP is active (Graphics Settings)

### 3. Test Game

1. Open main scene
2. Press Play
3. Test movement (WASD + Mouse)
4. Test sprint (Shift) + jump (Space)
5. Watch stamina bar drain

---

## 📖 Additional Documentation

| File | Location | Purpose |
|------|----------|---------|
| `README.md` | Project Root | Project overview |
| `TODO.md` | Project Root | Task list & roadmap |
| `HUD_EVENT_SYSTEM.md` | Project Root | HUD event documentation |
| `GIT_WORKFLOW_GUIDE.md` | Project Root | Git usage guide |
| `TETRAHEDRON_SYSTEM.md` | Project Root | Tetrahedron mesh system |
| `README.md` | **Assets/Docs/** | This file |
| `TODO.md` | **Assets/Docs/** | See below |

---

## 🎯 Current Status

**Production Ready:** ✅ YES

| System | Completion | Notes |
|--------|------------|-------|
| Core Architecture | 100% | Plug-in-and-out working |
| Player Controller | 100% | Sprint + jump with stamina |
| Status Effects | 100% | Full buff/debuff system |
| HUD & UI | 100% | Bars + dialogs + windows |
| Inventory | 100% | Slot-based with UI |
| Database | 100% | JSON persistence |
| Enemies | 80% | Basic AI working |
| Maze Generation | 100% | Procedural + rendering |

---

## 🔧 Troubleshooting

### Common Issues

**Unity won't compile:**
```bash
# Clear cache
.\clear-unity-cache.bat
# Reopen Unity
```

**Git issues:**
```bash
# Check status
.\git-status.ps1
# Fix with menu
.\git-quick.bat
```

**UI not showing:**
- Check if `UIBarsSystem` component is in scene
- Check Canvas is enabled
- Check sorting order (should be 100+)

**Input not working:**
- Verify New Input System is enabled (Project Settings)
- Check InputSystem_Actions.inputactions exists
- Check PlayerController has InputSystem reference

---

## 📞 Support

For issues or questions:
1. Check Console for errors
2. Run `.\scan-project-errors.ps1`
3. Review `HUD_EVENT_SYSTEM.md` for UI details
4. Check Unity Editor.log for crashes

---

**Happy Developing!** 🎮✨
