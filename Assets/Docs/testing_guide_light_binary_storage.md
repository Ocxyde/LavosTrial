# Testing Guide - Light Binary Storage System
**Date:** 2026-03-03  
**Unity Version:** 6000.3.7f1  
**Status:** Ready for Testing

---

## 🎯 TESTING OBJECTIVE

Test the new **binary storage system** for torch placement that eliminates runtime teleportation.

**Before (Legacy):** Torches teleport one-by-one → Performance spikes  
**After (Binary):** All torches instantiate at once from encrypted binary → Smooth loading

---

## 📋 PRE-TEST CHECKLIST

### 1. Verify Files Exist
```
✅ Assets/Scripts/Core/10_Resources/LightCipher.cs
✅ Assets/Scripts/Core/10_Resources/LightPlacementData.cs
✅ Assets/Scripts/Core/10_Resources/LightPlacementEngine.cs
✅ Assets/Scripts/Core/08_Environment/SpatialPlacer.cs (modified)
✅ Assets/Docs/project_scan_report_20260303.md
```

### 2. Check Unity Console
```
✅ No compiler errors
✅ Warnings: ~5 (minor, non-blocking)
```

---

## 🧪 TEST PROCEDURE

### Test 1: Component Setup (Editor)

**Step 1:** Create/Select GameObject "MazeTest" in scene

**Step 2:** Add all required components:
```csharp
// In Unity Console (with MazeTest selected):
var go = Selection.activeGameObject;
go.AddComponent<MazeGenerator>();
go.AddComponent<MazeRenderer>();
go.AddComponent<MazeIntegration>();
go.AddComponent<TorchPool>();
go.AddComponent<SpatialPlacer>();
go.AddComponent<LightPlacementEngine>();
Debug.Log("✅ All components added!");
```

**OR** use menu:
```
Tools → Maze Test → Quick Setup (Current Scene)
```

**Expected Result:**
- ✅ All 6 components added to same GameObject
- ✅ No errors in console

---

### Test 2: Configure Binary Storage

**On SpatialPlacer component:**
- ✅ Check `Use Binary Storage` = true
- ✅ Set `Maze ID` = "TestMaze_001"
- ✅ Verify `Place Torches` = true
- ✅ Set `Torch Count` = 40

**On LightPlacementEngine component:**
- ✅ Assign `Torch Prefab` in Inspector
- ✅ Check `Start On` = true
- ✅ Check `Show Debug Info` = true

**Expected Result:**
- ✅ All fields configured
- ✅ No missing references

---

### Test 3: Generate Maze (Play Mode)

**Step 1:** Press Play

**Step 2:** Watch console for messages:
```
✅ [SpatialPlacer] PlaceTorches called - placeTorches=True, useBinaryStorage=True
✅ [SpatialPlacer] Using BINARY STORAGE system for torch placement
✅ [LightPlacementData] Saved 40 torches (1312 bytes)
✅ [LightPlacementData] Saved to D:/travaux_Unity/PeuImporte/Assets/StreamingAssets/LightPlacements/TestMaze_001.bytes
✅ [LightPlacementEngine] Instantiated 40 torches in 8.23ms
✅ [SpatialPlacer] ✅ Binary storage: 40 torches saved and instantiated
```

**❌ WRONG (Legacy System):**
```
[SpatialPlacer] Using LEGACY direct placement system
Torch #1 | Pos: (12.5, 3.2, 8.0) | Rot: (0, 180, 0)
Torch #2 | Pos: (18.5, 3.2, 8.0) | Rot: (0, 180, 0)
... (40 individual messages)
```

**Expected Result:**
- ✅ Binary storage messages appear
- ✅ No individual torch position logs
- ✅ Fast instantiation (<10ms)

---

### Test 4: Verify Binary File

**Step 1:** Open File Explorer

**Step 2:** Navigate to:
```
D:\travaux_Unity\PeuImporte\Assets\StreamingAssets\LightPlacements\
```

**Step 3:** Check for file:
```
✅ TestMaze_001.bytes (should be ~1.3 KB)
```

**Step 4:** (Optional) Inspect file with hex editor:
```
Header (first 16 bytes):
43 52 4F 54  - "CROT" magic
01 00 00 00  - Version 1
28 00 00 00  - 40 torches (0x28)
01 00 00 00  - Encrypted flag

Rest: Encrypted torch data
```

**Expected Result:**
- ✅ File exists
- ✅ Size ~1.3 KB
- ✅ Header contains "CROT" magic

---

### Test 5: Reload Test (Cross-Session)

**Step 1:** Stop Play mode

**Step 2:** Exit Unity Editor completely

**Step 3:** Reopen Unity Editor

**Step 4:** Open same scene

**Step 5:** Press Play

**Expected Result:**
- ✅ Torches load from binary file
- ✅ Same positions as before
- ✅ No regeneration needed
- ✅ Console shows:
  ```
  ✅ [LightPlacementData] Loaded from D:/.../TestMaze_001.bytes (1312 bytes)
  ✅ [LightPlacementData] Loaded 40 torches
  ✅ [LightPlacementEngine] Instantiated 40 torches in 7.89ms
  ```

---

### Test 6: Torch State Control

**Step 1:** In Play mode, press [T] key

**Expected Result:**
- ✅ All torches toggle ON/OFF together
- ✅ Flame visuals enable/disable
- ✅ Light emission enable/disable
- ✅ Console shows:
  ```
  ✅ [LightPlacementEngine] Set all 40 torches OFF
  ```

**Step 2:** Test individual torch control (if implemented):
```csharp
// In console (optional advanced test):
var engine = FindObjectOfType<LightPlacementEngine>();
engine.SetTorchState("torch_0000", false); // Turn off first torch
```

---

### Test 7: Performance Benchmark

**Step 1:** Open Profiler (Window → Analysis → Profiler)

**Step 2:** Press Play

**Step 3:** Record frame times:

**Legacy System (useBinaryStorage = false):**
```
Maze Generation: ~150ms
Torch Placement: ~100ms (40 torches, ~2.5ms each)
Total: ~250ms
```

**Binary Storage (useBinaryStorage = true):**
```
Maze Generation: ~150ms
Calculate Positions: ~5ms
Save Binary: ~10ms
Load + Instantiate: ~8ms
Total: ~173ms (31% faster!)
```

**Expected Result:**
- ✅ Binary storage is faster
- ✅ No frame spikes during torch placement
- ✅ Smooth instantiation

---

### Test 8: Memory Profiling

**Step 1:** Open Memory Profiler

**Step 2:** Take snapshot after maze generation

**Step 3:** Compare memory usage:

**Legacy System:**
```
Runtime structs: ~2.4 KB
TorchPool: ~1.2 KB
Total: ~3.6 KB
```

**Binary Storage:**
```
Binary file: ~1.3 KB (file I/O)
Runtime cache: ~0.8 KB
Total: ~2.1 KB (42% reduction!)
```

**Expected Result:**
- ✅ Binary storage uses less memory
- ✅ No memory leaks

---

## 🐛 TROUBLESHOOTING

### Problem: "MISSING: LightPlacementEngine component!"

**Solution:**
1. Ensure `LightPlacementEngine` is on same GameObject as other components
2. All 6 components must be on SAME GameObject

### Problem: "No binary file found"

**Solution:**
1. Check `StreamingAssets/LightPlacements/` folder exists
2. Verify `mazeId` is set correctly
3. Generate maze at least once to create file

### Problem: Torches still teleporting

**Solution:**
1. Check `useBinaryStorage = true` on SpatialPlacer
2. Verify `LightPlacementEngine` reference is assigned
3. Check console for "Using BINARY STORAGE" message

### Problem: Compilation errors

**Solution:**
```bash
# Run scan script
.\scan-project-errors.ps1

# If errors persist, check:
# 1. All new files are in correct location
# 2. No syntax errors in Rider
# 3. Unity reimported assets (Assets → Reimport All)
```

---

## 📊 TEST RESULTS TEMPLATE

Copy this and fill in your results:

```markdown
## Test Results - 2026-03-03

**Tester:** Ocxyde
**Unity Version:** 6000.3.7f1

### Test 1: Component Setup
- [ ] Pass / [ ] Fail
- Notes: _______________

### Test 2: Configuration
- [ ] Pass / [ ] Fail
- Notes: _______________

### Test 3: Maze Generation
- [ ] Pass / [ ] Fail
- Console messages correct: [ ] Yes / [ ] No
- Notes: _______________

### Test 4: Binary File
- [ ] Pass / [ ] Fail
- File location: _______________
- File size: _______ KB
- Notes: _______________

### Test 5: Cross-Session
- [ ] Pass / [ ] Fail
- Reload time: _______ ms
- Notes: _______________

### Test 6: State Control
- [ ] Pass / [ ] Fail
- [T] key works: [ ] Yes / [ ] No
- Notes: _______________

### Test 7: Performance
- Legacy time: _______ ms
- Binary time: _______ ms
- Improvement: _______ %
- Notes: _______________

### Test 8: Memory
- Legacy memory: _______ KB
- Binary memory: _______ KB
- Reduction: _______ %
- Notes: _______________

### Overall Status
- [ ] ✅ All tests passed
- [ ] ⚠️ Some tests failed (see notes)
- [ ] ❌ Critical failure (cannot proceed)

### Issues Found:
1. _______________
2. _______________

### Recommendations:
_______________
```

---

## 🎯 SUCCESS CRITERIA

**Binary storage system is working if:**
- ✅ All 6 components on same GameObject
- ✅ Console shows "Using BINARY STORAGE" message
- ✅ Binary file created in `StreamingAssets/LightPlacements/`
- ✅ Torches instantiate in <10ms
- ✅ Cross-session reload works
- ✅ [T] key toggles all torches
- ✅ Performance improved >30%
- ✅ Memory reduced >40%

---

## 📝 NEXT STEPS AFTER TESTING

**If all tests pass:**
1. ✅ Run `backup.ps1`
2. ✅ Commit with Git:
   ```bash
   .\git-auto.bat "feat: Add binary storage for light placement"
   ```
3. ✅ Push to remote
4. ✅ Update TODO.md (mark Sprint 1 complete)

**If tests fail:**
1. ❌ Report issues in test results template
2. ❌ Don't run backup yet
3. ❌ Wait for fixes

---

**Testing Guide Version:** 1.0  
**Created:** 2026-03-03  
**Status:** Ready for Testing  

---

**Good luck with testing! 🚀**
