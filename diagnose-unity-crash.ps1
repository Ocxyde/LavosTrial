# diagnose-unity-crash.ps1
# Diagnose why Unity crashes on Play
# UTF-8 encoding - Unix line endings

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Unity Crash Diagnosis" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Check PlayerSettings
Write-Host "1. INPUT SYSTEM CONFIGURATION" -ForegroundColor Yellow
$projectSettings = "ProjectSettings\ProjectSettings.asset"
if (Test-Path $projectSettings) {
    $content = Get-Content $projectSettings -Raw
    if ($content -match "activeInputHandler:\s*(\d+)") {
        $value = $matches[1]
        switch ($value) {
            "0" { Write-Host "   Input System: Old Input System (InputManager)" -ForegroundColor Red }
            "1" { Write-Host "   Input System: New Input System  " -ForegroundColor Green }
            "2" { Write-Host "   Input System: Both" -ForegroundColor Yellow }
        }
    }
}
Write-Host ""

# Check for PlayerController dependencies
Write-Host "2. PLAYERCONTROLLER DEPENDENCIES" -ForegroundColor Yellow
$playerController = "Assets\Scripts\Player\PlayerController.cs"
if (Test-Path $playerController) {
    $content = Get-Content $playerController -Raw
    Write-Host "   File exists: YES" -ForegroundColor Green
    
    if ($content -match "using\s+UnityEngine\.InputSystem") {
        Write-Host "   Uses New Input System: YES" -ForegroundColor Green
    } else {
        Write-Host "   Uses New Input System: NO  " -ForegroundColor Red
    }
    
    if ($content -match "RequireComponent.*CharacterController") {
        Write-Host "   Requires CharacterController: YES" -ForegroundColor Green
    } else {
        Write-Host "   Requires CharacterController: NO" -ForegroundColor Yellow
    }
}
Write-Host ""

# Check for common crash causes
Write-Host "3. COMMON CRASH CAUSES" -ForegroundColor Yellow

# Check if there's a scene with objects
$sceneFiles = Get-ChildItem -Path "Assets" -Filter "*.unity" -Recurse
if ($sceneFiles.Count -eq 0) {
    Write-Host "   WARNING: No scene files found!" -ForegroundColor Red
} else {
    Write-Host "   Scene files found: $($sceneFiles.Count)" -ForegroundColor Green
}

# Check for broken script references
Write-Host "   Checking for broken script references..." -ForegroundColor Gray
$metaFiles = Get-ChildItem -Path "Assets" -Filter "*.meta" -Recurse
$brokenCount = 0
foreach ($meta in $metaFiles) {
    $content = Get-Content $meta.FullName -Raw
    if ($content -match "m_Script:\s*{fileID:\s*0}") {
        $brokenCount++
    }
}
if ($brokenCount -gt 0) {
    Write-Host "   Broken script references: $brokenCount" -ForegroundColor Red
} else {
    Write-Host "   Broken script references: None" -ForegroundColor Green
}
Write-Host ""

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  RECOMMENDATIONS" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "If Unity crashes on Play:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. CLOSE Unity Editor completely" -ForegroundColor White
Write-Host "2. Delete 'Library' folder (clear-unity-cache.bat)" -ForegroundColor White
Write-Host "3. REOPEN Unity Editor" -ForegroundColor White
Write-Host "4. Wait for full compilation (watch status bar)" -ForegroundColor White
Write-Host "5. Create a NEW empty scene" -ForegroundColor White
Write-Host "6. Create empty GameObject → add TestStartup script" -ForegroundColor White
Write-Host "7. Press Play - check Console for TestStartup messages" -ForegroundColor White
Write-Host ""
Write-Host "If TestStartup works but PlayerController crashes:" -ForegroundColor Yellow
Write-Host "  - Player may be missing CharacterController component" -ForegroundColor White
Write-Host "  - Camera may not be set up correctly" -ForegroundColor White
Write-Host "  - Input System package may need reinstall" -ForegroundColor White
Write-Host ""
