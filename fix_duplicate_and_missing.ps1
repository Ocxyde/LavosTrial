# Fix duplicate enums and missing types

$ErrorActionPreference = "Stop"
$coreDir = Join-Path $PSScriptRoot "Assets\Scripts\Core"
$playerDir = Join-Path $PSScriptRoot "Assets\Scripts\Player"

Write-Host "Fixing Core compilation errors..." -ForegroundColor Cyan

# 1. Delete DoorTypes.cs (enums are in DoorsEngine.cs)
if (Test-Path "$coreDir\DoorTypes.cs") {
    Remove-Item "$coreDir\DoorTypes.cs" -Force
    Remove-Item "$coreDir\DoorTypes.cs.meta" -Force
    Write-Host "  Deleted: DoorTypes.cs (duplicate enums)" -ForegroundColor Green
}

# 2. Move PlayerController to Core
if (Test-Path "$playerDir\PlayerController.cs") {
    Move-Item "$playerDir\PlayerController.cs" "$coreDir\PlayerController.cs" -Force
    Move-Item "$playerDir\PlayerController.cs.meta" "$coreDir\PlayerController.cs.meta" -Force
    Write-Host "  Moved: PlayerController.cs" -ForegroundColor Green
}

# 3. Move PlayerStats to Core (it's referenced everywhere)
if (Test-Path "$playerDir\PlayerStats.cs") {
    Move-Item "$playerDir\PlayerStats.cs" "$coreDir\PlayerStats.cs" -Force
    Move-Item "$playerDir\PlayerStats.cs.meta" "$coreDir\PlayerStats.cs.meta" -Force
    Write-Host "  Moved: PlayerStats.cs" -ForegroundColor Green
}

Write-Host "`nDone! Open Unity to recompile." -ForegroundColor Green
