@echo off
REM ============================================================
REM  move_torch_to_core.bat
REM  Move TorchPool and related files from Ressources to Core
REM  to fix cyclic dependency issue
REM ============================================================

echo ============================================
echo   Move Torch Files to Core
echo ============================================
echo.

cd /d "%~dp0"

echo Moving files...
echo.

REM --- Move TorchPool.cs ---
if exist "Assets\Scripts\Ressources\TorchPool.cs" (
    move /Y "Assets\Scripts\Ressources\TorchPool.cs" "Assets\Scripts\Core\TorchPool.cs"
    echo   Moved: TorchPool.cs
) else (
    echo   [SKIP] TorchPool.cs not found
)

REM --- Move TorchPool.cs.meta ---
if exist "Assets\Scripts\Ressources\TorchPool.cs.meta" (
    move /Y "Assets\Scripts\Ressources\TorchPool.cs.meta" "Assets\Scripts\Core\TorchPool.cs.meta"
    echo   Moved: TorchPool.cs.meta
) else (
    echo   [SKIP] TorchPool.cs.meta not found
)

REM --- Move TorchController.cs ---
if exist "Assets\Scripts\Ressources\TorchController.cs" (
    move /Y "Assets\Scripts\Ressources\TorchController.cs" "Assets\Scripts\Core\TorchController.cs"
    echo   Moved: TorchController.cs
) else (
    echo   [SKIP] TorchController.cs not found
)

REM --- Move TorchController.cs.meta ---
if exist "Assets\Scripts\Ressources\TorchController.cs.meta" (
    move /Y "Assets\Scripts\Ressources\TorchController.cs.meta" "Assets\Scripts\Core\TorchController.cs.meta"
    echo   Moved: TorchController.cs.meta
) else (
    echo   [SKIP] TorchController.cs.meta not found
)

echo.
echo ============================================
echo   Move Complete!
echo ============================================
echo.
echo NEXT STEPS:
echo   1. Run backup.ps1 to backup changes
echo   2. Verify in Unity Editor
echo.
pause
