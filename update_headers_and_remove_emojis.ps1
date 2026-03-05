# update_headers_and_remove_emojis.ps1
# Updates GPL license headers and removes emojis from all C# files
# Unity 6 compatible - UTF-8 encoding - Unix line endings

param(
    [switch]$WhatIf  # Show what would be changed without making changes
)

$projectRoot = $PSScriptRoot
$scriptsFolder = Join-Path $projectRoot "Assets\Scripts"

# Counter for statistics
$totalFiles = 0
$modifiedFiles = 0
$headerUpdated = 0
$emojisRemoved = 0

# New GPL header template (without emojis, with correct project name)
$newGplHeader = @'
// Copyright (C) 2026 Ocxyde
//
// This file is part of CodeDotLavos.
//
// CodeDotLavos is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// CodeDotLavos is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with CodeDotLavos.  If not, see <https://www.gnu.org/licenses/>.
'@

# List of emoji characters to remove (explicit list for reliability)
# This covers all emojis found in the codebase
$emojiList = '🎮|⚔️|✅|❌|⚠️|📦|🔥|💾|👤|🌍|🏛️|🧱|🚪|🔨|💡|📋|🏗️|🧹|🎯|🎲|📄|📂|💿|🔧|🎨|📁|📝|🔄|📊|📈|📉|🔒|🔓|🌟|✨|🎪|🎭|🎬|🎵|🎶|🎸|🎺|🎻|🥁|🎹|🎼|🎤|🎧|🎷|🎳|🎱|🎰|ℹ️|⏳|⏸'

# Build regex pattern - wrap in group for matching
$emojiPattern = "($emojiList)"

# Function to remove emojis from a line
function Remove-Emojis {
    param([string]$line)
    
    # Remove emoji characters using regex
    $result = $line -replace $emojiPattern, ''
    
    # Clean up any double spaces that might result
    $result = $result -replace '  +', ' '
    
    # Trim trailing whitespace
    $result = $result.TrimEnd()
    
    return $result
}

# Function to check if line contains emojis
function Test-HasEmoji {
    param([string]$line)
    return ($line -match $emojiPattern)
}

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  Update GPL Headers & Remove Emojis" -ForegroundColor Cyan
Write-Host "  Project: CodeDotLavos" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

# Get all C# files recursively
$csFiles = Get-ChildItem -Path $scriptsFolder -Filter "*.cs" -Recurse -File

Write-Host "Found $($csFiles.Count) C# files to process..." -ForegroundColor Yellow
Write-Host ""

foreach ($file in $csFiles) {
    $totalFiles++
    $filePath = $file.FullName
    $relativePath = $filePath.Replace($projectRoot, "").TrimStart('\')
    
    # Read file content with proper encoding
    try {
        $content = [System.IO.File]::ReadAllText($filePath, [System.Text.UTF8Encoding]::new($false))
    }
    catch {
        Write-Host "  [ERROR] Cannot read $relativePath : $_" -ForegroundColor Red
        continue
    }
    
    # Normalize line endings to LF for consistent processing
    $content = $content -replace "`r`n", "`n"
    $lines = $content.Split("`n")
    
    $fileModified = $false
    $headerWasUpdated = $false
    $emojiCountInFile = 0
    $hasOldHeader = $false
    
    # Check if file has old GPL header with "PeuImporte"
    if ($content -match "This file is part of PeuImporte") {
        $hasOldHeader = $true
        $headerWasUpdated = $true
        $headerUpdated++
        $fileModified = $true
        Write-Host "  [HEADER] $relativePath" -ForegroundColor Orange
    }
    
    # Process each line to remove emojis from comments
    $newLines = @()
    $skipHeaderLines = 0
    
    for ($i = 0; $i -lt $lines.Length; $i++) {
        $line = $lines[$i]
        
        # Skip old header lines if we're replacing the header
        if ($hasOldHeader -and $skipHeaderLines -gt 0) {
            $skipHeaderLines--
            continue
        }
        
        # Detect start of old GPL header and skip it (will be replaced with new one)
        if ($hasOldHeader -and $line -match "^// Copyright \(C\) \d{4}") {
            # Check if next lines contain "PeuImporte" to confirm it's the old header
            if ($i + 1 -lt $lines.Length -and $lines[$i + 1] -match "^//$") {
                $skipHeaderLines = 16  # Skip remaining 16 lines of old 17-line header
                continue
            }
        }
        
        # Process comment lines to remove emojis
        if ($line.TrimStart() -match "^//") {
            if (Test-HasEmoji -line $line) {
                $cleanedLine = Remove-Emojis -line $line
                $newLines += $cleanedLine
                $emojiCountInFile++
                $fileModified = $true
                
                # Show first few emoji removals per file
                if ($emojiCountInFile -le 3) {
                    Write-Host "    - Removed emoji: $($line.Trim()) => $($cleanedLine.Trim())" -ForegroundColor Gray
                }
            }
            else {
                $newLines += $line
            }
        }
        else {
            # Non-comment line, keep as-is
            $newLines += $line
        }
    }
    
    if ($emojiCountInFile -gt 0) {
        $emojisRemoved += $emojiCountInFile
        if ($emojiCountInFile -gt 3) {
            Write-Host "    ... and $($emojiCountInFile - 3) more emojis removed" -ForegroundColor Gray
        }
    }
    
    # Write modified content back to file
    if ($fileModified) {
        if ($WhatIf) {
            Write-Host "  [WOULD UPDATE] $relativePath" -ForegroundColor Cyan
        }
        else {
            try {
                if ($headerWasUpdated) {
                    # Build final content with new header
                    $finalContent = $newGplHeader + "`n"
                    
                    # Add remaining lines (after skipped header)
                    $finalContent += ($newLines -join "`n")
                }
                else {
                    # Only emojis removed, no header change
                    $finalContent = ($newLines -join "`n")
                }
                
                # Write with Unix line endings (LF) and UTF-8 encoding (no BOM)
                [System.IO.File]::WriteAllText($filePath, $finalContent, [System.Text.UTF8Encoding]::new($false))
                
                $modifiedFiles++
            }
            catch {
                Write-Host "  [ERROR] Cannot write $relativePath : $_" -ForegroundColor Red
            }
        }
    }
}

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  Summary" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  Total C# files scanned:  $totalFiles" -ForegroundColor White
Write-Host "  Files modified:          $modifiedFiles" -ForegroundColor Green
Write-Host "  Headers updated:         $headerUpdated" -ForegroundColor Green
Write-Host "  Total emojis removed:    $emojisRemoved" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

if ($WhatIf) {
    Write-Host "  [DRY RUN] No files were actually modified." -ForegroundColor Yellow
    Write-Host "  Run without -WhatIf to apply changes." -ForegroundColor Yellow
}
else {
    Write-Host "  Cleanup completed!" -ForegroundColor Green
}

Write-Host ""
Write-Host "  Remember to run backup.ps1 after making changes!" -ForegroundColor Yellow
Write-Host ""
