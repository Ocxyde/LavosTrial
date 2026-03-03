# Manual Torch Activation System
**Date:** 2026-03-03  
**Unity Version:** 6000.3.7f1

---

## ✅ WHAT CHANGED

### Torches now start **OFF** at scene start
### You must **manually activate** each torch one by one

---

## 🎯 TWO ACTIVATION METHODS

### Method 1: Use TorchManualActivator (Recommended)

**Add Component:** `TorchManualActivator.cs`

**Controls:**
```
[F] - Activate next torch (one by one)
[H] - Activate ALL torches at once
[X] - Turn OFF all torches
```

**Setup:**
```csharp
// In Unity Editor:
1. Select your "MazeTest" GameObject
2. Add Component: TorchManualActivator
3. Assign LightPlacementEngine reference (or auto-find)
4. Press Play
5. Press [F] to activate torches one by one!
```

**Debug UI:**
```
Shows in Game view (top-left):
[TorchManualActivator] Total Torches: 40
[TorchManualActivator] Activated: 5
[TorchManualActivator] Remaining: 35

Controls:
[F] - Activate next torch
[H] - Activate all torches
[X] - Turn off all torches
```

---

### Method 2: Use LightPlacementEngine Context Menu

**In Unity Editor:**
```
1. Select GameObject with LightPlacementEngine
2. Right-click component
3. Choose:
   - "Turn On All Torches"
   - "Turn Off All Torches"
```

**Or via Script:**
```csharp
var engine = FindObjectOfType<LightPlacementEngine>();
engine.SetTorchState("torch_0000", true);  // Turn on specific torch
engine.TurnOnAllTorches();                  // Turn on all
engine.TurnOffAllTorches();                 // Turn off all
```

---

## 📊 TORCH STATE AT SCENE START

### Before:
```
❌ Torches start ON automatically
❌ All torches light up at once
❌ No manual control
```

### After:
```
✅ Torches start OFF (dark)
✅ Manual activation required
✅ Full control over each torch
✅ Can activate one by one or all at once
```

---

## 🔧 FILES MODIFIED

### 1. LightPlacementEngine.cs

**Changes:**
- `startOn = false` (default OFF)
- Removed gradual activation system
- Added `TurnOnAllTorches()` context menu
- Added `TurnOffAllTorches()` context menu
- All torches start OFF for manual activation

**Lines:** -30 (cleanup) +20 (manual control) = **-10 lines**

---

### 2. TorchManualActivator.cs (NEW)

**Purpose:** Helper script for manual torch activation

**Features:**
- [F] Activate next torch
- [H] Activate all torches
- [X] Turn off all torches
- Debug UI overlay
- Auto-finds LightPlacementEngine

**Lines:** +193 (new file)

---

## 🧪 TESTING

### In Unity Editor:

**Step 1: Add TorchManualActivator**
```
1. Select "MazeTest" GameObject
2. Add Component: TorchManualActivator
3. Press Play
```

**Step 2: Test Controls**
```
1. All torches start OFF (dark maze)
2. Press [F] - First torch ignites 🔥
3. Press [F] again - Second torch ignites 🔥
4. Continue pressing [F] for each torch
5. Press [H] - All remaining torches ignite
6. Press [X] - All torches turn OFF
```

**Console Messages:**
```
✅ [TorchManualActivator] Ready to activate 40 torches manually
✅ [TorchManualActivator] Controls: [F] Next torch | [H] All torches | [X] Turn off all
✅ [TorchManualActivator] Torch 1/40 activated (torch_0000)
✅ [TorchManualActivator] Torch 2/40 activated (torch_0001)
...
```

---

## 🎮 GAMEPLAY USE CASES

### Atmospheric Exploration:
```
Player enters dark maze
→ Press [F] to light torches as you explore
→ Creates tension and atmosphere
→ Each torch ignites with sound/flame effect
```

### Puzzle Solving:
```
Player must light torches in specific pattern
→ Use SetTorchState() for puzzle logic
→ Light torch #5, then #12, then #23...
→ Unlit torches mark unexplored areas
```

### Performance Testing:
```
Test torch impact on performance
→ Start with all OFF
→ Press [H] to activate all at once
→ Monitor frame rate
→ Press [X] to compare with all OFF
```

---

## 💡 ADVANCED USAGE

### Script Example:

```csharp
public class TorchPuzzle : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Light torches near player
            var engine = FindObjectOfType<LightPlacementEngine>();
            engine.SetTorchState("torch_0005", true);
            engine.SetTorchState("torch_0006", true);
        }
    }
}
```

### Trigger-Based Activation:

```csharp
// Light torches when player enters room
void OnRoomEnter()
{
    for (int i = 0; i < 5; i++)
    {
        Invoke(nameof(LightNextTorch), i * 0.2f);
    }
}

void LightNextTorch()
{
    // Light torch logic
}
```

---

## 📋 CONTEXT MENU COMMANDS

### On LightPlacementEngine:

**Right-click component → Choose:**

1. **"Turn On All Torches"**
   - Instantly activates all torches
   - Useful for testing
   - Bypasses manual activation

2. **"Turn Off All Torches"**
   - Extinguishes all torches
   - Resets to initial state
   - Prepares for manual activation

---

## 🎨 VISUAL FEEDBACK

### Debug UI (Top-Left Corner):
```
┌─────────────────────────────────┐
│ [TorchManualActivator]          │
│ Total Torches:     40           │
│ Activated:         5            │
│ Remaining:         35           │
│                                 │
│ Controls:                       │
│ [F] - Activate next torch       │
│ [H] - Activate all torches      │
│ [X] - Turn off all torches      │
└─────────────────────────────────┘
```

### Console Log:
```
[TorchManualActivator] Torch 1/40 activated (torch_0000)
[TorchManualActivator] Torch 2/40 activated (torch_0001)
[TorchManualActivator] Torch 3/40 activated (torch_0002)
...
```

---

## 📁 FILE SUMMARY

| File | Status | Lines | Purpose |
|------|--------|-------|---------|
| `LightPlacementEngine.cs` | Modified | -10 | Manual control |
| `TorchManualActivator.cs` | NEW | +193 | Helper script |
| **Total** | | **+183** | |

---

## 💾 BACKUP & GIT

**After testing:**

```powershell
# Run backup
.\backup.ps1

# Commit
.\git-auto.bat "feat: Manual torch activation system"

# Push
git push
```

---

## ✅ SUCCESS CRITERIA

**Manual activation is working if:**

- ✅ All torches start OFF at scene start
- ✅ Press [F] activates next torch
- ✅ Press [H] activates all torches
- ✅ Press [X] turns off all torches
- ✅ Debug UI shows activation progress
- ✅ Console logs each activation
- ✅ Rotation: X 35° outward
- ✅ Particles: Orange 8-bit (no pink)

---

**Documentation Generated:** 2026-03-03  
**Status:** ✅ Ready for Testing  
**Activation:** ✅ Manual (one by one)  
**Rotation:** ✅ X 35° outward  
**Particles:** ✅ Orange 8-bit (no pink)

---

**Enjoy your manual torch lighting system! 🔥✨**
