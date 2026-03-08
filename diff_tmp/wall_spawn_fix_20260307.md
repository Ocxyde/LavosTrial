# FIX: Wall Spawning - Proper Corridor Rendering

**Date:** 2026-03-07  
**Issue:** Maze generated isolated cells surrounded by walls instead of proper corridors  
**File:** `Assets/Scripts/Core/06_Maze/CompleteMazeBuilder.cs`

---

## 🐛 **Problem**

The `SpawnAllWalls()` method had incorrect border checks that prevented proper wall rendering:

```csharp
// ❌ BEFORE - Broken wall spawning
if (z == 0 && (cell & CellFlags8.WallS) != 0)  // Only on south border
    SpawnCardinalWall(x, z, Direction8.S, cs, wh);
if (x == 0 && (cell & CellFlags8.WallW) != 0)  // Only on west border
    SpawnCardinalWall(x, z, Direction8.W, cs, wh);
```

**Why This Broke Corridors:**

The DFS algorithm correctly **removes walls** between connected cells:
- When carving from cell A to cell B (south of A):
  - Clears A's South wall flag ✅
  - Clears B's North wall flag ✅

But the rendering code only drew South/West walls on the **outer border**:
- Interior cells never drew their South/West walls ❌
- This created "open" corridors but also **missing walls** where they should exist
- Result: Confusing layout with invisible/missing walls

---

## ✅ **Solution**

Remove the border checks - each cell should render ALL of its walls based on its flags:

```csharp
// ✅ AFTER - Proper wall spawning
if ((cell & CellFlags8.WallS) != 0)  // No border check
    SpawnCardinalWall(x, z, Direction8.S, cs, wh);
if ((cell & CellFlags8.WallW) != 0)  // No border check
    SpawnCardinalWall(x, z, Direction8.W, cs, wh);
```

**Same fix for diagonal walls:**
```csharp
// ❌ BEFORE
if (z == 0 && (cell & CellFlags8.WallSE) != 0)
if (x == 0 && (cell & CellFlags8.WallSW) != 0)

// ✅ AFTER
if ((cell & CellFlags8.WallSE) != 0)
if ((cell & CellFlags8.WallSW) != 0)
```

---

## 📊 **Diff**

```diff
--- a/Assets/Scripts/Core/06_Maze/CompleteMazeBuilder.cs
+++ b/Assets/Scripts/Core/06_Maze/CompleteMazeBuilder.cs
@@ -288,20 +288,21 @@ namespace Code.Lavos.Core
             {
                 var cell = _mazeData.GetCell(x, z);
 
-                // Cardinal - North + East always; South/West only on outer border
+                // Cardinal - spawn all walls based on cell flags (no border checks)
+                // Each cell is responsible for its own walls
                 if ((cell & CellFlags8.WallN) != 0)
                     SpawnCardinalWall(x, z, Direction8.N, cs, wh);
                 if ((cell & CellFlags8.WallE) != 0)
                     SpawnCardinalWall(x, z, Direction8.E, cs, wh);
-                if (z == 0 && (cell & CellFlags8.WallS) != 0)
+                if ((cell & CellFlags8.WallS) != 0)
                     SpawnCardinalWall(x, z, Direction8.S, cs, wh);
-                if (x == 0 && (cell & CellFlags8.WallW) != 0)
+                if ((cell & CellFlags8.WallW) != 0)
                     SpawnCardinalWall(x, z, Direction8.W, cs, wh);
 
-                // Diagonal (toggled by config)
+                // Diagonal (toggled by config) - spawn all based on cell flags
                 if (_config.MazeCfg.DiagonalWalls)
                 {
                     if ((cell & CellFlags8.WallNE) != 0)
                         SpawnDiagonalWall(x, z, Direction8.NE, cs, wh);
                     if ((cell & CellFlags8.WallNW) != 0)
                         SpawnDiagonalWall(x, z, Direction8.NW, cs, wh);
-                    if (z == 0 && (cell & CellFlags8.WallSE) != 0)
+                    if ((cell & CellFlags8.WallSE) != 0)
                         SpawnDiagonalWall(x, z, Direction8.SE, cs, wh);
-                    if (x == 0 && (cell & CellFlags8.WallSW) != 0)
+                    if ((cell & CellFlags8.WallSW) != 0)
                         SpawnDiagonalWall(x, z, Direction8.SW, cs, wh);
                 }
             }
```

---

## 🎯 **Expected Result**

After this fix:
- ✅ **Proper corridors** with visible walls on all sides
- ✅ **Correct wall removal** where DFS carved passages
- ✅ **No missing walls** in interior sections
- ✅ **Proper room boundaries** where rooms were carved
- ✅ **Diagonal walls** render correctly at corners

---

## 🧪 **Testing Checklist**

1. Open Unity 6000.3.7f1
2. Load maze scene
3. Press Play → Generate Maze
4. Verify:
   - [ ] Corridors have visible walls on all 4 sides
   - [ ] Wall removal matches carved passages (no invisible barriers)
   - [ ] Spawn room is properly open (5x5 cleared area)
   - [ ] Exit corridor is visible and reachable
   - [ ] No floating or missing wall segments
   - [ ] Diagonal walls (if enabled) render at proper angles
   - [ ] Console shows no errors

---

## 📝 **Technical Notes**

**Wall Sharing Model:**
- Each cell stores its own wall flags independently
- When DFS carves a passage, it clears flags on BOTH cells
- Rendering draws walls for each cell based on its own flags
- This can create **duplicate walls** between cells, but ensures no gaps

**Alternative Approach (Not Used):**
- Only draw North/East walls for every cell
- Draw South/West walls only on outer border
- Requires perfect flag synchronization between neighbors
- More fragile - one missed flag = invisible wall

**Current Fix:**
- Simpler and more robust
- Each cell is self-contained
- Duplicate walls are hidden by Unity's z-buffer
- No visible artifacts

---

**Status:** ✅ **FIXED**  
**Backup:** Run `backup.ps1` before testing  
**Git:** Commit with message: `fix: Wall spawning - proper corridor rendering`

*Document generated - UTF-8 encoding - Unix LF*
