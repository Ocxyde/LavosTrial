﻿# ARCHITECTURE_MAP.md

**Project:** CodeDotLavos  
**Unity Version:** 6000.3.10f1  
**Architecture:** Plug-in-Out  
**License:** GPL-3.0  
**Last Updated:** 2026-03-10  
**Author:** Ocxyde

---

## 📚 DOCUMENTATION HUB

**Start Here:** [PROJECT_DEEP_SCAN_SUMMARY_20260310.md](PROJECT_DEEP_SCAN_SUMMARY_20260310.md) - 📊 Complete project overview

### Quick Links
| Document | Purpose |
|----------|---------|
| [TODO.md](TODO.md) | 📋 Tasks & priorities with progress bars |
| [PROJECT_DEEP_SCAN_SUMMARY_20260310.md](PROJECT_DEEP_SCAN_SUMMARY_20260310.md) | 📊 Complete project overview |
| [GIT_COMMIT_HISTORY_202603.md](GIT_COMMIT_HISTORY_202603.md) | 📝 All 94 commits tracked |
| [ARCHITECTURE_OVERVIEW.md](ARCHITECTURE_OVERVIEW.md) | 🏗️ System architecture |
| [PROJECT_STANDARDS.md](PROJECT_STANDARDS.md) | 📋 Coding standards |
| [README.md](README.md) | 📖 Modder's guide |

---

## 🏗️ ARCHITECTURE OVERVIEW

### Central Hub Pattern

```
┌─────────────────────────────────────────────────────────────────┐
│                    EVENTHANDLER (Central Hub)                    │
│  - 40+ Events (Player, Combat, Item, Door, Chest, Maze, UI)    │
│  - ALL systems publish HERE                                     │
│  - ALL systems subscribe FROM HERE                              │
│  - Service Locator pattern (lazy initialization)                │
└─────────────────────────────────────────────────────────────────┘
         ▲                    ▲                    ▲
         │                    │                    │
    ┌────┴────┐         ┌────┴────┐         ┌────┴────┐
    │ PUBLISH │         │ PUBLISH │         │ PUBLISH │
    └────┬────┘         └────┬────┘         └────┬────┘
         │                   │                   │
    ┌────┴───────────────────┼───────────────────┴────┐
    │                        │                        │
┌───▼────┐            ┌──────▼──────┐         ┌──────▼──────┐
│Player  │            │  Combat     │         │  Interaction│
│Stats   │            │  System     │         │  System     │
└────────┘            └─────────────┘         └─────────────┘
```

### Plug-in-Out Principle

```csharp
// ✅ CORRECT - Plug-in-Out
private void FindComponents()
{
    spatialPlacer = FindFirstObjectByType<SpatialPlacer>();
    lightPlacementEngine = FindFirstObjectByType<LightPlacementEngine>();
}

// ❌ WRONG - Creates dependency
private void CreateComponents()
{
    spatialPlacer = gameObject.AddComponent<SpatialPlacer>();  // DON'T!
}
```

---

## 📁 FOLDER STRUCTURE (Updated 2026-03-10)

```
Assets/
├── Scripts/
│   └── Core/
│       ├── 01_CoreSystems/          ← CENTRAL HUB
│       │   ├── EventHandler.cs      ← Event central hub (40+ events)
│       │   ├── GameManager.cs       ← Game state management
│       │   └── CoreInterfaces.cs    ← Interface definitions
│       │
│       ├── 02_Player/               ← Player systems
│       │   ├── PlayerController.cs  ← FPS movement (WASD + mouse)
│       │   ├── PlayerStats.cs       ← Health, Mana, Stamina, 8 stats
│       │   └── CameraFollow.cs      ← Camera follow logic
│       │
│       ├── 03_Interaction/          ← Interaction system
│       │   ├── InteractionSystem.cs ← E-key interaction
│       │   └── SafeController.cs    ← Safe interaction
│       │
│       ├── 04_Inventory/            ← Inventory & items
│       │   ├── ItemEngine.cs        ← Item management
│       │   ├── Inventory.cs         ← Inventory container
│       │   ├── InventorySlot.cs     ← Slot definition
│       │   └── ItemData.cs          ← Item data structure
│       │
│       ├── 05_Combat/               ← Combat system
│       │   ├── CombatSystem.cs      ← Damage calculation
│       │   └── Ennemi.cs            ← Enemy AI
│       │
│       ├── 06_Maze/                 ← MAZE SYSTEM (33 files)
│       │   ├── CompleteMazeBuilder.cs        ← MAIN ORCHESTRATOR
│       │   ├── CompleteCorridorMazeBuilder.cs ← Corridor variant
│       │   ├── BaseMazeBuilder.cs            ← Base class (extracted)
│       │   │
│       │   ├── GridMazeGenerator.cs          ← Cardinal-only DFS + A*
│       │   ├── DungeonMazeGenerator.cs       ← 8-direction DFS + rooms
│       │   ├── GuaranteedPathMazeGenerator.cs ← Minotaur maze
│       │   ├── PassageFirstMazeGenerator.cs  ← Passage-first approach
│       │   │
│       │   ├── DeadEndCorridorSystem.cs      ← Mathematical dead-ends
│       │   ├── CorridorFillSystem.cs         ← Space filling
│       │   ├── CorridorFlowSystem.cs         ← Three-tier hierarchy
│       │   ├── DifficultyScaler.cs           ← Power curve scaling
│       │   │
│       │   ├── MazeData8.cs                  ← 2 bytes/cell (16-bit flags)
│       │   ├── DungeonMazeData.cs            ← Advanced maze data
│       │   ├── MazeBinaryStorage8.cs         ← Binary .lvm format
│       │   │
│       │   ├── MazeWallSpawner.cs            ← Wall instantiation
│       │   ├── MazeDoorSpawner.cs            ← Door instantiation
│       │   ├── MazeObjectSpawner.cs          ← Chests, enemies, torches
│       │   ├── MazeMarkerSpawner.cs          ← Visual markers (rings)
│       │   │
│       │   ├── GameConfig.cs                 ← Central config (JSON)
│       │   ├── DungeonMazeConfig.cs          ← Dungeon-specific config
│       │   ├── DeadEndCorridorConfig.cs      ← Dead-end config
│       │   │
│       │   ├── TestDeadEndConfig.cs          ← Config testing
│       │   ├── AutoMazeSetup.cs              ← Auto-setup tool
│       │   └── _Legacy/                      ← Deprecated files
│       │       ├── MazeCorridorGenerator.cs
│       │       └── MazeMathEngine_8Axis.cs
│       │
│       ├── 07_Doors/                ← Door system
│       │   ├── DoorsEngine.cs       ← Door logic
│       │   ├── DoorAnimation.cs     ← Door animation
│       │   ├── DoorHolePlacer.cs    ← Door hole carving
│       │   ├── RoomDoorPlacer.cs    ← Room door placement
│       │   └── DoorSFXManager.cs    ← Door sound effects
│       │
│       ├── 08_Environment/          ← Environment objects
│       │   ├── SpatialPlacer.cs     ← Object placement orchestrator
│       │   ├── ChestPlacer.cs       ← Chest placement
│       │   ├── EnemyPlacer.cs       ← Enemy placement
│       │   ├── ItemPlacer.cs        ← Item placement
│       │   ├── TorchPlacer.cs       ← Torch placement
│       │   ├── ChestBehavior.cs     ← Chest interaction
│       │   ├── TrapBehavior.cs      ← Trap behavior
│       │   └── SpecialRoom.cs       ← Special room logic
│       │
│       ├── 09_Art/                  ← Art generation
│       │   ├── ArtFactory.cs        ← Art asset factory
│       │   └── FloorMaterialFactory.cs ← Floor material factory
│       │
│       ├── 10_Resources/            ← Resources & lighting
│       │   ├── SeedManager.cs       ← Seed computation
│       │   ├── TorchController.cs   ← Torch animation
│       │   ├── TorchPool.cs         ← Torch object pool
│       │   └── LightPlacementEngine.cs ← Light placement
│       │
│       ├── 11_Utilities/            ← Utility systems
│       │   └── ShareSystem.cs       ← Maze sharing system
│       │
│       ├── 12_Animation/            ← Animation systems
│       │   ├── DoorAnimator.cs      ← Door animation
│       │   ├── FlameAnimator.cs     ← Flame animation
│       │   └── BraseroFlame.cs      ← Brasero flame
│       │
│       ├── 13_Compute/              ← Computation systems
│       │   ├── LightEngine.cs       ← Dynamic lighting (927 lines)
│       │   ├── DrawingPool.cs       ← Drawing compute pool
│       │   ├── ParticleGenerator.cs ← Particle effects
│       │   └── SFXVFXEngine.cs      ← SFX & VFX management
│       │
│       ├── 14_Geometry/             ← Geometry math library
│       │   ├── Tetrahedron.cs       ← Tetrahedron primitive
│       │   ├── TetrahedronMath.cs   ← Tetrahedron calculations
│       │   ├── Triangle.cs          ← Triangle primitive
│       │   └── Vector3d.cs          ← Double-precision vector
│       │
│       └── Base/                    ← Base classes
│           └── BehaviorEngine.cs    ← Item/interaction base class
│
├── Scripts/Editor/                ← Editor tools
│   ├── MazeBuilderEditor.cs       ← Custom editor
│   ├── SetupMazeComponents.cs     ← Component setup tool
│   ├── MazePreviewEditor.cs       ← Preview editor
│   └── GenerateLevel10Maze.cs     ← Level generation tool
│
├── Scripts/Tests/                 ← Unit tests (58 passing)
│   ├── Code.Lavos.Tests.asmdef
│   ├── MazeGeometryTests.cs       ← 18 tests
│   ├── GeometryMathTests.cs       ← 16 tests
│   └── MazeBinaryStorageTests.cs  ← 15 tests
│
├── Scripts/HUD/                   ← HUD system
│   ├── UIBarsSystem.cs            ← Health/mana/stamina bars
│   ├── DialogEngine.cs            ← Dialog system
│   ├── PopWinEngine.cs            ← Popup windows
│   └── HUDSystem.cs               ← HUD management
│
├── Scripts/Player/                ← Player data
│   └── PlayerData.cs              ← Player save data
│
├── Scripts/Interaction/           ← Interaction interface
│   └── InteractableObject.cs      ← Interactable base class
│
├── Scripts/Inventory/             ← Inventory UI
│   └── InventoryUI.cs             ← Inventory UI display
│
├── Scripts/Status/                ← Stats/effects
│   └── StatusEffects.cs           ← Status effect system
│
├── Scripts/Ennemies/              ← Enemy system
│   └── EnemyAI.cs                 ← Enemy AI logic
│
├── Scripts/Gameplay/              ← Collectibles
│   └── Collectible.cs             ← Collectible items
│
└── Scripts/DBSQLite/              ← Database system
    └── DatabaseManager.cs         ← SQLite management

├── Resources/                     ← Unity resources
│   └── Prefabs/
│       ├── WallPrefab.prefab
│       ├── DoorPrefab.prefab
│       ├── TorchHandlePrefab.prefab
│       ├── ChestPrefab.prefab
│       ├── EnemyPrefab.prefab
│       └── PlayerPrefab.prefab
│
├── Settings/                      ← Unity settings
│   ├── PC_RPAsset.asset           ← URP render asset
│   ├── PC_Renderer.asset          ← URP renderer
│   └── UniversalRenderPipelineGlobalSettings.asset
│
├── Scenes/                        ← Unity scenes
│   ├── MazeLav8s_v1-0_1_4.unity   ← Main maze scene
│   ├── BoundaryTest.unity         ← Boundary test scene
│   └── MainScene_Maze.unity       ← Main scene
│
├── Docs/                          ← Documentation (158 .md files)
│   ├── PROJECT_DEEP_SCAN_SUMMARY_20260310.md  ← Complete overview
│   ├── GIT_COMMIT_HISTORY_202603.md           ← All 94 commits
│   ├── TODO.md                                ← Tasks & priorities
│   ├── ARCHITECTURE_OVERVIEW.md               ← Architecture docs
│   ├── ARCHITECTURE_MAP.md                    ← This file
│   ├── CRITICAL_FIXES_APPLIED_20260310.md     ← Latest fixes
│   └── ... (152 more files)
│
└── Config/                        ← JSON configuration
    └── GameConfig-default.json    ← Central config file
```

---

## 🔌 PLUG-IN-AND-OUT FLOW

### Correct Flow

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

### Example - Player Takes Damage

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
  - UIBarsSystem: Update health bar
    ↓
PlayerStats → EventHandler.InvokePlayerHealthChanged()
    ↓
EventHandler → OnPlayerHealthChanged?.Invoke()
    ↓
Subscribers receive:
  - UIBarsSystem: Update display
  - GameManager: Check death condition
```

---

## 📊 EVENT CATEGORIES

### Player Events (12)
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

### Combat Events (4)
- OnDamageDealt
- OnDamageTaken
- OnKill
- OnDeath

### Item Events (5)
- OnItemPickedUp
- OnItemUsed
- OnItemDropped
- OnItemCreated
- OnItemDestroyed

### Door Events (4)
- OnDoorOpened
- OnDoorClosed
- OnDoorLocked
- OnDoorUnlocked

### Chest Events (4)
- OnChestOpened
- OnChestClosed
- OnChestLootGenerated
- OnChestLootClaimed

### Maze Events (2)
- OnMazeLevelChanged
- OnMazeGenerated

### UI Events (5)
- OnUIBarsInitialized
- OnScoreChanged
- OnDialogStarted
- OnDialogEnded
- OnPopupShown

---

## 🎯 MAZE GENERATION PIPELINE

### Phase 1: Logical Generation (GridMazeGenerator)

```
1. FillAllWalls()           → All cells = 0x000F (all walls)
2. CarvePassagesCardinal()  → 4-direction DFS (2-step carving)
3. CarveSpawnRoom()         → 5×5 cleared at (1,1)
4. SetExit()                → Exit marker at (W-2, H-2)
5. EnsurePathCardinal()     → A* guarantees path (with iteration limit)
6. AddDeadEndCorridors()    → Mathematical dead-end generation (30%→75%)
7. AddCorridorFlowSystem()  → Space filling with corridors
8. PlaceTorches()           → Set torch flags (30% wall-adjacent)
9. PlaceObjects()           → Set chest/enemy flags

Output: MazeData8 structure with cell flags
```

### Phase 2: Physical Instantiation (CompleteMazeBuilder)

```
10. LoadConfig()            → Load GameConfig from JSON
11. ValidateAssets()        → Resolve prefab references
12. DestroyMazeObjects()    → Clean up previous maze
13. SpawnGround()           → Instantiate floor plane
14. SpawnAllWalls()         → Instantiate wall prefabs
15. SpawnDoors()            → Place doors on access walls
16. SpawnTorches()          → Instantiate torch prefabs
17. SpawnObjects()          → Instantiate chests + enemies
18. SpawnRoomMarkers()      → Floating rings, lights, particles
19. SpawnPlayer()           → Spawn at spawn point (with validation)
20. SaveMaze()              → Binary .lvm save to Runtimes/Mazes/

Output: Fully instantiated 3D maze with prefabs
```

---

## 📈 SYSTEM COMPLETION STATUS

```
┌─────────────────────────────────────────────────────────────────┐
│  CORE ENGINE SYSTEMS         [████████████] 100% ✅ Complete    │
│  PLAYER SYSTEMS              [████████████] 100% ✅ Complete    │
│  INTERACTION & INVENTORY     [████████░░░░]  80% 🟡 In Progress │
│  COMBAT SYSTEMS              [███████░░░░░]  70% 🟡 In Progress │
│  MAZE GENERATION             [████████████] 100% ✅ Complete    │
│  ENVIRONMENT & LIGHTING      [████████████] 100% ✅ Complete    │
│  UI & HUD                    [█████████░░░]  75% 🟡 In Progress │
│  AUDIO SYSTEM                [███████░░░░░]  75% 🟡 In Progress │
│  UTILITIES & TOOLS           [████████████] 100% ✅ Complete    │
│                                                                 │
│  OVERALL GAME PROGRESS       [████████████░░]  92% 🟢 Excellent │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔗 DOCUMENTATION LINKS

### Architecture & Design
- [PROJECT_DEEP_SCAN_SUMMARY_20260310.md](PROJECT_DEEP_SCAN_SUMMARY_20260310.md) - Complete overview
- [ARCHITECTURE_OVERVIEW.md](ARCHITECTURE_OVERVIEW.md) - System architecture
- [PROJECT_STANDARDS.md](PROJECT_STANDARDS.md) - Coding standards

### Maze System
- [MAZE_CARDINAL_UPDATE_2026-03-09.md](MAZE_CARDINAL_UPDATE_2026-03-09.md) - Cardinal-only update
- [CRITICAL_FIXES_APPLIED_20260310.md](CRITICAL_FIXES_APPLIED_20260310.md) - Priority 1 fixes
- [DEAD_END_CORRIDOR_SYSTEM.md](DEAD_END_CORRIDOR_SYSTEM.md) - Dead-end system docs

### Git & Backup
- [GIT_COMMIT_HISTORY_202603.md](GIT_COMMIT_HISTORY_202603.md) - All 94 commits tracked
- [GIT_WORKFLOW_GUIDE.md](GIT_WORKFLOW_GUIDE.md) - Git procedures
- [backup.md](../../backup.md) - Backup guide

---

**Generated:** 2026-03-10  
**Author:** Ocxyde  
**License:** GPL-3.0  
**Encoding:** UTF-8 Unix LF

---

*End of Architecture Map*
