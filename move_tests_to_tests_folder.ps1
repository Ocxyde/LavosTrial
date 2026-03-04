# Move Test Components BACK to Tests Folder
# This script moves test/debug components from Core/ back to Tests/ folder

$ErrorActionPreference = "Stop"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Moving Test Components to Tests Folder"
Write-Host "  Unity 6 Project - PeuImporte"
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Files to move back to Tests/
$filesToMove = @(
    @{
        Source = "Assets\Scripts\Core\06_Maze\FpsMazeTest.cs"
        Dest   = "Assets\Scripts\Tests\FpsMazeTest.cs"
        Name   = "FpsMazeTest.cs"
    },
    @{
        Source = "Assets\Scripts\Core\06_Maze\MazeTorchTest.cs"
        Dest   = "Assets\Scripts\Tests\MazeTorchTest.cs"
        Name   = "MazeTorchTest.cs"
    },
    @{
        Source = "Assets\Scripts\Core\10_Resources\TorchManualActivator.cs"
        Dest   = "Assets\Scripts\Tests\TorchManualActivator.cs"
        Name   = "TorchManualActivator.cs"
    },
    @{
        Source = "Assets\Scripts\Core\02_Player\DebugCameraIssue.cs"
        Dest   = "Assets\Scripts\Tests\DebugCameraIssue.cs"
        Name   = "DebugCameraIssue.cs"
    }
)

# Meta files to move with each .cs file
$movedCount = 0
$skippedCount = 0

foreach ($file in $filesToMove) {
    $sourcePath = Join-Path $PSScriptRoot $file.Source
    $destPath = Join-Path $PSScriptRoot $file.Dest
    $metaSource = "$sourcePath.meta"
    $metaDest = "$destPath.meta"
    
    if (Test-Path $sourcePath) {
        # Ensure Tests directory exists
        $testsDir = Join-Path $PSScriptRoot "Assets\Scripts\Tests"
        if (-not (Test-Path $testsDir)) {
            Write-Host "  ERROR: Tests folder does not exist!" -ForegroundColor Red
            Write-Host "  Please create Assets/Scripts/Tests/ folder first" -ForegroundColor Yellow
            exit 1
        }
        
        # Move .cs file
        Move-Item -Path $sourcePath -Destination $destPath -Force
        Write-Host "  ✓ Moved: $($file.Name)" -ForegroundColor Green
        
        # Move .meta file if exists
        if (Test-Path $metaSource) {
            Move-Item -Path $metaSource -Destination $metaDest -Force
            Write-Host "    + Meta file" -ForegroundColor Gray
        }
        
        $movedCount++
    } else {
        Write-Host "  ⊘ Skipped (not found): $($file.Name)" -ForegroundColor Yellow
        $skippedCount++
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Migration Summary"
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Files moved:    $movedCount" -ForegroundColor Green
Write-Host "  Files skipped:  $skippedCount" -ForegroundColor Yellow
Write-Host ""

if ($movedCount -gt 0) {
    Write-Host "✅ Test components moved to Tests folder!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Run backup.ps1 to backup changes" -ForegroundColor White
    Write-Host "  2. Reimport in Unity Editor (Assets > Reimport All)" -ForegroundColor White
    Write-Host "  3. Verify compilation (should be 0 errors)" -ForegroundColor White
} else {
    Write-Host "⚠️  No files were moved. Check if files already in Tests/." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Press any key to continue..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
