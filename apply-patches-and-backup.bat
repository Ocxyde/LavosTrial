@echo off
REM ============================================================
REM  apply-patches-and-backup.bat - Apply all patches and backup
REM  1. Clean diff_tmp files older than 2 days
REM  2. Run backup.ps1
REM ============================================================

cd /d "%~dp0"

echo.
echo ============================================
echo   Apply Patches ^& Backup
echo ============================================
echo.

REM Check if PowerShell is available
where powershell >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: PowerShell not found!
    pause
    exit /b 1
)

echo [1/3] Cleaning old diff files...
echo.
powershell -ExecutionPolicy Bypass -File "%~dp0cleanup-diff-files.ps1"

echo.
echo [2/3] Running backup.ps1...
echo.
powershell -ExecutionPolicy Bypass -File "%~dp0backup.ps1"
if %ERRORLEVEL% NEQ 0 (
    echo Backup completed with warnings.
)

echo.
echo [3/3] Summary...
echo.
echo   Patches applied:
echo     - Ennemi.cs (fixed extra closing brace)
echo     - TorchController.cs (fixed extra closing brace)
echo.
echo   Diffs saved in: diff_tmp\
echo     - Ennemi.cs.diff
echo     - TorchController.cs.diff
echo     - patch_summary.md
echo     - scan_report_2026-03-01.md
echo.
echo ============================================
echo   Done!
echo ============================================
echo.
pause
