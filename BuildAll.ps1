#!/usr/bin/env pwsh

# ============================================================
# Unity6 LTS Build Script - LavosTrial
# Builds for Windows, macOS, and Linux
# ============================================================
# Configuration: Edit these variables for your setup
# ============================================================

# Option 1: Auto-detect Unity from current directory structure
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$UnityPath = "D:\travaux_Unity\6000.3.7f1\Editor\Unity.exe"

# Option 2: Set your Unity path here (uncomment and modify)
# $UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.exe"

# Option 3: Use environment variable (set UNITY_PATH in your system)
if ([string]::IsNullOrEmpty($UnityPath) -or $UnityPath -eq "D:\travaux_Unity\6000.3.7f1\Editor\Unity.exe") {
    if (-not [string]::IsNullOrEmpty($env:UNITY_PATH)) {
        $UnityPath = $env:UNITY_PATH
    }
}

# Validate Unity path
if (-not (Test-Path $UnityPath)) {
    Write-Host "[ERROR] Unity executable not found at: $UnityPath" -ForegroundColor Red
    Write-Host "Please set UNITY_PATH environment variable or edit `$UnityPath in this script." -ForegroundColor Yellow
    exit 1
}

# Project path is always relative to script location
$ProjectPath = Resolve-Path $ScriptDir
$OutputName = "LavosTrial"
$BuildSummary = Join-Path $ProjectPath "build_summary.log"

# ============================================================
# Pre-Build Verification
# ============================================================

function Test-PreBuildConditions {
    param(
        [string]$ProjectPath
    )
    
    Write-Host ""
    Write-Host "[VERIFY] Running pre-build verification..." -ForegroundColor Cyan
    
    $errors = 0
    
    # Check 1: Required folders exist
    $requiredFolders = @("Assets", "ProjectSettings", "Packages")
    foreach ($folder in $requiredFolders) {
        $folderPath = Join-Path $ProjectPath $folder
        if (-not (Test-Path $folderPath -PathType Container)) {
            Write-Host "[ERROR] Required folder missing: $folderPath" -ForegroundColor Red
            $errors++
        }
    }
    
    # Check 2: Main scene exists
    $mainScene = Join-Path $ProjectPath "Assets\Scenes\MainScene_Maze.unity"
    if (-not (Test-Path $mainScene)) {
        Write-Host "[WARNING] Main scene not found: $mainScene" -ForegroundColor Yellow
    }
    
    # Check 3: BuildScript.cs exists (required for builds)
    $buildScript = Join-Path $ProjectPath "Assets\Scripts\Editor\BuildScript.cs"
    if (-not (Test-Path $buildScript)) {
        Write-Host "[WARNING] BuildScript.cs not found - builds may fail" -ForegroundColor Yellow
    }
    
    # Check 4: Packages manifest exists
    $manifest = Join-Path $ProjectPath "Packages\manifest.json"
    if (-not (Test-Path $manifest)) {
        Write-Host "[ERROR] Packages manifest missing: $manifest" -ForegroundColor Red
        $errors++
    }
    
    # Check 5: Sufficient disk space (at least 5GB free)
    try {
        $disk = Get-PSDrive -Name (Split-Path $ProjectPath -Qualifier).TrimEnd(':')
        $freeSpaceGB = [math]::Round($disk.Free / 1GB, 2)
        if ($disk.Free / 1GB -lt 5) {
            Write-Host "[WARNING] Low disk space: ${freeSpaceGB}GB free (recommended: 5GB+)" -ForegroundColor Yellow
        } else {
            Write-Host "[OK] Disk space: ${freeSpaceGB}GB free" -ForegroundColor Green
        }
    } catch {
        Write-Host "[INFO] Could not check disk space" -ForegroundColor Gray
    }
    
    if ($errors -gt 0) {
        Write-Host "[ERROR] Pre-build verification failed with $errors error(s)" -ForegroundColor Red
        return $false
    }
    
    Write-Host "[OK] Pre-build verification passed" -ForegroundColor Green
    return $true
}

# ============================================================
# Post-Build Verification
# ============================================================

function Test-PostBuildVerification {
    param(
        [string]$BuildPath,
        [string]$Platform
    )
    
    Write-Host "[VERIFY] Verifying $Platform build..." -ForegroundColor Cyan
    
    if (-not (Test-Path $BuildPath -PathType Container)) {
        Write-Host "[ERROR] Build output folder not created: $BuildPath" -ForegroundColor Red
        return $false
    }
    
    # Check for expected output files based on platform
    $expectedFiles = @()
    switch ($Platform) {
        "Windows" {
            $expectedFiles = @(
                (Join-Path $BuildPath "$OutputName.exe"),
                (Join-Path $BuildPath "$OutputName_Data")
            )
        }
        "Mac" {
            $expectedFiles = @(
                (Join-Path $BuildPath "$OutputName.app")
            )
        }
        "Linux" {
            $expectedFiles = @(
                (Join-Path $BuildPath "$OutputName.x86_64"),
                (Join-Path $BuildPath "${OutputName}_Data")
            )
        }
    }
    
    $allExist = $true
    foreach ($file in $expectedFiles) {
        if (-not (Test-Path $file)) {
            Write-Host "[WARNING] Expected file/folder not found: $file" -ForegroundColor Yellow
            $allExist = $false
        }
    }
    
    # Check build size (warn if < 50MB, might indicate incomplete build)
    try {
        $buildSize = (Get-ChildItem $BuildPath -Recurse -File | Measure-Object -Property Length -Sum).Sum
        $buildSizeMB = [math]::Round($buildSize / 1MB, 2)
        if ($buildSizeMB -lt 50) {
            Write-Host "[WARNING] Build size unusually small: ${buildSizeMB}MB" -ForegroundColor Yellow
        } else {
            Write-Host "[OK] Build size: ${buildSizeMB}MB" -ForegroundColor Green
        }
    } catch {
        Write-Host "[INFO] Could not check build size" -ForegroundColor Gray
    }
    
    return $allExist
}

# ============================================================
# Build Execution
# ============================================================

# Create or clear build summary
@"
=========================================
Unity6 LTS Build Summary - $OutputName
Build Date: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
=========================================

"@ | Out-File -FilePath $BuildSummary -Encoding UTF8

# Run pre-build verification
if (-not (Test-PreBuildConditions -ProjectPath $ProjectPath)) {
    Write-Host "[ERROR] Pre-build verification failed. Aborting builds." -ForegroundColor Red
    exit 1
}

$BuildSuccess = 0
$BuildFailed = 0
$BuildVerified = 0

function Invoke-UnityBuild {
    param(
        [string]$Platform,
        [string]$BuildPath
    )

    Write-Host ""
    Write-Host "[BUILD] Starting build for $Platform..." -ForegroundColor Cyan

    $BuildLog = Join-Path $ProjectPath "build_log_$Platform.log"

    $Args = @(
        "-batchmode",
        "-quit",
        "-projectPath", $ProjectPath,
        "-executeMethod", "BuildScript.PerformBuild",
        $Platform,
        $BuildPath,
        $OutputName
    )

    & $UnityPath $Args 2>&1 | Out-File -FilePath $BuildLog -Encoding UTF8

    if ($LASTEXITCODE -eq 0) {
        Write-Host "[SUCCESS] $Platform build completed successfully" -ForegroundColor Green
        "[SUCCESS] $Platform build completed successfully" | Out-File -FilePath $BuildSummary -Append -Encoding UTF8
        $script:BuildSuccess++
        
        # Run post-build verification
        if (Test-PostBuildVerification -BuildPath $BuildPath -Platform $Platform) {
            $script:BuildVerified++
            "[VERIFIED] $Platform build output verified" | Out-File -FilePath $BuildSummary -Append -Encoding UTF8
        } else {
            Write-Host "[WARNING] $Platform build verification failed" -ForegroundColor Yellow
            "[WARNING] $Platform build verification failed" | Out-File -FilePath $BuildSummary -Append -Encoding UTF8
        }
    } else {
        Write-Host "[FAILED] $Platform build failed. Check $BuildLog" -ForegroundColor Red
        "[FAILED] $Platform build failed. Check $BuildLog" | Out-File -FilePath $BuildSummary -Append -Encoding UTF8
        $script:BuildFailed++
    }
}

# Build for Windows
Invoke-UnityBuild -Platform "Windows" -BuildPath (Join-Path $ProjectPath "Build\Windows")

# Build for macOS
Invoke-UnityBuild -Platform "Mac" -BuildPath (Join-Path $ProjectPath "Build\Mac")

# Build for Linux
Invoke-UnityBuild -Platform "Linux" -BuildPath (Join-Path $ProjectPath "Build\Linux")

# Print summary
Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "BUILD SUMMARY" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Windows:   $BuildSuccess succeeded, $BuildFailed failed" -ForegroundColor $(if ($BuildFailed -gt 0) { "Red" } else { "Green" })
Write-Host "macOS:     $BuildSuccess succeeded, $BuildFailed failed" -ForegroundColor $(if ($BuildFailed -gt 0) { "Red" } else { "Green" })
Write-Host "Linux:     $BuildSuccess succeeded, $BuildFailed failed" -ForegroundColor $(if ($BuildFailed -gt 0) { "Red" } else { "Green" })
Write-Host ""
Write-Host "Verified:  $BuildVerified build(s) passed verification" -ForegroundColor $(if ($BuildVerified -lt 3) { "Yellow" } else { "Green" })
Write-Host ""
Write-Host "Full log saved to: $BuildSummary" -ForegroundColor Yellow
Write-Host "=========================================" -ForegroundColor Cyan

@"

=========================================
FINAL SUMMARY
=========================================
Builds:      $BuildSuccess succeeded, $BuildFailed failed
Verified:    $BuildVerified build(s) passed verification
=========================================
"@ | Out-File -FilePath $BuildSummary -Append -Encoding UTF8

if ($BuildFailed -gt 0) {
    exit 1
} else {
    exit 0
}
