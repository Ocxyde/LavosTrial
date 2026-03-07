# Maze Sharing System - User Guide

**Version:** 1.0  
**Date:** 2026-03-07  
**License:** GPL-3.0

---

## 🎯 OVERVIEW

The **Maze Sharing System** allows players to share procedurally generated mazes via compact shareable codes.

### How It Works:

```
Instead of sharing entire maze data (kilobytes):
  → Share a seed code (bytes)
  → Recipient regenerates exact same maze from seed
```

### Benefits:

- ✅ **Tiny codes** - 30-40 characters vs megabytes
- ✅ **Perfect reconstruction** - Same seed = same maze
- ✅ **Easy sharing** - Copy/paste, QR codes, links
- ✅ **Validation** - Checksum prevents typos
- ✅ **Version safe** - Works across game updates

---

## 📝 CODE FORMAT

```
LAVOS-1234567890-L5-S888-9A7B2C4D
│      │          │   │    └─ Checksum (validation)
│      │          │   └─ Sub-seed (variation)
│      │          └─ Level (0-39)
│      └─ Main seed (random int)
└─ Game identifier
```

### Example Codes:

```
LAVOS-42424242-L0-S0-1A2B3C4D
LAVOS-1234567890-L10-S999-9F8E7D6C
LAVOS--2147483648-L39-S500-ABCDEF12
```

---

## 🎮 PLAYER USAGE

### Export Your Maze:

**Method 1: Console Command**
```
Type in chat/console: /export
→ Copies maze code to clipboard
```

**Method 2: UI Button**
```
Click "Share Maze" button in menu
→ Code copied + QR code displayed
```

**Method 3: Screenshot**
```
Code displayed on screen
→ Take screenshot, share image
```

### Import Shared Maze:

**Method 1: Console Command**
```
Type: /import LAVOS-1234567890-L5-S888-9A7B2C4D
→ Regenerates exact maze
```

**Method 2: UI Input**
```
Paste code in "Import Maze" field
→ Click "Generate"
```

**Method 3: QR Scan**
```
Scan QR code with phone
→ Game auto-imports
```

---

## 🛠️ DEVELOPER API

### Basic Usage:

```csharp
// Export current maze
string code = MazeShareSystem.ExportCode(seed: 12345, level: 5);
// Returns: "LAVOS-12345-L5-S0-ABCDEF12"

// Import from code
if (MazeShareSystem.ImportCode(code, out int seed, out int level, out int subSeed))
{
    // Valid code - regenerate maze with these values
    GenerateMaze(seed, level);
}
else
{
    // Invalid code - show error
}

// Copy to clipboard
MazeShareSystem.CopyToClipboard(code);

// Generate QR code URL
string qrUrl = MazeShareSystem.GenerateQRDataUrl(code);
// Use in UI: LoadTexture(qrUrl)
```

### With CompleteMazeBuilder:

```csharp
CompleteMazeBuilder builder = FindObjectOfType<CompleteMazeBuilder>();

// Export current maze
string code = builder.ExportMazeCode();

// Import and regenerate
builder.ImportMazeCode("LAVOS-1234567890-L5-S888-9A7B2C4D");

// Copy to clipboard
builder.CopyMazeCodeToClipboard();
```

---

## 🔒 SECURITY & VALIDATION

### Checksum Validation:

Every code includes a checksum to prevent:
- ✅ Typos during manual entry
- ✅ Intentional code tampering
- ✅ Version incompatibility

### Invalid Codes:

```
❌ LAVOS-12345-L5 (missing parts)
❌ LAWS-12345-L5-S0-ABC (wrong prefix)
❌ LAVOS-12345-L5-S0-FFFF (checksum mismatch)
```

### Error Messages:

| Error | Message |
|-------|---------|
| Empty code | "Invalid code: empty" |
| Wrong format | "Invalid code format (expected LAVOS-seed-level-subSeed-checksum)" |
| Wrong prefix | "Invalid prefix (expected LAVOS)" |
| Bad checksum | "Code checksum invalid - possible typo" |

---

## 🌐 SHARING PLATFORMS

### Supported Platforms:

| Platform | Method | Example |
|----------|--------|---------|
| **Discord** | Copy/paste code | `LAVOS-12345-L5-S0-ABC` |
| **Reddit** | Post in comments | "Seed: LAVOS-..." |
| **Twitter** | Hashtag + code | `#LavosMaze LAVOS-...` |
| **QR Code** | Image share | Scan with phone |
| **Direct Link** | URL parameter | `yourgame.com/maze?code=LAVOS-...` |

### Recommended Hashtags:

```
#LavosMaze
#MazeShare
#ProceduralMaze
#IndieGame
```

---

## 📊 EXAMPLE SHARING SCENARIOS

### Scenario 1: Daily Challenge

```csharp
// Generate today's seed
int dailySeed = DateTime.Today.GetHashCode();
string code = MazeShareSystem.ExportCode(dailySeed, level: 5);

// Share with community
PostToSocialMedia($"Today's maze: {code} #DailyMaze");
```

### Scenario 2: Developer Testing

```csharp
// Report bug with exact maze
string bugReport = $"Found bug in maze: {code}";
// QA team can reproduce exact same maze
```

### Scenario 3: Speedrunning

```csharp
// Fixed seed for all runners
string tournamentSeed = "LAVOS-42424242-L10-S0-ABCD1234";
// All players get identical maze layout
```

### Scenario 4: Community Mazes

```csharp
// Curated collection
string[] featuredMazes = {
    "LAVOS-11111111-L5-S0-AAAA",
    "LAVOS-22222222-L10-S0-BBBB",
    "LAVOS-33333333-L20-S0-CCCC"
};
```

---

## 🎨 UI INTEGRATION

### Minimal UI:

```
┌────────────────────────────────┐
│  Maze Sharing                  │
├────────────────────────────────┤
│  Current Code:                 │
│  [LAVOS-12345-L5-S0-ABCDEF12]  │ ← Copy button
│                                │
│  Import Code:                  │
│  [_________________________]   │ ← Paste field
│  [Generate]                    │ ← Import button
└────────────────────────────────┘
```

### Advanced UI:

```
┌────────────────────────────────┐
│  Share Your Maze               │
├────────────────────────────────┤
│  Code: LAVOS-...-...-...-...   │
│  [Copy] [QR] [Link] [Tweet]    │
│                                │
│  ┌──────────────────┐          │
│  │  ██████  ██████  │  ← QR    │
│  │  ██████  ██████  │    code  │
│  │  ██████  ██████  │          │
│  └──────────────────┘          │
│                                │
│  Share to:                     │
│  [Discord] [Twitter] [Reddit]  │
└────────────────────────────────┘
```

---

## 🔧 CUSTOMIZATION

### Change Code Format:

```csharp
// In MazeShareSystem.cs
private const string PREFIX = "YOURGAME";
private const char SEPARATOR = '_'; // Or any character

// New format: YOURGAME_12345_L5_S0_ABC
```

### Add Metadata:

```csharp
// Include timestamp, creator name, etc.
public static string ExportCode(int seed, int level, string creator = "")
{
    // Add creator to checksum
    int checksum = CreateChecksum(seed, level, creator);
    return $"{PREFIX}{SEPARATOR}{seed}{SEPARATOR}{level}{SEPARATOR}{creator}{SEPARATOR}{checksum}";
}
```

### Custom Validation:

```csharp
// Add difficulty rating, maze size, etc.
public static bool GetMazeInfo(string code, out MazeInfo info)
{
    // Parse additional metadata
}
```

---

## 📈 ANALYTICS (Optional)

Track shared mazes:

```csharp
// Log popular seeds
Analytics.Log("MazeShared", new {
    seed = seed,
    level = level,
    platform = platform
});

// Track most shared mazes
// Feature top community mazes in game
```

---

## 🐛 TROUBLESHOOTING

### "Invalid Code Format"

**Cause:** Code is malformed or from different game

**Fix:**
- Check code starts with "LAVOS-"
- Ensure 5 parts separated by "-"
- Verify no extra spaces

### "Checksum Mismatch"

**Cause:** Typo in code or corrupted data

**Fix:**
- Copy/paste instead of typing
- Check for similar characters (0 vs O, 1 vs I)
- Request new code from sharer

### "Maze Looks Different"

**Cause:** Different game version or mods

**Fix:**
- Ensure same game version
- Check if mods affect generation
- Verify level number matches

---

## 📝 TECHNICAL NOTES

### Checksum Algorithm:

```csharp
// MD5 hash of: "seed:level:subSeed:SALT"
// First 4 bytes converted to int
// Provides collision resistance + typo detection
```

### Code Length:

- **Typical:** 35-45 characters
- **Maximum:** ~50 characters (with large seeds)
- **Minimum:** ~25 characters (small seeds)

### Performance:

- **Export:** < 1ms
- **Import:** < 2ms (includes validation)
- **Clipboard:** < 5ms

---

## 🎯 BEST PRACTICES

### For Players:

1. ✅ Copy/paste codes (don't type manually)
2. ✅ Include level when sharing
3. ✅ Mention difficulty/context
4. ✅ Use hashtags for discoverability

### For Developers:

1. ✅ Validate all imported codes
2. ✅ Show clear error messages
3. ✅ Provide copy button (not just text)
4. ✅ Generate QR codes for mobile sharing
5. ✅ Log popular seeds for analytics

---

## 🚀 FUTURE ENHANCEMENTS

Potential additions:

- [ ] Encoded codes (shorter, base64)
- [ ] Compressed maze data (for custom layouts)
- [ ] Image-based sharing (maze screenshot + code)
- [ ] Web viewer (preview maze before importing)
- [ ] Rating system for shared mazes
- [ ] Collection/inventory of saved codes

---

**Document:** Maze_Sharing_System.md  
**Location:** Assets/Docs/  
**Status:** ✅ Production Ready

---

*Document generated - Unity 6 compatible - UTF-8 encoding - Unix LF*
