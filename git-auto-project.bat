@echo off
REM ============================================================
REM  Git Auto-Commit - Project Files Only (Assets/Scripts)
REM  Usage: git-auto-project.bat "Your commit message here"
REM ============================================================

setlocal EnableDelayedExpansion

echo.
echo ============================================
echo  Git Auto-Update - Project Files Only
echo  (Assets/Scripts, Editor, Input, Prefabs)
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
    echo Usage: git-auto-project.bat "Your commit message"
    echo.
    pause
    exit /b 1
)

echo [1/7] Current Git Status (Project Files Only)...
echo --------------------------------------------
git status --short "Assets/Scripts/" "Assets/Editor/" "Assets/Input/" "Assets/Prefabs/" "Assets/Scenes/" "Assets/Ressources/" 2>&1 | findstr /v "^$"
echo.

REM --- Stage project files only (not Library, obj, bin, etc.) ---
echo [2/7] Staging project files only...
echo       (Assets, Scripts, Editor, Input, Prefabs, Scenes, Ressources)
git add "Assets/Scripts/" 2>nul
git add "Assets/Editor/" 2>nul
git add "Assets/Input/" 2>nul
git add "Assets/Prefabs/" 2>nul
git add "Assets/Scenes/" 2>nul
git add "Assets/Ressources/" 2>nul
git add "Assets/Core/" 2>nul
git add "Assets/HUD/" 2>nul
git add "Assets/Inventory/" 2>nul
git add "Assets/DBase/" 2>nul
git add "Assets/Interaction/" 2>nul
git add "Assets/Ennemies/" 2>nul
git add "Assets/Player/" 2>nul
git add "Assets/*.unity" 2>nul
git add "Assets/*.prefab" 2>nul
git add "Assets/*.asset" 2>nul
git add "Assets/*.mat" 2>nul
git add "Assets/*.inputactions" 2>nul
echo       Done: Project files staged
echo.

REM --- Normalize line endings for staged files ---
echo [3/7] Normalizing line endings to LF...
git add --renormalize "Assets/Scripts/*.cs" 2>nul
git add --renormalize "Assets/Editor/*.cs" 2>nul
git add --renormalize "Assets/*.cs" 2>nul
echo       Done: Line endings normalized
echo.

REM --- Show what will be committed ---
echo [4/7] Changes to be committed:
echo --------------------------------------------
git diff --cached --stat
echo.

REM --- Run backup if script exists ---
if exist "backup.ps1" (
    echo [5/7] Running backup...
    powershell -ExecutionPolicy Bypass -File backup.ps1
    if !ERRORLEVEL! EQU 0 (
        echo       Done: Backup completed
    ) else (
        echo       Warning: Backup failed or skipped
    )
    echo.
) else (
    echo [5/7] Skipping backup (backup.ps1 not found)
    echo.
)

REM --- Commit ---
echo [6/7] Committing changes...
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
    if !ERRORLEVEL! EQU 0 (
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
echo  Note: Only project files were committed (not Library/obj/bin)
echo.
pause
