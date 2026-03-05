# Generate Diff Files for Recent Changes
# Unity 6 compatible - UTF-8 encoding - Unix LF
# Run this script to generate diff files in diff_tmp/ folder

$ErrorActionPreference = "Stop"
$projectRoot = "D:\travaux_Unity\PeuImporte"
$diffFolder = Join-Path $projectRoot "diff_tmp"
$backupFolder = Join-Path $projectRoot "Backup_Solution"

Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  GENERATING DIFF FILES" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan

# Create diff_tmp folder if it doesn't exist
if (-not (Test-Path $diffFolder)) {
    Write-Host "📁 Creating diff_tmp folder..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $diffFolder | Out-Null
}

# List of modified files (relative to project root)
$modifiedFiles = @(
    "Assets\Scripts\Core\06_Maze\CompleteMazeBuilder.cs",
    "Assets\Scripts\Core\06_Maze\GameConfig.cs",
    "Assets\Scripts\Core\06_Maze\MazeSaveData.cs",
    "Assets\Scripts\Core\06_Maze\MazeConsoleCommands.cs",
    "Assets\Scripts\Core\10_Resources\LightPlacementEngine.cs",
    "Assets\Scripts\Core\12_Compute\LightEngine.cs",
    "Config\GameConfig-default.json",
    "Assets\Docs\TODO.md",
    "Assets\Docs\VERBOSITY_GUIDE.md"
)

$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$diffCount = 0

foreach ($file in $modifiedFiles) {
    $filePath = Join-Path $projectRoot $file
    $fileName = Split-Path $file -Leaf
    $fileBaseName = [System.IO.Path]::GetFileNameWithoutExtension($fileName)
    $fileExtension = [System.IO.Path]::GetExtension($fileName)
    
    Write-Host ""
    Write-Host "───────────────────────────────────────────" -ForegroundColor Gray
    Write-Host "📄 Processing: $file" -ForegroundColor Cyan
    
    if (-not (Test-Path $filePath)) {
        Write-Host "  ⚠️  File not found (might be new): $filePath" -ForegroundColor Yellow
        
        # If file is new (doesn't exist in backup), create a "new file" diff
        if ($file -like "*MazeConsoleCommands.cs" -or $file -like "*VERBOSITY_GUIDE.md") {
            $diffFile = Join-Path $diffFolder "${fileBaseName}_NEW_${timestamp}.diff"
            "NEW FILE: $file`r`nCreated: $timestamp`r`n`r`n[This is a new file - no previous version exists]" | Out-File -FilePath $diffFile -Encoding UTF8
            Write-Host "  ✅ Created NEW FILE diff: $diffFile" -ForegroundColor Green
            $diffCount++
        }
        continue
    }
    
    # Find backup file
    $backupFile = Join-Path $backupFolder $file
    
    if (Test-Path $backupFile) {
        # Compare with backup
        $diffFile = Join-Path $diffFolder "${fileBaseName}_${timestamp}.diff"
        
        Write-Host "  📊 Comparing with backup..." -ForegroundColor Gray
        
        # Generate unified diff
        $oldContent = Get-Content $backupFile -Raw -Encoding UTF8
        $newContent = Get-Content $filePath -Raw -Encoding UTF8
        
        if ($oldContent -eq $newContent) {
            Write-Host "  ⚠️  No changes detected" -ForegroundColor Yellow
            continue
        }
        
        # Create diff file
        $diffContent = @"
================================================================================
DIFF FILE: $file
Generated: $timestamp
Source: $filePath
Backup: $backupFile
================================================================================

--- OLD (Backup)
+++ NEW (Current)

"@
        
        # Simple line-by-line comparison
        $oldLines = $oldContent -split "`r?`n"
        $newLines = $newContent -split "`r?`n"
        
        $maxLines = [Math]::Max($oldLines.Length, $newLines.Length)
        
        for ($i = 0; $i -lt $maxLines; $i++) {
            $oldLine = if ($i -lt $oldLines.Length) { $oldLines[$i] } else { "<EOF>" }
            $newLine = if ($i -lt $newLines.Length) { $newLines[$i] } else { "<EOF>" }
            
            if ($oldLine -ne $newLine) {
                $diffContent += "`r`nLine $($i + 1):`r`n"
                if ($oldLine -ne "<EOF>") {
                    $diffContent += "- $oldLine`r`n"
                }
                if ($newLine -ne "<EOF>") {
                    $diffContent += "+ $newLine`r`n"
                }
            }
        }
        
        $diffContent += "`r`n================================================================================`r`n"
        $diffContent += "END OF DIFF`r`n"
        $diffContent += "================================================================================`r`n"
        
        $diffContent | Out-File -FilePath $diffFile -Encoding UTF8
        Write-Host "  ✅ Created diff: $diffFile" -ForegroundColor Green
        $diffCount++
        
    } else {
        # No backup exists (new file)
        $diffFile = Join-Path $diffFolder "${fileBaseName}_NEW_${timestamp}.diff"
        
        Write-Host "  🆕 No backup found (new file)" -ForegroundColor Yellow
        
        $diffContent = @"
================================================================================
NEW FILE: $file
Created: $timestamp
Path: $filePath
================================================================================

[This is a new file - no previous version exists]

File content summary:
$(Get-Content $filePath | Measure-Object -Line).Lines lines
$(Get-Content $filePath | Measure-Object -Character).Characters characters

================================================================================
END OF NEW FILE RECORD
================================================================================
"@
        
        $diffContent | Out-File -FilePath $diffFile -Encoding UTF8
        Write-Host "  ✅ Created NEW FILE record: $diffFile" -ForegroundColor Green
        $diffCount++
    }
}

Write-Host ""
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  DIFF GENERATION COMPLETE" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "✅ Generated $diffCount diff file(s)" -ForegroundColor Green
Write-Host "📁 Location: $diffFolder" -ForegroundColor Cyan
Write-Host ""
Write-Host "⚠️  REMINDER: Diff files older than 2 days will be auto-deleted" -ForegroundColor Yellow
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Review diffs in diff_tmp/ folder" -ForegroundColor White
Write-Host "  2. Test in Unity (Press Play)" -ForegroundColor White
Write-Host "  3. If tests pass, run: .\git-commit.ps1" -ForegroundColor White
Write-Host ""
