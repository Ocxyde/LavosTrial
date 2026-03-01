@echo off
setlocal enabledelayedexpansion

:: ============================================================
:: Unity6 LTS Build Script - LavosTrial
:: Builds for Windows, macOS, and Linux
:: ============================================================

set "UNITY_PATH=D:\travaux_Unity\6000.3.7f1\Editor\Unity.exe"
set "PROJECT_PATH=D:\travaux_Unity\PeuImporte"
set "OUTPUT_NAME=LavosTrial"
set "BUILD_SUMMARY=%PROJECT_PATH%\build_summary.log"

:: Create or clear build summary
echo ========================================= > "%BUILD_SUMMARY%"
echo Unity6 LTS Build Summary - %OUTPUT_NAME% >> "%BUILD_SUMMARY%"
echo Build Date: %date% %time% >> "%BUILD_SUMMARY%"
echo ========================================= >> "%BUILD_SUMMARY%"
echo. >> "%BUILD_SUMMARY%"

set "BUILD_SUCCESS=0"
set "BUILD_FAILED=0"

:: Function to run Unity build
:run_build
set "PLATFORM=%~1"
set "BUILD_PATH=%~2"

echo.
echo [BUILD] Starting build for %PLATFORM%...
echo [BUILD] Starting build for %PLATFORM%... >> "%BUILD_SUMMARY%"

"%UNITY_PATH%" -batchmode -quit -projectPath "%PROJECT_PATH%" -executeMethod BuildScript.PerformBuild %PLATFORM% %BUILD_PATH% %OUTPUT_NAME% >> "%PROJECT_PATH%\build_log_%PLATFORM%.log" 2>&1

if %errorlevel% equ 0 (
    echo [SUCCESS] %PLATFORM% build completed successfully
    echo [SUCCESS] %PLATFORM% build completed successfully >> "%BUILD_SUMMARY%"
    set /a BUILD_SUCCESS+=1
) else (
    echo [FAILED] %PLATFORM% build failed. Check build_log_%PLATFORM%.log
    echo [FAILED] %PLATFORM% build failed. Check build_log_%PLATFORM%.log >> "%BUILD_SUMMARY%"
    set /a BUILD_FAILED+=1
)

exit /b 0

:: Build for Windows
call :run_build "Windows" "%PROJECT_PATH%\Build\Windows"

:: Build for macOS
call :run_build "Mac" "%PROJECT_PATH%\Build\Mac"

:: Build for Linux
call :run_build "Linux" "%PROJECT_PATH%\Build\Linux"

:: Print summary
echo.
echo =========================================
echo BUILD SUMMARY
echo =========================================
echo Windows:   %BUILD_SUCCESS% succeeded, %BUILD_FAILED% failed
echo.
echo Full log saved to: %BUILD_SUMMARY%
echo =========================================

echo. >> "%BUILD_SUMMARY%"
echo ========================================= >> "%BUILD_SUMMARY%"
echo FINAL SUMMARY >> "%BUILD_SUMMARY%"
echo ========================================= >> "%BUILD_SUMMARY%"
echo Windows:   %BUILD_SUCCESS% succeeded, %BUILD_FAILED% failed >> "%BUILD_SUMMARY%"
echo ========================================= >> "%BUILD_SUMMARY%"

if %BUILD_FAILED% gtr 0 (
    exit /b 1
) else (
    exit /b 0
)
