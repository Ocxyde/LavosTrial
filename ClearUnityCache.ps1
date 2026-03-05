# ClearUnityCache.ps1
# Clears Unity cache and Library folder for clean rebuild
# Unity 6 compatible
#
# USAGE:
#   1. Close Unity Editor
#   2. Run: .\ClearUnityCache.ps1
#   3. Reopen Unity (will rebuild Library)

Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  UNITY CACHE CLEANER" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$unityProjectPath = Get-Location

Write-Host "Project: $unityProjectPath" -ForegroundColor Yellow
Write-Host ""

# Check if Unity is running
$unityProcesses = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
if ($unityProcesses) {
    Write-Host "⚠️  WARNING: Unity is still running!" -ForegroundColor Red
    Write-Host "   Please close Unity before running this script." -ForegroundColor Red
    Write-Host ""
    Write-Host "Press any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

Write-Host "✓ Unity is not running - safe to proceed" -ForegroundColor Green
Write-Host ""

# Folders to clean
$foldersToClean = @(
    "Library",
    "Temp",
    "Obj",
    "Logs",
    "UserSettings"
)

# Files to clean
$filesToClean = @(
    "*.pidb",
    "*.userprefs",
    "*.sln",
    "*.csproj"
)

Write-Host "Cleaning folders..." -ForegroundColor Yellow
foreach ($folder in $foldersToClean) {
    $path = Join-Path $unityProjectPath $folder
    if (Test-Path $path) {
        Write-Host "  • Removing: $folder" -ForegroundColor Gray
        Remove-Item -Path $path -Recurse -Force -ErrorAction SilentlyContinue
    }
}

Write-Host ""
Write-Host "Cleaning files..." -ForegroundColor Yellow
foreach ($file in $filesToClean) {
    $items = Get-ChildItem -Path $unityProjectPath -Filter $file -ErrorAction SilentlyContinue
    foreach ($item in $items) {
        Write-Host "  • Removing: $($item.Name)" -ForegroundColor Gray
        Remove-Item -Path $item.FullName -Force -ErrorAction SilentlyContinue
    }
}

Write-Host ""
Write-Host "Cleaning Assembly-CSharp projects..." -ForegroundColor Yellow
$csprojFiles = Get-ChildItem -Path $unityProjectPath -Filter "Assembly-CSharp*.csproj" -ErrorAction SilentlyContinue
foreach ($file in $csprojFiles) {
    Write-Host "  • Removing: $($file.Name)" -ForegroundColor Gray
    Remove-Item -Path $file.FullName -Force -ErrorAction SilentlyContinue
}

Write-Host ""
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  ✅ CACHE CLEANED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Open Unity Editor" -ForegroundColor White
Write-Host "  2. Wait for Library to rebuild (may take 1-2 minutes)" -ForegroundColor White
Write-Host "  3. Tools → Maze → Generate Maze" -ForegroundColor White
Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
