# Git Scripts - Project Files Only

## Quick Reference

| Script | Usage | Purpose |
|--------|-------|---------|
| `git-auto-project.bat` | `.\git-auto-project.bat "message"` | Commit **project files only** (not Library/obj) |
| `git-sync-project.bat` | `.\git-sync-project.bat "message"` | Pull → merge → commit **project files** |
| `git-status-project.bat` | `.\git-status-project.bat` | Status of **project folders** only |

---

## What Gets Committed (Project Files)

✅ **Included:**
- `Assets/Scripts/` - All C# scripts
- `Assets/Editor/` - Editor scripts
- `Assets/Input/` - Input System files
- `Assets/Prefabs/` - Prefab files
- `Assets/Scenes/` - Scene files
- `Assets/Ressources/` - Resources
- `Assets/*.unity`, `*.prefab`, `*.asset`, `*.mat`
- `Assets/*.inputactions`

❌ **Excluded:**
- `Library/` - Unity cache (never commit)
- `obj/` - Build intermediates
- `bin/` - Build outputs
- `Builds/` - Local builds
- `Packages/` - Package cache
- `UserSettings/` - Local settings

---

## Common Workflows

### After Coding - Commit Scripts Only
```bash
# Made changes to scripts, want to commit only those:
.\git-auto-project.bat "Fixed player movement bug"
```

This will:
1. Stage only `Assets/Scripts/` and related project folders
2. Skip `Library/`, `obj/`, `bin/`, etc.
3. Normalize line endings to LF
4. Run `backup.ps1` automatically
5. Commit with your message
6. Ask if you want to push

### Start of Day - Sync Project Files
```bash
# Pull latest, merge your project changes:
.\git-sync-project.bat "Merged latest changes"
```

### Check Project Status
```bash
# See only script/project changes:
.\git-status-project.bat
```

---

## Comparison: Project vs Full Scripts

| Script | Commits | Use Case |
|--------|---------|----------|
| `git-auto-project.bat` | Project files only | Daily coding work |
| `git-auto.bat` | Everything | Full solution changes |
| `git-sync-project.bat` | Project files only | Daily sync with remote |
| `git-sync.bat` | Everything | Full solution sync |

---

## Manual Git Commands (Project Files)

```bash
# Stage scripts only
git add Assets/Scripts/

# Stage all project files
git add Assets/Scripts/ Assets/Editor/ Assets/Input/ Assets/Prefabs/ Assets/Scenes/

# Check what's changed in scripts
git status --short Assets/Scripts/

# Commit scripts
git commit -m "Updated player controller"

# Normalize line endings for C# files
git add --renormalize "Assets/Scripts/*.cs"
```

---

## .gitignore Recommendations

Ensure these are in your `.gitignore` to never commit Unity temp files:

```gitignore
# Unity generated
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/

# Visual Studio / Rider
.vs/
*.suo
*.user
*.userosscache
*.sln.docstates

# Unity cache
*.pidb.meta
*.pdb.meta
*.mdb.meta

# OS generated
.DS_Store
Thumbs.db

# Backup files (handled by backup.ps1)
Backup_Solution/
```

---

## Best Practices

1. **Use `git-auto-project.bat`** for daily coding commits
2. **Never commit Library/obj/** - they're auto-generated
3. **Run backup** - scripts automatically call `backup.ps1`
4. **Commit often** - small commits are easier to review
5. **Write clear messages** - "Fixed X bug" not "Update"

---

## Troubleshooting

### "No changes detected in project folders"
The script only checks `Assets/Scripts/` and related folders.
To commit everything, answer `y` when prompted.

### "Commit failed - No changes"
Check if files are already staged:
```bash
git diff --cached
```

### Want to see what would be committed?
```bash
git diff --cached --stat
```
