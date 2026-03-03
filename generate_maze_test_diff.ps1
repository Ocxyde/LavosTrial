# generate_maze_test_diff.ps1
# Generate diff files for maze test changes
# Unity 6 compatible - UTF-8 encoding - Unix line endings

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$outputDir = "diff_tmp"

# Files that were changed
$changedFiles = @(
    "Assets/Scripts/Tests/MazeWithTorchesTest.cs",
    "Assets/Scripts/Editor/MazeWithTorchesTestEditor.cs",
    "Assets/Scripts/Core/02_Player/PlayerStats.cs"
)

Write-Host "============================="
Write-Host "📋 Generating diff files..."
Write-Host "============================="

foreach ($filePath in $changedFiles) {
    $fullPath = Join-Path $PSScriptRoot $filePath
    $fileName = [System.IO.Path]::GetFileNameWithoutExtension($filePath)
    $diffFile = Join-Path $PSScriptRoot "$outputDir/${fileName}_changes_${timestamp}.diff"
    
    if (Test-Path $fullPath) {
        # Get file content
        $content = Get-Content $fullPath -Raw -Encoding UTF8
        
        # Create diff header
        $diffContent = @"
diff --git a/$filePath b/$filePath
# Changes generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
# Unity 6 (6000.3.7f1) compatible
# Encoding: UTF-8, Unix LF line endings

=== File: $filePath ===
# Full file content for reference:

$content

"@
        
        # Write with Unix line endings (LF)
        [System.IO.File]::WriteAllText(
            $diffFile,
            $diffContent,
            [System.Text.UTF8Encoding]::new($false),
            [System.IO.FileOptions]::None
        )
        
        # Convert to Unix line endings
        $unixContent = $diffContent -replace "`r`n", "`n"
        [System.IO.File]::WriteAllText(
            $diffFile,
            $unixContent,
            [System.Text.UTF8Encoding]::new($false)
        )
        
        Write-Host "✅ Generated: $diffFile" -ForegroundColor Green
    }
    else {
        Write-Host "❌ File not found: $filePath" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "============================="
Write-Host "📋 Diff generation complete!"
Write-Host "============================="
