# HUDSystem - Complete Dynamic HUD Guide

**Location:** `Assets/Docs/HUD_SYSTEM_GUIDE.md`  
**Unity Version:** 6000.3.7f1 (Unity 6)  
**Last Updated:** 2026-03-02  
**Status:** ✅ **PRODUCTION READY**

---

## Overview

**HUDSystem** is the complete, dynamic HUD solution for PeuImporte. It plugs into the **EventHandler** for all stat updates and game events, following the plug-in-and-out architecture.

### Features

| Feature | Description |
|---------|-------------|
| **Health/Mana/Stamina Bars** | Real-time updates with smooth color interpolation |
| **Hotbar** | 5 slots (keys 1-5) with active slot highlighting |
| **Status Effects** | Buff/debuff icons with duration bars and stack counts |
| **Floating Combat Text** | Damage/heal numbers that float upward |
| **Notifications** | Top-center toast notifications |
| **Interaction Prompts** | Dynamic "[E] Interact" prompts |
| **Auto-Construct** | Builds entire UI at runtime - no manual setup |

---

## Architecture (Plug-in-and-Out)

```
EventHandler (Central Event Hub)
    │
    ├── OnPlayerHealthChanged ──┐
    ├── OnPlayerDamaged ────────┤
    ├── OnPlayerHealed ─────────┤
    ├── OnPlayerManaChanged ────┤
    ├── OnPlayerStaminaChanged ─┤
    ├── OnStatChanged ──────────┤
    ├── OnFloatingTextRequested─┤
    └── OnNotificationRequested─┘
                                │
                                ▼
                        HUDSystem (Subscriber)
```

**HUDSystem** subscribes to **EventHandler** events in `Start()` and unsubscribes in `OnDestroy()`.

---

## Setup

### Method 1: Auto-Setup (Recommended)

1. **In Unity Editor:**
   - Tools > PeuImporte > Verify/Setup Scene
   - Click "⚡ Auto-Setup Missing Components"
   - HUDSystem is automatically created

### Method 2: Manual Setup

1. **Create empty GameObject:**
   - Name: "HUDSystem"
   
2. **Add component:**
   - Add `HUDSystem.cs`

3. **Configure (optional):**
   - All settings are optional - defaults work great
   - Adjust colors, sizes in Inspector

4. **Press Play:**
   - HUD auto-constructs at runtime
   - No further setup needed!

---

## How It Works

### 1. Event Subscription (Automatic)

```csharp
// In HUDSystem.Start():
SubscribeToEvents();

// Subscribes to:
- EventHandler.OnPlayerHealthChanged
- EventHandler.OnPlayerDamaged
- EventHandler.OnPlayerHealed
- EventHandler.OnPlayerManaChanged
- EventHandler.OnPlayerStaminaChanged
- EventHandler.OnStatChanged
- EventHandler.OnFloatingTextRequested
- EventHandler.OnNotificationRequested
```

### 2. Bar Updates (Automatic)

When player takes damage:

```csharp
// In CombatSystem or any script:
EventHandler.Instance.InvokePlayerDamaged(25f);

// Result:
// 1. Health bar flashes red
// 2. Floating text "-25" appears
// 3. Health bar updates to new value
// 4. Color interpolates based on health %
```

### 3. Status Effects (Via EventHandler)

```csharp
// Apply a buff/debuff:
EventHandler.Instance.InvokeStatChanged("Poison", 1f);

// HUDSystem displays:
// - Icon in bottom-left corner
// - Duration bar (counts down)
// - Stack count (if > 1)
```

---

## Usage Examples

### Show Damage Number

```csharp
// Deal damage and show floating text
EventHandler.Instance.InvokeFloatingTextRequested("-50", Color.red, 1.5f);
```

### Show Heal Number

```csharp
// Heal and show floating text
EventHandler.Instance.InvokeFloatingTextRequested("+30", Color.green, 1.5f);
```

### Show Notification

```csharp
// Show toast notification
EventHandler.Instance.InvokeNotificationRequested("Level Up!");
```

### Update Interaction Prompt

```csharp
// Show/hide interaction prompt
HUDSystem.Instance.SetInteractionPrompt("Open Chest");
// or
HUDSystem.Instance.SetInteractionPrompt("");
```

### Add Status Effect

```csharp
// Create status effect data
var poisonEffect = new StatusEffectData
{
    effectName = "Poison",
    effectType = EffectType.Debuff,
    // ... other properties
};

// Add to HUD
HUDSystem.Instance.AddStatusEffect(poisonEffect, duration: 10f, stacks: 1);
```

### Remove Status Effect

```csharp
HUDSystem.Instance.RemoveStatusEffect("Poison");
```

---

## Inspector Settings

### UI Settings

| Property | Default | Description |
|----------|---------|-------------|
| `Auto Construct` | true | Build UI at runtime |
| `UI Scale` | 1.0 | Global scale multiplier |

### Bar Colors

| Property | Default | Description |
|----------|---------|-------------|
| `Health Color High` | Bright Green | Color when health > 60% |
| `Health Color Low` | Yellow-Orange | Color when health 30-60% |
| `Health Color Critical` | Red | Color when health < 30% |
| `Mana Color` | Blue | Mana bar color |
| `Stamina Color` | Yellow | Stamina bar color |

### Hotbar

| Property | Default | Description |
|----------|---------|-------------|
| `Hotbar Slot Count` | 5 | Number of slots |
| `Hotbar Slot Size` | 52 | Size in pixels |
| `Hotbar Active Color` | Gold | Active slot border |
| `Hotbar Inactive Color` | Gray | Inactive slot border |

### Status Effects

| Property | Default | Description |
|----------|---------|-------------|
| `Effect Icon Size` | 40 | Icon size in pixels |
| `Effect Spacing` | 8 | Space between icons |

---

## Code Reference

### Public Methods

```csharp
// Set interaction prompt
HUDSystem.Instance.SetInteractionPrompt(string prompt);

// Show notification
HUDSystem.Instance.ShowNotification(string message);

// Add status effect
HUDSystem.Instance.AddStatusEffect(StatusEffectData effect, float duration, int stacks);

// Remove status effect
HUDSystem.Instance.RemoveStatusEffect(string effectName);

// Show floating text
HUDSystem.Instance.ShowFloatingText(string text, Color color, float duration);
```

### Properties

```csharp
// Singleton access
HUDSystem.Instance

// Current active hotbar slot (0-4)
HUDSystem.Instance.ActiveHotbarSlot
```

---

## Troubleshooting

### HUD doesn't appear

**Check:**
1. HUDSystem component is added to a GameObject
2. `Auto Construct` is enabled
3. Canvas is created (check Hierarchy)

### Bars don't update

**Check:**
1. EventHandler exists in scene
2. Events are being invoked (check Console for logs)
3. Player has PlayerStats component

### Status effects don't show

**Check:**
1. StatusEffectData is properly configured
2. Effect name is unique
3. Duration is > 0

### Hotbar doesn't respond to keys

**Check:**
1. Input is working (test in Unity Input Manager)
2. Game is not paused
3. Inventory is assigned to player

---

## Performance Notes

- **Auto-construct:** Runs once at Start() - no runtime cost
- **Bar updates:** Event-driven - only updates when stats change
- **Floating text:** Auto-destroys after duration
- **Status effects:** Removed automatically when expired

**Recommended:** Keep status effect count under 10 for best performance.

---

## Migration from Legacy HUD

### Old Code (UIBarsSystem only)

```csharp
// Old: Direct bar access
UIBarsSystem.Instance.UpdateHealth(100, 1000);
```

### New Code (HUDSystem via EventHandler)

```csharp
// New: Event-driven
EventHandler.Instance.InvokePlayerHealthChanged(100f, 1000f);
```

**Benefits:**
- Decoupled architecture
- Multiple subscribers possible
- Automatic floating text
- Color interpolation built-in

---

## Related Documentation

| Document | Location |
|----------|----------|
| TODO.md | `Assets/Docs/TODO.md` |
| EVENT_HANDLER.md | `Assets/Docs/SFXVFX_EVENT_INTEGRATION.md` |
| STATUS_SYSTEM.md | `Assets/Scripts/Status/STATUS_SYSTEM_USAGE.md` |
| SCENE_SETUP_GUIDE.md | `Assets/Docs/SCENE_SETUP_GUIDE.md` |

---

**Last Reviewed:** 2026-03-02  
**Status:** ✅ **PRODUCTION READY**  
**Unity Version:** 6000.3.7f1

---

*Document generated - Unity 6 compatible - UTF-8 encoding - Unix LF*
