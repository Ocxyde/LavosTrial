# HUD Event System - Complete ✅

**Status:** FULLY WORKING  
**Last Updated:** 2026-03-01

---

## ✅ Event Flow (Working)

```
PlayerStats (stat changes)
    │
    ├─→ OnHealthChanged (static event)
    │   └─→ UIBarsSystem.OnHealthChanged()
    │       └─→ Updates Health Bar (left edge, vertical, green)
    │
    ├─→ OnManaChanged (instance event)
    │   └─→ UIBarsSystem.OnManaChanged()
    │       └─→ Updates Mana Bar (right edge, vertical, blue)
    │
    ├─→ OnStaminaChanged (instance event)
    │   └─→ UIBarsSystem.OnStaminaChanged()
    │       └─→ Updates Stamina Bar (bottom edge, horizontal, yellow/green)
    │
    ├─→ OnEffectAdded (instance event)
    │   └─→ UIBarsSystem.OnEffectAdded()
    │       └─→ Creates status effect icon (top center)
    │
    └─→ OnEffectRemoved (instance event)
        └─→ UIBarsSystem.OnEffectRemoved()
            └─→ Removes status effect icon
```

---

## 📊 How It Works

### 1. **Stat Changes**

When any code modifies player stats:

```csharp
// Example: Player takes damage
playerStats.TakeDamage(50f);

// This triggers:
// 1. StatsEngine updates CurrentHealth
// 2. StatsEngine invokes OnHealthChanged event
// 3. PlayerStats relays the event
// 4. UIBarsSystem receives and updates bar
```

### 2. **Event Subscription** (Automatic)

In `UIBarsSystem.Start()` → `SubscribeToEvents()`:

```csharp
// Uses reflection to avoid circular dependencies
var statsType = System.Type.GetType("Code.Lavos.Core.PlayerStats, Code.Lavos.Core");

// Subscribe to health (static event)
var onHealthEvent = statsType.GetEvent("OnHealthChanged", 
    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
onHealthEvent.AddEventHandler(null, new System.Action<float, float>(OnHealthChanged));

// Subscribe to mana/stamina (instance events)
var onManaEvent = playerStatsInstance.GetType().GetEvent("OnManaChanged", ...);
onManaEvent.AddEventHandler(playerStatsInstance, new System.Action<float, float>(OnManaChanged));

// Subscribe to effects
var onEffectAdded = playerStatsInstance.GetType().GetEvent("OnEffectAdded", ...);
onEffectAdded.AddEventHandler(playerStatsInstance, new Action<StatusEffectData>(OnEffectAdded));
```

### 3. **Bar Updates** (Real-time)

```csharp
// When health changes
public void OnHealthChanged(float current, float max)
{
    SetHealth(current, max);  // Updates bar fill and color
}

// Sets bar visual
public void SetHealth(float current, float max)
{
    _healthFill.fillAmount = current / max;  // Fill percentage
    _healthFill.color = GetHealthColor(current / max);  // Color based on %
    _healthText.text = $"{(current/max)*100:F0}%";  // Text display
}
```

---

## 🎯 Status Effect Icons

### When Buff/Debuff is Added:

```csharp
// In PlayerStats
_statsEngine.ApplyEffect(effectData);  // Apply status effect
// ↓
// OnEffectAdded event fires
// ↓
// UIBarsSystem.OnEffectAdded(effect)
// ↓
// Creates icon at top center of screen
// Shows duration bar + stack count
```

### Icon Features:

- **Position:** Top center of screen
- **Appearance:** Colored square (green = buff, red = debuff)
- **Duration:** Shows remaining time as fill bar
- **Stacks:** Shows "x2", "x3" if stacked
- **Auto-remove:** Disappears when effect ends

---

## 🔍 Debug Console Messages

When working correctly, you'll see:

```
[UIBarsSystem] Subscribed to PlayerStats events
[UIBarsSystem] Setting initial values...
[UIBarsSystem] Initial values from PlayerHealth - HP: 1000/1000
[UIBarsSystem] OnHealthChanged: 950/1000 (after taking damage)
[UIBarsSystem] OnManaChanged: 45/100 (after casting spell)
[UIBarsSystem] OnStaminaChanged: 75/100 (after sprinting)
[UIBarsSystem] OnEffectAdded: Regeneration (buff icon appears)
[UIBarsSystem] OnEffectRemoved: Regeneration (buff icon disappears)
```

---

## 🧪 Test It In-Game

### Test Health Bar:
```csharp
// In Console or test script
PlayerStats.Instance.TakeDamage(100f);
// Health bar should drop from 1000 → 900
```

### Test Mana Bar:
```csharp
PlayerStats.Instance.UseMana(20f);
// Mana bar should drop
```

### Test Stamina Bar:
```csharp
PlayerStats.Instance.UseStamina(30f);
// Stamina bar should drop
```

### Test Buff Icon:
```csharp
var effect = new StatusEffectData { 
    id = "test_buff", 
    effectName = "Test Buff",
    effectType = EffectType.Buff,
    duration = 10f,
    icon = null  // Or assign a sprite
};
PlayerStats.Instance.Engine.ApplyEffect(effect);
// Icon should appear at top center
```

---

## ✅ Requirements Met

| Feature | Status |
|---------|--------|
| Health bar updates on damage/heal | ✅ Working |
| Mana bar updates on spell cast | ✅ Working |
| Stamina bar updates on sprint | ✅ Working |
| Buff icons appear | ✅ Working |
| Debuff icons appear | ✅ Working |
| Icons show duration | ✅ Working |
| Icons show stacks | ✅ Working |
| Icons auto-remove | ✅ Working |
| Events persist across scenes | ✅ Working (with PersistentUI) |
| No memory leaks | ✅ Proper unsubscribe |

---

## 🎮 In Your Game

**Everything is automatic!** Just:

1. **Add UIBarsSystem to scene** ✅
2. **Have PlayerStats on Player** ✅
3. **Call stat methods** (TakeDamage, UseMana, etc.)
4. **Bars update automatically** ✨

No manual updates needed - events handle everything!

---

## 🐛 Troubleshooting

### Bars Don't Update?

**Check Console for:**
```
[UIBarsSystem] Subscribed to PlayerStats events
```

If missing:
- PlayerStats not found in scene
- Events not firing from StatsEngine
- Check `PlayerStats.Instance != null`

### Icons Don't Appear?

**Check:**
- Effect has valid `id` and `effectName`
- Effect duration > 0 (or is infinite)
- `_effectsContainer` exists in UIBarsSystem
- Check Console for `OnEffectAdded` messages

### Bars Show Wrong Values?

**Verify:**
- PlayerStats has correct base stats
- StatsEngine is updating properly
- Events are being invoked in StatsEngine
- No other scripts overriding values

---

## 📝 Code Examples

### Taking Damage (Health Bar Updates):
```csharp
// In enemy collision or trap
void OnCollisionEnter(Collision col)
{
    if (col.gameObject.CompareTag("Player"))
    {
        var stats = col.gameObject.GetComponent<PlayerStats>();
        stats.TakeDamage(25f);  // Health bar updates automatically!
    }
}
```

### Casting Spell (Mana Bar Updates):
```csharp
// In spell script
void CastSpell()
{
    if (PlayerStats.Instance.UseMana(30f))
    {
        // Spell succeeds - mana bar updated automatically!
        SpawnProjectile();
    }
    else
    {
        // Not enough mana
    }
}
```

### Sprinting (Stamina Bar Updates):
```csharp
// In PlayerController Update
if (isSprinting)
{
    PlayerStats.Instance.UseStamina(staminaDrain * Time.deltaTime);
    // Stamina bar updates automatically!
}
```

### Applying Buff (Icon Appears):
```csharp
// In powerup or potion
void ApplyBuff()
{
    var buffData = new StatusEffectData
    {
        id = "strength_buff",
        effectName = "Strength",
        effectType = EffectType.Buff,
        duration = 30f,
        intensity = 1.5f  // +50% strength
    };
    
    PlayerStats.Instance.Engine.ApplyEffect(buffData);
    // Icon appears at top center automatically!
}
```

---

**Your HUD is fully event-driven and automatic!** 🎉

Just use the stats system and the UI updates itself! ✨
