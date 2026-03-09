%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
---
# SPECIFIC CODE FIXES - MazeLav8s_v1-0_1_4 DEBUG SESSION
## Applied Fixes for Scene Debugging
## Code: BetsyBoop
## Date: 2026-03-09
---

## FILE 1: Collectible.cs

**Location:** `Assets/Scripts/Gameplay/Collectible.cs`

```diff
- namespace Code.Lavos.Core
+ namespace Code.Lavos.Gameplay
{
    public class Collectible : MonoBehaviour
    {
        // Class content unchanged
    }
}
```

---

## FILE 2: InteractableObject.cs

**Location:** `Assets/Scripts/Interaction/InteractableObject.cs`

```diff
- namespace Code.Lavos.Core
+ namespace Code.Lavos.Interaction
{
    public class InteractableObject : MonoBehaviour
    {
        // Class content unchanged
    }
}
```

---

## FILE 3: IInteractable.cs

**Location:** `Assets/Scripts/Interaction/IInteractable.cs`

```diff
- namespace Code.Lavos.Core
+ namespace Code.Lavos.Interaction
{
    public interface IInteractable
    {
        // Interface content unchanged
    }
}
```

---

## FILE 4: PersistentUI.cs

**Location:** `Assets/Scripts/HUD/PersistentUI.cs`

```diff
- namespace Code.Lavos.Core
+ namespace Code.Lavos.HUD
{
    public class PersistentUI : MonoBehaviour
    {
        // Class content unchanged
    }
}
```

---

## FILE 5: TorchDiagnostics.cs

**Location:** `Assets/Scripts/Ressources/TorchDiagnostics.cs`

```diff
- namespace Code.Lavos.Core
+ namespace Code.Lavos.Ressources
{
    public class TorchDiagnostics : MonoBehaviour
    {
        // Class content unchanged
    }
}
```

---

## FILE 6: RoomTextureGenerator.cs

**Location:** `Assets/Scripts/Ressources/RoomTextureGenerator.cs`

```diff
- namespace Code.Lavos.Core
+ namespace Code.Lavos.Ressources
{
    public class RoomTextureGenerator : MonoBehaviour
    {
        // Class content unchanged
    }
}
```

---

## FILE 7: PixelArtTextureFactory.cs

**Location:** `Assets/Scripts/Ressources/PixelArtTextureFactory.cs`

```diff
- namespace Code.Lavos.Core
+ namespace Code.Lavos.Ressources
{
    public class PixelArtTextureFactory
    {
        // Class content unchanged
    }
}
```

---

## FILE 8: PixelArtGenerator.cs

**Location:** `Assets/Scripts/Ressources/PixelArtGenerator.cs`

```diff
- namespace Code.Lavos.Core
+ namespace Code.Lavos.Ressources
{
    public class PixelArtGenerator
    {
        // Class content unchanged
    }
}
```

---

## FILE 9: Lav8s_PixelArt8Bit.cs

**Location:** `Assets/Scripts/Ressources/Lav8s_PixelArt8Bit.cs`

```diff
- namespace Code.Lavos.Core
+ namespace Code.Lavos.Ressources
{
    public class Lav8s_PixelArt8Bit
    {
        // Class content unchanged
    }
}
```

---

## FILE 10: DoorFactory.cs

**Location:** `Assets/Scripts/Ressources/DoorFactory.cs`

```diff
- namespace Code.Lavos.Core
+ namespace Code.Lavos.Ressources
{
    public class DoorFactory
    {
        // Class content unchanged
    }
}
```

---

## FILE 11: ChestPixelArtFactory.cs

**Location:** `Assets/Scripts/Ressources/ChestPixelArtFactory.cs`

```diff
- namespace Code.Lavos.Core
+ namespace Code.Lavos.Ressources
{
    public class ChestPixelArtFactory
    {
        // Class content unchanged
    }
}
```

---

## FILE 12: AnimatedFlame.cs

**Location:** `Assets/Scripts/Ressources/AnimatedFlame.cs`

```diff
- namespace Code.Lavos.Core
+ namespace Code.Lavos.Ressources
{
    public class AnimatedFlame : MonoBehaviour
    {
        // Class content unchanged
    }
}
```

---

## FILE 13: StatusEffect.cs (if in Player folder)

**Location:** `Assets/Scripts/Player/StatusEffect.cs` (if exists)  
**Note:** Already exists in `Assets/Scripts/Status/StatusEffect.cs` with correct namespace

---

## FOLDER RENAMES REQUIRED

Execute in PowerShell or file explorer:

```powershell
# Navigate to Assets/Scripts/Core

# Rename folders in sequence
Rename-Item -Path "10_Resources" -NewName "11_Resources"
Rename-Item -Path "11_Utilities" -NewName "12_Utilities"
Rename-Item -Path "12_Animation" -NewName "13_Animation"
Rename-Item -Path "13_Compute" -NewName "14_Compute"

# After renaming: Close Unity completely, then reopen
```

---

## HUD SYSTEM REFACTORING - Example (UIBarsSystem.cs)

**Location:** `Assets/Scripts/HUD/UIBarsSystem.cs`

```diff
- public void InitializeHealthBar()
- {
-     GameObject healthBar = new GameObject("HealthBar");
-     Image image = healthBar.AddComponent<Image>();
-     RectTransform rect = healthBar.AddComponent<RectTransform>();
-     rect.sizeDelta = new Vector2(200, 20);
-     image.color = Color.red;
- }

+ public void InitializeHealthBar()
+ {
+     // Load prefab from Resources
+     GameObject prefab = Resources.Load<GameObject>("Prefabs/HUD/HealthBar");
+     if (prefab == null)
+     {
+         Debug.LogError("HealthBar prefab not found!");
+         return;
+     }
+
+     // Instantiate from prefab
+     GameObject healthBar = Instantiate(prefab);
+     
+     // Get existing components (don't create new ones)
+     Image image = healthBar.GetComponent<Image>();
+     RectTransform rect = healthBar.GetComponent<RectTransform>();
+     
+     if (image == null || rect == null)
+     {
+         Debug.LogError("HealthBar prefab missing required components!");
+     }
+ }
```

**Process for all HUD files:**

1. Identify all `new GameObject()` calls
2. Create corresponding prefabs in `Assets/Resources/Prefabs/HUD/`
3. Replace calls with prefab loading
4. Replace all `AddComponent<>()` with `GetComponent<>()`
5. Test in scene

---

## TRIANGLE.cs EMOJI REMOVAL

**Location:** `Assets/Scripts/Core/14_Geometry/Triangle.cs`

```diff
- /// ✅ Area calculation (Heron's formula and 3D cross product)
- /// ✅ Centroid calculation
- /// ✅ Circumcenter calculation
+ /// Area calculation - IMPLEMENTED (Heron's formula and 3D cross product)
+ /// Centroid calculation - IMPLEMENTED
+ /// Circumcenter calculation - IMPLEMENTED
```

---

## SHARESYSTM.CS RENAME

**Current Path:** `Assets/Scripts/Core/11_Utilities/ShareSystm.cs`  
**New Path:** `Assets/Scripts/Core/12_Utilities/ShareSystem.cs` (after folder rename)

```diff
- public class ShareSystm
+ public class ShareSystem
{
    // Content unchanged
}
```

---

## DIFF SUMMARY

| File | Type | Change |
|------|------|--------|
| 12 files | Namespace | Code.Lavos.Core → Correct assembly |
| 1 file | Rename | ShareSystm.cs → ShareSystem.cs |
| 1 file | Cleanup | Remove emoji from comments |
| 6 folders | Rename | Fix numbering conflicts |
| 6 files | Refactor | HUD prefab usage (complex) |

---

## IMPLEMENTATION ORDER

### Step 1: Namespace Fixes (15 minutes)
1. Edit each file's namespace declaration
2. Save all files
3. Unity recompiles automatically

### Step 2: Folder Renames (5 minutes)
1. Close Unity
2. Rename folders in file system
3. Open Unity (wait for reimport)

### Step 3: Minor Cleanup (10 minutes)
1. Remove emoji from Triangle.cs
2. Rename ShareSystm.cs
3. Save and verify

### Step 4: HUD Refactoring (2-3 hours)
1. Create HUD prefabs
2. Refactor each HUD file
3. Test in scene after each file

---

## VERIFICATION COMMANDS

After each fix, check in Unity console:

```csharp
// Check compilation
Debug.Log("[BetsyBoop] Compilation check");

// Check namespaces are correct
using Code.Lavos.Gameplay;
using Code.Lavos.Interaction;
using Code.Lavos.Ressources;
// etc.

// Verify prefabs load
GameObject prefab = Resources.Load<GameObject>("Prefabs/WallPrefab");
Debug.Log(prefab ? "Prefab loaded: OK" : "Prefab NOT found");
```

---

## GIT COMMIT MESSAGES

```bash
git commit -m "fix: correct namespace declarations in 12 files

- Collectible.cs: Code.Lavos.Core -> Code.Lavos.Gameplay
- InteractableObject.cs: Code.Lavos.Core -> Code.Lavos.Interaction
- [etc for each file]

This fixes assembly reference issues and improves code organization."

git commit -m "fix: rename directories to resolve numbering conflicts

- 10_Resources -> 11_Resources
- 11_Utilities -> 12_Utilities
- 12_Animation -> 13_Animation
- 13_Compute -> 14_Compute

This ensures proper Unity import order and prevents reference resolution issues."

git commit -m "refactor: convert HUD system to use prefabs

- UIBarsSystem.cs: Replace new GameObject() with prefab instantiation
- PopWinEngine.cs: Replace new GameObject() with prefab instantiation
- [etc for each file]

This restores plug-in-out system compliance and improves performance."
```

---

## DEBUGGING: If Scene Still Has Issues

**Console Check:**
```
Window > General > Console
Look for: [MazeBuilder8] messages
Look for: Any red error messages
Look for: CS errors (shouldn't happen)
```

**Scene Hierarchy Check:**
- Main Camera present?
- GameConfig GameObject present?
- CompleteMazeBuilder8 component attached?
- AutoMazeSetup component attached?

**Prefab Check:**
```
Assets > Resources > Prefabs > ?
Check folders exist:
- HUD/
- Walls/
- Doors/
- Objects/
```

---

## POST-FIX TESTING

Run in scene:

1. Click Play button
2. Check console for generation logs
3. Verify maze generates
4. Check walls appear
5. Check floor renders
6. Check player spawns
7. Check HUD displays
8. Stop Play
9. Check for errors

---

**Document Status:** IMPLEMENTATION READY  
**Encoding:** UTF-8 Unix LF  
**Last Updated:** 2026-03-09  
**Codename:** BetsyBoop

---
