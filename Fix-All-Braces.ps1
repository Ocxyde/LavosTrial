# Fix-All-Braces.ps1
# Comprehensive script to fix all brace issues in the project
#
# Run: .\Fix-All-Braces.ps1

$ErrorActionPreference = "Stop"

Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  FIX ALL - Brace Issues" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$filesToCheck = @(
    "Assets\Scripts\Core\02_Player\PlayerStats.cs",
    "Assets\Scripts\Core\04_Inventory\Inventory.cs",
    "Assets\Scripts\Core\05_Combat\CombatSystem.cs"
)

$totalFixed = 0

foreach ($file in $filesToCheck) {
    $fullPath = Join-Path $PSScriptRoot $file
    
    if (-not (Test-Path $fullPath)) {
        Write-Host "⚠️  File not found: $file" -ForegroundColor Yellow
        continue
    }
    
    Write-Host "Checking: $file" -ForegroundColor Cyan
    
    $content = Get-Content $fullPath -Raw -Encoding UTF8
    $lines = Get-Content $fullPath -Encoding UTF8
    $originalLineCount = $lines.Count
    $fixed = $false
    
    # Pattern 1: Remove extra } right after class declaration
    # Look for pattern: "    {\n\n        }\n\n        // ───"
    $pattern1 = '(\s*{\s*\r?\n\s*\r?\n\s*}\s*\r?\n\s*\r?\n\s*// ───)'
    if ($content -match $pattern1) {
        $content = $content -replace $pattern1, "    {`n        // ───"
        Write-Host "  ✅ Removed extra closing brace after class declaration" -ForegroundColor Green
        $fixed = $true
        $totalFixed++
    }
    
    # Pattern 2: Remove multiple closing braces at end of file
    $trailingBraces = 0
    for ($i = $lines.Count - 1; $i -ge 0; $i--) {
        $line = $lines[$i].Trim()
        if ($line -eq "}") {
            $trailingBraces++
        } elseif (-not [string]::IsNullOrWhiteSpace($line)) {
            break
        }
    }
    
    if ($trailingBraces -gt 1) {
        # Keep only one closing brace
        $removeCount = $trailingBraces - 1
        $newLines = @()
        $braceCount = 0
        
        for ($i = $lines.Count - 1; $i -ge 0; $i--) {
            $line = $lines[$i].Trim()
            if ($line -eq "}") {
                if ($braceCount -eq 0) {
                    $newLines.Insert(0, $lines[$i])
                    $braceCount++
                }
            } else {
                $newLines.Insert(0, $lines[$i])
            }
        }
        
        $content = $newLines -join "`n"
        Write-Host "  ✅ Removed $removeCount trailing brace(s)" -ForegroundColor Green
        $fixed = $true
        $totalFixed++
    }
    
    # Save if fixed
    if ($fixed) {
        [System.IO.File]::WriteAllText($fullPath, $content, [System.Text.UTF8Encoding]::new($true))
    } else {
        Write-Host "  ℹ️  No issues found" -ForegroundColor Gray
    }
    
    Write-Host ""
}

Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SUMMARY" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Files checked: $($filesToCheck.Count)" -ForegroundColor White
Write-Host "Files fixed: $totalFixed" -ForegroundColor White
Write-Host ""

if ($totalFixed -gt 0) {
    Write-Host "✅ Fixes applied!" -ForegroundColor Green
} else {
    Write-Host "ℹ️  No fixes needed" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Next: Open Unity and check for errors" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
