# Fix Test Suite Errors - Unity Test Runner

**Date:** 2026-03-10
**Unity Version:** 6000.3.10f1
**Status:** ⚠️ REQUIRES MANUAL FIX
**License:** GPL-3.0

---

## 🚨 ERRORS FOUND

### Error 1: URP Assets Included in Build

**Error Message:**
```
URP assets included in build
- PC_RPAsset - Assets/Settings/PC_RPAsset.asset
- PC_Renderer - Assets/Settings/PC_Renderer.asset
  - UnityEngine.Rendering.Universal.UniversalRendererData
```

**Severity:** WARNING

**Cause:**
URP (Universal Render Pipeline) assets are being included in the player build unnecessarily. This happens when:
1. URP assets are referenced in the Graphics Settings
2. Test scenes use URP renderer
3. Build settings include URP packages

**Impact:**
- Increased build size
- Longer build times
- Potential runtime issues if URP not properly configured

---

### Error 2: Missing Scene File

**Error Message:**
```
'Assets/Scenes/FpsMazeTest_Fresh.unity' is an incorrect path for a scene file.
BuildPlayer expects paths relative to the project folder.
```

**Severity:** CRITICAL

**Cause:**
The Test Runner is configured to use a scene that doesn't exist:
- Expected: `Assets/Scenes/FpsMazeTest_Fresh.unity`
- Actual: **FILE NOT FOUND**

**Available Test Scenes:**
```
Assets/Scenes/
├── BoundaryTest.unity
├── MainScene_Maze.unity
└── MazeLav8s_v1-0_*.unity (multiple versions)
```

**Impact:**
- ❌ Player build fails
- ❌ Tests cannot run in player mode
- ❌ TestLaunchFailedException thrown

---

### Error 3: Player Build Failed

**Error Message:**
```
Player build failed
TestLaunchFailedException: Player build failed
```

**Severity:** CRITICAL

**Cause:**
Cascading failure from Error 2 (missing scene file)

**Stack Trace:**
```
UnityEditor.TestTools.TestRunner.PlayerLauncher.BuildAndRunPlayer()
UnityEditor.TestTools.TestRunner.PlayerLauncher.Run()
```

---

## 🔧 SOLUTIONS

### Solution 1: Fix Missing Scene (CRITICAL)

**Option A: Create Missing Scene**

1. In Unity Editor: `File > New Scene`
2. Add required test components:
   - Player prefab
   - Camera
   - Test environment
3. Save as: `Assets/Scenes/FpsMazeTest_Fresh.unity`
4. Add to Build Settings: `File > Build Settings > Add Open Scenes`

**Option B: Update Test Runner Configuration**

1. Open Test Runner: `Window > General > Test Runner`
2. Find test configuration
3. Change scene path from:
   ```
   Assets/Scenes/FpsMazeTest_Fresh.unity
   ```
   To existing scene:
   ```
   Assets/Scenes/MazeLav8s_v1-0_1_4.unity
   ```
4. Save configuration

**Option C: Remove Player Mode Tests**

If player mode tests are not needed:
1. Open Test Runner
2. Disable "Run tests in player mode"
3. Run tests in Edit Mode only

**Recommended:** Option B (use existing scene)

---

### Solution 2: Fix URP Asset Inclusion

**Step 1: Check Graphics Settings**

1. `Edit > Project Settings > Graphics`
2. Check "Scriptable Render Pipeline Settings"
3. If using Built-in Render Pipeline:
   - Remove URP references
4. If using URP:
   - Ensure PC_RPAsset is properly configured

**Step 2: Check Build Settings**

1. `File > Build Settings`
2. Click "Player Settings"
3. Navigate to: `Player > Other Settings > Rendering`
4. Verify:
   - Color Space: Linear (for URP)
   - Auto Graphics API: ON (or manually remove unused APIs)

**Step 3: Check URP Asset References**

File: `Assets/Settings/PC_RPAsset.asset`

Ensure this asset is:
- ✅ Used by active scene
- ✅ Referenced in Universal Render Pipeline Asset list
- ❌ NOT in "Always Included" if not needed

**Step 4: Update Test Scenes**

If test scenes don't need URP:
1. Open each test scene
2. Change camera to use Built-in renderer
3. Remove URP post-processing volumes

---

## 📋 VERIFICATION CHECKLIST

After applying fixes:

### Scene Configuration
- [ ] `FpsMazeTest_Fresh.unity` exists OR test runner uses existing scene
- [ ] Scene added to Build Settings
- [ ] Scene path is relative to project folder

### URP Configuration
- [ ] Graphics settings configured correctly
- [ ] URP assets properly referenced
- [ ] No unnecessary assets in build

### Test Runner
- [ ] Tests can run in Edit Mode
- [ ] Tests can run in Player Mode (if needed)
- [ ] No build errors

---

## 🎯 IMMEDIATE ACTIONS REQUIRED

### Priority 1: Fix Missing Scene (5 minutes)

**Quick Fix:**
```
1. Open Test Runner window
2. Find configuration referencing FpsMazeTest_Fresh
3. Change to: Assets/Scenes/MazeLav8s_v1-0_1_4.unity
4. Save and retry
```

### Priority 2: Verify URP Settings (10 minutes)

**Check:**
1. Project Settings > Graphics > Scriptable Render Pipeline
2. Verify PC_RPAsset is correctly configured
3. Check if URP is actually needed for tests

### Priority 3: Run Tests (5 minutes)

**Test:**
1. Run Edit Mode tests first
2. If successful, try Player Mode
3. Verify no build errors

---

## 📝 TECHNICAL DETAILS

### Test Assembly Configuration

**File:** `Assets/Scripts/Tests/Code.Lavos.Tests.asmdef`

```json
{
    "name": "Code.Lavos.Tests",
    "references": ["Code.Lavos.Core"],
    "optionalUnityReferences": ["TestAssemblies"],
    "includePlatforms": ["Editor"]
}
```

**Note:** Currently configured for **Editor only** - this is correct for unit tests.

### Available Test Files

```
Assets/Scripts/Tests/
├── Code.Lavos.Tests.asmdef
├── MazeGeometryTests.cs
├── MazeBinaryStorageTests.cs
└── GeometryMathTests.cs
```

All tests are **Edit Mode** tests (don't require player build).

---

## 🚫 WHAT NOT TO DO

1. **DON'T** create FpsMazeTest_Fresh.unity unless actually needed
2. **DON'T** remove URP assets if project uses URP
3. **DON'T** change asmdef to include Player unless tests need it
4. **DON'T** ignore the errors - they indicate configuration issues

---

## 💡 RECOMMENDATION

**For Your Project:**

Based on analysis, your tests are **Edit Mode unit tests** that don't require player mode:

1. **Disable Player Mode** in Test Runner
   - Tests run faster
   - No build errors
   - Same test coverage

2. **If Player Mode is needed:**
   - Use existing scene: `MazeLav8s_v1-0_1_4.unity`
   - Update test runner configuration
   - Ensure URP is properly configured

3. **URP Configuration:**
   - Keep PC_RPAsset if project uses URP
   - Warning about "included in build" is normal
   - Can be ignored if URP is intentional

---

## 📊 ERROR SUMMARY

| Error | Severity | Status | Fix Time |
|-------|----------|--------|----------|
| Missing scene file | CRITICAL | ⏳ Pending | 5 min |
| Player build failed | CRITICAL | ⏳ Pending | Auto-fixed |
| URP assets in build | WARNING | ℹ️ Info | N/A |

**Total Fix Time:** 5-15 minutes

---

## 🔗 RELATED FILES

- `Assets/Scripts/Tests/Code.Lavos.Tests.asmdef`
- `Assets/Settings/PC_RPAsset.asset`
- `Assets/Settings/PC_Renderer.asset`
- `Assets/Scenes/MazeLav8s_v1-0_1_4.unity`

---

**Generated:** 2026-03-10
**License:** GPL-3.0
**Encoding:** UTF-8 Unix LF

---

*End of Test Suite Fix Document*
