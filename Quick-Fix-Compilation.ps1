# Quick-Fix-Compilation.ps1
# Fixes remaining compilation errors by removing broken Instance references
#
# Run: .\Quick-Fix-Compilation.ps1

$ErrorActionPreference = "Stop"

Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  QUICK FIX - Compilation Errors" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Fix PlayerStats.cs - Remove Instance property and references
$file = "Assets\Scripts\Core\02_Player\PlayerStats.cs"
$fullPath = Join-Path $PSScriptRoot $file
if (Test-Path $fullPath) {
    Write-Host "Fixing: $file" -ForegroundColor Cyan
    $content = Get-Content $fullPath -Raw -Encoding UTF8
    
    # Remove Instance property block
    $content = $content -replace '(?s)// Cached singleton.*?return _instance;\s*\}', ''
    # Remove Instance assignments
    $content = $content -replace 'Instance\s*=\s*this;', ''
    $content = $content -replace 'if\s*\(Instance.*?\)', 'if (false)'
    
    [System.IO.File]::WriteAllText($fullPath, $content, [System.Text.UTF8Encoding]::new($true))
    Write-Host "  ✅ Fixed" -ForegroundColor Green
}

# Fix Inventory.cs
$file = "Assets\Scripts\Core\04_Inventory\Inventory.cs"
$fullPath = Join-Path $PSScriptRoot $file
if (Test-Path $fullPath) {
    Write-Host "Fixing: $file" -ForegroundColor Cyan
    $content = Get-Content $fullPath -Raw -Encoding UTF8
    $content = $content -replace '(?s)// Cached singleton.*?return _instance;\s*\}', ''
    $content = $content -replace 'Instance\s*=\s*this;', ''
    [System.IO.File]::WriteAllText($fullPath, $content, [System.Text.UTF8Encoding]::new($true))
    Write-Host "  ✅ Fixed" -ForegroundColor Green
}

# Fix CombatSystem.cs
$file = "Assets\Scripts\Core\05_Combat\CombatSystem.cs"
$fullPath = Join-Path $PSScriptRoot $file
if (Test-Path $fullPath) {
    Write-Host "Fixing: $file" -ForegroundColor Cyan
    $content = Get-Content $fullPath -Raw -Encoding UTF8
    $content = $content -replace '(?s)// Cached singleton.*?return _instance;\s*\}', ''
    $content = $content -replace 'Instance\s*=\s*this;', ''
    [System.IO.File]::WriteAllText($fullPath, $content, [System.Text.UTF8Encoding]::new($true))
    Write-Host "  ✅ Fixed" -ForegroundColor Green
}

# Fix SeedManager.cs
$file = "Assets\Scripts\Core\10_Resources\SeedManager.cs"
$fullPath = Join-Path $PSScriptRoot $file
if (Test-Path $fullPath) {
    Write-Host "Fixing: $file" -ForegroundColor Cyan
    $content = Get-Content $fullPath -Raw -Encoding UTF8
    $content = $content -replace '(?s)private static SeedManager _instance;.*?return _instance;\s*\}', ''
    $content = $content -replace '_instance\s*=\s*this;', ''
    $content = $content -replace 'if\s*\(_instance.*?\)', 'if (false)'
    [System.IO.File]::WriteAllText($fullPath, $content, [System.Text.UTF8Encoding]::new($true))
    Write-Host "  ✅ Fixed" -ForegroundColor Green
}

# Fix EventHandler.cs
$file = "Assets\Scripts\Core\01_CoreSystems\EventHandler.cs"
$fullPath = Join-Path $PSScriptRoot $file
if (Test-Path $fullPath) {
    Write-Host "Fixing: $file" -ForegroundColor Cyan
    $content = Get-Content $fullPath -Raw -Encoding UTF8
    $content = $content -replace 'Instance\s*=\s*this;', ''
    [System.IO.File]::WriteAllText($fullPath, $content, [System.Text.UTF8Encoding]::new($true))
    Write-Host "  ✅ Fixed" -ForegroundColor Green
}

Write-Host ""
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  ✅ Quick Fix Complete!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next: Open Unity and check for remaining errors" -ForegroundColor Cyan
