@echo off
REM ============================================================
REM  Setup LavosTrial Git Remote + Assets/ Only Configuration
REM  Usage: git-lavos-setup.bat
REM ============================================================

setlocal EnableDelayedExpansion

echo.
echo ============================================
echo  Setup LavosTrial Git Remote
echo  (Assets/ files only)
echo ============================================
echo.

REM --- Check if Git is available ---
git --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Git is not installed or not in PATH
    pause
    exit /b 1
)

REM --- Check if we're in a Git repo ---
if not exist ".git" (
    echo ERROR: Not a Git repository.
    echo.
    echo Initialize Git first:
    echo   git init
    echo.
    pause
    exit /b 1
)

echo Current Git Configuration:
echo --------------------------------------------

REM --- Check for existing remote ---
git remote get-url origin >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo [origin] Remote already configured:
    git remote get-url origin
    echo.
    set /p change_remote="Change remote URL? (y/n): "
    if /i "!change_remote!"=="y" (
        set /p new_url="Enter new LavosTrial Git URL: "
        git remote set-url origin "!new_url!"
        echo       Updated remote to: !new_url!
    )
) else (
    echo [origin] No remote configured
    echo.
    set /p add_remote="Add LavosTrial remote now? (y/n): "
    if /i "!add_remote!"=="y" (
        set /p remote_url="Enter LavosTrial Git URL: "
        git remote add origin "!remote_url!"
        echo       Added remote: !remote_url!
    )
)
echo.

REM --- Configure line endings ---
echo Line Endings Configuration:
echo --------------------------------------------
for /f "delims=" %%i in ('git config core.autocrlf') do set "CRLFAUTO=%%i"
for /f "delims=" %%i in ('git config core.eol') do set "EOL=%%i"

echo core.autocrlf = !CRLFAUTO!
echo core.eol = !EOL!
echo.

set /p fix_eol="Configure for Unix LF line endings? (y/n): "
if /i "!fix_eol!"=="y" (
    git config core.autocrlf input
    git config core.eol lf
    echo       Done: Configured for LF
)
echo.

REM --- Create/Update .gitignore ---
echo .gitignore Configuration:
echo --------------------------------------------
if exist ".gitignore" (
    echo [Found] .gitignore exists
    set /p update_gitignore="Update .gitignore for Assets/ only? (y/n): "
    if /i "!update_gitignore!"=="y" (
        call :CreateGitignore
        echo       Updated .gitignore
    )
) else (
    echo [New] Creating .gitignore...
    call :CreateGitignore
    echo       Created .gitignore
)
echo.

REM --- Create .gitattributes ---
echo .gitattributes Configuration:
echo --------------------------------------------
if exist ".gitattributes" (
    echo [Found] .gitattributes exists
    set /p update_gitattributes="Update .gitattributes for LF? (y/n): "
    if /i "!update_gitattributes!"=="y" (
        call :CreateGitattributes
        echo       Updated .gitattributes
    )
) else (
    echo [New] Creating .gitattributes...
    call :CreateGitattributes
    echo       Created .gitattributes
)
echo.

REM --- Show remotes ---
echo All Remotes:
echo --------------------------------------------
git remote -v
echo.

REM --- Test connection ---
echo Test Connection:
echo --------------------------------------------
git remote get-url origin >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo Testing connection to remote...
    git ls-remote origin >nul 2>&1
    if !ERRORLEVEL! EQU 0 (
        echo       Connection successful!
    ) else (
        echo       Connection failed - check URL and credentials
    )
) else (
    echo       No remote configured
)
echo.

echo ============================================
echo  Setup Complete!
echo ============================================
echo.
echo  Tracked files:
echo    ✓ Assets/ and all subfolders
echo    ✓ Unity files (.unity, .prefab, .asset, .mat)
echo    ✓ C# scripts (.cs)
echo    ✓ Input System (.inputactions)
echo.
echo  Excluded files:
echo    ✗ *.ps1 (PowerShell scripts)
echo    ✗ *.bat (Batch files)
echo    ✗ Backup_Solution/
echo    ✗ Library/, obj/, bin/, Builds/
echo.
echo  Next steps:
echo    git-lavos.bat "message"  - Commit Assets/ and push
echo    git-lavos-status.bat     - Check status
echo.
pause
goto :EOF

:CreateGitignore
(
    echo # Unity generated
    echo [Ll]ibrary/
    echo [Tt]emp/
    echo [Oo]bj/
    echo [Bb]uild/
    echo [Bb]uilds/
    echo.
    echo # Visual Studio / Rider
    echo .vs/
    echo *.suo
    echo *.user
    echo *.userosscache
    echo *.sln.docstates
    echo.
    echo # Unity cache
    echo *.pidb.meta
    echo *.pdb.meta
    echo *.mdb.meta
    echo.
    echo # Backup files
    echo Backup_Solution/
    echo.
    echo # Scripts (not tracked in Git)
    echo *.ps1
    echo *.bat
    echo *.cmd
    echo.
    echo # OS generated
    echo .DS_Store
    echo Thumbs.db
    echo.
    echo # Diff files
    echo diff_tmp/
) > ".gitignore"
goto :EOF

:CreateGitattributes
(
    echo # Force LF for all text files
    echo * text=auto eol=lf
    echo.
    echo # C# files
    echo *.cs text eol=lf
    echo *.asmdef text eol=lf
    echo.
    echo # Unity YAML
    echo *.unity text eol=lf
    echo *.prefab text eol=lf
    echo *.asset text eol=lf
    echo *.mat text eol=lf
    echo *.controller text eol=lf
    echo *.anim text eol=lf
    echo.
    echo # Shaders
    echo *.shader text eol=lf
    echo *.hlsl text eol=lf
    echo *.cginc text eol=lf
    echo.
    echo # Text and config
    echo *.json text eol=lf
    echo *.xml text eol=lf
    echo *.md text eol=lf
    echo *.txt text eol=lf
    echo.
    echo # Input System
    echo *.inputactions text eol=lf
) > ".gitattributes"
goto :EOF
