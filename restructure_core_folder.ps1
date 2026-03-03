# Restructure Core folder into organized subfolders
# Unity 6 compatible - preserves meta files

$ErrorActionPreference = "Stop"
$coreDir = Join-Path $PSScriptRoot "Assets\Scripts\Core"

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     RESTRUCTURE CORE FOLDER - Organized subfolders            ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Create subfolders
$subfolders = @(
    "01_CoreSystems",      # GameManager, EventHandler, CoreInterfaces
    "02_Player",           # PlayerController, PlayerStats
    "03_Interaction",      # InteractionSystem
    "04_Inventory",        # Inventory, InventorySlot, ItemData, ItemTypes, ItemEngine
    "05_Combat",           # CombatSystem, Ennemi
    "06_Maze",             # MazeGenerator, MazeIntegration, MazeRenderer, MazeSetupHelper, RoomGenerator
    "07_Doors",            # DoorsEngine, DoorAnimation, DoorHolePlacer, RoomDoorPlacer, DoorSystemSetup, RealisticDoorFactory, DoorSFXManager, PixelArtDoorTextures
    "08_Environment",      # ChestBehavior, TrapBehavior, TrapType, SpawnPlacerEngine
    "09_Visual",           # ParticleGenerator, SFXVFXEngine, FlameAnimator, BraseroFlame, DrawingManager, DrawingPool
    "10_Resources",        # TorchController, TorchPool, LootTable, SeedManager
    "11_Utilities"         # GameManager (if not in CoreSystems)
)

foreach ($folder in $subfolders) {
    $path = Join-Path $coreDir $folder
    if (-not (Test-Path $path)) {
        New-Item -ItemType Directory -Path $path | Out-Null
        Write-Host "  ✅ Created: $folder/" -ForegroundColor Green
    }
}
Write-Host ""

# Define file mappings: "filename.cs" -> "Subfolder"
$fileMappings = @{
    # Core Systems
    "EventHandler.cs" = "01_CoreSystems"
    "EventHandler.cs.meta" = "01_CoreSystems"
    "EventHandlerInitializer.cs" = "01_CoreSystems"
    "EventHandlerInitializer.cs.meta" = "01_CoreSystems"
    "CoreInterfaces.cs" = "01_CoreSystems"
    "CoreInterfaces.cs.meta" = "01_CoreSystems"
    "GameManager.cs" = "01_CoreSystems"
    "GameManager.cs.meta" = "01_CoreSystems"
    
    # Player
    "PlayerController.cs" = "02_Player"
    "PlayerController.cs.meta" = "02_Player"
    "PlayerStats.cs" = "02_Player"
    "PlayerStats.cs.meta" = "02_Player"
    
    # Interaction
    "InteractionSystem.cs" = "03_Interaction"
    "InteractionSystem.cs.meta" = "03_Interaction"
    
    # Inventory
    "Inventory.cs" = "04_Inventory"
    "Inventory.cs.meta" = "04_Inventory"
    "InventorySlot.cs" = "04_Inventory"
    "InventorySlot.cs.meta" = "04_Inventory"
    "ItemData.cs" = "04_Inventory"
    "ItemData.cs.meta" = "04_Inventory"
    "ItemTypes.cs" = "04_Inventory"
    "ItemTypes.cs.meta" = "04_Inventory"
    "ItemEngine.cs" = "04_Inventory"
    "ItemEngine.cs.meta" = "04_Inventory"
    
    # Combat
    "CombatSystem.cs" = "05_Combat"
    "CombatSystem.cs.meta" = "05_Combat"
    "Ennemi.cs" = "05_Combat"
    "Ennemi.cs.meta" = "05_Combat"
    
    # Maze
    "MazeGenerator.cs" = "06_Maze"
    "MazeGenerator.cs.meta" = "06_Maze"
    "MazeIntegration.cs" = "06_Maze"
    "MazeIntegration.cs.meta" = "06_Maze"
    "MazeRenderer.cs" = "06_Maze"
    "MazeRenderer.cs.meta" = "06_Maze"
    "MazeSetupHelper.cs" = "06_Maze"
    "MazeSetupHelper.cs.meta" = "06_Maze"
    "RoomGenerator.cs" = "06_Maze"
    "RoomGenerator.cs.meta" = "06_Maze"
    
    # Doors
    "DoorsEngine.cs" = "07_Doors"
    "DoorsEngine.cs.meta" = "07_Doors"
    "DoorAnimation.cs" = "07_Doors"
    "DoorAnimation.cs.meta" = "07_Doors"
    "DoorHolePlacer.cs" = "07_Doors"
    "DoorHolePlacer.cs.meta" = "07_Doors"
    "RoomDoorPlacer.cs" = "07_Doors"
    "RoomDoorPlacer.cs.meta" = "07_Doors"
    "DoorSystemSetup.cs" = "07_Doors"
    "DoorSystemSetup.cs.meta" = "07_Doors"
    "RealisticDoorFactory.cs" = "07_Doors"
    "RealisticDoorFactory.cs.meta" = "07_Doors"
    "DoorSFXManager.cs" = "07_Doors"
    "DoorSFXManager.cs.meta" = "07_Doors"
    "PixelArtDoorTextures.cs" = "07_Doors"
    "PixelArtDoorTextures.cs.meta" = "07_Doors"
    
    # Environment
    "ChestBehavior.cs" = "08_Environment"
    "ChestBehavior.cs.meta" = "08_Environment"
    "TrapBehavior.cs" = "08_Environment"
    "TrapBehavior.cs.meta" = "08_Environment"
    "TrapType.cs" = "08_Environment"
    "TrapType.cs.meta" = "08_Environment"
    "SpawnPlacerEngine.cs" = "08_Environment"
    "SpawnPlacerEngine.cs.meta" = "08_Environment"
    
    # Visual
    "ParticleGenerator.cs" = "09_Visual"
    "ParticleGenerator.cs.meta" = "09_Visual"
    "SFXVFXEngine.cs" = "09_Visual"
    "SFXVFXEngine.cs.meta" = "09_Visual"
    "FlameAnimator.cs" = "09_Visual"
    "FlameAnimator.cs.meta" = "09_Visual"
    "BraseroFlame.cs" = "09_Visual"
    "BraseroFlame.cs.meta" = "09_Visual"
    "DrawingManager.cs" = "09_Visual"
    "DrawingManager.cs.meta" = "09_Visual"
    "DrawingPool.cs" = "09_Visual"
    "DrawingPool.cs.meta" = "09_Visual"
    
    # Resources
    "TorchController.cs" = "10_Resources"
    "TorchController.cs.meta" = "10_Resources"
    "TorchPool.cs" = "10_Resources"
    "TorchPool.cs.meta" = "10_Resources"
    "LootTable.cs" = "10_Resources"
    "LootTable.cs.meta" = "10_Resources"
    "SeedManager.cs" = "10_Resources"
    "SeedManager.cs.meta" = "10_Resources"
}

# Move files
$movedCount = 0
foreach ($file in $fileMappings.Keys) {
    $sourcePath = Join-Path $coreDir $file
    $destFolder = $fileMappings[$file]
    $destPath = Join-Path $coreDir "$destFolder\$file"
    
    if (Test-Path $sourcePath) {
        try {
            Move-Item $sourcePath $destPath -Force
            Write-Host "  ✅ Moved: $file → $destFolder/" -ForegroundColor Green
            $movedCount++
        } catch {
            Write-Host "  ❌ Failed to move $file`: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📊 SUMMARY" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Files moved: $movedCount" -ForegroundColor White
Write-Host ""
Write-Host "  New folder structure:" -ForegroundColor White
Write-Host "    Core/01_CoreSystems/     - GameManager, EventHandler, Interfaces" -ForegroundColor Gray
Write-Host "    Core/02_Player/          - PlayerController, PlayerStats" -ForegroundColor Gray
Write-Host "    Core/03_Interaction/     - InteractionSystem" -ForegroundColor Gray
Write-Host "    Core/04_Inventory/       - Inventory, Items" -ForegroundColor Gray
Write-Host "    Core/05_Combat/          - CombatSystem, Enemies" -ForegroundColor Gray
Write-Host "    Core/06_Maze/            - Maze generation" -ForegroundColor Gray
Write-Host "    Core/07_Doors/           - Door system" -ForegroundColor Gray
Write-Host "    Core/08_Environment/     - Chests, Traps, Spawning" -ForegroundColor Gray
Write-Host "    Core/09_Visual/          - Particles, VFX, Flames" -ForegroundColor Gray
Write-Host "    Core/10_Resources/       - Torches, Loot, Seed" -ForegroundColor Gray
Write-Host ""
Write-Host "  ⚠️  Open Unity to reimport files!" -ForegroundColor Yellow
Write-Host ""
