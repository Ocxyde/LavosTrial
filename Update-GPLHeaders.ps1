# Copyright (C) 2026 Ocxyde
# GPL-3.0 License
#
# Update-GPLHeaders.ps1
# Batch update GPL license headers from "PeuImporte" to "Code.Lavos"
# Unity 6 compatible - UTF-8 encoding - Unix LF
#
# USAGE:
#   .\Update-GPLHeaders.ps1
#
# WHAT IT DOES:
#   - Scans all .cs files in Assets/Scripts/
#   - Replaces "PeuImporte" with "Code.Lavos" in GPL headers
#   - Preserves file encoding (UTF-8) and line endings (Unix LF)
#   - Creates backup in diff_tmp/ before changes
#   - Shows diff of all changes
#
# LOCATION: Project root (CodeDotLavos/)

param(
    [switch]$WhatIf,      # Show what would change without modifying
    [switch]$Verbose      # Show detailed output
)

# Configuration
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$scriptsFolder = Join-Path $scriptPath "Assets\Scripts"
$diffFolder = Join-Path $scriptPath "diff_tmp"
$backupFile = Join-Path $diffFolder "gpl_headers_backup_$(Get-Date -Format 'yyyyMMdd_HHmmss').txt"

# Ensure diff_tmp folder exists
if (-not (Test-Path $diffFolder)) {
    Write-Host "[Update-GPLHeaders] Creating diff_tmp folder..." -ForegroundColor Cyan
    New-Item -ItemType Directory -Path $diffFolder -Force | Out-Null
}

# Initialize counters
$totalFiles = 0
$modifiedFiles = 0
$errorCount = 0
$changes = @()

Write-Host "════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  GPL HEADER UPDATE: PeuImporte → Code.Lavos   " -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Get all C# files
Write-Host "[Update-GPLHeaders] Scanning for .cs files in Assets/Scripts/..." -ForegroundColor Cyan
$csFiles = Get-ChildItem -Path $scriptsFolder -Filter "*.cs" -Recurse -File

Write-Host "[Update-GPLHeaders] Found $($csFiles.Count) C# files" -ForegroundColor Cyan
Write-Host ""

# Process each file
foreach ($file in $csFiles) {
    $totalFiles++
    
    if ($Verbose) {
        Write-Host "  Checking: $($file.Name)" -ForegroundColor Gray
    }
    
    try {
        # Read file content
        $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
        
        # Check if file contains "PeuImporte" in GPL header
        if ($content -match "This file is part of PeuImporte") {
            $modifiedFiles++
            
            # Store original for backup
            $originalContent = $content
            
            # Replace "PeuImporte" with "Code.Lavos" (case-sensitive)
            $newContent = $content -replace 'This file is part of PeuImporte\.', 'This file is part of Code.Lavos.'
            $newContent = $newContent -replace 'PeuImporte is free software:', 'Code.Lavos is free software:'
            $newContent = $newContent -replace 'PeuImporte is distributed', 'Code.Lavos is distributed'
            $newContent = $newContent -replace 'along with PeuImporte\.', 'along with Code.Lavos.'
            
            # Store change for reporting
            $relativePath = $file.FullName.Replace($scriptPath, '').TrimStart('\')
            $changes += [PSCustomObject]@{
                File = $relativePath
                Changes = 4  # 4 replacements per file
            }
            
            if ($WhatIf) {
                Write-Host "  [WHAT-IF] Would update: $($file.Name)" -ForegroundColor Yellow
            } else {
                # Write new content with Unix LF line endings
                $newContent = $newContent.Replace("`r`n", "`n")
                Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8 -NoNewline
                
                if ($Verbose) {
                    Write-Host "  ✓ Updated: $($file.Name)" -ForegroundColor Green
                }
            }
        }
    }
    catch {
        $errorCount++
        Write-Host "  ✗ Error processing $($file.Name): $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Write backup log
if (-not $WhatIf) {
    $backupLog = @"
GPL Header Update Backup Log
============================
Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
Total Files Scanned: $totalFiles
Files Modified: $modifiedFiles
Errors: $errorCount

Modified Files:
"@
    
    foreach ($change in $changes) {
        $backupLog += "`n  - $($change.File) ($($change.Changes) replacements)"
    }
    
    $backupLog += "`n`nOriginal Project Name: PeuImporte"
    $backupLog += "`nNew Project Name: Code.Lavos"
    $backupLog += "`n`nBackup created by Update-GPLHeaders.ps1"
    
    $backupLog | Out-File -FilePath $backupFile -Encoding UTF8
    
    Write-Host ""
    Write-Host "[Update-GPLHeaders] Backup log saved to: $backupFile" -ForegroundColor Cyan
}

# Summary
Write-Host ""
Write-Host "════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  SUMMARY                                      " -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Total Files Scanned:  $totalFiles" -ForegroundColor White
Write-Host "  Files Modified:       $modifiedFiles" -ForegroundColor $(if ($modifiedFiles -gt 0) { 'Green' } else { 'Gray' })
Write-Host "  Errors:               $errorCount" -ForegroundColor $(if ($errorCount -gt 0) { 'Red' } else { 'Green' })
Write-Host "════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

if ($WhatIf) {
    Write-Host "[Update-GPLHeaders] WHAT-IF mode - No files were modified" -ForegroundColor Yellow
    Write-Host "[Update-GPLHeaders] Run without -WhatIf to apply changes" -ForegroundColor Yellow
} else {
    if ($modifiedFiles -gt 0) {
        Write-Host "[Update-GPLHeaders] ✓ Successfully updated $modifiedFiles files!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Modified Files:" -ForegroundColor Cyan
        foreach ($change in $changes) {
            Write-Host "  - $($change.File)" -ForegroundColor Gray
        }
        Write-Host ""
        Write-Host "[REMINDER] Run backup.ps1 to save these changes!" -ForegroundColor Yellow
    } else {
        Write-Host "[Update-GPLHeaders] No files needed updating (already up-to-date?)" -ForegroundColor Green
    }
}

Write-Host ""
