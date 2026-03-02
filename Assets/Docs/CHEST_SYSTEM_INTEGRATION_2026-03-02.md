# Chest System Integration - 2026-03-02

**Unity Version:** 6000.3.7f1 (Unity 6)  
**Files:** `ChestBehavior.cs`, `EventHandler.cs`, `ChestPixelArtFactory.cs`  
**Architecture:** Plug-in-and-Out via EventHandler  
**Date:** 2026-03-02

---

## Overview

The treasure chest system has been fully integrated into the plug-in-and-out architecture using `EventHandler` as the central event hub. The chest now:

1. **Raises events** through `EventHandler` for all actions
2. **Uses procedural pixel art** textures (8-bit style)
3. **Supports multiple chest types** (Standard, Gold, Iron)
4. **Integrates with loot system** via `LootTable`

---

## 🎯 Plug-in-and-Out Architecture

### Event Flow

```
┌─────────────────┐
│  Player Interacts │
└────────┬──────────┘
         │
         ▼
┌─────────────────┐
│ ChestBehavior   │
│  - Open()       │
│  - GenerateLoot()│
└────────┬──────────┘
         │ Calls
         ▼
┌─────────────────┐
│ EventHandler    │◄─── Single Point of Truth
│  - InvokeChest  │
│    Events       │
└────────┬──────────┘
         │ Broadcasts
         ▼
┌─────────────────────────────────────────┐
│  Subscribers (Plug-in components):      │
│  - HUDSystem (show loot notification)   │
│  - QuestSystem (update chest quests)    │
│  - AchievementSystem (unlock achievements)│
│  - AudioManager (play chest open sound) │
│  - ParticleSystem (spawn loot particles)│
└─────────────────────────────────────────┘
```

---

## 📋 New Chest Events in EventHandler

### Event Declarations

```csharp
#region Chest Events

public event Action<Vector3, int> OnChestOpened;
public event Action<Vector3> OnChestClosed;
public event Action<Vector3, int, GameObject> OnChestLootGenerated;
public event Action<Vector3, LootTable> OnChestItemSpawned;

#endregion
```

### Event Invokers

```csharp
// Open chest with gold amount
EventHandler.Instance.InvokeChestOpened(position, goldAmount);

// Close chest
EventHandler.Instance.InvokeChestClosed(position);

// Loot generated (with looter reference)
EventHandler.Instance.InvokeChestLootGenerated(position, goldAmount, looter);

// Item spawned from loot table
EventHandler.Instance.InvokeChestItemSpawned(position, lootTable);
```

---

## 🎨 Pixel Art Factory

### ChestPixelArtFactory

**Location:** `Assets/Scripts/Ressources/ChestPixelArtFactory.cs`

**Features:**
- Procedural 8-bit pixel art textures
- Three chest variants (Standard, Gold, Iron)
- Texture caching for performance
- Consistent with `PixelArtTextureFactory` patterns

### Usage

```csharp
// Get cached texture
Texture2D chestTex = ChestPixelArtFactory.GetStandardChestTexture();

// Or by type
Texture2D goldChestTex = ChestPixelArtFactory.GetChestTexture(ChestType.Gold);
Texture2D ironChestTex = ChestPixelArtFactory.GetChestTexture(ChestType.Iron);

// Clear cache (when resources need rebuild)
ChestPixelArtFactory.ClearCache();
```

### Color Palettes

#### Standard Chest
| Color | RGB | Usage |
|-------|-----|-------|
| WoodDark | (52, 28, 12) | Wood shadows |
| WoodMid | (88, 50, 20) | Base wood |
| WoodLight | (120, 75, 35) | Wood highlights |
| GoldBright | (255, 220, 60) | Gold trim |
| GoldDark | (200, 160, 40) | Gold accents |
| Iron | (70, 75, 85) | Metal bands |
| GemRed | (220, 40, 40) | Ruby gem |

#### Gold Chest
- Shiny gold body with sapphire gem
- Ornate decorative patterns
- Premium/rare variant

#### Iron Chest
- Dark iron plating with rivets
- Simple lock mechanism
- Common/basic variant

---

## 🔧 ChestBehavior Integration

### Updated Methods

#### Open()
```csharp
public void Open()
{
    // ... animation ...
    
    // Raise event through EventHandler
    if (raiseEvents && EventHandler.Instance != null)
    {
        EventHandler.Instance.InvokeChestOpened(transform.position, _goldAmount);
    }
}
```

#### GenerateLoot()
```csharp
private void GenerateLoot(GameObject looter)
{
    _goldAmount = Random.Range(minGold, maxGold + 1);
    
    // Raise loot event
    if (raiseEvents && EventHandler.Instance != null)
    {
        EventHandler.Instance.InvokeChestLootGenerated(
            transform.position, 
            _goldAmount, 
            looter
        );
    }
    
    // Spawn additional item
    if (Random.value < itemChance && lootTable != null)
    {
        EventHandler.Instance.InvokeChestItemSpawned(transform.position, lootTable);
    }
}
```

### Configuration

```csharp
[Header("Events")]
[SerializeField] private bool raiseEvents = true; // Toggle event raising
```

---

## 📝 Example: Subscribing to Chest Events

### HUD System Integration

```csharp
public class HUDSystem : MonoBehaviour
{
    private void OnEnable()
    {
        if (EventHandler.Instance != null)
        {
            EventHandler.Instance.OnChestOpened += OnChestOpened;
            EventHandler.Instance.OnChestLootGenerated += OnChestLootGenerated;
        }
    }

    private void OnDisable()
    {
        if (EventHandler.Instance != null)
        {
            EventHandler.Instance.OnChestOpened -= OnChestOpened;
            EventHandler.Instance.OnChestLootGenerated -= OnChestLootGenerated;
        }
    }

    private void OnChestOpened(Vector3 position, int goldAmount)
    {
        // Show floating text
        ShowFloatingText($"+{goldAmount} Gold", position);
    }

    private void OnChestLootGenerated(Vector3 position, int goldAmount, GameObject looter)
    {
        // Show notification
        ShowNotification($"Found {goldAmount} gold!");
    }
}
```

### Quest System Integration

```csharp
public class QuestSystem : MonoBehaviour
{
    private void OnEnable()
    {
        if (EventHandler.Instance != null)
        {
            EventHandler.Instance.OnChestOpened += HandleChestOpened;
        }
    }

    private void HandleChestOpened(Vector3 position, int goldAmount)
    {
        // Update quest progress
        QuestManager.Instance.IncrementQuest("OpenChests", 1);
        
        // Check for achievement
        if (goldAmount >= 50)
        {
            AchievementManager.Instance.Unlock("RichFinder");
        }
    }
}
```

### Audio Manager Integration

```csharp
public class AudioManager : MonoBehaviour
{
    private void OnEnable()
    {
        if (EventHandler.Instance != null)
        {
            EventHandler.Instance.OnChestOpened += PlayChestOpenSound;
            EventHandler.Instance.OnChestClosed += PlayChestCloseSound;
        }
    }

    private void PlayChestOpenSound(Vector3 position, int gold)
    {
        AudioSource.PlayClipAtPoint(chestOpenClip, position);
    }

    private void PlayChestCloseSound(Vector3 position)
    {
        AudioSource.PlayClipAtPoint(chestCloseClip, position);
    }
}
```

---

## 🎮 Testing

### In Unity Editor

1. **Create a chest:**
   ```csharp
   var chestObj = new GameObject("TreasureChest");
   var chest = chestObj.AddComponent<ChestBehavior>();
   chest.Initialize(1.5f, 1.2f, lootTable);
   ```

2. **Enable debug events:**
   - Set `EventHandler.debugEvents = true`
   - Interact with chest (press E)
   - Check Console for event logs

3. **Verify events:**
   ```
   [EventHandler] ChestOpened: 35 gold at (10.5, 0.0, 5.2)
   [EventHandler] ChestLootGenerated: 35 gold at (10.5, 0.0, 5.2)
   ```

### Via Script

```csharp
// Subscribe to events
EventHandler.Instance.OnChestOpened += (pos, gold) => {
    Debug.Log($"Chest opened with {gold} gold!");
};

// Open chest
chest.Open();
```

---

## 📐 Architecture Benefits

### Plug-in-and-Out Advantages

| Benefit | Description |
|---------|-------------|
| **Decoupling** | Chest doesn't need references to HUD, Audio, Quest systems |
| **Flexibility** | Add/remove subscribers without modifying chest code |
| **Scalability** | Multiple systems can react to same event |
| **Maintainability** | Single point of truth (EventHandler) |
| **Testability** | Easy to mock events for unit tests |

---

## 🔗 Related Files

| File | Purpose |
|------|---------|
| `ChestBehavior.cs` | Main chest behavior with events |
| `EventHandler.cs` | Central event hub (4 new chest events) |
| `ChestPixelArtFactory.cs` | Procedural texture generator |
| `PixelArtTextureFactory.cs` | Base pixel art patterns |
| `LootTable.cs` | Loot table definitions |
| `BehaviorEngine.cs` | Base class for items |

---

## 📊 Event Summary

| Event | Parameters | When Raised |
|-------|-----------|-------------|
| `OnChestOpened` | `Vector3 position, int goldAmount` | When chest lid opens |
| `OnChestClosed` | `Vector3 position` | When chest lid closes |
| `OnChestLootGenerated` | `Vector3 position, int goldAmount, GameObject looter` | When loot is generated |
| `OnChestItemSpawned` | `Vector3 position, LootTable lootTable` | When additional item spawns |

---

**Documentation saved:** `Assets/Docs/CHEST_SYSTEM_INTEGRATION_2026-03-02.md`  
**Unity 6 Compatible:** ✅  
**Plug-in-and-Out:** ✅  
**EventHandler Integrated:** ✅
