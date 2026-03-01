# fix-all-issues.ps1
# Complete fix for all compilation issues
# UTF-8 encoding - Unix line endings
#
# Usage: powershell -ExecutionPolicy Bypass -File fix-all-issues.ps1

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Fix All Compilation Issues" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$filesToCheck = @(
    "Assets\Scripts\HUD\DebugHUD.cs",
    "Assets\Scripts\HUD\UIBarsSystem.cs",
    "Assets\Scripts\HUD\HUDSystem.cs",
    "Assets\Scripts\Player\PlayerHealth.cs",
    "Assets\Scripts\Player\PlayerStats.cs",
    "Assets\Scripts\Player\PlayerController.cs",
    "Assets\Scripts\Core\GameManager.cs"
)

foreach ($file in $filesToCheck) {
    $fullPath = Join-Path $scriptDir $file
    if (Test-Path $fullPath) {
        $content = Get-Content $fullPath -Raw -Encoding UTF8
        
        # Remove duplicate headers
        $content = $content -replace "^(// .*?\.cs\r?\n// .*?\r?\n// .*?\r?\n// .*?\r?\n.*?\r?\n)\r?\n// .*?\.cs\r?\n// .*?\r?\n// .*?\r?\n// .*?\r?\n.*?\r?\n", '$1'
        
        # Remove leading blank lines
        $content = $content -replace "^\r?\n+", ''
        
        # Normalize to Unix LF
        $content = $content -replace "`r`n", "`n"
        
        [System.IO.File]::WriteAllText($fullPath, $content, [System.Text.UTF8Encoding]::new($false))
        Write-Host "  [FIXED] $file" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "All files fixed. Now run backup:" -ForegroundColor Yellow
Write-Host "  powershell -ExecutionPolicy Bypass -File apply-patches-and-backup.ps1" -ForegroundColor Gray
Write-Host ""
