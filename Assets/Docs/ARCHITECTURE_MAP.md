# ARCHITECTURE_MAP.md

**Project:** PeuImporte  
**Date:** 2026-03-03  
**Version:** 1.0

---

## 🏗️ **ARCHITECTURE OVERVIEW**

### **Central Hub Pattern:**

```
┌──────────────────────────────────────────────────────────────┐
│                    EVENTHANDLER (Central)                    │
│  - 40+ Events (Player, Combat, Item, Door, Chest, Maze)     │
│  - ALL systems publish HERE                                  │
│  - ALL systems subscribe FROM HERE                           │
│  - Single point of truth                                     │
└──────────────────────────────────────────────────────────────┘
         ▲                    ▲                    ▲
         │                    │                    │
    ┌────┴────┐         ┌────┴────┐         ┌────┴────┐
    │ PUBLISH │         │ PUBLISH │         │ PUBLISH │
    │   TO    │         │   TO    │         │   TO    │
    └────┬────┘         └────┬────┘         └────┬────┘
         │                   │                   │
    ┌────┴───────────────────┼───────────────────┴────┐
    │                        │                        │
┌───▼────┐            ┌──────▼──────┐         ┌──────▼──────┐
│Player  │            │  Combat     │         │  Interaction│
│Stats   │            │  System     │         │  System     │
└────────┘            └─────────────┘         └─────────────┘

    ┌────────────────┐              ┌────────────────┐
    │   SUBSCRIBE    │              │   SUBSCRIBE    │
    │     FROM       │              │     FROM       │
    └────┬───────────┘              └────┬───────────┘
         │                               │
    ┌────▼────┐                    ┌─────▼─────┐
    │ SFXVFX  │                    │  Spatial  │
    │ Engine  │                    │  Placer   │
    └─────────┘                    └───────────┘
```

---

## 📁 **FOLDER STRUCTURE**

```
Assets/Scripts/Core/
├── 01_CoreSystems/          ← CENTRAL HUB
│   ├── EventHandler.cs      ← Event central hub
│   ├── GameManager.cs       ← Game state
│   ├── CoreInterfaces.cs    ← Interface definitions
│   └── EventHandlerInitializer.cs
│
├── 02_Player/               ← Player systems
│   ├── PlayerController.cs
│   ├── PlayerStats.cs
│   └── CameraFollow.cs
│
├── 03_Interaction/          ← Interaction system
│   └── InteractionSystem.cs
│
├── 04_Inventory/            ← Inventory & items
│   ├── ItemEngine.cs
│   ├── Inventory.cs
│   ├── InventorySlot.cs
│   ├── ItemData.cs
│   └── ItemTypes.cs
│
├── 05_Combat/               ← Combat system
│   ├── CombatSystem.cs
│   └── Ennemi.cs
│
├── 06_Maze/                 ← Maze generation
│   ├── MazeGenerator.cs
│   ├── MazeRenderer.cs
│   ├── MazeIntegration.cs
│   ├── RoomGenerator.cs
│   └── MazeSetupHelper.cs
│
├── 07_Doors/                ← Door system
│   ├── DoorsEngine.cs
│   ├── DoorAnimation.cs
│   ├── DoorHolePlacer.cs
│   ├── RoomDoorPlacer.cs
│   └── DoorSFXManager.cs
│
├── 08_Environment/          ← Environment objects
│   ├── SpatialPlacer.cs
│   ├── ChestBehavior.cs
│   ├── SpecialRoom.cs
│   └── TrapBehavior.cs
│
├── 09_Art/                  ← Art generation
│   ├── ArtFactory.cs
│   └── FloorMaterialFactory.cs
│
├── 10_Resources/            ← Resources & lighting
│   ├── SeedManager.cs
│   ├── TorchController.cs
│   ├── TorchPool.cs
│   └── LightPlacementEngine.cs
│
├── 12_Animation/            ← Animation systems
│   ├── DoorAnimator.cs
│   ├── FlameAnimator.cs
│   └── BraseroFlame.cs
│
├── 12_Compute/              ← Computation systems
│   ├── ProceduralCompute.cs ← Central procedural gen
│   ├── LightEngine.cs
│   ├── DrawingPool.cs
│   ├── ParticleGenerator.cs
│   └── SFXVFXEngine.cs
│
└── Base/                    ← Base classes
    └── BehaviorEngine.cs    ← Item base class
```

---

## 🔌 **PLUG-IN-AND-OUT FLOW**

### **Correct Flow:**

```
System A needs data/action
    ↓
Publishes event via EventHandler
    ↓
EventHandler invokes OnEventName
    ↓
All subscribers receive event
    ↓
System B responds to event
    ↓
System B publishes response event
    ↓
EventHandler invokes OnResponseEvent
    ↓
System A receives response
```

### **Example - Player Takes Damage:**

```
CombatSystem.DealDamage()
    ↓
CombatSystem → EventHandler.InvokeDamageTaken()
    ↓
EventHandler → OnDamageTaken?.Invoke()
    ↓
Subscribers receive:
  - PlayerStats: Update health
  - SFXVFXEngine: Play damage effect
  - HUD: Update health bar
    ↓
PlayerStats → EventHandler.InvokePlayerHealthChanged()
    ↓
EventHandler → OnPlayerHealthChanged?.Invoke()
    ↓
Subscribers receive:
  - HUD: Update display
  - GameManager: Check death condition
```

---

## 📊 **EVENT CATEGORIES**

### **Player Events (12)**
- OnPlayerHealthChanged
- OnPlayerDamaged
- OnPlayerHealed
- OnPlayerDied
- OnPlayerRespawned
- OnPlayerManaChanged
- OnPlayerStaminaChanged
- OnPlayerManaUsed/Restored
- OnPlayerStaminaUsed/Restored
- OnStatChanged
- OnLevelChanged

### **Combat Events (4)**
- OnDamageDealt
- OnDamageTaken
- OnKill
- OnDeath

### **Item Events (5)**
- OnItemPickedUp
- OnItemUsed
- OnItemDropped
- OnItemStacked
- OnItemSpawned

### **Door Events (4)**
- OnDoorOpened
- OnDoorClosed
- OnDoorLocked
- OnDoorTrapTriggered

### **Chest Events (4)**
- OnChestOpened
- OnChestClosed
- OnChestLootGenerated
- OnChestItemSpawned

### **Maze Events (2)**
- OnMazeLevelChanged
- OnMazeGenerated (MISSING - TODO)

### **Material Events (3)**
- OnMaterialRequested
- OnTextureRequested
- OnMaterialGenerated (MISSING - TODO)

### **UI Events (5)**
- OnUIBarsInitialized
- OnScoreChanged
- OnQuestUpdated
- OnQuestCompleted
- OnAchievementUnlocked

---

## 🎯 **SYSTEM DEPENDENCIES**

### **Zero Dependencies (Base Layer):**
- `EventHandler.cs`
- `CoreInterfaces.cs`
- `GameManager.cs`

### **Low Dependencies (Core Layer):**
- `PlayerStats.cs` → StatsEngine (pure C#)
- `CombatSystem.cs` → StatsEngine
- `BehaviorEngine.cs` → ItemEngine

### **Medium Dependencies (Feature Layer):**
- `PlayerController.cs` → PlayerStats, CombatSystem
- `InteractionSystem.cs` → Inventory, CombatSystem
- `MazeRenderer.cs` → EventHandler, DrawingPool

### **High Dependencies (Integration Layer):**
- `MazeIntegration.cs` → All maze systems
- `SpatialPlacer.cs` → All placement systems
- `SFXVFXEngine.cs` → All event systems

---

## ⚠️ **KNOWN ARCHITECTURE VIOLATIONS**

### **Critical (Must Fix):**

1. **EventHandler → ProceduralCompute Direct Call**
   - File: `EventHandler.cs:339, 350`
   - Fix: Use event invocation instead

2. **PlayerController → PlayerStats Direct Access**
   - File: `PlayerController.cs:267, 279, 286`
   - Fix: Use stamina events

3. **MazeGenerator → SeedManager Direct Access**
   - File: `MazeGenerator.cs:121-127`
   - Fix: Use seed events

### **High (Should Fix):**

4. **Missing Event Unsubscriptions**
   - File: `PlayerStats.cs`
   - Fix: Add OnDestroy cleanup

5. **Static Event Memory Leaks**
   - File: `PlayerStats.cs:63-65`
   - Fix: Convert to instance events

6. **GetComponent in Loops**
   - Files: Multiple
   - Fix: Cache references

---

## 🔄 **INITIALIZATION ORDER**

```
1. SeedManager (DontDestroyOnLoad)
2. EventHandler (DontDestroyOnLoad)
3. GameManager (DontDestroyOnLoad)
4. PlayerStats (Scene-specific)
5. CombatSystem (Scene-specific)
6. InteractionSystem (Scene-specific)
7. All other systems (Scene-specific)
```

---

## 📝 **RECOMMENDATIONS**

### **Short Term:**
1. Fix EventHandler direct calls
2. Add missing event handlers
3. Add null checks to all subscriptions

### **Medium Term:**
4. Fix static event leaks
5. Cache GetComponent calls
6. Add initialization manager

### **Long Term:**
7. Implement weak event pattern
8. Create architecture tests
9. Document all event flows

---

**Architecture Status:** ⚠️ **Needs Critical Fixes**  
**Compliance:** 70% Plug-in-and-Out  
**Target:** 100% Event-Driven
