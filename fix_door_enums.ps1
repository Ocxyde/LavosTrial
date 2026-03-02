# fix_door_enums.ps1
# Move DoorVariant and DoorTrapType enums back to Core assembly
# Unity 6 compatible - UTF-8 encoding - Unix line endings

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     FIX DOOR ENUMS - Move to Core assembly                     ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$scriptRoot = $PSScriptRoot

# ============================================================================
# CREATE DoorTypes.cs IN CORE WITH ENUMS
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📝 CREATE DoorTypes.cs IN CORE" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$doorTypesPath = Join-Path $scriptRoot "Assets\Scripts\Core\DoorTypes.cs"
$doorTypesContent = @'
// DoorTypes.cs
// Door variant and trap type enums for Core assembly
// Unity 6 compatible - UTF-8 encoding - Unix line endings

namespace Code.Lavos.Core
{
    /// <summary>
    /// Door types with different behaviors and interactions.
    /// </summary>
    public enum DoorVariant
    {
        Normal,         // Standard door, opens normally
        Locked,         // Requires key to open
        Trapped,        // Triggers trap when opened
        Secret,         // Hidden door, reveals secret area
        OneWay,         // Can only pass through one direction
        Cursed,         // Applies debuff when opened
        Blessed,        // Applies buff when opened
        Boss            // Boss door, special effects
    }

    /// <summary>
    /// Trap types that can be attached to doors.
    /// </summary>
    public enum DoorTrapType
    {
        None,
        Spike,          // Deals physical damage
        Fire,           // Deals fire damage
        Poison,         // Applies poison DoT
        Freeze,         // Slows/freezes player
        Shock,          // Deals lightning damage
        Teleport,       // Teleports player elsewhere
        Alarm,          // Alerts enemies
        Collapse        // Blocks passage after use
    }
}
'@

$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText($doorTypesPath, $doorTypesContent, $utf8NoBom)
Write-Host "  ✅ Created: DoorTypes.cs" -ForegroundColor Green
Write-Host ""

# ============================================================================
# CREATE TrapType.cs IN CORE
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📝 CREATE TrapType.cs IN CORE" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$trapTypePath = Join-Path $scriptRoot "Assets\Scripts\Core\TrapType.cs"
$trapTypeContent = @'
// TrapType.cs
// Trap type enum for Core assembly
// Unity 6 compatible - UTF-8 encoding - Unix line endings

namespace Code.Lavos.Core
{
    /// <summary>
    /// Types of traps that can be placed in the maze.
    /// </summary>
    public enum TrapType
    {
        None,
        Spike,          // Deals physical damage
        Fire,           // Deals fire damage over time
        Poison,         // Applies poison DoT
        Freeze,         // Slows/freezes player
        Shock,          // Deals lightning damage
        Teleport,       // Teleports player to random location
        Alarm,          // Alerts nearby enemies
        Collapse        // Causes ceiling/wall collapse
    }
}
'@

[System.IO.File]::WriteAllText($trapTypePath, $trapTypeContent, $utf8NoBom)
Write-Host "  ✅ Created: TrapType.cs" -ForegroundColor Green
Write-Host ""

# ============================================================================
# UPDATE Code.Lavos.Core.asmdef - Add Gameplay reference for ItemData
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📝 UPDATE Code.Lavos.Core.asmdef" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$coreAsmdefPath = Join-Path $scriptRoot "Assets\Scripts\Core\Code.Lavos.Core.asmdef"
if (Test-Path $coreAsmdefPath) {
    $coreContent = @{
        name = "Code.Lavos.Core"
        rootNamespace = "Code.Lavos.Core"
        references = @(
            "Code.Lavos.Status",
            "Code.Lavos.Gameplay",
            "Code.Lavos.Ressources",
            "Code.Lavos.Player",
            "GUID:6280726566d84ea0be05b4f0a91fd88d"
        )
        includePlatforms = @()
        excludePlatforms = @()
        allowUnsafeCode = $false
        overrideReferences = $true
        precompiledReferences = @()
        autoReferenced = $true
        generateProgrammingAssets = $false
    } | ConvertTo-Json -Depth 10

    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($coreAsmdefPath, $coreContent, $utf8NoBom)
    Write-Host "  ✅ Updated: Code.Lavos.Core.asmdef" -ForegroundColor Green
    Write-Host "     Added references:" -ForegroundColor Gray
    Write-Host "       - Code.Lavos.Gameplay (ItemData, TrapBehavior)" -ForegroundColor DarkGray
    Write-Host "       - Code.Lavos.Ressources (MazeRenderer)" -ForegroundColor DarkGray
    Write-Host "       - Code.Lavos.Player (PlayerStats, PlayerController)" -ForegroundColor DarkGray
} else {
    Write-Host "  ❌ Not found: Code.Lavos.Core.asmdef" -ForegroundColor Red
}
Write-Host ""

# ============================================================================
# SUMMARY
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📊 SUMMARY" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Created: DoorTypes.cs (DoorVariant, DoorTrapType enums)" -ForegroundColor White
Write-Host "  Created: TrapType.cs (TrapType enum)" -ForegroundColor White
Write-Host "  Updated: Code.Lavos.Core.asmdef with required references" -ForegroundColor White
Write-Host ""

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "🔧 NEXT STEPS" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  1. Open Unity Editor and recompile" -ForegroundColor White
Write-Host "  2. Verify no compilation errors" -ForegroundColor White
Write-Host ""

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "🔧 GIT REMINDER" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  After verifying everything works, commit changes:" -ForegroundColor White
Write-Host ""
Write-Host "  git add -A" -ForegroundColor Gray
Write-Host "  git commit -m `"fix: Add DoorTypes and TrapType enums to Core assembly`"" -ForegroundColor Cyan
Write-Host ""
