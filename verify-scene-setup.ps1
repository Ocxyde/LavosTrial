# verify-scene-setup.ps1
# Verify Unity scene setup for game to run
# UTF-8 encoding - Unix line endings

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Scene Setup Verification" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "CHECKLIST - Verify in Unity Editor:" -ForegroundColor Yellow
Write-Host ""

Write-Host "1. PLAYER SETUP" -ForegroundColor Cyan
Write-Host "   [ ] Player GameObject exists in scene" -ForegroundColor Gray
Write-Host "   [ ] Has CharacterController component" -ForegroundColor Gray
Write-Host "   [ ] Has PlayerController script attached" -ForegroundColor Gray
Write-Host "   [ ] Has PlayerStats script attached" -ForegroundColor Gray
Write-Host "   [ ] Has PlayerHealth script attached (if used)" -ForegroundColor Gray
Write-Host "   [ ] Has Camera component (main camera)" -ForegroundColor Gray
Write-Host "   [ ] Tagged as 'Player'" -ForegroundColor Gray
Write-Host ""

Write-Host "2. GAME MANAGER" -ForegroundColor Cyan
Write-Host "   [ ] GameManager GameObject exists" -ForegroundColor Gray
Write-Host "   [ ] Has GameManager script attached" -ForegroundColor Gray
Write-Host "   [ ] Set to 'Dont Destroy On Load'" -ForegroundColor Gray
Write-Host ""

Write-Host "3. INPUT SYSTEM" -ForegroundColor Cyan
Write-Host "   [ ] Project Settings > Input System > Active" -ForegroundColor Gray
Write-Host "   [ ] InputSystem_Actions.inputactions exists" -ForegroundColor Gray
Write-Host "   [ ] Player actions configured (Move, Look, Jump, etc.)" -ForegroundColor Gray
Write-Host ""

Write-Host "4. UI/HUD" -ForegroundColor Cyan
Write-Host "   [ ] Canvas exists in scene" -ForegroundColor Gray
Write-Host "   [ ] HUDSystem or UIBarsSystem attached" -ForegroundColor Gray
Write-Host ""

Write-Host "5. SCENE ASSETS" -ForegroundColor Cyan
Write-Host "   [ ] Ground/floor exists (with Collider)" -ForegroundColor Gray
Write-Host "   [ ] Lighting is set up" -ForegroundColor Gray
Write-Host "   [ ] Camera is not clipping through objects" -ForegroundColor Gray
Write-Host ""

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Quick Fix Commands" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "In Unity Editor Menu:" -ForegroundColor Yellow
Write-Host "  1. Edit > Project Settings > Input System" -ForegroundColor Gray
Write-Host "     Ensure 'Active Input System' = 'Both' or 'New Input System'" -ForegroundColor Gray
Write-Host ""
Write-Host "  2. Check Console > Collapse errors (icon top-left)" -ForegroundColor Gray
Write-Host ""
Write-Host "  3. Try: Edit > Preferences > External Tools" -ForegroundColor Gray
Write-Host "     Generate .csproj files again" -ForegroundColor Gray
Write-Host ""

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Common Issues" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "ISSUE: Press Play but nothing happens" -ForegroundColor Yellow
Write-Host "SOLUTION:" -ForegroundColor Gray
Write-Host "  - Check if Player has CharacterController" -ForegroundColor White
Write-Host "  - Check if camera is child of Player" -ForegroundColor White
Write-Host "  - Press Tab to unlock cursor (may be locked)" -ForegroundColor White
Write-Host "  - Press ESC to show cursor, then click game view" -ForegroundColor White
Write-Host ""

Write-Host "ISSUE: Controls don't work" -ForegroundColor Yellow
Write-Host "SOLUTION:" -ForegroundColor Gray
Write-Host "  - Press ESC to unlock cursor" -ForegroundColor White
Write-Host "  - Click inside game window to focus" -ForegroundColor White
Write-Host "  - Check Input System is enabled" -ForegroundColor White
Write-Host ""

Write-Host "ISSUE: Black screen" -ForegroundColor Yellow
Write-Host "SOLUTION:" -ForegroundColor Gray
Write-Host "  - Check Camera is enabled" -ForegroundColor White
Write-Host "  - Check Camera culling mask includes all layers" -ForegroundColor White
Write-Host "  - Check lighting (add directional light)" -ForegroundColor White
Write-Host ""
