@echo off
REM ============================================================
REM  Git LF Normalize - For Future Changes
REM  Run this after making changes to ensure LF line endings
REM ============================================================

echo.
echo ============================================
echo  Git LF Normalize - Future Changes
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

echo [1/3] Checking Git status...
git status --short
echo.

REM --- Normalize changed files ---
echo [2/3] Normalizing line endings to LF...
git add --renormalize .
echo       Done: Files normalized
echo.

REM --- Show what changed ---
echo [3/3] Checking for changes...
git status --short
echo.

REM --- Prompt for commit ---
set /p commit="Commit changes? (y/n): "
if /i "%commit%"=="y" (
    git commit -m "Normalize line endings to LF"
    if %ERRORLEVEL% EQU 0 (
        echo       Done: Changes committed
    ) else (
        echo       No changes to commit
    )
) else (
    echo       Skipping commit - changes staged but not committed
)
echo.

echo ============================================
echo  Normalization Complete!
echo ============================================
echo.
echo  Tip: Run 'git status' to see current state
echo.
pause
