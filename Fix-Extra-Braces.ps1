# Fix-Extra-Braces.ps1
# Removes extra closing braces added by mistake
#
# Run: .\Fix-Extra-Braces.ps1

$ErrorActionPreference = "Stop"

Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  FIX - Remove Extra Closing Braces" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$filesToFix = @(
    "Assets\Scripts\Core\02_Player\PlayerStats.cs",
    "Assets\Scripts\Core\04_Inventory\Inventory.cs",
    "Assets\Scripts\Core\05_Combat\CombatSystem.cs"
)

foreach ($file in $filesToFix) {
    $fullPath = Join-Path $PSScriptRoot $file
    
    if (-not (Test-Path $fullPath)) {
        Write-Host "⚠️  File not found: $file" -ForegroundColor Yellow
        continue
    }
    
    Write-Host "Fixing: $file" -ForegroundColor Cyan
    
    $content = Get-Content $fullPath -Raw -Encoding UTF8
    $lines = $content -split "`n"
    
    # Count trailing empty lines and closing braces
    $extraBraces = 0
    for ($i = $lines.Count - 1; $i -ge 0; $i--) {
        $line = $lines[$i].Trim()
        if ($line -eq "}" -or [string]::IsNullOrWhiteSpace($line)) {
            $extraBraces++
        } else {
            break
        }
    }
    
    # Remove extra braces (keep only one closing brace)
    if ($extraBraces -gt 1) {
        $removeCount = $extraBraces - 1
        $newLines = $lines[0..($lines.Count - $removeCount - 1)]
        
        # Add single closing brace
        $newLines += "}"
        
        $fixedContent = $newLines -join "`n"
        [System.IO.File]::WriteAllText($fullPath, $fixedContent, [System.Text.UTF8Encoding]::new($true))
        
        Write-Host "  ✅ Removed $removeCount extra brace(s)" -ForegroundColor Green
    } else {
        Write-Host "  ℹ️  No extra braces found" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  ✅ Fix Complete!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next: Open Unity and check for errors" -ForegroundColor Cyan
