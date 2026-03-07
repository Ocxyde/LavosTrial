@echo off
REM ============================================================
REM  Git Auto-Commit - Quick Update Script
REM  Usage: git-auto.bat "Your commit message here"
REM
REM  Note: This script is NOT tracked by git (see .gitignore)
REM  Scripts excluded: *.bat, *.cmd, *.ps1, *.sh
REM
REM  Respects .gitignore exclusions:
REM    - Logs/, *.log, *.sysinfo
REM    - Backup_Solution/, backup/, backups/
REM    - diff_tmp/, diff/
REM    - Library/, Temp/, Obj/, Build/
REM    - *.bak, *.backup, *.unitybackup
REM ============================================================

setlocal enabledelayedexpansion

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
    echo ERROR: Not a Git repository
    pause
    exit /b 1
)

REM --- Get commit message from argument ---
set "COMMIT_MSG=%~1"
if "%COMMIT_MSG%"=="" (
    echo ERROR: No commit message provided
    echo Usage: git-auto.bat "Your commit message"
    echo.
    pause
    exit /b 1
)

REM --- Show status ---
echo [1/5] Git Status...
echo --------------------------------------------
git status --short
echo.

REM --- Check for changes ---
git diff --quiet && git diff --cached --quiet
if %ERRORLEVEL% EQU 0 (
    echo No changes to commit.
    echo.
    echo Excluded by .gitignore:
    echo   *.log, *.sysinfo, Logs/
    echo   Backup_Solution/, diff_tmp/
    echo   Library/, Temp/, Build/
    echo   *.bak, *.backup, *.meta
    echo   *.bat, *.cmd, *.ps1, *.sh
    pause
    exit /b 0
)

REM --- Stage changes ---
echo [2/5] Staging changes...
git add .
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to stage changes
    pause
    exit /b 1
)
echo       Done
echo.

REM --- Show staged changes ---
echo [3/5] Staged Changes:
echo --------------------------------------------
git diff --cached --stat
echo.

REM --- Commit ---
echo [4/5] Committing...
git commit -m "%COMMIT_MSG%"
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Commit failed!
    pause
    exit /b 1
)
echo       Done
echo.

REM --- Show commit ---
echo [5/5] Commit Summary:
echo --------------------------------------------
git log -1 --oneline
echo.

REM --- Ask to push ---
set /p push_now="Push to remote? (y/n): "
if /i "%push_now%"=="y" (
    echo.
    echo Pushing...
    git push
    if %ERRORLEVEL% EQU 0 (
        echo       Done
    ) else (
        echo       Push failed
    )
)

echo.
echo ============================================
echo  Complete!
echo ============================================
echo.
pause
endlocal
