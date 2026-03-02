# cleanup-old-diffs.ps1
# Remove diff files older than 2 days from diff_tmp folder
# Unity 6 compatible - UTF-8 encoding - Unix line endings

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

$diffFolder = Join-Path $scriptDir "diff_tmp"
$cutoffDate = (Get-Date).AddDays(-2)

if (!(Test-Path $diffFolder)) {
    Write-Host "diff_tmp folder not found. Nothing to clean." -ForegroundColor Yellow
    exit
}

$deletedCount = 0

Get-ChildItem -Path $diffFolder -File | Where-Object { $_.LastWriteTime -lt $cutoffDate } | ForEach-Object {
    Remove-Item $_.FullName -Force
    Write-Host "Deleted: $($_.Name)" -ForegroundColor DarkGray
    $deletedCount++
}

Write-Host ""
Write-Host "============================================" -ForegroundColor DarkCyan
Write-Host "  Diff Cleanup - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor DarkCyan
Write-Host "  Files deleted: $deletedCount" -ForegroundColor White
Write-Host "============================================" -ForegroundColor DarkCyan
Write-Host ""
