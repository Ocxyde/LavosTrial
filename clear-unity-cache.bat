# clear-unity-cache.bat
@echo off
echo ============================================
echo   Clear Unity Cache and Force Recompile
echo ============================================
echo.
echo Closing Unity if open...
taskkill /F /IM Unity.exe 2>nul
timeout /t 2 /nobreak >nul

echo.
echo Deleting Library folder...
if exist "Library" (
    rmdir /s /q "Library"
    echo Library deleted.
) else (
    echo Library folder not found.
)

echo.
echo Deleting obj folder...
if exist "obj" (
    rmdir /s /q "obj"
    echo obj deleted.
) else (
    echo obj folder not found.
)

echo.
echo Deleting Temp folder...
if exist "Temp" (
    rmdir /s /q "Temp"
    echo Temp deleted.
) else (
    echo Temp folder not found.
)

echo.
echo ============================================
echo   Cache Cleared!
echo ============================================
echo.
echo Next Steps:
echo   1. Open Unity 6 Editor (6000.3.7f1)
echo   2. Wait for full recompilation (may take 2-5 minutes)
echo   3. Check Console for errors
echo.
pause
