# MAZE GENERATION VERIFICATION SCRIPT
# Run this to verify CompleteMazeBuilder before testing

Write-Host "════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  MAZE SYSTEM VERIFICATION" -ForegroundColor Cyan
Write-Host "  Debunking/Verifying CompleteMazeBuilder" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$assets = Join-Path $root "Assets"
$scripts = Join-Path $assets "Scripts"
$config = Join-Path $root "Config"

$script:totalTests = 0
$script:passedTests = 0

function Test-File {
    param($path, $description)
    $script:totalTests = $script:totalTests + 1
    if (Test-Path $path) {
        Write-Host "  ✓ $description" -ForegroundColor Green
        Write-Host "    Path: $path" -ForegroundColor Gray
        $script:passedTests = $script:passedTests + 1
        return $true
    } else {
        Write-Host "  ✗ $description" -ForegroundColor Red
        Write-Host "    Missing: $path" -ForegroundColor Yellow
        return $false
    }
}

function Test-Content {
    param($path, $pattern, $description, $shouldExist = $true)
    $script:totalTests = $script:totalTests + 1
    if (Test-Path $path) {
        $content = Get-Content $path -Raw -Encoding UTF8
        $match = $content -match $pattern
        if ($match -eq $shouldExist) {
            Write-Host "  ✓ $description" -ForegroundColor Green
            $script:passedTests = $script:passedTests + 1
            return $true
        } else {
            Write-Host "  ✗ $description" -ForegroundColor Red
            if ($shouldExist) {
                Write-Host "    Pattern not found: $pattern" -ForegroundColor Yellow
            } else {
                Write-Host "    Pattern found (should not exist): $pattern" -ForegroundColor Yellow
            }
            return $false
        }
    } else {
        Write-Host "  ✗ File not found: $path" -ForegroundColor Red
        return $false
    }
}

Write-Host "1. CHECKING CORE FILES..." -ForegroundColor Cyan
Test-File (Join-Path $scripts "Core\06_Maze\CompleteMazeBuilder.cs") "CompleteMazeBuilder.cs"
Test-File (Join-Path $scripts "Core\06_Maze\GridMazeGenerator.cs") "GridMazeGenerator.cs"
Test-File (Join-Path $scripts "Core\08_Environment\SpatialPlacer.cs") "SpatialPlacer.cs"
Test-File (Join-Path $scripts "Core\10_Resources\LightPlacementEngine.cs") "LightPlacementEngine.cs"
Test-File (Join-Path $scripts "Core\10_Resources\TorchPool.cs") "TorchPool.cs"
Test-File (Join-Path $scripts "Core\07_Doors\DoorsEngine.cs") "DoorsEngine.cs"
Write-Host ""

Write-Host "2. CHECKING CONFIG FILES..." -ForegroundColor Cyan
Test-File (Join-Path $config "GameConfig-default.json") "GameConfig-default.json"
Write-Host ""

Write-Host "3. CHECKING PLUG-IN-OUT COMPLIANCE..." -ForegroundColor Cyan
$mazeBuilder = Join-Path $scripts "Core\06_Maze\CompleteMazeBuilder.cs"
Test-Content $mazeBuilder "FindFirstObjectByType" "Uses FindFirstObjectByType (plug-in-out)" $true
Test-Content $mazeBuilder "new GameObject\(\)" "No new GameObject() calls" $false
Test-Content $mazeBuilder "AddComponent<" "No AddComponent calls" $false
Write-Host ""

Write-Host "4. CHECKING JSON CONFIG USAGE..." -ForegroundColor Cyan
Test-Content $mazeBuilder "GameConfig\.Instance" "Uses GameConfig.Instance" $true
Test-Content $mazeBuilder "SerializeField.*private" "Uses [SerializeField] private fields" $true
Write-Host ""

Write-Host "5. CHECKING UNITY 6 COMPLIANCE..." -ForegroundColor Cyan
Test-Content $mazeBuilder "FindFirstObjectByType" "Unity 6 API (FindFirstObjectByType)" $true
Test-Content $mazeBuilder "FindObjectOfType" "No deprecated FindObjectOfType" $false
Write-Host ""

Write-Host "6. CHECKING FILE ENCODING..." -ForegroundColor Cyan
$files = Get-ChildItem -Path (Join-Path $scripts "Core\06_Maze") -Filter "*.cs" -Encoding UTF8
foreach ($file in $files) {
    $bytes = [System.IO.File]::ReadAllBytes($file.FullName)
    $hasBOM = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
    if (-not $hasBOM) {
        Write-Host "  ✓ $($file.Name) - UTF-8 without BOM" -ForegroundColor Green
        $script:passedTests = $script:passedTests + 1
    } else {
        Write-Host "  ⚠ $($file.Name) - UTF-8 with BOM (should be without)" -ForegroundColor Yellow
    }
    $script:totalTests = $script:totalTests + 1
}
Write-Host ""

Write-Host "7. CHECKING LINE ENDINGS..." -ForegroundColor Cyan
foreach ($file in $files) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    if ($content -match "\r\n") {
        Write-Host "  ⚠ $($file.Name) - Has CRLF (should be LF only)" -ForegroundColor Yellow
    } else {
        Write-Host "  ✓ $($file.Name) - Unix LF" -ForegroundColor Green
        $script:passedTests = $script:passedTests + 1
    }
    $script:totalTests = $script:totalTests + 1
}
Write-Host ""

Write-Host "8. CHECKING FOR TODOs AND FIXMEs..." -ForegroundColor Cyan
foreach ($file in $files) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $todos = ([regex]::Matches($content, "//.*?(TODO|FIXME|XXX|HACK)")).Count
    if ($todos -gt 0) {
        Write-Host "  ⚠ $($file.Name) - $todos TODOs/FIXMEs found" -ForegroundColor Yellow
    } else {
        Write-Host "  ✓ $($file.Name) - No TODOs/FIXMEs" -ForegroundColor Green
        $script:passedTests = $script:passedTests + 1
    }
    $script:totalTests = $script:totalTests + 1
}
Write-Host ""

Write-Host "════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  VERIFICATION RESULTS" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Tests Passed: $passedTests / $totalTests" -ForegroundColor $(if ($passedTests -eq $totalTests) { "Green" } else { "Yellow" })
Write-Host "  Success Rate: $([math]::Round(($passedTests / $totalTests) * 100, 1))%" -ForegroundColor $(if ($passedTests -eq $totalTests) { "Green" } else { "Yellow" })
Write-Host ""

if ($passedTests -eq $totalTests) {
    Write-Host "  ✓ ALL CHECKS PASSED!" -ForegroundColor Green
    Write-Host "  System is ready for testing in Unity Editor" -ForegroundColor Green
    Write-Host ""
    Write-Host "  Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Open Unity Editor" -ForegroundColor White
    Write-Host "  2. Press Ctrl+Alt+G to generate maze" -ForegroundColor White
    Write-Host "  3. Press Play to test" -ForegroundColor White
    Write-Host "  4. Run backup.ps1 after successful test" -ForegroundColor White
} else {
    $failed = $totalTests - $passedTests
    Write-Host "  ⚠ $failed CHECKS FAILED!" -ForegroundColor Red
    Write-Host "  Review issues above before testing" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  Recommended actions:" -ForegroundColor Cyan
    Write-Host "  1. Fix issues listed above" -ForegroundColor White
    Write-Host "  2. Run this script again to verify" -ForegroundColor White
    Write-Host "  3. Then test in Unity Editor" -ForegroundColor White
}

Write-Host ""
Write-Host "════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  BetsyBoop Verification Complete!" -ForegroundColor Magenta
Write-Host "════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
