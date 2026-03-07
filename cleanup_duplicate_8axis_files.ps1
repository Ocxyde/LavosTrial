# Delete Duplicate 8-Axis Source Files
# Safety script - only deletes duplicate files from maze_v0-6-8_ushort_2byte_saves integration
# Date: 2026-03-06

$ErrorActionPreference = "Stop"
$projectRoot = "D:\travaux_Unity\CodeDotLavos"
$deletedCount = 0
$skippedCount = 0

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Delete Duplicate 8-Axis Source Files" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Files to delete (duplicates from source archive)
$duplicateFiles = @(
    "Assets\Scripts\Core\06_Maze\GameConfig8.cs",
    "Assets\Scripts\Core\06_Maze\GridMazeGenerator8.cs",
    "Assets\Scripts\Core\06_Maze\CompleteMazeBuilder8.cs"
)

# Keep these files (they are the adapted versions)
$keepFiles = @(
    "Assets\Scripts\Core\06_Maze\GameConfig.cs",
    "Assets\Scripts\Core\06_Maze\GridMazeGenerator.cs",
    "Assets\Scripts\Core\06_Maze\CompleteMazeBuilder.cs",
    "Assets\Scripts\Core\06_Maze\MazeData8.cs",
    "Assets\Scripts\Core\06_Maze\MazeBinaryStorage8.cs",
    "Assets\Scripts\Core\06_Maze\MazeRenderer.cs"
)

Write-Host "Checking for duplicate files..." -ForegroundColor Yellow
Write-Host ""

# Check that we keep the right files
Write-Host "Files to KEEP (adapted versions):" -ForegroundColor Green
foreach ($file in $keepFiles) {
    $fullPath = Join-Path $projectRoot $file
    if (Test-Path $fullPath) {
        Write-Host "  [KEEP] $file" -ForegroundColor Green
    } else {
        Write-Host "  [MISSING] $file - WARNING!" -ForegroundColor Red
    }
}
Write-Host ""

# Check duplicate files to delete
Write-Host "Files to DELETE (duplicate source):" -ForegroundColor Red
foreach ($file in $duplicateFiles) {
    $fullPath = Join-Path $projectRoot $file
    if (Test-Path $fullPath) {
        Write-Host "  [DELETE] $file" -ForegroundColor Red
        $deletedCount++
    } else {
        Write-Host "  [SKIP] $file (not found)" -ForegroundColor Gray
        $skippedCount++
    }
}
Write-Host ""

# Confirm deletion
if ($deletedCount -eq 0) {
    Write-Host "No duplicate files found. Nothing to delete." -ForegroundColor Green
    Write-Host ""
    Write-Host "Press any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 0
}

Write-Host "========================================" -ForegroundColor Yellow
Write-Host "  CONFIRMATION REQUIRED" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "This will delete $deletedCount duplicate file(s)." -ForegroundColor Yellow
Write-Host "These are source files from maze_v0-6-8_ushort_2byte_saves/" -ForegroundColor Yellow
Write-Host "We already have adapted versions (without the '8' suffix)." -ForegroundColor Yellow
Write-Host ""
Write-Host "Press 'Y' to confirm deletion, any other key to cancel..." -ForegroundColor Cyan

$key = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

if ($key.Character -ne 'Y' -and $key.Character -ne 'y') {
    Write-Host ""
    Write-Host "Deletion cancelled by user." -ForegroundColor Red
    Write-Host ""
    Write-Host "Press any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 0
}

# Delete duplicate files
Write-Host ""
Write-Host "Deleting duplicate files..." -ForegroundColor Yellow

foreach ($file in $duplicateFiles) {
    $fullPath = Join-Path $projectRoot $file
    
    if (Test-Path $fullPath) {
        try {
            Remove-Item $fullPath -Force
            Write-Host "  [DELETED] $file" -ForegroundColor Green
        }
        catch {
            Write-Host "  [ERROR] Failed to delete $file" -ForegroundColor Red
            Write-Host "          $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CLEANUP COMPLETE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Summary:" -ForegroundColor White
Write-Host "  - Files deleted: $deletedCount" -ForegroundColor Green
Write-Host "  - Files skipped: $skippedCount" -ForegroundColor Gray
Write-Host ""
Write-Host "Remaining files (adapted versions):" -ForegroundColor Green
foreach ($file in $keepFiles) {
    $fullPath = Join-Path $projectRoot $file
    if (Test-Path $fullPath) {
        Write-Host "  [OK] $file" -ForegroundColor Green
    }
}
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Open Unity and verify 0 compilation errors" -ForegroundColor White
Write-Host "  2. Run backup.ps1 to save changes" -ForegroundColor White
Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
