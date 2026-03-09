# Git Large Files Cleanup - Quick Start

**Date:** 2026-03-09
**Purpose:** Quick reference for cleaning up large tracked files

---

## 🚀 QUICK START (3 Steps)

### Step 1: Analyze
```powershell
.\analyze-git-large-files.ps1
```
**What it does:** Shows you what's too large and shouldn't be tracked.

### Step 2: Cleanup
```powershell
.\cleanup-git-large-files.ps1
```
**What it does:** Removes large files from Git tracking (keeps local copies).

### Step 3: Commit
```powershell
git status                    # Review changes
git commit -m "chore: remove large files from Git tracking"
git push                      # Optional: push to remote
```

---

## 📦 WHAT WILL BE REMOVED

Based on your project scan:

| Category | Files | Estimated Size |
|----------|-------|----------------|
| **ZIP Archives** | ~25 files | ~100+ MB |
| **Backup Folders** | maze_v0-*/ | ~50+ MB |
| **Build Folders** | Builds_*/ | ~20+ MB |
| **Unity Cache** | Library/, Temp/ | ~200+ MB |
| **Scripts** | *.ps1, *.bat | ~1 MB |

**Total estimated cleanup:** ~370+ MB

---

## ⚠️ IMPORTANT NOTES

1. **Files are NOT deleted** - Only removed from Git tracking
2. **Local copies remain** - Your files stay on disk
3. **Run backup first** - Always backup before Git operations
4. **Review before committing** - Check `git status` output

---

## 📋 DETAILED WORKFLOW

### Option A: Automated (Recommended)

```powershell
# 1. Analyze first
.\analyze-git-large-files.ps1

# 2. Preview cleanup (dry run)
.\cleanup-git-large-files.ps1 -WhatIf

# 3. Run actual cleanup
.\cleanup-git-large-files.ps1

# 4. Review and commit
git status
git commit -m "chore: remove large files from Git tracking"
```

### Option B: Manual Commands

```powershell
# Remove ZIP files
git rm --cached *.zip

# Remove backup folders
git rm -r --cached maze_v0-*/ Backup/ Backup_Solution/

# Remove build folders
git rm -r --cached Builds/ Builds_*/

# Remove Unity cache
git rm -r --cached Library/ Temp/ Obj/

# Remove scripts (optional)
git rm --cached *.ps1 *.bat

# Commit
git status
git commit -m "chore: remove large files from Git tracking"
```

---

## 🔧 ADVANCED: Clean Git History

If repository is still large after cleanup:

### Using BFG Repo-Cleaner (Recommended)

```bash
# Download BFG from: https://rtyley.github.io/bfg-repo-cleaner/

# Remove blobs > 5MB
java -jar bfg.jar --strip-blobs-bigger-than 5M .git

# Clean up
git reflog expire --expire=now --all
git gc --prune=now --aggressive

# Force push (WARNING: rewrites history!)
git push --force
```

### Using Git Filter-Repo (Advanced)

```bash
pip install git-filter-repo
git filter-repo --strip-blobs-bigger-than 5M
git push --force
```

⚠️ **WARNING:** History rewriting affects all collaborators!

---

## 📊 EXPECTED RESULTS

| Metric | Before | After |
|--------|--------|-------|
| `.git` size | ~500MB | ~50MB |
| Clone time | 5+ min | <1 min |
| ZIP files | 25+ | 0 |
| Tracked files | 500+ | ~200 |

---

## 🛡️ PREVENTION

Your `.gitignore` has been updated with:

```gitignore
# ZIP archives (root and all subfolders)
*.zip
*.zip.meta
```

**Additional prevention:**
- Pre-commit hooks (see `GIT_LARGE_FILES_CLEANUP.md`)
- Regular monthly cleanup
- Monitor with `git ls-files`

---

## 📚 DOCUMENTATION

| File | Purpose |
|------|---------|
| `GIT_LARGE_FILES_CLEANUP.md` | Complete guide |
| `analyze-git-large-files.ps1` | Analysis script |
| `cleanup-git-large-files.ps1` | Cleanup script |
| `GIT_LARGE_FILES_QUICKSTART.md` | This file |

---

## ✅ CHECKLIST

- [ ] Read this guide
- [ ] Run backup: `.\backup.ps1`
- [ ] Analyze: `.\analyze-git-large-files.ps1`
- [ ] Review analysis output
- [ ] Cleanup: `.\cleanup-git-large-files.ps1`
- [ ] Check: `git status`
- [ ] Commit: `git commit -m "chore: remove large files"`
- [ ] (Optional) Push: `git push`
- [ ] (Optional) Clean history with BFG
- [ ] Verify `.git` folder size reduced

---

## 🆘 TROUBLESHOOTING

**Q: "File still tracked after cleanup"**
- A: Use recursive remove: `git rm -r --cached FolderName/`

**Q: "Can't push after cleanup"**
- A: Normal - large files are being removed from remote
- A: If history rewritten, use: `git push --force`

**Q: "Teammates can't pull"**
- A: They need to re-clone after history rewrite
- A: Or: `git fetch origin && git reset --hard origin/main`

---

## 📞 REMINDERS

⚠️ **Remember:**
1. Run `backup.ps1` before Git operations (your rule!)
2. Review `git status` before committing
3. Use git for version control
4. Keep large files in external storage

---

**Ready to clean up?** Run the analysis script first! 🧹

```powershell
.\analyze-git-large-files.ps1
```

---

*Keep your Git repository lean and fast!* 💨
