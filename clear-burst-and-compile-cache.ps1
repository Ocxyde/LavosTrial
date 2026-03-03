# Clear Burst Cache and Compilation Errors
# Unity 6 (6000.3.7f1) Compatible
# Run this script to clear Burst cache, compilation caches, and force clean rebuild
#
# Usage: .\clear-burst-and-compile-cache.ps1

$ErrorActionPreference = "Stop"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Clear Burst & Compilation Cache" -ForegroundColor Cyan
Write-Host "  $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')" -ForegroundColor Cyan
Write-Host "  Unity 6 (6000.3.7f1) Compatible" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$projectRoot = $PSScriptRoot

# Close Unity if running
Write-Host "[1/7] Closing Unity if running..." -ForegroundColor Yellow
Get-Process "Unity" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 2
Write-Host "  Unity closed" -ForegroundColor Green

# Clear Burst cache
Write-Host "[2/7] Clearing Burst cache..." -ForegroundColor Yellow
$burstCacheDirs = @(
    "Library\Burst",
    "Library\BurstCache",
    "Temp\Burst",
    "Library\PackageCache\com.unity.burst"
)
foreach ($dir in $burstCacheDirs) {
    $fullPath = Join-Path $projectRoot $dir
    if (Test-Path $fullPath) {
        Remove-Item $fullPath -Recurse -Force
        Write-Host "  Deleted: $dir" -ForegroundColor Green
    } else {
        Write-Host "  Not found: $dir" -ForegroundColor Gray
    }
}

# Clear compilation caches
Write-Host "[3/7] Clearing compilation caches..." -ForegroundColor Yellow
$compileCacheDirs = @(
    "Library\ScriptAssemblies",
    "Library\LastSceneManagerSetup.txt",
    "Library\LastSceneManagerSetup.txt.meta",
    "Library\BuildSettings.asset",
    "Library\BuildSettings.asset.meta",
    "Library\ProjectSettings.asset",
    "Library\ProjectSettings.asset.meta",
    "Library\AssetDatabase",
    "Library\MonoManager",
    "Library\NativePlugins",
    "Library\UnityAssemblies",
    "Library\LastBuild.build",
    "Library\LastBuild.run"
)
foreach ($item in $compileCacheDirs) {
    $fullPath = Join-Path $projectRoot $item
    if (Test-Path $fullPath) {
        Remove-Item $fullPath -Recurse -Force
        Write-Host "  Deleted: $item" -ForegroundColor Green
    } else {
        Write-Host "  Not found: $item" -ForegroundColor Gray
    }
}

# Clear C# compilation cache (obj)
Write-Host "[4/7] Clearing C# compilation cache (obj)..." -ForegroundColor Yellow
if (Test-Path "$projectRoot\obj") {
    Remove-Item "$projectRoot\obj" -Recurse -Force
    Write-Host "  obj folder deleted" -ForegroundColor Green
} else {
    Write-Host "  obj folder not found" -ForegroundColor Gray
}

# Clear Temp folder
Write-Host "[5/7] Clearing Temp folder..." -ForegroundColor Yellow
if (Test-Path "$projectRoot\Temp") {
    Remove-Item "$projectRoot\Temp" -Recurse -Force
    Write-Host "  Temp folder deleted" -ForegroundColor Green
} else {
    Write-Host "  Temp folder not found" -ForegroundColor Gray
}

# Clear generated .csproj and .sln files
Write-Host "[6/7] Clearing generated project files..." -ForegroundColor Yellow
Get-ChildItem $projectRoot -Filter "*.csproj" -File -ErrorAction SilentlyContinue | Remove-Item -Force
Get-ChildItem $projectRoot -Filter "*.sln" -File -ErrorAction SilentlyContinue | Remove-Item -Force
Write-Host "  Project files deleted (will regenerate)" -ForegroundColor Green

# Clear Rider/ReSharper cache (optional but recommended)
Write-Host "[7/7] Clearing Rider cache (if exists)..." -ForegroundColor Yellow
$riderCacheDirs = @(
    ".idea\.ideaPeuImporte",
    ".idea\.cache",
    ".idea\solutionCache"
)
foreach ($dir in $riderCacheDirs) {
    $fullPath = Join-Path $projectRoot $dir
    if (Test-Path $fullPath) {
        Remove-Item $fullPath -Recurse -Force
        Write-Host "  Deleted: $dir" -ForegroundColor Green
    } else {
        Write-Host "  Not found: $dir" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Cache Cleared Successfully!" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor White
Write-Host "  1. Open Unity Hub" -ForegroundColor Cyan
Write-Host "  2. Open this project" -ForegroundColor Cyan
Write-Host "  3. Wait for full recompile (Burst + Scripts)" -ForegroundColor Cyan
Write-Host "  4. Check Console for errors" -ForegroundColor Cyan
Write-Host "  5. Run backup.ps1 after verification" -ForegroundColor Yellow
Write-Host ""
Write-Host "Note: First compilation after clearing Burst cache" -ForegroundColor Gray
Write-Host "      may take longer as Burst recompiles all jobs." -ForegroundColor Gray
Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
