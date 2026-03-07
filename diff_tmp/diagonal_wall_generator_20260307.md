# Diagonal Wall & Corner Generator Tool - 2026-03-07

**Author:** BetsyBoop
**Date:** 2026-03-07
**Feature:** Editor tool for 8-axis maze diagonal walls and corners

---

## 🎯 **WHAT WAS CREATED**

### **New Editor Tool:** `DiagonalWallGenerator.cs`

**Location:** `Assets/Scripts/Editor/Maze/DiagonalWallGenerator.cs`

**Access:** Unity Editor → **Tools** → **Generate Diagonal Walls & Corners**

---

## 🛠️ **FEATURES**

### **1. Diagonal Walls** 🔷

Generates diagonal wall prefabs that perfectly fit 8-axis maze diagonal connections.

**Specs:**
- **Length:** 8.485m (6 × √2, fits cell diagonal)
- **Height:** 4.0m (matches wall height)
- **Thickness:** 0.5m
- **Rotation:** 45°, 135°, 225°, 315° (all 4 diagonal directions)

**Prefabs Created:**
- `DiagonalWallPrefab.prefab` (base, 45° rotation)
- `DiagonalWallPrefab_NE.prefab` (45°)
- `DiagonalWallPrefab_NW.prefab` (-45°)
- `DiagonalWallPrefab_SE.prefab` (-45°)
- `DiagonalWallPrefab_SW.prefab` (45°)

---

### **2. L-Corners (Internal 90°)** 🔶

Creates internal corner pieces for 90° wall connections.

**Specs:**
- **Shape:** L-shaped (two walls at 90°)
- **Size:** 6m × 6m footprint
- **Height:** 4.0m
- **Variants:** 4 directions (NW, NE, SE, SW)

**Prefabs Created:**
- `LCorner_NW.prefab`
- `LCorner_NE.prefab`
- `LCorner_SE.prefab`
- `LCorner_SW.prefab`

**Use Case:** When two cardinal walls meet at a corner (e.g., North wall + East wall)

---

### **3. Triangle Corners (External 45°)** 🔺

Creates triangular corner caps for diagonal wall ends.

**Specs:**
- **Shape:** Right triangular prism
- **Leg Length:** 3.0m (half cell)
- **Hypotenuse:** 4.243m (3 × √2)
- **Height:** 4.0m
- **Variants:** 8 directions (N, NE, E, SE, S, SW, W, NW)

**Prefabs Created:**
- `TriangleCorner_N.prefab` (0°)
- `TriangleCorner_NE.prefab` (45°)
- `TriangleCorner_E.prefab` (90°)
- `TriangleCorner_SE.prefab` (135°)
- `TriangleCorner_S.prefab` (180°)
- `TriangleCorner_SW.prefab` (225°)
- `TriangleCorner_W.prefab` (270°)
- `TriangleCorner_NW.prefab` (315°)

**Use Case:** Capping the ends of diagonal walls, creating clean exterior corners

---

## 📐 **MATHEMATICAL FOUNDATION**

### **Cell Grid Math:**

```
CARDINAL CELL:
┌─────┬─────┬─────┐
│     │     │     │
│  W  │  C  │  E   │  ← Cell size = 6m
│     │     │     │
└─────┴─────┴─────┘

DIAGONAL CELL:
┌─────┬─────┬─────┐
│     │    /│     │
│  W  │ C/  │  E  │  ← Diagonal = 6√2 ≈ 8.485m
│     │/    │     │
└─────┴─────┴─────┘

TRIANGLE CORNER:
┌─────┬─────┬─────┐
│     │    /│     │
│     │  /│     │  ← Leg = 3m (half cell)
│     │/___│     │  ← Hypotenuse = 3√2 ≈ 4.243m
└─────┴─────┴─────┘
```

### **Key Formulas:**

```csharp
// Diagonal length (Pythagorean theorem)
diagonalLength = cellSize × √2 = 6 × 1.414 = 8.485m

// Triangle corner leg
leg = cellSize / 2 = 3.0m

// Triangle corner hypotenuse
hypotenuse = leg × √2 = 3 × 1.414 = 4.243m
```

---

## 🎨 **EDITOR WINDOW**

### **Window Layout:**

```
┌─────────────────────────────────────────┐
│   8-AXIS DIAGONAL WALL GENERATOR        │
│   Generates diagonal walls and corners  │
│   for 8-directional maze system         │
├─────────────────────────────────────────┤
│  MATERIAL SETTINGS                      │
│  ┌───────────────────────────────────┐  │
│  │ Wall Material: [WallMaterial.mat] │  │
│  └───────────────────────────────────┘  │
├─────────────────────────────────────────┤
│  GENERATION                             │
│  ┌───────────────────────────────────┐  │
│  │  🎯 GENERATE ALL PREFABS          │  │
│  │     (Large blue button)           │  │
│  └───────────────────────────────────┘  │
│                                         │
│  [Generate Diagonal Walls] [L-Corners] │
│  [Triangle Corners] [All Corners]      │
│                                         │
│  [🗑️ Delete All Diagonal Prefabs]      │
├─────────────────────────────────────────┤
│  INFORMATION                            │
│  📐 DIAGONAL WALL SPECS:                │
│    • Cell Size: 6.0m                    │
│    • Wall Height: 4.0m                  │
│    • Wall Thickness: 0.5m               │
│    • Diagonal Length: 8.485m            │
│    • Rotation: 45°, 135°, 225°, 315°    │
│                                         │
│  🔷 L-CORNER SPECS:                     │
│    • Internal 90° corners               │
│    • 4 variants: NE, NW, SE, SW         │
│                                         │
│  🔺 TRIANGLE CORNER SPECS:              │
│    • External 45° corners               │
│    • 8 variants for all directions      │
│                                         │
│  📁 OUTPUT: Assets/Resources/Prefabs/   │
└─────────────────────────────────────────┘
```

---

## 📁 **PREFAB HIERARCHY**

### **After Generation:**

```
Assets/Resources/Prefabs/
├── DiagonalWallPrefab.prefab          ← Base diagonal (45°)
├── DiagonalWallPrefab_NE.prefab       ← NE-SW diagonal
├── DiagonalWallPrefab_NW.prefab       ← NW-SE diagonal
├── DiagonalWallPrefab_SE.prefab       ← SE-NW diagonal
├── DiagonalWallPrefab_SW.prefab       ← SW-NE diagonal
│
├── LCorner_NW.prefab                  ← Internal NW corner
├── LCorner_NE.prefab                  ← Internal NE corner
├── LCorner_SE.prefab                  ← Internal SE corner
├── LCorner_SW.prefab                  ← Internal SW corner
│
├── TriangleCorner_N.prefab            ← External N cap
├── TriangleCorner_NE.prefab           ← External NE cap
├── TriangleCorner_E.prefab            ← External E cap
├── TriangleCorner_SE.prefab           ← External SE cap
├── TriangleCorner_S.prefab            ← External S cap
├── TriangleCorner_SW.prefab           ← External SW cap
├── TriangleCorner_W.prefab            ← External W cap
└── TriangleCorner_NW.prefab           ← External NW cap
```

---

## 🔧 **HOW TO USE**

### **Step 1: Open Tool**

```
Unity Editor → Tools → Generate Diagonal Walls & Corners
```

### **Step 2: Assign Material (Optional)**

- Drag your wall material to "Wall Material" field
- Default: Brownish standard material if none assigned
- Recommended: Use same material as cardinal walls

### **Step 3: Generate**

**Option A - Generate All:**
```
Click "🎯 GENERATE ALL PREFABS"
→ Creates all 17 prefabs at once
```

**Option B - Selective:**
```
Click individual buttons:
- "Generate Diagonal Walls" → 5 diagonal variants
- "Generate L-Corners" → 4 L-corner variants
- "Generate Triangle Corners" → 8 triangle variants
- "Generate All Corners" → 12 corner variants
```

### **Step 4: Assign to CompleteMazeBuilder**

```
Select CompleteMazeBuilder in Hierarchy
↓
Inspector → Diagonal Prefabs section
↓
Assign:
  • wallDiagPrefab → DiagonalWallPrefab.prefab
  • wallCornerPrefab → LCorner_NW.prefab (or TriangleCorner_N.prefab)
```

---

## 🎮 **IN-GAME USAGE**

### **CompleteMazeBuilder Integration:**

The tool integrates with your existing 8-axis maze system:

```csharp
// CompleteMazeBuilder.cs (already has these fields)
[Header("Diagonal Prefabs")]
[SerializeField] private GameObject wallDiagPrefab;
[SerializeField] private GameObject wallCornerPrefab;

// SpawnWallIfPresent() uses wallDiagPrefab for diagonal walls
private void SpawnWallIfPresent(int x, int z, float wx, float wz, 
                                 Direction8 dir, CellFlags8 cell)
{
    bool isDiagonal = Direction8Helper.IsDiagonal(dir);
    GameObject prefab = isDiagonal ? wallDiagPrefab : wallPrefab;
    
    // Rotation already handled by SpawnWallIfPresent
    // (45°, -45°, etc. for diagonal directions)
}
```

### **When to Use Which:**

| Prefab Type | Use Case | Example |
|-------------|----------|---------|
| **DiagonalWall** | Diagonal corridor connections | NE, NW, SE, SW passages |
| **L-Corner** | Internal 90° turns | North wall + East wall junction |
| **TriangleCorner** | External diagonal caps | End of diagonal wall, outer corners |

---

## 📊 **MESH DETAILS**

### **Diagonal Wall Mesh:**

```csharp
Vertices: 8 (box corners)
Triangles: 36 (6 faces × 2 triangles × 3 vertices)
Dimensions: 8.485m × 4.0m × 0.5m
Rotation: 45° around Y axis
Collider: BoxCollider (same size as mesh)
```

### **L-Corner Mesh:**

```csharp
Vertices: 16 (two boxes combined)
Triangles: 72 (two boxes, 36 each)
Dimensions: 6.0m × 6.0m footprint, 4.0m height
Shape: Two walls at 90° angle
Collider: MeshCollider (combined shape)
```

### **Triangle Corner Mesh:**

```csharp
Vertices: 6 (triangular prism)
Triangles: 30 (5 faces: 2 triangles + 3 quads)
Dimensions: 3.0m leg, 4.243m hypotenuse, 4.0m height
Shape: Right triangular prism
Collider: MeshCollider (triangular shape)
```

---

## 🗑️ **CLEANUP**

### **Delete Generated Prefabs:**

```
Click "🗑️ Delete All Diagonal Prefabs"
→ Removes all diagonal and corner prefabs
→ Keeps cardinal wall prefabs
```

**Deleted:**
- All `DiagonalWallPrefab*.prefab`
- All `LCorner_*.prefab`
- All `TriangleCorner_*.prefab`

**Preserved:**
- `WallPrefab.prefab` (cardinal)
- `DoorPrefab.prefab`
- Other non-diagonal prefabs

---

## ✅ **VERIFICATION CHECKLIST**

After generation, verify:

```
□ Prefabs created in Assets/Resources/Prefabs/
□ DiagonalWallPrefab has 8.485m length
□ L-Corner prefabs form 90° angle
□ TriangleCorner prefabs are triangular prisms
□ All prefabs have colliders
□ All prefabs have mesh renderers
□ Materials assigned correctly
□ No console errors
□ Prefabs preview correctly in Inspector
```

---

## 🔍 **TECHNICAL IMPLEMENTATION**

### **Mesh Generation:**

```csharp
// Diagonal wall uses box mesh, rotated 45°
Mesh CreateDiagonalWallMesh()
{
    // Box vertices: 8 corners
    // Box dimensions: diagonalLength × wallHeight × wallThickness
    // Rotated 45° in SpawnWallIfPresent()
}

// Triangle corner uses triangular prism
Mesh CreateTriangleCornerMesh()
{
    // Prism vertices: 6 (2 triangles)
    // Faces: 5 (2 triangle ends + 3 rectangular sides)
    // Right triangle with legs = 3m
}
```

### **Prefab Saving:**

```csharp
// Create GameObject in scene
GameObject prefab = new GameObject("DiagonalWallPrefab");

// Add components
prefab.AddComponent<MeshFilter>();
prefab.AddComponent<MeshRenderer>();
prefab.AddComponent<BoxCollider>(); // or MeshCollider

// Assign mesh and material
meshFilter.mesh = createdMesh;
meshRenderer.sharedMaterial = wallMaterial;

// Save as prefab asset
PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);

// Cleanup scene object
DestroyImmediate(prefab);
```

---

## 📝 **FUTURE ENHANCEMENTS**

Potential additions:

1. **T-Junction Pieces** - 3-way wall connections
2. **Cross Junction Pieces** - 4-way wall connections
3. **Sloped Walls** - For stairs, ramps
4. **Curved Walls** - Smooth diagonal transitions
5. **Texture Atlas Support** - UV mapping for varied textures
6. **LOD Support** - Lower poly versions for distant walls
7. **Modular Wall System** - Snap-together pieces

---

## 🎯 **SUMMARY**

**Created:** Complete editor tool for 8-axis diagonal walls and corners

**Prefabs:** 17 total (5 diagonal + 4 L-corner + 8 triangle corner)

**Integration:** Works with existing `CompleteMazeBuilder.SpawnWallIfPresent()`

**Math:** Precise cell-size matching (6m cells, 45° diagonals)

**Access:** Tools → Generate Diagonal Walls & Corners

---

## 🚀 **NEXT STEPS**

1. **Open Unity Editor**
2. **Tools → Generate Diagonal Walls & Corners**
3. **Click "GENERATE ALL PREFABS"**
4. **Assign to CompleteMazeBuilder**
5. **Test maze generation with diagonal walls!**

---

*Diff generated - 2026-03-07 - Unity 6 compatible - UTF-8 encoding - Unix LF*

**Ready to build those diagonal walls, captain!** 🎯🔷
