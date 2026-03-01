@echo off
REM ============================================================
REM  fix_cyclic_dependency.bat
REM  Respect plug-in-and-out architecture:
REM  - Core/ = Main engines (GameManager, MazeGenerator, SpawnPlacerEngine)
REM  - Ressources/ = Plug-in modules (Torch, Items, etc.)
REM ============================================================

echo ============================================
echo   Fix Cyclic Dependency
echo   Respecting Plug-in-and-Out Architecture
echo ============================================
echo.

cd /d "%~dp0"

echo Moving files...
echo.

REM --- Move TorchPool back to Ressources (plug-in) ---
if exist "Assets\Scripts\Core\TorchPool.cs" (
    move /Y "Assets\Scripts\Core\TorchPool.cs" "Assets\Scripts\Ressources\TorchPool.cs"
    echo   [OK] Moved: TorchPool.cs -^> Ressources/ (plug-in)
) else (
    echo   [SKIP] TorchPool.cs not found
)
if exist "Assets\Scripts\Core\TorchPool.cs.meta" (
    move /Y "Assets\Scripts\Core\TorchPool.cs.meta" "Assets\Scripts\Ressources\TorchPool.cs.meta"
    echo   [OK] Moved: TorchPool.cs.meta
) else (
    echo   [SKIP] TorchPool.cs.meta not found
)

REM --- Move TorchController back to Ressources (plug-in) ---
if exist "Assets\Scripts\Core\TorchController.cs" (
    move /Y "Assets\Scripts\Core\TorchController.cs" "Assets\Scripts\Ressources\TorchController.cs"
    echo   [OK] Moved: TorchController.cs -^> Ressources/ (plug-in)
) else (
    echo   [SKIP] TorchController.cs not found
)
if exist "Assets\Scripts\Core\TorchController.cs.meta" (
    move /Y "Assets\Scripts\Core\TorchController.cs.meta" "Assets\Scripts\Ressources\TorchController.cs.meta"
    echo   [OK] Moved: TorchController.cs.meta
) else (
    echo   [SKIP] TorchController.cs.meta not found
)

REM --- Move MazeRenderer to Ressources (uses torch plug-ins) ---
if exist "Assets\Scripts\Core\MazeRenderer.cs" (
    move /Y "Assets\Scripts\Core\MazeRenderer.cs" "Assets\Scripts\Ressources\MazeRenderer.cs"
    echo   [OK] Moved: MazeRenderer.cs -^> Ressources/ (uses plug-ins)
) else (
    echo   [SKIP] MazeRenderer.cs not found
)
if exist "Assets\Scripts\Core\MazeRenderer.cs.meta" (
    move /Y "Assets\Scripts\Core\MazeRenderer.cs.meta" "Assets\Scripts\Ressources\MazeRenderer.cs.meta"
    echo   [OK] Moved: MazeRenderer.cs.meta
) else (
    echo   [SKIP] MazeRenderer.cs.meta not found
)

REM --- Move SpawnPlacerEngine to Core (spawning engine for monsters/items) ---
if exist "Assets\Scripts\Ressources\SpawnPlacerEngine.cs" (
    move /Y "Assets\Scripts\Ressources\SpawnPlacerEngine.cs" "Assets\Scripts\Core\SpawnPlacerEngine.cs"
    echo   [OK] Moved: SpawnPlacerEngine.cs -^> Core/ (spawning engine)
) else (
    echo   [SKIP] SpawnPlacerEngine.cs not found
)
if exist "Assets\Scripts\Ressources\SpawnPlacerEngine.cs.meta" (
    move /Y "Assets\Scripts\Ressources\SpawnPlacerEngine.cs.meta" "Assets\Scripts\Core\SpawnPlacerEngine.cs.meta"
    echo   [OK] Moved: SpawnPlacerEngine.cs.meta
) else (
    echo   [SKIP] SpawnPlacerEngine.cs.meta not found
)

echo.
echo ============================================
echo   Move Complete!
echo ============================================
echo.
echo Architecture organized:
echo.
echo Core/ (main engines):
echo   - GameManager.cs           (game state engine)
echo   - MazeGenerator.cs         (maze generation engine)
echo   - SpawnPlacerEngine.cs     (spawning engine - monsters/items)
echo   - DrawingManager.cs        (drawing engine)
echo   - ParticleGenerator.cs     (particle engine)
echo.
echo Ressources/ (plug-in modules):
echo   - Torch System (plug-in)
echo   │   ├── TorchPool.cs
echo   │   ├── TorchController.cs
echo   │   ├── BraseroFlame.cs
echo   │   └── FlameAnimator.cs
echo   - Item System (plug-in)
echo   │   ├── ItemEngine.cs
echo   │   ├── ItemBehavior.cs
echo   │   ├── DoubleDoor.cs
echo   │   └── ChestBehavior.cs
echo   - MazeRenderer.cs (renders maze, uses torch plug-ins)
echo.
echo NEXT STEPS:
echo   1. Run backup.ps1 to backup changes
echo   2. Open Unity Editor
echo   3. Verify no cyclic dependency errors
echo.
pause
