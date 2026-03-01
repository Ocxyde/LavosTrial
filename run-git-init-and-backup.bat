@echo off
REM ============================================================
REM  Git Initialization and Backup Runner
REM  Double-click to run Git init + backup
REM ============================================================

cd /d "%~dp0"

echo.
echo ============================================
echo   LavosTrial - Git Init ^& Backup
echo ============================================
echo.

REM Check if PowerShell is available
where powershell >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: PowerShell not found!
    pause
    exit /b 1
)

echo [1/2] Running backup.ps1...
echo.
powershell -ExecutionPolicy Bypass -File "%~dp0backup.ps1"
if %ERRORLEVEL% NEQ 0 (
    echo Backup completed with warnings or was skipped.
)

echo.
echo [2/2] Running Git initialization...
echo.
powershell -ExecutionPolicy Bypass -File "%~dp0init-git.ps1"

echo.
echo ============================================
echo   Done!
echo ============================================
echo.
pause
