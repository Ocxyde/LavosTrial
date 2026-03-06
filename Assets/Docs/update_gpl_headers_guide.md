# Update-GPLHeaders.ps1 - Documentation

**Date:** 2026-03-06  
**Purpose:** Batch update GPL license headers from "PeuImporte" to "Code.Lavos"  
**Location:** Project root (CodeDotLavos/)

---

## 📋 **WHAT IT DOES**

This script scans all C# files in `Assets/Scripts/` and updates the GPL-3.0 license header:

**Before:**
```csharp
// This file is part of PeuImporte.
//
// PeuImporte is free software...
// along with PeuImporte.
```

**After:**
```csharp
// This file is part of Code.Lavos.
//
// Code.Lavos is free software...
// along with Code.Lavos.
```

---

## 🎯 **USAGE**

### **Basic Usage (Dry Run):**
```powershell
.\Update-GPLHeaders.ps1 -WhatIf
```
Shows what would change without modifying files.

### **Apply Changes:**
```powershell
.\Update-GPLHeaders.ps1
```
Updates all files with "PeuImporte" in GPL header.

### **Verbose Mode:**
```powershell
.\Update-GPLHeaders.ps1 -Verbose
```
Shows detailed output for each file processed.

---

## 📊 **FEATURES**

✅ **Safe Operation:**
- Creates backup log in `diff_tmp/` before changes
- Shows what would change with `-WhatIf` flag
- Reports all modifications

✅ **Preserves File Format:**
- UTF-8 encoding maintained
- Unix LF line endings enforced
- No BOM added

✅ **Comprehensive:**
- Scans all `.cs` files recursively
- Replaces all 4 occurrences per file:
  1. "This file is part of PeuImporte."
  2. "PeuImporte is free software:"
  3. "PeuImporte is distributed"
  4. "along with PeuImporte."

✅ **Detailed Reporting:**
- Shows total files scanned
- Lists modified files
- Reports errors
- Creates backup log

---

## 📁 **OUTPUT**

### **Console Output:**
```
════════════════════════════════════════════════
  GPL HEADER UPDATE: PeuImporte → Code.Lavos   
════════════════════════════════════════════════

[Update-GPLHeaders] Scanning for .cs files in Assets/Scripts/...
[Update-GPLHeaders] Found 80 C# files

  ✓ Updated: PathFinder.cs
  ✓ Updated: CompleteMazeBuilder.cs
  ...

════════════════════════════════════════════════
  SUMMARY                                      
════════════════════════════════════════════════
  Total Files Scanned:  80
  Files Modified:       45
  Errors:               0
════════════════════════════════════════════════

[Update-GPLHeaders] ✓ Successfully updated 45 files!

[REMINDER] Run backup.ps1 to save these changes!
```

### **Backup Log:**
Location: `diff_tmp/gpl_headers_backup_YYYYMMDD_HHMMSS.txt`

Contains:
- Date/time of update
- List of modified files
- Number of replacements per file
- Original and new project names

---

## 🔒 **SAFETY FEATURES**

| Feature | Description |
|---------|-------------|
| **WhatIf Mode** | Test without modifying (`-WhatIf`) |
| **Backup Log** | Automatic backup in `diff_tmp/` |
| **Error Handling** | Continues on error, reports issues |
| **Encoding Safe** | Preserves UTF-8 encoding |
| **Line Ending Safe** | Enforces Unix LF |

---

## 📝 **EXAMPLES**

### **1. Test First (Recommended):**
```powershell
.\Update-GPLHeaders.ps1 -WhatIf
```

### **2. Apply Changes:**
```powershell
.\Update-GPLHeaders.ps1
```

### **3. Verbose Output:**
```powershell
.\Update-GPLHeaders.ps1 -Verbose -WhatIf
```

---

## ⚠️ **IMPORTANT NOTES**

1. **Run from project root:**
   ```powershell
   cd D:\travaux_Unity\CodeDotLavos
   .\Update-GPLHeaders.ps1
   ```

2. **Backup first (recommended):**
   ```powershell
   .\backup.ps1
   .\Update-GPLHeaders.ps1
   .\backup.ps1  # Backup after changes
   ```

3. **Git users:**
   ```bash
   git status  # See all modified files
   git diff    # Review changes
   git commit -m "Update GPL headers to Code.Lavos"
   ```

---

## 📊 **EXPECTED RESULTS**

| File Type | Count | Action |
|-----------|-------|--------|
| **Total .cs files** | ~80 | Scanned |
| **Files with GPL header** | ~60 | Updated |
| **Files without header** | ~20 | Skipped |
| **Editor scripts** | ~10 | Updated |
| **Test files** | ~5 | Updated |

---

## 🐛 **TROUBLESHOOTING**

### **Error: "Cannot run script"**
**Solution:** Enable script execution:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### **Error: "Access denied"**
**Solution:** Run PowerShell as Administrator

### **No files modified**
**Possible causes:**
- Files already updated (run once only)
- Files don't have GPL header
- Wrong directory (run from project root)

---

## ✅ **VERIFICATION**

After running, verify changes:

### **1. Check a file:**
```powershell
Get-Content Assets\Scripts\Core\11_Utilities\PathFinder.cs -Head 20
```

### **2. Search for old name:**
```powershell
Select-String -Path "Assets\Scripts\**\*.cs" -Pattern "PeuImporte" -SimpleMatch
```
Should return no results (or only comments).

### **3. Search for new name:**
```powershell
Select-String -Path "Assets\Scripts\**\*.cs" -Pattern "Code.Lavos" -SimpleMatch
```
Should return all updated files.

---

## 📦 **AFTER RUNNING**

**DON'T FORGET:**
```powershell
.\backup.ps1
```

Then commit to git:
```bash
git add .
git commit -m "Update GPL headers from PeuImporte to Code.Lavos"
```

---

## 📄 **LICENSE**

This script is part of **Code.Lavos** and licensed under **GPL-3.0**.

**Copyright © 2026 Ocxyde. All rights reserved.**

---

*Documentation - Unity 6 compatible - UTF-8 encoding - Unix LF*
