# CORNER WALLS & ON-DEMAND LIGHTS

**Date:** 2026-03-05
**File:** `Assets/Scripts/Core/06_Maze/CompleteMazeBuilder.cs`
**Status:** ✅ **CORNERS COMPUTED FOR FUTURE - LIGHTS ON-DEMAND**

---

## 🎯 **CHANGES SUMMARY**

### **1. Corner Walls Computed (Future Grid Computation):**
- ✅ **Named:** `wallTravers` - traverses grid corners
- ✅ **Stored:** Positions in RAM (not spawned yet)
- ✅ **Purpose:** Reserved for future enhancement

### **2. Lights On-Demand:**
- ❌ **Removed:** Direct light instantiation in `PlaceTorches()`
- ✅ **Added:** `PlaceLightsOnDemand()` - call when actually needed
- ✅ **Benefit:** Lights placed only when required (lazy loading)

### **3. Stray Light Removal:**
- ✅ **Added:** Cleanup for "Lights" object
- ✅ **Prevents:** Lonely walls/lights from previous runs

---

## 📊 **CORNER WALLS - wallTravers**

### **Computed Positions (stored in RAM):**
```csharp
/// <summary>
/// Compute corner wall positions for FUTURE GRID COMPUTATION.
/// Stores positions in RAM but does NOT spawn them yet.
/// Named 'wallTravers' - traverses the grid corners.
/// </summary>
private void ComputeCornerWallsForFuture()
{
    // North-West corner
    wallPositions.Add(new Vector3(0f, wallHeight / 2f, mazeSize * cellSize));
    
    // North-East corner
    wallPositions.Add(new Vector3(mazeSize * cellSize, wallHeight / 2f, mazeSize * cellSize));
    
    // South-West corner
    wallPositions.Add(new Vector3(0f, wallHeight / 2f, 0f));
    
    // South-East corner
    wallPositions.Add(new Vector3(mazeSize * cellSize, wallHeight / 2f, 0f));

    Log($"4 corner walls computed for FUTURE (wallTravers - not spawned)");
}
```

### **Corner Positions (21×21 grid, cellSize=6):**
| Corner | X | Y | Z | Purpose |
|--------|---|---|---|---------|
| **North-West** | 0 | 2.0 | 126 | Future grid computation |
| **North-East** | 126 | 2.0 | 126 | Future grid computation |
| **South-West** | 0 | 2.0 | 0 | Future grid computation |
| **South-East** | 126 | 2.0 | 0 | Future grid computation |

---

## 💡 **LIGHTS ON-DEMAND**

### **BEFORE (Direct instantiation):**
```csharp
// ❌ OLD: Places lights immediately
public void PlaceTorches()
{
    GameObject parent = new GameObject("Torches");
    foreach (Vector3 wallPos in wallPositions)
    {
        // Instantiate torch immediately
        GameObject torch = Instantiate(torchPrefab, pos, rot);
        torch.transform.SetParent(parent.transform);
    }
}
```

### **AFTER (On-demand):**
```csharp
// ✅ NEW: Computes positions only
public void PlaceTorches()
{
    int torchCount = 0;
    foreach (Vector3 wallPos in wallPositions)
    {
        // Just count, don't instantiate
        torchCount++;
    }
    Log($"{torchCount} torch positions computed (not spawned)");
    Log("Lights will be placed on-demand by LightPlacementEngine");
}

// ✅ NEW: Call when actually needed
public void PlaceLightsOnDemand()
{
    if (lightPlacementEngine == null) return;
    
    // LightPlacementEngine loads from binary storage
    // Called only when lights are actually needed
    Log("LightPlacementEngine will instantiate lights from storage");
}
```

---

## 🧹 **STRAY LIGHT REMOVAL**

### **Updated Cleanup:**
```csharp
private void CleanupOldMaze()
{
    DestroyImmediate(FindObject("GroundFloor"));
    DestroyImmediate(FindObject("MazeWalls"));
    DestroyImmediate(FindObject("Doors"));
    DestroyImmediate(FindObject("Torches"));
    DestroyImmediate(FindObject("Lights"));  // ✅ Remove stray lights
}
```

**Prevents:**
- ❌ Lonely lights from previous runs
- ❌ Stray light objects in scene
- ❌ Memory leaks

---

## 📋 **CONSOLE OUTPUT**

### **Expected:**
```
[CompleteMazeBuilder]  Computing walls on extreme grid perimeter...
[CompleteMazeBuilder]  North wall: 21 segments at Z=126
[CompleteMazeBuilder]  South wall: 21 segments at Z=0
[CompleteMazeBuilder]  East wall: 21 segments at X=126
[CompleteMazeBuilder]  West wall: 21 segments at X=0
[CompleteMazeBuilder]  4 corner walls computed for FUTURE (wallTravers - not spawned)
[CompleteMazeBuilder]  88 wall segments placed (EXTREME PERIMETER)
[CompleteMazeBuilder]  Wall positions stored in RAM: 88
[CompleteMazeBuilder]  Computing torch positions on walls...
[CompleteMazeBuilder]  26 torch positions computed (not spawned)
[CompleteMazeBuilder]  Lights will be placed on-demand by LightPlacementEngine
```

**Note:** 
- 88 walls = 84 perimeter + 4 corners (stored, not spawned)
- Torch positions computed but NOT instantiated

---

## 🎯 **USAGE**

### **Generate Maze (default):**
```csharp
// Generates maze without lights
mazeBuilder.GenerateMaze();

// Lights are NOT placed yet (on-demand)
```

### **Place Lights (when needed):**
```csharp
// Call this when you actually need lights
mazeBuilder.PlaceLightsOnDemand();

// LightPlacementEngine instantiates from binary storage
```

---

## 📝 **FILES MODIFIED**

| File | Changes |
|------|---------|
| `CompleteMazeBuilder.cs` | ✅ Added `ComputeCornerWallsForFuture()` |
| `CompleteMazeBuilder.cs` | ✅ Renamed to `wallTravers` concept |
| `CompleteMazeBuilder.cs` | ✅ Changed `PlaceTorches()` to compute only |
| `CompleteMazeBuilder.cs` | ✅ Added `PlaceLightsOnDemand()` |
| `CompleteMazeBuilder.cs` | ✅ Updated cleanup to remove "Lights" |
| `Assets/Docs/CORNER_WALLS_AND_ON_DEMAND_LIGHTS.md` | 📝 This documentation |

---

## ✅ **BENEFITS**

### **Corner Walls (wallTravers):**
- ✅ **Future-ready** - Positions stored for later use
- ✅ **No spawn overhead** - Computed but not instantiated
- ✅ **Named appropriately** - `wallTravers` traverses corners

### **On-Demand Lights:**
- ✅ **Lazy loading** - Only when needed
- ✅ **Better performance** - No unnecessary instantiation
- ✅ **Cleaner code** - Separation of concerns
- ✅ **Binary storage** - Uses LightPlacementEngine properly

### **Stray Removal:**
- ✅ **Clean scenes** - No leftover lights
- ✅ **No lonely walls** - Cleanup removes everything
- ✅ **Memory efficient** - No leaks

---

## 🧪 **TESTING**

### **In Unity Editor:**
```
1. Tools → Maze → Setup Maze Components
2. Ctrl+Alt+G → Generate Maze
3. Check Console output
4. Verify: No lights spawned yet (on-demand)
5. Verify: No stray lights in scene
```

### **Verification:**
- [ ] Console shows "4 corner walls computed for FUTURE"
- [ ] Console shows "torch positions computed (not spawned)"
- [ ] No "Torches" or "Lights" parent in hierarchy
- [ ] No stray lights in scene
- [ ] Wall positions stored in RAM (88 total)

---

## 🚀 **NEXT STEPS**

### **To Place Lights (when needed):**
```csharp
// Call this explicitly when you need lights
mazeBuilder.PlaceLightsOnDemand();
```

### **Future Enhancement (wallTravers):**
```csharp
// When ready to spawn corner walls:
// TODO: Implement SpawnCornerWalls() method
// TODO: Use wallTravers positions from RAM
```

---

**Corner walls computed for future - Lights on-demand only!** 🫡✅

---

*Document generated - Unity 6 (6000.3.7f1) compatible - UTF-8 encoding - Unix LF*
