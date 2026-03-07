# SQLite Save System - Complete Implementation

**Date:** 2026-03-05
**Status:** ✅ PRODUCTION READY
**Database:** SQLite (Saves/MazeDB.sqlite)
**NO JSON - Pure SQL!**

---

## 📁 **FOLDER STRUCTURE**

```
Project Root/
├── Saves/                  ← NEW: Database folder
│   └── MazeDB.sqlite       ← SQLite database file
├── Assets/
│   └── Scripts/
│       └── Core/
│           └── 06_Maze/
│               ├── CompleteMazeBuilder.cs
│               └── MazeSaveData.cs          ← NEW: SQLite handler
```

---

## 💾 **DATABASE SCHEMA (SQLite)**

### **Table: MazeData**
```sql
CREATE TABLE MazeData (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    seed INTEGER NOT NULL,
    spawnX INTEGER NOT NULL,
    spawnZ INTEGER NOT NULL,
    mazeWidth INTEGER NOT NULL,
    mazeHeight INTEGER NOT NULL,
    timestamp TEXT NOT NULL
);
```

### **Table: RoomData**
```sql
CREATE TABLE RoomData (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    seed INTEGER NOT NULL,
    roomX INTEGER NOT NULL,
    roomZ INTEGER NOT NULL,
    roomType TEXT NOT NULL,  -- "Entrance", "Exit", "Normal"
    timestamp TEXT NOT NULL
);
```

### **Table: PrefabData**
```sql
CREATE TABLE PrefabData (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    prefabName TEXT NOT NULL,
    prefabPath TEXT NOT NULL,
    timestamp TEXT NOT NULL
);
```

---

## 🔧 **USAGE**

### **Save Maze Data:**
```csharp
// Save to SQLite database
MazeSaveData.SaveMazeData(seed, spawnX, spawnZ, mazeWidth, mazeHeight);

// Save room positions
var rooms = new List<RoomDataModel>
{
    new RoomDataModel { X = 3, Z = 5, Type = "Entrance" }
};
MazeSaveData.SaveRoomData(seed, rooms);

// Save prefab assignments
var prefabs = new Dictionary<string, string>
{
    { "Wall", "Prefabs/WallPrefab.prefab" },
    { "Door", "Prefabs/DoorPrefab.prefab" }
};
MazeSaveData.SaveAllPrefabData(prefabs);
```

### **Load Maze Data:**
```csharp
// Load from SQLite database
var mazeData = MazeSaveData.LoadMazeData();

if (mazeData != null)
{
    int seed = mazeData.Seed;
    int spawnX = mazeData.SpawnX;
    int spawnZ = mazeData.SpawnZ;
}

// Load room positions
List<RoomDataModel> rooms = MazeSaveData.LoadRoomData(seed);

// Load prefab paths
Dictionary<string, string> prefabs = MazeSaveData.LoadAllPrefabData();
```

---

## ✅ **FEATURES**

| Feature | Status |
|---------|--------|
| **Location** | ✅ Saves/ at project root (NOT in Assets/) |
| **Database** | ✅ SQLite (MazeDB.sqlite) |
| **NO JSON** | ✅ Pure SQL database |
| **Seed Storage** | ✅ Stored with maze data |
| **Prefab Data** | ✅ All prefab paths saved |
| **Room Data** | ✅ Room positions + types |
| **Timestamps** | ✅ Auto-generated on save |
| **Plug-in-Out** | ✅ Independent module |

---

## 📊 **DATA MODELS**

### **MazeDataModel**
```csharp
public class MazeDataModel
{
    public int Seed { get; set; }        // Maze generation seed
    public int SpawnX { get; set; }      // Player spawn X cell
    public int SpawnZ { get; set; }      // Player spawn Z cell
    public int MazeWidth { get; set; }   // Maze width in cells
    public int MazeHeight { get; set; }  // Maze height in cells
    public string Timestamp { get; set; } // ISO 8601 timestamp
}
```

### **RoomDataModel**
```csharp
public class RoomDataModel
{
    public int X { get; set; }     // Room X cell position
    public int Z { get; set; }     // Room Z cell position
    public string Type { get; set; } // "Entrance", "Exit", "Normal"
}
```

---

## 🎮 **INTEGRATION WITH CompleteMazeBuilder**

### **Automatic Save:**
```csharp
// Called after maze generation
SaveSpawnPosition(entranceRoomCell.x, entranceRoomCell.y, (int)currentSeed);

// Saves to SQLite:
// - Maze data (seed, spawn, dimensions)
// - Room data (entrance room position)
// - Prefab data (all prefab paths)
```

### **Automatic Load:**
```csharp
// Called when spawning player
Vector2Int spawnCell = LoadSpawnPosition();

// Loads from SQLite:
// - Checks if seed matches
// - Returns spawn position if valid
// - Returns (-1, -1) if regeneration needed
```

---

## 🗄️ **DATABASE OPERATIONS**

### **Initialize:**
```csharp
MazeSaveData.Initialize();
// Creates Saves/ folder
// Creates MazeDB.sqlite
// Creates tables if not exist
```

### **Save:**
```csharp
MazeSaveData.SaveMazeData(seed, spawnX, spawnZ, width, height);
// SQL: INSERT OR REPLACE INTO MazeData ...
```

### **Load:**
```csharp
var data = MazeSaveData.LoadMazeData();
// SQL: SELECT * FROM MazeData ORDER BY timestamp DESC LIMIT 1
```

### **Clear:**
```csharp
MazeSaveData.ClearMazeData();
// SQL: DELETE FROM MazeData
```

---

## 📍 **DATABASE LOCATION**

**Path:** `<ProjectRoot>/Saves/MazeDB.sqlite`

**Example:**
```
D:/travaux_Unity/PeuImporte/Saves/MazeDB.sqlite
```

**NOT in Assets!** This ensures:
- ✅ Database persists across Unity restarts
- ✅ Not reimported by Unity
- ✅ Clean separation of data and code
- ✅ Easy to backup/restore

---

## 🔒 **DATA PERSISTENCE**

### **What's Saved:**
- ✅ Maze seed (for reproducibility)
- ✅ Player spawn position (cell coordinates)
- ✅ Room positions and types
- ✅ All prefab assignments
- ✅ Timestamps (auto-generated)

### **What's NOT Saved:**
- ❌ Generated geometry (walls, floors, etc.)
- ❌ Runtime state (player position, health, etc.)
- ❌ Temporary data

---

## 🎯 **PLUG-IN-OUT COMPLIANCE**

```csharp
// ✅ Independent module
// ✅ No dependencies on other systems
// ✅ Can be used standalone
// ✅ Clean API interface

// Usage example:
MazeSaveData.Initialize();
MazeSaveData.SaveMazeData(seed, x, z, width, height);
var data = MazeSaveData.LoadMazeData();
```

---

## ✅ **VERIFICATION**

### **Check Database Exists:**
```csharp
if (MazeSaveData.DatabaseExists())
{
    Debug.Log("✅ Database exists!");
}
```

### **Get Database Path:**
```csharp
string path = MazeSaveData.GetDatabasePath();
Debug.Log($"Database at: {path}");
// Output: Database at: D:/travaux_Unity/PeuImporte/Saves/MazeDB.sqlite
```

---

## 🎉 **FINAL RESULT**

**MazeSaveData is now:**
- ✅ **SQLite database** (Saves/MazeDB.sqlite)
- ✅ **Project root storage** (NOT in Assets/)
- ✅ **NO JSON** (Pure SQL!)
- ✅ **Seed-based** (procedural + persistent)
- ✅ **Prefab storage** (all assignments saved)
- ✅ **Room data** (positions + types)
- ✅ **Plug-in-out** (independent module)

---

**Generated:** 2026-03-05
**Unity Version:** 6000.3.7f1
**Database:** SQLite (Saves/MazeDB.sqlite)
**Status:** ✅ PRODUCTION READY

---

*Document generated - Unity 6 compatible - UTF-8 encoding - Unix LF*
