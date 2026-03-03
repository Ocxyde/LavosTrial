# fix_remaining_core_deps.ps1
# Fix remaining Core dependencies
# Unity 6 compatible - UTF-8 encoding - Unix line endings

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     FIX REMAINING CORE DEPENDENCIES                            ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$scriptRoot = $PSScriptRoot
$coreDir = Join-Path $scriptRoot "Assets\Scripts\Core"
$ressourcesDir = Join-Path $scriptRoot "Assets\Scripts\Ressources"
$gameplayDir = Join-Path $scriptRoot "Assets\Scripts\Gameplay"
$fixedCount = 0

# ============================================================================
# MOVE TorchPool.cs TO CORE
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📦 MOVE TorchPool.cs TO CORE" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$torchPoolSrc = Join-Path $ressourcesDir "TorchPool.cs"
$torchPoolDest = Join-Path $coreDir "TorchPool.cs"
if (Test-Path $torchPoolSrc) {
    Move-Item $torchPoolSrc $torchPoolDest -Force
    Write-Host "  ✅ Moved: TorchPool.cs → Core/" -ForegroundColor Green
    $fixedCount++
}
if (Test-Path "$torchPoolSrc.meta") {
    Move-Item "$torchPoolSrc.meta" "$torchPoolDest.meta" -Force
}
Write-Host ""

# ============================================================================
# MOVE DrawingPool.cs TO CORE
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📦 MOVE DrawingPool.cs TO CORE" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$drawingPoolSrc = Join-Path $ressourcesDir "DrawingPool.cs"
$drawingPoolDest = Join-Path $coreDir "DrawingPool.cs"
if (Test-Path $drawingPoolSrc) {
    Move-Item $drawingPoolSrc $drawingPoolDest -Force
    Write-Host "  ✅ Moved: DrawingPool.cs → Core/" -ForegroundColor Green
    $fixedCount++
}
if (Test-Path "$drawingPoolSrc.meta") {
    Move-Item "$drawingPoolSrc.meta" "$drawingPoolDest.meta" -Force
}
Write-Host ""

# ============================================================================
# MOVE RealisticDoorFactory.cs TO CORE
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📦 MOVE RealisticDoorFactory.cs TO CORE" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$doorFactorySrc = Join-Path $ressourcesDir "RealisticDoorFactory.cs"
$doorFactoryDest = Join-Path $coreDir "RealisticDoorFactory.cs"
if (Test-Path $doorFactorySrc) {
    Move-Item $doorFactorySrc $doorFactoryDest -Force
    Write-Host "  ✅ Moved: RealisticDoorFactory.cs → Core/" -ForegroundColor Green
    $fixedCount++
}
if (Test-Path "$doorFactorySrc.meta") {
    Move-Item "$doorFactorySrc.meta" "$doorFactoryDest.meta" -Force
}
Write-Host ""

# ============================================================================
# MOVE TrapBehavior.cs TO CORE
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📦 MOVE TrapBehavior.cs TO CORE" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$trapBehaviorSrc = Join-Path $gameplayDir "TrapBehavior.cs"
$trapBehaviorDest = Join-Path $coreDir "TrapBehavior.cs"
if (Test-Path $trapBehaviorSrc) {
    Move-Item $trapBehaviorSrc $trapBehaviorDest -Force
    Write-Host "  ✅ Moved: TrapBehavior.cs → Core/" -ForegroundColor Green
    $fixedCount++
}
if (Test-Path "$trapBehaviorSrc.meta") {
    Move-Item "$trapBehaviorSrc.meta" "$trapBehaviorDest.meta" -Force
}
Write-Host ""

# ============================================================================
# UPDATE TrapType.cs - ADD MISSING ENUM VALUES
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📝 UPDATE TrapType.cs - ADD MISSING VALUES" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$trapTypePath = Join-Path $coreDir "TrapType.cs"
$trapTypeContent = @'
// TrapType.cs
// Trap type enum for Core assembly
// Unity 6 compatible - UTF-8 encoding - Unix line endings

namespace Code.Lavos.Core
{
    /// <summary>
    /// Types of traps that can be placed in the maze.
    /// </summary>
    public enum TrapType
    {
        None,
        Pit,            // Hole in floor - player falls in
        Spike,          // Deals physical damage
        Dart,           // Shooting dart trap
        Fire,           // Deals fire damage over time
        Flame,          // Wall flame trap
        Poison,         // Applies poison DoT
        Freeze,         // Slows/freezes player
        Electric,       // Deals lightning damage
        Shock,          // Electric shock trap
        Ice,            // Ice slip trap
        Teleport,       // Teleports player to random location
        Alarm,          // Alerts nearby enemies
        Collapse,       // Causes ceiling/wall collapse
        Explosion       // Explosive trap
    }
}
'@

$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText($trapTypePath, $trapTypeContent, $utf8NoBom)
Write-Host "  ✅ Updated: TrapType.cs with all trap types" -ForegroundColor Green
$fixedCount++
Write-Host ""

# ============================================================================
# FIX MazeSetupHelper.cs - Use Component instead of concrete types
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📝 FIX MazeSetupHelper.cs" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$mazeSetupPath = Join-Path $coreDir "MazeSetupHelper.cs"
if (Test-Path $mazeSetupPath) {
    $content = Get-Content $mazeSetupPath -Raw -Encoding UTF8
    
    # Replace concrete types with Component
    $content = $content -replace 'AddOrGetComponent<MazeRenderer>\(\)', 'AddOrGetComponent(typeof(MazeRenderer))'
    $content = $content -replace 'AddOrGetComponent<TorchPool>\(\)', 'AddOrGetComponent(typeof(TorchPool))'
    $content = $content -replace 'AddOrGetComponent<DrawingPool>\(\)', 'AddOrGetComponent(typeof(DrawingPool))'
    
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($mazeSetupPath, $content, $utf8NoBom)
    Write-Host "  ✅ Fixed: MazeSetupHelper.cs" -ForegroundColor Green
    $fixedCount++
}
Write-Host ""

# ============================================================================
# FIX DoorSystemSetup.cs
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📝 FIX DoorSystemSetup.cs" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$doorSetupPath = Join-Path $coreDir "DoorSystemSetup.cs"
if (Test-Path $doorSetupPath) {
    $content = Get-Content $doorSetupPath -Raw -Encoding UTF8
    
    # Replace MazeRenderer with Component
    $content = $content -replace 'AddOrGetComponent<MazeRenderer>\(\)', 'AddOrGetComponent(typeof(MazeRenderer))'
    
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($doorSetupPath, $content, $utf8NoBom)
    Write-Host "  ✅ Fixed: DoorSystemSetup.cs" -ForegroundColor Green
    $fixedCount++
}
Write-Host ""

# ============================================================================
# FIX CombatSystem.cs - Remove PlayerHealth reference
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📝 FIX CombatSystem.cs - Remove deprecated PlayerHealth" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$combatPath = Join-Path $coreDir "CombatSystem.cs"
if (Test-Path $combatPath) {
    $content = Get-Content $combatPath -Raw -Encoding UTF8
    
    # Replace PlayerHealth with PlayerStats
    $content = $content -replace 'PlayerHealth', 'PlayerStats'
    
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($combatPath, $content, $utf8NoBom)
    Write-Host "  ✅ Fixed: CombatSystem.cs" -ForegroundColor Green
    $fixedCount++
}
Write-Host ""

# ============================================================================
# FIX EventHandler.cs - Use static events from IPlayerStats
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📝 FIX EventHandler.cs" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$eventHandlerPath = Join-Path $coreDir "EventHandler.cs"
if (Test-Path $eventHandlerPath) {
    $content = Get-Content $eventHandlerPath -Raw -Encoding UTF8
    
    # Replace PlayerStats static event references with interface-compatible approach
    $content = $content -replace 'PlayerStats\.OnHealthChanged', 'OnPlayerHealthChangedStatic'
    $content = $content -replace 'PlayerStats\.OnPlayerDied', 'OnPlayerDiedStatic'
    
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($eventHandlerPath, $content, $utf8NoBom)
    Write-Host "  ✅ Fixed: EventHandler.cs" -ForegroundColor Green
    $fixedCount++
}
Write-Host ""

# ============================================================================
# SUMMARY
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📊 SUMMARY" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Files fixed: $fixedCount" -ForegroundColor $(if ($fixedCount -gt 0) { "Green" } else { "Yellow" })
Write-Host ""

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "🔧 NEXT STEPS" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  1. Open Unity Editor and recompile" -ForegroundColor White
Write-Host "  2. Verify no compilation errors" -ForegroundColor White
Write-Host ""
