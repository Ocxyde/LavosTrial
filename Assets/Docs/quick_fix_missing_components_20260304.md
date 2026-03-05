# Quick Fix Guide - Missing Components

**Problem:** FpsMazeTest reports missing components in Console

---

## ✅ **ONE-CLICK FIX**

### **In Unity Editor:**

1. Go to menu: **Tools → Auto-Fix MazeTest Setup**
2. Wait for "SETUP COMPLETE!" message
3. Press Play!

**That's it!** ✨

---

## 📋 **WHAT IT DOES**

The auto-fix script:
- ✅ Creates `MazeTest` GameObject (if needed)
- ✅ Adds all 7 required components
- ✅ Configures maze size (21x21)
- ✅ Creates LightEngine (separate)
- ✅ Logs setup confirmation

---

## 🎯 **MANUAL FIX (Alternative)**

If you prefer manual setup:

1. **Create GameObject:**
   - Right-click in Hierarchy → Create Empty
   - Rename to "MazeTest"

2. **Add Components** (all on same GameObject):
   - FpsMazeTest
   - MazeGenerator
   - MazeRenderer
   - MazeIntegration
   - SpatialPlacer
   - TorchPool
   - LightPlacementEngine

3. **Create LightEngine:**
   - Create Empty GameObject named "LightEngine"
   - Add LightEngine component

4. **Press Play!**

---

## 🔍 **VERIFY SETUP**

**In Inspector (select MazeTest GameObject):**
```
✅ FpsMazeTest
✅ MazeGenerator
✅ MazeRenderer
✅ MazeIntegration
✅ SpatialPlacer
✅ TorchPool
✅ LightPlacementEngine
```

**In Hierarchy:**
```
✅ MazeTest (with all components above)
✅ LightEngine (separate GameObject)
```

---

## ⚠️ **COMMON MISTAKES**

❌ Components on different GameObjects  
✅ **ALL components must be on SAME GameObject**

❌ Missing LightEngine  
✅ **LightEngine must exist in scene**

❌ Wrong maze size (31x31)  
✅ **Use 21x21 for better performance**

---

## 🎮 **EXPECTED CONSOLE**

**After fix, Console should show:**
```
[AutoFix] ✅ Created 'MazeTest' GameObject
[AutoFix] ✅ Added MazeGenerator
[AutoFix] ✅ Added MazeRenderer
...
[AutoFix] ✅ SETUP COMPLETE!
[FpsMazeTest] All components found. Ready to test.
[MazeGenerator] Generated 21x21 maze
```

**NO "MISSING component" errors!**

---

## 📝 **FILES CREATED**

| File | Purpose |
|------|---------|
| `AutoFixMazeTest.cs` | One-click fix script |
| `AddFpsMazeTestComponents.cs` | Alternative fix script |

**Location:** `Assets/Scripts/Editor/`

---

## 🔧 **STILL BROKEN?**

If still having issues:

1. **Delete old MazeTest GameObject**
2. **Run:** Tools → Auto-Fix MazeTest Setup
3. **Check Console** for errors
4. **Verify** all components in Inspector

---

**Generated:** 2026-03-04  
**Unity Version:** 6000.3.7f1  
**Status:** ✅ FIXED

---

*Document generated - Unity 6 (6000.3.7f1) compatible - UTF-8 encoding - Unix LF*
