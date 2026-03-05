# Obsolete Editor Scripts Cleanup

## Overview

This document lists editor scripts that have been marked as **OBSOLETE** and can be safely deleted. These files have been replaced by the new **CompleteMazeBuilder + MazeBuilderEditor** architecture.

## 🗑️ Files Marked for Deletion

### Core Replacement
The following files are obsolete because their functionality is now handled by:
- **CompleteMazeBuilder.cs** (Core) - Main maze generation system
- **MazeBuilderEditor.cs** (Editor) - Main editor tool (Tools → Maze → Generate Maze)

### Obsolete Files List

| File | Location | Replaced By | Reason |
|------|----------|-------------|--------|
| `AutoFixMazeTest.cs` | Editor/Maze/ | MazeBuilderEditor | Auto-fix functionality now in MazeBuilderEditor |
| `QuickSceneSetup.cs` | Editor/Maze/ | CompleteMazeBuilder | Complete scene setup now automated |
| `CreateFreshMazeTestScene.cs` | Editor/Maze/ | MazeBuilderEditor | Scene creation now integrated |
| `AddFpsMazeTestComponents.cs` | Editor/Maze/ | MazeBuilderEditor | Component addition now automated |
| `FixMazeTestScene.cs` | Editor/Maze/ | MazeBuilderEditor | Scene fixing now integrated |
| `FixSceneReferences.cs` | Editor/Maze/ | MazeBuilderEditor | Build settings handled automatically |
| `DeleteAllGroundObjects.cs` | Editor/Cleanup/ | CompleteMazeBuilder | Cleanup now part of maze lifecycle |
| `PurgeOldGround.cs` | Editor/Cleanup/ | CompleteMazeBuilder | Aggressive cleanup no longer needed |
| `FixFloorMaterials.cs` | Editor/Materials/ | FloorMaterialFactory | Material handling moved to core |
| `FixSceneTexturesAndPrefabs.cs` | Editor/Materials/ | CompleteMazeBuilder | Setup now automated |
| `FloorMaterialFactoryMenu.cs` | Editor/Materials/ | FloorMaterialFactory | Menu wrapper no longer needed |
| `ReorganizeEditorScripts.cs` | Editor/ | N/A | One-time use script (organization complete) |
| `ClearSerializationCache.cs` | Editor/ | N/A | One-time use script (no longer needed) |

## ✅ Files to KEEP (Still Useful)

| File | Location | Purpose |
|------|----------|---------|
| `MazeBuilderEditor.cs` | Editor/ | **MAIN** editor tool for maze generation |
| `CreateMazePrefabs.cs` | Editor/Maze/ | Creates wall/door prefabs for maze |
| `DeleteBinaryFiles.cs` | Editor/Cleanup/ | Cleans old binary maze data files |
| `AddDoorSystemToScene.cs` | Editor/Setup/ | Adds door system to existing scenes |
| `URPSetupUtility.cs` | Editor/Setup/ | URP configuration utility |

## 🧹 How to Clean Up

### Option 1: Automated Cleanup (Recommended)

Run the PowerShell cleanup script from the project root:

```powershell
# Preview what will be deleted
.\cleanup_obsolete_editor_scripts.ps1 -WhatIf

# Actually delete files (creates backup first)
.\cleanup_obsolete_editor_scripts.ps1
```

The script will:
1. List all obsolete files found
2. Create a backup in `Backup/EditorScripts_Backup_YYYYMMDD_HHMMSS/`
3. Ask for confirmation before deletion
4. Delete the obsolete files
5. Clean up empty folders (optional)

### Option 2: Manual Cleanup

1. Navigate to `Assets/Scripts/Editor/`
2. Delete the files listed in the "Obsolete Files List" table above
3. Restart Unity Editor to refresh the Asset Database

## ⚠️ Important Notes

1. **Backup First**: The cleanup script automatically creates a backup, but you can also run `backup.ps1` manually before making changes.

2. **Unity Editor**: Close Unity Editor before running the cleanup script, then restart it afterward to refresh the Asset Database.

3. **Compilation Errors**: After deletion, you may see console warnings about `[Obsolete]` attributes disappearing - this is normal and expected.

4. **Git Users**: Consider committing the backup before deletion:
   ```bash
   git add Backup/
   git commit -m "Backup obsolete editor scripts before cleanup"
   ```

## 📋 What Changed

### Old Workflow (Obsolete)
1. Create empty scene
2. Run multiple editor tools separately
3. Manually add components
4. Fix references manually
5. Run cleanup tools

### New Workflow (Current)
1. **Tools → Maze → Generate Maze** (or press `Ctrl+Alt+G`)
2. Everything is automatic:
   - Components added
   - Maze generated (rooms first, then corridors)
   - Player spawned inside entrance room
   - All references configured
   - Cleanup handled automatically

## 🔧 Technical Details

All obsolete files have been marked with `[System.Obsolete]` attributes:
- Shows compiler warnings when code references them
- Includes message pointing to replacement
- Helps identify any remaining dependencies

Example:
```csharp
[System.Obsolete("AutoFixMazeTest is deprecated. Use MazeBuilderEditor instead.")]
public class AutoFixMazeTest { ... }
```

## 📞 Troubleshooting

### "File is read-only"
Run the cleanup script as Administrator, or manually remove the read-only attribute:
```powershell
Set-ItemProperty -Path "Assets/Scripts/Editor/..." -Name IsReadOnly -Value $false
```

### "Unity won't recompile"
1. Close Unity Editor
2. Delete `Library/` folder (optional, forces full reimport)
3. Reopen Unity project

### "I accidentally deleted a needed file"
Restore from backup:
```powershell
# Backups are stored in:
Backup/EditorScripts_Backup_YYYYMMDD_HHMMSS/
```

---

**Last Updated**: 2026-03-05  
**Unity Version**: 6000.3.7f1 (Unity 6)  
**Project**: PeuImporte
