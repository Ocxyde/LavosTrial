# Code Lavos — DB System Analysis Report
**Date:** 2026-03-07  
**Files Analysed:** `DatabaseManager.cs`, `DatabaseConfig.cs`, `DatabaseSaveLoadHelper.cs`  
**Project:** Code.Lavos / PeuImporte — Unity 6 (6000.3.7f1)

---

## 🔴 CRITICAL BUGS

### 1. `Dictionary<string, string>` Will Not Serialize (GameSaveData)
**File:** `DatabaseManager.cs` — `GameSaveData` class  
**Problem:** `JsonUtility` does **not** support `Dictionary<K,V>`. The `gameSettings` field will silently serialize as `null` — every game setting is lost on every save/load cycle.  
**Fix:** Replace with a serializable key-value pair list:
```csharp
[System.Serializable]
public class StringPair
{
    public string key;
    public string value;
}

// In GameSaveData:
public List<StringPair> gameSettings; // instead of Dictionary<string, string>
```
Then convert to/from `Dictionary` at the boundary in `SaveAllData` / `LoadAllData`.

---

### 2. `ApplyPlayerDataToStats()` Does Nothing
**File:** `DatabaseManager.cs` — `ApplyPlayerDataToStats(PlayerStats stats)`  
**Problem:** The method only calls `stats.FullHeal()`. It never reads from `_currentPlayerData`. Any loaded health, mana, stamina, level, or experience values are discarded — the player is always reset to full.  
**Fix:**
```csharp
public void ApplyPlayerDataToStats(PlayerStats stats)
{
    if (!_isInitialized || stats == null) return;

    stats.SetHealth(_currentPlayerData.currentHealth, _currentPlayerData.maxHealth);
    stats.SetMana(_currentPlayerData.currentMana, _currentPlayerData.maxMana);
    stats.SetStamina(_currentPlayerData.currentStamina, _currentPlayerData.maxStamina);
    stats.level = _currentPlayerData.level;
    stats.experience = _currentPlayerData.experience;
}
```
*(Adjust to match actual `PlayerStats` API.)*

---

### 3. Single-File Multi-Slot: Only One Slot Really Works
**File:** `DatabaseManager.cs` — `SaveAllData` / `LoadAllData`  
**Problem:** All save slots write to the **same file** (`_databasePath`). Saving to slot 2 overwrites slot 1. `LoadAllData` then rejects slot 1 because `wrapper.data.saveSlot != saveSlot`. There is no real multi-slot support.  
**Fix:** Include the slot number in the filename:
```csharp
private string GetSavePath(int slot) =>
    Path.Combine(Path.GetDirectoryName(_databasePath),
                 $"GameData_slot{slot}.db");
```
Update `SaveAllData` and `LoadAllData` to call `GetSavePath(saveSlot)`.

---

## 🟠 BUGS

### 4. NullReferenceException Risk in `GetPlayerPosition` / `GetPlayerRotation`
**File:** `DatabaseManager.cs`  
**Problem:** Both methods directly access `_currentPlayerData` fields without null-checking. If called before `LoadAllData` or after a failed load, `_currentPlayerData` is `null` → NullReferenceException.  
**Fix:** Add guard:
```csharp
public Vector3 GetPlayerPosition()
{
    if (_currentPlayerData == null) return Vector3.zero;
    return new Vector3(...);
}
```

---

### 5. Status Effects Never Saved or Loaded in `DatabaseSaveLoadHelper`
**File:** `DatabaseSaveLoadHelper.cs` — `SaveGameCoroutine` / `LoadGameCoroutine`  
**Problem:** `DatabaseManager` has full `SetStatusEffects()` / `ApplyStatusEffects()` methods, but `DatabaseSaveLoadHelper` never calls either. Status effects are silently dropped on every save/load.  
**Fix:** Add to `SaveGameCoroutine`:
```csharp
// After gathering inventory
var playerStats = player?.GetComponent<PlayerStats>()
                  ?? FindFirstObjectByType<PlayerStats>();
if (playerStats != null)
    DatabaseManager.Instance.SetStatusEffects(playerStats.GetActiveEffects());
```
Add to `LoadGameCoroutine`:
```csharp
var playerStats = player?.GetComponent<PlayerStats>()
                  ?? FindFirstObjectByType<PlayerStats>();
if (playerStats != null)
    DatabaseManager.Instance.ApplyStatusEffects(playerStats);
```

---

### 6. Status Effect `remainingTime` Not Preserved on Load
**File:** `DatabaseManager.cs` — `GetStatusEffects()`  
**Problem:** `remainingTime` is set to `record.duration` instead of `record.remainingTime`. Every loaded status effect restarts from its full duration.  
**Fix:**
```csharp
remainingTime = record.remainingTime, // was: record.duration
```

---

### 7. `HasSave(int saveSlot)` Ignores the Slot Parameter
**File:** `DatabaseSaveLoadHelper.cs`  
**Problem:** `HasSave(saveSlot)` calls `DatabaseManager.Instance.HasSaveFile()` which only checks whether **any** file exists. Slot 2 will report "save exists" even if only slot 1 was ever saved.  
**Fix:** Once the multi-slot filename fix (#3) is in place, `HasSaveFile` should accept a slot parameter, or `HasSave` should check for the slot-specific file:
```csharp
public bool HasSave(int saveSlot)
{
    return File.Exists(DatabaseManager.Instance.GetSavePath(saveSlot));
}
```

---

### 8. Dead Singleton Reference After `OnDestroy`
**File:** `DatabaseManager.cs` — `OnDestroy()`  
**Problem:** `OnDestroy` sets `_isInitialized = false` but does **not** null `_instance`. After the object is destroyed, any call to `DatabaseManager.Instance` will try to use the dead reference and throw a `MissingReferenceException` before the null check can create a new instance.  
**Fix:**
```csharp
private void OnDestroy()
{
    _isInitialized = false;
    if (_instance == this) _instance = null;
}
```

---

## 🟡 CODE QUALITY ISSUES

### 9. `FindFirstObjectByType<T>()` Used Directly in `DatabaseSaveLoadHelper`
**File:** `DatabaseSaveLoadHelper.cs` — line using `FindFirstObjectByType<Inventory>()`  
**Problem:** `DatabaseManager` already provides a private `FindObject<T>()` wrapper for Unity 6 / pre-6 compatibility (`FindFirstObjectByType` vs `FindObjectOfType`). `DatabaseSaveLoadHelper` bypasses this and calls the Unity 6-only API directly, breaking builds on Unity 5/2021/2022.  
**Fix:** Expose `FindObject<T>()` as `internal static` in `DatabaseManager` and reuse it, or duplicate the same wrapper pattern in `DatabaseSaveLoadHelper`.

---

### 10. Data Classes Declared Outside the Namespace
**File:** `DatabaseManager.cs`  
**Problem:** `PlayerDataRecord`, `InventoryRecord`, `StatusEffectRecord`, `GameSaveData`, and `GameSaveDataWrapper` are all declared **after** the closing `}` of the `Code.Lavos.DB` namespace, placing them in the global namespace. This can cause naming collisions and violates the project's own namespace conventions.  
**Fix:** Move all `#region Data Classes` content inside `namespace Code.Lavos.DB { }`.

---

### 11. Assembly Definition Is Missing (`.asmdef.backup`)
**File:** `Assets/DB_SQLite/Code.Lavos.DB.asmdef.backup`  
**Problem:** The file is a renamed backup — there is **no active `.asmdef`** for this module. Without it, the DB scripts compile as part of the main assembly, losing isolation, slower incremental compilation, and risking circular dependency conflicts the architecture is designed to prevent.  
**Fix:** Rename the backup to `Code.Lavos.DB.asmdef` (removing `.backup`) and verify its assembly references (`Code.Lavos.Core` etc.) are correct.

---

### 12. Unknown `using Code.Lavos.Status` Namespace
**File:** `DatabaseManager.cs`  
**Problem:** `using Code.Lavos.Status;` is imported but this namespace does not appear in any architecture document. If it doesn't exist, this is a silent dead import at best, or a compile error at worst depending on the asmdef configuration.  
**Fix:** Verify whether `Code.Lavos.Status` exists. If not, remove the `using` directive and check whether `StatusEffect` / `EffectType` / `StatusEffectData` are resolved from elsewhere (likely `Code.Lavos.Core`).

---

### 13. `BackupDatabase()` Does Not Check for Null `dir`
**File:** `DatabaseManager.cs` — `BackupDatabase(string backupPath)`  
**Problem:** `Path.GetDirectoryName(backupPath)` returns `null` if `backupPath` has no directory component. Calling `Directory.Exists(null)` / `Directory.CreateDirectory(null)` will throw an `ArgumentNullException`.  
**Fix:**
```csharp
string dir = Path.GetDirectoryName(backupPath);
if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
    Directory.CreateDirectory(dir);
```

---

### 14. Inconsistent Brace Style in Class Declarations
**File:** `DatabaseManager.cs`, `DatabaseSaveLoadHelper.cs`  
**Problem:** Namespace bodies use Allman style (brace on new line), but the class declarations inside use a mixed style where the opening `{` is on the **same line** as the class name for the class body but the namespace follows Allman. This is inconsistent with both the surrounding code and standard C# conventions.  
**Fix:** Choose one style (Allman recommended for Unity C#) and apply it consistently:
```csharp
public class DatabaseManager : MonoBehaviour
{       // ← brace on new line, consistent with namespace
    ...
}
```

---

### 15. `EditorDbPath` Saves Inside the Assets Folder
**File:** `DatabaseConfig.cs`  
**Problem:** `EditorDbPath` points to `Assets/DB_SQLite/GameData.db` — inside the version-controlled Unity project. This will get committed to source control or cause `.meta` churn. It also means test saves from Editor Play Mode pollute the source tree.  
**Fix:** Use `Application.persistentDataPath` or a `Temp/` subfolder for editor saves, or at minimum add `*.db` to `.gitignore`.

---

## 📋 SUMMARY TABLE

| # | Severity | File | Issue |
|---|----------|------|-------|
| 1 | 🔴 Critical | DatabaseManager | `Dictionary` not serializable by JsonUtility — settings lost |
| 2 | 🔴 Critical | DatabaseManager | `ApplyPlayerDataToStats` never applies data |
| 3 | 🔴 Critical | DatabaseManager | Single file breaks multi-slot saves |
| 4 | 🟠 Bug | DatabaseManager | Null deref in `GetPlayerPosition` / `GetPlayerRotation` |
| 5 | 🟠 Bug | DatabaseSaveLoadHelper | Status effects never saved or loaded |
| 6 | 🟠 Bug | DatabaseManager | Status effect `remainingTime` reset to full on load |
| 7 | 🟠 Bug | DatabaseSaveLoadHelper | `HasSave(slot)` ignores slot parameter |
| 8 | 🟠 Bug | DatabaseManager | Dead singleton after `OnDestroy` |
| 9 | 🟡 Quality | DatabaseSaveLoadHelper | `FindFirstObjectByType` called without Unity 6 compat wrapper |
| 10 | 🟡 Quality | DatabaseManager | Data classes outside namespace |
| 11 | 🟡 Quality | DB_SQLite folder | `.asmdef` missing (only `.backup` present) |
| 12 | 🟡 Quality | DatabaseManager | `using Code.Lavos.Status` — namespace unverified |
| 13 | 🟡 Quality | DatabaseManager | Null arg risk in `BackupDatabase` |
| 14 | 🟡 Quality | DatabaseManager / Helper | Inconsistent brace style |
| 15 | 🟡 Quality | DatabaseConfig | Editor DB path inside Assets folder |

---

*Report generated 2026-03-07 — Code.Lavos DB System (DatabaseManager, DatabaseConfig, DatabaseSaveLoadHelper)*
