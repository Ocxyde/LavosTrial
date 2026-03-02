# PROJECT STANDARDS & REMINDERS - PeuImporte

**Unity Version:** 6000.3.7f1 (Unity 6)  
**IDE:** Rider  
**Working Directory:** `D:\travaux_Unity\PeuImporte`  
**Last Updated:** 2026-03-02  

---

## 🎯 Core Architecture: Plug-in-and-Out System

```
GameManager.cs (Main Pivot Point)
    └── EventHandler.cs (Central Event Hub)
            ├── ItemEngine.cs (Item Registry)
            ├── InteractionSystem.cs (Input Manager)
            ├── CombatSystem.cs (Damage Calculator)
            └── PlayerStats.cs (Stat Engine)
                    └── All scripts plug in via EventHandler events
```

**Key Principle:** All scripts work independently but revolve around the core `GameManager` and `EventHandler`.

---

## 📝 Coding Standards

### 1. Unity 6 Standard (6000.3.7f1)

```csharp
// ✅ CORRECT - Unity 6 API
var player = FindFirstObjectByType<PlayerController>();

// ❌ WRONG - Deprecated Unity API
var player = FindObjectOfType<PlayerController>();
```

### 2. New Input System (Always)

```csharp
// ✅ CORRECT - New Input System
using UnityEngine.InputSystem;
var keyboard = Keyboard.current;
if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame) { ... }

// ❌ WRONG - Old Input System
if (Input.GetKeyDown(KeyCode.Space)) { ... }
```

### 3. Namespace Convention

```csharp
// ✅ CORRECT
namespace Code.Lavos.Core
namespace Code.Lavos.HUD
namespace Code.Lavos.Status

// ❌ WRONG
namespace Unity6.LavosTrial.*  // DEPRECATED
```

### 4. File Encoding

- **Line Endings:** Unix LF (`\n`)
- **Encoding:** UTF-8
- **Header:** Unity 6 compatible comment block

```csharp
// FileName.cs
// Description
// Unity 6 compatible - UTF-8 encoding - Unix line endings
//
// CORE: Role description
```

### 5. EventHandler Integration

```csharp
// ✅ CORRECT - Subscribe to events
private void SubscribeToEvents()
{
    if (EventHandler.Instance != null)
    {
        EventHandler.Instance.OnPlayerHealthChanged += OnHealthChanged;
        EventHandler.Instance.OnPlayerStaminaChanged += OnStaminaChanged;
    }
}

// ❌ WRONG - Direct component references
private PlayerStats _stats;  // Use events instead!
```

---

## 🔄 Backup Workflow

### Before Running Backup

**ALWAYS ask user first:**
```
⚠️ Ready to backup changes. Should I run backup.ps1 now?
```

### Backup Command

```powershell
# User runs this manually:
.\backup.ps1
```

### Backup Files

- **Location:** `Backup_Solution/`
- **Status:** READ-ONLY (never modify)
- **System:** Smart MD5 hash detection (only backs up changed files)

---

## 📂 Project Structure

```
PeuImporte/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/              # Core systems (GameManager, EventHandler)
│   │   ├── HUD/               # UI systems (UIBarsSystem, HUDSystem)
│   │   ├── Player/            # Player controller & stats
│   │   ├── Inventory/         # Inventory management
│   │   ├── Status/            # Status effects & damage
│   │   ├── Ressources/        # Resource factories
│   │   └── Editor/            # Editor tools
│   ├── Docs/                  # ALL *.md files go here
│   └── Input/                 # New Input System assets
│
├── diff_tmp/                  # Temporary diff files (auto-cleanup 2 days)
├── Backup_Solution/           # Read-only backups
└── Assets/Docs/               # Documentation storage
```

---

## 📜 Git Workflow Reminders

### After Making Changes

```powershell
# 1. Check status
git status

# 2. Review changes
git diff HEAD

# 3. Stage changes
git add Assets/Scripts/...

# 4. Commit
git commit -m "fix: description of changes"

# 5. Push (if needed)
git push
```

### Git Scripts Available

| Script | Purpose |
|--------|---------|
| `git-status.bat` | Quick status check |
| `git-commit.ps1` | Interactive commit |
| `git-sync.bat` | Pull + Push |
| `git-auto-commit.ps1` | Auto-commit all changes |

---

## 🔧 Diff Management

### Create Diff Files

All code changes should have corresponding diff files in `diff_tmp/`.

### Cleanup Old Diffs

```powershell
# User runs this manually:
.\cleanup-old-diffs.ps1
```

**Retention:** 2 days (auto-delete older files)

---

## 🚫 NEVER DO

1. ❌ **Never execute commands directly** on user's computer
2. ❌ **Never modify backup files** (read-only)
3. ❌ **Never use old Unity API** (FindObjectOfType, etc.)
4. ❌ **Never use old Input System** (Input.GetKeyDown, etc.)
5. ❌ **Never save .md files outside Assets/Docs/**
6. ❌ **Never break plug-in-and-out architecture**

---

## ✅ ALWAYS DO

1. ✅ **Ask before running backup.ps1**
2. ✅ **Use Unity 6 API** (FindFirstObjectByType, etc.)
3. ✅ **Use New Input System** (Keyboard.current, etc.)
4. ✅ **Use EventHandler** for cross-system communication
5. ✅ **Save .md files to Assets/Docs/**
6. ✅ **Maintain plug-in-and-out architecture**
7. ✅ **Show diffs for all changes**
8. ✅ **Use UTF-8 encoding with Unix LF**

---

## 🎯 Current System Status

| System | Status | Notes |
|--------|--------|-------|
| **UIBarsSystem** | ✅ Working | Uses EventHandler (Unity 6 standard) |
| **HUDSystem** | ✅ Available | Alternative modular HUD |
| **EventHandler** | ✅ Working | Central event hub |
| **PlayerStats** | ✅ Working | Stats engine integration |
| **CombatSystem** | ✅ Working | Auto-initializes StatsEngine |
| **MazeRenderer** | ✅ Working | Brighter lighting (2x ambient) |
| **Input System** | ✅ Working | New Input System throughout |

---

## 📋 Quick Reference

### EventHandler Events Available

```csharp
// Player Stats
EventHandler.Instance.OnPlayerHealthChanged += OnHealthChanged;
EventHandler.Instance.OnPlayerManaChanged += OnManaChanged;
EventHandler.Instance.OnPlayerStaminaChanged += OnStaminaChanged;

// Combat
EventHandler.Instance.OnDamageDealt += OnDamageDealt;
EventHandler.Instance.OnDamageTaken += OnDamageTaken;

// Items
EventHandler.Instance.OnItemPickedUp += OnItemPickedUp;
EventHandler.Instance.OnItemUsed += OnItemUsed;

// UI
EventHandler.Instance.OnFloatingTextRequested += ShowFloatingText;
EventHandler.Instance.OnNotificationRequested += ShowNotification;
```

### Unity 6 API Cheat Sheet

| Old API | Unity 6 API |
|---------|-------------|
| `FindObjectOfType<T>()` | `FindFirstObjectByType<T>()` |
| `FindObjectsOfType<T>()` | `FindObjectsByType<T>()` |
| `Input.GetKeyDown()` | `Keyboard.current.key.wasPressedThisFrame` |
| `Input.GetAxis()` | `Mouse.current.delta.ReadValue()` |

---

**Last Review:** 2026-03-02  
**Status:** ✅ **PRODUCTION READY**  
**Architecture:** Plug-in-and-Out compliant

---

*Document generated - Unity 6 (6000.3.7f1) compatible - UTF-8 encoding - Unix LF*
