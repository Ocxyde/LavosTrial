# Git Scripts - LavosTrial Project

**Last Updated:** March 2026  
**Unity Version:** 6000.3.7f1

---

## Quick Reference

| Script | Usage | Purpose |
|--------|-------|---------|
| `git-lavos.bat` | `.\git-lavos.bat "message"` | **Main script** - Commit Assets/ only, push to remote |
| `git-lavos-sync.bat` | `.\git-lavos-sync.bat "message"` | Pull → merge → commit Assets/ only |
| `git-lavos-status.bat` | `.\git-lavos-status.bat` | Status of Assets/ folder |
| `git-lavos-setup.bat` | `.\git-lavos-setup.bat` | Configure remote + .gitignore |

---

## Tracked Files (Assets/ Only)

✅ **Included:**
```
Assets/
├── Scripts/          # All C# scripts
├── Editor/           # Editor scripts
├── Scenes/           # Unity scenes
├── Prefabs/          # Prefab files
├── Input/            # Input System files
├── Ressources/       # Resources
├── Art/              # Art assets
├── Settings/         # Project settings
├── Tests/            # Unit tests
├── *.unity           # Scene files
├── *.prefab          # Prefab files
├── *.asset           # Asset files
├── *.mat             # Material files
├── *.inputactions    # Input System
└── *.cs              # C# scripts
```

❌ **Excluded (via .gitignore):**
```
[Ll]ibrary/           # Unity cache
[Tt]emp/              # Temp files
[Oo]bj/               # Build intermediates
[Bb]uild/             # Build outputs
[Bb]uilds/            # Local builds
Backup_Solution/      # Backup folder
*.ps1                 # PowerShell scripts (local tools)
*.bat                 # Batch files (local tools)
*.log                 # Log files
.vs/                  # Visual Studio
```

---

## First Time Setup

```bash
# 1. Run setup script
.\git-lavos-setup.bat

# 2. Enter your Git remote URL when prompted
# Example: https://github.com/yourname/LavosTrial.git

# 3. Setup creates/updates:
#    - .gitignore (excludes build artifacts, backups, scripts)
#    - .gitattributes (LF line endings for Unity files)
#    - Git remote configuration
```

---

## Daily Workflows

### After Coding - Commit & Push
```bash
.\git-lavos.bat "Fixed player movement bug"
```

This will:
1. Check remote configuration
2. Show Assets/ status
3. Stage only `Assets/` files
4. Normalize line endings to LF
5. Run backup automatically (not committed)
6. Commit with your message
7. Ask if you want to push

### Start of Day - Sync with Remote
```bash
.\git-lavos-sync.bat "Merged latest changes"
```

### Check Status
```bash
.\git-lavos-status.bat
```

---

## Available Scripts

### Core Scripts
| Script | Description |
|--------|-------------|
| `git-lavos.bat` | Main commit script for Assets/ |
| `git-lavos-sync.bat` | Pull + merge + commit |
| `git-lavos-status.bat` | Show Assets/ status |
| `git-lavos-setup.bat` | Initial setup |

### Helper Scripts
| Script | Description |
|--------|-------------|
| `git-status.bat` | Full git status |
| `git-status-project.bat` | Status for project files |
| `git-sync.bat` | Full sync (all files) |
| `git-sync-project.bat` | Sync project files |
| `git-auto.bat` | Auto-commit helper |
| `git-auto-project.bat` | Auto-commit project files |
| `git-normalize.bat` | Normalize line endings |
| `git-setup-lf.bat` | Setup LF line endings |

### Backup Scripts (Not Git)
| Script | Description |
|--------|-------------|
| `backup.ps1` | Create backup |
| `backup_full.ps1` | Full backup |
| `run_backup_and_diff.bat` | Backup + diff |

---

## .gitignore

The `.gitignore` file excludes:

```gitignore
# Unity generated
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Lo]ogs/

# Visual Studio / Rider
.vs/
*.suo
*.user
*.sln.docstates

# Backup files
Backup_Solution/
backup/
*.bak

# Scripts (local tools only)
*.ps1
*.bat
*.cmd

# OS generated
.DS_Store
Thumbs.db
desktop.ini
```

---

## .gitattributes

Ensures LF line endings for Unity files:

```gitattributes
# Force LF for all text files
* text=auto eol=lf

# C# files
*.cs text eol=lf
*.asmdef text eol=lf

# Unity YAML
*.unity text eol=lf
*.prefab text eol=lf
*.asset text eol=lf
*.mat text eol=lf

# Shaders
*.shader text eol=lf
*.hlsl text eol=lf

# Text and config
*.json text eol=lf
*.xml text eol=lf
*.md text eol=lf
```

---

## Manual Git Commands

```bash
# Stage Assets/ only
git add Assets/

# Check Assets/ status
git status --short Assets/

# Commit Assets/ changes
git commit -m "Updated scripts"

# Push to remote
git push origin main

# Pull from remote
git pull origin main
```

---

## What Gets Committed

| File Type | Committed? |
|-----------|------------|
| `Assets/Scripts/*.cs` | ✅ Yes |
| `Assets/Scenes/*.unity` | ✅ Yes |
| `Assets/Prefabs/*.prefab` | ✅ Yes |
| `Assets/Tests/*.cs` | ✅ Yes |
| `Assets/*.inputactions` | ✅ Yes |
| `backup.ps1` | ❌ No |
| `git-lavos.bat` | ❌ No |
| `Backup_Solution/` | ❌ No |
| `Library/` | ❌ No |
| `Builds/` | ❌ No |

---

## Troubleshooting

### "No changes in Assets/ folder"
Only files in `Assets/` are tracked. Scripts (*.ps1, *.bat) are intentionally excluded.

### "Remote not configured"
Run `.\git-lavos-setup.bat` to add your remote.

### "File still appears in git status"
Run setup again to update `.gitignore`:
```bash
.\git-lavos-setup.bat
# Answer 'y' to update .gitignore

# Then remove cached files
git rm --cached *.ps1 *.bat
git commit -m "Remove scripts from tracking"
```

### "Line ending warnings"
Run `.\git-normalize.bat` to fix line endings.

---

## Backup Integration

- `backup.ps1` runs automatically before commits
- Backups go to `Backup_Solution/` folder (excluded from Git)
- Verify backups in `Backup_Solution/` after each commit

---

## Best Practices

1. **Only Assets/ is tracked** - Scripts stay local for flexibility
2. **Commit often** - Small commits are easier to manage
3. **Backup runs automatically** - Verify `Backup_Solution/` folder
4. **LF line endings** - Normalized automatically
5. **Test in Unity** - Ensure no compile errors before push
6. **Run unit tests** - Tests located in `Assets/Scripts/Tests/`

---

## Remote URL Examples

```
# GitHub HTTPS
https://github.com/username/LavosTrial.git

# GitHub SSH
git@github.com:username/LavosTrial.git

# GitLab HTTPS
https://gitlab.com/username/LavosTrial.git

# Azure DevOps
https://dev.azure.com/org/project/_git/LavosTrial

# Bitbucket
https://username@bitbucket.org/username/LavosTrial.git
```

---

## Related Documentation

- `TODO.md` - Current issues and resolutions
- `README.md` - Project overview
- `backup.md` - Backup system documentation
