@echo off
REM ============================================================
REM  setup-git-scheduler.bat - Run Scheduler Setup as Admin
REM  Right-click this file and select "Run as Administrator"
REM ============================================================

cd /d "%~dp0"

echo.
echo ============================================
echo   LavosTrial - Git Scheduler Setup
echo ============================================
echo.
echo   This will create a Windows Task Scheduler task
echo   to auto-commit your Assets/ folder every 24 hours.
echo.
echo   IMPORTANT: This script requires Administrator privileges.
echo.
pause

REM Check if running as admin
net session >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo   Requesting Administrator privileges...
    echo.
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

REM Run the PowerShell setup script
powershell -ExecutionPolicy Bypass -File "%~dp0setup-git-scheduler.ps1"

echo.
pause
