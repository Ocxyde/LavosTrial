# fix_floor_materials_direct.ps1
# Directly fixes floor materials to reference their texture files by updating YAML
# Run this script from project root: .\fix_floor_materials_direct.ps1
# After running, remember to run: .\backup.ps1

$ErrorActionPreference = "Stop"
$projectRoot = $PSScriptRoot

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Floor Materials Texture Fix (Direct)" -ForegroundColor Cyan
Write-Host "  Unity 6 Project - PeuImporte" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Floor types with their texture GUIDs
$floorData = @(
    @{ Type = "Stone"; TextureGuid = "8191a99fe3f579241b4dbbbbe7689e1f" },
    @{ Type = "Wood"; TextureGuid = "b8f8e5a0c5d4e3f2a1b0c9d8e7f6a5b4" },
    @{ Type = "Tile"; TextureGuid = "c9f9e6a1d6e5f4a3b2c1d0e9f8a7b6c5" },
    @{ Type = "Brick"; TextureGuid = "d0a0f7b2e7f6a5b4c3d2e1f0a9b8c7d6" },
    @{ Type = "Marble"; TextureGuid = "e1b1a8c3f8a7b6c5d4e3f2a1b0c9d8e7" }
)

$materialsFolder = Join-Path $projectRoot "Assets\Materials\Floor"
$changesMade = 0

# First, get actual GUIDs from texture meta files
Write-Host "[Step 1] Reading texture GUIDs from meta files..." -ForegroundColor White
Write-Host ""

$textureGuids = @{}
foreach ($type in $floorData) {
    $texName = "$($type.Type)_Floor_Texture.png"
    $metaPath = Join-Path $materialsFolder "$texName.meta"
    
    if (Test-Path $metaPath) {
        $metaContent = Get-Content $metaPath -Raw -Encoding UTF8
        if ($metaContent -match 'guid:\s*([a-f0-9]{32})') {
            $guid = $matches[1]
            $textureGuids[$type.Type] = $guid
            Write-Host "  ✓ $($type.Type)_Floor_Texture.png -> GUID: $guid" -ForegroundColor Green
        }
        else {
            Write-Host "  ✗ Could not find GUID in $($type.Type)_Floor_Texture.png.meta" -ForegroundColor Red
        }
    }
    else {
        Write-Host "  ✗ Meta file not found: $metaPath" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "[Step 2] Fixing material files..." -ForegroundColor White
Write-Host ""

foreach ($type in $floorData) {
    $matName = "$($type.Type)_Floor.mat"
    $matPath = Join-Path $materialsFolder $matName
    
    if (-not $textureGuids.ContainsKey($type.Type)) {
        Write-Host "  ⚠ Skipping $($type.Type): No texture GUID found" -ForegroundColor Yellow
        continue
    }
    
    if (-not (Test-Path $matPath)) {
        Write-Host "  ✗ Material not found: $matPath" -ForegroundColor Red
        continue
    }
    
    $textureGuid = $textureGuids[$type.Type]
    
    Write-Host "  Fixing $($type.Type)_Floor.mat:" -ForegroundColor Yellow
    
    # Read the material file
    $matContent = Get-Content $matPath -Raw -Encoding UTF8
    
    # Pattern to match the _BaseMap texture entry
    $baseMapPattern = '(_BaseMap:\s*\n\s*m_Texture: \{fileID: )0(\})'
    $mainTexPattern = '(_MainTex:\s*\n\s*m_Texture: \{fileID: )0(\})'
    
    $newBaseMap = "`${1}2130477294, guid: $textureGuid, type: 3`${2}"
    $newMainTex = "`${1}2800000, guid: $textureGuid, type: 3`${2}"
    
    $modified = $false
    
    # Fix _BaseMap
    if ($matContent -match $baseMapPattern) {
        $matContent = $matContent -replace $baseMapPattern, $newBaseMap
        $modified = $true
        Write-Host "    ✓ Updated _BaseMap texture reference" -ForegroundColor Green
    }
    
    # Fix _MainTex
    if ($matContent -match $mainTexPattern) {
        $matContent = $matContent -replace $mainTexPattern, $newMainTex
        $modified = $true
        Write-Host "    ✓ Updated _MainTex texture reference" -ForegroundColor Green
    }
    
    if ($modified) {
        # Write back the modified content
        Set-Content -Path $matPath -Value $matContent -Encoding UTF8 -NoNewline
        Write-Host "    ✓ Saved changes" -ForegroundColor Green
        $changesMade++
    }
    else {
        Write-Host "    ⚠ No changes needed or pattern not found" -ForegroundColor Yellow
    }
    
    Write-Host ""
}

Write-Host "[Step 3] Summary..." -ForegroundColor White
Write-Host ""
Write-Host "Materials fixed: $changesMade / 5" -ForegroundColor Cyan
Write-Host ""

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  NEXT STEPS" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Open Unity Editor (if not already open)" -ForegroundColor White
Write-Host "2. Wait for Unity to reimport the materials" -ForegroundColor Yellow
Write-Host "3. Check if textures appear correctly on materials" -ForegroundColor Yellow
Write-Host ""
Write-Host "⚠️  IMPORTANT: Run backup.ps1 now!" -ForegroundColor Yellow
Write-Host "   Command: .\backup.ps1" -ForegroundColor Yellow
Write-Host ""
Write-Host "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
