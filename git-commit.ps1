# git-commit.ps1
# Quick commit script for Unity 6 project
# UTF-8 encoding - Unix line endings
#
# Usage: 
#   powershell -ExecutionPolicy Bypass -File git-commit.ps1 -Message "Your commit message"
#   Or: .\git-commit.ps1 "Your commit message"

param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$Message,
    
    [Parameter(Mandatory=$false)]
    [switch]$All,
    
    [Parameter(Mandatory=$false)]
    [switch]$NoBackup
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Git Commit - Unity 6 Project" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Run backup first (unless disabled)
if (-not $NoBackup) {
    Write-Host "[1/4] Running backup..." -ForegroundColor Yellow
    if (Test-Path ".\apply-patches-and-backup.ps1") {
        & .\apply-patches-and-backup.ps1
    } else {
        Write-Host "  [SKIP] Backup script not found" -ForegroundColor Gray
    }
} else {
    Write-Host "[1/4] Skipping backup (disabled)" -ForegroundColor Gray
}

# Check git status
Write-Host "`n[2/4] Checking Git status..." -ForegroundColor Yellow
$status = git status --porcelain
if (-not $status) {
    Write-Host "  [INFO] No changes to commit" -ForegroundColor Green
    exit 0
} else {
    $changeCount = ($status -split "`n").Count
    Write-Host "  [OK] $changeCount file(s) changed" -ForegroundColor Green
}

# Add files
Write-Host "`n[3/4] Staging files..." -ForegroundColor Yellow
if ($All) {
    git add -A
} else {
    git add .
}
Write-Host "  [OK] Files staged" -ForegroundColor Green

# Commit
Write-Host "`n[4/4] Committing..." -ForegroundColor Yellow
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$fullMessage = "$Message (auto-committed on $timestamp)"
git commit -m $fullMessage

if ($LASTEXITCODE -eq 0) {
    Write-Host "  [OK] Committed successfully!" -ForegroundColor Green
} else {
    Write-Host "  [WARNING] Commit may have failed (no changes?)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Commit Complete!" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "To push to remote:" -ForegroundColor Yellow
Write-Host "  git push" -ForegroundColor DarkGray
Write-Host ""
Write-Host "Or use: .\git-push.ps1" -ForegroundColor DarkGray
Write-Host ""
