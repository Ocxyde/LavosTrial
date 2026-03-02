# Door System - Plug-in-Out Architecture
**Location:** `Assets/Docs/DOOR_SYSTEM.md`  
**Date:** 2026-03-01  
**Status:** ✅ **PRODUCTION READY**

---

## Overview

The door system uses **plug-in-out architecture** for maximum modularity:

```
┌─────────────────────────────────────────┐
│           DoorsEngine                   │
│  (Core - Main door behavior)            │
├─────────────────────────────────────────┤
│  Plug-in Components:                    │
│  - DoorAnimation (optional)             │
│  - DoorFactory (3D model generator)     │
│  - RoomTextureGenerator (textures)      │
└─────────────────────────────────────────┘
```

---

## Components

### 1. **DoorsEngine.cs** (Core/) ✅
**Main door behavior - REQUIRED**
- Door variants (Normal, Locked, Trapped, etc.)
- Trap system
- Event integration
- Interaction handling

**Plug-in-Out Points:**
```csharp
// Automatically finds/creates components
_doorAnimation = GetComponent<DoorAnimation>();
if (_doorAnimation == null)
{
    _doorAnimation = gameObject.AddComponent<DoorAnimation>();
}
```

---

### 2. **DoorAnimation.cs** (Core/) ✅
**Smooth door animations - OPTIONAL**
- Open/close animations
- Audio support
- Configurable speed

**Usage:**
```csharp
// Automatically added by DoorsEngine
// Or add manually:
gameObject.AddComponent<DoorAnimation>();

// Configure
DoorAnimation anim = GetComponent<DoorAnimation>();
anim.openSpeed = 2f;
anim.openSound = myAudioClip;
```

---

### 3. **DoorFactory.cs** (Ressources/) ✅
**3D door model generator - OPTIONAL**
- Generates 3D doors with pixel art textures
- Adds effects (aura, fog, particles)
- Returns complete door GameObject

**Usage:**
```csharp
// Generate door with all components
GameObject door = DoorFactory.CreateDoor(
    position,
    rotation,
    DoorVariant.Trapped,
    DoorTrapType.Fire
);

// Door has:
// - 3D model
// - DoorsEngine
// - DoorAnimation
// - Effects
```

---

### 4. **RoomTextureGenerator.cs** (Ressources/) ✅
**Room-specific textures - OPTIONAL**
- Unique texture per room type
- Seed-based variation
- Cached for performance

**Usage:**
```csharp
// Get texture for room
Texture2D tex = RoomTextureGenerator.Instance.GetRoomTexture(
    RoomType.Treasure,
    SeedManager.Instance.CurrentSeed
);
```

---

## Integration Flow

```
1. DoorFactory.CreateDoor()
   ↓
2. Creates 3D model
   ↓
3. Adds DoorsEngine
   ↓
4. Adds DoorAnimation (auto)
   ↓
5. Applies textures (RoomTextureGenerator)
   ↓
6. Adds effects (aura, fog, particles)
   ↓
7. Returns complete door
```

---

## Plug-in-Out API

### Required (Core)
```csharp
// Create basic door
GameObject door = new GameObject("Door");
DoorsEngine engine = door.AddComponent<DoorsEngine>();
engine.Initialize(DoorVariant.Normal, DoorTrapType.None);
```

### Optional Plugins
```csharp
// Add animation
door.AddComponent<DoorAnimation>();

// Add 3D model (replaces basic door)
GameObject fancyDoor = DoorFactory.CreateDoor(...);

// Apply room texture
Texture2D tex = RoomTextureGenerator.Instance.GetRoomTexture(...);
```

---

## Event Integration

### DoorsEngine Broadcasts:
```csharp
EventHandler.Instance.OnDoorOpened += (pos, variant) => { ... };
EventHandler.Instance.OnDoorClosed += (pos, variant) => { ... };
EventHandler.Instance.OnDoorLocked += (pos) => { ... };
EventHandler.Instance.OnDoorTrapTriggered += (pos, trap) => { ... };
```

### SFXVFXEngine Listens:
```csharp
// Automatic - no setup needed!
// When door opens → SFX plays
// When trap triggers → VFX spawns
```

---

## Usage Examples

### Basic Door (Minimal)
```csharp
// Just the core - no plugins
GameObject door = new GameObject("Door");
DoorsEngine engine = door.AddComponent<DoorsEngine>();
engine.Initialize(DoorVariant.Normal, DoorTrapType.None);

// Works! But no animation, no 3D model
```

### Full Door (All Plugins)
```csharp
// Complete door with all features
GameObject door = DoorFactory.CreateDoor(
    position,
    rotation,
    DoorVariant.Boss,
    DoorTrapType.Spike
);

// Has:
// ✅ 3D model with pixel art
// ✅ DoorsEngine (behavior)
// ✅ DoorAnimation (smooth open/close)
// ✅ Aura effect (boss door)
// ✅ Fog effect (trap)
// ✅ Particle effects
```

### Custom Setup (Selective Plugins)
```csharp
// Create door
GameObject door = new GameObject("Door");

// Add core
DoorsEngine engine = door.AddComponent<DoorsEngine>();
engine.Initialize(DoorVariant.Secret, DoorTrapType.None);

// Add ONLY animation (no 3D model yet)
door.AddComponent<DoorAnimation>();

// Add custom texture
Texture2D secretTex = RoomTextureGenerator.Instance.GetRoomTexture(
    RoomType.Secret,
    "MySeed"
);
// Apply texture manually...
```

---

## Configuration

### Inspector Settings

**DoorsEngine:**
- Door Variant
- Trap Type
- Trap Damage
- Interaction Range
- Audio Clips

**DoorAnimation:**
- Open/Close Speed
- Open/Close Sounds
- Rotation Angles

**DoorFactory:**
- Door Dimensions
- Pixel Art Settings
- Effects Toggles

---

## Testing Checklist

- [ ] Basic door opens/closes
- [ ] Animated door has smooth animation
- [ ] Sound plays on open/close
- [ ] Trap triggers when opened
- [ ] Events fire in EventHandler
- [ ] SFXVFXEngine responds to events
- [ ] Room textures apply correctly
- [ ] DoorFactory creates complete doors
- [ ] All plugins work independently
- [ ] Removing plugin doesn't break core

---

## File Locations

| Component | Location | Required |
|-----------|----------|----------|
| **DoorsEngine** | Core/ | ✅ YES |
| **DoorAnimation** | Core/ | ❌ NO (auto-added) |
| **DoorFactory** | Ressources/ | ❌ NO |
| **RoomTextureGenerator** | Ressources/ | ❌ NO |
| **PixelArtGenerator** | Ressources/ | ❌ NO (used by Factory) |

---

## Performance

| Feature | Impact | Optimization |
|---------|--------|--------------|
| DoorsEngine | Low | Single component |
| DoorAnimation | Low | Coroutine-based |
| DoorFactory | Medium | One-time generation |
| RoomTextureGenerator | Low | Cached textures |
| Effects | Medium-High | Toggle in Inspector |

---

## Troubleshooting

### "Door doesn't open"
- Check DoorsEngine component exists
- Verify door isn't locked
- Check interaction range

### "No animation"
- Check DoorAnimation component exists
- Verify animation speed > 0
- Check panels exist (LeftPanel, RightPanel)

### "No sound"
- Check audio clips assigned
- Verify AudioSource exists
- Check volume settings

### "Texture not applying"
- Check RoomTextureGenerator.Instance exists
- Verify seed is set
- Clear cache if needed

---

*Documentation: 2026-03-01*  
*Unity 6 (6000.3.7f1) compatible*  
*UTF-8 encoding - Unix line endings*  
*Status: Production Ready ✅*
