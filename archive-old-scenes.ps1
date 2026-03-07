# archive-old-scenes.ps1
# Archive old/unused Unity scenes
# Generated: 2026-03-07
#
# USAGE: .\archive-old-scenes.ps1
#
# Keeps only the latest working scene (MazeLav8s_V0-9_2.unity)
# Moves old scenes to Assets/Docs/Archive/Scenes/

param(
    [switch]$WhatIf,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$scenesDir = Join-Path $scriptRoot "Assets\Scenes"
$archiveDir = Join-Path $scriptRoot "Assets\Docs\Archive\Scenes"

# Scene to keep (latest working version)
$keepScene = "MazeLav8s_V0-9_2.unity"

# Scenes to archive (older versions)
$scenesToArchive = @(
    "MazeLav8s_V0-9_2_1.unity",
    "MazeLav8s_V0-8-2.unity",
    "MazeBuilder_V0-8-2.unity",
    "MazeBuilder_V0-3.unity",
    "MazeBuilder_V0-4.unity",
    "MainScene_Maze.unity"
)

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  OLD SCENES ARCHIVAL" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Working scene (KEEP):" -ForegroundColor Green
Write-Host "  ✅ $keepScene" -ForegroundColor Green
Write-Host ""

# Check which scenes exist
$scenesFound = @()
$scenesNotFound = @()

foreach ($scene in $scenesToArchive) {
    $fullPath = Join-Path $scenesDir $scene
    if (Test-Path $fullPath) {
        $scenesFound += $scene
        if ($Verbose) { Write-Host "  [FOUND] $scene" -ForegroundColor Gray }
    } else {
        $scenesNotFound += $scene
        if ($Verbose) { Write-Host "  [NOT FOUND] $scene" -ForegroundColor DarkGray }
    }
}

Write-Host "Scenes to archive: $($scenesFound.Count)" -ForegroundColor Yellow
Write-Host "Scenes not found:  $($scenesNotFound.Count)" -ForegroundColor DarkGray
Write-Host ""

if ($scenesFound.Count -eq 0) {
    Write-Host "No old scenes found. Nothing to archive." -ForegroundColor Green
    return
}

# Display scenes to archive
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SCENES TO ARCHIVE:" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
foreach ($scene in $scenesFound) {
    Write-Host "  📦 $scene" -ForegroundColor DarkYellow
}
Write-Host ""

# WhatIf mode
if ($WhatIf) {
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  WHATIF MODE - NO CHANGES MADE" -ForegroundColor Yellow
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Would create archive directory:" -ForegroundColor Gray
    Write-Host "  $archiveDir" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "Would move $($scenesFound.Count) scene(s) to archive." -ForegroundColor Gray
    return
}

# Confirm archival
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  CONFIRMATION REQUIRED" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
$confirmation = Read-Host "Archive $($scenesFound.Count) old scene(s)? [y/N]"

if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
    Write-Host "Operation cancelled by user." -ForegroundColor Yellow
    return
}

# Create archive directory
Write-Host ""
Write-Host "Creating archive directory..." -ForegroundColor Gray
Write-Host "  $archiveDir" -ForegroundColor DarkGray

try {
    New-Item -ItemType Directory -Path $archiveDir -Force | Out-Null
    Write-Host "  ✅ Archive directory created" -ForegroundColor Green
} catch {
    Write-Host "  ❌ Failed to create archive directory: $_" -ForegroundColor Red
    return
}

# Move scenes to archive
Write-Host ""
Write-Host "Archiving scenes..." -ForegroundColor Gray
$movedCount = 0
$failedCount = 0

foreach ($scene in $scenesFound) {
    $sourcePath = Join-Path $scenesDir $scene
    $destPath = Join-Path $archiveDir $scene
    
    try {
        Move-Item -Path $sourcePath -Destination $destPath -Force
        Write-Host "  ✅ Archived: $scene" -ForegroundColor Green
        $movedCount++
    } catch {
        Write-Host "  ❌ Failed to archive: $scene - $_" -ForegroundColor Red
        $failedCount++
    }
}

# Also move associated .unity.meta files if they exist
Write-Host ""
Write-Host "Archiving meta files..." -ForegroundColor Gray
foreach ($scene in $scenesFound) {
    $metaFile = $scene + ".meta"
    $sourcePath = Join-Path $scenesDir $metaFile
    $destPath = Join-Path $archiveDir $metaFile
    
    if (Test-Path $sourcePath) {
        try {
            Move-Item -Path $sourcePath -Destination $destPath -Force
            Write-Host "  ✅ Archived: $metaFile" -ForegroundColor Green
        } catch {
            Write-Host "  ⚠️  Meta file exists but couldn't move: $metaFile" -ForegroundColor DarkYellow
        }
    } else {
        if ($Verbose) { Write-Host "  [SKIP] No meta file: $metaFile" -ForegroundColor DarkGray }
    }
}

# Summary
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  ARCHIVAL SUMMARY" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Scenes archived: $movedCount / $($scenesFound.Count)" -ForegroundColor $(if ($movedCount -eq $scenesFound.Count) { "Green" } else { "Yellow" })
Write-Host "  Failures:        $failedCount" -ForegroundColor $(if ($failedCount -eq 0) { "Green" } else { "Red" })
Write-Host "  Archive location:" -ForegroundColor Gray
Write-Host "  $archiveDir" -ForegroundColor DarkGray
Write-Host ""
Write-Host "  Remaining scenes in Assets/Scenes/:" -ForegroundColor Gray

$remainingScenes = Get-ChildItem -Path $scenesDir -Filter "*.unity"
foreach ($s in $remainingScenes) {
    Write-Host "    - $($s.Name)" -ForegroundColor DarkGray
}
Write-Host ""

if ($movedCount -eq $scenesFound.Count) {
    Write-Host "✅ Archival completed successfully!" -ForegroundColor Green
} else {
    Write-Host "⚠️  Archival completed with warnings" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  NEXT STEPS" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Open Unity and verify the working scene loads correctly" -ForegroundColor White
Write-Host "2. Run backup.ps1 to backup entire project" -ForegroundColor White
Write-Host "3. Commit changes with git:" -ForegroundColor White
Write-Host "   git add ." -ForegroundColor DarkGray
Write-Host "   git commit -m `"chore: archive old unused scenes`"" -ForegroundColor DarkGray
Write-Host ""
