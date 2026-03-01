// move_pixel_art_factory.ps1
# Move PixelArtTextureFactory.cs to Scripts/Ressources/ and add namespace
# Unity 6 compatible - UTF-8 encoding - Unix line endings

$ErrorActionPreference = "Stop"

$sourceFile = "Assets\Scripts\PixelArtTextureFactory.cs"
$sourceMeta = "Assets\Scripts\PixelArtTextureFactory.cs.meta"
$destFolder = "Assets\Scripts\Ressources"
$destFile = "Assets\Scripts\Ressources\PixelArtTextureFactory.cs"
$destMeta = "Assets\Scripts\Ressources\PixelArtTextureFactory.cs.meta"

Write-Host "[Move] Moving PixelArtTextureFactory.cs to Scripts/Ressources/..." -ForegroundColor Cyan

# Move .cs file
if (Test-Path $sourceFile) {
    Move-Item -Path $sourceFile -Destination $destFile -Force
    Write-Host "  [OK] Moved: $sourceFile -> $destFile" -ForegroundColor Green
} else {
    Write-Host "  [ERROR] Source file not found: $sourceFile" -ForegroundColor Red
    exit 1
}

# Move .meta file if exists
if (Test-Path $sourceMeta) {
    Move-Item -Path $sourceMeta -Destination $destMeta -Force
    Write-Host "  [OK] Moved: $sourceMeta -> $destMeta" -ForegroundColor Green
} else {
    Write-Host "  [INFO] No .meta file found to move" -ForegroundColor Yellow
}

Write-Host "`n[Move] Done!" -ForegroundColor Cyan
