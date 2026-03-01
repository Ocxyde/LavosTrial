# git-status.ps1
# Show detailed Git status for Unity 6 project
# UTF-8 encoding - Unix line endings
#
# Usage: powershell -ExecutionPolicy Bypass -File git-status.ps1

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Git Status - Unity 6 Project" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Current branch
Write-Host "BRANCH:" -ForegroundColor Yellow
$branch = git rev-parse --abbrev-ref HEAD
Write-Host "  Current: $branch" -ForegroundColor Gray
Write-Host ""

# Remote info
Write-Host "REMOTE:" -ForegroundColor Yellow
$remoteUrl = git remote get-url origin 2>$null
if ($remoteUrl) {
    Write-Host "  Origin: $remoteUrl" -ForegroundColor Gray
} else {
    Write-Host "  [NOT SET] No remote configured" -ForegroundColor Red
    Write-Host "  Add with: git remote add origin <URL>" -ForegroundColor DarkGray
}
Write-Host ""

# Status
Write-Host "CHANGES:" -ForegroundColor Yellow
$status = git status --short
if ($status) {
    Write-Host $status -ForegroundColor Cyan
} else {
    Write-Host "  [CLEAN] No uncommitted changes" -ForegroundColor Green
}
Write-Host ""

# Recent commits
Write-Host "RECENT COMMITS:" -ForegroundColor Yellow
git log --oneline -5
Write-Host ""

# Statistics
Write-Host "REPOSITORY INFO:" -ForegroundColor Yellow
$commitCount = git rev-list --count HEAD
Write-Host "  Total commits: $commitCount" -ForegroundColor Gray

$branchCount = git branch | Measure-Object | Select-Object -ExpandProperty Count
Write-Host "  Local branches: $branchCount" -ForegroundColor Gray

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Status Check Complete!" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
