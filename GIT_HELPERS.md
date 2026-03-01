# Git Helper Scripts - Quick Reference

## Available Scripts

| Script | Usage | Description |
|--------|-------|-------------|
| `git-auto.bat` | `.\git-auto.bat "message"` | Stage, normalize LF, backup, commit, push |
| `git-sync.bat` | `.\git-sync.bat "message"` | Pull → restore → commit → push |
| `git-status.bat` | `.\git-status.bat` | Quick status overview |
| `git-normalize.bat` | `.\git-normalize.bat` | Normalize line endings only |
| `git-setup-lf.bat` | `.\git-setup-lf.bat` | One-time LF setup |

---

## Common Workflows

### Daily Work (Quick Commit & Push)
```bash
# Make your changes in Rider...

# Then commit and push in one command:
.\git-auto.bat "Fixed player movement bug"
```

### Start of Day (Get Latest + Merge Your Changes)
```bash
# Pull latest, merge your local changes, commit and push:
.\git-sync.bat "Merged latest changes"
```

### Quick Status Check
```bash
# See what changed:
.\git-status.bat

# Or native Git:
git status
```

### Before Commit (Ensure LF Endings)
```bash
# Normalize line endings:
.\git-normalize.bat

# Then commit:
git commit -m "Your message"
```

---

## Optional: Add to PATH for Global Access

Create aliases in PowerShell profile for quicker access:

```powershell
# Open PowerShell profile
notepad $PROFILE

# Add these lines:
function ga { & "D:\travaux_Unity\PeuImporte\git-auto.bat" $args }
function gs { & "D:\travaux_Unity\PeuImporte\git-status.bat" }
function gn { & "D:\travaux_Unity\PeuImporte\git-normalize.bat" }
function gsync { & "D:\travaux_Unity\PeuImporte\git-sync.bat" $args }

# Save and restart PowerShell
# Now you can run: ga "message" from anywhere
```

---

## Script Details

### git-auto.bat
**Best for:** Quick commits after coding sessions

**Steps:**
1. Shows current status
2. Stages all changes (`git add -A`)
3. Normalizes line endings to LF
4. Runs `backup.ps1` (if exists)
5. Commits with your message
6. Asks if you want to push

**Example:**
```bash
.\git-auto.bat "Added new inventory system"
```

---

### git-sync.bat
**Best for:** Syncing with remote before/after work

**Steps:**
1. Stashes your local changes
2. Pulls latest from remote (with rebase)
3. Restores your stashed changes
4. Stages everything
5. Commits (if message provided)
6. Asks if you want to push

**Example:**
```bash
.\git-sync.bat "Merged remote changes"
```

---

### git-status.bat
**Best for:** Quick overview

**Shows:**
- Current branch
- Changed files (short format)
- Last 5 commits
- Unpushed commits
- Stash list

---

## Git Aliases (Native Alternative)

Add these to `.git/config` or global config:

```bash
git config --global alias.st status
git config --global alias.co checkout
git config --global alias.br branch
git config --global alias.ci commit
git config --global alias.last "log -1 HEAD"
git config --global alias.lg "log --oneline --graph --decorate"
```

Then use: `git st`, `git lg`, etc.

---

## Troubleshooting

### "Git is not installed"
- Install Git from: https://git-scm.com/
- Or add Git to PATH manually

### "Not a Git repository"
- Run scripts from project root: `D:\travaux_Unity\PeuImporte`

### "Commit failed"
- Check if there are actual changes: `git status`
- Check if Git user is configured:
  ```bash
  git config user.name
  git config user.email
  ```

### Line ending warnings
- Run `.\git-normalize.bat` before committing
- Ensure `.gitattributes` exists

---

## Best Practices

1. **Commit often** - Small commits are easier to manage
2. **Write clear messages** - "Fixed X" not "Update"
3. **Run backup** - `git-auto.bat` runs backup.ps1 automatically
4. **Pull before push** - Use `git-sync.bat` at start of work
5. **Normalize endings** - Always use LF for Unity projects
