# delete_old_pixel_art.ps1
# Delete old PixelArtTextureFactory.cs from Scripts/ folder
# Unity 6 compatible - UTF-8 encoding - Unix line endings

$ErrorActionPreference = "Stop"

$oldFile = "Assets\Scripts\PixelArtTextureFactory.cs"
$oldMeta = "Assets\Scripts\PixelArtTextureFactory.cs.meta"

Write-Host "[Cleanup] Removing old PixelArtTextureFactory.cs location..." -ForegroundColor Cyan

if (Test-Path $oldFile) {
    Remove-Item $oldFile -Force
    Write-Host "  [OK] Removed: $oldFile" -ForegroundColor Green
} else {
    Write-Host "  [INFO] File not found: $oldFile" -ForegroundColor Yellow
}

if (Test-Path $oldMeta) {
    Remove-Item $oldMeta -Force
    Write-Host "  [OK] Removed: $oldMeta" -ForegroundColor Green
} else {
    Write-Host "  [INFO] Meta file not found: $oldMeta" -ForegroundColor Yellow
}

Write-Host "`n[Cleanup] Done!" -ForegroundColor Cyan
