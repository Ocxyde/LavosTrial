# Verbosity System Guide

**Date:** 2026-03-06
**Unity Version:** 6000.3.7f1

---

## 📢 **CONSOLE VERBOSITY CONTROL**

The maze system has **3 verbosity levels** to control console output:

| Level | Output | Use Case |
|-------|--------|----------|
| **Mute** | No console output | Production, performance testing |
| **Short** | Critical only (errors, warnings, key milestones) | **DEFAULT** - Regular gameplay |
| **Full** | All debug messages | Development, testing, debugging |

---

## 🎮 **HOW TO CHANGE VERBOSITY**

### **Method 1: JSON Config File (RECOMMENDED)**

**Edit `Config/GameConfig-default.json`:**

```json
{
    ...
    "consoleVerbosity": "short"
}
```

**Valid values:**
- `"short"` - Default (recommended for most users)
- `"full"` - All debug output (for testing)
- `"mute"` - No output (for production)

**Benefits:**
- ✅ No hardcoded values in code
- ✅ Easy to mod without opening Unity
- ✅ Persists across game sessions
- ✅ Can be changed by players/modders

---

### **Method 2: Unity Inspector (Override JSON)**

```
1. Select CompleteMazeBuilder GameObject in Hierarchy
2. Find "Console Verbosity (Override JSON config)" field
3. Choose from dropdown:
   - Mute
   - Short
   - Full
```

**Note:** Inspector setting overrides JSON config if not set to Full (default).

---

### **Method 3: Runtime Console Command**

```
1. Press ~ (tilde) to open Unity console
2. Type one of:
   - maze.verbosity full
   - maze.verbosity short
   - maze.verbosity mute
3. Press Enter
```

**Available Commands:**
```
maze.verbosity full   → Show all debug messages
maze.verbosity short  → Show only critical messages
maze.verbosity mute   → No console output
maze.generate         → Generate new maze
maze.status           → Show current status
maze.help             → Show all commands
```

---

### **Method 4: Code (For Developers)**

```csharp
// In any script:
CompleteMazeBuilder.SetVerbosity("full");   // All debug
CompleteMazeBuilder.SetVerbosity("short");  // Critical only
CompleteMazeBuilder.SetVerbosity("mute");   // No output

// Check current verbosity:
var current = CompleteMazeBuilder.CurrentVerbosity;
Debug.Log($"Current: {current}");
```

---

## 📝 **LOGGING METHODS**

Use these methods in your code for consistent logging:

```csharp
// Regular log (respects verbosity)
CompleteMazeBuilder.Log("This is a debug message");
CompleteMazeBuilder.Log("Critical milestone", isCritical: true);

// Warning (always shown unless Mute)
CompleteMazeBuilder.LogWarning("This is a warning");

// Error (always shown)
CompleteMazeBuilder.LogError("This is an error");
```

**Behavior by Verbosity:**

| Method | Mute | Short | Full |
|--------|------|-------|------|
| `Log(msg)` | ❌ | ❌ | ✅ |
| `Log(msg, true)` | ❌ | ✅ | ✅ |
| `LogWarning(msg)` | ❌ | ✅ | ✅ |
| `LogError(msg)` | ✅ | ✅ | ✅ |

---

## 🎯 **RECOMMENDED SETTINGS**

### **Default Config (Recommended for Players):**
```json
"consoleVerbosity": "short"
```
See only critical issues, less noise.

### **During Development:**
```json
"consoleVerbosity": "full"
```
See all debug messages to understand what's happening.

### **During Production/Playtesting:**
```json
"consoleVerbosity": "mute"
```
No console spam, better performance.

### **When Debugging Issues:**
```json
"consoleVerbosity": "full"
```
See everything to diagnose problems.

---

## 📊 **EXAMPLE OUTPUT**

### **Full Verbosity (Testing):**
```
[CompleteMazeBuilder] ════════════════════════════════════════
[CompleteMazeBuilder] 🏗️ Starting maze generation (FRESH START)...
[CompleteMazeBuilder] ════════════════════════════════════════
[CompleteMazeBuilder] 🧹 Cleaning up ALL scene objects (fresh start)...
[CompleteMazeBuilder] 🗑️ Removed: Ceiling
[CompleteMazeBuilder] 🗑️ Removed: GroundFloor
[CompleteMazeBuilder] ✅ Cleanup complete - SCENE IS NOW EMPTY
[CompleteMazeBuilder] 🌍 Step 1: Ground spawned (base layer)
[CompleteMazeBuilder] 🏛️ Step 2: Rooms placed in virtual grid at (2, 2)
[CompleteMazeBuilder] 🔨 Step 3: Corridors carved in grid
[CompleteMazeBuilder] 🧱 Step 4: Walls spawned from grid (rooms/corridors are CLEAR)
[CompleteMazeBuilder] 🚪 Step 5: Doors placed
[CompleteMazeBuilder] 🎒 Step 6: Objects placed (torches, chests, enemies, items)
[CompleteMazeBuilder] 💾 Step 7: Grid saved to database
[CompleteMazeBuilder] ☁️ Ceiling SKIPPED (testing mode - top-down view)
[CompleteMazeBuilder] ════════════════════════════════════════
[CompleteMazeBuilder] ✅ Maze geometry complete!
[CompleteMazeBuilder] 📏 Dimensions: 11x11 cells (66m x 66m)
[CompleteMazeBuilder] 🏛️ Rooms: Placed FIRST, corridors carved around
[CompleteMazeBuilder] 👤 Player spawn: (15, 0.9, 15)
[CompleteMazeBuilder] ════════════════════════════════════════
```

### **Short Verbosity (DEFAULT - Regular Play):**
```
[CompleteMazeBuilder] 🏗️ Starting maze generation (FRESH START)...
[CompleteMazeBuilder] 🌍 Step 1: Ground spawned (base layer)
[CompleteMazeBuilder] 🏛️ Step 2: Rooms placed in virtual grid at (2, 2)
[CompleteMazeBuilder] ✅ Maze geometry complete!
[CompleteMazeBuilder] 👤 Player spawn: (15, 0.9, 15)
```

### **Mute Verbosity (Production):**
```
(no output)
```

---

## 🔧 **TECHNICAL DETAILS**

### **JSON Config Structure:**

**File:** `Config/GameConfig-default.json`

```json
{
    ...
    "consoleVerbosity": "short"
}
```

**Field:** `consoleVerbosity`
- Type: `string`
- Valid values: `"short"`, `"full"`, `"mute"`
- Default: `"short"`

### **C# Implementation:**

**Enum:**
```csharp
public enum VerbosityLevel
{
    Mute,    // No output
    Short,   // Critical only
    Full     // All debug
}
```

**Loading from JSON:**
```csharp
private void ApplyVerbosityFromConfig()
{
    var config = GameConfig.Instance;
    
    switch (config.consoleVerbosity.ToLower())
    {
        case "mute":
            verbosity = VerbosityLevel.Mute;
            break;
        case "short":
            verbosity = VerbosityLevel.Short;
            break;
        case "full":
            verbosity = VerbosityLevel.Full;
            break;
    }
}
```

**Static Instance:**
```csharp
private static CompleteMazeBuilder _instance;
public static VerbosityLevel CurrentVerbosity => 
    _instance != null ? _instance.verbosity : VerbosityLevel.Short;
```

---

## 📋 **FILES MODIFIED/CREATED**

| File | Status | Purpose |
|------|--------|---------|
| `Config/GameConfig-default.json` | ✅ Modified | Added `consoleVerbosity` field |
| `GameConfig.cs` | ✅ Modified | Added `consoleVerbosity` property |
| `CompleteMazeBuilder.cs` | ✅ Modified | Added verbosity system |
| `MazeConsoleCommands.cs` | ✅ Created | Console commands |
| `VERBOSITY_GUIDE.md` | ✅ Created | This guide |

---

## 🎮 **QUICK REFERENCE**

### **Change Verbosity (JSON - Recommended):**
```json
"consoleVerbosity": "short"
```

### **Change Verbosity (Inspector):**
```
Select CompleteMazeBuilder → Verbosity dropdown
```

### **Change Verbosity (Console):**
```
Press ~ → maze.verbosity short
```

### **Change Verbosity (Code):**
```csharp
CompleteMazeBuilder.SetVerbosity("short");
```

### **Check Current:**
```csharp
var current = CompleteMazeBuilder.CurrentVerbosity;
```

---

## ✅ **BEST PRACTICES**

### **For Players:**
- Leave default at `"short"` (clean console)
- Change to `"full"` only when debugging issues
- Use `"mute"` for performance testing

### **For Modders:**
- Edit `Config/GameConfig-default.json`
- No need to recompile code
- Changes apply immediately on restart

### **For Developers:**
- Use `CompleteMazeBuilder.Log()` instead of `Debug.Log()`
- Mark critical messages with `isCritical: true`
- Respect verbosity in custom scripts

---

**Generated:** 2026-03-06
**Unity Version:** 6000.3.7f1
**Default Verbosity:** ✅ **SHORT** (clean console)
**Config File:** ✅ **JSON** (no hardcoded values)

---

*Clean console, happy debugging!* 🎮✨
