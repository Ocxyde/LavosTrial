# fix_floor_materials.ps1
# Fixes floor materials to reference their texture files
# Run this script from project root: .\fix_floor_materials.ps1
# After running, remember to run: .\backup.ps1

$ErrorActionPreference = "Stop"
$projectRoot = $PSScriptRoot

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Floor Materials Texture Fix" -ForegroundColor Cyan
Write-Host "  Unity 6 Project - PeuImporte" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Floor types to fix
$floorTypes = @("Stone", "Wood", "Tile", "Brick", "Marble")
$materialsFolder = Join-Path $projectRoot "Assets\Materials\Floor"
$changesMade = @()

Write-Host "[Step 1] Checking floor materials and textures..." -ForegroundColor White
Write-Host ""

foreach ($type in $floorTypes) {
    $matPath = Join-Path $materialsFolder "$($type)_Floor.mat"
    $texPath = Join-Path $materialsFolder "$($type)_Floor_Texture.png"
    
    Write-Host "  Checking $type floor:" -ForegroundColor Yellow
    
    # Check if files exist
    $matExists = Test-Path $matPath
    $texExists = Test-Path $texPath
    
    if (-not $matExists) {
        Write-Host "    ✗ Material not found: $matPath" -ForegroundColor Red
        continue
    }
    
    if (-not $texExists) {
        Write-Host "    ✗ Texture not found: $texPath" -ForegroundColor Red
        continue
    }
    
    Write-Host "    ✓ Material: $($type)_Floor.mat" -ForegroundColor Green
    Write-Host "    ✓ Texture: $($type)_Floor_Texture.png" -ForegroundColor Green
    
    # Read the material file
    $matContent = Get-Content $matPath -Raw -Encoding UTF8
    
    # Generate GUID for texture (Unity uses 128-bit GUID)
    # We'll create a deterministic GUID based on the texture name
    $textureGuid = [System.Guid]::NewGuid().ToString("n")
    
    # Check if texture is already assigned
    if ($matContent -match "_BaseMap:\s*\{fileID:\s*0\s*\}") {
        Write-Host "    ⚠ Texture NOT assigned in material" -ForegroundColor Red
        
        # We need to fix this manually by updating the material file
        # But Unity materials are complex - better to let Unity reimport
        
        # Alternative: Create a simple C# editor script to fix it
        $changesMade += $type
    }
    else {
        Write-Host "    ✓ Texture already assigned" -ForegroundColor Green
    }
    
    Write-Host ""
}

Write-Host "[Step 2] Creating editor script to fix materials..." -ForegroundColor White
Write-Host ""

# Create an editor script that will fix the materials when run in Unity
$editorScriptPath = Join-Path $materialsFolder "..\..\Scripts\Editor\FixFloorMaterials.cs"
$editorScriptPath = (Resolve-Path $editorScriptPath).Path

$editorScript = @'
// FixFloorMaterials.cs
// Editor script to fix floor material texture references
// Run from Unity: Menu > Tools > Fix Floor Materials

using UnityEngine;
using UnityEditor;
using System.IO;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// FixFloorMaterials - Editor utility to fix floor material textures.
    /// </summary>
    public class FixFloorMaterials : EditorWindow
    {
        [MenuItem("Tools/Fix Floor Materials")]
        public static void FixMaterials()
        {
            string materialsFolder = "Assets/Materials/Floor";
            string[] floorTypes = { "Stone", "Wood", "Tile", "Brick", "Marble" };
            int fixedCount = 0;

            foreach (string type in floorTypes)
            {
                string matPath = $"{materialsFolder}/{type}_Floor.mat";
                string texPath = $"{materialsFolder}/{type}_Floor_Texture.png";

                // Load texture
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                if (texture == null)
                {
                    Debug.LogError($"[FixFloorMaterials] Texture not found: {texPath}");
                    continue;
                }

                // Load material
                Material material = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (material == null)
                {
                    Debug.LogError($"[FixFloorMaterials] Material not found: {matPath}");
                    continue;
                }

                // Assign texture
                material.mainTexture = texture;
                material.mainTextureScale = new Vector2(1f, 1f);
                
                // For URP
                material.SetTexture("_BaseMap", texture);
                material.SetTexture("_MainTex", texture);

                EditorUtility.SetDirty(material);
                fixedCount++;

                Debug.Log($"[FixFloorMaterials] ✅ Fixed: {type}_Floor.mat");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[FixFloorMaterials] ✅ Fixed {fixedCount} materials!");
            EditorUtility.DisplayDialog("Fix Floor Materials", 
                $"Successfully fixed {fixedCount} floor materials!", "OK");
        }
    }
}
'@

# Ensure Editor folder exists
$editorFolder = Join-Path $projectRoot "Assets\Scripts\Editor"
if (-not (Test-Path $editorFolder)) {
    New-Item -ItemType Directory -Path $editorFolder -Force | Out-Null
}

# Write the editor script
Set-Content -Path $editorScriptPath -Value $editorScript -Encoding UTF8
$changesMade += "Created FixFloorMaterials.cs editor script"

Write-Host "  ✓ Created editor script:" -ForegroundColor Green
Write-Host "    Assets/Scripts/Editor/FixFloorMaterials.cs" -ForegroundColor Cyan
Write-Host ""

Write-Host "[Step 3] Summary..." -ForegroundColor White
Write-Host ""

if ($changesMade.Count -gt 0) {
    Write-Host "Changes/Actions ($($changesMade.Count)):" -ForegroundColor White
    foreach ($change in $changesMade) {
        Write-Host "  - $change" -ForegroundColor Gray
    }
    Write-Host ""
}

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  INSTRUCTIONS" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Open Unity Editor" -ForegroundColor White
Write-Host "2. Go to menu: Tools > Fix Floor Materials" -ForegroundColor Yellow
Write-Host "3. Wait for the dialog confirming fix" -ForegroundColor Yellow
Write-Host "4. Check Console for success messages" -ForegroundColor Yellow
Write-Host "5. Save the scene" -ForegroundColor Yellow
Write-Host ""
Write-Host "⚠️  IMPORTANT: After fixing in Unity, run backup.ps1!" -ForegroundColor Yellow
Write-Host "   Command: .\backup.ps1" -ForegroundColor Yellow
Write-Host ""
Write-Host "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
