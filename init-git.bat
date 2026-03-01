# ============================================================
#  Git Initialization Script for LavosTrial
#  Run this to initialize Git tracking for Assets/ only
# ============================================================

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Git Initialization - LavosTrial" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Check if git is available
Write-Host "  Checking Git installation..." -ForegroundColor DarkGray
try {
    $gitVersion = git --version 2>&1
    Write-Host "  Git found: $gitVersion" -ForegroundColor Green
} catch {
    Write-Host "  ERROR: Git is not installed or not in PATH" -ForegroundColor Red
    Write-Host "  Please install Git from: https://git-scm.com/download/win" -ForegroundColor Yellow
    Write-Host ""
    exit 1
}

# Check if already initialized
if (Test-Path ".git") {
    Write-Host "  Git repository already initialized." -ForegroundColor Yellow
} else {
    Write-Host "  Initializing Git repository..." -ForegroundColor DarkGray
    git init
    Write-Host "  Git repository created." -ForegroundColor Green
}

Write-Host ""
Write-Host "  Setting up .gitattributes (LF line endings)..." -ForegroundColor DarkGray

# Create .gitattributes if not exists
$gitattributes = @"
# Force LF for all text files
* text=auto eol=lf

# C# files
*.cs text eol=lf
*.asmdef text eol=lf

# Unity YAML
*.unity text eol=lf
*.prefab text eol=lf
*.asset text eol=lf
*.mat text eol=lf

# Shaders
*.shader text eol=lf
*.hlsl text eol=lf
*.cginc text eol=lf

# Input System
*.inputactions text eol=lf

# Text and config
*.json text eol=lf
*.xml text eol=lf
*.md text eol=lf
*.txt text eol=lf

# Scripts (if tracked)
*.ps1 text eol=lf
*.bat text eol=lf
*.cmd text eol=lf

# Images (binary)
*.png binary
*.jpg binary
*.jpeg binary
*.gif binary
*.tga binary
*.psd binary
*.ai binary

# Audio (binary)
*.mp3 binary
*.wav binary
*.ogg binary

# Models (binary)
*.fbx binary
*.obj binary
*.blend binary
"@

Set-Content -Path ".gitattributes" -Value $gitattributes -Encoding UTF8
Write-Host "  .gitattributes created/updated." -ForegroundColor Green

Write-Host ""
Write-Host "  Current .gitignore status:" -ForegroundColor DarkGray
if (Test-Path ".gitignore") {
    Write-Host "  .gitignore exists (not modified)" -ForegroundColor Yellow
} else {
    Write-Host "  .gitignore NOT found - creating one..." -ForegroundColor Yellow
    
    $gitignore = @"
# Unity generated folders
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Lo]ogs/
[U]ser[Ss]ettings/

# Visual Studio / Rider
.vs/
*.suo
*.user
*.sln.docstates
*.csproj.user
*.unityproj.user

# MonoDevelop / Xamarin Studio
*.pidb.meta
*.pidb
unity-generated-project

# Unity3D generated files
*.booproj
*.sln
*.csproj
*.unityproj

# Unity crash logs
*.log
*.sysinfo

# Unity backup files
*.bak
*.backup
*.unitybackup

# Plugin cache
[Pp]ackages/*.tgz
[Pp]ackages/*.sha256

# NuGet
*.nupkg
*.snupkg
packages/
.nuget/

# Backup folders
Backup_Solution/
backup/
backups/

# Diff files
diff_tmp/
diff/

# OS generated files
.DS_Store
.DS_Store?
._*
.Spotlight-V100
.Trashes
ehthumbs.db
Thumbs.db
desktop.ini

# Unity specific excludes
ProjectSettings/*.asset
ProjectSettings/*.unity
ProjectSettings/*.prefab

# Keep these project settings
!ProjectSettings/ProjectSettings.asset
!ProjectSettings/EditorBuildSettings.asset
!ProjectSettings/InputManager.asset
!ProjectSettings/TagManager.asset
!ProjectSettings/TimeManager.asset
!ProjectSettings/Physics2DSettings.asset
!ProjectSettings/NavMeshAreas.asset
!ProjectSettings/QualitySettings.asset
!ProjectSettings/GraphicsSettings.asset
!ProjectSettings/AudioManager.asset
!ProjectSettings/ProjectVersion.txt
"@
    
    Set-Content -Path ".gitignore" -Value $gitignore -Encoding UTF8
    Write-Host "  .gitignore created." -ForegroundColor Green
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Setup Complete!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Next steps:" -ForegroundColor White
Write-Host "  1. Set your Git user name:" -ForegroundColor DarkGray
Write-Host "     git config user.name `"Your Name`"" -ForegroundColor Gray
Write-Host ""
Write-Host "  2. Set your Git email:" -ForegroundColor DarkGray
Write-Host "     git config user.email `"your@email.com`"" -ForegroundColor Gray
Write-Host ""
Write-Host "  3. Add your remote repository:" -ForegroundColor DarkGray
Write-Host "     git remote add origin <your-repo-url>" -ForegroundColor Gray
Write-Host ""
Write-Host "  4. Make your first commit:" -ForegroundColor DarkGray
Write-Host "     git add Assets/" -ForegroundColor Gray
Write-Host "     git commit -m `"Initial commit - LavosTrial project`"" -ForegroundColor Gray
Write-Host "     git push -u origin main" -ForegroundColor Gray
Write-Host ""
Write-Host "  Or use the helper script:" -ForegroundColor DarkGray
Write-Host "     .\git-lavos.bat `"Initial commit`"" -ForegroundColor Gray
Write-Host ""
