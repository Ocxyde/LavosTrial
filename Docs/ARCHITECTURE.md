# LavosTrial - Plug-in-and-Out Architecture

**Last Updated:** Mars 2026  
**Unity Version:** 6000.3.7f1 (Unity 6)  
**Pattern:** Engine-Based Plug-in-and-Out System

---

## 🎯 Core Philosophy

> **"All scripts work independently but revolve around a core main file"**

Your project uses an **Engine-Based Architecture** where:
- **Core Engines** act as central hubs (Singletons)
- **Plug-in Components** auto-register with engines
- **Loose Coupling** - components don't directly depend on each other
- **Event-Driven** - communication via events/actions

---

## 🏗️ Architecture Hierarchy

```
┌─────────────────────────────────────────────────────────────┐
│                    GameManager (Top Level)                  │
│  • Game State (Playing, Paused, GameOver, Victory)         │
│  • Score Management                                         │
│  • Scene Management                                         │
│  • Global Events                                            │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ All engines check GameManager.CurrentState
                              ▼
┌─────────────────────────────────────────────────────────────┐
│              CORE ENGINES (Central Hubs)                    │
├─────────────────────────────────────────────────────────────┤
│  ItemEngine        │ Central item registry                  │
│  SpawnPlacerEngine │ Procedural item placement              │
│  MazeGenerator     │ Maze generation                        │
│  HUDEngine         │ HUD management                         │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ Components plug into engines via base classes
                              ▼
┌─────────────────────────────────────────────────────────────┐
│            PLUG-IN COMPONENTS (Auto-Register)               │
├─────────────────────────────────────────────────────────────┤
│  ItemBehavior ─────┬─────► DoubleDoor                       │
│  (Base Class)      ├─────► ChestBehavior                    │
│                    ├─────► ItemPickup                       │
│                    └─────► BraseroFlame                     │
├─────────────────────────────────────────────────────────────┤
│  HUDModule ────────┬─────► BarsModule                       │
│  (Base Class)      ├─────► StatusEffectsModule              │
│                    ├─────► HotbarModule                     │
│                    └─────► PopupModule                      │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ Engines communicate via events
                              ▼
┌─────────────────────────────────────────────────────────────┐
│              PLAYER (Main Actor)                            │
├─────────────────────────────────────────────────────────────┤
│  PlayerController  │ Input, movement, interaction           │
│  PlayerStats       │ Stats via StatsEngine                  │
│  PlayerHealth      │ Health (legacy + StatsEngine)          │
│  Inventory         │ Item management                        │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔌 Plug-in-and-Out Pattern

### How It Works

```csharp
// 1. ENGINE: Central registry with Singleton pattern
public class ItemEngine : MonoBehaviour
{
    public static ItemEngine Instance { get; private set; }
    
    private List<ItemBehavior> _registeredItems;
    
    private void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        _registeredItems = new List<ItemBehavior>();
    }
    
    // Registration method
    public void RegisterItem(ItemBehavior item)
    {
        if (!_registeredItems.Contains(item))
            _registeredItems.Add(item);
    }
    
    // Unregistration method
    public void UnregisterItem(ItemBehavior item)
    {
        _registeredItems.Remove(item);
    }
}

// 2. BASE CLASS: Auto-register in Awake()
public abstract class ItemBehavior : MonoBehaviour
{
    protected virtual void Awake()
    {
        // AUTO-REGISTER: Plug into engine
        ItemEngine.Instance?.RegisterItem(this);
    }
    
    protected virtual void OnDestroy()
    {
        // AUTO-UNREGISTER: Unplug from engine
        ItemEngine.Instance?.UnregisterItem(this);
    }
}

// 3. PLUG-IN COMPONENT: Just inherit, no manual registration
public class ChestBehavior : ItemBehavior
{
    // Automatically registered with ItemEngine!
    // Automatically unregistered when destroyed!
    
    public override void Interact(GameObject interactor)
    {
        // Custom behavior
        base.Interact(interactor);
        Open();
    }
}
```

### Usage Example

```csharp
// In Unity Editor:
// 1. Add ChestBehavior component to any GameObject
// 2. That's it! Chest auto-registers with ItemEngine

// In code - Access all registered items:
var allChests = ItemEngine.Instance.GetItemsOfType<ChestBehavior>();
var nearestItem = ItemEngine.Instance.GetNearestInteractable(playerPos, 3f);

// Events - No direct coupling needed:
ItemEngine.Instance.OnItemCollected += (item) => {
    Debug.Log($"Item collected: {item.ItemType}");
};
```

---

## 📦 Core Engines

### 1. GameManager (Top Level)

**Location:** `Assets/Scripts/Core/GameManager.cs`

**Purpose:** Global game state orchestration

```csharp
// Singleton
GameManager.Instance

// Key Methods:
GameManager.Instance.SetGameState(GameState.Playing)
GameManager.Instance.AddScore(100)
GameManager.Instance.LoadScene("NextLevel")

// Events:
GameManager.OnScoreChanged += (score) => { };
GameManager.OnGameStateChanged += (state) => { };
```

**Plugins:** None (top-level coordinator)

---

### 2. ItemEngine

**Location:** `Assets/Scripts/Core/ItemEngine.cs`

**Purpose:** Central item registry and management

```csharp
// Singleton
ItemEngine.Instance

// Key Methods:
ItemEngine.Instance.RegisterItem(item)
ItemEngine.Instance.GetNearestInteractable(pos, range)
ItemEngine.Instance.GetItemsOfType<T>()
ItemEngine.Instance.InteractWithNearest(pos, interactor)

// Events:
ItemEngine.Instance.OnItemRegistered += (item) => { };
ItemEngine.Instance.OnItemCollected += (item) => { };
```

**Plugins (via ItemBehavior base class):**
- `DoubleDoor` - Double-sided doors
- `ChestBehavior` - Treasure chests
- `ItemPickup` - Pickups
- `BraseroFlame` - Brazier flames

---

### 3. SpawnPlacerEngine

**Location:** `Assets/Scripts/Core/SpawnPlacerEngine.cs`

**Purpose:** Procedural item placement in maze

```csharp
// Component (attached to MazeGenerator GameObject)
var spawner = GetComponent<SpawnPlacerEngine>();

// Key Methods:
spawner.PlaceAllItems()
spawner.PlaceDoors()
spawner.PlaceChests()
spawner.GetStatistics()

// Configuration:
// - Door density and types
// - Chest density and loot tables
// - Pickup density and prefabs
// - Trap density and types
```

**Dependencies:**
- `MazeGenerator` (same GameObject)
- `ItemEngine.Instance` (singleton)

---

### 4. MazeGenerator

**Location:** `Assets/Scripts/Core/MazeGenerator.cs`

**Purpose:** DFS maze generation

```csharp
// Component (attached to maze GameObject)
var maze = GetComponent<MazeGenerator>();

// Key Methods:
maze.Generate()

// Properties:
maze.Grid          // Wall[,] data
maze.Width         // Maze width
maze.Height        // Maze height
maze.CurrentSeed   // Generation seed
maze.StartCell     // Entry point
maze.ExitCell      // Exit point
```

**Plugins:**
- `MazeRenderer` - Visual geometry (separate component)
- `SpawnPlacerEngine` - Item placement (same GameObject)

---

### 5. HUDEngine

**Location:** `Assets/Scripts/HUD/HUDEngine.cs`

**Purpose:** Central HUD management

```csharp
// Singleton
HUDEngine.Instance

// Key Methods:
HUDEngine.Instance.RegisterModule(module)
HUDEngine.Instance.EnableAllModules()
HUDEngine.Instance.DisableAllModules()

// Events:
HUDEngine.Instance.OnHUDInitialized += () => { };
HUDEngine.Instance.OnModuleRegistered += (module) => { };
```

**Plugins (via HUDModule base class):**
- `BarsModule` - Health/Mana/Stamina bars
- `StatusEffectsModule` - Buff/debuff icons
- `HotbarModule` - Item hotbar
- `PopupModule` - Damage numbers, notifications

---

## 🔧 Supporting Systems

### StatsEngine (Non-MonoBehaviour)

**Location:** `Assets/Scripts/Status/StatsEngine.cs`

**Purpose:** Pure stat calculation (no Unity dependencies)

```csharp
// Not a MonoBehaviour - instantiated in PlayerStats
var statsEngine = new StatsEngine();

// Key Methods:
statsEngine.SetBaseStats(...)
statsEngine.ApplyEffect(effectData)
statsEngine.TakeDamage(damageInfo)
statsEngine.UseStamina(amount)

// Events:
statsEngine.OnHealthChanged += (current, max) => { };
statsEngine.OnEffectAdded += (effect) => { };
```

**Wrapper:** `PlayerStats` (MonoBehaviour) provides Unity integration

---

### UIBarsSystem

**Location:** `Assets/Scripts/HUD/UIBarsSystem.cs`

**Purpose:** Responsive health/mana/stamina bars

```csharp
// Singleton
UIBarsSystem.Instance

// Auto-subscribes to:
// - PlayerStats events
// - PlayerHealth events (legacy)

// Key Methods:
UIBarsSystem.Instance.SetHealth(current, max)
UIBarsSystem.Instance.SetMana(current, max)
UIBarsSystem.Instance.SetStamina(current, max)
```

**Dependencies:**
- `PlayerStats.Instance` (preferred)
- `PlayerHealth` (legacy fallback)

---

## 📁 Folder Structure

```
Assets/Scripts/
├── Core/                          # CORE ENGINES
│   ├── GameManager.cs             # Top-level coordinator
│   ├── ItemEngine.cs              # Item registry
│   ├── SpawnPlacerEngine.cs       # Procedural placement
│   ├── MazeGenerator.cs           # Maze generation
│   ├── ItemTypes.cs               # ItemType enum
│   ├── LootTable.cs               # Loot configuration
│   ├── ItemData.cs                # ScriptableObject items
│   ├── DoubleDoor.cs              # ⚠️ Should be in Ressources
│   ├── ParticleGenerator.cs       # Particle effects
│   ├── DrawingManager.cs          # Drawing management
│   ├── FlameAnimator.cs           # Flame animation
│   └── BraseroFlame.cs            # ⚠️ Should be in Ressources
│
├── Core/Base/                     # BASE CLASSES
│   └── ItemBehavior.cs            # Base for all items
│
├── Ressources/                    # VISUAL/POOLING
│   ├── MazeRenderer.cs            # Maze visual geometry
│   ├── DrawingPool.cs             # Object pool (drawings)
│   ├── TorchPool.cs               # Object pool (torches)
│   ├── TorchController.cs         # Torch behavior
│   ├── Door.cs                    # Door component
│   ├── ChestBehavior.cs           # ⚠️ Should be in Core
│   └── AnimatedFlame.cs           # Flame animation
│
├── Player/                        # PLAYER COMPONENTS
│   ├── PlayerController.cs        # Input, movement
│   ├── PlayerStats.cs             # Stats (StatsEngine wrapper)
│   ├── PlayerHealth.cs            # Health (legacy)
│   └── PersistentPlayerData.cs    # Save data
│
├── HUD/                           # HUD SYSTEM
│   ├── HUDEngine.cs               # HUD coordinator
│   ├── HUDModule.cs               # Base class for modules
│   ├── UIBarsSystem.cs            # Health/Mana/Stamina bars
│   ├── UIBarsSystemStandalone.cs  # Independent bars
│   ├── HUDSystem.cs               # Legacy coordinator
│   └── DebugHUD.cs                # Debug overlay
│
├── Status/                        # STATS SYSTEM
│   ├── StatsEngine.cs             # Pure stat calculation
│   ├── StatusEffectData.cs        # Effect data
│   ├── StatusEffect.cs            # Legacy wrapper
│   ├── StatModifier.cs            # Modifier system
│   └── DamageType.cs              # Damage types
│
├── Inventory/                     # INVENTORY
│   ├── Inventory.cs               # Inventory manager
│   ├── InventorySlot.cs           # Slot data
│   ├── InventoryUI.cs             # UI display
│   ├── InventorySlotUI.cs         # Slot UI
│   ├── ItemPickup.cs              # Pickup component
│   └── ItemData.cs                # ScriptableObject
│
├── Interaction/                   # INTERACTION
│   ├── IInteractable.cs           # Interface
│   └── InteractableObject.cs      # Base component
│
├── Ennemies/                      # ENEMIES
│   └── Ennemi.cs                  # Enemy AI
│
├── Gameplay/                      # GAMEPLAY
│   └── Collectible.cs             # Collectible items
│
├── Tests/                         # UNIT TESTS
│   ├── StatsEngineTests.cs        # Stats tests
│   └── MazeGeneratorTests.cs      # Maze tests
│
└── Editor/                        # EDITOR TOOLS
    ├── BuildScript.cs             # Build automation
    └── ClearShaderCache.cs        # Shader cache utility
```

---

## 🔗 Dependency Graph

```
GameManager (No dependencies)
    │
    ├─► ItemEngine (depends on: nothing)
    │       └─► ItemBehavior (auto-register)
    │           ├─► DoubleDoor
    │           ├─► ChestBehavior
    │           └─► ItemPickup
    │
    ├─► MazeGenerator (depends on: nothing)
    │       ├─► MazeRenderer (depends on: MazeGenerator, DrawingPool)
    │       └─► SpawnPlacerEngine (depends on: MazeGenerator, ItemEngine)
    │
    ├─► HUDEngine (depends on: nothing)
    │       └─► HUDModule (auto-register)
    │           ├─► BarsModule
    │           └─► StatusEffectsModule
    │
    └─► Player (depends on: GameManager, StatsEngine)
            ├─► PlayerController (depends on: InputSystem, Inventory)
            ├─► PlayerStats (depends on: StatsEngine)
            └─► PlayerHealth (legacy, depends on: nothing)

StatsEngine (Pure C#, no Unity)
    └─► Wrapped by PlayerStats for Unity integration

UIBarsSystem (depends on: PlayerStats OR PlayerHealth)
```

---

## 🎯 Key Design Principles

### 1. **Singleton Pattern for Engines**
All core engines use Singleton pattern for global access:
```csharp
public static T Instance { get; private set; }
```

### 2. **Auto-Registration in Awake()**
Plug-in components auto-register in their base class:
```csharp
protected virtual void Awake()
{
    Engine.Instance?.Register(this);
}
```

### 3. **Event-Driven Communication**
Components communicate via events, not direct references:
```csharp
// Publisher
OnItemCollected?.Invoke(item);

// Subscriber
ItemEngine.Instance.OnItemCollected += HandleItemCollected;
```

### 4. **Loose Coupling**
Engines don't know about specific implementations:
```csharp
// Engine knows about base class only
private List<ItemBehavior> _registeredItems;

// Not specific types like ChestBehavior, DoubleDoor, etc.
```

### 5. **DontDestroyOnLoad for Persistence**
All engines persist between scenes:
```csharp
DontDestroyOnLoad(gameObject);
```

---

## 📝 How to Add New Components

### Step 1: Identify the Engine

What does your component do?
- **Item?** → Inherit from `ItemBehavior`
- **HUD Module?** → Inherit from `HUDModule`
- **Maze-related?** → Add to `MazeGenerator` GameObject

### Step 2: Create the Component

```csharp
// Example: New trap type
public class SpikeTrap : ItemBehavior
{
    [Header("Trap Settings")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private float triggerDelay = 0.5f;
    
    protected override void Awake()
    {
        base.Awake(); // Auto-registers with ItemEngine
        itemType = ItemType.Trap;
    }
    
    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);
        Trigger();
    }
    
    private void Trigger()
    {
        // Deal damage
        var stats = interactor.GetComponent<PlayerStats>();
        stats?.TakeDamage(new DamageInfo(damage, DamageType.Physical));
    }
}
```

### Step 3: Add to GameObject in Unity

1. Create empty GameObject (or use existing)
2. Add your component (e.g., `SpikeTrap`)
3. Configure in Inspector
4. **Done!** Component auto-registers with engine

---

## 🔄 Lifecycle Flow

```
Game Start
    │
    ▼
GameManager.Awake() → Singleton initialized
    │
    ▼
ItemEngine.Awake() → Singleton initialized
    │
    ▼
MazeGenerator.Generate() → Creates maze data
    │
    ▼
SpawnPlacerEngine.PlaceAllItems()
    ├─► Creates doors → Auto-register with ItemEngine
    ├─► Creates chests → Auto-register with ItemEngine
    └─► Creates torches → Auto-register with ItemEngine
    │
    ▼
Player enters scene
    │
    ▼
PlayerController.Awake() → Gets references
    ├─► Inventory = GetComponent<Inventory>()
    └─► PlayerStats = GetComponent<PlayerStats>()
    │
    ▼
PlayerStats.Awake()
    ├─► StatsEngine = new StatsEngine()
    └─► Spawns UIBarsSystem
    │
    ▼
Game Loop
    ├─► PlayerController.Update() → Movement, interaction
    ├─► ItemEngine monitors items
    ├─► HUDEngine updates UI
    └─► GameManager tracks state
    │
    ▼
Player interacts with item
    │
    ▼
PlayerController.Interact()
    │
    ▼
ItemEngine.InteractWithNearest()
    │
    ▼
ItemBehavior.Interact() → Custom behavior
    │
    ▼
Events fire → UI updates, score changes, etc.
```

---

## ✅ Benefits of This Architecture

| Benefit | Description |
|---------|-------------|
| **Modularity** | Add/remove components without breaking others |
| **Testability** | Each engine can be tested independently |
| **Scalability** | Easy to add new item types, HUD modules, etc. |
| **Maintainability** | Clear separation of concerns |
| **Reusability** | Engines can be reused in other projects |
| **Debugging** | Central points for logging and debugging |

---

## 🐛 Common Pitfalls

### 1. **Circular Dependencies**
```csharp
// ❌ BAD: Engine A references Engine B, Engine B references Engine A
// ✅ GOOD: Use events or a third-party mediator
```

### 2. **Forgetting Base.Awake()**
```csharp
// ❌ BAD: Component doesn't call base.Awake()
protected override void Awake()
{
    // Missing: base.Awake();
    // Result: Never registered with engine!
}

// ✅ GOOD:
protected override void Awake()
{
    base.Awake(); // Auto-register
    // Custom initialization
}
```

### 3. **Direct References Instead of Events**
```csharp
// ❌ BAD: Direct coupling
public class Chest : ItemBehavior
{
    void Open()
    {
        UIManager.Instance.ShowChestUI(this); // Tight coupling!
    }
}

// ✅ GOOD: Event-driven
public class Chest : ItemBehavior
{
    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);
        OnChestOpened?.Invoke(this); // Loose coupling!
    }
}
```

---

## 📚 Related Documentation

- [`TODO.md`](TODO.md) - Current tasks and priorities
- [`GIT_LAVOSTRIAL.md`](../GIT_LAVOSTRIAL.md) - Git workflow
- [`backup.md`](../backup.md) - Backup system

---

**Remember:** Always run `backup.ps1` after file changes!
