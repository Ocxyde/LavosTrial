# ============================================================
#  setup-git-scheduler.ps1 - Setup Windows Task Scheduler
#  Creates a scheduled task to run git-auto-commit.ps1 daily
# ============================================================

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

# --- Configuration --------------------------------------------
$taskName = "LavosTrial-Git-AutoCommit"
$taskDescription = "Automatically commit Assets/ changes to Git every 24 hours"
$triggerTime = "02:00"  # Run at 2:00 AM daily

# --- Helper Functions -----------------------------------------
function Write-Color {
    param([string]$message, [string]$color = "White")
    Write-Host $message -ForegroundColor $color
}

# --- Main Script ----------------------------------------------
Write-Color ""
Write-Color "============================================" "Cyan"
Write-Color "  Git Auto-Commit Scheduler Setup" "Cyan"
Write-Color "============================================" "Cyan"
Write-Color ""

# Check if running as administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (!$isAdmin) {
    Write-Color "  ERROR: This script must be run as Administrator" "Red"
    Write-Color "  Right-click PowerShell and select 'Run as Administrator'" "Yellow"
    Write-Color "  Then run this script again." "Yellow"
    Write-Color ""
    exit 1
}

Write-Color "  Running as Administrator: YES" "Green"
Write-Color ""

# Check if Git is available
Write-Color "  Checking Git installation..." "DarkGray"
try {
    $gitPath = (Get-Command git -ErrorAction Stop).Source
    Write-Color "  Git found: $gitPath" "Green"
} catch {
    Write-Color "  ERROR: Git is not installed or not in PATH" "Red"
    Write-Color "  Please install Git from: https://git-scm.com/download/win" "Yellow"
    Write-Color ""
    exit 1
}

# Check if auto-commit script exists
$autoCommitScript = Join-Path $scriptDir "git-auto-commit.ps1"
if (!(Test-Path $autoCommitScript)) {
    Write-Color "  ERROR: git-auto-commit.ps1 not found" "Red"
    exit 1
}
Write-Color "  Auto-commit script found: $autoCommitScript" "Green"

# Check if Git repo is initialized
if (!(Test-Path ".git")) {
    Write-Color "  ERROR: Git repository not initialized" "Red"
    Write-Color "  Run init-git.ps1 first" "Yellow"
    exit 1
}
Write-Color "  Git repository initialized" "Green"
Write-Color ""

# Get user preferences
Write-Color "  Schedule Configuration:" "DarkGray"
Write-Color "  Task Name: $taskName" "Gray"
Write-Color "  Run Time: $triggerTime daily" "Gray"
Write-Color ""

$response = Read-Host "  Do you want to proceed? (y/n)"
if ($response -ne "y" -and $response -ne "Y") {
    Write-Color "  Setup cancelled." "Yellow"
    exit 0
}

Write-Color ""
Write-Color "  Creating scheduled task..." "DarkGray"

# Build the command
$powershellPath = "C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe"
$executeCommand = "-ExecutionPolicy Bypass -File `"$autoCommitScript`""

# Remove existing task if it exists
$existingTask = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue
if ($existingTask) {
    Write-Color "  Removing existing task..." "DarkGray"
    Unregister-ScheduledTask -TaskName $taskName -Confirm:$false
}

# Create the scheduled task action
$action = New-ScheduledTaskAction -Execute $powershellPath -Argument $executeCommand -WorkingDirectory $scriptDir

# Create the daily trigger at specified time
$trigger = New-ScheduledTaskTrigger -Daily -At $triggerTime -DaysInterval 1

# Create settings (allow task to run on battery, etc.)
$settings = New-ScheduledTaskSettingsSet `
    -AllowStartIfOnBatteries `
    -DontStopIfGoingOnBatteries `
    -StartWhenAvailable `
    -RunOnlyIfNetworkAvailable `
    -WakeToRun

# Create the principal (run with highest privileges)
$principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount -RunLevel Highest

# Register the task
try {
    Register-ScheduledTask `
        -TaskName $taskName `
        -Action $action `
        -Trigger $trigger `
        -Settings $settings `
        -Principal $principal `
        -Description $taskDescription `
        -ErrorAction Stop
    
    Write-Color "  Task created successfully!" "Green"
} catch {
    Write-Color "  ERROR: Failed to create task: $($_.Exception.Message)" "Red"
    exit 1
}

Write-Color ""
Write-Color "============================================" "Cyan"
Write-Color "  Setup Complete!" "Green"
Write-Color "============================================" "Cyan"
Write-Color ""
Write-Color "  Task Details:" "DarkGray"
Write-Color "  Name: $taskName" "Gray"
Write-Color "  Schedule: Daily at $triggerTime" "Gray"
Write-Color "  Working Directory: $scriptDir" "Gray"
Write-Color ""
Write-Color "  Management Commands:" "DarkGray"
Write-Color "  - View task: Get-ScheduledTask -TaskName '$taskName'" "Gray"
Write-Color "  - Run now: Start-ScheduledTask -TaskName '$taskName'" "Gray"
Write-Color "  - Check status: Get-ScheduledTaskInfo -TaskName '$taskName'" "Gray"
Write-Color "  - Disable: Disable-ScheduledTask -TaskName '$taskName'" "Gray"
Write-Color "  - Enable: Enable-ScheduledTask -TaskName '$taskName'" "Gray"
Write-Color "  - Remove: Unregister-ScheduledTask -TaskName '$taskName'" "Gray"
Write-Color ""
Write-Color "  Log file: Logs\git-auto-commit.log" "Gray"
Write-Color ""

# Test run the task (optional)
$testResponse = Read-Host "  Do you want to run a test commit now? (y/n)"
if ($testResponse -eq "y" -or $testResponse -eq "Y") {
    Write-Color ""
    Write-Color "  Running test commit..." "DarkGray"
    try {
        & powershell -ExecutionPolicy Bypass -File $autoCommitScript
        Write-Color "  Test completed. Check Logs\git-auto-commit.log" "Green"
    } catch {
        Write-Color "  Test failed: $($_.Exception.Message)" "Red"
    }
}

Write-Color ""
Write-Color "  Setup finished!" "Green"
Write-Color ""
