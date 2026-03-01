# fix-hud-namespace.ps1
# Fix HUD namespace issues for PlayerStats and PlayerHealth
# UTF-8 encoding - Unix line endings
#
# Usage: powershell -ExecutionPolicy Bypass -File fix-hud-namespace.ps1

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Fix HUD Namespace Issues" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Fix UIBarsSystem.cs
$uiBarsPath = "Assets\Scripts\HUD\UIBarsSystem.cs"
$uiBarsContent = Get-Content $uiBarsPath -Raw -Encoding UTF8

# Fix reflection type lookup - change from Code.Lavos.Status.PlayerStats to Code.Lavos.Core.PlayerStats
$uiBarsContent = $uiBarsContent -replace 'Type\.GetType\("Code\.Lavos\.Status\.PlayerStats', 'Type.GetType("Code.Lavos.Core.PlayerStats'

[System.IO.File]::WriteAllText($uiBarsPath, $uiBarsContent, [System.Text.UTF8Encoding]::new($false))
Write-Host "  [FIXED] UIBarsSystem.cs - Fixed PlayerStats namespace" -ForegroundColor Green

# Fix HUDSystem.cs  
$hudSystemPath = "Assets\Scripts\HUD\HUDSystem.cs"
$hudSystemContent = Get-Content $hudSystemPath -Raw -Encoding UTF8

# Fix PlayerStats references
$hudSystemContent = $hudSystemContent -replace 'Code\.Lavos\.Core\.PlayerStats\.Instance', 'PlayerStats.Instance'

[System.IO.File]::WriteAllText($hudSystemPath, $hudSystemContent, [System.Text.UTF8Encoding]::new($false))
Write-Host "  [FIXED] HUDSystem.cs - Fixed PlayerStats references" -ForegroundColor Green

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Fixes Complete" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Now run:" -ForegroundColor Yellow
Write-Host "  1. clear-unity-cache.bat" -ForegroundColor Gray
Write-Host "  2. Open Unity Editor" -ForegroundColor Gray
Write-Host ""
