# InteractionSystem - Documentation

**Version:** 1.0  
**Unity Version:** 6000.3.7f1  
**Namespace:** `Code.Lavos.Core`  
**File:** `Assets/Scripts/Core/InteractionSystem.cs`

---

## 📖 Overview

The **InteractionSystem** is the centralized manager for all player interactions in the game. It provides a single point of control for:

- **E-key interactions** with objects (chests, doors, switches, NPCs)
- **Combat actions** (melee attacks, weapon swings)
- **Spell casting** (mana-based abilities)
- **Item usage** (consumables, equipment)

All interactions are broadcast through the **EventHandler** system, ensuring consistent logging, UI updates, and achievement tracking.

---

## 🏗️ Architecture

### System Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    Player Input                             │
│  E (Interact) | Left Click (Attack) | Right Click (Spell)  │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                 InteractionSystem                           │
│  ┌──────────────┬──────────────┬──────────────┐            │
│  │ Interaction  │    Combat    │     Spell    │            │
│  │   Handler    │    Handler   │    Handler   │            │
│  └──────┬───────┴──────┬───────┴──────┬───────┘            │
│         │              │              │                     │
│         ▼              ▼              ▼                     │
│  ┌──────────────────────────────────────────────┐          │
│  │          EventHandler (Broadcast)            │          │
│  └──────────────────────────────────────────────┘          │
└─────────────────────────────────────────────────────────────┘
         │              │              │
         ▼              ▼              ▼
┌─────────────┐  ┌─────────────┐  ┌─────────────┐
│   Chest     │  │   Enemy     │  │   Spell     │
│   Door      │  │   Combat    │  │   Effect    │
│   Switch    │  │   System    │  │   VFX       │
└─────────────┘  └─────────────┘  └─────────────┘
```

### Component Relationships

```
PlayerController
    ↓ (references)
InteractionSystem
    ↓ (uses)
CombatSystem → StatsEngine → EventHandler
    ↓ (uses)
Inventory → ItemData
    ↓ (detects)
IInteractable objects
```

---

## 🔧 Setup & Configuration

### 1. Automatic Setup (Recommended)

The InteractionSystem auto-initializes on scene start:

```csharp
void Awake()
{
    // Auto-finds required references
    playerController = FindFirstObjectByType<PlayerController>();
    playerStats = FindFirstObjectByType<PlayerStats>();
    combatSystem = FindFirstObjectByType<CombatSystem>();
    inventory = FindFirstObjectByType<Inventory>();
}
```

**No manual setup required!** Just ensure the following exist in your scene:
- Player with `PlayerController` component
- Player with `PlayerStats` component
- `CombatSystem` singleton (auto-created if missing)
- `Inventory` component on player

### 2. Manual Setup (Optional)

For custom setups, assign references in the Inspector:

```
InteractionSystem (Component)
├── Interaction Settings
│   ├── Interaction Range: 3.0
│   ├── Interaction Layer Mask: Everything
│   └── Player Layer Mask: Player
├── Input
│   ├── Interact Key: E
│   ├── Attack Key: Mouse 0
│   ├── Spell Key: Mouse 1
│   └── Use Item Key: F
├── References
│   ├── Player Controller: [Drag Player]
│   ├── Player Stats: [Drag Player]
│   ├── Combat System: [Drag CombatSystem]
│   └── Inventory: [Drag Player]
```

---

## 📚 API Reference

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `static InteractionSystem` | Singleton instance |
| `CurrentInteractable` | `IInteractable` | Currently highlighted interactable object |
| `HasInteractable` | `bool` | True if player is looking at an interactable |
| `InteractionRange` | `float` | Current interaction range in meters |

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnInteractableChanged` | `Action<string>` | Fired when interactable changes (instance) |
| `OnInteractableChangedStatic` | `static Action<string>` | Fired when interactable changes (static) |
| `OnInteractionPerformed` | `Action<string, GameObject>` | Fired when any interaction occurs |

### Public Methods

#### Interaction

```csharp
/// <summary>Manually trigger interaction with current object</summary>
void Interact();

/// <summary>Set the interaction camera (for VR or multi-camera setups)</summary>
void SetInteractionCamera(Camera camera);

/// <summary>Set interaction range at runtime</summary>
void SetInteractionRange(float range);

/// <summary>Clear current interaction (for debugging or state reset)</summary>
void ClearInteraction();
```

#### Combat

```csharp
/// <summary>Manually trigger basic attack</summary>
void Attack();

/// <summary>Manually trigger spell cast</summary>
void CastSpell();

/// <summary>Manually trigger item use</summary>
void UseItem();
```

---

## 🎮 Usage Examples

### Example 1: Create a Custom Interactable Object

```csharp
using UnityEngine;
using Code.Lavos.Core;

public class TreasureChest : InteractableObject
{
    [SerializeField] private int goldAmount = 50;
    [SerializeField] private AudioClip openSound;

    public override string InteractionPrompt => "Open Chest";

    public override void OnInteract(PlayerController player)
    {
        // Play sound
        AudioSource.PlayClipAtPoint(openSound, transform.position);
        
        // Give gold to player
        var inventory = player.GetComponent<Inventory>();
        inventory?.AddGold(goldAmount);
        
        // Broadcast through EventHandler (automatic via InteractionSystem)
        Debug.Log($"Opened chest! Received {goldAmount} gold");
        
        // Disable chest after looting
        gameObject.SetActive(false);
    }
}
```

**Usage:**
1. Add `TreasureChest` component to a GameObject
2. Assign gold amount and sound
3. Add `Collider` (auto-set to trigger)
4. Player can now interact with E key!

---

### Example 2: Create a Spell

```csharp
using UnityEngine;
using Code.Lavos.Core;
using Code.Lavos.Status;

[CreateAssetMenu(fileName = "Fireball", menuName = "Spells/Fireball")]
public class FireballSpell : ScriptableObject
{
    [Header("Spell Stats")]
    public int manaCost = 20;
    public float damage = 35f;
    public float range = 10f;
    public DamageType damageType = DamageType.Fire;
    
    [Header("Visual Effects")]
    public GameObject projectilePrefab;
    public GameObject impactVFX;
}
```

**Usage:**
1. Create spell asset: `Assets > Create > Spells > Fireball`
2. Assign to inventory spell slot
3. Right-click to cast (InteractionSystem handles mana check automatically)

---

### Example 3: Create a Weapon

```csharp
using UnityEngine;
using Code.Lavos.Core;

[CreateAssetMenu(fileName = "IronSword", menuName = "Weapons/Iron Sword")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Stats")]
    public string weaponName = "Iron Sword";
    public float damage = 15f;
    public float staminaCost = 8f;
    public float attackRange = 2.5f;
    public DamageType damageType = DamageType.Physical;
    
    [Header("Visual")]
    public GameObject weaponModel;
    public AudioClip swingSound;
    public AudioClip hitSound;
}
```

**Usage:**
1. Create weapon asset: `Assets > Create > Weapons > Iron Sword`
2. Add to inventory
3. Equip weapon
4. Left-click to attack (InteractionSystem checks stamina automatically)

---

### Example 4: Subscribe to Interaction Events

```csharp
using UnityEngine;
using Code.Lavos.Core;

public class InteractionLogger : MonoBehaviour
{
    void OnEnable()
    {
        // Subscribe to interaction events
        if (InteractionSystem.Instance != null)
        {
            InteractionSystem.Instance.OnInteractionPerformed += LogInteraction;
            InteractionSystem.OnInteractableChangedStatic += UpdateUIPrompt;
        }
    }
    
    void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        if (InteractionSystem.Instance != null)
        {
            InteractionSystem.Instance.OnInteractionPerformed -= LogInteraction;
            InteractionSystem.OnInteractableChangedStatic -= UpdateUIPrompt;
        }
    }
    
    private void LogInteraction(string interactionType, GameObject source)
    {
        Debug.Log($"[Interaction] {interactionType} by {source.name}");
        
        // Could trigger achievements, quests, etc.
        if (interactionType == "Interact")
        {
            // Increment "Explorer" achievement counter
        }
    }
    
    private void UpdateUIPrompt(string prompt)
    {
        // Update UI text
        uiPromptText.text = string.IsNullOrEmpty(prompt) 
            ? "" 
            : $"[E] {prompt}";
    }
}
```

---

## 🔍 Interaction Flow Details

### E-Key Interaction Flow

```
1. Player presses E key
        ↓
2. InteractionSystem.HandleInteractionInput()
        ↓
3. Check: Is CurrentInteractable valid?
        ↓
4. Check: CanInteract(player) == true?
        ↓
5. Broadcast: EventHandler.InvokeItemUsed(item, 1)
        ↓
6. Execute: CurrentInteractable.OnInteract(player)
        ↓
7. Log: OnInteractionPerformed("Interact", player)
        ↓
8. Result: Interaction complete!
```

### Attack Flow

```
1. Player left-clicks
        ↓
2. InteractionSystem.PerformAttack()
        ↓
3. Check: Is weapon equipped?
        ├─ YES → Weapon Attack
        │   ├─ Check stamina >= weapon.staminaCost
        │   ├─ Consume stamina
        │   └─ ExecuteWeaponAttack(weapon)
        │
        └─ NO  → Unarmed Attack
            └─ ExecuteUnarmedAttack()
        ↓
4. Raycast from camera
        ↓
5. Hit enemy?
        ├─ YES → CombatSystem.DealDamage()
        └─ NO  → Miss
        ↓
6. Broadcast: EventHandler.InvokeDamageDealt()
```

### Spell Cast Flow

```
1. Player right-clicks
        ↓
2. InteractionSystem.PerformCastSpell()
        ↓
3. Check: Is spell equipped?
        ├─ YES → Continue
        └─ NO  → Log warning, abort
        ↓
4. Check: Mana >= spell.manaCost?
        ├─ YES → Consume mana, cast spell
        └─ NO  → Log warning, abort
        ↓
5. Broadcast: EventHandler.InvokePlayerManaUsed(manaCost)
        ↓
6. ExecuteSpellCast(spell)
        ↓
7. Raycast or projectile
        ↓
8. Apply damage/effect
```

---

## ⚙️ Customization

### Change Interaction Key

```csharp
// In Inspector or via code:
interactionSystem.interactKey = KeyCode.Q; // Change from E to Q
```

### Change Interaction Range

```csharp
// At runtime:
InteractionSystem.Instance.SetInteractionRange(5f); // 5 meters
```

### Add Custom Interaction Type

```csharp
// In InteractionSystem.cs, add new method:
public void PerformSpecialAction()
{
    // Your custom logic
    if (playerStats.CurrentStamina >= 10f)
    {
        playerStats.UseStamina(10f);
        // Do special action
        OnInteractionPerformed?.Invoke("Special_Action", playerController.gameObject);
    }
}

// Call from Update:
if (Input.GetKeyDown(KeyCode.Q))
{
    PerformSpecialAction();
}
```

### Change Attack Input

```csharp
// In Inspector:
Attack Key = Mouse 0 (default)

// Or in code:
interactionSystem.attackKey = KeyCode.J; // Change to J key
```

---

## 🐛 Troubleshooting

### Issue: Interaction prompt not showing

**Solution:**
1. Check if `InteractionSystem` exists in scene
2. Verify `interactionPromptText` is assigned in PlayerController
3. Check layer masks (should not block player layer)

### Issue: Can't interact with objects

**Solution:**
1. Ensure object has `IInteractable` component
2. Check interaction range (default 3m)
3. Verify object's collider is set to Trigger
4. Check layer mask includes object's layer

### Issue: Attacks not dealing damage

**Solution:**
1. Verify enemy has `Ennemi` component
2. Check enemy has collider
3. Ensure `CombatSystem` exists in scene
4. Verify raycast range (default 5m for weapons)

### Issue: Spell casting consumes no mana

**Solution:**
1. Check spell has `manaCost > 0`
2. Verify `PlayerStats` reference is valid
3. Ensure spell is equipped in inventory

---

## 📊 Performance Considerations

### Raycast Optimization

The InteractionSystem performs raycasts every frame. Optimize with:

```csharp
// Use layer masks to reduce raycast checks
[SerializeField] private LayerMask interactionLayerMask = ~0;

// Exclude unnecessary layers:
// Player, Projectiles, VFX, etc.
```

### Event Subscription Cleanup

Always unsubscribe in `OnDisable`:

```csharp
void OnDisable()
{
    if (InteractionSystem.Instance != null)
    {
        InteractionSystem.Instance.OnInteractionPerformed -= YourHandler;
    }
}
```

---

## 📝 Best Practices

1. **Always inherit from `InteractableObject`** for new interactables
2. **Use EventHandler** for logging/achievements
3. **Check resources before actions** (stamina, mana)
4. **Provide visual feedback** for interactions
5. **Keep interaction prompts short** (1-3 words)
6. **Test with low resources** (ensure proper failure handling)

---

## 🔗 Related Systems

| System | File | Description |
|--------|------|-------------|
| **EventHandler** | `EventHandler.cs` | Central event broadcasting |
| **CombatSystem** | `CombatSystem.cs` | Damage calculation & resource management |
| **PlayerStats** | `PlayerStats.cs` | Player stats wrapper |
| **Inventory** | `Inventory.cs` | Item & equipment management |
| **IInteractable** | `IInteractable.cs` | Interaction interface |

---

## 📜 Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-03-01 | Initial release |
| | | - Centralized E-key interactions |
| | | - Combat action handling |
| | | - Spell casting system |
| | | - Item usage integration |
| | | - EventHandler broadcasting |

---

**Generated:** 2026-03-01  
**Author:** Ocxyde  
**Project:** LavosTrial (PeuImporte)
