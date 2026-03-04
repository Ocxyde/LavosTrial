# Torch Prefab Setup Guide - Manual Positioning
**Date:** 2026-03-03  
**Unity Version:** 6000.3.7f1

---

## 🎯 MANUAL PREFAB SETUP

**You can manually adjust the torch prefab in Unity Editor!**

---

## 📦 PREFAB STRUCTURE

```
TorchPrefab (Root)
├── Handle (Mesh/Visual)
│   └── Position: (0, 0, 0)
│   └── Rotation: (0, 0, 0)
│   └── Scale: (1, 1, 1)
│
├── FlameSocket (Empty GameObject) ← Position flame here
│   └── Position: (0, Y, 0) ← ADJUST Y for flame height
│   └── Rotation: (0, 0, 0)
│   └── Scale: (1, 1, 1)
│
├── BraseroFlame (Particle System)
│   └── Parent: FlameSocket
│   └── Position: (0, 0, 0) [local]
│   └── Rotation: (0, 0, 0)
│   └── Scale: (1, 1, 1)
│
└── FlameLight (Point Light)
    └── Parent: FlameSocket
    └── Position: (0, 0, 0) [local]
    └── Rotation: (0, 0, 0)
    └── Scale: (1, 1, 1)
```

---

## 🔧 MANUAL ADJUSTMENT STEPS

### In Unity Editor:

**1. Open Torch Prefab:**
```
- Find your torch prefab in Project window
- Double-click to open Prefab Mode
```

**2. Select FlameSocket (or create it):**
```
- Right-click on TorchPrefab root
- Create Empty → Name it "FlameSocket"
```

**3. Adjust FlameSocket Position:**
```
Position Y Values to Try:
- Y = 0.3f → Low flame
- Y = 0.5f → Medium flame (recommended)
- Y = 0.7f → High flame
- Y = 1.0f → Very high flame

Start with Y = 0.5f, then adjust in Play mode!
```

**4. Move Particle System:**
```
- Select BraseroFlame
- Set Parent to FlameSocket
- Set Local Position to (0, 0, 0)
```

**5. Move Point Light:**
```
- Select FlameLight
- Set Parent to FlameSocket
- Set Local Position to (0, 0, 0)
```

**6. Save Prefab:**
```
- Click "Save" in Prefab toolbar
- Exit Prefab Mode
```

---

## 🎨 FLAME POSITION TESTING

### Quick Test in Play Mode:

**1. Create Debug Script:**
```csharp
// FlamePositionDebugger.cs
using UnityEngine;

public class FlamePositionDebugger : MonoBehaviour
{
    [Header("Adjust in Play Mode")]
    public float flameHeight = 0.5f;
    
    [Header("References")]
    public Transform flameSocket;
    
    void Update()
    {
        if (flameSocket != null)
        {
            flameSocket.localPosition = new Vector3(0, flameHeight, 0);
            
            // Show position in Scene view
            Debug.DrawLine(transform.position, 
                          transform.position + Vector3.up * flameHeight, 
                          Color.orange, 0.1f);
        }
    }
}
```

**2. Adjust in Real-time:**
```
- Add script to torch prefab
- Press Play
- Adjust flameHeight slider
- Watch flame move up/down
- Note the best value
- Stop Play and set permanently
```

---

## 📊 RECOMMENDED COORDINATES

### For Standard Torch (1 unit tall):

```
Handle:
  Position: (0, 0, 0)
  Rotation: (0, 0, 0)
  Scale: (1, 1, 1)

FlameSocket:
  Position: (0, 0.5f, 0)  ← Halfway up the handle
  Rotation: (0, 0, 0)
  Scale: (1, 1, 1)

BraseroFlame (Particle):
  Local Position: (0, 0, 0)  ← Relative to socket
  Local Rotation: (0, 0, 0)
  Local Scale: (1, 1, 1)

FlameLight:
  Local Position: (0, 0, 0)  ← Relative to socket
  Local Rotation: (0, 0, 0)
  Local Scale: (1, 1, 1)
```

---

## 🎯 DIFFERENT TORCH TYPES

### Wall Torch (Short):
```
FlameSocket Y: 0.3f
Handle Length: 0.6f
Light Range: 7f
```

### Standing Torch (Tall):
```
FlameSocket Y: 1.5f
Handle Length: 2.0f
Light Range: 12f
```

### Brazier (Wide):
```
FlameSocket Y: 0.8f
Handle Width: 1.0f
Light Range: 10f
```

### Chandelier (Hanging):
```
FlameSocket Y: -0.5f (below parent)
Handle: Chain/rope
Light Range: 15f
```

---

## 🔍 TROUBLESHOOTING

### Flame Too Low:
```
Solution: Increase FlameSocket Y position
Try: 0.3f → 0.5f → 0.7f
```

### Flame Too High:
```
Solution: Decrease FlameSocket Y position
Try: 0.7f → 0.5f → 0.3f
```

### Flame Offset (Not Centered):
```
Solution: Reset X and Z to 0
Check: FlameSocket localPosition = (0, Y, 0)
```

### Particles Not Visible:
```
Solution: 
1. Check BraseroFlame component exists
2. Check particle system is playing
3. Check material is assigned (orange, not pink)
```

### Light Not Working:
```
Solution:
1. Check FlameLight component exists
2. Check light is enabled
3. Check light color is orange (not pink)
4. Check light range is sufficient (7f+)
```

---

## 💾 PREFAB VARIANTS

### Create Multiple Torch Prefabs:

**1. Torch_Wall (Short):**
```
- FlameSocket Y: 0.3f
- For dungeon walls
- Light range: 7f
```

**2. Torch_Standing (Tall):**
```
- FlameSocket Y: 1.5f
- For corridor standing torches
- Light range: 12f
```

**3. Torch_Brazier (Wide):**
```
- FlameSocket Y: 0.8f
- For large rooms
- Light range: 10f
```

**4. Torch_Chandelier (Hanging):**
```
- FlameSocket Y: -0.5f
- For ceiling hanging
- Light range: 15f
```

---

## ✅ CHECKLIST

**Prefab Setup:**
- [ ] Handle mesh/visual in place
- [ ] FlameSocket created at (0, 0, 0)
- [ ] FlameSocket Y adjusted (0.3f - 0.7f)
- [ ] BraseroFlame parented to FlameSocket
- [ ] BraseroFlame local position (0, 0, 0)
- [ ] FlameLight parented to FlameSocket
- [ ] FlameLight local position (0, 0, 0)
- [ ] All rotations reset to (0, 0, 0)
- [ ] All scales set to (1, 1, 1)
- [ ] Prefab saved

**Material Check:**
- [ ] Particle material is orange (not pink)
- [ ] Light color is orange (1f, 0.7f, 0.3f)
- [ ] Emission color is orange (1f, 0.7f, 0.3f)

**Plug-in System:**
- [ ] TorchController component on root
- [ ] Auto-registers with LightEngine
- [ ] TurnOn() / TurnOff() work
- [ ] OnDestroy() cleans up

---

## 🎮 IN-GAME TESTING

**1. Place Torch in Scene:**
```
- Drag torch prefab into scene
- Position on wall
- Rotate to face outward
```

**2. Test in Play Mode:**
```
- Press Play
- Torch should ignite (first torch ON)
- Press [T] for next torch
- Check flame position
- Adjust if needed
```

**3. Fine-Tune:**
```
- If flame too low/high → Adjust FlameSocket Y
- If flame not visible → Check rotation
- If light wrong color → Check material
```

---

**Now you can manually adjust the torch prefab to perfection! 🔥✨**

**Take your time to get the flame position just right!**
