# TODO.md - Project Tasks & Issues

**Project:** PeuImporte (Unity 6000.3.7f1)
**Last Updated:** 2026-03-02
**Status:** ✅ C# Compilation Successful | ✅ Plug-in-and-Out Architecture Complete | ✅ Door System Complete | ⚠️ Shader Warnings (Non-Critical)

---

## ✅ COMPLETED (2026-03-02)

### Door System Complete - 6 Swing Modes with Advanced Features
- [x] **TestDoubleDoor.cs** - Complete double door test tool with 6 modes:
  - Mode 1: Both OUTSWING (hinges on outer borders)
  - Mode 2: Both INSWING (hinges on outer borders)
  - Mode 3: Left IN / Right OUT (asymmetric swing)
  - Mode 4: Left OUT / Right IN (asymmetric swing)
  - **Mode 5: CENTER POST** (vertical mullion/idle row between doors)
  - **Mode 6: AUTO-CLOSE ON IMPACT** (opens on collision, auto-closes after 3s)
- [x] **TestSingleDoor.cs** - Single door inswing/outswing test
- [x] **Door fitting math** - Proper clearances:
  - Top/Bottom: 0.1f gap
  - Depth: 0.05f gap each side (door sits inside wall)
  - Pivot magnetization to hole cutting edges
- [x] **Center post system** - Vertical mullion that stays fixed while doors swing
- [x] **Auto-close timer** - Impact-triggered opening with delayed closing
- [x] **Door panel mesh generation** - Procedural door meshes with proper dimensions
- [x] **DoorAnimator component** - Smooth door animation system
- [x] **Documentation** - SingleDoor_Math.md, DoubleDoor_Math.md

### Architecture Overhaul - Plug-in-and-Out System
- [x] Created interface-based architecture to eliminate circular dependencies
- [x] **CoreInterfaces.cs** - Defined core interfaces:
  - `IPlayerStats` - Player stats interface (implemented by PlayerStats)
  - `IInventory` - Inventory interface (implemented by Inventory)
  - `IMazeRenderer` - Maze renderer interface (implemented by MazeRenderer)
  - `IInteractable` - Interaction interface (implemented by interactable objects)
  - `IPlayerController` - Player controller interface
- [x] All Core scripts now use interfaces instead of concrete types
- [x] Fixed all circular dependency errors

### Core Folder Restructure
- [x] Organized 82 files into 10 logical subfolders:
  - `01_CoreSystems/` - GameManager, EventHandler, CoreInterfaces
  - `02_Player/` - PlayerController, PlayerStats
  - `03_Interaction/` - InteractionSystem
  - `04_Inventory/` - Inventory, InventorySlot, ItemData, ItemTypes, ItemEngine
  - `05_Combat/` - CombatSystem, Ennemi
  - `06_Maze/` - MazeGenerator, MazeIntegration, MazeRenderer, MazeSetupHelper, RoomGenerator
  - `07_Doors/` - DoorsEngine, DoorAnimation, DoorHolePlacer, RoomDoorPlacer, DoorSystemSetup, RealisticDoorFactory, DoorSFXManager, PixelArtDoorTextures
  - `08_Environment/` - ChestBehavior, TrapBehavior, TrapType, SpawnPlacerEngine
  - `09_Visual/` - ParticleGenerator, SFXVFXEngine, FlameAnimator, BraseroFlame, DrawingManager, DrawingPool
  - `10_Resources/` - TorchController, TorchPool, LootTable, SeedManager

### Assembly Definition Fixes
- [x] Fixed `Code.Lavos.Core.asmdef` - Added Input System reference
- [x] Fixed `Code.Lavos.HUD.asmdef` - Added Input System and TextMeshPro references
- [x] Fixed `Code.Lavos.Inventory.asmdef` - Added Input System and TextMeshPro references
- [x] All assemblies now compile without errors

### Script Fixes
- [x] Replaced all `PlayerHealth` references with `PlayerStats`
- [x] Fixed `IInteractable` interface method calls (`GetInteractionPrompt()` instead of property)
- [x] Fixed interface method signatures to use `GameObject` instead of `PlayerController`
- [x] Moved `Inventory.cs` to Core assembly
- [x] Moved `Ennemi.cs` to Core assembly
- [x] Fixed `AddOrGetComponent` generic calls in MazeSetupHelper.cs
- [x] Fixed array vs List confusion (`allSlots.Length` instead of `.Count`)
- [x] Updated DebugHUD.cs to use PlayerStats directly
- [x] Updated UIBarsSystem.cs to use PlayerStats fallback

### Package Updates
- [x] Updated Input System from v1.18.0 to v1.19.0
- [x] Verified TextMeshPro package installed

### Build Status
- [x] **C# Compilation: SUCCESS (0 errors)**
- [x] **In-Game Testing: Working**
- [ ] Shader warnings remain (non-critical, in Unity package cache)

---

## 🔴 CRITICAL (Must Fix Before Release)

### Memory Leaks

- [ ] **DoubleDoor.cs:440-447** - `_haloEffect` GameObject never destroyed in OnDestroy
- [ ] **ChestBehavior.cs:297-303** - `_glowLight` and glow mesh GameObjects never destroyed
- [ ] **UIBarsSystem.cs:203-241** - Event unsubscription uses fragile reflection code
- [ ] **HUDSystem.cs:437-478** - Event unsubscription uses fragile reflection code

### Null Reference Risks

- [ ] **CombatSystem.cs:154-171** - Silent failure when target has no StatsEngine (damage not applied)
- [ ] **PlayerController.cs:189-203** - Complex fallback chain allows free jumps if systems not initialized

### Singleton Issues

- [ ] **ItemEngine.cs:17-32** - Auto-creation in getter can cause race conditions
- [ ] **HUDEngine.cs:19-35** - Same auto-creation pattern

---

## 🟠 HIGH (Should Fix Soon)

### Performance

- [ ] **PlayerController.cs:127** - `FindFirstObjectByType<CombatSystem>()` in Awake (expensive)
- [ ] **UIBarsSystem.cs:136-149** - Multiple FindFirstObjectByType calls throughout codebase
- [ ] Replace all `FindFirstObjectByType` with Inspector assignments or cached references

### Missing Inspector Setup

- [ ] **SpawnPlacerEngine.cs:43** - `excludedCells` list never initialized (will be null)
- [ ] **InventoryUI.cs:17-24** - Required fields have no null checks in Start()
- [ ] Add `[SerializeField]` to all private fields used in Inspector

### Hard-coded Values

- [ ] **DrawingManager.cs:14-30** - EGA_PALETTE magic numbers should be constants
- [ ] **PlayerController.cs:23-26** - Movement speeds should be in config file
- [ ] Extract magic numbers to configuration:
  - `cellSize = 6f` (updated from 4f, appears in 6+ files)
  - `wallHeight = 3.5f` (updated from 3f, appears in 5+ files)
  - `interactionRange = 3f` (appears in 4+ files)

### Compiler Warnings

- [x] **CS0234** - Fixed: Input System namespace errors (added asmdef references)
- [x] **CS0246** - Fixed: TextMeshPro errors (added Unity.TextMeshPro reference)
- [x] **CS0266** - Fixed: MonoBehaviour to PlayerStats conversion (changed type)
- [x] **CS0101** - Fixed: Duplicate TrapType enum definitions
- [x] **CS1513** - Fixed: Missing closing brace in PlayerController.cs

---

## 🟡 MEDIUM (Consider Fixing)

### Code Duplication

- [ ] **PixelCanvas classes** - Consolidate duplicate implementations:
  - `DoorPixelCanvas` in DoubleDoor.cs (Lines 553-593)
  - `ChestPixelCanvas` in ChestBehavior.cs (Lines 311-339)
  - `PixelCanvas` in DrawingManager.cs (Lines 235-267)
  - → Create single reusable `PixelCanvasHelper.cs`

- [ ] **HUD Systems** - Three competing implementations:
  - `UIBarsSystem` - Standalone bars
  - `HUDSystem` - Legacy HUD
  - `HUDEngine` + `HUDModule` - Modular system
  - → Choose one, remove others, update `UIBarsSystemInitializer.cs` [Obsolete]

### Architecture

- [x] **PlayerStats.cs** - Now implements `IPlayerStats` interface
- [x] **Inventory.cs** - Now implements `IInventory` interface
- [x] **MazeRenderer.cs** - Now implements `IMazeRenderer` interface
- [ ] **CombatSystem.cs:10-15** - Requires both StatsEngine AND EventHandler
- [ ] Add interfaces for better testability

### Missing Unity Attributes

- [ ] **PlayerStats.cs** - Add `[RequireComponent(typeof(PlayerController))]`
- [ ] **CombatSystem.cs** - Add `[RequireComponent(typeof(StatsEngine))]`
- [ ] Add `[RequireComponent]` where dependencies exist

### Documentation

- [ ] **Inventory.cs** - Add XML docs to public methods
- [ ] **InventorySlot.cs** - Add class documentation
- [ ] **StatModifier.cs** - Document undocumented methods
- [ ] Add `<summary>` to all public APIs

---

## 🟢 LOW (Nice to Have)

### Cleanup

- [ ] **CombatSystem.cs** - Remove or implement unused `OnCombatStatChanged` event
- [ ] **StatsEngine.cs:71** - Remove or implement unused `OnDamageTaken` event
- [ ] Remove reserved fields or implement features:
  - `invincibilityTime` (i-frames system)
  - `staminaDodgeCost` (dodge roll system)

### Debug Code

- [ ] **CombatSystem.cs:175,195,209** - Make Debug.Log conditional on debug flag
- [ ] Remove or conditionally compile debug warnings in PlayerController

### Naming Conventions

- [ ] Rename folder `Ennemies/` → `Enemies/`
- [ ] Standardize comments (mix of English/French):
  - GameManager.cs - French comments
  - PlayerController.cs - French comments
  - Consider translating to English for consistency

### Shader Warnings (Non-Critical)

- [ ] **Shader Graph Package** - Precision conversion warnings in shadows.hlsl
  - These are in Unity's package cache, won't affect gameplay
  - Can be fixed by updating Shader Graph package or changing shader precision

---

## 📋 GIT & VERSION CONTROL

### Available Git Scripts

| Script | Command | Description |
|--------|---------|-------------|
| Quick Menu | `.\git-quick.bat` | Interactive menu for all git operations |
| Auto Commit | `.\git-auto.bat "message"` | Stage → normalize LF → backup → commit → push |
| Sync | `.\git-sync.bat "message"` | Pull → restore → commit → push |
| Status | `.\git-status.bat` | Quick status overview |
| Normalize | `.\git-normalize.bat` | Normalize line endings to LF |
| Setup LF | `.\git-setup-lf.bat` | One-time LF setup |

### Common Workflows

#### Daily Commit (After Coding)
```bash
# In Rider, make changes...
# Then in terminal:
.\git-auto.bat "Fixed stamina regen bug"
```

#### Start of Day (Sync with Remote)
```bash
# Get latest and merge your changes:
.\git-sync.bat "Merged latest changes"
```

#### Quick Status Check
```bash
# See what changed:
.\git-status.bat

# Or native git:
git status
```

### Git Configuration

**Check Git User:**
```bash
git config user.name
git config user.email
```

**Set Git User (if needed):**
```bash
git config --global user.name "Your Name"
git config --global user.email "your@email.com"
```

### Git Aliases (Optional)

Add to `.gitconfig` for shorter commands:
```bash
git config --global alias.st status
git config --global alias.co checkout
git config --global alias.br branch
git config --global alias.ci commit
git config --global alias.last "log -1 HEAD"
git config --global alias.lg "log --oneline --graph --decorate"
```

Then use: `git st`, `git lg`, etc.

### PowerShell Aliases (Optional)

Add to `$PROFILE` for global access:
```powershell
notepad $PROFILE

# Add these lines:
function ga { & "D:\travaux_Unity\PeuImporte\git-auto.bat" $args }
function gs { & "D:\travaux_Unity\PeuImporte\git-status.bat" }
function gn { & "D:\travaux_Unity\PeuImporte\git-normalize.bat" }
function gsync { & "D:\travaux_Unity\PeuImporte\git-sync.bat" $args }

# Restart PowerShell, then use: ga "message" from anywhere
```

---

## 🎯 NEXT SPRINT RECOMMENDATIONS

### Sprint 1 - Door System Visual Overhaul ✅ COMPLETE
- [x] Remesh all doors to look like real 2D pixel art (8-bit colors)
- [x] Make doors fit properly in wall holes
- [x] Ensure doors are interactive/operable
- [x] Replace current "weird visual" with proper pixel art style
- [x] **6 swing modes implemented**
- [x] **Center post/mullion system**
- [x] **Auto-close on impact system**

### Sprint 2 - Critical Fixes (Priority 1 - NOW FOCUS)
1. Fix memory leaks in DoubleDoor.cs and ChestBehavior.cs
2. Fix event subscription leaks in UIBarsSystem.cs and HUDSystem.cs
3. Add null checks in CombatSystem.cs and PlayerController.cs
4. Consolidate singleton initialization pattern

### Sprint 3 - Performance & Stability (Priority 2)
1. Replace FindFirstObjectByType with Inspector assignments
2. Initialize all serialized lists/arrays
3. Add RequireComponent attributes
4. Add null checks in InventoryUI.cs

### Sprint 4 - Code Quality (Priority 3)
1. Consolidate PixelCanvas duplicate code
2. Choose one HUD system, remove others
3. Extract magic numbers to configuration
4. Add XML documentation to public APIs

### Sprint 5 - Cleanup (Priority 4)
1. Remove unused events and reserved fields
2. Make debug logs conditional
3. Standardize naming conventions
4. Rename Ennemies folder

---

## 📊 CODE METRICS

| Metric | Before (2026-03-01) | After (2026-03-02) |
|--------|---------------------|---------------------|
| Total C# Files | 52 | 60+ |
| Lines of Code | ~16,000+ | ~16,000+ |
| Critical Issues | 7 | 7 |
| High Issues | 12 | 12 |
| Medium Issues | 14 | 14 |
| Low Issues | 6 | 7 (shader warnings added) |
| **Compiler Errors** | **31+** | **0** ✅ |
| **Compiler Warnings** | **14** | **0** ✅ |
| **Circular Dependencies** | **Yes** | **No** ✅ |
| **Folder Organization** | **Flat (messy)** | **10 subfolders** ✅ |

---

## 🔧 QUICK FIXES (Copy-Paste Solutions)

### Fix 1: DoubleDoor OnDestroy
```csharp
// DoubleDoor.cs:440-447
private new void OnDestroy()
{
    base.OnDestroy();
    if (_doorLeftMat != null) Destroy(_doorLeftMat);
    if (_doorRightMat != null) Destroy(_doorRightMat);
    if (_frameMat != null) Destroy(_frameMat);
    if (_haloMat != null) Destroy(_haloMat);
    if (_flameMat != null) Destroy(_flameMat);
    if (_haloEffect != null) Destroy(_haloEffect); // ADD THIS
}
```

### Fix 2: ChestBehavior OnDestroy
```csharp
// ChestBehavior.cs:297-303
private new void OnDestroy()
{
    base.OnDestroy();
    if (_chestMat != null) Destroy(_chestMat);
    if (_glowMat != null) Destroy(_glowMat);
    if (_glowLight != null) Destroy(_glowLight.gameObject); // ADD THIS
    if (_glowMesh != null) Destroy(_glowMesh); // ADD THIS
}
```

### Fix 3: CombatSystem Null Check
```csharp
// CombatSystem.cs:154-171
public float DealDamage(GameObject source, GameObject target, DamageInfo damageInfo)
{
    if (target == null || damageInfo.amount <= 0) return 0f;

    StatsEngine targetStats = GetTargetStatsEngine(target);
    float finalDamage = damageInfo.amount;

    if (targetStats != null)
    {
        finalDamage = targetStats.CalculateDamage(damageInfo);
        targetStats.ModifyHealth(-finalDamage); // Apply damage
    }
    else
    {
        // Simple crit calculation for targets without StatsEngine
        if (damageInfo.isCritical || UnityEngine.Random.value < baseCritChance)
        {
            finalDamage *= damageInfo.criticalMultiplier;
        }
        // Apply damage directly to target's health component if available
        var health = target.GetComponent<PlayerHealth>();
        health?.TakeDamage(finalDamage);
    }

    // ... rest of method
}
```

---

## 📝 NOTES

- All files must use **Unix LF** line endings
- All files must use **UTF-8** encoding
- Backup files are **read-only** - don't modify
- Use `backup.ps1` after any file changes
- Use `git-normalize.bat` before committing if unsure about line endings
- **Core folder is now organized** - 10 subfolders for easy navigation
- **Interface-based architecture** - No more circular dependencies!

---

**Generated:** 2026-03-02
**Unity Version:** 6000.3.7f1
**IDE:** Rider
**Input System:** 1.19.0
**Architecture:** Plug-in-and-Out (Interface-based)
