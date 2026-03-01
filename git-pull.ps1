# git-pull.ps1
# Pull latest changes from remote repository
# UTF-8 encoding - Unix line endings
#
# Usage: powershell -ExecutionPolicy Bypass -File git-pull.ps1

param(
    [Parameter(Mandatory=$false)]
    [string]$Remote = "origin",
    
    [Parameter(Mandatory=$false)]
    [switch]$Rebase
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Git Pull - Unity 6 Project" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Check for uncommitted changes
Write-Host "[1/2] Checking for uncommitted changes..." -ForegroundColor Yellow
$status = git status --porcelain
if ($status) {
    Write-Host "  [WARNING] You have uncommitted changes!" -ForegroundColor Yellow
    Write-Host "  Consider committing or stashing them first" -ForegroundColor DarkGray
    Write-Host ""
    $choice = Read-Host "  Continue anyway? (y/n)"
    if ($choice -ne 'y') {
        Write-Host "  [ABORTED] Pull cancelled" -ForegroundColor Red
        exit 0
    }
}

# Pull
Write-Host "`n[2/2] Pulling from $Remote..." -ForegroundColor Yellow
try {
    if ($Rebase) {
        Write-Host "  Using rebase strategy..." -ForegroundColor Gray
        git pull $Remote --rebase
    } else {
        git pull $Remote
    }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n  [SUCCESS] Pulled latest changes" -ForegroundColor Green
    } else {
        Write-Host "`n  [ERROR] Pull failed!" -ForegroundColor Red
        Write-Host "  You may have merge conflicts to resolve" -ForegroundColor DarkGray
    }
} catch {
    Write-Host "`n  [ERROR] Pull failed: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Pull Complete!" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
