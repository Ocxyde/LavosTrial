@echo off
echo ============================================
echo   Clear Unity Package Cache
echo ============================================
echo.
echo This will delete the Library\PackageCache folder.
echo Unity will re-download all packages on next launch.
echo.
pause
echo.
echo Deleting PackageCache...
rmdir /s /q "%~dp0Library\PackageCache"
echo.
echo Done! PackageCache cleared.
echo.
echo Now open Unity and let it reimport packages.
echo ============================================
pause
