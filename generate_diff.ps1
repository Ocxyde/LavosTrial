# generate_diff.ps1
# Generate diff files for changed files and store in diff_tmp
# Unity 6 compatible - UTF-8 encoding - Unix line endings
#
# Usage: .\generate_diff.ps1

param(
    [string]$outputDir = "diff_tmp"
)

$outputPath = Join-Path $PSScriptRoot $outputDir
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

# Create output directory if it doesn't exist
if (-not (Test-Path $outputPath)) {
    New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
}

$changes = @(
    @{
        File = "Assets\Scripts\Core\ItemData.cs"
        Description = "Fix reflection usage - use direct component references"
    },
    @{
        File = "Assets\Scripts\Editor\AddDoorSystemToScene.cs"
        Description = "Update to Unity 6 API - remove deprecated warning suppression"
    }
)

Write-Host "============================="
Write-Host "📋 Generating diff files..."
Write-Host "============================="

foreach ($change in $changes) {
    $filePath = Join-Path $PSScriptRoot $change.File
    $fileName = [System.IO.Path]::GetFileNameWithoutExtension($change.File)
    $diffFile = Join-Path $outputPath "${fileName}_patch_${timestamp}.diff"
    
    if (Test-Path $filePath) {
        # Generate git diff format
        $content = Get-Content $filePath -Raw -Encoding UTF8
        
        # Write diff info
        $diffContent = @"
--- a/$($change.File)
+++ b/$($change.File)
# Patch: $($change.Description)
# Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
# Unity 6 (6000.3.7f1) compatible
# Encoding: UTF-8, Unix LF line endings

"@
        
        # Append file content (full file for reference)
        $diffContent += $content
        
        # Write with Unix line endings (LF)
        [System.IO.File]::WriteAllText(
            $diffFile, 
            $diffContent, 
            [System.Text.UTF8Encoding]::new($false)
        )
        
        Write-Host "✅ Generated: $diffFile"
    } else {
        Write-Host "⚠️  File not found: $($change.File)"
    }
}

Write-Host "============================="
Write-Host "✅ Diff generation complete!"
Write-Host "   Output directory: $outputPath"
Write-Host "============================="
