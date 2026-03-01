# LavosTrial - TODO & Project Status

**Last Updated:** Mars 2026  
**Unity Version:** 6000.3.7f1 (Unity 6)  
**IDE:** Rider  
**Input System:** New Input System  
**Backup:** `backup.ps1` (run after any file change)  
**Line Endings:** Unix LF  
**Encoding:** UTF-8

---

## 📋 Quick Links

| Document | Purpose |
|----------|---------|
| [`ARCHITECTURE.md`](ARCHITECTURE.md) | **Plug-in-and-Out system documentation** |
| [`/TODO.md`](../TODO.md) | Legacy code review status |
| [`GIT_LAVOSTRIAL.md`](../GIT_LAVOSTRIAL.md) | Git workflow (Assets/ only) |
| [`backup.md`](../backup.md) | Backup system |
| [`TETRAHEDRON_SYSTEM.md`](../TETRAHEDRON_SYSTEM.md) | Tetrahedron mesh system |
| [`HUD_SETUP_GUIDE.md`](../HUD_SETUP_GUIDE.md) | HUD configuration |

---

## 🎯 Architecture Overview

### Engine-Based Design (Plug-in-and-Out)

All core systems follow the **Engine.cs** pattern:

```
SystemNameEngine.cs
├── Singleton instance (Instance)
├── Registration/Unregistration methods
├── Central configuration
├── Event system
└── Debug visualization (Gizmos)
```

**Active Engines:**

| Engine | Location | Purpose |
|--------|----------|---------|
| `GameManager` | `Assets/Scripts/Core/` | Game state, score, scene management |
| `ItemEngine` | `Assets/Scripts/Core/` | Central item registry (doors, chests, pickups) |
| `SpawnPlacerEngine` | `Assets/Scripts/Core/` | Procedural item placement in maze |
| `MazeGenerator` | `Assets/Scripts/Core/` | DFS maze generation |
| `MazeRenderer` | `Assets/Scripts/Ressources/` | Visual maze geometry |
| `DrawingManager` | `Assets/Scripts/Core/` | Drawing/pool management |
| `DrawingPool` | `Assets/Scripts/Ressources/` | Object pooling for drawings |
| `TorchPool` | `Assets/Scripts/Ressources/` | Object pooling for torches |
| `TorchController` | `Assets/Scripts/Ressources/` | Torch behavior |
| `ParticleGenerator` | `Assets/Scripts/Core/` | Particle effects |
| `HUDEngine` | `Assets/Scripts/HUD/` | HUD system coordinator |
| `UIBarsSystem` | `Assets/Scripts/HUD/` | Health/Mana/Stamina bars |
| `HUDSystem` | `Assets/Scripts/HUD/` | Legacy HUD coordinator |
| `StatsEngine` | `Assets/Scripts/Status/` | Player stats calculation |
| `TetrahedronEngine` | `Temp/TetrahedronAssets/` | Procedural tetrahedron meshes |

---

## ✅ Completed Systems

### Core Gameplay
- [x] **Player Controller** - WASD movement, mouse look, sprint, jump, head bob, interaction
- [x] **Player Stats** - Health, mana, stamina with regeneration and modifiers
- [x] **Player Health** - Legacy health system (wrapped by StatsEngine)
- [x] **Inventory System** - Slots, UI, item pickup, stacking
- [x] **Interaction System** - IInteractable interface, raycast-based interaction
- [x] **Status Effects** - Buffs, debuffs, curses with StatModifier system

### Maze System
- [x] **Maze Generator** - DFS algorithm, seed-based, configurable dimensions
- [x] **Maze Renderer** - Procedural wall geometry, material generation
- [x] **Door System** - DoubleDoor with glow/halo effects, 8-bit flame style
- [x] **Spawn Placer Engine** - Auto-place doors, chests, torches, traps, enemies

### Trap System (Complete)
- [x] **TrapEngine** - Central trap registry, damage multiplier, lifecycle
- [x] **GroundTrap** - Spikes, Pitfall, Electrified/Ice Floor, Pressure Plate
- [x] **WallTrap** - Spike Wall, Moving Wall, Arrow Shooter, Swinging Blade
- [x] **RollTrap** - Boulder, Rolling Log, Blade, Mine
- [x] **TrapVisualFactory** - Procedural trap visuals
- [x] **RenderTetraEngine** - Tetrahedron-based trap meshes

### UI/HUD
- [x] **UIBarsSystem** - Responsive health/mana/stamina bars (screen-edge layout)
- [x] **UIBarsSystemStandalone** - Independent bar system
- [x] **HUDModule** - Modular HUD components (status effects, minimap, etc.)
- [x] **DebugHUD** - FPS, coordinates, debug info

### Database (SQLite)
- [x] **DatabaseManager** - SQLite wrapper
- [x] **DatabaseSaveLoadHelper** - Save/load serialization
- [x] **DatabaseConfig** - Configuration

### Utilities
- [x] **DrawingPool** - Object pooling for drawings
- [x] **TorchPool** - Object pooling for torches
- [x] **AnimatedFlame** - 8-bit flame animation
- [x] **PixelArtTextureFactory** - Procedural pixel art textures
- [x] **LootTable** - Loot drop configuration

### Testing
- [x] **StatsEngineTests** - 25+ unit tests for stats system
- [x] **MazeGeneratorTests** - 15+ unit tests for maze generation

---

## ⏳ High Priority Tasks

### 1. Input System Migration
**Status:** ⚠️ Partial  
**File:** `Assets/Scripts/Player/PlayerController.cs`

**Issue:** PlayerController uses direct keyboard input instead of InputAction assets.

```csharp
// Current (direct input):
_kb.wKey.isPressed
_kb.aKey.isPressed

// Should use InputAction from InputSystem_Actions.inputactions:
_playerInput.actions["Move"]
_playerInput.actions["Look"]
```

**Action Items:**
- [ ] Create PlayerInput component with InputAction references
- [ ] Replace direct keyboard calls with InputAction.ReadValue()
- [ ] Test gamepad support (already configured in .inputactions)
- [ ] Update documentation

**Estimated Time:** 2-3 hours

---

### 2. Trap System Integration
**Status:** ⚠️ Needs Testing  
**Location:** `Assets/Temp/Traps/`

**Action Items:**
- [ ] Create trap prefabs in Unity Editor
- [ ] Test trap spawning in actual maze
- [ ] Verify trap damage applies to player
- [ ] Test trap visual animations
- [ ] Add trap trigger zones
- [ ] Balance trap damage values

**Estimated Time:** 4-6 hours

---

### 3. Documentation Gaps
**Status:** 🟡 In Progress

**Files needing XML documentation:**
- [ ] `PlayerController.cs`
- [ ] `PlayerStats.cs`
- [ ] `Inventory.cs`
- [ ] `MazeGenerator.cs`
- [ ] `MazeRenderer.cs`
- [ ] `DoubleDoor.cs`
- [ ] `ItemEngine.cs`
- [ ] `SpawnPlacerEngine.cs`

**Estimated Time:** 3-4 hours

---

## ⏳ Medium Priority Tasks

### 4. UI/UX Improvements
**Status:** 🔵 Not Started

**Action Items:**
- [ ] Add damage flash effect on health bar
- [ ] Add mana/stamina depletion animations
- [ ] Implement status effect icons in HUD
- [ ] Add minimap module
- [ ] Create pause menu
- [ ] Add settings menu (volume, graphics, controls)

**Estimated Time:** 6-8 hours

---

### 5. Enemy AI
**Status:** 🔵 Not Started  
**File:** `Assets/Scripts/Ennemies/Ennemi.cs`

**Action Items:**
- [ ] Implement enemy state machine (Idle, Chase, Attack, Death)
- [ ] Add enemy navigation (NavMesh or custom)
- [ ] Implement enemy attacks (melee, ranged)
- [ ] Add enemy variety (types, stats)
- [ ] Implement enemy spawning via SpawnPlacerEngine
- [ ] Add enemy death rewards (XP, loot)

**Estimated Time:** 8-12 hours

---

### 6. Save/Load System
**Status:** 🟡 Partial  
**Location:** `Assets/DB_SQLite/`

**Action Items:**
- [ ] Test SQLite integration in Unity 6
- [ ] Implement player data serialization
- [ ] Implement maze state serialization
- [ ] Implement inventory save/load
- [ ] Add save slots (multiple saves)
- [ ] Add auto-save feature

**Estimated Time:** 6-8 hours

---

### 7. Performance Optimization
**Status:** 🔵 Not Started

**Action Items:**
- [ ] Profile maze generation performance
- [ ] Optimize Draw Calls (batching, atlasing)
- [ ] Implement LOD for distant objects
- [ ] Add occlusion culling for maze corridors
- [ ] Optimize particle systems
- [ ] Add object pooling for enemies/projectiles

**Estimated Time:** 4-6 hours

---

## ⏳ Low Priority Tasks

### 8. Audio System
**Status:** 🔵 Not Started

**Action Items:**
- [ ] Create AudioManager singleton
- [ ] Implement background music
- [ ] Add SFX for player actions (steps, jumps, attacks)
- [ ] Add SFX for traps
- [ ] Add SFX for UI interactions
- [ ] Implement audio occlusion (maze walls)

**Estimated Time:** 4-6 hours

---

### 9. Visual Effects
**Status:** 🟡 Partial

**Completed:**
- [x] Torch flames
- [x] Door glow/halo
- [x] ParticleGenerator system

**Action Items:**
- [ ] Add hit impact effects
- [ ] Add death/dissolve effects
- [ ] Add environmental effects (fog, dust)
- [ ] Add screen shake
- [ ] Add post-processing (bloom, color grading)

**Estimated Time:** 4-6 hours

---

### 10. Level Progression
**Status:** 🔵 Not Started

**Action Items:**
- [ ] Implement level transitions
- [ ] Add difficulty scaling
- [ ] Create multiple maze themes
- [ ] Add boss encounters
- [ ] Implement checkpoint system
- [ ] Add collectible tracking (keys, gems)

**Estimated Time:** 6-10 hours

---

### 11. Analytics & Debugging
**Status:** 🔵 Not Started

**Action Items:**
- [ ] Add in-game debug console
- [ ] Implement performance metrics (FPS, memory)
- [ ] Add gameplay analytics (deaths, time, collectibles)
- [ ] Create editor tools for level design
- [ ] Add automated testing in CI/CD

**Estimated Time:** 4-6 hours

---

## 🐛 Known Issues

### Minor Issues

| Issue | Severity | File | Notes |
|-------|----------|------|-------|
| InputSystem not fully integrated | Medium | PlayerController.cs | Uses direct keyboard input |
| Trap prefabs not created | Medium | Assets/Temp/Traps/ | Needs Unity Editor setup |
| Some Debug.Log spam | Low | Multiple | Consider Debug.ILog for release |
| Enemy AI placeholder | Medium | Ennemi.cs | Basic implementation only |

### ✅ Fixed Issues

| Issue | Status | File | Fix |
|-------|--------|------|-----|
| ChestBehavior not found | ✅ Fixed | SpawnPlacerEngine.cs | Moved ChestBehavior.cs to Core folder |
| Shader precision error | ✅ Fixed | Library/PackageCache | Created `ClearShaderCache.cs` editor script |
| Cyclic dependency Core ↔ Ressources | ✅ Fixed | Code.Lavos.Core.asmdef | Moved ChestBehavior.cs to Core folder |
| DoorPixelCanvas not found | ✅ Fixed | DoubleDoor.cs | Moved DoorPixelCanvas class into DoubleDoor.cs |
| PlayerStats reference in Core | ✅ Fixed | ItemData.cs | Used reflection to avoid assembly dependency |
| ItemBehavior renamed to BehaviorEngine | ✅ Fixed | All Core files | Renamed base class and updated all references |
| TorchController syntax error | ✅ Fixed | TorchController.cs | Fixed missing closing brace `}` |
| TorchDiagnostics syntax error | ✅ Fixed | TorchDiagnostics.cs | Fixed missing closing brace `}` |
| Ennemi syntax error | ✅ Fixed | Ennemi.cs | Fixed missing closing brace `}` |
| TMPro/InputSystem references | ✅ Fixed | Inventory/HUD/Player.asmdef | Added Unity.TextMeshPro and Unity.InputSystem |

### Potential Issues (Watch List)

| Issue | Risk | Mitigation |
|-------|------|------------|
| StatsEngine complexity | Medium | Well-tested with unit tests |
| Object pooling edge cases | Low | DrawingPool, TorchPool tested |
| SQLite platform compatibility | Medium | Test on all target platforms |
| Maze generation performance | Low | Profile for large mazes |

---

## 📊 Project Health

| Category | Status | Notes |
|----------|--------|-------|
| **Code Organization** | ✅ Excellent | Clean folder structure, asmdef files |
| **Namespace Usage** | ✅ Consistent | `Code.Lavos.*` pattern |
| **Singleton Pattern** | ✅ Proper | GameManager, ItemEngine, etc. |
| **Object Pooling** | ✅ Implemented | DrawingPool, TorchPool |
| **Unit Testing** | ✅ Started | 40+ tests (Stats, Maze) |
| **Documentation** | 🟡 In Progress | XML docs added to core systems |
| **Git Configuration** | ✅ Complete | Assets/ only workflow |
| **Build System** | ✅ Portable | BuildAll.ps1 with env var support |
| **Backup System** | ✅ Complete | backup.ps1 with MD5 hashing |
| **Engine Architecture** | ✅ Complete | Plug-in-and-out design |

---

## 📁 Folder Structure

```
D:\travaux_Unity\PeuImporte\
├── Assets/
│   ├── Scripts/
│   │   ├── Core/           # GameManager, ItemEngine, MazeGenerator, etc.
│   │   ├── Player/         # PlayerController, PlayerStats, PlayerHealth
│   │   ├── HUD/            # UIBarsSystem, HUDSystem, HUDModule
│   │   ├── Inventory/      # Inventory, InventoryUI, ItemData
│   │   ├── Status/         # StatsEngine, StatusEffectData, DamageType
│   │   ├── Ressources/     # MazeRenderer, DrawingPool, TorchPool
│   │   ├── Interaction/    # IInteractable, InteractableObject
│   │   ├── Ennemies/       # Ennemi
│   │   ├── Gameplay/       # Collectible
│   │   ├── Tests/          # Unit tests
│   │   └── Editor/         # BuildScript, Migration tools
│   ├── Scenes/             # Unity scenes
│   ├── Prefabs/            # Prefab files
│   ├── Input/              # InputSystem_Actions.inputactions
│   └── Settings/           # URP, Input System settings
├── Temp/
│   ├── Traps/              # Trap system (experimental)
│   └── TetrahedronAssets/  # Tetrahedron mesh system
├── Docs/                   # Documentation (this file)
├── Backup_Solution/        # Auto-generated backups (read-only)
├── diff_tmp/               # Temp diff files (auto-cleaned)
└── [Scripts & Tools]       # PowerShell scripts, batch files
```

---

## 🔧 Development Workflow

### Before Coding
1. Pull latest changes: `.\git-lavos-sync.bat "Sync"`
2. Open Unity, wait for compilation
3. Verify no console errors

### While Coding
1. Follow Engine.cs pattern for new systems
2. Use Unix LF line endings (Rider default)
3. Use UTF-8 encoding
4. Add XML documentation for public APIs
5. Write unit tests for core logic

### After Coding
1. **Run backup:** `.\backup.ps1`
2. Test in Unity (no compile errors)
3. Run unit tests (if applicable)
4. Commit changes: `.\git-lavos.bat "message"`
5. Push to remote (when prompted)

### Cleanup
- Delete diff files older than 2 days: `.\cleanup-diff-files.ps1`
- Clear Unity cache: `.\clear_package_cache.bat`
- Fix Unity meta files: `.\fix_unity_meta_and_cache.bat`

---

## 📝 Coding Standards

### Naming Conventions
```csharp
// Classes: PascalCase
public class PlayerController { }

// Methods: PascalCase
public void HandleMovement() { }

// Private fields: _camelCase
private Vector3 _velocity;

// Serialized fields: _camelCase (with [SerializeField])
[SerializeField] private float walkSpeed = 5f;

// Properties: PascalCase
public float CurrentHealth { get; private set; }

// Events: PascalCase with On prefix
public event Action<float, float> OnHealthChanged;

// Interfaces: IPascalCase
public interface IInteractable { }

// Enums: PascalCase
public enum GameState { Playing, Paused, GameOver }
```

### Architecture Principles
1. **Single Responsibility** - Each class does one thing well
2. **Dependency Injection** - Use interfaces, avoid tight coupling
3. **Event-Driven** - Use events for cross-system communication
4. **Object Pooling** - Pool frequently created/destroyed objects
5. **Engine Pattern** - Central registry for complex systems

---

## 🧪 Testing Checklist

### Unit Tests (Automated)
- [x] StatsEngine - Stat calculations
- [x] StatsEngine - Resource management
- [x] StatsEngine - Damage calculations
- [x] StatsEngine - Status effects
- [x] MazeGenerator - Maze generation
- [x] MazeGenerator - Seed consistency
- [x] MazeGenerator - Path existence

### Runtime Tests (Manual)
- [ ] Player movement (WASD + sprint + jump)
- [ ] Mouse look (camera rotation)
- [ ] Interaction system (E key)
- [ ] Inventory (pickup, use, drop)
- [ ] Health/Mana/Stamina bars update
- [ ] Status effects apply correctly
- [ ] Maze generation produces valid mazes
- [ ] TorchPool recycles objects
- [ ] TrapEngine registers/unregisters traps
- [ ] SpawnPlacerEngine spawns objects
- [ ] Trap visuals animate
- [ ] Trap damage applies to player
- [ ] Save/Load works correctly
- [ ] No memory leaks (long sessions)

---

## 📚 References

### Unity Documentation
- [Unity 6 Manual](https://docs.unity3d.com/6000.0/Documentation/Manual/index.html)
- [New Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)
- [URP Documentation](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest)

### Project Documentation
- [`GIT_LAVOSTRIAL.md`](../GIT_LAVOSTRIAL.md) - Git workflow
- [`backup.md`](../backup.md) - Backup system
- [`TETRAHEDRON_SYSTEM.md`](../TETRAHEDRON_SYSTEM.md) - Tetrahedron meshes
- [`HUD_SETUP_GUIDE.md`](../HUD_SETUP_GUIDE.md) - HUD configuration
- [`README.md`](../README.md) - Project overview

---

## 📅 Changelog

### March 2026
- ✅ Created comprehensive TODO.md in Docs/
- ✅ Implemented trap system with Engine architecture
- ✅ Added SpawnPlacerEngine for unified spawning
- ✅ Created TetrahedronEngine for procedural meshes
- ✅ Added 40+ unit tests
- ✅ Configured Git workflow (Assets/ only)
- ✅ Enhanced backup.ps1 with MD5 hashing
- ✅ Added cleanup-diff-files.ps1 (auto-clean 2+ days)
- ✅ Fixed duplicate file issues
- ✅ Added XML documentation to core systems

### Previous
- See [`/TODO.md`](../TODO.md) for legacy code review status

---

## 🎯 Next Steps

1. **Immediate:** Run `.\backup.ps1` after any file changes
2. **This Session:** Review and prioritize tasks above
3. **Next Session:** Tackle high-priority items (Input System, Trap Integration)

---

**Remember:** Always run `backup.ps1` after file changes!
Always use Unix LF line endings and UTF-8 encoding!
