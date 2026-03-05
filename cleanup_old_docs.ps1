# cleanup_old_docs.ps1
# Cleanup documentation files older than 2 days
# Unity 6 compatible - UTF-8 encoding - Unix line endings
#
# USAGE:
#   1. Review what will be deleted (dry run)
#   2. Run script to delete old docs
#   3. Verify cleanup completed
#
# FOLDERS CLEANED:
# - Assets/Docs/ (except *.md files < 2 days old)
# - diff_tmp/ (all files > 2 days old)
# - Logs/ (chat logs > 2 days old)
#
# SAFE MODE: Script shows what WILL be deleted before actually deleting

param(
    [switch]$WhatIf,    # Show what would be deleted without actually deleting
    [switch]$Force,     # Skip confirmation prompt
    [int]$DaysOld = 2   # Delete files older than this many days (default: 2)
)

$ErrorActionPreference = "Stop"

# ============================================================================
# CONFIGURATION
# ============================================================================

$projectRoot = $PSScriptRoot
$cutoffDate = (Get-Date).AddDays(-$DaysOld)

$foldersToClean = @(
    "Assets/Docs",
    "diff_tmp",
    "Logs"
)

# File extensions to clean
$extensionsToClean = @("*.md", "*.txt", "*.diff", "*.patch")

# Files to ALWAYS KEEP (never delete)
$protectedFiles = @(
    "TODO.md",
    "ARCHITECTURE_OVERVIEW.md",
    "ARCHITECTURE_MAP.md",
    "README.md",
    "PROJECT_STANDARDS.md",
    "VERBOSITY_GUIDE.md",
    "VERIFICATION_CHECKLIST.md",
    "TEST_CHECKLIST.md"
)

# ============================================================================
# FUNCTIONS
# ============================================================================

function Get-FilesToDelete {
    param(
        [string]$folderPath,
        [DateTime]$cutoffDate
    )
    
    $filesToDelete = @()
    
    if (Test-Path $folderPath) {
        foreach ($ext in $extensionsToClean) {
            $files = Get-ChildItem -Path $folderPath -Filter $ext -File -Recurse -ErrorAction SilentlyContinue
            foreach ($file in $files) {
                # Check if file is older than cutoff
                if ($file.LastWriteTime -lt $cutoffDate) {
                    # Check if file is protected
                    $isProtected = $false
                    foreach ($protected in $protectedFiles) {
                        if ($file.Name -eq $protected) {
                            $isProtected = $true
                            break
                        }
                    }
                    
                    if (-not $isProtected) {
                        $filesToDelete += $file
                    }
                }
            }
        }
    }
    
    return $filesToDelete
}

# ============================================================================
# MAIN SCRIPT
# ============================================================================

Write-Host "════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  CLEANUP OLD DOCUMENTATION FILES" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Configuration:" -ForegroundColor Yellow
Write-Host "  • Delete files older than: $DaysOld days" -ForegroundColor White
Write-Host "  • Cutoff date: $cutoffDate" -ForegroundColor White
Write-Host "  • Folders to clean: $($foldersToClean -join ', ')" -ForegroundColor White
Write-Host "  • Extensions: $($extensionsToClean -join ', ')" -ForegroundColor White
Write-Host ""

# Collect all files to delete
$allFilesToDelete = @()

foreach ($folder in $foldersToClean) {
    $folderPath = Join-Path $projectRoot $folder
    $files = Get-FilesToDelete -folderPath $folderPath -cutoffDate $cutoffDate
    $allFilesToDelete += $files
}

# Report what will be deleted
Write-Host "📋 FILES TO BE DELETED:" -ForegroundColor Yellow
Write-Host "────────────────────────────────────────────────────" -ForegroundColor DarkGray

if ($allFilesToDelete.Count -eq 0) {
    Write-Host "  ✅ No old files found - nothing to delete!" -ForegroundColor Green
    Write-Host ""
    Write-Host "All documentation is recent (< $DaysOld days old)" -ForegroundColor Cyan
    exit 0
} else {
    Write-Host "  Found $($allFilesToDelete.Count) files older than $DaysOld days:" -ForegroundColor White
    Write-Host ""
    
    # Group by folder
    $groupedFiles = $allFilesToDelete | Group-Object DirectoryName
    
    foreach ($group in $groupedFiles) {
        Write-Host "  📁 $($group.Name)" -ForegroundColor Cyan
        foreach ($file in $group.Group) {
            $age = (Get-Date) - $file.LastWriteTime
            Write-Host "     - $($file.Name) ($([math]::Round($age.TotalDays, 1)) days old)" -ForegroundColor Gray
        }
        Write-Host ""
    }
}

Write-Host "════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SUMMARY: $($allFilesToDelete.Count) files will be deleted" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# WhatIf mode - just show what would be deleted
if ($WhatIf) {
    Write-Host "🔍 WHAT-IF MODE - No files were deleted" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To actually delete these files, run:" -ForegroundColor Cyan
    Write-Host "  .\cleanup_old_docs.ps1" -ForegroundColor White
    Write-Host ""
    Write-Host "To skip confirmation, run:" -ForegroundColor Cyan
    Write-Host "  .\cleanup_old_docs.ps1 -Force" -ForegroundColor White
    exit 0
}

# Ask for confirmation
if (-not $Force) {
    Write-Host "⚠️  WARNING: This action will permanently delete $($allFilesToDelete.Count) files!" -ForegroundColor Red
    Write-Host ""
    $confirmation = Read-Host "Are you sure you want to delete these files? (yes/no)"
    
    if ($confirmation -ne "yes") {
        Write-Host ""
        Write-Host "❌ Deletion cancelled by user" -ForegroundColor Yellow
        exit 0
    }
}

Write-Host ""
Write-Host "🗑️  DELETING FILES..." -ForegroundColor Yellow
Write-Host "────────────────────────────────────────────────────" -ForegroundColor DarkGray

$deletedCount = 0
$failedCount = 0
$skippedCount = 0

foreach ($file in $allFilesToDelete) {
    try {
        # Check if file is read-only (backup files are read-only)
        if ($file.IsReadOnly) {
            Write-Host "  ⏭️  Skipped (read-only): $($file.Name)" -ForegroundColor DarkGray
            $skippedCount++
            continue
        }
        
        Remove-Item -Path $file.FullName -Force -ErrorAction Stop
        Write-Host "  ✅ Deleted: $($file.Name)" -ForegroundColor Green
        $deletedCount++
    } catch {
        Write-Host "  ❌ Failed to delete: $($file.Name)" -ForegroundColor Red
        Write-Host "     Error: $($_.Exception.Message)" -ForegroundColor DarkGray
        $failedCount++
    }
}

Write-Host ""
Write-Host "════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  CLEANUP COMPLETE" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  ✅ Successfully deleted: $deletedCount files" -ForegroundColor Green
if ($skippedCount -gt 0) {
    Write-Host "  ⏭️  Skipped (read-only): $skippedCount files" -ForegroundColor Yellow
}
if ($failedCount -gt 0) {
    Write-Host "  ❌ Failed to delete: $failedCount files" -ForegroundColor Red
}
Write-Host ""

# Reminders
Write-Host "════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  ⚠️  IMPORTANT NEXT STEPS:" -ForegroundColor Yellow
Write-Host "════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  1. Run backup.ps1 to backup the changes:" -ForegroundColor White
Write-Host "     .\backup.ps1" -ForegroundColor Cyan
Write-Host ""
Write-Host "  2. Check Unity Console for any issues" -ForegroundColor White
Write-Host ""

Write-Host "✅ Done!" -ForegroundColor Green
