$PROJECT_DIR = $PSScriptRoot
$PROJECT_NAME = "PeuImporte"
$BACKUP_DIR = "$env:USERPROFILE\Documents\Backups"

if (-not (Test-Path $BACKUP_DIR)) {
    New-Item -ItemType Directory -Path $BACKUP_DIR | Out-Null
}

$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$zipName = "${PROJECT_NAME}_backup_${timestamp}.zip"
$zipPath = Join-Path $BACKUP_DIR $zipName

Write-Host "============================================"
Write-Host "Project Backup - $timestamp"
Write-Host "============================================"
Write-Host "Project: $PROJECT_DIR"
Write-Host "Destination: $zipPath"
Write-Host ""

$excludeFolders = @("Library", "Temp", "obj", "bin", ".git", "Logs")
$excludeFiles = @("*.log", "*.lock", "*.pid")

$items = Get-ChildItem -Path $PROJECT_DIR -Recurse -File -ErrorAction SilentlyContinue | Where-Object {
    $fullPath = $_.FullName
    $name = $_.Name
    $exclude = $false
    foreach ($folder in $excludeFolders) {
        if ($fullPath -like "*\$folder\*" -or $fullPath -like "*\$folder") {
            $exclude = $true
            break
        }
    }
    if (-not $exclude) {
        foreach ($pattern in $excludeFiles) {
            if ($name -like $pattern) {
                $exclude = $true
                break
            }
        }
    }
    -not $exclude
}

try {
    Compress-Archive -Path $items.FullName -DestinationPath $zipPath -Force -ErrorAction Stop
} catch {
    Write-Host "Warning: Some files could not be compressed (likely locked by another process)"
    $failedFiles = $items | Where-Object { -not (Test-Path $_.FullName) }
    $items = $items | Where-Object { Test-Path $_.FullName }
    if ($items.Count -gt 0) {
        Compress-Archive -Path $items.FullName -DestinationPath $zipPath -Force
    }
}

if (Test-Path $zipPath) {
    $size = (Get-Item $zipPath).Length
    Write-Host ""
    Write-Host "Backup completed successfully."
    Write-Host "File: $zipPath"
    Write-Host "Size: $size bytes"
} else {
    Write-Host ""
    Write-Host "ERROR: Backup failed."
    exit 1
}
