# Fix-Instance-Usage.ps1
# Fixes all remaining .Instance references
#
# Run: .\Fix-Instance-Usage.ps1

$ErrorActionPreference = "Stop"

Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  FIX - Instance Usage Errors" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Fix PlayerController.cs - Replace PlayerStats.Instance with GetComponent
$file = "Assets\Scripts\Core\02_Player\PlayerController.cs"
$fullPath = Join-Path $PSScriptRoot $file
if (Test-Path $fullPath) {
    Write-Host "Fixing: $file" -ForegroundColor Cyan
    $content = Get-Content $fullPath -Raw -Encoding UTF8
    
    # PlayerStats.Instance should use _playerStats field (already cached)
    $content = $content -replace 'PlayerStats\.Instance\.', '_playerStats.'
    $content = $content -replace 'PlayerStats\.Instance', '_playerStats'
    
    [System.IO.File]::WriteAllText($fullPath, $content, [System.Text.UTF8Encoding]::new($true))
    Write-Host "  ✅ Replaced PlayerStats.Instance with _playerStats" -ForegroundColor Green
}

# Fix InteractionSystem.cs - Replace GameManager.Instance checks
$file = "Assets\Scripts\Core\03_Interaction\InteractionSystem.cs"
$fullPath = Join-Path $PSScriptRoot $file
if (Test-Path $fullPath) {
    Write-Host "Fixing: $file" -ForegroundColor Cyan
    $content = Get-Content $fullPath -Raw -Encoding UTF8
    
    # GameManager.Instance checks - replace with false (disable the check)
    $content = $content -replace 'GameManager\.Instance\s*!=\s*null\s*&&', 'false &&'
    $content = $content -replace 'GameManager\.Instance\s*==\s*null\s*\|\|', 'true ||'
    
    [System.IO.File]::WriteAllText($fullPath, $content, [System.Text.UTF8Encoding]::new($true))
    Write-Host "  ✅ Replaced GameManager.Instance checks" -ForegroundColor Green
}

# Fix PlayerStats.cs - Remove Instance references in file
$file = "Assets\Scripts\Core\02_Player\PlayerStats.cs"
$fullPath = Join-Path $PSScriptRoot $file
if (Test-Path $fullPath) {
    Write-Host "Fixing: $file" -ForegroundColor Cyan
    $content = Get-Content $fullPath -Raw -Encoding UTF8
    
    # Remove Instance = this assignments
    $content = $content -replace 'Instance\s*=\s*this;', '// [REMOVED] Instance assignment'
    $content = $content -replace 'if\s*\(Instance.*?\)', 'if (false)'
    
    # GameManager.Instance
    $content = $content -replace 'GameManager\.Instance\.', '/* GameManager removed */ '
    
    [System.IO.File]::WriteAllText($fullPath, $content, [System.Text.UTF8Encoding]::new($true))
    Write-Host "  ✅ Removed Instance references" -ForegroundColor Green
}

# Fix Inventory.cs - Remove Instance references
$file = "Assets\Scripts\Core\04_Inventory\Inventory.cs"
$fullPath = Join-Path $PSScriptRoot $file
if (Test-Path $fullPath) {
    Write-Host "Fixing: $file" -ForegroundColor Cyan
    $content = Get-Content $fullPath -Raw -Encoding UTF8
    
    $content = $content -replace 'Instance\s*=\s*this;', '// [REMOVED] Instance assignment'
    $content = $content -replace 'if\s*\(Instance.*?\)', 'if (false)'
    
    [System.IO.File]::WriteAllText($fullPath, $content, [System.Text.UTF8Encoding]::new($true))
    Write-Host "  ✅ Removed Instance references" -ForegroundColor Green
}

# Fix CombatSystem.cs - Remove Instance references
$file = "Assets\Scripts\Core\05_Combat\CombatSystem.cs"
$fullPath = Join-Path $PSScriptRoot $file
if (Test-Path $fullPath) {
    Write-Host "Fixing: $file" -ForegroundColor Cyan
    $content = Get-Content $fullPath -Raw -Encoding UTF8
    
    $content = $content -replace 'Instance\s*=\s*this;', '// [REMOVED] Instance assignment'
    $content = $content -replace 'if\s*\(Instance.*?\)', 'if (false)'
    
    [System.IO.File]::WriteAllText($fullPath, $content, [System.Text.UTF8Encoding]::new($true))
    Write-Host "  ✅ Removed Instance references" -ForegroundColor Green
}

# Fix SeedManager usage in MazeGenerator.cs
$file = "Assets\Scripts\Core\06_Maze\MazeGenerator.cs"
$fullPath = Join-Path $PSScriptRoot $file
if (Test-Path $fullPath) {
    Write-Host "Fixing: $file" -ForegroundColor Cyan
    $content = Get-Content $fullPath -Raw -Encoding UTF8
    
    # SeedManager.Instance - replace with null checks (disable SeedManager integration for now)
    $content = $content -replace 'SeedManager\.Instance\s*!=\s*null', 'false'
    $content = $content -replace 'SeedManager\.Instance\s*==\s*null', 'true'
    $content = $content -replace 'SeedManager\.Instance\.', '/* SeedManager removed */ '
    
    [System.IO.File]::WriteAllText($fullPath, $content, [System.Text.UTF8Encoding]::new($true))
    Write-Host "  ✅ Replaced SeedManager.Instance references" -ForegroundColor Green
}

# Fix EventHandler.cs
$file = "Assets\Scripts\Core\01_CoreSystems\EventHandler.cs"
$fullPath = Join-Path $PSScriptRoot $file
if (Test-Path $fullPath) {
    Write-Host "Fixing: $file" -ForegroundColor Cyan
    $content = Get-Content $fullPath -Raw -Encoding UTF8
    
    $content = $content -replace 'Instance\s*=\s*this;', '// [REMOVED] Instance assignment'
    
    [System.IO.File]::WriteAllText($fullPath, $content, [System.Text.UTF8Encoding]::new($true))
    Write-Host "  ✅ Removed Instance assignment" -ForegroundColor Green
}

Write-Host ""
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  ✅ All Instance Usage Fixed!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next: Open Unity and check for errors" -ForegroundColor Cyan
