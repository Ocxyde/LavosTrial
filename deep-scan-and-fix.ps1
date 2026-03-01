# deep-scan-and-fix.ps1
# Deep scan all C# files and fix compilation issues
# UTF-8 encoding - Unix line endings
#
# Usage: powershell -ExecutionPolicy Bypass -File deep-scan-and-fix.ps1

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Deep Scan & Fix All Files" -ForegroundColor White
Write-Host "  Checking for compilation issues" -ForegroundColor DarkGray
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$issues = @()
$fixedCount = 0

# Get all C# files
$csFiles = Get-ChildItem -Path "Assets\Scripts" -Filter "*.cs" -Recurse

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $originalContent = $content
    $fileName = $file.Name
    
    Write-Host "Checking: $fileName" -ForegroundColor Gray
    
    # 1. Remove leading blank lines
    $content = $content -replace "^\r?\n+", ''
    
    # 2. Remove duplicate headers
    $content = $content -replace "^(// .*?\.cs\r?\n// .*?\r?\n// .*?\r?\n// .*?\r?\n.*?\r?\n)\r?\n// .*?\.cs\r?\n// .*?\r?\n// .*?\r?\n// .*?\r?\n.*?\r?\n", '$1'
    
    # 3. Check for missing closing braces (simple bracket count)
    $openBraces = ([regex]::Matches($content, "\{")).Count
    $closeBraces = ([regex]::Matches($content, "\}")).Count
    
    if ($openBraces -ne $closeBraces) {
        Write-Host "  [!] Bracket mismatch in $fileName : Open=$openBraces, Close=$closeBraces" -ForegroundColor Yellow
        $issues += "BRACKET MISMATCH: $fileName (Open: $openBraces, Close: $closeBraces)"
    }
    
    # 4. Check for namespace declaration
    if ($content -notmatch "namespace\s+[A-Za-z0-9_.]+") {
        Write-Host "  [!] Missing namespace in $fileName" -ForegroundColor Yellow
        $issues += "MISSING NAMESPACE: $fileName"
    }
    
    # 5. Check for class/interface/enum/struct declaration
    if ($content -notmatch "(class|interface|enum|struct|record)\s+[A-Za-z0-9_]+") {
        # Might be a file with only using statements or comments
        if ($content -match "^(//|using|namespace|\{|\})") {
            Write-Host "  [!] No type declaration in $fileName" -ForegroundColor Yellow
            $issues += "NO TYPE DECLARATION: $fileName"
        }
    }
    
    # 6. Check for common missing using directives
    if ($content -match "StatusEffectData|StatsEngine|DamageType|ModifierType|EffectType" -and 
        $content -notmatch "using\s+Code\.Lavos\.Status") {
        # Add using directive after existing using statements
        $content = $content -replace "(using\s+Code\.Lavos;)", "`$1`r`nusing Code.Lavos.Status;"
        Write-Host "  [FIX] Added Code.Lavos.Status using in $fileName" -ForegroundColor Green
        $fixedCount++
    }
    
    # 7. Check for MonoBehaviour without UnityEngine
    if ($content -match ": MonoBehaviour" -and $content -notmatch "using\s+UnityEngine") {
        $issues += "MISSING USING: $fileName needs UnityEngine"
    }
    
    # 8. Normalize line endings
    $content = $content -replace "`r`n", "`n"
    
    # Write back if changed
    if ($content -ne $originalContent) {
        try {
            [System.IO.File]::WriteAllText($file.FullName, $content, [System.Text.UTF8Encoding]::new($false))
            Write-Host "  [UPDATED] $fileName" -ForegroundColor Green
            $fixedCount++
        }
        catch {
            Write-Host "  [ERROR] Failed to write $fileName : $_" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Scan Summary" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

if ($issues.Count -gt 0) {
    Write-Host "Issues Found ($($issues.Count)):" -ForegroundColor Yellow
    foreach ($issue in $issues) {
        Write-Host "  [!] $issue" -ForegroundColor Yellow
    }
    Write-Host ""
} else {
    Write-Host "  No critical issues found!" -ForegroundColor Green
}

Write-Host ""
Write-Host "Files updated: $fixedCount" -ForegroundColor Green
Write-Host ""
Write-Host "Next: Run backup" -ForegroundColor Yellow
Write-Host "  powershell -ExecutionPolicy Bypass -File apply-patches-and-backup.ps1" -ForegroundColor Gray
Write-Host ""
