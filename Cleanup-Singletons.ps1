# Cleanup-Singletons.ps1
# SAFE automated script to remove singleton Instance properties
# Unity 6 compatible - UTF-8 encoding - Unix line endings
#
# NOTE: Run backup.ps1 before this script if you want backups!
#
# USAGE:
#   1. Close Unity Editor
#   2. Run: .\backup.ps1   (YOUR backup system - RECOMMENDED)
#   3. Run: .\Cleanup-Singletons.ps1 -WhatIf   (dry run - SAFE)
#   4. Review what will change
#   5. Run: .\Cleanup-Singletons.ps1   (actual cleanup)
#   6. Reopen Unity Editor
#   7. Verify code compiles without errors
#
# Location: Project Root

param(
    [switch]$WhatIf           # Show what would be changed without making changes
    # Note: No -NoBackup option - use backup.ps1 before running if you want backups
)

$ErrorActionPreference = "Stop"

# Files to clean (exact paths)
$filesToClean = @(
    "Assets\Scripts\Core\01_CoreSystems\GameManager.cs",
    "Assets\Scripts\Core\02_Player\PlayerStats.cs",
    "Assets\Scripts\Core\10_Resources\SeedManager.cs",
    "Assets\Scripts\Core\05_Combat\CombatSystem.cs",
    "Assets\Scripts\Core\04_Inventory\Inventory.cs"
)

Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SINGLETON CLEANUP SCRIPT - SAFE MODE" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Check if files exist first
Write-Host "Checking files..." -ForegroundColor Cyan
$missingFiles = @()
foreach ($file in $filesToClean) {
    $fullPath = Join-Path $PSScriptRoot $file
    if (-not (Test-Path $fullPath)) {
        $missingFiles += $file
    }
}

if ($missingFiles.Count -gt 0) {
    Write-Host ""
    Write-Host "⚠️  ERROR: Missing files:" -ForegroundColor Red
    foreach ($file in $missingFiles) {
        Write-Host "   - $file" -ForegroundColor Red
    }
    Write-Host ""
    Write-Host "Please verify file paths and try again." -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ All files found" -ForegroundColor Green
Write-Host ""

$totalFiles = 0
$totalLinesRemoved = 0

foreach ($file in $filesToClean) {
    $fullPath = Join-Path $PSScriptRoot $file
    
    Write-Host "Processing: $file" -ForegroundColor Cyan
    
    try {
        $content = Get-Content $fullPath -Raw -Encoding UTF8
        $originalLines = ($content -split "`n").Count
        $modifiedContent = $content
        $changesMade = $false
        
        # CONSERVATIVE APPROACH: Only remove specific known patterns
        
        # Pattern 1: Remove [Obsolete] attribute lines
        $obsoletePattern = '\[System\.Obsolete\("Use EventHandler events instead of direct singleton access\. Deprecated: 2026-03-03"\)\]'
        $matches = [regex]::Matches($modifiedContent, $obsoletePattern)
        if ($matches.Count -gt 0) {
            Write-Host "  Found $($matches.Count) [Obsolete] attribute(s)" -ForegroundColor Gray
            if (-not $WhatIf) {
                $modifiedContent = [regex]::Replace($modifiedContent, $obsoletePattern + '\s*', '')
                $changesMade = $true
            }
        }
        
        # Pattern 2: Remove public static Instance property lines
        $instancePattern = 'public\s+static\s+\w+\s+Instance\s*\{\s*get;\s*private\s+set;\s*\}'
        $matches = [regex]::Matches($modifiedContent, $instancePattern)
        if ($matches.Count -gt 0) {
            Write-Host "  Found $($matches.Count) Instance property(ies)" -ForegroundColor Gray
            if (-not $WhatIf) {
                $modifiedContent = [regex]::Replace($modifiedContent, $instancePattern + '\s*', '')
                $changesMade = $true
            }
        }
        
        # Pattern 3: Remove cached singleton fields
        $cachedPattern = 'private\s+static\s+\w+\s+_instance;\s*private\s+static\s+bool\s+_instanceChecked\s*=\s*false;'
        $matches = [regex]::Matches($modifiedContent, $cachedPattern)
        if ($matches.Count -gt 0) {
            Write-Host "  Found $($matches.Count) cached singleton field(s)" -ForegroundColor Gray
            if (-not $WhatIf) {
                $modifiedContent = [regex]::Replace($modifiedContent, $cachedPattern + '\s*', '')
                $changesMade = $true
            }
        }
        
        # Pattern 4: Remove [REMOVED] comments
        $removedPattern = '//\s*\[REMOVED.*?Use\s+EventHandler.*?(\r?\n)'
        $matches = [regex]::Matches($modifiedContent, $removedPattern)
        if ($matches.Count -gt 0) {
            Write-Host "  Found $($matches.Count) [REMOVED] comment(s)" -ForegroundColor Gray
            if (-not $WhatIf) {
                $modifiedContent = [regex]::Replace($modifiedContent, $removedPattern, '')
                $changesMade = $true
            }
        }
        
        # Pattern 5: Remove XML summary blocks about singletons
        $summaryPattern = '///\s*<summary>.*?Singleton.*?</summary>'
        $matches = [regex]::Matches($modifiedContent, $summaryPattern)
        if ($matches.Count -gt 0) {
            Write-Host "  Found $($matches.Count) singleton summary comment(s)" -ForegroundColor Gray
            if (-not $WhatIf) {
                $modifiedContent = [regex]::Replace($modifiedContent, $summaryPattern, '', [System.Text.RegularExpressions.RegexOptions]::Singleline)
                $changesMade = $true
            }
        }
        
        $newLines = ($modifiedContent -split "`n").Count
        $linesRemoved = $originalLines - $newLines
        
        if ($linesRemoved -gt 0 -or $changesMade) {
            $totalLinesRemoved += $linesRemoved
            $totalFiles++
            
            if (-not $WhatIf) {
                # Save with Unix line endings and UTF-8 with BOM (Unity standard)
                $modifiedContent = $modifiedContent -replace "`r`n", "`n"
                [System.IO.File]::WriteAllText($fullPath, $modifiedContent, [System.Text.UTF8Encoding]::new($true))
                Write-Host "  ✅ Removed $linesRemoved lines" -ForegroundColor Green
            } else {
                Write-Host "  ℹ️  Would remove $linesRemoved lines (WhatIf mode)" -ForegroundColor Yellow
            }
        } else {
            Write-Host "  ℹ️  No changes needed" -ForegroundColor Gray
        }
    }
    catch {
        Write-Host "  ❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
        continue
    }
    
    Write-Host ""
}

Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  CLEANUP SUMMARY" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Files processed: $totalFiles" -ForegroundColor White
Write-Host "Total lines removed: $totalLinesRemoved" -ForegroundColor White
Write-Host ""

if ($WhatIf) {
    Write-Host "ℹ️  WhatIf mode - no changes made" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To apply changes, run:" -ForegroundColor Cyan
    Write-Host "  .\Cleanup-Singletons.ps1" -ForegroundColor White
} else {
    Write-Host "✅ Cleanup complete!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "1. Open Unity Editor" -ForegroundColor White
    Write-Host "2. Check Console for errors (should be 0)" -ForegroundColor White
    Write-Host "3. Test your game" -ForegroundColor White
}

Write-Host ""
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
