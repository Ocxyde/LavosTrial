# Compute Grid Integration - Architecture Report
**Date:** 2026-03-06
**Unity Version:** 6000.3.7f1
**Status:** ✅ Complete

---

## 🎯 **QUESTION: How is ComputeGridData called?**

**Answer:** Via **EventHandler** - the central communication hub.

### **Architecture Pattern:**

```
CompleteMazeBuilder → EventHandler → ComputeGridEngine → ComputeGridData
     (Publisher)       (Central Hub)   (Subscriber)      (Static API)
```

---

## 📊 **COMMUNICATION FLOW**

### **Step 1: CompleteMazeBuilder Publishes Event**

```csharp
// In CompleteMazeBuilder.PlaceWalls()
byte[] gridBytes = BuildGridBytesForComputeGrid();

// Publish via EventHandler (NOT direct call!)
EventHandler.Instance.InvokeComputeGridSaveRequested(
    mazeId,      // "Maze_000"
    gridBytes,   // byte array (1 byte per cell)
    seed         // encryption seed
);
```

### **Step 2: ComputeGridEngine Receives Event**

```csharp
// In ComputeGridEngine.Awake() - subscription
EventHandler.Instance.OnComputeGridSaveRequested += HandleSaveRequested;

// Event handler method
private void HandleSaveRequested(string mazeId, byte[] gridData, int seed)
{
    Debug.Log($"[ComputeGridEngine] Save requested via event: {mazeId}");
    
    // Update internal state
    SetMazeId(mazeId);
    SetMazeSeed(seed);
    SetGridBytes(gridData);
    
    // Save to binary
    SaveToBinary();
}
```

### **Step 3: ComputeGridEngine Calls ComputeGridData (Static API)**

```csharp
// In ComputeGridEngine.SaveToBinary()
public bool SaveToBinary()
{
    // ComputeGridData is a STATIC API class
    // Like a data access layer - no events needed here
    bool saved = ComputeGridData.SaveGrid(mazeId, GetGridBytes(), mazeSeed);
    return saved;
}
```

---

## 🔌 **WHY THIS ARCHITECTURE?**

### **EventHandler as Central Hub:**

Your project uses a **plug-in-out architecture** where:

1. **Systems don't call each other directly** (no coupling)
2. **All communication via EventHandler** (loose coupling)
3. **Systems subscribe to events** (find, don't create)

### **Example from Your Codebase:**

**ProceduralCompute** (similar pattern):

```csharp
// 1. Subscribe to event
EventHandler.Instance.OnMaterialRequested += OnMaterialRequested;

// 2. Handle event
private void OnMaterialRequested(MaterialType type, TextureType textureType)
{
    Material mat = GenerateMaterial(type, textureType);
    // Material is cached and available
}
```

**CompleteMazeBuilder** publishes:

```csharp
// Publish event (don't call ProceduralCompute directly!)
EventHandler.Instance.RequestMaterial(MaterialType.Stone, TextureType.Floor);
```

---

## 📁 **COMPUTE GRID COMPONENTS**

### **1. ComputeGridEngine (MonoBehaviour)**

**Role:** Independent process for compute grid management

**Responsibilities:**
- Subscribe to EventHandler events
- Store grid in RAM (byte array)
- Save/load binary files
- Manage grid lifecycle

**Location:** `Assets/Scripts/Core/12_Compute/`

### **2. ComputeGridData (Static Class)**

**Role:** Binary I/O API layer

**Responsibilities:**
- Save binary files to disk
- Load binary files from disk
- Encrypt/decrypt data
- Validate checksums

**Location:** `Assets/Scripts/Core/12_Compute/`

**Why static?** It's a **utility/data access layer**, not a system. Like `File.WriteAllText()`, it doesn't need events.

### **3. EventHandler (Central Hub)**

**Role:** Single point of truth for all game events

**New Events Added:**
```csharp
// Compute Grid Events
public event Action<string, byte[], int> OnComputeGridSaveRequested;
public event Action<string, int> OnComputeGridLoadRequested;
public event Action<byte[], int, int> OnComputeGridCellSet;
public event Action OnComputeGridCleared;
```

**Location:** `Assets/Scripts/Core/01_CoreSystems/`

---

## 🎮 **USAGE EXAMPLE**

### **In Your Scene:**

```
Hierarchy:
├── EventHandler (singleton)
├── ComputeGridEngine (independent process)
├── CompleteMazeBuilder (maze orchestrator)
└── Player (independent)
```

### **At Runtime:**

```
1. CompleteMazeBuilder.GenerateMaze()
   ↓
2. CompleteMazeBuilder.PlaceWalls()
   ↓
3. BuildGridBytesForComputeGrid() → byte[]
   ↓
4. EventHandler.InvokeComputeGridSaveRequested(mazeId, bytes, seed)
   ↓
5. ComputeGridEngine.HandleSaveRequested(...) ← receives event
   ↓
6. ComputeGridEngine.SaveToBinary()
   ↓
7. ComputeGridData.SaveGrid(mazeId, bytes, seed) → disk
   ↓
8. File saved: Assets/StreamingAssets/ComputeGrid/Maze_000.bin
```

---

## ✅ **PLUG-IN-OUT COMPLIANCE**

| Component | Type | Communication | Compliant |
|-----------|------|---------------|-----------|
| **CompleteMazeBuilder** | Publisher | Events via EventHandler | ✅ |
| **ComputeGridEngine** | Subscriber | Receives events | ✅ |
| **ComputeGridData** | Static API | Direct calls (utility) | ✅ |
| **EventHandler** | Central Hub | Event routing | ✅ |

### **Why ComputeGridData is Static:**

- ✅ **Utility class** (like `Mathf`, `File`, `PlayerPrefs`)
- ✅ **No state** (pure functions)
- ✅ **No dependencies** (doesn't call other systems)
- ✅ **API layer** (data access, not business logic)

**Business logic** (ComputeGridEngine) uses events.
**Data access** (ComputeGridData) is a static API.

---

## 📝 **COMPARISON WITH OTHER SYSTEMS**

### **Similar Pattern: ProceduralCompute**

```
ProceduralCompute (like ComputeGridEngine)
├── Subscribes to: OnMaterialRequested, OnTextureRequested
├── Handles events
└── Generates materials/textures

MazeRenderer (like CompleteMazeBuilder)
├── Publishes: EventHandler.RequestMaterial()
└── Receives generated material
```

### **Your New Pattern: ComputeGrid**

```
ComputeGridEngine (like ProceduralCompute)
├── Subscribes to: OnComputeGridSaveRequested, etc.
├── Handles events
└── Saves/loads grid

CompleteMazeBuilder (like MazeRenderer)
├── Publishes: InvokeComputeGridSaveRequested()
└── Grid saved automatically
```

---

## 🔧 **IMPLEMENTATION DETAILS**

### **Event Subscription (ComputeGridEngine.Awake)**

```csharp
void Awake()
{
    // Subscribe to EventHandler events (plug-in-out architecture)
    SubscribeToEvents();
    Initialize();
}

private void SubscribeToEvents()
{
    if (EventHandler.Instance != null)
    {
        EventHandler.Instance.OnComputeGridSaveRequested += HandleSaveRequested;
        EventHandler.Instance.OnComputeGridLoadRequested += HandleLoadRequested;
        EventHandler.Instance.OnComputeGridCellSet += HandleCellSet;
        EventHandler.Instance.OnComputeGridCleared += HandleCleared;
        Debug.Log("[ComputeGridEngine] Subscribed to EventHandler events");
    }
}
```

### **Event Publishing (CompleteMazeBuilder.PlaceWalls)**

```csharp
public void PlaceWalls()
{
    // Build grid bytes
    byte[] gridBytes = BuildGridBytesForComputeGrid();
    
    // Publish event (NOT direct call!)
    if (EventHandler.Instance != null)
    {
        string mazeId = $"Maze_{currentLevel:D3}";
        EventHandler.Instance.InvokeComputeGridSaveRequested(mazeId, gridBytes, (int)seed);
        Log($"[CompleteMazeBuilder] Published compute grid save event: {mazeId}");
    }
    
    // Place wall GameObjects...
}
```

---

## 📊 **MEMORY LAYOUT**

### **RAM Storage (ComputeGridEngine):**

```
Grid: byte[,] _gridRam
Size: gridSize × gridSize bytes

Example (21x21):
- Total: 441 bytes
- Metadata: 5,292 bytes (12 bytes per cell)
- Total RAM: ~5.7 KB
```

### **Binary File (ComputeGridData):**

```
File: Maze_000.bin
Structure:
├─ Magic: 4 bytes (0x434F4D50 = "COMP")
├─ Version: 2 bytes (1)
├─ GridSize: 2 bytes (21)
├─ Seed: 4 bytes (encryption key)
├─ Data: 441 bytes (encrypted)
└─ Checksum: 4 bytes (CRC32)

Total: 457 bytes
```

---

## 🧪 **TESTING**

### **In Unity Editor:**

1. **Add ComputeGridEngine to scene**
   - Create empty GameObject
   - Add component: `ComputeGridEngine`
   - Configure: Grid Size=21, Enable RAM & Binary

2. **Press Play**

3. **Watch Console:**
```
[CompleteMazeBuilder] Generating grid maze...
[CompleteMazeBuilder] Grid bytes built: 441 bytes
[CompleteMazeBuilder] Published compute grid save event: Maze_000
[EventHandler] ComputeGridSaveRequested: Maze_000 (441 bytes)
[ComputeGridEngine] Save requested via event: Maze_000
[ComputeGridEngine] Saving to binary...
[ComputeGridData] Saving grid: Maze_000 (441 bytes)...
[ComputeGridData] Saved: .../StreamingAssets/ComputeGrid/Maze_000.bin (457 bytes)
```

4. **Verify file created:**
   - `Assets/StreamingAssets/ComputeGrid/Maze_000.bin`

---

## ✅ **CHECKLIST**

| Requirement | Status | Notes |
|-------------|--------|-------|
| **Byte-to-byte RAM storage** | ✅ | 1 byte per cell |
| **Binary persistence** | ✅ | Encrypted files |
| **Independent process** | ✅ | ComputeGridEngine MonoBehaviour |
| **Plug-in-out compliant** | ✅ | All via EventHandler |
| **EventHandler integration** | ✅ | Events added |
| **Static API for data** | ✅ | ComputeGridData |
| **Unity 6 compatible** | ✅ | 6000.3.7f1 |
| **UTF-8 encoding** | ✅ | All files |
| **Unix LF line endings** | ✅ | All files |
| **GPL-3.0 license** | ✅ | Headers included |

---

## 🎯 **SUMMARY**

**Q:** How is ComputeGridData called?

**A:** 
1. **CompleteMazeBuilder** publishes event via **EventHandler**
2. **ComputeGridEngine** receives event (subscribed)
3. **ComputeGridEngine** calls **ComputeGridData.SaveGrid()** (static API)

**Architecture:**
```
Publisher → EventHandler → Subscriber → Static API
```

**This matches your existing pattern:**
- ProceduralCompute (subscriber) ← EventHandler ← MazeRenderer (publisher)
- ComputeGridEngine (subscriber) ← EventHandler ← CompleteMazeBuilder (publisher)

---

**Your compute grid system is now fully plug-in-out compliant!** 🫡💾⚡

---

*Document generated - Unity 6 compatible - UTF-8 encoding - Unix LF*
