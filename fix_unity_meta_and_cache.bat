@echo off
REM ============================================================
REM  fix_unity_meta_and_cache.bat
REM  1. Fix invalid DoubleDoor.cs.meta GUID
REM  2. Clean Unity Library cache for Tests assembly
REM ============================================================

echo ============================================
echo   Fix Unity Meta and Cache Issues
echo ============================================
echo.

cd /d "%~dp0"

echo 1. Fixing DoubleDoor.cs.meta...
echo.

REM --- Delete invalid meta file so Unity regenerates it ---
if exist "Assets\Scripts\Core\DoubleDoor.cs.meta" (
    del /Q "Assets\Scripts\Core\DoubleDoor.cs.meta"
    echo   [OK] Deleted: DoubleDoor.cs.meta (Unity will regenerate)
) else (
    echo   [SKIP] DoubleDoor.cs.meta not found
)

echo.
echo 2. Cleaning Unity Cache for Tests assembly...
echo.

REM --- Delete Library/ScriptAssemblies to force rebuild ---
if exist "Library\ScriptAssemblies" (
    rmdir /S /Q "Library\ScriptAssemblies"
    echo   [OK] Deleted: Library\ScriptAssemblies
) else (
    echo   [INFO] Library\ScriptAssemblies not found
)

REM --- Delete Temp/obj to force recompilation ---
if exist "Temp\obj" (
    rmdir /S /Q "Temp\obj"
    echo   [OK] Deleted: Temp\obj
) else (
    echo   [INFO] Temp\obj not found
)

echo.
echo ============================================
echo   Cleanup Complete!
echo ============================================
echo.
echo NEXT STEPS:
echo   1. Close Unity Editor completely
echo   2. Wait 5 seconds
echo   3. Reopen Unity Editor
echo   4. Wait for reimport to complete
echo   5. Check Console for errors
echo.
echo Unity will:
echo   - Regenerate DoubleDoor.cs.meta with valid GUID
echo   - Rebuild all assemblies from scratch
echo   - Clear Burst compiler cache
echo.
pause
