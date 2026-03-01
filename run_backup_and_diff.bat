@echo off
REM Run backup and show diff for changed files
REM Date: 2026-02-28

cd /d "%~dp0"

echo ============================================
echo   Unity 6 Project - Backup and Diff
echo   Date: %DATE% %TIME%
echo ============================================
echo.

echo Running backup.ps1...
powershell -ExecutionPolicy Bypass -File "%~dp0backup.ps1"

echo.
echo ============================================
echo   Diff Files Created
echo ============================================
echo.
echo The following files were modified:
echo   1. Assets/Scripts/HUD/HUDSystem.cs
echo      - Removed duplicate using UnityEngine.InputSystem;
echo.
echo   2. Assets/Scripts/Editor/BuildScript.cs
echo      - Removed unused using UnityEngine.InputSystem;
echo.
echo Diff files saved to: diff_tmp\
echo.
