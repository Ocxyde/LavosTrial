# PeuImporte - Unity 6 Game Project

**Unity Version:** 6000.3.7f1  
**Render Pipeline:** URP Standard  
**Input System:** New Input System  
**Coding Standard:** Unity 6  
**Last Updated:** 2026-03-01  
**Status:** ✅ **PRODUCTION READY - PLAYABLE BUILD**

---

## 🎮 Quick Start

### **Setup Instructions**

1. **Open in Unity 6000.3.7f1**
2. **Add these components to your Player GameObject:**
   - CharacterController
   - PlayerController
   - PlayerStats ← **Required for stamina system!**
3. **Create EventHandler:**
   - Create Empty GameObject → Name: "EventHandler"
   - Add Component: `EventHandlerInitializer`
4. **Add UI:**
   - Create Empty GameObject → Name: "UIBarsSystem"
   - Add Component: `UIBarsSystem`
5. **Press Play!**

### **Controls**

| Action | Key |
|--------|-----|
| Move | WASD / Arrow Keys |
| Sprint | Hold Shift (costs 1% stamina/sec) |
| Jump | Space (costs 1% stamina) |
| Look | Mouse |
| Interact | E |
| Unlock Cursor | ESC / Tab |

---

## 🏗️ Architecture

### **Core-Centric Plug-in-and-Out System**

**The Core is the heart of the system** - all other scripts work independently but pivot around Core main files:

```
┌─────────────────────────────────────────────────────────┐
│              CORE (Heart of System)                     │
│  GameManager │ ItemEngine │ BehaviorEngine │ MazeGen   │
│  SpawnPlacer │ TrapSystem │ DoorSystem     │ EventHandler │
└─────────────────────────────────────────────────────────┘
                          │
        ┌─────────────────┼─────────────────┐
        ▼                 ▼                 ▼
┌───────────────┐  ┌───────────────┐  ┌───────────────┐
│  Player       │  │  Ressources   │  │  HUD          │
│  (plugs in)   │  │  (plugs in)   │  │  (plugs in)   │
└───────────────┘  └───────────────┘  └───────────────┘
```

**How it works:**
1. **Core files** define base classes and managers
2. **Other systems** inherit from Core base classes
3. **Plug-in**: Add component → automatically registers with Core
4. **Plug-out**: Remove component → automatically unregisters

**Example:**
```csharp
// DoubleDoor inherits from BehaviorEngine (Core)
public class DoubleDoor : BehaviorEngine
{
    // Automatically registers with ItemEngine
    // Automatically works with SpawnPlacerEngine
    // Just add component → it works!
}
```

```
┌─────────────────────────────────────────────────────────┐
│                    GameManager                          │
│              (Central Game State Singleton)             │
└─────────────────────────────────────────────────────────┘
                          │
        ┌─────────────────┼─────────────────┐
        ▼                 ▼                 ▼
┌───────────────┐  ┌───────────────┐  ┌───────────────┐
│  ItemEngine   │  │ PlayerController│  │  EventHandler│
│  (Items Mgr)  │  │  (New Input)   │  │ (Centralized) │
└───────┬───────┘  └───────┬───────┘  └───────┬───────┘
        │                  │                  │
        ▼                  ▼                  ▼
┌───────────────┐  ┌───────────────┐  ┌───────────────┐
│BehaviorEngine │  │ PlayerStats   │  │  UIBarsSystem│
│  (Base Class) │  │  (StatsEngine)│  │  (Floating)  │
└───────┬───────┘  └───────┬───────┘  └───────────────┘
        │                  │                  │
        ▼                  ▼                  ▼
┌───────────────┐  ┌───────────────┐  ┌───────────────┐
│ DoubleDoor    │  │ StatsEngine   │  │ DialogEngine │
│ ChestBehavior │  │ (Pure C#)     │  │  (Dialogs)   │
└───────────────┘  └───────────────┘  └───────────────┘
                                                  │
                                                  ▼
                                          ┌───────────────┐
                                          │ PopWinEngine │
                                          │  (Windows)   │
                                          └───────────────┘
```

---

## 📦 Core Systems

### **1. Player System** (`Assets/Scripts/Player/`)

| Script | Purpose | Status |
|--------|---------|--------|
| `PlayerController.cs` | Movement, camera, input | ✅ Complete |
| `PlayerStats.cs` | Stats wrapper (StatsEngine) | ✅ Complete |
| `PlayerHealth.cs` | Health management | ✅ Complete |
| `PersistentPlayerData.cs` | Save/load data | ✅ Complete |

**Features:**
- ✅ Sprint system (+10% speed, 1% stamina/sec)
- ✅ Jump system (1% stamina per jump)
- ✅ Camera follow with head bob
- ✅ Interaction system (E key)
- ✅ New Input System only

---

### **2. Status System** (`Assets/Scripts/Status/`)

| Script | Purpose | Status |
|--------|---------|--------|
| `StatsEngine.cs` | Pure C# stat calculations | ✅ Complete |
| `StatusEffectData.cs` | Effect definitions | ✅ Complete |
| `StatModifier.cs` | Stat modifiers | ✅ Complete |
| `DamageType.cs` | 11 damage types | ✅ Complete |

**Features:**
- ✅ Buff/Debuff system
- ✅ Stat modifiers (additive, multiplicative, override)
- ✅ Damage resistances/vulnerabilities
- ✅ DoT/HoT (damage/heal over time)

---

### **3. UI System** (`Assets/Scripts/HUD/`)

| Script | Purpose | Status |
|--------|---------|--------|
| `UIBarsSystem.cs` | Health/Mana/Stamina bars | ✅ Complete |
| `DialogEngine.cs` | Floating text + dialogs | ✅ Complete |
| `PopWinEngine.cs` | Popup windows + inventories | ✅ Complete |
| `EventHandler.cs` | Central event manager | ✅ Complete |
| `HUDSystem.cs` | Main HUD manager | ✅ Complete |

**Features:**
- ✅ Real-time bar updates (events)
- ✅ Color interpolation (based on %)
- ✅ Floating combat text (damage/heal/stamina)
- ✅ Dialog system (bottom-left, resizable)
- ✅ Inventory windows (slot-based)
- ✅ Stats board window (scrollable)
- ✅ Centralized event system

---

### **4. Event System** (`Assets/Scripts/Core/`)

| Script | Purpose | Status |
|--------|---------|--------|
| `EventHandler.cs` | Central event manager | ✅ Complete |
| `EventHandlerInitializer.cs` | Auto-creates EventHandler | ✅ Complete |

**Event Categories:**
- ✅ Player events (health, mana, stamina, stats)
- ✅ Combat events (damage, healing, death)
- ✅ Item events (pickup, use, drop)
- ✅ Game events (score, level, quests)
- ✅ UI events (bars, dialogs, windows)

**Usage Example:**
```csharp
// Subscribe to events
EventHandler.Instance.OnPlayerHealthChanged += OnHealthChanged;
EventHandler.Instance.OnPlayerStaminaUsed += OnStaminaUsed;

// Invoke events
EventHandler.Instance.InvokePlayerStaminaUsed(5f);
EventHandler.Instance.InvokeFloatingText("-50", Color.red);
```

---

### **5. Inventory System** (`Assets/Scripts/Inventory/`)

| Script | Purpose | Status |
|--------|---------|--------|
| `Inventory.cs` | Inventory manager (Singleton) | ✅ Complete |
| `InventorySlot.cs` | Slot data structure | ✅ Complete |
| `InventoryUI.cs` | UI display | ✅ Complete |
| `InventorySlotUI.cs` | UI slot component | ✅ Complete |
| `ItemPickup.cs` | World pickups | ✅ Complete |

**Features:**
- ✅ Stackable items
- ✅ Grid-based UI (via PopWinEngine)
- ✅ Item categories (Consumable, Equipment, etc.)

---

### **6. Core Systems** (`Assets/Scripts/Core/`)

| Script | Purpose | Status |
|--------|---------|--------|
| `GameManager.cs` | Central game state singleton | ✅ Complete |
| `ItemEngine.cs` | Item registry & management | ✅ Complete |
| `BehaviorEngine.cs` | Base class for interactables | ✅ Complete |
| `MazeGenerator.cs` | Procedural maze generation | ✅ Complete |
| `DrawingManager.cs` | Texture generation | ✅ Complete |
| `ParticleGenerator.cs` | Particle VFX | ✅ Complete |
| `DoubleDoor.cs` | Procedural doors with glow | ✅ Complete |
| `ChestBehavior.cs` | Treasure chests with loot | ✅ Complete |

---

### **7. Database System** (`Assets/DB_SQLite/`)

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

### **Stamina System**

| Action | Cost | Notes |
|--------|------|-------|
| Sprint | 1% current/sec | Exponential decay |
| Jump | 1% current/jump | Min 0.5 cost |
| Cannot sprint/jump | < 1 stamina | Auto-stops |

**Bar Visualization:**
- **Full (100%):** Bright yellow
- **Medium (50%):** Orange-yellow
- **Low (<30%):** Dark orange/red

---

### **Combat System**

- ✅ 11 damage types (Physical, Fire, Ice, Lightning, etc.)
- ✅ Critical hits (5% base chance, 150% damage)
- ✅ Resistance system (per damage type)
- ✅ Invincibility frames (0.5s after hit)
- ✅ Floating damage numbers (red)
- ✅ Floating heal numbers (green)

---

### **UI Features**

**Bars:**
- **Health:** Left edge, vertical, green→red
- **Mana:** Right edge, vertical, blue
- **Stamina:** Bottom edge, horizontal, yellow

**Floating Text:**
- Damage: Red numbers
- Heal: Green numbers
- Stamina loss: Orange numbers
- Stamina gain: Yellow numbers

**Dialogs:**
- Bottom-left positioning (RPG style)
- Semi-transparent background
- Auto-fade after duration

**Windows:**
- Inventory (slot-based grid)
- Stats board (scrollable)
- Shop/store
- Custom panels

---

## 🛠️ Development Tools

### **Automation Scripts**

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

### **Git Workflow**

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

## 📖 Documentation

| File | Location | Purpose |
|------|----------|---------|
| `README.md` | Project Root | This file |
| `TODO.md` | Project Root | Task list & roadmap |
| `HUD_EVENT_SYSTEM.md` | Project Root | HUD event documentation |
| `GIT_WORKFLOW_GUIDE.md` | Project Root | Git usage guide |
| `TETRAHEDRON_SYSTEM.md` | Project Root | Tetrahedron mesh system |
| `README.md` | Assets/Docs/ | Project overview |
| `TODO.md` | Assets/Docs/ | Development roadmap |

---

## 🚀 Getting Started

### **1. Open Project**

1. Open Unity Hub
2. Click "Add" → Select `D:\travaux_Unity\PeuImporte`
3. Open with Unity 6000.3.7f1

### **2. Setup Scene**

1. **Create Player:**
   - Right-click Hierarchy → 3D Object → Capsule
   - Rename to "Player"
   - Add Component → CharacterController
   - Add Component → PlayerController
   - Add Component → **PlayerStats** ← Required!
   - Add Camera as child (or assign in PlayerController)
   - Set Tag to "Player"

2. **Create EventHandler:**
   - Create Empty GameObject
   - Name: "EventHandler"
   - Add Component → `EventHandlerInitializer`

3. **Create UI:**
   - Create Empty GameObject
   - Name: "UIBarsSystem"
   - Add Component → `UIBarsSystem`

4. **Configure PlayerStats:**
   - Max Health: 1000
   - Max Mana: 500
   - Max Stamina: 100

### **3. Test Game**

1. Press Play
2. **WASD** to move
3. **Shift** to sprint (watch stamina bar!)
4. **Space** to jump (costs stamina)
5. **Watch UI bars** - they update in real-time!
6. **Watch floating text** - appears when stamina changes

---

## 🔧 Troubleshooting

### **Common Issues**

**"PlayerStats not initialized" warning:**
- **Cause:** PlayerStats component missing from Player
- **Fix:** Add PlayerStats component to Player GameObject

**Stamina bar not showing:**
- **Cause:** UIBarsSystem not in scene
- **Fix:** Add UIBarsSystem component to GameObject

**Build fails with nunit error:**
- **Cause:** Test files included in build
- **Fix:** Move test files to Editor folder or disable them

**UI not visible:**
- **Cause:** Canvas sorting order too low
- **Fix:** Set Canvas sorting order to 100+

**Controls don't work:**
- **Cause:** New Input System not enabled
- **Fix:** Project Settings → Input System → Set to "Both" or "New"

---

## 📞 Support

For issues or questions:
1. Check Console for errors
2. Run `.\scan-project-errors.ps1`
3. Review `HUD_EVENT_SYSTEM.md` for UI details
4. Check `Assets/Docs/README.md` for architecture

---

## 🎯 Current Status

**Production Ready:** ✅ YES - **BUILDABLE & PLAYABLE**

| System | Completion | Status |
|--------|------------|--------|
| Core Architecture | 100% | ✅ Complete |
| Player Controller | 100% | ✅ Complete |
| Status Effects | 100% | ✅ Complete |
| HUD & UI | 100% | ✅ Complete |
| Event System | 100% | ✅ Complete |
| Inventory | 100% | ✅ Complete |
| Database | 100% | ✅ Complete |
| Maze Generation | 100% | ✅ Complete |

---

**Happy Gaming! 🎮✨**

*Built with Unity 6000.3.7f1 - URP Standard - New Input System*
