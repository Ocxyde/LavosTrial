# scan-project-errors.ps1
# Scan Unity 6 project for errors and issues
# UTF-8 encoding - Unix line endings
#
# Usage: powershell -ExecutionPolicy Bypass -File scan-project-errors.ps1

$ErrorActionPreference = "SilentlyContinue"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Unity 6 Project Error Scanner" -ForegroundColor White
Write-Host "  Version: 6000.3.7f1 (URP Standard)" -ForegroundColor DarkGray
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$issues = @()
$warnings = @()
$info = @()

# Scan C# files
Write-Host "[Scan] Checking C# scripts..." -ForegroundColor Yellow
$csFiles = Get-ChildItem -Path "Assets\Scripts" -Filter "*.cs" -Recurse -ErrorAction SilentlyContinue

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
    
    # Check for missing file headers
    if ($content -notmatch "^//\s.*\.cs") {
        $issues += "MISSING HEADER: $($file.FullName)"
    }
    
    # Check for old Input System usage
    if ($content -match "Input\.Get(Axis|Button|Key)") {
        $issues += "OLD INPUT SYSTEM: $($file.FullName) - Use New Input System instead"
    }
    
    # Check for deprecated Unity API
    if ($content -match "FindObjectOfType" -and $content -notmatch "//.*FindObjectOfType") {
        $warnings += "PERFORMANCE: $($file.FullName) - FindObjectOfType can be slow, cache reference"
    }
    
    # Check for IL2CPP incompatible code
    if ($content -match "\.GetType\(\)") {
        $info += "IL2CPP CHECK: $($file.FullName) - Ensure type is preserved in link.xml"
    }
    
    # Check for proper namespace
    if ($content -match "namespace\s+Code\.Lavos") {
        # Good - using correct namespace
    } elseif ($content -match "namespace\s+") {
        $info += "NAMESPACE: $($file.FullName) - Uses non-standard namespace"
    }
}

Write-Host "  Found $($csFiles.Count) C# files." -ForegroundColor Gray
Write-Host ""

# Check URP settings
Write-Host "[Scan] Checking URP configuration..." -ForegroundColor Yellow
$urpAsset = Get-ChildItem -Path "Assets" -Filter "*UniversalRenderPipeline*.asset" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
if ($urpAsset) {
    Write-Host "  [OK] URP Asset found: $($urpAsset.Name)" -ForegroundColor Green
} else {
    $issues += "URP MISSING: No Universal Render Pipeline asset found"
}

$graphicsSettings = "ProjectSettings\GraphicsSettings.asset"
if (Test-Path $graphicsSettings) {
    $content = Get-Content $graphicsSettings -Raw
    # Check if m_CustomRenderPipeline has a valid guid reference (not null)
    # URP is configured if m_CustomRenderPipeline points to an asset (has a non-zero guid)
    if ($content -match "m_CustomRenderPipeline:\s*\{fileID:\s*\d+,\s*guid:\s*[a-f0-9]{32}") {
        Write-Host "  [OK] URP is set as active render pipeline" -ForegroundColor Green
    } elseif ($content -match "m_CustomRenderPipeline:\s*\{fileID:\s*0") {
        $warnings += "URP WARNING: GraphicsSettings has null render pipeline"
    } else {
        $warnings += "URP WARNING: Could not verify URP configuration in GraphicsSettings"
    }
} else {
    $warnings += "URP WARNING: GraphicsSettings.asset not found"
}
Write-Host ""

# Check Input System
Write-Host "[Scan] Checking Input System..." -ForegroundColor Yellow
$inputActions = Get-ChildItem -Path "Assets" -Filter "*.inputactions" -Recurse -ErrorAction SilentlyContinue
if ($inputActions.Count -gt 0) {
    Write-Host "  [OK] $($inputActions.Count) Input System action file(s) found" -ForegroundColor Green
    foreach ($file in $inputActions) {
        Write-Host "    - $($file.Name)" -ForegroundColor Gray
    }
} else {
    $warnings += "INPUT: No .inputactions file found"
}

$inputSystemSettings = "ProjectSettings\InputManager.asset"
if (Test-Path $inputSystemSettings) {
    $content = Get-Content $inputSystemSettings -Raw
    if ($content -match "EnableInputActions") {
        Write-Host "  [OK] New Input System is enabled" -ForegroundColor Green
    }
}
Write-Host ""

# Check for meta files
Write-Host "[Scan] Checking Unity meta files..." -ForegroundColor Yellow
$missingMeta = @()
foreach ($file in $csFiles) {
    $metaFile = "$($file.FullName).meta"
    if (-not (Test-Path $metaFile)) {
        $missingMeta += $file.Name
    }
}
if ($missingMeta.Count -gt 0) {
    $warnings += "META FILES: $($missingMeta.Count) script(s) missing .meta files"
} else {
    Write-Host "  [OK] All scripts have meta files" -ForegroundColor Green
}
Write-Host ""

# Check diff_tmp folder
Write-Host "[Scan] Checking diff_tmp folder..." -ForegroundColor Yellow
$diffTmp = "diff_tmp"
if (Test-Path $diffTmp) {
    $diffFiles = Get-ChildItem -Path $diffTmp -File -ErrorAction SilentlyContinue
    Write-Host "  Found $($diffFiles.Count) diff file(s)" -ForegroundColor Gray
    $cutoffDate = (Get-Date).AddDays(-2)
    $oldFiles = $diffFiles | Where-Object { $_.LastWriteTime -lt $cutoffDate }
    if ($oldFiles.Count -gt 0) {
        $info += "DIFF CLEANUP: $($oldFiles.Count) diff file(s) older than 2 days"
    }
} else {
    $info += "DIFF_TMP: Folder does not exist"
}
Write-Host ""

# Summary
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Scan Summary" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

if ($issues.Count -gt 0) {
    Write-Host "ISSUES ($($issues.Count)) - Should be fixed:" -ForegroundColor Red
    foreach ($issue in $issues) {
        Write-Host "  [!] $issue" -ForegroundColor Red
    }
    Write-Host ""
}

if ($warnings.Count -gt 0) {
    Write-Host "WARNINGS ($($warnings.Count)) - Should be reviewed:" -ForegroundColor Yellow
    foreach ($warning in $warnings) {
        Write-Host "  [*] $warning" -ForegroundColor Yellow
    }
    Write-Host ""
}

if ($info.Count -gt 0) {
    Write-Host "INFO ($($info.Count)) - For your information:" -ForegroundColor Gray
    foreach ($item in $info) {
        Write-Host "  [i] $item" -ForegroundColor Gray
    }
    Write-Host ""
}

if ($issues.Count -eq 0 -and $warnings.Count -eq 0) {
    Write-Host "  No critical issues found!" -ForegroundColor Green
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Scan Complete" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
