# fix-meta-files.ps1
# Fix corrupted .meta files
# UTF-8 encoding - Unix line endings

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Fix Corrupted Meta Files" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Find all .meta files that might be corrupted
$metaFiles = Get-ChildItem -Path "Assets" -Filter "*.meta" -Recurse

$fixedCount = 0

foreach ($meta in $metaFiles) {
    $content = Get-Content $meta.FullName -Raw -Encoding UTF8
    
    # Check if meta file has valid GUID
    if ($content -notmatch "guid:\s*[a-f0-9]{32}") {
        # Generate new GUID
        $newGuid = [System.Guid]::NewGuid().ToString("N")
        
        # Try to fix the guid line
        if ($content -match "guid:\s*[^`r`n]+") {
            $content = $content -replace "guid:\s*[^`r`n]+", "guid: $newGuid"
            [System.IO.File]::WriteAllText($meta.FullName, $content, [System.Text.UTF8Encoding]::new($false))
            Write-Host "  [FIXED] $($meta.Name)" -ForegroundColor Green
            $fixedCount++
        }
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Meta Files Fixed" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Files fixed: $fixedCount" -ForegroundColor Green
Write-Host ""
Write-Host "Now restart Unity Editor" -ForegroundColor Yellow
Write-Host ""
