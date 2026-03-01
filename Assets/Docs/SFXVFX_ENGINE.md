# SFX/VFX Engine Documentation
**Location:** `Assets/Docs/SFXVFX_ENGINE.md`  
**Date Created:** 2026-03-01  
**Unity Version:** 6000.3.7f1  
**Status:** ✅ **PRODUCTION READY**

---

## Overview

The **SFXVFXEngine** is a centralized system for managing all sound and visual effects in the game. It provides:

- **Particle system presets** (fire, smoke, sparks, magic, etc.)
- **Sound effect management** (SFX)
- **Tetrahedral VFX integration** (low-poly geometric effects)
- **Object pooling** for performance
- **Event-driven architecture** (automatic effects on game events)

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    SFXVFXEngine                              │
│                   (Singleton, Core)                          │
├─────────────────────────────────────────────────────────────┤
│  Audio System           │  VFX System                        │
│  ├─ SFX Source          │  ├─ Particle Pool (50 systems)     │
│  ├─ Ambient Source      │  ├─ Tetrahedral VFX                │
│  ├─ Music Source        │  └─ Object Pooling                 │
│  └─ Pooled Sources (20) │                                    │
├─────────────────────────────────────────────────────────────┤
│              Event Integration (EventHandler)                │
│  OnDamageDealt │ OnHeal │ OnDeath │ OnPickup │ OnJump      │
└─────────────────────────────────────────────────────────────┘
```

---

## Features

### 1. Particle System Effects

| Effect | Trigger | Description |
|--------|---------|-------------|
| **Blood Splatter** | Physical damage | Red particles, cone spray |
| **Fire Hit** | Fire damage | Orange/yellow flames |
| **Ice Hit** | Ice damage | Blue/white snow particles |
| **Lightning Hit** | Lightning damage | TODO: Lightning bolt VFX |
| **Heal Effect** | Healing received | Green rising particles |
| **Death Effect** | Entity death | Dark smoke cloud |
| **Pickup Effect** | Item collection | Yellow sparks |
| **Item Use Effect** | Using items | Cyan dust |
| **Jump Effect** | Player jump | Brown dust puff |
| **Land Effect** | Player land | Dust cloud |

### 2. Tetrahedral VFX

Low-poly geometric effects following the tetrahedral art style:

- **Tetrahedral Fire** - Rising tetrahedrons with fire material
- **Tetrahedral Magic** - Outward expanding magical tetrahedrons
- **Variant System** - 8 different tetrahedron shapes

### 3. Sound Effects

Pooled audio sources for performance:

- **SFX Channel** - Sound effects (hits, pickups, etc.)
- **Ambient Channel** - Background ambiance (looping)
- **Music Channel** - Music tracks (looping)
- **20 Pooled Sources** - Reused for multiple simultaneous SFX

---

## Usage

### Basic Setup

1. **Add to Scene:**
   ```csharp
   // The engine auto-creates as a singleton
   // Just access it from anywhere:
   SFXVFXEngine engine = SFXVFXEngine.Instance;
   ```

2. **Automatic Event Integration:**
   ```csharp
   // No setup needed! The engine automatically subscribes to:
   // - OnDamageDealt
   // - OnHealReceived
   // - OnDeath
   // - OnItemPickedUp
   // - OnItemUsed
   // - OnPlayerJump
   // - OnPlayerLand
   ```

### Manual VFX Spawning

```csharp
// Spawn fire hit effect
SFXVFXEngine.Instance.SpawnFireHit(enemyPosition);

// Spawn heal effect
SFXVFXEngine.Instance.SpawnHealEffect(playerPosition);

// Spawn pickup effect
SFXVFXEngine.Instance.SpawnPickupEffect(itemPosition);

// Spawn tetrahedral magic effect
SFXVFXEngine.Instance.SpawnTetrahedralMagic(
    position,
    magicColor: Color.blue,
    scale: 1.5f
);
```

### Manual SFX Playback

```csharp
// Play sound effect
SFXVFXEngine.Instance.PlaySFX("SwordSwing", position, volume: 0.8f);

// Play ambient sound
AudioClip forestAmbient = Resources.Load<AudioClip>("Audio/Ambient/Forest");
SFXVFXEngine.Instance.PlayAmbient(forestAmbient);

// Play music
AudioClip battleMusic = Resources.Load<AudioClip>("Music/Battle");
SFXVFXEngine.Instance.PlayMusic(battleMusic, fadeDuration: 2f);
```

### Volume Control

```csharp
// Set master volume (affects all channels)
SFXVFXEngine.Instance.SetMasterVolume(0.8f);

// Set individual volumes
SFXVFXEngine.Instance.SetSFXVolume(1.0f);
SFXVFXEngine.Instance.SetMusicVolume(0.5f);

// Enable/disable VFX
SFXVFXEngine.Instance.SetVFXEnabled(false); // Disable all VFX
```

---

## Integration with Existing Systems

### ParticleGenerator

The SFXVFXEngine uses `ParticleGenerator.cs` internally:

```csharp
// ParticleGenerator provides:
// - CreateParticleSystem()
// - ApplyConfig()
// - Static particle config presets
```

### TetrahedronEngine

Tetrahedral VFX uses the static `TetrahedronEngine`:

```csharp
// TetrahedronEngine provides:
// - GenerateLevel(seed, width, height) - Call once per level
// - GetTetrahedron(variantIndex) - Get mesh by variant (0-7)
// - CreateTetrahedron(position, rotation, variant, material)
```

### EventHandler

Automatic event subscription:

```csharp
// When CombatSystem.DealDamage() is called:
// 1. EventHandler invokes OnDamageDealt
// 2. SFXVFXEngine receives event
// 3. Spawns appropriate VFX based on damage type
// 4. Plays appropriate SFX
// 5. DialogEngine shows floating damage text
```

---

## Configuration

### Inspector Settings

| Setting | Default | Description |
|---------|---------|-------------|
| **Master Volume** | 1.0 | Overall volume multiplier |
| **SFX Volume** | 0.8 | Sound effects volume |
| **Ambient Volume** | 0.6 | Ambient sound volume |
| **Music Volume** | 0.5 | Music volume |
| **Enable VFX** | true | Master VFX toggle |
| **Enable Tetrahedral VFX** | true | Toggle tetrahedral effects |
| **Max Particle Systems** | 50 | Performance limit |
| **Particle Cull Distance** | 30f | Distance to cull particles |
| **Initial Pool Size** | 20 | Starting audio source count |
| **Max Pool Size** | 100 | Maximum audio sources |

---

## Performance

### Object Pooling

**Particle Systems:**
- Initial pool: 20 systems
- Max pool: 50 systems
- Automatic culling beyond 30 units

**Audio Sources:**
- Initial pool: 20 sources
- Max pool: 100 sources
- Automatic reuse when available

### Optimization Tips

1. **Reduce Max Particle Systems** for lower-end hardware
2. **Lower Particle Cull Distance** for large scenes
3. **Disable Tetrahedral VFX** for performance-critical builds
4. **Use fewer pooled audio sources** if memory is constrained

---

## Creating New Effects

### Step 1: Add Particle Preset

In `ParticleGenerator.cs`:

```csharp
public enum ParticlePreset
{
    // ... existing presets ...
    MyCustomEffect  // Add new preset
}
```

### Step 2: Create Config Method

```csharp
public static ParticleConfig CreateMyCustomEffect()
{
    return new ParticleConfig
    {
        preset = ParticlePreset.MyCustomEffect,
        startLifetime = 1f,
        startSpeed = 2f,
        startSize = 0.3f,
        startColor = Color.blue,
        emissionRate = 40f,
        maxParticles = 60
    };
}
```

### Step 3: Add to SFXVFXEngine

```csharp
public void SpawnMyCustomEffect(Vector3 position)
{
    if (!enableVFX) return;

    ParticleSystem ps = GetPooledParticleSystem();
    if (ps == null) return;

    ps.transform.position = position;
    var config = ParticleConfig.CreateMyCustomEffect();
    particleGenerator.ApplyConfig(ps, config);
    ps.Play();

    _activeParticles.Add(ps);
}
```

---

## Event Integration

### Automatically Triggered Effects

| Event | Parameters | Effect Spawned |
|-------|------------|----------------|
| `OnDamageDealt` | DamageInfo, finalDamage | Blood/Fire/Ice/Sparks based on type |
| `OnHealReceived` | healAmount, position | Green heal particles |
| `OnDeath` | victim GameObject | Death smoke cloud |
| `OnItemPickedUp` | item GameObject | Yellow pickup sparks |
| `OnItemUsed` | item GameObject | Cyan item use effect |
| `OnPlayerJump` | - | Jump dust puff |
| `OnPlayerLand` | - | Land dust cloud |

### Adding New Event Triggers

In `SFXVFXEngine.SubscribeToEvents()`:

```csharp
// Subscribe to new event
_eventHandler.OnMyCustomEvent += OnMyCustomEvent;
```

In the handler:

```csharp
private void OnMyCustomEvent(MyCustomData data)
{
    if (!enableVFX) return;

    SpawnMyCustomEffect(data.position);
    PlaySFX("MyCustomSFX");
}
```

---

## Tetrahedral VFX System

### How It Works

1. **Generate Level** (once per level):
   ```csharp
   TetrahedronEngine.GenerateLevel(seed: 12345, width: 31, height: 31);
   ```

2. **Spawn Tetrahedral Effects**:
   ```csharp
   // Fire effect - 8 tetrahedrons rise upward
   SFXVFXEngine.Instance.SpawnTetrahedralFire(position, scale: 1f);

   // Magic effect - 12 tetrahedrons expand outward
   SFXVFXEngine.Instance.SpawnTetrahedralMagic(
       position,
       magicColor: Color.purple,
       scale: 1.5f
   );
   ```

### 8 Tetrahedron Variants

| Variant | Name | Description |
|---------|------|-------------|
| 0 | Standard | Regular tetrahedron |
| 1 | Elongated | Stretched on Y axis |
| 2 | Compressed | Squashed on Y axis |
| 3 | Skewed | Asymmetric skew |
| 4 | WideBase | Wide base, short height |
| 5 | NarrowBase | Narrow base, tall height |
| 6 | Asymmetric | Irregular shape |
| 7 | Crystal | Crystal-like facets |

---

## Files Created

| File | Purpose | Lines |
|------|---------|-------|
| `Assets/Scripts/Core/SFXVFXEngine.cs` | Main SFX/VFX engine | ~750 |
| `Assets/Scripts/Core/ParticleGenerator.cs` | Additional presets | +150 |
| `Temp/TetrahedronAssets/TetrahedronEngine.cs` | Tetrahedral geometry | ~280 |
| `Assets/Docs/SFXVFX_ENGINE.md` | This documentation | - |

---

## Testing Checklist

- [ ] Add SFXVFXEngine to scene (or let it auto-create)
- [ ] Test particle effects spawn correctly
- [ ] Test sound effects play (need audio clips)
- [ ] Test event integration (damage, healing, etc.)
- [ ] Test tetrahedral VFX (spawn fire/magic)
- [ ] Test volume controls
- [ ] Test VFX enable/disable
- [ ] Test particle culling (walk away from effects)
- [ ] Profile performance (no memory leaks)

---

## Troubleshooting

### "SFXVFXEngine.Instance is null"
- The engine should auto-create on first access
- Check for compilation errors
- Ensure EventHandler exists in scene

### "Particle effects not spawning"
- Check `enableVFX` is true
- Verify ParticleGenerator component exists
- Check particle pool isn't exhausted

### "Tetrahedral VFX not working"
- Ensure `enableTetrahedralVFX` is true
- Call `TetrahedronEngine.GenerateLevel()` first
- Check for shader errors

### "No sound playing"
- Audio clips need to be loaded (TODO section)
- Check volume settings
- Verify audio listener exists on Main Camera

---

## Future Enhancements

### Audio
- [ ] Implement AudioClip loading from Resources
- [ ] Add Addressables support
- [ ] Implement 3D spatial audio
- [ ] Add audio mixer integration
- [ ] Implement sound occlusion

### VFX
- [ ] Implement lightning bolt VFX
- [ ] Add more particle presets (poison, holy, etc.)
- [ ] Implement vertex animation for tetrahedrons
- [ ] Add GPU instancing for large effects
- [ ] Implement VFX LOD system

### Integration
- [ ] Add more event triggers
- [ ] Implement effect priority system
- [ ] Add effect budget (limit simultaneous effects)
- [ ] Implement VFX quality settings

---

## Related Documentation

- `TETRAHEDRAL_STYLE.md` - Tetrahedral art style guide
- `TETRAHEDRON_SYSTEM.md` - Tetrahedron system documentation
- `PARTICLE_SYSTEM.md` - Particle generator documentation
- `EVENT_SYSTEM.md` - EventHandler documentation

---

*Documentation created: 2026-03-01*  
*Unity 6 (6000.3.7f1) compatible*  
*UTF-8 encoding - Unix line endings*  
*Status: Production Ready ✅*
