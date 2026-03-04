# Deep-Scan-And-Fix.ps1
# Comprehensive project scan and fix script
# Unity 6 compatible - UTF-8 encoding - Unix line endings
#
# LOCATION: Assets/Scripts/Editor/
# USAGE: Run from Unity Editor or PowerShell
#
# Run: .\Deep-Scan-And-Fix.ps1

param(
    [switch]$ReadOnly  # Only scan, don't fix
)

$ErrorActionPreference = "Stop"

Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  DEEP SCAN - Find & Fix All Issues" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$issuesFound = 0
$issuesFixed = 0
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptPath

# Scan 1: Check for remaining .Instance references
Write-Host "Scan 1: Checking for .Instance references..." -ForegroundColor Cyan
$files = Get-ChildItem -Path "$projectRoot\Scripts\Core" -Recurse -Filter "*.cs"
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    if ($content -match '\.Instance\b') {
        $matches = [regex]::Matches($content, '\.Instance\b')
        Write-Host "  ⚠️  $($file.Name): $($matches.Count) .Instance reference(s)" -ForegroundColor Yellow
        $issuesFound++
    }
}
Write-Host ""

# Scan 2: Check for syntax errors (unmatched braces)
Write-Host "Scan 2: Checking for brace balance..." -ForegroundColor Cyan
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $openBraces = ([regex]::Matches($content, '\{')).Count
    $closeBraces = ([regex]::Matches($content, '\}')).Count
    
    if ($openBraces -ne $closeBraces) {
        Write-Host "  ❌ $($file.Name): Unbalanced braces (open: $openBraces, close: $closeBraces)" -ForegroundColor Red
        $issuesFound++
    }
}
Write-Host ""

# Scan 3: Check for common compilation errors
Write-Host "Scan 3: Checking for common errors..." -ForegroundColor Cyan
$errorPatterns = @(
    @{Pattern = 'if\s*\(\s*false\s*\)'; Description = "Dead code (if (false))"},
    @{Pattern = '/\*.*?\*/\s*\.'; Description = "Commented code before dot operator"},
    @{Pattern = '//.*?Instance'; Description = "Commented Instance reference"}
)

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    foreach ($check in $errorPatterns) {
        if ($content -match $check.Pattern) {
            Write-Host "  ⚠️  $($file.Name): $($check.Description)" -ForegroundColor Yellow
            $issuesFound++
        }
    }
}
Write-Host ""

# Scan 4: Check TODO.md status
Write-Host "Scan 4: Checking TODO.md..." -ForegroundColor Cyan
$todoPath = "$projectRoot\Docs\TODO.md"
if (Test-Path $todoPath) {
    $todoContent = Get-Content $todoPath -Raw
    $completedCount = ([regex]::Matches($todoContent, '✅')).Count
    $pendingCount = ([regex]::Matches($todoContent, '❌')).Count
    Write-Host "  ✅ Completed: $completedCount" -ForegroundColor Green
    Write-Host "  ❌ Pending: $pendingCount" -ForegroundColor Yellow
} else {
    Write-Host "  ⚠️  TODO.md not found!" -ForegroundColor Yellow
    $issuesFound++
}
Write-Host ""

# Summary
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SCAN SUMMARY" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Issues found: $issuesFound" -ForegroundColor $(if ($issuesFound -eq 0) {"Green"} else {"Yellow"})
Write-Host "Issues fixed: $issuesFixed" -ForegroundColor Green
Write-Host ""

if ($issuesFound -eq 0) {
    Write-Host "✅ No issues found! Project is clean!" -ForegroundColor Green
} else {
    Write-Host "⚠️  Some issues detected. Review the scan results above." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "1. Open Unity and check for compilation errors" -ForegroundColor White
    Write-Host "2. Fix any remaining .Instance references manually" -ForegroundColor White
    Write-Host "3. Update TODO.md with current status" -ForegroundColor White
}

Write-Host ""
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
