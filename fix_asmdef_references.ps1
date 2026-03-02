# fix_asmdef_references.ps1
# Remove orphaned Code.Lavos.Maze assembly references
# Unity 6 compatible - UTF-8 encoding - Unix line endings

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     FIX ASMDEF REFERENCES - Remove Code.Lavos.Maze             ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$scriptRoot = $PSScriptRoot
$fixedCount = 0

# ============================================================================
# DELETE ORPHANED Code.Lavos.Maze.asmdef
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "🗑️  DELETE ORPHANED Code.Lavos.Maze.asmdef" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$orphanedAsmdef = Join-Path $scriptRoot "Assets\Scripts\Core\Code.Lavos.Maze.asmdef"
if (Test-Path $orphanedAsmdef) {
    try {
        Remove-Item $orphanedAsmdef -Force
        Write-Host "  ✅ DELETED: Assets/Scripts/Core/Code.Lavos.Maze.asmdef" -ForegroundColor Green
        $fixedCount++
    } catch {
        Write-Host "  ❌ FAILED: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "  ⚠️  Not found (already deleted)" -ForegroundColor Gray
}
Write-Host ""

# ============================================================================
# UPDATE REFERENCES IN OTHER .asmdef FILES
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📝 UPDATE .asmdef FILES - Remove Code.Lavos.Maze ref" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$filesToUpdate = @(
    "Assets\Scripts\Editor\Code.Lavos.Editor.asmdef",
    "Assets\Scripts\Ressources\Code.Lavos.Ressources.asmdef"
)

foreach ($file in $filesToUpdate) {
    $fullPath = Join-Path $scriptRoot $file
    $relativePath = $file -replace '\\', '/'

    Write-Host "  Processing: $relativePath" -ForegroundColor White

    if (Test-Path $fullPath) {
        try {
            $content = Get-Content $fullPath -Raw -Encoding UTF8

            # Remove the Code.Lavos.Maze reference line
            $newContent = $content -replace '(?m)^\s*"Code\.Lavos\.Maze",?\r?\n?', ''
            # Also clean up any trailing comma issues
            $newContent = $newContent -replace ',\s*]', ']'

            if ($content -ne $newContent) {
                # Write with UTF-8 encoding and Unix line endings
                $utf8NoBom = New-Object System.Text.UTF8Encoding $false
                [System.IO.File]::WriteAllText($fullPath, $newContent, $utf8NoBom)
                Write-Host "    ✅ Updated - removed Code.Lavos.Maze reference" -ForegroundColor Green
                $fixedCount++
            } else {
                Write-Host "    ℹ️  No changes needed" -ForegroundColor Yellow
            }
        } catch {
            Write-Host "    ❌ FAILED: $($_.Exception.Message)" -ForegroundColor Red
        }
    } else {
        Write-Host "    ⚠️  File not found" -ForegroundColor Gray
    }
    Write-Host ""
}

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
Write-Host "🔧 GIT REMINDER" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  After verifying everything works, commit changes:" -ForegroundColor White
Write-Host ""
Write-Host "  git add -A" -ForegroundColor Gray
Write-Host "  git commit -m `"fix: Remove orphaned Code.Lavos.Maze.asmdef`"" -ForegroundColor Cyan
Write-Host ""
