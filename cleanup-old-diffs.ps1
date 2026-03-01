# cleanup-old-diffs.ps1
# Removes diff files older than 2 days from diff_tmp folder
# Unity 6 compatible - UTF-8 encoding - Unix line endings

param(
    [int]$daysToKeep = 2
)

$diffPath = Join-Path $PSScriptRoot "diff_tmp"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Diff Files Cleanup Utility" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if (Test-Path $diffPath) {
    $cutoffDate = (Get-Date).AddDays(-$daysToKeep)
    $oldFiles = Get-ChildItem -Path $diffPath -Filter "*.diff" -File | 
                Where-Object { $_.LastWriteTime -lt $cutoffDate }
    
    if ($oldFiles.Count -gt 0) {
        Write-Host "Found $($oldFiles.Count) diff file(s) older than $daysToKeep day(s):" -ForegroundColor Yellow
        foreach ($file in $oldFiles) {
            Write-Host "  - $($file.Name) (last modified: $($file.LastWriteTime))" -ForegroundColor Gray
            Remove-Item $file.FullName -Force
            Write-Host "    [DELETED]" -ForegroundColor Red
        }
        Write-Host ""
        Write-Host "Cleanup complete." -ForegroundColor Green
    } else {
        Write-Host "No diff files older than $daysToKeep day(s) found." -ForegroundColor Cyan
        Write-Host "All diff files are recent." -ForegroundColor Green
    }
    
    # Show current diff files
    Write-Host ""
    Write-Host "Current diff files in diff_tmp:" -ForegroundColor Cyan
    $currentFiles = Get-ChildItem -Path $diffPath -Filter "*.diff" -File | Sort-Object LastWriteTime -Descending
    if ($currentFiles.Count -gt 0) {
        foreach ($file in $currentFiles) {
            Write-Host "  + $($file.Name)" -ForegroundColor Gray
        }
    } else {
        Write-Host "  (no diff files)" -ForegroundColor Gray
    }
} else {
    Write-Host "diff_tmp folder not found. Nothing to clean." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
