@echo off
REM ============================================================
REM  Git Status - Project Files Only
REM  Usage: git-status-project.bat
REM ============================================================

setlocal EnableDelayedExpansion

echo.
echo ============================================
echo  Git Status - Project Files Only
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

REM --- Project files status ---
echo Project Files Status:
echo --------------------------------------------

echo [Scripts]
git status --short "Assets/Scripts/" 2>&1 | findstr /v "^$"
if ERRORLEVEL 1 echo   (no changes)
echo.

echo [Editor]
git status --short "Assets/Editor/" 2>&1 | findstr /v "^$"
if ERRORLEVEL 1 echo   (no changes)
echo.

echo [Scenes]
git status --short "Assets/Scenes/" 2>&1 | findstr /v "^$"
if ERRORLEVEL 1 echo   (no changes)
echo.

echo [Prefabs]
git status --short "Assets/Prefabs/" 2>&1 | findstr /v "^$"
if ERRORLEVEL 1 echo   (no changes)
echo.

echo [Input]
git status --short "Assets/Input/" 2>&1 | findstr /v "^$"
if ERRORLEVEL 1 echo   (no changes)
echo.

echo [Ressources]
git status --short "Assets/Ressources/" 2>&1 | findstr /v "^$"
if ERRORLEVEL 1 echo   (no changes)
echo.

echo [Other Assets]
git status --short "Assets/*.unity" "Assets/*.prefab" "Assets/*.asset" "Assets/*.mat" "Assets/*.inputactions" 2>&1 | findstr /v "^$"
if ERRORLEVEL 1 echo   (no changes)
echo.

REM --- Recent commits ---
echo Last 5 commits:
echo --------------------------------------------
git log -5 --oneline --graph
echo.

REM --- Unpushed commits ---
echo Unpushed commits (if any):
echo --------------------------------------------
git log --oneline @{u}..HEAD 2>nul | findstr /v "^$"
if ERRORLEVEL 1 echo   (none or not tracking remote)
echo.

echo ============================================
echo  Quick Commands:
echo    git-auto-project.bat "msg" - Commit project files
echo    git add Assets/Scripts/    - Stage scripts only
echo    git status                 - Full repo status
echo ============================================
echo.

pause
