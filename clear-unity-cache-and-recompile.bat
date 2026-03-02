# clear-unity-cache-and-recompile.bat
# Clear Unity cache and force full recompile
# Unity 6 compatible
#
# Usage: Double-click this file in Explorer

@echo off
echo ============================================
echo   Clear Unity Cache and Recompile
echo   %DATE% %TIME%
echo ============================================
echo.

echo [1/4] Closing Unity if running...
taskkill /F /IM Unity.exe 2>nul
timeout /t 2 /nobreak >nul

echo [2/4] Deleting Library folder...
if exist "Library" (
    rmdir /s /q "Library"
    echo   Library deleted
) else (
    echo   Library not found (already clean)
)

echo [3/4] Deleting Temp folder...
if exist "Temp" (
    rmdir /s /q "Temp"
    echo   Temp deleted
) else (
    echo   Temp not found (already clean)
)

echo [4/4] Deleting obj folder...
if exist "obj" (
    rmdir /s /q "obj"
    echo   obj deleted
) else (
    echo   obj not found (already clean)
)

echo.
echo ============================================
echo   Cache Cleared!
echo ============================================
echo.
echo Next steps:
echo   1. Open Unity Hub
echo   2. Open this project
echo   3. Wait for full recompile (all scripts)
echo   4. Check Console - should be 0 errors
echo.
pause
