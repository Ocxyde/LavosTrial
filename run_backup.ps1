# run_backup.ps1
# Quick backup runner after code changes
# Unity 6 compatible - UTF-8 encoding - Unix line endings

$ErrorActionPreference = "Stop"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  RUNNING BACKUP" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$backupScript = Join-Path $PSScriptRoot "backup.ps1"

if (Test-Path $backupScript) {
    Write-Host "`nExecuting: $backupScript" -ForegroundColor Yellow
    & $backupScript
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "  BACKUP COMPLETED" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
} else {
    Write-Host "`n[ERROR] backup.ps1 not found at: $backupScript" -ForegroundColor Red
    Write-Host "Please run backup manually." -ForegroundColor Yellow
}
