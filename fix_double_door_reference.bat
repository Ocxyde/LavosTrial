@echo off
REM ============================================================
REM  fix_double_door_reference.bat
REM  Move DoubleDoor.cs to Core/ (needed by SpawnPlacerEngine)
REM ============================================================

echo ============================================
echo   Move DoubleDoor to Core
echo ============================================
echo.

cd /d "%~dp0"

echo Moving DoubleDoor.cs to Core/...
echo.

if exist "Assets\Scripts\Ressources\DoubleDoor.cs" (
    move /Y "Assets\Scripts\Ressources\DoubleDoor.cs" "Assets\Scripts\Core\DoubleDoor.cs"
    echo   [OK] Moved: DoubleDoor.cs
) else (
    echo   [SKIP] DoubleDoor.cs not found
)

if exist "Assets\Scripts\Ressources\DoubleDoor.cs.meta" (
    move /Y "Assets\Scripts\Ressources\DoubleDoor.cs.meta" "Assets\Scripts\Core\DoubleDoor.cs.meta"
    echo   [OK] Moved: DoubleDoor.cs.meta
) else (
    echo   [SKIP] DoubleDoor.cs.meta not found
)

echo.
echo ============================================
echo   Move Complete!
echo ============================================
echo.
echo DoubleDoor.cs is now in Core/
echo SpawnPlacerEngine.cs can now reference it
echo.
echo NEXT STEPS:
echo   1. Run backup.ps1
echo   2. Open Unity Editor
echo.
pause
