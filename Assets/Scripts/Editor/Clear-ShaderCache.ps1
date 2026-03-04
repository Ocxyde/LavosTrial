# Clear-ShaderCache.ps1
# Clears Unity shader cache to fix shader compilation errors
# Unity 6 compatible - UTF-8 encoding - Unix line endings
#
# USAGE:
#   1. Close Unity Editor
#   2. Run this script
#   3. Reopen Unity Editor
#
# Location: Assets/Scripts/Editor/

param(
    [switch]$WhatIf  # Show what would be deleted without making changes
)

$ErrorActionPreference = "Stop"

Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  CLEAR SHADER CACHE" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Get project root (parent of Assets folder)
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent (Split-Path -Parent $scriptPath)

Write-Host "Project Root: $projectRoot" -ForegroundColor Gray
Write-Host ""

# Shader cache locations
$shaderCachePaths = @(
    "$projectRoot\Library\ShaderCache",
    "$projectRoot\Library\ShaderCache",
    "$env:LOCALAPPDATA\Unity\cache\shaders"
)

$totalDeleted = 0

foreach ($path in $shaderCachePaths) {
    if (Test-Path $path) {
        Write-Host "Found shader cache: $path" -ForegroundColor Cyan
        
        if ($WhatIf) {
            $size = (Get-ChildItem $path -Recurse -File | Measure-Object -Property Length -Sum).Sum / 1MB
            Write-Host "  Size: $([math]::Round($size, 2)) MB" -ForegroundColor Yellow
            Write-Host "  ℹ️  Would delete (WhatIf mode)" -ForegroundColor Yellow
        } else {
            try {
                $size = (Get-ChildItem $path -Recurse -File | Measure-Object -Property Length -Sum).Sum / 1MB
                Remove-Item $path -Recurse -Force
                Write-Host "  ✅ Deleted ($([math]::Round($size, 2)) MB)" -ForegroundColor Green
                $totalDeleted += $size
            } catch {
                Write-Host "  ⚠️  Failed to delete: $_" -ForegroundColor Yellow
            }
        }
    } else {
        Write-Host "⚠️  Not found: $path" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SUMMARY" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

if ($WhatIf) {
    Write-Host "ℹ️  WhatIf mode - no changes made" -ForegroundColor Yellow
} else {
    Write-Host "Total deleted: $([math]::Round($totalDeleted, 2)) MB" -ForegroundColor White
    Write-Host ""
    Write-Host "✅ Shader cache cleared!" -ForegroundColor Green
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Reopen Unity Editor" -ForegroundColor White
Write-Host "2. Wait for shader recompilation (1-2 minutes)" -ForegroundColor White
Write-Host "3. Check Console for shader errors" -ForegroundColor White
Write-Host ""
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
