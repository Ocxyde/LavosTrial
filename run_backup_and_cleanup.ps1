# run_backup_and_cleanup.ps1
# Run backup and cleanup old diff files
# Unity 6 compatible - UTF-8 encoding - Unix line endings
#
# Usage: .\run_backup_and_cleanup.ps1

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Backup & Cleanup Script" -ForegroundColor Cyan
Write-Host "  $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Run backup
Write-Host "[1/2] Running backup.ps1..." -ForegroundColor Yellow
Write-Host ""

if (Test-Path "$scriptDir\backup.ps1") {
    & "$scriptDir\backup.ps1"
} else {
    Write-Host "  ERROR: backup.ps1 not found!" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 2: Cleanup old diff files
Write-Host "[2/2] Cleaning up diff files older than 2 days..." -ForegroundColor Yellow
Write-Host ""

if (Test-Path "$scriptDir\cleanup-old-diffs.ps1") {
    & "$scriptDir\cleanup-old-diffs.ps1"
} else {
    Write-Host "  WARNING: cleanup-old-diffs.ps1 not found, skipping..." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  Backup & Cleanup Complete!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
