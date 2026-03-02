# Project Hierarchy & Architecture
**Project:** PeuImporte - Unity 6 Roguelike
**Unity Version:** 6000.3.7f1
**Last Updated:** 2026-03-02
**Architecture:** Plug-in-and-Out System

---

## Executive Summary

This project uses a **plug-in-and-out architecture** where all scripts work independently but revolve around a central core system. The `GameManager` acts as the main pivot point, with `EventHandler` serving as the central communication hub.

---

## Core Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        GameManager.cs                            │
│                    (Main Pivot Point)                            │
│          - Game State Management (Singleton)                     │
│          - Scene Management                                      │
│          - Global Score/State Events                             │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                       EventHandler.cs                            │
│                   (Central Event Hub)                            │
│          - 50+ Event Types                                       │
│          - Player/Combat/Item/Door/UI Events                     │
│          - Publish-Subscribe Pattern                             │
└─────────────────────────────────────────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        ▼                     ▼                     ▼
┌───────────────┐    ┌────────────────┐   ┌────────────────┐
│  ItemEngine   │    │InteractionSys  │   │  CombatSystem  │
│ (Item Registry)│   │ (Input Manager)│   │ (Damage/Stats) │
└───────────────┘    └────────────────┘   └────────────────┘
        │                     │                     │
        └─────────────────────┼─────────────────────┘
                              │
                ┌─────────────┴─────────────┐
                ▼                           ▼
        ┌───────────────┐           ┌───────────────┐
        │ BehaviorEngine│           │  PlayerStats  │
        │  (Base Class) │           │ (Stat Engine) │
        └───────────────┘           └───────────────┘
                │
        ┌───────┴────────┐
        ▼                ▼
┌─────────────┐   ┌─────────────┐
│  DoorsEngine│   │ChestBehavior│
│ (Door Logic)│   │ (Chest UI)  │
└─────────────┘   └─────────────┘
```

---

## Folder Structure

```
PeuImporte/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/              # Core Systems (Pivot Point)
│   │   │   ├── GameManager.cs         ← Main pivot
│   │   │   ├── EventHandler.cs        ← Central event hub
│   │   │   ├── ItemEngine.cs          ← Item registry
│   │   │   ├── InteractionSystem.cs   ← Input manager
│   │   │   ├── CombatSystem.cs        ← Damage calculation
│   │   │   ├── SeedManager.cs         ← Procedural seed
│   │   │   └── Base/
│   │   │       └── BehaviorEngine.cs  ← Base class for items
│   │   │
│   │   ├── HUD/               # UI System
│   │   │   ├── HUDEngine.cs           ← Modular HUD manager
│   │   │   ├── HUDSystem.cs           ← Legacy HUD
│   │   │   ├── UIBarsSystem.cs        ← Status bars
│   │   │   ├── HUDModule.cs           ← Base module class
│   │   │   └── DebugHUD.cs            ← Debug overlay
│   │   │
│   │   ├── Player/            # Player Controller
│   │   │   ├── PlayerController.cs    ← Movement/Input
│   │   │   ├── PlayerStats.cs         ← Health/Mana/Stamina
│   │   │   └── PlayerHealth.cs        ← Health component
│   │   │
│   │   ├── Inventory/         # Inventory System
│   │   │   ├── Inventory.cs           ← Data store
│   │   │   ├── InventoryUI.cs         ← UI display
│   │   │   └── ItemPickup.cs          ← Collectible logic
│   │   │
│   │   ├── Status/            # Status Effects
│   │   │   ├── StatsEngine.cs         ← Pure stat calculations
│   │   │   ├── StatusEffect.cs        ← Buff/Debuff logic
│   │   │   └── DamageType.cs          ← Damage categories
│   │   │
│   │   ├── Ressources/        # Resource Factories
│   │   │   ├── MazeRenderer.cs        ← Maze visualization
│   │   │   ├── TorchPool.cs           ← Object pooling
│   │   │   └── DoorFactory.cs         ← Door generation
│   │   │
│   │   ├── Ennemies/          # Enemy AI
│   │   │   └── Ennemi.cs              ← Basic enemy logic
│   │   │
│   │   └── Editor/            # Editor Tools
│   │       └── BuildScript.cs         ← Automated builds
│   │
│   ├── DB_SQLite/           # Database Save/Load
│   │   ├── DatabaseManager.cs
│   │   └── DatabaseSaveLoadHelper.cs
│   │
│   ├── Input/               # New Input System
│   │   └── InputSystem_Actions.inputactions
│   │
│   ├── Scenes/              # Unity Scenes
│   ├── Prefabs/             # Prefab Assets
│   ├── Art/                 # Visual Assets
│   └── Docs/                # Documentation (all *.md files)
│
├── diff_tmp/                # Temporary diff files (auto-cleanup 2 days)
├── Backup_Solution/         # Read-only backup storage
└── ProjectSettings/         # Unity Project Settings
```

---

## Core Systems (Pivot Points)

### 1. GameManager.cs
**Location:** `Assets/Scripts/Core/GameManager.cs`
**Namespace:** `Code.Lavos.Core`

**Responsibilities:**
- Singleton game state manager
- Scene transitions
- Global score tracking
- Pause/GameOver/Victory states

**Key Events:**
```csharp
public static event System.Action<int> OnScoreChanged;
public static event System.Action<GameState> OnGameStateChanged;
```

**Usage:**
```csharp
// Access from anywhere
GameManager.Instance.AddScore(100);
GameManager.Instance.SetGameState(GameState.Paused);
```

---

### 2. EventHandler.cs
**Location:** `Assets/Scripts/Core/EventHandler.cs`
**Namespace:** `Code.Lavos.Core`

**Responsibilities:**
- Central event hub for all systems
- 50+ event types for decoupled communication
- Event invoker methods with debug logging

**Event Categories:**
| Category | Events |
|----------|--------|
| Player | Health, Mana, Stamina, Death, Respawn |
| Combat | DamageDealt, DamageTaken, Kill, Death |
| Item | PickedUp, Used, Dropped, Stacked |
| Door | Opened, Closed, Locked, TrapTriggered |
| Game | Score, Quest, Achievement |
| UI | Bars, Dialog, Notification, Window |

**Usage:**
```csharp
// Subscribe
EventHandler.Instance.OnPlayerHealthChanged += HandleHealthChange;

// Invoke
EventHandler.Instance.InvokePlayerHealthChanged(75f, 100f);
```

---

### 3. ItemEngine.cs
**Location:** `Assets/Scripts/Core/ItemEngine.cs`
**Namespace:** `Code.Lavos.Core`

**Responsibilities:**
- Central item registry
- Plug-in architecture for item behaviors
- Item location tracking
- Type-based queries

**Plug-in Pattern:**
```csharp
// All items inherit from BehaviorEngine
public class CustomItem : BehaviorEngine
{
    void Awake() {
        // Auto-registers with ItemEngine
    }
}
```

---

### 4. BehaviorEngine.cs (Base Class)
**Location:** `Assets/Scripts/Core/Base/BehaviorEngine.cs`
**Namespace:** `Code.Lavos.Core`

**Responsibilities:**
- Base class for all interactable items
- Provides common interaction interface
- Auto-registration with ItemEngine

**Known Implementations:**
- `DoorsEngine.cs` - Door logic
- `ChestBehavior.cs` - Chest UI and loot
- `ItemPickup.cs` - Collectible items
- Custom items (plug-in)

---

## Data Flow

### Player Interaction Flow
```
Player Input → InteractionSystem → EventHandler → Subscribers
     │
     ├──→ PlayerController (movement)
     ├──→ CombatSystem (attack/cast)
     └──→ Inventory (use item)
```

### Damage Calculation Flow
```
Player Attack → CombatSystem → StatsEngine (calculation)
                    │
                    ├──→ EventHandler.InvokeDamageDealt()
                    ├──→ PlayerStats.TakeDamage()
                    └──→ Enemy.TakeDamage()
```

### Item Collection Flow
```
Player Collision → ItemPickup → Inventory.Add()
                                    │
                                    └──→ EventHandler.InvokeItemPickedUp()
```

---

## Namespace Convention

**Standard:** `Code.Lavos.{Category}`

| Category | Namespace | Examples |
|----------|-----------|----------|
| Core | `Code.Lavos.Core` | GameManager, EventHandler, ItemEngine |
| HUD | `Code.Lavos.HUD` | HUDEngine, UIBarsSystem, HUDModule |
| Status | `Code.Lavos.Status` | StatsEngine, StatusEffect, DamageType |
| Build | `Code.Lavos.Build` | BuildScript |

**Note:** All files previously using `Unity6.LavosTrial.*` have been migrated to `Code.Lavos.*` (2026-03-02).

---

## Unity 6 Features Used

| Feature | Usage |
|---------|-------|
| `FindFirstObjectByType` | Singleton pattern (Unity 6 API) |
| New Input System | `InputActionReference` for player input |
| UTF-8 Encoding | All C# files |
| Unix LF Line Endings | Cross-platform compatibility |

---

## Coding Standards

### File Header Template
```csharp
// {FileName}.cs
// {Description}
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// {Role}: {Description}
```

### Namespace Pattern
```csharp
namespace Code.Lavos.{Category}
{
    public class {ClassName} : MonoBehaviour
```

### Singleton Pattern (Unity 6)
```csharp
public static {ClassName} Instance { get; private set; }

void Awake()
{
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);
}
```

---

## Backup & Version Control

### Backup Script
**File:** `backup.ps1`
**Location:** Project root
**Function:** Smart backup with MD5 hash detection

**Usage:**
```powershell
.\backup.ps1
```

### Diff Files
**Location:** `diff_tmp/`
**Retention:** 2 days (auto-cleanup via `cleanup-old-diffs.ps1`)

### Git Workflow
See `Assets/Docs/GIT_WORKFLOW.md` for detailed setup.

---

## Documentation Files

| Document | Location |
|----------|----------|
| TODO List | `Assets/Docs/TODO.md` |
| Door System | `Assets/Docs/DOOR_SYSTEM.md` |
| Seed System | `Assets/Docs/SEED_SYSTEM.md` |
| Room System | `Assets/Docs/ROOM_SYSTEM.md` |
| Interaction System | `Assets/Docs/INTERACTION_SYSTEM_DOCUMENTATION.md` |
| SFX/VFX | `Assets/Docs/SFXVFX_ENGINE.md` |
| HUD Setup | `Assets/Docs/HUD_Disposition_Implementation.md` |
| Git Workflow | `Assets/Docs/GIT_WORKFLOW.md` |
| Error Reports | `Assets/Docs/ErrorScanReport_2026-03-01.md` |

---

## Quick Reference

### Add New Item (Plug-in Pattern)
1. Create class inheriting from `BehaviorEngine`
2. Implement required methods (`Interact`, `CanInteract`, etc.)
3. Add to scene - auto-registers with `ItemEngine`

### Subscribe to Event
```csharp
void OnEnable() {
    EventHandler.Instance.OnPlayerHealthChanged += HandleHealth;
}

void OnDisable() {
    EventHandler.Instance.OnPlayerHealthChanged -= HandleHealth;
}
```

### Access Core Systems
```csharp
GameManager.Instance      // Game state
EventHandler.Instance     // Events
ItemEngine.Instance       // Item registry
InteractionSystem.Instance // Input
CombatSystem.Instance     // Damage
PlayerStats.Instance      // Stats
```

---

*Document generated: 2026-03-02*
*Unity 6 (6000.3.7f1) compatible*
*UTF-8 encoding - Unix line endings*
