# Git Commit Instructions - 2026-03-06

## Session Summary

**Date:** 2026-03-06  
**Author:** Ocxyde & BetsyBoop  
**Focus:** Plug-in-out compliance, cleanup, and documentation

---

## Changes Made

### HIGH PRIORITY:
1. **MazeBuilderEditor.cs** - Reworked for full plug-in-out compliance
   - Removed all `AddComponent()` calls
   - Removed all `new GameObject()` creation
   - Added validation methods
   - Added clear error messages for missing components

2. **SpawnPlacerEngine References** - Updated to SpatialPlacer
   - `TrapBehavior.cs` - Comment updated
   - `LootTable.cs` - Comment updated
   - `ItemTypes.cs` - Comments updated (2 occurrences)

### MEDIUM PRIORITY:
3. **Commented Code Cleanup**
   - `MazeIntegration.cs` - Removed OnGUI debug block (~50 lines)
   - `SeedManager.cs` - Removed OnGUI debug block (~60 lines)
   - Fixed duplicate `#endregion` in SeedManager.cs

4. **File Verification**
   - `LightEngine.cs` - Verified complete (927 lines)
   - `ParticleGenerator.cs` - Verified complete (896 lines)

### DOCUMENTATION:
5. **TODO.md** - Fully updated with session changes

### TOOLS CREATED:
6. **remove-emoji-from-cs.ps1** - Script to remove emoji from C# files

---

## Git Commands

### Step 1: Review Changes

```bash
# Check status
git status

# Review all changes (including unstaged)
git diff HEAD

# Review staged changes only (if any)
git diff --staged
```

### Step 2: Stage Files

```bash
# Stage all modified files
git add .

# OR stage specific files:
git add Assets/Scripts/Editor/MazeBuilderEditor.cs
git add Assets/Scripts/Core/08_Environment/TrapBehavior.cs
git add Assets/Scripts/Core/10_Resources/LootTable.cs
git add Assets/Scripts/Core/04_Inventory/ItemTypes.cs
git add Assets/Scripts/Core/06_Maze/MazeIntegration.cs
git add Assets/Scripts/Core/10_Resources/SeedManager.cs
git add Assets/Docs/TODO.md
git add remove-emoji-from-cs.ps1
```

### Step 3: Commit

```bash
git commit -m "refactor: MazeBuilderEditor plug-in-out compliance + cleanup

- MazeBuilderEditor: Remove all AddComponent/new GameObject calls
  - Add ValidateSceneSetup() method
  - Add ValidateMazeBuilderComponents() method
  - Add clear error messages for missing components
  - Full plug-in-out compliance (finds only, never creates)

- Update SpawnPlacerEngine references to SpatialPlacer
  - TrapBehavior.cs comment updated
  - LootTable.cs comment updated
  - ItemTypes.cs comments updated (2 occurrences)

- Clean commented code from legacy files
  - MazeIntegration.cs: Remove OnGUI debug block (~50 lines)
  - SeedManager.cs: Remove OnGUI debug block (~60 lines)
  - Fix duplicate #endregion in SeedManager.cs

- Verify truncated files are complete
  - LightEngine.cs: Complete (927 lines)
  - ParticleGenerator.cs: Complete (896 lines)

- Documentation: Update TODO.md with session changes

- Tools: Create remove-emoji-from-cs.ps1 script
  - Removes emoji from all C# files
  - Skips backup and read-only directories

Refs: #cleanup #plug-in-out #compliance"
```

### Step 4: Verify Commit

```bash
# Check status (should be clean)
git status

# View last commit
git log -1 --stat
```

### Step 5: Push (if remote configured)

```bash
# Push to remote repository
git push origin main

# OR if using different branch
git push origin <branch-name>
```

---

## Alternative: Quick Commit

If you want a shorter commit message:

```bash
git add .
git commit -m "refactor: MazeBuilderEditor compliance + cleanup commented code

- MazeBuilderEditor: Full plug-in-out compliance (no component creation)
- Update SpawnPlacerEngine → SpatialPlacer references in comments
- Remove commented code from MazeIntegration.cs and SeedManager.cs
- Verify LightEngine.cs and ParticleGenerator.cs are complete
- Update TODO.md with session changes
- Add remove-emoji-from-cs.ps1 tool"
```

---

## Files Modified Summary

| File | Change Type | Description |
|------|-------------|-------------|
| `MazeBuilderEditor.cs` | Reworked | Plug-in-out compliance |
| `TrapBehavior.cs` | Comment | SpawnPlacerEngine → SpatialPlacer |
| `LootTable.cs` | Comment | SpawnPlacerEngine → SpatialPlacer |
| `ItemTypes.cs` | Comment | SpawnPlacerEngine → SpatialPlacer (2x) |
| `MazeIntegration.cs` | Cleanup | Removed OnGUI debug block |
| `SeedManager.cs` | Cleanup | Removed OnGUI debug block |
| `TODO.md` | Updated | Session documentation |
| `remove-emoji-from-cs.ps1` | New | Emoji removal tool |

---

## Commit Message Style

Following **Conventional Commits**:
- `refactor:` - Code restructuring without changing functionality
- `docs:` - Documentation changes
- `chore:` - Tooling/scripts

---

**Ready to commit!** 🎯
