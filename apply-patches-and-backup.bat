@echo off
REM apply-patches-and-backup.bat
REM Applies all critical patches and runs backup
REM Unity 6 compatible - UTF-8 encoding - Unix line endings

echo ========================================
echo   Apply Patches and Backup Script
echo ========================================
echo.
echo This script will:
echo   1. Clean up old diff files (older than 2 days)
echo   2. Show summary of patches to apply
echo   3. Run backup.ps1
echo.
echo NOTE: The patches have already been applied to the files.
echo       This script just cleans up and creates a backup.
echo.
pause

echo.
echo [1/3] Cleaning up old diff files...
powershell.exe -ExecutionPolicy Bypass -File "%~dp0cleanup-old-diffs.ps1"

echo.
echo [2/3] Listing current diff files...
dir /b "%~dp0diff_tmp\*.diff" 2>nul
if %errorlevel% neq 0 echo No diff files found.

echo.
echo [3/3] Running backup...
powershell.exe -ExecutionPolicy Bypass -File "%~dp0backup.ps1"

echo.
echo ========================================
echo   Complete!
echo ========================================
echo.
echo Next steps:
echo   1. Open Unity 6 (6000.3.7f1)
echo   2. Check Console for any errors
echo   3. Test in Play Mode
echo.
pause
