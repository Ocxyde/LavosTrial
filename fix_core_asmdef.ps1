# fix_core_asmdef.ps1
# Fix Code.Lavos.Core.asmdef references and create missing Interaction asmdef
# Unity 6 compatible - UTF-8 encoding - Unix line endings

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     FIX CORE ASMDEF - Add missing references                   ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$scriptRoot = $PSScriptRoot
$fixedCount = 0

# ============================================================================
# CREATE MISSING Code.Lavos.Interaction.asmdef
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📝 CREATE Code.Lavos.Interaction.asmdef" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$interactionAsmdefPath = Join-Path $scriptRoot "Assets\Scripts\Interaction\Code.Lavos.Interaction.asmdef"
if (-not (Test-Path $interactionAsmdefPath)) {
    $interactionContent = @{
        name = "Code.Lavos.Interaction"
        rootNamespace = "Code.Lavos.Interaction"
        references = @(
            "Code.Lavos.Core",
            "Code.Lavos.Player"
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
    Write-Host "  ✅ Created: Assets/Scripts/Interaction/Code.Lavos.Interaction.asmdef" -ForegroundColor Green
    $fixedCount++
} else {
    Write-Host "  ℹ️  Already exists: Code.Lavos.Interaction.asmdef" -ForegroundColor Yellow
}
Write-Host ""

# ============================================================================
# UPDATE Code.Lavos.Core.asmdef - Add missing references
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📝 UPDATE Code.Lavos.Core.asmdef - Add references" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$coreAsmdefPath = Join-Path $scriptRoot "Assets\Scripts\Core\Code.Lavos.Core.asmdef"
if (Test-Path $coreAsmdefPath) {
    $coreContent = @{
        name = "Code.Lavos.Core"
        rootNamespace = "Code.Lavos.Core"
        references = @(
            "Code.Lavos.Status",
            "Code.Lavos.Player",
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
    [System.IO.File]::WriteAllText($coreAsmdefPath, $coreContent, $utf8NoBom)
    Write-Host "  ✅ Updated: Code.Lavos.Core.asmdef" -ForegroundColor Green
    Write-Host "     Added references:" -ForegroundColor Gray
    Write-Host "       - Code.Lavos.Status" -ForegroundColor DarkGray
    Write-Host "       - Code.Lavos.Player" -ForegroundColor DarkGray
    Write-Host "       - Code.Lavos.Inventory" -ForegroundColor DarkGray
    Write-Host "       - Code.Lavos.Interaction" -ForegroundColor DarkGray
    Write-Host "       - Code.Lavos.Ressources" -ForegroundColor DarkGray
    $fixedCount++
} else {
    Write-Host "  ❌ Not found: Code.Lavos.Core.asmdef" -ForegroundColor Red
}
Write-Host ""

# ============================================================================
# ADD INPUT SYSTEM PACKAGE REFERENCE
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📦 ADD INPUT SYSTEM PACKAGE REFERENCE" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

Write-Host "  ℹ️  The Input System package is required for UnityEngine.InputSystem" -ForegroundColor Cyan
Write-Host ""
Write-Host "  To add the Input System package, run in Unity:" -ForegroundColor White
Write-Host "    1. Open Package Manager (Window > Package Manager)" -ForegroundColor Gray
Write-Host "    2. Search for 'Input System'" -ForegroundColor Gray
Write-Host "    3. Click Install" -ForegroundColor Gray
Write-Host ""
Write-Host "  Or add to Packages/manifest.json:" -ForegroundColor White
Write-Host "    `"com.unity.inputsystem`": `"1.7.0`"" -ForegroundColor Gray
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
Write-Host "  1. Install Input System package (see above)" -ForegroundColor White
Write-Host "  2. Open Unity Editor and recompile" -ForegroundColor White
Write-Host "  3. Verify no compilation errors" -ForegroundColor White
Write-Host ""

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "🔧 GIT REMINDER" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  After verifying everything works, commit changes:" -ForegroundColor White
Write-Host ""
Write-Host "  git add -A" -ForegroundColor Gray
Write-Host "  git commit -m `"fix: Add missing asmdef references for Core assembly`"" -ForegroundColor Cyan
Write-Host ""
