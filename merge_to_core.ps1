# merge_to_core.ps1
# Merge all gameplay scripts into Core assembly
# Unity 6 compatible - UTF-8 encoding - Unix line endings
#
# This matches your plug-in-and-out architecture:
# - Core = Central hub with ALL gameplay logic
# - Status = Independent module (no dependencies)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     MERGE TO CORE - Consolidate gameplay assemblies            ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$scriptRoot = $PSScriptRoot
$movedCount = 0

# ============================================================================
# MOVE ALL SCRIPTS FROM PLAYER TO CORE
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📦 MOVE PLAYER → CORE" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$playerDir = Join-Path $scriptRoot "Assets\Scripts\Player"
$coreDir = Join-Path $scriptRoot "Assets\Scripts\Core"

Get-ChildItem -Path $playerDir -Filter "*.cs" -File | ForEach-Object {
    $destPath = Join-Path $coreDir $_.Name
    Move-Item $_.FullName $destPath -Force
    Write-Host "  ✅ Moved: $($_.Name) → Core/" -ForegroundColor Green
    $movedCount++
}

Get-ChildItem -Path $playerDir -Filter "*.cs.meta" -File | ForEach-Object {
    $destPath = Join-Path $coreDir $_.Name
    if (Test-Path $destPath) {
        Remove-Item $destPath -Force
    }
    Move-Item $_.FullName $destPath -Force
    Write-Host "  ✅ Moved: $($_.Name) → Core/" -ForegroundColor Green
    $movedCount++
}
Write-Host ""

# ============================================================================
# MOVE ALL SCRIPTS FROM INVENTORY TO CORE
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📦 MOVE INVENTORY → CORE" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$inventoryDir = Join-Path $scriptRoot "Assets\Scripts\Inventory"

Get-ChildItem -Path $inventoryDir -Filter "*.cs" -File | ForEach-Object {
    $destPath = Join-Path $coreDir $_.Name
    Move-Item $_.FullName $destPath -Force
    Write-Host "  ✅ Moved: $($_.Name) → Core/" -ForegroundColor Green
    $movedCount++
}

Get-ChildItem -Path $inventoryDir -Filter "*.cs.meta" -File | ForEach-Object {
    $destPath = Join-Path $coreDir $_.Name
    if (Test-Path $destPath) {
        Remove-Item $destPath -Force
    }
    Move-Item $_.FullName $destPath -Force
    Write-Host "  ✅ Moved: $($_.Name) → Core/" -ForegroundColor Green
    $movedCount++
}
Write-Host ""

# ============================================================================
# MOVE ALL SCRIPTS FROM HUD TO CORE
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📦 MOVE HUD → CORE" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$hudDir = Join-Path $scriptRoot "Assets\Scripts\HUD"

Get-ChildItem -Path $hudDir -Filter "*.cs" -File | ForEach-Object {
    $destPath = Join-Path $coreDir $_.Name
    Move-Item $_.FullName $destPath -Force
    Write-Host "  ✅ Moved: $($_.Name) → Core/" -ForegroundColor Green
    $movedCount++
}

Get-ChildItem -Path $hudDir -Filter "*.cs.meta" -File | ForEach-Object {
    $destPath = Join-Path $coreDir $_.Name
    if (Test-Path $destPath) {
        Remove-Item $destPath -Force
    }
    Move-Item $_.FullName $destPath -Force
    Write-Host "  ✅ Moved: $($_.Name) → Core/" -ForegroundColor Green
    $movedCount++
}
Write-Host ""

# ============================================================================
# MOVE ALL SCRIPTS FROM GAMEPLAY TO CORE
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📦 MOVE GAMEPLAY → CORE" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$gameplayDir = Join-Path $scriptRoot "Assets\Scripts\Gameplay"

Get-ChildItem -Path $gameplayDir -Filter "*.cs" -File | ForEach-Object {
    $destPath = Join-Path $coreDir $_.Name
    Move-Item $_.FullName $destPath -Force
    Write-Host "  ✅ Moved: $($_.Name) → Core/" -ForegroundColor Green
    $movedCount++
}

Get-ChildItem -Path $gameplayDir -Filter "*.cs.meta" -File | ForEach-Object {
    $destPath = Join-Path $coreDir $_.Name
    if (Test-Path $destPath) {
        Remove-Item $destPath -Force
    }
    Move-Item $_.FullName $destPath -Force
    Write-Host "  ✅ Moved: $($_.Name) → Core/" -ForegroundColor Green
    $movedCount++
}
Write-Host ""

# ============================================================================
# MOVE ALL SCRIPTS FROM INTERACTION TO CORE
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📦 MOVE INTERACTION → CORE" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$interactionDir = Join-Path $scriptRoot "Assets\Scripts\Interaction"

Get-ChildItem -Path $interactionDir -Filter "*.cs" -File | ForEach-Object {
    $destPath = Join-Path $coreDir $_.Name
    Move-Item $_.FullName $destPath -Force
    Write-Host "  ✅ Moved: $($_.Name) → Core/" -ForegroundColor Green
    $movedCount++
}

Get-ChildItem -Path $interactionDir -Filter "*.cs.meta" -File | ForEach-Object {
    $destPath = Join-Path $coreDir $_.Name
    if (Test-Path $destPath) {
        Remove-Item $destPath -Force
    }
    Move-Item $_.FullName $destPath -Force
    Write-Host "  ✅ Moved: $($_.Name) → Core/" -ForegroundColor Green
    $movedCount++
}
Write-Host ""

# ============================================================================
# MOVE ALL SCRIPTS FROM ENNEMIES TO CORE
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📦 MOVE ENNEMIES → CORE" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$ennemiesDir = Join-Path $scriptRoot "Assets\Scripts\Ennemies"

Get-ChildItem -Path $ennemiesDir -Filter "*.cs" -File | ForEach-Object {
    $destPath = Join-Path $coreDir $_.Name
    Move-Item $_.FullName $destPath -Force
    Write-Host "  ✅ Moved: $($_.Name) → Core/" -ForegroundColor Green
    $movedCount++
}

Get-ChildItem -Path $ennemiesDir -Filter "*.cs.meta" -File | ForEach-Object {
    $destPath = Join-Path $coreDir $_.Name
    if (Test-Path $destPath) {
        Remove-Item $destPath -Force
    }
    Move-Item $_.FullName $destPath -Force
    Write-Host "  ✅ Moved: $($_.Name) → Core/" -ForegroundColor Green
    $movedCount++
}
Write-Host ""

# ============================================================================
# UPDATE Code.Lavos.Core.asmdef - Remove all assembly references
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📝 UPDATE Code.Lavos.Core.asmdef" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$coreAsmdefPath = Join-Path $scriptRoot "Assets\Scripts\Core\Code.Lavos.Core.asmdef"
if (Test-Path $coreAsmdefPath) {
    $coreContent = @{
        name = "Code.Lavos.Core"
        rootNamespace = "Code.Lavos.Core"
        references = @(
            "GUID:6280726566d84ea0be05b4f0a91fd88d"
        )
        includePlatforms = @()
        excludePlatforms = @()
        allowUnsafeCode = $false
        overrideReferences = $true
        precompiledReferences = @()
        autoReferenced = $true
        generateProgrammingAssets = $false
    } | ConvertTo-Json -Depth 10

    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($coreAsmdefPath, $coreContent, $utf8NoBom)
    Write-Host "  ✅ Updated: Code.Lavos.Core.asmdef" -ForegroundColor Green
    Write-Host "     Now only depends on: Input System package" -ForegroundColor Gray
    $movedCount++
} else {
    Write-Host "  ❌ Not found: Code.Lavos.Core.asmdef" -ForegroundColor Red
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
Write-Host "  New Assembly Structure:" -ForegroundColor White
Write-Host "    Code.Lavos.Core (all gameplay scripts)" -ForegroundColor Cyan
Write-Host "    Code.Lavos.Status (independent - no deps)" -ForegroundColor Cyan
Write-Host "    Code.Lavos.Ressources (MazeRenderer, etc.)" -ForegroundColor Cyan
Write-Host "    Code.Lavos.Editor (editor tools)" -ForegroundColor Cyan
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
Write-Host "  git commit -m `"fix: Consolidate gameplay scripts into Core assembly`"" -ForegroundColor Cyan
Write-Host ""
