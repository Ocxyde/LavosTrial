# apply-patches-and-backup.ps1
# Apply all patches and run backup - Unity 6 compatible
# UTF-8 encoding - Unix line endings
#
# Usage: powershell -ExecutionPolicy Bypass -File apply-patches-and-backup.ps1

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Unity 6 Patch & Backup System" -ForegroundColor White
Write-Host "  Version: 6000.3.7f1 (URP Standard)" -ForegroundColor DarkGray
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Clean diff_tmp folder (files older than 2 days)
Write-Host "[Step 1] Cleaning diff_tmp folder..." -ForegroundColor Yellow
$diffTmpPath = Join-Path $scriptDir "diff_tmp"
if (Test-Path $diffTmpPath) {
    $cutoffDate = (Get-Date).AddDays(-2)
    $oldFiles = Get-ChildItem -Path $diffTmpPath -File | Where-Object { $_.LastWriteTime -lt $cutoffDate }
    
    if ($oldFiles.Count -gt 0) {
        foreach ($file in $oldFiles) {
            Remove-Item $file.FullName -Force
            Write-Host "  [Deleted] $($file.Name)" -ForegroundColor DarkGray
        }
        Write-Host "  Cleaned $($oldFiles.Count) old diff file(s)." -ForegroundColor Green
    } else {
        Write-Host "  No old diff files to clean." -ForegroundColor Gray
    }
} else {
    New-Item -ItemType Directory -Path $diffTmpPath | Out-Null
    Write-Host "  Created diff_tmp folder." -ForegroundColor Green
}
Write-Host ""

# Step 2: Run cleanup_deprecated_files.ps1
Write-Host "[Step 2] Cleaning deprecated files..." -ForegroundColor Yellow
$cleanupScript = Join-Path $scriptDir "cleanup_deprecated_files.ps1"
if (Test-Path $cleanupScript) {
    & $cleanupScript
    Write-Host "  Cleanup complete." -ForegroundColor Green
} else {
    Write-Host "  Cleanup script not found, skipping." -ForegroundColor Yellow
}
Write-Host ""

# Step 3: Run backup.ps1
Write-Host "[Step 3] Running backup..." -ForegroundColor Yellow
$backupScript = Join-Path $scriptDir "backup.ps1"
if (Test-Path $backupScript) {
    & $backupScript
    Write-Host "  Backup complete." -ForegroundColor Green
} else {
    Write-Host "  Backup script not found, skipping." -ForegroundColor Yellow
}
Write-Host ""

# Summary
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  All Operations Complete!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor White
Write-Host "  1. Open Unity 6 Editor (6000.3.7f1)" -ForegroundColor Gray
Write-Host "  2. Wait for compilation to complete" -ForegroundColor Gray
Write-Host "  3. Check Console for any errors" -ForegroundColor Gray
Write-Host "  4. Review diff files in: diff_tmp\" -ForegroundColor Gray
Write-Host ""
