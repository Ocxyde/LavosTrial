# git-init-and-push.ps1
# Initialize Git repository and push to remote
# UTF-8 encoding - Unix line endings
#
# Usage: powershell -ExecutionPolicy Bypass -File git-init-and-push.ps1 -RepoUrl "https://github.com/username/repo.git"

param(
    [Parameter(Mandatory=$false)]
    [string]$RepoUrl = "",
    
    [Parameter(Mandatory=$false)]
    [string]$Branch = "main",
    
    [Parameter(Mandatory=$false)]
    [string]$CommitMessage = "Initial commit - Unity 6 project setup"
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Git Repository Setup & Push" -ForegroundColor White
Write-Host "  Unity 6 Project - Unity 6000.3.7f1" -ForegroundColor DarkGray
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Check if git is installed
try {
    $gitVersion = git --version
    Write-Host "  [OK] Git found: $gitVersion" -ForegroundColor Green
} catch {
    Write-Host "  [ERROR] Git not found! Please install Git first." -ForegroundColor Red
    Write-Host "  Download from: https://git-scm.com/" -ForegroundColor Yellow
    exit 1
}

# Initialize git if not already initialized
if (-not (Test-Path ".git")) {
    Write-Host "`n[1/5] Initializing Git repository..." -ForegroundColor Yellow
    git init
    Write-Host "  [OK] Git initialized" -ForegroundColor Green
} else {
    Write-Host "`n[1/5] Git repository already initialized" -ForegroundColor Gray
}

# Configure git user if not set
Write-Host "`n[2/5] Checking Git configuration..." -ForegroundColor Yellow
$userName = git config user.name
$userEmail = git config user.email

if (-not $userName) {
    Write-Host "  Enter your Git username:" -ForegroundColor Yellow
    $userName = Read-Host "  Username"
    git config user.name $userName
} else {
    Write-Host "  [OK] Username: $userName" -ForegroundColor Green
}

if (-not $userEmail) {
    Write-Host "  Enter your Git email:" -ForegroundColor Yellow
    $userEmail = Read-Host "  Email"
    git config user.email $userEmail
} else {
    Write-Host "  [OK] Email: $userEmail" -ForegroundColor Green
}

# Add remote if URL provided
if ($RepoUrl) {
    Write-Host "`n[3/5] Adding remote repository..." -ForegroundColor Yellow
    git remote remove origin 2>$null
    git remote add origin $RepoUrl
    Write-Host "  [OK] Remote added: $RepoUrl" -ForegroundColor Green
} else {
    Write-Host "`n[3/5] Skipping remote (no URL provided)" -ForegroundColor Gray
    Write-Host "  You can add it later with: git remote add origin <URL>" -ForegroundColor DarkGray
}

# Add all files
Write-Host "`n[4/5] Adding files to Git..." -ForegroundColor Yellow
git add .
Write-Host "  [OK] Files staged" -ForegroundColor Green

# Commit
Write-Host "`n[5/5] Committing changes..." -ForegroundColor Yellow
git commit -m $CommitMessage
Write-Host "  [OK] Committed: $CommitMessage" -ForegroundColor Green

# Push to remote
if ($RepoUrl) {
    Write-Host "`n[Push] Pushing to remote repository..." -ForegroundColor Yellow
    try {
        git push -u origin $Branch
        Write-Host "  [OK] Pushed to $Branch branch" -ForegroundColor Green
    } catch {
        Write-Host "  [WARNING] Push failed. You may need to:" -ForegroundColor Yellow
        Write-Host "    - Set up SSH keys or credentials" -ForegroundColor DarkGray
        Write-Host "    - Check repository permissions" -ForegroundColor DarkGray
        Write-Host "    - Run: git push -u origin $Branch" -ForegroundColor DarkGray
    }
}

# Summary
Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Git Setup Complete!" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Repository Status:" -ForegroundColor Yellow
git status --short
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. If you didn't provide a repo URL, add it:" -ForegroundColor Gray
Write-Host "     git remote add origin <your-repo-url>" -ForegroundColor DarkGray
Write-Host ""
Write-Host "  2. Push to remote:" -ForegroundColor Gray
Write-Host "     git push -u origin $Branch" -ForegroundColor DarkGray
Write-Host ""
Write-Host "  3. For future commits, use:" -ForegroundColor Gray
Write-Host "     .\git-commit.ps1 -Message `"Your commit message`"" -ForegroundColor DarkGray
Write-Host ""
