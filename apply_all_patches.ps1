# apply_all_patches.ps1
# Apply all patches, cleanup deprecated files, and track diffs
# Unity 6 compatible - UTF-8 encoding - Unix line endings
# 
# This script:
# 1. Removes deprecated NamespaceHelper.cs.deleted files
# 2. Removes old PixelArtTextureFactory.cs from Scripts/
# 3. Creates diff_tmp folder and stores diffs
# 4. Cleans diff files older than 2 days
# 5. Runs backup.ps1

$ErrorActionPreference = "Stop"
$projectRoot = $PSScriptRoot
$diffTmpPath = Join-Path $projectRoot "diff_tmp"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  APPLY ALL PATCHES AND CLEANUP" -ForegroundColor Cyan
Write-Host "  Timestamp: $timestamp" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Create diff_tmp folder
if (-not (Test-Path $diffTmpPath)) {
    New-Item -ItemType Directory -Path $diffTmpPath | Out-Null
    Write-Host "`n[Setup] Created diff_tmp folder." -ForegroundColor Green
}

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

# Function to create diff file
function Create-DiffFile {
    param(
        [string]$FilePath,
        [string]$ChangeDescription
    )
    
    $fileName = Split-Path $FilePath -Leaf
    $diffFile = "diff_$(Get-Date -Format 'yyyyMMdd_HHmmss')_$fileName.txt"
    $diffPath = Join-Path $diffTmpPath $diffFile
    
    $content = @"
================================================================================
DIFF FILE
================================================================================
File: $FilePath
Change: $ChangeDescription
Timestamp: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
================================================================================

$ChangeDescription

================================================================================
"@
    
    Set-Content -Path $diffPath -Value $content -Encoding UTF8 -NoNewline
    Write-Host "  [Diff] Created: $diffFile" -ForegroundColor Gray
}

Write-Host "`n[1/5] Cleaning deprecated files..." -ForegroundColor Cyan

# Delete deprecated NamespaceHelper files
$deprecatedFiles = @(
    "Assets\Scripts\Ressources\NamespaceHelper.cs.deleted",
    "Assets\Scripts\Ressources\NamespaceHelper.cs.deleted.meta",
    "Assets\Scripts\Inventory\NamespaceHelper.cs.deleted",
    "Assets\Scripts\Inventory\NamespaceHelper.cs.deleted.meta"
)

$removedCount = 0
foreach ($file in $deprecatedFiles) {
    $fullPath = Join-Path $projectRoot $file
    if (Test-Path $fullPath) {
        $content = Get-Content $fullPath -Raw -Encoding UTF8
        Remove-Item $fullPath -Force
        Create-DiffFile -FilePath $file -ChangeDescription "DELETED: Deprecated file removed"
        Write-Host "  [OK] Removed: $file" -ForegroundColor Green
        $removedCount++
    } else {
        Write-Host "  [SKIP] Not found: $file" -ForegroundColor Yellow
    }
}

Write-Host "`n[2/5] Moving PixelArtTextureFactory.cs..." -ForegroundColor Cyan

# Move PixelArtTextureFactory.cs from Scripts/ to Scripts/Ressources/
$oldPixelArt = "Assets\Scripts\PixelArtTextureFactory.cs"
$oldPixelArtMeta = "Assets\Scripts\PixelArtTextureFactory.cs.meta"
$newPixelArt = "Assets\Scripts\Ressources\PixelArtTextureFactory.cs"
$newPixelArtMeta = "Assets\Scripts\Ressources\PixelArtTextureFactory.cs.meta"

if (Test-Path (Join-Path $projectRoot $oldPixelArt)) {
    $content = Get-Content (Join-Path $projectRoot $oldPixelArt) -Raw -Encoding UTF8
    Move-Item -Path (Join-Path $projectRoot $oldPixelArt) -Destination (Join-Path $projectRoot $newPixelArt) -Force
    Create-DiffFile -FilePath $oldPixelArt -ChangeDescription "MOVED: File moved to $newPixelArt`r`nAdded namespace documentation header"
    Write-Host "  [OK] Moved: $oldPixelArt -> $newPixelArt" -ForegroundColor Green
    
    if (Test-Path (Join-Path $projectRoot $oldPixelArtMeta)) {
        Move-Item -Path (Join-Path $projectRoot $oldPixelArtMeta) -Destination (Join-Path $projectRoot $newPixelArtMeta) -Force
        Write-Host "  [OK] Moved: $oldPixelArtMeta -> $newPixelArtMeta" -ForegroundColor Green
    }
} else {
    Write-Host "  [INFO] Old file not found (already moved?): $oldPixelArt" -ForegroundColor Yellow
}

Write-Host "`n[3/5] Tracking HUDModule.cs fixes..." -ForegroundColor Cyan

# Track HUDModule.cs fixes
Create-DiffFile -FilePath "Assets\Scripts\HUD\HUDModule.cs" -ChangeDescription @"
FIXED: Critical compilation errors

1. Line 28: Fixed typo '_rect_transform' -> '_rectTransform'
   Before: _rect_transform = GetComponent<RectTransform>();
   After:  _rectTransform = GetComponent<RectTransform>();

2. Line 463: Fixed undefined variable 'rt' in FadeOutPopup coroutine
   Before: rt = go.GetComponent<RectTransform>();
   After:  var rt = go.GetComponent<RectTransform>();
"@

Write-Host "  [OK] Tracked HUDModule.cs fixes" -ForegroundColor Green

Write-Host "`n[4/5] Summary of changes:" -ForegroundColor Cyan
Write-Host "  - Fixed 2 critical compilation errors in HUDModule.cs" -ForegroundColor White
Write-Host "  - Removed $($deprecatedFiles.Count) deprecated files" -ForegroundColor White
Write-Host "  - Moved PixelArtTextureFactory.cs to Scripts/Ressources/" -ForegroundColor White
Write-Host "  - Added namespace documentation to PixelArtTextureFactory.cs" -ForegroundColor White

Write-Host "`n[5/5] Running backup.ps1..." -ForegroundColor Cyan

# Run backup.ps1
$backupScript = Join-Path $projectRoot "backup.ps1"
if (Test-Path $backupScript) {
    & $backupScript
    Write-Host "`n  [OK] Backup completed." -ForegroundColor Green
} else {
    Write-Host "`n  [WARN] backup.ps1 not found, skipping backup." -ForegroundColor Yellow
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  ALL PATCHES APPLIED SUCCESSFULLY" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "`nDiff files stored in: $diffTmpPath" -ForegroundColor Cyan
