# ============================================================
#  git-auto-commit.ps1 - Automatic Daily Git Commit
#  Commits Assets/ changes to Git repository every 24 hours
# ============================================================

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

# --- Configuration --------------------------------------------
$commitMessagePrefix = "Auto-commit"
$gitUser = ""  # Leave empty to use global config
$gitEmail = "" # Leave empty to use global config
$logFile = Join-Path $scriptDir "Logs\git-auto-commit.log"

# --- Helper Functions -----------------------------------------
function Write-Log {
    param([string]$message, [string]$level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "[$timestamp] [$level] $message"
    
    # Ensure Logs folder exists
    $logDir = Split-Path $logFile
    if (!(Test-Path $logDir)) {
        New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    }
    
    # Write to log file
    Add-Content -Path $logFile -Value $logEntry -Encoding UTF8
    
    # Also write to console if running interactively
    if ($Host.Name -ne "ServerRemoteHost") {
        switch ($level) {
            "ERROR" { Write-Host $logEntry -ForegroundColor Red }
            "WARN"  { Write-Host $logEntry -ForegroundColor Yellow }
            "SUCCESS" { Write-Host $logEntry -ForegroundColor Green }
            default { Write-Host $logEntry -ForegroundColor Gray }
        }
    }
}

function Test-GitAvailable {
    try {
        $null = & git --version 2>&1
        return $true
    } catch {
        return $false
    }
}

function Test-GitRepoInitialized {
    return (Test-Path ".git")
}

function Get-GitStatus {
    $status = & git status --porcelain 2>&1
    return $status
}

function Get-ChangedFilesCount {
    $status = Get-GitStatus
    if ($status -and $status.Count -gt 0) {
        return $status.Count
    }
    return 0
}

# --- Main Script ----------------------------------------------
Write-Log "=========================================="
Write-Log "Starting auto-commit process"
Write-Log "=========================================="

# Check Git availability
if (!(Test-GitAvailable)) {
    Write-Log "ERROR: Git is not installed or not in PATH" "ERROR"
    exit 1
}
Write-Log "Git is available"

# Check if Git repo is initialized
if (!(Test-GitRepoInitialized)) {
    Write-Log "ERROR: Git repository not initialized. Run init-git.ps1 first." "ERROR"
    exit 1
}
Write-Log "Git repository found"

# Set Git user config if specified
if ($gitUser) {
    & git config user.name $gitUser 2>&1 | Out-Null
    Write-Log "Set Git user.name: $gitUser"
}
if ($gitEmail) {
    & git config user.email $gitEmail 2>&1 | Out-Null
    Write-Log "Set Git user.email: $gitEmail"
}

# Run backup first (per project workflow)
Write-Log "Running backup.ps1..."
try {
    $backupScript = Join-Path $scriptDir "backup.ps1"
    if (Test-Path $backupScript) {
        & powershell -ExecutionPolicy Bypass -File $backupScript
        Write-Log "Backup completed" "SUCCESS"
    } else {
        Write-Log "WARN: backup.ps1 not found, skipping backup" "WARN"
    }
} catch {
    Write-Log "WARN: Backup failed: $($_.Exception.Message)" "WARN"
}

# Check for changes
Write-Log "Checking for changes in Assets/ folder..."
$changedCount = Get-ChangedFilesCount

if ($changedCount -eq 0) {
    Write-Log "No changes detected. Skipping commit." "INFO"
    Write-Log "Auto-commit completed (no action needed)" "SUCCESS"
    exit 0
}

Write-Log "Found $changedCount changed file(s)" "INFO"

# Stage Assets/ only (per project workflow)
Write-Log "Staging Assets/ folder..."
& git add Assets/ 2>&1 | Out-Null

# Create commit message with timestamp
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm"
$commitMessage = "$commitMessagePrefix - $timestamp"

# Commit changes
Write-Log "Committing changes..."
$commitResult = & git commit -m $commitMessage 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Log "Commit successful: $commitMessage" "SUCCESS"
} else {
    Write-Log "WARN: Commit may have failed or no changes to commit: $commitResult" "WARN"
}

# Optional: Push to remote (uncomment if you want auto-push)
# Write-Log "Pushing to remote..."
# & git push origin main 2>&1 | Out-Null
# if ($LASTEXITCODE -eq 0) {
#     Write-Log "Push successful" "SUCCESS"
# } else {
#     Write-Log "WARN: Push failed (remote may not be configured)" "WARN"
# }

Write-Log "=========================================="
Write-Log "Auto-commit process completed"
Write-Log "=========================================="

exit 0
