@echo off
REM ============================================================
REM  fix_critical_bugs.bat
REM  Fix critical compilation errors in Core/ and Ressources/
REM ============================================================

echo ============================================
echo   Fix Critical Bugs
echo ============================================
echo.

cd /d "%~dp0"

echo 1. Creating shared ItemTypes.cs in Core/...
if not exist "Assets\Scripts\Core\ItemTypes.cs" (
    echo    [OK] ItemTypes.cs will be created manually
) else (
    echo    [EXISTS] ItemTypes.cs already exists
)

echo.
echo 2. Checking assembly definitions...

REM --- Fix Ressources.asmdef (remove unused Status reference) ---
echo    Updating Code.Lavos.Ressources.asmdef...
powershell -Command ^
    "$asmdef = Get-Content 'Assets\Scripts\Ressources\Code.Lavos.Ressources.asmdef' -Raw; ^
    $asmdef = $asmdef -replace '\"Code\.Lavos\.Status\",?\s*', ''; ^
    $asmdef | Set-Content 'Assets\Scripts\Ressources\Code.Lavos.Ressources.asmdef' -Encoding UTF8"

echo.
echo ============================================
echo   MANUAL STEPS REQUIRED
echo ============================================
echo.
echo 1. Verify ItemTypes.cs was created in Core/
echo    (Defines: ItemType, DoorType enums)
echo.
echo 2. Open Unity Editor and check for errors
echo.
echo 3. Run backup.ps1
echo.
pause
