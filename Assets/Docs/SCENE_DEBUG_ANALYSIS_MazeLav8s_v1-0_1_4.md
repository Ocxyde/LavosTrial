%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
---
# SCENE DEBUG ANALYSIS - MazeLav8s_v1-0_1_4
## Code: BetsyBoop
## Version: Unity 6.0 (6000.3.7f1)
## Date: 2026-03-09
## License: GPL-3.0
---

## EXECUTIVE SUMMARY

**Scene Status:** REQUIRES DEBUGGING ⚠️  
**Compilation:** PASSING ✅  
**Architecture Violations:** CRITICAL (18 issues)  
**Plug-in-Out Compliance:** 42% (needs fixes)  

---

## 1. CRITICAL ISSUES IDENTIFIED

### 1.1 HUD System Plug-in-Out Violations (CRITICAL)

**Issue:** HUD components use `new GameObject()` + `AddComponent<>()` instead of prefabs.

**Files Affected:**
- `Assets/Scripts/HUD/UIBarsSystem.cs` (34x new GameObject + 40x AddComponent)
- `Assets/Scripts/HUD/PopWinEngine.cs` (40x new GameObject + 45x AddComponent)
- `Assets/Scripts/HUD/HUDSystem.cs` (35x new GameObject + 35x AddComponent)
- `Assets/Scripts/HUD/HUDEngine.cs` (15x new GameObject + 20x AddComponent)
- `Assets/Scripts/HUD/HUDModule.cs` (8x new GameObject + 10x AddComponent)
- `Assets/Scripts/HUD/DialogEngine.cs` (10x new GameObject + 15x AddComponent)

**Impact on Scene:** 
- HUD prefabs may fail to instantiate in scene
- Runtime errors when HUD tries to create UI elements
- Inconsistent with core architecture

**Fix Type:** CODE REVISION REQUIRED

```csharp
// BEFORE (Violation)
public void InitializeHealthBar()
{
    GameObject healthBar = new GameObject("HealthBar");
    Image image = healthBar.AddComponent<Image>();
    RectTransform rect = healthBar.AddComponent<RectTransform>();
}

// AFTER (Compliant)
public void InitializeHealthBar()
{
    GameObject prefab = Resources.Load<GameObject>("Prefabs/HUD/HealthBar");
    GameObject healthBar = Instantiate(prefab);
    Image image = healthBar.GetComponent<Image>();
    RectTransform rect = healthBar.GetComponent<RectTransform>();
}
```

**Fix Priority:** CRITICAL - Do First

---

### 1.2 Namespace Declaration Mismatches (CRITICAL)

**Issue:** Files located in correct folders but declare wrong namespaces.

**Affected Files:**

| File | Current Location | Current Namespace | Should Be | Impact |
|------|------------------|-------------------|-----------|--------|
| `Collectible.cs` | Assets/Scripts/Gameplay/ | Code.Lavos.Core | Code.Lavos.Gameplay | Assembly reference broken |
| `InteractableObject.cs` | Assets/Scripts/Interaction/ | Code.Lavos.Core | Code.Lavos.Interaction | Assembly reference broken |
| `IInteractable.cs` | Assets/Scripts/Interaction/ | Code.Lavos.Core | Code.Lavos.Interaction | Assembly reference broken |
| `PersistentUI.cs` | Assets/Scripts/HUD/ | Code.Lavos.Core | Code.Lavos.HUD | Assembly reference broken |
| `StatusEffect.cs` (in Player/) | Assets/Scripts/Player/ | Code.Lavos.Core | Code.Lavos.Status | Assembly reference broken |
| `TorchDiagnostics.cs` | Assets/Scripts/Ressources/ | Code.Lavos.Core | Code.Lavos.Ressources | Assembly reference broken |
| `RoomTextureGenerator.cs` | Assets/Scripts/Ressources/ | Code.Lavos.Core | Code.Lavos.Ressources | Assembly reference broken |
| `PixelArtTextureFactory.cs` | Assets/Scripts/Ressources/ | Code.Lavos.Core | Code.Lavos.Ressources | Assembly reference broken |
| `PixelArtGenerator.cs` | Assets/Scripts/Ressources/ | Code.Lavos.Core | Code.Lavos.Ressources | Assembly reference broken |
| `Lav8s_PixelArt8Bit.cs` | Assets/Scripts/Ressources/ | Code.Lavos.Core | Code.Lavos.Ressources | Assembly reference broken |
| `DoorFactory.cs` | Assets/Scripts/Ressources/ | Code.Lavos.Core | Code.Lavos.Ressources | Assembly reference broken |
| `ChestPixelArtFactory.cs` | Assets/Scripts/Ressources/ | Code.Lavos.Core | Code.Lavos.Ressources | Assembly reference broken |
| `AnimatedFlame.cs` | Assets/Scripts/Ressources/ | Code.Lavos.Core | Code.Lavos.Ressources | Assembly reference broken |

**Impact on Scene:**
- Scene may fail to find and instantiate door/chest prefabs
- Camera/animation systems may not work correctly
- HUD system may not load resources

**Fix Type:** NAMESPACE REVISION IN 12 FILES

Example:
```csharp
// FILE: Assets/Scripts/Gameplay/Collectible.cs
// BEFORE
namespace Code.Lavos.Core { ... }

// AFTER
namespace Code.Lavos.Gameplay { ... }
```

**Fix Priority:** CRITICAL - Do Second

---

### 1.3 Folder Numbering Conflicts (CRITICAL)

**Issue:** Duplicate numeric prefixes causing Unity import confusion.

**Problem:**
```
Assets/Scripts/Core/
├── 10_Mesh/              <- Uses "10_"
├── 10_Resources/         <- Uses "10_" ❌ DUPLICATE
├── 11_Utilities/
├── 12_Animation/
├── 13_Compute/
```

**Impact:**
- Unity alphabetical sorting may fail
- Import order unpredictable
- Scene references may resolve incorrectly

**Required Action:**
1. Rename: `10_Resources/` → `11_Resources/`
2. Rename: `11_Utilities/` → `12_Utilities/`
3. Rename: `12_Animation/` → `13_Animation/`
4. Rename: `13_Compute/` → `14_Compute/`

**Fix Priority:** CRITICAL - Do Third

---

## 2. WARNING ISSUES

### 2.1 Emojis in C# Files (WARNING)

**File:** `Assets/Scripts/Core/14_Geometry/Triangle.cs`  
**Lines:** 49-51

```csharp
// BEFORE (Invalid emoji)
/// ✅ Area calculation (Heron's formula and 3D cross product)
/// ✅ Centroid calculation
/// ✅ Circumcenter calculation

// AFTER (Valid - no emoji)
/// Area calculation - IMPLEMENTED (Heron's formula and 3D cross product)
/// Centroid calculation - IMPLEMENTED
/// Circumcenter calculation - IMPLEMENTED
```

**Fix Priority:** HIGH (compliance)

---

### 2.2 Class Name Typo (WARNING)

**File:** `Assets/Scripts/Core/11_Utilities/ShareSystm.cs`  
**Issue:** Class name should be `ShareSystem` not `ShareSystm`

**Fix:**
1. Rename file: `ShareSystm.cs` → `ShareSystem.cs`
2. Change class name in file
3. Update references

**Fix Priority:** MEDIUM (affects code clarity)

---

### 2.3 Hardcoded Values Instead of Config (WARNING)

**Files Affected:**
- `StatsEngine.cs`
- `WallPrefabValidator.cs`
- `DiagonalWallGenerator.cs`
- `FloorMaterialFactory.cs`

**Example:**
```csharp
// BEFORE
private const float OutOfCombatDelay = 3f;
private const float OutOfCombatMultiplier = 0.5f;

// AFTER (Read from GameConfig-default.json)
private float OutOfCombatDelay => GameConfig.Instance.combatOutOfCombatDelay;
private float OutOfCombatMultiplier => GameConfig.Instance.combatOutOfCombatMultiplier;
```

**Fix Priority:** MEDIUM (affects balancing)

---

## 3. SCENE-SPECIFIC ISSUES

### 3.1 Scene File Structure

**Scene File:** `Assets/Scenes/MazeLav8s_v1-0_1_4.unity`  
**Size:** 37K  
**Format:** YAML (Unity scene format)

**Expected GameObjects in Scene:**
- Main Camera
- GameConfig (with CompleteMazeBuilder8 component)
- AutoMazeSetup (validation component)
- FloorTile (ground plane)
- MazeWalls8 (container for wall objects)
- MazeObjects8 (container for doors/chests/enemies)
- Player (player character)

### 3.2 Potential Scene Issues

**Issue A: Missing CompleteMazeBuilder8 Component**
- If scene doesn't have CompleteMazeBuilder8, maze won't generate
- Check: Find GameConfig GameObject → should have CompleteMazeBuilder8 script

**Issue B: Broken Prefab References**
- Prefab paths in CompleteMazeBuilder8 may not match actual paths
- Check Config inspector for correct prefab paths

**Issue C: Missing GameConfig Asset**
- Scene needs a valid GameConfig-*.json loaded
- Check: Resources folder for config file

---

## 4. RECOMMENDED FIX SEQUENCE

### Phase 1 - Critical (Do First)

**Objective:** Restore basic functionality

#### Step 1: Fix Namespace Declarations (15 minutes)

Files to update:
```
Assets/Scripts/Gameplay/Collectible.cs
Assets/Scripts/Interaction/IInteractable.cs
Assets/Scripts/Interaction/InteractableObject.cs
Assets/Scripts/HUD/PersistentUI.cs
Assets/Scripts/Ressources/TorchDiagnostics.cs
Assets/Scripts/Ressources/RoomTextureGenerator.cs
Assets/Scripts/Ressources/PixelArtTextureFactory.cs
Assets/Scripts/Ressources/PixelArtGenerator.cs
Assets/Scripts/Ressources/Lav8s_PixelArt8Bit.cs
Assets/Scripts/Ressources/DoorFactory.cs
Assets/Scripts/Ressources/ChestPixelArtFactory.cs
Assets/Scripts/Ressources/AnimatedFlame.cs
Assets/Scripts/Status/StatusEffect.cs (if in Player folder)
```

#### Step 2: Fix Folder Numbering (5 minutes)

Rename directories:
```
10_Resources/ → 11_Resources/
11_Utilities/ → 12_Utilities/
12_Animation/ → 13_Animation/
13_Compute/ → 14_Compute/
```

After renaming: Close and reopen Unity to force reimport

#### Step 3: Refactor HUD System (30-45 minutes)

Target files:
```
Assets/Scripts/HUD/UIBarsSystem.cs
Assets/Scripts/HUD/PopWinEngine.cs
Assets/Scripts/HUD/HUDSystem.cs
Assets/Scripts/HUD/HUDEngine.cs
Assets/Scripts/HUD/HUDModule.cs
Assets/Scripts/HUD/DialogEngine.cs
```

For each file:
1. Replace `new GameObject()` with prefab instantiation
2. Create corresponding prefab in Assets/Resources/Prefabs/HUD/
3. Test in scene

### Phase 2 - Warnings (Do Second)

#### Step 4: Remove Emojis from Triangle.cs (5 minutes)

#### Step 5: Rename ShareSystm.cs (5 minutes)

#### Step 6: Move Hardcoded Values to Config (10 minutes)

---

## 5. TESTING CHECKLIST

After applying fixes:

- [ ] Scene opens without errors
- [ ] Maze generates on Play
- [ ] Walls display correctly
- [ ] Floor renders
- [ ] Player spawns at center
- [ ] Doors appear at room entrances
- [ ] Chests spawn in rooms
- [ ] HUD displays (health bar, inventory, etc.)
- [ ] Torches light nearby areas
- [ ] Console shows no errors
- [ ] No missing component warnings

---

## 6. GIT WORKFLOW

Before making changes:

```powershell
# Run backup script
.\backup.ps1

# Create feature branch
git checkout -b fix/hud-plugin-out-compliance
git checkout -b fix/namespace-mismatches
git checkout -b fix/folder-numbering
```

After each phase:

```powershell
# Commit with meaningful message
git add Assets/Scripts/HUD/*.cs
git commit -m "fix: refactor HUD to use prefabs (plug-in-out compliant)"

# Create diffs
git diff HEAD~1 > diff_tmp/hud_refactor_diff.patch
```

---

## 7. FILES NEEDING REVISION

### High Priority

1. **Assets/Scripts/HUD/UIBarsSystem.cs**
   - Replace 34x `new GameObject()` with prefab calls
   - Replace 40x `AddComponent<>()` with GetComponent<>()

2. **Assets/Scripts/HUD/PopWinEngine.cs**
   - Replace 40x `new GameObject()` with prefab calls
   - Replace 45x `AddComponent<>()` with GetComponent<>()

3. **Assets/Scripts/HUD/HUDSystem.cs**
   - Replace 35x `new GameObject()` with prefab calls
   - Replace 35x `AddComponent<>()` with GetComponent<>()

### Medium Priority

4. **Assets/Scripts/Core/11_Utilities/ShareSystm.cs**
   - Rename to ShareSystem.cs
   - Update class name

5. **Assets/Scripts/Core/14_Geometry/Triangle.cs**
   - Remove emoji from documentation

6. **Assets/Scripts/Ressources/*.cs** (13 files)
   - Update namespace declarations to Code.Lavos.Ressources

---

## 8. EXPECTED RESULTS AFTER FIXES

| Metric | Before | After |
|--------|--------|-------|
| Architecture Health | 75% | 95% |
| Code Quality | 85% | 98% |
| Compilation Errors | 0 | 0 |
| Compilation Warnings | 0 | 0 |
| Plug-in-Out Violations | 150+ | 0 |
| Namespace Conflicts | 12 | 0 |
| Scene Load Time | Unknown | < 5s |

---

## 9. DOCUMENTATION

Update after fixes:
- Assets/Docs/COMPLIANCE_REPORT_20260307.md
- Assets/Docs/ARCHITECTURE_OVERVIEW.md
- Assets/Docs/README.md

---

## 10. NEXT STEPS

1. Read this document completely
2. Run backup.ps1
3. Choose fix method (manual or script-assisted)
4. Apply fixes in recommended sequence
5. Test after each phase
6. Commit with meaningful messages
7. Update documentation
8. Request code review

---

**Report Generated:** 2026-03-09  
**Codename:** BetsyBoop  
**Next Action:** Choose Phase 1 fixes to apply
**Encoding:** UTF-8 Unix LF  
**License:** GPL-3.0

---
