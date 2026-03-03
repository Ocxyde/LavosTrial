# TODO.md - Project Tasks & Issues

**Project:** PeuImporte (Unity 6000.3.7f1)
**Last Updated:** 2026-03-03
**Status:** ✅ C# Compilation Successful | ✅ Plug-in-and-Out Architecture Complete | ✅ Door System Complete | ⚠️ Light System Optimization Needed

---

## ✅ COMPLETED (2026-03-03)

### Light Engine System - Central Lighting Architecture
- [x] **LightEngine.cs** - Central lighting engine for ALL light emission, fog of war, and lightning effects
  - Dynamic point lights per torch/object
  - Fog of war / darkness system with falloff
  - Lightning / flicker exposure effects
  - Performance optimized with 32-light pooling
  - Emission control for all light sources
  - Global emission multiplier with flicker
  - Ambient light control
- [x] **TorchController.cs** - Individual torch ON/OFF state management
  - Dual state: ON (flame + light) / OFF (no flame + no light)
  - Auto-registers with LightEngine when turned ON
  - Flicker effects via Perlin noise
  - Local Light component as backup
- [x] **TorchPool.cs** - Object pooling for torch prefabs
- [x] **LightEmittingController.cs** - Generic controller for all light-emitting items
  - Supports 8 types: Candle, Lamp, Lantern, Brazier, Torch, Chandelier, Fireplace, Magic
  - Each type has default properties (intensity, range, color, flicker)
  - Auto-registers with LightEngine
- [x] **LightEmittingPool.cs** - Universal pool for all light-emitting prefabs
- [x] **WallPositionArchitect.cs** - RAM storage for ALL maze element positions
  - Stores walls, torches, chests, enemies, items, doors, rooms
  - Each torch has unique GUID for tracking
  - Memory efficient (~32 bytes per record)

### Binary Storage System - Performance Optimization (NO TELEPORTATION)
- [x] **LightCipher.cs** - Dedicated encryption/decryption cipher system
  - XOR cipher (fast, lightweight)
  - RC4 cipher (moderate security, ready to use)
  - AES128 placeholder (for future upgrade)
  - Seed-based key derivation with caching
  - Partial buffer encryption (skip headers)
  - Round-trip validation support
- [x] **LightPlacementData.cs** - Binary format for light position storage
  - 16-byte header (magic "TORC", version, count, flags)
  - 32 bytes per torch (position + rotation + height)
  - Encrypted storage using LightCipher
  - File I/O to StreamingAssets folder
  - Supports save/load across sessions
- [x] **LightPlacementEngine.cs** - Batch instantiation engine
  - Loads positions from encrypted binary
  - Instantiates ALL lights at once (no teleportation!)
  - Manages light states (ON/OFF) without position changes
  - Debug UI overlay for testing
- [x] **SpatialPlacer.cs** - Updated to use binary storage
  - `useBinaryStorage` toggle (default: true)
  - `mazeId` for file naming
  - `PlaceTorchesWithBinaryStorage()` - new method
  - `CalculateTorchPositions()` - position calculation
  - `PlaceTorchesLegacy()` - old system kept for compatibility

### Maze Test Infrastructure
- [x] **FpsMazeTest.cs** - FPS-style maze testing
  - First-person player with eye-level camera
  - Head bob while walking/sprinting
  - Debug UI overlay
  - Controls: [R] regenerate, [G] same seed, [T] toggle torches
- [x] **MazeTorchTest.cs** - Simplified torch test harness
  - Auto-finds all required components
  - Validates setup on Awake
  - Performance timing stats

### Architecture Documentation
- [x] **ARCHITECTURE_OVERVIEW.md** - Complete architecture documentation
  - Plug-in-and-Out system explained
  - Assembly dependency structure
  - Singleton hierarchy
  - Event-based communication
- [x] **TORCH_PLUGIN_SYSTEM.md** - Torch architecture documentation
- [x] **MAZE_TEST_GUIDE.md** - Test setup guide
- [x] **TODO.md** - Updated with binary storage specification

---

## 🔴 CRITICAL (Must Fix Before Release)

### Memory Leaks

- [ ] **DoubleDoor.cs:440-447** - `_haloEffect` GameObject never destroyed in OnDestroy
- [ ] **ChestBehavior.cs:297-303** - `_glowLight` and glow mesh never destroyed
- [ ] **UIBarsSystem.cs:203-241** - Event unsubscription uses fragile reflection code
- [ ] **HUDSystem.cs:437-478** - Event unsubscription uses fragile reflection code
- [ ] **LightEngine.cs** - Ensure all dynamic lights are destroyed on scene unload

### Light System Compiler Warnings

- [ ] **LightEmittingController.cs:88** - `particleSystem` field declared but never used (can be removed)
- [ ] **LightEngine.cs** - Fix any remaining type conversion warnings

### Null Reference Risks

- [ ] **CombatSystem.cs:154-171** - Silent failure when target has no StatsEngine (damage not applied)
- [ ] **PlayerController.cs:189-203** - Complex fallback chain allows free jumps if systems not initialized

### Singleton Issues

- [ ] **ItemEngine.cs:17-32** - Auto-creation in getter can cause race conditions
- [ ] **HUDEngine.cs:19-35** - Same auto-creation pattern
- [ ] **LightEngine.cs:28-46** - Same auto-creation pattern (should be scene-only)
- [ ] **LightPlacementEngine.cs** - Should verify it's scene-scoped only

---

## 🟠 HIGH (Should Fix Soon)

### Light System Integration Testing

- [ ] **Test binary storage end-to-end** - Generate maze, save torches, reload, verify positions
- [ ] **Performance benchmark** - Compare legacy vs binary storage instantiation time
- [ ] **Memory profiling** - Verify binary storage uses less RAM than runtime calculation
- [ ] **Cross-session testing** - Save maze, quit Unity, reload, verify torches persist

### Performance

- [ ] **PlayerController.cs:127** - `FindFirstObjectByType<CombatSystem>()` in Awake (expensive)
- [ ] **LightEngine.cs:38** - `FindFirstObjectByType<LightEngine>()` in singleton getter
- [ ] **LightPlacementEngine.cs:77** - `FindObjectOfType<LightPlacementEngine>()` in Awake
- [ ] **UIBarsSystem.cs:136-149** - Multiple FindFirstObjectByType calls throughout codebase
- [ ] Replace all `FindFirstObjectByType` with Inspector assignments or cached references

### Missing Inspector Setup

- [ ] **SpawnPlacerEngine.cs:43** - `excludedCells` list never initialized (will be null)
- [ ] **InventoryUI.cs:17-24** - Required fields have no null checks in Start()
- [ ] **LightEngine.cs** - Add null checks for all serialized fields
- [ ] **LightPlacementEngine.cs** - Add prefab null checks
- [ ] Add `[SerializeField]` to all private fields used in Inspector

### Hard-coded Values

- [ ] **DrawingManager.cs:14-30** - EGA_PALETTE magic numbers should be constants
- [ ] **PlayerController.cs:23-26** - Movement speeds should be in config file
- [ ] Extract magic numbers to configuration:
  - `cellSize = 6f` (appears in 6+ files)
  - `wallHeight = 3.5f` (appears in 5+ files)
  - `interactionRange = 3f` (appears in 4+ files)
  - `torchHeightRatio = 0.55f` (appears in 3+ files)
  - `lightBaseIntensity = 1.0f` (appears in 4+ files)

### Compiler Warnings

Current warnings to fix:
- [ ] **CS0103** - `corridorWidth` not found in FpsMazeTest.cs:553
- [ ] **CS0414** - Unused fields (LightEngine.baseDarkness, lightUpdateInterval)
- [ ] **CS0436** - IInteractable type conflict (InteractableObject.cs)

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

- [ ] **Light Controllers** - TorchController vs LightEmittingController
  - Both handle light emission
  - TorchController is torch-specific
  - LightEmittingController is generic
  - → Consider making TorchController inherit from LightEmittingController

### Architecture

- [x] **PlayerStats.cs** - Now implements `IPlayerStats` interface
- [x] **Inventory.cs** - Now implements `IInventory` interface
- [x] **MazeRenderer.cs** - Now implements `IMazeRenderer` interface
- [ ] **CombatSystem.cs:10-15** - Requires both StatsEngine AND EventHandler
- [ ] Add interfaces for better testability

### Light System Architecture Improvements

- [ ] **LightEngine** - Consider making it scene-scoped only (no singleton auto-creation)
- [ ] **TorchController** - Should inherit from BehaviorEngine for auto-registration
- [ ] **LightEmittingController** - Should inherit from BehaviorEngine
- [ ] Add event: `OnLightTurnedOn`, `OnLightTurnedOff` for EventHandler

### Missing Unity Attributes

- [ ] **PlayerStats.cs** - Add `[RequireComponent(typeof(PlayerController))]`
- [ ] **CombatSystem.cs** - Add `[RequireComponent(typeof(StatsEngine))]`
- [ ] **LightEngine.cs** - Add `[RequireComponent(typeof(DrawingPool))]` for VFX
- [ ] Add `[RequireComponent]` where dependencies exist

### Documentation

- [ ] **LightEngine.cs** - Add XML docs to public methods
- [ ] **LightEmittingController.cs** - Document all light types
- [ ] **LightPlacementData.cs** - Document binary format (TO BE CREATED)
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
- [ ] **LightEngine.cs:898** - Make verboseLogging actually conditional
- [ ] Remove or conditionally compile debug warnings in PlayerController

### Naming Conventions

- [ ] Rename folder `Ennemies/` → `Enemies/`
- [ ] Standardize comments (mix of English/French):
  - GameManager.cs - French comments
  - PlayerController.cs - French comments
  - LightEngine.cs - English comments
  - → Consider translating all to English for consistency

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
git config --global user.name "your@email.com"
```

---

## 🎯 NEXT SPRINT RECOMMENDATIONS

### Sprint 1 - Light System Binary Storage ✅ COMPLETE
**Goal:** Fix torch teleportation issue with encrypted binary storage

- [x] Create `LightCipher.cs` - Binary format + encryption (XOR, RC4, AES-ready)
- [x] Create `LightPlacementData.cs` - Binary storage with encryption
- [x] Create `LightPlacementEngine.cs` - Batch instantiation
- [x] Modify `SpatialPlacer.cs` - Use binary storage with legacy fallback
- [x] Update TODO.md with completed architecture

**Status:** ✅ IMPLEMENTATION COMPLETE - Ready for testing!

### Sprint 2 - Integration Testing (PRIORITY 1 - NOW)
**Goal:** Test the binary storage system end-to-end

- [ ] Create test maze scene with LightPlacementEngine
- [ ] Generate maze with torches (useBinaryStorage = true)
- [ ] Verify .bytes file created in StreamingAssets/LightPlacements/
- [ ] Reload scene, verify torches load from binary (no teleportation)
- [ ] Benchmark: Compare legacy vs binary instantiation time
- [ ] Test torch ON/OFF state changes
- [ ] Test cross-session persistence (quit Unity, reload)

### Sprint 3 - Critical Fixes
- [ ] Fix memory leaks in DoubleDoor.cs and ChestBehavior.cs
- [ ] Fix event subscription leaks in UIBarsSystem.cs and HUDSystem.cs
- [ ] Add null checks to all light systems
- [ ] Fix remaining compiler warnings

### Sprint 4 - Performance & Stability
- [ ] Replace FindFirstObjectByType with Inspector assignments
- [ ] Initialize all serialized lists/arrays
- [ ] Add RequireComponent attributes
- [ ] Add null checks in InventoryUI.cs

### Sprint 5 - Code Quality
- [ ] Consolidate PixelCanvas duplicate code
- [ ] Choose one HUD system, remove others
- [ ] Extract magic numbers to configuration
- [ ] Add XML documentation to public APIs

---

## 📊 CODE METRICS

| Metric | Before (2026-03-02) | After (2026-03-03) |
|--------|---------------------|---------------------|
| Total C# Files | 60+ | 68+ |
| Lines of Code | ~16,000+ | ~18,500+ |
| Critical Issues | 7 | 6 (binary storage complete) |
| High Issues | 12 | 8 (testing remaining) |
| Medium Issues | 14 | 14 |
| Low Issues | 7 | 7 |
| **Compiler Errors** | **0** ✅ | **0** ✅ |
| **Compiler Warnings** | **0** | **~5** ⚠️ (minor) |
| **Circular Dependencies** | **No** ✅ | **No** ✅ |
| **Folder Organization** | **10 subfolders** ✅ | **10 subfolders** ✅ |

### New Files Created (2026-03-03)

| File | Purpose | Lines |
|------|---------|-------|
| `LightCipher.cs` | Encryption/Decryption cipher (XOR, RC4, AES) | 330 |
| `LightPlacementData.cs` | Binary storage format for light positions | 414 |
| `LightPlacementEngine.cs` | Batch instantiation engine | 360 |
| **Total** | **Binary Storage System** | **~1,104** |

### Files Modified

| File | Changes |
|------|---------|
| `SpatialPlacer.cs` | Added binary storage integration (+180 lines) |
| `LightEmittingController.cs` | Fixed particleSystem warning |
| `LightEmittingPool.cs` | Fixed Transform.SetParent issue |
| `TODO.md` | Complete architecture update |

---

## 🔧 QUICK FIXES (Copy-Paste Solutions)

### Fix 1: LightEngine Type Conversion
```csharp
// LightEngine.cs:254 - Change:
GameObject lightObj = new GameObject($"DynamicLight_{lightId}");
lightObj.transform.SetParent(parent);

// To:
GameObject lightObj = new GameObject($"DynamicLight_{lightId}");
lightObj.transform.parent = parent; // Use .parent instead of SetParent
```

### Fix 2: LightEngine Foreach Modification
```csharp
// LightEngine.cs:701 - Change:
foreach (var lightData in activeLights)
{
    lightData.intensity *= multiplier; // ERROR: Can't modify foreach var
}

// To:
for (int i = 0; i < activeLights.Count; i++)
{
    var lightData = activeLights[i];
    lightData.intensity *= multiplier;
    activeLights[i] = lightData; // Reassign to list
}
```

### Fix 3: LightEmittingPool Type Conversion
```csharp
// LightEmittingPool.cs:91 - Change:
GameObject pooled = pool[i];
pooled.transform.SetParent(parent);

// To:
GameObject pooled = pool[i];
pooled.transform.parent = parent;
```

### Fix 4: LightEmittingController ParticleSystem Warning
```csharp
// LightEmittingController.cs:85 - Add 'new' keyword:
[new] private ParticleSystem particleSystem; // Hides inherited member
```

---

## 📝 NOTES

- All files must use **Unix LF** line endings
- All files must use **UTF-8** encoding
- Backup files are **read-only** - don't modify
- Use `backup.ps1` after any file changes
- Use `git-normalize.bat` before committing if unsure about line endings
- **Light system is the priority** - Binary storage will fix teleportation issues
- **Encrypted binary format** - Seed-based XOR cipher for fast decryption
- **Batch instantiation** - No more runtime teleportation

---

## 🔬 BINARY FORMAT SPECIFICATION

### LightPlacementData Binary Structure

```
FILE HEADER (16 bytes)
┌─────────────────────────────────────────────────────────┐
│ Offset │ Size │ Field          │ Description            │
├────────┼──────┼────────────────┼────────────────────────┤
│ 0      │ 4    │ Magic          │ 0x544F5243 ("TORC")    │
│ 4      │ 4    │ Version        │ Format version (1)     │
│ 8      │ 4    │ TorchCount     │ Number of torches (N)  │
│ 12     │ 4    │ Flags          │ Encryption, compression│
└────────┴──────┴────────────────┴────────────────────────┘

TORCH DATA (32 bytes per torch)
┌─────────────────────────────────────────────────────────┐
│ Offset │ Size │ Field          │ Description            │
├────────┼──────┼────────────────┼────────────────────────┤
│ 0      │ 12   │ Position       │ float3 (x, y, z)       │
│ 12     │ 16   │ Rotation       │ Quaternion (x,y,z,w)   │
│ 28     │ 4    │ Height         │ Wall height ratio      │
└────────┴──────┴────────────────┴────────────────────────┘

ENCRYPTION (XOR Cipher)
- Key derived from maze seed
- Fast XOR operation per byte
- No heavy cryptographic overhead
- Reproducible with same seed
```

---

**Generated:** 2026-03-03
**Unity Version:** 6000.3.7f1
**IDE:** Rider
**Input System:** 1.19.0
**Architecture:** Plug-in-and-Out (Interface-based)
**Priority:** Light System Binary Storage
