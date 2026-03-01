@echo off
REM ============================================================
REM  rename_spawn_placer.bat
REM  Rename ItemPlacerEngine to SpawnPlacerEngine
REM  Unity 6 Compatible - UTF-8 Encoding
REM ============================================================

echo ============================================
echo   Rename ItemPlacerEngine to SpawnPlacerEngine
echo ============================================
echo.

cd /d "%~dp0"

echo Searching for ItemPlacerEngine references...
echo.

REM --- Rename class references in all .cs files ---
echo Updating class references in source files...
powershell -Command ^
    "Get-ChildItem 'Assets\Scripts' -Recurse -Filter '*.cs' -ErrorAction SilentlyContinue | ForEach-Object { ^
        $content = Get-Content $_.FullName -Raw -Encoding UTF8; ^
        if ($content -match 'ItemPlacerEngine') { ^
            $content -replace 'ItemPlacerEngine', 'SpawnPlacerEngine' | Set-Content $_.FullName -Encoding UTF8 -NoNewline; ^
            Write-Host '  Updated: ' $_.Name ^
        } ^
    }"

echo.
echo Renaming physical files...

REM --- Rename the actual .cs file ---
if exist "Assets\Scripts\Ressources\ItemPlacerEngine.cs" (
    ren "Assets\Scripts\Ressources\ItemPlacerEngine.cs" "SpawnPlacerEngine.cs"
    echo   Renamed: ItemPlacerEngine.cs -^> SpawnPlacerEngine.cs
) else (
    echo   [OK] ItemPlacerEngine.cs already renamed or not found
)

REM --- Rename the .meta file if exists ---
if exist "Assets\Scripts\Ressources\ItemPlacerEngine.cs.meta" (
    ren "Assets\Scripts\Ressources\ItemPlacerEngine.cs.meta" "SpawnPlacerEngine.cs.meta"
    echo   Renamed: ItemPlacerEngine.cs.meta -^> SpawnPlacerEngine.cs.meta
) else (
    echo   [SKIP] ItemPlacerEngine.cs.meta not found
)

echo.
echo ============================================
echo   Rename Complete!
echo ============================================
echo.
echo NEXT STEPS:
echo   1. Run backup.ps1 to backup changes
echo   2. Verify in Unity Editor
echo.
pause
