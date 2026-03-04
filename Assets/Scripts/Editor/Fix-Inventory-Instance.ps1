# Fix-Inventory-Instance.ps1
# Fixes all Inventory.Instance references in Inventory folder
#
# LOCATION: Assets/Scripts/Editor/
# Run: .\Fix-Inventory-Instance.ps1

$ErrorActionPreference = "Stop"

Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  FIX - Inventory.Instance References" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Fix ItemPickup.cs
$file = "Assets\Scripts\Inventory\ItemPickup.cs"
$fullPath = Join-Path $PSScriptRoot "..\..\$file"

if (Test-Path $fullPath) {
    Write-Host "Fixing: $file" -ForegroundColor Cyan
    $content = Get-Content $fullPath -Raw -Encoding UTF8
    
    # Replace Inventory.Instance with FindFirstObjectByType (temporary solution)
    # Better: inject via Inspector or use event
    $content = $content -replace 'Inventory\.Instance\.', 'FindFirstObjectByType<Inventory>()?.'
    
    [System.IO.File]::WriteAllText($fullPath, $content, [System.Text.UTF8Encoding]::new($true))
    Write-Host "  ✅ Replaced Inventory.Instance" -ForegroundColor Green
}

# Fix InventoryUI.cs
$file = "Assets\Scripts\Inventory\InventoryUI.cs"
$fullPath = Join-Path $PSScriptRoot "..\..\$file"

if (Test-Path $fullPath) {
    Write-Host "Fixing: $file" -ForegroundColor Cyan
    $content = Get-Content $fullPath -Raw -Encoding UTF8
    
    # Replace Inventory.Instance with cached reference pattern
    $content = $content -replace 'Inventory\.Instance\.', '_inventory?.'
    
    # Add _inventory field if not exists
    if ($content -notmatch 'private Inventory _inventory;') {
        $content = $content -replace '(public class InventoryUI : MonoBehaviour)', '$1' + "`n" + '    { private Inventory _inventory;'
    }
    
    # Add Awake to cache reference
    if ($content -notmatch 'void Awake\(\)') {
        $content = $content -replace '(void Start\(\))', 'void Awake()' + "`n" + '    {' + "`n" + '        _inventory = FindFirstObjectByType<Inventory>();' + "`n" + '    }' + "`n`n" + '    $1'
    }
    
    [System.IO.File]::WriteAllText($fullPath, $content, [System.Text.UTF8Encoding]::new($true))
    Write-Host "  ✅ Replaced Inventory.Instance with cached _inventory" -ForegroundColor Green
}

Write-Host ""
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  ✅ Inventory.Instance Fixed!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next: Open Unity and verify 0 errors" -ForegroundColor Cyan
