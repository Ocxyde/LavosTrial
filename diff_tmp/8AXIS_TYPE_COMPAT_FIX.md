# 8-Axis Type Compatibility Fixes

**Date:** 2026-03-06
**Issue:** Old files compare `CellFlags8` with `GridMazeCell`
**Solution:** Added extension methods + fixed comparisons

---

## ✅ FIX APPLIED

### **1. Added Compatibility Helpers**

**File:** `Assets/Scripts/Core/06_Maze/GridMazeCell.cs`

**Added:**
```csharp
public static class CellTypeCompatibility
{
    public static bool IsWall(this CellFlags8 cell)
        => (cell & CellFlags8.AllWalls) == CellFlags8.AllWalls;

    public static bool IsWalkable(this CellFlags8 cell)
        => (cell & CellFlags8.AllWalls) != CellFlags8.AllWalls;

    public static bool IsSpawnPoint(this CellFlags8 cell)
        => (cell & CellFlags8.SpawnRoom) != CellFlags8.None;

    public static bool HasTorch(this CellFlags8 cell)
        => (cell & CellFlags8.HasTorch) != CellFlags8.None;

    public static bool HasChest(this CellFlags8 cell)
        => (cell & CellFlags8.HasChest) != CellFlags8.None;

    public static bool HasEnemy(this CellFlags8 cell)
        => (cell & CellFlags8.HasEnemy) != CellFlags8.None;
}
```

---

## 🔧 FILES FIXED

### **DoorHolePlacer.cs** ✅

**Line 193:**
```csharp
// Before:
if (cell == GridMazeCell.Room || cell == GridMazeCell.Corridor)

// After:
if (!cell.IsWall())  // 8-axis compatibility
```

**Line 249:**
```csharp
// Before:
if (adjacentCell == GridMazeCell.Wall)

// After:
if (adjacentCell.IsWall())  // 8-axis compatibility
```

---

### **Files Still Needing Fixes:**

| File | Lines | Old Code | Fix |
|------|-------|----------|-----|
| `ChestPlacer.cs` | 247 | `cell == GridMazeCell.Room` | `!cell.IsWall()` |
| `ChestPlacer.cs` | 247 | `cell == GridMazeCell.Corridor` | `!cell.IsWall()` |
| `EnemyPlacer.cs` | 237 | `cell == GridMazeCell.Room` | `!cell.IsWall()` |
| `EnemyPlacer.cs` | 237 | `cell == GridMazeCell.Corridor` | `!cell.IsWall()` |
| `ItemPlacer.cs` | 215 | `cell == GridMazeCell.Room` | `!cell.IsWall()` |
| `ItemPlacer.cs` | 215 | `cell == GridMazeCell.Corridor` | `!cell.IsWall()` |
| `TorchPlacer.cs` | 220 | `cell != GridMazeCell.Wall` | `!cell.IsWall()` |
| `TorchPlacer.cs` | 230-239 | `cell != GridMazeCell.Wall` | `!cell.IsWall()` |

---

## 📝 MANUAL FIXES REQUIRED

**You need to manually fix these files in Unity/Rider:**

### **ChestPlacer.cs (Line 247):**
```csharp
// Find this line:
if (cell == GridMazeCell.Room || cell == GridMazeCell.Corridor)

// Replace with:
if (!cell.IsWalkable())  // 8-axis compatibility
```

### **EnemyPlacer.cs (Line 237):**
```csharp
// Find this line:
if (cell == GridMazeCell.Room || cell == GridMazeCell.Corridor)

// Replace with:
if (!cell.IsWalkable())  // 8-axis compatibility
```

### **ItemPlacer.cs (Line 215):**
```csharp
// Find this line:
if (cell == GridMazeCell.Room || cell == GridMazeCell.Corridor)

// Replace with:
if (!cell.IsWalkable())  // 8-axis compatibility
```

### **TorchPlacer.cs (Lines 220, 230-239):**
```csharp
// Find these lines:
if (cell != GridMazeCell.Wall)

// Replace with:
if (!cell.IsWall())  // 8-axis compatibility
```

---

## ✅ SUMMARY

**Fixed:**
- `GridMazeCell.cs` - Added compatibility extension methods
- `DoorHolePlacer.cs` - Fixed 2 comparisons

**Needs Manual Fix:**
- `ChestPlacer.cs` - 1 line
- `EnemyPlacer.cs` - 1 line
- `ItemPlacer.cs` - 1 line
- `TorchPlacer.cs` - 5 lines

**Total:** 4 files, 8 lines to fix

---

## 🚀 AFTER FIXING

**Run in Unity:**
```
1. Open Unity 6000.3.7f1
2. Wait for compilation
3. Verify: 0 errors, 0 warnings
```

**Then backup:**
```powershell
.\backup.ps1
```

---

*Fix guide generated - UTF-8 encoding - Unix LF*

**Type compatibility added, manual fixes needed!** 🫡⚔️
