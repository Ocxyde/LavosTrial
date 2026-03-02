# Chest Pixel Art Enhancement - 2026-03-02

**Unity Version:** 6000.3.7f1 (Unity 6)  
**File:** `Assets/Scripts/Core/ChestBehavior.cs`  
**Style:** 8-bit Pixel Art  
**Texture Size:** 32x32 pixels  
**Date:** 2026-03-02

---

## Overview

Enhanced the procedural treasure chest texture generation with a **cool 8-bit pixel art cube design** featuring vibrant colors, decorative elements, and classic treasure chest aesthetics.

---

## 🎨 Color Palette

### Wood Tones
| Color | RGB | Usage |
|-------|-----|-------|
| Dark Brown | (52, 28, 12) | Wood grain shadows |
| Medium Brown | (88, 50, 20) | Base wood planks |
| Light Brown | (120, 75, 35) | Wood highlights |

### Metal & Trim
| Color | RGB | Usage |
|-------|-----|-------|
| Bright Gold | (255, 220, 60) | Gold trim, studs |
| Dark Gold | (200, 160, 40) | Gold accents, lock plate |
| Iron | (70, 75, 85) | Metal bands, straps |
| Iron Highlight | (120, 130, 145) | Rivet details |

### Gem
| Color | RGB | Usage |
|-------|-----|-------|
| Ruby Red | (220, 40, 40) | Center gem |
| Gem Glow | (255, 100, 100) | Gem aura |

---

## 📐 Design Layout (32x32 grid)

```
┌────────────────────────────┐
│ ══════════════════════════ │  ← Lid: Gold trim border (checkered)
│   ✚  DECORATIVE LID     ✚  │  ← Gold stud patterns
│        ◆ RUBY ◆           │  ← Center ruby gem with glow
│══════════════════════════│  ← Lid/body seam
│▓▓▓ CHEST BODY ▓▓▓        │
│▓▓▓ [====LOCK====] ▓▓▓    │  ← Ornate lock plate
│▓▓▓       ⊕       ▓▓▓     │  ← Keyhole (cross shape)
│▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓│
└────────────────────────────┘
```

### Sections

| Section | Y Range | Features |
|---------|---------|----------|
| **Lid** | 0-12 (40%) | Gold trim, decorative patterns, ruby gem |
| **Body** | 13-31 (60%) | Wood planks, metal bands, lock plate |

---

## ✨ Visual Features

### Lid Design
- **Gold trim border** with checkered pattern (bright/dark gold)
- **Decorative gold studs** in horizontal pattern
- **Center ruby gem** with diamond shape and glow aura

### Body Design
- **Horizontal wood planks** with varied grain pattern
- **Top and bottom metal bands** with rivet details (every 8 pixels)
- **Vertical metal straps** on left, right, and center
- **Ornate lock plate** with:
  - Gold background
  - Iron keyhole surround
  - Dark cross-shaped keyhole opening
  - Gold studs around perimeter

### 3D Effects
- **Edge highlighting** on left edge (lightened 30 units)
- **Edge shadow** on right edge (darkened 30 units)
- Creates depth perception for cube shape

---

## 🔧 Code Changes

### New Methods
```csharp
private static Color32 LightenColor(Color32 c, int amount)
private static Color32 DarkenColor(Color32 c, int amount)
```

### Enhanced Texture Generation
- Split into **lid** and **body** sections
- Procedural wood grain with plank rows
- Metal band patterns with rivets
- Lock plate with keyhole detail
- Gem placement with distance-based coloring

---

## 🎮 In-Game Appearance

The chest renders as a **3D cube** with:
- **Procedurally generated** 8-bit texture on all faces
- **Glowing aura** (golden point light)
- **Pulsing glow effect** (sin wave animation)
- **Animated lid** that opens on interaction
- **Loot generation** from attached LootTable

---

## 📋 Testing

To see the enhanced chest:

1. **In Unity Editor:**
   - Create empty GameObject
   - Add `ChestBehavior` component
   - Press Play to see procedural generation

2. **Via Code:**
   ```csharp
   var chest = new GameObject("TreasureChest");
   var behavior = chest.AddComponent<ChestBehavior>();
   behavior.Initialize(1.5f, 1.2f, lootTable);
   ```

3. **Interact:**
   - Approach within 3 units
   - Press interaction key (E)
   - Lid opens, loot generates, glow intensifies

---

## 📝 Notes

- **Texture Format:** RGBA32, Point filter (pixel-perfect)
- **Material:** URP Lit shader (compatible with standard render pipeline)
- **Performance:** Procedural generation at runtime (cached)
- **Memory:** Properly cleaned up in `OnDestroy()`

---

## 🔗 Related Files

| File | Purpose |
|------|---------|
| `ChestBehavior.cs` | Main chest behavior with pixel art generation |
| `LootTable.cs` | Loot table definitions for chest contents |
| `BehaviorEngine.cs` | Base class for item behaviors |
| `ItemEngine.cs` | Central item registry |

---

**Documentation saved:** `Assets/Docs/CHEST_PIXEL_ART_2026-03-02.md`  
**Generated:** 2026-03-02  
**Unity 6 Compatible:** ✅
