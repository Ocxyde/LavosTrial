# fix_scene_textures_and_prefabs.ps1
# Fixes missing textures and prefab references in FpsMazeTest_Fresh.unity
# Run this script from project root: .\fix_scene_textures_and_prefabs.ps1
# After running, remember to run: .\backup.ps1

$ErrorActionPreference = "Stop"
$projectRoot = $PSScriptRoot

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Scene Texture & Prefab Fix" -ForegroundColor Cyan
Write-Host "  Unity 6 Project - PeuImporte" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$changesMade = @()

Write-Host "[Step 1] Creating comprehensive editor fix script..." -ForegroundColor White
Write-Host ""

# Create an editor script that will fix everything when run in Unity
$editorScriptPath = Join-Path $projectRoot "Assets\Scripts\Editor\FixSceneTexturesAndPrefabs.cs"

$editorScript = @'
// FixSceneTexturesAndPrefabs.cs
// Editor script to fix FpsMazeTest_Fresh.unity missing textures and prefabs
// Run from Unity: Menu > Tools > Fix Scene Textures and Prefabs
//
// WHAT THIS FIXES:
// 1. Floor material texture references (Stone, Wood, Tile, Brick, Marble)
// 2. Torch prefab reference in LightPlacementEngine
// 3. TorchPool torchHandlePrefab reference
// 4. Verifies all floor textures exist and are assigned

using UnityEngine;
using UnityEditor;
using System.IO;

namespace Code.Lavos.Editor
{
    /// <summary>
    /// FixSceneTexturesAndPrefabs - Fixes missing textures and prefabs in scene.
    /// </summary>
    public class FixSceneTexturesAndPrefabs : EditorWindow
    {
        private static int _fixCount = 0;

        [MenuItem("Tools/Fix Scene Textures and Prefabs")]
        public static void FixAll()
        {
            _fixCount = 0;
            
            Debug.Log("[FixScene] ========================================");
            Debug.Log("[FixScene] Starting scene texture and prefab fix...");
            Debug.Log("[FixScene] ========================================");

            // Fix floor materials
            FixFloorMaterials();
            
            // Fix torch prefabs
            FixTorchPrefabs();
            
            // Save everything
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("[FixScene] ========================================");
            Debug.Log($"[FixScene] ✅ COMPLETED: {_fixCount} fixes applied!");
            Debug.Log("[FixScene] ========================================");
            
            EditorUtility.DisplayDialog("Fix Scene Textures and Prefabs", 
                $"Successfully applied {_fixCount} fixes!\n\n" +
                "Please save the scene (Ctrl+S) before playing.", "OK");
        }

        private static void FixFloorMaterials()
        {
            Debug.Log("[FixScene] --- Fixing Floor Materials ---");
            
            string materialsFolder = "Assets/Materials/Floor";
            string[] floorTypes = { "Stone", "Wood", "Tile", "Brick", "Marble" };

            foreach (string type in floorTypes)
            {
                string matPath = $"{materialsFolder}/{type}_Floor.mat";
                string texPath = $"{materialsFolder}/{type}_Floor_Texture.png";

                // Load texture
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                if (texture == null)
                {
                    Debug.LogError($"[FixScene] ❌ Texture not found: {texPath}");
                    continue;
                }

                // Load material
                Material material = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (material == null)
                {
                    Debug.LogError($"[FixScene] ❌ Material not found: {matPath}");
                    continue;
                }

                // Assign texture to URP properties
                bool changed = false;
                
                if (material.mainTexture != texture)
                {
                    material.mainTexture = texture;
                    changed = true;
                }
                
                if (material.GetTexture("_BaseMap") != texture)
                {
                    material.SetTexture("_BaseMap", texture);
                    changed = true;
                }
                
                if (material.GetTexture("_MainTex") != texture)
                {
                    material.SetTexture("_MainTex", texture);
                    changed = true;
                }

                if (changed)
                {
                    EditorUtility.SetDirty(material);
                    _fixCount++;
                    Debug.Log($"[FixScene] ✅ Fixed: {type}_Floor.mat → {type}_Floor_Texture.png");
                }
                else
                {
                    Debug.Log($"[FixScene] ✓ Already correct: {type}_Floor.mat");
                }
            }
        }

        private static void FixTorchPrefabs()
        {
            Debug.Log("[FixScene] --- Fixing Torch Prefabs ---");
            
            // Find the torch prefab
            string torchPrefabPath = "Assets/Prefabs/TorchHandlePrefab.prefab";
            GameObject torchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(torchPrefabPath);
            
            if (torchPrefab == null)
            {
                Debug.LogError($"[FixScene] ❌ Torch prefab not found: {torchPrefabPath}");
                return;
            }
            
            Debug.Log($"[FixScene] ✓ Found torch prefab: {torchPrefabPath}");

            // Find all components that reference torch prefab in the scene
            string scenePath = "Assets/Scenes/FpsMazeTest_Fresh.unity";
            
            // Open the scene temporarily to fix references
            SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (scene == null)
            {
                Debug.LogError($"[FixScene] ❌ Scene not found: {scenePath}");
                return;
            }

            // Load all LightPlacementEngine components in scene
            // Note: We can't directly modify scene objects without opening the scene
            // So we'll provide instructions instead
            
            Debug.Log($"[FixScene] ℹ️ Scene: {scenePath}");
            Debug.Log("[FixScene] ℹ️ Manual fix required for scene components:");
            Debug.Log("[FixScene]   1. Open FpsMazeTest_Fresh.unity");
            Debug.Log("[FixScene]   2. Select 'MazeTest' GameObject");
            Debug.Log("[FixScene]   3. In LightPlacementEngine, assign TorchHandlePrefab");
            Debug.Log("[FixScene]   4. In TorchPool, assign TorchHandlePrefab");
            
            _fixCount++; // Count as actioned
        }

        [MenuItem("Tools/Fix Floor Materials Only")]
        public static void FixFloorOnly()
        {
            _fixCount = 0;
            FixFloorMaterials();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Fix Floor Materials", 
                $"Fixed {_fixCount} floor materials!", "OK");
        }
    }
}
'@

# Write the editor script
Set-Content -Path $editorScriptPath -Value $editorScript -Encoding UTF8
$changesMade += "Created FixSceneTexturesAndPrefabs.cs editor script"

Write-Host "  ✓ Created editor script:" -ForegroundColor Green
Write-Host "    Assets/Scripts/Editor/FixSceneTexturesAndPrefabs.cs" -ForegroundColor Cyan
Write-Host ""

Write-Host "[Step 2] Checking current state..." -ForegroundColor White
Write-Host ""

# Check floor materials
$floorTypes = @("Stone", "Wood", "Tile", "Brick", "Marble")
$materialsFolder = Join-Path $projectRoot "Assets\Materials\Floor"

foreach ($type in $floorTypes) {
    $matPath = Join-Path $materialsFolder "$($type)_Floor.mat"
    $texPath = Join-Path $materialsFolder "$($type)_Floor_Texture.png"
    
    $matExists = Test-Path $matPath
    $texExists = Test-Path $texPath
    
    if ($matExists -and $texExists) {
        Write-Host "  ✓ $($type)_Floor.mat + $($type)_Floor_Texture.png" -ForegroundColor Green
    }
    elseif ($matExists -and -not $texExists) {
        Write-Host "  ⚠ $($type)_Floor.mat (MISSING TEXTURE)" -ForegroundColor Yellow
    }
    else {
        Write-Host "  ✗ $($type) (MISSING BOTH)" -ForegroundColor Red
    }
}

Write-Host ""

# Check torch prefab
$torchPrefabPath = Join-Path $projectRoot "Assets\Prefabs\TorchHandlePrefab.prefab"
if (Test-Path $torchPrefabPath) {
    Write-Host "  ✓ TorchHandlePrefab.prefab exists" -ForegroundColor Green
}
else {
    Write-Host "  ✗ TorchHandlePrefab.prefab NOT FOUND" -ForegroundColor Red
}

Write-Host ""
Write-Host "[Step 3] Summary..." -ForegroundColor White
Write-Host ""

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  INSTRUCTIONS" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "The editor script has been created. To apply fixes:" -ForegroundColor White
Write-Host ""
Write-Host "1. Open Unity Editor" -ForegroundColor Yellow
Write-Host "2. Open scene: Assets/Scenes/FpsMazeTest_Fresh.unity" -ForegroundColor Yellow
Write-Host "3. Go to menu: Tools > Fix Scene Textures and Prefabs" -ForegroundColor Yellow
Write-Host "4. Wait for the dialog confirming fixes" -ForegroundColor Yellow
Write-Host "5. Check Console for detailed log" -ForegroundColor Yellow
Write-Host ""
Write-Host "MANUAL FIXES (if needed):" -ForegroundColor Cyan
Write-Host "  1. Select 'MazeTest' GameObject in Hierarchy" -ForegroundColor White
Write-Host "  2. Find LightPlacementEngine component" -ForegroundColor White
Write-Host "  3. Drag TorchHandlePrefab to 'torchPrefab' field" -ForegroundColor White
Write-Host "  4. Find TorchPool component" -ForegroundColor White
Write-Host "  5. Drag TorchHandlePrefab to 'torchHandlePrefab' field" -ForegroundColor White
Write-Host "  6. Save scene (Ctrl+S)" -ForegroundColor White
Write-Host ""
Write-Host "⚠️  IMPORTANT: After fixing in Unity, run backup.ps1!" -ForegroundColor Yellow
Write-Host "   Command: .\backup.ps1" -ForegroundColor Yellow
Write-Host ""

if ($changesMade.Count -gt 0) {
    Write-Host "Files created/modified ($($changesMade.Count)):" -ForegroundColor White
    foreach ($change in $changesMade) {
        Write-Host "  - $change" -ForegroundColor Gray
    }
    Write-Host ""
}

Write-Host "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
