# 🎯 A-MAZE-LAV8S 2.0.0 - SCENE SETUP RESUME

**Quick Reference Guide - Screenshot This!**

---

## ✅ FIXED: SetupMazeScene_v2.cs

**Location:** `Assets/Scripts/Editor/SetupMazeScene_v2.cs`

**Usage:**
```
Unity Editor → Tools → Setup Maze Scene v2.0.0
```

---

## 📦 WHAT'S CREATED AUTOMATICALLY

```
┌─────────────────────────────────────────────────────────────┐
│              A-MAZE-LAV8S 2.0.0 SCENE HIERARCHY             │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ✅ GameConfig           (Maze settings, JSON config)       │
│  ✅ EventHandler         (Global event hub, 40+ events)     │
│  ✅ GameManager          (Game state, singleton)            │
│  ✅ MazeGenerator        (CompleteMazeBuilder8 - MAIN)      │
│  ✅ LightEngine          (Dynamic lighting - 927 lines)     │
│  ✅ TorchPool            (Torch object pooling)             │
│  ✅ SpatialPlacer        (Chests, enemies, items)           │
│  ✅ LightPlacementEngine (Torch auto-placement on walls)    │
│  ✅ DoorsEngine          (Door behavior system)             │
│  ✅ Canvas               (UI root, URP compatible)          │
│  │   ├── EventSystem    (UI interactions)                  │
│  │   └── UIBarsSystem   (HUD: health, mana, stamina)       │
│  ✅ Directional Light    (Sun, URP shadows)                 │
│                                                             │
│  ⏳ Player               (Spawned by CompleteMazeBuilder8)  │
│      └── Main Camera     (FPS, eye level Y=1.7m)            │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔧 COMPONENTS BREAKDOWN

| # | Component | Purpose | Lines |
|---|-----------|---------|-------|
| 1 | **GameConfig** | Maze settings, JSON config | ~200 |
| 2 | **EventHandler** | Global event hub (40+ events) | ~400 |
| 3 | **GameManager** | Game state, singleton | ~150 |
| 4 | **CompleteMazeBuilder8** | Main orchestrator | ~800 |
| 5 | **LightEngine** | Dynamic lighting system | 927 |
| 6 | **TorchPool** | Torch object pooling | ~200 |
| 7 | **SpatialPlacer** | Object placement (chests, enemies) | ~300 |
| 8 | **LightPlacementEngine** | Torch auto-placement on walls | ~250 |
| 9 | **DoorsEngine** | Door behavior, animations | ~200 |
| 10 | **Canvas** | UI root (URP overlay) | - |
| 11 | **EventSystem** | UI interactions (New Input) | - |
| 12 | **UIBarsSystem** | HUD bars (health, mana, stamina) | ~400 |

**Total:** ~3,827 lines of core systems

---

## 🎯 KEY FEATURES

### ✅ Plug-in-Out Architecture
```csharp
// Components FIND each other, never CREATE each other
private void Awake()
{
    // ✅ CORRECT
    lightEngine = FindFirstObjectByType<LightEngine>();
    
    // ❌ WRONG
    lightEngine = gameObject.AddComponent<LightEngine>();
}
```

### ✅ All Values From JSON
```json
// Config/GameConfig-default.json
{
  "CellSize": 6,
  "WallHeight": 4,
  "TorchChance": 0.3,
  "ChestDensity": 0.03,
  "EnemyDensity": 0.05,
  "DeadEndDensity": 0.1
}
```

### ✅ Dead-End Corridor Scaling
```
Level 0:  30% density → 12-18 dead-ends (21×21 maze)
Level 10: 45% density → 15-22 dead-ends
Level 20: 60% density → 20-28 dead-ends
Level 39: 75% density → 24-35 dead-ends
```

### ✅ A* Pathfinding Guarantee
```
• Spawn → Exit path ALWAYS guaranteed
• A* carves corridor if DFS fails
• Iteration limit prevents infinite loops
• Max iterations: Width × Height × 2
```

---

## 🔥 QUICK COMMANDS

### Create Scene
```
Unity Editor → Tools → Setup Maze Scene v2.0.0
```

### Generate Maze
```
Press Play in Unity Editor
OR
CompleteMazeBuilder8 → Inspector → Generate Maze (context menu)
```

### Console Commands (Press ~)
```
maze.export         → Copy maze code to clipboard
maze.import [code]  → Import and generate maze
maze.share          → Share with QR code
```

---

## 📊 SCENE CHECKLIST

```
BEFORE PRESSING PLAY:
☐ GameConfig in scene
☐ EventHandler in scene
☐ GameManager in scene
☐ CompleteMazeBuilder8 in scene
☐ LightEngine in scene
☐ TorchPool in scene
☐ SpatialPlacer in scene
☐ LightPlacementEngine in scene
☐ DoorsEngine in scene
☐ Canvas + EventSystem in scene
☐ UIBarsSystem in scene
☐ Directional Light in scene

NOT IN SCENE (spawned at runtime):
☐ Player (spawned by CompleteMazeBuilder8)
☐ Walls (spawned by CompleteMazeBuilder8)
☐ Doors (spawned by CompleteMazeBuilder8)
☐ Torches (spawned by LightPlacementEngine)
☐ Chests/Enemies (spawned by SpatialPlacer)
```

---

## 🐛 TROUBLESHOOTING

| Issue | Solution |
|-------|----------|
| **No maze generates** | Check CompleteMazeBuilder8 has prefabs assigned |
| **Player falls through floor** | Verify FloorPrefab has BoxCollider |
| **No torches appear** | Check TorchPool and LightPlacementEngine present |
| **No HUD bars** | Verify Canvas + UIBarsSystem in scene |
| **Player controls don't work** | Check EventSystem + InputSystemUIInputModule |
| **Dark maze** | Verify LightEngine component present |
| **Doors don't open** | Check DoorsEngine in scene |

---

## ⚠️ REMINDERS

```
☐ Run backup.ps1 after ANY file change
☐ Git commit: git add -A → commit → backup.ps1
☐ Run cleanup_diff_tmp.ps1 daily (delete files >2 days)
☐ Test in Unity 6000.3.10f1
☐ Verify 0 compilation errors before testing
```

---

**Generated:** 2026-03-10  
**Author:** Ocxyde  
**License:** GPL-3.0  
**Unity:** 6000.3.10f1  
**Encoding:** UTF-8 Unix LF

---

*Happy coding with me : Ocxyde :)*
