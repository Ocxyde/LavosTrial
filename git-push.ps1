# git-push.ps1
# Push commits to remote repository
# UTF-8 encoding - Unix line endings
#
# Usage: powershell -ExecutionPolicy Bypass -File git-push.ps1

param(
    [Parameter(Mandatory=$false)]
    [string]$Remote = "origin",
    
    [Parameter(Mandatory=$false)]
    [string]$Branch = "main"
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Git Push - Unity 6 Project" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Check current branch
Write-Host "[1/3] Checking current branch..." -ForegroundColor Yellow
$currentBranch = git rev-parse --abbrev-ref HEAD
Write-Host "  Current branch: $currentBranch" -ForegroundColor Gray

# Check for uncommitted changes
Write-Host "`n[2/3] Checking for uncommitted changes..." -ForegroundColor Yellow
$status = git status --porcelain
if ($status) {
    Write-Host "  [WARNING] You have uncommitted changes!" -ForegroundColor Yellow
    Write-Host "  Commit them first with: .\git-commit.ps1 `"message`"" -ForegroundColor DarkGray
    Write-Host ""
    $choice = Read-Host "  Continue anyway? (y/n)"
    if ($choice -ne 'y') {
        Write-Host "  [ABORTED] Push cancelled" -ForegroundColor Red
        exit 0
    }
} else {
    Write-Host "  [OK] No uncommitted changes" -ForegroundColor Green
}

# Push
Write-Host "`n[3/3] Pushing to $Remote/$Branch..." -ForegroundColor Yellow
try {
    git push $Remote $currentBranch
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n  [SUCCESS] Pushed to $Remote/$currentBranch" -ForegroundColor Green
    } else {
        Write-Host "`n  [ERROR] Push failed!" -ForegroundColor Red
        Write-Host "  Try: git push --set-upstream $Remote $currentBranch" -ForegroundColor DarkGray
    }
} catch {
    Write-Host "`n  [ERROR] Push failed: $_" -ForegroundColor Red
    Write-Host "  Make sure your remote is configured correctly" -ForegroundColor DarkGray
    Write-Host "  Check with: git remote -v" -ForegroundColor DarkGray
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Push Complete!" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
