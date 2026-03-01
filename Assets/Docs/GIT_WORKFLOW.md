# Git Workflow Guide - Unity 6 Project
**Location:** `Assets/Docs/GIT_WORKFLOW.md`  
**Created:** 2026-03-01  
**Unity Version:** 6000.3.7f1  

---

## 📜 Available Scripts

### 1. **git-quick.bat** - Interactive Menu (Recommended!)
Quick access to all Git operations via a simple menu.

```batch
.\git-quick.bat
```

**Features:**
- Status, Commit, Push, Pull
- View log and diff
- Clean Git cache
- Easy first-time setup

---

### 2. **git-commit.ps1** - Quick Commit
Commit changes with automatic backup.

```powershell
# Simple commit
.\git-commit.ps1 "Fixed player movement bug"

# Commit all changes
.\git-commit.ps1 "Updated shaders" -All

# Commit without backup
.\git-commit.ps1 "Minor fix" -NoBackup
```

**Features:**
- ✅ Auto-runs backup before commit
- ✅ Adds timestamp to commit message
- ✅ Shows change summary

---

### 3. **git-push.ps1** - Push to Remote
Push commits to remote repository.

```powershell
# Push to origin/main
.\git-push.ps1

# Push to specific remote
.\git-push.ps1 -Remote "upstream" -Branch "develop"
```

---

### 4. **git-pull.ps1** - Pull from Remote
Pull latest changes from remote.

```powershell
# Standard pull
.\git-pull.ps1

# Pull with rebase
.\git-pull.ps1 -Rebase
```

---

### 5. **git-status.ps1** - Detailed Status
Show comprehensive repository status.

```powershell
.\git-status.ps1
```

**Shows:**
- Current branch
- Remote URL
- Uncommitted changes
- Recent commits

---

### 6. **git-auto-commit.ps1** - Auto-Message Commit
Automatically generates commit message from changes.

```powershell
.\git-auto-commit.ps1
```

---

### 7. **git-sync.bat** - Sync (Push + Pull)
Quick sync with remote repository.

```batch
.\git-sync.bat
```

---

## 🎯 Daily Workflow

### For Your Current Changes (SFX/VFX Engine + Door Fix):

#### **Option 1: Interactive (Easiest)**
```batch
.\git-quick.bat
```
Then select:
- Status (see changes)
- Commit (add message)
- Push (upload)

#### **Option 2: Manual Steps**
```powershell
# 1. Check what changed
.\git-status.ps1

# 2. Commit with message
.\git-commit.ps1 "Added SFX/VFX Engine with event integration + disabled start door"

# 3. Push to remote
.\git-push.ps1
```

#### **Option 3: One-Liner Auto-Commit**
```powershell
.\git-auto-commit.ps1
```
Auto-generates commit message from changes!

---

## 📋 Recommended Workflow

```powershell
# 1. Run backup first (ALWAYS!)
.\backup.ps1

# 2. Check what changed
.\git-status.ps1

# 3. Commit with descriptive message
.\git-commit.ps1 "Added SFX/VFX Engine + disabled start door"

# 4. Push to remote
.\git-push.ps1
```

---

## 🚀 First Time Setup

```powershell
# Initialize Git with remote
.\git-init-and-push.ps1 -RepoUrl "https://github.com/yourusername/PeuImporte.git"

# Or local only (add remote later)
.\git-init-and-push.ps1
```

---

## 📊 Script Reference

| Script | Purpose | When to Use |
|--------|---------|-------------|
| `git-quick.bat` | ⭐ Interactive menu | Anytime! |
| `git-status.ps1` | Show repository status | Before/after work |
| `git-commit.ps1` | Commit with backup | After changes |
| `git-auto-commit.ps1` | Auto-message commit | Quick commits |
| `git-push.ps1` | Push to remote | After commit |
| `git-pull.ps1` | Pull from remote | Before work |
| `git-sync.bat` | Push + Pull | Sync workflow |

---

## ⚠️ Important Notes

1. **ALWAYS run backup.ps1 before committing**
2. **All .md files must be in Assets/Docs/**
3. **Use UTF-8 encoding with Unix LF line endings**
4. **Commit messages should be clear and descriptive**

---

## 🔧 Troubleshooting

### Git not found?
```powershell
# Check Git installation
git --version

# If not installed, download from: https://git-scm.com/
```

### Permission denied on push?
```powershell
# Check remote URL
git remote -v

# Update if needed
git remote set-url origin https://github.com/username/repo.git
```

### Conflicts on pull?
```powershell
# See conflicts
git status

# Resolve manually, then:
git add .
git commit -m "Resolved merge conflicts"
git push
```

---

*Documentation: 2026-03-01*  
*Unity 6 (6000.3.7f1) compatible*  
*UTF-8 encoding - Unix line endings*
