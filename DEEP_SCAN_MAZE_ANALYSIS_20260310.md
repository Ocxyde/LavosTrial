# Deep Scan Report - Maze System Critical Analysis

**Date:** 2026-03-10
**Session:** Deep Scan & Maze Rebalancing
**Unity Version:** 6000.3.10f1
**License:** GPL-3.0
**Encoding:** UTF-8 Unix LF

---

## ⚠️ CRITICAL WARNINGS

### 1. **DUPLICATE CODE - GridMazeGenerator vs DungeonMazeGenerator**

**Severity:** HIGH
**Files Affected:**
- `Assets/Scripts/Core/06_Maze/GridMazeGenerator.cs`
- `Assets/Scripts/Core/06_Maze/DungeonMazeGenerator.cs`
- `Assets/Scripts/Core/06_Maze/CompleteMazeBuilder.cs`

**Problem:**
Two different maze generators exist with overlapping functionality:
- `GridMazeGenerator`: Uses 4-direction cardinal-only DFS + A*
- `DungeonMazeGenerator`: Uses 8-direction DFS + chamber expansion

Both are used by `CompleteMazeBuilder` but there's no clear separation of concerns.

**Impact:**
- Confusing architecture (which generator to use?)
- Code duplication (fixes must be applied twice)
- Maintenance burden

**Recommendation:**
Consolidate into single generator with strategy pattern.

---

### 2. **HARDCODED VALUES in DeadEndCorridorSystem**

**Severity:** HIGH
**File:** `Assets/Scripts/Core/06_Maze/DeadEndCorridorSystem.cs`

**Problem:**
Despite claims of "no hardcoding", default values don't match documentation:

| Parameter | Current | Expected (from docs) | Delta |
|-----------|---------|---------------------|-------|
| `BaseDensity` | 0.15f | 0.30f | -50% ❌ |
| `MinLength` | 2 | 3 | -33% ❌ |
| `MaxLength` | 5 | 8 | -37.5% ❌ |
| `ChestChance` | 0.5f | 0.4f | +25% ⚠️ |
| `EnemyChance` | 0.3f | 0.4f | -25% ⚠️ |
| `TrapChance` | 0.1f | 0.05f | +100% ⚠️ |
| `MaxGridPercentage` | 0.05f | 0.35f | -85% ❌ |

**Impact:**
- Dead-end density is 50% of intended
- Corridors are too short
- Configuration doesn't match documentation

---

### 3. **POWER CURVE SCALING NOT ENABLED**

**Severity:** CRITICAL
**File:** `DeadEndCorridorSystem.cs`
**Line:** ~201

**Current Code:**
```csharp
float spawnDensity = _config.BaseDensity;  // Always 15% - NO SCALING!
```

**Should Be:**
```csharp
float spawnDensity = CalculateScaledDensity(level);
// Formula: BaseDensity × Lerp(1.0, MaxMultiplier, t^Exponent)
```

**Impact:**
- All levels have same dead-end density (no progression)
- Level 39 should have 75% density, actually has 15%
- Game doesn't get harder as intended

---

### 4. **POISSON DISK SAMPLING NOT IMPLEMENTED**

**Severity:** MEDIUM
**Documentation Claims:**
> "Poisson distribution for natural spacing"

**Reality:**
Uses uniform random distribution (simple `Random.value < density`)

**Impact:**
Dead-ends cluster together instead of being evenly distributed.

---

### 5. **MISSING Null Checks in CompleteMazeBuilder**

**Severity:** MEDIUM
**File:** `Assets/Scripts/Core/06_Maze/CompleteMazeBuilder.cs`

**Problem:**
Multiple methods access `_config` and `_mazeData` without proper null checks.

**Risk:** `NullReferenceException` if components missing.

---

## 🐛 POTENTIAL BUGS

### Bug #1: Array Index Out of Bounds Risk

**File:** `GridMazeGenerator.cs`
**Line:** ~360

Recursive DFS doesn't validate initial bounds before accessing `visited[cx, cz]`.

---

### Bug #2: Infinite Loop in A* Pathfinding

**File:** `GridMazeGenerator.cs`
**Line:** ~385-415

No iteration limit - for 51×51 maze, may explore all 2601 cells before giving up.

**Fix:** Add `maxIterations` check.

---

### Bug #3: Memory Leak in CorridorFlowSystem

**File:** `CorridorFlowSystem.cs`

Lists created fresh each `Generate()` call without clearing old ones.

---

### Bug #4: Player Spawn Position Not Validated

**File:** `CompleteMazeBuilder.cs`
**Line:** ~623-640

No validation that spawn position is walkable - player may spawn inside wall.

---

## 📊 COMPILATION STATUS

✅ **0 compilation errors** (confirmed via Rider build)

---

## 🎯 RECOMMENDED FIXES

### Priority 1: Critical (Fix Today)

1. **Update DeadEndCorridorConfig defaults:**
   - `BaseDensity`: 0.15f → 0.30f
   - `MinLength`: 2 → 3
   - `MaxLength`: 5 → 8
   - `ChestChance`: 0.5f → 0.4f
   - `EnemyChance`: 0.3f → 0.4f
   - `TrapChance`: 0.1f → 0.05f
   - `MaxGridPercentage`: 0.05f → 0.35f

2. **Enable power curve scaling:**
   - Call `CalculateScaledDensity(level)` instead of using `_config.BaseDensity`

3. **Add A* iteration limit:**
   - Prevent infinite loops on large mazes

4. **Add null checks:**
   - Protect against missing components

### Priority 2: Rebalancing (This Week)

5. Implement Poisson disk sampling
6. Test all levels (0-39)
7. Verify dead-end counts match expectations

### Priority 3: Architecture (Next Week)

8. Consolidate duplicate maze generators
9. Standardize namespaces
10. Add conditional compilation for debug logs

---

## 📝 FILES REQUIRING CHANGES

| File | Priority | Estimated Time |
|------|----------|----------------|
| `DeadEndCorridorSystem.cs` | CRITICAL | 45 min |
| `GridMazeGenerator.cs` | HIGH | 30 min |
| `CompleteMazeBuilder.cs` | MEDIUM | 30 min |
| `CorridorFlowSystem.cs` | MEDIUM | 20 min |

**Total:** ~2 hours for Priority 1 fixes

---

## ❓ NEXT STEPS

**Which approach do you want?**

1. **Fix Critical Bugs Only** (~1 hour)
   - Power curve scaling
   - Config defaults
   - A* iteration limits

2. **Complete Rebalancing** (~3 hours)
   - All Priority 1 + 2 fixes
   - Poisson disk sampling
   - Full testing

3. **Full Architecture Cleanup** (~6 hours)
   - All priorities
   - Consolidate generators
   - Code cleanup

**Recommendation:** Start with **Option 1** (Critical Fixes), then test before proceeding.

---

**Generated:** 2026-03-10
**License:** GPL-3.0
**Encoding:** UTF-8 Unix LF

---

*End of Deep Scan Report*
