@echo off
REM ============================================================================
REM  Git Quick Auto - Automated Git Operations for Code.Lavos
REM  Unity 6 compatible - Includes 06_Maze, Excludes Maze*.zip
REM ============================================================================
REM  USAGE:
REM    git-quick-auto.bat [command] [message]
REM
REM  COMMANDS:
REM    status   - Show git status
REM    commit   - Commit with message (auto-includes 06_Maze, excludes Maze*.zip)
REM    push     - Push to remote
REM    pull     - Pull from remote
REM    log      - Show recent commits
REM    diff     - Show changes
REM    addmaze  - Force add 06_Maze folder
REM
REM  EXAMPLES:
REM    git-quick-auto.bat status
REM    git-quick-auto.bat commit "Fixed maze export bug"
REM    git-quick-auto.bat addmaze
REM    git-quick-auto.bat push
REM ============================================================================

setlocal enabledelayedexpansion

REM Configuration
set "PROJECT_NAME=Code.Lavos"
set "REMOTE=origin"
set "BRANCH=main"
set "MAZE_FOLDER=Assets/Scripts/Core/06_Maze"

REM Check if git is available
where git >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] Git is not installed or not in PATH
    echo Download from: https://git-scm.com/
    exit /b 1
)

REM Parse arguments
set "CMD=%~1"
set "MSG=%~2"

REM If no command, show menu
if "%CMD%"=="" goto :menu

REM Execute command
if /i "%CMD%"=="status" goto :status
if /i "%CMD%"=="commit" goto :commit
if /i "%CMD%"=="push" goto :push
if /i "%CMD%"=="pull" goto :pull
if /i "%CMD%"=="log" goto :log
if /i "%CMD%"=="diff" goto :diff
if /i "%CMD%"=="addmaze" goto :addmaze

echo [ERROR] Unknown command: %CMD%
echo.
echo Available commands: status, commit, push, pull, log, diff, addmaze
exit /b 1

:menu
echo.
echo ============================================================================
echo  %PROJECT_NAME% - Git Quick Auto
echo ============================================================================
echo.
echo  1. Status
echo  2. Commit
echo  3. Push
echo  4. Pull
echo  5. Log (last 5 commits)
echo  6. Diff
echo  7. Force add 06_Maze folder
echo  8. Exit
echo.
echo ============================================================================
set /p "choice=Enter choice [1-8]: "

if "%choice%"=="1" goto :status
if "%choice%"=="2" goto :commit_interactive
if "%choice%"=="3" goto :push
if "%choice%"=="4" goto :pull
if "%choice%"=="5" goto :log
if "%choice%"=="6" goto :diff
if "%choice%"=="7" goto :addmaze
if "%choice%"=="8" exit /b

echo Invalid choice
goto :menu

:status
echo.
echo ============================================================================
echo  GIT STATUS
echo ============================================================================
echo.
echo  Checking 06_Maze folder status...
git status "%MAZE_FOLDER%"
echo.
echo  Full repository status:
git status
echo.
exit /b

:commit_interactive
set /p "MSG=Enter commit message: "
if "%MSG%"=="" (
    echo [ERROR] Commit message cannot be empty
    exit /b 1
)
goto :commit

:commit
echo.
echo ============================================================================
echo  COMMITTING CHANGES
echo ============================================================================
echo.
echo  Message: "%MSG%"
echo.

REM Ensure 06_Maze folder is tracked (override .gitignore excludes)
echo  Including 06_Maze files...
git add -f "%MAZE_FOLDER%/"

REM Remove any Maze*.zip from staging (safety - should already be excluded)
echo  Excluding Maze*.zip files...
git rm --cached --ignore-unmatch Maze*.zip >nul 2>nul
git rm --cached --ignore-unmatch **\Maze*.zip >nul 2>nul
git rm --cached --ignore-unmatch %MAZE_FOLDER%\*.zip >nul 2>nul

REM Stage all other changes
echo  Staging remaining changes...
git add .

REM Show what will be committed
echo.
echo  Files to commit:
echo  ------------------------------------------------------------------------
git status --short
echo  ------------------------------------------------------------------------
echo.

set /p "confirm=Proceed with commit? [Y/N]: "
if /i not "%confirm%"=="Y" (
    echo Commit cancelled
    exit /b 0
)

REM Commit
git commit -m "%MSG%"
if %errorlevel% neq 0 (
    echo [ERROR] Commit failed
    exit /b 1
)

echo.
echo [SUCCESS] Commit successful!
echo.
exit /b

:push
echo.
echo ============================================================================
echo  PUSHING TO REMOTE (%REMOTE%/%BRANCH%)
echo ============================================================================
echo.

git push %REMOTE% %BRANCH%
if %errorlevel% neq 0 (
    echo [ERROR] Push failed
    exit /b 1
)

echo.
echo [SUCCESS] Push successful!
echo.
exit /b

:pull
echo.
echo ============================================================================
echo  PULLING FROM REMOTE (%REMOTE%/%BRANCH%)
echo ============================================================================
echo.

git pull %REMOTE% %BRANCH%
if %errorlevel% neq 0 (
    echo [ERROR] Pull failed
    exit /b 1
)

echo.
echo [SUCCESS] Pull successful!
echo.
exit /b

:log
echo.
echo ============================================================================
echo  RECENT COMMITS (last 5)
echo ============================================================================
echo.
git log -5 --oneline --graph
echo.
exit /b

:diff
echo.
echo ============================================================================
echo  CURRENT CHANGES
echo ============================================================================
echo.
git diff HEAD
echo.
exit /b

:addmaze
echo.
echo ============================================================================
echo  FORCE ADD 06_MAZE FOLDER
echo ============================================================================
echo.
echo  This will force-add all files in %MAZE_FOLDER%
echo  (including .meta files that may be excluded by .gitignore)
echo.

set /p "confirm=Force add 06_Maze folder? [Y/N]: "
if /i not "%confirm%"=="Y" (
    echo Operation cancelled
    exit /b 0
)

echo.
echo  Adding 06_Maze files...
git add -f "%MAZE_FOLDER%/"

echo.
echo  Status of 06_Maze folder:
git status --short "%MAZE_FOLDER%"
echo.
echo [SUCCESS] 06_Maze folder staged for commit!
echo.
echo  Run 'git commit -m "your message"' to commit these changes.
echo.
exit /b
