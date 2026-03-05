# Regenerate Floor Materials Script
# Unity 6 Project - PeuImporte
# Fixes floor texture assignments in materials
#
# Usage: Run this script, then open Unity to let it compile and generate

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Floor Materials Regeneration" -ForegroundColor Cyan
Write-Host "  Unity 6 Project - PeuImporte" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$floorMaterialsFolder = "Assets/Materials/Floor"

# List existing floor materials
Write-Host "[Step 1] Checking existing floor materials..." -ForegroundColor Yellow
$materials = @(
    "Stone_Floor.mat",
    "Wood_Floor.mat",
    "Tile_Floor.mat",
    "Brick_Floor.mat",
    "Marble_Floor.mat"
)

foreach ($mat in $materials) {
    $path = Join-Path $floorMaterialsFolder $mat
    if (Test-Path $path) {
        Write-Host "  ✓ Found: $path" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Missing: $path" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "[Step 2] Floor materials will be regenerated in Unity" -ForegroundColor Yellow
Write-Host ""
Write-Host "INSTRUCTIONS:" -ForegroundColor Cyan
Write-Host "1. Open Unity Editor (if not already open)" -ForegroundColor White
Write-Host "2. Go to: Tools → Floor Materials → Generate All Floor Materials" -ForegroundColor White
Write-Host "3. Wait for generation to complete" -ForegroundColor White
Write-Host "4. Check console for [FloorFactory] messages" -ForegroundColor White
Write-Host ""
Write-Host "The fixed FloorMaterialFactory.cs now:" -ForegroundColor Cyan
Write-Host "  • Uses correct URP shader properties (_BaseMap, _Smoothness)" -ForegroundColor White
Write-Host "  • Sets both _BaseMap and _MainTex for compatibility" -ForegroundColor White
Write-Host "  • Uses PixelCanvas from DrawingManager.cs (Code.Lavos.Core namespace)" -ForegroundColor White
Write-Host ""
Write-Host "After Unity generates the materials, verify:" -ForegroundColor Cyan
Write-Host "  • Stone_Floor.mat → Stone_Floor_Texture.png" -ForegroundColor White
Write-Host "  • Wood_Floor.mat → Wood_Floor_Texture.png" -ForegroundColor White
Write-Host "  • Tile_Floor.mat → Tile_Floor_Texture.png" -ForegroundColor White
Write-Host "  • Brick_Floor.mat → Brick_Floor_Texture.png" -ForegroundColor White
Write-Host "  • Marble_Floor.mat → Marble_Floor_Texture.png" -ForegroundColor White
Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Script Ready - Follow Instructions Above" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
