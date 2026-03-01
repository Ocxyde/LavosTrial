@echo off
REM ============================================================
REM  Git LF Line Endings Setup - Unity 6 Project
REM  Usage: Run this once, then use git-normalize.bat for future changes
REM ============================================================

echo.
echo ============================================
echo  Git LF Line Endings Configuration
echo ============================================
echo.

REM --- Configure Git for LF (repository-level) ---
echo [1/4] Configuring Git for LF line endings...
git config core.autocrlf input
git config core.eol lf
echo       Done: core.autocrlf = input
echo       Done: core.eol = lf
echo.

REM --- Create .gitattributes if not exists ---
if not exist ".gitattributes" (
    echo [2/4] Creating .gitattributes...
    (
        echo # Force LF for all text files
        echo * text=auto eol=lf
        echo.
        echo # C# files
        echo *.cs text eol=lf
        echo *.asmdef text eol=lf
        echo.
        echo # Unity YAML
        echo *.unity text eol=lf
        echo *.prefab text eol=lf
        echo *.asset text eol=lf
        echo *.mat text eol=lf
        echo *.controller text eol=lf
        echo *.anim text eol=lf
        echo.
        echo # Shaders
        echo *.shader text eol=lf
        echo *.hlsl text eol=lf
        echo *.cginc text eol=lf
        echo.
        echo # Text and config
        echo *.json text eol=lf
        echo *.xml text eol=lf
        echo *.md text eol=lf
        echo *.txt text eol=lf
        echo.
        echo # Scripts
        echo *.sh text eol=lf
        echo *.ps1 text eol=lf
        echo *.bash text eol=lf
        echo.
        echo # Keep CRLF for Windows batch files
        echo *.bat text eol=crlf
        echo *.cmd text eol=crlf
    ) > ".gitattributes"
    echo       Created: .gitattributes
) else (
    echo [2/4] .gitattributes already exists - skipping
)
echo.

REM --- Normalize existing files ---
echo [3/4] Normalizing existing files to LF...
git rm --cached -r .
git add --renormalize .
echo       Done: Files normalized
echo.

REM --- Prompt for commit ---
echo [4/4] Committing changes...
git commit -m "Normalize line endings to LF"
if %ERRORLEVEL% EQU 0 (
    echo       Done: Changes committed
) else (
    echo       No changes to commit or commit failed
)
echo.

echo ============================================
echo  Setup Complete!
echo ============================================
echo.
echo  Your Git repo now uses Unix LF line endings.
echo  For future changes, use: git-normalize.bat
echo.
pause
