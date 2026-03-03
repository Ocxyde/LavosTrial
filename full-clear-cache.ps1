# Full Unity Cache Clear and Regenerate
# Run this script, then open Unity

$ErrorActionPreference = "Stop"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Full Unity Cache Clear" -ForegroundColor Cyan
Write-Host "  $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$projectRoot = $PSScriptRoot

# Close Unity if running
Write-Host "[1/6] Closing Unity if running..." -ForegroundColor Yellow
Get-Process "Unity" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2
Write-Host "  Unity closed" -ForegroundColor Green

# Delete Library folder
Write-Host "[2/6] Deleting Library folder..." -ForegroundColor Yellow
if (Test-Path "$projectRoot\Library") {
    Remove-Item "$projectRoot\Library" -Recurse -Force
    Write-Host "  Library deleted" -ForegroundColor Green
} else {
    Write-Host "  Library not found (already clean)" -ForegroundColor Gray
}

# Delete Temp folder
Write-Host "[3/6] Deleting Temp folder..." -ForegroundColor Yellow
if (Test-Path "$projectRoot\Temp") {
    Remove-Item "$projectRoot\Temp" -Recurse -Force
    Write-Host "  Temp deleted" -ForegroundColor Green
} else {
    Write-Host "  Temp not found" -ForegroundColor Gray
}

# Delete obj folder
Write-Host "[4/6] Deleting obj folder..." -ForegroundColor Yellow
if (Test-Path "$projectRoot\obj") {
    Remove-Item "$projectRoot\obj" -Recurse -Force
    Write-Host "  obj deleted" -ForegroundColor Green
} else {
    Write-Host "  obj not found" -ForegroundColor Gray
}

# Delete .vscode folder (VS Code cache)
Write-Host "[5/6] Deleting .vscode folder..." -ForegroundColor Yellow
if (Test-Path "$projectRoot\.vscode") {
    Remove-Item "$projectRoot\.vscode" -Recurse -Force
    Write-Host "  .vscode deleted" -ForegroundColor Green
} else {
    Write-Host "  .vscode not found" -ForegroundColor Gray
}

# Delete all .csproj and .sln files (will be regenerated)
Write-Host "[6/6] Deleting generated project files..." -ForegroundColor Yellow
Get-ChildItem $projectRoot -Filter "*.csproj" -File | Remove-Item -Force
Get-ChildItem $projectRoot -Filter "*.sln" -File | Remove-Item -Force
Write-Host "  Project files deleted" -ForegroundColor Green

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Cache Cleared Successfully!" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor White
Write-Host "  1. Open Unity Hub" -ForegroundColor Cyan
Write-Host "  2. Open this project" -ForegroundColor Cyan
Write-Host "  3. Wait for full recompile (2-5 minutes)" -ForegroundColor Cyan
Write-Host "  4. Check Console for errors" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
