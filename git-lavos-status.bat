@echo off
REM ============================================================
REM  Git Status - LavosTrial Project (Assets Only)
REM  Usage: git-lavos-status.bat
REM  Shows status of Assets/ folder only
REM ============================================================

setlocal EnableDelayedExpansion

echo.
echo ============================================
echo  Git Status - LavosTrial
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
    pause
    exit /b 1
)

REM --- Current branch ---
echo Branch:
git branch --show-current
echo.

REM --- Remote info ---
echo Remote (LavosTrial):
echo --------------------------------------------
git remote get-url origin 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo   (no remote configured - run git-lavos.bat to set up)
)
echo.

REM --- Assets/ status ---
echo Assets/ Status:
echo --------------------------------------------
git status --short "Assets/" 2>&1 | findstr /v "^$"
if %ERRORLEVEL% NEQ 0 (
    echo   (no changes in Assets/)
)
echo.

REM --- Recent commits ---
echo Last 5 commits:
echo --------------------------------------------
git log -5 --oneline --graph
echo.

REM --- Unpushed commits ---
echo Unpushed commits:
echo --------------------------------------------
git log --oneline @{u}..HEAD 2>nul | findstr /v "^$"
if %ERRORLEVEL% NEQ 0 (
    echo   (none or not tracking remote)
)
echo.

REM --- Stash ---
echo Stash:
echo --------------------------------------------
git stash list 2>&1 | findstr /v "^$"
if %ERRORLEVEL% NEQ 0 (
    echo   (empty)
)
echo.

echo ============================================
echo  Quick Commands:
echo    git-lavos.bat "msg"        - Commit Assets/ and push
echo    git-lavos-sync.bat "msg"   - Pull, merge, commit Assets/
echo    git-lavos-status.bat       - This status
echo    git push                   - Push to LavosTrial
echo    git pull                   - Pull from LavosTrial
echo ============================================
echo.
echo  Note: Only Assets/ files are tracked
echo        (*.ps1, *.bat, Backup_Solution/ excluded)
echo.

pause
