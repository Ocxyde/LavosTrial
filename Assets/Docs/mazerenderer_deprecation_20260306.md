# MazeRenderer Deprecation Notice

**Date:** 2026-03-06
**Status:** ⚠️ **DEPRECATED**
**Replacement:** `CompleteMazeBuilder.cs`

---

## ⚠️ **DEPRECATION NOTICE**

**`MazeRenderer.cs` is DEPRECATED and will be removed in a future version.**

All maze generation functionality has been moved to `CompleteMazeBuilder.cs`.

---

## 🔄 **MIGRATION GUIDE**

### **Old Way (DEPRECATED):**
```csharp
// ❌ DON'T DO THIS
var mazeRenderer = GetComponent<MazeRenderer>();
mazeRenderer.BuildMaze();
```

### **New Way (CORRECT):**
```csharp
// ✅ DO THIS
var mazeBuilder = GetComponent<CompleteMazeBuilder>();
mazeBuilder.GenerateMaze();
```

---

## 📋 **WHAT CHANGED**

| Feature | Old (MazeRenderer) | New (CompleteMazeBuilder) |
|---------|-------------------|--------------------------|
| **Ground** | `BuildGeometry()` | `SpawnGround()` |
| **Walls** | `BuildGeometry()` | `SpawnOuterWalls()` |
| **Objects** | Manual placement | `PlaceObjects()` |
| **Player Spawn** | `SpawnPlayer()` | `SpawnPlayer()` |
| **Config** | Inspector fields | JSON config |
| **Architecture** | Mixed | Plug-in-out compliant |

---

## 🎯 **WHY DEPRECATE MazeRenderer?**

### **Problems with MazeRenderer:**
1. ❌ **Duplicate functionality** - Built geometry that CompleteMazeBuilder now handles
2. ❌ **Hardcoded values** - Used Inspector fields instead of JSON config
3. ❌ **Not plug-in-out compliant** - Created components directly
4. ❌ **Confusing architecture** - Multiple systems doing the same thing

### **Benefits of CompleteMazeBuilder:**
1. ✅ **Single source of truth** - One system for all maze generation
2. ✅ **JSON config driven** - All values from `GameConfig-default.json`
3. ✅ **Plug-in-out compliant** - Finds components, never creates them
4. ✅ **Clean architecture** - Clear separation of concerns

---

## 🔧 **AFFECTED FILES**

### **Files Using MazeRenderer (Need Migration):**

| File | Usage | Action Required |
|------|-------|-----------------|
| `MazeIntegration.cs` | `mazeRenderer.BuildMaze()` | Update to use CompleteMazeBuilder |
| `FpsMazeTest.cs` | Component reference | Keep for backward compat |
| `MazeTorchTest.cs` | Component reference | Keep for backward compat |
| `TestMazeWithTorches.cs` | Component reference | Keep for backward compat |

**Note:** Test files can keep MazeRenderer references for backward compatibility. New code should use CompleteMazeBuilder.

---

## 📝 **DEPRECATION WARNINGS**

When `MazeRenderer` is used, the following warnings will appear in the console:

```
[MazeRenderer] ⚠️ DEPRECATED: Use CompleteMazeBuilder for maze generation!
[MazeRenderer] ⚠️ DEPRECATED: Add CompleteMazeBuilder component instead!
[MazeRenderer] BuildMaze() called - DEPRECATED! Use CompleteMazeBuilder instead.
```

These warnings are **intentional** to remind developers to migrate to the new system.

---

## ✅ **MIGRATION CHECKLIST**

### **For New Code:**
- [ ] Use `CompleteMazeBuilder` for all maze generation
- [ ] Do NOT add `MazeRenderer` to new scenes
- [ ] Do NOT call `MazeRenderer.BuildMaze()`
- [ ] Use JSON config for all values

### **For Existing Code:**
- [ ] Review all `MazeRenderer` usage
- [ ] Migrate to `CompleteMazeBuilder` where possible
- [ ] Keep `MazeRenderer` for backward compatibility if needed
- [ ] Add deprecation warnings to legacy code

### **For Test Files:**
- [ ] Test files can keep `MazeRenderer` for now
- [ ] Update tests to use `CompleteMazeBuilder` when convenient
- [ ] No urgent migration required for tests

---

## 🗓️ **REMOVAL TIMELINE**

| Version | Status |
|---------|--------|
| **Current** | Deprecated, kept for backward compatibility |
| **Next Major** | Removal candidate |
| **Future** | Will be removed entirely |

---

## 📚 **DOCUMENTATION**

### **CompleteMazeBuilder Usage:**

```csharp
// Add CompleteMazeBuilder to scene
// Required components (add independently):
// - MazeGenerator (or GridMazeGenerator)
// - SpatialPlacer
// - LightPlacementEngine
// - TorchPool
// - EventHandler

// Generate maze (editor):
// Right-click CompleteMazeBuilder → "Generate Maze"
// OR press Ctrl+Alt+G

// Generate maze (runtime):
var builder = GetComponent<CompleteMazeBuilder>();
builder.GenerateMaze();

// Console commands (runtime):
// maze.generate     → Generate new maze
// maze.verbosity    → Set verbosity level
// maze.status       → Show current status
```

### **JSON Configuration:**

All values are loaded from `Config/GameConfig-default.json`:

```json
{
    "defaultMazeWidth": 21,
    "defaultMazeHeight": 21,
    "defaultCellSize": 6.0,
    "defaultRoomSize": 5,
    "defaultCorridorWidth": 2,
    "defaultGridSize": 21
}
```

---

## 🎯 **SUMMARY**

**`MazeRenderer.cs` is deprecated.**

**Use `CompleteMazeBuilder.cs` for all new maze generation.**

**Legacy code will continue to work but will show deprecation warnings.**

---

*Document generated - Unity 6 (6000.3.7f1) compatible - UTF-8 encoding - Unix LF*
