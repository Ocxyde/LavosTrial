# Final Core Dependencies Fix
# Move remaining required types to Core assembly

$ErrorActionPreference = "Stop"
$coreDir = Join-Path $PSScriptRoot "Assets\Scripts\Core"
$ressourcesDir = Join-Path $PSScriptRoot "Assets\Scripts\Ressources"
$gameplayDir = Join-Path $PSScriptRoot "Assets\Scripts\Gameplay"

Write-Host "Moving remaining dependencies to Core..." -ForegroundColor Cyan

# Move MazeRenderer to Core
if (Test-Path "$ressourcesDir\MazeRenderer.cs") {
    Move-Item "$ressourcesDir\MazeRenderer.cs" "$coreDir\MazeRenderer.cs" -Force
    Move-Item "$ressourcesDir\MazeRenderer.cs.meta" "$coreDir\MazeRenderer.cs.meta" -Force
    Write-Host "  Moved: MazeRenderer.cs" -ForegroundColor Green
}

# Move DoorsEngine to Core
if (Test-Path "$gameplayDir\DoorsEngine.cs") {
    Move-Item "$gameplayDir\DoorsEngine.cs" "$coreDir\DoorsEngine.cs" -Force
    Move-Item "$gameplayDir\DoorsEngine.cs.meta" "$coreDir\DoorsEngine.cs.meta" -Force
    Write-Host "  Moved: DoorsEngine.cs" -ForegroundColor Green
}

# Move DoorSFXManager to Core
if (Test-Path "$ressourcesDir\DoorSFXManager.cs") {
    Move-Item "$ressourcesDir\DoorSFXManager.cs" "$coreDir\DoorSFXManager.cs" -Force
    Move-Item "$ressourcesDir\DoorSFXManager.cs.meta" "$coreDir\DoorSFXManager.cs.meta" -Force
    Write-Host "  Moved: DoorSFXManager.cs" -ForegroundColor Green
}

# Move PixelArtDoorTextures to Core
if (Test-Path "$ressourcesDir\PixelArtDoorTextures.cs") {
    Move-Item "$ressourcesDir\PixelArtDoorTextures.cs" "$coreDir\PixelArtDoorTextures.cs" -Force
    Move-Item "$ressourcesDir\PixelArtDoorTextures.cs.meta" "$coreDir\PixelArtDoorTextures.cs.meta" -Force
    Write-Host "  Moved: PixelArtDoorTextures.cs" -ForegroundColor Green
}

Write-Host "`nDone! Open Unity to recompile." -ForegroundColor Green
