// Safe_Prefab_Setup.md
// Safe Prefab Implementation Guide - 8-Bit Pixel Art Edition
// Unity 6000.10f1 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Complete setup and usage documentation for SafePrefab with animation and treasure system

# Safe Prefab Setup Guide - 8-Bit Pixel Art

**Project:** CodeDotLavos / PeuImporte  
**Unity Version:** 6000.10f1  
**Architecture:** Plug-in-Out  
**License:** GPL-3.0  
**Last Updated:** 2026-03-10  

---

## Overview

This guide covers the complete implementation of a **SafePrefab** with:
- Interactive treasure chest with F-key interaction
- Smooth lid lifting animation (90-degree rotation)
- Item/treasure distribution via EventHandler
- Auto-destruction after 60 seconds of interaction
- 8-bit pixel art material setup

---

## Architecture

### System Components

```
SafePrefab (GameObject Root)
├── SafeController.cs (Orchestrator)
│   ├── Handles F-key interaction detection
│   ├── Manages 60-second destruction timer
│   └── Broadcasts events via EventHandler
│
├── SafeAnimationController.cs (Animation)
│   ├── Manages lid rotation/elevation
│   ├── Smooth easing (configurable)
│   └── Duration: 1.5 seconds (configurable)
│
├── SafeItemContainer.cs (Inventory)
│   ├── Stores treasure items
│   ├── Distributes via EventHandler.OnItemPickedUp
│   └── Supports rarity system
│
└── SafeInteractionTrigger.cs (Detection)
    ├── Player proximity detection
    ├── Tag-based player finding
    └── Configurable range (default: 3 units)
```

### Plug-in-Out Compliance

All components use **FindFirstObjectByType<T>()** to locate:
- EventHandler (for item distribution)
- Player GameObject (via "Player" tag)

**No hard dependencies** → Easy to extend/modify

---

## File Placement

Place files in your project structure:

```
Assets/
├── Scripts/
│   └── Code.Lavos.Interactables/
│       ├── SafeController.cs
│       ├── SafeAnimationController.cs
│       ├── SafeItemContainer.cs
│       └── SafeInteractionTrigger.cs
│
├── Prefabs/
│   └── Interactables/
│       └── SafePrefab.prefab (you'll create this)
│
├── Materials/
│   └── Interactables/
│       └── Safe_PixelArt.mat
│
└── Docs/
    └── Safe_Prefab_Setup.md (this file)
```

---

## Step 1: Create Safe Model (3D Mesh)

### Option A: Manual 3D Model (Recommended)

Create a simple safe box model in Blender or 3D modeling tool:

**Base Safe (Main Body):**
- Cube: 2 units wide × 2.5 units tall × 1.5 units deep
- Lock detail on front face

**Lid (Top):**
- Cube: 2 units wide × 0.5 units tall × 1.5 units deep
- **Position:** Top of safe body
- **Tag/Name:** "Lid" (critical for SafeAnimationController)

**Export as FBX** → `Assets/Models/SafePrefab.fbx`

### Option B: Procedural (Quick Testing)

Use Unity primitives:

1. Create empty GameObject: `SafeRoot`
2. Add child Cube, scale to 2 × 2.5 × 1.5 → **SafeBody**
3. Add child Cube, scale to 2 × 0.5 × 1.5, position at Y=1.25 → **Lid**

---

## Step 2: Create 8-Bit Pixel Art Material

### Texture Setup

Create or find an 8-bit pixel art texture: `safe_texture.png`

**Recommended palette (8-bit):**
```
Wood/Metal colors:
#8B4513  Brown base
#A0522D  Light brown
#654321  Dark brown
#D2B48C  Gold/brass detail

Stone/Gray:
#808080  Medium gray
#606060  Dark gray
#A9A9A9  Light gray
```

### Import Settings

1. Select `safe_texture.png` in Project
2. Inspector → Texture Type: **Sprite (2D and UI)**
3. Filter Mode: **Point (no filter)** ← CRITICAL for pixel art
4. Compression: **None**
5. Format: **Truecolor (32 bit)**
6. Pixels Per Unit: **16**
7. Click **Apply**

### Create Material

1. Right-click `Assets/Materials/Interactables/` → Create → Material
2. Name: `Safe_PixelArt`
3. Shader: **Universal Render Pipeline → 2D → Sprite Unlit**
4. Base Map: `safe_texture.png`
5. Base Color: RGBA(255, 255, 255, 255) ← White (texture provides color)
6. Surface Type: **Opaque**
7. Pixel Snap: **On**

---

## Step 3: Create SafePrefab

### Manual Setup (Once Per Project)

1. Create new empty GameObject in your scene: `SafePrefab_Working`

2. **Attach Scripts (in order):**
   - Add Component → SafeInteractionTrigger
   - Add Component → SafeAnimationController
   - Add Component → SafeItemContainer
   - Add Component → SafeController

3. **Assign the Safe Model:**
   - Drag `SafeRoot` (or FBX) as child of SafePrefab_Working
   - Assign material `Safe_PixelArt` to all meshes

4. **Configure SafeAnimationController:**
   - Inspector → Lid Transform: Select the "Lid" child object
   - Lid Open Rotation: X=90, Y=0, Z=0 (lid lifts up)
   - Animation Duration: 1.5 seconds
   - Easing Curve: EaseInOut (smooth acceleration/deceleration)

5. **Configure SafeItemContainer:**
   - Treasure Items: Add items via inspector (see below)

6. **Configure SafeController:**
   - Destruction Delay: 60 seconds
   - Debug Mode: Toggle for testing

7. **Configure SafeInteractionTrigger:**
   - Interaction Range: 3 units
   - Player GameObject must have tag "Player"

8. **Save as Prefab:**
   - Drag SafePrefab_Working into `Assets/Prefabs/Interactables/`
   - Name: `SafePrefab.prefab`

---

## Step 4: Add Treasure Items

### Via Inspector

In SafePrefab Inspector → SafeItemContainer component:

1. Treasure Items: Click "+" to add items
2. For each item, set:
   - **Item ID:** "gold_coin" (unique identifier)
   - **Item Name:** "Gold Coin" (display name)
   - **Quantity:** 10 (how many)
   - **Rarity:** 0.5 (0.0 = common, 1.0 = legendary)

**Example Treasure Setup:**
```
Item 1:
  ID: gold_coin
  Name: Gold Coin
  Quantity: 25
  Rarity: 0.3

Item 2:
  ID: emerald_gem
  Name: Emerald Gem
  Quantity: 3
  Rarity: 0.8

Item 3:
  ID: ancient_scroll
  Name: Ancient Scroll
  Quantity: 1
  Rarity: 0.95
```

### Via Code (Optional)

```csharp
SafeItemContainer container = GetComponent<SafeItemContainer>();
container.AddTreasureItem("gold_coin", "Gold Coin", 25, 0.3f);
container.AddTreasureItem("emerald_gem", "Emerald Gem", 3, 0.8f);
```

---

## Step 5: EventHandler Integration

### Update EventHandler.cs

Add this event (if not already present):

```csharp
public class EventHandler : MonoBehaviour
{
    // Existing events...
    
    // Safe Events
    public event System.Action OnSafeOpened;
    
    // Call from SafeController when safe opens
    public void NotifySafeOpened()
    {
        OnSafeOpened?.Invoke();
    }
}
```

### Hook into Safe Events

Listen for safe opening in other systems:

```csharp
// In your UI or quest system
private void OnEnable()
{
    if (EventHandler.Instance != null)
    {
        EventHandler.Instance.OnSafeOpened += OnSafeOpened;
        EventHandler.Instance.OnItemPickedUp += OnItemPickedUp;
    }
}

private void OnSafeOpened()
{
    Debug.Log("Safe was opened!");
    // Trigger quest update, play sound, etc.
}

private void OnItemPickedUp(string itemId, int quantity)
{
    Debug.Log($"Picked up {quantity}x {itemId}");
}
```

---

## Step 6: Placement via SpatialPlacer (Future)

### Current Status

SafePrefab is **manually placed** in scenes. SpatialPlacer integration coming soon.

### Manual Placement

1. Drag `SafePrefab.prefab` into your scene
2. Position at desired location
3. Run the scene and press F near the safe

---

## Interaction Flow

```
Player enters range (3 units)
    ↓
Player presses F key
    ↓
SafeController.OnPlayerInteraction()
    ├── Plays open animation (SafeAnimationController)
    ├── Distributes items (SafeItemContainer → EventHandler)
    ├── Broadcasts OnSafeOpened event
    └── Starts 60-second destruction timer
    ↓
60 seconds elapse
    ↓
SafeController.DestroySafe()
    └── gameObject.Destroy()
```

---

## Configuration Reference

### SafeController

| Property | Default | Range | Purpose |
|----------|---------|-------|---------|
| Destruction Delay | 60.0 | 1-300 | Seconds before safe disappears |
| Debug Mode | false | - | Log all events to console |

### SafeAnimationController

| Property | Default | Range | Purpose |
|----------|---------|-------|---------|
| Lid Open Rotation | (90, 0, 0) | - | Target rotation (degrees) |
| Animation Duration | 1.5 | 0.1-5.0 | Animation length (seconds) |
| Easing Curve | EaseInOut | - | Smooth acceleration |
| Debug Mode | false | - | Log animation events |

### SafeItemContainer

| Property | Default | Range | Purpose |
|----------|---------|-------|---------|
| Treasure Items | [] | - | List of items to distribute |
| Debug Mode | false | - | Log distribution events |

### SafeInteractionTrigger

| Property | Default | Range | Purpose |
|----------|---------|-------|---------|
| Interaction Range | 3.0 | 1-20 | Distance to player (units) |
| Debug Mode | false | - | Log proximity events |

---

## Troubleshooting

### Safe doesn't respond to F key

**Check:**
1. Player GameObject has tag "Player"
2. SafeInteractionTrigger is enabled
3. Player is within 3 units of safe
4. Keyboard.current is not null (New Input System active)

**Test:** Enable Debug Mode on SafeController

### Lid doesn't animate

**Check:**
1. Lid Transform is assigned in SafeAnimationController inspector
2. Lid is a child of SafeRoot
3. Lid Open Rotation is set to (90, 0, 0)

**Test:** Press F and check console for "[SafeAnimationController] Starting lid open animation"

### Items not distributed

**Check:**
1. EventHandler exists in scene
2. OnItemPickedUp event is not null
3. Treasure Items list has items configured

**Test:** Enable Debug Mode on SafeItemContainer

### Safe disappears instantly

**Check:**
1. Destruction Delay is set to 60 (not 0)
2. No other scripts are destroying the safe

---

## Performance Notes

- Each safe has 4 components (lightweight)
- Animation uses simple Lerp (O(1) performance)
- No physics processing required
- Recommended: 10-20 safes per scene max

---

## Future Enhancements

- [ ] Integration with SpatialPlacer for procedural placement
- [ ] Safe locking/unlocking via key items
- [ ] Visual/audio feedback during animation
- [ ] Configurable destruction delay per instance
- [ ] Safe respawning after time delay
- [ ] Multiplayer synchronization (networked safes)

---

## License

This prefab system is part of **CodeDotLavos** and licensed under **GPL-3.0**.

See COPYING file for full license text.

---

## References

- Architecture Overview: `Assets/Docs/ARCHITECTURE_OVERVIEW.md`
- Project Standards: `Assets/Docs/PROJECT_STANDARDS.md`
- Pixel Art Setup: `Assets/Docs/PIXEL_ART_MATERIAL_SETUP.md`

---

**Status:** Ready for implementation  
**Created:** 2026-03-10  
**Unity Version:** 6000.10f1  
**Author:** BetsyBoop  

*Document generated - Unity 6 (6000.10f1) compatible - UTF-8 encoding - Unix LF*
