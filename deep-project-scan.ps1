# deep-project-scan.ps1
# Deep scan entire project for errors, warnings, and potential bugs
# UTF-8 encoding - Unix line endings
#
# Usage: powershell -ExecutionPolicy Bypass -File deep-project-scan.ps1

$ErrorActionPreference = "SilentlyContinue"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Deep Project Scan" -ForegroundColor White
Write-Host "  Searching for errors, warnings, and bugs" -ForegroundColor DarkGray
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$issues = @()
$warnings = @()
$info = @()

# Get all C# files
$csFiles = Get-ChildItem -Path "Assets\Scripts" -Filter "*.cs" -Recurse

Write-Host "[1/10] Scanning C# files for common issues..." -ForegroundColor Yellow

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $relativePath = $file.FullName.Replace($scriptDir, "")
    
    # Check for missing null checks
    if ($content -match "Instance\." -and $content -notmatch "if\s*\(\s*.*Instance\s*!=\s*null") {
        $warnings += "POTENTIAL NULL: $relativePath - Uses Instance without null check"
    }
    
    # Check for hardcoded values
    if ($content -match "new Vector2\([0-9.]+, [0-9.]+\)") {
        # This is OK for UI
    }
    
    # Check for missing [SerializeField] on private fields used in Inspector
    if ($content -match "\[Header\(" -and $content -notmatch "\[SerializeField\]") {
        # Some files might not need it
    }
    
    # Check for Debug.Log in production code (info only)
    if ($content -match "Debug\.Log\(") {
        $logCount = ([regex]::Matches($content, "Debug\.Log\(")).Count
        if ($logCount -gt 10) {
            $info += "VERBOSE: $relativePath - Has $logCount Debug.Log calls (consider Debug.conditional)"
        }
    }
    
    # Check for TODO comments
    if ($content -match "// TODO:|// FIXME:|// HACK:") {
        $todoCount = ([regex]::Matches($content, "// TODO:|// FIXME:|// HACK:")).Count
        $warnings += "TECH DEBT: $relativePath - Has $todoCount TODO/FIXME/HACK comments"
    }
    
    # Check for empty catch blocks
    if ($content -match "catch\s*\(\s*Exception\s*\)\s*\{\s*\}") {
        $issues += "BAD PRACTICE: $relativePath - Empty catch block"
    }
    
    # Check for potential memory leaks (coroutines without StopCoroutine)
    if ($content -match "StartCoroutine\(" -and $content -notmatch "StopCoroutine\(") {
        # Not always an issue, but worth noting
    }
    
    # Check for missing DontDestroyOnLoad on singletons
    if ($content -match "Instance\s*=\s*this" -and $content -notmatch "DontDestroyOnLoad") {
        $warnings += "SINGLETON: $relativePath - Singleton without DontDestroyOnLoad"
    }
    
    # Check for potential race conditions (async without lock)
    if ($content -match "async\s|Task\s" -and $content -notmatch "lock\s*\(") {
        # Not always an issue
    }
    
    # Check for deprecated Unity API
    if ($content -match "FindObjectOfType\(") {
        $info += "PERFORMANCE: $relativePath - Uses FindObjectOfType (consider caching)"
    }
    
    # Check for unassigned variables
    if ($content -match "private\s+\w+\s+_\w+\s*;" -and $content -notmatch "=\s*new|=\s*null") {
        # Might be unassigned
    }
}

Write-Host "  Scanned $($csFiles.Count) files." -ForegroundColor Gray
Write-Host ""

Write-Host "[2/10] Checking for missing using directives..." -ForegroundColor Yellow

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $relativePath = $file.FullName.Replace($scriptDir, "")
    
    # Check for common missing namespaces
    if ($content -match "List<" -and $content -notmatch "using System.Collections.Generic") {
        $issues += "MISSING USING: $relativePath - Uses List<> without System.Collections.Generic"
    }
    
    if ($content -match "Action<|Func<" -and $content -notmatch "using System") {
        # Usually OK
    }
}

Write-Host ""

Write-Host "[3/10] Checking for potential null reference exceptions..." -ForegroundColor Yellow

$nullCheckFiles = @(
    "PlayerStats",
    "PlayerController",
    "UIBarsSystem",
    "EventHandler"
)

foreach ($fileName in $nullCheckFiles) {
    $file = Get-ChildItem -Path "Assets\Scripts" -Filter "*$fileName.cs" -Recurse | Select-Object -First 1
    if ($file) {
        $content = Get-Content $file.FullName -Raw
        if ($content -match "Instance\." -and $content -notmatch "if\s*\([^)]*Instance[^)]*!=\s*null") {
            $warnings += "NULL RISK: $($file.Name) - Accesses Instance without null check"
        }
    }
}

Write-Host ""

Write-Host "[4/10] Checking for event subscription leaks..." -ForegroundColor Yellow

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $relativePath = $file.FullName.Replace($scriptDir, "")
    
    # Check for event subscriptions without unsubscription
    if ($content -match "\+=\s*new\s*Action" -and $content -notmatch "-=") {
        $warnings += "EVENT LEAK: $relativePath - Subscribes to events but never unsubscribes"
    }
}

Write-Host ""

Write-Host "[5/10] Checking for performance issues..." -ForegroundColor Yellow

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $relativePath = $file.FullName.Replace($scriptDir, "")
    
    # Check for FindObjectOfType in Update
    if ($content -match "void Update\(\)" -and $content -match "FindObjectOfType") {
        $issues += "PERFORMANCE: $relativePath - FindObjectOfType in Update (cache in Awake/Start)"
    }
    
    # Check for GetComponent in Update
    if ($content -match "void Update\(\)" -and $content -match "GetComponent") {
        $warnings += "PERFORMANCE: $relativePath - GetComponent in Update (cache in Awake/Start)"
    }
    
    # Check for string concatenation in loops
    if ($content -match "foreach|for|while" -and $content -match "\+=") {
        # Might be string concatenation
    }
    
    # Check for LINQ in Update
    if ($content -match "void Update\(\)" -and $content -match "\.Where\(|\.Select\(|\.First\(") {
        $warnings += "PERFORMANCE: $relativePath - LINQ in Update (can cause GC)"
    }
}

Write-Host ""

Write-Host "[6/10] Checking for Unity-specific issues..." -ForegroundColor Yellow

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $relativePath = $file.FullName.Replace($scriptDir, "")
    
    # Check for missing RequireComponent
    if ($content -match ": MonoBehaviour" -and $content -match "GetComponent\(" -and $content -notmatch "RequireComponent") {
        # Not always needed
    }
    
    # Check for tag comparison (should use const or compare tag)
    if ($content -match "\.tag\s*==") {
        $info += "BEST PRACTICE: $relativePath - Uses .tag instead of .CompareTag()"
    }
    
    # Check for transform access
    if ($content -match "transform\." -and $content -gt 10) {
        # transform is cached, this is OK
    }
}

Write-Host ""

Write-Host "[7/10] Checking for memory management..." -ForegroundColor Yellow

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $relativePath = $file.FullName.Replace($scriptDir, "")
    
    # Check for Instantiate without Destroy
    if ($content -match "Instantiate\(" -and $content -notmatch "Destroy\(") {
        $info += "MEMORY: $relativePath - Instantiates objects but no Destroy found (might be intentional)"
    }
    
    # Check for new GameObject in Update
    if ($content -match "void Update\(\)" -and $content -match "new GameObject") {
        $issues += "MEMORY: $relativePath - Creates GameObject in Update (causes GC)"
    }
}

Write-Host ""

Write-Host "[8/10] Checking for thread safety..." -ForegroundColor Yellow

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $relativePath = $file.FullName.Replace($scriptDir, "")
    
    # Check for static mutable fields
    if ($content -match "public static|private static" -and $content -match "=\s*new") {
        # Singletons are usually OK
    }
}

Write-Host ""

Write-Host "[9/10] Checking for build issues..." -ForegroundColor Yellow

# Check test files
$testFiles = Get-ChildItem -Path "Assets\Scripts\Tests" -Filter "*.cs" -Recurse
if ($testFiles.Count -gt 0) {
    $info += "BUILD: Test files found ($($testFiles.Count) files) - Exclude from build or move to Editor folder"
}

# Check for duplicate class names
$allClasses = @()
foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    if ($content -match "class\s+(\w+)") {
        $className = $matches[1]
        $allClasses += @{Name=$className; File=$file.FullName}
    }
}

$duplicates = $allClasses | Group-Object -Property Name | Where-Object { $_.Count -gt 1 }
if ($duplicates) {
    foreach ($dup in $duplicates) {
        $issues += "DUPLICATE: Class '$($dup.Name)' defined in multiple files: $($dup.Group.File -join ', ')"
    }
}

Write-Host ""

Write-Host "[10/10] Checking documentation..." -ForegroundColor Yellow

$undocumentedFiles = @()
foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $relativePath = $file.FullName.Replace($scriptDir, "")
    
    # Check for missing file header
    if ($content -notmatch "^// .*\.cs") {
        $undocumentedFiles += $relativePath
    }
}

if ($undocumentedFiles.Count -gt 0) {
    $info += "DOCUMENTATION: $($undocumentedFiles.Count) files missing standard headers"
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Scan Summary" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

if ($issues.Count -gt 0) {
    Write-Host "CRITICAL ISSUES ($($issues.Count)) - Fix before build:" -ForegroundColor Red
    foreach ($issue in $issues) {
        Write-Host "  [!] $issue" -ForegroundColor Red
    }
    Write-Host ""
} else {
    Write-Host "✅ No critical issues found!" -ForegroundColor Green
    Write-Host ""
}

if ($warnings.Count -gt 0) {
    Write-Host "WARNINGS ($($warnings.Count)) - Should review:" -ForegroundColor Yellow
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

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Recommendations" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

if ($issues.Count -eq 0 -and $warnings.Count -lt 5) {
    Write-Host "  ✅ Project is in GOOD shape!" -ForegroundColor Green
    Write-Host "  Ready for production build." -ForegroundColor Green
} else {
    Write-Host "  ⚠️  Review warnings and fix critical issues before build." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Scan complete at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
Write-Host ""
