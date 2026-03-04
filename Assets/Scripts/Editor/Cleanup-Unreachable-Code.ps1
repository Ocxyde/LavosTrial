# Cleanup-Unreachable-Code.ps1
# Removes unreachable code warnings (if (false) blocks)
# Unity 6 compatible - UTF-8 encoding - Unix line endings
#
# LOCATION: Assets/Scripts/Editor/
#
# Run: .\Cleanup-Unreachable-Code.ps1

param(
    [switch]$WhatIf  # Show what would be changed
)

$ErrorActionPreference = "Stop"

Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  CLEANUP - Unreachable Code" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$filesToClean = @(
    "Assets\Scripts\Core\04_Inventory\Inventory.cs",
    "Assets\Scripts\Core\02_Player\PlayerStats.cs",
    "Assets\Scripts\Core\05_Combat\CombatSystem.cs",
    "Assets\Scripts\Core\10_Resources\SeedManager.cs"
)

$changesMade = 0

foreach ($file in $filesToClean) {
    $fullPath = Join-Path $PSScriptRoot "..\..\$file"
    
    if (-not (Test-Path $fullPath)) {
        # Try alternate path
        $fullPath = Join-Path $PSScriptRoot "..\..\..\$file"
    }
    
    if (-not (Test-Path $fullPath)) {
        Write-Host "⚠️  File not found: $file" -ForegroundColor Yellow
        continue
    }
    
    Write-Host "Checking: $file" -ForegroundColor Cyan
    
    $content = Get-Content $fullPath -Raw -Encoding UTF8
    $originalContent = $content
    
    # Remove if (false) blocks
    $content = $content -replace '(?s)if\s*\(\s*false\s*\)\s*\{[^}]*\}', '// [REMOVED] Unreachable code'
    $content = $content -replace '(?s)if\s*\(\s*false\s*\)\s*\r?\n\s*[^\r\n]+', '// [REMOVED] Unreachable code'
    
    if ($content -ne $originalContent) {
        $changesMade++
        
        if (-not $WhatIf) {
            [System.IO.File]::WriteAllText($fullPath, $content, [System.Text.UTF8Encoding]::new($true))
            Write-Host "  ✅ Cleaned unreachable code" -ForegroundColor Green
        } else {
            Write-Host "  ℹ️  Would clean unreachable code (WhatIf mode)" -ForegroundColor Yellow
        }
    } else {
        Write-Host "  ℹ️  No unreachable code found" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SUMMARY" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Files checked: $($filesToClean.Count)" -ForegroundColor White
Write-Host "Files cleaned: $changesMade" -ForegroundColor White
Write-Host ""

if ($WhatIf) {
    Write-Host "ℹ️  WhatIf mode - no changes made" -ForegroundColor Yellow
} else {
    Write-Host "✅ Cleanup complete!" -ForegroundColor Green
}

Write-Host ""
Write-Host "Next: Open Unity and verify 0 errors" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
