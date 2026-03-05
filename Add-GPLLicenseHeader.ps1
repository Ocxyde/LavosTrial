# Add-GPLLicenseHeader.ps1
# Adds GPL-3.0 license header to all C# files in the project
# Unity 6 compatible - UTF-8 encoding - Unix line endings
#
# USAGE:
#   .\Add-GPLLicenseHeader.ps1
#
# This script adds the following header to all .cs files:
# // Copyright (C) 2026 CodeDotLavos
# //
# // This file is part of CodeDotLavos.
# //
# // CodeDotLavos is free software: you can redistribute it and/or modify
# // it under the terms of the GNU General Public License as published by
# // the Free Software Foundation, either version 3 of the License, or
# // (at your option) any later version.
# //
# // CodeDotLavos is distributed in the hope that it will be useful,
# // but WITHOUT ANY WARRANTY; without even the implied warranty of
# // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# // GNU General Public License for more details.
# //
# // You should have received a copy of the GNU General Public License
# // along with CodeDotLavos.  If not, see <https://www.gnu.org/licenses/>.

param(
    [string]$ProjectRoot = "D:\travaux_Unity\PeuImporte",
    [switch]$WhatIf  # Show what would be done without actually doing it
)

$ErrorActionPreference = "Stop"

# GPL-3.0 Header Template
$gplHeader = @'
// Copyright (C) 2026 CodeDotLavos
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

# Folders to exclude (backup, temp, etc.)
$excludeFolders = @("obj", "bin", "Library", "Temp", "Backup_Solution", "Backup", "backups")

# Find all C# files (excluding backup/temp folders)
$csFiles = Get-ChildItem -Path $ProjectRoot -Filter "*.cs" -Recurse | 
    Where-Object { 
        $path = $_.FullName
        $excludeFolders | ForEach-Object { 
            if ($path -like "*\$_\*") { return $false }
        }
        return $true
    }

Write-Host "═══════════════════════════════════════════"
Write-Host "  ADD GPL-3.0 LICENSE HEADERS"
Write-Host "═══════════════════════════════════════════"
Write-Host ""
Write-Host "Found $($csFiles.Count) C# files to process"
Write-Host ""

$processedCount = 0
$skippedCount = 0

foreach ($file in $csFiles) {
    try {
        $content = Get-Content $file.FullName -Raw -Encoding UTF8
        
        # Check if file already has GPL header
        if ($content.StartsWith("// Copyright (C)")) {
            Write-Host "⏭️  Skipped (already licensed): $($file.Name)" -ForegroundColor Yellow
            $skippedCount++
            continue
        }
        
        if ($WhatIf) {
            Write-Host "📝 Would add header: $($file.FullName)" -ForegroundColor Cyan
            $processedCount++
            continue
        }
        
        # Add header to file
        $newContent = $gplHeader + $content
        Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8 -NoNewline
        
        Write-Host "✅ Added header: $($file.Name)" -ForegroundColor Green
        $processedCount++
    }
    catch {
        Write-Host "⚠️  Skipped (read-only or access denied): $($file.Name)" -ForegroundColor DarkGray
        $skippedCount++
    }
}

Write-Host ""
Write-Host "═══════════════════════════════════════════"
Write-Host "  COMPLETE"
Write-Host "═══════════════════════════════════════════"
Write-Host "  ✅ Files processed: $processedCount"
Write-Host "  ⏭️  Files skipped: $skippedCount"
Write-Host "═══════════════════════════════════════════"
Write-Host ""

if ($WhatIf) {
    Write-Host "ℹ️  WHAT-IF MODE - No files were modified" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To actually add headers, run:" -ForegroundColor Cyan
    Write-Host "  .\Add-GPLLicenseHeader.ps1" -ForegroundColor White
}
