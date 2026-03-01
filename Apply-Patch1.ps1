Begin Apply-Patch1.ps1
Patch 1 application script for three target files
- OverlayHUDManager.cs
- Toggle.cs
- RawImage.cs
- Creates PatchBatch.txt reference
- Backs up originals
- Supports -DryRun (default) and -Apply
param( [switch]$Apply, [switch]$DryRun = $true )
$ErrorActionPreference = "Stop"
$root = (Get-Location).Path
Target files
$target1 = "OverlayHUDManager.cs" $target2 = "Toggle.cs" $target3 = "RawImage.cs"
Patch 1 contents (replace with exact content blocks)
$overlayContent = @" // OverlayHUDManager.cs - Patch 1 content // Replace with your exact Patch 1 code for this file "@
$toggleContent = @" // Toggle.cs - Patch 1 content // Replace with your exact Patch 1 code for this file "@
$rawImageContent = @" // RawImage.cs - Patch 1 content // Replace with your exact Patch 1 code for this file "@
PatchBatch reference
$patchBatchPath = Join-Path $root "PatchBatch.txt"
PatchBatch reference content
$patchBatchContent = @" PatchBatch.txt *** Begin Patch *** End Patch "@
Helper: ensure directory exists
function Ensure-PathExists([string]$p){ $dir = Split-Path $p -Parent if (-not (Test-Path $dir)){ New-Item -ItemType Directory -Path $dir | Out-Null } }
Backup and write
function Write-WithBackup { param( [string]$path, [string]$content ) if (Test-Path $path){ $bak = "$path.bak-$(Get-Date -Format 'yyyyMMddHHmmss')" Copy-Item -Path $path -Destination $bak -Force Write-Host "Backup created: $bak" } $content | Out-File -Encoding ASCII -FilePath $path -Force Write-Host "Wrote: $path" }
Dry-run reporter
function Preview-Write { param( [string]$path, [string]$content ) Write-Host "Would write to: $path" Write-Host "New content length: $($content.Length) chars" }
Create PatchBatch.txt
$patchBatchPath = Join-Path $root "PatchBatch.txt" $patchBatchPath | Out-File -Encoding ASCII -FilePath $patchBatchPath -Force Write-Host "Created patch reference: PatchBatch.txt"
Decide operation
if (-not $Apply) { Write-Host "Dry-run mode: No files will be modified. Use -Apply to apply." }
OverlayHUDManager.cs
if ($Apply) { Write-WithBackup -path (Join-Path $root $target1) -content $overlayContent } else { Preview-Write -path (Join-Path $root $target1) -content $overlayContent }
Toggle.cs
if ($Apply) { Write-WithBackup -path (Join-Path $root $target2) -content $toggleContent } else { Preview-Write -path (Join-Path $root $target2) -content $toggleContent }
RawImage.cs
if ($Apply) { Write-WithBackup -path (Join-Path $root $target3) -content $rawImageContent } else { Preview-Write -path (Join-Path $root $target3) -content $rawImageContent }
Write-Host "Patch 1 operation complete. Summary:" Write-Host " - PatchBatch.txt created" Write-Host " - Target files: OverlayHUDManager.cs, Toggle.cs, RawImage.cs" if ($Apply) { Write-Host " - Changes applied." } else { Write-Host " - Dry-run completed; no files changed." }
End Apply-Patch1.ps1