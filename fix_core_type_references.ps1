# fix_core_type_references.ps1
# Replace direct type references with interfaces in Core scripts
# Unity 6 compatible - UTF-8 encoding - Unix line endings

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     FIX CORE TYPE REFERENCES - Use interfaces                  ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$scriptRoot = $PSScriptRoot
$coreDir = Join-Path $scriptRoot "Assets\Scripts\Core"
$fixedCount = 0

# ============================================================================
# FIX EventHandler.cs - Replace PlayerStats with IPlayerStats
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📝 FIX EventHandler.cs" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$eventHandlerPath = Join-Path $coreDir "EventHandler.cs"
if (Test-Path $eventHandlerPath) {
    $content = Get-Content $eventHandlerPath -Raw -Encoding UTF8
    
    # Replace PlayerStats parameter with IPlayerStats
    $content = $content -replace 'SubscribeToPlayerStats\(PlayerStats stats\)', 'SubscribeToPlayerStats(IPlayerStats stats)'
    $content = $content -replace 'var playerStats = FindFirstObjectByType<PlayerStats>\(\)', 'var playerStats = FindFirstObjectByType<Component>() as IPlayerStats'
    $content = $content -replace 'PlayerStats\.Instance', 'Component.FindFirstObjectByType<Component>() as IPlayerStats'
    $content = $content -replace 'PlayerStats\.OnHealthChanged', 'OnPlayerHealthChangedStatic'
    $content = $content -replace 'PlayerStats\.OnPlayerDied', 'OnPlayerDiedStatic'
    
    # Add static event backups
    if ($content -notmatch 'public static event Action<float, float> OnPlayerHealthChangedStatic') {
        $content = $content -replace '#region Player Events', "#region Player Events`n`n        // Static fallback events for Core compatibility`n        public static event Action<float, float> OnPlayerHealthChangedStatic;`n        public static event Action OnPlayerDiedStatic;"
    }
    
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($eventHandlerPath, $content, $utf8NoBom)
    Write-Host "  ✅ Fixed: EventHandler.cs" -ForegroundColor Green
    $fixedCount++
} else {
    Write-Host "  ⚠️  Not found: EventHandler.cs" -ForegroundColor Gray
}
Write-Host ""

# ============================================================================
# FIX EventHandlerInitializer.cs
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📝 FIX EventHandlerInitializer.cs" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$eventHandlerInitPath = Join-Path $coreDir "EventHandlerInitializer.cs"
if (Test-Path $eventHandlerInitPath) {
    $content = Get-Content $eventHandlerInitPath -Raw -Encoding UTF8
    
    # Replace PlayerStats.Instance with interface lookup
    $content = $content -replace 'PlayerStats\.Instance != null', 'Component.FindFirstObjectByType<Component>\(\) as IPlayerStats != null'
    $content = $content -replace 'PlayerStats\.Instance\)', 'Component.FindFirstObjectByType<Component>() as IPlayerStats)'
    
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($eventHandlerInitPath, $content, $utf8NoBom)
    Write-Host "  ✅ Fixed: EventHandlerInitializer.cs" -ForegroundColor Green
    $fixedCount++
} else {
    Write-Host "  ⚠️  Not found: EventHandlerInitializer.cs" -ForegroundColor Gray
}
Write-Host ""

# ============================================================================
# FIX ItemData.cs - Replace PlayerStats with IPlayerStats
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📝 FIX ItemData.cs" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$itemDataPath = Join-Path $coreDir "ItemData.cs"
if (Test-Path $itemDataPath) {
    $content = Get-Content $itemDataPath -Raw -Encoding UTF8
    
    # Replace GetComponent<PlayerStats>() with interface
    $content = $content -replace 'user\.GetComponent<PlayerStats>\(\)', 'user.GetComponent<IPlayerStats>()'
    $content = $content -replace 'var playerStats = user\.GetComponent<IPlayerStats>\(\)', 'var playerStats = user.GetComponent<IPlayerStats>()'
    
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($itemDataPath, $content, $utf8NoBom)
    Write-Host "  ✅ Fixed: ItemData.cs" -ForegroundColor Green
    $fixedCount++
} else {
    Write-Host "  ⚠️  Not found: ItemData.cs" -ForegroundColor Gray
}
Write-Host ""

# ============================================================================
# FIX CombatSystem.cs
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📝 FIX CombatSystem.cs" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$combatSystemPath = Join-Path $coreDir "CombatSystem.cs"
if (Test-Path $combatSystemPath) {
    $content = Get-Content $combatSystemPath -Raw -Encoding UTF8
    
    # Replace FindFirstObjectByType<PlayerStats>() with interface
    $content = $content -replace 'FindFirstObjectByType<PlayerStats>\(\)', 'FindFirstObjectByType<Component>() as IPlayerStats'
    $content = $content -replace 'target\.GetComponent<PlayerStats>\(\)', 'target.GetComponent<IPlayerStats>()'
    
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($combatSystemPath, $content, $utf8NoBom)
    Write-Host "  ✅ Fixed: CombatSystem.cs" -ForegroundColor Green
    $fixedCount++
} else {
    Write-Host "  ⚠️  Not found: CombatSystem.cs" -ForegroundColor Gray
}
Write-Host ""

# ============================================================================
# FIX MazeIntegration.cs - Replace MazeRenderer with IMazeRenderer
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📝 FIX MazeIntegration.cs" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$mazeIntegrationPath = Join-Path $coreDir "MazeIntegration.cs"
if (Test-Path $mazeIntegrationPath) {
    $content = Get-Content $mazeIntegrationPath -Raw -Encoding UTF8
    
    # Replace MazeRenderer type with IMazeRenderer
    $content = $content -replace 'private MazeRenderer mazeRenderer', 'private IMazeRenderer mazeRenderer'
    $content = $content -replace 'GetComponent<MazeRenderer>\(\)', 'GetComponent<IMazeRenderer>()'
    
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($mazeIntegrationPath, $content, $utf8NoBom)
    Write-Host "  ✅ Fixed: MazeIntegration.cs" -ForegroundColor Green
    $fixedCount++
} else {
    Write-Host "  ⚠️  Not found: MazeIntegration.cs" -ForegroundColor Gray
}
Write-Host ""

# ============================================================================
# FIX DoorSystemSetup.cs
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📝 FIX DoorSystemSetup.cs" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$doorSystemSetupPath = Join-Path $coreDir "DoorSystemSetup.cs"
if (Test-Path $doorSystemSetupPath) {
    $content = Get-Content $doorSystemSetupPath -Raw -Encoding UTF8
    
    # Replace GetComponent<MazeRenderer>() with interface
    $content = $content -replace 'GetComponent<MazeRenderer>\(\)', 'GetComponent<IMazeRenderer>()'
    
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($doorSystemSetupPath, $content, $utf8NoBom)
    Write-Host "  ✅ Fixed: DoorSystemSetup.cs" -ForegroundColor Green
    $fixedCount++
} else {
    Write-Host "  ⚠️  Not found: DoorSystemSetup.cs" -ForegroundColor Gray
}
Write-Host ""

# ============================================================================
# SUMMARY
# ============================================================================

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📊 SUMMARY" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Files fixed: $fixedCount" -ForegroundColor $(if ($fixedCount -gt 0) { "Green" } else { "Yellow" })
Write-Host ""

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "🔧 NEXT STEPS" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  1. Update PlayerStats.cs to implement IPlayerStats" -ForegroundColor White
Write-Host "  2. Update MazeRenderer.cs to implement IMazeRenderer" -ForegroundColor White
Write-Host "  3. Open Unity Editor and recompile" -ForegroundColor White
Write-Host ""

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "🔧 GIT REMINDER" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  After verifying everything works, commit changes:" -ForegroundColor White
Write-Host ""
Write-Host "  git add -A" -ForegroundColor Gray
Write-Host "  git commit -m `"fix: Use interfaces in Core for plug-in-and-out architecture`"" -ForegroundColor Cyan
Write-Host ""
