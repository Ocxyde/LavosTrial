# Git Scripts for Unity 6 Project
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

### 2. **git-init-and-push.ps1** - First Time Setup
Initialize Git repository and push to remote.

```powershell
# Full setup with remote
.\git-init-and-push.ps1 -RepoUrl "https://github.com/username/repo.git"

# Local only (add remote later)
.\git-init-and-push.ps1
```

**Parameters:**
- `-RepoUrl` - Your GitHub/GitLab/Bitbucket repository URL
- `-Branch` - Branch name (default: "main")
- `-CommitMessage` - Initial commit message

---

### 3. **git-commit.ps1** - Quick Commit
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

### 4. **git-push.ps1** - Push to Remote
Push commits to remote repository.

```powershell
# Push to origin/main
.\git-push.ps1

# Push to specific remote
.\git-push.ps1 -Remote "upstream" -Branch "develop"
```

---

### 5. **git-pull.ps1** - Pull from Remote
Pull latest changes from remote.

```powershell
# Standard pull
.\git-pull.ps1

# Pull with rebase
.\git-pull.ps1 -Rebase
```

---

### 6. **git-status.ps1** - Detailed Status
Show comprehensive repository status.

```powershell
.\git-status.ps1
```

**Shows:**
- Current branch
- Remote URL
- Uncommitted changes
- Recent commits
- Repository statistics

---

## 🚀 Quick Start Guide

### First Time Setup

1. **Create repository** on GitHub/GitLab/Bitbucket
2. **Copy repository URL**
3. **Run:**
   ```batch
   .\git-quick.bat
   ```
4. **Choose option 5** (Init & Push)
5. **Enter URL** when prompted
6. **Done!** ✅

---

### Daily Workflow

1. **Make changes** in Unity
2. **Run:**
   ```batch
   .\git-quick.bat
   ```
3. **Choose option 2** (Commit)
4. **Enter message**
5. **Choose option 3** (Push)
6. **Done!** ✅

---

## 📋 Common Commands

### Check Status
```powershell
.\git-status.ps1
# or
git status
```

### Commit Changes
```powershell
.\git-commit.ps1 "Your message here"
# or
git add .
git commit -m "Your message here"
```

### Push to Remote
```powershell
.\git-push.ps1
# or
git push
```

### Pull Latest
```powershell
.\git-pull.ps1
# or
git pull
```

---

## 🔧 Git Configuration

### Set Username & Email (One Time)
```bash
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"
```

### Check Configuration
```bash
git config --list
```

---

## 📦 What Gets Committed

### ✅ Included
- All C# scripts
- Unity scene files
- Prefabs
- Materials & Textures
- Shaders
- ScriptableObjects
- Project settings

### ❌ Excluded (.gitignore)
- Library/
- Temp/
- Obj/
- Builds/
- Backup_Solution/
- .vs/
- User settings

---

## 💡 Tips

1. **Commit often** - Small, frequent commits are better
2. **Write clear messages** - Explain what and why
3. **Pull before push** - Avoid conflicts
4. **Use branches** - For new features
5. **Backup first** - Scripts auto-run backup

---

## 🆘 Troubleshooting

### "Git not found"
Install Git from: https://git-scm.com/

### "No remote configured"
```bash
git remote add origin https://github.com/username/repo.git
```

### "Authentication failed"
- Set up SSH keys, or
- Use Personal Access Token (GitHub), or
- Use Git credential manager

### "Merge conflicts"
1. Open conflicting files
2. Resolve conflicts manually
3. `git add <file>`
4. `git commit`

---

## 📞 Need Help?

Run:
```batch
.\git-quick.bat
```

Or check:
```bash
git --help
git <command> --help
```

---

**Happy Git-ing!** 🎮✨
