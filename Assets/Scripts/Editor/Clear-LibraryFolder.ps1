# Clear-LibraryFolder.ps1
# Deletes Unity Library folder to force full reimport
# Unity 6 compatible - UTF-8 encoding - Unix line endings
#
# ⚠️  WARNING: This will delete the Library folder!
# Unity will need to reimport all assets (2-5 minutes)
#
# USAGE:
#   1. Close Unity Editor (REQUIRED!)
#   2. Run this script
#   3. Reopen Unity Editor
#   4. Wait for full reimport
#
# Location: Assets/Scripts/Editor/

param(
    [switch]$WhatIf,      # Show what would be deleted without making changes
    [switch]$Force        # Skip confirmation prompt
)

$ErrorActionPreference = "Stop"

Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  CLEAR LIBRARY FOLDER" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Get project root (parent of Assets folder)
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent (Split-Path -Parent $scriptPath)

$libraryPath = "$projectRoot\Library"

Write-Host "Project Root: $projectRoot" -ForegroundColor Gray
Write-Host "Library Path: $libraryPath" -ForegroundColor Gray
Write-Host ""

# Check if Unity is running
$unityProcesses = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
if ($unityProcesses.Count -gt 0) {
    Write-Host "⚠️  WARNING: Unity is still running!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please close Unity before running this script." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Press any key to exit..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

# Check if Library folder exists
if (-not (Test-Path $libraryPath)) {
    Write-Host "ℹ️  Library folder not found - nothing to clear" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Press any key to exit..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 0
}

# Calculate size
Write-Host "Calculating Library folder size..." -ForegroundColor Cyan
$size = (Get-ChildItem $libraryPath -Recurse -File | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "Library folder size: $([math]::Round($size, 2)) MB" -ForegroundColor White
Write-Host ""

# Confirmation
if (-not $Force -and -not $WhatIf) {
    Write-Host "⚠️  WARNING: This will delete the entire Library folder!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Unity will need to reimport all assets." -ForegroundColor Yellow
    Write-Host "This can take 2-5 minutes depending on project size." -ForegroundColor Yellow
    Write-Host ""
    $response = Read-Host "Are you sure you want to continue? (y/n)"
    if ($response -ne 'y' -and $response -ne 'Y') {
        Write-Host "Operation cancelled." -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Press any key to exit..." -ForegroundColor Gray
        $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
        exit 0
    }
}

# Delete Library folder
if ($WhatIf) {
    Write-Host "ℹ️  WhatIf mode - would delete Library folder" -ForegroundColor Yellow
    Write-Host "Size: $([math]::Round($size, 2)) MB" -ForegroundColor Yellow
} else {
    try {
        Write-Host "Deleting Library folder..." -ForegroundColor Cyan
        Remove-Item $libraryPath -Recurse -Force
        Write-Host "✅ Library folder deleted successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Deleted: $([math]::Round($size, 2)) MB" -ForegroundColor White
    } catch {
        Write-Host "⚠️  Failed to delete Library folder: $_" -ForegroundColor Red
        Write-Host ""
        Write-Host "Please close Unity and try again." -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Press any key to exit..." -ForegroundColor Gray
        $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
        exit 1
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
    Write-Host "✅ Library folder cleared!" -ForegroundColor Green
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Reopen Unity Editor" -ForegroundColor White
Write-Host "2. Wait for full asset reimport (2-5 minutes)" -ForegroundColor White
Write-Host "3. Check Console for any errors" -ForegroundColor White
Write-Host "4. Test your game" -ForegroundColor White
Write-Host ""
Write-Host "Note: First launch after clearing Library will be slower" -ForegroundColor Yellow
Write-Host "      as Unity rebuilds all caches and reimports assets." -ForegroundColor Yellow
Write-Host ""
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
