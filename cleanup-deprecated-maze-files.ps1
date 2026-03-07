# cleanup-deprecated-maze-files.ps1
# Remove deprecated maze system files
# Generated: 2026-03-07
#
# USAGE: .\cleanup-deprecated-maze-files.ps1
#
# These files are marked as deprecated and no longer used in the core maze system:
# - SpawningRoom.cs (spawn handled by GridMazeGenerator8)
# - MazeSaveData.cs (replaced by MazeBinaryStorage8)
# - MazeRenderer.cs (wall spawning in CompleteMazeBuilder8)

param(
    [switch]$WhatIf,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$backupDir = Join-Path $scriptRoot "Backup_Deprecated_$(Get-Date -Format 'yyyyMMdd_HHmmss')"

# Files to delete (deprecated - no longer used in core system)
$deprecatedFiles = @(
    "Assets\Scripts\Core\06_Maze\SpawningRoom.cs",
    "Assets\Scripts\Core\06_Maze\MazeSaveData.cs",
    "Assets\Scripts\Core\06_Maze\MazeRenderer.cs"
)

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  DEPRECATED MAZE FILES CLEANUP" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Check if files exist
$filesFound = @()
$filesNotFound = @()

foreach ($file in $deprecatedFiles) {
    $fullPath = Join-Path $scriptRoot $file
    if (Test-Path $fullPath) {
        $filesFound += $file
        if ($Verbose) { Write-Host "  [FOUND] $file" -ForegroundColor Gray }
    } else {
        $filesNotFound += $file
        Write-Host "  [NOT FOUND] $file" -ForegroundColor DarkGray
    }
}

Write-Host ""
Write-Host "Files to delete: $($filesFound.Count)" -ForegroundColor Yellow
Write-Host "Files not found: $($filesNotFound.Count)" -ForegroundColor DarkGray
Write-Host ""

if ($filesFound.Count -eq 0) {
    Write-Host "No deprecated files found. Nothing to do." -ForegroundColor Green
    return
}

# Display files that will be deleted
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  FILES TO DELETE:" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
foreach ($file in $filesFound) {
    Write-Host "  ❌ $file" -ForegroundColor Red
}
Write-Host ""

# WhatIf mode
if ($WhatIf) {
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  WHATIF MODE - NO CHANGES MADE" -ForegroundColor Yellow
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Would create backup directory:" -ForegroundColor Gray
    Write-Host "  $backupDir" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "Would delete $($filesFound.Count) file(s)." -ForegroundColor Gray
    return
}

# Confirm deletion
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  CONFIRMATION REQUIRED" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
$confirmation = Read-Host "Delete $($filesFound.Count) deprecated file(s)? [y/N]"

if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
    Write-Host "Operation cancelled by user." -ForegroundColor Yellow
    return
}

# Create backup directory
Write-Host ""
Write-Host "Creating backup directory..." -ForegroundColor Gray
Write-Host "  $backupDir" -ForegroundColor DarkGray

try {
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    Write-Host "  ✅ Backup directory created" -ForegroundColor Green
} catch {
    Write-Host "  ❌ Failed to create backup directory: $_" -ForegroundColor Red
    return
}

# Copy files to backup
Write-Host ""
Write-Host "Backing up files..." -ForegroundColor Gray
foreach ($file in $filesFound) {
    $sourcePath = Join-Path $scriptRoot $file
    $fileName = Split-Path $file -Leaf
    $destPath = Join-Path $backupDir $fileName
    
    try {
        Copy-Item -Path $sourcePath -Destination $destPath -Force
        Write-Host "  ✅ Backed up: $fileName" -ForegroundColor Green
    } catch {
        Write-Host "  ❌ Failed to backup: $fileName - $_" -ForegroundColor Red
    }
}

# Delete files
Write-Host ""
Write-Host "Deleting deprecated files..." -ForegroundColor Gray
$deletedCount = 0
$failedCount = 0

foreach ($file in $filesFound) {
    $fullPath = Join-Path $scriptRoot $file
    
    try {
        Remove-Item -Path $fullPath -Force
        Write-Host "  ✅ Deleted: $file" -ForegroundColor Green
        $deletedCount++
    } catch {
        Write-Host "  ❌ Failed to delete: $file - $_" -ForegroundColor Red
        $failedCount++
    }
}

# Summary
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  CLEANUP SUMMARY" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Files deleted:  $deletedCount / $($filesFound.Count)" -ForegroundColor $(if ($deletedCount -eq $filesFound.Count) { "Green" } else { "Yellow" })
Write-Host "  Failures:       $failedCount" -ForegroundColor $(if ($failedCount -eq 0) { "Green" } else { "Red" })
Write-Host "  Backup location:" -ForegroundColor Gray
Write-Host "  $backupDir" -ForegroundColor DarkGray
Write-Host ""

if ($deletedCount -eq $filesFound.Count) {
    Write-Host "✅ Cleanup completed successfully!" -ForegroundColor Green
} else {
    Write-Host "⚠️  Cleanup completed with warnings" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  NEXT STEPS" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Run backup.ps1 to backup entire project" -ForegroundColor White
Write-Host "2. Test in Unity to verify no broken references" -ForegroundColor White
Write-Host "3. Commit changes with git:" -ForegroundColor White
Write-Host "   git add ." -ForegroundColor DarkGray
Write-Host "   git commit -m `"refactor: remove deprecated maze files`"" -ForegroundColor DarkGray
Write-Host ""
