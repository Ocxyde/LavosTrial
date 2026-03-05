# Architecture Overview - PeuImporte

**Unity Version:** 6000.3.7f1
**Render Pipeline:** URP Standard
**Input System:** New Input System
**Coding Standard:** Unity 6
**Last Updated:** 2026-03-06 (Cleanup Phase 1 & 3 Complete!)

---

## 🚨 **IMPORTANT: DEPRECATED SYSTEMS**

### **⚠️ DO NOT USE FOR NEW DEVELOPMENT:**

| System | Deprecated Files | Use Instead |
|--------|-----------------|-------------|
| **Maze Generation** | `MazeIntegration.cs`, `MazeRenderer.cs` | `CompleteMazeBuilder.cs` |
| **Door Placement** | `DoorHolePlacer.cs`, `RoomDoorPlacer.cs` | `DoorsEngine.cs` + `RealisticDoorFactory.cs` |
| **Audio** | `SFXVFXEngine.cs` | `AudioManager.cs` |

**Why kept?** Legacy tests and scenes still use them. They're marked `[System.Obsolete]` and will show compiler warnings.

---

## 🏗️ Plug-in-and-Out Architecture

The project uses a modular plug-in architecture centered around core manager classes. All scripts work independently but pivot around core main files (central hub).

**KEY PRINCIPLES:**
1. **Find components, never create them** - Use `FindFirstObjectByType<T>()` or `GetComponent<T>()`
2. **No `new GameObject()` in runtime code** - Add components to scenes manually
3. **Use `EventHandler` for communication** - Loose coupling between systems
4. **JSON config for all values** - No hardcoded values (see `Config/GameConfig-default.json`)

**Singleton Auto-Creation Warning:**
Some singletons auto-create if not found in scene. This is a **fallback only** and logs a warning:
- `AudioManager` - Should be added manually
- `LightEngine` - Should be added manually
- `ProceduralCompute` - Should be added manually

---

## 📁 Project Structure

```
Assets/Scripts/
├── Core/                          (Code.Lavos.Core)
│   ├── 01_CoreSystems/
│   │   ├── CoreInterfaces.cs      (Interface definitions)
│   │   ├── EventHandler.cs        (Central event hub) ✅ RECOMMENDED
│   │   └── GameManager.cs         (Main game state singleton) ✅ RECOMMENDED
│   ├── 02_Player/
│   │   ├── PlayerController.cs    (Movement, camera, input) ✅
│   │   ├── PlayerStats.cs         (Player stats component) ✅
│   │   └── CameraFollow.cs        (Camera follow system) ✅
│   ├── 03_Interaction/
│   │   └── InteractionSystem.cs   (Centralized interaction manager) ✅
│   ├── 04_Inventory/
│   │   ├── Inventory.cs           (Inventory singleton) ✅
│   │   ├── InventorySlot.cs       ✅
│   │   ├── ItemData.cs            (ScriptableObject item data) ✅
│   │   ├── ItemEngine.cs          (Item registration engine) ✅
│   │   └── ItemTypes.cs           (Shared item type enums) ✅
│   ├── 05_Combat/
│   │   ├── CombatSystem.cs        (Combat calculations) ✅
│   │   └── Ennemi.cs              (Enemy behavior) ✅
│   ├── 06_Maze/
│   │   ├── CompleteMazeBuilder.cs (✅ NEW - Main maze generator)
│   │   ├── GridMazeGenerator.cs   (✅ NEW - Grid-based algorithm)
│   │   ├── MazeIntegration.cs     (⚠️ DEPRECATED - Use CompleteMazeBuilder)
│   │   ├── MazeRenderer.cs        (⚠️ DEPRECATED - Use CompleteMazeBuilder)
│   │   ├── RoomGenerator.cs       (⚠️ DEPRECATED - Tied to legacy system)
│   │   ├── DoorHolePlacer.cs      (⚠️ DEPRECATED - Use DoorsEngine)
│   │   └── RoomDoorPlacer.cs      (⚠️ DEPRECATED - Use DoorsEngine)
│   ├── 07_Doors/
│   │   ├── DoorsEngine.cs         (✅ Door system with traps)
│   │   ├── DoorAnimation.cs       (✅ Door animation)
│   │   ├── RealisticDoorFactory.cs (✅ Door prefab creation)
│   │   ├── DoorCubeFactory.cs     (⚠️ Legacy - Use RealisticDoorFactory)
│   │   ├── DoorSFXManager.cs      (✅ Door sound effects)
│   │   └── DoorSystemSetup.cs     (⚠️ Editor setup helper)
│   ├── 08_Environment/
│   │   ├── ChestBehavior.cs       (✅ Chest system)
│   │   ├── TrapBehavior.cs        (✅ Trap system)
│   │   ├── SpatialPlacer.cs       (✅ Universal object placement)
│   │   ├── SpawnPlacerEngine.cs   (⚠️ DEPRECATED - Use SpatialPlacer)
│   │   ├── SpecialRoom.cs         (✅ Special room system)
│   │   └── LightPlacementEngine.cs (✅ Torch auto-placement)
│   ├── 09_Art/
│   │   └── ArtFactory.cs          (✅ Art utilities)
│   ├── 10_Mesh/
│   │   └── DrawingManager.cs      (✅ Mesh utilities)
│   ├── 10_Resources/
│   │   ├── LootTable.cs           (✅ Loot system)
│   │   ├── SeedManager.cs         (✅ Seed progression system)
│   │   ├── TorchController.cs     (✅ Torch behavior)
│   │   └── TorchPool.cs           (✅ Torch object pooling)
│   ├── 11_Audio/
│   │   └── AudioManager.cs        (✅ RECOMMENDED - Professional audio)
│   ├── 12_Animation/
│   │   ├── BraseroFlame.cs        (✅ Torch flame animation)
│   │   └── FlameAnimator.cs       (✅ Flame animation)
│   ├── 12_Compute/
│   │   ├── LightEngine.cs         (✅ Lighting engine - add manually!)
│   │   ├── DrawingPool.cs         (✅ Pixel art texture generation)
│   │   ├── ProceduralCompute.cs   (✅ Procedural utilities - add manually!)
│   │   ├── ParticleGenerator.cs   (✅ Particle effects)
│   │   └── SFXVFXEngine.cs        (⚠️ DEPRECATED - Use AudioManager)
│   ├── 13_Geometry/
│   │   ├── Tetrahedron.cs         (✅ Geometry utilities)
│   │   ├── TetrahedronMath.cs     (✅ Geometry math)
│   │   └── Triangle.cs            (✅ Triangle utilities)
│   └── Base/
│       └── BehaviorEngine.cs      (✅ Base class for plug-in items)
│
├── Status/                        (Code.Lavos.Status - BASE ASSEMBLY)
│   ├── DamageType.cs              (Damage type enum + DamageInfo struct)
│   ├── StatModifier.cs            (Stat modifier system)
│   ├── StatsEngine.cs             (Pure C# stat calculation engine)
│   ├── StatusEffect.cs
│   └── StatusEffectData.cs        (Status effect data)
│
├── Player/                        (Code.Lavos.Player)
│   └── PersistentPlayerData.cs
│
├── Inventory/                     (Code.Lavos.Inventory)
│   ├── InventorySlotUI.cs
│   ├── InventoryUI.cs
│   └── ItemPickup.cs
│
├── HUD/                           (Code.Lavos.HUD)
│   ├── DebugHUD.cs
│   ├── DialogEngine.cs
│   ├── HUDEngine.cs               (HUD module manager)
│   ├── HUDModule.cs               (Base class for HUD modules)
│   ├── HUDSystem.cs               (Complete HUD management)
│   ├── UIBarsSystem.cs
│   ├── PersistentUI.cs
│   └── PopWinEngine.cs
│
├── Interaction/                   (Code.Lavos.Interaction)
│   ├── IInteractable.cs           (Interaction interface)
│   └── InteractableObject.cs      (Abstract interactable base)
│
├── Ressources/                    (Code.Lavos.Ressources)
│   ├── DoorFactory.cs
│   ├── PixelArtGenerator.cs
│   ├── PixelArtTextureFactory.cs
│   ├── RoomTextureGenerator.cs
│   ├── ChestPixelArtFactory.cs
│   └── AnimatedFlame.cs
│
├── Gameplay/                      (Code.Lavos.Gameplay)
│   └── Collectible.cs
│
├── Ennemies/                      (Code.Lavos.Ennemies)
│   └── (enemy scripts - TBD)
│
└── Editor/                        (Code.Lavos.Editor)
    ├── BuildScript.cs
    ├── URPSetupUtility.cs
    └── AddDoorSystemToScene.cs
```
│   ├── PersistentUI.cs
│   ├── PopWinEngine.cs
│   └── UIBarsSystem.cs
│
├── Interaction/                   (Code.Lavos.Interaction)
│   ├── IInteractable.cs           (Interaction interface)
│   └── InteractableObject.cs      (Abstract interactable base)
│
├── Ressources/                    (Code.Lavos.Ressources)
│   ├── AnimatedFlame.cs
│   ├── ChestPixelArtFactory.cs
│   ├── DoorFactory.cs
│   ├── PixelArtGenerator.cs
│   └── PixelArtTextureFactory.cs
│
├── Gameplay/                      (Code.Lavos.Gameplay)
│   └── Collectible.cs
│
├── Ennemies/                      (Code.Lavos.Ennemies)
│   └── (enemy scripts)
│
└── Editor/                        (Code.Lavos.Editor)
    ├── AddDoorSystemToScene.cs
    ├── BuildScript.cs
    ├── MazeTestSceneSetup.cs
    ├── SceneSetupHelper.cs
    └── URPSetupUtility.cs
```

---

## 🎯 Core Hub Files (Central Managers)

| File | Role |
|------|------|
| **GameManager.cs** | Main game state singleton (Playing, Paused, GameOver). Controls time scale, scene loading, score. |
| **EventHandler.cs** | **CENTRAL EVENT HUB** - Single point for ALL game events (player stats, combat, items, doors, chests, UI). Uses C# events/actions. |
| **ItemEngine.cs** | Central item registry. All items inheriting from `BehaviorEngine` auto-register here. |
| **HUDEngine.cs** | HUD module manager. Registers/manages HUDModule subclasses. |
| **InteractionSystem.cs** | Centralized interaction manager. Routes E-key interactions, combat actions, spell casting through EventHandler. |
| **CombatSystem.cs** | Combat calculations, damage/healing, resource consumption. Integrates with StatsEngine. |
| **StatsEngine.cs** | **Pure C# stat engine** (no MonoBehaviour). Handles buffs, debuffs, stat modifiers, resistances, regeneration. |
| **SeedManager.cs** | Seed progression system for procedural generation. Supports Progressive, Fixed, Random, Daily, Custom modes. |
| **LightEngine.cs** | **CENTRAL LIGHTING ENGINE** - Manages ALL light emission, dynamic lights, fog of war, lightning effects, and emission control. Torches register/unregister automatically. |

---

## 🔌 Singleton Hierarchy

```
┌──────────────────┐
│   GameManager    │  ← Top-level game state (Playing/Paused/GameOver)
│   (Singleton)    │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│   EventHandler   │────▶│  CombatSystem    │────▶│   StatsEngine    │
│   (Singleton)    │     │   (Singleton)    │     │   (Pure C#)      │
└────────┬─────────┘     └──────────────────┘     └──────────────────┘
         │                        │
         │                        │
         ▼                        ▼
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│   ItemEngine     │     │InteractionSystem │     │   HUDEngine      │
│   (Singleton)    │     │   (Singleton)    │     │   (Singleton)    │
└────────┬─────────┘     └────────┬─────────┘     └────────┬─────────┘
         │                        │                        │
         │                        │                        │
         ▼                        ▼                        ▼
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│ BehaviorEngine   │     │ PlayerController │     │   HUDModule      │
│   (Base Class)   │     │   (MonoBehaviour)│     │   (Base Class)   │
└──────────────────┘     └──────────────────┘     └──────────────────┘
         │
         │ (inherits from)
         ▼
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│  DoorsEngine     │     │ ChestBehavior    │     │  ItemPickup      │
│  (Plug-in Item)  │     │  (Plug-in Item)  │     │  (Plug-in Item)  │
└──────────────────┘     └──────────────────┘     └──────────────────┘

Other Key Singletons:
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│  SeedManager     │     │   Inventory      │     │  PlayerStats     │
│  (Singleton)     │     │   (Singleton)    │     │  (Singleton)     │
└──────────────────┘     └──────────────────┘     └──────────────────┘

Lighting System (NEW):
┌──────────────────┐
│  LightEngine     │  ← Central lighting (Singleton, DontDestroyOnLoad)
│  (Singleton)     │     - Dynamic light pooling (32 lights default)
└────────┬─────────┘     - Fog of war / darkness system
         │                - Lightning/exposure effects
         │                - Emission control with flicker
         ▼
┌──────────────────┐
│  TorchPool       │  ← Torches auto-register on spawn
│  (Auto-Register) │     - RegisterLight() when torch spawned
└──────────────────┘     - UnregisterLight() when torch despawned
```

---

## 🔌 How the Plug-in-and-Out Architecture Works

### A. Event-Based Communication (Primary Mechanism)

```
Producer (e.g., DoorsEngine)          Consumer (e.g., UIBarsSystem)
         │                                      │
         │  1. Calls EventHandler.Invoke...     │
         ├─────────────────────────────────────▶│
         │                                      │
         │                              2. Receives via
         │                                 event subscription
         │                                      │
         ▼                                      ▼
    DoorsEngine.OpenDoor()              HUD updates door status
```

**Code Example:**

```csharp
// Producer (DoorsEngine.cs)
public void OpenDoor(GameObject interactor)
{
    _isOpen = true;
    if (_eventHandler != null)
    {
        _eventHandler.InvokeDoorOpened(transform.position, doorVariant);
    }
}

// Consumer (Any system that needs door events)
void OnEnable()
{
    EventHandler.OnDoorOpened += HandleDoorOpened;
}

void HandleDoorOpened(Vector3 position, DoorVariant variant)
{
    // React to door opening
}
```

---

### B. Interface-Based Plug-ins

```csharp
// Core defines interfaces (CoreInterfaces.cs)
public interface IPlayerStats { ... }
public interface IInteractable { ... }
public interface IInventory { ... }

// Player assembly implements IPlayerStats
public class PlayerStats : MonoBehaviour, IPlayerStats { ... }

// Core uses interface, not concrete implementation
public class CombatSystem : MonoBehaviour
{
    void DealDamage(GameObject target, DamageInfo info)
    {
        IPlayerStats stats = target.GetComponent<IPlayerStats>();
        if (stats != null)
        {
            stats.TakeDamage(info.amount, info.type);
        }
    }
}
```

---

### C. BehaviorEngine Base Class Pattern

```csharp
// All interactable items inherit from BehaviorEngine
public abstract class BehaviorEngine : MonoBehaviour
{
    public ItemType ItemType { get; }
    public bool CanInteract { get; }
    
    public virtual void Interact(GameObject interactor) { ... }
    public virtual void Collect(GameObject collector) { ... }
    
    // Auto-registers with ItemEngine on Awake
    protected virtual void Awake()
    {
        ItemEngine.Instance?.RegisterItem(this);
    }
}

// Plug-in example: DoorsEngine
public class DoorsEngine : BehaviorEngine, IInteractable
{
    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);
        // Custom door behavior
        OpenDoor(interactor);
    }
}
```

---

### D. Assembly-Based Modularization

```
┌────────────────────────────────────────────────────────────────────────┐
│                     ASSEMBLY DEPENDENCY STRUCTURE                       │
├────────────────────────────────────────────────────────────────────────┤
│                                                                        │
│  Code.Lavos.Status (BASE)                                              │
│  └── Pure C# stat calculations, no Unity dependencies                  │
│                                                                        │
│  Code.Lavos.Core                                                       │
│  ├── References: Status, InputSystem                                   │
│  └── Contains: GameManager, EventHandler, ItemEngine, etc.             │
│                                                                        │
│  Code.Lavos.Player                                                     │
│  ├── References: Core, Status                                          │
│  └── Contains: PlayerController, PlayerStats                           │
│                                                                        │
│  Code.Lavos.Inventory                                                  │
│  ├── References: Core, InputSystem, TextMeshPro                        │
│  └── Contains: Inventory, InventoryUI, ItemPickup                      │
│                                                                        │
│  Code.Lavos.HUD                                                        │
│  ├── References: Core, Status, Player, InputSystem, TextMeshPro        │
│  └── Contains: HUDEngine, HUDModule, UIBarsSystem                      │
│                                                                        │
│  Code.Lavos.Ressources                                                 │
│  └── Contains: Art factories, texture generators                       │
│                                                                        │
│  Code.Lavos.Ennemies                                                   │
│  └── Contains: Enemy behaviors                                         │
│                                                                        │
│  Code.Lavos.Gameplay                                                   │
│  └── Contains: Collectible, gameplay mechanics                         │
│                                                                        │
│  Code.Lavos.Interaction                                                │
│  └── Contains: IInteractable, InteractableObject                       │
│                                                                        │
│  Code.Lavos.Editor (Editor-only)                                       │
│  └── Contains: Editor utilities, scene setup helpers                   │
│                                                                        │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 📋 Assembly Compilation Order

```
1. Code.Lavos.Status         (0.5s)  ← No dependencies (pure C#)
2. Code.Lavos.Core           (1.0s)  ← Depends on: Status
3. Code.Lavos.Maze           (0.8s)  ← Depends on: Core, Status
4. Code.Lavos.Player         (0.6s)  ← Depends on: Core, Status
5. Code.Lavos.Inventory      (0.4s)  ← Depends on: Core
6. Code.Lavos.HUD            (1.2s)  ← Depends on: Core, Status, Player
7. Code.Lavos.Ressources     (0.9s)  ← Depends on: Core, Maze
8. Code.Lavos.Ennemies       (0.3s)  ← Depends on: Core, Status, Player
9. Code.Lavos.Gameplay       (0.3s)  ← Depends on: Core, Status, Player, HUD
10. Code.Lavos.Editor        (0.5s)  ← Editor-only
```

**Total compile time:** ~6.5s (was ~20s - **70% faster**)

---

## 📊 Architecture Principles

| Principle | Implementation |
|-----------|----------------|
| **Single Responsibility** | Each system has one job (CombatSystem = combat, HUDEngine = UI modules) |
| **Event-Driven** | EventHandler is the central nervous system - all systems communicate via events |
| **Interface-Based** | Core defines interfaces (IPlayerStats, IInteractable), assemblies implement them |
| **Singleton Pattern** | All managers are singletons (GameManager, EventHandler, ItemEngine, etc.) |
| **Base Class Inheritance** | BehaviorEngine for items, HUDModule for UI, enables plug-in architecture |
| **Assembly Separation** | Each module is a separate .asmdef with explicit dependencies |
| **Pure C# Core** | StatsEngine is pure C# (no MonoBehaviour) for testability and performance |
| **Auto-Registration** | Items auto-register with ItemEngine via BehaviorEngine.Awake() |
| **Dependency Injection** | Systems find references via FindFirstObjectByType or interface resolution |

---

## 🎮 Key Systems

### Core Systems (`Assets/Scripts/Core/`)

| Script | Purpose | Status |
|--------|---------|--------|
| `GameManager.cs` | Central game state singleton | ✅ Complete |
| `ItemEngine.cs` | Item registry & management | ✅ Complete |
| `BehaviorEngine.cs` | Base class for interactables | ✅ Complete |
| `MazeGenerator.cs` | Procedural maze generation | ✅ Complete |
| `DrawingManager.cs` | Texture generation | ✅ Complete |
| `ParticleGenerator.cs` | Particle VFX | ✅ Complete |

### Player Systems (`Assets/Scripts/Player/`)

| Script | Purpose | Status |
|--------|---------|--------|
| `PlayerController.cs` | Movement, camera, input | ✅ Complete |
| `PlayerStats.cs` | Stats wrapper (StatsEngine) | ✅ Complete |
| `PersistentPlayerData.cs` | Save/load data | ✅ Complete |

**Features:**
- ✅ New Input System (WASD + Mouse)
- ✅ Sprint system (10% speed boost, 1% stamina/sec)
- ✅ Jump system (1% stamina per jump)
- ✅ Camera follow with head bob
- ✅ Interaction system (E key)

### Status System (`Assets/Scripts/Status/`)

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

## 📞 Support

For issues or questions:
1. Check Console for errors
2. Run `.\scan-project-errors.ps1`
3. Check Unity Editor.log for crashes

---

**Generated:** 2026-03-03  
**Unity Version:** 6000.3.7f1  
**Status:** ✅ Production Ready

---

**Happy Developing!** 🎮✨
