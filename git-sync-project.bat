@echo off
REM ============================================================
REM  Git Sync - Project Files Only
REM  Usage: git-sync-project.bat "Your commit message"
REM ============================================================

setlocal EnableDelayedExpansion

echo.
echo ============================================
echo  Git Sync - Project Files Only
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
echo [1/6] Stashing local changes...
git stash push -m "auto-stash before sync" 2>nul
if %ERRORLEVEL% EQU 0 (
    echo       Done: Changes stashed
) else (
    echo       No changes to stash
)
echo.

REM --- Step 2: Pull latest from remote ---
echo [2/6] Pulling latest from remote...
git pull --rebase
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo WARNING: Pull had conflicts. You may need to resolve manually.
    echo.
)
echo.

REM --- Step 3: Restore stashed changes ---
echo [3/6] Restoring stashed changes...
git stash pop 2>nul
if %ERRORLEVEL% EQU 0 (
    echo       Done: Changes restored
) else (
    echo       No stashed changes to restore
)
echo.

REM --- Step 4: Stage project files only ---
echo [4/6] Staging project files only...
git add "Assets/Scripts/" 2>nul
git add "Assets/Editor/" 2>nul
git add "Assets/Input/" 2>nul
git add "Assets/Prefabs/" 2>nul
git add "Assets/Scenes/" 2>nul
git add "Assets/Ressources/" 2>nul
git add --renormalize "Assets/Scripts/*.cs" 2>nul
git add --renormalize "Assets/Editor/*.cs" 2>nul
echo       Done: Project files staged
echo.

REM --- Show status ---
echo Current status (project files):
echo --------------------------------------------
git status --short "Assets/Scripts/" "Assets/Editor/" "Assets/Scenes/" 2>&1 | findstr /v "^$"
echo.

REM --- Step 5: Commit if message provided ---
if "%COMMIT_MSG%"=="" (
    echo [5/6] No commit message provided - skipping commit
    echo.
    echo Tip: Run 'git commit -m "message"' to commit changes
    echo Tip: Or run 'git-sync-project.bat "message"' to commit with message
) else (
    echo [5/6] Committing changes...
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
        if !ERRORLEVEL! EQU 0 (
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
echo  Note: Only project files were processed
echo.
pause
