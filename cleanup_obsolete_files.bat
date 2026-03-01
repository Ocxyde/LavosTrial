@echo off
REM ============================================================
REM  cleanup_obsolete_files.bat
REM  Delete obsolete and unused files from project
REM  Unity 6 Compatible - UTF-8 Encoding
REM ============================================================

echo ============================================
echo   Cleanup Obsolete Files
echo ============================================
echo.

cd /d "%~dp0"

echo Deleting obsolete files...
echo.

REM --- Obsolete Editor Scripts ---
if exist "Assets\Scripts\Editor\TestHUD.cs" (
    del /Q "Assets\Scripts\Editor\TestHUD.cs"
    echo   Deleted: Assets\Scripts\Editor\TestHUD.cs
) else (
    echo   [SKIP] Assets\Scripts\Editor\TestHUD.cs not found
)

if exist "Assets\Scripts\Editor\HUDSetupUtility.cs" (
    del /Q "Assets\Scripts\Editor\HUDSetupUtility.cs"
    echo   Deleted: Assets\Scripts\Editor\HUDSetupUtility.cs
) else (
    echo   [SKIP] Assets\Scripts\Editor\HUDSetupUtility.cs not found
)

REM --- Obsolete Core Scripts ---
if exist "Assets\Scripts\Core\TetrahedronMesh.cs" (
    del /Q "Assets\Scripts\Core\TetrahedronMesh.cs"
    echo   Deleted: Assets\Scripts\Core\TetrahedronMesh.cs
) else (
    echo   [SKIP] Assets\Scripts\Core\TetrahedronMesh.cs not found
)

REM --- Obsolete Tetris Scripts ---
if exist "Assets\Scripts\Tetris\Tetromino.cs" (
    del /Q "Assets\Scripts\Tetris\Tetromino.cs"
    echo   Deleted: Assets\Scripts\Tetris\Tetromino.cs
) else (
    echo   [SKIP] Assets\Scripts\Tetris\Tetromino.cs not found
)

REM --- Unused Chat Logs ---
if exist "chat-b51cc06b-b2d2-400f-ad93-47cb0cb64874.txt" (
    del /Q "chat-b51cc06b-b2d2-400f-ad93-47cb0cb64874.txt"
    echo   Deleted: chat-b51cc06b-b2d2-400f-ad93-47cb0cb64874.txt
) else (
    echo   [SKIP] chat-b51cc06b-b2d2-400f-ad93-47cb0cb64874.txt not found
)

if exist "chat-be601907-edbd-40b1-b4de-5a343663b5c0.txt" (
    del /Q "chat-be601907-edbd-40b1-b4de-5a343663b5c0.txt"
    echo   Deleted: chat-be601907-edbd-40b1-b4de-5a343663b5c0.txt
) else (
    echo   [SKIP] chat-be601907-edbd-40b1-b4de-5a343663b5c0.txt not found
)

echo.
echo ============================================
echo   Cleanup Complete!
echo ============================================
echo.
echo NEXT STEPS:
echo   1. Run backup.ps1 to backup changes
echo   2. Verify in Unity Editor
echo.
pause
