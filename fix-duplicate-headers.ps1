# fix-duplicate-headers.ps1
# Remove duplicate file headers from C# files
# UTF-8 encoding - Unix line endings
#
# Usage: powershell -ExecutionPolicy Bypass -File fix-duplicate-headers.ps1

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Fix Duplicate Headers" -ForegroundColor White
Write-Host "  Removing duplicate file headers" -ForegroundColor DarkGray
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$csFiles = Get-ChildItem -Path "Assets\Scripts" -Filter "*.cs" -Recurse
$fixedCount = 0

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    
    # Check for duplicate header pattern (header appearing twice)
    if ($content -match "^(// .*?\.cs\r?\n// .*?\r?\n// .*?\r?\n// .*?\r?\n.*?\r?\n)\r?\n// .*?\.cs\r?\n// .*?\r?\n// .*?\r?\n// .*?\r?\n.*?\r?\n") {
        # Remove duplicate header - keep only first occurrence
        $newContent = $content -replace "^(// .*?\.cs\r?\n// .*?\r?\n// .*?\r?\n// .*?\r?\n.*?\r?\n)\r?\n// .*?\.cs\r?\n// .*?\r?\n// .*?\r?\n// .*?\r?\n.*?\r?\n", '$1'
        
        # Normalize line endings to Unix LF
        $newContent = $newContent -replace "`r`n", "`n"
        
        # Write back
        [System.IO.File]::WriteAllText($file.FullName, $newContent, [System.Text.UTF8Encoding]::new($false))
        
        Write-Host "  [FIXED] $($file.Name)" -ForegroundColor Green
        $fixedCount++
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Fix Complete" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Files fixed: $fixedCount" -ForegroundColor Green
Write-Host ""
Write-Host "Next: Run backup" -ForegroundColor Yellow
Write-Host "  powershell -ExecutionPolicy Bypass -File apply-patches-and-backup.ps1" -ForegroundColor Gray
Write-Host ""
