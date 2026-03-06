# Copyright (C) 2026 Ocxyde
# GPL-3.0 License
#
# Script: cleanup-deprecated-maze-files.ps1
# Purpose: Delete deprecated maze generation files
# Date: 2026-03-06
#
# Files to delete:
# - MazeIntegration.cs (legacy orchestrator)
# - MazeGenerator.cs (old DFS algorithm)
# - MazeSetupHelper.cs (legacy editor helper)
# - GridPEnvPlacer.cs (orphaned wall placer)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = $scriptDir

# Files to delete
$deprecatedFiles = @(
    "Assets\Scripts\Core\06_Maze\MazeIntegration.cs",
    "Assets\Scripts\Core\06_Maze\MazeGenerator.cs",
    "Assets\Scripts\Core\06_Maze\MazeSetupHelper.cs",
    "Assets\Scripts\Core\08_Environment\GridPEnvPlacer.cs",
    "Assets\Scripts\Core\06_Maze\RoomGenerator.cs",
    "Assets\Scripts\Core\07_Doors\DoorSystemSetup.cs",
    "Assets\Scripts\Editor\Setup\AddDoorSystemToScene.cs"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Deprecated Maze Files Cleanup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$deletedCount = 0
$failedCount = 0

foreach ($relativePath in $deprecatedFiles)
{
    $fullPath = Join-Path $projectRoot $relativePath
    
    if (Test-Path $fullPath)
    {
        Write-Host "Deleting: $relativePath" -ForegroundColor Yellow
        
        try
        {
            Remove-Item -Path $fullPath -Force
            Write-Host "  ✓ Deleted successfully" -ForegroundColor Green
            $deletedCount++
        }
        catch
        {
            Write-Host "  ✗ Failed to delete: $_" -ForegroundColor Red
            $failedCount++
        }
    }
    else
    {
        Write-Host "Skipping (not found): $relativePath" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Cleanup Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Files deleted: $deletedCount" -ForegroundColor Green
Write-Host "Files failed: $failedCount" -ForegroundColor $(if ($failedCount -eq 0) { "Green" } else { "Red" })
Write-Host ""

if ($failedCount -eq 0)
{
    Write-Host "Cleanup completed successfully!" -ForegroundColor Green
}
else
{
    Write-Host "Cleanup completed with errors." -ForegroundColor Yellow
    exit 1
}
