# apply_changes_and_backup.ps1
# Run cleanup/diff generation and then backup
# UTF-8 encoding - Unix line endings
# 
# Usage: Run this script in PowerShell to:
# 1. Clean diff_tmp folder (remove files > 2 days)
# 2. Generate diffs for changed files
# 3. Run backup.ps1

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $PSScriptRoot

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Apply Changes and Backup Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Run cleanup and diff generation
Write-Host "[Step 1/2] Running cleanup and diff generation..." -ForegroundColor Yellow
& (Join-Path $projectRoot "diff_tmp\cleanup_and_diff.ps1")
Write-Host ""

# Step 2: Run backup
Write-Host "[Step 2/2] Running backup.ps1..." -ForegroundColor Yellow
$backupScript = Join-Path $projectRoot "backup.ps1"

if (Test-Path $backupScript) {
    & $backupScript
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nBackup completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "`nBackup failed with exit code $LASTEXITCODE" -ForegroundColor Red
        exit $LASTEXITCODE
    }
} else {
    Write-Host "backup.ps1 not found at: $backupScript" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  All tasks completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
