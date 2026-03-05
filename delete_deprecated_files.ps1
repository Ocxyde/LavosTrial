# delete_deprecated_files.ps1
# Safely deletes all deprecated/obsolete files from the project
# Unity 6 compatible - UTF-8 encoding - Unix line endings
#
# USAGE:
#   1. Review the file list below
#   2. Run: .\delete_deprecated_files.ps1
#   3. Confirm deletion
#   4. Run backup.ps1 after deletion
#
# NOTE: This script asks for confirmation before deleting!

param(
    [switch]$WhatIf,    # Show what would be deleted without actually deleting
    [switch]$Force      # Skip confirmation prompt
)

$ErrorActionPreference = "Stop"

# ============================================================================
# DEPRECATED FILES LIST
# ============================================================================

$deprecatedFiles = @(
    # Test Files (marked [System.Obsolete])
    "Assets/Scripts/Tests/FpsMazeTest.cs"
    "Assets/Scripts/Tests/MazeTorchTest.cs"
    "Assets/Scripts/Tests/DebugCameraIssue.cs"

    # Core Maze Files
    "Assets/Scripts/Core/06_Maze/MazeRenderer.cs"  # Geometry handled by CompleteMazeBuilder

    # Redundant Files
    "Assets/Scripts/Core/08_Environment/SpawnPlacerEngine.cs"

    # KEPT - CORE ENGINES (DO NOT DELETE):
    # - MazeIntegration.cs - Still used by door system
    # - DoorHolePlacer.cs - Core door engine (updated to use GridMazeGenerator)
    # - RoomDoorPlacer.cs - Core door engine (updated to use GridMazeGenerator)
    # - SFXVFXEngine.cs - Special FX & Visual FX
)

# ============================================================================
# SCRIPT LOGIC
# ============================================================================

Write-Host "════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  DEPRECATED FILES DELETION SCRIPT" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$projectRoot = $PSScriptRoot
$filesToDelete = @()
$filesNotFound = @()

# Check which files exist
foreach ($file in $deprecatedFiles) {
    $fullPath = Join-Path $projectRoot $file
    if (Test-Path $fullPath) {
        $filesToDelete += $file
    } else {
        $filesNotFound += $file
    }
}

# Report what will be deleted
Write-Host "📋 FILES TO BE DELETED:" -ForegroundColor Yellow
Write-Host "────────────────────────────────────────────────────" -ForegroundColor DarkGray

if ($filesToDelete.Count -eq 0) {
    Write-Host "  ✅ No deprecated files found - already cleaned!" -ForegroundColor Green
} else {
    foreach ($file in $filesToDelete) {
        Write-Host "  ❌ $file" -ForegroundColor Red
    }
}

Write-Host ""

# Report files not found (already deleted)
if ($filesNotFound.Count -gt 0) {
    Write-Host "📋 FILES NOT FOUND (already deleted):" -ForegroundColor Green
    Write-Host "────────────────────────────────────────────────────" -ForegroundColor DarkGray
    foreach ($file in $filesNotFound) {
        Write-Host "  ✅ $file" -ForegroundColor Green
    }
    Write-Host ""
}

# Summary
Write-Host "════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SUMMARY: $($filesToDelete.Count) files to delete" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Exit if nothing to delete
if ($filesToDelete.Count -eq 0) {
    Write-Host "✅ Nothing to do - all deprecated files already deleted!" -ForegroundColor Green
    exit 0
}

# WhatIf mode - just show what would be deleted
if ($WhatIf) {
    Write-Host "🔍 WHAT-IF MODE - No files were deleted" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To actually delete these files, run:" -ForegroundColor Cyan
    Write-Host "  .\delete_deprecated_files.ps1" -ForegroundColor White
    Write-Host ""
    Write-Host "To skip confirmation, run:" -ForegroundColor Cyan
    Write-Host "  .\delete_deprecated_files.ps1 -Force" -ForegroundColor White
    exit 0
}

# Ask for confirmation
if (-not $Force) {
    Write-Host "⚠️  WARNING: This action will permanently delete these files!" -ForegroundColor Red
    Write-Host ""
    $confirmation = Read-Host "Are you sure you want to delete these files? (yes/no)"
    
    if ($confirmation -ne "yes") {
        Write-Host ""
        Write-Host "❌ Deletion cancelled by user" -ForegroundColor Yellow
        exit 0
    }
}

Write-Host ""
Write-Host "🗑️  DELETING FILES..." -ForegroundColor Yellow
Write-Host "────────────────────────────────────────────────────" -ForegroundColor DarkGray

$deletedCount = 0
$failedCount = 0

foreach ($file in $filesToDelete) {
    $fullPath = Join-Path $projectRoot $file
    
    try {
        Remove-Item -Path $fullPath -Force -ErrorAction Stop
        Write-Host "  ✅ Deleted: $file" -ForegroundColor Green
        $deletedCount++
    } catch {
        Write-Host "  ❌ Failed to delete: $file" -ForegroundColor Red
        Write-Host "     Error: $($_.Exception.Message)" -ForegroundColor DarkGray
        $failedCount++
    }
}

Write-Host ""
Write-Host "════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  DELETION COMPLETE" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  ✅ Successfully deleted: $deletedCount files" -ForegroundColor Green
if ($failedCount -gt 0) {
    Write-Host "  ❌ Failed to delete: $failedCount files" -ForegroundColor Red
}
Write-Host ""

# Reminders
Write-Host "════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  ⚠️  IMPORTANT NEXT STEPS:" -ForegroundColor Yellow
Write-Host "════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  1. Run backup.ps1 to backup the changes:" -ForegroundColor White
Write-Host "     .\backup.ps1" -ForegroundColor Cyan
Write-Host ""
Write-Host "  2. Check Unity Console for any remaining errors" -ForegroundColor White
Write-Host ""
Write-Host "  3. Update TODO.md to mark these as completed" -ForegroundColor White
Write-Host ""

Write-Host "✅ Done!" -ForegroundColor Green
