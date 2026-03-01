@echo off
REM ============================================================
REM  Git Quick Status - Fast Overview
REM  Usage: git-status.bat or just: gs
REM ============================================================

echo.
echo ============================================
echo  Git Repository Status
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
    pause
    exit /b 1
)

REM --- Current branch ---
echo Branch:
git branch --show-current
echo.

REM --- Short status ---
echo Changes (git status --short):
echo --------------------------------------------
git status --short
if %ERRORLEVEL% EQU 0 (
    git diff --quiet && git diff --cached --quiet
    if %ERRORLEVEL% EQU 0 (
        echo   (none - working tree clean)
    )
)
echo.

REM --- Recent commits ---
echo Last 5 commits:
echo --------------------------------------------
git log -5 --oneline --graph
echo.

REM --- Unpushed commits ---
echo Unpushed commits (if any):
echo --------------------------------------------
git log --oneline @{u}..HEAD 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo   (none or not tracking remote)
)
echo.

REM --- Stash ---
echo Stash:
echo --------------------------------------------
git stash list
if %ERRORLEVEL% NEQ 0 (
    echo   (empty)
)
echo.

echo ============================================
echo  Quick Commands:
echo    git add .        - Stage all changes
echo    git commit -m "" - Commit with message
echo    git push         - Push to remote
echo    git pull         - Pull from remote
echo ============================================
echo.
