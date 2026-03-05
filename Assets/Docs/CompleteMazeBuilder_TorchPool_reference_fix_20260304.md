# CompleteMazeBuilder TorchPool Reference Fix

**Date:** 2026-03-04
**File:** `Assets/Scripts/Core/06_Maze/CompleteMazeBuilder.cs`
**Status:** ✅ **FIXED**

---

## 🐛 **ERROR**

```
[SpatialPlacer] TorchPool reference not assigned!
```

**Stack Trace:**
```
Code.Lavos.Core.SpatialPlacer:PlaceTorches () 
  (at Assets/Scripts/Core/08_Environment/SpatialPlacer.cs:537)
Code.Lavos.Core.SpatialPlacer:PlaceAll () 
  (at Assets/Scripts/Core/08_Environment/SpatialPlacer.cs:1074)
Code.Lavos.Core.CompleteMazeBuilder:PlaceObjects () 
  (at Assets/Scripts/Core/06_Maze/CompleteMazeBuilder.cs:730)
```

---

## 🔍 **ROOT CAUSE**

`SpatialPlacer.PlaceTorches()` requires the `torchPool` reference to be assigned, but `CompleteMazeBuilder` wasn't wiring up this reference before calling `SpatialPlacer.PlaceAll()`.

**Code in SpatialPlacer.cs (line 537):**
```csharp
if (torchPool == null)
{
    Debug.LogError("[SpatialPlacer] TorchPool reference not assigned!");
    return;  // ← Torch placement fails
}
```

---

## ✅ **SOLUTION**

### **Wire up TorchPool and LightPlacementEngine references:**

```csharp
private void PlaceObjects()
{
    // Step 8a: Place torches using LightPlacementEngine
    PlaceTorches();

    // Step 8b: Place other objects using SpatialPlacer
    spatialPlacer = GetComponent<SpatialPlacer>();
    if (spatialPlacer != null)
    {
        // Wire up TorchPool reference (required for torch placement)
        if (torchPool == null)
        {
            torchPool = FindFirstObjectByType<TorchPool>();
            if (torchPool == null)
            {
                var torchGO = new GameObject("TorchPool");
                torchPool = torchGO.AddComponent<TorchPool>();
            }
        }
        
        // Assign TorchPool to SpatialPlacer via reflection
        var torchPoolField = typeof(SpatialPlacer).GetField("torchPool", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (torchPoolField != null)
        {
            torchPoolField.SetValue(spatialPlacer, torchPool);
        }
        
        // Assign LightPlacementEngine to SpatialPlacer via reflection
        var lightPlacementEngineField = typeof(SpatialPlacer).GetField(
            "lightPlacementEngine", BindingFlags.NonPublic | BindingFlags.Instance);
        if (lightPlacementEngineField != null && lightPlacementEngine != null)
        {
            lightPlacementEngineField.SetValue(spatialPlacer, lightPlacementEngine);
        }

        spatialPlacer.PlaceTorchesEnabled = true;
        spatialPlacer.PlaceAll();
    }
}
```

### **Added using statement:**
```csharp
using System.Reflection;
```

---

## 📝 **FILES MODIFIED**

| File | Changes |
|------|---------|
| `CompleteMazeBuilder.cs` | ✅ Added `using System.Reflection;` |
| | ✅ Wire up `TorchPool` reference to `SpatialPlacer` |
| | ✅ Wire up `LightPlacementEngine` reference to `SpatialPlacer` |

**Location:** `Assets/Scripts/Core/06_Maze/CompleteMazeBuilder.cs`

**Diff stored in:** `diff_tmp/CompleteMazeBuilder_TorchPool_reference_fix_20260304.diff`

---

## 🎯 **VERIFICATION**

### **In Unity Editor:**

1. **Press Play** with `CompleteMazeBuilder` in scene

2. **Check Console for:**
   ```
   ✅ No "TorchPool reference not assigned" errors
   [CompleteMazeBuilder] 🎆 Torches placed from binary storage
   [CompleteMazeBuilder] ✅ Objects placed (torches, chests, enemies, items)
   ```

3. **Verify torches:**
   - Torches should be placed on walls
   - Flame particles should be animated
   - No errors in console

---

## ⚠️ Your Action Required

**Could you please run:**
```powershell
.\backup.ps1
```

**Then test in Unity:**
1. Press Play with CompleteMazeBuilder scene
2. Check Console for no TorchPool errors
3. Verify torches are placed correctly on walls
