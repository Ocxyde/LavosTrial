# fix_core_dependencies.ps1
# Move scripts with external dependencies from Core to Gameplay
# Unity 6 compatible - UTF-8 encoding - Unix line endings

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     FIX CORE DEPENDENCIES - Move scripts to Gameplay           ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$scriptRoot = $PSScriptRoot
$movedCount = 0

$gameplayDir = Join-Path $scriptRoot "Assets\Scripts\Gameplay"
$coreDir = Join-Path $scriptRoot "Assets\Scripts\Core"

# ============================================================================
# MOVE SCRIPTS WITH EXTERNAL DEPENDENCIES FROM CORE → GAMEPLAY
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📦 MOVE SCRIPTS FROM CORE → GAMEPLAY" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Scripts that use PlayerStats, PlayerController, Inventory, MazeRenderer, etc.
$scriptsToMove = @(
    "EventHandler.cs",
    "EventHandler.cs.meta",
    "EventHandlerInitializer.cs",
    "EventHandlerInitializer.cs.meta",
    "CombatSystem.cs",
    "CombatSystem.cs.meta",
    "MazeIntegration.cs",
    "MazeIntegration.cs.meta",
    "MazeSetupHelper.cs",
    "MazeSetupHelper.cs.meta",
    "RoomDoorPlacer.cs",
    "RoomDoorPlacer.cs.meta",
    "SpawnPlacerEngine.cs",
    "SpawnPlacerEngine.cs.meta",
    "SFXVIXEngine.cs",
    "SFXVIXEngine.cs.meta",
    "LootTable.cs",
    "LootTable.cs.meta"
)

foreach ($script in $scriptsToMove) {
    $sourcePath = Join-Path $coreDir $script
    $destPath = Join-Path $gameplayDir $script

    if (Test-Path $sourcePath) {
        try {
            Move-Item $sourcePath $destPath -Force
            Write-Host "  ✅ Moved: $script → Gameplay/" -ForegroundColor Green
            $movedCount++
        } catch {
            Write-Host "  ❌ Failed to move $script`: $($_.Exception.Message)" -ForegroundColor Red
        }
    } else {
        Write-Host "  ⚠️  Not found: $script" -ForegroundColor Gray
    }
}
Write-Host ""

# ============================================================================
# UPDATE Code.Lavos.Gameplay.asmdef
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📝 UPDATE Code.Lavos.Gameplay.asmdef" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$gameplayAsmdefPath = Join-Path $scriptRoot "Assets\Scripts\Gameplay\Code.Lavos.Gameplay.asmdef"
if (Test-Path $gameplayAsmdefPath) {
    $gameplayContent = @{
        name = "Code.Lavos.Gameplay"
        rootNamespace = "Code.Lavos.Gameplay"
        references = @(
            "Code.Lavos.Core",
            "Code.Lavos.Status",
            "Code.Lavos.Player",
            "Code.Lavos.HUD",
            "Code.Lavos.Inventory",
            "Code.Lavos.Interaction",
            "Code.Lavos.Ressources"
        )
        includePlatforms = @()
        excludePlatforms = @()
        allowUnsafeCode = $false
        overrideReferences = $false
        precompiledReferences = @()
        autoReferenced = $true
        generateProgrammingAssets = $false
    } | ConvertTo-Json -Depth 10

    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($gameplayAsmdefPath, $gameplayContent, $utf8NoBom)
    Write-Host "  ✅ Updated: Code.Lavos.Gameplay.asmdef" -ForegroundColor Green
    Write-Host "     Added reference: Code.Lavos.Ressources" -ForegroundColor Gray
    $movedCount++
} else {
    Write-Host "  ❌ Not found: Code.Lavos.Gameplay.asmdef" -ForegroundColor Red
}
Write-Host ""

# ============================================================================
# SUMMARY
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📊 SUMMARY" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Files moved/updated: $movedCount" -ForegroundColor $(if ($movedCount -gt 0) { "Green" } else { "Yellow" })
Write-Host ""

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "🔧 NEXT STEPS" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  1. Open Unity Editor and recompile" -ForegroundColor White
Write-Host "  2. Verify no circular dependency errors" -ForegroundColor White
Write-Host "  3. Test game functionality" -ForegroundColor White
Write-Host ""

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "🔧 GIT REMINDER" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  After verifying everything works, commit changes:" -ForegroundColor White
Write-Host ""
Write-Host "  git add -A" -ForegroundColor Gray
Write-Host "  git commit -m `"fix: Move Core scripts with external deps to Gameplay`"" -ForegroundColor Cyan
Write-Host ""
