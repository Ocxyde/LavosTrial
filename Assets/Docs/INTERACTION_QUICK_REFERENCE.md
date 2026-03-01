# InteractionSystem - Quick Reference Card

**For:** Developers & Designers  
**Unity:** 6000.3.7f1  
**Namespace:** `Code.Lavos.Core`

---

## ⌨️ Default Controls

| Action | Key | Cost |
|--------|-----|------|
| **Interact** | `E` | None |
| **Attack** | `Left Click` | Stamina (weapon) or None (unarmed) |
| **Cast Spell** | `Right Click` | Mana |
| **Use Item** | `F` | Item-specific |

---

## 🎯 Quick Start

### Create Interactable Object (3 steps)

```csharp
// 1. Inherit from InteractableObject
public class MyObject : InteractableObject
{
    // 2. Override InteractionPrompt
    public override string InteractionPrompt => "Do Thing";
    
    // 3. Implement OnInteract
    public override void OnInteract(PlayerController player)
    {
        Debug.Log("Interacted!");
    }
}
```

**Done!** Player can now interact with E key.

---

## 📦 Create Spell (2 steps)

```csharp
// 1. Create ScriptableObject
[CreateAssetMenu(menuName = "Spells/Fireball")]
public class FireballSpell : ScriptableObject
{
    public int manaCost = 20;
    public float damage = 35f;
}

// 2. Assign to inventory spell slot
```

**Done!** Right-click to cast (mana check automatic).

---

## ⚔️ Create Weapon (2 steps)

```csharp
// 1. Create ScriptableObject
[CreateAssetMenu(menuName = "Weapons/Iron Sword")]
public class IronSword : ScriptableObject
{
    public float damage = 15f;
    public float staminaCost = 8f;
}

// 2. Add to inventory & equip
```

**Done!** Left-click to attack (stamina check automatic).

---

## 🔔 Subscribe to Events

```csharp
void OnEnable()
{
    InteractionSystem.Instance.OnInteractionPerformed += OnInteract;
    InteractionSystem.OnInteractableChangedStatic += UpdatePrompt;
}

void OnDisable()
{
    InteractionSystem.Instance.OnInteractionPerformed -= OnInteract;
    InteractionSystem.OnInteractableChangedStatic -= UpdatePrompt;
}
```

---

## 🛠️ Common Tasks

### Change Interaction Range
```csharp
InteractionSystem.Instance.SetInteractionRange(5f);
```

### Change Interact Key
```csharp
interactionSystem.interactKey = KeyCode.Q;
```

### Force Interaction
```csharp
InteractionSystem.Instance.Interact();
```

### Force Attack
```csharp
InteractionSystem.Instance.Attack();
```

### Clear Current Target
```csharp
InteractionSystem.Instance.ClearInteraction();
```

---

## ✅ Checklist for New Interactables

- [ ] Inherit from `InteractableObject`
- [ ] Override `InteractionPrompt` property
- [ ] Implement `OnInteract()` method
- [ ] Add Collider (set to Trigger)
- [ ] Test interaction range (default 3m)
- [ ] Add audio feedback (optional)
- [ ] Add visual feedback (optional)

---

## 🐛 Quick Debug

**Prompt not showing?**
```csharp
// Check in Inspector:
// 1. PlayerController has interactionPromptText assigned
// 2. Object has IInteractable component
// 3. Collider exists and is Trigger
```

**Can't interact?**
```csharp
// Check:
// 1. Distance < interactionRange (3m default)
// 2. Layer mask includes object layer
// 3. CanInteract() returns true
```

**Attack not working?**
```csharp
// Check:
// 1. CombatSystem exists in scene
// 2. Enemy has Ennemi component
// 3. Enemy has Collider
// 4. Stamina > weapon cost (if equipped)
```

---

## 📊 Default Values

| Setting | Default |
|---------|---------|
| Interaction Range | 3.0 m |
| Attack Range (weapon) | 5.0 m |
| Attack Range (unarmed) | 3.0 m |
| Spell Range | 10.0 m |
| Stamina Regen | 0.15/sec (0.225 OOC) |
| Sprint Cost | 2.0/sec |
| Jump Cost | 5.0 |
| Unarmed Damage | 2.0 |

---

## 🔗 Full Documentation

See: `Assets/Scripts/Core/INTERACTION_SYSTEM_DOCUMENTATION.md`

---

**Print this page for quick reference!**
