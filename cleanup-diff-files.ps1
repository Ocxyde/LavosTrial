# ============================================================
#  cleanup-diff-files.ps1 - Clean diff_tmp folder
#  Deletes all diff files older than 2 days
# ============================================================

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

$diffFolder = Join-Path $scriptDir "diff_tmp"
$daysToKeep = 2

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Clean diff_tmp Folder" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Ensure diff_tmp folder exists
if (!(Test-Path $diffFolder)) {
    Write-Host "  Creating diff_tmp folder..." -ForegroundColor DarkGray
    New-Item -ItemType Directory -Path $diffFolder -Force | Out-Null
}

Write-Host "  Scanning for files older than $daysToKeep days..." -ForegroundColor DarkGray
Write-Host "  Folder: $diffFolder" -ForegroundColor Gray
Write-Host ""

# Get cutoff date
$cutoffDate = (Get-Date).AddDays(-$daysToKeep)

# Get all files older than cutoff
$oldFiles = Get-ChildItem -Path $diffFolder -File | Where-Object {
    $_.LastWriteTime -lt $cutoffDate
}

$fileCount = $oldFiles.Count

if ($fileCount -eq 0) {
    Write-Host "  No files older than $daysToKeep days found." -ForegroundColor Green
} else {
    Write-Host "  Found $fileCount file(s) to delete:" -ForegroundColor Yellow
    
    foreach ($file in $oldFiles) {
        Write-Host "    - $($file.Name) (modified: $($file.LastWriteTime.ToString('yyyy-MM-dd')))" -ForegroundColor DarkGray
        Remove-Item -Path $file.FullName -Force
    }
    
    Write-Host ""
    Write-Host "  Deleted $fileCount file(s)." -ForegroundColor Green
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Cleanup Complete!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Show remaining files
$remainingFiles = Get-ChildItem -Path $diffFolder -File
if ($remainingFiles.Count -gt 0) {
    Write-Host "  Remaining files in diff_tmp:" -ForegroundColor DarkGray
    foreach ($file in $remainingFiles) {
        Write-Host "    - $($file.Name)" -ForegroundColor Gray
    }
} else {
    Write-Host "  diff_tmp folder is now empty." -ForegroundColor DarkGray
}

Write-Host ""
