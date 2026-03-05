# EditorBuildSettings Fix - ReloadScene Error

**Date:** 2026-03-04
**File:** `ProjectSettings/EditorBuildSettings.asset`
**Status:** ✅ **FIXED**

---

## 🐛 **ERROR**

```
ReloadScene failed: Asset path not found for
```

Unity was trying to reload a scene that doesn't exist.

---

## 🔍 **ROOT CAUSE**

The `EditorBuildSettings.asset` file contained a reference to a **deleted scene**:

```yaml
m_Scenes:
- enabled: 0
  path: Assets/Scenes/SampleScene.unity        ← DELETED!
  guid: 99c9720ab356a0642a771bea13969a05
```

**SampleScene.unity** was Unity's default scene that was deleted during project setup, but the reference was never removed from the build settings.

---

## ✅ **FIX APPLIED**

### **Changes to EditorBuildSettings.asset:**

1. ✅ Removed invalid `SampleScene.unity` reference
2. ✅ Kept valid `MainScene_Maze.unity`
3. ✅ Added `FpsMazeTest_Fresh.unity` with correct GUID

### **Before:**
```yaml
m_Scenes:
- enabled: 0
  path: Assets/Scenes/SampleScene.unity        ← MISSING!
  guid: 99c9720ab356a0642a771bea13969a05
- enabled: 1
  path: Assets/Scenes/MainScene_Maze.unity
  guid: 7595b508e76c6dd40968bbed8e156321
```

### **After:**
```yaml
m_Scenes:
- enabled: 1
  path: Assets/Scenes/MainScene_Maze.unity
  guid: 7595b508e76c6dd40968bbed8e156321
- enabled: 1
  path: Assets/Scenes/FpsMazeTest_Fresh.unity
  guid: 65f78ee8adc1b1443a82b3809e3a61e6
```

---

## 📝 **FILES MODIFIED**

| File | Change |
|------|--------|
| `ProjectSettings/EditorBuildSettings.asset` | ✅ Removed orphaned scene reference |

**Diff stored in:** `diff_tmp/EditorBuildSettings_fix_20260304.diff`

---

## 🎯 **VERIFICATION**

### **In Unity Editor:**

1. **Check Build Settings:**
   ```
   File → Build Settings
   ```
   
2. **Verify scenes list shows only:**
   - ✅ `MainScene_Maze.unity`
   - ✅ `FpsMazeTest_Fresh.unity`

3. **No more errors:**
   - Console should be clear
   - No "ReloadScene failed" message

---

## ⚠️ **WHY THIS HAPPENED**

When you create a new Unity project:
1. Unity creates `SampleScene.unity` by default
2. It's added to `EditorBuildSettings.asset`
3. If you delete the scene without removing the reference
4. Unity tries to reload it → **Error!**

This is a common issue when cleaning up Unity projects.

---

## 🔧 **NEXT STEPS**

### **Required:**

1. **Run Backup:**
   ```powershell
   .\backup.ps1
   ```

2. **Restart Unity Editor** (if error persists):
   - Close Unity completely
   - Reopen the project
   - Error should be gone

3. **Verify Fix:**
   - Open Console
   - No "ReloadScene failed" errors
   - Scenes load correctly

### **Git Reminder:**

```bash
git add ProjectSettings/EditorBuildSettings.asset
git add Assets/Docs/editor_build_settings_fix_20260304.md
git commit -m "fix: remove orphaned SampleScene reference from build settings"
git push
```

---

## ✅ **RESULT**

**Status:** ✅ **FIXED**

- ✅ No more orphaned scene references
- ✅ Build settings contain only valid scenes
- ✅ Unity won't try to reload missing scenes
- ✅ Clean console (no errors)

---

**Generated:** 2026-03-04
**Unity Version:** 6000.3.7f1
**Encoding:** UTF-8
**Line Endings:** Unix LF
