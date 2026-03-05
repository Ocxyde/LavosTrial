# Fix Scene References - Quick Guide

**Problem:** Unity shows "ReloadScene failed: Asset path not found"

**Cause:** `SampleScene.unity` is in EditorBuildSettings but doesn't exist

---

## ✅ **FIX (2 Options)**

### **Option 1: Use Editor Menu**

**In Unity Editor:**
```
1. Tools → Fix Scene References
2. Removes missing scenes
3. Adds FpsMazeTest_Fresh.unity
```

### **Option 2: Manual Fix**

**In Unity Editor:**
```
1. File → Build Settings (Ctrl+Shift+B)
2. Remove "Assets/Scenes/SampleScene.unity" from list
3. Add "Assets/Scenes/FpsMazeTest_Fresh.unity"
4. Close Build Settings
```

---

## 🎯 **OPEN CORRECT SCENE**

**In Unity Editor:**
```
1. Tools → Open FpsMazeTest Scene
   OR
2. Double-click: Assets/Scenes/FpsMazeTest_Fresh.unity
```

---

## 📁 **VALID SCENES**

Your existing scenes:
- ✅ `Assets/Scenes/FpsMazeTest_Fresh.unity`
- ✅ `Assets/Scenes/FpsMazeTestScene.unity`
- ✅ `Assets/Scenes/SimpleMazeTest.unity`
- ✅ `Assets/Scenes/MainScene_Maze.unity`
- ✅ `Assets/Scenes/testingDoor.unity`
- ✅ `Assets/Scenes/TestMazeTorches.unity`
- ✅ `Assets/Scenes/TestMazeWithTorches.unity`

**Missing (to be removed):**
- ❌ `Assets/Scenes/SampleScene.unity`

---

## 🔧 **AFTER FIX**

**Expected Console:**
```
[FixSceneReferences] 🔧 Checking scene references...
[FixSceneReferences] ❌ Missing scene: Assets/Scenes/SampleScene.unity
[FixSceneReferences] ✅ Valid scene: Assets/Scenes/MainScene_Maze.unity
[FixSceneReferences] ✅ Added: Assets/Scenes/FpsMazeTest_Fresh.unity
[FixSceneReferences] ✅ Fixed! Removed 1 missing, 1 valid
```

**No more reload errors!**

---

**Generated:** 2026-03-04  
**Unity Version:** 6000.3.7f1  

---

*Document generated - Unity 6 (6000.3.7f1) compatible - UTF-8 encoding - Unix LF*
