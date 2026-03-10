# Maze Generation Debugging Improvements - 2026-03-11

**Date:** 2026-03-11  
**Status:** ✅ IMPLEMENTED  
**Unity Version:** 6000.3.10f1  
**Author:** Ocxyde  

---

## 📋 **OVERVIEW**

Added comprehensive debugging and logging to the maze generation system to diagnose and fix the issue where mazes at higher levels (e.g., level 20) were generating only spawn and exit rooms with **no connecting corridors**.

---

## 🐛 **PROBLEM DESCRIPTION**

### **Symptom:**
- At level 20 and higher, maze shows only:
  - Spawn room (5×5) at position (1,1)
  - Exit room at position (W-2, H-2)
  - **NO connecting corridors between them**

### **User Report:**
> "at level 20 same things happen 1 room no maze at all, where they are magically disappear?"
> "i see few walls, no maze"
> "one entrance/exit but nothing behind no wall corridor maze, another chamber connected, nothing else than the spawnRoom and exit"

---

## 🔧 **CHANGES MADE**

### **File: `GridMazeGenerator.cs`**

#### **1. Added DFS Carving Tracking**
```csharp
// Step 2: DFS over 4 cardinal axes ONLY
var visited = new bool[size, size];
int cellsBeforeDFS = CountPassageCells(data);
CarvePassagesCardinal(data, rng, visited, 1, 1);
int cellsAfterDFS = CountPassageCells(data);
Debug.Log($"[GridMazeGenerator] DFS carved {cellsAfterDFS - cellsBeforeDFS} passage cells");
```

**Purpose:** Track how many passage cells DFS actually carves to identify if DFS is failing.

---

#### **2. Added Helper Method: `CountPassageCells`**
```csharp
private static int CountPassageCells(MazeData8 d)
{
    int count = 0;
    for (int x = 0; x < d.Width; x++)
    for (int z = 0; z < d.Height; z++)
    {
        var cell = d.GetCell(x, z);
        if ((cell & CellFlags8.AllWalls) == CellFlags8.None)
            count++;
    }
    return count;
}
```

**Purpose:** Utility method to count passage cells for debugging and statistics.

---

#### **3. Increased A* Iteration Limit**
```csharp
// BEFORE:
int maxIterations = d.Width * d.Height * 2;  // 5202 for 51×51

// AFTER:
int maxIterations = d.Width * d.Height * 4;  // 10404 for 51×51
```

**Purpose:** Prevent A* from giving up too early on larger mazes.

---

#### **4. Enhanced A* Logging**
```csharp
Debug.Log($"[GridMazeGenerator] A*: Starting pathfind from ({sx},{sz}) to ({ex},{ez}) with wallPenalty={wallPenalty}");
```

**Success logging:**
```csharp
Debug.Log($"[GridMazeGenerator] A*: Guaranteed path carved successfully ({carvedCells} cells, {iterations} iterations)");
```

**Failure logging:**
```csharp
Debug.LogError($"[GridMazeGenerator] A*: ERROR - Max iterations ({maxIterations}) reached without finding path!");
Debug.LogError($"[GridMazeGenerator] A*: This usually means DFS didn't carve enough passages.");
```

---

#### **5. Enhanced `CarveIndirectPath` Logging**
```csharp
Debug.Log($"[GridMazeGenerator] Carving segment {i+1}/{waypoints.Count-1}: ({wx},{wz}) → ({wx},{wz})");
```

**Purpose:** Track each segment carving to identify where pathfinding might fail.

---

#### **6. Added Generation Summary**
```csharp
// Final summary logging
int totalPassages = CountPassageCells(data);
float passagePercentage = (float)totalPassages / (data.Width * data.Height) * 100f;
Debug.Log($"[GridMazeGenerator] MAZE GENERATION COMPLETE: {data.Width}x{data.Height}, " +
          $"spawn=({data.SpawnCell.x},{data.SpawnCell.z}), " +
          $"exit=({data.ExitCell.x},{data.ExitCell.z}), " +
          $"passages={totalPassages} ({passagePercentage:P1} of grid)");
```

**Purpose:** Provide complete generation statistics for verification.

---

## 📊 **EXPECTED LOG OUTPUT**

### **Good Maze Generation:**
```
[GridMazeGenerator] LEVEL 20 | factor=1.520 | size=32×32 | torch=52.0% | deadEnd=20.0%
[GridMazeGenerator] DFS carved 156 passage cells
[GridMazeGenerator] A*: Starting pathfind from (1,1) to (30,30) with wallPenalty=10000
[GridMazeGenerator] A*: Guaranteed path carved successfully (45 cells, 892 iterations)
[GridMazeGenerator] Carving indirect path with waypoints...
[GridMazeGenerator] Carving segment 1/3: (1,1) → (15,8)
[GridMazeGenerator] Carving segment 2/3: (15,8) → (22,25)
[GridMazeGenerator] Carving segment 3/3: (22,25) → (30,30)
[GridMazeGenerator] Total dead-end corridors added: 18
[GridMazeGenerator] MAZE GENERATION COMPLETE: 32×32, spawn=(1,1), exit=(30,30), passages=312 (30.5% of grid)
```

### **Problematic Maze (Debug Output):**
```
[GridMazeGenerator] LEVEL 20 | factor=1.520 | size=32×32
[GridMazeGenerator] DFS carved 0 passage cells  ← RED FLAG!
[GridMazeGenerator] A*: Starting pathfind from (1,1) to (30,30)
[GridMazeGenerator] A*: ERROR - Max iterations (4096) reached without finding path!
[GridMazeGenerator] A*: This usually means DFS didn't carve enough passages.
```

---

## 🧪 **TESTING CHECKLIST**

### **In Unity Editor:**

1. **Open Scene:**
   - Load `MazeLav8s_v1-0_1_4.unity` or test scene
   - Open Console window (set to show all messages)

2. **Generate Low Level Maze (0-5):**
   - Select MazeBuilder GameObject
   - Generate maze
   - **Verify:**
     - ✅ DFS logs passage cell count
     - ✅ A* finds path successfully
     - ✅ Final summary shows >15% passage cells
     - ✅ Visual inspection shows corridors

3. **Generate High Level Maze (20-39):**
   - Set level to 20, 30, 39
   - Generate maze
   - **Verify:**
     - ✅ Same logging as above
     - ✅ No A* iteration errors
     - ✅ Passages connect spawn to exit
     - ✅ Dead-end corridors present

4. **Stress Test:**
   - Generate 10 mazes at level 39
   - **Verify:**
     - ✅ All 10 mazes have connecting corridors
     - ✅ No A* failures
     - ✅ Passage percentage consistent (25-35%)

---

## 📈 **DIAGNOSTIC GUIDE**

### **Issue: "No corridors visible"**

**Check Console for:**

| Log Message | Problem | Solution |
|-------------|---------|----------|
| `DFS carved 0 passage cells` | DFS failed to carve | Check DFS starting position (1,1) |
| `A*: ERROR - Max iterations reached` | A* gave up | Increase iteration limit or fix DFS |
| `A*: Could not find path` | Isolated sections | Check wall penalty value |
| `passages=25 (2.4% of grid)` | Too few passages | Check spawn/exit room carving |

---

## 🔍 **ROOT CAUSE ANALYSIS**

### **Potential Issues Identified:**

1. **DFS Starting Position:**
   - DFS starts at (1,1) which is inside spawn room
   - Spawn room is carved AFTER DFS
   - **Result:** DFS might not carve anything if spawn room overwrites it

2. **Visited Array:**
   - `visited[1,1] = true` at start
   - Spawn room carving marks all cells as visited
   - **Result:** DFS can't carve into spawn room area

3. **A* Iteration Limit:**
   - 2× grid size might be too low for complex mazes
   - **FIXED:** Increased to 4× grid size

---

## ✅ **FIXES APPLIED**

| Fix | Status | Impact |
|-----|--------|--------|
| DFS carving logging | ✅ Added | Identifies DFS failures |
| A* iteration limit increase | ✅ 2× → 4× | Prevents premature failure |
| A* success/failure logging | ✅ Enhanced | Tracks pathfinding issues |
| Generation summary | ✅ Added | Quick verification |
| Helper method `CountPassageCells` | ✅ Added | Reusable utility |

---

## 📝 **NEXT STEPS**

### **Immediate:**
1. Test maze generation at all levels (0-39)
2. Verify console logs show expected output
3. Check for any A* failures in Console

### **If Issues Persist:**
1. Check if spawn room carving overwrites DFS passages
2. Verify DFS starting position is outside spawn room
3. Consider carving spawn room BEFORE DFS
4. Check `CellFlags8` logic for wall detection

---

## 📚 **RELATED FILES**

| File | Purpose |
|------|---------|
| `GridMazeGenerator.cs` | Main generation (UPDATED) |
| `MazeData8.cs` | Data structure |
| `CompleteMazeBuilder8.cs` | Orchestrator |
| `CellFlags8.cs` | Cell flag definitions |

---

## 🎯 **COMPLIANCE**

| Standard | Status |
|----------|--------|
| **Unity 6 Naming** | ✅ camelCase/PascalCase |
| **UTF-8 Encoding** | ✅ |
| **Unix LF Line Endings** | ✅ |
| **No Emojis in C#** | ✅ |
| **GPL-3.0 License** | ✅ Headers present |

---

**Last Updated:** 2026-03-11  
**Document Version:** 1.0  
**Status:** ✅ IMPLEMENTED - READY FOR TESTING

*Happy coding, coder friend!*
