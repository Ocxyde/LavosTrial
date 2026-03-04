# Restore-Class-Structure.ps1
# Fixes files where class declarations were accidentally removed
#
# Run: .\Restore-Class-Structure.ps1

$ErrorActionPreference = "Stop"

Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  RESTORE - Class Structure" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# File 1: PlayerStats.cs
$file = "Assets\Scripts\Core\02_Player\PlayerStats.cs"
$fullPath = Join-Path $PSScriptRoot $file

if (Test-Path $fullPath) {
    Write-Host "Fixing: $file" -ForegroundColor Cyan
    
    $content = Get-Content $fullPath -Raw -Encoding UTF8
    
    # Check if class declaration exists
    if ($content -notmatch 'public class PlayerStats') {
        # Find namespace line and add class after it
        $fixedContent = $content -replace 
            '(namespace Code\.Lavos\.Core\s*\{)',
            '$1' + "`n" + '    /// <summary>' + "`n" +
            '    /// PlayerStats - MonoBehaviour wrapper for StatsEngine.' + "`n" +
            '    /// </summary>' + "`n" +
            '    public class PlayerStats : MonoBehaviour, IPlayerStats' + "`n" + '    {'
        
        [System.IO.File]::WriteAllText($fullPath, $fixedContent, [System.Text.UTF8Encoding]::new($true))
        Write-Host "  ✅ Added class declaration" -ForegroundColor Green
    } else {
        Write-Host "  ℹ️  Class declaration already exists" -ForegroundColor Gray
    }
}

# File 2: Inventory.cs
$file = "Assets\Scripts\Core\04_Inventory\Inventory.cs"
$fullPath = Join-Path $PSScriptRoot $file

if (Test-Path $fullPath) {
    Write-Host "Fixing: $file" -ForegroundColor Cyan
    
    $content = Get-Content $fullPath -Raw -Encoding UTF8
    
    if ($content -notmatch 'public class Inventory') {
        $fixedContent = $content -replace 
            '(namespace Code\.Lavos\.Core\s*\{)',
            '$1' + "`n" + '    public class Inventory : MonoBehaviour, IInventory' + "`n" + '    {'
        
        [System.IO.File]::WriteAllText($fullPath, $fixedContent, [System.Text.UTF8Encoding]::new($true))
        Write-Host "  ✅ Added class declaration" -ForegroundColor Green
    } else {
        Write-Host "  ℹ️  Class declaration already exists" -ForegroundColor Gray
    }
}

# File 3: CombatSystem.cs
$file = "Assets\Scripts\Core\05_Combat\CombatSystem.cs"
$fullPath = Join-Path $PSScriptRoot $file

if (Test-Path $fullPath) {
    Write-Host "Fixing: $file" -ForegroundColor Cyan
    
    $content = Get-Content $fullPath -Raw -Encoding UTF8
    
    if ($content -notmatch 'public class CombatSystem') {
        $fixedContent = $content -replace 
            '(namespace Code\.Lavos\.Core\s*\{)',
            '$1' + "`n" + '    /// <summary>' + "`n" +
            '    /// CombatSystem - Combat calculations and resource management.' + "`n" +
            '    /// </summary>' + "`n" +
            '    public class CombatSystem : MonoBehaviour' + "`n" + '    {'
        
        [System.IO.File]::WriteAllText($fullPath, $fixedContent, [System.Text.UTF8Encoding]::new($true))
        Write-Host "  ✅ Added class declaration" -ForegroundColor Green
    } else {
        Write-Host "  ℹ️  Class declaration already exists" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  ✅ Restore Complete!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next: Open Unity and check for errors" -ForegroundColor Cyan
