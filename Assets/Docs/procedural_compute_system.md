# Procedural Compute System - Architecture
**Date:** 2026-03-03  
**Unity Version:** 6000.3.7f1

---

## 🎯 **ARCHITECTURE OVERVIEW**

### **Centralized Procedural Generation:**

```
┌─────────────────────────────────────────────────────────┐
│              ProceduralCompute (Singleton)              │
│  - Central hub for ALL procedural generation            │
│  - Textures, Materials, Meshes, Patterns                │
│  - Seed-based reproducible generation                   │
│  - Caches results for performance                       │
└─────────────────────────────────────────────────────────┘
                        ▲
                        │
        ┌───────────────┴───────────────┐
        │                               │
┌───────▼────────┐            ┌────────▼────────┐
│  EventHandler  │            │  FloorMaterial  │
│  (Hub)         │◄───────────│  Factory        │
│                │  Request   │  (Legacy)       │
└───────┬────────┘            └─────────────────┘
        │
        │ Request
        │
┌───────▼────────┐
│  MazeRenderer  │
│  (Consumer)    │
└────────────────┘
```

---

## 📁 **FILES CREATED**

| File | Purpose | Location |
|------|---------|----------|
| `ProceduralCompute.cs` | Central procedural system | `Assets/Scripts/Core/12_Compute/` |
| `FloorMaterialFactory.cs` | Floor material generator | `Assets/Scripts/Core/09_Art/` |
| `FloorMaterialFactoryMenu.cs` | Editor tools | `Assets/Editor/` |
| `EventHandler.cs` | Updated with material events | `Assets/Scripts/Core/01_CoreSystems/` |
| `MazeRenderer.cs` | Updated to use EventHandler | `Assets/Scripts/Core/06_Maze/` |

---

## 🔌 **PLUG-IN-AND-OUT FLOW**

### **Correct Flow (Now):**

```
MazeRenderer needs floor material
    ↓
Requests via EventHandler
    ↓
EventHandler invokes OnMaterialRequested
    ↓
ProceduralCompute handles request
    ↓
Generates/caches material
    ↓
Returns material to EventHandler
    ↓
EventHandler returns to MazeRenderer
```

### **Code Example:**

```csharp
// MazeRenderer requests material
if (EventHandler.Instance != null)
{
    _floorMat = EventHandler.Instance.RequestMaterial(
        FloorMaterialFactory.FloorType.Stone,
        ProceduralCompute.TextureType.Floor
    );
}
```

---

## 🎨 **PROCEDURAL TYPES**

### **Texture Types:**
```csharp
public enum TextureType
{
    Floor,
    Wall,
    Ceiling,
    Door,
    Torch
}
```

### **Material Types:**
```csharp
public enum MaterialType
{
    Stone,    // Gray stone
    Wood,     // Brown wood
    Tile,     // Ceramic
    Brick,    // Red brick
    Marble,   // White marble
    Metal,    // Metallic
    Magic     // Glowing
}
```

### **Pattern Types:**
```csharp
public enum PatternType
{
    Tiles,   // Square tiles
    Planks,  // Wood planks
    Bricks,  // Brick pattern
    Veins,   // Marble veins
    Noise    // Random noise
}
```

---

## 🛠️ **HOW TO USE**

### **Method 1: Via EventHandler (Recommended)**

```csharp
// Request material
Material floorMat = EventHandler.Instance.RequestMaterial(
    MaterialType.Stone,
    TextureType.Floor
);

// Request texture
Texture2D wallTex = EventHandler.Instance.RequestTexture(
    TextureType.Wall,
    MaterialType.Brick
);
```

### **Method 2: Direct (For Editor Tools)**

```csharp
// Generate material directly
Material mat = ProceduralCompute.Instance.GenerateMaterial(
    MaterialType.Marble,
    TextureType.Floor
);

// Generate texture directly
Texture2D tex = ProceduralCompute.Instance.GenerateTexture(
    TextureType.Ceiling,
    MaterialType.Stone,
    PatternType.Noise
);
```

### **Method 3: Editor Menu**

```
Tools → Floor Materials → Generate All Floor Materials
```

Creates all 5 floor types as saved assets!

---

## 📊 **CACHING SYSTEM**

### **Automatic Caching:**

```csharp
// First call - generates
Texture2D tex1 = ProceduralCompute.Instance.GenerateFloor(MaterialType.Stone);

// Second call - returns cached (instant!)
Texture2D tex2 = ProceduralCompute.Instance.GenerateFloor(MaterialType.Stone);

// Cache key: "Floor_Stone_Tiles_32_{seed}"
```

### **Clear Cache:**

```csharp
ProceduralCompute.Instance.ClearCache();
```

---

## ⚙️ **CONFIGURATION**

### **In MazeRenderer Inspector:**

```yaml
Floor Material:
├─ Floor Type: Stone [Dropdown]
│  ├─ Stone
│  ├─ Wood
│  ├─ Tile
│  ├─ Brick
│  └─ Marble
│
└─ Auto Generate Floor Materials: [ ]
```

---

## 🎯 **BENEFITS**

### **Before (Direct Calls):**
```
❌ MazeRenderer → FloorMaterialFactory (coupled)
❌ Hardcoded generation
❌ No caching
❌ Not reusable
```

### **After (EventHandler):**
```
✅ MazeRenderer → EventHandler → ProceduralCompute (decoupled)
✅ Centralized generation
✅ Automatic caching
✅ Reusable across systems
✅ Plug-in-and-out architecture
```

---

## 🧪 **TESTING**

### **In Unity Editor:**

1. **Generate materials:**
   ```
   Tools → Floor Materials → Generate All
   ```

2. **Check Materials folder:**
   ```
   Assets/Materials/Floor/
   ├── Stone_Floor.mat
   ├── Wood_Floor.mat
   ├── Tile_Floor.mat
   ├── Brick_Floor.mat
   └── Marble_Floor.mat
   ```

3. **Configure MazeRenderer:**
   ```
   Select MazeTest → MazeRenderer
   Floor Type: Stone
   ```

4. **Press Play**

**Console shows:**
```
[ProceduralCompute] Initialized with seed: 12345
[MazeRenderer] Requested floor material via EventHandler: Stone
[EventHandler] MaterialRequested: Stone_Floor
[ProceduralCompute] Generating texture: Floor_Stone_Tiles_32_12345
[ProceduralCompute] Generating material: Mat_Stone_Floor_12345
```

---

## 📝 **FUTURE EXPANSION**

### **Add New Material Type:**

```csharp
// 1. Add to enum
public enum MaterialType
{
    // ... existing types ...
    Crystal,  // NEW!
}

// 2. Add generator in ProceduralCompute
private Texture2D GenerateCrystalFloor(PixelCanvas canvas, int size)
{
    // Crystal generation logic
}

// 3. Add to switch statement
(TextureType.Floor, MaterialType.Crystal) => GenerateCrystalFloor(canvas, size),
```

### **Add New Texture Type:**

```csharp
// 1. Add to enum
public enum TextureType
{
    // ... existing types ...
    Trap,  // NEW!
}

// 2. Add request method
public Texture2D GenerateTrap(MaterialType material)
{
    return GenerateTexture(TextureType.Trap, material, PatternType.Tiles, 32);
}
```

---

## ✅ **ARCHITECTURE COMPLIANCE**

### **Plug-in-and-Out Rules:**

✅ **Systems don't call each other directly**  
✅ **All requests go through EventHandler**  
✅ **ProceduralCompute is central generator**  
✅ **Materials saved as assets for reuse**  
✅ **Configurable via Inspector**  
✅ **Event-driven architecture**  

---

**Your procedural system is now fully modular and plug-in-and-out compliant! 🎮✨**
