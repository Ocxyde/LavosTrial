@echo off
REM ============================================================
REM  Git Sync - Pull, Commit Push Helper
REM  Usage: git-sync.bat "Your commit message"
REM ============================================================

echo.
echo ============================================
echo  Git Sync - Pull, Commit & Push
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

set "COMMIT_MSG=%~1"

REM --- Step 1: Stash local changes ---
echo [1/5] Stashing local changes...
git stash push -m "auto-stash before sync" 2>nul
if %ERRORLEVEL% EQU 0 (
    echo       Done: Changes stashed
) else (
    echo       No changes to stash
)
echo.

REM --- Step 2: Pull latest from remote ---
echo [2/5] Pulling latest from remote...
git pull --rebase
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo WARNING: Pull had conflicts. You may need to resolve manually.
    echo.
)
echo.

REM --- Step 3: Restore stashed changes ---
echo [3/5] Restoring stashed changes...
git stash pop 2>nul
if %ERRORLEVEL% EQU 0 (
    echo       Done: Changes restored
) else (
    echo       No stashed changes to restore
)
echo.

REM --- Step 4: Stage and normalize ---
echo [4/5] Staging changes and normalizing line endings...
git add -A
git add --renormalize . 2>nul
echo       Done
echo.

REM --- Show status ---
echo Current status:
git status --short
echo.

REM --- Step 5: Commit if message provided ---
if "%COMMIT_MSG%"=="" (
    echo [5/5] No commit message provided - skipping commit
    echo.
    echo Tip: Run 'git commit -m "message"' to commit changes
    echo Tip: Or run 'git-sync.bat "message"' to commit with message
) else (
    echo [5/5] Committing changes...
    git commit -m "%COMMIT_MSG%"
    if %ERRORLEVEL% EQU 0 (
        echo       Done: Committed
    ) else (
        echo       No changes to commit or commit failed
    )
    echo.
    
    REM --- Ask to push ---
    set /p push_now="Push to remote now? (y/n): "
    if /i "%push_now%"=="y" (
        echo.
        echo Pushing to remote...
        git push
        if %ERRORLEVEL% EQU 0 (
            echo       Done: Pushed successfully
        ) else (
            echo       Warning: Push failed
        )
    )
)

echo.
echo ============================================
echo  Sync Complete!
echo ============================================
echo.
pause
