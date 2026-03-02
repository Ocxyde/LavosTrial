# cleanup_diff_tmp.ps1
# Cleanup diff_tmp folder - remove files older than 2 days
# Unity 6 compatible - UTF-8 encoding - Unix line endings
#
# Usage: .\cleanup_diff_tmp.ps1

param(
    [int]$daysToKeep = 2
)

$diffTmpPath = Join-Path $PSScriptRoot "diff_tmp"

# Create folder if it doesn't exist
if (-not (Test-Path $diffTmpPath)) {
    New-Item -ItemType Directory -Path $diffTmpPath -Force | Out-Null
    Write-Host "✅ Created diff_tmp folder"
}

# Remove files older than specified days
$cutoffDate = (Get-Date).AddDays(-$daysToKeep)
$deletedCount = 0

Get-ChildItem -Path $diffTmpPath -File | Where-Object {
    $_.LastWriteTime -lt $cutoffDate
} | ForEach-Object {
    Remove-Item $_.FullName -Force
    Write-Host "🗑️ Deleted: $($_.Name)"
    $deletedCount++
}

Write-Host "============================="
Write-Host "✅ Diff cleanup complete!"
Write-Host "   Files deleted: $deletedCount"
Write-Host "   Files older than $daysToKeep days removed"
Write-Host "============================="
