# Fix-Scene-Walls.ps1
# Quick fix for white walls in FpsMazeTest_Fresh.unity
# Adds proper ambient light and fixes render settings
#
# USAGE: Run from project root
#   .\fix-scene-walls.ps1

$ErrorActionPreference = "Stop"

$scenePath = "Assets/Scenes/FpsMazeTest_Fresh.unity"
$backupPath = "Assets/Scenes/FpsMazeTest_Fresh.unity.backup"

Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Fix Scene Walls - FpsMazeTest_Fresh.unity" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan

if (-not (Test-Path $scenePath)) {
    Write-Host "❌ Scene not found: $scenePath" -ForegroundColor Red
    exit 1
}

# Create backup
Write-Host "`n[1/3] Creating backup..." -ForegroundColor Yellow
Copy-Item $scenePath $backupPath -Force
Write-Host "  ✅ Backup: $backupPath" -ForegroundColor Green

# Read scene file
Write-Host "`n[2/3] Fixing render settings..." -ForegroundColor Yellow
$sceneContent = Get-Content $scenePath -Raw -Encoding UTF8

# Fix ambient light (increase from 0.35 to 0.6 for better visibility)
$sceneContent = $sceneContent -replace 'm_AmbientSkyColor: \{r: 0\.35, g: 0\.3, b: 0\.25, a: 1\}', 'm_AmbientSkyColor: {r: 0.6, g: 0.55, b: 0.5, a: 1}'

# Fix ambient intensity (increase from 1 to 1.5)
$sceneContent = $sceneContent -replace 'm_AmbientIntensity: 1', 'm_AmbientIntensity: 1.5'

# Fix fog density (reduce from 0.01 to 0.005 for less darkness)
$sceneContent = $sceneContent -replace 'm_FogDensity: 0\.01', 'm_FogDensity: 0.005'

# Fix fog start (reduce from 15 to 10 for smoother transition)
$sceneContent = $sceneContent -replace 'm_LinearFogStart: 15', 'm_LinearFogStart: 10'

# Fix fog end (reduce from 120 to 80 for better visibility)
$sceneContent = $sceneContent -replace 'm_LinearFogEnd: 120', 'm_LinearFogEnd: 80'

# Write fixed scene
Write-Host "`n[3/3] Saving fixed scene..." -ForegroundColor Yellow
$sceneContent | Set-Content $scenePath -Encoding UTF8 -NoNewline

Write-Host "`n═══════════════════════════════════════════" -ForegroundColor Green
Write-Host "  ✅ Scene Fixed Successfully!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════" -ForegroundColor Green
Write-Host "`nChanges applied:" -ForegroundColor White
Write-Host "  • Ambient Sky Color: 0.35 → 0.6 (brighter)" -ForegroundColor Cyan
Write-Host "  • Ambient Intensity: 1 → 1.5 (+50%)" -ForegroundColor Cyan
Write-Host "  • Fog Density: 0.01 → 0.005 (less dense)" -ForegroundColor Cyan
Write-Host "  • Fog Start: 15 → 10 (smoother)" -ForegroundColor Cyan
Write-Host "  • Fog End: 120 → 80 (better visibility)" -ForegroundColor Cyan
Write-Host "`n🎮 Open Unity and test the scene!" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════`n" -ForegroundColor Cyan
