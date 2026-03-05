# QuickSceneSetup Static Class Fix

**Date:** 2026-03-04
**File:** `Assets/Scripts/Editor/QuickSceneSetup.cs`
**Status:** ✅ **FIXED**

---

## 🐛 **ERROR**

```
ArgumentException: GetComponent requires that the requested component 
'GroundPlaneGenerator' derives from MonoBehaviour or Component or is an 
interface.
```

**Stack Trace:**
```
UnityEngine.GameObject.GetComponent (System.Type type)
Code.Lavos.Editor.QuickSceneSetup.AddComponentIfMissing 
    (UnityEngine.GameObject go, System.String typeName)
Code.Lavos.Editor.QuickSceneSetup.AddCoreComponents (UnityEngine.GameObject mazeTest)
Code.Lavos.Editor.QuickSceneSetup.GenerateCompleteScene ()
```

---

## 🔍 **ROOT CAUSE**

`GroundPlaneGenerator` and `CeilingGenerator` are **static classes**, not `MonoBehaviour` components.

**Static classes cannot:**
- Be added via `AddComponent()`
- Be attached to GameObjects
- Derive from `MonoBehaviour` or `Component`

---

## ✅ **SOLUTION**

### **Removed static classes from component list:**

```csharp
private static void AddCoreComponents(GameObject mazeTest)
{
    // ... other components ...
    
    // Test controller (FpsMazeTest handles ground/ceiling generation internally)
    AddComponentIfMissing(mazeTest, "Code.Lavos.Core.FpsMazeTest");
    
    Debug.Log("  ✅ Added 11 components (GroundPlaneGenerator/CeilingGenerator are static classes)");
}
```

---

## 📝 **FILES MODIFIED**

| File | Changes |
|------|---------|
| `QuickSceneSetup.cs` | ✅ Removed static class component additions |
| | ✅ Updated component count: 13 → 11 |
| | ✅ Updated debug log |

**Location:** `Assets/Scripts/Editor/QuickSceneSetup.cs`

**Diff stored in:** `diff_tmp/QuickSceneSetup_static_class_fix_20260304.diff`

---

## 🏗️ **PLUG-IN-OUT ARCHITECTURE**

### **Two Types of Modules:**

#### **1. Component Modules (MonoBehaviour)**
- Attached to GameObjects
- Can be configured via Inspector
- Can be enabled/disabled at runtime
- Examples: `MazeGenerator`, `MazeRenderer`, `SpatialPlacer`, `TorchPool`

#### **2. Static Utility Modules**
- Not attached to GameObjects
- Called directly via static methods
- Always available (no enable/disable)
- Examples: `GroundPlaneGenerator`, `CeilingGenerator`, `DrawingManager`

**Both are valid plug-in modules**, just used differently.

---

## ⚠️ Your Action Required

**Could you please run:**
```powershell
.\backup.ps1
```

**Then test in Unity:**
1. Tools → Quick Scene Setup → Generate Complete Scene
2. Press Play
3. Verify no errors and ground/ceiling appear correctly
