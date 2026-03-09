# Deep Scan Report - Code.Lavos
**Date:** 2026-03-09  
**Scanner:** BetsyBoop  
**Project:** CodeDotLavos (Unity 6 - 6000.3.7f1)

---

## Executive Summary

| Category | Count | Status |
|----------|-------|--------|
| Compilation Errors | 0 | ✓ Fixed |
| Warnings | 0 | ✓ Fixed |
| Header Issues | 1 | ⚠ Pending |
| TODO Markers | 16 | ℹ Info |
| Exception Handlers | 21 | ℹ Info |

---

## 1. Compilation Issues (RESOLVED)

### 1.1 PassageFirstMazeGenerator.cs - FIXED ✓

**Location:** `Assets/Scripts/Core/06_Maze/PassageFirstMazeGenerator.cs`

**Issues Fixed:**
| Line | Error | Fix Applied |
|------|-------|-------------|
| 623,22 | CS0721: 'CellFlags8': static types cannot be used as parameters | Changed parameter type from `CellFlags8` to `uint` |
| 130,40 | CS0117: 'CellFlags8' does not contain a definition for 'AllWalls' | Replaced with `CellFlags8.Wall_All` |
| 345,44 | CS0117: 'CellFlags8' does not contain a definition for 'AllWalls' | Replaced with `CellFlags8.Wall_All` |
| 423,36 | CS0117: 'CellFlags8' does not contain a definition for 'AllWalls' | Replaced with `CellFlags8.Wall_All` |
| 425,36 | CS0117: 'CellFlags8' does not contain a definition for 'HasPillar' | Added constant to `CellFlags8` |
| 455,48 | CS0117: 'CellFlags8' does not contain a definition for 'AllWalls' | Replaced with `CellFlags8.Wall_All` |
| 459,48 | CS0117: 'CellFlags8' does not contain a definition for 'HasNiche' | Added constant to `CellFlags8` |
| 510,48 | CS0117: 'CellFlags8' does not contain a definition for 'AllWalls' | Replaced with `CellFlags8.Wall_All` |
| 526,48 | CS0117: 'CellFlags8' does not contain a definition for 'AllWalls' | Replaced with `CellFlags8.Wall_All` |
| 617,52 | CS0117: 'CellFlags8' does not contain a definition for 'WallS' | Replaced with `CellFlags8.Wall_S` |
| 618,52 | CS0117: 'CellFlags8' does not contain a definition for 'WallN' | Replaced with `CellFlags8.Wall_N` |
| 619,52 | CS0117: 'CellFlags8' does not contain a definition for 'WallW' | Replaced with `CellFlags8.Wall_W` |
| 620,52 | CS0117: 'CellFlags8' does not contain a definition for 'WallE' | Replaced with `CellFlags8.Wall_E` |
| 660,39 | CS0117: 'CellFlags8' does not contain a definition for 'AllWalls' | Replaced with `CellFlags8.Wall_All` |

**Related Files Modified:**
- `DungeonMazeData.cs` - Added `HasPillar` (bit 20) and `HasNiche` (bit 21) constants

### 1.2 RoomDoorPlacer.cs - FIXED ✓

**Location:** `Assets/Scripts/Core/07_Doors/RoomDoorPlacer.cs`

**Issue Fixed:**
| Line | Warning | Fix Applied |
|------|---------|-------------|
| 68,52 | CS0169: The field 'RoomDoorPlacer.gridMazeGenerator' is never used | Removed unused field and related comment |

### 1.3 PrefabLoaderFix.cs - IL2CPP Fixes REMOVED ✓

**Location:** `Assets/Scripts/Core/06_Maze/PrefabLoaderFix.cs`

**Actions:**
- Removed `[UnityEngine.Scripting.Preserve]` attribute (per user request)
- Deleted `Assets/link.xml` (per user request)
- Fixed header to match project standard (GPL-3.0)

---

## 2. Header Issues (PENDING)

### 2.1 AutoMazeSetup.cs - NON-STANDARD HEADER ⚠

**Location:** `Assets/Scripts/Core/06_Maze/AutoMazeSetup.cs`

**Current Header:**
```csharp
// ================================================================================
// AUTO MAZE SETUP - Automatic prefab and material validation
// ================================================================================
// File: Assets/Scripts/Core/06_Maze/AutoMazeSetup.cs
// Purpose: Automatically validates and fixes maze generation setup
// Author: BetsyBoop
// Date: 2026-03-08
// Encoding: UTF-8 | Line Endings: Unix LF
// ================================================================================
```

**Expected Header (GPL-3.0 Standard):**
```csharp
// Copyright (C) 2026 Ocxyde
//
// This file is part of Code.Lavos.
//
// Code.Lavos is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Code.Lavos is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Code.Lavos.  If not, see <https://www.gnu.org/licenses/>.
```

**Recommendation:** Update header to match project GPL-3.0 standard.

---

## 3. TODO Markers (INFO)

**Total:** 16 occurrences across core files

| File | Line | Marker | Description |
|------|------|--------|-------------|
| DoorsEngine.cs | 416 | TODO | Check inventory for key |
| DoorsEngine.cs | 581 | TODO | Apply poison status effect |
| DoorsEngine.cs | 590 | TODO | Apply slow status effect |
| DoorsEngine.cs | 613 | TODO | Alert enemies in radius |
| DoorsEngine.cs | 622 | TODO | Spawn blocking debris |
| DoorsEngine.cs | 646 | TODO | Reveal secret area |
| DoorsEngine.cs | 649 | TODO | Trigger boss encounter |
| DoorsEngine.cs | 656 | TODO | Apply buff |
| DoorsEngine.cs | 662 | TODO | Apply debuff |
| DoorHolePlacer.cs | 319 | TODO | Add public accessor to CompleteMazeBuilder |
| TetrahedronMath.cs | 41 | TODO | Future features |
| Tetrahedron.cs | 41 | TODO | Future features |
| AudioManager.cs | 32 | TODO | Create Audio folder structure |
| SeedManager.cs | 614 | INFO | Maze code extraction logic |
| CastleDoubleDoor.cs | 101 | INFO | Vector math (not a TODO) |

**Note:** `CastleDoubleDoor.cs:101` is a false positive (contains "TODO" in variable name context).

---

## 4. Exception Handling (INFO)

**Total:** 21 try-catch blocks using generic `Exception`

| File | Count | Notes |
|------|-------|-------|
| LevelDatabaseManager.cs | 7 | File I/O operations |
| MazeBinaryStorage8.cs | 2 | Binary serialization |
| ComputeGridData.cs | 4 | Compute buffer operations |
| ComputeGridEngine.cs | 1 | Compute shader dispatch |
| XCom.cs | 2 | Command execution |
| ShareSystem.cs | 1 | Maze sharing |
| ProceduralLevelGenerator.cs | 1 | Maze generation |
| UniversalLevelGeneratorTool_V2.cs (Editor) | 3 | Editor tool |

**Recommendation:** Consider using specific exception types where appropriate for better error handling.

---

## 5. File Encoding & Line Endings

**Status:** ✓ COMPLIANT

All scanned files use:
- Encoding: UTF-8
- Line Endings: Unix LF

---

## 6. Architecture Compliance

### 6.1 Plug-in-Out Pattern ✓

Files following plug-in-out architecture:
- `RoomDoorPlacer.cs` - Finds components, never creates
- `CompleteMazeBuilder8.cs` - FindFirstObjectByType everywhere
- `BehaviorEngine.cs` - Base class for item behaviors

### 6.2 JSON Config Loading ✓

Files using JSON config (no hardcoded values):
- `RoomDoorPlacer.cs` - All values from GameConfig-default.json
- `DoorsEngine.cs` - Door properties from config

---

## 7. Recommendations

### Priority 1 (High)
1. **Fix AutoMazeSetup.cs header** - Update to GPL-3.0 standard header

### Priority 2 (Medium)
2. **Review TODO items** - Implement or remove outdated TODOs
3. **Exception handling** - Use specific exception types in critical paths

### Priority 3 (Low)
4. **Documentation** - Add XML docs to public APIs lacking documentation
5. **Code cleanup** - Remove false-positive TODO markers

---

## 8. Files Modified in This Session

| File | Changes |
|------|---------|
| `PassageFirstMazeGenerator.cs` | Fixed CellFlags8 usage (14 errors) |
| `DungeonMazeData.cs` | Added HasPillar, HasNiche constants |
| `RoomDoorPlacer.cs` | Removed unused gridMazeGenerator field |
| `PrefabLoaderFix.cs` | Removed IL2CPP fixes, fixed header |
| `link.xml` | Deleted (IL2CPP config) |

---

## 9. Build Status

```
Compilation: SUCCESS (0 errors, 0 warnings)
Build Time: < 2 minutes
Platform: Unity 6 (6000.3.7f1)
```

---

**Report Generated:** 2026-03-09  
**Next Scan:** After implementing Priority 1 items

---

## Appendix A - CellFlags8 Constants Reference

```csharp
// Walls (bits 0-7)
Wall_N, Wall_S, Wall_E, Wall_W, Wall_NE, Wall_NW, Wall_SE, Wall_SW, Wall_All

// Room Types (bits 8-14)
IsRoom, IsHall, IsTrapRoom, IsTreasureRoom, IsBossRoom, SpawnRoom, IsExit

// Objects (bits 15-17)
HasTorch, HasEnemy, HasChest

// Decoration (bits 20-21)
HasPillar, HasNiche

// Advanced markers (bits 18-19)
IsMainPath, IsDanger
```

---

*End of Report*
