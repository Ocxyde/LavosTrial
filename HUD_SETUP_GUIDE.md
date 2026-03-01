# HUD Setup Guide - LavosTrial

**Last Updated:** March 2026

---

## What Was Fixed

### 1. Color System Updated
- **Old behavior:** Colors had alpha, didn't dim smoothly
- **New behavior:** 8-bit style colors (no alpha) that dim as resources are spent
- **Health:** Bright green (100%) → Yellow (50%) → Red (0%)
- **Mana:** Bright blue (100%) → Dark blue (0%)
- **Stamina:** Bright yellow (100%) → Dark brown (0%)

### 2. PlayerHealth/PlayerStats Integration
- **UIBarsSystem** now supports both systems
- Automatically detects which system is active
- Falls back to PlayerHealth if PlayerStats is not found

### 3. Scene Configuration Updated
- Updated `MainScene_Maze.unity` with new color values
- Bar thickness increased to 25 (more visible)
- Font size increased to 36 (better readability)

---

## Testing Instructions

### Step 1: Open Unity and Load Scene
```
1. Open Unity 6000.3.7f1
2. Load scene: Assets/Scenes/MainScene_Maze.unity
3. Press Play
```

### Step 2: Check Console for Debug Logs
You should see:
```
[UIBarsSystem] Awake - Instance initialized
[UIBarsSystem] Start - Building bars...
[UIBarsSystem] Bars built - Health: True, Mana: True, Stamina: True
[UIBarsSystem] Subscribed to PlayerHealth events (legacy mode)
[UIBarsSystem] Setting initial values...
[UIBarsSystem] Initial values from PlayerHealth - HP: 1000/1000
```

### Step 3: Verify Bars Are Visible
- **Health Bar:** Left edge of screen (vertical, green)
- **Mana Bar:** Right edge of screen (vertical, blue)
- **Stamina Bar:** Bottom edge of screen (horizontal, yellow)
- **Text:** White percentage in center of each bar

### Step 4: Test Color Changes (Optional)
Use the test script to verify color interpolation:

**Number Keys (1-0):** Change health
- `1` = 100% (bright green)
- `5` = 50% (yellow-orange)
- `9` = 10% (dark red)
- `0` = 5% (very dark red)

**QWER Keys:** Change mana
- `Q` = 100% (bright blue)
- `E` = 25% (medium blue)
- `R` = 10% (dark blue)

**ASDF Keys:** Change stamina
- `A` = 100% (bright yellow)
- `D` = 25% (medium yellow)
- `F` = 10% (dark brown)

---

## Files Modified

| File | Changes |
|------|---------|
| `UIBarsSystem.cs` | Color interpolation, PlayerHealth support, debug logs |
| `HUDSystem.cs` | Matching color system |
| `MainScene_Maze.unity` | Updated UIBarsSystem component values |
| `TestHUD.cs` | New test script (Editor folder) |

---

## Troubleshooting

### No Bars Visible
1. Check Console for errors
2. Verify UIBarsSystem is in scene (GameObject "UIBarsSystem")
3. Check Canvas is created (should be auto-created)

### Bars Not Updating
1. Check if PlayerHealth or PlayerStats exists in scene
2. Verify events are firing (check Console logs)
3. Try pressing test keys (1-9) to force update

### Colors Look Wrong
1. Select UIBarsSystem GameObject
2. Check color values in Inspector match:
   - Health High: RGB(0.2, 0.9, 0.3)
   - Health Low: RGB(0.9, 0.7, 0.1)
   - Health Critical: RGB(0.9, 0.1, 0.1)

---

## Expected Behavior

### Health Bar Color Interpolation
| Health % | Color | RGB Value |
|----------|-------|-----------|
| 100-60% | Bright Green | (0.2, 0.9, 0.3) |
| 60-30% | Yellow-Orange | (0.9, 0.7, 0.1) |
| 30-0% | Dark Red | (0.9, 0.1, 0.1) dims to black |

### Mana/Stamina Bar Color Interpolation
| Resource % | Color Behavior |
|------------|----------------|
| 100% | Full bright color |
| 50% | Halfway to dark |
| 0% | Dark color |

---

## Notes

- **Alpha = 0** on all bar colors (pure 8-bit RGB)
- **Smooth interpolation** - no sudden color jumps
- **Text shows percentage** (0-100%) for quick reading
- **Bars are responsive** - adjust to screen resolution automatically
