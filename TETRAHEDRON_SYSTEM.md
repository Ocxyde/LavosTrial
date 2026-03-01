# Tetrahedron System - Complete

**Location:** `Temp/TetrahedronAssets/`

---

## ✅ What Was Created

| File | Purpose |
|------|---------|
| `TetrahedronEngine.cs` | **MAIN ENGINE** - Static class, call from anywhere |
| `TetrahedronVariants.cs` | 8 different tetrahedron shape definitions |
| `README.md` | Usage documentation |

---

## 🎯 How It Works

```
┌─────────────────────────────────────────────────────────┐
│  TetrahedronEngine (Static - Call from Any Script)     │
├─────────────────────────────────────────────────────────┤
│  GenerateLevel(seed, width, height)  ← Call ONCE       │
│  GetTetrahedron(variantIndex)        ← Get mesh        │
│  GetRandomTetrahedron()              ← Random variant  │
│  CreateTetrahedron(pos, rot, var, mat) ← Create GO     │
└─────────────────────────────────────────────────────────┘
                          │
                          │ 8 Variants
                          ▼
            ┌───────────────────────────────┐
            │  TetrahedronVariants          │
            │  0. Standard                  │
            │  1. Elongated                 │
            │  2. Compressed                │
            │  3. Skewed                    │
            │  4. WideBase                  │
            │  5. NarrowBase                │
            │  6. Asymmetric                │
            │  7. Crystal                   │
            └───────────────────────────────┘
```

---

## 📖 Usage

### Step 1: Generate (Once Per Level)

```csharp
// In MazeRenderer.Start() or GameManager.Start():
TetrahedronEngine.GenerateLevel(seed: 12345, width: 31, height: 31);
```

### Step 2: Use Tetrahedrons

```csharp
// In your wall/object creation:
int variant = Random.Range(0, TetrahedronEngine.VariantCount);
Mesh tetra = TetrahedronEngine.GetTetrahedron(variant);

// Or create directly:
TetrahedronEngine.CreateTetrahedron(position, rotation, variant, material);
```

---

## 🔌 Integration Points

Call `TetrahedronEngine` from:
- **MazeRenderer.cs** - For walls, floors, ceilings
- **GameManager.cs** - For level initialization
- **Any script** - It's static and optional

**Your game works WITHOUT it** - completely optional system!

---

## 📁 Folder Structure

```
D:\travaux_Unity\PeuImporte\
├── Temp/TetrahedronAssets/     ← NEW: Tetrahedron system
│   ├── TetrahedronEngine.cs    ← MAIN ENGINE
│   ├── TetrahedronVariants.cs  ← 8 variants
│   └── README.md
├── Assets/Scripts/
│   ├── Core/
│   │   └── TetrahedronMesh.cs  ← Marked obsolete
│   ├── HUD/
│   ├── Player/
│   └── ...
└── ...
```

---

## ✨ Key Features

| Feature | Benefit |
|---------|---------|
| **Static class** | Call from anywhere, no instantiation |
| **Generate once** | Performance - cached meshes |
| **8 variants** | Visual variety |
| **Seed-based** | Reproducible levels |
| **Optional** | Game works without it |

---

## 🎮 Next Steps

1. **Open Unity** - Wait for compilation
2. **Add to MazeRenderer.cs** (optional):
   ```csharp
   void Start()
   {
       TetrahedronEngine.GenerateLevel(_gen.CurrentSeed, _gen.Width, _gen.Height);
   }
   ```
3. **Replace cube walls** with tetrahedrons (optional)

---

## 📝 Notes

- **Temp folder** - Experimental, can be moved later
- **One call per level** - `GenerateLevel()` is idempotent
- **Variants 0-7** - Use `Random.Range(0, 8)`
- **Clean API** - No dependencies on other scripts
