# UI Bars System - Quick Setup Guide

## Problem: No PlayerStats on Player GameObject

Your scene has `PlayerHealth` and `PlayerController` but not `PlayerStats`. Here's how to make the UI bars appear anyway.

---

## Solution 1: Use UIBarsSystemStandalone (Recommended - Quick Setup)

### Steps:

1. **In Unity Editor**, open your scene (`Assets/Scenes/MainScene_Maze.unity`)

2. **Create an empty GameObject**:
   - Right-click in Hierarchy → Create Empty
   - Name it "UIManager" or "GameManager"

3. **Add the UIBarsSystemStandalone component**:
   - Select the empty GameObject
   - In Inspector → Add Component → Search for "UIBarsSystemStandalone"
   - Click to add

4. **(Optional) Assign Player reference**:
   - Drag your Player GameObject into the "Player" field in Inspector
   - Or leave empty - it will auto-find by "Player" tag

5. **Play the scene** - You should see:
   - Health bar (left edge) - updates when you take damage
   - Mana bar (right edge) - shows 100/100 (default)
   - Stamina bar (bottom edge) - shows 100/100 (default)

### Console Logs to Watch For:
```
[UIBarsSystemStandalone] UIBarsSystem created
[UIBarsSystemStandalone] Linked to PlayerHealth on 'Player'
[UIBarsSystemStandalone] Initial values set
[UIBarsSystem] Awake - Instance initialized
[UIBarsSystem] Start - Building bars...
[UIBarsSystem] BuildBars - Starting...
[UIBarsSystem] Bars built with responsive screen-edge layout
```

---

## Solution 2: Add PlayerStats to Player (Full Features)

If you want full mana/stamina management with the existing `PlayerStats` system:

### Steps:

1. **Add PlayerStats component to Player**:
   - Select your Player GameObject
   - Add Component → Search for "PlayerStats"

2. **Configure PlayerStats**:
   - Set maxHealth, maxMana, maxStamina in Inspector
   - The UI bars will automatically link via PlayerStats

3. **Remove or keep UIBarsSystemStandalone**:
   - You can remove it (PlayerStats will spawn UIBarsSystem)
   - Or keep it as backup

---

## Troubleshooting

### Bars not appearing?

1. **Check Console for errors** - Fix any compilation errors first

2. **Verify Canvas exists**:
   - Look in Hierarchy for "UI_Canvas" or "HUDCanvas"
   - If missing, the system should create one automatically

3. **Check UI layer order**:
   - Canvas sorting order should be 100 (on top)
   - Canvas render mode: Screen Space - Overlay

4. **Verify components**:
   - PlayerHealth must exist on Player for health bar updates
   - Check that Player has the "Player" tag

### Health bar not updating?

- Make sure PlayerHealth.OnHealthChanged event is firing
- Check that damage/healing is being applied correctly

### Wrong positions?

- The bars use screen-edge positioning
- They should be at:
  - Health: Left edge, centered vertically
  - Mana: Right edge, centered vertically  
  - Stamina: Bottom edge, centered horizontally
  - Status Effects: Top center

---

## Files Created

| File | Purpose |
|------|---------|
| `UIBarsSystem.cs` | Main UI bars system (singleton) |
| `UIBarsSystemStandalone.cs` | Simple initializer (use this!) |
| `UIBarsSystemInitializer.cs` | Alternative initializer |
| `HUD_Disposition_Implementation.md` | Full documentation |

---

## Quick Test

1. Add `UIBarsSystemStandalone` to any GameObject
2. Press Play
3. You should see 3 colored bars around the screen edges

If it works, you're done! 🎉
