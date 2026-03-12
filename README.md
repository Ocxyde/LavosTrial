﻿﻿﻿﻿﻿# CodeDotLavos - Modder's Guide

**A Unity 6 First-Person Maze Roguelike Engine**

**Unity Version:** 6000.3.10f1 | **License:** GPL-3.0 | **Author:** Ocxyde

---

## 🚀 Quick Start for Modders

### 5-Minute Setup

1. **Open Project**
   ```bash
   # Open Unity Hub → Add → Select project folder
   # Use Unity 6000.3.10f1 or compatible Unity 6 version
   ```

2. **Load Main Scene**
   - Open `Assets/Scenes/MazeLav8s_v1-0_1_7.unity`
   - Press **Play** to test

3. **Default Controls**
   | Action | Key |
   |--------|-----|
   | Move | WASD |
   | Look | Mouse |
   | Sprint | Shift (1% stamina/sec) |
   | Jump | Space (1% stamina/jump) |
   | Interact | F |

   **📝 Note:** Controls will be configurable in future update! See [KEYMAP_SYSTEM_PLANNED.md](KEYMAP_SYSTEM_PLANNED.md) for planned key binding system with keyboard, gamepad, and dodge mechanics.

---

## 📦 What You Can Mod

### ✅ Easy Mods (No Code)

| What | How | Files |
|------|-----|-------|
| **Change maze seed** | Edit in Inspector | Scene → MazeBuilder |
| **Adjust player stats** | Modify Health, Mana, Stamina | `PlayerStats` component |
| **Swap textures** | Replace material assets | `Assets/Materials/` |
| **Tweak colors** | Change material colors | Inspector |
| **Adjust difficulty** | Modify enemy spawn rates | `ProceduralLevelGenerator` |

### ⚙️ Medium Mods (Some Code)

| What | How | Files |
|------|-----|-------|
| **Add new items** | Create item JSON + prefab | `Assets/Resources/Items/` |
| **New enemy types** | Extend `EnemyController` | `Assets/Scripts/Ennemies/` |
| **Custom doors** | Modify door factory | `DoorFactory.cs` |
| **UI changes** | Edit HUD prefabs | `Assets/Prefabs/HUD/` |
| **Sound effects** | Add audio clips | `Assets/Audio/` |

### 🔨 Advanced Mods (C# Required)

| What | How | Files |
|------|-----|-------|
| **New maze algorithms** | Extend `GridMazeGenerator8` | `GridMazeGenerator8.cs` |
| **Custom room types** | Modify corridor system | `CorridorFillSystem.cs` |
| **New status effects** | Add to `StatusEffectData` | `StatusEffectData.cs` |
| **Combat mechanics** | Extend `CombatSystem` | `CombatSystem.cs` |
| **Save system changes** | Modify `DatabaseManager` | `DatabaseManager.cs` |

---

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── Core/                    # Core systems - DO NOT MODIFY unless extending
│   │   ├── 01_CoreSystems/      # GameManager, EventHandler (global state)
│   │   ├── 02_Player/           # PlayerController, CameraFollow
│   │   ├── 04_Inventory/        # Inventory management
│   │   ├── 06_Maze/             # 🎯 MAZE GENERATION (mod this!)
│   │   ├── 07_Doors/            # Door systems
│   │   ├── 13_Compute/          # Lighting, audio, effects
│   │   └── 15_Resources/        # Torch, chest, object behaviors
│   ├── HUD/                     # UI systems (bars, dialogs, windows)
│   ├── Status/                  # Buffs, debuffs, stats
│   ├── Ennemies/                # Enemy AI behaviors
│   └── Editor/                  # Unity editor tools
├── Resources/
│   ├── Prefabs/                 # 🎯 Add your prefabs here
│   ├── Items/                   # 🎯 Add item JSON definitions
│   └── Config/                  # Game configuration files
├── Materials/                   # Surface textures & colors
├── Textures/                    # Image assets
├── Audio/                       # Sound effects & music
├── Scenes/                      # Game scenes
└── Docs/                        # Documentation (you are here!)
```

**🎯 = Good starting points for modding**

---

## 🎯 Core Architecture

### Plug-in-Out System (How Components Connect)

This project uses a **loose-coupling architecture**: components find each other, they don't create each other.

**Why this matters for modders:**
- ✅ You can add new scripts without breaking existing ones
- ✅ Components auto-discover each other at runtime
- ✅ Easy to test mods in isolation

```csharp
// ✅ CORRECT - How to find other components
private void Awake()
{
    // Find existing component (don't create new one!)
    var player = FindFirstObjectByType<PlayerController>();
}

// ❌ WRONG - Don't do this
private void Awake()
{
    var player = gameObject.AddComponent<PlayerController>(); // Creates duplicate!
}
```

### Key Components

| Component | Purpose | Modding Notes |
|-----------|---------|---------------|
| `GameManager` | Central game state | Singleton - extend, don't replace |
| `EventHandler` | Global event hub | Subscribe to events, don't modify |
| `PlayerController` | Player movement | Use New Input System |
| `CompleteMazeBuilder` | Maze orchestrator | Main entry for maze mods |
| `GridMazeGenerator8` | Maze algorithm | DFS + A* - extend for custom algos |
| `SpatialPlacer` | Object placement | Handles spawning objects |
| `LightPlacementEngine` | Torch lighting | Auto-registers light sources |

---

## 🔧 Modding Tutorials

### Tutorial 1: Add a New Item

**Step 1:** Create item JSON in `Assets/Resources/Items/`

```json
{
  "id": "my_custom_potion",
  "name": "Custom Healing Potion",
  "description": "Restores 50 HP over 5 seconds",
  "type": "Consumable",
  "maxStack": 99,
  "effects": [
    {
      "type": "HealOverTime",
      "value": 10,
      "duration": 5.0,
      "tickRate": 1.0
    }
  ]
}
```

**Step 2:** Create prefab (optional)
- Right-click `Assets/Resources/Prefabs/` → Create → Prefab
- Add `ItemBehavior` component
- Configure properties

**Step 3:** Test
- Run game
- Use console command or spawn in scene

---

### Tutorial 2: Create Custom Maze Algorithm

**Step 1:** Create new script `Assets/Scripts/Core/06_Maze/MyCustomMazeGenerator.cs`

```csharp
using UnityEngine;
using Code.Lavos.Core;

namespace Code.Lavos.Core.Maze
{
    /// <summary>
    /// Custom maze generator using your own algorithm.
    /// </summary>
    public class MyCustomMazeGenerator : GridMazeGenerator8
    {
        [Header("Custom Settings")]
        [SerializeField] private float myCustomParameter = 0.5f;

        protected override void GenerateMazeInternal()
        {
            // Your algorithm here
            Debug.Log($"[MyCustomMaze] Generating with param: {myCustomParameter}");

            // Call base methods as needed
            base.GenerateMazeInternal();
        }
    }
}
```

**Step 2:** Assign in Inspector
- Select `MazeBuilder` GameObject
- Replace `GridMazeGenerator8` component with `MyCustomMazeGenerator`

**Step 3:** Test
- Press Play
- Watch Console for your log messages

---

### Tutorial 3: Add New Status Effect

**Step 1:** Edit `Assets/Scripts/Status/StatusEffectData.cs`

```csharp
public enum StatusEffectType
{
    // Existing effects...
    Burn,
    Freeze,
    Poison,

    // Add your new effect
    MyCustomBuff,      // ✅ Add here
    MyCustomDebuff,    // ✅ Add here
}
```

**Step 2:** Add effect logic in `StatusEffect.cs`

```csharp
public class StatusEffect : MonoBehaviour
{
    private void ApplyEffect(StatusEffectType type, float value)
    {
        switch (type)
        {
            case StatusEffectType.MyCustomBuff:
                // Your effect logic
                target.Stats.AddModifier("Damage", value);
                break;

            case StatusEffectType.MyCustomDebuff:
                // Your debuff logic
                target.Stats.AddModifier("Speed", -value);
                break;
        }
    }
}
```

---

### Tutorial 4: Modify Player Stats

**In Unity Inspector:**

1. Select `Player` GameObject
2. Find `PlayerStats` component
3. Modify values:
   - **Base Health:** 100 → 200
   - **Base Mana:** 50 → 100
   - **Stamina Regen:** 5 → 10

**Or via code:**

```csharp
// In a custom script
var stats = FindFirstObjectByType<PlayerStats>();
if (stats != null)
{
    stats.SetBaseHealth(200);
    stats.SetBaseMana(100);
    stats.SetStaminaRegenRate(10);
}
```

---

## 🎮 Game Systems Reference

### Player Stats System

| Stat | Default | Description | Modifiable |
|------|---------|-------------|------------|
| Health | 100 | Player HP | ✅ Yes |
| Mana | 50 | Resource for abilities | ✅ Yes |
| Stamina | 100 | Sprint/jump resource | ✅ Yes |
| Strength | 10 | Physical damage | ✅ Yes |
| Agility | 10 | Speed, crit chance | ✅ Yes |
| Intelligence | 10 | Magic power | ✅ Yes |
| Vitality | 10 | HP regen | ✅ Yes |
| Dexterity | 10 | Accuracy, dodge | ✅ Yes |

### Damage Types

| Type | Effectiveness | Resistance |
|------|---------------|------------|
| Physical | Normal | Armor |
| Fire | Strong vs Nature | Weak vs Water |
| Ice | Strong vs Fire | Weak vs Lightning |
| Lightning | Strong vs Ice | Weak vs Earth |
| Nature | Strong vs Lightning | Weak vs Fire |
| Water | Strong vs Fire | Weak vs Nature |
| Holy | Strong vs Dark | Weak vs Dark |
| Dark | Strong vs Holy | Weak vs Holy |

### Status Effects

| Effect | Type | Duration | Stackable |
|--------|------|----------|-----------|
| Burn | DoT | 5 sec | ✅ Yes |
| Freeze | Debuff | 3 sec | ❌ No |
| Poison | DoT | 10 sec | ✅ Yes |
| Haste | Buff | 15 sec | ✅ Yes |
| Slow | Debuff | 8 sec | ❌ No |
| Shield | Buff | 20 sec | ❌ No |

---

## 🛠️ Development Tools

### PowerShell Scripts

Located in project root:

| Script | Purpose | Usage |
|--------|---------|-------|
| `backup.ps1` | Smart backup | Runs automatically on git commit |
| `cleanup_diff_tmp.ps1` | Clean old temp files | Run daily |
| `git-commit.ps1` | Quick commit | `.\git-commit.ps1 "message"` |
| `git-push.ps1` | Push to remote | `.\git-push.ps1` |

### Git Workflow

```bash
# Make your changes, then:

# 1. Stage all changes
git add -A

# 2. Commit (auto-backup)
.\git-commit.ps1 "Added custom potion item"

# 3. Push to remote
.\git-push.ps1
```

---

## 📖 Documentation Index

| File | Purpose | For |
|------|---------|-----|
| [README.md](README.md) | This file - modder's guide | **Modders** |
| [TODO.md](TODO.md) | Task list & roadmap | Developers |
| [ARCHITECTURE_OVERVIEW.md](ARCHITECTURE_OVERVIEW.md) | System architecture | Advanced modders |
| [PROJECT_STANDARDS.md](PROJECT_STANDARDS.md) | Coding standards | Developers |
| [MAZE_TROUBLESHOOTING_GUIDE.md](MAZE_TROUBLESHOOTING_GUIDE.md) | Maze issues | Debugging |
| [TEST_CHECKLIST.md](TEST_CHECKLIST.md) | Testing guide | QA |

---

## 🐛 Troubleshooting

### Common Issues

**Unity won't compile:**
```bash
# Clear Unity cache
.\clear-unity-cache.bat
# Reopen Unity
```

**Maze not generating:**
1. Check `CompleteMazeBuilder` has config assigned
2. Verify seed is valid number
3. Check Console for errors

**Player falls through floor:**
1. Check colliders on floor prefabs
2. Verify player has Rigidbody
3. Check layer collision matrix

**UI not showing:**
1. Check `Canvas` is enabled in scene
2. Verify `UIBarsSystem` component exists
3. Check sorting order (should be 100+)

**Input not working:**
1. Project Settings → Input Manager → Enable New Input System
2. Check `InputSystem_Actions.inputactions` exists
3. Restart Unity

---

## 📞 Getting Help

### Before Asking

1. ✅ Check Console for errors
2. ✅ Run `.\scan-project-errors.ps1`
3. ✅ Search existing documentation
4. ✅ Check Unity Editor.log

### Useful Commands

```bash
# Check project health
.\scan-project-errors.ps1

# View git status
.\git-status.ps1

# Clean temporary files
.\cleanup_diff_tmp.ps1
```

### Resources

- **Unity Docs:** https://docs.unity3d.com/
- **New Input System:** https://docs.unity3d.com/Packages/com.unity.inputsystem@latest
- **URP:** https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest

---

## 🎯 Next Steps

### Beginner Path

1. ✅ Run the game as-is
2. ✅ Modify player stats in Inspector
3. ✅ Change maze seed
4. ✅ Swap textures/materials
5. ✅ Add simple item

### Intermediate Path

1. ✅ Create custom enemy type
2. ✅ Modify maze generation parameters
3. ✅ Add new status effect
4. ✅ Create custom UI panel
5. ✅ Extend existing system

### Advanced Path

1. ✅ Write custom maze algorithm
2. ✅ Implement new damage type
3. ✅ Create multiplayer system
4. ✅ Mod the save/load system
5. ✅ Fork and maintain your own branch

---

## 📜 License

**GPL-3.0** - You are free to:
- ✅ Use for personal projects
- ✅ Use for commercial projects
- ✅ Modify and distribute
- ✅ Fork and maintain

**Requirements:**
- 📄 Include GPL-3.0 license in derivatives
- 🔓 Open source your modifications
- 📝 Credit original authors

See [COPYING](../../COPYING) for full license text.

---

**Happy Modding! 🎮✨**

*Last Updated: 2026-03-10 | Unity 6000.3.10f1 | Author: Ocxyde*
