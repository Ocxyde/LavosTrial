# Status System - Complete Guide

## Folder Structure
```
Assets/Scripts/Status/
├── StatModifier.cs       // Stat modifier types and collections
├── DamageType.cs         // Damage types and DamageInfo struct
├── StatusEffectData.cs   // Comprehensive effect data (buffs, debuffs, etc.)
└── StatsEngine.cs        // Main stat calculator - all stat calculations
```

## Overview

The Status system centralizes all player stat calculations, buff/debuff management, damage calculations, and resource consumption.

### Key Components

| Component | Purpose |
|-----------|---------|
| `StatsEngine` | Main class - handles all status effects and stat calculations |
| `StatModifier` | Individual stat modifier (additive, multiplicative, override) |
| `StatModifierCollection` | Collection of modifiers with calculation logic |
| `StatusEffectData` | Complete effect definition (buffs, debuffs, passives, etc.) |
| `DamageType` | 10 damage types with resistance support |
| `DamageInfo` | Damage event data with type, crit, source info |

---

## StatsEngine API

### Initialization

```csharp
var stats = new StatsEngine();
stats.SetBaseStats(
    maxHealth: 100f,
    maxMana: 50f,
    maxStamina: 100f,
    healthRegen: 2f,
    manaRegen: 5f,
    staminaRegen: 10f
);
```

### Update Loop

```csharp
void Update()
{
    stats.UpdateEffects();           // Update effect timers, ticks
    stats.ApplyRegeneration(Time.deltaTime);  // Apply regen
}
```

---

## Stat Modifiers

### Adding Modifiers

```csharp
// +25% max health for 30 seconds
stats.AddModifier(
    statName: "health",
    id: "health_buff_25",
    sourceId: "strength_potion",
    type: ModifierType.Multiplicative,
    value: 0.25f,
    duration: 30f
);

// -30% stamina cost (permanent)
stats.AddModifier(
    statName: "staminaCost",
    id: "light_footed",
    sourceId: "rogue_passive",
    type: ModifierType.Multiplicative,
    value: -0.30f
);

// +50 flat health
stats.AddModifier(
    statName: "health",
    id: "health_flat_50",
    sourceId: "warrior_bonus",
    type: ModifierType.Additive,
    value: 50f
);
```

### Available Stats

| Stat Name | Description |
|-----------|-------------|
| `health` | Max health pool |
| `mana` | Max mana pool |
| `stamina` | Max stamina pool |
| `healthRegen` | Health per second |
| `manaRegen` | Mana per second |
| `staminaRegen` | Stamina per second |
| `healthCost` | Health cost modifier |
| `staminaCost` | Stamina cost modifier |
| `manaCost` | Mana cost modifier |
| `damage` | Damage multiplier |
| `resistance` | Damage resistance |

### Removing Modifiers

```csharp
// Remove all modifiers from a source
stats.RemoveModifiersBySource("strength_potion");
```

---

## Status Effects (Buffs/Debuffs)

### Creating Effects

```csharp
// Strength Buff (+25% damage, +10% attack speed)
var strengthBuff = new StatusEffectData
{
    id = "strength_buff",
    effectName = "Strength",
    effectType = EffectType.Buff,
    duration = 30f,
    intensity = 1f,
    maxStacks = 3,
    stackType = StackType.Stacks,
    isDispellable = true,
    description = "Increases damage by 25%"
};
strengthBuff.AddModifier("damage", ModifierType.Multiplicative, 0.25f);

// Poison Debuff (DoT)
var poison = new StatusEffectData
{
    id = "poison",
    effectName = "Poison",
    effectType = EffectType.Debuff,
    duration = 10f,
    tickRate = 1f,
    damageOverTime = 5f,
    damageType = DamageType.Poison,
    intensity = 1f
};

// Haste Buff (+30% stamina regen, -25% stamina cost)
var haste = new StatusEffectData
{
    id = "haste",
    effectName = "Haste",
    effectType = EffectType.Buff,
    duration = 15f,
    intensity = 1f
};
haste.AddModifier("staminaRegen", ModifierType.Multiplicative, 0.30f);
haste.AddModifier("staminaCost", ModifierType.Multiplicative, -0.25f);

// Corruption Curse (reduces healing, deals Corruption DoT)
var corruptionCurse = new StatusEffectData
{
    id = "corruption_curse",
    effectName = "Corruption",
    effectType = EffectType.Curse,
    duration = 20f,
    tickRate = 2f,
    damageOverTime = 3f,
    damageType = DamageType.Corruption,
    intensity = 1f,
    isDispellable = false  // Curses are hard to remove
};
corruptionCurse.AddModifier("healthRegen", ModifierType.Multiplicative, -0.50f); // -50% healing
```

### Applying Effects

```csharp
// Apply effect
stats.ApplyEffect(strengthBuff, applierId: "player_spell");

// Check for effect
if (stats.HasEffect("strength_buff"))
{
    float intensity = stats.GetEffectIntensity("strength_buff");
}

// Remove specific effect
stats.RemoveEffect("poison");

// Remove all debuffs
stats.RemoveEffectsByType(EffectType.Debuff);

// Dispel all dispellable effects
stats.DispelAll();

// Clear all effects
stats.ClearAllEffects();
```

### Effect Types

| Type | Description |
|------|-------------|
| `Buff` | Positive effect |
| `Debuff` | Negative effect |
| `Curse` | Curse debuff (Corruption damage, healing reduction) |
| `Passive` | Always active (racial, class) |
| `Temporary` | Short-term (food, potion) |
| `Aura` | Area effect |
| `Triggered` | Activates on condition |

### Stack Types

| Type | Behavior |
|------|----------|
| `None` | Cannot stack, refreshes duration |
| `Stacks` | Stacks up to maxStacks |
| `Refresh` | Refreshes duration, keeps highest intensity |
| `Override` | New effect replaces old |

---

## Damage System

### Damage Types

```csharp
public enum DamageType
{
    Physical,    // Standard physical damage
    Fire,        // Fire/heat damage
    Ice,         // Ice/cold damage
    Lightning,   // Electric damage
    Poison,      // Toxic/poison DoT
    Magic,       // Generic magical damage
    Holy,        // Divine/light damage
    Shadow,      // Dark/void damage
    Bleed,       // Bleeding DoT
    Corruption,  // Curse-based damage, drains stats/healing
    True         // Ignores all resistances
}
```

### Taking Damage

```csharp
// Simple damage
playerStatus.ModifyHealth(-25f);

// With damage calculation
var damageInfo = new DamageInfo(50f, DamageType.Fire, "enemy_wizard")
    .WithCritical(true, 2.5f);

float finalDamage = playerStatus.CalculateDamage(damageInfo);
playerStatus.ModifyHealth(-finalDamage);
```

### Resistances

```csharp
// Set base resistance (50% physical resistance)
playerStatus.SetBaseResistance(DamageType.Physical, 0.5f);

// Get resistance multiplier (0.5 = 50% reduction)
float multiplier = playerStatus.GetResistanceMultiplier(DamageType.Fire);

// Get resistance percentage
float percent = playerStatus.GetResistancePercent(DamageType.Ice);

// Temporary resistance buff (+25% fire resistance for 10s)
playerStatus.ModifyResistance(
    type: DamageType.Fire,
    amount: 0.25f,
    modifierType: ModifierType.Additive,
    duration: 10f,
    sourceId: "fire_resistance_potion"
);
```

### Resistance Values

| Value | Effect |
|-------|--------|
| `0.0` | Neutral (100% damage) |
| `0.5` | 50% damage reduction |
| `-0.25` | 25% damage increase (weakness) |
| `1.0` | Immune |

---

## Resource Management

### Health

```csharp
// Use health for abilities that cost HP (auto-applies cost modifiers)
if (stats.UseHealth(10f))
{
    // Cast blood magic ability
}

// Check if can afford health cost
if (stats.CanAfford(healthCost: 20f))
{
    // Can use ability
}
```

### Mana

```csharp
// Use mana (auto-applies cost modifiers)
if (stats.UseMana(20f))
{
    // Cast spell
}

// Restore mana
stats.RestoreMana(15f);

// Check if can afford
if (stats.CanAfford(manaCost: 50f))
{
    // Can cast
}
```

### Stamina

```csharp
// Use stamina (auto-applies cost modifiers)
if (stats.UseStamina(10f))
{
    // Perform action
}

// Sprint example (1% current stamina per frame)
if (isSprinting)
{
    float drain = stats.CurrentStamina * 0.01f;
    stats.UseStamina(drain);
}
```

### Combined Costs

```csharp
// Check if player can afford multiple resource costs
if (stats.CanAfford(
    healthCost: 10f,
    manaCost: 20f,
    staminaCost: 15f))
{
    // Cast hybrid ability
    stats.UseHealth(10f);
    stats.UseMana(20f);
    stats.UseStamina(15f);
}
```

---

## Events

```csharp
void OnEnable()
{
    stats.OnHealthChanged += OnHealthChanged;
    stats.OnManaChanged += OnManaChanged;
    stats.OnStaminaChanged += OnStaminaChanged;
    stats.OnEffectAdded += OnEffectAdded;
    stats.OnEffectRemoved += OnEffectRemoved;
}

void OnDisable()
{
    stats.OnHealthChanged -= OnHealthChanged;
    stats.OnManaChanged -= OnManaChanged;
    stats.OnStaminaChanged -= OnStaminaChanged;
    stats.OnEffectAdded -= OnEffectAdded;
    stats.OnEffectRemoved -= OnEffectRemoved;
}

void OnHealthChanged(float current, float max) { }
void OnManaChanged(float current, float max) { }
void OnStaminaChanged(float current, float max) { }
void OnEffectAdded(StatusEffectData effect) { }
void OnEffectRemoved(StatusEffectData effect) { }
```

---

## Complete Example: Spell Casting

```csharp
public class FireballSpell : MonoBehaviour
{
    [SerializeField] private float baseManaCost = 20f;
    [SerializeField] private float baseDamage = 35f;

    private StatsEngine _stats;

    void Awake()
    {
        _stats = PlayerStats.Instance.Engine;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CastFireball();
        }
    }

    void CastFireball()
    {
        // Check and consume mana (with modifiers)
        if (!_stats.UseMana(baseManaCost))
        {
            Debug.Log("Not enough mana!");
            return;
        }

        // Calculate damage (with modifiers and crit)
        var damageInfo = new DamageInfo(baseDamage, DamageType.Fire, "player_fireball")
            .WithCritical(UnityEngine.Random.value < _stats.CriticalChance);

        float finalDamage = _stats.CalculateDamage(damageInfo);

        // Apply to enemy
        var enemy = GetTarget();
        enemy.TakeDamage(finalDamage, DamageType.Fire);

        Debug.Log($"Fireball hit for {finalDamage:F1} damage!");
    }
}
```

---

## Tips

1. **Always use `UseMana()` / `UseStamina()`** - They automatically apply cost modifiers
2. **Use `DamageType` for all damage** - Enables resistance calculations
3. **Clean up modifiers** - Effects automatically clean up when expired
4. **True damage ignores resistance** - Use `DamageType.True` to bypass defenses
5. **Stack modifiers carefully** - Additive stack, multiplicative compound
6. **Use `StatsEngine` for calculations** - `PlayerStats` is just a MonoBehaviour wrapper
