# fix-cyclic-dependency.ps1
# Fix cyclic dependency by consolidating assemblies
# UTF-8 encoding - Unix line endings

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Fix Cyclic Dependency" -ForegroundColor White
Write-Host "  Consolidating assemblies" -ForegroundColor DarkGray
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Backup and rename asmdef files to disable them
# Unity will then compile everything into Assembly-CSharp

$asmdefFiles = @(
    "Assets\DB_SQLite\Code.Lavos.DB.asmdef",
    "Assets\Scripts\Ennemies\Code.Lavos.Ennemies.asmdef",
    "Assets\Scripts\Gameplay\Code.Lavos.Gameplay.asmdef",
    "Assets\Scripts\HUD\Code.Lavos.HUD.asmdef",
    "Assets\Scripts\Interaction\Code.Lavos.Interaction.asmdef",
    "Assets\Scripts\Inventory\Code.Lavos.Inventory.asmdef",
    "Assets\Scripts\Player\Code.Lavos.Player.asmdef",
    "Assets\Scripts\Ressources\Code.Lavos.Ressources.asmdef",
    "Assets\Scripts\Tests\Code.Lavos.Tests.asmdef",
    "Assets\Scripts\Core\Code.Lavos.Core.asmdef",
    "Assets\Scripts\Status\Code.Lavos.Status.asmdef"
)

foreach ($file in $asmdefFiles) {
    $fullPath = Join-Path $scriptDir $file
    if (Test-Path $fullPath) {
        $backupPath = "$fullPath.backup"
        Rename-Item $fullPath $backupPath
        Write-Host "  [DISABLED] $file" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Cyclic Dependency Fixed" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "All .asmdef files have been renamed to .asmdef.backup" -ForegroundColor Green
Write-Host "Unity will now compile everything into Assembly-CSharp" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Close Unity Editor completely" -ForegroundColor Gray
Write-Host "  2. Delete Library folder (clear-unity-cache.bat)" -ForegroundColor Gray
Write-Host "  3. Open Unity Editor" -ForegroundColor Gray
Write-Host "  4. Wait for compilation" -ForegroundColor Gray
Write-Host ""
