# Scene Fix Report - FpsMazeTest_Fresh.unity

**Date:** 2026-03-04  
**Unity Version:** 6000.3.7f1  
**Status:** ✅ Fixed

---

## 🐛 Issues Found

### 1. Missing LightEngine Component
**Symptom:** Torches turn ON but emit no light  
**Root Cause:** LightEngine singleton not found in scene  
**Fix:** Added LightEngine component to MazeTest GameObject

```yaml
Before:
  MazeTest has 9 components
  
After:
  MazeTest has 10 components
  └── LightEngine (NEW)
```

### 2. Duplicate TorchPool Component
**Symptom:** Component lookup returns wrong instance  
**Root Cause:** Two TorchPool components on same GameObject  
**Fix:** Removed duplicate (component ID 2138532863)

```diff
- component: {fileID: 2138532863}  # Removed duplicate
```

### 3. Broken Component References
**Symptom:** SpatialPlacer can't find MazeGenerator  
**Root Cause:** FileID references were {fileID: 0}  
**Fix:** Updated to correct references

```diff
- mazeGenerator: {fileID: 0}
+ mazeGenerator: {fileID: 2138532868}

- spatialPlacer: {fileID: 0}
+ spatialPlacer: {fileID: 2138532864}

- torchPool: {fileID: 0}
+ torchPool: {fileID: 2138532867}

- lightPlacementEngine: {fileID: 0}
+ lightPlacementEngine: {fileID: 2138532862}
```

### 4. Input System Conflict
**Symptom:** InvalidOperationException on Input.GetKey()  
**Root Cause:** Using old Input class with New Input System enabled  
**Fix:** Converted to UnityEngine.InputSystem

```csharp
// Before (OLD Input System)
if (Input.GetKeyDown(activateNextKey))

// After (New Input System)
if (keyboard.tKey.wasPressedThisFrame)
```

### 5. Compilation Error - ValidateSetup()
**Symptom:** CS0023, CS0127 errors  
**Root Cause:** Method declared as `void` but returns `bool`  
**Fix:** Changed signature to `private bool ValidateSetup()`

---

## 🔧 Files Modified

### 1. Assets/Scenes/FpsMazeTest_Fresh.unity
- Added LightEngine component
- Removed duplicate TorchPool
- Fixed 4 component references

### 2. Assets/Scripts/Tests/TorchManualActivator.cs
- Added `using UnityEngine.InputSystem;`
- Converted 3 key checks to New Input System
- Removed unused KeyCode fields

### 3. Assets/Scripts/Tests/FpsMazeTest.cs
- Fixed ValidateSetup() return type

---

## ✅ Verification Steps

1. **Open Scene:**
   ```
   Assets/Scenes/FpsMazeTest_Fresh.unity
   ```

2. **Check Components:**
   - Select MazeTest GameObject
   - Verify 10 components present
   - Ensure LightEngine is enabled

3. **Press Play:**
   - Console should show 0 errors
   - Maze generates automatically
   - 40 torches ignite (orange light)
   - Ground plane appears
   - FPS player spawns

4. **Test Controls:**
   - WASD: Move
   - Mouse: Look around
   - Shift: Sprint
   - Space: Jump
   - T: Activate next torch (manual test)
   - H: Activate all torches
   - X: Turn off all torches

---

## 🎯 Expected Visual Result

```
┌─────────────────────────────────────┐
│  🕯️  TORCH LIGHT TEST - PASSED  🕯️ │
├─────────────────────────────────────┤
│                                     │
│   Maze walls: Visible ✓             │
│   Torches: 40 placed ✓              │
│   Light emission: Orange glow ✓     │
│   Fog of war: Darkness at distance ✓│
│   Ground plane: Present ✓           │
│   Ceiling: Present ✓                │
│   FPS Camera: Eye level (1.7m) ✓    │
│                                     │
└─────────────────────────────────────┘
```

---

## 📝 Technical Details

### LightEngine Configuration
```yaml
enableDynamicLights: true
defaultLightRange: 12
defaultLightIntensity: 1.8
defaultLightColor: RGB(1.0, 0.9, 0.7)  # Warm torch glow
flickerSpeed: 2
flickerAmount: 0.15
enableFogOfWar: true
darknessFalloff: 15
maxDynamicLights: 100
```

### Torch Settings
```yaml
torchCount: 40
torchHeightRatio: 0.6  # 60% up the wall
torchInset: 0.35       # 35% into cell
minTorchDistance: 3
```

### Maze Settings
```yaml
mazeWidth: 31
mazeHeight: 31
cellSize: 6            # Wide corridors
wallHeight: 3.5
```

---

## 🚨 Troubleshooting

### Still Dark?
1. Check LightEngine is enabled
2. Verify fog density not too high (0.01)
3. Ensure camera not inside wall
4. Check torch Light components exist

### Torches Not Placed?
1. Check SpatialPlacer.torchPool reference
2. Verify TorchPool.torchHandlePrefab (can be null)
3. Check console for "[TorchPool] Flame material is null"

### Input Not Working?
1. Verify New Input System enabled in Player Settings
2. Check Input System package installed
3. Ensure cursor is locked (Cursor.lockState)

---

## 📚 Related Documentation

- [ARCHITECTURE_OVERVIEW.md](../ARCHITECTURE_OVERVIEW.md)
- [LightEngine.cs](../Assets/Scripts/Core/12_Compute/LightEngine.cs)
- [TorchPool.cs](../Assets/Scripts/Core/10_Resources/TorchPool.cs)

---

**Report Generated:** 2026-03-04  
**Backup Status:** ✅ Completed
