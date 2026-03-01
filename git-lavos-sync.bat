@echo off
REM ============================================================
REM  Git Sync - LavosTrial Remote (Assets Only)
REM  Usage: git-lavos-sync.bat "Your commit message"
REM  Only handles Assets/ folder - excludes scripts, backups, builds
REM ============================================================

setlocal EnableDelayedExpansion

echo.
echo ============================================
echo  Git Sync - LavosTrial
echo  (Assets/ files only)
echo ============================================
echo.

REM --- Check prerequisites ---
git --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Git is not installed or not in PATH
    pause
    exit /b 1
)

if not exist ".git" (
    echo ERROR: Not a Git repository.
    pause
    exit /b 1
)

REM --- Check/Add LavosTrial remote ---
echo [1/7] Checking LavosTrial remote...
git remote get-url origin >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo       ERROR: No 'origin' remote configured!
    set /p remote_url="Enter LavosTrial Git URL: "
    if "!remote_url!" NEQ "" (
        git remote add origin !remote_url!
        echo       Added remote: !remote_url!
    ) else (
        echo       Cannot proceed without remote URL
        pause
        exit /b 1
    )
) else (
    for /f "delims=" %%i in ('git remote get-url origin') do set "REMOTE_URL=%%i"
    echo       Remote: !REMOTE_URL!
)
echo.

REM --- Step 1: Stash local changes ---
echo [2/7] Stashing local changes...
git stash push -m "auto-stash before sync" 2>nul
if %ERRORLEVEL% EQU 0 (
    echo       Done: Changes stashed
) else (
    echo       No changes to stash
)
echo.

REM --- Step 2: Pull latest from LavosTrial ---
echo [3/7] Pulling latest from LavosTrial...
git pull --rebase origin
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo WARNING: Pull had conflicts. Resolve manually, then run:
    echo   git rebase --continue
    echo   git stash pop
    echo.
    pause
    exit /b 1
)
echo.

REM --- Step 3: Restore stashed changes ---
echo [4/7] Restoring stashed changes...
git stash pop 2>nul
if %ERRORLEVEL% EQU 0 (
    echo       Done: Changes restored
) else (
    echo       No stashed changes to restore
)
echo.

REM --- Step 4: Stage Assets/ only ---
echo [5/7] Staging Assets/ files only...
git add "Assets/" 2>nul
git add --renormalize "Assets/Scripts/*.cs" 2>nul
git add --renormalize "Assets/Editor/*.cs" 2>nul
echo       Done: Assets/ staged
echo.

REM --- Show status ---
echo Current status (Assets/ only):
echo --------------------------------------------
git status --short "Assets/" 2>&1 | findstr /v "^$"
echo.

REM --- Step 5: Commit if message provided ---
set "COMMIT_MSG=%~1"
if "%COMMIT_MSG%"=="" (
    echo [6/7] No commit message provided - skipping commit
    echo.
    echo Tip: Run 'git commit -m "message"' to commit
    echo Tip: Or run 'git-lavos-sync.bat "message"' to commit with message
) else (
    echo [6/7] Committing changes...
    git commit -m "%COMMIT_MSG%"
    if %ERRORLEVEL% EQU 0 (
        echo       Done: Committed
    ) else (
        echo       No changes to commit or commit failed
    )
    echo.
)

REM --- Step 6: Push ---
echo [7/7] Pushing to LavosTrial...
git push origin
if %ERRORLEVEL% EQU 0 (
    echo       Done: Pushed successfully
) else (
    echo       Warning: Push failed - check credentials/connection
)

echo.
echo ============================================
echo  Sync Complete!
echo ============================================
echo.
echo  Note: Only Assets/ files were processed
echo        (*.ps1, *.bat, Backup_Solution/ excluded)
echo.
pause
