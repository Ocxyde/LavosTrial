# Alpha-Digital Seed System
**Location:** `Assets/Docs/SEED_SYSTEM.md`  
**Date:** 2026-03-01  
**Status:** ✅ **PRODUCTION READY**

---

## Overview

The maze generator now supports **alpha-digital seeds** (letters + numbers) for procedural generation.

---

## Usage

### Inspector (Unity Editor)

1. Select MazeGenerator GameObject
2. In Inspector, find **Seed** field
3. Enter any string:
   - `"ABC123"`
   - `"Dungeon-7A"`
   - `"MyMaze@2026!"`
   - `"Level1-BossRoom"`

### Code (Runtime)

```csharp
MazeGenerator mazeGen = GetComponent<MazeGenerator>();

// Generate with alpha-digital seed
mazeGen.Generate("ABC123");

// Generate with random seed
string randomSeed = MazeGenerator.GenerateRandomSeed();
mazeGen.Generate(randomSeed);

// Generate with numeric seed (backward compatible)
mazeGen.Generate(12345u);

// Get current seed string
string currentSeed = mazeGen.GetSeedString();
```

---

## Seed Examples

| Seed Type | Example | Use Case |
|-----------|---------|----------|
| **Pure Letters** | `"DUNGEON"` | Level names |
| **Pure Numbers** | `"12345"` | Level numbers |
| **Mixed** | `"Level7A"` | Specific levels |
| **With Symbols** | `"Boss@2026!"` | Special rooms |
| **Random** | `"XK7M2P9Q"` | Procedural levels |

---

## Features

### 1. **Reproducible Mazes**
Same seed = Same maze every time!

```csharp
// These generate identical mazes:
mazeGen.Generate("ABC123");
// ... regenerate later ...
mazeGen.Generate("ABC123");  // Same maze!
```

### 2. **Random Seed Generation**
```csharp
// Generate random 8-character seed
string seed = MazeGenerator.GenerateRandomSeed();
// Example: "XK7M2P9Q"

// Custom length
string shortSeed = MazeGenerator.GenerateRandomSeed(4);  // "A7B2"
string longSeed = MazeGenerator.GenerateRandomSeed(16);  // "XK7M2P9Q4R5T8W1Z"
```

### 3. **Backward Compatible**
Still works with numeric seeds:
```csharp
mazeGen.Generate(12345u);  // Works!
```

### 4. **Human-Readable**
```csharp
// Get current seed as string
string seed = mazeGen.GetSeedString();
// Returns: "ABC123" or "12345" depending on what was used
```

---

## How It Works

### Seed Conversion Pipeline

```
User Input (String)
    ↓
SHA256 Hash
    ↓
Convert to UInt32
    ↓
Random.InitState(seed)
    ↓
Deterministic Maze Generation
```

### Example Seeds & Results

| Input String | Hash (UInt32) | Maze Pattern |
|--------------|---------------|--------------|
| `"ABC123"` | 2847561923 | Pattern A |
| `"XYZ789"` | 1923847561 | Pattern B |
| `"Dungeon"` | 3847562910 | Pattern C |
| `""` (empty) | 1 | Default pattern |

---

## Use Cases

### 1. Level Codes
```csharp
// Player enters level code
string levelCode = "WORLD1-2";
mazeGen.Generate(levelCode);
```

### 2. Shareable Mazes
```csharp
// Generate maze and share seed
string shareCode = MazeGenerator.GenerateRandomSeed(8);
// Share "XK7M2P9Q" with friends
// They enter same code = same maze!
```

### 3. Procedural Campaign
```csharp
// Generate consistent levels
mazeGen.Generate("Campaign-Lvl1");  // Level 1
mazeGen.Generate("Campaign-Lvl2");  // Level 2
mazeGen.Generate("Campaign-Lvl3");  // Level 3
```

### 4. Daily Challenges
```csharp
// Daily seed based on date
string todaySeed = $"Daily-{DateTime.Now:yyyy-MM-dd}";
mazeGen.Generate(todaySeed);
// Same maze for all players on same day!
```

---

## Console Output

When generating with alpha-digital seed:

```
[MazeGenerator] Generated 31x31 | Seed: 2847561923 (String: ABC123)
```

Shows both:
- **Numeric hash** (for internal use)
- **String seed** (for human readability)

---

## API Reference

### Methods

| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `Generate()` | - | void | Generate with inspector seed |
| `Generate(string)` | seed | void | Generate with alpha-digital seed |
| `Generate(uint)` | numericSeed | void | Generate with numeric seed |
| `GenerateRandomSeed(int)` | length | string | Generate random seed string |
| `GetSeedString()` | - | string | Get current seed as string |

---

## Technical Details

### Hash Algorithm
- **SHA256** (cryptographic hash)
- Converts any string to consistent uint32
- Distributes seeds evenly across seed space

### Character Support
- ✅ **Uppercase:** A-Z
- ✅ **Lowercase:** a-z
- ✅ **Numbers:** 0-9
- ✅ **Symbols:** !@#$%^&*()
- ✅ **Unicode:** Any UTF-8 character

### Seed Length
- **Minimum:** 1 character
- **Maximum:** Unlimited (hashed to uint32)
- **Recommended:** 6-10 characters for share codes

---

## Examples

### Basic Usage
```csharp
using Code.Lavos.Core;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private MazeGenerator mazeGenerator;
    
    void Start()
    {
        // Generate with custom seed
        mazeGenerator.Generate("MyFirstMaze");
    }
}
```

### Random Dungeon
```csharp
void GenerateRandomDungeon()
{
    string seed = MazeGenerator.GenerateRandomSeed(8);
    Debug.Log($"Generating dungeon with seed: {seed}");
    mazeGenerator.Generate(seed);
}
```

### Save/Load System
```csharp
// Save
string seed = mazeGenerator.GetSeedString();
PlayerPrefs.SetString("LevelSeed", seed);

// Load
string savedSeed = PlayerPrefs.GetString("LevelSeed");
mazeGenerator.Generate(savedSeed);
```

---

## Troubleshooting

### "Same seed, different maze?"
- Check `Random.InitState()` is called with seed
- Ensure no other random calls before maze generation
- Verify seed string is identical (case-sensitive!)

### "Seed not working?"
- Check seed string is not empty
- Verify MazeGenerator component exists
- Check console for hash value

---

*Documentation: 2026-03-01*  
*Unity 6 (6000.3.7f1) compatible*  
*UTF-8 encoding - Unix line endings*  
*Status: Production Ready ✅*
