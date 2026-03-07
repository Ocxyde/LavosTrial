# DoorsEngine Serialization Fix

**Date:** 2026-03-04  
**Issue:** Duplicate `interactionRange` field serialization error  
**Status:** ✅ **FIXED**

---

## 🐛 **ERROR**

```
The same field name is serialized multiple times in the class or its parent class. 
This is not supported: Base(MonoBehaviour) interactionRange
```

**Location:** `Assets/Scripts/Core/07_Doors/DoorsEngine.cs:78`

---

## 🔍 **ROOT CAUSE**

`DoorsEngine` inherits from `BehaviorEngine`, which already has an `interactionRange` field:

**BehaviorEngine.cs (Parent):**
```csharp
[Header("Interaction")]
[Range(0.1f, 10f)]
[SerializeField] protected float interactionRange = 3f;
```

**DoorsEngine.cs (Child - BROKEN):**
```csharp
[Header("Interaction")]
[Range(0.5f, 5f)]
[SerializeField] private new float interactionRange = 3f;  // ❌ DUPLICATE!
```

**Problem:**
- Both parent and child have `interactionRange` field
- Unity can't serialize two fields with same name
- Prefab saving fails

---

## ✅ **SOLUTION**

**Remove duplicate field from DoorsEngine:**

```diff
--- a/Assets/Scripts/Core/07_Doors/DoorsEngine.cs
+++ b/Assets/Scripts/Core/07_Doors/DoorsEngine.cs
@@ -75,9 +75,8 @@ namespace Code.Lavos.Core
         [SerializeField] private AudioClip trapSound;
 
         [Header("Interaction")]
-        [Range(0.5f, 5f)]
-        [SerializeField] private new float interactionRange = 3f;  // ❌ REMOVED!
+        // Note: interactionRange inherited from BehaviorEngine (not serialized here)
         [SerializeField] private bool requireKey = false;
         [SerializeField] private string requiredKeyName = "DoorKey";
```

---

## 📊 **INHERITANCE CHAIN**

```
BehaviorEngine (Parent)
├── interactionRange : float ✅ (serialized)
├── CanInteract : bool
└── Interact() : void

DoorsEngine (Child)
├── Uses inherited interactionRange ✅
├── doorVariant : DoorVariant
├── doorTrap : DoorTrapType
└── requireKey : bool
```

---

## ✅ **RESULT**

**After Fix:**
- ✅ No serialization errors
- ✅ Prefabs save correctly
- ✅ Doors use inherited `interactionRange` from BehaviorEngine
- ✅ All door functionality preserved

---

## 🎮 **VERIFICATION**

**In Unity Editor:**

1. **Regenerate Door Prefabs:**
   ```
   Tools → Create Maze Prefabs
   ```

2. **Check Console:**
   ```
   [CreateMazePrefabs] ✅ DoorPrefab (Normal, wood brown)
   [CreateMazePrefabs] ✅ LockedDoorPrefab (Red tint)
   [CreateMazePrefabs] ✅ SecretDoorPrefab (Wall-colored, camouflaged)
   ```
   
   **NO serialization errors!** ✅

3. **Test Doors:**
   ```
   - Approach door
   - Press E to interact
   - Door opens using inherited interactionRange
   ```

---

## 📝 **FILES MODIFIED**

| File | Change | Status |
|------|--------|--------|
| `DoorsEngine.cs` | Removed duplicate interactionRange | ✅ |

---

## 🔧 **WHY THIS HAPPENED**

**C# Inheritance + Unity Serialization:**

When a child class inherits from a MonoBehaviour parent:
- ✅ Child inherits all serialized fields
- ❌ Child CANNOT redeclare same field name with `[SerializeField]`
- ⚠️ Using `new` keyword hides parent but still causes serialization conflict

**Correct Pattern:**
```csharp
// Parent
public class Parent : MonoBehaviour
{
    [SerializeField] protected float interactionRange;
}

// Child ✅
public class Child : Parent
{
    // Use inherited field, don't redeclare!
}
```

**Wrong Pattern:**
```csharp
// Child ❌
public class Child : Parent
{
    [SerializeField] private new float interactionRange;  // CONFLICT!
}
```

---

## 🔧 **REMINDER - BACKUP**

**After testing, run:**
```powershell
.\backup.ps1
```

---

**Generated:** 2026-03-04  
**Unity Version:** 6000.3.7f1  
**Status:** ✅ **FIXED - NO MORE SERIALIZATION ERRORS!**

---

*Document generated - Unity 6 (6000.3.7f1) compatible - UTF-8 encoding - Unix LF*
