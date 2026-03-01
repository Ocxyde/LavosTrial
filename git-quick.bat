@echo off
REM git-quick.bat
REM Quick git operations menu for Unity 6 project
REM UTF-8 encoding
REM
REM Usage: git-quick.bat

cd /d "%~dp0"

:MENU
cls
echo ============================================
echo   Git Operations - Unity 6 Project
echo ============================================
echo.
echo 1. Git Status
echo 2. Git Commit (with backup)
echo 3. Git Push
echo 4. Git Pull
echo 5. Git Init & Push (first time)
echo 6. View Git Log
echo 7. Git Diff
echo 8. Clean Git Cache
echo.
echo 0. Exit
echo.
set /p choice="Enter choice (0-8): "

if "%choice%"=="1" goto STATUS
if "%choice%"=="2" goto COMMIT
if "%choice%"=="3" goto PUSH
if "%choice%"=="4" goto PULL
if "%choice%"=="5" goto INIT
if "%choice%"=="6" goto LOG
if "%choice%"=="7" goto DIFF
if "%choice%"=="8" goto CLEAN
if "%choice%"=="0" goto END
goto MENU

:STATUS
powershell -ExecutionPolicy Bypass -File git-status.ps1
pause
goto MENU

:COMMIT
set /p msg="Enter commit message: "
powershell -ExecutionPolicy Bypass -File git-commit.ps1 -Message "%msg%"
pause
goto MENU

:PUSH
powershell -ExecutionPolicy Bypass -File git-push.ps1
pause
goto MENU

:PULL
powershell -ExecutionPolicy Bypass -File git-pull.ps1
pause
goto MENU

:INIT
set /p url="Enter repository URL: "
set /p msg="Enter initial commit message (or press Enter for default): "
if "%msg%"=="" set msg=Initial commit - Unity 6 project setup
powershell -ExecutionPolicy Bypass -File git-init-and-push.ps1 -RepoUrl "%url%" -CommitMessage "%msg%"
pause
goto MENU

:LOG
git log --oneline -10
pause
goto MENU

:DIFF
git diff --stat
pause
goto MENU

:CLEAN
echo Cleaning Git cache...
git gc --prune=now
git remote prune origin
echo Done!
pause
goto MENU

:END
exit
