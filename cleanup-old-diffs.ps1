# Cleanup Old Diff Files (older than 2 days)
# Unity 6 compatible - UTF-8 encoding - Unix LF
# Automatically deletes diff files older than 2 days

$ErrorActionPreference = "Stop"
$projectRoot = "D:\travaux_Unity\PeuImporte"
$diffFolder = Join-Path $projectRoot "diff_tmp"

Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  CLEANING UP OLD DIFF FILES" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

if (-not (Test-Path $diffFolder)) {
    Write-Host "⚠️  diff_tmp folder does not exist" -ForegroundColor Yellow
    Write-Host "No cleanup needed." -ForegroundColor Gray
    exit 0
}

# Get current date
$now = Get-Date
$cutoffDate = $now.AddDays(-2)

Write-Host "📅 Current date: $($now.ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor Gray
Write-Host "📅 Cutoff date:  $($cutoffDate.ToString('yyyy-MM-dd HH:mm:ss')) (files older than this will be deleted)" -ForegroundColor Gray
Write-Host ""

# Get all diff files
$diffFiles = Get-ChildItem -Path $diffFolder -Filter "*.diff" -File
$totalFiles = $diffFiles.Count
$deletedCount = 0
$keptCount = 0

Write-Host "📊 Found $totalFiles diff file(s)" -ForegroundColor Cyan
Write-Host ""

foreach ($file in $diffFiles) {
    $age = $now - $file.LastWriteTime
    $daysOld = [Math]::Floor($age.TotalDays)
    
    if ($file.LastWriteTime -lt $cutoffDate) {
        # Delete old file
        Write-Host "🗑️  DELETED: $($file.Name)" -ForegroundColor Yellow
        Write-Host "     Age: $daysOld days (older than 2 days)" -ForegroundColor Gray
        Remove-Item -Path $file.FullName -Force
        $deletedCount++
    } else {
        # Keep recent file
        Write-Host "✅ KEPT: $($file.Name)" -ForegroundColor Green
        Write-Host "     Age: $daysOld days (recent)" -ForegroundColor Gray
        $keptCount++
    }
}

Write-Host ""
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  CLEANUP COMPLETE" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "📊 Summary:" -ForegroundColor Cyan
Write-Host "  Total files found: $totalFiles" -ForegroundColor White
Write-Host "  Files deleted:     $deletedCount" -ForegroundColor Yellow
Write-Host "  Files kept:        $keptCount" -ForegroundColor Green
Write-Host ""

if ($deletedCount -gt 0) {
    Write-Host "✅ Freed up disk space by removing old diff files" -ForegroundColor Green
} else {
    Write-Host "ℹ️  No old diff files to delete" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Generate new diffs: .\generate-diff-files.ps1" -ForegroundColor White
Write-Host "  2. Test in Unity" -ForegroundColor White
Write-Host "  3. Commit changes: .\git-commit.ps1" -ForegroundColor White
Write-Host ""
