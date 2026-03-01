# cleanup_deprecated_files.ps1
# Remove deprecated files and setup diff_tmp folder
# Unity 6 compatible - UTF-8 encoding - Unix line endings

$ErrorActionPreference = "Stop"

Write-Host "[Cleanup] Removing deprecated files..." -ForegroundColor Cyan

# Delete deprecated NamespaceHelper files
$deprecatedFiles = @(
    "Assets\Scripts\Ressources\NamespaceHelper.cs.deleted",
    "Assets\Scripts\Ressources\NamespaceHelper.cs.deleted.meta",
    "Assets\Scripts\Inventory\NamespaceHelper.cs.deleted",
    "Assets\Scripts\Inventory\NamespaceHelper.cs.deleted.meta"
)

$removedCount = 0
foreach ($file in $deprecatedFiles) {
    $fullPath = Join-Path $PSScriptRoot $file
    if (Test-Path $fullPath) {
        Remove-Item $fullPath -Force
        Write-Host "  [OK] Removed: $file" -ForegroundColor Green
        $removedCount++
    } else {
        Write-Host "  [SKIP] Not found: $file" -ForegroundColor Yellow
    }
}

Write-Host "`n[Cleanup] Removed $removedCount deprecated files." -ForegroundColor Cyan

# Create diff_tmp folder if not exists
$diffTmpPath = Join-Path $PSScriptRoot "diff_tmp"
if (-not (Test-Path $diffTmpPath)) {
    New-Item -ItemType Directory -Path $diffTmpPath | Out-Null
    Write-Host "`n[Setup] Created diff_tmp folder." -ForegroundColor Green
} else {
    Write-Host "`n[Setup] diff_tmp folder already exists." -ForegroundColor Yellow
    
    # Clean old diff files (older than 2 days)
    $cutoffDate = (Get-Date).AddDays(-2)
    $oldFiles = Get-ChildItem -Path $diffTmpPath -File | Where-Object { $_.LastWriteTime -lt $cutoffDate }
    
    if ($oldFiles.Count -gt 0) {
        foreach ($file in $oldFiles) {
            Remove-Item $file.FullName -Force
            Write-Host "  [Cleaned] Old diff file: $($file.Name)" -ForegroundColor Yellow
        }
        Write-Host "  [OK] Cleaned $($oldFiles.Count) old diff files." -ForegroundColor Green
    }
}

Write-Host "`n[Cleanup] Done!" -ForegroundColor Cyan
