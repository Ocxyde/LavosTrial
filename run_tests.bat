@echo off
REM ============================================================================
REM  CodeDotLavos - Unity Test Suite Runner
REM  Unity 6000.3.10f1 | NUnit Test Framework
REM ============================================================================

setlocal enabledelayedexpansion

REM ─────────────────────────────────────────────────────────────────────────────
REM Configuration
REM ─────────────────────────────────────────────────────────────────────────────
set "UNITY_EDITOR=C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe"
set "PROJECT_PATH=D:\travaux_Unity\CodeDotLavos"
set "TEST_RESULTS=%PROJECT_PATH%\TestResults.xml"
set "TEST_LOG=%PROJECT_PATH%\TestLog.log"

REM ─────────────────────────────────────────────────────────────────────────────
REM Header
REM ─────────────────────────────────────────────────────────────────────────────
echo.
echo ============================================================================
echo   CodeDotLavos - Unity Test Suite Runner
echo ============================================================================
echo.
echo   Unity Editor: %UNITY_EDITOR%
echo   Project Path: %PROJECT_PATH%
echo   Test Results: %TEST_RESULTS%
echo   Test Log:     %TEST_LOG%
echo.
echo ============================================================================
echo.

REM ─────────────────────────────────────────────────────────────────────────────
REM Check Unity Editor exists
REM ─────────────────────────────────────────────────────────────────────────────
if not exist "%UNITY_EDITOR%" (
    echo [ERROR] Unity Editor not found at: %UNITY_EDITOR%
    echo.
    echo Please update the UNITY_EDITOR path in this script.
    echo.
    pause
    exit /b 1
)

REM ─────────────────────────────────────────────────────────────────────────────
REM Run Unity Tests
REM ─────────────────────────────────────────────────────────────────────────────
echo [INFO] Starting Unity Test Runner...
echo.
echo   Platform: EditMode
echo   Tests:    All EditMode tests in Assets/Scripts/Tests/
echo.

"%UNITY_EDITOR%" ^
    -batchmode ^
    -quit ^
    -nographics ^
    -projectPath "%PROJECT_PATH%" ^
    -runTests ^
    -testPlatform EditMode ^
    -testResults "%TEST_RESULTS%" ^
    -logFile "%TEST_LOG%"

REM ─────────────────────────────────────────────────────────────────────────────
REM Check Results
REM ─────────────────────────────────────────────────────────────────────────────
echo.
echo ============================================================================
echo   TEST EXECUTION COMPLETE
echo ============================================================================
echo.

if exist "%TEST_RESULTS%" (
    echo [OK] Test results saved to: %TEST_RESULTS%
    echo.
    echo --- Test Results Summary ---
    findstr /C:"passed" /C:"failed" /C:"skipped" "%TEST_RESULTS%" 2>nul
    echo.
) else (
    echo [WARNING] Test results file not found.
    echo.
)

if exist "%TEST_LOG%" (
    echo [OK] Test log saved to: %TEST_LOG%
) else (
    echo [WARNING] Test log file not found.
)

echo.
echo ============================================================================
echo.
echo   Next steps:
echo   1. Open TestResults.xml in a browser for detailed results
echo   2. Check TestLog.log for any errors
echo   3. In Unity: Window → Test Runner → View detailed results
echo.
echo ============================================================================
echo.

pause
