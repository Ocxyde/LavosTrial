# ============================================================
#  backup.ps1 - Unity Project Smart Backup
#  - Detecte les fichiers MODIFIES via hash MD5
#  - Chaque fichier a sa propre version (_0001, _0002, ...)
#  - Les anciens backups ne sont JAMAIS supprimes ni ecrases
#  - Trie toutes les ressources du projet Unity
# ============================================================

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

# --- Configuration --------------------------------------------
$backupRoot = "Backup_Solution"

# Always run backup after changes; placeholder to call external backup script if present

# Ensure backup runs from repo root; respect existing backup if present

$extensions = @(
    "*.cs", "*.asmdef", "*.asmref",
    "*.unity", "*.prefab",
    "*.asset", "*.mat", "*.physicMaterial",
    "*.inputactions",
    "*.shader", "*.hlsl", "*.cginc", "*.shadergraph", "*.shadersubgraph",
    "*.anim", "*.controller", "*.overrideController", "*.mask",
    "*.mixer",
    "*.uss", "*.uxml",
    "*.json", "*.xml", "*.yaml", "*.yml",
    "*.playable",
    "*.terrainlayer"
)

$watchFolders = @("Assets")

# --- Creer le dossier racine de backup ------------------------
if (!(Test-Path $backupRoot)) {
    New-Item -ItemType Directory -Path $backupRoot -Force | Out-Null
    Write-Host "  Dossier cree : $backupRoot" -ForegroundColor DarkGray
}

# --- Fonction : Hash MD5 d'un fichier -------------------------
function Get-MD5Hash {
    param([string]$filePath)
    $md5    = [System.Security.Cryptography.MD5]::Create()
    $stream = [System.IO.File]::OpenRead($filePath)
    $hash   = [System.BitConverter]::ToString($md5.ComputeHash($stream)).Replace("-", "")
    $stream.Close()
    $md5.Dispose()
    return $hash
}

# --- Fonction : Dernier backup d'un fichier -------------------
function Get-LastBackupInfo {
    param(
        [string]$relativeNoExt,
        [string]$extension
    )
    $sep       = [System.IO.Path]::DirectorySeparatorChar
    $safeRel   = $relativeNoExt -replace "[/\\]", $sep
    $backupDir = Join-Path $backupRoot (Split-Path $safeRel)
    $baseName  = Split-Path $safeRel -Leaf

    if (!(Test-Path $backupDir)) { return $null }

    $escapedBase = [regex]::Escape($baseName)
    $escapedExt  = [regex]::Escape($extension)
    $pattern     = "^" + $escapedBase + "_0(\d{4})" + $escapedExt + "$"

    $existing = Get-ChildItem -Path $backupDir -File -ErrorAction SilentlyContinue |
                Where-Object { $_.Name -match $pattern } |
                Sort-Object Name

    if (!$existing -or $existing.Count -eq 0) { return $null }

    $last = $existing | Select-Object -Last 1

    if ($last.Name -match "_0(\d{4})") {
        $lastVersion = [int]$Matches[1]
    } else {
        $lastVersion = 0
    }

    $wasReadOnly = $last.IsReadOnly
    if ($wasReadOnly) {
        Set-ItemProperty -Path $last.FullName -Name IsReadOnly -Value $false
    }
    $lastHash = Get-MD5Hash -filePath $last.FullName
    if ($wasReadOnly) {
        Set-ItemProperty -Path $last.FullName -Name IsReadOnly -Value $true
    }

    return @{ Version = $lastVersion; Hash = $lastHash }
}

# --- Scan de tous les fichiers --------------------------------
Write-Host ""
Write-Host "============================================" -ForegroundColor DarkCyan
Write-Host "  Unity Smart Backup - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor DarkCyan
Write-Host ""

$allFiles = @()
foreach ($folder in $watchFolders) {
    if (!(Test-Path $folder)) {
        Write-Host "  AVERTISSEMENT : Dossier introuvable : $folder" -ForegroundColor Yellow
        continue
    }
    foreach ($ext in $extensions) {
        $found = Get-ChildItem -Path $folder -Filter $ext -Recurse -File -ErrorAction SilentlyContinue
        if ($found) { $allFiles += $found }
    }
}

if ($allFiles.Count -eq 0) {
    Write-Host "  Aucun fichier trouve dans les dossiers surveilles." -ForegroundColor Yellow
    exit
}

Write-Host "  $($allFiles.Count) fichier(s) trouve(s) dans le projet." -ForegroundColor DarkGray
Write-Host ""

# --- Traitement fichier par fichier ---------------------------
$newCount     = 0
$changedCount = 0
$skippedCount = 0
$errorCount   = 0

foreach ($file in $allFiles) {
    try {
        $relativePath  = $file.FullName.Substring((Get-Location).Path.Length + 1)
        $relativeDir   = Split-Path $relativePath
        $baseName      = $file.BaseName
        $extension     = $file.Extension
        $relativeNoExt = Join-Path $relativeDir $baseName

        $currentHash = Get-MD5Hash -filePath $file.FullName

        $lastInfo = Get-LastBackupInfo -relativeNoExt $relativeNoExt -extension $extension

        if ($null -eq $lastInfo) {
            $nextVersion = 1
            $status      = "NOUVEAU"
            $color       = "Green"
        }
        elseif ($currentHash -ne $lastInfo.Hash) {
            $nextVersion = $lastInfo.Version + 1
            $status      = "MODIFIE"
            $color       = "Cyan"
        }
        else {
            $skippedCount++
            continue
        }

        $versionStr = $nextVersion.ToString("0000")

        $destDir = Join-Path $backupRoot $relativeDir
        if (!(Test-Path $destDir)) {
            New-Item -ItemType Directory -Path $destDir -Force | Out-Null
        }

        $destName = $baseName + "_0" + $versionStr + $extension
        $destPath = Join-Path $destDir $destName

        if (Test-Path $destPath) {
            Write-Host ("  COLLISION EVITEE : " + $destName) -ForegroundColor Yellow
            $errorCount++
            continue
        }

        Copy-Item $file.FullName $destPath -Force
        Set-ItemProperty -Path $destPath -Name IsReadOnly -Value $true

        $shortPath = $relativePath -replace "^Assets[/\\]", ""
        Write-Host ("  [" + $status + "] " + $shortPath) -ForegroundColor $color
        Write-Host ("         -> " + $destName) -ForegroundColor DarkGray

        if ($status -eq "NOUVEAU") { $newCount++ } else { $changedCount++ }
    }
    catch {
        Write-Host ("  ERREUR sur " + $file.Name + " : " + $_) -ForegroundColor Red
        $errorCount++
    }
}

# --- Resume ---------------------------------------------------
Write-Host ""
Write-Host "============================================" -ForegroundColor DarkCyan
Write-Host ("  Nouveaux fichiers  : " + $newCount)     -ForegroundColor Green
Write-Host ("  Fichiers modifies  : " + $changedCount) -ForegroundColor Cyan
Write-Host ("  Fichiers inchanges : " + $skippedCount) -ForegroundColor DarkGray
if ($errorCount -gt 0) {
    Write-Host ("  Erreurs            : " + $errorCount) -ForegroundColor Red
}
Write-Host "============================================" -ForegroundColor DarkCyan
Write-Host ("  Backup dans : " + $backupRoot)           -ForegroundColor White
Write-Host "  Tous les fichiers sont en lecture seule." -ForegroundColor DarkGray
Write-Host ""
