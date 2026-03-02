# fix_assembly_structure.ps1
# Reorganize scripts to eliminate circular dependencies
# Unity 6 compatible - UTF-8 encoding - Unix line endings

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     FIX ASSEMBLY STRUCTURE - Eliminate Circular Dependencies   ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$scriptRoot = $PSScriptRoot
$movedCount = 0

# ============================================================================
# MOVE SCRIPTS FROM CORE TO GAMEPLAY
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📦 MOVE SCRIPTS FROM CORE → GAMEPLAY" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Scripts to move from Core to Gameplay (they use Player, Inventory, Interaction types)
$scriptsToMove = @(
    "InteractionSystem.cs",
    "InteractionSystem.cs.meta",
    "DoorsEngine.cs",
    "DoorsEngine.cs.meta",
    "TrapBehavior.cs",
    "TrapBehavior.cs.meta",
    "ItemData.cs",
    "ItemData.cs.meta"
)

$gameplayDir = Join-Path $scriptRoot "Assets\Scripts\Gameplay"
$coreDir = Join-Path $scriptRoot "Assets\Scripts\Core"

# Ensure Gameplay directory exists
if (-not (Test-Path $gameplayDir)) {
    New-Item -ItemType Directory -Path $gameplayDir | Out-Null
    Write-Host "  ✅ Created Gameplay directory" -ForegroundColor Green
}

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
# UPDATE Code.Lavos.Gameplay.asmdef REFERENCES
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
            "Code.Lavos.Interaction"
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
    Write-Host "     Added reference: Code.Lavos.Interaction" -ForegroundColor Gray
    $movedCount++
} else {
    Write-Host "  ❌ Not found: Code.Lavos.Gameplay.asmdef" -ForegroundColor Red
}
Write-Host ""

# ============================================================================
# UPDATE Code.Lavos.Interaction.asmdef REFERENCES
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📝 UPDATE Code.Lavos.Interaction.asmdef" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$interactionAsmdefPath = Join-Path $scriptRoot "Assets\Scripts\Interaction\Code.Lavos.Interaction.asmdef"
if (Test-Path $interactionAsmdefPath) {
    $interactionContent = @{
        name = "Code.Lavos.Interaction"
        rootNamespace = "Code.Lavos.Interaction"
        references = @(
            "Code.Lavos.Core",
            "Code.Lavos.Player",
            "Code.Lavos.Inventory"
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
    [System.IO.File]::WriteAllText($interactionAsmdefPath, $interactionContent, $utf8NoBom)
    Write-Host "  ✅ Updated: Code.Lavos.Interaction.asmdef" -ForegroundColor Green
    Write-Host "     Added reference: Code.Lavos.Inventory" -ForegroundColor Gray
    $movedCount++
} else {
    Write-Host "  ❌ Not found: Code.Lavos.Interaction.asmdef" -ForegroundColor Red
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
Write-Host "  git commit -m `"fix: Reorganize scripts to eliminate circular dependencies`"" -ForegroundColor Cyan
Write-Host ""
