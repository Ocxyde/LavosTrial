# reorganize_test_files.ps1
# Reorganizes test files into proper folder structure
# Run this script from project root: .\reorganize_test_files.ps1
# After running, remember to run: .\backup.ps1

$ErrorActionPreference = "Stop"
$projectRoot = $PSScriptRoot

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Test Files Reorganization Script" -ForegroundColor Cyan
Write-Host "  Unity 6 Project - PeuImporte" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Track changes for diff
$changesMade = @()

# Function to move file with meta
function Move-FileWithMeta {
    param(
        [string]$Source,
        [string]$Destination
    )
    
    if (Test-Path $Source) {
        $sourceMeta = $Source + ".meta"
        $destMeta = $Destination + ".meta"
        
        Write-Host "  Moving: $Source" -ForegroundColor Yellow
        
        # Move main file
        Move-Item -Path $Source -Destination $Destination -Force
        $changesMade += "MOVED: $Source -> $Destination"
        
        # Move meta file if exists
        if (Test-Path $sourceMeta) {
            Move-Item -Path $sourceMeta -Destination $destMeta -Force
            $changesMade += "MOVED: $sourceMeta -> $destMeta"
            Write-Host "    + Meta file" -ForegroundColor Gray
        }
        
        Write-Host "    ✓ Done" -ForegroundColor Green
    }
    else {
        Write-Host "  Skipping (not found): $Source" -ForegroundColor Gray
    }
}

# Function to create directory if not exists
function Ensure-Directory {
    param([string]$Path)
    
    if (-not (Test-Path $Path)) {
        New-Item -ItemType Directory -Path $Path -Force | Out-Null
        Write-Host "  Created directory: $Path" -ForegroundColor Cyan
    }
}

Write-Host "[Step 1] Ensuring target directories exist..." -ForegroundColor White
Ensure-Directory -Path (Join-Path $projectRoot "Assets\Scripts\Tests")
Ensure-Directory -Path (Join-Path $projectRoot "Assets\Scripts\Editor")

Write-Host ""
Write-Host "[Step 2] Moving test files from Core to Tests folder..." -ForegroundColor White

# Move MazeTorchTest.cs from Core/06_Maze to Tests
Move-FileWithMeta `
    -Source (Join-Path $projectRoot "Assets\Scripts\Core\06_Maze\MazeTorchTest.cs") `
    -Destination (Join-Path $projectRoot "Assets\Scripts\Tests\MazeTorchTest.cs")

Write-Host ""
Write-Host "[Step 3] Consolidating Editor files..." -ForegroundColor White

# Check if we should move Editor files from Assets/Editor/ to Assets/Scripts/Editor/
# For Unity assembly definitions, it's better to keep Editor scripts with their runtime counterparts
$editorFilesToMove = @(
    "CreateFreshMazeTestScene.cs",
    "FixMazeTestScene.cs",
    "DeleteBinaryFiles.cs",
    "DeleteAllGroundObjects.cs",
    "FloorMaterialFactoryMenu.cs",
    "PurgeOldGround.cs"
)

foreach ($file in $editorFilesToMove) {
    $source = Join-Path $projectRoot "Assets\Editor\$file"
    $dest = Join-Path $projectRoot "Assets\Scripts\Editor\$file"
    
    if (Test-Path $source) {
        Move-FileWithMeta -Source $source -Destination $dest
    }
}

Write-Host ""
Write-Host "[Step 4] Verifying final structure..." -ForegroundColor White

$expectedTestFiles = @(
    "FpsMazeTest.cs",
    "TorchManualActivator.cs",
    "DebugCameraIssue.cs",
    "MazeTorchTest.cs"
)

$expectedEditorFiles = @(
    "QuickSceneSetup.cs",
    "AddDoorSystemToScene.cs",
    "BuildScript.cs",
    "URPSetupUtility.cs",
    "CreateFreshMazeTestScene.cs",
    "FixMazeTestScene.cs",
    "DeleteBinaryFiles.cs",
    "DeleteAllGroundObjects.cs",
    "FloorMaterialFactoryMenu.cs",
    "PurgeOldGround.cs"
)

Write-Host ""
Write-Host "  Test Files (Assets/Scripts/Tests/):" -ForegroundColor Cyan
foreach ($file in $expectedTestFiles) {
    $path = Join-Path $projectRoot "Assets\Scripts\Tests\$file"
    if (Test-Path $path) {
        Write-Host "    ✓ $file" -ForegroundColor Green
    }
    else {
        Write-Host "    ✗ $file (MISSING)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "  Editor Files (Assets/Scripts/Editor/):" -ForegroundColor Cyan
foreach ($file in $expectedEditorFiles) {
    $path = Join-Path $projectRoot "Assets\Scripts\Editor\$file"
    if (Test-Path $path) {
        Write-Host "    ✓ $file" -ForegroundColor Green
    }
    else {
        Write-Host "    ✗ $file (MISSING)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "[Step 5] Checking for old directories to clean up..." -ForegroundColor White

$oldEditorDir = Join-Path $projectRoot "Assets\Editor"
if (Test-Path $oldEditorDir) {
    $remainingFiles = Get-ChildItem -Path $oldEditorDir -File
    if ($remainingFiles.Count -eq 0) {
        Write-Host "  Old Assets/Editor/ directory is empty, removing..." -ForegroundColor Yellow
        Remove-Item -Path $oldEditorDir -Force -Recurse
        $changesMade += "REMOVED: $oldEditorDir"
        Write-Host "    ✓ Directory removed" -ForegroundColor Green
    }
    else {
        Write-Host "  Old Assets/Editor/ still has files:" -ForegroundColor Yellow
        foreach ($f in $remainingFiles) {
            Write-Host "    - $($f.Name)" -ForegroundColor Gray
        }
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Reorganization Complete!" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

if ($changesMade.Count -gt 0) {
    Write-Host "Changes made ($($changesMade.Count)):" -ForegroundColor White
    foreach ($change in $changesMade) {
        Write-Host "  $change" -ForegroundColor Gray
    }
    Write-Host ""
    Write-Host "⚠️  IMPORTANT: Run backup.ps1 now!" -ForegroundColor Yellow
    Write-Host "   Command: .\backup.ps1" -ForegroundColor Yellow
    Write-Host ""
}
else {
    Write-Host "No changes were needed." -ForegroundColor Green
}

Write-Host "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
