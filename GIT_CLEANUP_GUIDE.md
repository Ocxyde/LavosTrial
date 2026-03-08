# Git Repository Cleanup Guide

**Created:** 2026-03-07  
**Script Version:** 1.0  
**Purpose:** Remove files from git tracking that should be ignored

---

## 🎯 **What This Does**

This cleanup script removes files from git tracking that are already in `.gitignore` but were accidentally committed. It does **NOT** delete the actual files from your disk - it only stops git from tracking them.

---

## 📁 **Files Cleaned**

### **Phase 1: Archive Files**
- `*.zip` - Build archives
- `*.7z` - Compressed archives
- `*.unitypackage` - Unity packages

**Examples:**
- `Builds_0-1.zip`
- `Builds.7z`
- `Code.Lav8s.zip`
- `maze_v0-6.zip`
- `Linh.unitypackage`

### **Phase 2: Backup Folders**
- `Backup/` - Numbered backup files
- `Backup_Solution/` - Old backup solution
- `Backup_Deprecated_*/` - Deprecated backups

**Why:** Use git history for versioning, not manual backups

### **Phase 3: Version Folders**
- `maze_v0-6/`
- `maze_v0-6-8_ushort_2byte_saves/`
- `maze_v0-6-9_1-ubit/`
- `maze_v0-6-9-ubit/`
- `maze_v0-7-8_ushort_2byte_saves/`

**Why:** Duplicate code - active code is in `Assets/Scripts/Core/`

### **Phase 4: Unity Generated**
- `Library/` - Unity library cache
- `Temp/` - Temporary files
- `Obj/` - Build intermediates
- `Builds/` - Build output
- `Logs/` - Log files

### **Phase 5: IDE Configuration**
- `.vs/` - Visual Studio
- `.idea/` - JetBrains Rider

---

## 🚀 **Usage**

### **Option 1: Dry Run (Recommended First)**

See what would be removed without actually removing anything:

```powershell
.\git-cleanup-dry-run.bat
```

**OR**

```powershell
.\git-cleanup-repo.ps1
```

### **Option 2: Apply Changes**

Actually remove files from git tracking:

```powershell
.\git-cleanup-apply.bat
```

**OR**

```powershell
.\git-cleanup-repo.ps1 -ApplyChanges
```

---

## 📋 **Step-by-Step Process**

### **1. Run Dry Run First**

```powershell
# See what would be removed
.\git-cleanup-dry-run.bat
```

**Review the output:**
- Check file count
- Check total size
- Verify no important files will be removed

### **2. Apply Cleanup**

```powershell
# Actually remove files from git
.\git-cleanup-apply.bat
```

**Enter YES to confirm**

### **3. Commit Changes**

```powershell
git commit -m "chore: Remove ignored files from git tracking

Removed:
- Archive files (zip, 7z, unitypackage)
- Backup folders (use git history instead)
- Version folders (active code in Assets/)
- Unity generated folders (Library, Temp, etc.)
- IDE folders (.vs, .idea)

This reduces repository size and removes files
that should not be tracked.
"
```

### **4. Verify**

```powershell
git status
```

Should show clean working tree.

### **5. Push (Optional)**

```powershell
git push
```

---

## ⚠️ **Important Notes**

### **Files Are NOT Deleted**

The script uses `git rm --cached` which:
- ✅ Removes files from git tracking
- ✅ **KEEPS** files on your disk
- ✅ Files become "untracked" in git status

### **Backup First**

Always run backup before git operations:

```powershell
.\backup.ps1
```

### **Large Files**

If you have large archives (>100MB), they may have already been pushed to remote. After cleanup, you may need to:

```powershell
# Force push (be careful!)
git push --force
```

**Warning:** This rewrites history. Coordinate with team if applicable.

---

## 🔧 **Manual Cleanup (Alternative)**

If you prefer manual control:

### **Remove single file:**
```powershell
git rm --cached Builds_0-1.zip
```

### **Remove folder:**
```powershell
git rm -r --cached Backup/
```

### **Remove all ignored files:**
```powershell
git rm -r --cached .
git add .
```

---

## 📊 **Expected Results**

### **Before Cleanup:**
```
On branch main
Changes to be committed:
  new file:   Builds_0-1.zip
  new file:   Backup/Script_00001.cs
  ...
```

### **After Cleanup:**
```
On branch main
nothing to commit, working tree clean
```

### **Repository Size Reduction:**

**Estimated:**
- Archive files: ~50-200 MB
- Backup folders: ~10-50 MB
- Version folders: ~5-20 MB
- Unity generated: ~100-500 MB

**Total:** ~165-770 MB potential reduction

---

## 🐛 **Troubleshooting**

### **Error: "git is not recognized"**

Git is not in your PATH. Use full path:

```powershell
"C:\Program Files\Git\bin\git.exe" rm --cached filename.zip
```

### **Error: "Permission denied"**

Close Unity and Rider/Visual Studio, then retry.

### **File still shows in git status**

The file is still tracked. Run:

```powershell
git rm --cached <filename>
git commit -m "Remove <filename> from tracking"
```

### **Remote still has large files**

After cleanup, force push may be needed:

```powershell
git push --force origin main
```

**Warning:** This rewrites history. Use with caution.

---

## 📝 **Post-Cleanup .gitignore**

The script will add these patterns to `.gitignore` if missing:

```gitignore
# Archives
*.zip
*.7z
*.unitypackage

# Backup folders
Backup/
Backup_Solution/
Backup_Deprecated_*/

# Version folders
maze_v0-*/

# Build folders
Builds_*/
```

---

## ✅ **Verification Checklist**

After cleanup:

- [ ] `git status` shows clean working tree
- [ ] Archive files no longer tracked
- [ ] Backup folders no longer tracked
- [ ] Version folders no longer tracked
- [ ] `.gitignore` updated with new patterns
- [ ] Backup created with `backup.ps1`
- [ ] Git commit created
- [ ] (Optional) Git push completed

---

## 📚 **Related Scripts**

| Script | Purpose |
|--------|---------|
| `git-cleanup-dry-run.bat` | Preview cleanup (no changes) |
| `git-cleanup-apply.bat` | Apply cleanup (removes files) |
| `git-cleanup-repo.ps1` | PowerShell version (full control) |
| `backup.ps1` | Create backup before cleanup |
| `git-status.bat` | Check git status |

---

## 🎯 **Best Practices**

1. **Always run dry-run first** - Know what will be removed
2. **Backup before cleanup** - Use `backup.ps1`
3. **Commit immediately after** - Don't mix with other changes
4. **Review .gitignore** - Ensure all patterns are correct
5. **Coordinate with team** - If working collaboratively

---

**Last Updated:** 2026-03-07  
**Script Version:** 1.0  
**Unity Version:** 6000.3.7f1

*Happy cleaning, coder friend!*
