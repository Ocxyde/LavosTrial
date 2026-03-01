# fix-all-syntax.ps1
# Fix all syntax errors - remove leading blank lines and duplicate headers
# UTF-8 encoding - Unix line endings
#
# Usage: powershell -ExecutionPolicy Bypass -File fix-all-syntax.ps1

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Fix All Syntax Errors" -ForegroundColor White
Write-Host "  Removing leading blank lines and duplicate headers" -ForegroundColor DarkGray
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$csFiles = Get-ChildItem -Path "Assets\Scripts" -Filter "*.cs" -Recurse
$fixedCount = 0

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $originalContent = $content
    
    # Remove leading blank lines (keep content starting from first non-empty line)
    $content = $content -replace "^\r?\n+", ''
    
    # Remove duplicate headers (header block appearing twice)
    $content = $content -replace "^(// .*?\.cs\r?\n// .*?\r?\n// .*?\r?\n// .*?\r?\n.*?\r?\n)\r?\n// .*?\.cs\r?\n// .*?\r?\n// .*?\r?\n// .*?\r?\n.*?\r?\n", '$1'
    
    # Normalize line endings to Unix LF
    $content = $content -replace "`r`n", "`n"
    
    # Only write if changed
    if ($content -ne $originalContent) {
        [System.IO.File]::WriteAllText($file.FullName, $content, [System.Text.UTF8Encoding]::new($false))
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
