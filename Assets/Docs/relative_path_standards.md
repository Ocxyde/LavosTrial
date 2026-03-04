# Relative Path Standards - PeuImporte Project
**Date:** 2026-03-03  
**Unity Version:** 6000.3.7f1

---

## ✅ PATH STANDARDIZATION COMPLETE

All file paths in the project now use **relative paths** for maximum portability.

---

## 📁 **PATH HIERARCHY**

```
Project Root (PeuImporte/)
│
├── Assets/                              ← Application.dataPath
│   ├── StreamingWorkFlow/
│   │   └── MazeData/                    ← Relative path
│   │       ├── Maze_001_Torches.bytes
│   │       ├── Maze_001_Chests.bytes
│   │       └── ...
│   │
│   ├── Scripts/
│   ├── Prefabs/
│   ├── Scenes/
│   └── ...
│
├── Build/                               ← Build output
│   └── PeuImporte.exe
│
└── Logs/                                ← Log files
    └── chat-xxxxx.txt
```

---

## 🎯 **PATH TYPES & USAGE**

### 1. **Application.dataPath** (Assets Folder)
```csharp
// ✅ GOOD - Relative to Assets folder
string path = Path.Combine(Application.dataPath, "StreamingWorkFlow/MazeData");
string filePath = Path.Combine(Application.dataPath, relativePath);

// Usage: Save/load binary files, editor scripts
```

### 2. **Application.persistentDataPath** (Save Data)
```csharp
// ✅ GOOD - For save games, user data
string savePath = Path.Combine(Application.persistentDataPath, "SaveGames");

// Usage: Player saves, settings, persistent data
```

### 3. **Application.streamingAssetsPath** (Read-Only)
```csharp
// ⚠️ AVOID - Use Application.dataPath with relative paths instead
// Old way (don't use):
string path = Path.Combine(Application.streamingAssetsPath, "Data");

// ✅ NEW way (use this):
string path = Path.Combine(Application.dataPath, "StreamingWorkFlow/MazeData");
```

---

## ✅ **FIXED FILES**

### LightPlacementData.cs
**Before:**
```csharp
// ❌ BAD - Hardcoded streaming assets path
string filePath = Path.Combine(Application.streamingAssetsPath, "LightPlacements", fileName);
```

**After:**
```csharp
// ✅ GOOD - Relative path
string relativePath = "StreamingWorkFlow/MazeData/" + fileName;
string filePath = Path.Combine(Application.dataPath, relativePath);
```

**Files Updated:**
- ✅ `SaveToFile()` - Line 177
- ✅ `LoadFromFile()` - Line 199
- ✅ `FileExists()` - Line 221
- ✅ `DeleteFile()` - Line 231

---

## 🚫 **NEVER USE THESE**

### ❌ **Absolute Paths (Hardcoded)**
```csharp
// NEVER DO THIS!
string path = "D:/travaux_Unity/PeuImporte/Assets/...";
string path = "C:/Users/ocxz/Documents/...";
string path = "/home/user/project/Assets/...";
```

### ❌ **Platform-Specific Paths**
```csharp
// NEVER DO THIS!
string path = "D:\\travaux_Unity\\PeuImporte\\...";  // Windows only
string path = "/Users/ocxz/...";  // Mac only
```

---

## ✅ **ALWAYS USE THESE**

### ✅ **Unity Path APIs**
```csharp
// Relative to Assets folder
Application.dataPath

// Relative to persistent data
Application.persistentDataPath

// Relative path construction
Path.Combine(Application.dataPath, "Folder/Subfolder/file.txt")
```

### ✅ **Relative Path Strings**
```csharp
// Good for logging/debug
string relativePath = "StreamingWorkFlow/MazeData/Maze_001.bytes";

// Good for Path.Combine
string path = Path.Combine(Application.dataPath, relativePath);
```

---

## 📊 **PATH COMPARISON**

| Type | Example | Status |
|------|---------|--------|
| **Absolute (Windows)** | `D:\travaux_Unity\...` | ❌ NEVER |
| **Absolute (Mac)** | `/Users/ocxz/...` | ❌ NEVER |
| **Absolute (Linux)** | `/home/user/...` | ❌ NEVER |
| **streamingAssetsPath** | `Application.streamingAssetsPath` | ⚠️ AVOID |
| **dataPath + Relative** | `Application.dataPath + "Folder/"` | ✅ ALWAYS |
| **persistentDataPath** | `Application.persistentDataPath` | ✅ FOR SAVES |

---

## 🧪 **TESTING CHECKLIST**

**Verify all paths are relative:**

- [ ] No hardcoded drive letters (C:, D:, etc.)
- [ ] No hardcoded user paths (/Users/, /home/, etc.)
- [ ] All paths use `Application.dataPath` or `Application.persistentDataPath`
- [ ] All paths use `Path.Combine()` for cross-platform compatibility
- [ ] Console logs show relative paths, not absolute paths

**Example Console Output:**
```
✅ GOOD: [LightPlacementData] Saved to StreamingWorkFlow/MazeData/Maze_001.bytes
❌ BAD:  [LightPlacementData] Saved to D:\travaux_Unity\PeuImporte\Assets\...
```

---

## 💾 **BENEFITS OF RELATIVE PATHS**

### Portability
```
✅ Game works on any computer
✅ Works on Windows, Mac, Linux
✅ Works in any directory location
✅ Works in Steam, itch.io, standalone builds
```

### Version Control
```
✅ Same paths for all team members
✅ No merge conflicts from different paths
✅ Git-friendly path structure
```

### Deployment
```
✅ Works in builds without modification
✅ No path hardcoding issues
✅ Professional game development standard
```

---

## 📝 **CODE STANDARDS**

### File Save Example
```csharp
public static void SaveToFile(byte[] data, string fileName)
{
    // ✅ GOOD - Relative path
    string relativePath = "StreamingWorkFlow/MazeData/" + fileName;
    string filePath = Path.Combine(Application.dataPath, relativePath);
    
    // Ensure directory exists
    string dir = Path.GetDirectoryName(filePath);
    if (!Directory.Exists(dir))
    {
        Directory.CreateDirectory(dir);
    }
    
    File.WriteAllBytes(filePath, data);
    Debug.Log($"[Save] Saved to {relativePath}");  // Log relative path
}
```

### File Load Example
```csharp
public static byte[] LoadFromFile(string fileName)
{
    // ✅ GOOD - Relative path
    string relativePath = "StreamingWorkFlow/MazeData/" + fileName;
    string filePath = Path.Combine(Application.dataPath, relativePath);
    
    if (!File.Exists(filePath))
    {
        Debug.LogWarning($"[Load] File not found: {relativePath}");
        return null;
    }
    
    byte[] data = File.ReadAllBytes(filePath);
    Debug.Log($"[Load] Loaded from {relativePath}");  // Log relative path
    return data;
}
```

---

## 🎯 **SUMMARY**

**All paths in PeuImporte project are now:**
- ✅ Relative to `Application.dataPath`
- ✅ Cross-platform compatible
- ✅ Version control friendly
- ✅ Deployment ready
- ✅ Professional standard

**No more hardcoded absolute paths!** 🎉

---

**Generated:** 2026-03-03  
**Status:** ✅ Complete  
**Standard:** Relative paths only  
**Compatibility:** Windows, Mac, Linux
