# Remove-EmojiFromCsFiles.ps1
# Removes all emoji from .cs files
# Unity 6 compatible - Unix LF, UTF-8 encoding

param(
    [string]$SearchPath = ".",
    [switch]$WhatIf
)

$ErrorActionPreference = "Stop"

# Emoji pattern: surrogate pairs (4-byte emoji) + common symbol emoji
$combinedPattern = "[\uD800-\uDBFF][\uDC00-\uDFFF]|[\u2600-\u26FF\u2700-\u27BF\u231A\u231B\u23E9-\u23F3\u23F8-\u23FA\u25AA\u25AB\u25B6\u25C0\u25FB-\u25FE\u2614\u2615\u2648-\u2653\u267F\u2693\u26A1\u26AA\u26AB\u26BD\u26BE\u26C4\u26C5\u26CE\u26D4\u26EA\u26F2\u26F3\u26F5\u26FA\u26FD\u2702\u2705\u2708-\u270D\u270F\u2712\u2714\u2716\u271D\u2721\u2728\u2733\u2734\u2744\u2747\u274C\u274E\u2753-\u2755\u2757\u2763\u2764\u2795-\u2797\u27A1\u27B0\u27BF\u2934\u2935\u2B05-\u2B07\u2B1B\u2B1C\u2B50\u2B55\u3030\u303D\u3297\u3299]"

# Skip backup and read-only directories
$excludeDirs = @("Backup_Solution", "Backup", "backup", "*_backup")
$csFiles = Get-ChildItem -Path $SearchPath -Filter "*.cs" -Recurse -File | Where-Object {
    $exclude = $false
    foreach ($ex in $excludeDirs) {
        if ($_.FullName -like "*\$ex\*") { $exclude = $true; break }
    }
    return -not $exclude -and -not $_.IsReadOnly
}
$modifiedCount = 0
$removedEmoji = @{}

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  EMOJI REMOVAL SCRIPT" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

foreach ($file in $csFiles) {
    $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
    
    # Find all emoji in the file
    $matches = [regex]::Matches($content, $combinedPattern)
    
    if ($matches.Count -gt 0) {
        foreach ($match in $matches) {
            $emoji = $match.Value
            if (-not $removedEmoji.ContainsKey($emoji)) {
                $removedEmoji[$emoji] = 0
            }
            $removedEmoji[$emoji]++
        }
        
        # Remove emoji
        $content = [regex]::Replace($content, $combinedPattern, "")
        
        if ($WhatIf) {
            Write-Host "[DRY RUN] Would modify: $($file.FullName)" -ForegroundColor Yellow
            Write-Host "          Emoji found: $($matches.Count)" -ForegroundColor Yellow
        } else {
            # Save with Unix LF and UTF-8
            [System.IO.File]::WriteAllText(
                $file.FullName,
                $content,
                [System.Text.Encoding]::UTF8
            )
            Write-Host "[MODIFIED] $($file.FullName)" -ForegroundColor Green
            Write-Host "           Emoji removed: $($matches.Count)" -ForegroundColor Green
        }
        
        $modifiedCount++
    }
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  SUMMARY" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

if ($removedEmoji.Count -gt 0) {
    Write-Host ""
    Write-Host "Emoji removed:" -ForegroundColor Yellow
    foreach ($kvp in $removedEmoji.GetEnumerator()) {
        $chars = $kvp.Key.ToCharArray()
        if ($chars.Length -eq 2) {
            # Surrogate pair - calculate actual code point
            $high = [int]$chars[0]
            $low = [int]$chars[1]
            $codePoint = 0x10000 + (($high -band 0x03FF) -shl 10) + ($low -band 0x03FF)
            $hexStr = $codePoint.ToString("X4")
            Write-Host "  '$($kvp.Key)' -> U+$hexStr (count: $($kvp.Value))" -ForegroundColor Yellow
        } else {
            $codePoint = [int]$chars[0]
            $hexStr = $codePoint.ToString("X4")
            Write-Host "  '$($kvp.Key)' -> U+$hexStr (count: $($kvp.Value))" -ForegroundColor Yellow
        }
    }
}

Write-Host ""
Write-Host "Files modified: $modifiedCount / $($csFiles.Count)" -ForegroundColor Cyan
Write-Host ""

if ($WhatIf) {
    Write-Host "WARNING: DRY RUN - No files were actually modified!" -ForegroundColor Yellow
    Write-Host "         Run without -WhatIf to apply changes." -ForegroundColor Yellow
} else {
    Write-Host "Done! Remember to run backup.ps1" -ForegroundColor Green
}

Write-Host ""
