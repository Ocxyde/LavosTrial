# fix-all-headers.ps1
# Add Unity 6 standard headers to all C# files missing them
# UTF-8 encoding - Unix line endings
#
# Usage: powershell -ExecutionPolicy Bypass -File fix-all-headers.ps1

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Unity 6 Header Fixer" -ForegroundColor White
Write-Host "  Adding standard headers to all scripts" -ForegroundColor DarkGray
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# List of files to fix with their header templates
$filesToFix = @(
    @{Path="Assets\Scripts\Core\BraseroFlame.cs"; Header="`n// BraseroFlame.cs`n// Brazier flame effect with 8-bit pixel art style`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Part of the Core system - works with FlameAnimator.cs`n"},
    @{Path="Assets\Scripts\Core\DrawingManager.cs"; Header="`n// DrawingManager.cs`n// Procedural texture generation and sprite creation`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Part of the Core system - generates textures for maze`n"},
    @{Path="Assets\Scripts\Core\FlameAnimator.cs"; Header="`n// FlameAnimator.cs`n// Animation system for 8-bit flame effects`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Part of the Core system - works with BraseroFlame.cs`n"},
    @{Path="Assets\Scripts\Core\GameManager.cs"; Header="`n// GameManager.cs`n// Central game state manager (Singleton)`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// CORE: Main pivot point for plug-in-and-out system`n"},
    @{Path="Assets\Scripts\Core\ItemData.cs"; Header="`n// ItemData.cs`n// ScriptableObject data for inventory items`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Part of the Inventory system - works with Inventory.cs`n"},
    @{Path="Assets\Scripts\Core\MazeGenerator.cs"; Header="`n// MazeGenerator.cs`n// Procedural maze generation algorithm`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Part of the Core system - generates maze layout`n"},
    @{Path="Assets\Scripts\Core\ParticleGenerator.cs"; Header="`n// ParticleGenerator.cs`n// Particle system generator for VFX`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Part of the Core system - creates particle effects`n"},
    @{Path="Assets\Scripts\Editor\BuildScript.cs"; Header="`n// BuildScript.cs`n// Automated build script for all platforms`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Editor tool - builds Windows, macOS, Linux`n"},
    @{Path="Assets\Scripts\HUD\DebugHUD.cs"; Header="`n// DebugHUD.cs`n// Debug UI for testing and development`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Part of the HUD system - debug only`n"},
    @{Path="Assets\Scripts\HUD\HUDSystem.cs"; Header="`n// HUDSystem.cs`n// Main HUD management system`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// CORE: Central UI manager for all HUD elements`n"},
    @{Path="Assets\Scripts\Interaction\InteractableObject.cs"; Header="`n// InteractableObject.cs`n// Base class for interactable objects`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Part of the Interaction system - implements IInteractable`n"},
    @{Path="Assets\Scripts\Inventory\Inventory.cs"; Header="`n// Inventory.cs`n// Inventory management system (Singleton)`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// CORE: Central inventory manager`n"},
    @{Path="Assets\Scripts\Inventory\InventorySlotUI.cs"; Header="`n// InventorySlotUI.cs`n// UI slot for inventory items`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Part of the Inventory system - UI component`n"},
    @{Path="Assets\Scripts\Inventory\InventoryUI.cs"; Header="`n// InventoryUI.cs`n// Inventory UI display and management`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Part of the Inventory system - UI controller`n"},
    @{Path="Assets\Scripts\Inventory\ItemPickup.cs"; Header="`n// ItemPickup.cs`n// Pickup behavior for inventory items`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Part of the Inventory system - world pickup`n"},
    @{Path="Assets\Scripts\Player\PersistentPlayerData.cs"; Header="`n// PersistentPlayerData.cs`n// Persistent player data between sessions`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Part of the Player system - save/load data`n"},
    @{Path="Assets\Scripts\Player\PlayerController.cs"; Header="`n// PlayerController.cs`n// Player movement, camera, and input`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// CORE: Player controller with New Input System`n"},
    @{Path="Assets\Scripts\Player\PlayerHealth.cs"; Header="`n// PlayerHealth.cs`n// Player health management`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Part of the Player system - health component`n"},
    @{Path="Assets\Scripts\Player\PlayerStats.cs"; Header="`n// PlayerStats.cs`n// Player stats wrapper for StatsEngine`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// CORE: MonoBehaviour integration for StatsEngine`n"},
    @{Path="Assets\Scripts\Player\StatusEffect.cs"; Header="`n// StatusEffect.cs`n// Legacy status effect wrapper`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Part of the Status system - backward compatibility`n"},
    @{Path="Assets\Scripts\Ressources\AnimatedFlame.cs"; Header="`n// AnimatedFlame.cs`n// Animated 8-bit flame sprite`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Part of the Ressources system - flame animation`n"},
    @{Path="Assets\Scripts\Ressources\DrawingPool.cs"; Header="`n// DrawingPool.cs`n// Object pool for procedural textures`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Part of the Ressources system - texture pooling`n"},
    @{Path="Assets\Scripts\Ressources\MazeRenderer.cs"; Header="`n// MazeRenderer.cs`n// Renders maze from MazeGenerator data`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Part of the Ressources system - maze visualization`n"},
    @{Path="Assets\Scripts\Ressources\TorchController.cs"; Header="`n// TorchController.cs`n// Controller for torch objects`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Part of the Ressources system - torch logic`n"},
    @{Path="Assets\Scripts\Ressources\TorchDiagnostics.cs"; Header="`n// TorchDiagnostics.cs`n// Diagnostic tools for torch system`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Part of the Ressources system - debugging`n"},
    @{Path="Assets\Scripts\Ressources\TorchPool.cs"; Header="`n// TorchPool.cs`n// Object pool for torch prefabs`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Part of the Ressources system - torch pooling`n"},
    @{Path="Assets\Scripts\Status\StatsEngine.cs"; Header="`n// StatsEngine.cs`n// Central stat calculation engine`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// CORE: Pure C# stat management (no MonoBehaviour)`n"},
    @{Path="Assets\Scripts\Status\StatusEffect.cs"; Header="`n// StatusEffect.cs`n// Status effect data wrapper`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Part of the Status system - effect data`n"},
    @{Path="Assets\Scripts\Status\StatusEffectData.cs"; Header="`n// StatusEffectData.cs`n// Comprehensive status effect data`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// CORE: Status effect definition and data`n"},
    @{Path="Assets\Scripts\Tests\MazeGeneratorTests.cs"; Header="`n// MazeGeneratorTests.cs`n// Unit tests for MazeGenerator`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Test file - validates maze generation`n"},
    @{Path="Assets\Scripts\Tests\StatsEngineTests.cs"; Header="`n// StatsEngineTests.cs`n// Unit tests for StatsEngine`n// Unity 6 compatible - UTF-8 encoding - Unix line endings`n//`n// Test file - validates stat calculations`n"}
)

$fixedCount = 0
$errorCount = 0

foreach ($fileInfo in $filesToFix) {
    $filePath = $fileInfo.Path
    $header = $fileInfo.Header
    
    $fullPath = Join-Path $scriptDir $filePath
    
    if (Test-Path $fullPath) {
        try {
            # Read current content
            $content = Get-Content $fullPath -Raw -Encoding UTF8
            
            # Check if already has header
            if ($content -match "^//\s.*\.cs") {
                Write-Host "  [SKIP] Already has header: $filePath" -ForegroundColor Gray
                continue
            }
            
            # Add header
            $newContent = $header + $content
            
            # Write back with UTF-8 encoding and Unix line endings
            $newContent = $newContent -replace "`r`n", "`n"
            [System.IO.File]::WriteAllText($fullPath, $newContent, [System.Text.UTF8Encoding]::new($false))
            
            Write-Host "  [FIXED] $filePath" -ForegroundColor Green
            $fixedCount++
        }
        catch {
            Write-Host "  [ERROR] Failed to fix: $filePath - $_" -ForegroundColor Red
            $errorCount++
        }
    } else {
        Write-Host "  [NOT FOUND] $filePath" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Header Fix Complete" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Files fixed: $fixedCount" -ForegroundColor Green
if ($errorCount -gt 0) {
    Write-Host "  Errors: $errorCount" -ForegroundColor Red
}
Write-Host ""
Write-Host "Next: Run backup script" -ForegroundColor Yellow
Write-Host "  powershell -ExecutionPolicy Bypass -File apply-patches-and-backup.ps1" -ForegroundColor Gray
Write-Host ""
