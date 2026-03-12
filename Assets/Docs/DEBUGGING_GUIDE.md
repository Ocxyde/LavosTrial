# Debugging Guide - Cell-Based Maze Generation

**Project:** CodeDotLavos
**Unity:** 6000.3.10f1
**Last Updated:** 2026-03-11
**Author:** Ocxyde

---

## 🐛 Common Issues & Solutions

### **1. Maze Not Generating**

**Symptoms:**
- Click "Generate Maze" but nothing happens
- Console shows no errors
- No maze appears in scene

**Checklist:**
- [ ] Check if `CompleteMazeBuilder8` component exists in scene
- [ ] Check if prefabs are assigned in inspector
- [ ] Check console for warnings (yellow messages)
- [ ] Verify `GameConfig` exists in Resources

**Solution:**
```
1. Select MazeBuilder GameObject
2. Check CompleteMazeBuilder8 component
3. Assign missing prefabs:
   - Wall Prefab
   - Door Prefab
   - Torch Prefab
4. Click "Generate Maze" again
```

---

### **2. Walls Not Appearing**

**Symptoms:**
- Maze generates but no walls visible
- Only floor plane visible
- Console shows "wallPrefab is NULL"

**Checklist:**
- [ ] Wall prefab assigned in CompleteMazeBuilder8
- [ ] Wall prefab exists in Resources/Prefabs/
- [ ] Wall material assigned

**Solution:**
```
Tools → Cell-Based Maze → Generate Maze (1-Click)
Ensure "Auto-Spawn Walls" is checked
```

---

### **3. Player Falls Through Floor**

**Symptoms:**
- Player spawns but falls through floor
- No collider on ground plane
- Console shows missing collider warnings

**Checklist:**
- [ ] Ground plane has MeshCollider
- [ ] Ground material has physics material
- [ ] Player has CharacterController

**Solution:**
```
1. Select GroundPlane in hierarchy
2. Add Component → MeshCollider
3. Check "Convex" if needed
4. Test again
```

---

### **4. Doors Not Interactable**

**Symptoms:**
- Press "F" but doors don't open
- No interaction prompt visible
- Console shows "DoorController missing"

**Checklist:**
- [ ] Door has DoorController component
- [ ] Door has Collider (for trigger detection)
- [ ] Player has DoorInteractionHandler

**Solution:**
```
1. Select door prefab
2. Add Component → DoorController
3. Add Component → BoxCollider (if missing)
4. Set collider as Trigger
5. Save prefab
```

---

### **5. Seed Not Changing**

**Symptoms:**
- Same maze generated every time
- Seed value doesn't change
- Console shows same seed repeatedly

**Checklist:**
- [ ] SeedManager exists in scene
- [ ] SeedManager not destroyed between scenes
- [ ] "useFixedSeed" is unchecked

**Solution:**
```
1. Check SeedManager component in scene
2. Uncheck "Use Fixed Seed" in CompleteMazeBuilder8
3. Or use 1-Click Generator (auto-generates new seed)
```

---

### **6. Performance Issues**

**Symptoms:**
- Maze generation takes >10 seconds
- Frame rate drops during generation
- Unity becomes unresponsive

**Checklist:**
- [ ] Maze size too large (>35x35)
- [ ] Too many decoy paths (>50)
- [ ] Too many rooms (>10)

**Solution:**
```
1. Reduce maze size (try 21x21)
2. Reduce decoy density (try 0.3)
3. Reduce room count (try 3)
4. Profile with Unity Profiler
```

---

## 🔍 Debug Tools

### **Console Commands**

Press `~` (tilde) to open console:

| Command | Description |
|---------|-------------|
| `maze.generate` | Generate new maze |
| `maze.status` | Show current maze info |
| `maze.clear` | Clear maze objects |
| `seed.show` | Show current seed |
| `seed.randomize` | Generate new seed |

---

### **Debug Logging**

Enable verbose logging in CompleteMazeBuilder8:

```csharp
[Header("Debug Settings")]
[SerializeField] private bool debugLogging = true;
```

**Console Output:**
```
[MazeBuilder8] === GENERATE MAZE STARTED ===
[MazeBuilder8] Step 1: Loading config...
[MazeBuilder8] Step 2+3: Validating assets...
[MazeBuilder8] Step 4: Cleanup...
[MazeBuilder8] Step 5: Ground...
[MazeBuilder8] Step 6: Walls...
[MazeBuilder8] Step 7: Doors...
[MazeBuilder8] Step 8: Torches...
[MazeBuilder8] Step 9: Objects...
[MazeBuilder8] Step 10: Player...
[MazeBuilder8] Done -- 245.67ms
```

---

### **Profiler**

**Unity Profiler Window:**
```
Window → Analysis → Profiler
```

**What to Check:**
- CPU Usage (should be <16ms per frame)
- Memory (should be <500MB)
- Rendering (should be <8ms per frame)

---

## 🧪 Testing Checklist

### **Pre-Test**
- [ ] Unity 6000.3.10f1 opened
- [ ] Scene has CompleteMazeBuilder8
- [ ] Console window open
- [ ] No errors before testing

### **Test 1: Basic Generation**
- [ ] Click "Generate Maze"
- [ ] Console shows generation steps
- [ ] Maze appears in scene
- [ ] No console errors

### **Test 2: Player Spawning**
- [ ] Player spawns at entry point
- [ ] Player doesn't fall through floor
- [ ] Player can move (WASD)
- [ ] Player can look (mouse)

### **Test 3: Door Interaction**
- [ ] Walk to door
- [ ] Press "F"
- [ ] Door opens (90° outward)
- [ ] Walk through doorway
- [ ] Press "F" to close

### **Test 4: Wall Collision**
- [ ] Walk into wall
- [ ] Player stops (doesn't clip)
- [ ] Walk along wall
- [ ] No clipping through corners

### **Test 5: Seed Variation**
- [ ] Generate maze (note seed)
- [ ] Generate again (different seed)
- [ ] Mazes are different layouts
- [ ] Console shows different seeds

---

## 📊 Performance Benchmarks

| Maze Size | Target Time | Acceptable Time |
|-----------|-------------|-----------------|
| 12x12 | <50ms | <100ms |
| 21x21 | <100ms | <200ms |
| 35x35 | <200ms | <500ms |
| 51x51 | <500ms | <1000ms |

**If slower than target:**
1. Reduce decoy density
2. Reduce room count
3. Check for infinite loops
4. Profile with Unity Profiler

---

## 🐞 Known Bugs

| Bug | Status | Workaround |
|-----|--------|------------|
| Door clipping on spawn | ⏳ Pending | Manually adjust door position |
| Perimeter walls missing | ✅ Fixed | Check "Surround with Perimeter Walls" |
| Seed not saving | ✅ Fixed | Uses MazeBinaryStorage8 now |
| Player spawns in wall | ✅ Fixed | Spawn offset to (1.5, 1.5) |

---

## 📝 Debug Log Locations

**Unity Editor:**
```
<ProjectRoot>/Logs/Editor.log
```

**Player Build:**
```
%APPDATA%\..\LocalLow\Ocxyde\LavosTrial\Player.log
```

**Maze Storage:**
```
<ProjectRoot>/Runtimes/Mazes/maze8_L{level}_S{seed}.lvm
```

---

## 🆘 Getting Help

**Before Asking:**
1. ✅ Check console for errors
2. ✅ Run through checklist above
3. ✅ Check this debugging guide
4. ✅ Search existing documentation

**When Asking for Help:**
- Provide console output (full log)
- Include Unity version
- Include maze settings (size, level, seed)
- Describe steps to reproduce

---

**License:** GPL-3.0
**Copyright © 2026 CodeDotLavos. All rights reserved.**

---

*Last Updated: 2026-03-11 | Unity 6000.3.10f1 | UTF-8 encoding - Unix LF*

**Motto:** "Happy debugging with me : Ocxyde :)"
