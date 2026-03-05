# CompleteMazeBuilder - Test Checklist

**Version:** 1.0
**Date:** 2026-03-05
**Tester:** You! 👋

---

## 🎯 **PRE-TEST SETUP**

### **1. Verify Files Exist:**
```
✅ Config/GameConfig-default.json
✅ Saves/ folder exists
✅ Assets/Scripts/Core/06_Maze/CompleteMazeBuilder.cs
✅ Assets/Scripts/Core/06_Maze/MazeSaveData.cs
✅ Assets/Scripts/Core/06_Maze/GameConfig.cs
✅ Manuals/MANUAL.md
```

### **2. Unity Setup:**
```
✅ Open Unity 6000.3.7f1
✅ Wait for compilation to finish
✅ No console errors
```

---

## 🧪 **TEST 1: FIRST MAZE GENERATION**

### **Steps:**
1. In Unity Editor
2. Menu: `Tools → Maze → Generate Maze` (Ctrl+Alt+G)
3. Wait for generation to complete

### **Expected Results:**
```
✅ Console shows:
   - "[CompleteMazeBuilder] 🏗️ Starting maze generation..."
   - "[CompleteMazeBuilder] 📐 Set MazeGenerator size: 21x21"
   - "[CompleteMazeBuilder] 🏛️ Placing X rooms FIRST..."
   - "[CompleteMazeBuilder] 🧱 Walls: XXX inner + 84 outer = XXX segments"
   - "[CompleteMazeBuilder] 💾 Maze saved to SQLite: Seed=XXXXX"

✅ Hierarchy shows:
   - MazeBuilder GameObject
   - GroundFloor
   - Ceiling
   - MazeWalls (or similar)
   - Doors
   - Torches

✅ No pink textures!
✅ No console errors!
```

### **Pass Criteria:**
- [ ] Maze generates without errors
- [ ] No pink textures
- [ ] Console shows generation logs
- [ ] SQLite save created (Saves/MazeDB.sqlite)

---

## 🎮 **TEST 2: PLAY MODE**

### **Steps:**
1. Press Play button
2. Wait for player to spawn
3. Check console logs

### **Expected Results:**
```
✅ Console shows:
   - "[CompleteMazeBuilder] 👤 SpawnPlayer() called"
   - "[CompleteMazeBuilder] 📦 First-time game - loading defaults from JSON config..."
   - "[CompleteMazeBuilder] 🎯 Spawn cell (X, Y) → World position: (XX, 0.9, XX)"
   - "[CompleteMazeBuilder] 👤 Player spawned INSIDE entrance room"

✅ Player spawns:
   - Inside a room (not in walls!)
   - At eye height (can look around)
   - On ground (not floating)

✅ Controls work:
   - WASD = Move
   - Mouse = Look around
   - Space = Jump
   - Shift = Sprint
```

### **Pass Criteria:**
- [ ] Player spawns inside room (not in walls)
- [ ] Can move with WASD
- [ ] Can look around with mouse
- [ ] No clipping issues
- [ ] No console errors

---

## 🏰 **TEST 3: MAZE EXPLORATION**

### **Steps:**
1. Walk around the maze
2. Try to find exit
3. Check various features

### **Expected Results:**
```
✅ Maze features:
   - Walls are solid (can't walk through)
   - Doors exist (some open, some locked)
   - Torches provide light
   - Rooms are clear space (3x3 cells)
   - Corridors connect rooms
   - Ceiling covers everything (no sky visible)
   - Outer walls fully enclosed

✅ Performance:
   - Smooth movement (no lag)
   - No frame drops
   - Lighting works
```

### **Pass Criteria:**
- [ ] Can navigate maze
- [ ] Walls are solid
- [ ] Doors work
- [ ] Torches light up
- [ ] No sky gaps
- [ ] Smooth performance

---

## 💾 **TEST 4: SAVE SYSTEM**

### **Steps:**
1. Play for a bit
2. Stop Play mode
3. Press Play again

### **Expected Results:**
```
✅ Second play session:
   - Console shows: "[CompleteMazeBuilder] 💾 Loaded maze data: Seed=XXXXX"
   - Same seed as first session
   - Player spawns at same position
   - Maze layout is identical
```

### **Pass Criteria:**
- [ ] Save file created (Saves/MazeDB.sqlite)
- [ ] Second play loads same seed
- [ ] Same maze layout
- [ ] Player spawns at same position

---

## ⚙️ **TEST 5: CONFIG MODDING**

### **Steps:**
1. Stop Play mode
2. Open `Config/GameConfig-default.json`
3. Change `defaultMazeWidth` from 21 to 15
4. Save file
5. Delete `Saves/MazeDB.sqlite`
6. Generate new maze
7. Press Play

### **Expected Results:**
```
✅ Console shows:
   - "[CompleteMazeBuilder] 📦 First-time game - loading defaults from JSON config..."
   - "[CompleteMazeBuilder] 📐 Set MazeGenerator size: 15x21"

✅ Maze is:
   - 15 cells wide (instead of 21)
   - 21 cells tall
   - Smaller maze
```

### **Pass Criteria:**
- [ ] Config changes apply
- [ ] Maze size changes
- [ ] No errors
- [ ] New seed generated

---

## 🧹 **TEST 6: RAM CLEANUP**

### **Steps:**
1. Press Play
2. Play for 30 seconds
3. Press Alt+F4 (or close Unity)
4. Check console before close

### **Expected Results:**
```
✅ Console shows:
   - "[CompleteMazeBuilder] 🧹 Releasing RAM on game quit..."
   - "[CompleteMazeBuilder] 💾 Player settings saved on quit"
   - "[CompleteMazeBuilder] ✅ RAM released - clean quit"
```

### **Pass Criteria:**
- [ ] RAM cleanup message appears
- [ ] Player settings saved
- [ ] Clean exit
- [ ] No memory leaks

---

## 🎯 **FINAL CHECKLIST**

### **Must Pass:**
- [ ] Test 1: First Maze Generation
- [ ] Test 2: Play Mode
- [ ] Test 3: Maze Exploration

### **Should Pass:**
- [ ] Test 4: Save System
- [ ] Test 5: Config Modding

### **Nice to Have:**
- [ ] Test 6: RAM Cleanup

---

## 🐛 **BUG REPORTING**

If you find issues, note:
1. **Test number** that failed
2. **Console error messages** (copy/paste)
3. **Steps to reproduce**
4. **Expected vs actual behavior**

---

## 🎉 **GOOD LUCK, TESTER!**

**You're about to test something AMAZING!** 🚀

**Have fun exploring your maze!** 🏰⚔️

---

**Test Checklist Generated:** 2026-03-05
**Status:** ✅ READY FOR TESTING
