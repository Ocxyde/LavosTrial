# TODO.md - Project Tasks & Issues

**Project:** PeuImporte (Unity 6000.3.7f1)  
**Last Scan:** 2026-03-01  
**Status:** ⚠️ 7 Critical | 12 High | 14 Medium | 6 Low

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
  - `cellSize = 4f` (appears in 6+ files)
  - `wallHeight = 3f` (appears in 5+ files)
  - `interactionRange = 3f` (appears in 4+ files)

### Compiler Warnings

- [x] **CS0618** - Fixed: `FindObjectOfType` → `FindFirstObjectByType` in PlayerController
- [x] **CS0067** - Fixed: Added pragma for unused `OnCombatStatChanged` event
- [x] **CS0414** - Fixed: Added comment for reserved `invincibilityTime` field

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

- [ ] **PlayerStats.cs:83-95** - Direct dependency on EventHandler (hard to test)
- [ ] **CombatSystem.cs:10-15** - Requires both StatsEngine AND EventHandler
- [ ] Add interfaces for better testability
- [ ] Reduce circular dependencies (currently uses reflection to avoid)

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

### Sprint 1 - Critical Fixes (Priority 1)
1. Fix memory leaks in DoubleDoor.cs and ChestBehavior.cs
2. Fix event subscription leaks in UIBarsSystem.cs and HUDSystem.cs
3. Add null checks in CombatSystem.cs and PlayerController.cs
4. Consolidate singleton initialization pattern

### Sprint 2 - Performance & Stability (Priority 2)
1. Replace FindFirstObjectByType with Inspector assignments
2. Initialize all serialized lists/arrays
3. Add RequireComponent attributes
4. Add null checks in InventoryUI.cs

### Sprint 3 - Code Quality (Priority 3)
1. Consolidate PixelCanvas duplicate code
2. Choose one HUD system, remove others
3. Extract magic numbers to configuration
4. Add XML documentation to public APIs

### Sprint 4 - Cleanup (Priority 4)
1. Remove unused events and reserved fields
2. Make debug logs conditional
3. Standardize naming conventions
4. Rename Ennemies folder

---

## 📊 CODE METRICS

| Metric | Value |
|--------|-------|
| Total C# Files | 60 |
| Lines of Code | ~16,000+ |
| Critical Issues | 7 |
| High Issues | 12 |
| Medium Issues | 14 |
| Low Issues | 6 |
| Compiler Warnings | 0 (fixed) |
| Compiler Errors | 0 |

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

---

**Generated:** 2026-03-01  
**Unity Version:** 6000.3.7f1  
**IDE:** Rider  
**Input System:** New Input System  
