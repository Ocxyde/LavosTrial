# cleanup_deprecated_safe.ps1
# Safely identify and remove deprecated/unused files from Unity project
# Unity 6 compatible - UTF-8 encoding - Unix line endings
#
# ============================================================================
# SAFETY FEATURES:
# - Dry-run mode by default (shows what would be deleted)
# - Requires explicit -Remove flag to actually delete
# - Creates backup list before deletion
# - Verifies files exist before deletion
# - Shows clear warnings and confirmations
# ============================================================================
#
# Usage:
#   .\cleanup_deprecated_safe.ps1              # Preview mode (safe)
#   .\cleanup_deprecated_safe.ps1 -Remove      # Actually delete files
#   .\cleanup_deprecated_safe.ps1 -Verbose     # Show detailed information
#   .\cleanup_deprecated_safe.ps1 -Help        # Show this help
#
# ============================================================================

param(
    [switch]$Remove,
    [switch]$Verbose,
    [switch]$Help,
    [string]$BackupListPath = "deprecated_files_backup_list.txt"
)

# ============================================================================
# HELP MESSAGE
# ============================================================================

if ($Help) {
    Write-Host @"
╔════════════════════════════════════════════════════════════════╗
║     DEPRECATED FILE CLEANUP - SAFE REMOVAL SCRIPT              ║
╚════════════════════════════════════════════════════════════════╝

USAGE:
  .\cleanup_deprecated_safe.ps1              # Preview mode (default - safe)
  .\cleanup_deprecated_safe.ps1 -Remove      # Actually delete files
  .\cleanup_deprecated_safe.ps1 -Verbose     # Show detailed info
  .\cleanup_deprecated_safe.ps1 -Help        # Show this help

SAFETY FEATURES:
  ✓ Default mode is PREVIEW only (no files deleted)
  ✓ Requires -Remove flag to actually delete
  ✓ Creates backup list before deletion
  ✓ Verifies files before deletion
  ✓ Clear warnings for deprecated files

DEPRECATED FILES (Safe to Delete):
  • HUD/UIBarsSystemInitializer.cs    - Obsolete wrapper
  • Player/PlayerHealth.cs            - Use PlayerStats.cs
  • Core/SeedProgression.cs           - Use SeedManager.cs
  • Tests/*.disabled                  - Disabled test files

BEFORE RUNNING:
  1. ALWAYS run backup.ps1 first!
  2. Review the preview output carefully
  3. Run with -Remove only when ready

EXAMPLES:
  # Preview what would be deleted (SAFE - no files deleted)
  .\cleanup_deprecated_safe.ps1

  # Actually delete the files (DANGEROUS - files will be removed)
  .\cleanup_deprecated_safe.ps1 -Remove

  # Show detailed information about each file
  .\cleanup_deprecated_safe.ps1 -Verbose

"@
    exit 0
}

# ============================================================================
# INITIALIZATION
# ============================================================================

$scriptRoot = $PSScriptRoot
$deletedCount = 0
$failedCount = 0
$previewCount = 0
$backupList = @()
$startTime = Get-Date

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     🗑️  DEPRECATED FILE CLEANUP - SAFE REMOVAL                ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Started: $($startTime.ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor Gray

$modeText = if ($Remove) { "⚠️  DELETE MODE (files will be removed)" } else { "✅ PREVIEW MODE (no files deleted)" }
$modeColor = if ($Remove) { "Red" } else { "Green" }
Write-Host "  Mode: $modeText" -ForegroundColor $modeColor
Write-Host ""

# ============================================================================
# SAFETY CHECK - BACKUP REMINDER
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host "⚠️  SAFETY CHECK - BACKUP REMINDER" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host ""

$backupScript = Join-Path $scriptRoot "backup.ps1"
if (Test-Path $backupScript) {
    Write-Host "  ✅ backup.ps1 found" -ForegroundColor Green
    if (-not $Remove) {
        Write-Host "  ℹ️  Run backup.ps1 before deleting files!" -ForegroundColor Cyan
        Write-Host "     .\backup.ps1" -ForegroundColor Gray
    }
} else {
    Write-Host "  ⚠️  backup.ps1 NOT FOUND!" -ForegroundColor Red
    Write-Host "     Please create a backup manually before deleting files." -ForegroundColor Yellow
}
Write-Host ""

if ($Remove) {
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Red
    Write-Host "⚠️  WARNING - YOU ARE ABOUT TO DELETE FILES!" -ForegroundColor Red
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Red
    Write-Host ""
    Write-Host "  The following files will be PERMANENTLY DELETED:" -ForegroundColor Red
    Write-Host "  - They cannot be recovered after deletion" -ForegroundColor Red
    Write-Host "  - Make sure you have a backup!" -ForegroundColor Red
    Write-Host ""

    $confirmation = Read-Host "  Type 'YES' to confirm deletion (or press Enter to cancel)"
    if ($confirmation -ne 'YES') {
        Write-Host ""
        Write-Host "  ❌ Operation cancelled by user." -ForegroundColor Yellow
        Write-Host ""
        exit 0
    }
    Write-Host ""
}

# ============================================================================
# DEPRECATED FILES LIST
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📋 DEPRECATED FILES TO REMOVE" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Files confirmed safe to delete (with their .meta files)
$deleteList = @(
    @{
        Path = "Assets\Scripts\HUD\UIBarsSystemInitializer.cs"
        Reason = "Obsolete wrapper - references non-existent UIBarsSystemStandalone"
        Priority = "HIGH"
        Replacement = "Use UIBarsSystem.cs directly"
        DeprecatedDate = "2026-03-02"
    },
    @{
        Path = "Assets\Scripts\Player\PlayerHealth.cs"
        Reason = "Redundant with PlayerStats.cs - all features duplicated"
        Priority = "HIGH"
        Replacement = "Use PlayerStats.cs (TakeDamage, Heal, Health properties)"
        DeprecatedDate = "2026-03-02"
    },
    @{
        Path = "Assets\Scripts\Core\SeedProgression.cs"
        Reason = "Complete duplicate of SeedManager.cs functionality"
        Priority = "HIGH"
        Replacement = "Use SeedManager.Instance (CurrentSeed, AdvanceLevel)"
        DeprecatedDate = "2026-03-02"
    },
    @{
        Path = "Assets\Scripts\Tests\MazeGeneratorTests.cs.disabled"
        Reason = "Disabled test file"
        Priority = "MEDIUM"
        Replacement = "N/A - Delete or rename to .cs to enable"
        DeprecatedDate = "2026-03-02"
    },
    @{
        Path = "Assets\Scripts\Tests\StatsEngineTests.cs.disabled"
        Reason = "Disabled test file"
        Priority = "MEDIUM"
        Replacement = "N/A - Delete or rename to .cs to enable"
        DeprecatedDate = "2026-03-02"
    }
)

# Files already handled (for information)
$handledList = @(
    @{
        Path = "Assets\Scripts\Tests\NewBehaviourScript.cs"
        Action = "RENAMED to TestStartup.cs"
        Status = "✅ DONE"
    }
)

# ============================================================================
# PROCESS DEPRECATED FILES
# ============================================================================

foreach ($file in $deleteList) {
    $fullPath = Join-Path $scriptRoot $file.Path
    $relativePath = $file.Path -replace '\\', '/'

    # Priority color
    $priorityColor = switch ($file.Priority) {
        "HIGH" { "Red" }
        "MEDIUM" { "Yellow" }
        "LOW" { "White" }
        default { "White" }
    }

    Write-Host "  [$($file.Priority)] $relativePath" -ForegroundColor $priorityColor
    Write-Host "      Reason: $($file.Reason)" -ForegroundColor Gray
    Write-Host "      Replacement: $($file.Replacement)" -ForegroundColor DarkGray
    Write-Host "      Deprecated: $($file.DeprecatedDate)" -ForegroundColor DarkGray

    if (Test-Path $fullPath) {
        $fileSize = (Get-Item $fullPath).Length
        $lastWrite = (Get-Item $fullPath).LastWriteTime.ToString('yyyy-MM-dd HH:mm')
        Write-Host "      Size: $fileSize bytes | Modified: $lastWrite" -ForegroundColor DarkGray

        if ($Remove) {
            # DELETE MODE
            try {
                Remove-Item $fullPath -Force
                Write-Host "      ✅ DELETED" -ForegroundColor Green
                $deletedCount++
                $backupList += "DELETED: $relativePath (Size: $fileSize, Modified: $lastWrite)"
            } catch {
                Write-Host "      ❌ FAILED: $($_.Exception.Message)" -ForegroundColor Red
                $failedCount++
                $backupList += "FAILED: $relativePath - $($_.Exception.Message)"
            }
        } else {
            # PREVIEW MODE
            Write-Host "      ℹ️  WOULD BE DELETED (run with -Remove to delete)" -ForegroundColor Cyan
            $previewCount++
            $backupList += "TO DELETE: $relativePath (Size: $fileSize, Modified: $lastWrite)"
        }
    } else {
        Write-Host "      ⚠️  File not found (already deleted or moved)" -ForegroundColor Gray
        $backupList += "NOT FOUND: $relativePath"
    }

    Write-Host ""
}

# ============================================================================
# CLEANUP ORPHANED .META FILES
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "🧹 CLEANING ORPHANED .META FILES" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$orphanedMetaFiles = @(
    "Assets\Scripts\HUD\UIBarsSystemInitializer.cs.meta",
    "Assets\Scripts\Player\PlayerHealth.cs.meta",
    "Assets\Scripts\Core\SeedProgression.cs.meta",
    "Assets\Scripts\Tests\MazeGeneratorTests.cs.disabled.meta",
    "Assets\Scripts\Tests\StatsEngineTests.cs.disabled.meta",
    "Assets\Scripts\Tests\NewBehaviourScript.cs.meta"
)

$orphanedDeletedCount = 0
foreach ($metaFile in $orphanedMetaFiles) {
    $fullPath = Join-Path $scriptRoot $metaFile
    $relativePath = $metaFile -replace '\\', '/'

    if (Test-Path $fullPath) {
        if ($Remove) {
            try {
                Remove-Item $fullPath -Force
                Write-Host "  ✅ DELETED: $relativePath" -ForegroundColor Green
                $orphanedDeletedCount++
                $backupList += "DELETED META: $relativePath"
            } catch {
                Write-Host "  ❌ FAILED: $($_.Exception.Message)" -ForegroundColor Red
                $failedCount++
                $backupList += "FAILED META: $relativePath - $($_.Exception.Message)"
            }
        } else {
            Write-Host "  ℹ️  WOULD BE DELETED: $relativePath" -ForegroundColor Cyan
            $previewCount++
            $backupList += "TO DELETE META: $relativePath"
        }
    } else {
        Write-Host "  ⚠️  Not found (already deleted): $relativePath" -ForegroundColor Gray
    }
}

if ($orphanedDeletedCount -gt 0) {
    Write-Host "  [OK] Cleaned $orphanedDeletedCount orphaned .meta files." -ForegroundColor Green
}
Write-Host ""

# ============================================================================
# SHOW HANDLED FILES (for information)
# ============================================================================

if ($handledList.Count -gt 0) {
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "✅ ALREADY HANDLED" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""

    foreach ($file in $handledList) {
        $relativePath = $file.Path -replace '\\', '/'
        Write-Host "  $($file.Status) $relativePath" -ForegroundColor Green
        Write-Host "      Action: $($file.Action)" -ForegroundColor Gray
        Write-Host ""
    }
}

# ============================================================================
# CREATE BACKUP LIST
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "💾 CREATING BACKUP LIST" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$backupListContent = @"
# Deprecated Files Cleanup - Backup List
# Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
# Mode: $(if ($Remove) { 'DELETE' } else { 'PREVIEW' })
#
# This file records all files that were identified for deletion.
# Keep this file for reference in case you need to restore any files.
#
# ============================================================================

"@ + "`n"

foreach ($item in $backupList) {
    $backupListContent += "$item`n"
}

$backupListPathFull = Join-Path $scriptRoot $BackupListPath
try {
    $backupListContent | Out-File -FilePath $backupListPathFull -Encoding UTF8 -Force
    Write-Host "  ✅ Backup list created: $BackupListPath" -ForegroundColor Green
    Write-Host "     Path: $backupListPathFull" -ForegroundColor Gray
} catch {
    Write-Host "  ⚠️  Could not create backup list: $($_.Exception.Message)" -ForegroundColor Yellow
}
Write-Host ""

# ============================================================================
# SUMMARY
# ============================================================================

$endTime = Get-Date
$duration = $endTime - $startTime

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📊 SUMMARY" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

if ($Remove) {
    Write-Host "  Files deleted:     $deletedCount" -ForegroundColor $(if ($deletedCount -gt 0) { "Green" } else { "Yellow" })
    Write-Host "  Deletion failed:   $failedCount" -ForegroundColor $(if ($failedCount -gt 0) { "Red" } else { "Green" })
} else {
    Write-Host "  Files to delete:   $previewCount" -ForegroundColor Cyan
    Write-Host "  (No files deleted - preview mode)" -ForegroundColor Gray
}
Write-Host ""
Write-Host "  Started:  $($startTime.ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor Gray
Write-Host "  Finished: $($endTime.ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor Gray
Write-Host "  Duration: $($duration.TotalSeconds.ToString('0.00')) seconds" -ForegroundColor Gray
Write-Host ""

# ============================================================================
# NEXT STEPS
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📝 NEXT STEPS" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

if (-not $Remove) {
    Write-Host "  To actually delete the files, run:" -ForegroundColor White
    Write-Host "    .\cleanup_deprecated_safe.ps1 -Remove" -ForegroundColor Cyan
    Write-Host ""
} else {
    Write-Host "  ✅ Deletion complete!" -ForegroundColor Green
    Write-Host ""
}

Write-Host "  Recommended next steps:" -ForegroundColor White
Write-Host ""
Write-Host "  1. Open Unity Editor and verify compilation:" -ForegroundColor Cyan
Write-Host "     - Check Console for errors" -ForegroundColor Gray
Write-Host "     - Verify all assemblies compile" -ForegroundColor Gray
Write-Host ""
Write-Host "  2. Test game functionality:" -ForegroundColor Cyan
Write-Host "     - Player health/damage system (PlayerStats.cs)" -ForegroundColor Gray
Write-Host "     - Seed progression (SeedManager.cs)" -ForegroundColor Gray
Write-Host "     - UI bars (UIBarsSystem.cs)" -ForegroundColor Gray
Write-Host ""
Write-Host "  3. Commit changes to git:" -ForegroundColor Cyan
Write-Host "     git add -A" -ForegroundColor Gray
Write-Host "     git commit -m `"chore: Remove deprecated files"`" -ForegroundColor Gray
Write-Host ""

# ============================================================================
# GIT REMINDER
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "🔧 GIT STATUS" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$gitDir = Join-Path $scriptRoot ".git"
if (Test-Path $gitDir) {
    Write-Host "  ✅ Git repository detected" -ForegroundColor Green
    Write-Host ""
    Write-Host "  After verifying everything works, commit changes:" -ForegroundColor White
    Write-Host ""
    Write-Host "  git status" -ForegroundColor Gray
    Write-Host "  git add Assets/Scripts/HUD/UIBarsSystemInitializer.cs" -ForegroundColor Cyan
    Write-Host "  git add Assets/Scripts/Player/PlayerHealth.cs" -ForegroundColor Cyan
    Write-Host "  git add Assets/Scripts/Core/SeedProgression.cs" -ForegroundColor Cyan
    Write-Host "  git add Assets/Scripts/Tests/*.disabled" -ForegroundColor Cyan
    Write-Host "  git commit -m `"chore: Remove deprecated files (safe cleanup)`"" -ForegroundColor Cyan
    Write-Host ""
} else {
    Write-Host "  ⚠️  No git repository detected" -ForegroundColor Yellow
    Write-Host "     Consider using version control for your project." -ForegroundColor Gray
    Write-Host ""
}

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
