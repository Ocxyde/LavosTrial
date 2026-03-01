# ============================================================
#  fix-png-and-shader.ps1 - Fix PNG and Shader issues
# ============================================================

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Fix PNG and Shader Issues" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Fix 1: Delete corrupted PNG
$pngFile = Join-Path $scriptDir "Assets\Art\door_double_sided.png"
if (Test-Path $pngFile) {
    Write-Host "  Removing corrupted PNG file..." -ForegroundColor Yellow
    Write-Host "    File: $pngFile" -ForegroundColor Gray
    
    # Set file as writable first
    Set-ItemProperty -Path $pngFile -Name IsReadOnly -Value $false -ErrorAction SilentlyContinue
    
    Remove-Item $pngFile -Force
    Write-Host "    Deleted!" -ForegroundColor Green
} else {
    Write-Host "  PNG file not found (already deleted?)" -ForegroundColor DarkGray
}

# Also delete meta file
$metaFile = $pngFile + ".meta"
if (Test-Path $metaFile) {
    Set-ItemProperty -Path $metaFile -Name IsReadOnly -Value $false -ErrorAction SilentlyContinue
    Remove-Item $metaFile -Force
    Write-Host "  Meta file deleted" -ForegroundColor Green
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Next Steps (Do in Unity):" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  1. Switch to URP (Recommended):" -ForegroundColor White
Write-Host "     - Edit → Project Settings → Graphics" -ForegroundColor Gray
Write-Host "     - Set 'Scriptable Render Pipeline Asset' to URP asset" -ForegroundColor Gray
Write-Host ""
Write-Host "  2. Or fix Built-in RP shadows:" -ForegroundColor White
Write-Host "     - Edit → Project Settings → Graphics" -ForegroundColor Gray
Write-Host "     - Built-in Shader Settings → Disable 'Use Legacy Shadows'" -ForegroundColor Gray
Write-Host ""
Write-Host "  3. Reimport Assets:" -ForegroundColor White
Write-Host "     - Assets → Reimport All" -ForegroundColor Gray
Write-Host ""
Write-Host "  The door texture will be regenerated at runtime by DoubleDoor.cs" -ForegroundColor Yellow
Write-Host ""
