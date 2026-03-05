# cleanup_obsolete_editor_scripts.ps1
# Identifies and removes obsolete/deprecated editor scripts
# 
# OBSOLETE FILES (replaced by CompleteMazeBuilder + MazeBuilderEditor):
# - AutoFixMazeTest.cs → Replaced by MazeBuilderEditor
# - QuickSceneSetup.cs → Replaced by CompleteMazeBuilder
# - CreateFreshMazeTestScene.cs → Replaced by MazeBuilderEditor
# - AddFpsMazeTestComponents.cs → Replaced by MazeBuilderEditor
# - FixMazeTestScene.cs → Replaced by MazeBuilderEditor
# - FixSceneReferences.cs → Not needed (scene management improved)
# - DeleteAllGroundObjects.cs → Replaced by CompleteMazeBuilder cleanup
# - PurgeOldGround.cs → Replaced by CompleteMazeBuilder cleanup
# - FixFloorMaterials.cs → Replaced by FloorMaterialFactory (core)
# - FixSceneTexturesAndPrefabs.cs → Replaced by CompleteMazeBuilder
# - FloorMaterialFactoryMenu.cs → Replaced by FloorMaterialFactory (core)
# - ReorganizeEditorScripts.cs → One-time use script (no longer needed)
# - ClearSerializationCache.cs → One-time use script (no longer needed)
#
# KEPT (still useful):
# - MazeBuilderEditor.cs → MAIN editor tool (KEEP)
# - CreateMazePrefabs.cs → Creates wall/door prefabs (KEEP)
# - DeleteBinaryFiles.cs → Cleans old binary maze data (KEEP)
# - AddDoorSystemToScene.cs → Adds door system to existing scenes (KEEP)
# - URPSetupUtility.cs → URP configuration (KEEP)

param(
    [switch]$WhatIf,  # Show what would be deleted without actually deleting
    [switch]$Help     # Show help message
)

$editorScriptsPath = "Assets\Scripts\Editor"
$backupPath = "Backup\EditorScripts_Backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"

function Show-Help {
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  Cleanup Obsolete Editor Scripts" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Usage:" -ForegroundColor Yellow
    Write-Host "  .\cleanup_obsolete_editor_scripts.ps1 [-WhatIf] [-Help]" -ForegroundColor White
    Write-Host ""
    Write-Host "Options:" -ForegroundColor Yellow
    Write-Host "  -WhatIf   Show what would be deleted without actually deleting" -ForegroundColor White
    Write-Host "  -Help     Show this help message" -ForegroundColor White
    Write-Host ""
    Write-Host "OBSOLETE FILES (will be deleted):" -ForegroundColor Red
    $obsoleteFiles | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
    Write-Host ""
    Write-Host "KEPT FILES (still useful):" -ForegroundColor Green
    $keptFiles | ForEach-Object { Write-Host "  - $_" -ForegroundColor Green }
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
}

# Obsolete files (deprecated/replaced by CompleteMazeBuilder + MazeBuilderEditor)
$obsoleteFiles = @(
    "AutoFixMazeTest.cs",
    "QuickSceneSetup.cs",
    "CreateFreshMazeTestScene.cs",
    "AddFpsMazeTestComponents.cs",
    "FixMazeTestScene.cs",
    "FixSceneReferences.cs",
    "DeleteAllGroundObjects.cs",
    "PurgeOldGround.cs",
    "FixFloorMaterials.cs",
    "FixSceneTexturesAndPrefabs.cs",
    "FloorMaterialFactoryMenu.cs",
    "ReorganizeEditorScripts.cs",
    "ClearSerializationCache.cs"
)

# Files to keep (still useful)
$keptFiles = @(
    "MazeBuilderEditor.cs",
    "CreateMazePrefabs.cs",
    "DeleteBinaryFiles.cs",
    "AddDoorSystemToScene.cs",
    "URPSetupUtility.cs"
)

# Subfolder files to check (Maze/, Cleanup/, Materials/, Setup/)
$subfolders = @("Maze", "Cleanup", "Materials", "Setup")

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  OBSOLETE EDITOR SCRIPTS CLEANUP" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

if ($Help) {
    Show-Help
    exit 0
}

# Find obsolete files
$foundObsolete = @()
foreach ($file in $obsoleteFiles) {
    # Check root editor folder
    $filePath = Join-Path $editorScriptsPath $file
    if (Test-Path $filePath) {
        $foundObsolete += $filePath
    }
    
    # Check subfolders
    foreach ($subfolder in $subfolders) {
        $subfilePath = Join-Path $editorScriptsPath (Join-Path $subfolder $file)
        if (Test-Path $subfilePath) {
            $foundObsolete += $subfilePath
        }
    }
}

if ($foundObsolete.Count -eq 0) {
    Write-Host "✅ No obsolete files found. Your editor scripts are clean!" -ForegroundColor Green
    Write-Host ""
    exit 0
}

Write-Host "📋 Found $($foundObsolete.Count) obsolete file(s):" -ForegroundColor Yellow
Write-Host ""
$foundObsolete | ForEach-Object {
    Write-Host "  [OBSOLETE] $_" -ForegroundColor Red
}
Write-Host ""

if ($WhatIf) {
    Write-Host "🔍 WhatIf mode - No files will be deleted" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Files that WOULD be deleted:" -ForegroundColor Yellow
    $foundObsolete | ForEach-Object {
        Write-Host "  - $_" -ForegroundColor Yellow
    }
    Write-Host ""
    exit 0
}

# Create backup
Write-Host "📦 Creating backup at: $backupPath" -ForegroundColor Cyan
if (-not (Test-Path "Backup")) {
    New-Item -ItemType Directory -Path "Backup" | Out-Null
}
New-Item -ItemType Directory -Path $backupPath | Out-Null

foreach ($file in $foundObsolete) {
    $relativePath = $file -replace '\\', '/'
    $destPath = Join-Path $backupPath (Split-Path -Leaf $file)
    Copy-Item $file -Destination $destPath -Force
    Write-Host "  ✓ Backed up: $file" -ForegroundColor Gray
}
Write-Host ""

# Confirm deletion
Write-Host "⚠️  WARNING: This will permanently delete $($foundObsolete.Count) file(s)!" -ForegroundColor Red
Write-Host ""
$confirmation = Read-Host "Type 'YES' to confirm deletion"
if ($confirmation -ne 'YES') {
    Write-Host "❌ Deletion cancelled. Backup created at: $backupPath" -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "🗑️  Deleting obsolete files..." -ForegroundColor Red

$deletedCount = 0
foreach ($file in $foundObsolete) {
    try {
        # Make file writable before deleting
        if (Test-Path $file) {
            Set-ItemProperty -Path $file -Name IsReadOnly -Value $false -ErrorAction SilentlyContinue
        }
        
        Remove-Item $file -Force
        Write-Host "  ✓ Deleted: $file" -ForegroundColor Green
        $deletedCount++
    }
    catch {
        Write-Host "  ✗ Failed to delete: $file" -ForegroundColor Red
        Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  ✅ CLEANUP COMPLETE!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Deleted: $deletedCount file(s)" -ForegroundColor White
Write-Host "  Backup:  $backupPath" -ForegroundColor White
Write-Host ""
Write-Host "💡 TIP: Restart Unity Editor to refresh the Asset Database" -ForegroundColor Yellow
Write-Host ""

# Also clean up empty folders
Write-Host "🧹 Checking for empty folders..." -ForegroundColor Cyan
foreach ($subfolder in $subfolders) {
    $folderPath = Join-Path $editorScriptsPath $subfolder
    if (Test-Path $folderPath) {
        $files = Get-ChildItem -Path $folderPath -File
        if ($files.Count -eq 0) {
            Write-Host "  ✓ Empty folder: $folderPath" -ForegroundColor Gray
            # Uncomment next line to also delete empty folders
            # Remove-Item $folderPath -Force
        }
    }
}
Write-Host ""
