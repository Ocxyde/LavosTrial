# Git Auto-Commit Scheduler - Documentation

**Last Updated:** March 2026

---

## Overview

This system automatically commits changes in your `Assets/` folder to Git every 24 hours.

---

## Files

| File | Purpose |
|------|---------|
| `git-auto-commit.ps1` | Main auto-commit script (runs daily) |
| `setup-git-scheduler.ps1` | Creates Windows Task Scheduler task |
| `setup-git-scheduler.bat` | Batch wrapper (run as Administrator) |
| `Logs/git-auto-commit.log` | Auto-generated log file |

---

## Quick Setup

### Step 1: Run Setup as Administrator

**Option A - Double-click:**
```
Right-click setup-git-scheduler.bat → Run as Administrator
```

**Option B - PowerShell:**
```powershell
# Open PowerShell as Administrator
.\setup-git-scheduler.ps1
```

### Step 2: Configure Schedule

The setup will:
1. Check Git is installed
2. Verify Git repo is initialized
3. Create a daily task at **2:00 AM**
4. Optionally run a test commit

---

## How It Works

```
┌─────────────────────────────────────────────────────────┐
│  Windows Task Scheduler                                 │
│  Trigger: Daily at 2:00 AM                              │
└──────────────────┬──────────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────────┐
│  git-auto-commit.ps1                                    │
├─────────────────────────────────────────────────────────┤
│  1. Run backup.ps1                                      │
│  2. Check for changes in Assets/                        │
│  3. Stage Assets/ folder                                │
│  4. Commit with timestamp message                       │
│  5. Log results to Logs/git-auto-commit.log             │
└─────────────────────────────────────────────────────────┘
```

---

## Default Schedule

- **Time:** 2:00 AM daily
- **Interval:** Every 24 hours
- **Run Location:** SYSTEM account (highest privileges)
- **Wake to Run:** Enabled (wakes computer from sleep)

---

## Customize Schedule

Edit `setup-git-scheduler.ps1`:

```powershell
$triggerTime = "02:00"  # Change to your preferred time (24h format)
```

Then re-run the setup script.

---

## Management Commands

Open PowerShell (any privileges):

```powershell
# View task details
Get-ScheduledTask -TaskName 'LavosTrial-Git-AutoCommit'

# Check last run status
Get-ScheduledTaskInfo -TaskName 'LavosTrial-Git-AutoCommit'

# Run commit manually now
Start-ScheduledTask -TaskName 'LavosTrial-Git-AutoCommit'

# Disable (keep task, don't run)
Disable-ScheduledTask -TaskName 'LavosTrial-Git-AutoCommit'

# Enable (re-enable after disable)
Enable-ScheduledTask -TaskName 'LavosTrial-Git-AutoCommit'

# Delete task completely
Unregister-ScheduledTask -TaskName 'LavosTrial-Git-AutoCommit'
```

---

## View Logs

```powershell
# View last 50 lines
Get-Content "Logs\git-auto-commit.log" -Tail 50

# View full log
notepad "Logs\git-auto-commit.log"

# Watch log in real-time
Get-Content "Logs\git-auto-commit.log" -Wait -Tail 20
```

---

## Commit Message Format

Auto-commits use this format:
```
Auto-commit - 2026-03-01 02:00
```

---

## What Gets Committed

✅ **Included:**
- All files in `Assets/` folder
- Scripts, scenes, prefabs, materials
- C# files, Unity assets

❌ **Excluded (via .gitignore):**
- `Library/`, `Temp/`, `Builds/`
- `Backup_Solution/`
- `*.log` files
- OS generated files

---

## Troubleshooting

### Task doesn't run
1. Check Task Scheduler library for "LavosTrial-Git-AutoCommit"
2. Verify task is enabled
3. Check "Last Run Result" (0x0 = success)
4. Review `Logs\git-auto-commit.log`

### Git not found
Ensure Git is installed and in PATH:
```powershell
git --version
```

### Commit fails
Check Git status manually:
```powershell
cd D:\travaux_Unity\PeuImporte
git status
git log -n 5
```

### Permission errors
Re-run setup as Administrator:
```
Right-click setup-git-scheduler.bat → Run as Administrator
```

---

## Enable Auto-Push (Optional)

Edit `git-auto-commit.ps1` and uncomment the push section:

```powershell
# Remove the # from these lines:
# Write-Log "Pushing to remote..."
# & git push origin main 2>&1 | Out-Null
# if ($LASTEXITCODE -eq 0) {
#     Write-Log "Push successful" "SUCCESS"
# } else {
#     Write-Log "WARN: Push failed (remote may not be configured)" "WARN"
# }
```

**Note:** Auto-push requires:
- Remote repository configured
- SSH keys or credentials set up
- Network available at run time

---

## Best Practices

1. **Test first:** Run a manual test during setup
2. **Check logs:** Review `Logs\git-auto-commit.log` regularly
3. **Configure remote:** Set up your Git remote before enabling auto-push
4. **Review commits:** Check Git history to ensure commits are clean
5. **Disable when not needed:** Use `Disable-ScheduledTask` during debugging

---

## Security Notes

- Task runs as **SYSTEM** account (highest privileges)
- Script uses `-ExecutionPolicy Bypass` (required for automation)
- Logs are stored locally in `Logs/` folder
- No external services or credentials are stored

---

## Uninstall

To completely remove auto-commit:

```powershell
# Delete scheduled task
Unregister-ScheduledTask -TaskName 'LavosTrial-Git-AutoCommit' -Confirm:$false

# Delete scripts (optional)
Remove-Item "git-auto-commit.ps1"
Remove-Item "setup-git-scheduler.ps1"
Remove-Item "setup-git-scheduler.bat"

# Delete logs (optional)
Remove-Item "Logs\git-auto-commit.log"
```

---

## Related Documentation

- `GIT_LAVOSTRIAL.md` - Git workflow for this project
- `backup.md` - Backup system documentation
- `README.md` - Project overview

---

## Support

If you encounter issues:
1. Check `Logs\git-auto-commit.log` for error messages
2. Verify Git is working manually: `git status`
3. Ensure Git repo is initialized: `.git` folder exists
4. Run setup script again as Administrator
