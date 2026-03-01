@echo off
REM ============================================================
REM  Git Auto-Commit - Quick Update Script
REM  Usage: git-auto.bat "Your commit message here"
REM ============================================================

echo.
echo ============================================
echo  Git Auto-Update
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
    echo ERROR: Not a Git repository. Run this from project root.
    pause
    exit /b 1
)

REM --- Get commit message from argument ---
set "COMMIT_MSG=%~1"
if "%COMMIT_MSG%"=="" (
    echo ERROR: No commit message provided.
    echo Usage: git-auto.bat "Your commit message"
    echo.
    pause
    exit /b 1
)

echo [1/6] Current Git Status...
echo --------------------------------------------
git status --short
echo.

REM --- Check for changes ---
git diff --quiet && git diff --cached --quiet
if %ERRORLEVEL% EQU 0 (
    echo No changes to commit.
    pause
    exit /b 0
)

REM --- Stage all changes ---
echo [2/6] Staging all changes...
git add -A
echo       Done: All changes staged
echo.

REM --- Normalize line endings ---
echo [3/6] Normalizing line endings to LF...
git add --renormalize . 2>nul
echo       Done: Line endings normalized
echo.

REM --- Show what will be committed ---
echo [4/6] Changes to be committed:
echo --------------------------------------------
git diff --cached --stat
echo.

REM --- Run backup if script exists ---
if exist "backup.ps1" (
    echo [5/6] Running backup...
    powershell -ExecutionPolicy Bypass -File backup.ps1
    if %ERRORLEVEL% EQU 0 (
        echo       Done: Backup completed
    ) else (
        echo       Warning: Backup failed or skipped
    )
    echo.
) else (
    echo [5/6] Skipping backup (backup.ps1 not found)
    echo.
)

REM --- Commit ---
echo [6/6] Committing changes...
git commit -m "%COMMIT_MSG%"
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Commit failed!
    echo Possible reasons:
    echo   - No changes to commit
    echo   - Git configuration issue
    echo.
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
set /p push_now="Push to remote now? (y/n): "
if /i "%push_now%"=="y" (
    echo.
    echo Pushing to remote...
    git push
    if %ERRORLEVEL% EQU 0 (
        echo       Done: Pushed to remote
    ) else (
        echo       Warning: Push failed - check connection/credentials
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
pause
