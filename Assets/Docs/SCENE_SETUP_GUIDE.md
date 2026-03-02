# Scene Setup Guide - PeuImporte

**Location:** `Assets/Docs/SCENE_SETUP_GUIDE.md`  
**Unity Version:** 6000.3.7f1 (Unity 6)  
**Last Updated:** 2026-03-02  
**Status:** ✅ **READY FOR PRODUCTION**

---

## Quick Start (Auto-Setup)

### Method 1: Editor Menu (Recommended)

1. Open Unity Editor
2. Go to **Tools > PeuImporte > Verify/Setup Scene**
3. Click **⚡ Auto-Setup Missing Components**
4. Wait for completion message
5. Press **Play** to test

### Method 2: Manual Setup

Follow the detailed steps below for manual configuration.

---

## Complete Scene Setup

### Step 1: Core Systems (Singletons)

These systems must exist in every scene and persist across scene loads.

#### 1.1 GameManager
```
GameObject: "GameManager"
└── Component: GameManager.cs
    - Singleton: Yes (DontDestroyOnLoad)
    - Purpose: Game state management, scene transitions, score tracking
```

**Setup:**
1. Create empty GameObject
2. Name it "GameManager"
3. Add `GameManager.cs` component
4. ✓ Auto-configures as singleton

#### 1.2 EventHandler
```
GameObject: "EventHandler"
└── Component: EventHandler.cs
    - Singleton: Yes (DontDestroyOnLoad)
    - Purpose: Central event hub for all game systems
```

**Setup:**
1. Create empty GameObject
2. Name it "EventHandler"
3. Add `EventHandler.cs` component
4. ✓ Auto-configures as singleton

#### 1.3 ItemEngine
```
GameObject: "ItemEngine"
└── Component: ItemEngine.cs
    - Singleton: Yes (DontDestroyOnLoad)
    - Purpose: Central item registry (plug-in-and-out architecture)
```

**Setup:**
1. Create empty GameObject
2. Name it "ItemEngine"
3. Add `ItemEngine.cs` component
4. ✓ Auto-configures as singleton

---

### Step 2: Player GameObject

The player is the main character controlled by the user.

```
GameObject: "Player" [Tag: Player]
├── Component: CharacterController
│   - Skin Width: 0.08
│   - Min Move Distance: 0.001
├── Component: PlayerController.cs
│   - Player Camera: [assign PlayerCamera]
│   - Mouse Sensitivity: 0.2
│   - Walk Speed: 5
│   - Sprint Speed: 9
│   - Jump Height: 1.5
├── Component: PlayerStats.cs
│   - Max Health: 1000
│   - Max Mana: 50
│   - Max Stamina: 100
├── Component: Inventory.cs
│   - Slot Count: 10
└── Component: CombatSystem.cs
    - Base Damage: 10
    - Crit Chance: 0.05
    - Crit Multiplier: 1.5

Child: "PlayerCamera"
└── Component: Camera
    - Tag: MainCamera
    - Local Position: (0, 0.75, 0)
```

**Setup:**

1. **Create Player GameObject**
   - GameObject > Create Empty
   - Name: "Player"
   - Tag: "Player" (create tag if needed)

2. **Add CharacterController**
   - Add Component > CharacterController
   - Configure:
     - Skin Width: `0.08`
     - Min Move Distance: `0.001`

3. **Add PlayerController**
   - Add Component > PlayerController.cs
   - Configure:
     - Player Camera: Drag PlayerCamera child
     - Mouse Sensitivity: `0.2`
     - Walk Speed: `5`
     - Sprint Speed: `9`
     - Jump Height: `1.5`

4. **Add PlayerStats**
   - Add Component > PlayerStats.cs
   - Configure base stats in Inspector

5. **Add Inventory**
   - Add Component > Inventory.cs
   - Configure slot count

6. **Add CombatSystem**
   - Add Component > CombatSystem.cs
   - Configure damage values

7. **Create Camera Child**
   - Right-click Player > Create Empty
   - Name: "PlayerCamera"
   - Add Camera component
   - Set as Main Camera
   - Local Position: `(0, 0.75, 0)`
   - Assign to PlayerController

---

### Step 3: UI System

The UI displays health, mana, stamina bars and other HUD elements.

```
GameObject: "Canvas"
├── Component: Canvas
│   - Render Mode: Screen Space - Overlay
├── Component: Canvas Scaler
│   - UI Scale Mode: Scale With Screen Size
│   - Reference Resolution: 1920x1080
└── Component: Graphic Raycaster

GameObject: "UIBarsSystem"
└── Component: UIBarsSystem.cs
    - Auto-creates all UI elements at runtime

GameObject: "HUDSystem"
└── Component: HUDSystem.cs
    - Manages status effects, hotbar, dialogs

GameObject: "HUDEngine"
└── Component: HUDEngine.cs
    - Modular HUD system (alternative to HUDSystem)
```

**Setup:**

1. **Create Canvas**
   - GameObject > UI > Canvas
   - Configure Canvas Scaler:
     - UI Scale Mode: `Scale With Screen Size`
     - Reference Resolution: `1920 x 1080`

2. **Add UIBarsSystem**
   - Create empty GameObject
   - Name: "UIBarsSystem"
   - Add `UIBarsSystem.cs` component
   - ✓ Auto-creates health/mana/stamina bars at runtime

3. **Add HUDSystem**
   - Create empty GameObject
   - Name: "HUDSystem"
   - Add `HUDSystem.cs` component

4. **Add HUDEngine** (Optional - alternative to HUDSystem)
   - Create empty GameObject
   - Name: "HUDEngine"
   - Add `HUDEngine.cs` component

---

### Step 4: Maze System (Procedural Generation)

The maze system generates procedural dungeons with rooms and doors.

```
GameObject: "Maze"
├── Component: MazeGenerator.cs
│   - Width: 50
│   - Height: 50
│   - Cell Size: 4
├── Component: RoomGenerator.cs
│   - Min Room Size: 3
│   - Max Room Size: 8
├── Component: DoorHolePlacer.cs
│   - Hole Width: 2.2
│   - Hole Height: 3.5
├── Component: RoomDoorPlacer.cs
│   - Door Variants: [assign door prefabs]
├── Component: MazeRenderer.cs
│   - Cell Size: 4
│   - Wall Height: 3
├── Component: MazeIntegration.cs
│   - Auto-generate on start: Yes
├── Component: SeedManager.cs
│   - Use Random Seed: Yes
├── Component: DrawingPool.cs
│   - Pool Size: 100
└── Component: SpawnPlacerEngine.cs
    - Auto-place on start: Yes
    - Chest Density: 0.05
    - Trap Density: 0.08
```

**Setup:**

1. **Create Maze GameObject**
   - Create empty GameObject
   - Name: "Maze"

2. **Add All Components**
   - Add all components listed above
   - Configure values as shown

3. **Configure Door Variants** (on RoomDoorPlacer)
   - Create door prefabs (see Door System section)
   - Assign to Door Variants array

4. **Configure Spawn Prefabs** (on SpawnPlacerEngine)
   - Assign chest, trap, pickup prefabs

---

## Verification

### Use Auto-Verification Tool

1. **Tools > PeuImporte > Verify/Setup Scene**
2. Click **🔍 Verify Scene Setup**
3. Check all items show ✅
4. Click **⚡ Auto-Setup Missing Components** if any ❌

### Manual Verification Checklist

- [ ] GameManager exists and is singleton
- [ ] EventHandler exists and receives events
- [ ] ItemEngine exists and registers items
- [ ] Player has CharacterController
- [ ] Player has PlayerController
- [ ] Player has PlayerStats
- [ ] Player has Camera assigned
- [ ] Player tag is "Player"
- [ ] Canvas exists with Screen Space - Overlay
- [ ] UIBarsSystem exists
- [ ] HUDSystem exists
- [ ] Maze has all required components
- [ ] Press Play - no console errors

---

## Testing

### Movement Test
1. Press Play
2. Press **WASD** to move
3. Hold **Left Shift** to sprint
4. Press **Space** to jump
5. Move mouse to look around

### UI Test
1. Check health bar (left border, vertical, red)
2. Check mana bar (right border, vertical, blue)
3. Check stamina bar (bottom border, horizontal, yellow)
4. Damage/Heal should show floating text

### Interaction Test
1. Approach interactable object
2. Look at it
3. Press **E** to interact
4. Check interaction prompt appears

### Maze Generation Test
1. Press Play
2. Wait for maze generation
3. Check rooms are created
4. Check doors are placed
5. Walk through maze

---

## Troubleshooting

### Player doesn't move
- Check Player tag is set to "Player"
- Check CharacterController is not disabled
- Check Input System is installed (Package Manager)

### Camera doesn't follow
- Check PlayerCamera is child of Player
- Check PlayerController has camera assigned
- Check Camera component is enabled

### UI bars don't show
- Check Canvas render mode is Screen Space - Overlay
- Check UIBarsSystem component is enabled
- Check Canvas sorting order is positive (e.g., 100)

### Maze doesn't generate
- Check MazeGenerator is on Maze GameObject
- Check Auto Generate on Start is enabled
- Check SeedManager has valid seed

### Doors don't appear
- Check RoomDoorPlacer has door variants assigned
- Check DoorHolePlacer hole size matches door size
- Check RoomGenerator creates rooms before door placement

---

## Scene Hierarchy Example

```
Scene
├── GameManager (DontDestroyOnLoad)
├── EventHandler (DontDestroyOnLoad)
├── ItemEngine (DontDestroyOnLoad)
├── Player [Tag: Player]
│   ├── CharacterController
│   ├── PlayerController
│   ├── PlayerStats
│   ├── Inventory
│   ├── CombatSystem
│   └── PlayerCamera (MainCamera)
├── Canvas
│   ├── Canvas Scaler
│   └── Graphic Raycaster
├── UIBarsSystem
├── HUDSystem
├── HUDEngine
└── Maze
    ├── MazeGenerator
    ├── RoomGenerator
    ├── DoorHolePlacer
    ├── RoomDoorPlacer
    ├── MazeRenderer
    ├── MazeIntegration
    ├── SeedManager
    ├── DrawingPool
    └── SpawnPlacerEngine
```

---

## Save/Load Scene Setup

To save your configured scene:

1. **File > Save As...**
2. Name: `MainScene.unity`
3. Location: `Assets/Scenes/`

To load in build settings:

1. **File > Build Settings**
2. Add `MainScene.unity` to Scenes In Build
3. Set as first scene (index 0)

---

## Next Steps

After scene setup is complete:

1. ✅ Test core movement loop
2. ✅ Test UI bars update correctly
3. ✅ Test maze generation
4. ⏭️ Add enemy AI (TODO.md v1.2)
5. ⏭️ Add collectibles (TODO.md v1.2)
6. ⏭️ Add chests and loot (TODO.md v1.2)

---

## Related Documentation

| Document | Location |
|----------|----------|
| TODO.md | `Assets/Docs/TODO.md` |
| PROJECT_HIERARCHY.md | `Assets/Docs/PROJECT_HIERARCHY_2026-03-02.md` |
| HUD Setup | `Assets/Docs/HUD_Disposition_Implementation.md` |
| Door System | `Assets/Docs/DOOR_SYSTEM.md` |
| Seed System | `Assets/Docs/SEED_SYSTEM.md` |

---

**Last Reviewed:** 2026-03-02  
**Status:** ✅ **PRODUCTION READY**  
**Unity Version:** 6000.3.7f1

---

*Document generated automatically - Unity 6 compatible - UTF-8 encoding - Unix LF*
