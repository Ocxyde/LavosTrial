# ============================================================
#  Git LF Line Endings - Setup Instructions
# ============================================================
#  Unity 6 Project - D:\travaux_Unity\PeuImporte
#  Unity Version: 6000.3.7f1
# ============================================================

## Overview

This folder contains scripts to configure and maintain Unix LF line endings
in your Git repository, which is the standard for Unity projects.

---

## Scripts

### 1. git-setup-lf.bat (Run Once)

Initial setup script that:
- Configures Git to use LF line endings
- Creates `.gitattributes` file with proper rules
- Normalizes all existing files to LF
- Commits the changes

**Usage:**
```bash
# Run once to set up
.\git-setup-lf.bat
```

---

### 2. git-normalize.bat (Run After Changes)

Use this script after making code changes to ensure all new/modified files
use LF line endings.

**Usage:**
```bash
# Run after making changes
.\git-normalize.bat
```

---

### 3. .gitattributes (Auto-created)

This file tells Git how to handle line endings for different file types.
It ensures consistency across all team members' machines.

**Key rules:**
- `*.cs`, `*.unity`, `*.prefab` → LF
- `*.bat`, `*.cmd` → CRLF (Windows compatibility)
- All other text files → LF

---

## Manual Git Commands (Alternative)

If you prefer manual Git commands:

```bash
# Configure Git for LF (run once)
git config core.autocrlf input
git config core.eol lf

# Normalize all files (run after changes)
git rm --cached -r .
git add --renormalize .
git commit -m "Normalize line endings to LF"

# Check which files have CRLF
git grep -l $'\r'

# Verify no CRLF issues
git diff --check
```

---

## Verify Line Endings

### Check for CRLF files:
```bash
git grep -l $'\r'
```

### Check a specific file:
```bash
# In PowerShell
(Get-Content SomeFile.cs -Encoding Byte) | ForEach-Object { "{0:X2}" -f $_ }

# Look for: 0D 0A = CRLF (Windows), 0A = LF (Unix)
```

### In VS Code:
- Look at bottom-right corner: `LF` or `CRLF`
- Click to change line ending type

---

## Rider IDE Settings

To ensure Rider always saves with LF:

1. **File → Settings → Editor → Code Style → Line Endings**
   - Set to: `Unix (LF)`

2. **Or add to `.editorconfig`:**
   ```editorconfig
   [*]
   end_of_line = lf
   ```

---

## Troubleshooting

### "No changes to commit"
This is normal if all files already have LF endings.

### Git still shows CRLF files
Run: `git add --renormalize .` then commit again.

### Team members see conflicts
They should also run `git config core.autocrlf input` on their machines.

---

## Best Practices

1. **Run `git-normalize.bat` before each commit** to ensure consistency
2. **Add `.gitattributes` to version control** so team members get same settings
3. **Configure your IDE** (Rider) to use LF by default
4. **Tell your team** to use the same settings

---

## Questions?

Run `git config core.autocrlf` to see current setting.
Should return: `input`
