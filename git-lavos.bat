@echo off
REM ============================================================
REM  Git Auto-Commit - LavosTrial Project (Assets Only)
REM  Usage: git-lavos.bat "Your commit message here"
REM  Only commits Assets/ folder - excludes scripts, backups, builds
REM ============================================================

setlocal EnableDelayedExpansion

echo.
echo ============================================
echo  Git Auto-Update - LavosTrial
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
    echo Make sure you're in the LavosTrial project folder.
    pause
    exit /b 1
)

REM --- Get commit message from argument ---
set "COMMIT_MSG=%~1"
if "%COMMIT_MSG%"=="" (
    echo ERROR: No commit message provided.
    echo Usage: git-lavos.bat "Your commit message"
    echo.
    pause
    exit /b 1
)

REM --- Check/Add LavosTrial remote ---
echo [1/8] Checking LavosTrial remote...
git remote get-url origin >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo       No 'origin' remote found. Add your LavosTrial repo:
    set /p remote_url="Enter LavosTrial Git URL (or press Enter to skip): "
    if "!remote_url!" NEQ "" (
        git remote add origin !remote_url!
        echo       Added remote: !remote_url!
    ) else (
        echo       Skipped - no remote configured
    )
) else (
    for /f "delims=" %%i in ('git remote get-url origin') do set "REMOTE_URL=%%i"
    echo       Remote 'origin' configured: !REMOTE_URL!
)
echo.

REM --- Current status (Assets only) ---
echo [2/8] Current Git Status (Assets/ only)...
echo --------------------------------------------
git status --short "Assets/" 2>&1 | findstr /v "^$"
echo.

REM --- Check for changes in Assets/ ---
git diff --quiet -- "Assets/" && git diff --cached --quiet -- "Assets/"
if %ERRORLEVEL% EQU 0 (
    echo No changes in Assets/ folder to commit.
    pause
    exit /b 0
)

REM --- Stage Assets/ folder only (excludes *.ps1, *.bat, Backup_Solution/) ---
echo [3/8] Staging Assets/ files only...
echo       Included: Assets/ and all subfolders
echo       Excluded: *.ps1, *.bat, Backup_Solution/, Library/, obj/, bin/
git add "Assets/" 2>nul
echo       Done: Assets/ staged
echo.

REM --- Normalize line endings ---
echo [4/8] Normalizing line endings to LF...
git add --renormalize "Assets/Scripts/*.cs" 2>nul
git add --renormalize "Assets/Editor/*.cs" 2>nul
git add --renormalize "Assets/*.unity" 2>nul
git add --renormalize "Assets/*.prefab" 2>nul
git add --renormalize "Assets/*.asset" 2>nul
echo       Done: Line endings normalized
echo.

REM --- Show what will be committed ---
echo [5/8] Changes to be committed:
echo --------------------------------------------
git diff --cached --stat
echo.

REM --- Run backup ---
if exist "backup.ps1" (
    echo [6/8] Running backup...
    powershell -ExecutionPolicy Bypass -File backup.ps1
    if !ERRORLEVEL! EQU 0 (
        echo       Done: Backup completed
    ) else (
        echo       Warning: Backup failed or skipped
    )
    echo.
) else (
    echo [6/8] Skipping backup (backup.ps1 not found)
    echo.
)

REM --- Commit ---
echo [7/8] Committing changes...
git commit -m "%COMMIT_MSG%"
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Commit failed!
    pause
    exit /b 1
)
echo       Done: Changes committed
echo.

REM --- Show commit summary ---
echo ============================================
echo  Commit Summary
echo ============================================
git log -1 --stat
echo.

REM --- Ask to push ---
echo [8/8] Push to LavosTrial remote?
set /p push_now="Push to remote now? (y/n): "
if /i "%push_now%"=="y" (
    echo.
    echo Pushing to LavosTrial remote...
    git push -u origin HEAD
    if !ERRORLEVEL! EQU 0 (
        echo       Done: Pushed to remote
    ) else (
        echo       Warning: Push failed - check credentials/connection
    )
) else (
    echo.
    echo Changes committed locally. Run 'git push' when ready.
)

echo.
echo ============================================
echo  Auto-Update Complete!
echo ============================================
echo.
echo  Note: Only Assets/ files were committed
echo        (*.ps1, *.bat, Backup_Solution/ excluded)
echo.
pause
