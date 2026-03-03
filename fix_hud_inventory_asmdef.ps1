# Fix HUD and Inventory asmdef with explicit package GUIDs
$ErrorActionPreference = "Stop"

Write-Host "Fixing HUD and Inventory asmdef files..." -ForegroundColor Cyan

# HUD asmdef - add InputSystem and TextMeshPro
$hudAsmdef = @{
    name = "Code.Lavos.HUD"
    rootNamespace = "Code.Lavos.HUD"
    references = @(
        "Code.Lavos.Core",
        "Code.Lavos.Status",
        "Code.Lavos.Player",
        "com.unity.inputsystem",
        "com.unity.ugui"
    )
    includePlatforms = @()
    excludePlatforms = @()
    allowUnsafeCode = $false
    overrideReferences = $true
    precompiledReferences = @()
    autoReferenced = $true
    generateProgrammingAssets = $false
} | ConvertTo-Json -Depth 10

$hudPath = Join-Path $PSScriptRoot "Assets\Scripts\HUD\Code.Lavos.HUD.asmdef"
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText($hudPath, $hudAsmdef, $utf8NoBom)
Write-Host "  ✅ Fixed: Code.Lavos.HUD.asmdef" -ForegroundColor Green

# Inventory asmdef - add InputSystem and TextMeshPro
$invAsmdef = @{
    name = "Code.Lavos.Inventory"
    rootNamespace = "Code.Lavos.Inventory"
    references = @(
        "Code.Lavos.Core",
        "com.unity.inputsystem",
        "com.unity.ugui"
    )
    includePlatforms = @()
    excludePlatforms = @()
    allowUnsafeCode = $false
    overrideReferences = $true
    precompiledReferences = @()
    autoReferenced = $true
    generateProgrammingAssets = $false
} | ConvertTo-Json -Depth 10

$invPath = Join-Path $PSScriptRoot "Assets\Scripts\Inventory\Code.Lavos.Inventory.asmdef"
[System.IO.File]::WriteAllText($invPath, $invAsmdef, $utf8NoBom)
Write-Host "  ✅ Fixed: Code.Lavos.Inventory.asmdef" -ForegroundColor Green

Write-Host "`nDone! Open Unity to recompile." -ForegroundColor Green
